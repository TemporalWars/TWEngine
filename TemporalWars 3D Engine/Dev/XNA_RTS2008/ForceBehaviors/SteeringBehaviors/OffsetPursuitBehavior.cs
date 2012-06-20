#region File Description
//-----------------------------------------------------------------------------
// OffsetPursuitBehavior.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using TWEngine.ForceBehaviors.Enums;
using TWEngine.ForceBehaviors.Structs;
using TWEngine.SceneItems;

namespace TWEngine.ForceBehaviors.SteeringBehaviors
{
    ///<summary>
    /// The <see cref="OffsetPursuitBehavior"/> class is used to create a steering force required to keep
    /// a <see cref="SceneItem"/> position at a specified offset from a target <see cref="SceneItem"/>.
    ///</summary>
    public sealed class OffsetPursuitBehavior : AbstractBehavior
    {
        private ArriveBehavior _arriveBehavior;                 
     
        private Vector3 _offset;
        private static readonly Vector3 Vector3Zero = Vector3.Zero;

        private const float WaypointSeekDistSq = (20 * 20);        

        ///<summary>
        /// Specified offset distance to use.
        ///</summary>
        public Vector3 OffsetBy
        {
            get { return _offset; }
            set { _offset = value; }
        }

        ///<summary>
        /// Constructor, which creates the <see cref="ArriveBehavior"/>.
        ///</summary>
        public OffsetPursuitBehavior()
            : base((int)BehaviorsEnum.OffsetPursuit, 1.0f)
        {
            _arriveBehavior = new ArriveBehavior();
            
        }

        /// <summary>
        /// Produces a steering force that keeps a vehicle at a specified _offset
        /// from a leader vehicle
        /// </summary>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> as elapsed game time.</param>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        /// <param name="force">(OUT) calculated force as <see cref="Vector3"/></param>
        public override void Update(ref BehaviorsTimeSpan elapsedTime, SceneItem item, out Vector3 force)
        {
            force = Vector3Zero;
 
            // Make sure there is a TargetItem to use
            var targetItem1 = ForceBehaviorManager.TargetItem1; // 5/16/2010 = Cache
            if (targetItem1 == null) return;          

            //calculate the _offset's Position in World space
            Vector3 worldOffsetPos;
            var leaderOrientation = targetItem1.ShapeItem.Orientation;
            leaderOrientation.Translation = targetItem1.Position;
            Vector3.Transform(ref _offset, ref leaderOrientation, out worldOffsetPos);

            // If we reached Goal, then return with Vector.Zero
            float result;
            CalculateDistancedSquared(item, ref worldOffsetPos, out result);
           
            if (result < WaypointSeekDistSq)
                return;            

            Vector3 toOffset, tmpPosition = item.Position; // = worldOffsetPos - SceneItemOwner.Position;
            Vector3.Subtract(ref worldOffsetPos, ref tmpPosition, out toOffset);
            
            //the lookahead Time is propotional to the distance between the leader
            //and the pursuer; and is inversely proportional to the sum of both
            //agent's velocities
            var lookAheadTime = toOffset.Length() / (item.MaxSpeed + targetItem1.Velocity.Length());

            // 8/4/2009: Optimized by using the Vector3 methods, which are FAST on the XBOX.
            // Old Formula: (worldOffsetPos + ForceBehaviorManager.targetItem1.Velocity * lookAheadTime)
            //now Arrive at the predicted future Position of the _offset  
            Vector3 tmpResult;
            var tmpVelocity = targetItem1.Velocity;
            Vector3.Multiply(ref tmpVelocity, lookAheadTime, out tmpVelocity);
            Vector3.Add(ref worldOffsetPos, ref tmpVelocity, out tmpResult);

            _arriveBehavior.Update(item, tmpResult, Deceleration.Fast, out force);
                       
        }

        // 8/4/2009
        /// <summary>
        /// Calculates the distance Squared, between the <see cref="SceneItem"/> position and given <paramref name="worldOffsetPos"/> value, returning
        /// the distance float value.
        /// </summary>
        /// <param name="item"><see cref="SceneItem"/> instance</param>
        /// <param name="worldOffsetPos">the <see cref="Vector3"/> offset position</param>
        /// <param name="distance">(OUT) calculated distance</param>
        private static void CalculateDistancedSquared(SceneItem item, ref Vector3 worldOffsetPos, out float distance)
        {
            var tmpCompare1 = new Vector2 {X = worldOffsetPos.X, Y = worldOffsetPos.Z};

            var itemPosition = item.Position;
            var tmpCompare2 = new Vector2 { X = itemPosition.X, Y = itemPosition.Z };

            Vector2.DistanceSquared(ref tmpCompare1, ref tmpCompare2, out distance);
        }

        // 11/14/2008
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            // null refs
            _arriveBehavior = null;

            base.Dispose();
        }
    }
}
