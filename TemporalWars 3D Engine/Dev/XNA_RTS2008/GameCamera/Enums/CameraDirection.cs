#region File Description
//-----------------------------------------------------------------------------
// CameraDirectionEnum.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace TWEngine.GameCamera.Enums
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

    ///<summary>
    /// Enumeration for direction the <see cref="Camera"/>
    /// can move in.
    ///</summary>
    public enum CameraDirectionEnum
    {
        // 6/15/2012 - Add new 'None' state.
        /// <summary>
        /// Represents NO camera direction is set.
        /// </summary>
        /// <remarks>>
        /// After each camera tick, this state is reset.
        /// </remarks>
        None,
        ///<summary>
        /// Scroll <see cref="Camera"/> in -Z axis.
        ///</summary>
        ScrollForward,
        ///<summary>
        /// Scroll <see cref="Camera"/> in +Z axis.
        ///</summary>
        ScrollBackward,
        ///<summary>
        /// Scroll <see cref="Camera"/> in -X axis.
        ///</summary>
        ScrollLeft,
        ///<summary>
        /// Scroll <see cref="Camera"/> in +X axis.
        ///</summary>
        ScrollRight,
        ///<summary>
        /// Scroll <see cref="Camera"/> in +Y axis.
        ///</summary>
        Up,
        ///<summary>
        /// Scroll <see cref="Camera"/> in -Y axis.
        ///</summary>
        Down,
        ///<summary>
        /// Rotates <see cref="Camera"/> clockwise.
        ///</summary>
        RotateRight,
        ///<summary>
        /// Rotates <see cref="Camera"/> counter-clockwise.
        ///</summary>
        RotateLeft
    }
}