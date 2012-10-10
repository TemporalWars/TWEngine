#region File Description
//-----------------------------------------------------------------------------
// FleeBehavior.cs
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
    /// <summary>
    /// The <see cref="FleeBehavior"/> class is used to create a force which steers a <see cref="SceneItem"/>
    /// away from some target position.
    /// </summary>
    /// <remarks><see cref="FleeBehavior"/> is the opposite of the <see cref="SeekBehavior"/></remarks>
    public sealed class FleeBehavior : AbstractBehavior
    {
        private Vector3 _aTargetPosition;
        private double _panicDistance = 100.0f;
        private double _panicDistanceSq = 100.0f * 100.0f;
        private static readonly Vector3 Vector3Zero = Vector3.Zero;

        #region Properties

        /// <summary>
        /// A static target position to Flee from, 
        /// set from the Properties-Tool form.
        /// </summary>
        public Vector3 TargetPosition
        {
            get { return _aTargetPosition; }
            set { _aTargetPosition = value; }
        }

        /// <summary>
        /// Distance this item starts to panic, and wants to Flee
        /// from target item.  Currently defaults to 100.0f.
        /// </summary>
        public double PanicDistance
        {
            get { return _panicDistance; }
            set 
            { 
                _panicDistance = value; 

                // 8/5/2009
                _panicDistanceSq = _panicDistance * _panicDistance;
            }
        }

        #endregion

        ///<summary>
        /// Constructor
        ///</summary>
        public FleeBehavior()
            : base((int)BehaviorsEnum.Flee, 1.0f)
        {
            
        }


        /// <summary>
        /// Does the opposite of <see cref="SeekBehavior"/>; only Flee if the target is within <see cref="PanicDistance"/>.
        /// </summary>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> as elapsed game time.</param>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        /// <param name="force">(OUT) calculated force as <see cref="Vector3"/></param>
        public override void Update(ref BehaviorsTimeSpan elapsedTime, SceneItem item, out Vector3 force)
        {
            // Choose Target to use;
            // - If no Ref to 'TargetItem' set, then use '_aTargetPosition' static value.
            var targetPosition = ForceBehaviorManager.TargetItem1 != null ? ForceBehaviorManager.TargetItem1.Position : _aTargetPosition;

            //only flee if the target is within 'panic distance'. 
            var itemPosition = item.Position;           

            float result;
            Vector3.DistanceSquared(ref itemPosition, ref targetPosition, out result);
            if (result > _panicDistanceSq)
            {
                force = Vector3Zero;
                return;
            }

            // 12/18/2008 - Updated to remove Overload Ops, since this slows down XBOX.
            // Direction away from target
            //direction = SceneItemOwner.Position - targetPos;
            Vector3 direction;
            Vector3.Subtract(ref itemPosition, ref targetPosition, out direction);
            
            // 8/5/2009: Avoid NaN errors, by not normalizing Zero values!
            if (!direction.Equals(Vector3Zero)) direction.Normalize();

            // 12/18/2008 - Updated to remove Overload Ops, since this slows down XBOX.
            //desiredVelocity = direction * SceneItemOwner.MaxSpeed;
            Vector3 desiredVelocity;
            Vector3.Multiply(ref direction, item.MaxSpeed, out desiredVelocity);

            //force = (desiredVelocity - SceneItemOwner.Velocity);
            var tmpVelocity = item.Velocity;
            Vector3.Subtract(ref desiredVelocity, ref tmpVelocity, out force);

        }
    }
}
