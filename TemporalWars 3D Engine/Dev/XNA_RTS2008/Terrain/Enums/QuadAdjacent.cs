#region File Description
//-----------------------------------------------------------------------------
// QuadAdjacent.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.Terrain.Structs;

namespace ImageNexus.BenScharbach.TWEngine.Terrain.Enums
{
    ///<summary>
    /// The <see cref="QuadAdjacent"/> Enum is used to identify one of the 4 adjacent quad locations, 
    ///  relative to a single <see cref="TerrainQuadPatch"/>.
    ///</summary>
    public enum QuadAdjacent
    {
        ///<summary>
        /// Quad adjacent to the top of given <see cref="TerrainQuadPatch"/>.
        ///</summary>
        Top,
        ///<summary>
        /// Quad adjacent to the bottom of given <see cref="TerrainQuadPatch"/>.
        ///</summary>
        Bottom,
        ///<summary>
        /// Quad adjacent to the left of given <see cref="TerrainQuadPatch"/>.
        ///</summary>
        Left,
        ///<summary>
        /// Quad adjacent to the right of given <see cref="TerrainQuadPatch"/>.
        ///</summary>
        Right
    }
}
