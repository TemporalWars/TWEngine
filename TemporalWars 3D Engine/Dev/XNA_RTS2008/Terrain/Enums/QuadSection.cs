#region File Description
//-----------------------------------------------------------------------------
// QuadSection.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.Terrain.Structs;

namespace ImageNexus.BenScharbach.TWEngine.Terrain.Enums
{
    ///<summary>
    /// A <see cref="TerrainQuadPatch"/> is split up into 4 quadridants.  The <see cref="QuadSection"/> Enum
    /// is used to identify a specific quadridant.
    ///</summary>
    public enum QuadSection
    {
        ///<summary>
        /// The <see cref="TerrainQuadPatch"/> top left quadridant.
        ///</summary>
        TopLeft,
        ///<summary>
        /// The <see cref="TerrainQuadPatch"/> top right quadridant.
        ///</summary>
        TopRight,
        ///<summary>
        /// The <see cref="TerrainQuadPatch"/> bottom left quadridant.
        ///</summary>
        BottomLeft,
        ///<summary>
        /// The <see cref="TerrainQuadPatch"/> bottom right quadridant.
        ///</summary>
        BottomRight
    }
}
