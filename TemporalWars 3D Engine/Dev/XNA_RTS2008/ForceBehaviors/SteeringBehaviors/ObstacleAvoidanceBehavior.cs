#region File Description
//-----------------------------------------------------------------------------
// ObstacleAvoidanceBehavior.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.ForceBehaviors.Enums;
using ImageNexus.BenScharbach.TWEngine.ForceBehaviors.Structs;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.ForceBehaviors.SteeringBehaviors
{
    ///<summary>
    /// The <see cref="ObstacleAvoidanceBehavior"/> class is used to create a steering force which steers a <see cref="SceneItem"/>
    ///  to avoid obstacles lying in its path.
    ///</summary>
    public sealed class ObstacleAvoidanceBehavior : AbstractBehavior
    {
        private static readonly Vector3 Vector3Zero = Vector3.Zero; // 8/11/2009

        const float MinDetectionBoxLength = 40.0f;
        const float BrakingWeight = 0.2f;

        ///<summary>
        /// Constructor
        ///</summary>
        public ObstacleAvoidanceBehavior()
            : base((int)BehaviorsEnum.ObstacleAvoidance, 10.0f)
        {
            // Empty
        }

        /// <summary>
        ///  Given a vector of Obstacles, this method returns a steering force
        ///  that will prevent the agent colliding with the closest obstacle
        /// </summary>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> as elapsed game time.</param>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        /// <param name="force">(OUT) calculated force as <see cref="Vector3"/></param>
        public override void Update(ref BehaviorsTimeSpan elapsedTime, SceneItem item, out Vector3 force)
        {
            // 5/20/2012 - Throw expection if not SceneItemWithPick.
            var sceneItemWithPick = (SceneItemWithPick)item;
            if (sceneItemWithPick == null)
            {
                throw new InvalidOperationException("FollowPathAbstractBehavior can ONLY be used with SceneItemWithPick types.");
            }

            // 5/16/2010
            force = default(Vector3);

            //the detection box length is proportional to the agent's velocity
            var boxLength = MinDetectionBoxLength + (item.Velocity.Length() / item.MaxSpeed) * MinDetectionBoxLength;

            //tag all obstacles within range of the box for processing
            TagObstaclesWithinViewRange(item, boxLength);

            //this will keep track of the closest intersecting obstacle (CIB)
            SceneItemWithPick closestIntersectingObstacle = null;

            //this will be used to track the distance to the CIB
            var distToClosestIp = Single.MaxValue;
            Matrix inTransform;

            //this will record the transformed local coordinates of the CIB
            var localPosOfClosestObstacle = Vector3Zero;

            var obstacles = ForceBehaviorsCalculator.Obstacles; // 5/16/2010 - Cache
            if (obstacles == null) return;

            var count = obstacles.Count; // 12/18/2009
            for (var i = 0; i < count; i++)
            {
                // 12/18/2009 - Cache
                var obstacle = obstacles[i];
                if (obstacle == null) continue;

                //if the obstacle has been tagged within range proceed
                if (!((SceneItemWithPick)obstacle).IsVisibleObstacle) continue;

                //calculate this obstacle's Position in local space                    
                var obstaclePosition = obstacle.Position;
                var itemPosition = item.Position;
                inTransform = item.ShapeItem.Orientation;

                Vector3 localPos;
                ForceBehaviorsCalculator.PointToLocalSpace(ref obstaclePosition, ref itemPosition, ref inTransform, out localPos);
                    
                //if the local Position has a negative z value then it must lay
                //behind the agent. (in which case it can be ignored)
                if (localPos.X < 0) continue;

                //if the distance from the x axis to the object's Position is less
                //than its radius + half the width of the detection box then there
                //is a potential intersection.
                var expandedRadius = obstacle.CollisionRadius + item.CollisionRadius;


                if (Math.Abs(localPos.Z) >= expandedRadius) continue;

                //now to do a line/circle intersection test. The center of the 
                //circle is represented by (cX, cY). The intersection points are 
                //given by the formula x = cX +/-sqrt(r^2-cY^2) for y=0. 
                //We only need to look at the smallest positive value of x because
                //that will be the closest point of intersection.
                var cX = localPos.X;
                var cY = localPos.Z;

                //we only need to calculate the sqrt part of the above equation once
                var sqrtPart = (float)Math.Sqrt(expandedRadius * expandedRadius - cY * cY);

                var ip = cX - sqrtPart;

                if (ip <= 0.0)
                {
                    ip = cX + sqrtPart;
                }

                //test to see if this is the closest so far. If it is keep a
                //record of the obstacle and its local coordinates
                if (ip >= distToClosestIp) continue;

                distToClosestIp = ip;

                closestIntersectingObstacle = (SceneItemWithPick) obstacle;

                localPosOfClosestObstacle = localPos;
            } // End Loop Obstacles


            //if we have found an intersecting obstacle, calculate a steering 
            //force away from it
            var steeringForce = Vector3Zero;

            if (closestIntersectingObstacle != null)
            {
                //the closer the agent is to an object, the stronger the 
                //steering force should be
                var multiplier = (float)(1.0 + (boxLength - localPosOfClosestObstacle.X) / boxLength); 

                //calculate the lateral force
                //10/18/2008: By changing the minus sign to division, the lateral movement is working 10x better!
                steeringForce.Z = (closestIntersectingObstacle.CollisionRadius /
                                   localPosOfClosestObstacle.Z) * multiplier; // Was minus sign

                //apply a braking force proportional to the obstacles distance from
                //the vehicle. 
                steeringForce.X = (closestIntersectingObstacle.CollisionRadius -
                                   localPosOfClosestObstacle.X) *
                                   BrakingWeight; 
            }

            //finally, convert the steering vector from local to World space
            inTransform = item.ShapeItem.Orientation;
           
            ForceBehaviorsCalculator.VectorToWorldSpace(ref steeringForce, ref inTransform, out force);                     
            
        }

        // 10/16/2008
        // 12/4/2008 - Updated to remove Overload ops, since this slows down XBOX!
        /// <summary>
        /// Tags any entities contained in Obstacles ListArray that are within the
        /// radius of the single entity parameter.
        /// </summary>
        /// <param name="item">SceneItem</param>
        /// <param name="radius">Radius to check if item is within</param>
        private static void TagObstaclesWithinViewRange(SceneItem item, float radius)
        {
            // 5/16/2010
            var obstacles = ForceBehaviorsCalculator.Obstacles;
            if (obstacles == null) return;

            //iterate through all entities checking for range
            var count = obstacles.Count; // 8/25/2009
            for (var i = 0; i < count; i++)
            {
                // 8/25/2009 - Cache
                var obstacle = obstacles[i]; 
                if (obstacle == null) continue; // 12/18/2009

                //first clear any current tag
                ((SceneItemWithPick)obstacle).IsVisibleObstacle = false;

                // 12/18/2009: Correction: Use 'Subtract', and not 'Min'.
                // 12/4/2008 - Updated to remove Overload ops, since this slows down XBOX!
                //to = obstacles[i].Position - SceneItemOwner.Position;
                var tmpPositionA = obstacle.Position;
                var tmpPositionB = item.Position;
                Vector3 to;
                Vector3.Subtract(ref tmpPositionA, ref tmpPositionB, out to);

                //the bounding radius of the other is taken into account by adding it 
                //to the range
                var range = radius + obstacle.CollisionRadius;

                //if entity within range, tag for further consideration. (working in
                //distance-squared space to avoid sqrts)
                if ((obstacle != item) && (to.LengthSquared() < range * range))
                {
                    ((SceneItemWithPick)obstacle).IsVisibleObstacle = true;
                }

            } // End Loop Obstacles
        }
    }
}
