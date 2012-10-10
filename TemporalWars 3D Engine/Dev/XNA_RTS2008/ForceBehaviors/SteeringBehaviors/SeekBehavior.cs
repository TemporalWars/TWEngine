#region File Description
//-----------------------------------------------------------------------------
// SeekBehavior.cs
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
    /// The <see cref="SeekBehavior"/> class is used to return a force which directs a <see cref="SceneItem"/> towards
    /// a target position.
    ///</summary>
    /// <remarks><see cref="SeekBehavior"/> is the opposite of the <see cref="FleeBehavior"/></remarks>
    public sealed class SeekBehavior : AbstractBehavior
    {
        // 12/18/2009 - Add ComfortDistance value.
        private double _comfortDistance = 250.0f;
        private double _comfortDistanceSq = 250.0f * 250.0f;
        // 12/18/2009 - Static TargetPosition
        private Vector3 _aTargetPosition;

        private static readonly Vector3 Vector3Zero = Vector3.Zero;

        #region Properties

        // 12/18/2009
        /// <summary>
        /// A static target position to Seek to, 
        /// set from the Properties-Tool form.
        /// </summary>
        public Vector3 TargetPosition
        {
            get { return _aTargetPosition; }
            set { _aTargetPosition = value; }
        }

        /// <summary>
        /// Distance this <see cref="SceneItem"/> needs to be within the target item, to
        /// feel comfortable and stop seeking. 
        /// </summary>
        /// <remarks>Currently defaults to 250.0f.</remarks>
        public double ComfortDistance
        {
            get { return _comfortDistance; }
            set
            {
                _comfortDistance = value;
                _comfortDistanceSq = _comfortDistance * _comfortDistance;
            }
        }

        // 12/18/2009
        /// <summary>
        /// Set to TRUE, to force the item to seek towards the saved
        /// <see cref="TargetPosition"/> position; otherwise, it will seek towards the 'MoveToPosition'. 
        /// </summary>
        /// <remarks>
        /// The <see cref="FollowPathBehavior"/> uses the default action, which makes the unit seek 
        /// from one node to the next during pathfinding.  Set this to TRUE, when you
        /// want the item to move to some <see cref="TargetPosition"/>.
        /// </remarks>
        public bool SeekToTargetItem { get; set; }

        #endregion

        ///<summary>
        /// Constructor
        ///</summary>
        public SeekBehavior()
            : base((int)BehaviorsEnum.Seek, 1.0f)
        {
            
        }

        /// <summary>
        ///  Using <see cref="SceneItem"/> MoveTo position, this AbstractBehavior returns a steering force which will
        ///  direct the agent towards the target.
        /// </summary>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> as elapsed game time.</param>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        /// <param name="force">(OUT) calculated force as <see cref="Vector3"/></param>
        public override void Update(ref BehaviorsTimeSpan elapsedTime, SceneItem item, out Vector3 force)
        {
            // 5/16/2010 - Refactored code out to STATIC method.
            DoUpdate(this, item, out force);
        }

        // 5/16/2010
        /// <summary>
        /// Method helper, for the Update method.
        /// </summary>
        /// <param name="seekAbstractBehavior">Instance of <see cref="SeekBehavior"/></param>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        /// <param name="force">(OUT) calculated force as <see cref="Vector3"/></param>
        private static void DoUpdate(SeekBehavior seekAbstractBehavior, SceneItem item, out Vector3 force)
        {
            // 12/18/2009
            if (seekAbstractBehavior.SeekToTargetItem)
            {
                // Choose Target to use;
                // - If no Ref to 'TargetItem' set, then use '_aTargetPosition' static value.
                var targetItem1 = seekAbstractBehavior.ForceBehaviorManager.TargetItem1; // 5/16/2010 - Cache
                var targetPosition = targetItem1 != null ? targetItem1.Position : seekAbstractBehavior._aTargetPosition;

                //only seek if the target is NOT within 'comfort distance'. 
                var itemPosition1 = item.Position;

                float result;
                Vector3.DistanceSquared(ref itemPosition1, ref targetPosition, out result);
                if (result <= seekAbstractBehavior._comfortDistanceSq)
                {
                    force = Vector3Zero;
                    return;
                }

                seekAbstractBehavior.Update(item, ref targetPosition, out force);
                return;
            }

            // 12/18/2008 - Updated to remove Overload Ops, since this slows down XBOX. 
            var itemPosition = item.Position;
            var tmpPos = new Vector2 { X = itemPosition.X, Y = itemPosition.Z };

            var sceneItemWithPick = item as SceneItemWithPick; // 5/20/2012
            var itemMovePosition = (sceneItemWithPick != null) ? sceneItemWithPick.MoveToPosition : item.MoveToWayPosition ;
            var tmpMoveTo = new Vector2 { X = itemMovePosition.X, Y = itemMovePosition.Z };

            // 12/18/2009 - (Strange; exactly 1 year later from update above!)
            GetForce(item, ref tmpMoveTo, ref tmpPos, out force);
           
        }

        // 10/14/2008
        /// <summary>
        /// Given a target, this AbstractBehavior returns a steering force which will
        /// direct the agent towards the target.
        /// </summary>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        /// <param name="targetPosition">the <see cref="Vector3"/> target position</param>
        /// <param name="force">(OUT) calculated force as <see cref="Vector3"/></param>
        public void Update(SceneItem item, ref Vector3 targetPosition, out Vector3 force)
        {
            // 12/18/2008 - Updated to remove Overload Ops, since this slows down XBOX.   
            var itemPosition = item.Position;
            var tmpPos = new Vector2 { X = itemPosition.X, Y = itemPosition.Z };
            var tmpMoveTo = new Vector2 { X = targetPosition.X, Y = targetPosition.Z };

            // 12/18/2009 - (Strange; exactly 1 year later from update above!)
            GetForce(item, ref tmpMoveTo, ref tmpPos, out force);
        }

        // 12/18/2009
        /// <summary>
        /// Helper method to do the actual 'Seek' force calculation.
        /// </summary>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        /// <param name="seekPosition">Position to Seek to.</param>
        /// <param name="startPosition">Starting Position</param>
        /// <param name="force">(OUT) calculated force as <see cref="Vector3"/></param>
        private static void GetForce(SceneItem item, ref Vector2 seekPosition, ref Vector2 startPosition, out Vector3 force)
        {
            Vector2 directionA;
            Vector2.Subtract(ref seekPosition, ref startPosition, out directionA);

            // 2/3/2009 - Set into Vector3 struct
            var directionB = new Vector3 { X = directionA.X, Y = 0, Z = directionA.Y };

            // 8/5/2009: Avoid NaN errors, by not normalizing Zero values!
            var desiredVelocity = Vector3Zero; 

            if (!directionB.Equals(Vector3Zero))
                Vector3.Normalize(ref directionB, out desiredVelocity);

            // 12/18/2008 - Updated to remove Overload Ops, since this slows down XBOX.           
            Vector3.Multiply(ref desiredVelocity, item.MaxSpeed, out desiredVelocity);

            var tmpVelocity = item.Velocity;
            Vector3.Subtract(ref desiredVelocity, ref tmpVelocity, out force);
            
        }
    }
}
