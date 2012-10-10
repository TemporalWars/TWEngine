#region File Description
//-----------------------------------------------------------------------------
// WaterData.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.Water.Enums;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.Water.Structs
{
#pragma warning disable 1587
    ///<summary>
    /// The <see cref="WaterData"/> struct stores the water's attributes, like
    /// wind direction, wind force, sun-light direction, wave speed, etc.
    ///</summary>
#pragma warning restore 1587
#if !XBOX360
    [Serializable]
#endif
    public struct WaterData
    {
        ///<summary>
        /// Set water component use for current map.
        ///</summary>
        public bool UseWater;

        // 6/1/2010
        /// <summary>
        /// The <see cref="WaterType"/> Enum to use.
        /// </summary>
        public WaterType WaterTypeToUse;

        /// <summary>
        /// Color tone of water.
        /// </summary>
        public Vector4 DullColor;

        /// <summary>
        /// Wind direction as Vector3.
        /// </summary>
        public Vector3 WindDirection;

        /// <summary>
        /// Wind force as float.
        /// </summary>
        public float WindForce;

        /// <summary>
        /// Sunlight direction as Vector3.
        /// </summary>
        public Vector3 SunlightDirection;

        /// <summary>
        /// Wave speed
        /// </summary>
        public float WaveSpeed;

        /// <summary>
        /// Wave length
        /// </summary>
        public float Wavelength;

        /// <summary>
        /// Wave height
        /// </summary>
        public float WaveHeight;

        /// <summary>
        /// Water table height, where 0 is ground level.
        /// </summary>
        public float WaterHeight; 

        // 12/17/2009 - Ocean Atts
        /// <summary>
        /// Ocean wave amplitude
        /// </summary>
        public float OceanWaveAmplitude;

        /// <summary>
        /// Ocean wave frequency
        /// </summary>
        public float OceanWaveFrequency;

        /// <summary>
        /// Ocean bump-map height
        /// </summary>
        public float OceanBumpHeight;

        /// <summary>
        /// Ocean deep color
        /// </summary>
        public Vector4 OceanDeepColor;

        /// <summary>
        /// Ocean shallow color
        /// </summary>
        public Vector4 OceanShallowColor;

        /// <summary>
        /// Ocean texture scale
        /// </summary>
        public Vector2 OceanTextureScale;

        /// <summary>
        /// Ocean wave speed
        /// </summary>
        public Vector2 OceanWaveSpeed;

        /// <summary>
        /// Ocean fresnel bias
        /// </summary>
        public float OceanFresnelBias;

        /// <summary>
        /// Ocean fresnel power
        /// </summary>
        public float OceanFresnelPower;

        /// <summary>
        /// Ocean HDR multiplier
        /// </summary>
        public float OceanHDRMultiplier;

        /// <summary>
        /// Ocean reflection amount, determines the influence of the reflection
        /// shown on the water surface.
        /// </summary>
        public float OceanReflectionAmt;

        /// <summary>
        /// Ocean water amount, determines the influence of the ocean floor shown.
        /// </summary>
        public float OceanWaterAmt;

        /// <summary>
        /// Ocean reflection color bias.
        /// </summary>
        public Vector4 OceanReflectionColor;

        /// <summary>
        /// Ocean sky amount, determines the influence of the sky clouds shown on
        /// the water surface.
        /// </summary>
        public float OceanReflectionSkyAmt;

    }
}