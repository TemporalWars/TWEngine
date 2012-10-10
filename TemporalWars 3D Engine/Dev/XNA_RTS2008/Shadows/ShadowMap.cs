#region File Description
//-----------------------------------------------------------------------------
// ShadowMap.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.Common.Extensions;
using ImageNexus.BenScharbach.TWEngine.GameCamera;
using ImageNexus.BenScharbach.TWEngine.HandleGameInput;
using ImageNexus.BenScharbach.TWEngine.IFDTiles;
using ImageNexus.BenScharbach.TWEngine.InstancedModels;
using ImageNexus.BenScharbach.TWEngine.Interfaces;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWEngine.Shadows.Enums;
using ImageNexus.BenScharbach.TWEngine.Shadows.Structs;
using ImageNexus.BenScharbach.TWEngine.Terrain;
using ImageNexus.BenScharbach.TWEngine.Viewports;
using ImageNexus.BenScharbach.TWEngine.Viewports.Structs;
using ImageNexus.BenScharbach.TWTools.PerfTimersComponent.Timers;
using ImageNexus.BenScharbach.TWTools.PerfTimersComponent.Timers.Enums;
using ImageNexus.BenScharbach.TWTools.ScreenTextDisplayer.ScreenText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ImageNexus.BenScharbach.TWEngine.Shadows
{
    
    /// <summary>
    /// The <see cref="ShadowMap"/> is used to create the shadow maps of the <see cref="SceneItem"/>
    /// and the <see cref="TWEngine.Terrain"/>.  These are then passed to the objects which request them;  
    /// for example, the <see cref="TWEngine.Terrain"/>.
    /// </summary>
    public sealed class ShadowMap : DrawableGameComponent, IShadowMap
    {               
        // 9/10/2008
// ShadowMap        
        private static float _shadowMapDepthBias = 0.0001f; // 6/4/2009 
        private static Texture2D _shadowMapTerrainTexture;

        private static Vector3 _lightDirection;
        private static readonly Vector3[] FrustumCorners = new Vector3[8];

        // ReSharper disable InconsistentNaming
        ///<summary>
        /// Show shadows?
        ///</summary>
        private static bool isVisible; // 5/26/2009
        // ReSharper restore InconsistentNaming
      
        // 9/10/2008 - 
        /// <summary>
        /// Dynamic <see cref="SceneItem"/> shadow map.
        /// </summary>
        private static Texture2D _shadowMapDynamicItemsTexture;

        // 9/10/2008 - 
        /// <summary>
        /// Boolean value to track when preShadowMap textures need to be created
        /// </summary>
        private static bool _doPreShadowMapTextures = true;

        // 4/3/2009 - 
        private static Vector2 _halfPixel;
        
        // 12/6/2009 - 
        /// <summary>
        /// Shadow quality to use for this game session.
        /// </summary>
        private static ShadowQuality _shadowQuality = ShadowQuality.High;
        
        // NOTE: (8/17/2010) Anything changed or added to this ShadowType, 
        // NOTE:  MUST be done to Enum ShadowType in TWTerrainToolsWPF.
        // 12/12/2009 - 
        ///<summary>
        ///The <see cref="ShadowType"/> Enum 
        ///</summary>
        public enum ShadowType
        {
            ///<summary>
            /// Draws the <see cref="ShadowMap"/> to its final target, using
            /// the 'Simple' shadowing technique.
            ///</summary>
            Simple = 0,
            ///<summary>
            /// Draws the <see cref="ShadowMap"/> to its final target, using
            /// the 'PCF' shadowing technique#1.
            ///</summary>
// ReSharper disable InconsistentNaming
            PercentageCloseFilter_1 = 1,
// ReSharper restore InconsistentNaming
            ///<summary>
            /// Draws the <see cref="ShadowMap"/> to its final target, using
            /// the 'PCF' shadowing technique#2.
            ///</summary>
            // ReSharper disable InconsistentNaming
            PercentageCloseFilter_2 = 2,
            // ReSharper restore InconsistentNaming
            ///<summary>
            /// Draws the <see cref="ShadowMap"/> to its final target, using
            /// the 'Variance' shadowing technique.
            ///</summary>
            Variance = 3
        }
        // 12/12/2009 - 
        /// <summary>
        /// The <see cref="ShadowType"/> currently in use.
        /// </summary>
        /// <remarks>Defaults to the <see cref="ShadowType.PercentageCloseFilter_1"/></remarks>
        private static ShadowType _useShadowType = ShadowType.PercentageCloseFilter_1;

        // 12/12/2009 - 
        /// <summary>
        /// ShadowMap darkness level.
        /// </summary>
        /// <remarks>default = 0.2f</remarks>
        private static float _shadowMapDarkness = 0.2f;

#if XBOX360
        static bool _cameraUpdated;
#endif
              
        // 10/15/2008 - ScreenTextItems
        private static ScreenTextItem _screenText1;
        private static ScreenTextItem _screenText2;
        private static ScreenTextItem _screenText3;
        private static ScreenTextItem _screenText4;

        // 6/26/2009 - Add GameViewPortItem
        private static GameViewPortItem _gameViewPortItem;

        // Show Shadow Debug Values?
// ReSharper disable InconsistentNaming
        internal static bool _DebugValues;
// ReSharper restore InconsistentNaming

        // 1/30/2009 - 
        /// <summary>
        /// Hack: Used just to keep init value, since the Visible
        ///       property will be set to false in the constructor; this is because it
        ///       needs to be turned off until the InitSettings method is called.
        ///       Once this is called, the original Visible value the user intended
        ///       will be reset.
        /// </summary>
        private static bool _originalVisibleValue;      

        private static Vector3 _lightTarget;

        // 3/22/2011 - XNA 4.0 Updates - 
        // NOTE: Since 4.0 removed GetTexture on RT, and texture ref is same as original RT when calling texture = renderTarget,
        //       conquently the XNA 4.0 will throw an invalid op exception on the shader draw call.  Therefore, need to ping-pong
        //       between 2 seperate RTs.
        private static RenderTarget2D _shadowRt;
        private static RenderTarget2D _shadowTerrainRt;

        // 9/17/2010 - XNA 4.0 Updates; 'DepthStencilBuffer' is gone!
        //private static DepthStencilBuffer _shadowDb; 
     
        // 1/6/2009 - 
        /// <summary>
        /// If initial try in setting <see cref="IsVisible"/> property failed, this
        /// is set so <see cref="ShadowMap"/> keeps trying until successful.
        /// </summary>
        private static bool _requiresSetShadowMapSettings;
        
        // These values are set from the TerrainScreen Class       
        private static Matrix _lightView;
        private static Matrix _lightProj;
        private static Matrix _lightViewStatic; // 7/10/2009       
        private static Matrix _lightProjStatic; // 7/10/2009 
// ReSharper disable InconsistentNaming
        /// <summary>
        /// The PCF sampling values.
        /// </summary>
        private static readonly Vector2[] pcfSamples = new Vector2[9];    
// ReSharper restore InconsistentNaming

        // 1/23/2009
        // The size of the shadow map
        // The larger the size the more detail we will have for our entire scene
        // However, the FPS will drop substantially, depending on the hosts hardware
        // specs!
#if XBOX360
        private const SurfaceFormat ShadowMapSurfaceFormat = SurfaceFormat.Single;
        private const int _shadowMapWidth = 2048; // 1280
        private const int _shadowMapHeight = 2048; // 720

#else
        private const SurfaceFormat ShadowMapSurfaceFormat = SurfaceFormat.Single; // was HalfVector2, when using variance.
        private static int _shadowMapWidth; // 512;1024;2048;4096       
        private static int _shadowMapHeight;  // was - ImageNexusRTSGameEngine.GameInstance.GraphicsDevice.Viewport.Height             
#endif

         // Terrain Interface Reference
        private static ITerrainShape _terrainShape;   
       

        // DEBUG:
        // 6/4/2008 - 
        /// <summary>
        /// DEBUG which Attribute - Used to change an attribute using number keypad.
        /// </summary>
        private static DebugIsFor _debugIsFor = DebugIsFor.LightPosition;

        // 6/1/2010
        /// <summary>
        /// Holds the <see cref="DebugIsFor"/> Enum as string, to eliminate heap garbage.
        /// </summary>
        private static string _debugIsForString;

        #region Properties

        ///<summary>
        /// Boolean value to track when pre-ShadowMap textures need to be created
        ///</summary>
        public static bool DoPreShadowMapTextures
        {
            set { _doPreShadowMapTextures = value; }
        }  

        /// <summary>
        /// <see cref="ShadowMap"/> <see cref="Effect"/> shader
        /// </summary>
        public static Effect ShadowMapEffect { get; private set; }

        // 6/4/2009
        /// <summary>
        /// Holds the <see cref="ShadowMap"/> depth <see cref="Texture2D"/> data.
        /// </summary>
        public static Texture2D ShadowMapTexture
        {
            get { return _shadowMapDynamicItemsTexture; } // 3/24/2010
        }

        // 7/13/2009
        /// <summary>
        /// <see cref="ShadowMap"/> of <see cref="TWEngine.Terrain"/> depth <see cref="Texture2D"/> data.
        /// </summary>
        public static Texture2D ShadowMapTerrainTexture
        {
            get { return _shadowMapTerrainTexture; }
        }     

        // 6/4/2009
        /// <summary>
        /// <see cref="ShadowMap"/> depthBias to use; for example, 0.01f.
        /// </summary>
        public static float ShadowMapDepthBias
        {
            get { return _shadowMapDepthBias; }
            set { _shadowMapDepthBias = value; }
        }

        /// <summary>
        /// <see cref="ShadowMap"/> target for the light source.
        /// </summary>
        public static Vector3 LightTarget
        {
            get { return _lightTarget; }
            set { _lightTarget = value; }
        }

        /// <summary>
        /// <see cref="ShadowMap"/> light view <see cref="Matrix"/>
        /// </summary>
        public static Matrix LightView
        {
            get { return _lightView; }
        }       

        /// <summary>
        /// <see cref="ShadowMap"/> light projection <see cref="Matrix"/>
        /// </summary>
        public static Matrix LightProj
        {
            get { return _lightProj; }
        }

        /// <summary>
        /// <see cref="ShadowMap"/> STATIC light view <see cref="Matrix"/>, used for the
        /// ShadowMapTerrain depth map.
        /// </summary>
        public static Matrix LightViewStatic
        {
            get { return _lightViewStatic; }
        }

        /// <summary>
        /// <see cref="ShadowMap"/> STATIC light projection <see cref="Matrix"/>, used for the
        /// ShadowMapTerrain depth map.
        /// </summary>
        public static Matrix LightProjStatic
        {
            get { return _lightProjStatic; }
        }

        // 2/16/2009 - 
        /// <summary>
        /// PCF collection samples 
        /// </summary>
        /// <remarks>For PCF (Percentage Close Filtering) sampling method only!</remarks>
        public static Vector2[] PcfSamples
        {
            get { return pcfSamples; }            
        }

        /// <summary>
        /// Half pixel adjustment.
        /// </summary>
        public static Vector2 HalfPixel
        {
            get { return _halfPixel; }
        }



        ///<summary>
        /// Show shadows?
        ///</summary>
        public static bool IsVisibleS
        {
            get { return isVisible; }
        }

        ///<summary>
        /// Show shadows?
        ///</summary>
        public bool IsVisible
        {
            get { return Visible; }
            set 
            { 
                Visible = value;
                isVisible = value; // 5/26/2009
               
                // 1/30/2009 - Store value, since it needs to be set back after InitSettings is called!
                _originalVisibleValue = value;                

                _doPreShadowMapTextures = true;
            
            }
        }

        // 1/21/2009 - Shortcut version
// ReSharper disable UnusedMember.Global
        ///<summary>
        /// Shadow shadows?
        ///</summary>
        /// <remarks>'V' is shortcut for <see cref="IsVisible"/></remarks>
        public bool V
// ReSharper restore UnusedMember.Global
        {
            get { return Visible; }
            set
            {
                Visible = value;               

                _doPreShadowMapTextures = true;

            }
        }

#if !XBOX360
        // 6/5/2009: Do not make STATIC, otherwise, the Python GameConsole will not be able to access.
// ReSharper disable UnusedMember.Global
        ///<summary>
        /// Allows debuging creation of <see cref="ShadowMap"/>, and updating
        /// of the light source.
        ///</summary>
        public bool DebugValues
// ReSharper restore UnusedMember.Global
        {
            get { return _DebugValues; }
            set
            {
                _DebugValues = value;

                // 1/11/2011 - Set Visible flag for all screenText items.
                _screenText1.Visible = value;
                _screenText2.Visible = value;
                _screenText3.Visible = value;
                _screenText4.Visible = value;

                // 12/6/2009 - Make sure GameViewPort is set.
                TemporalWars3DEngine.EngineGameConsole.GVP.Visible = value;

            }
        }
#endif

        // 12/6/2009
        /// <summary>
        /// Sets the <see cref="ShadowQuality"/> Enum, to one of three settings;
        /// 1) Low = 1024x
        /// 2) Med = 2048x
        /// 3) High = 4096x
        /// </summary>
        public static ShadowQuality ShadowQuality
        {
            get { return _shadowQuality; }
            set
            {
                _shadowQuality = value;

                // Reset the values.
                UpdateShadowQualitySettings();
            }
        }

        // 12/12/2009
        /// <summary>
        /// <see cref="ShadowType"/> Enum as technique to use;
        /// 1) Simple
        /// 2) Percentage-Close-Filter#1 (Technique#1)
        /// 3) Percentage-Close-Filter#2 (Technique#2)
        /// 4) Variance
        /// </summary>
        public static ShadowType UseShadowType
        {
            get { return _useShadowType; }
            set
            {
                _useShadowType = value; 

                // set into shader
                if (_terrainShape != null) _terrainShape.SetShadowMapType(_useShadowType);
            }
        }

        // 12/12/2009
        /// <summary>
        /// Sets the <see cref="ShadowMap"/> darkness, using a value between 0-1.0, with
        /// 1.0 being completely white with no shadows.
        /// </summary>
        public static float ShadowMapDarkness
        {
            get { return _shadowMapDarkness; }
            set
            {
                _shadowMapDarkness = value;

                // set into shader
                if (_terrainShape != null) _terrainShape.SetShadowMapDarkness(value);
            }
        }

        // 6/1/2010
        /// <summary>
        /// DEBUG which Attribute - Used to change an attribute using number keypad.
        /// </summary>
        public static DebugIsFor DebugIsFor
        {
            get { return _debugIsFor; }
            set
            {
                _debugIsFor = value;
                // 6/1/2010
                _debugIsForString = value.ToString();
            }
        }

        

        #endregion
       
        ///<summary>
        /// Constructor, which retrieves game services like <see cref="ITerrainShape"/>, creates
        /// the <see cref="ScreenTextItem"/>, and hooks the internal <see cref="EventHandler"/> to the
        /// 'SceneItemCreated' and 'CameraUpdated'.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        public ShadowMap(Game game)
            : base(game)
        {           
            
            // Set a Reference to the Terrain Interface
            _terrainShape = (ITerrainShape)Game.Services.GetService(typeof(ITerrainShape));

            // 12/6/2009
            UpdateShadowQualitySettings();
            
            // Init ScreenText Class
            ScreenTextManager.AddNewScreenTextItem(string.Empty, new Vector2(550, 530), Color.WhiteSmoke, out _screenText1);
            ScreenTextManager.AddNewScreenTextItem(string.Empty, new Vector2(550, 550), Color.WhiteSmoke, out _screenText2);
            ScreenTextManager.AddNewScreenTextItem(string.Empty, new Vector2(550, 570), Color.WhiteSmoke, out _screenText3);
            ScreenTextManager.AddNewScreenTextItem(string.Empty, new Vector2(550, 590), Color.Yellow, out _screenText4);

            // 6/26/2009 - Create gameViewPort for Debug Purposes of showing RenderTarget Texture
            GameViewPort.AddNewGameViewPortItem(_shadowMapDynamicItemsTexture, new Rectangle(0, 0, 256, 256), out _gameViewPortItem);
            // 3/23/2010 - Set this to be 'InUse', so item draws.
            _gameViewPortItem.InUse = true;

            // 10/19/2008 - Add EventHandler for SceneItemOwner Placement; will be used to update the Static ShadowMap
            IFDTileManager.SceneItemPlaced += InterFaceDisplaySceneItemPlaced;

            DrawOrder = 1; // was 1

            // 10/31/2008 - Turned off ShadowMap during Init, since LightPos cannot be set until the TerrainShape class
            //              is created when a level is loaded; therefore, the 'Visible' & 'Enabled' properties will be
            //              Enabled during a level load in the 'TerrainScreen' class.  Furthermore, the 'InitShadowLightSettings'
            //              is also required before using this component.            
            Visible = false;
            Enabled = false;

            // 7/24/2009 - Capture camera movement, to force an update to ShadowMap.
            Camera.CameraUpdated += CameraUpdated;
            
        }

        // 12/6/2009
        /// <summary>
        /// Sets the <see cref="ShadowMap"/> Width/Height, to the proper values depending
        /// on the current <see cref="ShadowQuality"/> Enum; also builds the PCF samples array.
        /// </summary>
        internal static void UpdateShadowQualitySettings()
        {
            
#if !XBOX360
            _shadowMapWidth = (_shadowQuality == ShadowQuality.Low) ? 1024 : (_shadowQuality == ShadowQuality.Medium) ? 2048 : 4096;
            _shadowMapHeight = (_shadowQuality == ShadowQuality.Low) ? 1024 : (_shadowQuality == ShadowQuality.Medium) ? 2048 : 4096;
#endif

            // 2/16/2009 - Setup PCF Sampling Array
            {
                var texelSize = 1.0f/_shadowMapWidth;

                pcfSamples[0] = new Vector2(0.0f, 0.0f);
                pcfSamples[1] = new Vector2(-texelSize, 0.0f);
                pcfSamples[2] = new Vector2(texelSize, 0.0f);
                pcfSamples[3] = new Vector2(0.0f, -texelSize);
                pcfSamples[4] = new Vector2(-texelSize, -texelSize);
                pcfSamples[5] = new Vector2(texelSize, -texelSize);
                pcfSamples[6] = new Vector2(0.0f, texelSize);
                pcfSamples[7] = new Vector2(-texelSize, texelSize);
                pcfSamples[8] = new Vector2(texelSize, texelSize);
            }
        }


        // 7/24/2009
        /// <summary>
        /// <see cref="EventHandler"/> for CameraUpdated event, which sets the internal flag to _cameraUpdated.  Will
        /// be used to force the <see cref="ShadowMap"/> to update.
        /// </summary>
        static void CameraUpdated(object sender, EventArgs e)
        {
#if XBOX360
            _cameraUpdated = true;
#endif
        }    
  
        // 6/5/2009
        ///<summary>
        /// Restores the original values for <see cref="IsVisible"/> and <see cref="GameComponent.Enabled"/> which
        /// user set at beginning of game.
        ///</summary>
        /// <remarks>This is necessary, since these values are temporarily changed when creating the <see cref="ShadowMap"/> class.</remarks>
        public void ResetVisiblityFlags()
        {
            // 1/30/2009 - Hack: Restore back the original values user gave.
            Visible = _originalVisibleValue;
            Enabled = true;
        }

       
        // 10/31/2008
        /// <summary>
        /// Builds the light source Projection/View <see cref="Matrix"/> for the Static <see cref="ShadowMap"/>.
        /// </summary>
        public static void InitStaticShadowLightSettings()
        {
            // Get a Reference to the Terrain Interface
            _terrainShape = (ITerrainShape)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(ITerrainShape));

            // 6/4/2009 - Discovered, if put level below ground, then less self-shadowing issues!
            _lightTarget = CreatePositionWithHeightSkew(TerrainData.MiddleMapX, TerrainData.MiddleMapY, -150.0f); 
            
            // 7/10/2009 - Create Static Light View/Projection, using OrthoGraphic.
            Camera.SetOrthogonalView(TerrainData.MapWidthToScale, TerrainData.MapHeightToScale);           
            var lightPosition = TerrainShape.LightPosition;
            CreateLightViewProjectionMatrix(ref lightPosition, ref _lightTarget, out _lightViewStatic, out _lightProjStatic);
            Camera.SetNormalRTSView(); // Reset view.

        }       

        // 10/19/2008
        /// <summary>
        /// <see cref="EventHandler"/> for the <see cref="IFDTileManager.SceneItemPlaced"/> event, which will update the Static
        /// <see cref="ShadowMap"/> to include the new <see cref="SceneItem"/>.
        /// </summary>
        private static void InterFaceDisplaySceneItemPlaced(object sender, EventArgs e)
        {
            _doPreShadowMapTextures = true;
        }
      

        // 7/17/2009
        /// <summary>
        /// Loads the 'ShadowEffect' shader, and calls the <see cref="InitializeRenderTargets"/> method.
        /// </summary>
        protected override void LoadContent()
        {
            // 4/6/2010: Updated to use 'ContentMiscLoc' global var.
            // ShadowMap Shader Effect
            ShadowMapEffect = Game.Content.Load<Effect>(TemporalWars3DEngine.ContentMiscLoc + @"\Shaders\ShadowEffect");

            // ShadowMap Initializiation
            InitializeRenderTargets();

            base.LoadContent();
        }

        // 12/6/2009
        /// <summary>
        /// Initalizes the <see cref="ShadowMap"/> <see cref="RenderTarget2D"/>, using the
        /// internal values <see cref="_shadowMapWidth"/> and <see cref="_shadowMapHeight"/>.  Should be called anytime
        /// the <see cref="ShadowQuality"/> Enum setting is updated.
        /// </summary>
        internal static void InitializeRenderTargets()
        {
            var graphicsDevice = TemporalWars3DEngine.GameInstance.GraphicsDevice;

            {
                // 8/8/2008 - Updated the Surfaceformat from Single to RG32. 
                // 8/21/2008 - Updated the Surfaceformat from RG32 to HalfVector2, which is compatible with XBOX.   
                if (_shadowRt != null) _shadowRt.Dispose();
                if (_shadowTerrainRt != null) _shadowTerrainRt.Dispose();

                _shadowRt = CreateRenderTarget(graphicsDevice, 1, ShadowMapSurfaceFormat); // SurfaceFormat.Single               
                // XNA 4.0 Updates - No more DepthStencilBuffer. 
                //_shadowDb = CreateDepthStencil(_shadowRt, DepthFormat.Depth24Stencil8Single); // DepthFormat.Depth24Stencil8Single or Depth24               
                _shadowTerrainRt = CreateRenderTarget(graphicsDevice, 1, ShadowMapSurfaceFormat);

            }

            // 4/3/2009 - Set HalfPixel Alignment
            var tmpHalfPixel = Vector2.Zero;
            tmpHalfPixel.X = 1.0f / _shadowMapWidth;
            tmpHalfPixel.Y = 1.0f / _shadowMapHeight;
            _halfPixel = tmpHalfPixel;
            ShadowMapEffect.Parameters["xHalfPixel"].SetValue(_halfPixel);

        }

        /// <summary>
        /// Updates the <see cref="ShadowMap"/>, by continually creating the light source's view/projection <see cref="Matrix"/>,
        /// and then applying these updates to the <see cref="TerrainShape"/>.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public override void Update(GameTime gameTime)
        {
            // If Not Visible, then no Update required.
            if (!Visible)
                return;

            // 5/29/2012 - Enter Pause check here.
            if (TemporalWars3DEngine.GamePaused)
                return;

            // 9/10/2008; 1/6/2009: Updated to make sure PreShadow Maps created successfully!
            // Create Pre-ShadowMap Textures; the Terrain & Static Items.
            // Usually done one Time, and when Static items change.
            if (_doPreShadowMapTextures)
            {
                // 12/12/2009 - Init the Proj/View matrices for the Static map.
                InitStaticShadowLightSettings();

                // 7/10/2009 - Create STATIC ShadowMap of ScenaryItems.
                if (CreateStaticShadowMap(gameTime))
                    _doPreShadowMapTextures = false;

                // 12/12/2009 - Set to update Static ShadowMap.
                _requiresSetShadowMapSettings = true;
            }

            // 7/5/2009 - Calculate LightViewProjection every frame!  This keeps the OrhtoProjection in align
            //            with the camera frustum.
            var lightPosition = TerrainShape.LightPosition;
            CreateLightViewProjectionMatrix(ref lightPosition, ref _lightTarget, out _lightView, out _lightProj);

            // 12/12/2009 - Set the updated Dynamic ShadowMap texture.
            _terrainShape.SetDynamicShadowMap(_shadowMapDynamicItemsTexture);

            // 7/5/2009 - Update in Terrain Effect
            _terrainShape.SetShadowMapSettings(isVisible, ref _lightView, ref _lightProj, ref _lightViewStatic, ref _lightProjStatic);

            // 6/11/2010 - Moved from Draw call to here, to eliminate the flashing purple and stalls.
            CreateShadowMap(Game.GraphicsDevice, gameTime);
            

            base.Update(gameTime);
        }

        // 8/27/2008: Updated to optimize memory.     
        /// <summary>
        /// Creates the <see cref="ShadowMap"/> for the dynamic items, while checking
        /// if <see cref="_doPreShadowMapTextures"/> or <see cref="_requiresSetShadowMapSettings"/> need
        /// attention.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public override void Draw(GameTime gameTime)
        {
#if DEBUG
            StopWatchTimers.StartStopWatchInstance(StopWatchName.GameDrawLoop_Main_ShadowDraw);//"ShadowDraw"
#endif
            
            // 1/6/2009 - If True, then ShadowMap needs to keep trying to set effect Settings.
            if (_requiresSetShadowMapSettings)
            {
                // Try again to Set into Effect
                if (_terrainShape != null)
                {
                    _terrainShape.SetShadowMapSettings(Visible, ref _lightView, ref _lightProj, ref _lightViewStatic, ref _lightProjStatic);

                    // 12/12/2009 - Set the Static ShadowMap
                    _terrainShape.SetStaticShadowMap(_shadowMapTerrainTexture);

                    // 12/12/2009 - Set ShadowType
                    _terrainShape.SetShadowMapType(_useShadowType);
                    // 12/12/2009 - Set ShadowMap Darkness level
                    _terrainShape.SetShadowMapDarkness(_shadowMapDarkness);
                    // 12/12/2009 - Set HalfPixel
                    _terrainShape.SetShadowMapHalfPixel(ref _halfPixel);

                    _requiresSetShadowMapSettings = false;
                }
                else
                    _requiresSetShadowMapSettings = true;
            }


            // 6/11/2010 - Moved to Update method, to eliminate the flashing purple and stalls.
            // 7/16/2009 - Tested in Update method; is slower by 1.5 - 2.5 ms per update!
            //CreateShadowMap(Game.GraphicsDevice, gameTime);
            
#if DEBUG && !XBOX360
            if (_DebugValues)
            {
                // DEBUG: Write data to screen
                {
                    // lightPosition Attributes      
                    var lightPos = TerrainShape.LightPosition;
// ReSharper disable RedundantToStringCall
                    _screenText1.DrawText = String.Format("Light Pos: {0},{1},{2}", lightPos.X.ToString(), lightPos.Y.ToString(), lightPos.Z.ToString()); // 8/25/2009: ToString() needed, otherwise Boxing occurs.
                    // _lightTarget Attributes                   
                    _screenText2.DrawText = String.Format("Light Tar: {0},{1},{2}", _lightTarget.X.ToString(), _lightTarget.Y.ToString(), _lightTarget.Z.ToString());
                    _screenText3.DrawText = String.Format("DepthBias: {0}", _shadowMapDepthBias.ToString());
// ReSharper restore RedundantToStringCall
                    // DEBUG: Show Value of DebugIsFor ENUM                    
                    _screenText4.DrawText = _debugIsForString; // 6/1/2010 - Optimized by using the new '_debugIsForString'.

                   
                }

                // 4/2/2009 - Update GameViewPort Texture, for debug purposes.           
                //_gameViewPortItem.Texture = _shadowMapDynamicItemsTexture;
                _gameViewPortItem.Texture = _shadowMapTerrainTexture;
                GameViewPort.UpdateGameViewPortItem(ref _gameViewPortItem);
            } 
#endif
           
            //base.Draw(gameTime);

#if DEBUG
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.GameDrawLoop_Main_ShadowDraw);//"ShadowDraw"
#endif
        }        
       

        // 6/3/2008 - 
        ///<summary>
        /// Draws the <see cref="TWEngine.Terrain"/> using the <see cref="ShadowMap"/> shader <see cref="Effect"/>, which essentially populates the
        /// <see cref="ShadowMap"/> texture with the depth information and is then passed into the normal Draw call later on.
        ///</summary>
        ///<param name="lightPos"><see cref="Vector3"/> as light position</param>
        ///<param name="lightView"><see cref="Matrix"/> as light view</param>
        ///<param name="lightProj"><see cref="Matrix"/> as light projection</param>
        public void DrawForShadowMap(ref Vector3 lightPos, ref Matrix lightView, ref Matrix lightProj)
        {
            // Actual code implemented in the IShadowItem.
            return;
        }       

#if XBOX360
        // 7/24/2009 - Create SM every other frame.
        //static bool _drawThisFrame;
#endif

        // 4/28/2009: Updated to be STATIC.
        // 8/8/2008 - Added the check 'InCameraFrustrum', before drawing Model for Shadowmap.
        /// <summary>
        /// This creates the dynamic shadow map, calling the <see cref="InstancedItem.DrawForShadowMap_DynamicItems"/>.
        /// </summary>   
        /// <param name="graphicsDevice"><see cref="GraphicsDevice"/> instance</param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>           
        private static void CreateShadowMap(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            if (_terrainShape == null) return;

#if XBOX360
            //_drawThisFrame = !_drawThisFrame;
            //if (!_drawThisFrame && !_cameraUpdated) return;
#endif

            StopWatchTimers.StartStopWatchInstance(StopWatchName.CreateShadowMap);//"CreateShadowMap"

#if XBOX360
            // 7/24/2009 - reset flag.
            //_cameraUpdated = false;
#endif
            // XNA 4.0 Updates; Index no longer needed.
            // Set Render Target for shadow map           
            //graphicsDevice.SetRenderTarget(0, _shadowRt);
            graphicsDevice.SetRenderTarget(_shadowRt);

            // XNA 4.0 Updates; RenderState gone; now use BlendState or DepthStencilState.
            // Set the depth buffer function that best fits our stencil type
            // and projection (a reverse projection would use GreaterEqual)
            //graphicsDevice.RenderState.DepthBufferFunction = CompareFunction.LessEqual;  // was LessEqual
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            

            // XNA 4.0 Updates; 'DepthStencilBuffer' is gone!
            // Cache the current depth buffer
            //var old = graphicsDevice.DepthStencilBuffer;
            // Set our custom depth buffer
            //graphicsDevice.DepthStencilBuffer = _shadowDb; 

            // Clear GraphicsDevice.           
            graphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);

            //InstancedItem.DrawForShadowMap_AllItems(gameTime, ref _lightView, ref _lightProj, false);
            InstancedItem.DrawForShadowMap_DynamicItems(gameTime, ref _lightView, ref _lightProj);

            // XNA 4.0 Updates; Index no longer needed.
            // Set render target back to the back buffer
            //graphicsDevice.SetRenderTarget(0, null);
            graphicsDevice.SetRenderTarget(null);

            // XNA 4.0 Updates; 'DepthStencilBuffer' is gone!
            // Reset the depth buffer
            //graphicsDevice.DepthStencilBuffer = old;

            // 3/22/2011 - XNA 4.0 Updates
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            // XNA 4.0 Updates; GetTexture is gone!
            // Set the shadow map as a texture            
            //_shadowMapDynamicItemsTexture = _shadowRt.GetTexture();
            _shadowMapDynamicItemsTexture = _shadowRt;
            

            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.CreateShadowMap);//"CreateShadowMap"
                        
        }

        /// <summary>
        /// This creates the static <see cref="ShadowMap"/> of the entire <see cref="TWEngine.Terrain"/>.  
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private static bool CreateStaticShadowMap(GameTime gameTime)
        {
           
            var graphicsDevice = TemporalWars3DEngine.GameInstance.GraphicsDevice;
            if (_terrainShape == null) return false;

            // XNA 4.0 Updates; Index no longer needed.
            // Set Render Target for shadow map           
            //graphicsDevice.SetRenderTarget(0, _shadowTerrainRt);
            graphicsDevice.SetRenderTarget(_shadowTerrainRt);

            // XNA 4.0 Updates; RenderState gone; now use BlendState or DepthStencilState.
            // Set the depth buffer function that best fits our stencil type
            // and projection (a reverse projection would use GreaterEqual)
            //graphicsDevice.RenderState.DepthBufferFunction = CompareFunction.LessEqual;  // was LessEqual
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            // XNA 4.0 Updates; 'DepthStencilBuffer' is gone!
            // Cache the current depth buffer
            //var old = graphicsDevice.DepthStencilBuffer;
            // Set our custom depth buffer
            //graphicsDevice.DepthStencilBuffer = _shadowDb;

            // Clear GraphicsDevice.           
            graphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);

            // Draw Empty Terrain; without any Structs or Units.   
            var lightPos = TerrainShape.LightPosition;
            _terrainShape.DrawForShadowMap(ref lightPos, ref _lightViewStatic, ref _lightProjStatic); // was _lightViewStatic, _lightProjStatic

            // TODO: Why doesn't static map work for all instance items?
            //InstancedItem.DrawForShadowMap_StaticItems(gameTime, ref _lightViewStatic, ref _lightProjStatic);

            // XNA 4.0 Updates; Index no longer needed.
            // Set render target back to the back buffer
            //graphicsDevice.SetRenderTarget(0, null);
            graphicsDevice.SetRenderTarget(null);

            // XNA 4.0 Updates; 'DepthStencilBuffer' is gone!
            // Reset the depth buffer
            //graphicsDevice.DepthStencilBuffer = old;

            // 3/22/2011 - XNA 4.0 Updates
            graphicsDevice.DepthStencilState = DepthStencilState.Default;

            // XNA 4.0 Updates; GetTexture is gone!
            // Set the shadow map as a texture            
            //shadowMapResult = _shadowTerrainRt.GetTexture();
             var tmpData = new float[_shadowTerrainRt.Height * _shadowTerrainRt.Width];
            _shadowTerrainRt.GetData(tmpData);

            if (_shadowMapTerrainTexture == null)
                _shadowMapTerrainTexture = new Texture2D(graphicsDevice, _shadowTerrainRt.Width, _shadowTerrainRt.Height,
                                                         false, SurfaceFormat.Single);

            _shadowMapTerrainTexture.SetData(tmpData);

            //graphicsDevice.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);

            return true;

        }

        // 9/10/2008
//        /// <summary>
//        /// This will create a Shadow Map of the Terrain.  In this call, only the Terrain
//        /// is drawn, without any Structures or Items.
//        /// </summary>
//        private static bool CreateTerrainShadowMap()
//        {
//            GraphicsDevice graphicsDevice = ImageNexusRTSGameEngine.GameInstance.GraphicsDevice;
//
//            // 1/6/2009 - Make sure TerrainShape is not null; this occurs when the game first starts
//            if (_terrainShape == null)
//                return false;
//
//            // Set the depth buffer function that best fits our stencil type
//            // and projection (a reverse projection would use GreaterEqual)
//            graphicsDevice.RenderState.DepthBufferFunction = CompareFunction.LessEqual;
//           
//            // Set Render Target for shadow map
//            graphicsDevice.SetRenderTarget(0, _shadowTerrainRt);
//            // Cache the current depth buffer
//            DepthStencilBuffer old = graphicsDevice.DepthStencilBuffer;
//            // Set our custom depth buffer
//            graphicsDevice.DepthStencilBuffer = _shadowDb;
//
//            // Clear GraphicsDevice.           
//            graphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0);
//
//            // Draw Empty Terrain; without any Structs or Units.   
//            Vector3 lightPos = TerrainShape.LightPosition;
//            _terrainShape.DrawForShadowMap(ref lightPos, ref _lightViewStatic, ref _lightProjStatic);
//           
//
//            // Set render target back to the back buffer
//            graphicsDevice.SetRenderTarget(0, null);
//            // Reset the depth buffer
//            graphicsDevice.DepthStencilBuffer = old;
//
//            // Set the shadow map as a texture
//            _shadowMapTerrainTexture = _shadowTerrainRt.GetTexture();   
//          
//            // DEBUG            
//            /*Microsoft.Xna.Framework.Graphics.PackedVector.HalfSingle[] pixels = new Microsoft.Xna.Framework.Graphics.PackedVector.HalfSingle[_shadowMapTerrainTexture.Width * _shadowMapTerrainTexture.Height];
//            _shadowMapTerrainTexture.GetData<Microsoft.Xna.Framework.Graphics.PackedVector.HalfSingle>(pixels);
//
//            // convert to float
//            float[] pixels2 = new float[pixels.Length];
//            for (int x = 0; x < pixels.Length; x++)
//            {
//                pixels2[x] = pixels[x].ToSingle();
//            }*/
//
//            return true;
//        }

       
        // 6/3/2008 -
        /// <summary>
        ///  Helper Fn: Returns a position at given x/y, while looking up the height and
        ///             adding the given value to height.
        /// </summary>
        /// <param name="x">X value</param>
        /// <param name="y">Y value</param>
        /// <param name="heightSkew">Height skew to apply</param>
        /// <returns><see cref="Vector3"/> as position</returns>
        private static Vector3 CreatePositionWithHeightSkew(float x, float y, float heightSkew)
        {
            var heightAtPos = TerrainData.GetTerrainHeight(x, y);
            heightAtPos += heightSkew;

            return new Vector3(x, heightAtPos, y);
        }
      
        /// <summary>
        /// Creates the light source View/Projection <see cref="Matrix"/> from the perspective of the 
        /// light using the <see cref="Camera"/> bounding frustum to determine what is visible 
        /// in the scene.
        /// </summary>
        /// <param name="lightPosition"><see cref="Vector3"/> as light position</param>
        /// <param name="lightTarget"><see cref="Vector3"/> as light target</param>
        /// <param name="lightView">(OUT) light view <see cref="Matrix"/> </param>
        /// <param name="lightProj">(OUT) light projection <see cref="Matrix"/></param>
        private static void CreateLightViewProjectionMatrix(ref Vector3 lightPosition, ref Vector3 lightTarget, 
                                                            out Matrix lightView, out Matrix lightProj)
        {

            // 8/7/2009: Optmized to use the Vector3 methods, which are faster on XBOX!
            // Light direction
            //Vector3 lightDir = new Vector3(-0.3333333f, 0.6666667f, 0.6666667f);
            //_lightDirection = lightTarget - lightPosition;
            Vector3.Subtract(ref lightTarget, ref lightPosition, out _lightDirection);
            if(!_lightDirection.Equals(Vector3.Zero)) _lightDirection.Normalize(); // 8/7/2009: Eliminate any NaN errors, by not normalizing zero values.

            // 8/7/2009: Optmized to use the Vector3 methods, which are faster on XBOX!
            // Matrix with that will rotate in points the direction of the light
            var tmpZero = Vector3.Zero;
            var tmpUp = Vector3.Up;
            Matrix lightRotation; // = Matrix.CreateLookAt(Vector3.Zero, _lightDirection, Vector3.Up);
            Matrix.CreateLookAt(ref tmpZero, ref _lightDirection, ref tmpUp, out lightRotation);

            // 8/7/2009: Updated to use the version of 'GetCorners', which does not create the array every Time!
            // Get the corners of the frustum
            var frustumCorners = FrustumCorners; // 4/30/2010 - Cache
            Camera.CameraFrustum.GetCorners(frustumCorners);

            // Transform the positions of the corners into the direction of the light
            var length = frustumCorners.Length; // 4/30/2010
            for (var i = 0; i < length; i++)
            {
                // 8/7/2009: Optmized to use the Vector3 methods, which are faster on XBOX!
                Vector3.Transform(ref frustumCorners[i], ref lightRotation, out frustumCorners[i]);
            }

            // 8/25/2009: Updated to use new Overload version of the 'CreateFromPoints', which
            //            iterates the Points array with the For, rather than ForEach as XNA does!
            // Find the smallest box around the points
            var lightBox = new BoundingBox();
            lightBox = lightBox.CreateFromPointsV2(frustumCorners);
            

            // 8/7/2009: Optmized to use the Vector3 methods, which are faster on XBOX!
            Vector3 boxSize; // = lightBox.Max - lightBox.Min;
            Vector3.Subtract(ref lightBox.Max, ref lightBox.Min, out boxSize);
            Vector3 halfBoxSize;// = boxSize * 0.5f;
            Vector3.Multiply(ref boxSize, 0.5f, out halfBoxSize);

            // 8/7/2009: Optmized to use the Vector3 methods, which are faster on XBOX!
            // The Position of the light should be in the center of the back
            // pannel of the box.             
            Vector3 adjustedLightPosition; // = lightBox.Min + halfBoxSize;
            Vector3.Add(ref lightBox.Min, ref halfBoxSize, out adjustedLightPosition);
            adjustedLightPosition.Z = lightBox.Min.Z;

            // 8/7/2009: Optmized to use the Vector3 methods, which are faster on XBOX!
            // We need the Position back in World coordinates so we Transform 
            // the light Position by the inverse of the lights rotation
            Matrix invertedLightPosition;
            Matrix.Invert(ref lightRotation, out invertedLightPosition);
            //adjustedLightPosition = Vector3.Transform(adjustedLightPosition, Matrix.Invert(lightRotation));
            Vector3.Transform(ref adjustedLightPosition, ref invertedLightPosition, out adjustedLightPosition);

            // 8/7/2009: Optmized to use the Vector3 methods, which are faster on XBOX!
            // Create the view matrix for the light
            Vector3 cameraTarget;
            Vector3.Add(ref adjustedLightPosition, ref _lightDirection, out cameraTarget);
            //lightView = Matrix.CreateLookAt(adjustedLightPosition, adjustedLightPosition + _lightDirection, Vector3.Up);
            Matrix.CreateLookAt(ref adjustedLightPosition, ref cameraTarget, ref tmpUp, out lightView);

            // 8/7/2009: Optmized to use the Vector3 methods, which are faster on XBOX!
            // Create the projection matrix for the light
            // The projection is orthographic since we are using a directional light
            //lightProj = Matrix.CreateOrthographic(boxSize.X, boxSize.Y, -boxSize.Z, boxSize.Z);
            Matrix.CreateOrthographic(boxSize.X, boxSize.Y, -boxSize.Z, boxSize.Z, out lightProj);
            //_lightProj = Matrix.CreateOrthographic(2500, 2500, 0, 5000); // was 8000,5000,0,10000
           
        }

       
        // 6/5/2009
        /// <summary>
        /// Helper method which takes the current KeyState, and the <see cref="ShadowMap"/> offset texels for a specific Quad.  It
        /// then will add or subtract 0.5f texel to the specified index.
        /// </summary>
        /// <param name="inputState"><see cref="InputState"/> instance</param>
        /// <param name="offsetSm"><see cref="Vector2"/> offset</param>
        /// <param name="offsetSmBy"><see cref="Vector2"/> offset by</param>
        internal static void UpdateShadowTexelOffsets(InputState inputState, ref Vector2 offsetSm, ref Vector2 offsetSmBy)
        {
            var graphicsDevice = TemporalWars3DEngine.GameInstance.GraphicsDevice;

            // DEBUG: Test adjusting ShadowMap Offset Texels
            if (inputState.IsKeyPress(Keys.NumPad8))
            {
                offsetSmBy.Y += 0.5f;
                offsetSm.X = offsetSmBy.X / graphicsDevice.PresentationParameters.BackBufferWidth;
                offsetSm.Y = offsetSmBy.Y / graphicsDevice.PresentationParameters.BackBufferHeight;
            }

            if (inputState.IsKeyPress(Keys.NumPad2))
            {
                offsetSmBy.Y -= 0.5f;
                offsetSm.X = offsetSmBy.X / graphicsDevice.PresentationParameters.BackBufferWidth;
                offsetSm.Y = offsetSmBy.Y / graphicsDevice.PresentationParameters.BackBufferHeight;
            }

            if (inputState.IsKeyPress(Keys.NumPad4))
            {
                offsetSmBy.X -= 0.5f;
                offsetSm.X = offsetSmBy.X / graphicsDevice.PresentationParameters.BackBufferWidth;
                offsetSm.Y = offsetSmBy.Y / graphicsDevice.PresentationParameters.BackBufferHeight;
            }

            if (inputState.IsKeyPress(Keys.NumPad6))
            {
                offsetSmBy.X += 0.5f;
                offsetSm.X = offsetSmBy.X / graphicsDevice.PresentationParameters.BackBufferWidth;
                offsetSm.Y = offsetSmBy.Y / graphicsDevice.PresentationParameters.BackBufferHeight;
            }
        }

        // 6/3/2008
        /// <summary>
        /// Creates a <see cref="RenderTarget2D"/> with given information.
        /// </summary>
        /// <param name="device"><see cref="GraphicsDevice"/> instance</param>
        /// <param name="numberLevels">Number of mip-map levels to use</param>
        /// <param name="surface"><see cref="SurfaceFormat"/> Enum to use</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="device"/> given is null.</exception>
        /// <returns>RenderTarget2D</returns>
        private static RenderTarget2D CreateRenderTarget(GraphicsDevice device, int numberLevels,
           SurfaceFormat surface)
        {

            // 4/30/2010 - Check if device is null
            if (device == null)
                throw new ArgumentNullException("device", @"The given graphics device value is null, which is not allowed in this method call!");

            // XNA 4.0 updates
            /*if (!GraphicsAdapter.DefaultAdapter.CheckDeviceFormat(DeviceType.Hardware,
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Format, TextureUsage.None,
                QueryUsages.None, ResourceType.RenderTarget, surface))
            {
                // Fall back to current display format
                surface = device.DisplayMode.Format;
            }*/

            // XNA 4.0 updates
            /*return new RenderTarget2D(device,
                _shadowMapWidth, // Device.Viewport.Width
                _shadowMapHeight, // Device.Viewport.Height
                numberLevels, surface,
                device.PresentationParameters.MultiSampleType,
                device.PresentationParameters.MultiSampleQuality); // was shadowMapWidthHeight*/
            return new RenderTarget2D(device,
                                      _shadowMapWidth, // Device.Viewport.Width
                                      _shadowMapHeight, // Device.Viewport.Height
                                      true, surface, DepthFormat.Depth24Stencil8);

        }

        // XNA 4.0 Updates; 'DepthStencilBuffer' no longer used.
        // 6/3/2008
        /*/// <summary>
        /// Creates <see cref="DepthStencilBuffer"/> using given <paramref name="target"/> parameter.
        /// </summary>
        /// <param name="target"><see cref="RenderTarget2D"/> instance to use</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="target"/> is null.</exception>
        /// <returns><see cref="DepthStencilBuffer"/> instance</returns>
        private static DepthStencilBuffer CreateDepthStencil(RenderTarget target)
        {
            // 4/30/2010 - Check if RenderTarget given null
            if (target == null)
                throw new ArgumentNullException("target", @"The given RenderTarget is null, which is not allowed for this method call!");
           
            return new DepthStencilBuffer(target.GraphicsDevice, _shadowMapWidth,
                _shadowMapHeight, target.GraphicsDevice.DepthStencilBuffer.Format,
                target.MultiSampleType, target.MultiSampleQuality);
        }*/

        // XNA 4.0 Updates; 'DepthStencilBuffer' no longer used.
        // 6/3/2008
        /*/// <summary>
        /// Creates <see cref="DepthStencilBuffer"/> using given <paramref name="target"/> parameter.
        /// </summary>
        /// <param name="target"><see cref="RenderTarget2D"/> instance to use</param>
        /// <param name="depth"><see cref="DepthFormat"/> Enum to use</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="target"/> is null.</exception>
        /// <returns><see cref="DepthStencilBuffer"/> instance</returns>
        private static DepthStencilBuffer CreateDepthStencil(RenderTarget target, DepthFormat depth)
        {
            // 4/30/2010 - Check if RenderTarget given null
            if (target == null)
                throw new ArgumentNullException("target", @"The given RenderTarget is null, which is not allowed for this method call!");

            if (GraphicsAdapter.DefaultAdapter.CheckDepthStencilMatch(DeviceType.Hardware,
               GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Format, target.Format,
                depth))
            {
                return new DepthStencilBuffer(target.GraphicsDevice, _shadowMapWidth,
                    _shadowMapHeight, depth, target.MultiSampleType, target.MultiSampleQuality); 
            }

            return CreateDepthStencil(target);
        }*/

        // 6/5/2009
        /// <summary>
        /// Gets all the internal <see cref="ShadowMap"/> attributes, and returns to
        /// caller in the <see cref="ShadowMapData"/> struct.
        /// </summary>
        /// <param name="shadowMapData">(OUT) Returns <see cref="ShadowMapData"/> structure</param>
        public static void GetShadowMapDataAttributes(out ShadowMapData shadowMapData)
        {
            shadowMapData = new ShadowMapData {shadowMapDepthBias = _shadowMapDepthBias, IsVisible = IsVisibleS};
        }

        // 6/5/2009
        /// <summary>
        /// Sets all the internal <see cref="ShadowMap"/> attributes using the given
        /// <see cref="ShadowMapData"/>  struct.
        /// </summary>
        /// <param name="shadowMapData"><see cref="ShadowMapData"/> structure to update</param>
        public static void SetShadowMapDataAttributes(ref ShadowMapData shadowMapData)
        {
            // 1/21/2011 - Retrieve ShadowMap interface
            var shadowMap = (IShadowMap) TemporalWars3DEngine.GameInstance.Services.GetService(typeof (IShadowMap));
            if (shadowMap != null)
            {
                // 1/21/2011 - Save IsVisible flag
                shadowMap.IsVisible = shadowMapData.IsVisible;
            }
            
            _shadowMapDepthBias = shadowMapData.shadowMapDepthBias;            

            // Reinit View Matricies
            InitStaticShadowLightSettings();
           
        }
       

        // 4/5/2009 - Dispose of resources
        /// <summary>
        /// Releases the unmanaged resources used by the DrawableGameComponent and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {               

                // Dispose of Resources.                
                if (ShadowMapEffect != null)
                    ShadowMapEffect.Dispose();
                if (_shadowRt != null)
                    _shadowRt.Dispose();                
                if (_shadowTerrainRt != null)
                    _shadowTerrainRt.Dispose();                
                if (_shadowMapDynamicItemsTexture != null)
                    _shadowMapDynamicItemsTexture.Dispose();                
                if (_shadowMapTerrainTexture != null)
                    _shadowMapTerrainTexture.Dispose();
                             
                _screenText1.Dispose();                

                // Null Interface and class References               
                ShadowMapEffect = null;
                _shadowMapDynamicItemsTexture = null;                
                _shadowMapTerrainTexture = null;
                _shadowRt = null;                           
                _shadowTerrainRt = null;
               
                _terrainShape = null;       

            }

            base.Dispose(disposing);
        }
    }
}