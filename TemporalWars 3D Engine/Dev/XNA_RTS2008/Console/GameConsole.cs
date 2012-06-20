#region File Description
//-----------------------------------------------------------------------------
// GameConsole.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using PerfTimersComponent.Timers;
using TWEngine.Console.Enums;
using TWEngine.IFDTiles;
using TWEngine.Interfaces;
using TWEngine.Networking;
using TWEngine.ScreenManagerC;
using TWEngine.Viewports;
using TWEngine.Terrain;

namespace TWEngine.Console
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.Console"/> namespace contains the common classes
    /// which make up the IronPython game console.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }


    /// <summary>
    /// Game console class creates an instance of the Python console internally,
    /// allowing reference to the interal classes via the console scripting window.
    /// </summary>
    public sealed class GameConsole : GameComponent, IGameConsole
    {
        // 7/15/2008 - Add IronPython Console
        private PythonConsoleComponent _gameConsole;
                
        // Terrain Interface Reference
        private ITerrainShape _terrain;
        // Terrain Screen Interface Reference
        private ITerrainScreen _terrainScreen;        
        // FogOfWar Interface Reference
        private IFogOfWar _fogOfWar;
        // MiniMap Interface Reference
        private IMinimap _miniMap;
        // Water Interface Reference
        private IWaterManager _water;
        // ShadowMap Interface Reference
        private IShadowMap _shadowMap;
        // FramePerSecond Interface Reference
        private IFPS _fps;
        // IFDTileManager Interface Reference
        private IIFDTileManager _ifd;
        // TerrainPicking Interface Reference
        private TerrainPickingRoutines _picking;  
        // 9/14/2008 - NetworkGameComponent
        private NetworkGameComponent _networkGameComponent;
        // 11/13/2008 - StopWatchTimers Performance Reference
        private StopWatchTimers _timers;    
        // 2/19/2009 - Screen Mananger Reference
        private ScreenManager _screenManager;
        // 7/6/2009 - GameViewPort Component
        private GameViewPort _gameViewPort;

        #region Properties

        /// <summary>
        /// Returns the State of the Python Console
        /// </summary>
        public ConsoleState ConsoleState
        {
            get { return _gameConsole.ConsoleState; }
        }

        /// <summary>
        /// Returns Ref to Camera Interface via script
        /// </summary>
        /*public Camera Camera
        {
            get
            {
                if (camera == null)
                    GetCameraInterfaceRef();

                return camera;

            }

        }*/

        /// <summary>
        /// Returns Ref to <see cref="IWater"/> Interface via script
        /// </summary>
        public IWaterManager Water
        {
            get 
            {
                if (_water == null)
                    GetWaterInterfaceRef();

                return _water;
            
            }
            
        }

        /// <summary>
        /// Returns Ref to <see cref="IMinimap"/> Interface via script
        /// </summary>
        public IMinimap MiniMap
        {
            get
            {
                if (_miniMap == null)
                    GetMiniMapInterfaceRef();

                return _miniMap;

            }

        }

        // 1/21/2009 - shortcut version
        /// <summary>
        /// Returns Ref to <see cref="IMinimap"/> Interface via script
        /// </summary>
// ReSharper disable InconsistentNaming
        public IMinimap MM
// ReSharper restore InconsistentNaming
        {
            get
            {
                if (_miniMap == null)
                    GetMiniMapInterfaceRef();

                return _miniMap;

            }

        }

        /// <summary>
        /// Returns Ref to <see cref="IFogOfWar"/> Interface via script
        /// </summary>
// ReSharper disable InconsistentNaming
        public IFogOfWar FOW
// ReSharper restore InconsistentNaming
        {
            get
            {
                if (_fogOfWar == null)
                    GetFogOfWarInterfaceRef();

                return _fogOfWar;

            }

        }

        /// <summary>
        /// Returns Ref to <see cref="IShadowMap"/> Interface via script
        /// </summary>
        public IShadowMap ShadowMap
        {
            get
            {
                if (_shadowMap == null)
                    GetShadowMapInterfaceRef();

                return _shadowMap;

            }
        }

        // 1/21/2009 - Shortcut version.
        /// <summary>
        /// Returns Ref to <see cref="IShadowMap"/> Interface via script
        /// </summary>
// ReSharper disable InconsistentNaming
        public IShadowMap SM
// ReSharper restore InconsistentNaming
        {
            get
            {
                if (_shadowMap == null)
                    GetShadowMapInterfaceRef();

                return _shadowMap;

            }
        }

        /// <summary>
        /// Returns Ref to <see cref="IFPS"/> Interface via Script
        /// </summary>
// ReSharper disable InconsistentNaming
        public IFPS FPS
// ReSharper restore InconsistentNaming
        {
            get
            {
                if (_fps == null)
                    GetFPSInterfaceRef();

                return _fps;

            }

        }

        /// <summary>
        /// Returns Ref to <see cref="IIFDTileManager"/> Interface via script
        /// </summary>
        public IIFDTileManager IFD
        {
            get
            {
                if (_ifd == null)
                    GetIFDInterfaceRef();

                return _ifd;

            }

        }

        /// <summary>
        /// Returns Ref to <see cref="ITerrainShape"/> Interface via script
        /// </summary>
        public ITerrainShape Terrain
        {
            get
            {
                if (_terrain == null)
                    GetTerrainInterfaceRef();

                return _terrain;

            }

        }

        // 5/16/2009
        ///<summary>
        /// Returns Ref to <see cref="ITerrainScreen"/> Interface via script.
        ///</summary>
        public ITerrainScreen TerrainScreen
        {
            get
            {
                if (_terrainScreen == null)
                    GetTerrainScreenInterfaceRef();

                return _terrainScreen;
            }

        }

        /// <summary>
        /// Returns Ref to <see cref="TerrainPickingRoutines"/> Interface via script
        /// </summary>
        public TerrainPickingRoutines Picking
        {
            get 
            {
                if (_picking == null)
                    GetPickingInterfaceRef();

                return _picking; 
            }
            
        }

        /// <summary>
        /// Returns Ref to <see cref="PerfTimersComponent.Timers.StopWatchTimers"/> Class via script
        /// </summary>
        public StopWatchTimers Timers
        {
            get 
            {
                if (_timers == null)
                    GetTimersInterfaceRef();

                return _timers; 
            }
           
        }

        // 7/6/2009 - GameViewPort
        ///<summary>
        /// Returns Ref to <see cref="GameViewPort"/> Class via script
        ///</summary>
// ReSharper disable InconsistentNaming
        public GameViewPort GVP
// ReSharper restore InconsistentNaming
        {
            get
            {
                if (_gameViewPort == null)
                    GetGameViewPortInterfaceRef();

                return _gameViewPort;
            }
        }

        // 2/19/2009
        ///<summary>
        /// Returns Ref to <see cref="ScreenManager"/> Class via script
        ///</summary>
        public ScreenManager ScreenManager
        {
            get
            {
                if (_screenManager == null)
                    GetScreenManagerInterfaceRef();

                return _screenManager;
            }

        }        


        // 9/9/2008
        /// <summary>
        /// Gets or Sets EnablePrediction, used in the
        /// <see cref="NetworkGameComponent"/>.
        /// </summary>
        public bool EnablePrediction
        {
            get
            {
                if (_networkGameComponent == null)
                    GetNetworkGameComponentInterfaceRef();

                return NetworkGameComponent.EnablePrediction;
            }
            set
            {
                if (_networkGameComponent == null)
                    GetNetworkGameComponentInterfaceRef();

                NetworkGameComponent.EnablePrediction = value;

            }

        }

        // 9/9/2008
        /// <summary>
        /// Gets or Sets EnableSmoothing, used in the
        /// <see cref="NetworkGameComponent"/>.
        /// </summary>
        public bool EnableSmoothing
        {
            get
            {
                if (_networkGameComponent == null)
                    GetNetworkGameComponentInterfaceRef();

                return NetworkGameComponent.EnableSmoothing;
            }
            set
            {
                if (_networkGameComponent == null)
                    GetNetworkGameComponentInterfaceRef();

                NetworkGameComponent.EnableSmoothing = value;

            }

        }


        #endregion

        ///<summary>
        /// Constructor for the GameConsole class, which creates the
        /// Python console internally, and adds this game instance 
        /// as Global reference.
        ///</summary>
        ///<param name="game">Game instance</param>
        public GameConsole(Game game)
            : base(game)
        {            
            // 7/15/2008 - Add IronPython Game Console as Pluggable Game Component
            _gameConsole = new PythonConsoleComponent(game, @"Fonts\ConsoleFont");
            _gameConsole.AddGlobal("rts", this);
            _gameConsole.AddGlobal("game", game);
            // 8/28/2008 - Made the call number high enough to always make it draw Last.
            _gameConsole.DrawOrder = 400; 
            game.Components.Add(_gameConsole);
           
        }

        // 8/14/2008 - Dispose of old Interface References
        //             Used when TerrainScreen was exited and restarted with
        //             new Instnaces of the classes.
        ///<summary>
        /// Disposes of older Interfaced References.
        ///</summary>
        public void DisposeInterfaceReferences()
        {
            // Dispose of Old References
            if (_terrain != null)
                _terrain = null;            
            if (_fogOfWar != null)
                _fogOfWar = null;
            if (_miniMap != null)
                _miniMap = null;
            if (_water != null)
                _water = null;
            if (_shadowMap != null)
                _shadowMap = null;
            if (_fps != null)
                _fps = null;
            if (_ifd != null)
                _ifd = null;
            if (_picking != null)
                _picking = null;
            
        }

        // 8/7/2008
        /// <summary>
        /// Can turn off Several Terrain Effects at once.
        /// </summary>
        public void EffectsOff()
        {
            ShadowMap.IsVisible = false;
            Water.IsVisible = false;
            FOW.IsVisible = false;
        }

        // 8/7/2008
        /// <summary>
        /// Can turn On Several Terrain Effects at once.
        /// </summary>
        public void EffectsOn()
        {
            ShadowMap.IsVisible = true;
            Water.IsVisible = true;
            FOW.IsVisible = true;
        }
       
        /// <summary>
        /// Get a Reference to <see cref="IMinimap"/> Interface using GameServices.
        /// </summary>
        private void GetMiniMapInterfaceRef()
        {
            _miniMap = (IMinimap)Game.Services.GetService(typeof(IMinimap));
        }

        /// <summary>
        /// Get a Reference to <see cref="IFogOfWar"/> Interface using GameServices.
        /// </summary>
        private void GetFogOfWarInterfaceRef()
        {
            _fogOfWar = (IFogOfWar)Game.Services.GetService(typeof(IFogOfWar));
        }

        /// <summary>
        /// Get a Reference to <see cref="IWater"/> Interface using GameServices.
        /// </summary>
        private void GetWaterInterfaceRef()
        {
            _water = (IWaterManager)Game.Services.GetService(typeof(IWaterManager));
        }

        /// <summary>
        /// Get a Reference to <see cref="IShadowMap"/> Interface using GameServices.
        /// </summary>
        private void GetShadowMapInterfaceRef()
        {
            _shadowMap = (IShadowMap)Game.Services.GetService(typeof(IShadowMap));
        }

        /// <summary>
        /// Get a Reference to <see cref="IFPS"/> Interface using GameServices.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void GetFPSInterfaceRef()
// ReSharper restore InconsistentNaming
        {
            _fps = (IFPS)Game.Services.GetService(typeof(IFPS));
        }

        /// <summary>
        /// Get a Reference to <see cref="ITerrainShape"/> Interface using GameServices.
        /// </summary>
        private void GetTerrainInterfaceRef()
        {
            _terrain = (ITerrainShape)Game.Services.GetService(typeof(ITerrainShape));
        }

        /// <summary>
        /// Get a Reference to <see cref="IIFDTileManager"/> Interface using GameServices.
        /// </summary>
        private void GetIFDInterfaceRef()
        {
            _ifd = (IIFDTileManager)Game.Services.GetService(typeof(IIFDTileManager));
        }

        /// <summary>
        /// Get a Reference to <see cref="TerrainPickingRoutines"/> Interface using GameServices.
        /// </summary>
        private void GetPickingInterfaceRef()
        {
            _picking = (TerrainPickingRoutines)Game.Services.GetService(typeof(TerrainPickingRoutines));
        }

        /// <summary>
        /// Get a Reference to <see cref="ITerrainScreen"/> Interface using GameServices.
        /// </summary>
        private void GetTerrainScreenInterfaceRef()
        {
            _terrainScreen = (ITerrainScreen)Game.Services.GetService(typeof(ITerrainScreen));
        }

        /// <summary>
        /// Get a Reference to <see cref="NetworkGameComponent"/> Interface using GameServices.
        /// </summary>
        private void GetNetworkGameComponentInterfaceRef()
        {
            _networkGameComponent = (NetworkGameComponent)Game.Services.GetService(typeof(NetworkGameComponent));
        }

        /// <summary>
        /// Get a Reference to <see cref="PerfTimersComponent.Timers.StopWatchTimers"/> Class using GameServices.
        /// </summary>
        private void GetTimersInterfaceRef()
        {
            _timers = (StopWatchTimers)Game.Services.GetService(typeof(StopWatchTimers));
        }

        /// <summary>
        /// Get a Reference to <see cref="ScreenManager"/> Class using GamerServices.
        /// </summary>
        private void GetScreenManagerInterfaceRef()
        {
            _screenManager = (ScreenManager)Game.Services.GetService(typeof(ScreenManager));
        }


        /// <summary>
        /// Get a Reference to <see cref="GameViewPort"/> Class using GamerServices.
        /// </summary>
        private void GetGameViewPortInterfaceRef()
        {
            _gameViewPort = (GameViewPort)Game.Services.GetService(typeof(GameViewPort));
        }

        // 8/14/2008 - Used when full application shutdown.
        /// <summary>
        /// Releases the unmanaged resources used by the GameComponent and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            DisposeInterfaceReferences();

            // Only do Full Dispose when exiting game.
            if (disposing)
            {
                // Remove from Game components
                Game.Components.Remove(_gameConsole);

                // Dispose of Python Console.
                if (_gameConsole != null)
                    _gameConsole.Dispose();
                _gameConsole = null;
            }

            base.Dispose(disposing);
        }  
    }
}