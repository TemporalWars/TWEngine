#region File Description
//-----------------------------------------------------------------------------
// ScriptingActionMovementRequestAbstract.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using TWEngine.SceneItems;
using TWEngine.Terrain;

namespace TWEngine.GameLevels.ChangeRequests
{
    // 5/28/2012
    /// <summary>
    /// The abstract <see cref="ScriptingActionMovementRequestAbstract"/> class is used to create movement classes which at the
    /// base level updates the rotation and position of the item.
    /// </summary>
    public abstract class ScriptingActionMovementRequestAbstract : ScriptingActionChangeRequestAbstract
    {
        protected const float LinearCeiling = 1000.0f;
        private Vector3 _rotationForce;
        private Vector3 _rotationVelocity;
        protected bool ApplyGroundFriction;
        protected Vector3 _goalPosition;
        protected float _linearMovementAmount;
        protected Vector3 _startPosition;


        #region Properties

        /// <summary>
        /// Gets the current goal position.
        /// </summary>
        public Vector3 GoalPosition
        {
            get { return _goalPosition; }
        }

        /// <summary>
        /// Gets or sets the 3-axis rotation force.
        /// </summary>
        public Vector3 RotationForce
        {
            get { return _rotationForce; }
            set { _rotationForce = value; }
        }

        /// <summary>
        /// Gets the current 3-axis rotation velocity.
        /// </summary>
        public Vector3 RotationVelocity
        {
            get { return _rotationVelocity; }
            private set { _rotationVelocity = value; }
        }

        /// <summary>
        /// Gets the start position for this movement request.
        /// </summary>
        public Vector3 StartPosition
        {
            get { return _startPosition; }
            private set { _startPosition = value; }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sceneItem">Instance of <see cref="SceneItem"/>.</param>
        /// <param name="instancedItemPickedIndex">Index value to the correct scenaryItem instance.</param>
        /// <param name="moveForce">Amount of force to apply.</param>
        protected ScriptingActionMovementRequestAbstract(SceneItem sceneItem, int instancedItemPickedIndex, float moveForce) 
            : base(sceneItem, instancedItemPickedIndex)
        {
           
            DeltaMagnitude = moveForce;
        }

        /// <summary>
        /// Updates the current change request.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (TerminateAction || IsCompleted)
            {
                return;
            }

            // Apply force operation
            Vector3 currentVelocity;
            DoForceUpdateCheck(this, gameTime, out currentVelocity);

            // Apply movement operation
            DoMovementUpdateCheck(this, gameTime, ref currentVelocity);

            // Check if throw is completed
            if (SceneItemToUpdate.HasReachedMoveToPosition(ref _goalPosition))
            {
                // check if at ground level
                if (SceneItemToUpdate.Position.Y <= _goalPosition.Y)
                    IsCompleted = true;
            }
        }

        /// <summary>
        /// Helper method which should be overriden to update the 'Force' and provide a current velocity.
        /// </summary>
        /// <param name="scriptingActionMovementRequest">Instance of <see cref="ScriptingActionMovementRequestAbstract"/></param>
        /// <param name="gameTime">Instance of <see cref="GameTime"/></param>
        /// <param name="currentVelocity">(OUT) <see cref="Vector3"/> as current velocity</param>
        protected virtual void DoForceUpdateCheck(ScriptingActionMovementRequestAbstract scriptingActionMovementRequest, GameTime gameTime, out Vector3 currentVelocity)
        {
            // Call calculate to get total Throw force
            var elapsedGameTime = gameTime.ElapsedGameTime;

            currentVelocity = Vector3Zero;
        }

        /// <summary>
        /// Helper method used to update the current <see cref="SceneItem"/> rotation and position.
        /// </summary>
        /// <param name="scriptingActionMovementRequest">Instance of <see cref="ScriptingActionMovementRequestAbstract"/></param>
        /// <param name="gameTime">Instance of <see cref="GameTime"/></param>
        /// <param name="currentVelocity"><see cref="Vector3"/> as current velocity</param>
        protected virtual void DoMovementUpdateCheck(ScriptingActionMovementRequestAbstract scriptingActionMovementRequest, GameTime gameTime, ref Vector3 currentVelocity)
        {
            var sceneItemToUpdate = scriptingActionMovementRequest.SceneItemToUpdate;
            if (sceneItemToUpdate == null) return;

            // If ScenaryItem, then set to the proper index value.
            var scenaryItemScene = sceneItemToUpdate as ScenaryItemScene;
            if (scenaryItemScene != null)
            {
                scenaryItemScene.InstancedItemPickedIndex = scriptingActionMovementRequest.InstancedItemPickedIndex;
            }

            // update 3-axis rotation velocity.
            var elapsedGameTime = gameTime.ElapsedGameTime;
            UpdateVelocityForRotationForce(scriptingActionMovementRequest, ref elapsedGameTime);

            // update Position using given velocity           
            UpdatePositionUsingVelocity(scriptingActionMovementRequest, ref currentVelocity);
        }

        /// <summary>
        /// Helper method used to apply the rotation force.
        /// </summary>
        protected static void UpdateVelocityForRotationForce(ScriptingActionMovementRequestAbstract scriptingActionMovementRequest, ref TimeSpan elapsedGameTime)
        {
            if (scriptingActionMovementRequest.RotationForce.Equals(Vector3Zero))
            {
                return;
            }

            // apply ground friction when at ground level.
            if (scriptingActionMovementRequest.ApplyGroundFriction)
            {
                // update force
                Vector3.Multiply(ref scriptingActionMovementRequest._rotationForce, 0.10f * (float)elapsedGameTime.TotalSeconds,
                                 out scriptingActionMovementRequest._rotationForce);
                // update velocity
                Vector3.Multiply(ref scriptingActionMovementRequest._rotationVelocity, 0.05f * (float)elapsedGameTime.TotalSeconds,
                                 out scriptingActionMovementRequest._rotationVelocity);

                scriptingActionMovementRequest.ApplyGroundFriction = false;
            }

            scriptingActionMovementRequest.SceneItemToUpdate.Rotation =
                ScriptingActionRotationRequest.DoQuaternionRotation(ref scriptingActionMovementRequest._rotationForce,
                                                                    ref scriptingActionMovementRequest._rotationVelocity,
                                                                    ref elapsedGameTime);
        }

        /// <summary>
        /// Updates the current position with the given velocity.
        /// </summary>
        /// <param name="scriptingActionMovementRequest"> </param>
        /// <param name="velocity"><see cref="Vector3"/> velocity to use</param>
        protected static void UpdatePositionUsingVelocity(ScriptingActionMovementRequestAbstract scriptingActionMovementRequest, ref Vector3 velocity)
        {
            var sceneItemToUpdate = scriptingActionMovementRequest.SceneItemToUpdate;
            var sceneItemOwnerPosition = sceneItemToUpdate.Position;

            // Once velocity falls below 0.1, stop updating position.
            if (velocity.LengthSquared() <= 0.1)
            {
                return;
            }

            Vector3.Add(ref sceneItemOwnerPosition, ref velocity, out sceneItemOwnerPosition);

            // check if new position is beyond ground level
            if (sceneItemOwnerPosition.Y < scriptingActionMovementRequest._goalPosition.Y)
            {
                // yes, so set back at ground level.
                sceneItemOwnerPosition.Y = scriptingActionMovementRequest._goalPosition.Y;
                scriptingActionMovementRequest.ApplyGroundFriction = true;
            }

            TerrainQuadTree.UpdateSceneryCulledList = true;
            sceneItemToUpdate.Position = sceneItemOwnerPosition;
        }


        /// <summary>
        /// Helper method which calculates the movement velocity.
        /// </summary>
        /// <param name="scriptingActionMovementRequest">Instance of <see cref="ScriptingActionMovementRequest"/>.</param>
        /// <param name="gameTime">Instance of <see cref="GameTime"/></param>
        /// <param name="currentVelocity">(OUT) new calculated velocity</param>
        protected static void UpdateVelocityWithMoveForce(ScriptingActionMovementRequestAbstract scriptingActionMovementRequest, GameTime gameTime, out Vector3 currentVelocity)
        {
            // calculate the amount of linear movement.
            DoDeltaUpdate(scriptingActionMovementRequest, gameTime, false);
            scriptingActionMovementRequest._linearMovementAmount += scriptingActionMovementRequest.Delta;
            var lerpAmount = scriptingActionMovementRequest._linearMovementAmount/LinearCeiling;

            // calculate interpolation of movement request
            Vector3 newPosition;
            Vector3.Lerp(ref scriptingActionMovementRequest._startPosition,
                         ref scriptingActionMovementRequest._goalPosition, lerpAmount, out newPosition);

            // since base class calculate position for us, need to create velocity output.
            Vector3 currentPosition = scriptingActionMovementRequest.SceneItemToUpdate.Position;
            Vector3.Subtract(ref newPosition, ref currentPosition, out currentVelocity);
        }
    }
}