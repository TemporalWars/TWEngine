#region File Description
//-----------------------------------------------------------------------------
// ScriptingActionChangeRequestEnum.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using TWEngine.SceneItems;

namespace TWEngine.GameLevels.ChangeRequests.Enums
{
    // 5/22/2012
    /// <summary>
    /// The <see cref="ScriptingActionChangeRequestEnum"/>.
    /// </summary>
    public enum ScriptingActionChangeRequestEnum
    {
        /// <summary>
        /// Request to rotate the <see cref="SceneItem"/>.
        /// </summary>
        RotationRequest = 2,
        /// <summary>
        /// Request to Scale the <see cref="SceneItem"/>.
        /// </summary>
        ScaleRequest = 4,
        /// <summary>
        /// Request to Toss-Movement the <see cref="SceneItem"/>.
        /// </summary>
        TossMovementRequest = 8,
        /// <summary>
        /// Request to Move the <see cref="SceneItem"/>
        /// </summary>
        MovementRequest = 16,
        /// <summary>
        /// Request to move the <see cref="SceneItem"/> on a given waypoint path
        /// </summary>
        PathMovementRequest = 32,
    }
}