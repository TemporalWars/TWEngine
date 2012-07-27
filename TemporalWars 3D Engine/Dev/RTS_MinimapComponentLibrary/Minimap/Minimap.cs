using System;
using System.Diagnostics;
#if !XBOX360
using System.IO;
using System.Windows.Forms;
#endif
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using PerfTimersComponent.Timers;
using PerfTimersComponent.Timers.Enums;
using SimpleQuadDrawer;


namespace TWEngine.MiniMapC
{
    // 7/24/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.MiniMapC"/> namespace contains the classes
    /// which make up the entire <see cref="Minimap"/>.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    // 7/9/2008 - MiniMap for Terrain
    /// <summary>
    /// The <see cref="Minimap"/> is designed to show unit movement, structures
    /// placed in the game world and to take direct mouse-click orders (Windows Platform). 
    /// The <see cref="Minimap"/> XNA component will also communicate with the <see cref="IFogOfWar"/> XNA component,
    /// directly showing the influences of the <see cref="IFogOfWar"/> in real time.
    /// </summary>
    public sealed class Minimap : DrawableGameComponent, IMinimap
    {
        // 1/1/2010
        private static Game _gameInstance;

        // 8/13/2008 - Add ContentManager Instance
        private ContentManager _contentManager;

        // Terrain Interface Reference
        private static IMinimapTerrainShape _terrainShape;        
        // FogOfWar Interface Reference
        private static IFogOfWar _fogOfWar;
        private static IMinimapTerrainData _terrainData; // 1/1/2010
        private static ICamera _camera; // 1/1/2010; 4/22/2010 - Updated to directly use 'ICamera'.
        private static IMinimapInterfaceDisplay _interfaceDisplay; // 1/1/2010

        // 6/17/2010 - Holds collection of IMinimapSceneItem.
        private static IMinimapSceneItem[] _minimapSceneItems = new IMinimapSceneItem[1]; 

        // 10/6/2008 - IFDTile Overlay Wrapper Texture
        private int _ifdWrapperKey;
        // 12/7/2009 - Show Wrapper?
        private bool _showWrapper;
        private bool _useDefaultWrapperTexture; // 2/26/2011 
       
        private static bool _doUpdateMiniMap = true;

        private static SpriteBatch _spriteBatch;

        // 9/24/2008 - Size of MiniMap
        private static int _mmWidth = 225;
        private static int _mmHeight = 225;

        // 5/28/2008 - Minimap
        internal static Rectangle MiniMapDest;        
        private static float _itemPositionX;
        private static float _itemPositionY;
        private static RenderTarget2D _miniMapRt;
        private static RenderTarget2D _landScapeRt;
        private static Texture2D _landScapeTexture;
        private static Texture2D _miniMapTexture;
        private static Effect _miniMapShader;
        private static Texture2D _wrapperTexture; // 2/25/2011

        // 3/25/2011 - XNA 4.0 Updates - Unit Point texture, used to display units on minimap.
        private static Texture2D _UnitPointTexturePlayer;
        private static Texture2D _UnitPointTextureEnemy; // 4/19/2011
       
        // 11/17/2008 - Add EffectParams & EffectTechniques
        private static EffectTechnique _miniMapCombineTechnique;
        private static EffectParameter _landscapeEParam;
        private static EffectParameter _fogOfWarEParam;
        private static EffectParameter _enableFogOfWarParam;
        
        private static BasicEffect _basicEffect;

        private static readonly Vector3 Vector3Zero = Vector3.Zero; // 4/21/2010

        // RenderMiniMap Atts - Should not be in method, otherwise, these will still be allocated on ever call!
        private static readonly Vector3[] CfRaysDirection = new Vector3[4];
        private static readonly Vector3[] CfCorners = new Vector3[8];
        private static readonly Ray[] Rays = new Ray[4];
        private static readonly Vector2[] Points = new Vector2[4];
        private static Plane _plane = new Plane(Vector3.Up, 0); // was -300
        private static readonly VertexPositionColor[] PointList = { new VertexPositionColor(Vector3Zero, Color.White),
                                                        new VertexPositionColor(Vector3Zero, Color.White),
                                                        new VertexPositionColor(Vector3Zero, Color.White),
                                                        new VertexPositionColor(Vector3Zero, Color.White) };
        // ReSharper disable RedundantExplicitArraySize
        private static readonly short[] LineStripIndices = new short[5] { 0, 1, 2, 3, 0 };
        // ReSharper restore RedundantExplicitArraySize

        // 7/24/2009 - Do Update every other frame.
        private static bool _updateMmThisFrame;
        private static readonly Matrix MatrixIdentity = Matrix.Identity;

        // 2/25/2011
        //private static VertexDeclaration _vertexDeclaration;


        #region Properties

        // 12/7/2009
        /// <summary>
        /// Controls the displaying of the <see cref="Texture2D"/> wrapper
        /// around the <see cref="Minimap"/>.
        /// </summary>
        public bool ShowTextureWrapper
        {
            set
            {
                _showWrapper = value;

                if (value) SetWrapperOn();
                else SetWrapperOff();
               
            }
            get
            {
                return _showWrapper;
            }
        }

       /// <summary>
        /// <see cref="MiniMapDestination"/> destination <see cref="Rectangle"/>.
       /// </summary>
        public Rectangle MiniMapDestination
        {
            get { return MiniMapDest; }
            set { MiniMapDest = value; }
        }

        /// <summary>
        /// Controls <see cref="Minimap"/> display.
        /// </summary>
        public bool IsVisible
        {
            set 
            { 
                Visible = value;

                // 12/7/2009 - Update Showing Wrapper
                ShowTextureWrapper = value;
            
            }
            get { return Visible; }
        }

        // 1/21/2009 - shortcut version
// ReSharper disable UnusedMember.Global
        /// <summary>
        /// Controls <see cref="Minimap"/> display.
        /// </summary>
        public bool V
// ReSharper restore UnusedMember.Global
        {
            set
            {
                // 12/7/2009 - Pass through to 'IsVisible' property.
                IsVisible = value;
            }
        }
        /// <summary>
        /// Set to render landscape for <see cref="Minimap"/> 
        /// </summary>
        public bool DoInitMiniMap { private get; set; }

        /// <summary>
        /// Set to tell <see cref="Minimap"/> to update for unit positions.
        /// </summary>
        public static bool DoUpdateMiniMap
        {
            set { _doUpdateMiniMap = value; }
        }

        // 1/2/2010
        /// <summary>
        /// Set to tell <see cref="Minimap"/> to update for unit positions.
        /// </summary>
        bool IMinimap.DoUpdateMiniMap
        {
            set { DoUpdateMiniMap = value; }
        }

        // Updated in the InputChecks method.
        /// <summary>
        /// Tracks if <see cref="Minimap"/> contains the cursor. 
        /// </summary>
        public static bool MiniMapContainsCursor { get; internal set; }

        // 1/2/2010
         /// <summary>
        /// Tracks if <see cref="Minimap"/> contains the cursor. 
        /// </summary>
        bool IMinimap.MiniMapContainsCursor 
        {
            get { return MiniMapContainsCursor; }
            set { MiniMapContainsCursor = value; }
        }

        // 1/2/2010
        /// <summary>
        /// <see cref="Minimap"/> width
        /// </summary>
        public int MMWidth
        {
            get { return _mmWidth; }
            set { _mmWidth = value; }
        }

        // 1/2/2010
        /// <summary> 
        /// <see cref="Minimap"/> height
        /// </summary>
        public int MMHeight
        {
            get { return _mmHeight; }
            set { _mmHeight = value; }
        }

        #endregion


        // 1/2/2010 - Default Parameterless contructor required for the LateBinding on Xbox.
        /// <summary>
        /// Default Parameterless contructor required for the LateBinding on Xbox.
        /// </summary>
        public Minimap()
            : base(null)
        {
            // XBOX will call the CommonInitilization from the game engine!
        }

        /// <summary>
        /// <see cref="Minimap"/> constructor which calls the <see cref="CommonInitilization"/>.
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        public Minimap(Game game) : base(game)
        {
            // Init Common settings.
            CommonInitilization(game);

#if DEBUG
            // 11/7/2008 - StopWatchTimers           
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.MinimapUpdate, false);//"MinimapUpdate"
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.MinimapDraw, false);//"MinimapDraw"   
#endif
        }

        // 1/2/2010
        /// <summary>
        /// Set to capture the <see cref="NullReferenceException"/> error, which will be thrown by base, since the
        /// <see cref="Game"/> instance was not able to be set for the Xbox LateBinding version!
        /// </summary>
        public override void Initialize()
        {
            // Set to capture the NullRefExp Error, which will be thrown by base, since the
            // Game instance was not able to be set for the Xbox LateBinding version!
            try
            {

                base.Initialize();
            }
            catch (Exception)
            {
                // Make sure LoadContent is called; usually called in base 'Init' method.
                LoadContent();
                return;
            }

        }

        // 1/2/2010
        /// <summary>
        /// <see cref="CommonInitilization"/> which initializes required components; for example, <see cref="ContentManager"/>
        /// ,<see cref="SpriteBatch"/> ,<see cref="RenderTarget2D"/> .
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        public void CommonInitilization(Game game)
        {
            _gameInstance = game;

            // 4/6/2010: Updated to use '1ContentMisc' engine global var.
            if (_contentManager == null)
                _contentManager = new ContentManager(_gameInstance.Services, ((IMinimapEngineRef)_gameInstance).ContentMiscLoc); // was "Content"

            // Set a Reference to the Interface for the FogOfWar Class
            _fogOfWar = (IFogOfWar)_gameInstance.Services.GetService(typeof(IFogOfWar));

            // 4/5/2009 - Set to Global SpriteBatch
            //SpriteBatch = new SpriteBatch(game.GraphicsDevice);
            _spriteBatch = (SpriteBatch)_gameInstance.Services.GetService(typeof(SpriteBatch));

            // 4/21/2011 - Fixed spacing from edges from 0 to 40 to avoid screen clipping.
            // 1/1/2010 - Cache
            var graphicsDevice = _gameInstance.GraphicsDevice;
            MiniMapDest = new Rectangle(graphicsDevice.Viewport.Width - (_mmWidth + 80), 40, 
                                        _mmWidth, _mmHeight);                      

            // 12/10/2008 - Set Draw Order
            DrawOrder = 245; // was 245
            Visible = false;         
   
            // xna 4.0 changes
            // 2/25/2011
            //_vertexDeclaration = new VertexDeclaration(graphicsDevice, VertexPositionColor.VertexElements);

            // 10/31/3008 - Attach EventHandler to TerrainShape
            //TerrainShape.TerrainShapeCreated += TerrainShapeCreated;
            // 1/19/2009 - Attach EventHandler to AStarItem
            //AStarItem.PathMoveToCompletedG += AStarItem_PathMoveToCompletedG;

            // MiniMap Initializiation
            {
                // xna 4.0 changes
                /*_miniMapRt = new RenderTarget2D(graphicsDevice, _mmWidth, _mmHeight, 1, SurfaceFormat.Color, graphicsDevice.PresentationParameters.MultiSampleType,
                                                graphicsDevice.PresentationParameters.MultiSampleQuality);
                _landScapeRt = new RenderTarget2D(graphicsDevice, _mmWidth, _mmHeight, 1, SurfaceFormat.Color, graphicsDevice.PresentationParameters.MultiSampleType,
                                                  graphicsDevice.PresentationParameters.MultiSampleQuality);*/

                _miniMapRt = new RenderTarget2D(graphicsDevice, _mmWidth, _mmHeight, true, SurfaceFormat.Color, graphicsDevice.PresentationParameters.DepthStencilFormat);
                _landScapeRt = new RenderTarget2D(graphicsDevice, _mmWidth, _mmHeight, true, SurfaceFormat.Color, graphicsDevice.PresentationParameters.DepthStencilFormat);

                //_landScapeTexture = new Texture2D(graphicsDevice, _mmWidth, _mmHeight, 1, TextureUsage.None, SurfaceFormat.Color);
                //_miniMapTexture = new Texture2D(graphicsDevice, _mmWidth, _mmHeight, 1, TextureUsage.None, SurfaceFormat.Color);
                _landScapeTexture = new Texture2D(graphicsDevice, _mmWidth, _mmHeight, true, SurfaceFormat.Color);
                _miniMapTexture = new Texture2D(graphicsDevice, _mmWidth, _mmHeight, true, SurfaceFormat.Color);

                // 3/25/2011 - XNA 4.0 Updates
                // Make sure init background has an opacity alpha channel data!
                // 4/19/2011 - Player (Blue)
                {
                    _UnitPointTexturePlayer = new Texture2D(graphicsDevice, 10, 10, false, SurfaceFormat.Color);
                    var tmpData = new Color[_UnitPointTexturePlayer.Height * _UnitPointTexturePlayer.Width];
                    _UnitPointTexturePlayer.GetData(tmpData);

                    for (int i = 0; i < tmpData.Length; i++)
                    {
                        tmpData[i].B = 255;
                        tmpData[i].A = 255;
                    }
                    _UnitPointTexturePlayer.SetData(tmpData);
                }
                // 4/19/2011 - Enemy Player (Red)
                {
                    _UnitPointTextureEnemy = new Texture2D(graphicsDevice, 10, 10, false, SurfaceFormat.Color);
                    var tmpData = new Color[_UnitPointTextureEnemy.Height * _UnitPointTextureEnemy.Width];
                    _UnitPointTextureEnemy.GetData(tmpData);

                    for (int i = 0; i < tmpData.Length; i++)
                    {
                        tmpData[i].R = 255;
                        tmpData[i].A = 255;
                    }
                    _UnitPointTextureEnemy.SetData(tmpData);
                }   
                
                
                _miniMapShader = _contentManager.Load<Effect>(@"Shaders\MiniMap");

                // 5/30/2008 - Used in miniMap Camera Frustrum drawing!
                _basicEffect = new BasicEffect(graphicsDevice)
                                   {
                                       // 2/24/2011 - Updated to white
                                       Alpha = 1.0f,
                                       DiffuseColor = new Vector3(1.0f, 0.0f, 0.0f),
                                       SpecularColor = new Vector3(0.25f, 0.25f, 0.25f),
                                       SpecularPower = 5.0f,
                                       AmbientLightColor = new Vector3(0.75f, 0.75f, 0.75f)
                                   };
                
            }

            // 11/17/2008 - Set EffectParams & EffectTechniques
            _miniMapCombineTechnique = _miniMapShader.Techniques["MiniMapCombine"];
            _landscapeEParam = _miniMapShader.Parameters["landScape"];
            _fogOfWarEParam = _miniMapShader.Parameters["FogOfWar"];
            _enableFogOfWarParam = _miniMapShader.Parameters["xEnableFogOfWar"];
            // 2/20/2009
            _miniMapShader.CurrentTechnique = _miniMapCombineTechnique;
        }

        // 1/1/2010
        /// <summary>
        /// <see cref="EventHandler"/> to force the <see cref="Minimap"/> internal <see cref="IFogOfWarShapeItem"/> positions to update. 
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        public void UpdateMiniMapPosition_EventHandler(object sender, EventArgs e)
        {
            _doUpdateMiniMap = true;
        }

       

        // 1/1/2010
        /// <summary>
        /// Call to set the interface references for the <see cref="IMinimapTerrainShape"/>,
        /// <see cref="IMinimapTerrainData"/>, <see cref="ICamera"/> and <see cref="IMinimapInterfaceDisplay"/>.
        /// </summary>
        public void InitSettings()
        {
            // Set a Reference to the Interface for Terrain Class
            _terrainShape = (IMinimapTerrainShape)_gameInstance.Services.GetService(typeof(IMinimapTerrainShape));

            // Set a Reference to the _terrainData interface.
            _terrainData = (IMinimapTerrainData)_gameInstance.Services.GetService(typeof(IMinimapTerrainData));

            // Set a Reference to the _camera interface.
            _camera = (ICamera)_gameInstance.Services.GetService(typeof(ICamera));

            // Set a Refernece to the _InterfaceDisplay interface.
            _interfaceDisplay = (IMinimapInterfaceDisplay) _gameInstance.Services.GetService(typeof (IMinimapInterfaceDisplay));

        }

        //  8/13/2008
        /// <summary>
        /// Called when graphics resources need to be unloaded. Override this method to unload any component-specific graphics resources.
        /// </summary>
        protected override void UnloadContent()
        {
            if (_contentManager != null)
                _contentManager.Unload();

            base.UnloadContent();
        }

        /// <summary>
        /// Updates the <see cref="Minimap"/> by combining the landscape <see cref="Texture2D"/> and <see cref="IFogOfWar"/> <see cref="Texture2D"/>.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public override void Update(GameTime gameTime)
        {
            if (!Visible)
                return;

#if DEBUG
            // 11/7/2008 - Debug purposes           
            StopWatchTimers.StartStopWatchInstance(StopWatchName.MinimapUpdate);//"MinimapUpdate"  
#endif

            // 1/15/2009
            // Called only when units change Position
            if (_doUpdateMiniMap)
            {
                _doUpdateMiniMap = false;               
                UpdatePlayerPositionsOnMiniMap();
                
            }
            // Updates the MiniMap by combining the landscape Texture & Fog-Of-War Texture
            UpdateMiniMap(_gameInstance.GraphicsDevice);
            
#if DEBUG
            // 11/7/2008 - Debug purposes
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.MinimapUpdate);//"MinimapUpdate"
#endif

            base.Update(gameTime);
        }

        /// <summary>
        /// Renders the <see cref="Minimap"/> component.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public override void Draw(GameTime gameTime)
        {
            try
            {
#if DEBUG
                // 11/17/2008 - Debug purposes
                StopWatchTimers.StartStopWatchInstance(StopWatchName.MinimapDraw);//"MinimapDraw"
#endif

                // 8/31/2008 - Check to see if we need to Init MiniMap
                if (DoInitMiniMap)
                {
                    RenderLandscapeForMiniMap();
                    DoInitMiniMap = false;
                }

                // Render MiniMap Interface
                RenderMiniMap(_gameInstance);

#if DEBUG
                // 11/7/2008 - Debug purposes
                StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.MinimapDraw);//"MinimapDraw"
#endif

                base.Draw(gameTime);
            }
            // 4/21/2010 - Capture null exp error, and then set missing item.
            catch (NullReferenceException)
            {
                Debug.WriteLine("Draw method in Minimap class, threw NullRef error.", "NullRefException");

                // 10/31/2008
                if (_terrainShape == null)
                {
                    _terrainShape = (IMinimapTerrainShape)_gameInstance.Services.GetService(typeof(IMinimapTerrainShape));

                    Debug.WriteLine("The '_terrainShape' was null; however, retrieved from services.", "NullRefException");
                }
                
            } // End-Try
            
        }

        // 5/11/2009: Updated to be STATIC.
        /// <summary>
        /// Renders the landscape, using orthogonal projection, onto
        /// the <see cref="_landScapeRt"/> render-target.
        /// </summary>        
        public static void RenderLandscapeForMiniMap()
        {
            // 4/21/2010 - Cache
            var camera = _camera;
            var terrainData = _terrainData;
            var graphicsDevice = _gameInstance.GraphicsDevice; // 1/1/2010

            // Set orthogonal rendering view & projection
            camera.SetOrthogonalView(terrainData.MapWidthToScale, terrainData.MapHeightToScale);

            // Set render targer to visible surface
            graphicsDevice.SetRenderTarget(_landScapeRt);
            //ImageNexusRTSGameEngine._gameInstance.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            graphicsDevice.Clear(Color.Black);

            // Render the Terrain to renderTarget
            _terrainShape.DrawMiniMap(); // 1/1/2010
           
            // Reset the render target to back buffer
            graphicsDevice.SetRenderTarget(null);           

            // Set Camera back to Normal RTS View
            camera.SetNormalRTSView();

            // Copy Texture from landScapeSurface
            _landScapeTexture = _landScapeRt; //.GetTexture();

        }

        // 1/1/2010
        /// <summary>
        /// Interfaces explicit version of <see cref="RenderLandscapeForMiniMap"/>, used to indirectly call
        /// the static version.
        /// </summary>
        void IMinimap.RenderLandscapeForMiniMap()
        {
            RenderLandscapeForMiniMap();
        }

        // 5/29/2008
        // 1/15/2009: Updated to Static to optimize memory.
        // 1/28/2009: Updated by removing the SpriteBatch for drawing, and instead Draw
        //            using a regular Quad.
        /// <summary>
        /// Updates the <see cref="Minimap"/> by combining the landscape <see cref="Texture2D"/> and Fog-Of-War <see cref="Texture2D"/>
        /// using a Combine Shader.  Then the units and buildings are drawn in the player's team color.
        /// </summary>     
        /// <param name="graphicsDevice"><see cref="GraphicsDevice"/> instance</param>   
        private static void UpdateMiniMap(GraphicsDevice graphicsDevice)
        {   
            // 7/24/2009 - Optimize: Updated every other frame.
            _updateMmThisFrame = !_updateMmThisFrame;
            if (!_updateMmThisFrame)
                return;

            // 4/21/2010 - Cache
            var miniMapRt = _miniMapRt;
            var landscapeEParam = _landscapeEParam;
            var landScapeTexture = _landScapeTexture;
            var miniMapShader = _miniMapShader;
           
            // Set render target to miniMap Render target
            graphicsDevice.SetRenderTarget(miniMapRt);
            //graphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            graphicsDevice.Clear(Color.Black);

            // draw the scene using our pixel shader.
            landscapeEParam.SetValue(landScapeTexture);
            if (_fogOfWar != null)
            {
                _fogOfWarEParam.SetValue(_fogOfWar.FogOfWarTexture);
                _enableFogOfWarParam.SetValue(_fogOfWar.IsVisible);
            }
            
            // xna 4.0a changes
            //miniMapShader.Begin();
            var effectPass = miniMapShader.CurrentTechnique.Passes[0]; // 4/21/2010
            effectPass.Apply(); // Begin();

            SimpleQuadDraw.DrawSimpleQuad(graphicsDevice);
            
            //effectPass.End();
            //miniMapShader.End();

            // Draw Units and Buildings in the player team color         
            var players = ((IMinimapEngineRef)_gameInstance).Players; // 12/20/2009
            var length = players.Length; // 4/21/2010
            for (var i = 0; i < length; i++)
            {
                // 6/8/2009
                var player = players[i]; // 12/20/2009
                if (player == null) continue;
               
                // Clear rectangles using the team color
                // 9/7/2008: Fix the Array Input from 'ThisPlayer', to 'i' loop value, to display correct TeamColor.
                if (player.SelectableMinimapRects.Length != 0)
                {
                   
                    /*graphicsDevice.Clear(ClearOptions.Target,
                        player.PlayerColor, 1.0f, 0, player.SelectableMinimapRects);*/

                    // 3/25/2011 - XNA 4.0 Updates
                    _spriteBatch.Begin(SpriteSortMode.Texture, BlendState.Opaque);
                    for (var j = 0; j < player.SelectableMinimapRects.Length; j++)
                    {
                        var unitPosition = player.SelectableMinimapRects[j];
                        
                        // 4/19/2011 - Updated to draw blue for player 0 and red for enemy player
                        _spriteBatch.Draw((i == 0) ? _UnitPointTexturePlayer : _UnitPointTextureEnemy, unitPosition, Color.White);
                    }
                   _spriteBatch.End();
                }
                
            }

            // xna 4.0 chnages
            // Set render target back to backBuffer & Copy texture out of Render Target
            graphicsDevice.SetRenderTarget(null);
            _miniMapTexture = miniMapRt; //.GetTexture();          
           

        }

        // 1/15/2009
        /// <summary>
        /// Updates the positions of the Units and Buildings for each <see cref="IMinimapPlayer"/>, and saves the 
        /// data into the <see cref="IMinimapPlayer.SelectableMinimapRects"/> collection.  This will be used in the 
        /// <see cref="UpdateMiniMap"/> method to draw them with proper team colors.
        /// </summary>
        private static void UpdatePlayerPositionsOnMiniMap()
        {
            // 8/26/2009 - Cache
            var players = ((IMinimapEngineRef)_gameInstance).Players; // 4/21/2010
            var playersLength = players.Length;
            var mapWidthToScale = _terrainData.MapWidthToScale;
            var mapHeightToScale = _terrainData.MapHeightToScale;
            var miniMapTexture = _miniMapTexture; // 4/21/2010

            // Draw Units and Buildings in the player team color 
            for (var i = 0; i < playersLength; i++)
            {
                // 8/26/2009 - Cache
                var player = players[i];

                // 6/8/2009
                if (player == null) continue;

                // 6/18/2010 - Updated to retrieve Actual count.
                // 6/17/2010 - Updated to use new GetSelectableItems method.
                int selectableItemsCount;
                player.GetSelectableItems(ref _minimapSceneItems, out selectableItemsCount);

                // 10/2/2008 - First make sure array has enough room to store rectangles
                if (selectableItemsCount != player.SelectableMinimapRects.Length)
                {
                    var selectableMinimapRects = player.SelectableMinimapRects;
                    Array.Resize(ref selectableMinimapRects, selectableItemsCount);
                    player.SelectableMinimapRects = selectableMinimapRects;
                }

                // Iterate through Selectable Items and get rectangles in "Minimap Space"
                // 8/26/2008: Updated to ForLoop, rather than ForEach.
                for (var j = 0; j < selectableItemsCount; j++)
                {
                    var selectableItem = _minimapSceneItems[j];

                    // 5/24/2009
                    if (selectableItem == null) continue;

                    // 4/21/2010 - Skip items which are not 'Alive'.
                    if (!selectableItem.IsAlive) continue;

                    // 12/20/2009 - cache
                    var rectangle = player.SelectableMinimapRects[j]; 

                    // 1/15/2009: Check if on FOW visible tile.
                    var shapeItem = selectableItem.ShapeItem; // 4/21/2010
                    if (((IFogOfWarShapeItem)shapeItem).IsFOWVisible)
                    {
                        // Convert to Minimap Texture Size
                        _itemPositionX = (selectableItem.Position.X / mapWidthToScale) * miniMapTexture.Width;
                        _itemPositionY = (selectableItem.Position.Z / mapHeightToScale) * miniMapTexture.Height;

                        // 6/9/2009 - Get the PathBlockSize for SceneItemOwner.
                        var size = (shapeItem.PathBlockSize < 1) ? 1 : shapeItem.PathBlockSize;
                        var size2X = 2 * size; // 8/26/2009
                                          
                        rectangle.X = (int)_itemPositionX - size;
                        rectangle.Y = (int)_itemPositionY - size;
                        
                        rectangle.Width = size2X;
                        rectangle.Height = size2X;

                    }
                    else
                    {
                        // Then set empty placeholder; easier than trying to resize array to remove
                        // empty slots, when on the next iteration, would have to resize array back
                        // to original size!  This way, very little overhead on CPU & Memory.
                        rectangle.X = 0;
                        rectangle.Y = 0;
                        rectangle.Width = 0;
                        rectangle.Height = 0;
                    }

                    // 12/20/2009 - Update 'SelectableMinimapRects'
                    player.SelectableMinimapRects[j] = rectangle;

                } // End For SelectableItems Loop  

            } // End For Players Loop
        }

       
       

        // 5/29/2008
        // 1/15/2009: Updated to Static to optimize memory.
        // 1/28/2009: Updated to remove Ops overloads, whicha are slow on XBOX!
        /// <summary>
        /// Renders the <see cref="Minimap"/> texture onto the screen at the MiniMapDest <see cref="Rectangle"/> location.
        /// </summary>  
        /// <param name="game"><see cref="Game"/> instance</param>      
        private static void RenderMiniMap(Game game)
        {
            // 8/14/2009 - Cache
            var terrainData = _terrainData; // 4/21/2010
            var spriteBatch = _spriteBatch; // 4/21/2010
            var camera = _camera; // 4/21/2010
            var mapWidthToScale = terrainData.MapWidthToScale;
            var mapHeightToScale = terrainData.MapHeightToScale;
            var cfCorners = CfCorners; // 4/21/2010
            var cfRaysDirection = CfRaysDirection; // 4/21/2010
            var rays = Rays; // 4/21/2010
            var points = Points; // 4/21/2010
            var pointList = PointList; // 4/21/2010
            var graphicsDevice = game.GraphicsDevice; // 4/21/2010
            var basicEffect = _basicEffect; // 4/21/2010

            try
            {
                // Draw MiniMap on Screen
                spriteBatch.Begin(SpriteSortMode.Texture, BlendState.Opaque);
                spriteBatch.Draw(_miniMapTexture, MiniMapDest, Color.White);
                spriteBatch.End();

                // Drawing the View Frustum in the Minimap
                camera.CameraFrustum.GetCorners(cfCorners);

                // 1/28/2009: Updated by removing Ops Overloads, which are slow on XBOX!
                // Calculate Direction of Rays                
                Vector3.Subtract(ref cfCorners[4], ref cfCorners[0], out cfRaysDirection[0]);
                Vector3.Subtract(ref cfCorners[5], ref cfCorners[1], out cfRaysDirection[1]);
                Vector3.Subtract(ref cfCorners[6], ref cfCorners[2], out cfRaysDirection[2]);
                Vector3.Subtract(ref cfCorners[7], ref cfCorners[3], out cfRaysDirection[3]);

                // 12/20/2009 - Verify not to normalize zero values!
                if (!cfRaysDirection[0].Equals(Vector3Zero)) cfRaysDirection[0].Normalize();
                if (!cfRaysDirection[1].Equals(Vector3Zero)) cfRaysDirection[1].Normalize();
                if (!cfRaysDirection[2].Equals(Vector3Zero)) cfRaysDirection[2].Normalize();
                if (!cfRaysDirection[3].Equals(Vector3Zero)) cfRaysDirection[3].Normalize();

                // Init the four Rays
                rays[0].Position = cfCorners[0];
                rays[0].Direction = cfRaysDirection[0];
                rays[1].Position = cfCorners[1];
                rays[1].Direction = cfRaysDirection[1];
                rays[2].Position = cfCorners[2];
                rays[2].Direction = cfRaysDirection[2];
                rays[3].Position = cfCorners[3];
                rays[3].Direction = cfRaysDirection[3];

                var ok = true;
                var hit = Vector3Zero;
                float? distToIntersectionPoint;

                //check where each ray intersects with the ground _plane
                for (var i = 0; i < 4; i++)
                {
                    // Find Intersection point

                    // 9/12/2008
                    //distToIntersectionPoint = Rays[loop1].Intersects(_plane);
                    rays[i].Intersects(ref _plane, out distToIntersectionPoint);

                    // calcuate distance of _plane intersection point from ray Origin
                    /*double denominator = Vector3.Dot(_plane.Normal, dir[i].Direction);
                        double numerator = Vector3.Dot(_plane.Normal, dir[i].Position) + _plane.D;
                        double t = -(numerator / denominator);
                        // calculate the picked Position on the y = 0 _plane
                        hit = dir[i].Position + dir[i].Direction * (float)t;*/

                    if (distToIntersectionPoint != null)
                    {
                        // 1/28/2009: Updated by removing Ops Overloads, which are slow on XBOX!
                        //hit = (Vector3)(Rays[loop1].Position + Rays[loop1].Direction * distToIntersectionPoint);
                        Vector3 tmpCalc;
                        Vector3.Multiply(ref rays[i].Direction, distToIntersectionPoint.Value, out tmpCalc);
                        Vector3.Add(ref rays[i].Position, ref tmpCalc, out hit);
                    }
                    else
                        ok = false;

                    //Make sure the intersection point is on the positive side of the near _plane
                    if (distToIntersectionPoint < 0.0f)
                        ok = false;

                    // Convert the intersection point to a minimap coordinate
                    if (!ok) continue;

                    points[i].X = (hit.X/mapWidthToScale)*_mmWidth;
                    points[i].Y = (hit.Z/mapHeightToScale)*_mmHeight;
                }

                // Create PointList to draw lines of camera frustum
                pointList[0].Position.X = points[0].X;
                pointList[0].Position.Y = 0.0f;
                pointList[0].Position.Z = points[0].Y;
                pointList[1].Position.X = points[1].X;
                pointList[1].Position.Y = 0.0f;
                pointList[1].Position.Z = points[1].Y;
                pointList[2].Position.X = points[2].X;
                pointList[2].Position.Y = 0.0f;
                pointList[2].Position.Z = points[2].Y;
                pointList[3].Position.X = points[3].X;
                pointList[3].Position.Y = 0.0f;
                pointList[3].Position.Z = points[3].Y;
               

                // Set viewport to destination rectangle only...  
                var v1 = new Viewport
                             {
                                 X = MiniMapDest.X,
                                 Y = MiniMapDest.Y,
                                 Width = _mmWidth,
                                 Height = _mmHeight,
                                 MinDepth = 0.0f,
                                 MaxDepth = 1.0f
                             };

                var v2 = graphicsDevice.Viewport;
                graphicsDevice.Viewport = v1;

                // Draw Camera frustum in the minimap               
                if (ok)
                {
                    // Set OrthogonalView; otherwise, will not draw correctly!                    
                    camera.SetOrthogonalView(_mmWidth, _mmHeight);

                    basicEffect.World = MatrixIdentity;
                    basicEffect.View = camera.View;
                    basicEffect.Projection = camera.Projection;
                    basicEffect.VertexColorEnabled = true;

                    // XNA 4.0 changes
                    // 2/25/2011 - Required on Xbox; otherwise no drawing will appear.
                    //graphicsDevice.VertexDeclaration = _vertexDeclaration;
                    
                    //basicEffect.Begin();
                    var effectPass = basicEffect.CurrentTechnique.Passes[0]; // 4/21/2010
                    effectPass.Apply(); // Begin();

                    
                    graphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                        PrimitiveType.LineStrip,
                        pointList,
                        0, // vertex buffer offset to add to each element of the index buffer
                        4, // number of vertices to draw
                        LineStripIndices,
                        0, // first index element to read
                        4 // number of primitives to draw
                        );

                    //effectPass.End();
                    //basicEffect.End();

                    // Return back to normal RTS view.
                    camera.SetNormalRTSView();
                }
                

                // Reset Viewport
                graphicsDevice.Viewport = v2;
            }
            catch (NullReferenceException)
            {
                Debug.WriteLine("RenderMiniMap method in Minimap class, threw NullRef error.", "NullRefException");

                // 4/5/2009
                if (_spriteBatch == null)
                {
                    _spriteBatch = (SpriteBatch)game.Services.GetService(typeof(SpriteBatch));
                    Debug.WriteLine("'_spriteBatch' was null; however retrieved from services.", "NullRefException");
                }
            } // End Try-Catch

        }   
     
        // 4/8/2009
        /// <summary>
        /// Saves the <see cref="Minimap"/> texture using the given name; use the console
        /// to call this method.
        /// </summary>
        /// <remarks>The name given will be saved with '_MMP' and the .BMP extension.</remarks>
        /// <param name="name">The name you want to save the <see cref="Minimap"/> texture as.</param>
// ReSharper disable UnusedMember.Global
        public static void SaveMiniMapTexture(string name)
// ReSharper restore UnusedMember.Global
        {
#if !XBOX360
           
            if (_landScapeTexture == null) return;

            // 9/16/2010: XNA 4.0 Updates; Save() no longer exist; now use either SaveToJpg or SaveToPng
            // 9/16/2010: NOTE: http://blogs.msdn.com/b/shawnhar/archive/2010/05/10/image-codecs-in-xna-game-studio-4-0.aspx?PageIndex=2
            // Create Texture, and display success message.
            //_landScapeTexture.Save(String.Format("{0}_MMP.bmp", name), ImageFileFormat.Bmp);
            using (Stream stream = File.OpenWrite(String.Format("{0}_MMP.bmp", name)))
            {
                _landScapeTexture.SaveAsJpeg(stream, _landScapeTexture.Width, _landScapeTexture.Height);
            }

            MessageBox.Show("MiniMap texture (" + String.Format("{0}_MMP.bmp", name) + ") successfully created.", "Texture Created.",
                            MessageBoxButtons.OK);
#endif
        }

        // 1/2/2010
        /// <summary>
        /// Interfaces explicit version of <see cref="SaveMiniMapTexture"/>, used to indirectly call
        /// the static version.
        /// </summary>
        void IMinimap.SaveMiniMapTexture(string name)
        {
            SaveMiniMapTexture(name);
        }

        // 10/6/2008 
        /// <summary>
        /// Displays the <see cref="Minimap"/> wrapper <see cref="Texture2D"/>.
        /// </summary>
        private void SetWrapperOn()
        {
            // 2/25/2011 - Check if overwrapper set
            if (_wrapperTexture != null)
            { 
                // Add new wrapper texture
                _ifdWrapperKey = _interfaceDisplay.AddInterFaceDisplayTileOverlay(_wrapperTexture,
                                                                 new Rectangle(MiniMapDest.Left - 23, MiniMapDest.Top - 23,
                                                                               0, 0));
                return;
            }
          

        }

        // 10/6/2008
        /// <summary>
        /// Removes the Display of the <see cref="Minimap"/>  wrapper <see cref="Texture2D"/>.
        /// </summary>
        private void SetWrapperOff()
        {
            if (_interfaceDisplay != null) _interfaceDisplay.RemoveInterFaceDisplayTile(_ifdWrapperKey);
        }

        // 2/25/2011
        ///<summary>
        /// Updates the default wrapper texture to the given <paramref name="wrapperTexture"/>.
        ///</summary>
        ///<param name="wrapperTexture">Instance of <see cref="Texture2D"/>.</param>
        public void UpdateWrapperTexture(Texture2D wrapperTexture)
        {
            SetWrapperOff();

            _wrapperTexture = wrapperTexture;

            SetWrapperOn();
        }

        // 2/26/2011
        /// <summary>
        /// Uses the built-in default texture called 'IFDTileMinimapWrap'.
        /// </summary>
        public void UseDefaultWrapperTexture()
        {
            // 10/6/2008 - Add IFD Tile Overlay Wrapper Texture
            _ifdWrapperKey = _interfaceDisplay.AddInterFaceDisplayTileOverlay("IFDTileMinimapWrap",
                                                                              new Rectangle(MiniMapDest.Left - 23,
                                                                                            MiniMapDest.Top - 23, 0, 0));
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
                // Dispose of Resources             
                if (_miniMapRt != null)
                    _miniMapRt.Dispose();
                if (_landScapeRt != null)
                    _landScapeRt.Dispose();
                if (_landScapeTexture != null)
                    _landScapeTexture.Dispose();
                if (_miniMapTexture != null)
                    _miniMapTexture.Dispose();
                if (_miniMapShader != null)
                    _miniMapShader.Dispose();
                if (_basicEffect != null)
                    _basicEffect.Dispose();

                // Null Refs
                _terrainShape = null;                
                _fogOfWar = null;
                _miniMapRt = null;
                _landScapeRt = null;
                _landScapeTexture = null;
                _miniMapTexture = null;
                _miniMapShader = null;
                _basicEffect = null;

                if (_contentManager != null)
                {
                    _contentManager.Dispose();
                    _contentManager = null;
                }

            }

            base.Dispose(disposing);
        }
    }
}