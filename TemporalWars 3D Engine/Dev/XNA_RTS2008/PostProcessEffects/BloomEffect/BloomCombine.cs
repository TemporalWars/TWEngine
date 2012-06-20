#region File Description
//-----------------------------------------------------------------------------
// BloomCombine.cs
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
    /// The <see cref="BloomCombine"/> class combines the <see cref="Bloom"/> image with the original
    /// scene, using tweakable intensity levels and saturation. This is the final step in applying a bloom post process.
    ///</summary>
    public sealed class BloomCombine : PostProcessEffect
    {
        private readonly Effect _bloomCombineEffect;

        ///<summary>
        /// Constructor, which loads the required 'BloomCombine' <see cref="Effect"/>, and sets
        /// the effect's parameters.
        ///</summary>
        ///<param name="graphicsDevice"><see cref="GraphicsDevice"/> instance</param>
        ///<param name="content"><see cref="ContentManager"/> instance</param>
        public BloomCombine(GraphicsDevice graphicsDevice, ContentManager content)
            : base(graphicsDevice)
        {
            // 4/6/2010 - Updated to use 'ContentMiscLoc' global var.
            _bloomCombineEffect = content.Load<Effect>(TemporalWars3DEngine.ContentMiscLoc + @"\Shaders\Bloom\BloomCombine");

            var parameters = _bloomCombineEffect.Parameters;

            parameters["BloomIntensity"].SetValue(Bloom.Settings.BloomIntensity);
            parameters["BaseIntensity"].SetValue(Bloom.Settings.BaseIntensity);
            parameters["BloomSaturation"].SetValue(Bloom.Settings.BloomSaturation);
            parameters["BaseSaturation"].SetValue(Bloom.Settings.BaseSaturation);    

        }

        ///<summary>
        /// Sets the <see cref="Texture2D"/> used as the 'BaseTexture' in the <see cref="Effect"/>.
        ///</summary>
        public Texture2D SetBaseTexture
        {
            set
            {
                // Set baseTexture
                _bloomCombineEffect.Parameters["BaseTexture"].SetValue(value);
            }
        }

        ///<summary>
        /// Sets the given <paramref name="sourceTexture"/> into the <see cref="Effect"/>, and then
        /// calls the base <see cref="PostProcessEffect.DrawQuad"/> method to complete.
        ///</summary>
        ///<param name="sourceTexture"><see cref="Texture2D"/> instance</param>
        ///<param name="result"><see cref="RenderTarget2D"/> to return the final post process effect</param>
        public override void PostProcess(Texture2D sourceTexture, RenderTarget2D result)
        {
            // Set sourceTexture
            _bloomCombineEffect.Parameters["SourceTexture"].SetValue(sourceTexture);

            // XNA 4.0 Updates - Index no longer required on SetRenderTarget.
            //_GraphicsDevice.SetRenderTarget(0, result);
            _GraphicsDevice.SetRenderTarget(result);

            _GraphicsDevice.Clear(Color.Black);

            DrawQuad(_bloomCombineEffect);

            // XNA 4.0 Updates - Index no longer required on SetRenderTarget.
            //_GraphicsDevice.SetRenderTarget(0, null);
            _GraphicsDevice.SetRenderTarget(null);

        }
    }
}