#region File Description
//-----------------------------------------------------------------------------
// Bloom.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.GameScreens;
using ImageNexus.BenScharbach.TWEngine.Interfaces;
using ImageNexus.BenScharbach.TWEngine.PostProcessEffects.BloomEffect.Enums;
using ImageNexus.BenScharbach.TWEngine.PostProcessEffects.GBlurEffect;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWEngine.PostProcessEffects.BloomEffect
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.PostProcessEffects.BloomEffect"/> namespace contains the classes
    /// which make up the entire <see cref="TWEngine.TWEngine.PostProcessEffects.BloomEffectnent.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
    
    
    ///<summary>
    /// The <see cref="Bloom"/> class is used to create the 'Bloom' post process effect.
    ///</summary>
    public sealed class Bloom : PostProcessEffect
    {
        private readonly BloomExtract _bloomExtract;
        private readonly GBlur _gBlur;
        private readonly BloomCombine _bloomCombine;    
        
        private readonly RenderTarget2D _renderTarget1;


        // XNA 4.0 Updates; 'DepthStencilBuffer' is gone!
        //private readonly DepthStencilBuffer _depthBuffer;

        // XNA 4.0 Updates; 'ResolveTexture2D' is gone! - 
        // NOTE: http://blogs.msdn.com/b/shawnhar/archive/2010/03/30/resolvebackbuffer-and-resolvetexture2d-in-xna-game-studio-4-0.aspx
        // 11/4/2009
        //private readonly ResolveTexture2D _resolveTarget;

        // 11/5/2009 - Bloom Settings.
        private static BloomSettings _settings = BloomSettings.PresetSettings[(int)BloomType.Default];

        // 6/12/2009
        private bool _doGaussianBlurPasses = true;

        // 12/7/2009
        private static BloomType _bloomTypeSetting;

        // 1/10/2011
        private static bool _useBloom;

        #region Properties

        // 1/10/2011
        /// <summary>
        /// Controls the use of the <see cref="Bloom"/> PostProcess effect.
        /// </summary>
        public static bool UseBloom
        {
            get { return _useBloom; }
            set
            {
                _useBloom = value;

                // Update TerrainShape's with new bloom atts.
                if (TerrainScreen.TerrainShapeInterface == null) return;
                if (TerrainScreen.TerrainShapeInterface.Effect == null) return;

                var effect = TerrainScreen.TerrainShapeInterface.Effect;
                effect.Parameters["xEnableBloom"].SetValue(value);
            }
        }

        ///<summary>
        /// Get or Set to do a gaussian blur pass.
        ///</summary>
        public bool DoGaussianBlurPasses
        {
            get { return _doGaussianBlurPasses; }
            set { _doGaussianBlurPasses = value; }
        }

        // 11/5/2009
        /// <summary>
        /// Returns the <see cref="BloomSettings"/> instance.
        /// </summary>
        public static BloomSettings Settings
        {
            get { return _settings; }
        }
       
        /// <summary>
        /// Allows settings the type of <see cref="Bloom"/> post process
        /// effect to using the <see cref="BloomType"/> Enum.
        /// </summary>
        public static BloomType BloomTypeSetting
        {
            get { return _bloomTypeSetting; }
            set
            {
                _bloomTypeSetting = value;
                _settings = BloomSettings.PresetSettings[(int)value];

                // 5/23/2010 - Update TerrainShape's with new bloom atts.
                if (TerrainScreen.TerrainShapeInterface == null) return;
                if (TerrainScreen.TerrainShapeInterface.Effect == null) return;

                var effect = TerrainScreen.TerrainShapeInterface.Effect;
                BloomSettings.SetBloomAttsIntoEffect(effect, _settings);
            }
        }

        #endregion

        ///<summary>
        /// Constructor, which creates the required classes <see cref="BloomExtract"/> and
        /// <see cref="BloomCombine"/>.  Also creates the required <see cref="RenderTarget2D"/>.
        ///</summary>
        ///<param name="graphicsDevice"><see cref="GraphicsDevice"/> instance</param>
        ///<param name="content"><see cref="ContentManager"/> instance</param>
        public Bloom(GraphicsDevice graphicsDevice, ContentManager content)
            : base(graphicsDevice)
        {
            _bloomExtract = new BloomExtract(graphicsDevice, content);
            _bloomCombine = new BloomCombine(graphicsDevice, content);
            _gBlur = new GBlur(graphicsDevice, content);             

            // Look up the resolution and format of our main backbuffer.
            var pp = graphicsDevice.PresentationParameters;

            var width = pp.BackBufferWidth;
            var height = pp.BackBufferHeight;

            var format = pp.BackBufferFormat;

            // XNA 4.0 Updates; 'ResolveTexture2D' is gone! - 
            // NOTE: http://blogs.msdn.com/b/shawnhar/archive/2010/03/30/resolvebackbuffer-and-resolvetexture2d-in-xna-game-studio-4-0.aspx
            // 11/4/2009
            // Create a texture for reading back the backbuffer contents.
            //_resolveTarget = new ResolveTexture2D(GraphicsDevice, width, height, 1, format);
           
            // Create two rendertargets for the bloom processing. These are half the
            // size of the backbuffer, in order to minimize fillrate costs. Reducing
            // the resolution in this way doesn't hurt quality, because we are going
            // to be blurring the bloom images in any case.
            width /= 2;
            height /= 2;

            // XNA 4.0 Updates;
            _renderTarget1 = new RenderTarget2D(graphicsDevice, width, height, true, format, DepthFormat.Depth24Stencil8);


            // XNA 4.0 Updates; 'DepthStencilBuffer' is gone!
            // Create depth buffer, since XBOX requires same size, we need one to match the ColorRT.
            //_depthBuffer = ScreenManager.CreateDepthStencil(_renderTarget1, DepthFormat.Depth24Stencil8Single);

        }

        // 1/10/2011
        /// <summary>
        /// Set Bloom/BloomSettings to effect.
        /// </summary>
        /// <param name="terrainShapeInterface">Instance of <see cref="ITerrainShape"/>.</param>
        internal static void SetBloomSettingToEffect(ITerrainShape terrainShapeInterface)
        {
            // Update TerrainShape's with new bloom atts.
            if (terrainShapeInterface == null) return;
            if (terrainShapeInterface.Effect == null) return;

            var effect = terrainShapeInterface.Effect;
            BloomSettings.SetBloomAttsIntoEffect(effect, _settings);
            effect.Parameters["xEnableBloom"].SetValue(_useBloom);
        }

        ///<summary>
        /// The <see cref="Bloom"/> effect is achieved by calling the <see cref="BloomExtract"/> first, then
        /// using the result and calling the <see cref="BloomCombine"/> to complete the effect.
        ///</summary>
        ///<param name="sourceTexture"><see cref="Texture2D"/> instance</param>
        ///<param name="result"><see cref="RenderTarget2D"/> to return the final post process effect</param>
        public override void PostProcess(Texture2D sourceTexture, RenderTarget2D result)
        {
            // XNA 4.0 Updates; 'ResolveTexture2D' is gone! - 
            // TODO: Since gone, need to figure out a solution here?
            // 11/4/2009 - check if 'sourceTexture' is NULL, and if so, then simply 
            //             get texture by Resolving the current RT.
            if (sourceTexture == null)
            {
                // Resolve the scene into a texture, so we can
                // use it as input data for the bloom processing.
                //_GraphicsDevice.ResolveBackBuffer(_resolveTarget);
                //sourceTexture = _resolveTarget;
            }


            // XNA 4.0 Updates; 'DepthStencilBuffer' is gone!
            // set DepthBuffer; especially, since XBOX requires same size DB as RenderTarget            
            //#if XBOX
            //var oldBuffer = _GraphicsDevice.DepthStencilBuffer;
            //_GraphicsDevice.DepthStencilBuffer = _depthBuffer;
            //#endif
            
            // Pass 1: draw the scene into rendertarget 1, using a
            // shader that extracts only the brightest parts of the image.
            _bloomExtract.PostProcess(sourceTexture, _renderTarget1);

            // XNA 4.0 Updates; GetTexture is gone.
            //if (_doGaussianBlurPasses)
            //_gBlur.PostProcess(_renderTarget1.GetTexture(), _renderTarget1);
            _gBlur.PostProcess(_renderTarget1, _renderTarget1);


            // XNA 4.0 Updates; 'DepthStencilBuffer' is gone!
            // Reset the DepthBuffer            
            //#if XBOX
            //_GraphicsDevice.DepthStencilBuffer = oldBuffer;
            //#endif

            // Pass 4: draw both rendertarget 1 and the original scene
            // image back into the main backbuffer, using a shader that
            // combines them to produce the final bloomed result.          
            _GraphicsDevice.Textures[1] = sourceTexture;           
            _bloomCombine.PostProcess(_renderTarget1, result);
           

        }
    }
}