#region File Description
//-----------------------------------------------------------------------------
// ScriptingActionScaleRequest.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using TWEngine.GameLevels.ChangeRequests.Enums;
using TWEngine.SceneItems;
using TWEngine.Terrain;

namespace TWEngine.GameLevels.ChangeRequests
{
    // 5/17/2012
    /// <summary>
    /// The <see cref="ScriptingActionScaleRequest"/> class is used to update the Scale of the given ItemType.
    /// </summary>
    public class ScriptingActionScaleRequest : ScriptingActionChangeRequestAbstract
    {
        // 6/7/2012
        private const int MaxScale = 100;

        #region Properties

        /// <summary>
        /// Gets the <see cref="ScaleTypeEnum"/> applied.
        /// </summary>
        public ScaleTypeEnum ScaleTypeApplied { get; private set; }

        /// <summary>
        /// Gets or sets the new Scale value to update.
        /// </summary>
        public float NewScale { get; private set; }

        #endregion

        /// <summary>
        /// Constructor to update the Scale of a <see cref="SceneItem"/>
        /// </summary>
        /// <param name="sceneItem">Instance of <see cref="SceneItem"/> to update.</param>
        /// <param name="newScale">Set to Scale to adjust to.</param>
        /// <param name="scaleType">Set the ScaleType to use.</param>
        /// <param name="instancedItemPickedIndex">Set to the current index value for ScenaryItems.</param>
        public ScriptingActionScaleRequest(SceneItem sceneItem, float newScale, ScaleTypeEnum scaleType, int instancedItemPickedIndex)
            :base(sceneItem, instancedItemPickedIndex)
        {
            NewScale = newScale;
            ScaleTypeApplied = scaleType;
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

            // Apply scale operation
            DoScaleUpdateCheck(this, gameTime);
        }

        /// <summary>
        /// Helper method used to update the current <see cref="SceneItem"/> scale.
        /// </summary>
        private static void DoScaleUpdateCheck(ScriptingActionScaleRequest scriptingActionScaleRequest, GameTime gameTime)
        {
            var sceneItemToUpdate = scriptingActionScaleRequest.SceneItemToUpdate;
            if (sceneItemToUpdate == null) return;

            // If ScenaryItem, then set to the proper index value.
            var scenaryItemScene = sceneItemToUpdate as ScenaryItemScene;
            if (scenaryItemScene != null)
            {
                scenaryItemScene.InstancedItemPickedIndex = scriptingActionScaleRequest.InstancedItemPickedIndex;
            }

            var scale = sceneItemToUpdate.Scale;
            var currentScale = scale.Y;

            // Apply delta change.
            switch (scriptingActionScaleRequest.ScaleTypeApplied)
            {
                case ScaleTypeEnum.Grow:
                    DoDeltaUpdate(scriptingActionScaleRequest, gameTime, false);
                    break;
                case ScaleTypeEnum.Shrink:
                    DoDeltaUpdate(scriptingActionScaleRequest, gameTime, true);
                    break;
            }

            currentScale += scriptingActionScaleRequest.Delta;
            TerrainQuadTree.UpdateSceneryCulledList = true;
            MathHelper.Clamp(currentScale, 0.01f, MaxScale);

            scale.X = scale.Y = scale.Z = currentScale;
            sceneItemToUpdate.Scale = scale;

            // check if completed
            switch (scriptingActionScaleRequest.ScaleTypeApplied)
            {
                case ScaleTypeEnum.Grow:
                    if (currentScale >= scriptingActionScaleRequest.NewScale)
                        scriptingActionScaleRequest.IsCompleted = true;
                    break;
                case ScaleTypeEnum.Shrink:
                    if (currentScale <= scriptingActionScaleRequest.NewScale)
                        scriptingActionScaleRequest.IsCompleted = true;
                    break;
            }
        }
    }
}