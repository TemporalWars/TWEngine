#region File Description
//-----------------------------------------------------------------------------
// BloomSettings.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework.Graphics;

namespace TWEngine.PostProcessEffects.BloomEffect
{
    /// <summary>
    /// The <see cref="BloomSettings"/> holds all the Settings used to tweak the <see cref="Bloom"/> effect.
    /// </summary>
    public class BloomSettings
    {
        #region Fields
        
        ///<summary>
        /// name of a preset bloom setting, for display to the user.
        ///</summary>
        public readonly string Name;
        
        ///<summary>
        /// Controls how bright a pixel needs to be before it will bloom.
        /// Zero makes everything bloom equally, while higher values select
        /// only brighter colors. Somewhere between 0.25 and 0.5 is good.
        ///</summary>
        public readonly float BloomThreshold;
       
        ///<summary>
        /// Controls how much blurring is applied to the bloom image.
        /// The typical range is from 1 up to 10 or so.
        ///</summary>
        public readonly float BlurAmount;
        
        ///<summary>
        /// Controls the amount of the bloom and base images that
        /// will be mixed into the final scene. Range 0 to 1.
        ///</summary>
        public readonly float BloomIntensity;
        ///<summary>
        /// Controls the amount of the bloom and base images that
        /// will be mixed into the final scene. Range 0 to 1.
        ///</summary>
        public readonly float BaseIntensity;
        
        ///<summary>
        /// Independently control the color saturation of the bloom and
        /// base images. Zero is totally desaturated, 1.0 leaves saturation
        /// unchanged, while higher values increase the saturation level.
        ///</summary>
        public readonly float BloomSaturation;

        ///<summary>
        /// Independently control the color saturation of the bloom and
        /// base images. Zero is totally desaturated, 1.0 leaves saturation
        /// unchanged, while higher values increase the saturation level.
        ///</summary>
        public readonly float BaseSaturation;


        #endregion

        /// <summary>
        /// Constructs a new bloom Settings descriptor.
        /// </summary>
        /// <param name="name">Enter <see cref="Name"/></param>
        /// <param name="bloomThreshold">Enter <see cref="BloomThreshold"/></param>
        /// <param name="blurAmount">Enter <see cref="BlurAmount"/></param>
        /// <param name="bloomIntensity">Enter <see cref="BloomIntensity"/></param>
        /// <param name="baseIntensity">Enter <see cref="BaseIntensity"/></param>
        /// <param name="bloomSaturation">Enter <see cref="BloomSaturation"/></param>
        /// <param name="baseSaturation">Enter <see cref="BaseSaturation"/></param>
        public BloomSettings(string name, float bloomThreshold, float blurAmount,
                             float bloomIntensity, float baseIntensity,
                             float bloomSaturation, float baseSaturation)
        {
            Name = name;
            BloomThreshold = bloomThreshold;
            BlurAmount = blurAmount;
            BloomIntensity = bloomIntensity;
            BaseIntensity = baseIntensity;
            BloomSaturation = bloomSaturation;
            BaseSaturation = baseSaturation;
        }

        // 5/23/2010
        ///<summary>
        /// Updates a given <paramref name="effect"/> file with the current Bloom attributes.
        ///</summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="effect"/> given is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="bloomSettings"/> given is null.</exception>
        ///<param name="effect"><see cref="Effect"/> instance to update, which has the <see cref="BloomSettings"/> attributes.</param>
        ///<param name="bloomSettings"><see cref="BloomSettings"/> instance</param>
        public static void SetBloomAttsIntoEffect(Effect effect, BloomSettings bloomSettings)
        {
            // check if effect given is null.
            if (effect == null)
                throw new ArgumentNullException("effect", @"Effect instance given is null, which is not allowed!");

            if (bloomSettings == null)
                throw new ArgumentNullException("bloomSettings", @"The bloomSettings given is null, which is not allowed!");

            effect.Parameters["BloomThreshold"].SetValue(bloomSettings.BloomThreshold);
            effect.Parameters["BloomIntensity"].SetValue(bloomSettings.BloomIntensity);
            effect.Parameters["BaseIntensity"].SetValue(bloomSettings.BaseIntensity);
            effect.Parameters["BloomSaturation"].SetValue(bloomSettings.BloomSaturation);
            effect.Parameters["BaseSaturation"].SetValue(bloomSettings.BaseSaturation);
        }
        

        /// <summary>
        /// Table of preset bloom Settings.
        /// </summary>
        public static BloomSettings[] PresetSettings =
            {
                //                name           Thresh  Blur Bloom  Base  BloomSat BaseSat
                new BloomSettings("Default",     0.25f,  4,   1.25f, 1,    1,       1),
                new BloomSettings("Soft",        0,      3,   1,     1,    1,       1),
                new BloomSettings("Desaturated", 0.5f,   8,   2,     1,    0,       1),
                new BloomSettings("Saturated",   0.25f,  4,   2,     1,    2,       0),
                new BloomSettings("Blurry",      0,      2,   1,     0.1f, 1,       1),
                new BloomSettings("Subtle",      0.5f,   2,   1,     1,    1,       1),
            };
    }
}