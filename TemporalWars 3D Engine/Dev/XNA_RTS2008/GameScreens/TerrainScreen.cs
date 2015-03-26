#region File Description
//-----------------------------------------------------------------------------
// TerrainScreen.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using ImageNexus.BenScharbach.TWEngine.AI;
using ImageNexus.BenScharbach.TWEngine.Audio;
using ImageNexus.BenScharbach.TWEngine.Audio.Enums;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.BeginGame.Enums;
using ImageNexus.BenScharbach.TWEngine.Common;
using ImageNexus.BenScharbach.TWEngine.Common.Extensions;
using ImageNexus.BenScharbach.TWEngine.ForceBehaviors;
using ImageNexus.BenScharbach.TWEngine.GameCamera;
using ImageNexus.BenScharbach.TWEngine.GameScreens.Generic;
using ImageNexus.BenScharbach.TWEngine.HandleGameInput;
using ImageNexus.BenScharbach.TWEngine.IFDTiles;
using ImageNexus.BenScharbach.TWEngine.InstancedModels;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.Interfaces;
using ImageNexus.BenScharbach.TWEngine.MemoryPool;
using ImageNexus.BenScharbach.TWEngine.Networking;
using ImageNexus.BenScharbach.TWEngine.Networking.Structs;
using ImageNexus.BenScharbach.TWEngine.Particles;
using ImageNexus.BenScharbach.TWEngine.Players;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWEngine.ScreenManagerC;
using ImageNexus.BenScharbach.TWEngine.Shadows;
using ImageNexus.BenScharbach.TWEngine.Shapes;
using ImageNexus.BenScharbach.TWEngine.SkyDomes;
using ImageNexus.BenScharbach.TWEngine.Terrain;
using ImageNexus.BenScharbach.TWEngine.Terrain.Enums;
using ImageNexus.BenScharbach.TWEngine.Terrain.Structs;
using ImageNexus.BenScharbach.TWEngine.Utilities;
using ImageNexus.BenScharbach.TWEngine.rtsCommands;
using ImageNexus.BenScharbach.TWEngine.rtsCommands.Enums;
using ImageNexus.BenScharbach.TWLate.AStarInterfaces.AStarAlgorithm;
using ImageNexus.BenScharbach.TWLate.RTS_FogOfWarInterfaces.FOW;
using ImageNexus.BenScharbach.TWLate.RTS_MinimapInterfaces.Minimap;
using ImageNexus.BenScharbach.TWLate.RTS_StatusBarInterfaces.StatusBar;
using ImageNexus.BenScharbach.TWScripting.Interfaces;
using ImageNexus.BenScharbach.TWTools.Particles3DComponentLibrary;
using ImageNexus.BenScharbach.TWTools.PerfTimersComponent.Timers;
using ImageNexus.BenScharbach.TWTools.PerfTimersComponent.Timers.Enums;
using ImageNexus.BenScharbach.TWTools.ScreenTextDisplayer.ScreenText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Net;

#if !XBOX360
using ImageNexus.BenScharbach.TWEngine.Console.Enums;
using TWEngine.TerrainTools;
#else
#endif

namespace ImageNexus.BenScharbach.TWEngine.GameScreens
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.GameScreens"/> namespace contains the common classes
    /// which make up the entire <see cref="TWEngine.GameScreens"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }


    /// <summary>
    /// This <see cref="TerrainScreen"/> implements the actual RTS game logic, which includes
    /// the creation of the <see cref="Player"/> instances, creation of the <see cref="TWEngine.Terrain"/>,
    /// loading the current level map, as well as continually calling update and draw throughout 
    /// the game cycle.
    /// </summary>
    public sealed class TerrainScreen : GameScreen, ITerrainScreen
    {
        #region TerrainScreen Fields

        // 6/10/2012 - UniqueKey.
        private readonly Guid _uniqueKey = Guid.NewGuid();

        // 3/29/2011 - XNA 4.0 Updates - Multi RenderTargets to draw Shadows on Terrain, due to 512 shader constant limit.
        private static RenderTarget2D _terrainDrawRt;
        private static RenderTarget2D _terrainDraw2Rt;
        private static RenderTarget2D _terrainDraw3Rt;

        // 3/23/2011 - XNA 4.0 Updates -
        // Note: This creates a BlendState where the SourceColor is Multiplied against the DestColor.  
        //       Should ONLY be used with techniques which reduce the color values on the destination target.
        private static readonly BlendState BlendStateMultiply = new BlendState()
        {
            AlphaBlendFunction = BlendFunction.Add,
            AlphaDestinationBlend = Blend.InverseSourceAlpha,
            AlphaSourceBlend = Blend.One,
            BlendFactor = Color.White,
            ColorBlendFunction = BlendFunction.Add,
            ColorDestinationBlend = Blend.Zero,
            ColorSourceBlend = Blend.DestinationColor,
            ColorWriteChannels = ColorWriteChannels.All
        };

        // 10/27/2009
        private static SkyDome _skyDome;

        // 8/14/2008
        private GameTime _gameTime;

        private NetworkSession _networkSession;

        // 1/29/2009 - 
        /// <summary>
        /// Set when a <see cref="SceneItem"/> is ready to Delete; when true, this will force a RemoveAll call 
        /// on this <see cref="TerrainScreen"/> collection of <see cref="SceneItem"/>; for example, all the 
        /// <see cref="ScenaryItemScene"/>.
        /// </summary>
        internal static bool DoRemoveAllCheck;

        // 12/2/2008 - NetworkGame IsReady flags

        // 4/7/2009 - GamerInfo, for PlayerSide/PlayerColor.            
        private GamerInfo _gamerInfoHost;
        private GamerInfo _gamerInfoClient;

        // 1/16/2010
        private static readonly Vector3 Vector3Zero = Vector3.Zero;

        // 9/23/2008 - Add InstanceItem Batch Draw Class
        private InstancedItem _instancedItem;
       
        // 2/21/2009 - Now added during initial loadup
        private NetworkGameComponent _networkGameComponent;        

        // Terrain
        /// <summary>
        /// <see cref="TerrainScene"/> reference
        /// </summary>
        internal static TerrainScene TerrainScene;
        // Terrain Shape
        /// <summary>
        /// <see cref="ITerrainShape"/> reference
        /// </summary>
        internal static ITerrainShape TerrainShapeInterface;

        // 7/15/2008 - Add IGameConsole Interface
#if !XBOX360
        private IGameConsole _gameConsole;
#endif

        // 7/9/2008 - Add FogOfWar Class for Indirect Inheritance
        private IFogOfWar _fogOfWar;

        // 1/2/2010 - Add MiniMap Interface
        private static IMinimap _miniMap;
        private bool _initMiniMap; // 8/18/2008

        // 7/8/2008 - Add ShadowMap Interface
        private ShadowMap _shadowMap; 

        // 7/9/2008 - Add IFDTileManager Class for Indirect Inheritance
        private IFDTileManager _ifdTileManager;

        // 10/15/2008 - TODO: Debug - ScreenTextItems; shows the bytes sent accross wire.
        private ScreenTextItem _screenText1;
        private ScreenTextItem _screenText2;

        // 8/12/2008 - Save Maps Tool
        #if !XBOX360
        private SaveMapsTool _saveMapsTool;
        #endif        

        #endregion

        // 12/2/2013 - Add backing field.
        private static RenderingType _renderingType;

        #region Events

        // 1/6/2010 - 
        ///<summary>
        /// Occurs when all <see cref="Player"/> instances are created.
        ///</summary>
        /// <remarks>
        /// Necessary, since static references in other components will not be refencing the correct instance!
        /// </remarks>
        public static event EventHandler PlayerInstancesCreated;

        // 1/22/2011
        /// <summary>
        /// Occurs when the <see cref="TerrainScreen"/> first starts to load
        /// the current instance.
        /// </summary>
        public static event EventHandler Loading;

        // 1/22/2011
        /// <summary>
        /// Occurs when the <see cref="TerrainScreen"/> is unloading the current instance.
        /// </summary>
        public static event EventHandler UnLoading;

        #endregion

        #region TerrainScreen Properties

        // 12/2/2013 - AppSetting Override..
        /// <summary>
        /// Gets or sets the use of the <see cref="RenderingType"/> on the terrain, which overrides the 'RenderingType' setting; this is set
        /// from the App.Config xml file.
        /// </summary>
        public static RenderingType? AppSettingRenderingType { get; set; }

        // 5/23/2010; 12/2/2013 - Updated to set the ScreenManager.RenderingType / Updated to check the AppSettingRenderType override.
        /// <summary>
        /// The <see cref="RenderingType"/> Enum to use as default
        /// for <see cref="TerrainScreen"/>. (Ex: Normal, NormalPP, or Deferred)
        /// </summary>
        public static RenderingType RenderingType
        {
            get { return (AppSettingRenderingType != null) ? AppSettingRenderingType.Value : _renderingType; }
            set
            {
                _renderingType = value;

                // 12/2/2013 - Set what Rendering Style the TerrainScreen should use.
                ScreenManager.RenderingType = (AppSettingRenderingType != null) ? AppSettingRenderingType.Value : value; 
            }
        }


        // 11/19/2009 - 
        ///<summary>
        /// Set when <see cref="Player"/> starts MP game by themselves!  This allows skipping the
        /// <see cref="NetworkGameIsReadyCheck"/> section; otherwise game would be stuck. 
        ///</summary>
        public static bool SandBoxMode { get; internal set; }

        // 10/7/2009 
        /// <summary>
        /// When set to true, the <see cref="Player._selectableItems"/> placed on the map, like buildings, turrets, and tanks, for example,
        /// will be saved with the map.  This is exclusively for creating single player levels, via scripting conditions.
        /// </summary>
        public static bool SaveSelectableItemsWithMap { get; set; }

        /// <summary>
        /// The logic for deciding whether the game is paused depends on whether
        /// this is a networked or single player game. If we are in a network session,
        /// we should go on updating the game even when the user tabs away from us or
        /// brings up the pause menu, because even though the local player is not
        /// responding to input, other remote players may not be paused. In single
        /// player modes, however, we want everything to pause if the game loses focus.
        /// </summary>
        new bool IsActive
        {
            get { return _networkSession == null ? base.IsActive : !IsExiting; }
        }
       
        // 12/2/2008
        ///<summary>
        /// Flag used to know when client player is ready during MP games.
        ///</summary>
        public static bool ClientIsReady { get; set; }
        // 12/2/2008
        ///<summary>
        /// Flag used to know when the server is ready during MP games.
        ///</summary>
        public static bool ServerIsReady { get; set; }

        // 4/28/2008 - Return 'scene' SceneItemOwner for terrain; added this for Access from Player Class
        ///<summary>
        /// The <see cref="SceneItem"/> is actually a collection of child <see cref="SceneItem"/>.  Therfore, this
        /// returns the base <see cref="SceneItem"/> collection for the <see cref="TerrainScreen"/>.
        ///</summary>
        public static SceneItem SceneCollection { get; private set; }

        // 5/13/2008 - Return 'TerrainShape' SceneItemOwner
        ///<summary>
        /// Returns reference to <see cref="ITerrainShape"/>.
        ///</summary>
        public ITerrainShape ITerrainShape
        {
            get { return TerrainShapeInterface; }
        }

        // 8/18/2008 - 
        ///<summary>
        /// Level game map to load
        ///</summary>
        public static string TerrainMapToLoad { get; set; }
        // 11/17/2009 -
        ///<summary>
        /// Level game map type to load; 'MP' or 'SP'.
        ///</summary>
        /// <remarks>
        /// MP = Multi-player game, and SP = Single-player game.
        /// </remarks>
        public static string TerrainMapGameType { get; set; }

        // 6/5/2009
        ///<summary>
        /// Used to turn on the 'Rain' <see cref="ParticleSystem"/> effect.
        ///</summary>
        public static bool IsRaining { get; set; }

        // 3/1/2011
        /// <summary>
        /// Used to turn on the 'Snow' <see cref="ParticleSystem"/> effect.
        /// </summary>
        public static bool IsSnowing { get; set; }

        // 5/16/2012
        /// <summary>
        /// Used to turn on the 'SkyBox' effect.
        /// </summary>
        public static bool UseSkyBox { get; set; }

        #endregion
        
        #region Initialization

        // 4/8/2009 - 
        /// <summary>
        /// Constructor for <see cref="TerrainScreen"/>, which sets the level <see cref="mapName"/> to load,
        /// and sets the <see cref="GameScreen.TransitionOnTime"/> and <see cref="GameScreen.TransitionOffTime"/> properties,
        /// to the values of 1.5 sec and 0.5 sec, respectively.
        /// </summary>
        /// <remarks>Single-Player games constructor</remarks>
        /// <param name="mapName">Level game map to load</param>
        /// <param name="gamerInfo"><see cref="GamerInfo"/> structure</param>
        public TerrainScreen(string mapName, GamerInfo gamerInfo) : this(null, mapName, gamerInfo, null)
        {

            // 3/3/2009 - Add to Game Services
            /*TemporalWars3DEngine.GameInstance.Services.AddService(typeof(TerrainScreen), this);

            // 10/28/2009 - Set what Rendering Style the TerrainScreen should use.
            ScreenManager.RenderingType = RenderingType; // 5/23/2010: Updated to use default AutoProp.

            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            // 5/3/2009: FXCop - Use IsNullOrEmpty
            // 8/31/2008 - Set MapName to Load
            TerrainMapToLoad = !string.IsNullOrEmpty(mapName) ? mapName : "WinterWonderland";   
            // 11/17/2009 - Set to SP Map load type.
            TerrainMapGameType = "SP";

            // 4/8/2009 - Save GamerInfo data
            _gamerInfoHost = gamerInfo; // Also used for SP games! */           
           
        }

        // 
        /// <summary>
        ///  Constructor for <see cref="TerrainScreen"/>, which sets the level <see cref="mapName"/> to load,
        /// and sets the <see cref="GameScreen.TransitionOnTime"/> and <see cref="GameScreen.TransitionOffTime"/> properties,
        /// to the values of 1.5 sec and 0.5 sec, respectively.
        /// </summary>
        /// <remarks>Multi-Player games constructor</remarks>
        /// <param name="networkSession"><see cref="NetworkSession"/> instance</param>
        /// <param name="mapName">Level game map to load</param>
        /// <param name="gamerInfoHost"><see cref="GamerInfo"/> structure for Host player</param>
        /// <param name="gamerInfoClient"><see cref="GamerInfo"/> structure for Client player</param>
        public TerrainScreen(NetworkSession networkSession, string mapName, 
                            GamerInfo gamerInfoHost, GamerInfo? gamerInfoClient)
        {
            // 3/29/2011 - XNA 4.0 Updates
            var graphicsDevice = TemporalWars3DEngine.GameInstance.GraphicsDevice;
            var width = graphicsDevice.Viewport.Width;
            var height = graphicsDevice.Viewport.Height;
            _terrainDrawRt = new RenderTarget2D(graphicsDevice, width, height, true, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            _terrainDraw2Rt = new RenderTarget2D(graphicsDevice, width, height, true, SurfaceFormat.Color, DepthFormat.None);
            _terrainDraw3Rt = new RenderTarget2D(graphicsDevice, width, height, true, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);

            // 3/3/2009 - Add to Game Services
            TemporalWars3DEngine.GameInstance.Services.AddService(typeof(TerrainScreen), this);

            // 10/28/2009 - Set what Rendering Style the TerrainScreen should use.
            ScreenManager.RenderingType = RenderingType; // 5/23/2010: Updated to use default AutoProp.

            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            // 5/3/2009: FXCop - Use IsNullOrEmpty
            // 8/31/2008 - Set MapName to Load
            TerrainMapToLoad = !string.IsNullOrEmpty(mapName) ? mapName : "WinterWonderland";
            // 11/17/2009 - Set to MP Map load type.
            TerrainMapGameType = (networkSession == null) ? "SP" : "MP";

            // 4/8/2009 - Save GamerInfo data
            _gamerInfoHost = gamerInfoHost; // Also used for SP games!
            if (gamerInfoClient != null) _gamerInfoClient = gamerInfoClient.Value;
            _networkSession = networkSession;
            
        }

        // 8/13/2008 - Called from LoadContent()     
        /// <summary>
        /// Helper method, which does all the heavy-lifting for setting up and intializing
        /// a new game level.
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        private void TerrainInitialize(Game game)
        {
            // 3/23/2010 - Force a Memory clear call, before attempting level re-load.
            GC.Collect();

            // 4/21/2011 - Retrieve Cursor interface and show cursor.
            var cursor = (Cursor)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(ICursor));
            cursor.Visible = true;

            // 1/22/2011 - Trigger Event
            if (Loading != null)
                Loading(this, EventArgs.Empty);

#if DEBUG
            // 11/7/2008 - StopWatchTimers            
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.TerrainUpdate, false);//"TerrainUpdate"        
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.TerrainDrawSelectables, false);//"TerrainDrawSelectables"
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.TerrainDrawSelectablesA, false);//"TerrainDrawSelectables_a"
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.TerrainDrawSelectablesB, false);//"TerrainDrawSelectables_b"
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.TerrainDrawAlpha, false);//"TerrainDrawAlpha"

            // 10/31/2008 - to measure the Load times.
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.StartLoad, true);//"StartLoad"
#endif
            // 6/18/2012 - Get the GameLevelManager service
            var gameLevelManager = (IGameLevelManager)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(IGameLevelManager));

            // NOTE: This is ONLY set in the inheriting game projects, like the TemporalWarsGame example!
            // 2/1/2010 - Check if 'UseGameLevelManager' is set to TRUE; if so, then
            //             starts a 'GameLevel'. (Scripting Purposes)
            if (gameLevelManager != null && gameLevelManager.UseGameLevelManager)
            {
                // Make sure 'Update' is enabled.
                gameLevelManager.Enabled = true;

                // 2/1/2010 - Level Ready to load, so get TerrainMap name.
                TerrainMapToLoad = gameLevelManager.GetTerrainMapToLoad();
            }

            
            // 4/8/2009 - Get PlayerSide / PlayerLocation
            int playerSide;
            int playerLocation;
            if (_networkSession == null)
            {
                // then SP game
                playerSide = _gamerInfoHost.PlayerSide;
                playerLocation = _gamerInfoHost.PlayerLocation == 0 ? 1 : _gamerInfoHost.PlayerLocation;
            }
            else
            {
                // 2/21/2009: Moved creation of NetworkGameComponent to be at start of game; this way
                //            it could be used within the Lobby network screens.
                //            The setting of the 'NetworkSession' instance is done in the 'CreateOrFind'
                //            and 'JoinSession' screens.
                // 2/21/2009 - Get Reference to NetworkGameComponent
                _networkGameComponent = (NetworkGameComponent)game.Services.GetService(typeof(NetworkGameComponent));

                // then MP game
                var gamerInfo = (GamerInfo)_networkSession.LocalGamers[0].Tag;
                playerSide = gamerInfo.PlayerSide;
                playerLocation = gamerInfo.PlayerLocation == 0 ? 1 : gamerInfo.PlayerLocation;
            }
           
            // Add IFDTileManager Interface           
            _ifdTileManager = (IFDTileManager)game.Services.GetService(typeof(IIFDTileManager));
            if (_ifdTileManager != null) // 5/29/2010
            {
                _ifdTileManager.IsVisible = true; // 11/18/09 - Activates drawing; turned off in 'UnLoadContent'.
                _ifdTileManager.CreateIFDTiles(playerSide); // 4/8/2009: Pass Player's Side#.
                _ifdTileManager.Visible = true;
            }

            // Init Terrain Mesh  
            var tmpInPos = Vector3Zero;
            TerrainScene = new TerrainScene(game, ref tmpInPos, TerrainIsIn.PlayableMode);
            SceneCollection.Add(TerrainScene);

            // 10/15/2008 - Init ScreenText Class
            ScreenTextManager.AddNewScreenTextItem(string.Empty, new Vector2(10, 10), Color.Red, out _screenText1);
            ScreenTextManager.AddNewScreenTextItem(string.Empty, new Vector2(10, 25), Color.Red, out _screenText2);

            // Store terrain Map width & height attributes for quick access
            TerrainShapeInterface = (ITerrainShape)TerrainScene.ShapeItem;
            // 6/13/2010 - Reset counter to zero.
            TerrainShape.FirstTenFramesOfGame = 0;

            // 10/27/2009 - Get SkyDome instance from services.
            _skyDome = (SkyDome)ScreenManager.Game.Services.GetService(typeof(SkyDome));
            // 11/2/2009 - Init skyDome, for level-reloads.
            if (_skyDome != null) _skyDome.Initialize();

            // Add ShadowMap Interface   
            ShadowMap.InitStaticShadowLightSettings();
            _shadowMap = (ShadowMap)game.Services.GetService(typeof(IShadowMap));
            if (_shadowMap != null) _shadowMap.ResetVisiblityFlags(); // 5/29/2010

            // 9/23/2008 - Add InstancedItem Component
            _instancedItem = new InstancedItem(game);
            game.Components.Add(_instancedItem);
            
            // 11/7/2009 - Wait here until 'PreLoad' instancedItems thread is complete!
            //LoadingScreen.LoadingMessage = "Waiting on PreLoad 'InstancedItems' Thread...";
            //InstancedItemLoader.PreLoadInstanceItemsFinished.WaitOne(); // 1/5/2010
            //InstancedItemLoader.PreLoadInstanceItemsFinished.Reset(); // 1/6/2010
           

            // 9/15/2008
            // Thread 1: Start Loading Map data in seperate threads
            TerrainStorageRoutines.LoadTerrainData(TerrainMapToLoad, TerrainMapGameType);

            // 4/9/2009: Updated to a thread version.
            // Used For Showing PathNodes for Debugging purposes
            // 5/12/2008 - Populate pathNodes Array List           
            TerrainShape.PopulatePathNodesArray_Threaded();            


            //Get GameConsole Ref
#if !XBOX360
            _gameConsole = (IGameConsole)game.Services.GetService(typeof(IGameConsole));
#endif

            // Add FogOfWar Interface           
            _fogOfWar = (IFogOfWar)game.Services.GetService(typeof(IFogOfWar));
            if (_fogOfWar != null)
            {
                _fogOfWar.InitFogOfWarSettings();
                _fogOfWar.IsVisible = true; // 11/18/09 - Activates drawing; turned off in 'UnLoadContent'.
            }

            // Add MiniMap Interface           
            _miniMap = (IMinimap)game.Services.GetService(typeof(IMinimap));
            if (_miniMap != null)
            {
                _miniMap.InitSettings(); // 1/2/2010
                _miniMap.IsVisible = true;  // 11/18/09 - Activates drawing; turned off in 'UnLoadContent'.
                _miniMap.ShowTextureWrapper = true; // 12/7/2009 - Turn on Minimap Wrapper

                // 2/26/2011 - Set to use default Minimap wrapper
                if (TemporalWars3DEngine.IfdTileMiniMapWrapper == null)
                {
                    _miniMap.UseDefaultWrapperTexture();
                }
                else
                {
                    // 2/26/2011 - Replace Minimap wrapper

                    // retrieve IMinimap
                    var iMiniMap = (IMinimap)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(IMinimap));

                    // check if null
                    if (iMiniMap == null) return;

                    // Set new wrapper texture
                    iMiniMap.UpdateWrapperTexture(TemporalWars3DEngine.IfdTileMiniMapWrapper);
                }
            }

            // Camera Height is set internally, using the MaxHeight + TerrainHeight as the default.           
            Camera.CameraTarget = new Vector3(TerrainData.MiddleMapX, 0, TerrainData.MiddleMapY);

            // Set AStar Globals
            if (TemporalWars3DEngine.AStarGraph != null) // 1/13/2010
            {
                TemporalWars3DEngine.AStarGraph.NodeArraySize = TemporalWars3DEngine.SPathNodeSize;
                TemporalWars3DEngine.AStarGraph.NodeStride = TemporalWars3DEngine._pathNodeStride;
                TemporalWars3DEngine.AStarGraph.CreatePathfindingGraph(); // 1/13/2010
            }

            // 4/28/2008 - Create Player Instances
            CreatePlayers(game); 

            // 9/15/2008 - Wait until Load Thread is done.
            LoadingScreen.LoadingMessage = "Main Thread done.";
            TerrainStorageRoutines.LoadDataThread.Join();
            LoadingScreen.LoadingMessage = "LoadData Thread#1 done.";
            TerrainStorageRoutines.LoadDataThread2.Join();
            LoadingScreen.LoadingMessage = "LoadData Thread#2 done.";

            // 11/13/2009 - Wait here until 'PreLoad' InstancedItems thread is complete!
            /*LoadingScreen.LoadingMessage = "Waiting on PreLoad 'InstancedItems' Thread...";
            InstancedItemLoader.PreLoadInstanceItemsFinished.WaitOne();
            InstancedItemLoader.PreLoadInstanceItemsFinished.Reset(); // 1/6/2010*/

            // 5/5/2009 - Add Birds Sound group, to play in a loop with constant delay
            //AudioManager.PlayCueLoopedWithConstantDelay(Sounds.Birds_Mountain, 45);
            //SoundManager.PlayCueLoopedWithConstantDelay(SoundBankGroup.Ambient, Sounds.Amibient_Birds_Typical, 30);
            
            // 5/5/2009 - Start looping rain sound
            if (IsRaining)
                AudioManager.Play(_uniqueKey, Sounds.Rain1);

            // 10/31/2008 - Get Stopwatch Time
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.StartLoad);//"StartLoad"

            // 11/19/2009: Updated to check the new 'SandBoxMode', which allows skipping the wait!
            // 12/2/2008 - Checks if NetworkGame, and if so, waits for both sides to load before 
            //             letting this game start.
            if (!SandBoxMode) NetworkGameIsReadyCheck();

            // 7/23/2008 - Call Camera UpdatePosition Once to set proper view.
            Camera.UpdatePosition(null, null);

            // 3/23/2010 - Force a Memory clear call, before starting level.
            GC.Collect();

            // NOTE: This is ONLY set in the inheriting game projects, like the TemporalWarsGame example!
            // 10/2/2009 - Check if 'UseGameLevelManager' is set to TRUE; if so, then
            //             start the first 'GameLevel'. (Scripting Purposes)
            if (gameLevelManager != null && gameLevelManager.UseGameLevelManager)
            {
                if (!gameLevelManager.StartGameLevel())
                    throw new InvalidOperationException("GameLevel failed to start!");
            }
            else
            {
                // 1/15/2011 - Refactored into new method.
                CreatePlayerHqBuildings(playerSide, playerLocation);
                // 1/15/2011 - Refactored into new method.
                SetPlayerBuildableArea(playerLocation);
            }
        }

        // 1/15/2011
        /// <summary>
        /// Sets the player's buildable rectangle area arond the given <paramref name="playerLocation"/>.
        /// </summary>
        /// <param name="playerLocation">Player's location on the map.</param>
        private static void SetPlayerBuildableArea(int playerLocation)
        {
            var thisPlayer = TemporalWars3DEngine.SThisPlayer;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(thisPlayer, out player);

            // 5/1/2009 - Set the BuildableAreaRectangle's location
            switch (playerLocation)
            {
                case 1:
                    player.SetBuildableAreaRectangle(TerrainShapeInterface.MapMarkerPositions.markerLoc1, 1000); // 1/6/2010
                    break;
                case 2:
                    player.SetBuildableAreaRectangle(TerrainShapeInterface.MapMarkerPositions.markerLoc2, 1000); // 1/6/2010
                    break;
            }
        }

        // 3/30/2011
        ///<summary>
        /// Retrieves the <see cref="SceneItem"/> instance for the given <see cref="ItemType"/>.
        ///</summary>
        ///<param name="itemType"><see cref="ItemType"/> Enum to retrieve</param>
        ///<param name="sceneItemToRetrieve">(OUT) Instance of <see cref="SceneItem"/></param>
        ///<returns>true/false of result</returns>
        public static bool GetSceneItemInstance<TSceneItem>(ItemType itemType, out TSceneItem sceneItemToRetrieve)
            where TSceneItem : SceneItem
        {
            sceneItemToRetrieve = default(TSceneItem);

            // Iterate collection, returning 1st found instance of ItemType
            var count = SceneCollection.Count;
            for (var i = 0; i < count; i++)
            {
                var sceneItem = SceneCollection[i];
                if (sceneItem == null) continue;

                if (sceneItem.ShapeItem.ItemType != itemType) continue;

                
                // Found item
                sceneItemToRetrieve = (TSceneItem)sceneItem;
                return true;
            }

            return false;
        }

        // 1/15/2011
        /// <summary>
        /// Creates the proper player's HeadQuarters at the given <see cref="MapMarkerPositions"/>.
        /// </summary>
        /// <param name="playerSide">Player's side.</param>
        /// <param name="playerLocation">Player's location on the map.</param>
        private void CreatePlayerHqBuildings(int playerSide, int playerLocation)
        {
            // move camera to this Position
            Camera.CameraTarget = playerLocation == 1 ? TerrainShapeInterface.MapMarkerPositions.markerLoc1 : TerrainShapeInterface.MapMarkerPositions.markerLoc2;

            // 4/30/2009: Updated to pass the 'Production Type', since the IFDTiles are now created for BuildingScene when placed!
            // place HQ for the given side
            var itemArgs = new ItemCreatedArgs(ItemGroupType.Buildings, 
                                               ItemGroupType.Buildings, 
                                               playerSide == 1 ? ItemType.sciFiBldb15 : ItemType.sciFiBldb14, 
                                               null,
                                               playerLocation == 1 ? TerrainShapeInterface.MapMarkerPositions.markerLoc1 : TerrainShapeInterface.MapMarkerPositions.markerLoc2, 
                                               0, 
                                               null, 
                                               0);

            var thisPlayer = TemporalWars3DEngine.SThisPlayer;
                
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(thisPlayer, out player);
                    

            player.IFDPlacement_ItemCreated(this, itemArgs);
            
        }

        // 4/7/2009
        /// <summary>
        /// Creates the <see cref="Player"/> for both SP and MP games.
        /// </summary>
        /// /// <remarks>
        /// MP = Multi-player game, and SP = Single-player game.
        /// </remarks>
        /// <param name="game"><see cref="Game"/> instance</param>
        private void CreatePlayers(Game game)
        {
            // 9/3/2008 - Update to Create Players for Network Game
            var networkSession = _networkSession; // 4/29/2010 - Cache
            if (networkSession == null)
            {
                var player1 = new Player(game, _gamerInfoHost.PlayerSide, _gamerInfoHost.PlayerColor, null, 0);
                TemporalWars3DEngine.AddPlayerAtIndex(player1, 0); // 6/15/2010

                // 10/20/2009 - Since SP game, then 2nd player is AI player.
                var player2 = new Player(game, _gamerInfoHost.PlayerSide, _gamerInfoHost.PlayerColor, null, 1);
                TemporalWars3DEngine.AddPlayerAtIndex(player2, 1); // 6/15/2010
            }
            else
            {
                var player1 = new Player(game, _gamerInfoHost.PlayerSide, _gamerInfoHost.PlayerColor, networkSession, 0);
                TemporalWars3DEngine.AddPlayerAtIndex(player1, 0); // 6/15/2010

                var player2 = new Player(game, _gamerInfoClient.PlayerSide, _gamerInfoClient.PlayerColor, networkSession, 1);
                TemporalWars3DEngine.AddPlayerAtIndex(player2, 1); // 6/15/2010

                // Iterate through localPlayers, and set 'ThisPlayer' to proper number, depending if
                // Host or Client.
                var count = networkSession.LocalGamers.Count; // 4/29/2010
                for (var i = 0; i < count; i++)
                {
                    // 11/25/2009 - Cache
                    var localGamer = networkSession.LocalGamers[i];
                    if (localGamer == null) continue;

                    if (localGamer.IsHost)
                        TemporalWars3DEngine.SThisPlayer = 0; // Host 
                    else
                    {
                        TemporalWars3DEngine.SThisPlayer = 1; // Client 1

                        // 9/8/2008
                        // DEBUG TESTING ONLY
                        // Set SimulatedLatency and SimulatedPacketLoss properties.
                        networkSession.SimulatedLatency = TimeSpan.FromMilliseconds(0);
                        networkSession.SimulatedPacketLoss = 0.0f; // % loss
                    }
                }

                // Create a Player Instance for each Network Gamer.
                var count1 = networkSession.AllGamers.Count; // 4/29/2010
                for (var i = 0; i < count1; i++)
                {
                    // 4/29/2010 - Cache
                    var networkGamer = networkSession.AllGamers[i];
                    if (networkGamer == null) continue;

                    // 6/15/2010 - Updated to use new GetPlayer method.
                    Player player;
                    if (!TemporalWars3DEngine.GetPlayer(i, out player))
                        break;

                    networkGamer.Tag = player;
                }
            } // End If NetworkSession

            // 1/6/2010 - Invoke the PlayerCreated Event.
            if (PlayerInstancesCreated != null) PlayerInstancesCreated(this, EventArgs.Empty);
        }

        // 12/2/2008
        /// <summary>
        /// If networkGame, send the <see cref="NetworkCommands.IsReady"/> Enum to other computer, and waits until
        /// both <see cref="NetworkCommands.IsReady"/> Enums are true.
        /// </summary>
        private void NetworkGameIsReadyCheck()
        {
            // 12/2/2008 - If NetworkGame, then we must wait until both sides are fully loaded 
            //             before starting game!
            var networkSession = _networkSession; // 4/29/2010
            if (networkSession == null) return;

            // Send Ready command to other computer
            if (networkSession.IsHost)
            {
                // 6/29/2009 - Send IsReady to Client                                     
                RTSCommIsReady isReady;
                PoolManager.GetNode(out isReady);

                isReady.Clear();
                isReady.NetworkCommand = NetworkCommands.IsReady;
                isReady.IsReadyToStart = true;
                NetworkGameComponent.AddCommandsForClientG(isReady); // 12/2/2008 - Updated to 'ReliableInOrder' queue.

                // Set our flag
                ServerIsReady = true;
            }
            else // Is client
            {
                // Send IsReady to Server
                RTSCommIsReady isReady;
                PoolManager.GetNode(out isReady);                   

                isReady.Clear();
                isReady.NetworkCommand = NetworkCommands.IsReady;
                isReady.IsReadyToStart = true;
                NetworkGameComponent.AddCommandsForServerG(isReady);// 12/2/2008 - Updated to 'ReliableInOrder' queue.

                // Set our flag
                ClientIsReady = true;
            }

            // Wait until both 'IsReady' flags are true.
            LoadingScreen.LoadingMessage = "Waiting for other player...";
            _gameTime = new GameTime();
            while (!ClientIsReady || !ServerIsReady)
            {
                // 11/5/2009 - Make sure 'SendPacket' is TRUE!
                NetworkGameComponent.SendPacketThisFrame = true;

                // Pump Network Session
                _networkGameComponent.Update(_gameTime);  
                  
                // 8/10/2009 - Pump Process Network Thread
                NetworkGameComponent.PumpUpdateThreads();
                NetworkGameComponent.WaitForThreadsToFinishCurrentFrame();

                // Sleep for few ms
                Thread.Sleep(10);
            }

            // 1/19/2009 - Reset GameTime clocks to zero
            TemporalWars3DEngine.GameInstance.ResetElapsedTime();

            LoadingScreen.LoadingMessage = null;
            _gameTime = null;

            // 12/10/2008 - Start NetworkGameSyncer
            NetworkGameSyncer.Enabled = true;

            // 1/26/2009 - Update Minimap
            if (_miniMap != null) _miniMap.DoUpdateMiniMap = true;
        }

       
        #endregion

        /// <summary>
        /// Creates the <see cref="SceneItem"/> collection for the <see cref="TerrainScreen"/>,
        /// adds itself to the game services, and then calls the <see cref="TerrainInitialize"/>.
        /// </summary>
        /// <param name="contentManager"> </param>
        public override void LoadContent(ContentManager contentManager)
        {
            // 8/13/2008 - Ben: Add Sceneitem
            SceneCollection = new SceneItem(ScreenManager.Game);
            
            // 8/13/2008 - Add TerrainScreen into Services.
            ScreenManager.Game.Services.AddService(typeof(ITerrainScreen), this);

            // 8/13/2008
            TerrainInitialize(ScreenManager.Game);
           
        }

        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            // 1/22/2011 - Trigger Event
            if (UnLoading != null)
                UnLoading(this, EventArgs.Empty);

            // 4/21/2011 - Retrieve Cursor interface and hide cursor.
            var cursor = (Cursor)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(ICursor));
            cursor.Visible = false;

            // 2/27/2009 - Set Target FPS back to 16ms (60 FPS)
            var gameInstance = TemporalWars3DEngine.GameInstance; // 1/3/2010
            gameInstance.TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 16);

            // 3/3/2009 - Remove from Game Services
            gameInstance.Services.RemoveService(typeof(TerrainScreen));
            // Remove Service Intefaces            
            ScreenManager.Game.Services.RemoveService(typeof(ITerrainScreen));

            // 5/5/2009 - Stop all sounds from playing
            AudioManager.StopAll();

            // 1/8/2010 - Dispose of WaterManager content.
            var waterManager = (IWaterManager) gameInstance.Services.GetService(typeof (IWaterManager));
            if (waterManager != null) waterManager.Dispose(false);

            // Stop components from drawing
            if (_shadowMap != null)
                _shadowMap.IsVisible = false;
            if (_ifdTileManager != null)
                _ifdTileManager.IsVisible = false;
            if (_fogOfWar != null)
                _fogOfWar.IsVisible = false;
            if (_miniMap != null)
                _miniMap.IsVisible = false;

            // 11/13/2008 - Clear IFD Tiles
            if (_ifdTileManager != null)
                IFDTileManager.ClearIFDTiles(false);

            // 11/24/2008 - Clear all StatusBarItems
            var statusBar = (IStatusBar) gameInstance.Services.GetService(typeof (IStatusBar)); // 1/3/2010
            if (statusBar != null) statusBar.ClearAllStatusBarItems();

            // 1/7/2010 - Clear AStar internal arrays
            var astarManager = (IAStarManager) gameInstance.Services.GetService(typeof (IAStarManager));
            if (astarManager != null) astarManager.ClearForLevelReload();

            // 1/15/2010 - Stop GameLevelManager from updating, since exiting level.
            var gameLevelManager = (IGameLevelManager)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(IGameLevelManager));
            if (gameLevelManager != null) gameLevelManager.Enabled = false;

            // 6/06/2012 - Dispose of resources and collections within GameLevelManager class. (Scripting Purposes)
            if (gameLevelManager != null) gameLevelManager.UnloadContent();

            // 6/29/2012 - Clear Camera Cinematics Dictionary
            CameraCinematics.CinematicSplinesCompleted.Clear();

            // 6/6/2012 - Dispose of resources
            TerrainDirectionalIconManager.UnloadContent();
            
            // CAll Dispose
            Dispose();

        } 

        #region Update and Draw

        /// <summary>
        /// Updates the state of the <see cref="TerrainScreen"/>, by calling the Update method
        /// on the internal <see cref="SceneCollection"/> collection, which causes updates for all <see cref="SceneItem"/>
        /// contain in collection.  Also, if Single-player game, will call the <see cref="Player.Update"/> for the AI player.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="otherScreenHasFocus">Another <see cref="GameScreen"/> has focus?</param>
        /// <param name="coveredByOtherScreen">Is covered by another <see cref="GameScreen"/>?</param>  
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
#if DEBUG
            // 11/7/2008 
            StopWatchTimers.StartStopWatchInstance(StopWatchName.TerrainUpdate); //"TerrainUpdate"
#endif
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // 5/29/2012 - Enter Pause check here.
            if (!TemporalWars3DEngine.GamePaused)
            {
                // 4/29/2010 - Refactored core update code to new STATIC method.
                UpdateSceneCollection(this, gameTime);
            }

#if DEBUG
            // 11/7/2008
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.TerrainUpdate);//"TerrainUpdate"
#endif
        }

        // 4/29/2010
        /// <summary>
        /// Helper method, which iterates the <see cref="SceneCollection"/> of <see cref="SceneItem"/>, calling
        /// Update method for each.
        /// </summary>
        /// <param name="terrainScreen">This instance of the <see cref="TerrainScreen"/></param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private static void UpdateSceneCollection(TerrainScreen terrainScreen, GameTime gameTime)
        {
            // 6/5/2009
            if (IsRaining) ParticlesManager.UpdateRain();

            // 3/1/2011
            if (IsSnowing) ParticlesManager.UpdateSnow();
           
            // 8/14/2008
            terrainScreen._gameTime = gameTime;

            // 11/21/2009 - In EditMode?
            var editMode = (TerrainShapeInterface != null) && TerrainShape.TerrainIsIn == TerrainIsIn.EditMode;

            // 11/21/2009: Updated to still be TRUE, when in EditMode, since the 'IsActive' will be FALSE; this
            //             is because the presence of the windows 'Form', makes the game window NOT 'IsActive'.
            if (terrainScreen.IsActive || editMode)
            {
                // 8/31/2008 - Initialize Minimap
                if (!terrainScreen._initMiniMap)
                {
                    // Tell MiniMap to Initialize itself.
                    if (_miniMap != null) _miniMap.DoInitMiniMap = true;
                    terrainScreen._initMiniMap = true;
                }

                // Ben: Update the Scene                
                var tmpTime = gameTime.TotalGameTime;
                var tmpElapsed = gameTime.ElapsedGameTime;
                SceneCollection.Update(gameTime, ref tmpTime, ref tmpElapsed, false);                

                // 1/29/2008: Do RemoveAll call from here, in case ScenaryItem needs to be removed.
                //      Note: Items like Tanks are SelectableItems, which are removed in the Player classes 'Update'.
                if (DoRemoveAllCheck)
                {
                    SceneCollection.RemoveAll(IsDeleted);
                    DoRemoveAllCheck = false;
                }

                // 9/3/2008 - If Single Player Game               
                if (terrainScreen._networkSession == null)
                {
                    // 1/14/2011 - Fixed invisible sceneItems
                    for (var i = 0; i < TemporalWars3DEngine._maxAllowablePlayers; i++)
                    {
                        // 6/15/2010 - Updated to use new GetPlayer method.
                        Player player;
                        TemporalWars3DEngine.GetPlayer(i, out player);

                        if (player != null) player.Update(gameTime); // Call Player Class Update
                    }
                   
                }
            }

            // If we are in a network game, check if we should return to the lobby.
            if ((terrainScreen._networkSession == null) || terrainScreen.IsExiting) return;

            if (terrainScreen._networkSession.SessionState == NetworkSessionState.Lobby)
            {
                LoadingScreen.Load(terrainScreen.ScreenManager, true,
                                   new BackgroundScreen(),
                                   new LobbyScreen(terrainScreen._networkSession));
            }
        }

        // 1/1/2009
        /// <summary>
        /// Predicate used in the RemoveAll method, of the List, which removes any items
        /// with the 'Delete' set to TRUE.
        /// </summary>
        /// <param name="item"><see cref="SceneItem"/> instance</param>
        /// <returns>true/false of result.</returns>
        private static bool IsDeleted(SceneItem item)
        {
            return item.Delete;
        }

        /// <summary>
        /// Lets the <see cref="Game"/> respond to <see cref="Player"/> input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="input"><see cref="InputState"/> instance</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
        public override void DoHandleInput(GameTime gameTime, InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input", @"The parameter 'Input' was null.");

            // 4/29/2010 - Refactored out code into new STATIC method.
            DoHandleInput(this, input, gameTime);
        }

        // 4/29/2010
        /// <summary>
        /// Helper method, which handles the game input check, by checking for 'PauseGame' and 'LeftCtrlS', and
        /// then calls the <see cref="HandleInput.UpdateInput"/>.
        /// </summary>
        /// <param name="terrainScreen">This instance of <see cref="TerrainScreen"/></param>
        /// <param name="input"><see cref="InputState"/> instance</param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private static void DoHandleInput(TerrainScreen terrainScreen, InputState input, GameTime gameTime)
        {
            // 5/29/2012 - Check for Developer pause for pics
            if (input.PauseGameForPic)
            {
                TemporalWars3DEngine.GamePaused = !TemporalWars3DEngine.GamePaused;
                return;
            }

            if (input.PauseGame)
            {
                // 6/29/2012 - Refactored pause code to new method in ScreenManager
                if (terrainScreen.ScreenManager.DisplayPauseScreen(terrainScreen._networkSession)) return;
            }

// 2/2/2010 - Following is only for PC development; not for Xbox.
#if !XBOX360
            // 9/9/2008 - Only HandleInput when GameConsole Closed.
            if (terrainScreen._gameConsole.ConsoleState != ConsoleState.Closed)
                return;

            // 8/18/2008 - Activates the Save Maps Tool
            if (input.LeftCtlS)
            {
                if (terrainScreen._saveMapsTool == null || !terrainScreen._saveMapsTool.Visible)
                {
                    terrainScreen._saveMapsTool = new SaveMapsTool(terrainScreen) { Visible = true };
                }
            }

            // 6/2/2012: Moved to TerrainEditRoutines class.
            // If (TerrainIsIn == EditMode), then we check all items in the Screen Scene 
            // for pickable, since in EditMode, we want the ability to be able to delete
            // any SceneItemOwner placed on the Terrain.
            /*if (TerrainShape.TerrainIsIn == TerrainIsIn.EditMode)
            {
                // Make sure Properties Tool does not have mouse in control
                if (TerrainEditRoutines.ToolInUse == ToolType.PropertiesTool
                    || TerrainEditRoutines.ToolInUse == ToolType.ItemTool)
                {
                    // Only call when mouse not in control
                    if (TerrainEditRoutines.PropertiesTools != null &&
                        !TerrainEditRoutines.PropertiesTools.IsMouseInControl()) 
                        EditModePicking(input);

                    // 4/3/2011
                    // Only call when mouse not in control
                    if (!TerrainItemToolRoutines.IsMouseInControl())
                        EditModePicking(input);
                }

                // 5/5/2008 - Delete ScenaryItems if in EditMode               
                if (input.Delete)
                {
                    EditModeDeleting();
                }
            }*/

           
#endif

            // 4/28/2009 - Call HandleGameInput for input handling.
            HandleInput.UpdateInput(gameTime, input);
        }

        // 10/27/2009
        /// <summary>
        /// Draws the <see cref="SkyDome"/>, using the default <see cref="Camera.View"/> matrix.
        /// </summary>
        public static void DrawSkyBox()
        {
            var cameraView = Camera.View;
            var cameraPosition = Camera.CameraPosition;
            if (_skyDome != null) _skyDome.DrawSkyDome(ref cameraView, ref cameraPosition);  
        }

        // 12/14/2009
        /// <summary>
        /// Draws the <see cref="SkyDome"/>, using the given <paramref name="viewMatrix"/> matrix.
        /// </summary>
        /// <param name="viewMatrix">View <see cref="Matrix"/> to use</param>
        /// <param name="cameraPosition"><see cref="Camera"/> position</param>
        public static void DrawSkyBox(ref Matrix viewMatrix, ref Vector3 cameraPosition)
        {
            if (_skyDome != null) _skyDome.DrawSkyDome(ref viewMatrix, ref cameraPosition); 
        }
        
        /// <summary>
        /// Draws the <see cref="TWEngine.Terrain"/> ONLY.
        /// </summary>    
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>    
        public override void Draw3D(GameTime gameTime)
        {
            // 12/2/2013 - Check RenderType here.
            switch (RenderingType)
            {
                case RenderingType.DeferredRendering:
                case RenderingType.NormalRendering:
                    DoDraw3D(gameTime);
                    break;
                case RenderingType.NormalRenderingWithPostProcessEffects:
                    // 12/2/2013 - Draws in 2 passes to include the PostProcessing Effects.
                    DoDraw3DWithPostProcessingEffects(gameTime);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // 12/2/2013
        /// <summary>
        /// Draws the <see cref="TWEngine.Terrain"/> using 1-pass without the Post-Processing Effects.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private void DoDraw3D(GameTime gameTime)
        {
            // XNA 4.0 updates - Set for regular 'Solid' draw mode.
            var graphicsDevice = TemporalWars3DEngine.GameInstance.GraphicsDevice;
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

#if DEBUG
            StopWatchTimers.StartStopWatchInstance(StopWatchName.TerrainDraw);
#endif

            // Set RenderTarget for SkyBox
            graphicsDevice.SetRenderTarget(_terrainDrawRt);

            // The following CLEAR command is a MUST; otherwise, the renderTarget color will be Purple!
            graphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0); // 

            // 3/13/2009 - Draw Terrain
            TerrainShape.Render(gameTime, false);

            // 5/16/2012 - Draw SkyBox
            if (UseSkyBox) DrawSkyBox();

            // Draw Other Items into 1st RT.
            Draw3DSelectables(gameTime);
            Draw3DSceneryItems(gameTime); // 8/1/2009
            Draw3DAlpha(gameTime); // 3/19/2009 - Draw AlphaItems Now, like Trees for example.

#if !XBOX360
            // 5/28/2012 - Render Collision spheres for debug purposes
            RenderDebugArtworkScenaryItems(gameTime);
            RenderDebugArtworkPlayableItems(gameTime);
#endif
            // Resolve texture
            graphicsDevice.SetRenderTargets(null);
            ScreenManager.DrawRenderTargetTexture(graphicsDevice, _terrainDrawRt, 1.0f, true, BlendState.Opaque);

            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.BlendState = BlendState.Opaque;

#if DEBUG
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.TerrainDraw);
#endif
        }

        // 12/2/2013
        /// <summary>
        /// Draws the <see cref="TWEngine.Terrain"/> using 2-passes to include the Post-Processing Effects pass.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private void DoDraw3DWithPostProcessingEffects(GameTime gameTime)
        {
            // XNA 4.0 updates - Set for regular 'Solid' draw mode.
            var graphicsDevice = TemporalWars3DEngine.GameInstance.GraphicsDevice;
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

#if DEBUG
            StopWatchTimers.StartStopWatchInstance(StopWatchName.TerrainDraw);
#endif

            // Set RenderTarget for SkyBox
            graphicsDevice.SetRenderTarget(_terrainDrawRt);

            // The following CLEAR command is a MUST; otherwise, the renderTarget color will be Purple!
            graphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.White, 1.0f, 0); // 

            // 3/13/2009 - Draw Terrain
            TerrainShape.Render(gameTime, false);

            // Resolve texture
            graphicsDevice.SetRenderTargets(null);

            // Set RenderTargets
            graphicsDevice.SetRenderTargets(_terrainDraw2Rt);

            graphicsDevice.Clear(Color.Black);

            // 5/16/2012 - Draw SkyBox
            if (UseSkyBox) DrawSkyBox();

            // 3/13/2009 - Draw Terrain with additional effects on.
            TerrainShape.Render(gameTime, true);

            // Resolve texture
            graphicsDevice.SetRenderTargets(null);

            // Set RenderTargets
            graphicsDevice.SetRenderTargets(_terrainDraw3Rt);

            // The following CLEAR command is a MUST; otherwise, the renderTarget color will be Purple!
            graphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0); // 

            // Note: This useless Render call is required for the XBOX-360!  Otherwise, it will throw a SetData error.
            TerrainShape.Render(gameTime, false);

            // Draw Textures
            // Draw the combine texture to screen
            ScreenManager.DrawRenderTargetTexture(graphicsDevice, _terrainDrawRt, 1.0f, false, BlendState.Opaque);
                // 9/22/2010 - XNA 4.0 Updates - Pass in blendState.

            //BlurManager.RenderScreenQuad(graphicsDevice, BlurTechnique.ColorTexture, _terrainDraw2Rt, new Vector4(1.0f));
            ScreenManager.DrawRenderTargetTexture(graphicsDevice, _terrainDraw2Rt, 1.0f, true, BlendStateMultiply);

#if !XBOX360
            // 5/28/2012 - Render Collision spheres for debug purposes
            RenderDebugArtworkScenaryItems(gameTime);
            RenderDebugArtworkPlayableItems(gameTime);
#endif

            // Draw Other Items into 1st RT.
            Draw3DSelectables(gameTime);
            Draw3DSceneryItems(gameTime); // 8/1/2009
            Draw3DAlpha(gameTime); // 3/19/2009 - Draw AlphaItems Now, like Trees for example.

            // Resolve texture
            graphicsDevice.SetRenderTargets(null);
            ScreenManager.DrawRenderTargetTexture(graphicsDevice, _terrainDraw3Rt, 1.0f, true, BlendState.Opaque);
                // _terrainDraw3Rt

            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.BlendState = BlendState.Opaque;

#if DEBUG
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.TerrainDraw);
#endif

        }

        // /5/28/2012
        /// <summary>
        /// Helper method which iterates all the playable <see cref="TWEngine.SceneItems"/> and draws
        /// the debug artwork, like Collision spheres.
        /// </summary>
        /// <param name="gameTime"></param>
        [Conditional("WINDOWS")]
        private void RenderDebugArtworkPlayableItems(GameTime gameTime)
        {
            if (!DebugShapeRenderer.IsVisible)
                return;

            if (!DebugShapeRenderer.DrawCollisionSpheresForPlayableItems)
                return;

            // iterate players and their selectable items
            for (var i = 0; i < TemporalWars3DEngine._maxAllowablePlayers; i++)
            {
                Player player;
                if (!TemporalWars3DEngine.GetPlayer(i, out player))
                {
                    continue;
                }

                if (player == null)
                {
                    continue;
                }

                ReadOnlyCollection<SceneItemWithPick> selectableItems;
                Player.GetSelectableItems(player, out selectableItems);

                if (selectableItems == null)
                {
                    continue;
                }

                var count = selectableItems.Count;
                if (count == 0)
                {
                    continue;
                }

                // iterate the playable items collection
                for (var j = 0; j < count; j++)
                {
                    var selectableItem = selectableItems[j];
                    if (selectableItem == null)
                    {
                        continue;
                    }

                    // render debug art
                    selectableItem.RenderDebug(gameTime);
                }
            }
        }

        // 5/28/2012
        /// <summary>
        /// Helper method which iterates all the <see cref="ScenaryItemScene"/> and draws
        /// the debug artwork, like Collision spheres.
        /// </summary>
        [Conditional("WINDOWS")]
        private void RenderDebugArtworkScenaryItems(GameTime gameTime)
        {
            if (!DebugShapeRenderer.IsVisible)
                return;

            if (!DebugShapeRenderer.DrawCollisionSpheresForScenaryItems)
                return;

            if (TerrainShapeInterface.ScenaryItems == null)
            {
                return;
            }

            if (TerrainShapeInterface.ScenaryItems.Count == 0)
            {
                return;
            }

            // cache
            var scenaryItems = TerrainShapeInterface.ScenaryItems;
            var count = scenaryItems.Count;

            // iterate scenary items
            for (int i = 0; i < count; i++)
            {
                var scenaryItem = scenaryItems[i];
                if (scenaryItem == null)
                {
                    continue;
                }

                // render debug art
                scenaryItem.RenderDebug(gameTime);
            }
        }

        /// <summary>
        /// Draws the <see cref="ScenaryItemScene"/> items.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public override void Draw3DSceneryItems(GameTime gameTime)
        {
            // Render the ScenaryItems, like Rocks for example.
            SceneCollection.Render();

            // 5/24/2010 - Render the Scenary items
            InstancedItem.DrawInstanceItems_ScenaryDraw(gameTime);
        }

        /// <summary>
        /// Draws the <see cref="Player._selectableItems"/>.
        /// </summary>   
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>     
        public override void Draw3DSelectables(GameTime gameTime)
        {
            // 4/29/2010 - Refactored core code to STATIC method.
            DoDraw3DSelectables(this, gameTime);
        }

        // 4/29/2010
        /// <summary>
        /// Helper method, which draws the <see cref="Player.DrawSelectionBoxes"/> and renders all
        /// the <see cref="Player"/> selectable <see cref="SceneItem"/>.
        /// </summary>
        /// <param name="terrainScreen">This instance of <see cref="TerrainScreen"/></param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private static void DoDraw3DSelectables(TerrainScreen terrainScreen, GameTime gameTime)
        {

#if DEBUG
            // 3/28/2009
            StopWatchTimers.StartStopWatchInstance(StopWatchName.TerrainDrawSelectables);//"TerrainDrawSelectables"

            // 3/28/2009
            StopWatchTimers.StartStopWatchInstance(StopWatchName.TerrainDrawSelectablesA);//"TerrainDrawSelectables_a"
#endif

            // Is Single Player Game?
            var networkSession = terrainScreen._networkSession; // 4/20/2010
            if (networkSession == null)
            {
                // 9/3/2008 - Render the SelectableItems in Player Class

                // 6/15/2010 - Updated to use new GetPlayer method.
                Player player;
                TemporalWars3DEngine.GetPlayer(0, out player);

                if (player != null) player.DrawSelectionBoxes();
            }
                // Else Network Session
            else
            {
                // 11/25/2009
                var allGamers = networkSession.AllGamers;
                if (allGamers != null)
                {
                    // For each person in the session...
                    // Render the SelectableItems
                    var count = allGamers.Count; // 11/25/2009
                    for (var i = 0; i < count; i++)
                    {
                        // 11/25/2009 - Cache
                        var gamer = allGamers[i];
                        if (gamer == null) continue;

                        var player = gamer.Tag as Player;

                        if (player != null) player.DrawSelectionBoxes();
                    }
                }

#if DEBUG
                // 9/8/2008
                // DEBUG Purposes: Write Bytes per second sent across Wire 

                // 8/7/2009 - Directly update the StringBuilder to reduce string garbage.
                terrainScreen._screenText1.SbDrawText.Length = 0;
                terrainScreen._screenText2.SbDrawText.Length = 0;
                terrainScreen._screenText1.SbDrawText.Append("Bytes Received: ");
                terrainScreen._screenText1.SbDrawText.Append(networkSession.BytesPerSecondReceived);
                terrainScreen._screenText2.SbDrawText.Append("Bytes Sent: ");
                terrainScreen._screenText2.SbDrawText.Append(networkSession.BytesPerSecondSent);
#endif

            }

#if DEBUG
            // 3/28/2009
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.TerrainDrawSelectablesA);//"TerrainDrawSelectables_a"

            // 3/28/2009
            StopWatchTimers.StartStopWatchInstance(StopWatchName.TerrainDrawSelectablesB);//"TerrainDrawSelectables_b"
#endif

            // 11/24/2008 - Render the InstanceItems Selectables
            InstancedItem.DrawInstanceItems_SelectablesDraw(gameTime);

#if DEBUG
            // 3/28/2009
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.TerrainDrawSelectablesB);//"TerrainDrawSelectables_b"
#endif

            // If the game is transitioning on or off, fade it out to black.
            if (terrainScreen.TransitionPosition > 0)
                terrainScreen.ScreenManager.FadeBackBufferToBlack(255 - terrainScreen.TransitionAlpha);

#if DEBUG
            // 3/28/2009
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.TerrainDrawSelectables);//"TerrainDrawSelectables"
#endif
        }

        // 2/18/2009
        /// <summary>
        /// Draws the <see cref="InstancedItem"/> which are 2-pass Alphamap items.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public override void Draw3DAlpha(GameTime gameTime)
        {
#if DEBUG
            // 3/28/2009
            StopWatchTimers.StartStopWatchInstance(StopWatchName.TerrainDrawAlpha);//"TerrainDrawAlpha"
#endif

            InstancedItem.DrawInstanceItems_AlphaMapDraw(gameTime);

#if DEBUG
            // 3/28/2009
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.TerrainDrawAlpha);//"TerrainDrawAlpha"
#endif

        }

        // 2/19/2009
        /// <summary>
        /// Draws the <see cref="InstancedItem"/> which have Illumination maps.
        /// </summary>
        ///<param name="gameTime"><see cref="GameTime"/> instance</param>
        public override void Draw3DIllumination(GameTime gameTime)
        {
            InstancedItem.DrawInstanceItems_IllumMapDraw(gameTime);
        }

        #endregion

        // 8/12/2008
        /// <summary>
        /// Loads a new game level map, using the given <paramref name="mapName"/>, and
        /// clears out all <see cref="SceneItem"/>,= and <see cref="ScenaryItemScene"/>.
        /// </summary>
        /// <param name="mapName">MapName to Load</param>
        public void LoadTerrainMap(string mapName)
        {
            // 8/12/2008 - Clear out all Scene items except TerrainScene
            // 8/26/2008: Updated to For-Loop, rather than ForEach.
            var sceneCollection = SceneCollection; // 4/29/2010
            var count = sceneCollection.Count; // 11/11/09
            for (var i = 0; i < count; i++)
            {
                // 11/11/09 - cache
                var sceneItem = sceneCollection[i];
                if (sceneItem == null) continue; // 11/11/09 - null check

                if (!(sceneItem.ShapeItem is TerrainShape))
                    sceneItem.Delete = true;
            }

            var tmpTime = _gameTime.TotalGameTime; 
            var tmpElapsed = _gameTime.ElapsedGameTime;
            sceneCollection.Update(_gameTime, ref tmpTime, ref tmpElapsed, false);

            // Load the Terrain Data now.
            TerrainStorageRoutines.LoadTerrainData(mapName, TerrainMapGameType);
           
        }

        #region Terrain EditMode Methods



        // 7/2/2009
        /// <summary>
        /// Deletes a specific <see cref="ItemType"/> from terrain.
        /// </summary>
        /// <param name="sceneItemType"><see cref="ItemType"/> to delete</param>
        public void EditModeDeleteSpecificSceneItem(ItemType sceneItemType)
        {
            // 11/25/2009 - Cache
            var sceneItems = SceneCollection;
            if (sceneItems == null) return;

            // iterate list, searching for the given 'ItemType'
            var count = sceneItems.Count; // 11/21/09
            for (var i = 0; i < count; i++)
            {
                // 11/21/2009 - Cache
                var sceneItem = sceneItems[i];
                if (sceneItem == null) continue;

                if (sceneItem.ShapeItem.ItemType != sceneItemType) continue;

                // 7/2/2009
                DeleteSceneItemFromList(i, sceneItem.ShapeItem.ItemInstanceKey);
            }
        }

        // 7/2/2009
        /// <summary>
        /// Deletes all instances of a specific <see cref="ScenaryItemScene"/> from the terrain.  This is
        /// called exclusively by the ItemTool form 'Undo' operation.
        /// </summary>
        /// <param name="scenaryItemScene"><see cref="ScenaryItemScene"/> to delete</param>
        public void EditModeDeleteSpecificScenarySceneItem(ScenaryItemScene scenaryItemScene)
        {
            // 11/25/2009 - Cache
            var sceneItems = SceneCollection;
            if (sceneItems == null) return;

            // iterate list, searching for the given 'ScenarySceneItem'
            var count = sceneItems.Count; // 11/21/09
            for (var i = 0; i < count; i++)
            {
                // 11/21/2009 - Cache
                var sceneItem = sceneItems[i];
                if (sceneItem == null) continue;

                if (sceneItem != scenaryItemScene) continue;

                // 7/2/2009
                DeleteSceneItemFromList(i, sceneItem.ShapeItem.ItemInstanceKey);
            }
        }

        // 3/3/2009
        /// <summary>
        /// Deletes all <see cref="SceneItem"/> from terrain.
        /// </summary>
        public void EditModeDeleteAllSceneItems()
        {
            // 11/25/2009 - Cache
            var sceneItems = SceneCollection;
            if (sceneItems == null) return;

            // 1st - Iterate through Scene and remove
            var count = sceneItems.Count; // 11/21/09
            for (var i = 0; i < count; i++)
            {
                // 11/21/2009 - Cache
                var sceneItem = sceneItems[i];
                if (sceneItem == null) continue;

                // Make sure we are only checking ScenaryItemShape Classes
                var instancedItemPick = (sceneItem.ShapeItem as IInstancedItem); // Extract InstancedItem for quick access
                if (instancedItemPick == null) continue;

                // 7/2/2009
                DeleteSceneItemFromList(i, instancedItemPick.ItemInstanceKey);
            } // End For Scene Loop
        }

        // 7/2/2009
        /// <summary>
        /// Helper Method, used to remove a <see cref="SceneItem"/> from the internal list.
        /// </summary>
        /// <param name="index">Index to remove at</param>
        /// <param name="itemInstanceKey">Specific instance unique key</param>
        internal static void DeleteSceneItemFromList(int index, int itemInstanceKey)
        {
            // TODO: Fix Removing A* Position for ScenaryItem.
            // 5/13/2008 - Remove Cost From A* Graph
            //((SceneItemWithPick)Scene[loop1]).aStarItem.RemoveCostAtCurrentPosition();
            // DEBUG: Update the PathNodes Array
            TerrainShape.PopulatePathNodesArray();

            // 9/19/2008 - Remove from ScenaryItems Array
            _matchItemKey = itemInstanceKey;
            if (TerrainShapeInterface.ScenaryItems != null) 
                TerrainShapeInterface.ScenaryItems.RemoveAll(HasItemInstanceKey);

            // 11/11/2008 - Set 'Delete' to True, which automatically takes care of InstanceItems                     
            if (SceneCollection != null) SceneCollection[index].Delete = true;

            // 9/24/208 - Remove from Selectable Items - Applies to Items, like Buildings.
            
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out player);

            if (player == null) return;
            
            // 11/25/2009; 6/15/2010 - Updated to use new RemoveAll method.
            Player.RemoveAllSelectableItemsByItemInstanceKey(player, itemInstanceKey);
           
        }

        // 9/19/2008 -
        private static int _matchItemKey;

        /// <summary>
        /// Predicate Delegate for the RemoveAll() call, which returns true/false for
        /// any scenaryItem.InstanceKey == _matchItemKey.
        /// </summary>
        /// <param name="sceneItem"><see cref="SceneItem"/> to check</param>
        /// <returns>true/false of result</returns>
        private static bool HasItemInstanceKey(SceneItem sceneItem)
        {
            if (sceneItem == null) return false;

            return (sceneItem.ShapeItem as IInstancedItem).ItemInstanceKey == _matchItemKey;
        }

        #endregion

#if DEBUG

        // 5/16/2009
        /// <summary>
        /// Turns off all Sounds playing in <see cref="SoundManager"/>.  This method call is ONLY for the GameConsole access, since Python 
        /// can't seem to access STATIC classes directly.
        /// </summary>
        public void SoundsOff()
        {
            AudioManager.StopAll();
        }

#endif

        #region Dispose
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this final dispose?</param>
        private void Dispose(bool disposing)
        {
            if (!disposing) return;

#if !XBOX
            if (_saveMapsTool != null)
                _saveMapsTool.Dispose();
#endif

            // 11/18/2009 - Clear some arrays for level reloads.
            InstancedItem.ClearForLevelReload();

            // 1/5/2010 - Set Skydome to Visible off.
            if (_skyDome != null)
            {
                _skyDome.Visible = false;
                // 1/6/2010 - Call Skydome's Dispose, to get UnloadContent to fire.
                _skyDome.Dispose();
            }

            // 2/27/2009
            // Clear out ExplosionManager array
            //if (TemporalWars3DEngine.ExplosionManager != null) TemporalWars3DEngine.ExplosionManager.ClearExplosionItems();

            // 7/17/2009 - Clear out ForceBehaviors
            ForceBehaviorsManager.ClearSteeringBehaviorArrays();
            // 7/17/2009 - Clear out AIThread Arrays
            AIManager.ClearAIArrays();

            // Remove components     
            ScreenManager.Game.Components.Remove(_instancedItem);               
               

#if !XBOX360
            if (_gameConsole != null)
                _gameConsole.DisposeInterfaceReferences();
#endif

           
            _screenText1.Dispose();

            // 
            // 1/5/2010 - Note: Up to this point, no InternalDriverError will be thrown in the SpriteBatch.
            //          - Note: Discovered, the error is coming from the call to 'Player' dispose!
            Player[] players;
            TemporalWars3DEngine.GetPlayers(out players);

            // 1/5/2010 - Cache
            for (var i = 0; i < players.Length; i++)
            {
                // 11/18/09
                // 6/15/2010 - Updated to use new GetPlayer method.
                Player player;
                TemporalWars3DEngine.GetPlayer(i, out player);

                if (player != null)
                    player.Dispose();

                players[i] = null;
            }

            // Dispose of all Scene Items
            if (SceneCollection != null)
            {
                var count = SceneCollection.Count; // 11/18/09
                for (var j = 0; j < count; j++)
                {
                    // 11/18/09 - Cache
                    var sceneItem = SceneCollection[j];

                    var shapeWithPick = sceneItem.ShapeItem as ShapeWithPick;
                    if (shapeWithPick != null)
                    {
                        // Now Dispose of SceneItem, which should Dispose of ShapeItem internally.
                        sceneItem.Dispose(true);
                    }

                    SceneCollection[j] = null;
                }
            }
            SceneCollection = null;

            // Null Refs           
            TerrainShapeInterface = null;
            
#if !XBOX360
            _gameConsole = null;
#endif
            _networkSession = null;                               
                               
            _instancedItem = null;
               

            // Force Garbage Collection
            GC.Collect(); // This gets rid of the dead objects
            GC.WaitForPendingFinalizers(); // This waits for any finalizers to finish.
            GC.Collect(); // This releases the memory associated with the objects that were just finalized.
            // free native resources
        }

        ///<summary>
        /// Disposes of unmanaged resources.
        ///</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
