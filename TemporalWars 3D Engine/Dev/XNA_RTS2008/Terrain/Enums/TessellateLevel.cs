#region File Description
//-----------------------------------------------------------------------------
// TessellateLevel.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using TWEngine.Terrain.Structs;

namespace TWEngine.Terrain.Enums
{
    ///<summary>
    /// The level of tessellation to apply to a given <see cref="TerrainQuadPatch"/>.
    ///</summary>
    public enum TessellateLevel
    {
        ///<summary>
        /// Default original detail level for <see cref="TerrainQuadPatch"/>.  For example, if
        /// <see cref="LOD"/> is set to medium, then this is the base for level-1.
        ///</summary>
        Level1,
        ///<summary>
        /// One level higher of <see cref="LOD"/> for the given <see cref="TerrainQuadPatch"/>, than level-1.  For example, if
        /// <see cref="LOD"/> was medium at level-1, now it will be set to HIGH for level-2.
        ///</summary>
        Level2,
        ///<summary>
        /// One level higher of <see cref="LOD"/> for the given <see cref="TerrainQuadPatch"/>, than level-2.  For example, if
        /// <see cref="LOD"/> was high at level-2, now it will be set to ULTRA for level-3.
        ///</summary>
        Level3
    }
}
