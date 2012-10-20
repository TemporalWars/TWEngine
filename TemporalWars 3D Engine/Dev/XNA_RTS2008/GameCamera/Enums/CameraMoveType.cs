#region File Description
//-----------------------------------------------------------------------------
// CameraMoveType.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace ImageNexus.BenScharbach.TWEngine.GameCamera.Enums
{
    // 10/16/2012: Updated enum to inherit from short value.
    ///<summary>
    /// Enumeration for camera's movement type.
    ///</summary>
    public enum CameraMoveType : short
    {
        ///<summary>
        /// Move Position of the camera
        ///</summary>
        Position,
        ///<summary>
        /// Move Target of the camera
        ///</summary>
        Target
    }
}