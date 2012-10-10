#region File Description
//-----------------------------------------------------------------------------
// TerrainAlphaMaps.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.Interfaces;
using ImageNexus.BenScharbach.TWEngine.Terrain.Enums;
using ImageNexus.BenScharbach.TWEngine.TerrainTools;
using ImageNexus.BenScharbach.TWEngine.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

#if !XBOX360
using System.Windows.Forms;
#endif

namespace ImageNexus.BenScharbach.TWEngine.Terrain
{
    ///<summary>
    /// The <see cref="TerrainAlphaMaps"/> class is used to control the texture splatting onto the <see cref="TWEngine.Terrain"/>.
    /// This class is specifically updated from the edit PaintTool form, and used during game play to draw the textures at
    /// the proper placement, with the proper splatting effect.
    ///</summary>
    public class TerrainAlphaMaps : ITerrainAlphaMaps, IDisposable
    {
        // 9/6/2008 - Add ContentManager Instance
        private const int TextureCountPerVolume = 4;
        private static readonly float[] MaxRange = new float[4];
        private static readonly float[] MinRange = new float[4];
        private static ContentManager _contentManager;
       
        // Save Graphics Device Instance
        private static GraphicsDevice _graphicsDevice;

        private static float _alphaLy1Percent = 0.25f;
        private static float _alphaLy2Percent = 0.50f;
        private static float _alphaLy3Percent = 0.75f;
        private static float _alphaLy4Percent = 1;
        private static float _alphaScale; // Set to length of Terrain Width       
        private static Color[] _bits1; // Used to change AlphaMaps Bits        
        
        // Stores a value of 1 in one of the 4 channels to designate a specific texture.
        private static Vector4 _paintTexture;

        // 1/22/2009 - # of textures in 1 volume.
        // 1/22/2009 - Blend Texture amount to use.
        private static float _textureBlendAmount;
        private static Texture2D _textureMap;
        
        private static int _useTexturePosition = 1; // Keeps track of what texture Position is currently picked.

        // 7/9/2010 - Dirty flag to track changes
        private static bool _isDirty;

        // 7/9/2010
        /// <summary>
        /// During edit-mode, the '_toEdit' buffer is used
        /// for current editing.
        /// </summary>
        private static int _toEditAlphaMapBuffer = 1;

        /// <summary>
        /// During edit-mode, the '_toDraw' buffer is displayed to Effect class.
        /// </summary>
        private static int _toDrawAlphaMapBuffer;
        

        #region Properties

        // 1/22/2009 - 
        /// <summary>
        /// Which texture layer is 'inUse'; both could be active at same Time for
        /// multi-texturing effects!
        /// </summary>
        public static bool InUseLayer1 { get; set; }

        // 1/22/2009 - 
        /// <summary>
        /// Which texture layer is 'inUse'; both could be active at same Time for
        /// multi-texturing effects!
        /// </summary>
        public static bool InUseLayer2 { get; set; }

        // 8/12/2009
        ///<summary>
        /// Sets the collection of <see cref="Texture2D"/> maps.
        ///</summary>
        ///<param name="value">Collection of <see cref="Texture2D"/></param>
        public void SetTextureMaps(Texture2D value)
        {
            _textureMap = value;
        }

        // 8/12/009 - Convert to Get method, per FXCop.
        ///<summary>
        /// Returns a collection of <see cref="Texture2D"/> maps.
        ///</summary>
        ///<returns>Collection of <see cref="Texture2D"/></returns>
        public Texture2D GetTextureMaps()
        {
            return _textureMap;
        }


        ///<summary>
        /// Scale to apply for each texel position of the <see cref="TerrainAlphaMaps"/>.
        ///</summary>
        public float AlphaScale
        {
            get { return _alphaScale; }
            set { _alphaScale = value; }
        }

        ///<summary>
        /// Visiblity percent of texture-1.
        ///</summary>
        public float AlphaLy1Percent
        {
            get { return _alphaLy1Percent; }
            set { _alphaLy1Percent = value; }
        }

        ///<summary>
        /// Visiblity percent of texture-2.
        ///</summary>
        public float AlphaLy2Percent
        {
            get { return _alphaLy2Percent; }
            set { _alphaLy2Percent = value; }
        }

        ///<summary>
        /// Visiblity percent of texture-3.
        ///</summary>
        public float AlphaLy3Percent
        {
            get { return _alphaLy3Percent; }
            set { _alphaLy3Percent = value; }
        }

        ///<summary>
        /// Visiblity percent of texture-4.
        ///</summary>
        public float AlphaLy4Percent
        {
            get { return _alphaLy4Percent; }
            set { _alphaLy4Percent = value; }
        }

        ///<summary>
        /// Holds the pixel color data for layer-1/layer-2 in x/y, respectively.  Each channel, like the X channel, stores
        /// the blended pixel color for the choosen texture position (1-4).  Channel Z stores the bump data for Layer-1.
        ///</summary>
        public Vector4 PaintTexture
        {
            get { return _paintTexture; }
            set { _paintTexture = value; }
        }

        #endregion

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        public TerrainAlphaMaps(Game game)
        {
            // Save Game Ref

            // 4/6/2010: Updated to use 'ContentMapsLoc' global var.
            if (_contentManager == null)
                _contentManager = new ContentManager(game.Services, TemporalWars3DEngine.ContentMapsLoc);

            // Save GrapicsDevice            
            _graphicsDevice = game.GraphicsDevice;
        }

        // 4/23/2008 - 
        // 1/22/2009: Updated to use the new Texture Normalized positions.
        /// <summary>
        /// Helper Fn: Populates the AlphaMap1 <see cref="Texture2D"/> using the Height
        /// constraints given per texture layer.
        /// </summary>
        private static void PopulateAlphaMap1()
        {
            // 11/20/2009 - Capture 'TileBracket' error, which is thrown sometime when calling 'SetData'.
           
            try
            {
                // 5/14/2009 - cache values
                var textureWidth = _textureMap.Width;
                var textureHeight = _textureMap.Height;

                // Populate AlphaMap1 with Data depending on Heights            
                PopulateColorBitsFromTexture(_textureMap, out _bits1);

                // 8/29/2009 - Cache
                const int scale = TerrainData.cScale;
                const int textureCountPerVolume = TextureCountPerVolume; // 5/17/2010 - Cache

                // 5/17/2010 - Cache arrays
                var minRange = MinRange;
                var maxRange = MaxRange;

                for (var texturePos = 0; texturePos < textureCountPerVolume; texturePos++)
                    for (var loopY = 0; loopY < textureHeight; loopY++)
                        for (var loopX = 0; loopX < textureWidth; loopX++)
                        {
                            float hmX = loopX * scale;
                            float hmY = loopY * scale;

                            var height = TerrainData.GetTerrainHeight(hmX, hmY);

                            if (height < minRange[texturePos] || height > maxRange[texturePos]) continue;

                            var pixelColor = _bits1[loopX + loopY * textureWidth].ToVector4();

                            // 1/22/2009 - Layer-1 is now just the Red channel.
                            pixelColor.X = (CalcTextureNormalizedPosition(texturePos + 1) + _textureBlendAmount);

                            // For Bump, need to store the actual texture Position value (1, 2, 3...) into Blue channel.
                            pixelColor.Z = (texturePos + 1);

                            _bits1[loopX + loopY * textureWidth] = new Color(pixelColor);
                        }

                // 7/9/2010 - Use new Double buffer technique
                _textureMap = null;
                // XNA 4.0 Updates
                //_textureMaps[0] = new Texture2D(_graphicsDevice, TerrainData.MapWidth, TerrainData.MapHeight, 1, TextureUsage.None, SurfaceFormat.Color);
                _textureMap = new Texture2D(_graphicsDevice, TerrainData.MapWidth, TerrainData.MapHeight, false, SurfaceFormat.Color);
                _textureMap.SetData(_bits1);
               
            }
            catch (InvalidOperationException)
            {
                Debug.WriteLine("(PopulateAlphaMap1) threw the InvalidOpExp error.");
            }
        }

        // 1/22/2009       
        /// <summary>
        /// Sets the <see cref="TerrainAlphaMaps"/> to use a given texture Position and a specific <see cref="LayerGroup"/>,
        /// when painting.
        /// </summary>
        /// <param name="inUseTexturePosition">A texture Position; for example 1, 2, 3, etc.</param>
        /// <param name="layerGroup">The <see cref="LayerGroup"/> to use</param>
        public static void SetPaintTextureToUse(int inUseTexturePosition, LayerGroup layerGroup)
        {
            // 1st - Then save the Position
            _useTexturePosition = inUseTexturePosition;

            // 2nd - set into proper alpha group channel; Red = Layer-1, & Green = Layer-2.
            switch (layerGroup)
            {
                case LayerGroup.Layer1:
                    // Convert the texture Position into a normalize Position.
                    _paintTexture.X = CalcTextureNormalizedPosition(_useTexturePosition) + _textureBlendAmount;
                    // For Bump, need to store the actual texture Position value (1, 2, 3...) into Blue channel.
                    _paintTexture.Z = _useTexturePosition;
                    break;
                case LayerGroup.Layer2:
                    // Convert the texture Position into a normalize Position.
                    _paintTexture.Y = CalcTextureNormalizedPosition(_useTexturePosition) + _textureBlendAmount;
                    break;
                default:
                    break;
            }
        }

        // 1/22/2009
        /// <summary>
        /// Sets how much the texture to be painted is interpolated with
        /// one of it's neighbor textures in the volume texture stack.
        /// 
        /// Negative percents given will interpolate with neighboring texture
        /// to the left, while Positive percents will interpolate with neighboring
        /// texture to the right.
        /// </summary>
        /// <param name="blendPercent">A value from -100 to 100</param>
        public static void SetTextureBlendToUse(float blendPercent)
        {
            _textureBlendAmount = CalculateTextureBlend(blendPercent);

            // call the SetPaintTextureToUse to set the new blending offset.
            if (InUseLayer1)
                SetPaintTextureToUse(_useTexturePosition, LayerGroup.Layer1);

            if (InUseLayer2)
                SetPaintTextureToUse(_useTexturePosition, LayerGroup.Layer2);
        }

        // 5/15/2009        
        /// <summary>
        /// Helper Function, which takes a given <paramref name="blendPercent"/>, which is -100 to 100 percent, and
        /// returns the proper <see cref="_textureBlendAmount"/>.
        /// </summary>
        /// <param name="blendPercent">A value from -100 to 100</param>
        /// <returns>TextureBlend float amount</returns>
        private static float CalculateTextureBlend(float blendPercent)
        {
            // verify only values between -100 and 100 are given.
            if (blendPercent < -100 || blendPercent > 100)
                throw new ArgumentOutOfRangeException("blendPercent", @"Value must be between -100 and 100.");


            // Value is given in whole numbers between -100 to 100; however, the
            // value needs to be normalized between 0 - 1.
            var normalizedBlendPercent = blendPercent/100.0f;

            // Texture size is the size in normalized space, the texture occupies within
            // the volume texture.  For example, if 4 textures in volume stack, then 
            // (1 / 4) = 0.25f texture size.
            const float textureSize = (1.0f/TextureCountPerVolume);

            // Calculates the middle Position in normalized space; this is important, because
            // the blending or interpolation of a texture is dependent on how close the normalized
            // value is to its neighboring texture!  Therefore, the middle Position will have no
            // blending what-so-ever, and will be the original texture.
            const float midTexturePosition = textureSize/2.0f;

            // calculates the actual blendOffset value to use
            return midTexturePosition*normalizedBlendPercent;
        }

        // 1/22/2009
        /// <summary>
        /// Takes a Position, like 1-8 for example, and normalize it between 0-1.
        /// </summary>
        /// <param name="useTexturePosition">A texture Position; for example 1, 2, 3, etc.</param>
        /// <returns>A normalized texture Position between 0.01f - 1.0f</returns>
        private static float CalcTextureNormalizedPosition(float useTexturePosition)
        {
            //       Formula: 1st: Take texture Count per volume 'Texture3D', and divide into Position given.
            //                Since 0 based, the 2nd texture is at the location of 1/n.
            //                2nd: To get 100% of texture, take the middle Position, in normalized space, for
            //                each texture Position.


            // Make sure value of 1 or greater.
            if (useTexturePosition < 1)
                throw new ArgumentOutOfRangeException("useTexturePosition", @"Value has to be 1 or greater.");

            // Texture size is the size in normalized space, the texture occupies within
            // the volume texture.  For example, if 4 textures in volume stack, then 
            // (1 / 4) = 0.25f texture size.
            const float textureSize = (1.0f/TextureCountPerVolume);

            // Calculates the middle Position in normalized space; this is important, because
            // the blending or interpolation of a texture is dependent on how close the normalized
            // value is to its neighboring texture!  Therefore, the middle Position will have no
            // blending what-so-ever, and will be the original texture.
            const float midTexturePosition = textureSize/2.0f;

            // take texture Position divided by Total texture Count, then subtract textureSize, since zero based.  Then add
            // midTexture Position for 100% texture sampling, with no interpolation with neighboring textures.
            var normalizedTexturePosition = ((useTexturePosition/TextureCountPerVolume) - textureSize) +
                                              midTexturePosition;

            return normalizedTexturePosition;
        }

        // 5/15/2009; 5/17/2010: Updated param 1 from 'int' to LayerGroup enum.
        ///<summary>
        /// Applies Perlin-Noise to given <paramref name="texturePosition"/> channel, within the
        /// <see cref="LayerGroup"/> choice.
        ///</summary>
        ///<param name="layerToAffect"><see cref="LayerGroup"/> Enum</param>
        ///<param name="texturePosition">Texture position to use (1-4).</param>
        ///<param name="noise">Collection of floats as perlin noise</param>
        public static void ApplyPerlinNoise(LayerGroup layerToAffect, int texturePosition, List<float> noise)
        {
            // 11/20/2009 - Capture 'TileBracket' error, which is thrown sometime when calling 'SetData'.
            try
            {
                // 7/15/2009 - Check if Bits needs to be populated.
                if (_bits1 == null || _bits1.Length == 0)
                    PopulateColorBitsFromTexture(_textureMap, out _bits1);

                var bits1 = _bits1; // 5/17/2010 - Cache

                // The Height test needs to be in the Range of the Bottom-Texture to Top-Texture, where the Top-Texture
                // is the value given; for example if texture-2 channel, then bottom-texture = 0, while top-texture = 1.            
                var topTexture = (int)MathHelper.Clamp(texturePosition - 1, 0, 3); // Clamp in 0-3 range.
                var bottomTexture = (int)MathHelper.Clamp(topTexture - 1, 0, 3); // Clamp in 0-3 range.

                // Get Normalized texture values for texture-2, in layer 1
                var normalizedTexture2Position = CalcTextureNormalizedPosition(texturePosition);

                // Update Min-Max arrays, just to be sure.
                CalculateMinMaxHeightRanges();

                var height1 = _textureMap.Height; // 11/19/2009
                var width1 = _textureMap.Width; // 11/19/2009
                const int scale = TerrainData.cScale; // 11/19/2009

                // 5/17/2010 - Cache collections
                var minRange = MinRange;
                var maxRange = MaxRange;

                // 2nd - Apply Noise values to the Texture channel
                for (var loopY = 0; loopY < height1; loopY++)
                    for (var loopX = 0; loopX < width1; loopX++)
                    {
                        var index = loopX + loopY*width1; // 11/19/2009

                        var height = TerrainData.GetTerrainHeight(loopX*scale, loopY*scale);

                        // Only affect the pixels in the given min-max height range for given texture.
                        if (height < minRange[bottomTexture] || height > maxRange[topTexture]) continue;

                        // Retrieve original color data
                        var pixelColor = bits1[index].ToVector4();

                        // Normalized the Perlin Noise, which is 0-255, to now be 0-1
                        var normalizedPerlinNoiseValue = (noise[index]/255.0f);

                        var textureBlendAmount = CalculateTextureBlend(normalizedPerlinNoiseValue*100.0f);

                        // Apply the normalized textures value with the perlin noise applied.
                        switch (layerToAffect)
                        {
                            case LayerGroup.Layer1:
                                // For Layer-1, the 'blendAmount' is SUBTRACTED from normalized result, because
                                // this will always show some texture amount!
                                pixelColor.X = normalizedTexture2Position - textureBlendAmount*2;

                                // NOTE: 11/19/2009 - This is no longer necessary, since now I look at the Red channels % in the shader!
                                // For Bump, need to store the actual texture Position value (1, 2, 3...) into Blue channel.
                                pixelColor.Z = (texturePosition + 1);

                                break;
                            case LayerGroup.Layer2:
                                // For Layer-2, the 'perlinNoiseValue' is MULTIPLIED against normalized result, because
                                // we want to be able to see the pixels from layer-1 where the result is 0 on layer 2!
                                pixelColor.Y = normalizedTexture2Position*normalizedPerlinNoiseValue;

                                break;
                            default:
                                break;
                        }

                        // Store result back into array
                        bits1[index] = new Color(pixelColor);

                    } // End ForLoop

                // 7/9/2010 - Use new Double buffer technique
                //textureMap.SetData(bits1);
                _bits1 = bits1;
               // _isDirty = true;
                _textureMap = null;
                // XNA 4.0 Updates
                //_textureMaps[0] = new Texture2D(_graphicsDevice, TerrainData.MapWidth, TerrainData.MapHeight, 1, TextureUsage.None, SurfaceFormat.Color);
                _textureMap = new Texture2D(_graphicsDevice, TerrainData.MapWidth, TerrainData.MapHeight, false, SurfaceFormat.Color);
                _textureMap.SetData(_bits1);

            }
            catch (InvalidOperationException)
            {
                Debug.WriteLine("(ApplyPerlinNoise) threw the InvalidOpExp error.");
            }
        }

        // 1/22/2009
        /// <summary>
        /// Updates the <see cref="TerrainAlphaMaps"/>, using the given X/Y coordinates, for the specific
        /// <see cref="LayerGroup"/>.  The map is updated using the internal 'PaintTexture' property,
        /// which must be set first using the 'SetPaintTextureToUse' method call.
        /// </summary>
        /// <param name="x">X value to use</param>
        /// <param name="y">Y value to use</param>        
        public static void UpdateAlphaMap_Fill(int x, int y)
        {
            // 11/20/2009 - Capture 'TileBracket' error, which is thrown sometime when calling 'SetData'.
           
            try
            {
                // 7/15/2009 - Check if Bits needs to be populated.
                if (_bits1 == null || _bits1.Length == 0)
                    PopulateColorBitsFromTexture(_textureMap, out _bits1);
               

                // 1st - Retrieve single Vector4 of Color Bit
                var pixelColor = Vector4.Zero;
                //Vector4 tmpPixelColor = _bits1[mx + my * _textureMaps[0].Height].ToVector4();

                // 2nd - Update pixelColor via Vector4 struct
                if (InUseLayer1)
                {
                    pixelColor.X = _paintTexture.X;
                    pixelColor.Z = _paintTexture.Z; // bumpmap
                }
                if (InUseLayer2)
                {
                    pixelColor.Y = _paintTexture.Y;
                }

                // 5/17/2010 - refactored out code into new reusable method.
                UpdateTextureMapPixel(x, y, ref pixelColor);

               
            }
            catch (InvalidOperationException)
            {
                Debug.WriteLine("(UpdateAlphaMap_Fill) threw the 'InvalidOpExp' error.");
            }
        }

        // 1/22/2009
        /// <summary>
        /// Updates the <see cref="TerrainAlphaMaps"/>, using the given X/Y coordinates, for the specific
        /// <see cref="LayerGroup"/>.  The map is updated by setting all color values to zero.
        /// </summary>
        /// <param name="x">X value to use</param>
        /// <param name="y">Y value to use</param>        
        public static void UpdateAlphaMap_UnFill(int x, int y)
        {
            // 11/20/2009 - Capture 'TileBracket' error, which is thrown sometime when calling 'SetData'.
           
            try
            {
                // 7/15/2009 - Check if Bits needs to be populated.
                if (_bits1 == null || _bits1.Length == 0)
                    PopulateColorBitsFromTexture(_textureMap, out _bits1);

                // 1st - Retrieve single Vector4 of Color Bit
                var pixelColor = Vector4.Zero;
                //Vector4 tmpPixelColor = _bits1[mx + my * _textureMaps[0].Height].ToVector4();

                // 2nd - Update pixelColor to Zero.
                if (InUseLayer1)
                {
                    pixelColor.X = 0;
                    pixelColor.Z = 0; // bump map
                }
                if (InUseLayer2)
                {
                    pixelColor.Y = 0;
                }

                // 5/17/2010 - re factored out code into new reusable method.
                UpdateTextureMapPixel(x, y, ref pixelColor);
               
            }
            catch (InvalidOperationException)
            {
                Debug.WriteLine("(UpdateAlphaMap_UnFill) threw the 'InvalidOpExp' error.");
            }
        }

        // 5/17/2010
        /// <summary>
        /// Helper method, which updates a given <paramref name="pixelColor"/> at the specified
        /// X/Y position in the <see cref="_textureMap"/> collection.
        /// </summary>
        /// <param name="x">X value to use</param>
        /// <param name="y">Y value to use</param>
        /// <param name="pixelColor">New <see cref="Vector4"/> pixel color to apply</param>
        private static void UpdateTextureMapPixel(int x, int y, ref Vector4 pixelColor)
        {
            // 1/27/2009 - Clamp size to map to avoid crashes!
            var paintCursorSize = TerrainEditRoutines.PaintCursorSize; // 5/17/2010
           
            var paintCursorWidth =
                ((int)MathHelper.Clamp(x + paintCursorSize, 0, _textureMap.Width)) - x;
            var paintCursorHeight =
                ((int)MathHelper.Clamp(y + paintCursorSize, 0, _textureMap.Height)) - y;

            if (paintCursorWidth < 1) paintCursorWidth = 1;
            if (paintCursorHeight < 1) paintCursorHeight = 1;

            // 3rd - Update Bit Array with new pixelColor
            var bits1 = _bits1; // 5/17/2010 - Cache
            for (var loopY = 0; loopY < paintCursorHeight; loopY++)
                for (var loopX = 0; loopX < paintCursorWidth; loopX++)
                {
                    bits1[(x + loopX) + (y + loopY) * _textureMap.Height] = new Color(pixelColor);
                }
                
            // 7/9/2010 - Use new Double buffer technique
            //textureMap.SetData(bits1);
            _bits1 = bits1;

            _textureMap = null;
            // XNA 4.0 Updates
            //_textureMaps[0] = new Texture2D(_graphicsDevice, TerrainData.MapWidth, TerrainData.MapHeight, 1, TextureUsage.None, SurfaceFormat.Color);
            _textureMap = new Texture2D(_graphicsDevice, TerrainData.MapWidth, TerrainData.MapHeight, false, SurfaceFormat.Color);
            _textureMap.SetData(_bits1);
        }


        // 4/23/2008 - 
        // 9/15/2008 - Updated to optimize memory.
        ///<summary>
        /// Updates the AlphaMap1 <see cref="LayerGroup"/> percents; called from the PaintTool.
        ///</summary>
        public static void UpdateAlphaMap1Layers()
        {
            // 5/15/2009
            CalculateMinMaxHeightRanges();

            // Init AlphaMap Texture2D
            _textureMap = null;

            // XNA 4.0 Updates
            //_textureMaps[_toEditAlphaMapBuffer] = new Texture2D(_graphicsDevice, TerrainData.MapWidth, TerrainData.MapHeight, 1,TextureUsage.None, SurfaceFormat.Color);
            _textureMap = new Texture2D(_graphicsDevice, TerrainData.MapWidth, TerrainData.MapHeight, false, SurfaceFormat.Color);
            
            // Populate AlphaMap1 using the Height Data.
            PopulateAlphaMap1();

            // 9/15/2008 - Set the AlphaMaps Texture
            SetAlphaMapsTextureEffect();
        }

        // 4/21/2008
        // 9/12/2008 - Updated to optimize memory.       
        /// <summary>
        /// Creates the <see cref="TerrainAlphaMaps"/> which are used in the Shader file to do the MultiTexturing.
        /// The <see cref="TerrainAlphaMaps"/> are <see cref="Texture2D"/> structs.  Each of the 4 channels, (R,G,B,A) for a specific pixel,
        /// are set to 1 or 0; 1 = Use and 0 = Off.  Since they are 4 channels per pixel, we are able to multitexture 4 textures per AlphaMap. 
        /// </summary>
        public static void CreateAlphaMaps()
        {
            // 9/15/2008
            _bits1 = new Color[TerrainData.MapHeight*TerrainData.MapWidth];

            // 9/16/2008
            // Init AlphaMap Texture2D
            InitializeTextureMaps();

            // Populate AlphaMap Layer-2 (Green channel) with zeros
            ClearGivenLayer(LayerGroup.Layer2);

            // Populate AlphaMap1 using the Height Data.
            PopulateAlphaMap1();

            // Set the AlphaMaps Texture
            SetAlphaMapsTextureEffect();
        }

        // 9/16/2008
        /// <summary>
        /// Initializes the <see cref="_textureMap"/> collection.
        /// </summary>
        private static void InitializeTextureMaps()
        {
            _alphaScale = TerrainData.MapWidth;

            // 5/15/2009
            CalculateMinMaxHeightRanges();

            // XNA 4.0 Updates
            _textureMap = null;
            _textureMap =
                new Texture2D(_graphicsDevice, TerrainData.MapWidth, TerrainData.MapHeight, false, SurfaceFormat.Color);

        }

        // 5/15/2009
        /// <summary>
        /// Helper method, which calculates the Min-Max height ranges, used as constraints when 
        /// building the <see cref="TerrainAlphaMaps"/>.
        /// </summary>
        private static void CalculateMinMaxHeightRanges()
        {
            // Calculate the % each Texture layer should be using Max Height value.
            // Default = 25% per Layer.
            if (TerrainData.HeightMapMaxHeight == null) return;

            var heightMax = (float) TerrainData.HeightMapMaxHeight;

            MinRange[0] = 0.0f;
            MinRange[1] = (heightMax*_alphaLy1Percent) - 1;
            MinRange[2] = (heightMax*_alphaLy2Percent) - 1;
            MinRange[3] = (heightMax*_alphaLy3Percent) - 1;

            MaxRange[0] = heightMax*_alphaLy1Percent;
            MaxRange[1] = heightMax*_alphaLy2Percent;
            MaxRange[2] = heightMax*_alphaLy3Percent;
            MaxRange[3] = heightMax*_alphaLy4Percent;
        }

        // 1/22/2009; 5/17/2010: Updated parameter to use the LayerGroup enum. 
        /// <summary>
        /// Clears a given <see cref="LayerGroup"/> by setting the color value to zero.
        /// </summary>
        /// <param name="layerValue"><see cref="LayerGroup"/> to clear</param>
        public static void ClearGivenLayer(LayerGroup layerValue)
        {
            // 11/20/2009 - Capture 'TileBracket' error, which is thrown sometime when calling 'SetData'.
           
            try
            {
                // 7/25/2009 - Check if bits needs to be loaded 1st!
                if (_bits1 == null)
                    PopulateColorBitsFromTexture(_textureMap, out _bits1);

                var bits1 = _bits1; // 5/17/2010 - Cache

                _textureMap.GetData(bits1);
                var height = _textureMap.Height; // 5/18/2010 - Cache
                var width = _textureMap.Width; // 5/18/2010 - Cache
                for (var loopY = 0; loopY < height; loopY++)
                    for (var loopX = 0; loopX < width; loopX++)
                    {
                        // 5/17/2010 - Cache calc
                        var index0 = loopY*height + loopX;

                        switch (layerValue)
                        {
                            case LayerGroup.Layer1:
                                bits1[index0].R = 0;
                                break;
                            case LayerGroup.Layer2:
                                bits1[index0].G = 0;
                                break;
                            default:
                                break;
                        } // End Switch
                    } // End For Loop

                // 7/9/2010 - Use new Double buffer technique
                //textureMap.SetData(bits1);
                _bits1 = bits1;
                //_isDirty = true;

                _textureMap = null;
                // XNA 4.0 Updates
                //_textureMaps[0] = new Texture2D(_graphicsDevice, TerrainData.MapWidth, TerrainData.MapHeight, 1, TextureUsage.None, SurfaceFormat.Color);
                _textureMap = new Texture2D(_graphicsDevice, TerrainData.MapWidth, TerrainData.MapHeight, false, SurfaceFormat.Color);
                _textureMap.SetData(_bits1);

            }
            catch (InvalidOperationException)
            {
                Debug.WriteLine("(ClearGivenLayer) threw the 'InvalidOpExp' error.");

               
            }
        }

        #region Load/Save Routines
#if !XBOX360
        // 8/20/2008 - Updated the calls to pass the Texture2D, rather than the direct Color Bits arrays.
        // 9/6/2008: Updated to save the AlphaMaps to the debug bins 'Content\AlphaMaps' folder; however, the file
        //           will still have to be brought into the Content folder, within the project, and set to Compile
        //           to an '.xnb' file.
        ///<summary>
        /// Saves the <see cref="TerrainAlphaMaps"/> (texture placement mapping) for the current <see cref="TWEngine.Terrain"/> map.
        ///</summary>
        ///<param name="storageTool"><see cref="Storage"/> instance used to save the map</param>
        /// <param name="mapName">MapName</param>
        /// <param name="mapType">MapType; either SP or MP.</param>
        public static void SaveAlphaMaps(Storage storageTool, string mapName, string mapType)
        { 
            // Get Bits
            var saveBits = new Color[_textureMap.Height*_textureMap.Width];
            _textureMap.GetData(saveBits);
            
            //Debugger.Break();

            // 9/23/2010 - XNA 4.0 Updates - Bmp not supported; now use Png.
            // 4/6/2010: UPdated to use 'ContentMApsLoc' global var.
            // Save AlphaBits1
            // ReSharper disable RedundantToStringCall
            int errorCode;
            if (storageTool.StartBitsSaveOperation(saveBits, "tdAlphaBits1.abd",
                                                      String.Format(@"{0}\{1}\{2}\",
                                                                    TemporalWars3DEngine.ContentMapsLoc, mapType,
                                                                    mapName), out errorCode))
                return;

            // 4/9/2010 - Error occured, so check which one.
            if (errorCode == 1)
            {
                MessageBox.Show(@"Locked files detected for 'AlphaMap' (tdAlphaBits1.bmp) save.  Unlock files, and try again.",
                                @"Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (errorCode == 2)
            {
                MessageBox.Show(@"Directory location for 'AlphaMap' (tdAlphaBits1.bmp) save, not found.  Verify directory exist, and try again.",
                                @"Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show(@"Invalid Operation error for 'AlphaMap' (tdAlphaBits1.bmp) save.  Check for file locks, and try again.",
                            @"Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            // ReSharper restore RedundantToStringCall
                   
        }
#endif

        // 11/17/2009: Updated to include 2nd pararm 'MapType'.
        // 7/15/2009: Updated by removing the populating of the Color Bits, which is now only done when editing occurs!
        // 8/20/2008 - Updated the calls to pass the Texture2D, rather than the direct Color Bits arrays.
        // 9/6/2008: Updated to now load the AlphaMaps textures from the 'Content\AlphaMaps' folder, since
        //           now the textures are compiled to an '.xnb.' file, so they can also be loaded on XBOX.
        ///<summary>
        /// Loads the <see cref="TerrainAlphaMaps"/> (texture placement mapping) for the current <see cref="TWEngine.Terrain"/> map.
        ///</summary>
        /// <param name="mapName">MapName</param>
        /// <param name="mapType">MapType; either SP or MP.</param>
        public static void LoadAlphaMaps(string mapName, string mapType)
        {
            try
            {
                // 9/16/2008
                InitializeTextureMaps();

                // Load Texture AlphaMaps
                //_textureMap  = _contentManager.Load<Texture2D>(String.Format(@"{0}\{1}\tdAlphaBits1", mapType, mapName));
                var storageTool = new Storage();
                
                Color[] loadBits;
                if (storageTool.StartBitsLoadOperation("tdAlphaBits1.abd",
                                                          String.Format(@"{0}\{1}\{2}\",
                                                                        TemporalWars3DEngine.ContentMapsLoc, mapType,
                                                                        mapName), out loadBits))
                {
                    //Set texture with bits
                    _textureMap.SetData(loadBits);

                    // 9/10/2008 - Set the AlphaMaps Texture
                    SetAlphaMapsTextureEffect();

                    return;
                }
               
                
            }
            catch (ContentLoadException)
            {
                Debug.WriteLine("ContentLoadException Error in LoadAlphaMaps method.");
            }
        }

        // 7/15/2009
        /// <summary>
        /// Helper method, which takes a <see cref="Texture2D"/> to read from, and returns a collection 
        /// of Color format bits.
        /// </summary>
        /// <param name="texture"><see cref="Texture2D"/> to read from</param>
        /// <param name="bits">(OUT) collection of <see cref="Color"/> format bits</param>
        private static void PopulateColorBitsFromTexture(Texture2D texture, out Color[] bits)
        {
            // SetUp Color Array
            bits = new Color[texture.Width*texture.Height];

            // Populate Color Bit Arrays
            texture.GetData(bits);

            // 7/15/2009 - Test: Reading DXT1 compression. (use byte[(w*h)/2] for dxt1 and  byte[w*h] for 2-5)
            //byte[] dxtBytes = new byte[(_textureMaps[0].Width * _textureMaps[0].Height) / 2];
            //_textureMaps[0].GetData<byte>(dxtBytes);
        }


        // 9/15/2008 - 
        ///<summary>
        /// Sets <see cref="TerrainAlphaMaps"/> Textures into current <see cref="TerrainShape"/> Effect.
        ///</summary>
        public static void SetAlphaMapsTextureEffect()
        {
            TerrainShape.SetAlphaMapsTextureEffect(_alphaScale, _textureMap);
        }

        #endregion



        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // 8/14/2008
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="isDisposing">Is this final dispose?</param>
        private static void Dispose(bool isDisposing)
        {
            if (!isDisposing) return;

            // Dispose of Textures
            if (_textureMap != null)
                _textureMap.Dispose();

            // 1/8/2010 - Clear Arrays
            if (_bits1 != null) 
                Array.Clear(_bits1, 0, _bits1.Length);

            // 1/6/2010 - Unload Content.
            if (_contentManager != null)
            {
                _contentManager.Unload();
            }
        }


        #endregion
    }
}