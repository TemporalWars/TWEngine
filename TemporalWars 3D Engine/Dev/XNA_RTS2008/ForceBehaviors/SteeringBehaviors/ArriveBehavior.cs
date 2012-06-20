#region File Description
//-----------------------------------------------------------------------------
// ArriveBehavior.cs
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
    /// The <see cref="ArriveBehavior"/> class is used to seek towards a target item, but
    /// decelerates onto the target position.
    ///</summary>
    /// <remarks>Similar to the <see cref="SeekBehavior"/>, with the exception of the deceleration quality.</remarks>
    public sealed class ArriveBehavior : AbstractBehavior
    {
        //because Deceleration is enumerated as an int, this value is required
        //to provide fine tweaking of the Deceleration..
        const float DecelerationTweaker = 0.3f;

        ///<summary>
        /// The <see cref="Deceleration"/>
        ///</summary>
        public Deceleration Deceleration = Deceleration.Normal;

        private static readonly Vector2 Vector2Zero = Vector2.Zero; // 8/11/2009
        private static readonly Vector3 Vector3Zero = Vector3.Zero; // 8/11/2009

        ///<summary>
        /// Constructor
        ///</summary>
        public ArriveBehavior()
            : base((int)Enums.BehaviorsEnum.Arrive, 1.0f)
        {
            // Empty
        }


        /// <summary>
        /// This AbstractBehavior is similar to seek but it attempts to arrive at the
        /// target with a zero velocity
        /// </summary>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> as elapsed game time.</param>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        /// <param name="force">(OUT) calculated force as <see cref="Vector3"/></param>
        public override void Update(ref BehaviorsTimeSpan elapsedTime, SceneItem item, out Vector3 force)
        {
            // 5/16/2010 - 
            DoUpdate(this, item, out force);
        }

        // 5/16/2010 
        /// <summary>
        /// Helper Method, for the 'Update' method.
        /// </summary>
        /// <param name="arriveBehavior">Instance of <see cref="ArriveBehavior"/>.</param>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        /// <param name="force">(OUT) calculated force as <see cref="Vector3"/></param>
        private static void DoUpdate(ArriveBehavior arriveBehavior, SceneItem item, out Vector3 force)
        {
            // 12/18/2008 - Remove Overload Ops, since slows down XBOX.
            var tmpPos = Vector2Zero; var tmpMoveTo = Vector2Zero;
            tmpPos.X = item.Position.X; tmpPos.Y = item.Position.Z;

            var sceneItemWithPick = item as SceneItemWithPick; // 5/20/2012
            tmpMoveTo.X = (sceneItemWithPick != null) ? sceneItemWithPick.MoveToPosition.X : item.MoveToWayPosition.X; 
            tmpMoveTo.Y = (sceneItemWithPick != null) ? sceneItemWithPick.MoveToPosition.Z : item.MoveToWayPosition.Z;

            Vector2 directionA;
            Vector2.Subtract(ref tmpMoveTo, ref tmpPos, out directionA);

            // 2/3/2009 - Set into Vector3 struct
            var directionB = new Vector3 { X = directionA.X, Z = directionA.Y };

            //calculate the distance to the target
            var dist = directionB.Length();

            if (dist > 0)
            {
                //calculate the speed required to reach the target given the desired
                //Deceleration
                var speed = dist / ((float)arriveBehavior.Deceleration * DecelerationTweaker);

                //make sure the velocity does not exceed the max
                MathHelper.Clamp(speed, -item.MaxSpeed, item.MaxSpeed);

                // 12/18/2008 - Remove Overload Ops, since slows down XBOX.
                //from here proceed just like Seek except we don't need to normalize 
                //the ToTarget vector because we have already gone to the trouble
                //of calculating its length: dist. 
                //desiredVelocity = direction * speed / dist;
                Vector3.Multiply(ref directionB, speed, out directionB);
                Vector3 desiredVelocity;
                Vector3.Divide(ref directionB, dist, out desiredVelocity);

                //force = (desiredVelocity - SceneItemOwner.Velocity);
                var tmpVelocity = item.Velocity;
                Vector3.Subtract(ref desiredVelocity, ref tmpVelocity, out force);

                return;

            }

            force = Vector3Zero;
            return;
        }

        // Overload 2: Called from the OffsetPursuitAbstractBehavior
        ///<summary>
        /// This AbstractBehavior is similar to seek but it attempts to arrive at the
        /// target with a zero velocity
        ///</summary>
        ///<param name="item"><see cref="SceneItemWithPick"/> instance</param>
        ///<param name="targetPosition">the <see cref="Vector3"/> target position</param>
        ///<param name="deceleration"><see cref="Deceleration"/> Enum</param>
        ///<param name="force">(OUT) calculated force as <see cref="Vector3"/></param>
        public void Update(SceneItem item, Vector3 targetPosition, Deceleration deceleration, out Vector3 force)
        {
            // 12/18/2008 - Remove Overload Ops, since slows down XBOX.
            DoUpdate(item, ref targetPosition, deceleration, out force);
        }

        // 5/16/2010
        /// <summary>
        /// Method helper, for the 'Update' overload for <see cref="OffsetPursuitBehavior"/>.
        /// </summary>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        /// <param name="targetPosition">the <see cref="Vector3"/> target position</param>
        /// <param name="deceleration"><see cref="Deceleration"/> Enum</param>
        /// <param name="force">(OUT) calculated force as <see cref="Vector3"/></param>
        private static void DoUpdate(SceneItem item, ref Vector3 targetPosition, Deceleration deceleration, out Vector3 force)
        {
            var tmpPos = Vector2Zero; var tmpMoveTo = Vector2Zero;
            tmpPos.X = item.Position.X; tmpPos.Y = item.Position.Z;
            tmpMoveTo.X = targetPosition.X; tmpMoveTo.Y = targetPosition.Z;

            Vector2 directionA;
            Vector2.Subtract(ref tmpMoveTo, ref tmpPos, out directionA);

            // 2/3/2009 - Set into Vector3 struct
            var directionB = new Vector3 { X = directionA.X, Z = directionA.Y };

            //calculate the distance to the target
            var dist = directionB.Length();

            if (dist > 0)
            {
                //calculate the speed required to reach the target given the desired
                //Deceleration
                var speed = dist / ((float)deceleration * DecelerationTweaker);

                //make sure the velocity does not exceed the max
                MathHelper.Clamp(speed, -item.MaxSpeed, item.MaxSpeed);

                // 12/18/2008 - Remove Overload Ops, since slows down XBOX.
                //from here proceed just like Seek except we don't need to normalize 
                //the ToTarget vector because we have already gone to the trouble
                //of calculating its length: dist. 
                //desiredVelocity = direction * speed / dist;
                Vector3.Multiply(ref directionB, speed, out directionB);
                Vector3 desiredVelocity;
                Vector3.Divide(ref directionB, dist, out desiredVelocity);


                //force = (desiredVelocity - SceneItemOwner.Velocity);
                var tmpVelocity = item.Velocity;
                Vector3.Subtract(ref desiredVelocity, ref tmpVelocity, out force);

                return;

            }
            force = Vector3Zero;
        }
    }
}
