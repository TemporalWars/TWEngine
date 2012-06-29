#region File Description
//-----------------------------------------------------------------------------
// TemporalWars3DEngine.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
#if !XBOX360
using System.Diagnostics;
using System.Windows.Forms;
using TWEngine.Console;
using TWEngine.Utilities;
using fbDeprofiler;
using MessageBoxIcon = System.Windows.Forms.MessageBoxIcon;
#endif
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using AStarInterfaces.AStarAlgorithm;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using ParallelTasksComponent;
using PerfTimersComponent.Timers;
using PerfTimersComponent.Timers.Enums;
using ScreenTextDisplayer.ScreenText;
using SimpleQuadDrawer;
using TWEngine.AI;
using TWEngine.Audio;
using TWEngine.BeginGame.Enums;
using TWEngine.Common;
using TWEngine.GameScreens;
using TWEngine.GameScreens.Generic;
using TWEngine.HandleGameInput;
using TWEngine.InstancedModels;
using TWEngine.Interfaces;
using TWEngine.ItemTypeAttributes;
using TWEngine.IFDTiles;
using TWEngine.GameCamera;
using TWEngine.Particles;
using Particles3DComponentLibrary;
using TWEngine.Players;
using TWEngine.PostProcessEffects.BloomEffect.Enums;
using TWEngine.SceneItems;
using TWEngine.ScreenManagerC;
using TWEngine.Shadows.Enums;
using TWEngine.SkyDomes;
using TWEngine.Terrain;
using TWEngine.Shadows;
using TWEngine.Explosions;
using TWEngine.Networking;
using TWEngine.MemoryPool;
using TWEngine.Terrain.Enums;
using TWEngine.InstancedModels.Enums;
using TWEngine.TerrainTools;
using TWEngine.Utilities;
using TWEngine.Viewports;
using TWEngine.ForceBehaviors;
using TWEngine.Water;
using BenScharbachTWScriptingInterfaces;
using Microsoft.Xna.Framework.Content;
using Cursor = TWEngine.Common.Cursor;


namespace TWEngine
{
    // 9/11/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine"/> namespace contains the classes
    /// which make up the entire Temporal Wars 3D Framework.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class TemporalWars3DEngine : Game, IFOWEngineRef, IMinimapEngineRef, IStatusBarEngineRef
    {
        // 4/6/2010 - Set Content locations from Resource files, using proper resource file
        //            depending on XBOX or PC build.
// ReSharper disable InconsistentNaming
#if XBOX360
        private static readonly string _contentMapsLoc = Resource360.ContentMaps;
        private static readonly string _contentAudioLoc = Resource360.ContentAudio;
        private static readonly string _contentTexturesLoc = Resource360.ContentTextures;
        private static readonly string _contentMiscLoc = Resource360.ContentMisc;
        // 3/23/2011 - XNA 4.0 Updates
        /*private static readonly string _contentPlayableLoc = Resource360.ContentPlayable;
        private static readonly string _contentScenaryLoc = Resource360.ContentScenary;
        private static readonly string _contentGroundTexturesLoc = Resource360.ContentGroundTextures;*/
#else
        private static readonly string _contentMapsLoc = Resources.ContentMaps;
        private static readonly string _contentAudioLoc = Resources.ContentAudio;
        private static readonly string _contentTexturesLoc = Resources.ContentTextures;
        private static readonly string _contentMiscLoc = Resources.ContentMisc;
#endif
        // 4/8/2010 - Store Visual Studio location on the users computer.
        // 4/9/2010 - Stores FULL path Project location within the solution.

// ReSharper restore InconsistentNaming

        // 6/28/2012 - Used to set specific playable items to load, rather than the default set.
        private static readonly List<ItemType> _playableItemTypes = new List<ItemType>();

        ///<summary>
        /// Returns collection of <see cref="Player"/>.
        ///</summary>
        private static readonly Player[] _sPlayers = new Player[_maxAllowablePlayers];

        //
        // 9/25/2009 - Allows inherting classes, to turn on/off internal components
        //
        private bool _usePhysX;

        // 1/15/2010 - GameLevelManager 
        protected IGameLevelManager GameLevelManager;

        // 8/13/2008 Ben: ScreenManager Class
        private ScreenManager _screenManager;        
    
        // 9/9/2008 - FPS Class
        private FPS _fps;
        // 9/9/2008 - MessageDisplayComponent
        private MessageDisplayComponent _messageDisplayComponent;

        // 11/7/2008 - StopWatchTimers for performance debugging
        private StopWatchTimers _timers;

        // 10/31/2008 - Services
        private Camera _camera;
        private Cursor _cursor;
        private ShadowMap _shadowMap;
        private object _fogOfWar; // LateBind component
        private object _miniMap; // LateBind component
        private object _statusBar; // LateBind component
        private object _aStarInstance; // LateBind component

        private WaterManager _water;
        private SkyDome _skyDome; // 10/27/2009 - Moved from water class.

        private IFDTileManager _ifdTileManager;
        private ParticlesManager _particles;
       
        // 2/21/2009 - NetworkGame Component
        private NetworkGameComponent _networkGameComponent;
        // 5/4/2009 - SoundManager Component
        private AudioManager _soundManager;
        // 6/26/2009 - Add GameViewPort Component
        private GameViewPort _gameViewport; 
        // 6/22/2009 - PhysX Component; ONLY for PC.
#if !XBOX
        //private PhysX.PhysXEngine _physXEngine;
#endif
        // 7/17/2009 - ForceBehaviorManager Component
        private ForceBehaviorsManager _forceBehaviorManager;
        // 7/17/2009 - AIThreadManager Component
        private AIManager _aiThreadManager;
        // 6/6/2012 - TerrainDirectionIconManager Component
        private TerrainDirectionalIconManager _directionIconManager;

        // A* PathNodeStride
// ReSharper disable InconsistentNaming
        public const int _pathNodeStride = 90; // Spacing between Nodes.
// ReSharper restore InconsistentNaming

        // 8/12/2009 - Max number of players allowed!
// ReSharper disable InconsistentNaming
        internal const int _maxAllowablePlayers = 2;
// ReSharper restore InconsistentNaming


        // these are the size of the output window, ignored
        // on Xbox 360
        private const int PreferredWindowWidth = 1280;
        private const int PreferredWindowHeight = 720; 

        // 7/15/2008 - Add _gameConsole
#if !XBOX360
#endif

        private readonly GraphicsDeviceManager _graphicsDeviceMng;

        private RenderTarget2D _drawBuffer;
        private SpriteBatch _spriteBatch;

        // XNA 4.0 Updates; 'DepthStencilBuffer' is gone!
        //private DepthStencilBuffer _drawDepthBuffer;

        // 1/7/2010 - Get or set to load the default main menu at engine startup.
        private static bool _loadMainMenu = true;

        // 1/19/2011 - Stores the current signed in gamer.
        private static SignedInGamer _signedInGamer;

        // 1/19/2011 - Tracks the number of iterations to do before checking if in TrialMode.
        private static int _trialModeCheckCounter;
        // 2/26/2011
        protected internal static Texture2D IfdTileMiniMapWrapper;
        

        #region Properties 

        // 6/28/2012
        /// <summary>
        /// Used to set specific playable items to load, rather than the default set.
        /// </summary>
        public static List<ItemType> PlayableItemTypes
        {
            get { return _playableItemTypes; }
        }

        // 5/29/2012
        /// <summary>
        /// Gets when the game is paused.
        /// </summary>
        public static bool GamePaused { get; set; }

        // 1/19/2011 - Is Purchased Game?
        public static bool IsPurchasedGame { get; private set; }

        // 1/19/2011 - Is Game Trial over? (Scripting Purposes)
        public static bool IsGameTrialOver { get; set; }

        ///<summary>
        /// Get or set to load the default main menu at engine startup.
        ///</summary>
        public static bool LoadMainMenu
        {
            get { return _loadMainMenu; }
            set { _loadMainMenu = value; }
        }

        // 5/28/2010 - Add 'ThreadPool' attribute, checked in all the 'ParallelTasks' classes.
        ///<summary>
        /// This attribute is checked in all the 'ParallelTasks' classes, which identifies use of
        /// custom <see cref="MyThreadPool"/> or the .Net framework's ThreadPool.
        ///</summary>
        /// <remarks>
        /// Each ParallelTasks class which uses this attribute can be overriden with its own setting.  
        /// </remarks>
        public static bool UseDotNetThreadPool { get; set; }

        // 5/30/2008 - Add ThisPlayer attribute
        ///<summary>
        /// Get or set the <see cref="_sPlayers"/> collection index value, used
        /// to retrieve the current <see cref="Player"/>.
        ///</summary>
        public static int SThisPlayer { get; set; }

        ///<summary>
        /// Get or set the A* path node size.
        ///</summary>
        public static int SPathNodeSize { get; set; }

        ///<summary>
        /// Is current player's computer XNA Live Ready?
        ///</summary>
        public static bool IsXnaLiveReady { get; set; }

        // 10/30/2008
        ///<summary>
        /// Returns reference for <see cref="Game"/>.
        ///</summary>
        public static Game GameInstance { get; private set; }

        // 6/23/2010: Update 'SET' to be public, so TemporalWars can use.
        // 11/3/2009 - Terrain Texture Quality
        ///<summary>
        /// Returns reference for the <see cref="TerrainTextures"/> quality Enum.
        ///</summary>
        public static TerrainTextures TerrainTexturesQuality { get; set; }

        // 6/23/2010: Update 'SET' to be public, so TemporalWars can use.
        ///<summary>
        /// Returns the current game's <see cref="ScreenScale"/>.
        ///</summary>
        public static float ScreenScale { get; set; }

        // 6/23/2010: Update 'SET' to be public, so TemporalWars can use.
        // 4/8/2009 - ScreenResolution
        ///<summary>
        /// Returns reference for the <see cref="ScreenResolution"/> Enum.
        ///</summary>
        public static ScreenResolution ScreenResolution { get; set; }

        // 2/6/2009 - ExplosionManager Component
        ///<summary>
        /// Returns a reference for the <see cref="ExplosionsManager"/>.
        ///</summary>
        public static ExplosionsManager ExplosionManager { get; private set; }

        // 8/21/2009 - KillSceneItem Manager, used to Queue up items which need to die!
        ///<summary>
        /// Returns a reference for the <see cref="KillSceneItemManager"/>.
        ///</summary>
        public static KillSceneItemManager KillSceneItemManager { get; private set; }

        ///<summary>
        /// Get or set the <see cref="GameState"/> Enum.
        ///</summary>
        public static GameState GameState { get; set; }

        ///<summary>
        /// Returns a reference for the <see cref="Settings"/>.
        ///</summary>
        public static Settings Settings { get; private set; }
        

// ReSharper disable UnusedAutoPropertyAccessor.Local
        ///<summary>
        /// Returns what <see cref="PlatformID"/> the game is running on.
        ///</summary>
        public static PlatformID CurrentPlatform { get; private set; }
// ReSharper restore UnusedAutoPropertyAccessor.Local

        // 8/1/2009 - Content Resources for some Assets.
        ///<summary>
        /// Returns reference for the <see cref="ResourceContentManager"/>
        ///</summary>
        public static ResourceContentManager ContentResourceManager { get; private set; }

        ///<summary>
        /// Returns reference for the <see cref="ContentManager"/>.
        ///</summary>
        public new static ContentManager ContentMisc { get; private set; }

        // 10/30/2008 
        /// <summary>
        ///Returns reference for the <see cref="ContentManager"/> for maps.
        /// </summary>
        public static ContentManager ContentMaps { get; private set; }
      

#if XBOX360
        
        /// <summary>
        /// Returns reference for the ground textures.
        /// </summary>
        public static ContentManager ContentGroundTextures { get; private set; }

#else
          // 7/17/2009; 11/2/2009: Updated to ZippedContent version.
        /// <summary>
        /// Returns reference for the <see cref="ZippedContent"/> ground textures.
        /// </summary>
        public static ZippedContent ContentGroundTextures { get; private set; }

#endif

        // 12/31/2009
        /// <summary>
        /// Max allowed players in game engine.
        /// </summary>
        public int MaxAllowablePlayers
        {
            get { return _maxAllowablePlayers; }
        }

        // 1/2/2010 -  Add Non-Static 'Players' for Interface ref.
        IMinimapPlayer[] IMinimapEngineRef.Players
        {
            get { return _sPlayers; }
        }

        // 12/31/2009 - Add Non-Static 'Players' for Interface ref.
        /// <summary>
        /// Collection of <see cref="IFOWPlayer"/> items.
        /// </summary>
        public IFOWPlayer[] Players
        {
            get { return _sPlayers; }
        }

        // 1/3/2010 - Add Non-Static 'Player' for Interface red.
        IStatusBarPlayer[] IStatusBarEngineRef.Players
        {
            get { return _sPlayers; }
        }

        // 1/13/2010 - 
        ///<summary>
        /// Interface reference for <see cref="IAStarGraph"/>.
        ///</summary>
        public static IAStarGraph AStarGraph { get; private set; }

        // 12/31/2009
        /// <summary>
        /// Stores the A* Graph's path node stride, or distance between
        /// a tile node.
        /// </summary>
        public int PathNodeStride
        {
            get { return _pathNodeStride; }
        }

        // 12/31/2009 - Add Non-Static AutoProperty
        /// <summary>
        /// Stores the A* path node size, or number of nodes in
        /// the given graph; for example, 57 is 57x57.
        /// </summary>
        public int PathNodeSize
        {
            get { return SPathNodeSize; }
            set { SPathNodeSize = value; }
        }

        // 12/31/2009 - Add Non-Static 'ThisPlayer' AutoProperty
        /// <summary>
        /// Get or set the <see cref="_sPlayers"/> collection index value, used
        /// to retrieve the current <see cref="Player"/>.
        /// </summary>
        public int ThisPlayer
        {
            get
            {
                return SThisPlayer;
            }
        }
       

#if DEBUG
        // 8/10/2009 - Debug Atts for Fast BuildTimes!
        ///<summary>
        /// Sets all <see cref="SceneItem"/> built in game to have 1 second build times.
        ///</summary>
        /// <remarks>Specifically used for debugging purposes only!</remarks>
        public static bool FastBuildTimes { get; set; }

        // 9/25/2009
        /// <summary>
        /// Should engine initialize the PhysX component?
        /// </summary>
        public bool UsePhysX
        {
            get { return _usePhysX; }
            set { _usePhysX = value; }
        }

#endif

#if !XBOX360
        // 12/6/2009
        /// <summary>
        /// Gives access to the 'GameConsole' component.
        /// </summary>
        public static GameConsole EngineGameConsole { get; private set; }

#endif

        // 4/6/2010
        /// <summary>
        /// Content Maps project folder location.
        /// </summary>
        public static string ContentMapsLoc
        {
            get { return _contentMapsLoc; }
        }

        // 4/6/2010
        /// <summary>
        /// Content Audio project folder location.
        /// </summary>
        public static string ContentAudioLoc
        {
            get { return _contentAudioLoc; }
        }

        // 4/6/2010
        /// <summary>
        /// Content Textures project folder location.
        /// </summary>
        public static string ContentTexturesLoc
        {
            get { return _contentTexturesLoc; }
        }

        // 4/6/2010 - Non-static ref for IFOWEngineRef interface.
        string IFOWEngineRef.ContentTexturesLoc
        {
            get { return ContentTexturesLoc; }
        }

        // 4/6/2010
        /// <summary>
        /// Content Misc project folder location.
        /// </summary>
        public static string ContentMiscLoc
        {
            get { return _contentMiscLoc; }
        }

        // 4/6/2010 - Non-Static ref for IMinimapEngineRef interface.
        string IMinimapEngineRef.ContentMiscLoc
        {
            get { return ContentMiscLoc; }
        }

        // 4/6/2010 - Non-Static ref for IStatusBarEngineRef interface.
        string IStatusBarEngineRef.ContentMiscLoc
        {
            get { return ContentMiscLoc; }
        }

        // 4/8/2010
        /// <summary>
        /// Gets or Sets the current Visual Studio's folder location.
        /// </summary>
        public static string VisualStudioLocation { get; set; }

        // 4/9/2010
        ///<summary>
        /// Gets or Sets the current Visual Studio's project folder FULL path location.
        ///</summary>
        public static string VisualStudioProjectLocation { get; set; }

        // 1/8/2011
        ///<summary>
        /// Gets or Sets the current Paint-Tool textures folder FULL path location.
        ///</summary>
        public static string PaintToolsTexturesDirPath { get; set; }

        // 1/9/2011
        /// <summary>
        /// Gets or Sets the current Item-Tool asset preview pics.
        /// </summary>
        public static string ItemToolsAssetPreviewPics { get; set; }

        // 1/9/2011
        ///<summary>
        /// Gets or Sets the ItemTool's Content locations to search when populating the TreeList.
        ///</summary>
        /// <remarks>
        /// Leave NULL to use the default development path for 'ContentForResources'.
        /// </remarks>
        public static List<string> ContentSearchLocations { get; set; }

/*#if XBOX360
        // 3/23/2011 - XNA 4.0 Updates
        /// <summary>
        /// Content Playable project folder location.
        /// </summary>
        public static string ContentPlayableLoc
        {
            get { return _contentPlayableLoc; }
        }

        // 3/23/2011 - XNA 4.0 Updates
        /// <summary>
        /// Content Scenary project folder location.
        /// </summary>
        public static string ContentScenaryLoc
        {
            get { return _contentScenaryLoc; }
        }

        // 3/23/2011 - XNA 4.0 Updates
        /// <summary>
        /// Content ground textures project folder location.
        /// </summary>
        public static string ContentGroundTexturesLoc
        {
            get { return _contentGroundTexturesLoc; }
        }

#endif*/

        #endregion

        /// <summary>
        /// Static constructor, used to set some common internal states.
        /// </summary>
        static TemporalWars3DEngine()
        {
            GameState = GameState.Started;
            Settings = new Settings();
        }

        /// <summary>
        /// Engine constructor, used to initialize the main GrahicsDeviceManager, set 
        /// common attributes, and create the Content Managers.
        /// </summary>
        public TemporalWars3DEngine()
        {
            //Debugger.Break();

#if WithLicense
#if !XBOX360
            var license = new LicenseHelper();
            license.Required();
#endif
#endif

            // 4/11/2012 - Simple Hack which removes the check for the HiDef, which causes crash in VMWare.
#if DEBUG
#if !XBOX360
            DeProfiler.Run();
#endif
#endif

            // 1/19/2011 - Check if Game Purchased.
            IsPurchasedGame = !Guide.IsTrialMode;

            // 1/22/2011 - Subscribe to the TerrainScreen.Loading event.  Will be used to trigger the LoadGameLevels. (Scripting Purposes)
            TerrainScreen.Loading += TerrainScreen_Loading;
            // 2/7/2011 - Subscribe to the TerrainScreen.Unloading event. (Scripting Purposes)
            TerrainScreen.UnLoading += TerrainScreen_UnLoading;

            // Ben - this setting appears to turn on/off the use of the _drawBuffer RenderState!
            //       I am going to turn it OFF, because the bloomPostProcess Class I added uses it's
            //       own RenderState buffers to apply the effects to the tanks.

            // XNA 4.0 Updates; to use Shader 3.0, must use the new profile 'HiDef'.
            /*_graphicsDeviceMng = new GraphicsDeviceManager(this)
                                     {
                                         SynchronizeWithVerticalRetrace = false, // 6/11/2010 was 'true'
                                         PreferredBackBufferWidth = PreferredWindowWidth,
                                         PreferredBackBufferHeight = PreferredWindowHeight,
                                         MinimumPixelShaderProfile = ShaderProfile.PS_3_0,
                                         MinimumVertexShaderProfile = ShaderProfile.VS_3_0
                                     };*/
            _graphicsDeviceMng = new GraphicsDeviceManager(this)
            {
                SynchronizeWithVerticalRetrace = false, // 6/11/2010 was 'true'
                PreferredBackBufferWidth = PreferredWindowWidth,
                PreferredBackBufferHeight = PreferredWindowHeight,
                GraphicsProfile = GraphicsProfile.HiDef

            };
            _graphicsDeviceMng.PreparingDeviceSettings += PreparingDeviceSettings;

            // 10/30/2008 - Set Game Instance
            GameInstance = this;

            // 1/19/2011 - Add EventHandlers for the Signed-In gamer.
            SignedInGamer.SignedIn += SignedInGamer_SignedIn;
            SignedInGamer.SignedOut += SignedInGamer_SignedOut;

            // 3/16/2009 - Set RenderingType / ScreenResolutions to use - (XBOX can only use NormalRendering)
#if XBOX360
            // XBOX can not use 'DeferredRendering'; only Normal.
            ScreenManager.RenderingType = RenderingType.NormalRendering;
            TerrainScreen.RenderingType = RenderingType.NormalRenderingWithPostProcessEffects; // 5/23/2010 - Set default for 'TerrainScreen'.
            TerrainShape.LightingType = TerrainLightingType.Blinn; // 1/10/2011
            TerrainShape.EnableNormalMap = true; // 1/10/2011
            ScreenResolution = ScreenResolution.Type1280X720;
            TerrainTexturesQuality = TerrainTextures.Tex256X;
            ShadowMap.ShadowQuality = ShadowQuality.Medium; // 12/6/2009
            ShadowMap.UseShadowType = ShadowMap.ShadowType.PercentageCloseFilter_1; // 6/12/2010
            TerrainPerlinClouds.EnableClouds = true; // 1/10/2011 - Enable/Disable PerlinNoise clouds.
#else
            ScreenManager.RenderingType = RenderingType.NormalRendering;
            TerrainScreen.RenderingType = RenderingType.NormalRenderingWithPostProcessEffects; // 5/23/2010 - Set default for 'TerrainScreen'.
            TerrainShape.LightingType = TerrainLightingType.Blinn; // 1/10/2011
            TerrainShape.EnableNormalMap = true; // 1/10/2011
            ScreenResolution = ScreenResolution.Type1280X720;
            TerrainTexturesQuality = TerrainTextures.Tex256X;
            ShadowMap.ShadowQuality = ShadowQuality.High; // 12/6/2009
            ShadowMap.UseShadowType = ShadowMap.ShadowType.PercentageCloseFilter_1; // 6/12/2010
            TerrainPerlinClouds.EnableClouds = true; // 1/10/2011 - Enable/Disable PerlinNoise clouds.
            _usePhysX = false; // 12/7/2009

            // 4/8/2010 - Set VS location
            VisualStudioLocation = GetVisualStudioLocation();
            
            // 4/9/2010 - Set VS Project location
            VisualStudioProjectLocation = GetVisualStudioProjectLocation(this);

            const string visualStudioProjectLocation = @"TemporalWars 3D Engine\\Dev\\";  

            // 1/8/2011 - Set Paint-Tool's Texture folder path.
            PaintToolsTexturesDirPath = Path.GetDirectoryName(VisualStudioLocation + visualStudioProjectLocation + "ContentForResources\\ContentTextures\\high512x\\Terrain\\");

            // 1/9/2011 - Set Item-Tool's Asset preview pics directory path.
            ItemToolsAssetPreviewPics = Path.GetDirectoryName(VisualStudioLocation + visualStudioProjectLocation + "ItemToolPics\\");
            
#endif

            // Set TargetRate (1000/60) = 16.6ms or (1000/30) = 33.3 ms or (1000/20) = 50 ms.
            TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 15);
            // Game should run as fast as possible.
            IsFixedTimeStep = false; // Was false 

            // 5/28/2010 - Set if using 'ThreadPool' in 'ParallelTasks' namespace classes;
            //             otherwise, uses 'MyThreadPool' instead.
            UseDotNetThreadPool = false;

            // 11/17/2009 - Initialize the ContentManagers
            CreateContentManagers(GameInstance);

#if DEBUG
            // 8/10/2009 - Set FastBuildTimes for 1 second builds
            FastBuildTimes = true;
#endif
        }

        // 2/7/2011
        /// <summary>
        /// EventHandler for the TerrainScreen Unloading event.
        /// </summary>
        private void TerrainScreen_UnLoading(object sender, EventArgs e)
        {
            return;
        }

        // 1/22/2011
        /// <summary>
        /// EventHandler for the TerrainScreen Loading event.
        /// </summary>
        private void TerrainScreen_Loading(object sender, EventArgs e)
        {
            // Trigger the LoadGameLevels call (Scripting Purposes)
            LoadGameLevels();
        }

        // 1/22/2011
        /// <summary>
        /// This method is triggered when the TerrainScreen first starts to 
        /// initialize.  Use this method to add game levels to the <see cref="GameLevelManager"/> in
        /// overriding methods.
        /// </summary>
        protected virtual void LoadGameLevels()
        {
            return;
        }

        /// <summary>
        /// Sets the graphics device presentation settings. 
        /// </summary>
        static void PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            
            // 10/21/2008 - DEBUG: Used to get NVidia PerfHud Adapter
            /*foreach (GraphicsAdapter curAdapter in GraphicsAdapter.Adapters)
            {
                if (curAdapter.Description.Contains("PerfHUD"))
                {
                    e.GraphicsDeviceInformation.Adapter = curAdapter;
                    e.GraphicsDeviceInformation.DeviceType = DeviceType.Reference;
                    break;
                }
            }*/           

            //int quality = 0;            
            //var adapter = e.GraphicsDeviceInformation.Adapter;
            //SurfaceFormat format = adapter.CurrentDisplayMode.Format;
            //DisplayMode currentmode = adapter.CurrentDisplayMode;

            // 4/8/2009 - Set ScreenResolution, depending on enum setting.
            switch (ScreenResolution)
            {
                case ScreenResolution.Type1024X768:
                    ScreenScale = 0.75f;
                    e.GraphicsDeviceInformation.PresentationParameters.BackBufferWidth = 1024;
                    e.GraphicsDeviceInformation.PresentationParameters.BackBufferHeight = 768;
                    break;
                case ScreenResolution.Type1280X720:  // 1280x720 which is 720P
                    ScreenScale = 1.0f;
                    e.GraphicsDeviceInformation.PresentationParameters.BackBufferWidth = 1280;
                    e.GraphicsDeviceInformation.PresentationParameters.BackBufferHeight = 720;
                    break;
                case ScreenResolution.Type1280X1024:
                    ScreenScale = 1.0f;
                    e.GraphicsDeviceInformation.PresentationParameters.BackBufferWidth = 1280;
                    e.GraphicsDeviceInformation.PresentationParameters.BackBufferHeight = 1024; // 720 or 1024 for dad's
                    break;
                case ScreenResolution.Type1440X900:
                    ScreenScale = 1.15f;
                    e.GraphicsDeviceInformation.PresentationParameters.BackBufferWidth = 1440;
                    e.GraphicsDeviceInformation.PresentationParameters.BackBufferHeight = 900;
                    break;
                default:
                    break;
            }
            
#if XBOX360
            // TODO: How set this in XNA 4.0?
            /*e.GraphicsDeviceInformation.PresentationParameters.MultiSampleQuality = 1;
            e.GraphicsDeviceInformation.PresentationParameters.MultiSampleType = MultiSampleType.None;            
            e.GraphicsDeviceInformation.PresentationParameters.BackBufferFormat = SurfaceFormat.Bgr32;
            e.GraphicsDeviceInformation.PresentationParameters.AutoDepthStencilFormat = DepthFormat.Depth24Stencil8Single;
            e.GraphicsDeviceInformation.PresentationParameters.EnableAutoDepthStencil = true;*/
            
            return;
#endif



            // Xbox 360 and most PCs support FourSamples/1 (4x) and TwoSamples/1 (2x)
            // antialiasing.
            // Check for 4xAA
            /*int quality;
            if (adapter.CheckDeviceMultiSampleType(DeviceType.Hardware, format,
                false, MultiSampleType.FourSamples, out quality))
            {
                // even if a greater quality is returned, we only want quality 1
                e.GraphicsDeviceInformation.PresentationParameters.MultiSampleQuality = 1;
                e.GraphicsDeviceInformation.PresentationParameters.MultiSampleType =
                    MultiSampleType.FourSamples;
            }
            // Check for 2xAA
            else if (adapter.CheckDeviceMultiSampleType(DeviceType.Hardware, format,
                false, MultiSampleType.TwoSamples, out quality))
            {
                // even if a greater quality is returned, we only want quality 1
                e.GraphicsDeviceInformation.PresentationParameters.MultiSampleQuality = 1;
                e.GraphicsDeviceInformation.PresentationParameters.MultiSampleType =
                    MultiSampleType.TwoSamples;
            }*/

            // TODO: XNA 4.0: Need to figure out how to check for Deviceformat.
            // XNA 4.0 Updates - 'CheckDeviceFormat' seems to be gone?
            // Does this video card support Depth24Stencil8Single for the back buffer?
            /*if (adapter.CheckDeviceFormat(DeviceType.Hardware, adapter.CurrentDisplayMode.Format, TextureUsage.Linear, // was TextureUsage.None.
                                          QueryUsages.None, ResourceType.RenderTarget, DepthFormat.Depth24Stencil8Single))
            {
                // if so, let's use that
                e.GraphicsDeviceInformation.PresentationParameters.AutoDepthStencilFormat = DepthFormat.Depth24Stencil8Single;
                e.GraphicsDeviceInformation.PresentationParameters.EnableAutoDepthStencil = true;
            
            }*/
            
            return;
        }

        /// <summary>
        /// This is where the game Engine creates and initilizes all requires XNA components.
        /// </summary>
        protected override void Initialize()
        {
            // 4/21/2009: Moved Sound Init to top, since takes 8+ seconds on XBOX, and now threaded!            
            //SoundManager.Initialize();  
            _soundManager = new AudioManager(this);
            //Components.Add(_soundManager);

            // 5/13/2009 - Init the RTS Comm Memory Pools!
            PoolManager.InitializeNetworkRTSCommands();

            // 4/21/2009 - Add SpriteBatch to Game.Services
            CreateSpriteBatch();

            // 10/15/2008 - Create ScreenTextManager Component
            Components.Add(new ScreenTextManager(this, ContentMiscLoc + @"\Fonts\Arial8"));

            // 2/18/2010 - Add MyThreadPool

            // 10/27/2009 - Add SkyDome            
            _skyDome = new SkyDome(this);
            Components.Add(_skyDome);
            Services.AddService(typeof(SkyDome), _skyDome);

            // 8/13/2008 - Create ScreenManager Class & MessageDisplay Interface.
            _screenManager = new ScreenManager(this)
                                 {
                                     UseBloom = true,
                                     BloomSetting = BloomType.Saturated, // 5/8/2011 - Updated default to Saturated.
                                     UseSkyBox = true,
                                     UseGlow = true
                                 }; // 1/10/2011 - Updated to set Bloom/SkyBox/Glow here. 
          
            Components.Add(_screenManager);
            Services.AddService(typeof(ScreenManager), _screenManager); // 2/19/2009                   

            // Scenary Attributes
            ScenaryItemTypeAtts.LoadItemTypeAttributes(this);
            // Playable Attributes
            PlayableItemTypeAtts.LoadItemTypeAttributes(this);
            // Message Attributes
            MessageItemTypeAtts.LoadItemTypeAttributes(this);

            // 6/10/2012 - Set to load Playable items
            InstancedItemLoader.DoPreloadPlayableRtsItems = true;

            // 3/14/2009 - Init the SimpleQuadClass
            SimpleQuadDraw.CreateQuadVertices(_graphicsDeviceMng.GraphicsDevice);

#if DEBUG
            // 5/27/2012 - Initialize our renderer
            DebugShapeRenderer.Initialize(_graphicsDeviceMng.GraphicsDevice);
#endif

            // 11/7/2008 - Add StopWatchTimeres Component, used for debuging performance.
            _timers = new StopWatchTimers(this);
            Components.Add(_timers);
            Services.AddService(typeof(StopWatchTimers), _timers);
            _timers.IsVisible = false;

            // game initialization code here
            const float fieldOfView = MathHelper.PiOver4;
            var aspectRatio = GraphicsDevice.PresentationParameters.BackBufferWidth / (float)GraphicsDevice.PresentationParameters.BackBufferHeight;
            const float nearPlane = 0.1f;

            // 6/11/2010 -  was 10000
            // NOTE: The farPlane range indirectly affects the shadowmap quality!  Very important...
            //       So, when the number is greater, as it was at 10,000 range, the smaller the area used
            //       to store an item's shadow; therefore, the smaller the range, the better the shadow's quality!
            const float farPlane = 2000f; // 2000 
            // Get Camera Interface           
            _camera = new Camera(this, fieldOfView, aspectRatio, nearPlane, farPlane);
            Components.Add(_camera); // 10/22/2009
            Services.AddService(typeof(ICamera), _camera);

            // 3/24/2011 - Add Particles3D Component
            var particles3D = new Particle3DSampleGame(GameInstance);
            Components.Add(particles3D);
            Services.AddService(typeof(Particle3DSampleGame), particles3D);

            // Init Cursor and add to Game Components.
            _cursor = new Cursor(this) {Visible = false};
            Components.Add(_cursor);
            Services.AddService(typeof(ICursor), _cursor);

            // 6/22/2009 - Add PhysX Component; ONLY for PC.
#if !XBOX
           
            /*if (_usePhysX)
            {
                _physXEngine = new PhysX.PhysXEngine(this);
                Components.Add(_physXEngine);
                Services.AddService(typeof (PhysX.PhysXEngine), _physXEngine);
            }*/
#endif

            // Add ShadowMap Interface
            _shadowMap = new ShadowMap(this) {IsVisible = true};
            Components.Add(_shadowMap);
            Services.AddService(typeof(IShadowMap), _shadowMap);

            // 12/31/2009
            var terrainData = new TerrainData();
            Services.AddService(typeof (IFOWTerrainData), terrainData);
            Services.AddService(typeof (IMinimapTerrainData), terrainData); // 1/2/2010
            
           
            // 1/1/2010 - LateBind to FOW assembly, if it exist in the LateBind folders.
            if (LateBindAssembly("RTS_FogOfWarComponentLibrary.dll", "FogOfWar", out _fogOfWar))
            {
                // Add FogOfWar Interface
                //_fogOfWar = new FogOfWar(this) { IsVisible = true };
                Components.Add(_fogOfWar as IGameComponent);
                Services.AddService(typeof(IFogOfWar), _fogOfWar);

                // 1/1/2010 - Set EventHandler for SightMatrices.
                Camera.CameraUpdated += ((IFogOfWar)_fogOfWar).UpdateSightMatrices;
                AStarItem.PathMoveToCompletedG += ((IFogOfWar)_fogOfWar).UpdateSightMatrices;
            }

            // 1/1/2010 - LateBind to Minimap assembly, if it exist in the LateBind folders.
            if (LateBindAssembly("RTS_MinimapComponentLibrary.dll", "Minimap", out _miniMap))
            {
                //_miniMap = new Minimap(this);
                Components.Add(_miniMap as IGameComponent);
                Services.AddService(typeof (IMinimap), _miniMap);

                // 1/2/2010 - Attach EventHandler to AStarItem
                AStarItem.PathMoveToCompletedG += ((IMinimap)_miniMap).UpdateMiniMapPosition_EventHandler;
            }

            // 1/13/2010 - LateBind to Minimap assembly, if it exist in the LateBind folders.
            // Add AStar Component Class; 6/16/2009: Currently, method 'InitAStarEngines', called in LoadHeightData of 'TerrainData' class.
            if (LateBindAssembly("AStarComponentLibrary.dll", "AStarManager", out _aStarInstance))
            {
                //_aStarInstance = new AStarManager(this, SPathNodeSize, _pathNodeStride, false);
                Components.Add(_aStarInstance as IGameComponent);
                Services.AddService(typeof (IAStarManager), _aStarInstance);
                AStarGraph = ((IAStarManager) _aStarInstance).IAStarGraph; 
                AStarGraph.NodeStride = _pathNodeStride;
                AStarGraph.NodeArraySize = SPathNodeSize;
            }

            // Add Water Interface
            _water = new WaterManager(GameInstance) { IsVisible = false };
            Components.Add(_water);
            Services.AddService(typeof(IWaterManager), _water);

            // Add IFDTileManager Interface
            _ifdTileManager = new IFDTileManager(this);
            Components.Add(_ifdTileManager);
            Services.AddService(typeof(IIFDTileManager), _ifdTileManager);
            Services.AddService(typeof (IMinimapInterfaceDisplay), _ifdTileManager); // 1/2/2010

            // Add Particles Component
            _particles = new ParticlesManager(this);
            Components.Add(_particles);
            Services.AddService(typeof(ParticlesManager), _particles);
           
            // 1/3/2010 - LateBind to Minimap assembly, if it exist in the LateBind folders.
            if (LateBindAssembly("RTS_StatusBarComponentLibrary.dll", "StatusBar", out _statusBar))
            {
                //_statusBar = new StatusBar(this);
                Components.Add(_statusBar as IGameComponent);
                Services.AddService(typeof (IStatusBar), _statusBar);
            }

            // 2/6/2009 - Add ExplosionManager Component
            ExplosionManager = new ExplosionsManager(this);
            //Components.Add(ExplosionManager);
            Services.AddService(typeof(ExplosionsManager), ExplosionManager);

            // 8/21/2009 - Add KillSceneItem Manager
            KillSceneItemManager = new KillSceneItemManager(this);

            // 2/21/2009 - Add NetworkGame Component; since no NetworkSession yet, 'NULL' is passed in for now!
            _networkGameComponent = new NetworkGameComponent(this, null);
            Components.Add(_networkGameComponent);
            Services.AddService(typeof(NetworkGameComponent), _networkGameComponent);

            // 6/26/2009 - Add GameViewPort Component
            _gameViewport = new GameViewPort(this);
            Components.Add(_gameViewport);
            Services.AddService(typeof(GameViewPort), _gameViewport);

            // 7/17/2009 - Add ForceBehaviorManager Component
            _forceBehaviorManager = new ForceBehaviorsManager(this);
            Components.Add(_forceBehaviorManager);
            Services.AddService(typeof(ForceBehaviorsManager), _forceBehaviorManager);

            // 7/17/2009 - Add AIThreadManager Component
            _aiThreadManager = new AIManager(this);
            Components.Add(_aiThreadManager);
            Services.AddService(typeof(AIManager), _aiThreadManager);
            
            // 6/18/2012 - GameLevelManager is created by the 'Indie' game which consumes this engine.
            // 1/15/2010 - Add GameLevelManager Component
            //GameLevelManager = new GameLevelManager(GameInstance);
            if (GameLevelManager != null)
            {
                Components.Add(GameLevelManager);
                Services.AddService(typeof(IGameLevelManager), GameLevelManager);
            }

            // 6/6/2012 - Add TerrainDirectionalIconManager Component
            _directionIconManager = new TerrainDirectionalIconManager(GameInstance);
            Components.Add(_directionIconManager);
            Services.AddService(typeof(TerrainDirectionalIconManager), _directionIconManager);

            //Uncomment this line to force a save of the default Settings file. Useful when you had added things to Settings.cs
            //NOTE in VS this will go in DEBUG or RELEASE - need to copy up to main project
            //Settings.Save("Settings.xml");

            // Set this to true to make the mouse _cursor visible.
            // Use the default (false) if you are drawing your own
            // _cursor or don't want a _cursor.
            //this.IsMouseVisible = true;          

            CurrentPlatform = Environment.OSVersion.Platform;
            
            Window.Title = Settings.WindowTitle;

            base.Initialize();

            // 6/28/2012 - Check if overriding game gave its own subset of PlayableItems to load
            if (_playableItemTypes.Count != 0)
            {
                // Start Thread for Pre-Loading Some PlayableItems, for example SciFi-Tanks.           
                InstancedItemLoader.PreLoadSomeInstanceItems(this, _playableItemTypes);
            }
            else
            {
                // Start Thread for Pre-Loading Some PlayableItems, for example SciFi-Tanks.           
                InstancedItemLoader.PreLoadSomeInstanceItems(this);
            }

            // 1/25/2010 - Moved here to be after Playable items load.
            // 8/6/2009 - Start Thread for Pre-Loading; when done, it will automatically start the InstancedItem thread.            
            IFDTileTextureLoader.PreLoadIFDTileTextures();

            // 1/7/2010 - Load default MainMenu?
            if (LoadMainMenu)
            {
                // 4/22/2009 - Add the first screens; must come after the base.Init, otherwise,
                //             the MainMenuScreen will think XNA Live is not availble!
                _screenManager.AddScreen(new BackgroundScreen(), true);
                _screenManager.AddScreen(new MainMenuScreen((string) null), true);
            }
            /*else
            {
                // 1/7/2011
                // Load Game using given map name, and GamerInfo data.
                LoadingScreen.Load(_screenManager, true, new TerrainScreen("", new GamerInfo()
                                                                                   {
                                                                                       ColorName = "FireBrick Red",
                                                                                       PlayerColor = Color.Firebrick,
                                                                                       PlayerLocation = 1,
                                                                                       PlayerSide = 1
                                                                                   }));

            }*/

            //ToggleFullScreen();           

        }

        // 6/15/2010
        ///<summary>
        /// Adds a new <see cref="Player"/> instance to the iternal <see cref="_sPlayers"/> array, using
        /// the given <paramref name="playerIndex"/> as index location in array.
        ///</summary>
        ///<param name="player">New <see cref="Player"/> instance to add</param>
        ///<param name="playerIndex">Location in array to add new instance</param>
        ///<exception cref="NullReferenceException">Thrown when the internal <see cref="_sPlayers"/> array is Null.</exception>
        ///<exception cref="ArgumentOutOfRangeException">Thrown when the given <paramref name="playerIndex"/> is not in the allowable range
        ///  of 0 - <see cref="_maxAllowablePlayers"/>.</exception>
        public static void AddPlayerAtIndex(Player player, int playerIndex)
        {
            if (_sPlayers == null)
                throw new NullReferenceException("The internal Player collection has not been initialized yet.");

            if (playerIndex < 0 || playerIndex >= _maxAllowablePlayers)
                throw  new ArgumentOutOfRangeException("playerIndex", @"Given player index value must be in the range of 0 - MaxAllowablePlayers value.");

            // Update with new player instance.
            _sPlayers[playerIndex] = player;
        }

        // 6/15/2010
        ///<summary>
        /// Returns the <see cref="Player"/> collection to the caller.
        ///</summary>
        ///<param name="players">(OUT) <see cref="Player"/> collection</param>
        public static void GetPlayers(out Player[] players)
        {
            players = null;
            // check if nulls
            if (_sPlayers == null) return;

            // return collection ref
            players = _sPlayers;
        }

        // 6/15/2010
        ///<summary>
        /// Retrieves the given <see cref="Player"/>, using the given <paramref name="playerNumber"/>.
        /// Returns true or false of success.
        ///</summary>
        ///<param name="playerNumber"><see cref="Player"/> number</param>
        ///<param name="player">(OUT) returns the requests <see cref="Player"/> instance</param>
        ///<returns>true/false of success</returns>
        ///<exception cref="ArgumentOutOfRangeException">Thrown when given <paramref name="playerNumber"/> is less than zero.</exception>
        public static bool GetPlayer(int playerNumber, out Player player)
        {
            player = null;
            // check if nulls
            if (_sPlayers == null) return false;

            // check if given index is less than zero.
            if (playerNumber < 0)
                throw new ArgumentOutOfRangeException("playerNumber", @"Player number given MUST be larger than zero.");

            // check if larger than collection, if so, just return with False.
            if (playerNumber >= _sPlayers.Length) return false;

            try
            {
                // retrieve player instance
                player = _sPlayers[playerNumber];
            }
            catch (ArgumentOutOfRangeException)
            {
                // capture and just return false.
                return false;
            }

            return true;
        }

        // 1/1/2010
        /// <summary>
        /// Creates the global SpriteBatch, and registers with the Game Services.
        /// </summary>
        public void CreateSpriteBatch()
        {
            _spriteBatch = new SpriteBatch(_graphicsDeviceMng.GraphicsDevice);
            Services.AddService(typeof(SpriteBatch), _spriteBatch);
            _spriteBatch.Disposing += SpriteBatchDisposing;
        }

        // 1/1/2010
        /// <summary>
        /// Captures the Disposing event for the SpriteBatch, and recreates
        /// the SpriteBatch.
        /// </summary>
        void SpriteBatchDisposing(object sender, EventArgs e)
        {
            // Remove old service
            Services.RemoveService(typeof (SpriteBatch));

            // Recreate SpriteBatch
            CreateSpriteBatch();
        }

        protected sealed override void BeginRun()
        {
            //Sound.PlayCue(Sounds.TitleMusic);        
            
#if DEBUG
            // 7/15/2008 - Add FPS Component
            _fps = new FPS(this)
                       {
                           HeaderDrawLocation = new Vector2(30, PreferredWindowHeight - 25),
                           FpsDrawLocation = new Vector2(70, PreferredWindowHeight - 25),
                           HeaderDrawColor = Color.White,
                           FpsDrawColor = Color.White
                       };
            Components.Add(_fps);
            Services.AddService(typeof(IFPS), _fps);
#endif

            // 11/7/2008 - DEBUG: 
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.GameDrawLoop, false);//"Game_DrawLoop"
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.GameUpdateLoop, false);//"Game_UpdateLoop"
           
           
#if !XBOX360
            // 7/15/2008 - Add _gameConsole Component
            EngineGameConsole = new GameConsole(this);            
            Components.Add(EngineGameConsole);
            Services.AddService(typeof(IGameConsole), EngineGameConsole);
#endif

           
            _messageDisplayComponent = new MessageDisplayComponent(this);
            Components.Add(_messageDisplayComponent);     
            

            base.BeginRun();
        }

        // 9/11/2008 - Used to Start the SpriteBatch Draw Call.
        protected override bool BeginDraw()
        {


#if DEBUG
            // 11/7/2008 - DEBUG: Start StopWatch
            StopWatchTimers.StartStopWatchInstance(StopWatchName.GameDrawLoop);//"Game_DrawLoop"

            // 5/26/2010 - DEBUG
            StopWatchTimers.StartStopWatchInstance(StopWatchName.GameDrawLoop_Begin);
#endif

            var result = base.BeginDraw();

            // 6/1/2010 - Updated to now use the method call, 'AlternateDoubleBuffer'.
            InstancedModel.AlternateDoubleBuffer();

            NetworkGameComponent.PumpUpdateThreads(); // 8/10/2009
            ForceBehaviorsManager.PumpUpdateThreads(); // 8/11/2009

#if DEBUG
            // 5/26/2010 - DEBUG
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.GameDrawLoop_Begin);
#endif

            return result;
            
        }

        /// <summary>
        /// This is where the Engine pumps all internal Threads, which are double-buffered 
        /// along side with the drawing thread.
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Draw(GameTime gameTime)
        {
#if DEBUG
            // 5/26/2010 - DEBUG
            StopWatchTimers.StartStopWatchInstance(StopWatchName.GameDrawLoop_Main);
#endif

            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
           
            base.Draw(gameTime);
           

#if DEBUG
            // 5/26/2010 - DEBUG
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.GameDrawLoop_Main);
#endif
            
        }

        // 9/11/2008 - Used to End the SpriteBatch Draw Call.
        /// <summary>
        /// This where the Engine waits for all internal Threads, which are double-buffered,
        /// to complete.
        /// </summary>
        protected override void EndDraw()
        {
#if DEBUG
            // 5/26/2010 - DEBUG
            StopWatchTimers.StartStopWatchInstance(StopWatchName.GameDrawLoop_End);
#endif

            
#if DEBUG
            StopWatchTimers.StartStopWatchInstance(StopWatchName.GameEndDrawWaitForThreads);//"Game_EndDraw_WaitForThreads"
#endif
            {
#if DEBUG
                StopWatchTimers.StartStopWatchInstance(StopWatchName.GameEndDrawWaitForThreads_Steer);//"Game_EndDraw_WaitForThreads_Steer"
#endif
                ForceBehaviorsManager.WaitForThreadsToFinishCurrentFrame();
#if DEBUG
                StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.GameEndDrawWaitForThreads_Steer);//"Game_EndDraw_WaitForThreads_Steer"
                
#endif

                NetworkGameComponent.WaitForThreadsToFinishCurrentFrame(); // 8/10/2009

                //Thread.MemoryBarrier();
            }
#if DEBUG
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.GameEndDrawWaitForThreads);//"Game_EndDraw_WaitForThreads"

            // 11/7/2008 - DEBUG: Reset StopWatch
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.GameDrawLoop);//"Game_DrawLoop"
#endif
            base.EndDraw();
#if DEBUG
            // 5/26/2010 - DEBUG
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.GameDrawLoop_End);
#endif

            
#if EditMode
            // 7/1/2010 - When in EditMode, check for any changes in the TerrainEditRoutines class
            //            for the current VertexBuffer.
            TerrainEditRoutines.DoUpdateCheckForVertexBuffers();

            // 7/9/2010 - When in EditMode, check for any changes to the AlphaMaps textures.
            //TerrainAlphaMaps.DoUpdateCheckForAlphaMap();
#endif

        }

        /// <summary>
        /// This is where the Engine creates the ResourceContent Managers.
        /// </summary>
        protected override void LoadContent()
        {
           

            // 8/29/2009 - Updated to create 'Resources360', for the XBOX360.
            // 8/1/2009 - Create instance of ResourceContentManager, used to get some Assets.
#if XBOX360
            ContentResourceManager = new ResourceContentManager(GameInstance.Services, Resource360.ResourceManager);
#else
            ContentResourceManager = new ResourceContentManager(GameInstance.Services, Resources.ResourceManager);
#endif
            
            base.LoadContent();                       
        }

        // 7/22/2009; 1/19/2011 - Add check for TrialMode.
        /// <summary>
        /// Engine uses this section to stop/start the StopWatch Timers.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        protected override void Update(GameTime gameTime)
        {
            StopWatchTimers.StartStopWatchInstance(StopWatchName.GameUpdateLoop);//"Game_UpdateLoop"

            base.Update(gameTime);

            // Check if in Trial Mode
            if (IsPurchasedGame) return;

            // Check to show the Marketplace screen
            CheckToShowMarketplace();

            // Only check ever N frames since 'Guide.IsTrialMode' call is 60ms.
            if (++_trialModeCheckCounter < 60) return;

            IsPurchasedGame = !Guide.IsTrialMode;
            _trialModeCheckCounter = 0; // Reset counter.
            
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.GameUpdateLoop);//"Game_UpdateLoop"



        }

        // 1/19/2011
        /// <summary>
        /// Checks if either the keyboard combination 'Alt-M' is pressed or
        /// the gamepad 'Right-Trigger' is pressed to show the
        /// <see cref="Microsoft.Xna.Framework.GamerServices.Guide.ShowMarketplace"/>.
        /// </summary>
        protected void CheckToShowMarketplace()
        {
            // Show Marketplace
            if (HandleInput.InputState != null && HandleInput.InputState.ShowMarketplace) 
                ShowMarketplace();
        }

        // 1/19/2011
        /// <summary>
        /// Calls the framework <see cref="Microsoft.Xna.Framework.GamerServices.Guide.ShowMarketplace"/>.
        /// </summary>
        public static void ShowMarketplace()
        {
            try
            {
                // Check if user signed in.
                if (_signedInGamer == null)
                {
                    if (!Guide.IsVisible)
                        Guide.ShowSignIn(1, true);

                    return;
                }

                // Checks if the current gamer can purchase items.
                if (!_signedInGamer.Privileges.AllowPurchaseContent) return;

                // Show Guide's marketplace.
                if (!Guide.IsVisible)
                    Guide.ShowMarketplace(_signedInGamer.PlayerIndex);
            }
            catch (GamerPrivilegeException)
            {
                // empty
            }
           
        }

        // 1/19/2011
        /// <summary>
        /// Occurs when the gamer signs out.
        /// </summary>
        private void SignedInGamer_SignedOut(object sender, SignedOutEventArgs e)
        {
            _signedInGamer = null;
        }

        // 1/19/2011
        /// <summary>
        /// Occurs when the gamer signs in.
        /// </summary>
        private void SignedInGamer_SignedIn(object sender, SignedInEventArgs e)
        {
            _signedInGamer = e.Gamer;
        }

// ReSharper disable UnusedMember.Local
        protected void ToggleFullScreen()
// ReSharper restore UnusedMember.Local
        {
            var presentation = _graphicsDeviceMng.GraphicsDevice.PresentationParameters;

            if (presentation.IsFullScreen)
            {   // going windowed
                _graphicsDeviceMng.PreferredBackBufferWidth = PreferredWindowWidth;
                _graphicsDeviceMng.PreferredBackBufferHeight = PreferredWindowHeight;
            }
            else
            {
                // XNA 4.0 Updates; 'CreationParameters' is gone; however, 'Adapter' is now with 'GraphicsDevice'.
                // going fullscreen, use desktop resolution to minimize display mode changes
                // this also has the nice effect of working around some displays that lie about 
                // supporting 1280x720
                //var adapter = _graphicsDeviceMng.GraphicsDevice.CreationParameters.Adapter;
                var adapter = _graphicsDeviceMng.GraphicsDevice.Adapter;
                _graphicsDeviceMng.PreferredBackBufferWidth = adapter.CurrentDisplayMode.Width;
                _graphicsDeviceMng.PreferredBackBufferHeight = adapter.CurrentDisplayMode.Height;
            }

            _graphicsDeviceMng.ToggleFullScreen();
        }

        // 4/6/2009 - 
        /// <summary>
        /// Disposes of unmanaged resources. 
        /// </summary>
        /// <remarks>
        /// When overriding this method, you must call its base.Dispose() method; otherwise, 
        /// important resources will not be disposed of properly.
        /// </remarks>
        /// <param name="disposing">Is this final dispose?</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 1/2/2010
                Services.RemoveService(typeof(IFOWTerrainData));
                Services.RemoveService(typeof(IMinimapTerrainData));
               
                if (_drawBuffer != null)
                {
                    _drawBuffer.Dispose();
                    _drawBuffer = null;
                }

                if (_spriteBatch != null)
                {
                    _spriteBatch.Dispose();
                    _spriteBatch = null;
                }

                // 9/9/2008 - Remove FPS Components
                if (_fps != null)
                {
                    Services.RemoveService(typeof(IFPS));
                    Components.Remove(_fps);
                    _fps.Dispose();
                    _fps = null;
                }

#if !XBOX360
                // Remove _gameConsole Component
                if (EngineGameConsole != null)
                {
                    Services.RemoveService(typeof(IGameConsole));
                    Components.Remove(EngineGameConsole);
                    EngineGameConsole.Dispose();
                    EngineGameConsole = null;
                }
#endif

                // Remove ScreenManager Component
                if (_screenManager != null)
                {
                    Components.Remove(_screenManager);
                    _screenManager.Dispose();
                    _screenManager = null;

                }
                
                // Remove MessageDisplay Component
                if (_messageDisplayComponent != null)
                {
                    Components.Remove(_messageDisplayComponent);
                    _messageDisplayComponent.Dispose();
                    _messageDisplayComponent = null;
                }

                // Remove Camera Component
                if (_camera != null)
                {
                    Components.Remove(_camera); // 10/22/2009
                    Services.RemoveService(typeof(ICamera));
                    //Camera.Dispose();
                    _camera = null;
                }

                // Remove Cursor Component
                if (_cursor != null)
                {
                    Components.Remove(_cursor);
                    Services.RemoveService(typeof(ICursor));
                    _cursor.Dispose();
                    _cursor = null;

                }

                // Remove ShadowMap Component
                if (_shadowMap != null)
                {
                    Components.Remove(_shadowMap);
                    Services.RemoveService(typeof(IShadowMap));
                    _shadowMap.Dispose();
                    _shadowMap = null;

                }

                // Remove FOW Component
                if (_fogOfWar != null)
                {
                    Components.Remove(_fogOfWar as IGameComponent);
                    Services.RemoveService(typeof(IFogOfWar));
                    ((IFogOfWar) _fogOfWar).Dispose();
                    _fogOfWar = null;
                }

                // Remove Minimap Component
                if (_miniMap != null)
                {
                    Components.Remove(_miniMap as IGameComponent);
                    Services.RemoveService(typeof(IMinimap));
                    ((IMinimap)_miniMap).Dispose();
                    _miniMap = null;
                }

                // Remove AStarManager Component
                if (_aStarInstance != null)
                {
                    Components.Remove(_aStarInstance as IGameComponent);
                    Services.RemoveService(typeof(IAStarManager));
                    ((IAStarManager)_aStarInstance).Dispose();
                    _aStarInstance = null;
                }

                // Remove Water Component
                if (_water != null)
                {
                    Components.Remove(_water);
                    Services.RemoveService(typeof(IWaterManager));
                    _water.Dispose();
                    _water = null;
                }

                if (_skyDome != null)
                {
                    _skyDome.Dispose();
                    _skyDome = null;
                }

                // Remove InterfaceDisplay Component
                if (_ifdTileManager != null)
                {
                    Components.Remove(_ifdTileManager);
                    Services.RemoveService(typeof(IIFDTileManager));
                    Services.RemoveService(typeof(IMinimapInterfaceDisplay)); // 1/2/2010
                    _ifdTileManager.Dispose();
                    _ifdTileManager = null;
                }

                // Remove Particles Component
                if (_particles != null)
                {
                    Components.Remove(_particles);
                    Services.RemoveService(typeof(ParticlesManager));
                    _particles.Dispose();
                    _particles = null;
                }

                // 11/24/2008 - Remove StatusBar Component
                if (_statusBar != null)
                {
                    Components.Remove(_statusBar as IGameComponent);
                    Services.RemoveService(typeof(IStatusBar));
                    ((IStatusBar)_statusBar).Dispose();
                    _statusBar = null;
                }

                // 2/6/2009 - Remove ExplosionManager Component
                if (ExplosionManager != null)
                {
                    //Components.Remove(ExplosionManager);
                    Services.RemoveService(typeof(ExplosionsManager));
                    ExplosionManager.Dispose();
                    ExplosionManager = null;
                }

                // 8/21/2009 - Remove KillSceneItemManager
                if (KillSceneItemManager != null)
                {
                    KillSceneItemManager.Dispose();
                    KillSceneItemManager = null;
                }

                // 11/7/2008 - Remove StopWatchTimers Component
                if (_timers != null)
                {
                    Components.Remove(_timers);
                    Services.RemoveService(typeof(StopWatchTimers));
                    _timers.Dispose();
                    _timers = null;
                }

                // 2/21/2009 - Remove NetworkGame Component
                if (_networkGameComponent != null)
                {
                    Components.Remove(_networkGameComponent);
                    Services.RemoveService(typeof(NetworkGameComponent));
                    _networkGameComponent.Dispose();
                    _networkGameComponent = null;
                }

                // 6/26/2009 - Remove GameViewPort Component
                if (_gameViewport != null)
                {
                    Components.Remove(_gameViewport);
                    Services.RemoveService(typeof(GameViewPort));
                    _gameViewport.Dispose();
                    _gameViewport = null;
                }

                // 5/4/2009 - Remove SoundManager Component
                if (_soundManager != null)
                {
                    //Components.Remove(_soundManager);
                    _soundManager.Dispose();
                    _soundManager = null;
                }

                // 7/17/2009 - Remove ForceBehaviorManager Component
                if (_forceBehaviorManager != null)
                {
                    Components.Remove(_forceBehaviorManager);
                    Services.RemoveService(typeof(ForceBehaviorsManager));
                    _forceBehaviorManager.Dispose();
                    _forceBehaviorManager = null;                    
                }

                // 7/17/2009 - Remove AIThreadManager Component
                if (_aiThreadManager != null)
                {
                    Components.Remove(_aiThreadManager);
                    Services.RemoveService(typeof(AIManager));
                    _aiThreadManager.Dispose();
                    _aiThreadManager = null;

                }

                // 1/15/2010 - Remove GameLevelManager Component
                if (GameLevelManager != null)
                {
                    Components.Remove(GameLevelManager);
                    Services.RemoveService(typeof(IGameLevelManager));
                    GameLevelManager.Dispose();
                    GameLevelManager = null;
                }

                // 6/16/2012 - Remove TerrainDirectionIcon Component
                if (_directionIconManager != null)
                {
                    Components.Remove(_directionIconManager);
                    Services.RemoveService(typeof(TerrainDirectionalIconManager));
                    _directionIconManager.Dispose();
                    _directionIconManager = null;
                }

                // 11/17/2009
                if (ContentMisc != null)
                {
                    ContentMisc.Unload(); // 1/5/2010
                    ContentMisc.Dispose();
                    ContentMisc = null;
                }

                // 11/17/2009
                if (ContentMaps != null)
                {
                    ContentMaps.Unload(); // 1/5/2010
                    ContentMaps.Dispose();
                    ContentMaps = null;
                }

                // 11/17/2009
                if (ContentGroundTextures != null)
                {
                    ContentGroundTextures.Unload();
                    ContentGroundTextures.Dispose();
                    ContentGroundTextures = null;
                }

                // 1/6/2010: Fixed: moved this call to be before the DisposeInstanceModels() call!
                // 5/29/2009 - Terminate the PreLoad Thread
                InstancedItemCulling.StopInstancedItemThreads();

                // 5/27/2009 - Dispose of InstanceModels
                InstancedItem.DisposeInstanceModels();

                // 6/22/2009 - Remove PhysX Compenent; ONLY for the PC
#if !XBOX
                /*if (_physXEngine != null)
                {
                    Components.Remove(_physXEngine);
                    Services.RemoveService(typeof(PhysX.PhysXEngine));
                    _physXEngine.Dispose();
                    _physXEngine = null;
                }*/
#endif               

                

            }

            base.Dispose(disposing);
        }

#if !XBOX360
        // 4/8/2010
        /// <summary>
        /// Using the Environment variable for Visual Studio, returns
        /// the current location for the project folders.
        /// </summary>
        /// <returns>Visual Studio's directory file path</returns>
        protected static string GetVisualStudioLocation()
        {
            try
            {
                // 1st - get path to Visual Studio DTE
                /*var dte = (EnvDTE.DTE)System.Runtime.InteropServices.Marshal.GetActiveObject("VisualStudio.DTE");

                // 2nd - get folder path to current 'Solution', which should be this project file.
                var folderPath = Path.GetDirectoryName(dte.Solution.FullName);*/

                return Environment.GetEnvironmentVariable("VisualStudioDir") + @"\Projects\"; 
            }
            catch (Exception)
            {
                Debug.WriteLine("Unable to set VSProjLoc variable - ", "Warning");
                return String.Empty;
            }
           
        }

        // 4/9/2010
        /// <summary>
        /// Using the 'ContentManager' class, retrieves this project's current 
        /// directory location, by reading the 'RootDirectory' variable.
        /// </summary>
        /// <param name="game">Game instance.</param>
        /// <returns>RootDirectory string</returns>
        private static string GetVisualStudioProjectLocation(Game game)
        {
            var tempContent = new ContentManager(game.Services);
            var rootDirectory = tempContent.RootDirectory;
            tempContent.Dispose();

            // 4/9/2010 - Split off Bin directory string, which is not needed.
            rootDirectory = rootDirectory.TrimEnd(@"\\Debug".ToCharArray());
            rootDirectory = rootDirectory.TrimEnd(@"\\x86".ToCharArray());
            rootDirectory = rootDirectory.TrimEnd(@"\\bin".ToCharArray());
            
            return rootDirectory;
        }

       
#endif
        // 11/17/2009
        /// <summary>
        /// Initializes the ContentManagers for the game engine.
        /// </summary>
        private static void CreateContentManagers(Game game)
        {
            // 4/6/2010 - Updated to new 'ContentMiscLoc' global var.
            if (ContentMisc == null)
                ContentMisc = new ContentManager(game.Services, ContentMiscLoc); // was "Content"

            // 4/6/2010 - Updated to new 'ContentMapsLoc' global var.
            if (ContentMaps == null)
                ContentMaps = new ContentManager(game.Services, ContentMapsLoc); // was @"1ContentMaps\Xbox360"
            

#if !XBOX360

            // 11/3/2009 - Check Texture Quality setting
            switch (TerrainTexturesQuality)
            {
                case TerrainTextures.Tex128X:
                    // 7/17/2009; 11/2/2009: Updated to use the 'ZippedContent' version.
                    if (ContentGroundTextures == null)
                    {
                        //ContentGroundTextures = new ContentManager(game.Services, @"1ContentTerrainTextures\x86\low128x\"); // ContentTextures_low_x86.xzb
                        ContentGroundTextures = new ZippedContent(@"1ContentZipped\ContentTextures_low_x86.xzb", game.Services); 
                    }
                    break;
                case TerrainTextures.Tex256X:
                    // 7/17/2009; 11/2/2009: Updated to use the 'ZippedContent' version.
                    if (ContentGroundTextures == null)
                    {
                        //ContentGroundTextures = new ContentManager(game.Services, @"1ContentTerrainTextures\x86\med256x\"); // ContentTextures_med_x86.xzb
                        ContentGroundTextures = new ZippedContent(@"1ContentZipped\ContentTextures_med_x86.xzb", game.Services); 
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
#else

            // 11/3/2009: Non-Zipped format content artwork for Xbox.
            //if (ContentGroundTextures == null) ContentGroundTextures = new ContentManager(game.Services, ContentGroundTexturesLoc);

            if (ContentGroundTextures == null)
                ContentGroundTextures = new ZippedContent(@"1ContentZipped\ContentTextures_med_Xbox 360.xzb", game.Services); // ContentTextures_med_Xbox 360.xzb
#endif
        }

       

        // 12/31/2009
        /// <summary>
        /// Allows LateBinding some Assembly (dll) file, and then will
        /// instantiate the given 'ClassName', and return the object to the caller.
        /// </summary>
        /// <param name="assemblyFile">AssemblyFile name to load</param>
        /// <param name="className">Class Name to instantiate within Assembly</param>
        /// <param name="instantiatedObject">(OUT) Instantiated object</param>
        /// <returns>True/False of success</returns>
        private bool LateBindAssembly(string assemblyFile, string className, out object instantiatedObject)
        {
            instantiatedObject = null;

            try
            {

                // set platform location to load from.
#if XBOX360
                const string platformType = "XBox360";
#else
                const string platformType = "x86";
#endif

                var a = Assembly.LoadFrom("0LateBinds/" + platformType + "/" + assemblyFile);

                var mytypes = a.GetTypes();

                //const BindingFlags flags = (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static |
                //                           BindingFlags.Instance);

                // Search for Instance to instantiate from Assembly.
                foreach (var t in mytypes)
                {
                    //var mi = t.GetMethods(flags);

#if XBOX360
    // locate class instance to instantiate.
                if (t.Name == className)
                {
                    instantiatedObject = Activator.CreateInstance(t);
                    ((ICommonInitilization) instantiatedObject).CommonInitilization(this);
                    return true;
                }
#else
                    // locate class instance to instantiate.
                    if (t.Name == className)
                    {
                        instantiatedObject = Activator.CreateInstance(t, this);
                        return true;
                    }
#endif


                    /*foreach (var m in mi)
                {
                    m.Invoke(obj, null);
                }*/
                }

                // Name not found
                return false;
            }
                // 1/1/2010 - Capture the possibility of the DLL not being in the folder at all.
            catch (FileNotFoundException) // PC throws this.
            {
                
                System.Console.WriteLine(@"DLL Component {0} not found.  Therefore, this will be skipped for late binding.", assemblyFile);
                return false;
            }
            catch(IOException) // XBOX throws this.
            {
                System.Console.WriteLine(@"DLL Component {0} not found.  Therefore, this will be skipped for late binding.", assemblyFile);
                return false;
            }
#if !XBOX360
            catch (ReflectionTypeLoadException err)
            {
                if (err.LoaderExceptions != null)
                {
                    // List out each LoaderException error to console.
                    foreach (var loaderException in err.LoaderExceptions)
                    {
                        if (loaderException.Message != null)
                            System.Console.WriteLine(@"LoaderExceptions reflection error - {0}", loaderException.Message);
                    }

                    MessageBox.Show(
                        @"Late-Binding failed, due to a Loading Exception on the Interface!  This usually occurs if you have an outdated interface; please update your interface for the assembly you are trying to late-bind.",
                        @"LateBind Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                System.Console.WriteLine(@"DLL Component {0} reflection error.  Therefore, this will be skipped for late binding.", assemblyFile);
                return false;
            }
#endif
        }
    }
}