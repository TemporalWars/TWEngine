#region File Description
//-----------------------------------------------------------------------------
// InstancedDataForDraw.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;

namespace TWEngine.InstancedModels.Structs
{
    // 4/15/2010 - Renamed from 'InstancedDataStruct', to 'InstancedDataForDraw'.
    // 8/28/2009 - InstancedDataStruct, used to pass to the graphicsDevice.
    ///<summary>
    /// The <see cref="InstancedDataForDraw"/> structure is used to carry the minimal
    /// information required to draw the given <see cref="InstancedModelPart"/>.
    ///</summary>
    public struct InstancedDataForDraw
    {
        ///<summary>
        /// <see cref="InstancedModelPart"/> transform.
        ///</summary>
        public Matrix Transform;
        
        ///<summary>
        /// <see cref="InstancedModelPart"/> player number and materialId.
        ///</summary>
        /// <remarks>MaterialId is no longer used, because now an entire Shader material is assigned to an <see cref="InstancedModelPart"/>.</remarks>
        public float PlayerNumberAndMaterialId; // Determine color using PlayerNumber, and ProcedurialMaterialId; stored as 2 parts.

        // 6/6/2010 - Add AccumTime, which is now required for explosions.
        ///<summary>
        /// Stores the current accumulative game time value, used specifically for the explosions.
        ///</summary>
        public float AccumElapsedTime;

        // 6/6/2010 - Add Projetile's Velocity, used for the explosions.
        ///<summary>
        /// Stores the last projectile's velocity, used specifically for explosions.
        ///</summary>
        public Vector3 ProjectileVelocity;

        ///<summary>
        /// Size in bytes of all data contain in this structure; requirement during the draw call.
        ///</summary>
        public const int SizeInBytes = (sizeof(float) * 16) + sizeof(float)*2 + sizeof(float)*3;

    }
}