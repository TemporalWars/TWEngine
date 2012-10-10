#region File Description
//-----------------------------------------------------------------------------
// SaveTerrainData.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using ImageNexus.BenScharbach.TWEngine.Shadows.Structs;
using ImageNexus.BenScharbach.TWEngine.Terrain.Structs;
using ImageNexus.BenScharbach.TWEngine.Water.Structs;
using ImageNexus.BenScharbach.TWLate.AStarInterfaces.AStarAlgorithm.Structs;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.Utilities.Structs
{
#pragma warning disable 1587
    ///<summary>
    /// The <see cref="SaveTerrainData"/> struct is used to save the <see cref="Terrain"/> attributes, like
    /// the map width, height, ambient color, texture collections, etc.
    ///</summary>
#pragma warning restore 1587
#if !XBOX360
    [Serializable] 
#endif
    public struct SaveTerrainData
    {
#pragma warning disable 1591
        public int MapWidth;
        public int MapHeight;
        public Vector3 AmbientColorLayer1;
        public float AmbientPowerLayer1;
        public Vector3 SpecularColorLayer1;
        public float SpecularPowerLayer1;
        public Vector3 AmbientColorLayer2;
        public float AmbientPowerLayer2;
        public Vector3 SpecularColorLayer2;
        public float SpecularPowerLayer2;
        public float AlphaLy1Percent;
        public float AlphaLy2Percent;
        public float AlphaLy3Percent;
        public float AlphaLy4Percent;
// ReSharper disable InconsistentNaming
        public List<int> quadParentsTessellated;
        public List<int> quadLOD3;
        public List<TexturesGroupData> textureGroupData1;
        public List<TexturesGroupData> textureGroupData2;
        public List<TexturesAtlasData> texturesAtlasData; // 1/20/2009
        public WaterData waterData; // 1/21/2009
        public List<PathNodeForSaving> blockingData; // 3/4/2009
        public PerlinNoiseData perlinNoiseDataTexture1To2Mix_Layer1; // 5/15/2009
        public PerlinNoiseData perlinNoiseDataTexture1To2Mix_Layer2; // 5/15/2009
        public Vector3 lightPosition; // 6/5/2009
        public bool IsRaining; // 6/5/2009
        public ShadowMapData shadowMapData; // 6/5/2009
        public bool SaveSelectableItemsWithMap; // 10/7/2009 - Single Player levels (Scripting)
// ReSharper restore InconsistentNaming 
#pragma warning restore 1591
    }
}