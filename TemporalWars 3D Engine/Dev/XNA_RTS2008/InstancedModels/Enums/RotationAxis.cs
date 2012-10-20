#region File Description
//-----------------------------------------------------------------------------
// RotationAxis.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums
{
    // 10/16/2012: Updated enum to inherit from short value.
    // 5/22/2009
    ///<summary>
    /// <see cref="RotationAxis"/> Enum.
    ///</summary>
    public enum RotationAxis : short
    {
        ///<summary>
        /// Rotate on X-Axis
        ///</summary>
        X = 1,
        ///<summary>
        /// Rotate on Y-Axis
        ///</summary>
        Y = 2,
        ///<summary>
        /// Rotate on Z-Axis
        ///</summary>
        Z = 3
    }
}