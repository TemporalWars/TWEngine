#region File Description
//-----------------------------------------------------------------------------
// DrawTransformsType.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using TWEngine.Explosions;
using TWEngine.Explosions.Structs;

namespace TWEngine.InstancedModels.Enums
{
    ///<summary>
    /// <see cref="DrawTransformsType"/>  enumeration, which
    /// <see cref="InstancedModelPart"/> class uses during each
    /// draw cycle, to determine the batch instances to use for
    /// the given cycle.
    ///</summary>
    public enum DrawTransformsType
    {
// ReSharper disable InconsistentNaming
        ///<summary>
        /// Draws ALL instances for current batch.
        ///</summary>
        NormalTransforms_All,
        ///<summary>
        /// Draws ONLY culled instances for current batch.
        ///</summary>
        /// <remarks>Culled = Out of sight of the camera view</remarks>
        NormalTransforms_Culled,
        ///<summary>
        /// Draws ONLY <see cref="ExplosionItem"/> type instances for current batch.
        ///</summary>
        /// <remarks>This ONLY occurs during the Explosion animation for the current item</remarks>
        ExplosionTransforms_Culled
// ReSharper restore InconsistentNaming
    }
}