#region File Description
//-----------------------------------------------------------------------------
// CursorTextureEnum.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace ImageNexus.BenScharbach.TWEngine.Common.Enums
{
    /// <summary>
    /// Mouse cursor texture to show, depending
    /// if ground area is 'Blocked' or 'Normal' mode.
    /// </summary>
    enum CursorTextureEnum
    {
        /// <summary>
        /// Normal moveable area, for units to transition to.
        /// </summary>
        Normal,
        /// <summary>
        /// Blocked area, where units are not allowed to move to.
        /// </summary>
        Blocked
    }
}