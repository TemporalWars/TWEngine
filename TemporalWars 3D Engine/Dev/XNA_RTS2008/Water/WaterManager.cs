#region File Description
//-----------------------------------------------------------------------------
// WaterManager.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Diagnostics;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.Interfaces;
using ImageNexus.BenScharbach.TWEngine.Viewports;
using ImageNexus.BenScharbach.TWEngine.Water.Enums;
using ImageNexus.BenScharbach.TWEngine.Water.Structs;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.Water
{
    /// <summary>
    /// The <see cref="WaterManager"/> class keeps copies of the water components, like the <see cref="Ocean"/> or <see cref="Lake"/>, 
    /// and calls the appropriate methods to setup vertices, intialize content, and render and update the components each game cycle.
    /// </summary>
    public class WaterManager : GameComponent, IWaterManager
    {
        private static Ocean _oceanWater;
        private static Lake _lakeWater;

        // 6/1/2010
        private static Game _gameInstance;
       
        // Updated by the 'V' or 'IsVisible' properties.
        ///<summary>
        /// Turn on/off drawing of water component.
        ///</summary>
        public static bool IsVisibleS;

        // 6/1/2010
        private static WaterType _waterTypeToUse;

        // 1/8/2010 - Track if disposed was called.
        private static bool _isDisposed;

        #region Properties

        // 5/22/2010
        /// <summary>
        /// The base water height.
        /// </summary>
        public float WaterHeight
        {
            get 
            {
                switch (WaterTypeToUse)
                {
                    case WaterType.None:
                        return 100f; // default
                    case WaterType.Lake:
                        return _lakeWater.WaterHeight;
                    case WaterType.Ocean:
                        return _oceanWater.WaterHeight;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
               
            }
            set
            {
                switch (WaterTypeToUse)
                {
                    case WaterType.None:
                        break;
                    case WaterType.Lake:
                        _lakeWater.WaterHeight = value;
                        break;
                    case WaterType.Ocean:
                        _oceanWater.WaterHeight = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        ///<summary>
        /// Turn on/off drawing of water component.
        ///</summary>
        public bool IsVisible
        {
            get { return IsVisibleS; }
            set
            {
                IsVisibleS = value;

                // 6/1/2010 - Set to proper water component.
                switch (WaterTypeToUse)
                {
                    case WaterType.None:
                        break;
                    case WaterType.Lake:
                        if (_lakeWater != null) _lakeWater.Visible = value;
                        break;
                    case WaterType.Ocean:
                        if (_oceanWater != null) _oceanWater.Visible = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                } // End Switch
            }
        }

        // 1/20/2009 - shortcut version
        ///<summary>
        ///  Turn on/off drawing of water component.
        ///</summary>
        public bool V
        {
            get { return IsVisibleS; }
            set
            {
                IsVisibleS = value;

                // 6/1/2010 - Set to proper water component.
                switch (WaterTypeToUse)
                {
                    case WaterType.None:
                        break;
                    case WaterType.Lake:
                        if (_lakeWater != null) _lakeWater.Visible = value;
                        break;
                    case WaterType.Ocean:
                        if (_oceanWater != null) _oceanWater.Visible = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                } // End Switch
            }
        }

        /// <summary>
        /// Set the debug <see cref="GameViewPort"/> texture to show via script
        /// </summary>
        /// <param name="textureName">Texture name to use</param>
        public void SetViewportTexture(string textureName)
        {
            // 6/1/2010
            switch (WaterTypeToUse)
            {
                case WaterType.None:
                    break;
                case WaterType.Lake:
                    if (_lakeWater != null) _lakeWater.SetViewportTexture(textureName);
                    break;
                case WaterType.Ocean:
                    if (_oceanWater != null) _oceanWater.SetViewportTexture(textureName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            } // End Switch
           
        }

        /// <summary>
        /// During debugging, this sets which <see cref="ViewPortTexture"/>
        /// to display in the <see cref="GameViewPort"/>.
        /// </summary>
        public ViewPortTexture ShowTexture
        {
            get
            {
                // 6/1/2010
                switch (WaterTypeToUse)
                {
                    case WaterType.None:
                        break;
                    case WaterType.Lake:
                        if (_lakeWater != null) return _lakeWater.ShowTexture;
                        break;
                    case WaterType.Ocean:
                        if (_oceanWater != null) return _oceanWater.ShowTexture;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                } // End Switch

                // 6/1/2010 - Return as default.
                return ViewPortTexture.Refraction;
            }
            set
            {
                // 6/1/2010
                switch (WaterTypeToUse)
                {
                    case WaterType.None:
                        break;
                    case WaterType.Lake:
                        if (_lakeWater != null) _lakeWater.ShowTexture = value;
                        break;
                    case WaterType.Ocean:
                        if (_oceanWater != null) _oceanWater.ShowTexture = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                } // End Switch
            }
        }

        // 6/1/2010 - Interface not static version.
        WaterType IWaterManager.WaterTypeToUse
        {
            get
            {
                return WaterTypeToUse; 
            }
            set
            {
                WaterTypeToUse = value;
            }
        }

        /// <summary>
        /// <see cref="WaterType"/> Enum to use; for example, Lake or Ocean.
        /// </summary>
        public static WaterType WaterTypeToUse
        {
            get
            {
                return _waterTypeToUse;
            }
            set
            {
                // 6/1/2010
                switch (WaterTypeToUse)
                {
                    case WaterType.None:
                        IsVisibleS = false;
                        // Dispose of water components
                        if (_lakeWater != null) _lakeWater.Dispose(true);
                        if (_oceanWater != null) _oceanWater.Dispose(true);
                        _lakeWater = null;
                        _oceanWater = null;
                        break;
                    case WaterType.Lake:
                        CreateWaterType(value, 100f);
                        IsVisibleS = true;
                        break;
                    case WaterType.Ocean:
                        CreateWaterType(value, 100f);
                        IsVisibleS = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                _waterTypeToUse = value;
            }
        }

        #region Implementation of ILake

        /// <summary>
        /// Wave length
        /// </summary>
        public float WaveLength
        {
            get
            {
                return _lakeWater != null ? _lakeWater.WaveLength : 0f;
            }
            set { if (_lakeWater != null) _lakeWater.WaveLength = value; }
        }

        /// <summary>
        /// Wave height
        /// </summary>
        public float WaveHeight
        {
            get
            {
                return _lakeWater != null ? _lakeWater.WaveHeight : 0f;
            }
            set { if (_lakeWater != null) _lakeWater.WaveHeight = value; }
        }

        /// <summary>
        /// Wave speed
        /// </summary>
        public float WaveSpeed
        {
            get
            {
                return _lakeWater != null ? _lakeWater.WaveSpeed : 1f;
            }
            set { if (_lakeWater != null) _lakeWater.WaveSpeed = value; }
        }

        /// <summary>
        /// Wind direction as Vector3.
        /// </summary>
        public Vector3 WindDirection
        {
            get
            {
                return _lakeWater != null ? _lakeWater.WindDirection : Vector3.Zero;
            }
            set { if (_lakeWater != null) _lakeWater.WindDirection = value; }
        }

        /// <summary>
        /// Wind force as float.
        /// </summary>
        public float WindForce
        {
            get
            {
                return _lakeWater != null ? _lakeWater.WindForce : 0f;
            }
            set { if (_lakeWater != null) _lakeWater.WindForce = value; }
        }

        /// <summary>
        /// Sunlight direction as Vector3.
        /// </summary>
        public Vector3 SunlightDirection
        {
            get
            {
                return _lakeWater != null ? _lakeWater.SunlightDirection : Vector3.Zero;
            }
            set { if (_lakeWater != null) _lakeWater.SunlightDirection = value; }
        }

        /// <summary>
        /// Color tone of water.
        /// </summary>
        public Vector4 DullColor
        {
            get
            {
                return _lakeWater != null ? _lakeWater.DullColor : Vector4.Zero;
            }
            set { if (_lakeWater != null) _lakeWater.DullColor = value; }
        }

        /// <summary>
        /// Use distort vertices option when drawing water.
        /// </summary>
        public bool UseDistortVertices
        {
            get
            {
                return _lakeWater != null && _lakeWater.UseDistortVertices;
            }
            set { if (_lakeWater != null) _lakeWater.UseDistortVertices = value; }
        }

        /// <summary>
        /// Frequency of waves
        /// </summary>
        public Vector4 WaveFreqs
        {
            get
            {
                return _lakeWater != null ? _lakeWater.WaveFreqs : Vector4.Zero;
            }
            set { if (_lakeWater != null) _lakeWater.WaveFreqs = value; }
        }

        /// <summary>
        /// Wave height
        /// </summary>
        public Vector4 WaveHeights
        {
            get
            {
                return _lakeWater != null ? _lakeWater.WaveHeights : Vector4.Zero;
            }
            set { if (_lakeWater != null) _lakeWater.WaveHeights = value; }
        }

        /// <summary>
        /// Wave length
        /// </summary>
        public Vector4 WaveLengths
        {
            get
            {
                return _lakeWater != null ? _lakeWater.WaveLengths : Vector4.Zero;
            }
            set { if (_lakeWater != null) _lakeWater.WaveLengths = value; }
        }

        ///<summary>
        /// Sets direction of waves.
        ///</summary>
        ///<param name="value">Array of Vector2</param>
        public void SetWaveDirs(Vector2[] value)
        {
            if (_lakeWater != null) _lakeWater.SetWaveDirs(value);
        }

        /// <summary>
        /// Returns array of Vector2 for wave directions.
        /// </summary>
        /// <returns>Array of Vector2</returns>
        public Vector2[] GetWaveDirs()
        {
            return _lakeWater != null ? _lakeWater.GetWaveDirs() : new Vector2[] {};
        }

        #endregion

        #region Implementation of IOcean

        /// <summary>
        /// Ocean wave amplitude
        /// </summary>
        public float OceanWaveAmplitude
        {
            get
            {
                return _oceanWater != null ? _oceanWater.OceanWaveAmplitude : 0f;
            }
            set { if (_oceanWater != null) _oceanWater.OceanWaveAmplitude = value; }
        }

        /// <summary>
        /// Ocean wave frequency
        /// </summary>
        public float OceanWaveFrequency
        {
            get
            {
                return _oceanWater != null ? _oceanWater.OceanWaveFrequency : 0f;
            }
            set { if (_oceanWater != null) _oceanWater.OceanWaveFrequency = value; }
        }

        /// <summary>
        /// Ocean bump-map height
        /// </summary>
        public float OceanBumpHeight
        {
            get
            {
                return _oceanWater != null ? _oceanWater.OceanBumpHeight : 0f;
            }
            set { if (_oceanWater != null) _oceanWater.OceanBumpHeight = value; }
        }

        /// <summary>
        /// Ocean deep color
        /// </summary>
        public Vector4 OceanDeepColor
        {
            get
            {
                return _oceanWater != null ? _oceanWater.OceanDeepColor : Vector4.Zero;
            }
            set { if (_oceanWater != null) _oceanWater.OceanDeepColor = value; }
        }

        /// <summary>
        /// Ocean shallow color
        /// </summary>
        public Vector4 OceanShallowColor
        {
            get
            {
                return _oceanWater != null ? _oceanWater.OceanShallowColor : Vector4.Zero;
            }
            set { if (_oceanWater != null) _oceanWater.OceanShallowColor = value; }
        }

        /// <summary>
        /// Ocean texture scale
        /// </summary>
        public Vector2 OceanTextureScale
        {
            get
            {
                return _oceanWater != null ? _oceanWater.OceanTextureScale : Vector2.Zero;
            }
            set { if (_oceanWater != null) _oceanWater.OceanTextureScale = value; }
        }

        /// <summary>
        /// Ocean wave speed
        /// </summary>
        public Vector2 OceanWaveSpeed
        {
            get
            {
                return _oceanWater != null ? _oceanWater.OceanWaveSpeed : Vector2.Zero;
            }
            set { if (_oceanWater != null) _oceanWater.OceanWaveSpeed = value; }
        }

        /// <summary>
        /// Ocean fresnel bias
        /// </summary>
        public float OceanFresnelBias
        {
            get
            {
                return _oceanWater != null ? _oceanWater.OceanFresnelBias : 0f;
            }
            set { if (_oceanWater != null) _oceanWater.OceanFresnelBias = value; }
        }

        /// <summary>
        /// Ocean fresnel power
        /// </summary>
        public float OceanFresnelPower
        {
            get
            {
                return _oceanWater != null ? _oceanWater.OceanFresnelPower : 1f; // min is 1.
            }
            set { if (_oceanWater != null) _oceanWater.OceanFresnelPower = value; }
        }

        /// <summary>
        /// Ocean HDR multiplier
        /// </summary>
        public float OceanHDRMultiplier
        {
            get
            {
                return _oceanWater != null ? _oceanWater.OceanHDRMultiplier : 0f;
            }
            set { if (_oceanWater != null) _oceanWater.OceanHDRMultiplier = value; }
        }

        /// <summary>
        /// Ocean reflection amount, determines the influence of the reflection
        /// shown on the water surface.
        /// </summary>
        public float OceanReflectionAmt
        {
            get
            {
                return _oceanWater != null ? _oceanWater.OceanReflectionAmt : 0f;
            }
            set { if (_oceanWater != null) _oceanWater.OceanReflectionAmt = value; }
        }

        /// <summary>
        /// Ocean water amount, determines the influence of the ocean floor shown.
        /// </summary>
        public float OceanWaterAmt
        {
            get
            {
                return _oceanWater != null ? _oceanWater.OceanWaterAmt : 0f;
            }
            set { if (_oceanWater != null) _oceanWater.OceanWaterAmt = value; }
        }

        /// <summary>
        /// Ocean reflection color bias.
        /// </summary>
        public Vector4 OceanReflectionColor
        {
            get
            {
                return _oceanWater != null ? _oceanWater.OceanReflectionColor : Vector4.Zero;
            }
            set { if (_oceanWater != null) _oceanWater.OceanReflectionColor = value; }
        }

        /// <summary>
        /// Ocean sky amount, determines the influence of the sky clouds shown on
        /// the water surface.
        /// </summary>
        public float OceanReflectionSkyAmt
        {
            get
            {
                return _oceanWater != null ? _oceanWater.OceanReflectionSkyAmt : 0f;
            }
            set { if (_oceanWater != null) _oceanWater.OceanReflectionSkyAmt = value; }
        }

        #endregion

        #endregion

        ///<summary>
        /// Constructor, which sets the <see cref="WaterType.Ocean"/> as default.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        public WaterManager(Game game)
            : base(game)
        {
            // 12/16/2009 - Default type to use
            WaterTypeToUse = WaterType.None; // 6/1/2010: Updated to 'None'

            // 6/1/2010 - Save 'Game' ref
            _gameInstance = game;
           
        }
        

        // 6/1/2010
        /// <summary>
        /// Used to instatiate the given <see cref="WaterType"/>.
        /// </summary>
        /// <param name="waterType"><see cref="WaterType"/> to create</param>
        /// <param name="waterHeight">Base level height for water.</param>
        public static void CreateWaterType(WaterType waterType, float waterHeight)
        {
            // Instantiate water type
            switch (waterType)
            {
                case WaterType.None:
                    break;
                case WaterType.Lake:
                    _lakeWater = new Lake(_gameInstance, waterHeight) { WaterHeight = waterHeight };
                    break;
                case WaterType.Ocean:
                    _oceanWater = new Ocean(_gameInstance, waterHeight) { WaterHeight = waterHeight };
                    break;
                default:
                    throw new ArgumentOutOfRangeException("waterType");
            }

            // 6/1/2010 - Load content for given WaterType.
            LoadContent(waterType);
        }

        // 1/8/2010
        /// <summary>
        /// Loads resources and graphics content.
        /// </summary>
        /// <param name="waterType"><see cref="WaterType"/> to load content for.</param>
        private static void LoadContent(WaterType waterType)
        {
            // 6/1/2010
            switch (waterType)
            {
                case WaterType.None:
                    break;
                case WaterType.Lake:
                    _lakeWater.LoadContent();
                    break;
                case WaterType.Ocean:
                    _oceanWater.LoadContent();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("waterType");
            }

            // reset value.
            _isDisposed = false; 
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // 5/29/2012 - Skip if game paused.
            if (TemporalWars3DEngine.GamePaused)
                return;

            base.Update(gameTime);

            // 1/8/2010 - When Disposed, just return.
            if (_isDisposed) return;
            
            // 6/1/2010 - Capture NullRef
            try
            {
                switch (WaterTypeToUse)
                {
                    case WaterType.None:
                        break;
                    case WaterType.Lake:
                        _lakeWater.Update(gameTime);
                        break;
                    case WaterType.Ocean:
                        _oceanWater.Update(gameTime);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (NullReferenceException)
            {
                Debug.WriteLine(@"Update method, in 'WaterManager', threw the NullRef exception; however, fixed by calling the 'CreateWaterType'.");

                // If null, then need to create given waterType.
                CreateWaterType(WaterTypeToUse, 100.0f);
            }
            
        }

        /// <summary>
        /// Renders the water component to screen
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public static void RenderWater(GameTime gameTime)
        {
            if (!IsVisibleS) return;

            // 1/8/2010 - When Disposed, just return.
            if (_isDisposed) return;

            // 6/1/2010 - Capture NullRef
            try
            {
                switch (WaterTypeToUse)
                {
                    case WaterType.None:
                        break;
                    case WaterType.Lake:
                        _lakeWater.Draw(gameTime);
                        break;
                    case WaterType.Ocean:
                        _oceanWater.Draw(gameTime);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (NullReferenceException)
            {
#if DEBUG
                Debug.WriteLine(
                    @"Render method, in 'WaterManager', threw the NullRef exception; however, fixed by calling the 'CreateWaterType'.");
#endif

                // If null, then need to create given waterType.
                CreateWaterType(WaterTypeToUse, 100.0f);
            }

        }

        /// <summary>
        /// Gets all <see cref="Lake"/> components attributes, and returns to
        /// caller in the <see cref="WaterData"/> struct.
        /// </summary>
        /// <param name="waterData"><see cref="WaterData"/> with <see cref="Lake"/> attributes</param>
        public static void GetWaterDataAttributes(out WaterData waterData)
        {
            // Create new instance.
            waterData = new WaterData
                            {
                                WaterTypeToUse = WaterTypeToUse,  // 6/1/2010 - Store 'WaterTypeToUse' Enum.
                                UseWater = IsVisibleS // 6/1/2010 - Store 'UseWater'.
                            }; 


            // 6/1/2010
            switch (WaterTypeToUse)
            {
                case WaterType.None:
                    break;
                case WaterType.Lake:
                    // Update Lake Atts
                    _lakeWater.GetWaterDataAttributes(ref waterData);
                    break;
                case WaterType.Ocean:
                    // Update Ocean Atts
                    _oceanWater.GetWaterDataAttributes(ref waterData);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            } // End Switch
            
        }

        /// <summary>
        /// Sets all the <see cref="Lake"/> attributes using the given
        /// <see cref="WaterData"/> struct.
        /// </summary>
        /// <param name="waterData"><see cref="WaterData"/> with <see cref="Lake"/> attributes</param>
        public static void SetWaterDataAttributes(ref WaterData waterData)
        {
            // 6/1/2010 - Set 'UseWater'.
            IsVisibleS = waterData.UseWater;
            // 6/1/2010 - Set 'WaterTypeToUse'
            WaterTypeToUse = waterData.WaterTypeToUse;

            // 6/1/2010 - Create given 'WaterTypeToUse', if not None.
            if (WaterTypeToUse != WaterType.None)
                CreateWaterType(WaterTypeToUse, waterData.WaterHeight);

            // 6/1/2010
            switch (WaterTypeToUse)
            {
                case WaterType.None:
                    break;
                case WaterType.Lake:
                    // Reapply Lake Atts
                    _lakeWater.SetWaterDataAttributes(ref waterData);
                    break;
                case WaterType.Ocean:
                    // Reapply Ocean Atts
                    _oceanWater.SetWaterDataAttributes(ref waterData);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            } // End Switch

        }

        // 1/8/2010
        /// <summary>
        /// Dispose of internal resources.
        /// </summary>
        /// <param name="disposing"></param>
        void IWaterManager.Dispose(bool disposing)
        {
            Dispose(disposing);
        }
       
        /// <summary>
        /// Dispose of items.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            // Dispose of items
            if (_lakeWater != null) _lakeWater.Dispose(disposing);
            if (_oceanWater != null) _oceanWater.Dispose(disposing);
            // 1/8/2010
            _isDisposed = true;

            if (disposing)
            {
                // Null regs
                _lakeWater = null;
                _oceanWater = null;
            }

            base.Dispose(disposing);
        }

       
    }
}