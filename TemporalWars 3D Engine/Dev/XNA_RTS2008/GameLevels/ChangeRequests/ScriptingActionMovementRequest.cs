#region File Description
//-----------------------------------------------------------------------------
// ScriptingActionMovementRequest.cs
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
    /// The <see cref="ScriptingActionMovementRequest"/> class is used to update the position of some <see cref="SceneItem"/>.
    /// </summary>
    public class ScriptingActionMovementRequest : ScriptingActionMovementRequestAbstract
    {
        #region Properties

        /// <summary>
        /// Gets or set to keep the item on the ground during the movement.
        /// </summary>
        public bool KeepOnGround { get; set; }

        #endregion

        /// <summary>
        /// Constructor, which initializes a movement from a <paramref name="startPosition"/> to some 
        /// goal position set in the <see cref="SceneItem.MoveToWayPosition"/>
        /// </summary>
        /// <param name="sceneItem">Instance of <see cref="SceneItem"/>.</param>
        /// <param name="instancedItemPickedIndex">Index value to the correct scenaryItem instance.</param>
        /// <param name="moveForce">Amount of force to apply.</param>
        /// <param name="startPosition">Set to position item should start from as <see cref="Vector3"/>; if Zero, then the <see cref="SceneItem"/> current position will be used.</param>
        public ScriptingActionMovementRequest(SceneItem sceneItem, int instancedItemPickedIndex, float moveForce, Vector3 startPosition) 
            : this(sceneItem, instancedItemPickedIndex, moveForce)
        {
            // if given start position is zero, then use the sceneitem's current position.
            _startPosition = (startPosition == Vector3Zero) ? sceneItem.Position : startPosition;
            _goalPosition = sceneItem.MoveToWayPosition;
            Delta = 0;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sceneItem">Instance of <see cref="SceneItem"/>.</param>
        /// <param name="instancedItemPickedIndex">Index value to the correct scenaryItem instance.</param>
        /// <param name="moveForce">Amount of force to apply.</param>
        public ScriptingActionMovementRequest(SceneItem sceneItem, int instancedItemPickedIndex, float moveForce)
            : base(sceneItem, instancedItemPickedIndex, moveForce)
        {
        }

        /// <summary>
        /// Updates the current change request.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Keep item on ground?
            if (!KeepOnGround)
            {
                return;
            }

            // Now adjust for height to keep item on ground.
            DoHeightAdjustment(this, gameTime);
        }

        /// <summary>
        /// Helper method which should be overriden to update the 'Force' and provide a current velocity.
        /// </summary>
        /// <param name="scriptingActionMovementRequest">Instance of <see cref="ScriptingActionMovementRequestAbstract"/></param>
        /// <param name="gameTime">Instance of <see cref="GameTime"/></param>
        /// <param name="currentVelocity">(OUT) <see cref="Vector3"/> as current velocity</param>
        protected override void DoForceUpdateCheck(ScriptingActionMovementRequestAbstract scriptingActionMovementRequest, GameTime gameTime, out Vector3 currentVelocity)
        {
            currentVelocity = Vector3Zero;

            var sceneItemToUpdate = scriptingActionMovementRequest.SceneItemToUpdate;
            if (sceneItemToUpdate == null) return;

            // If ScenaryItem, then set to the proper index value.
            var scenaryItemScene = sceneItemToUpdate as ScenaryItemScene;
            if (scenaryItemScene != null)
            {
                scenaryItemScene.InstancedItemPickedIndex = scriptingActionMovementRequest.InstancedItemPickedIndex;
            }

            // Call calculate to get total movement force
            UpdateVelocityWithMoveForce(scriptingActionMovementRequest, gameTime, out currentVelocity);
        }

        // 5/28/2012
        /// <summary>
        /// Checks the current <see cref="SceneItem"/>'s position and sets to the proper height at that position on the terrain.
        /// </summary>
        /// <param name="scriptingActionMovementRequest">Instance of <see cref="ScriptingActionMovementRequest"/>.</param>
        /// <param name="gameTime">Instance of <see cref="GameTime"/></param>
        private static void DoHeightAdjustment(ScriptingActionMovementRequest scriptingActionMovementRequest, GameTime gameTime)
        {
            var sceneItemToUpdate = scriptingActionMovementRequest.SceneItemToUpdate;
            var sceneItemOwnerPosition = sceneItemToUpdate.Position;

            // Get height for given position
            sceneItemOwnerPosition.Y = TerrainData.GetTerrainHeight(sceneItemOwnerPosition.X, sceneItemOwnerPosition.Z);

            sceneItemToUpdate.Position = sceneItemOwnerPosition;
        }
    }
}