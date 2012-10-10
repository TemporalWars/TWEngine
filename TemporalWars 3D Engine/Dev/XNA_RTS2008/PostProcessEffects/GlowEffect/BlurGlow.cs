#region File Description
//-----------------------------------------------------------------------------
// BlurGlow.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.Common.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWEngine.PostProcessEffects.GlowEffect
{
    ///<summary>
    /// The <see cref="BlurGlow"/> class is used by the <see cref="Glow"/> class, to 
    /// create the blur-flow post process effect, using the 'Glow' <see cref="Effect"/> file.
    ///</summary>
    public sealed class BlurGlow : PostProcessEffect
    {
        // blur effect
        private readonly Effect _blurEffect;

        // parameters       
        private readonly EffectParameter _paramColorMap;             // color texture
        private readonly EffectParameter _paramColor;                // color 
        private readonly EffectParameter _paramPixelSize;            // pixel size 
      
        // 1/25/2010 - Techniques
        private readonly EffectTechnique _techColor;
        private readonly EffectTechnique _techTexture;
        private readonly EffectTechnique _techBlurHorizontal;
        private readonly EffectTechnique _techBlurVertical;
        private readonly EffectTechnique _techBlurHorizontalSplit;

        // normalized pixel size (1.0/size)
        private readonly Vector2 _pixelSize;//
        readonly Vector4 _color = Vector4.One;

        #region Properties

        ///<summary>
        /// Used to set the current <see cref="BlurTechnique"/> to use.
        ///</summary>
        ///<exception cref="ArgumentOutOfRangeException">Thrown when given invalid <see cref="BlurTechnique"/> value.</exception>
        public BlurTechnique SetBlurTechnique
        {
            set
            {
                //_blurEffect.CurrentTechnique = _blurEffect.Techniques[(int)value];

                // 1/25/2010 - Set Technique
                switch (value)
                {
                    case BlurTechnique.Color:
                        _blurEffect.CurrentTechnique = _techColor;
                        break;
                    case BlurTechnique.ColorTexture:
                        _blurEffect.CurrentTechnique = _techTexture;
                        break;
                    case BlurTechnique.BlurHorizontal:
                        _blurEffect.CurrentTechnique = _techBlurHorizontal;
                        break;
                    case BlurTechnique.BlurVertical:
                        _blurEffect.CurrentTechnique = _techBlurVertical;
                        break;
                    case BlurTechnique.BlurHorizontalSplit:
                        _blurEffect.CurrentTechnique = _techBlurHorizontalSplit;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("value");
                }
            }
        }       

        #endregion


        ///<summary>
        /// Constructor, which loads the 'Glow' <see cref="Effect"/>, and sets
        /// the effect's parameters.
        ///</summary>
        ///<param name="graphicsDevice"><see cref="GraphicsDevice"/> instance</param>
        ///<param name="content"><see cref="ContentManager"/> instance</param>
        ///<param name="sizeX">Screen width value; used to set the half pixel-size adjustment for <see cref="Effect"/>.</param>
        ///<param name="sizeY">Screen height value; used to set the half pixel-size adjustment for <see cref="Effect"/>.</param>
        public BlurGlow(GraphicsDevice graphicsDevice, ContentManager content, int sizeX, int sizeY)
            : base(graphicsDevice)
        {
            // 4/6/2010: Updated to use 'ContentMiscLoc' global var.
            _blurEffect = content.Load<Effect>(TemporalWars3DEngine.ContentMiscLoc + @"\Shaders\Glow\Glow");

            // 1/25/2010 - Set Techniques
            _techColor = _blurEffect.Techniques["Color"];
            _techTexture = _blurEffect.Techniques["ColorTexture"];
            _techBlurHorizontal = _blurEffect.Techniques["BlurHorizontal"];
            _techBlurVertical = _blurEffect.Techniques["BlurVertical"];
            _techBlurHorizontalSplit = _blurEffect.Techniques["BlurHorizontalSplit"];

            // get effect parameters            
            _paramColorMap = _blurEffect.Parameters["g_ColorMap"];
            _paramColor = _blurEffect.Parameters["g_Color"];
            _paramPixelSize = _blurEffect.Parameters["g_PixelSize"];

            _pixelSize = new Vector2(1.0f / sizeX, 1.0f / sizeY);

            // 1/25/2010 - Moved from PostProcess
            _paramPixelSize.SetValue(_pixelSize);
            _paramColor.SetValue(_color);
        }


        ///<summary>
        /// Sets the given <paramref name="sourceTexture"/> into the <see cref="Effect"/>, and then
        /// calls the base <see cref="PostProcessEffect.DrawQuad"/> method to complete.
        ///</summary>
        ///<param name="sourceTexture"><see cref="Texture2D"/> instance</param>
        ///<param name="result"><see cref="RenderTarget2D"/> to return the final post process effect</param>
        public override void PostProcess(Texture2D sourceTexture, RenderTarget2D result)
        {
            // Set Params
            _paramColorMap.SetValue(sourceTexture);

            // XNA 4.0 Updates - Index no longer required on SetRenderTarget.
            //_GraphicsDevice.SetRenderTarget(0, result);
            _GraphicsDevice.SetRenderTarget(result);

            _GraphicsDevice.Clear(Color.Black);

            DrawQuad(_blurEffect);

            // XNA 4.0 Updates - Index no longer required on SetRenderTarget.
            //_GraphicsDevice.SetRenderTarget(0, null);
            _GraphicsDevice.SetRenderTarget(null);

        }
       
    }
}


