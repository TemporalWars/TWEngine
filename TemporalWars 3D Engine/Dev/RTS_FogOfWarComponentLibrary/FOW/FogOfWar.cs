using System;
using System.Collections.Generic;
using System.Diagnostics;
using ImageNexus.BenScharbach.TWLate.RTS_FogOfWarComponentLibrary.FOW.Structs;
using ImageNexus.BenScharbach.TWLate.RTS_FogOfWarInterfaces.FOW;
using ImageNexus.BenScharbach.TWTools.ParallelTasksComponent.LocklessDictionary;
using ImageNexus.BenScharbach.TWTools.PerfTimersComponent.Timers;
using ImageNexus.BenScharbach.TWTools.PerfTimersComponent.Timers.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;

namespace ImageNexus.BenScharbach.TWLate.RTS_FogOfWarComponentLibrary.FOW
{
    // 7/3/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="ImageNexus.BenScharbach.TWLate.RTS_FogOfWarComponentLibrary.FOW"/> namespace contains the classes
    /// which make up the entire <see cref="FogOfWar"/>.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    // 7/9/2008 - FogOFWar Effect
    /// <summary>
    /// The <see cref="FogOfWar"/> class is a shroud used to hide places, buildings, and enemy units that a player hasn't yet seen.  This means any
    ///	areas which aren't within the sight range of a friendly unit are hidden from the player's view by the fog-of-war.
    /// </summary>
    public class FogOfWar : DrawableGameComponent, IFogOfWar
    {        
        // Terrain Interface Reference
        private static IFOWTerrainShape _terrainShape;
        // 12/31/2009
        private static IFOWPlayer _thisPlayer;
        private static IFOWTerrainData _terrainData;

        // 6/10/2010 - Holds the current set of selectableItems, which are PUSH from
        //             the player's classes 'AddSelectableItem' and 'RemoveSelectableItem'.
        private static readonly LocklessDictionary<int, IFOWSceneItem> SelectableItemsDictionary = new LocklessDictionary<int, IFOWSceneItem>(4, 50);
        // 6/10/2010 - Simple array which holds the current set of IFowSceneItems; Updated from LocklessDictionary.
        private static volatile IFOWSceneItem[] _selectableItems = new IFOWSceneItem[50];
        private static int _selectableItemsCount;

        // _gameInstance
        private static Game _gameInstance;

        private static SpriteBatch _spriteBatch;

        // Fog-Of-War RT & Texture
        private static RenderTarget2D _fogOfWarRt;
        private static Texture2D _viewField;

        // Fog-Of-War Texture       
        private static Texture2D _fogOfWarTexture;        

        // 1/14/2009 - Logical Representation of FOW        
        private static Visited[] _visitedTiles;
        private static bool[] _visibleTiles;
        private static volatile bool _updateSight = true; // Set when an SceneItemOwner moves.  

        
        private Thread _updateLogicalFogOfWarThread;
        private static volatile GameTime _gameTime;
        private static volatile bool _isStopping;

        // 7/20/2009
        private bool _updateFowTexture;

        

        #region Properties

        // 1/1/2010 - 
        ///<summary>
        /// <see cref="Game"/> instance
        ///</summary>
        public Game GameInstance
        {
            set
            {
                _gameInstance = value;
            }
        }

        ///<summary>
        /// The <see cref="Texture2D"/> with fog-of-war <see cref="RenderTarget"/> result.
        ///</summary>
        public Texture2D FogOfWarTexture
        {
            get { return _fogOfWarTexture; }
            set { _fogOfWarTexture = value; }
        }

        ///<summary>
        /// Set to force SightMatrices to update themselves, which is 
        /// indirectly done via the <see cref="UpdateSightMatrices"/> method.
        ///</summary>
        public bool UpdateSight
        {
            get { return _updateSight; }
            set { _updateSight = value; }
        }        

        ///<summary>
        /// Show <see cref="FogOfWar"/>?
        ///</summary>
        public bool IsVisible
        {
            set 
            { 
                Visible = value; 

                // 9/10/2008 - Set into TerrainShape Effect
                if (_terrainShape != null)
                    _terrainShape.SetFogOfWarSettings(Visible);
            
            }
            get { return Visible; }
        }

        // 1/21/2009 - shortcut version
        ///<summary>
        /// Show <see cref="FogOfWar"/>?
        ///</summary>
        public bool V
        {
            set
            {
                Visible = value;

                // 9/10/2008 - Set into TerrainShape Effect
                if (_terrainShape != null)
                    _terrainShape.SetFogOfWarSettings(Visible);

            }
            get { return Visible; }
        }

        #endregion

        // 1/1/2010 - 
        ///<summary>
        /// Default Parameterless constructor, required for the LateBinding on Xbox.
        ///</summary>
        public FogOfWar()
            : base(null)
        {
            // XBOX will call the CommonInitilization from the game engine!
            
        }

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="game"><see cref="Game"/> game</param>
        public FogOfWar(Game game)
            : base(game)
        {
            // 12/31/2009
            _gameInstance = game;

            // Init Common settings.
            CommonInitilization(game);

            // 5/18/2010 - DrawORder
            DrawOrder = 5;
        }

        // 1/1/2010
        /// <summary>
        /// Set to capture the NullRefExp Error, which will be thrown by base, since the
        /// Game instance was not able to be set for the Xbox LateBinding version!
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

        // 1/1/2010
        /// <summary>
        /// Common constructor initilization
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        public void CommonInitilization(Game game)
        {
            // 12/31/2009
            _gameInstance = game;

            _spriteBatch = (SpriteBatch)_gameInstance.Services.GetService(typeof(SpriteBatch));

            // Xna 4.0 changes.
            //_fogOfWarTexture = new Texture2D(_gameInstance.GraphicsDevice, 512, 512, 1, TextureUsage.None, SurfaceFormat.Color);
            _fogOfWarTexture = new Texture2D(_gameInstance.GraphicsDevice, 512, 512, true, SurfaceFormat.Color); 

            // Fog-Of-War Initialization
            /*_fogOfWarRt = new RenderTarget2D(_gameInstance.GraphicsDevice, 256, 256, 1, SurfaceFormat.Color,
                                             _gameInstance.GraphicsDevice.PresentationParameters.MultiSampleType,
                                             _gameInstance.GraphicsDevice.PresentationParameters.MultiSampleQuality);*/
            _fogOfWarRt = new RenderTarget2D(_gameInstance.GraphicsDevice, 256, 256, true, SurfaceFormat.Color, _gameInstance.GraphicsDevice.PresentationParameters.DepthStencilFormat);
                
               

            // 1/19/2009 - Attach to Global PathToMoveCompleted event.
            //AStarItem.PathMoveToCompletedG += AStarItem_PathMoveToCompletedG;
            // 3/18/2009 - Attach to Global Camera Updated event. - Required in order to have the FOW buildings show; otherwise would be invisible at times!
            //_camera.CameraUpdated += CameraUpdated;
           
#if DEBUG
            // 7/20/2009 - DEBUG Timers
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.FOWUpdate, false);//"FOW_Update"
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.FOWUpdate_SightMatrices, false);
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.FOWUpdate_ObjectsVisible, false);
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.FOWRender, false);//"FOW_Render"
#endif

            
            // 7/20/2009 - Start FOW Thread.
            _updateLogicalFogOfWarThread = new Thread(UpdateLogicalRepresentationOfFOW)
                                               {
                                                   Name = "UpdateLogicalFOW_Thread",
                                                   IsBackground = true
                                               };
            _updateLogicalFogOfWarThread.Start();
        
            // 11/7/2008
            DrawOrder = 5;
        }

        // 7/17/2009
        /// <summary>
        /// Called when graphics resources need to be loaded. Override this method to load any component-specific graphics resources.
        /// </summary>
        protected override void LoadContent()
        {
            // 4/6/2010 - Updated to use 'ContentTexturesLoc' Engine var.
            _viewField = _gameInstance.Content.Load<Texture2D>(((IFOWEngineRef)_gameInstance).ContentTexturesLoc + @"\Textures\fogView");     

            base.LoadContent();
        }

        // 1/1/2010
        /// <summary>
        /// <see cref="EventHandler"/> for forcing the SightMatrices to update themselves, which is 
        /// indirectly done via the <see cref="UpdateSightMatrices"/> method.
        /// </summary>
        /// <param name="sender">Provides a reference to the object that raised the event.</param>
        /// <param name="e">Passes an object specific to the event that is being handled.</param>
        public void UpdateSightMatrices(object sender, EventArgs e)
        {
            _updateSight = true;
        }

        
        // 10/31/2008
        // 1/14/2009: Updated to Init the FOW Logical Representation Arrays.
        /// <summary>
        /// Call to set the interface references for the <see cref="IFOWTerrainShape"/> and
        /// <see cref="IFOWTerrainData"/>. 
        /// </summary>
        public void InitFogOfWarSettings()
        {
            // Get a Reference to the Interface for Terrain Class
            _terrainShape = (IFOWTerrainShape)_gameInstance.Services.GetService(typeof(IFOWTerrainShape));

            // 12/31/2009 - Set a Reference to the _terrainData interface.
            _terrainData = (IFOWTerrainData)_gameInstance.Services.GetService(typeof(IFOWTerrainData));

            // 9/10/2008 - Set into TerrainShape Effect
            _terrainShape.SetFogOfWarSettings(Visible);

            // 1/14/2009 - FOW Logical Representation Init
            var pathNodeSize = ((IFOWEngineRef)_gameInstance).PathNodeSize; // 8/20/2009
            _visitedTiles = new Visited[pathNodeSize * pathNodeSize];
            _visibleTiles = new bool[pathNodeSize * pathNodeSize];
        }    

        /// <summary>
        /// Renders the <see cref="FogOfWar"/> texture.
        /// </summary>
        /// <param name="inGameTime"><see cref="GameTime"/> instance</param>
        public sealed override void Update(GameTime inGameTime)
        {
            base.Update(_gameTime);

            if (!Visible) return;

            // 7/20/2009 - Save GameTime for thread use.
            _gameTime = inGameTime;

#if DEBUG
            StopWatchTimers.StartStopWatchInstance(StopWatchName.FOWRender);//"FOW_Render"
#endif
            // 6/11/2010: Moved creation of FogOFWar texture to Update method, which
            //            eliminates the flashing of the screen!
            { // Renders the <see cref="FogOfWar"/> texture.
                // 7/20/2009
                _updateFowTexture = !_updateFowTexture;

                // Update FogOfWar Render textures.
                if (_updateFowTexture)
                    UpdateFogOfWar(_gameInstance);
            }

#if DEBUG
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.FOWRender);//"FOW_Render"
#endif

        }

        // 6/10/2010
        ///<summary>
        /// Add a <see cref="IFOWSceneItem"/> selectable item to the
        /// internal collection, for use when doing the sight calculations.
        ///</summary>
        ///<param name="selectableItem"><see cref="IFOWSceneItem"/> instance</param>
        public void AddSelectableItem(IFOWSceneItem selectableItem)
        {
            // Add to Lockless dictionary
            SelectableItemsDictionary.TryAdd(selectableItem.UniqueItemNumber, selectableItem);

            // Update the collection from dictionary, used in FOW thread.
            SelectableItemsDictionary.GetValues(ref _selectableItems, out _selectableItemsCount);
        }

        // 6/10/2010
        /// <summary>
        /// Removes all <see cref="IFOWSceneItem"/> selectable items where the
        /// 'Delete' property is set to TRUE.
        /// </summary>
        /// <param name="selectableItem"><see cref="IFOWSceneItem"/> instance</param>
        public void RemoveSelectableItem(IFOWSceneItem selectableItem)
        {
            // Remove from Lockless dictionary
            IFOWSceneItem item;
            SelectableItemsDictionary.TryRemove(selectableItem.UniqueItemNumber, out item);

            // Update the collection from dictionary, used in FOW thread.
            SelectableItemsDictionary.GetValues(ref _selectableItems, out _selectableItemsCount);
        }

        /// <summary>
        /// Renders the <see cref="FogOfWar"/> texture.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public sealed override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
        
        /// <summary>
        /// Updates the <see cref="FogOfWar"/> texture.
        /// </summary> 
        /// <param name="game"><see cref="Game"/> instance</param>       
        private static void UpdateFogOfWar(Game game)
        {
            // 4/21/2010 - Cache
            var spriteBatch = _spriteBatch;
            var graphicsDevice = game.GraphicsDevice;
            var terrainShape = _terrainShape;

            try
            {
                var thisPlayerNumber = ((IFOWEngineRef)game).ThisPlayer; // 6/10/2010

                // Xna 4.0 changes
                // Set render target to visible surface
                graphicsDevice.SetRenderTarget(_fogOfWarRt);
                graphicsDevice.Clear(Color.DarkSlateGray);
                //spriteBatch.Begin(SpriteBlendMode.Additive, SpriteSortMode.Immediate, SaveStateMode.SaveState); // 5/18/2010: Important: SaveState must be on; otherwise flashing occurs!
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);               
               
                var selectableItems = _selectableItems; // 4/21/2010 - Cache

                // Render the sightTextures for all 'FogOfWarItem' Objects in ThisPlayer
                var colorWhite = Color.White;
                for (var i = 0; i < _selectableItemsCount; i++)
                {
                    var selectableItem = selectableItems[i];

                    // 3/1/2009
                    if (selectableItem == null) continue;

                    // 6/10/2010 - Skip enemy players.
                    if (selectableItem.PlayerNumber != thisPlayerNumber) continue;

                    var fowShapeItem = selectableItem.ShapeItem;

                    // 9/11/2008 - Only Draw if UseFogOfWar=True.
                    if (fowShapeItem == null || !fowShapeItem.UseFogOfWar) continue;

                    Rectangle tmpFogOfWarDest;
                    CalculateFogOfWarView(ref selectableItem, out tmpFogOfWarDest);

                    spriteBatch.Draw(_viewField, tmpFogOfWarDest, colorWhite);
                }


                spriteBatch.End();

                // Set back to BackBuffer
                graphicsDevice.SetRenderTarget(null);

                // Copy Texture
                _fogOfWarTexture = _fogOfWarRt; //.GetTexture();

                // 9/10/2008 - Set Texture into TerrainShapes Effect
                if (terrainShape != null) terrainShape.SetFogOfWarTextureEffect(_fogOfWarTexture);
            }
            // 4/21/2010 - Capture NullRef exception, and check if '_spriteBatch' was the cause, rather than
            //             always checking if the is null!
            catch (NullReferenceException)
            {
                Debug.WriteLine("UpdateFogOfWar method, in 'FogOFWar' class, threw the NullReference exception error.",
                                "NullReferenceExpcetion");

                if (_spriteBatch == null)
                {
                    _spriteBatch = (SpriteBatch)game.Services.GetService(typeof(SpriteBatch));

                    Debug.WriteLine("The '_spriteBatch' was null; however, got new instance from services.",
                                "NullReferenceExpcetion");
                }
            } // End Try-Catch
        }

        // 1/14/2009
        /// <summary>
        /// Does the Updating of the Logical Representation of <see cref="FogOfWar"/>.
        /// </summary>
        private static void UpdateLogicalRepresentationOfFOW()
        {

#if XBOX360
            Thread.CurrentThread.SetProcessorAffinity(3);            
#endif
            // 8/20/2009
            var updateLogicalFOW = false;

            while (!_isStopping)
            {
                // 8/20/2009
                updateLogicalFOW = !updateLogicalFOW;

                // Update Logical Representation of Fog-Of-War. 
                //if (_updateSight)
                if (updateLogicalFOW)
                {
#if DEBUG
                    StopWatchTimers.StartStopWatchInstance(StopWatchName.FOWUpdate); //"FOW_Update"
#endif

                    // 8/12/2009 - Cache 
                    var gameInstance = _gameInstance; // 4/21/2010
                    var gameTime = _gameTime; // 4/21/2010
                    var players = ((IFOWEngineRef) gameInstance).Players;
                    var thisPlayerNumber = ((IFOWEngineRef) gameInstance).ThisPlayer;
                    var playersLength = ((IFOWEngineRef) gameInstance).MaxAllowablePlayers;

                    // 4/6/2009 - avoid crashes
                    if (players != null)
                    {
                        // 8/12/2009
                        var thisPlayer = players[thisPlayerNumber];
                        if (thisPlayer != null)
                        {
                            // Update SightMatrices & visible variables
                            UpdateSightMatrices(gameTime, gameInstance, _selectableItems, thisPlayerNumber);
                        }


                        // Find enemy player, and update items visible.
                        for (var playerNumber = 0; playerNumber < playersLength; playerNumber++)
                        {
                            // 7/25/2009
                            if (playerNumber == thisPlayerNumber) continue;

                            IsMapObjectsVisible(gameInstance, _selectableItems, playerNumber);
                        }

                        _updateSight = false;

                    } // End IF Players != NULL
#if DEBUG
                    StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.FOWUpdate); //"FOW_Update"
#endif
                } // End If updateLogicalFOW

                
                Thread.Sleep(10); // 6/10/2010

            } // End While

        }

        
        // 1/14/2009
        /// <summary>
        /// Updates the Logical Representation of the <see cref="FogOfWar"/> by iterating through
        /// all the <paramref name="selectableItems"/> given, and sets the <see cref="_visitedTiles"/> and <see cref="_visibleTiles"/> collections
        /// accordingly.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="game"><see cref="Game"/> instance</param>
        /// <param name="selectableItems">Collection of <see cref="IFOWSceneItem"/></param>
        /// <param name="thisPlayerNumber">Player number for 'This' player.</param>
        private static void UpdateSightMatrices(GameTime gameTime, Game game, IList<IFOWSceneItem> selectableItems, int thisPlayerNumber)
        {
            try
            {
                // 4/12/2009
                if (selectableItems == null)
                    return;
#if DEBUG
                StopWatchTimers.StartStopWatchInstance(StopWatchName.FOWUpdate_SightMatrices);
#endif

                // 8/27/2009 - Cache
                var visibleTiles = _visibleTiles;
                var visitedTiles = _visitedTiles;

                // 4/9/2009 - Reduce LD1 hits, by saving value here in local variable! 
                var pathNodeStride = ((IFOWEngineRef)game).PathNodeStride;
                var scale = _terrainData.Scale;
                var pathNodeSize = ((IFOWEngineRef)game).PathNodeSize;
                var totalSeconds = gameTime.TotalGameTime.TotalSeconds;

                // Reset the visible tiles to false
                Array.Clear(visibleTiles, 0, visibleTiles.Length);

                // Set Visible/Visited tiles; 6/10/2010: Updated to use the '_selectableItemsCount'.
                for (var i = 0; i < _selectableItemsCount; i++)
                {
                    // 8/12/2009 - Cache to improve perm.
                    var selectableItem = selectableItems[i];

                    // 6/7/2009
                    if (selectableItem == null) continue;

                    // 6/10/2010 - Skip enemy players.
                    if (selectableItem.PlayerNumber != thisPlayerNumber) continue;

                    var fowShapeItem = selectableItem.ShapeItem;

                    // 6/8/2009 - Make sure has interface 'FogOfWarShapeItem'
                    if (fowShapeItem == null) continue;     

                    // 4/6/2009 - Check if SceneItemOwner placed yet; if not, then skip Visited calculations.
                    var skipVisited = false;
                    if (selectableItem is IFOWPlaceableItem)
                        if (!selectableItem.ItemPlacedInFinalPosition)
                            skipVisited = true;
                    
                    // 4/18/2009: Removed Round method, since casting to an Int anyway!
                    // 4/6/2009: Updated to use Round method which casts to a 'Double', rather than 'Decimal', which avoids the
                    //           [ArgumentOutOfRangeException] error!
                    // Convert World Cords to Graph Stride Cords
                    var mapObjectPosition = selectableItem.Position;
                    var tmpPosition = new Point
                                            {
                                                X = (int)mapObjectPosition.X / pathNodeStride,
                                                Y = (int)mapObjectPosition.Z / pathNodeStride
                                            };
                   

                    // Set 'Start' & 'End'                     
                    var tmpFogOfWarDest = fowShapeItem.FogOfWarDestination;
                    var widthXScale = tmpFogOfWarDest.Width*scale; // 8/12/2009
                    var heightXScale = tmpFogOfWarDest.Height*scale; // 8/12/2009
                    var widthByPathNodeStride = (widthXScale/pathNodeStride); // 8/20/2009
                    var heightByPathNodeStride = (heightXScale/pathNodeStride); // 8/20/2009

                    var start = new Point
                                      {
                                          X = tmpPosition.X - widthByPathNodeStride,
                                          Y = tmpPosition.Y - heightByPathNodeStride
                                      };

                    var end = new Point
                                    {
                                        X = tmpPosition.X + widthByPathNodeStride,
                                        Y = tmpPosition.Y + heightByPathNodeStride
                                    };

                    // Loop through area from Start to End to set Visible / Visited tiles.                
                    var startX = start.X; var endX = end.X; // 10/8/2008: Cache
                    var startY = start.Y; var endY = end.Y; // 10/8/2008: Cache
                    for (var x = startX; x < endX; x++)
                        for (var y = startY; y < endY; y++)
                        {
                            if (x < 0 || y < 0 || x >= pathNodeSize || y >= pathNodeSize) continue;

                            // Make sure in range of mapsize
                            var index = x + y * pathNodeSize;

                            // 4/6/2009
                            if (!skipVisited)
                            {
                                visitedTiles[index].WasVisited = true;
                                visitedTiles[index].TimeVisitedAt = totalSeconds; // 4/6/2009
                            }

                            visibleTiles[index] = true;
                        }
                    
                } // End For Loop
#if DEBUG
                StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.FOWUpdate_SightMatrices);
#endif

            }
            catch (NullReferenceException ex)
            {
                // empty
                Debug.WriteLine("Method Error: FogOfWar classes UpdateSightMatrices threw NullRefException.");

            }

        }

        
        // 1/14/2009
        /// <summary>
        /// Loops through all <paramref name="selectableItems"/> of a player and determines whether the map
        /// objects is visible or not.
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        /// <param name="selectableItems">Collection of <see cref="IFOWSceneItem"/></param>
        /// <param name="enemyPlayerNumber">Enemy player number</param>
        private static void IsMapObjectsVisible(Game game, IList<IFOWSceneItem> selectableItems, int enemyPlayerNumber)
        {
            // 4/21/2010 - Cache
            var players = ((IFOWEngineRef)game).Players;
            var thisPlayerNumber = ((IFOWEngineRef)game).ThisPlayer; // 6/10/2010
            var thisPlayers = players[thisPlayerNumber]; // 6/10/2010
            var terrainData = _terrainData; // 4/21/2010

            // 8/12/2009 - Cache
            var pathNodeStride = ((IFOWEngineRef)game).PathNodeStride;
            var pathNodeSize = ((IFOWEngineRef)game).PathNodeSize;
            

            try
            {
#if DEBUG
                StopWatchTimers.StartStopWatchInstance(StopWatchName.FOWUpdate_ObjectsVisible);
#endif
                // 10/5/2009 - Boolean check, set to TRUE when player sights enemy player.
                var playerSightedEnemyPlayer = false;
                
                // 6/10/2010: Updated to use the '_selectableItemsCount'.
                for (var i = 0; i < _selectableItemsCount; i++)
                {
                    // 8/12/2009 - Cache to improve perm.
                    var selectableItem = selectableItems[i];

                    if (selectableItem == null) continue;

                    // 6/10/2010 - Skip this player, and process enemy players
                    if (selectableItem.PlayerNumber != enemyPlayerNumber) continue;

                    var fowShapeItem = selectableItem.ShapeItem;

                    // 5/10/2009
                    // Make sure has interface 'FogOfWarShapeItem'
                    if (fowShapeItem == null) continue;  
               
                    // Convert World Cords to Graph Stride Cords   
                    var mapObjectPosition = selectableItem.Position;
                    var tmpPosition = new Point
                                            {
                                                X = (int) mapObjectPosition.X/pathNodeStride,
                                                Y = (int) mapObjectPosition.Z/pathNodeStride
                                            };
                   

                    // 2/20/2009 - Verify Position calculated is actually on the map!
                    var tmpPositionCheck = new Vector2 {X = tmpPosition.X, Y = tmpPosition.Y};

                    if (!terrainData.IsOnHeightmap(tmpPositionCheck.X, tmpPositionCheck.Y)) continue;

                    // 5/18/2009 
                    var visibleTilesIndex = tmpPosition.X + tmpPosition.Y * pathNodeSize;

                    if (visibleTilesIndex > _visibleTiles.Length) continue;

                    // Buildings only need to be Visited to be seen!
                    if (selectableItem is IFOWPlaceableItem)
                    {
                        // 4/6/2009 - Only check Visited tiles for the enemy buildings!
                        if (selectableItem.PlayerNumber != thisPlayerNumber)
                        {
                            // 4/6/2009 - Make sure timeStamp of mapObject is less or equal to the Visited Time stamp!                               
                            var visitedTile = _visitedTiles[visibleTilesIndex]; // 10/5/2009 - Cache value.
                            fowShapeItem.IsFOWVisible = visitedTile.WasVisited &&
                                                        selectableItem.TimePlacedAt <= visitedTile.TimeVisitedAt;

                            // 10/5/2009 - Check if player sighted an enenmy building
                            if (fowShapeItem.IsFOWVisible)
                                playerSightedEnemyPlayer = true;
                        }
                        else
                            fowShapeItem.IsFOWVisible = true;
                        
                    }
                        // All others items need to be on a visible tile.
                    else
                    {
                        // 10/5/2009: Removed visibleTileIndex <= _visitedTiles.Length, since already done above.
                        // 4/6/2009 - Verify inside array bounds first!
                        //if (visibleTilesIndex <= _visibleTiles.Length)
                        fowShapeItem.IsFOWVisible = _visibleTiles[visibleTilesIndex];

                        // 10/5/2009 - Check if player sighted an enenmy unit
                        if (selectableItem.PlayerNumber != thisPlayerNumber && fowShapeItem.IsFOWVisible)
                            playerSightedEnemyPlayer = true;
                        
                    }

                    // 7/20/2009 - Since FOWVisible will be updated, need to tell SceneItem to update its WorldMatrix!
                    selectableItem.UpdateWorldMatrix = true;

                } // End For Loop

                // 10/5/2009 - Update the Player's Property 'PlayerSightedEnemyPlayer'.
                if (thisPlayers != null)
                    thisPlayers.PlayerSightedEnemyPlayer = playerSightedEnemyPlayer;
                

#if DEBUG
                StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.FOWUpdate_ObjectsVisible);
#endif
            }
            catch (ArgumentOutOfRangeException)
            {
                Debug.WriteLine("Method Error: FogOfWar classes IsMapObjectsVisible threw index out of range in array.");
            }
            catch(NullReferenceException) // 6/10/2010
            {
                Debug.WriteLine("Method Error: FogOfWar classes IsMapObjectsVisible threw Null ref error.");
            }
        }

        // 8/12/2009: Optimized.
        // 8/27/2008: Updated to be a Generic Method, while adding the 'Where' Constraint.
        //            This is the first Time using the 'Where' Constraint, but it seems to
        //            allow the code within the method to recogonize the Position Attribute, 
        //            which was part of the 'SceneItemWithPick' Class; furthermore, I am
        //            able to pass in Child Inherited Classes, since there parent is the
        //            given constraint! - Ben
        // 7/9/2008
        /// <summary>
        /// Calculates the <see cref="FogOfWar"/> view field, using the properties
        /// contain inside the <see cref="IFOWSceneItem"/> Interface object.
        /// </summary>
        /// <param name="item"><see cref="IFOWSceneItem"/> to Calculate for</param>
        /// <param name="fogOfWarDest">(OUT) <see cref="Rectangle"/> for <see cref="FogOfWar"/> destination</param>
        private static void CalculateFogOfWarView<T>(ref T item, out Rectangle fogOfWarDest) where T : IFOWSceneItem // Was SceneItemWithPick
        {
            fogOfWarDest = default(Rectangle);
            
            try
            {
                var mapWidthToScale = _terrainData.MapWidthToScale;
                var mapHeightToScale = _terrainData.MapHeightToScale;

                // FogOfWar Position Conversion for Items.
                var itemPosition = item.Position;
                var fogOfWarTexture = _fogOfWarTexture; // 4/21/2010
                var fogPositionX = (itemPosition.X / mapWidthToScale) * fogOfWarTexture.Width;
                var fogPositionY = (itemPosition.Z / mapHeightToScale) * fogOfWarTexture.Height;

                // Convert to Texture Size
                var fogOfWarShapeItem = item.ShapeItem;
                fogOfWarDest = fogOfWarShapeItem.FogOfWarDestination;

                fogOfWarDest.X = (int)((int)(fogPositionX) - (fogOfWarDest.Width * 0.5f)); // 8/12/2009: Opt by mult by 0.5f, rather than div by 2.
                fogOfWarDest.Y = (int)((int)(fogPositionY) - (fogOfWarDest.Height * 0.5f));// 8/12/2009: Opt by mult by 0.5f, rather than div by 2.

                fogOfWarShapeItem.FogOfWarDestination = fogOfWarDest;
            }
            catch (NullReferenceException)
            {
                Debug.WriteLine("CalculateFogOfWarView method, in FogOfWar class, threw the NullReference exception.", "NullReferenceException");

                // 12/31/2009 - Set a Reference to the _terrainData interface.
                if (_terrainData == null)
                {
                    _terrainData = (IFOWTerrainData)_gameInstance.Services.GetService(typeof(IFOWTerrainData));
                    Debug.WriteLine("The '_terrainData' was Null; however, got new reference from services.", "NullReferenceException");
                }
            } // End Try-Catch
            
        }


        // 4/5/2009 - Dispose of objects
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this final dispose?</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of resources            
                if (_fogOfWarRt != null)
                    _fogOfWarRt.Dispose();
                if (_viewField != null)
                    _viewField.Dispose();
                if (_fogOfWarTexture != null)
                    _fogOfWarTexture.Dispose();

                // 8/12/2009 - Dispose of thread
                _isStopping = true;
                if (_updateLogicalFogOfWarThread != null)
                {
                    _updateLogicalFogOfWarThread.Join();
                    _updateLogicalFogOfWarThread.Abort();
                }
                
                // Null Refs
                _terrainShape = null;
                _spriteBatch = null;
                _fogOfWarRt = null;
                _viewField = null;
                _fogOfWarTexture = null;              

            }

            base.Dispose(disposing);
        }
    }
}