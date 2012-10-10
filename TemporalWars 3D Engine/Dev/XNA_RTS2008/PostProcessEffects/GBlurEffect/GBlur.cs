#region File Description
//-----------------------------------------------------------------------------
// GBlur.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.PostProcessEffects.GBlurEffect.Enums;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWEngine.PostProcessEffects.GBlurEffect
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.PostProcessEffects.GBlurEffect"/> namespace contains the classes
    /// which make up the entire <see cref="TWEngine.TWEngine.PostProcessEffects.GBlurEffectnent.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
       


    /// <summary>
    /// The <see cref="GBlur"/> class creates the smooth guassian blur post process effect.
    /// </summary>
    public sealed class GBlur : PostProcessEffect
    {
        private readonly GBlurPass _gBlurPass;           
        
        private readonly RenderTarget2D _gBlurRenderTarget;
        private readonly RenderTarget2D _gBlurRenderTarget2;

        // XNA 4.0 Updates; 'DepthStencilBuffer' is gone!
        //private readonly DepthStencilBuffer _depthBuffer;


        // 6/12/2009
        private bool _doGaussianBlurPasses = true;
        private readonly float _rtWidthOverOne;
        private readonly float _rtHeightOverOne;

        ///<summary>
        /// Get or Set to do the guassian blur pass.
        ///</summary>
        public bool DoGaussianBlurPasses
        {
            get { return _doGaussianBlurPasses; }
            set { _doGaussianBlurPasses = value; }
        }

        ///<summary>
        /// Constructor, which creates the internal <see cref="GBlurPass"/> class, and
        /// creates the two required <see cref="RenderTarget2D"/> to achieve the effect.
        ///</summary>
        ///<param name="graphicsDevice"><see cref="GraphicsDevice"/> instance</param>
        ///<param name="content"><see cref="ContentManager"/> instance</param>
        public GBlur(GraphicsDevice graphicsDevice, ContentManager content)
            : base(graphicsDevice)
        {            
            _gBlurPass = new GBlurPass(graphicsDevice, content);           

            // Look up the resolution and format of our main backbuffer.
            var pp = graphicsDevice.PresentationParameters;

            var width = pp.BackBufferWidth;
            var height = pp.BackBufferHeight;

            var format = pp.BackBufferFormat;
           
            // Create two rendertargets for the bloom processing. These are half the
            // size of the backbuffer, in order to minimize fillrate costs. Reducing
            // the resolution in this way doesn't hurt quality, because we are going
            // to be blurring the bloom images in any case.
            width /= 2;
            height /= 2;

            // XNA 4.0 Updates
            _gBlurRenderTarget = new RenderTarget2D(graphicsDevice, width, height, true, format, DepthFormat.Depth24Stencil8);
            _gBlurRenderTarget2 = new RenderTarget2D(graphicsDevice, width, height, true, format, DepthFormat.Depth24Stencil8);

            // XNA 4.0 Updates; 'DepthStencilBuffer' is gone!
            // 1/26/2010 - Updated to use new overload version, which allows setting width & height!
            // Create depth buffer, since XBOX requires same size, we need one to match the ColorRT.
            //_depthBuffer = ScreenManager.CreateDepthStencil(_gBlurRenderTarget, DepthFormat.Depth24Stencil8Single, width, height);


            // 8/13/2009 - Compute values used in PostProcess.
            _rtWidthOverOne = 1.0f / _gBlurRenderTarget.Width;
            _rtHeightOverOne = 1.0f / _gBlurRenderTarget.Height;

            // 1/26/2010 - Create the GBlur Offsets/Weights arrays for each pass type.
            _gBlurPass.SetBlurEffectParameters(_rtWidthOverOne, 0, GBlurPassToUse.HorizontalPass);
            _gBlurPass.SetBlurEffectParameters(0, _rtHeightOverOne, GBlurPassToUse.VerticalPass);
        }


        ///<summary>
        /// Using the internal <see cref="GBlurPass"/> instance, the <paramref name="sourceTexture"/> is passed in, with
        /// a call to the <see cref="GBlurPassToUse.HorizontalPass"/> for pass#1.  Then the result is passed back into
        /// the <see cref="GBlurPass"/> instance, using the <see cref="GBlurPassToUse.VerticalPass"/> for pass#2.
        /// The final result is passed out via the <paramref name="result"/> parameter.
        ///</summary>
        /// <remarks>This overload version passes back the result as a <see cref="RenderTarget2D"/>.</remarks>
        ///<param name="sourceTexture"><see cref="Texture2D"/> instance</param>
        ///<param name="result"><see cref="RenderTarget2D"/> to return the final post process effect</param>
        public override void PostProcess(Texture2D sourceTexture, RenderTarget2D result)
        {
            // XNA 4.0 Updates; 'DepthStencilBuffer' is gone!
            // 1/26/2010 - Updated to use new overload version, which allo
            // set DepthBuffer, especially for XBOX, since requires same size DB as RenderTarget  
            //var oldBuffer = _GraphicsDevice.DepthStencilBuffer;
            //_GraphicsDevice.DepthStencilBuffer = _depthBuffer;

            // Pass 1: draw from rendertarget 1 into rendertarget 2,
            // using a shader to apply a horizontal gaussian blur filter.
            //_gBlurPass.SetBlurEffectParameters(_rtWidthOverOne, 0);
            _gBlurPass.SetCurrentTechnique(GBlurPassToUse.HorizontalPass); // 1/26/2010
            _gBlurPass.PostProcess(sourceTexture, _gBlurRenderTarget);

            // Pass 2: draw from rendertarget 2 back into rendertarget 1,
            // using a shader to apply a vertical gaussian blur filter.
            //_gBlurPass.SetBlurEffectParameters(0, _rtHeightOverOne);
            _gBlurPass.SetCurrentTechnique(GBlurPassToUse.VerticalPass); // 1/26/2010

            // XNA 4.0 Updates; GetTexture is gone!
            //_gBlurPass.PostProcess(_gBlurRenderTarget.GetTexture(), result);
            _gBlurPass.PostProcess(_gBlurRenderTarget, result);

            // XNA 4.0 Updates; 'DepthStencilBuffer' is gone!
            // Reset the DepthBuffer
            //_GraphicsDevice.DepthStencilBuffer = oldBuffer;                     

        }

        // 6/26/2009 - Overload version of PostProcess, which returns the texture result.
        ///<summary>
        /// Using the internal <see cref="GBlurPass"/> instance, the <paramref name="sourceTexture"/> is passed in, with
        /// a call to the <see cref="GBlurPassToUse.HorizontalPass"/> for pass#1.  Then the result is passed back into
        /// the <see cref="GBlurPass"/> instance, using the <see cref="GBlurPassToUse.VerticalPass"/> for pass#2.
        /// The final result is passed out via the <paramref name="result"/> parameter.
        ///</summary>
        /// <remarks>This overload version passes back the result as a <see cref="Texture2D"/>.</remarks>
        ///<param name="sourceTexture">Instance of source texture as <see cref="Texture2D"/>.</param>
        ///<param name="result">(OUT) Returns the source texture transformed using the guassian blur as <see cref="Texture2D"/>.</param>
        public void PostProcess(Texture2D sourceTexture, out Texture2D result)
        {
            // XNA 4.0 Updates; 'DepthStencilBuffer' is gone!
            // set DepthBuffer, especially for XBOX, since requires same size DB as RenderTarget  
            //var oldBuffer = _GraphicsDevice.DepthStencilBuffer;
            //_GraphicsDevice.DepthStencilBuffer = _depthBuffer;

            // Pass 1: draw from rendertarget 1 into rendertarget 2,
            // using a shader to apply a horizontal gaussian blur filter.
            //_gBlurPass.SetBlurEffectParameters(_rtWidthOverOne, 0);
            _gBlurPass.SetCurrentTechnique(GBlurPassToUse.HorizontalPass); // 1/26/2010
            _gBlurPass.PostProcess(sourceTexture, _gBlurRenderTarget);

            // Pass 2: draw from rendertarget 2 back into rendertarget 1,
            // using a shader to apply a vertical gaussian blur filter.
            //_gBlurPass.SetBlurEffectParameters(0, _rtHeightOverOne);
            _gBlurPass.SetCurrentTechnique(GBlurPassToUse.VerticalPass); // 1/26/2010
            // XNA 4.0 Updates; GetTexture is gone!
            //_gBlurPass.PostProcess(_gBlurRenderTarget.GetTexture(), _gBlurRenderTarget2);
            _gBlurPass.PostProcess(_gBlurRenderTarget, _gBlurRenderTarget2);

            // XNA 4.0 Updates; 'DepthStencilBuffer' is gone!
            // Reset the DepthBuffer
            //_GraphicsDevice.DepthStencilBuffer = oldBuffer;

            // XNA 4.0 Updates; GetTexture is gone!
            // Pass result texture back to caller
            //result = _gBlurRenderTarget2.GetTexture();
            result = _gBlurRenderTarget2;

        }
    }
}


