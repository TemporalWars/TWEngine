#region File Description
//-----------------------------------------------------------------------------
// TerrainLightingType.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace ImageNexus.BenScharbach.TWEngine.Terrain.Enums
{
    // 12/12/2009
    /// <summary>
    /// The <see cref="TerrainLightingType"/> Enum Controls the material LightingType to use during rendering, 
    /// specifically for the <see cref="TWEngine.Terrain"/>.
    /// </summary>
    public enum TerrainLightingType
    {
        ///<summary>
        /// Applies the plastic material lighting type.
        ///</summary>
        Plastic = 0,
        ///<summary>
        /// Applies the metal material lighting type.
        ///</summary>
        Metal = 1,
        ///<summary>
        /// Applies the blinn material lighting type.
        ///</summary>
        Blinn = 2,
        ///<summary>
        /// Applies the glossy material lighting type.
        ///</summary>
        Glossy = 3
    }
}