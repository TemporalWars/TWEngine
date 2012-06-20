#region File Description
//-----------------------------------------------------------------------------
// BloomExtract.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TWEngine.PostProcessEffects.BloomEffect
{
    ///<summary>
    /// The <see cref="BloomExtract"/> class extracts the brighter areas of an image.
    /// This is the first step in applying a <see cref="Bloom"/> post process effect.
    ///</summary>
    public sealed class BloomExtract : PostProcessEffect
    {
        private readonly Effect _bloomExtractEffect;

        ///<summary>
        /// Constructor, which loads the required 'BloomExtract' <see cref="Effect"/> file.
        ///</summary>
        ///<param name="graphicsDevice"><see cref="GraphicsDevice"/> instance</param>
        ///<param name="content"><see cref="ContentManager"/> instance</param>
        public BloomExtract(GraphicsDevice graphicsDevice, ContentManager content)
            : base(graphicsDevice)
        {
            // 4/6/2010: Updated to use 'ContentMiscLoc' global var.
            _bloomExtractEffect = content.Load<Effect>(TemporalWars3DEngine.ContentMiscLoc + @"\Shaders\Bloom\BloomExtract");

        }


        ///<summary>
        /// Sets the given <paramref name="sourceTexture"/> into the <see cref="Effect"/>, and then required
        /// 'BloomThreshold' parameter, and then calls the base <see cref="PostProcessEffect.DrawQuad"/> method to complete.
        ///</summary>
        ///<param name="sourceTexture"><see cref="Texture2D"/> instance</param>
        ///<param name="result"><see cref="RenderTarget2D"/> to return the final post process effect</param>
        public override void PostProcess(Texture2D sourceTexture, RenderTarget2D result)
        {
            // Set sourceTexture
            _bloomExtractEffect.Parameters["SourceTexture"].SetValue(sourceTexture);

            _bloomExtractEffect.Parameters["BloomThreshold"].SetValue(Bloom.Settings.BloomThreshold);

            // XNA 4.0 Updates - Index no longer required on SetRenderTarget.
            //_GraphicsDevice.SetRenderTarget(0, result);
            _GraphicsDevice.SetRenderTarget(result);

            _GraphicsDevice.Clear(Color.Black);

            DrawQuad(_bloomExtractEffect);

            // XNA 4.0 Updates - Index no longer required on SetRenderTarget.
            //_GraphicsDevice.SetRenderTarget(0, null);
            _GraphicsDevice.SetRenderTarget(null);

        }
    }
}