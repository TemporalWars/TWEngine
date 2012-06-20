#region File Description
//-----------------------------------------------------------------------------
// PerlinNoiseData.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace TWEngine.Terrain.Structs
{
    // 5/15/2009
    /// <summary>
    /// Contains perlin noise data, for use with the
    /// HeightMaps and texture splatting methods.
    /// </summary>   
    public struct PerlinNoiseData
    {
// ReSharper disable InconsistentNaming
        ///<summary>
        /// Perlin-Noise seed value.
        ///</summary>
        public int seed;
        ///<summary>
        /// Perlin-Noise noise size value.
        ///</summary>
        public float noiseSize;
        ///<summary>
        /// Perlin-Noise persistence value.
        ///</summary>
        public float persistence;
        ///<summary>
        /// Perlin-Noise octaves value.
        ///</summary>
        public int octaves;
// ReSharper restore InconsistentNaming
    }
}
