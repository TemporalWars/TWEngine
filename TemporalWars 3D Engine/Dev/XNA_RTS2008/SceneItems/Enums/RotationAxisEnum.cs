﻿#region File Description
//-----------------------------------------------------------------------------
// RotationAxisEnum.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace ImageNexus.BenScharbach.TWEngine.SceneItems.Enums
{
    // 10/16/2012: Updated enum to inherit from short value.
    // 6/11/2012
    /// <summary>
    /// The <see cref="RotationAxisEnum"/> is used to set the Axis to affect.
    /// </summary>
    public enum RotationAxisEnum : short
    {
        /// <summary>
        /// Affect the X-Axis.
        /// </summary>
        RotationOnX,
        /// <summary>
        /// Affect the Y-Axis.
        /// </summary>
        RotationOnY,
        /// <summary>
        /// Affect the Z-Axis.
        /// </summary>
        RotationOnZ,
    }
}