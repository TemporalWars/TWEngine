#region File Description
//-----------------------------------------------------------------------------
// GBlurPass.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TWEngine.PostProcessEffects.BloomEffect;
using TWEngine.PostProcessEffects.GBlurEffect.Enums;

namespace TWEngine.PostProcessEffects.GBlurEffect
{
    ///<summary>
    /// The <see cref="GBlurPass"/> class does the work for creating the guassian blur
    /// effect, by using the <see cref="Effect"/> 'GuassianBlur', which applies a one dimensional gaussian blur filter.
    /// This is used twice by the bloom post process, first to blur horizontally, and then again to blur vertically.
    ///</summary>
    public sealed class GBlurPass : PostProcessEffect
    {
        private readonly Effect _gaussianBlurEffect;
       

        // 1/26/2010 - EffectTechniques
        private readonly EffectTechnique _gaussianBlurHPass;
        private readonly EffectTechnique _gaussianBlurVPass;

        private readonly BloomSettings _settings = BloomSettings.PresetSettings[0];

        private readonly EffectParameter _sourceTextureEP; // 8/13/2009
        private readonly EffectParameter _weightsHPassEP;
        private readonly EffectParameter _offsetsHPassEP;
        private readonly EffectParameter _weightsVPassEP; // 1/26/2010
        private readonly EffectParameter _offsetsVPassEP; // 1/26/2010

        // Create arrays for computing our filter _settings.
        private Vector2[] _sampleOffsetsHPass = new Vector2[1];
        private float[] _sampleWeightsHPass = new float[1];
        private Vector2[] _sampleOffsetsVPass = new Vector2[1]; // 1/26/2010
        private float[] _sampleWeightsVPass = new float[1]; // 1/26/2010

        ///<summary>
        /// Constructor, which loads the required <see cref="Effect"/> 'GuassianBlur', and
        /// saves references to the internal parameter effects.
        ///</summary>
        ///<param name="graphicsDevice"><see cref="GraphicsDevice"/> instance</param>
        ///<param name="content"><see cref="ContentManager"/> instance</param>
        public GBlurPass(GraphicsDevice graphicsDevice, ContentManager content)
            : base(graphicsDevice)
        {
            // 4/6/2010: Updated to use 'ContentMiscLoc' global var.
            _gaussianBlurEffect = content.Load<Effect>(TemporalWars3DEngine.ContentMiscLoc + @"\Shaders\Bloom\GaussianBlur");

            // 1/26/2010 - Set Techniques
            _gaussianBlurHPass = _gaussianBlurEffect.Techniques["GaussianBlur_HPass"];
            _gaussianBlurVPass = _gaussianBlurEffect.Techniques["GaussianBlur_VPass"];

            // 8/13/2009
            _sourceTextureEP = _gaussianBlurEffect.Parameters["SourceTexture"];

            _weightsHPassEP = _gaussianBlurEffect.Parameters["H_SampleWeights"];
            _offsetsHPassEP = _gaussianBlurEffect.Parameters["H_SampleOffsets"];

            // 1/26/2010
            _weightsVPassEP = _gaussianBlurEffect.Parameters["V_SampleWeights"];
            _offsetsVPassEP = _gaussianBlurEffect.Parameters["V_SampleOffsets"];
        }


        ///<summary>
        /// Sets the given <paramref name="sourceTexture"/> into the <see cref="Effect"/>, and then calls
        /// the base <see cref="PostProcessEffect.DrawQuad"/> to complete.
        ///</summary>
        ///<param name="sourceTexture"><see cref="Texture2D"/> instance</param>
        ///<param name="result"><see cref="RenderTarget2D"/> to return the final post process effect</param>
        public override void PostProcess(Texture2D sourceTexture, RenderTarget2D result)
        {
            // Set sourceTexture
            _sourceTextureEP.SetValue(sourceTexture);

            // XNA 4.0 Updates - Index no longer required on SetRenderTarget.
            //_GraphicsDevice.SetRenderTarget(0, result);
            _GraphicsDevice.SetRenderTarget(result);

            _GraphicsDevice.Clear(Color.Black);

            DrawQuad(_gaussianBlurEffect);

            // XNA 4.0 Updates - Index no longer required on SetRenderTarget.
            //_GraphicsDevice.SetRenderTarget(0, null);
            _GraphicsDevice.SetRenderTarget(null);
        }

        // 1/26/2010
        /// <summary>
        /// Sets the internal <see cref="Effect"/> to use the proper technique, depending
        /// on the <see cref="GBlurPassToUse"/> param.
        /// </summary>
        /// <param name="passToSet"><see cref="GBlurPassToUse"/> Enum</param>
        public void SetCurrentTechnique(GBlurPassToUse passToSet)
        {
            switch (passToSet)
            {
                case GBlurPassToUse.HorizontalPass:
                    _gaussianBlurEffect.CurrentTechnique = _gaussianBlurHPass;
                    break;
                case GBlurPassToUse.VerticalPass:
                    _gaussianBlurEffect.CurrentTechnique = _gaussianBlurVPass;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("passToSet");
            }
        }

        // 1/26/2010
        /// <summary>
        /// Computes sample weightings and <see cref="Texture2D"/> coordinate offsets
        /// for one pass of a separable gaussian blur filter.  The data 
        /// is saved into either the H-pass or V-pass arrays, depedent on
        /// the choise of the <see cref="GBlurPassToUse"/> Enum.
        /// </summary>
        /// <param name="dx">Dx data</param>
        /// <param name="dy">Dy data</param>
        /// <param name="passToSet"><see cref="GBlurPassToUse"/> Enum</param>
        public void SetBlurEffectParameters(float dx, float dy, GBlurPassToUse passToSet)
        {
            switch (passToSet)
            {
                case GBlurPassToUse.HorizontalPass:
                    SetBlurEffectParameters(dx, dy, ref _sampleWeightsHPass, ref _sampleOffsetsHPass);
                    _weightsHPassEP.SetValue(_sampleWeightsHPass);
                    _offsetsHPassEP.SetValue(_sampleOffsetsHPass);
                    break;
                case GBlurPassToUse.VerticalPass:
                    SetBlurEffectParameters(dx, dy, ref _sampleWeightsVPass, ref _sampleOffsetsVPass);
                    _weightsVPassEP.SetValue(_sampleWeightsVPass);
                    _offsetsVPassEP.SetValue(_sampleOffsetsVPass);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("passToSet");
            }
        }

        /// <summary>
        /// Computes sample weightings and <see cref="Texture2D"/> coordinate offsets
        /// for one pass of a separable gaussian blur filter.
        /// </summary>
        private void SetBlurEffectParameters(float dx, float dy, ref float[] sampleWeights, ref Vector2[] sampleOffsets)
        {
            // Look up how many samples our gaussian blur effect supports.
            var sampleCount = _weightsHPassEP.Elements.Count;

            // 8/7/2009 - Check if temporary arrays need resizing!
            var length = sampleWeights.Length; // 4/26/2010
            if (length != sampleCount)
                Array.Resize(ref sampleWeights, sampleCount);
            if (sampleOffsets.Length != sampleCount)
                Array.Resize(ref sampleOffsets, sampleCount);

            // The first sample always has a zero offset.
            sampleWeights[0] = ComputeGaussian(0);
            sampleOffsets[0] = new Vector2(0);

            // Maintain a sum of all the weighting values.
            var totalWeights = sampleWeights[0];

            // Add pairs of additional sample taps, positioned
            // along a line in both directions from the center.
            var halfSampleCount = sampleCount/2; // 4/26/2010
            for (var i = 0; i < halfSampleCount; i++)
            {
                // Store weights for the positive and negative taps.
                var weight = ComputeGaussian(i + 1);

                // 8/7/2009
                var indexX2 = i*2;

                sampleWeights[indexX2 + 1] = weight;
                sampleWeights[indexX2 + 2] = weight;

                totalWeights += weight*2;

                // To get the maximum amount of blurring from a limited number of
                // pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we Position our texture
                // coordinates exactly halfway between two texels, the filtering unit
                // will Average them for us, giving two samples for the price of one.
                // This allows us to step in units of two texels per sample, rather
                // than just one at a Time. The 1.5 offset kicks things off by
                // positioning us nicely in between two texels.
                var sampleOffset = indexX2 + 1.5f;

                var delta = new Vector2(dx, dy);
                Vector2.Multiply(ref delta, sampleOffset, out delta); // 8/7/2009

                // Store texture coordinate offsets for the positive and negative taps.
                sampleOffsets[indexX2 + 1] = delta;
                sampleOffsets[indexX2 + 2] = -delta;
            }

            // Normalize the list of sample weightings, so they will always sum to one.
            for (var i = 0; i < length; i++)
            {
                sampleWeights[i] /= totalWeights;
            }
           
        }


        /// <summary>
        /// Evaluates a single point on the gaussian falloff curve.
        /// Used for setting up the blur filter weightings.
        /// </summary>
        private float ComputeGaussian(float n)
        {
            var theta = _settings.BlurAmount;

            return (float) ((1.0/Math.Sqrt(2*Math.PI*theta))*
                            Math.Exp(-(n*n)/(2*theta*theta)));
        }
    }
}