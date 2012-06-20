#region File Description
//-----------------------------------------------------------------------------
// ScriptingActionRotationRequest.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using TWEngine.GameLevels.ChangeRequests.Enums;
using TWEngine.SceneItems;
using TWEngine.Terrain;

namespace TWEngine.GameLevels.ChangeRequests
{
    // 5/19/2012
    /// <summary>
    ///  The <see cref="ScriptingActionRotationRequest"/> class is used to update the Rotation of the given ItemType.
    /// </summary>
    public class ScriptingActionRotationRequest : ScriptingActionChangeRequestAbstract
    {
        private float _currentRotationValue;
        private readonly float _rotationTimeMax;
        private int _timeElapsed;

        #region Properties

        /// <summary>
        /// Gets the <see cref="RotationTypeEnum"/> applied.
        /// </summary>
        public RotationTypeEnum RotationTypeApplied { get; private set; }

        /// <summary>
        /// Gets the <see cref="RotationDirectionEnum"/> applied.
        /// </summary>
        public RotationDirectionEnum RotationDirectionApplied { get; private set; }

        #endregion

        /// <summary>
        /// Constructor to create a new rotation of a <see cref="SceneItem"/>
        /// </summary>
        /// <param name="sceneItem">Instance of <see cref="SceneItem"/></param>
        /// <param name="rotationDirection">Set to the rotation direction to use.</param>
        /// <param name="rotationType">Set to the rotation type to use.</param>
        /// <param name="rotationTimeMax">Set to length of given rotation in milliseconds; 0 implies infinite.</param>
        /// <param name="instancedItemPickedIndex">Index value to the correct scenaryItem instance.</param>
        public ScriptingActionRotationRequest(SceneItem sceneItem, RotationTypeEnum rotationType, RotationDirectionEnum rotationDirection, 
                                              float rotationTimeMax, int instancedItemPickedIndex) 
            : base(sceneItem, instancedItemPickedIndex)
        {
            if (rotationTimeMax < 0)
                throw new ArgumentOutOfRangeException("rotationTimeMax", "Rotation time length MUST be zero or greater.");

            RotationTypeApplied = rotationType;
            RotationDirectionApplied = rotationDirection;
            _rotationTimeMax = rotationTimeMax;
        }

        /// <summary>
        /// Updates the current change request.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            if (TerminateAction || IsCompleted)
            {
                return;
            }

            // Apply rotation operation
            DoRotationUpdateCheck(this, gameTime);

            // If Non-Stop operation, then return.
            if (Math.Abs(_rotationTimeMax - 0) < float.Epsilon) return;

            // Otherwise, check if time is up
            _timeElapsed += gameTime.ElapsedGameTime.Milliseconds;
            if (_timeElapsed >= _rotationTimeMax)
            {
                IsCompleted = true;
            }
        }

        /// <summary>
        /// Helper method used to update the current <see cref="SceneItem"/> scale.
        /// </summary>
        private static void DoRotationUpdateCheck(ScriptingActionRotationRequest scriptingActionRotationRequest, GameTime gameTime)
        {
            var sceneItemToUpdate = scriptingActionRotationRequest.SceneItemToUpdate;
            if (sceneItemToUpdate == null) return;

            // If ScenaryItem, then set to the proper index value.
            var scenaryItemScene = sceneItemToUpdate as ScenaryItemScene;
            if (scenaryItemScene != null)
            {
                scenaryItemScene.InstancedItemPickedIndex = scriptingActionRotationRequest.InstancedItemPickedIndex;
            }

            // Update the Rotation Delta
            switch (scriptingActionRotationRequest.RotationDirectionApplied)
            {
                case RotationDirectionEnum.Forward:
                    DoDeltaUpdate(scriptingActionRotationRequest, gameTime, false);
                    break;
                case RotationDirectionEnum.Reverse:
                    DoDeltaUpdate(scriptingActionRotationRequest, gameTime, true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Update current rotation value
            scriptingActionRotationRequest._currentRotationValue += scriptingActionRotationRequest.Delta;

            // Update the Quaternion rotation struct.
            TerrainQuadTree.UpdateSceneryCulledList = true;
            sceneItemToUpdate.Rotation = DoQuaternionRotation(scriptingActionRotationRequest.RotationTypeApplied, scriptingActionRotationRequest._currentRotationValue);
        }

        /// <summary>
        /// Returns a new Quaternion rotation based on the given parameters.
        /// </summary>
        /// <param name="rotationType">Rotation type to use.</param>
        /// <param name="delta">Rotation delta.</param>
        /// <returns>New <see cref="Quaternion"/></returns>
        internal static Quaternion DoQuaternionRotation(RotationTypeEnum rotationType, float delta)
        {
            Matrix rotationAxis;
            switch (rotationType)
            {
                case RotationTypeEnum.RotateOnY:
                    rotationAxis = Matrix.CreateRotationY(MathHelper.ToRadians(delta));
                    break;
                case RotationTypeEnum.RotateOnX:
                    rotationAxis = Matrix.CreateRotationX(MathHelper.ToRadians(delta));
                    break;
                case RotationTypeEnum.RotateOnZ:
                    rotationAxis = Matrix.CreateRotationZ(MathHelper.ToRadians(delta));
                    break;
                default:
                    throw new ArgumentOutOfRangeException("rotationType");
            }

            Quaternion newQuaternion;
            Quaternion.CreateFromRotationMatrix(ref rotationAxis, out newQuaternion);

            return newQuaternion;
        }

        /// <summary>
        /// Returns a new Quaternion rotation based on the given parameters.
        /// </summary>
        /// <param name="rotationForce">Rotation force on all 3-axis to apply.</param>
        /// <param name="rotationVelocity">Rotation velocity</param>
        /// <param name="elapsedGameTime">Instance of <see cref="GameTime"/></param>
        /// <returns>New <see cref="Quaternion"/></returns>
        internal static Quaternion DoQuaternionRotation(ref Vector3 rotationForce, ref Vector3 rotationVelocity, ref TimeSpan elapsedGameTime)
        {
            Vector3 currentVelocity;
            Vector3.Multiply(ref rotationForce, (float)elapsedGameTime.TotalSeconds, out currentVelocity);
            Vector3.Add(ref rotationVelocity, ref currentVelocity, out rotationVelocity);

            var currentYaw = rotationVelocity.X; // X-Axis
            var currentPitch = rotationVelocity.Y; // Y-Axis
            var currentRoll = rotationVelocity.Z; // Z-Axis

            Quaternion newQuaternion;
            Quaternion.CreateFromYawPitchRoll(currentYaw, currentPitch, currentRoll, out newQuaternion);

            return newQuaternion;
        }
    }
}