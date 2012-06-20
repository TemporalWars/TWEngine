#region File Description
//-----------------------------------------------------------------------------
// MovementDirectionEnum.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace TWEngine.GameLevels.Enums
{
    // 6/6/2012
    /// <summary>
    /// The <see cref="MovementDirectionEnum"/> is used to set movement request for Directional Icon.
    /// </summary>
    public enum MovementDirectionEnum
    {
        /// <summary>
        /// No movement.
        /// </summary>
        Still,
        /// <summary>
        /// Move to right.
        /// </summary>
        Right,
        /// <summary>
        /// Move to Left.
        /// </summary>
        Left,
        /// <summary>
        /// Move up.
        /// </summary>
        Up,
        /// <summary>
        /// Move down.
        /// </summary>
        Down,
    }
}