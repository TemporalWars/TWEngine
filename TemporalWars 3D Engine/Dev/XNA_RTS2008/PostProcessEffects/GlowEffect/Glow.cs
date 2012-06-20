#region File Description
//-----------------------------------------------------------------------------
// Glow.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TWEngine.Common.Enums;

namespace TWEngine.PostProcessEffects.GlowEffect
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.PostProcessEffects.GlowEffect"/> namespace contains the classes
    /// which make up the entire <see cref="GlowEffect"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
     
       

    ///<summary>
    /// The <see cref="Glow"/> class uses the <see cref="BlurGlow"/> class to create
    /// the blur glow post process <see cref="Effect"/>.
    ///</summary>
    public sealed class Glow : PostProcessEffect
    {
        private readonly BlurGlow _blurGlow;

        private readonly RenderTarget2D _glowRT1;   // render target for glow blur 
       
        // XNA 4.0 Updates; 'DepthStencilBuffer' is gone!
        //private readonly DepthStencilBuffer _glowDb;             

        ///<summary>
        /// Constructor, which creates the <see cref="BlurGlow"/> instance, and the required
        /// <see cref="RenderTarget2D"/>.
        ///</summary>
        ///<param name="graphicsDevice"><see cref="GraphicsDevice"/> instance</param>
        ///<param name="content"><see cref="ContentManager"/> instance</param>
        public Glow(GraphicsDevice graphicsDevice, ContentManager content)
            : base(graphicsDevice)
        {
            // 1/25/2010 - Updated RT size from 512x512 to 256x256.
            _blurGlow = new BlurGlow(graphicsDevice, content, 256, 256);

            // XNA 4.0 Updates.
            // 1/25/2010 - Updated RT size from 512x512 to 256x256.
            /*_glowRT1 = new RenderTarget2D(graphicsDevice, 256, 256, 1, SurfaceFormat.Color,
                                          graphicsDevice.PresentationParameters.MultiSampleType, graphicsDevice.PresentationParameters.MultiSampleQuality);*/
            _glowRT1 = new RenderTarget2D(graphicsDevice, 256, 256, true, SurfaceFormat.Color,
                                          DepthFormat.Depth24Stencil8);

            // XNA 4.0 Updates; 'DepthStencilBuffer' is gone!
            // Create depth buffer, since XBOX requires same size, we need one to match the ColorRT.
            //_glowDb = new DepthStencilBuffer(graphicsDevice, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, graphicsDevice.PresentationParameters.AutoDepthStencilFormat);  
        }


        ///<summary>
        /// Sets the given <paramref name="sourceTexture"/> into the 'BlurGlow' <see cref="Effect"/>, and then calls
        /// the <see cref=" BlurTechnique.BlurHorizontal"/> for pass#1.  The result is then passed back into the 'BlurGlow'
        /// <see cref="Effect"/>, and the <see cref="BlurTechnique.BlurVertical"/> is used for pass#2.  The result is passed
        /// back via the <paramref name="result"/> parameter.
        ///</summary>
        ///<param name="sourceTexture"><see cref="Texture2D"/> instance</param>
        ///<param name="result"><see cref="RenderTarget2D"/> to return the final post process effect</param>
        public override void PostProcess(Texture2D sourceTexture, RenderTarget2D result)
        {
            // XNA 4.0 Updates; 'DepthStencilBuffer' is gone!
            // Set to GLOW DB
            //var old = _GraphicsDevice.DepthStencilBuffer;
            //_GraphicsDevice.DepthStencilBuffer = _glowDb;

            // blur horizontal with regular horizontal blur shader
            _blurGlow.SetBlurTechnique = BlurTechnique.BlurHorizontal;
            _blurGlow.PostProcess(sourceTexture, _glowRT1);

            // XNA 4.0 Updates; GetTexture is gone!
            // blur vertical with regular vertical blur shader
            _blurGlow.SetBlurTechnique = BlurTechnique.BlurVertical;
            //_blurGlow.PostProcess(_glowRT1.GetTexture(), result);
            _blurGlow.PostProcess(_glowRT1, result);

            // XNA 4.0 Updates; 'DepthStencilBuffer' is gone!
            // reset DB 
            //_GraphicsDevice.DepthStencilBuffer = old;

            // XNA 4.0 Updates; 'RenderState' removed and replaced with 3 new states - http://blogs.msdn.com/b/shawnhar/archive/2010/04/02/state-objects-in-xna-game-studio-4-0.aspx
            //_GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            //_GraphicsDevice.RenderState.DepthBufferEnable = true;
            var depthStencilState = new DepthStencilState { DepthBufferWriteEnable = true, DepthBufferEnable = true };
            _GraphicsDevice.DepthStencilState = depthStencilState;


        }
      
    }
}


