#region File Description
//-----------------------------------------------------------------------------
// CameraDirectionEnum.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;

namespace ImageNexus.BenScharbach.TWEngine.GameCamera.Enums
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.GameCamera.Enums"/> namespace contains the common enumerations
    /// which make up the entire <see cref="TWEngine.GameCamera"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    // 6/28/2012 - Updated to include 'Flags' attribute
    ///<summary>
    /// Enumeration for direction the <see cref="Camera"/>
    /// can move in.
    ///</summary>
    [Flags]
    public enum CameraDirectionEnum
    {
        // 6/15/2012 - Add new 'None' state.
        /// <summary>
        /// Represents NO camera direction is set.
        /// </summary>
        /// <remarks>>
        /// After each camera tick, this state is reset.
        /// </remarks>
        None = 0,
        ///<summary>
        /// Scroll <see cref="Camera"/> in -Z axis.
        ///</summary>
        ScrollForward = 1,
        ///<summary>
        /// Scroll <see cref="Camera"/> in +Z axis.
        ///</summary>
        ScrollBackward = 2,
        ///<summary>
        /// Scroll <see cref="Camera"/> in -X axis.
        ///</summary>
        ScrollLeft = 4,
        ///<summary>
        /// Scroll <see cref="Camera"/> in +X axis.
        ///</summary>
        ScrollRight = 8,
        ///<summary>
        /// Scroll <see cref="Camera"/> in +Y axis.
        ///</summary>
        Up = 16,
        ///<summary>
        /// Scroll <see cref="Camera"/> in -Y axis.
        ///</summary>
        Down = 32,
        ///<summary>
        /// Rotates <see cref="Camera"/> clockwise.
        ///</summary>
        RotateRight = 64,
        ///<summary>
        /// Rotates <see cref="Camera"/> counter-clockwise.
        ///</summary>
        RotateLeft = 128
    }
}