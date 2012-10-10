#region File Description
//-----------------------------------------------------------------------------
// InstancedItemTransform.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.InstancedModels.Structs
{
    // 4/15/2009; updated to be a STRUCT!
    // 2/17/2009 - InstanceItem Transform class
    /// <summary>
    /// The <see cref="InstancedItemTransform"/> structure is used to obtain
    /// the <see cref="InstancedModelPart"/> current transform and absolute 
    /// transform.  Currently, this is primarily used for the Bullet spawn
    /// positions during attacks.
    /// </summary>
    public struct InstancedItemTransform
    {
        /// <summary>
        /// <see cref="InstancedModelPart"/> current transform matrix.
        /// </summary>
        public Matrix Transform;

        /// <summary>
        /// <see cref="InstancedModelPart"/> current Absolute transform matrix.
        /// </summary>
        public Matrix AbsoluteTransform;
    }
}


