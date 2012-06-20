#region File Description
//-----------------------------------------------------------------------------
// IOcean.cs
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
    /// The <see cref="Ocean"/> class creates realistic waves using animated vertices and is resource intensive.
    /// </summary>
    /// <remarks>The <see cref="Lake"/> class is created using a simple flat quad rectangle where the waves are represented
    /// using a bump map texture and animated over time.</remarks>
    public interface IOcean : IWaterBase
    { 
        /// <summary>
        /// Ocean wave amplitude
        /// </summary>
        float OceanWaveAmplitude { get; set; }
        /// <summary>
        /// Ocean wave frequency
        /// </summary>
        float OceanWaveFrequency { get; set; }
        /// <summary>
        /// Ocean bump-map height
        /// </summary>
        float OceanBumpHeight { get; set; }
        /// <summary>
        /// Water table height, where 0 is ground level.
        /// </summary>
        float WaterHeight { get; set; }
        /// <summary>
        /// Ocean deep color
        /// </summary>
        Vector4 OceanDeepColor { get; set; }
        /// <summary>
        /// Ocean shallow color
        /// </summary>
        Vector4 OceanShallowColor { get; set; }
        /// <summary>
        /// Ocean texture scale
        /// </summary>
        Vector2 OceanTextureScale { get; set; }
        /// <summary>
        /// Ocean wave speed
        /// </summary>
        Vector2 OceanWaveSpeed { get; set; }
        /// <summary>
        /// Ocean fresnel bias
        /// </summary>
        float OceanFresnelBias { get; set; }
        /// <summary>
        /// Ocean fresnel power
        /// </summary>
        float OceanFresnelPower { get; set; }
        /// <summary>
        /// Ocean HDR multiplier
        /// </summary>
        float OceanHDRMultiplier { get; set; }
        /// <summary>
        /// Ocean reflection amount, determines the influence of the reflection
        /// shown on the water surface.
        /// </summary>
        float OceanReflectionAmt { get; set; }
        /// <summary>
        /// Ocean water amount, determines the influence of the ocean floor shown.
        /// </summary>
        float OceanWaterAmt { get; set; }
        /// <summary>
        /// Ocean reflection color bias.
        /// </summary>
        Vector4 OceanReflectionColor { get; set; }
        /// <summary>
        /// Ocean sky amount, determines the influence of the sky clouds shown on
        /// the water surface.
        /// </summary>
        float OceanReflectionSkyAmt { get; set; }
    }
}