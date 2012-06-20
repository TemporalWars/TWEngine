#region File Description
//-----------------------------------------------------------------------------
// ILake.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using TWEngine.Water;

namespace TWEngine.Interfaces
{
    /// <summary>
    /// The <see cref="Lake"/> class is created using a simple flat quad rectangle, where the waves are represented
    /// using a bump map texture and animated over time. The <see cref="Lake"/> class is not resource intensive.
    /// </summary>
    /// <remarks>Use the <see cref="Ocean"/>class to create realistic waves using animated vertices.</remarks>
    public interface ILake : IWaterBase
    {
        /// <summary>
        /// Wave length
        /// </summary>
        float WaveLength { get; set; }
        /// <summary>
        /// Wave height 
        /// </summary>
        float WaveHeight { get; set; }
        /// <summary>
        ///  Wave speed
        /// </summary>
        float WaveSpeed { get; set; }
        /// <summary>
        /// Wind direction as Vector3.
        /// </summary>
        Vector3 WindDirection { get; set; }
        /// <summary>
        /// Wind force as float.
        /// </summary>
        float WindForce { get; set; }
        /// <summary>
        /// Sunlight direction as Vector3.
        /// </summary>
        Vector3 SunlightDirection { get; set; }
        /// <summary>
        /// Color tone of water.
        /// </summary>
        Vector4 DullColor { get; set; }
        /// <summary>
        /// Use distort vertices option when drawing water.
        /// </summary>
        bool UseDistortVertices { get; set; }
        /// <summary>
        /// Frequency of waves
        /// </summary>
        Vector4 WaveFreqs { get; set; }
        /// <summary>
        /// Wave height
        /// </summary>
        Vector4 WaveHeights { get; set; }
        /// <summary>
        /// Wave length
        /// </summary>
        Vector4 WaveLengths { get; set; }
        ///<summary>
        /// Sets direction of waves.
        ///</summary>
        /// <param name="value">Collection of <see cref="Vector2"/>.</param>
        void SetWaveDirs(Vector2[] value);
        /// <summary>
        /// Returns array of Vector2 for wave directions.
        /// </summary>
        /// <returns>Array of Vector2</returns>
        Vector2[] GetWaveDirs();
    }
}