#region File Description
//-----------------------------------------------------------------------------
// IFDTileManager.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using AStarInterfaces.AStarAlgorithm.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PerfTimersComponent.Timers;
using PerfTimersComponent.Timers.Enums;
using TWEngine.GameCamera;
using TWEngine.GameLevels;
using TWEngine.GameScreens;
using TWEngine.HandleGameInput;
using TWEngine.IFDTiles.Delegates;
using TWEngine.IFDTiles.Enums;
using TWEngine.IFDTiles.Structs;
using TWEngine.InstancedModels.Enums;
using TWEngine.Interfaces;
using TWEngine.MemoryPool.PoolItems;
using TWEngine.Players;
using TWEngine.SceneItems;
using TWEngine.Terrain;
using TWEngine.Terrain.Enums;
using TWEngine.Utilities;

#if XBOX360
using TWEngine.Common.Extensions;
using TWEngine.GameCamera;
#endif


namespace TWEngine.IFDTiles
{
    /// <summary>
    /// The <see cref="IFDTileManager"/> class manages all <see cref="IFDTile"/> instances, by
    /// adding them to the internal collections, drawing and updating each game cycle, and updating
    /// <see cref="SceneItem"/> placement into the game world.
    /// </summary>
    public class IFDTileManager : DrawableGameComponent, IIFDTileManager
    {
        // 10/12/2009 - Cache value
        private static Player _thisPlayer;

        // 1/6/2010 - Content Resources for some Assets.
        internal static ResourceContentManager ContentResourceManager;

        // 1/6/2009 - Add Triangle Helper class to draw Placement Overlay
        private static TriangleShapeHelper _tShapeHelper;
        private static VertexPositionColor[] _selectionBoxes = new VertexPositionColor[25 * 6];
        private static Color _selectionBoxColor = Color.White; // Default color. 

        // 1/5/2009 - To display player's current Cash amount.
        private static IFDTileMessage _cashMessageTile;
        // 1/30/2009 - To display player's current Energy amount.
        private static IFDTileMessage _energyMessageTile;

        // 10/19/2008 - Add new 'SceneItemPlaced' Event Handler
        ///<summary>
        /// Add new delegate <see cref="SceneItemPlacedEventHandler"/>.
        ///</summary>
        public static event SceneItemPlacedEventHandler SceneItemPlaced;

        // 10/11/2009 - Dictionary to keep a 1-many relationship between the '_ifdTileGroups', and the
        //              SubQueues which can belong it, via a List<SubQueueKey>s.
        //              Key = (int) of Enum 'IFDGroupControlType', Value = SubQueueKey.
        private static Dictionary<int, List<SubQueueKey>> _ifdTileRetrieveGroups;

        // 9/24/2008 - Dictionary to hold the Groups of IFD tiles.
        // 10/2/2008 - Updated Key to Dictionary to be 'int', rather than Enum to avoid Boxing/Unboxing!
        private static Dictionary<int, List<IFDTile>> _ifdTileGroups;

        // 11/4/2008 - Dictionary to hold the GroupControl Tiles, which is needed to get to one when adding
        //             a new SubControl Group Building Queue tile.
        private static Dictionary<int, IFDTileGroupControl> _ifdGroupControlTiles;

        // 12/8/2008 - Dictionary to hold the Queue of clicked Tiles, which is needed to track which Tile should
        //             be the one currently being built within a subgroup; therefore, the Subgroup key will be
        //             used as the Dictionaries Key.  And the Queue will hold 'IFD_Placement' tile references.
        private static Dictionary<int, Queue<IFDTilePlacement>> _ifdTilesClicked;

        // Current InterFace Tiles Textures to render; set with call to 'SetAsCurrentGroupToDisplay'.     

        // 9/24/2008 - Tiles which always need to display; for example, group-control tiles.
        private static List<IFDTile> _ifdTilesPerm;

        // 6/17/2010
        private static List<IFDTile> _ifdTiles;

        // 6/17/2010 - ROC of IfdTiles.
        private static ReadOnlyCollection<IFDTile> _readOnlyIFDTiles;

        // 5/25/2012 - Stores ref to the IFD tiles GroupControl, to allow turning On/Off (Scripting Purposes)
        private static IFDTileGroupControl _ifdTileGcBuildings;
        private static IFDTileGroupControl _ifdTileGcPeople;
        private static IFDTileGroupControl _ifdTileGcVehicles;
        private static IFDTileGroupControl _ifdTileGcShields;
        private static IFDTileGroupControl _ifdTileGcAirplanes;

        // 4/30/2009 - 
        /// <summary>
        /// XBOX Only - If an <see cref="IFDTile"/> set is currently being displayed...  
        /// Used in the InputState to determine if the <see cref="Camera"/> can move;
        /// Otherwise, the movement is used for <see cref="IFDTile"/> selecting.
        /// </summary>
        /// <remarks>For XBOX, the <see cref="IFDTile"/> set remains fixed to center of screen.</remarks>
        internal static bool IFDTileSetIsDisplaying;

        // 5/25/2012 - Turns off the player checks for the scoreboard when the scriptingAction is using this tile.
        private static bool _displayScoreboardForScripting;
        // 
        /// <summary>
        /// Attributes used for placing <see cref="SceneItem"/> on <see cref="Terrain"/>.
        /// </summary>
        internal static Vector3 PlaceItemAt;
        private static ItemType _itemTypeToUse;
        private static ItemGroupType? _itemGroupToAttack; // 12/26/2008
        private static ItemGroupType? _productionType;
        private static ItemGroupType _buildingType;
        private static SceneItem _itemToPlace;
        private static IFDTile _ifdTileCaller; // 3/25/2009
        public static bool AttemptingItemPlacement;

        // 4/9/2009 - Ref to HQ Tiles; needed to activate enablers when game starts!
        /// <summary>
        /// Required reference to Head-Quarters#1 <see cref="IFDTile"/>; used to activate enablers when game starts.
        /// </summary>
        internal static IFDTilePlacement HQSide1;
        /// <summary>
        /// Required reference to Head-Quarters#2 <see cref="IFDTile"/>; used to activate enablers when game starts.
        /// </summary>
        internal static IFDTilePlacement HQSide2;

        // 2/24/2009 - Tracks the PoolManager Nodes
        private BuildingScenePoolItem _buildingPoolNode;
        private DefenseScenePoolItem _defensePoolNode;

        // XNA 4.0 Updates
        private static readonly RasterizerState _rasterizerState = new RasterizerState { FillMode = FillMode.Solid };
        private static readonly DepthStencilState _depthStencilState = new DepthStencilState { DepthBufferEnable = false };

        #region Properties

        ///<summary>
        /// Set to turn On/Off rendering of all <see cref="IFDTile"/> instances.
        ///</summary>
        public bool IsVisible
        {
            get { return Visible; }
            set { Visible = value; }
        }

        // 1/21/2009 - shortcut version
        // ReSharper disable UnusedMember.Global
        ///<summary>
        /// Set to turn On/Off rendering of all <see cref="IFDTile"/> instances.
        ///</summary>
        public bool V
        // ReSharper restore UnusedMember.Global
        {
            get { return Visible; }
            set { Visible = value; }
        }

        ///<summary>
        /// Returns if current cursor is within the given <see cref="IFDTile"/> instance.
        ///</summary>
        public static bool CursorInSomeIFDTile { get; private set; }

        // 5/25/2012
        /// <summary>
        /// Sets turning off/on the 'Scoreboard' tile. (Scripting Purposes)
        /// </summary>
        public static bool DisplayScoreboard
        {
            set
            {
                if (_energyMessageTile != null)
                {
                    _energyMessageTile.DrawTile = value;
                }

                if (_cashMessageTile != null)
                {
                    _cashMessageTile.DrawTile = value;
                }

                _displayScoreboardForScripting = value;
            }
        }

        // 5/25/2012
        /// <summary>
        /// Sets to display the IFDGroup control tile. (Scripting Purposes)
        /// </summary>
        public static bool DisplayIFDGroupTileControl
        {
            set
            {
                if (_ifdTileGcBuildings != null)
                {
                    _ifdTileGcBuildings.DrawTile = value;
                }

                if (_ifdTileGcAirplanes != null)
                {
                    _ifdTileGcAirplanes.DrawTile = value;
                }

                if (_ifdTileGcPeople != null)
                {
                    _ifdTileGcPeople.DrawTile = value;
                }

                if (_ifdTileGcShields != null)
                {
                    _ifdTileGcShields.DrawTile = value;
                }

                if (_ifdTileGcVehicles != null)
                {
                    _ifdTileGcVehicles.DrawTile = value;
                }
            }
        }

        #endregion

        ///<summary>
        /// The constructor for <see cref="IFDTileManager"/>, intializes all required internal
        /// dictionaries, creates the <see cref="TriangleShapeHelper"/>, retrieves reference for
        /// the <see cref="IMinimap"/>, and calls the method <see cref="SetTilePlacementStartPointsForXbox"/>.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        public IFDTileManager(Game game)
            : base(game)
        {
            // 6/15/2010 - Updated to use new GetPlayer method.
            TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out _thisPlayer);

            // 1/6/2009 - Set EventHandler for the PlayerCreated event.
            TerrainScreen.PlayerInstancesCreated += TerrainScreen_PlayerInstancesCreated;

            // 4/30/2009 - Init List & Dictionaries
            {
                // 10/11/2009
                if (_ifdTileRetrieveGroups == null)
                    _ifdTileRetrieveGroups = new Dictionary<int, List<SubQueueKey>>();

                if (_ifdTileGroups == null)
                    _ifdTileGroups = new Dictionary<int, List<IFDTile>>();

                if (_ifdGroupControlTiles == null)
                    _ifdGroupControlTiles = new Dictionary<int, IFDTileGroupControl>();

                if (_ifdTilesClicked == null)
                    _ifdTilesClicked = new Dictionary<int, Queue<IFDTilePlacement>>();

                if (_ifdTilesPerm == null)
                    _ifdTilesPerm = new List<IFDTile>();
            }

            // 1/6/2009 - Used to draw the Placement-Overlay Boxes onto the Terrain, when player
            //            is attempting to place an SceneItemOwner.
            _tShapeHelper = new TriangleShapeHelper(ref game);

            // Set a Reference to the Interface for Cursor Class

            // Minimap Ref
            var miniMap = (IMinimap)game.Services.GetService(typeof(IMinimap));

            // 11/4/2008 - Add IFDTileManager to Services, so IFDTile has access
            game.Services.AddService(typeof(IFDTileManager), this);

            // 4/29/2009 - Set Tile Placement StartPoint, depending if PC or XBOX
#if XBOX360
            SetTilePlacementStartPointsForXbox(miniMap);
#else
            SetTilePlacementStartPointsForPc(miniMap);
            //SetTilePlacementStartPointsForXbox(miniMap);
#endif

            // 12/10/2008 - Set Draw Order
            DrawOrder = 240; // 250
            Visible = false;

            // 11/7/2008 - StopWatchTimers            
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.IFDUpdate, false); //"IFDUpdate"
        }

        // 6/17/2010
        ///<summary>
        /// Returns a <see cref="ReadOnlyCollection{T}"/> of the internal <see cref="_ifdTiles"/>
        /// collection.
        ///</summary>
        ///<param name="readOnlyCollectoin">(OUT) Read-Only collectoin of <see cref="_ifdTiles"/></param>
        public static void GetIFDTiles(out ReadOnlyCollection<IFDTile> readOnlyCollectoin)
        {
            if (_readOnlyIFDTiles == null)
                _readOnlyIFDTiles = new ReadOnlyCollection<IFDTile>(_ifdTiles);

            readOnlyCollectoin = _readOnlyIFDTiles;
        }

        // 1/6/2010
        /// <summary>
        /// <see cref="EventHandler"/> method to capture the <see cref="Player"/> 'Created'
        /// event, which forces an update to the internal player static reference variable.
        /// </summary>
        private static void TerrainScreen_PlayerInstancesCreated(object sender, EventArgs e)
        {
            // Set new reference
            // 6/15/2010 - Updated to use new GetPlayer method.
            TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out _thisPlayer);

        }

        // 4/29/2009
        /// <summary>
        /// Sets the proper start point for the Pc platform.
        /// </summary>
        /// <param name="miniMap">Interface <see cref="IMinimap"/> reference</param>
        private static void SetTilePlacementStartPointsForPc(IMinimap miniMap)
        {
            // 1/2/2010 - If Null, then just return.
            if (miniMap == null) return;

            // 11/4/20008
            // Set Start Point for the Group Control Tiles
            IFDTileGroupControl.TilePlacementStartPoint = new Point(miniMap.MiniMapDestination.Left,
                                                                    miniMap.MiniMapDestination.Bottom);

            // 11/4/20008
            // Set Start Point for the Sub-Queue Group Control Tiles
            IFDTileSubGroupControl.TilePlacementStartPoint = new Point(miniMap.MiniMapDestination.Left + 20,
                                                                       miniMap.MiniMapDestination.Bottom + 20 + 35 + 5);
            // 20 = wrapper; 35 = GC Tile; 5 = Margin

            // TODO: Set the 'wrapper' value (20), to be inside minimap.
            // Set IFD Placement Tiles Starting Point
            IFDTilePlacement.TilePlacementStartPoint = new Point(miniMap.MiniMapDestination.Left,
                                                                  miniMap.MiniMapDestination.Bottom + 20 + 35 + 30);
            // 20 = wrapper; 35 = GC Tile; 30 = subTab.
        }

        internal static int MiddleScreenX;
        internal static int MiddleScreenY;

        // 4/29/2009
        // ReSharper disable UnusedMember.Local
        /// <summary>
        /// Sets the proper start point for the Xbox platform.
        /// </summary>
        /// <param name="miniMap">Interface <see cref="IMinimap"/> reference</param>
        private static void SetTilePlacementStartPointsForXbox(IMinimap miniMap)
        // ReSharper restore UnusedMember.Local
        {
            // 1/2/2010 - If Null, then just return.
            if (miniMap == null) return;

            // 11/4/20008
            // Set Start Point for the Group Control Tiles
            IFDTileGroupControl.TilePlacementStartPoint = new Point(miniMap.MiniMapDestination.Left,
                                                                    miniMap.MiniMapDestination.Bottom);

            // 11/4/20008
            // Set Start Point for the Sub-Queue Group Control Tiles
            IFDTileSubGroupControl.TilePlacementStartPoint = new Point(miniMap.MiniMapDestination.Left + 20,
                                                                       miniMap.MiniMapDestination.Bottom + 20 + 35 + 5);

            // 1/9/2010: Note: Moved this to the constructor of the IFD_TilePlacement, so it is set also when the Minimap component is missing!
            // 4/29/2009 - Set Start of Circle in middle of screen.
            /*MiddleScreenX = ImageNexusRTSGameEngine.GameInstance.GraphicsDevice.PresentationParameters.BackBufferWidth/2;
            MiddleScreenY = ImageNexusRTSGameEngine.GameInstance.GraphicsDevice.PresentationParameters.BackBufferHeight/2;

            // Set IFD Placement Tiles Starting Point
            IFDTile_Placement.TilePlacementStartPoint = new Point(MiddleScreenX, MiddleScreenY);*/
        }

        // 11/13/2008; 3/3/2009: Add 'FinalDispose' parameter.
        /// <summary>
        /// Clears out all internal collections which reference any <see cref="IFDTile"/> instances.
        /// </summary>
        /// <param name="finalDispose">Is final dispose?</param>
        public static void ClearIFDTiles(bool finalDispose)
        {
            // 1/6/2010 - Call ContentManager Unload.
            ContentResourceManager.Unload();

            // 10/11/2009 - Clear Dictionary
            if (_ifdTileRetrieveGroups != null)
            {
                _ifdTileRetrieveGroups.Clear();
            }

            // 9/24/2008 - Clear Dictionary
            if (_ifdTileGroups != null)
            {
                foreach (var group in _ifdTileGroups)
                {
                    for (var i = 0; i < group.Value.Count; i++)
                    {
                        group.Value[i].Dispose(finalDispose);
                        group.Value[i] = null;
                    }

                    // Clear List
                    group.Value.Clear();
                }

                // Clear Dictionary
                _ifdTileGroups.Clear();
            }

            // 9/24/2008 - Clear IFD Perm tiles    
            if (_ifdTilesPerm != null)
            {
                for (var i = 0; i < _ifdTilesPerm.Count; i++)
                {
                    _ifdTilesPerm[i].Dispose(finalDispose);
                    _ifdTilesPerm[i] = null;
                }
                // Clear List
                _ifdTilesPerm.Clear();
            }

            // Clear IFD current tiles    
            if (_ifdTiles != null)
            {
                for (var i = 0; i < _ifdTiles.Count; i++)
                {
                    _ifdTiles[i].Dispose(finalDispose);
                    _ifdTiles[i] = null;
                }
                // Clear List
                _ifdTiles.Clear();
            }

            // Clear IFD Group tiles
            if (_ifdGroupControlTiles != null)
            {
                foreach (var tile in _ifdGroupControlTiles)
                {
                    tile.Value.Dispose(finalDispose);
                }

                // Clear List
                _ifdGroupControlTiles.Clear();
            }

            // Clear IFD tiles clicked
            if (_ifdTilesClicked != null)
            {
                foreach (var tile in _ifdTilesClicked)
                {
                    tile.Value.Clear();
                }

                // Clear List
                _ifdTilesClicked.Clear();
            }
        }

        /// <summary>
        /// Processes attempts of user placing some <see cref="SceneItem"/>, checks for
        /// selection events, and processes the <see cref="InterFaceRoundMeter"/>.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public override sealed void Update(GameTime gameTime)
        {
            // Only Update if Visible
            if (!Visible)
                return;

            // 11/7/2008 - TODO: Debug purposes
            //timers.stopWatches["IFDUpdate"].Start();
            StopWatchTimers.StartStopWatchInstance(StopWatchName.IFDUpdate);//"IFDUpdate"

            // Updates placement of SceneItemOwner
            if (AttemptingItemPlacement)
                UpdateCheckForItemPlacement(this, gameTime);

            // Update RTS Interface Tiles
            //  ** Checks for mouse clicks on Interface Display
            //  ** Updates Round Meter Bar
            UpdateInterfaceDisplay(gameTime);

            // 11/7/2008 - TODO: Debug purposes
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.IFDUpdate);//"IFDUpdate"

            base.Update(gameTime);
        }

        /// <summary>
        /// Renders the <see cref="IFDTile"/> instances.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public override sealed void Draw(GameTime gameTime)
        {
#if DEBUG
            // 5/26/2010 - DEBUG
            StopWatchTimers.StartStopWatchInstance(StopWatchName.GameDrawLoop_Main_IFDTiles);
#endif
            //Render RTS Interface Tiles
            RenderInterFaceDisplay(gameTime);

            base.Draw(gameTime);

#if DEBUG
            // 5/26/2010 - DEBUG
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.GameDrawLoop_Main_IFDTiles);
#endif
        }


        /// <summary>
        /// Adds a new <see cref="IFDTile"/> to the Dictionary.
        /// </summary>
        /// <param name="subControlQueueKey"><see cref="SubQueueKey"/> structure</param>
        /// <param name="tileToAdd"><see cref="IFDTile"/> instance to add</param>
        private static void AddInterFaceDisplayTile(ref SubQueueKey subControlQueueKey, IFDTile tileToAdd)
        {
            // 1st - Let's check if Group already exist
            List<IFDTile> tmpIFDList;
            if (_ifdTileGroups.TryGetValue(subControlQueueKey.InstanceKey, out tmpIFDList))
            {
                // Yes, so let's add new tile to current group
                tmpIFDList.Add(tileToAdd);

                // 3/25/2009 - Gets the index to where the tile was placed.
                //int indexOf = tmpIFDList.IndexOf(tileToAdd);

                // Save back into Dictionary
                _ifdTileGroups[subControlQueueKey.InstanceKey] = tmpIFDList;

                return;
            }

            // No, so let's create it now.
            tmpIFDList = new List<IFDTile> { tileToAdd }; // Add new tile

            // 3/25/2009 - Gets the index to where the tile was placed.
            //int indexOf = tmpIFDList.IndexOf(tileToAdd);

            // Add to Dictionary
            _ifdTileGroups.Add(subControlQueueKey.InstanceKey, tmpIFDList);

            return;
        }

        // Overload 1
        /// <summary>
        /// Adds a new <see cref="IFDTile"/> tile to the 'Display-Always' collection. This is a tile
        /// which will always be displayed.
        /// </summary>        
        /// <param name="tileToAdd"><see cref="IFDTile"/> to add</param>   
        public static void AddInterFaceDisplayTile(IFDTile tileToAdd)
        {
            // Add to PermTiles for Display purposes.
            _ifdTilesPerm.Add(tileToAdd);
        }

        // 1/1/2010
        /// <summary>
        /// Adds a new <see cref="IFDTile"/> tile to the 'Display-Always' collection. This is a tile
        /// which will always be displayed.
        /// </summary>        
        /// <param name="tileTextureName"><see cref="IFDTile"/> to add</param>
        /// <param name="tileLocation"><see cref="Rectangle"/> as tile location </param>  
        /// <returns>Returns the tile's unique instance key.</returns>
        public int AddInterFaceDisplayTileOverlay(string tileTextureName, Rectangle tileLocation)
        {
            var ifdWrapper = new IFDTileOverlay(Game, tileTextureName, tileLocation);
            AddInterFaceDisplayTile(ifdWrapper);

            return ifdWrapper.TileInstanceKey;
        }

        // 2/25/2011 - Overload#1
        /// <summary>
        /// Adds a new <see cref="IFDTile"/> tile to the 'Display-Always' collection. This is a tile
        /// which will always be displayed.
        /// </summary>        
        /// <param name="tileTexture"><see cref="IFDTile"/> texture to add</param>
        /// <param name="tileLocation"><see cref="Rectangle"/> as tile location </param>  
        /// <returns>Returns the tile's unique instance key.</returns>
        public int AddInterFaceDisplayTileOverlay(Texture2D tileTexture, Rectangle tileLocation)
        {
            var ifdWrapper = new IFDTileOverlay(Game, tileTexture, tileLocation);
            AddInterFaceDisplayTile(ifdWrapper);

            return ifdWrapper.TileInstanceKey;
        }

        /// <summary>
        /// Explicit implementation for the interface <see cref="IMinimapInterfaceDisplay"/>.
        /// </summary>
        /// <param name="tileInstanceKey">The tile's unique instance key.</param>
        void IMinimapInterfaceDisplay.RemoveInterFaceDisplayTile(int tileInstanceKey)
        {
            RemoveInterFaceDisplayTile(tileInstanceKey);
        }

       

        // 12/8/2008
        /// <summary>
        /// Adds an <see cref="IFDTilePlacement"/> instance clicked, to the Queue.
        /// </summary>
        /// <param name="subControlKey"><see cref="SubQueueKey"/> structure</param>
        /// <param name="tileToAdd"><see cref="IFDTilePlacement"/> instance to add</param>        
        public static void AddTileClickedToQueue(ref SubQueueKey subControlKey, IFDTilePlacement tileToAdd)
        {
            // 1st - Let's check if Group already exist
            Queue<IFDTilePlacement> tmpTilesClickedQueue;
            if (_ifdTilesClicked.TryGetValue(subControlKey.InstanceKey, out tmpTilesClickedQueue))
            {
                // Yes, so let's add new tile to current group
                tmpTilesClickedQueue.Enqueue(tileToAdd);

                // Save back into Dictionary
                _ifdTilesClicked[subControlKey.InstanceKey] = tmpTilesClickedQueue;
            }
            else
            {
                // No, so let's create it now.
                tmpTilesClickedQueue = new Queue<IFDTilePlacement>();

                // Add new tile
                tmpTilesClickedQueue.Enqueue(tileToAdd);

                // Add to Dictionary
                _ifdTilesClicked.Add(subControlKey.InstanceKey, tmpTilesClickedQueue);
            }
        }


        /// <summary>
        /// Adds an <see cref="IFDTileGroupControl"/> to the 'Display-Always' collection. This is a tile
        /// which will always display.
        /// </summary>        
        /// <param name="tileToAdd">The IFDTile to add</param>        
        private static void AddGroupControlTile(IFDTileGroupControl tileToAdd)
        {
            // Add to PermTiles for Display purposes.
            _ifdTilesPerm.Add(tileToAdd);

            // 11/4/2008 - Add to Dictionary, for adding SubQueue purposes.            
            _ifdGroupControlTiles.Add((int)tileToAdd.GroupControlType, tileToAdd);
        }

        private static int _searchInstanceKey;

        // 10/6/2008-
        /// <summary>
        /// Removes an <see cref="IFDTile"/>, using the
        /// given <paramref name="tileInstanceKey"/> as the search criteria.
        /// </summary>
        /// <param name="tileInstanceKey"><see cref="IFDTile"/> instance Key</param>       
        public static void RemoveInterFaceDisplayTile(int tileInstanceKey)
        {
            // Set Key to use for Predicate search
            _searchInstanceKey = tileInstanceKey;

            // 1st - Check Perm tiles
            if (_ifdTilesPerm != null)
                _ifdTilesPerm.RemoveAll(HasIFDInstanceKey);

            // 2nd - Check Current tiles
            if (_ifdTiles != null)
                _ifdTiles.RemoveAll(HasIFDInstanceKey);

            // Finally - Check All Group Control tiles
            // Get Dictionary Keys
            var ifdTileGroupsCount = _ifdTileGroups.Keys.Count; // 7/27/2009
            if (_groupKeys.Length < ifdTileGroupsCount)
            {
                Array.Resize(ref _groupKeys, ifdTileGroupsCount);
            }
            _ifdTileGroups.Keys.CopyTo(_groupKeys, 0);

            // TODO: Is the J loop necessary?
            // Iterate Dictionary using Keys.
            for (var i = 0; i < ifdTileGroupsCount; i++)
            {
                // Loop through List of given Key
                for (var j = 0; j < _ifdTileGroups[_groupKeys[i]].Count; j++)
                {
                    _ifdTileGroups[_groupKeys[i]].RemoveAll(HasIFDInstanceKey);
                }
            }
        }

        // 10/6/2008 - 
        /// <summary>
        /// Predicate search <see cref="Delegate"/>, used in the List arrays, which 
        /// returns True/False for finding given <see cref="IFDTile.TileInstanceKey"/>.
        /// </summary>
        /// <param name="tileToSearch">IFDTile</param>
        /// <returns>Boolean Value</returns>
        private static bool HasIFDInstanceKey(IFDTile tileToSearch)
        {
            return tileToSearch.TileInstanceKey == _searchInstanceKey;
        }

        // 11/9/2009
        /// <summary>
        /// Removes any <see cref="IFDTile"/> groups from display.
        /// </summary>
        public static void DeActivateCurrentDisplayGroup()
        {
#if XBOX360
            _ifdTiles = null;
            // 4/30/2009 - XBOX Only - Keeps Input checking from occuring for IFDTiles.
            IFDTileSetIsDisplaying = false;

            // 11/9/2009 - XBOX Only - Unlock Camera movement.
            Camera.LockAll = false;
#endif
        }

        // 9/24/2008 - 
        /// <summary>
        /// Sets the given <see cref="IFDTile"/> group as the current group to display, 
        /// using the <see cref="SubQueueKey"/> to locate.
        /// </summary>
        /// <param name="subQueueKey"><see cref="SubQueueKey"/> structure</param>
        public static void SetAsCurrentGroupToDisplay(ref SubQueueKey subQueueKey)
        {
            // If Group exists, then set pointer of 'ifd_tilescurrent' to the group
            if (_ifdTileGroups.ContainsKey(subQueueKey.InstanceKey))
            {
                _ifdTiles = _ifdTileGroups[subQueueKey.InstanceKey];

#if XBOX360
                // 4/30/2009 - XBOX Only - Keeps Input checking from occuring for IFDTiles.
                IFDTileSetIsDisplaying = true;

                // 11/9/2009 - XBOX Only - Lock Camera movement.
                Camera.LockAll = true;
#endif

                return;
            }

            // 11/9/2009 - Default to off, if no group found.
            DeActivateCurrentDisplayGroup();
        }

        // 10/9/2009
        /// <summary>
        /// Sets the given <see cref="ItemGroupType"/> as the current group to display, as
        /// well as sets the <see cref="IFDTileGroupControl.ActiveGroup"/> to be equal to the proper 
        /// <see cref="IFDGroupControlType"/> of the given building.
        /// </summary>
        /// <param name="buildingScene"><see cref="BuildingScene"/> instance</param>
        public static void SetAsCurrentGroupToDisplay(BuildingScene buildingScene)
        {
            if (buildingScene.ProductionType != null)
            {
                // set the ActiveGroup type, using the ProductionType set in buildingScene as guide.
                switch (buildingScene.ProductionType.Value)
                {
                    case ItemGroupType.Buildings:
                        IFDTileGroupControl.ActiveGroup = IFDGroupControlType.Buildings;
                        break;
                    case ItemGroupType.Shields:
                        IFDTileGroupControl.ActiveGroup = IFDGroupControlType.Shields;
                        break;
                    case ItemGroupType.People:
                        IFDTileGroupControl.ActiveGroup = IFDGroupControlType.People;
                        break;
                    case ItemGroupType.Vehicles:
                        IFDTileGroupControl.ActiveGroup = IFDGroupControlType.Vehicles;
                        break;
                    case ItemGroupType.Airplanes:
                        IFDTileGroupControl.ActiveGroup = IFDGroupControlType.Airplanes;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            } // end if not Null

            // call base overload, with subQueue key.
            SetAsCurrentGroupToDisplay(ref buildingScene.SubQueueKeyIFDTiles);
        }

        // 10/22/2009
        /// <summary>
        /// Set the given <see cref="IFDGroupControlType"/> as the current group to display. (Scripting Purposes)
        /// </summary>
        /// <param name="groupControlTypeToDisable"><see cref="IFDGroupControlType"/> Enum</param>
        public static void SetAsCurrentGroupToDisplay(IFDGroupControlType groupControlTypeToDisable)
        {
            // verify NOT NULL
            if (_ifdTileRetrieveGroups == null)
                return;

            // 1st - Retrieve List of SubQueueKeys for given GroupControlType.
            List<SubQueueKey> subQueueKeys;
            if (!_ifdTileRetrieveGroups.TryGetValue((int)groupControlTypeToDisable, out subQueueKeys)) return;

            // Now Display the 1st Queue in list.
            var subQueueKey = subQueueKeys[0];
            SetAsCurrentGroupToDisplay(ref subQueueKey);

        }

        // 10/11/2009
        /// <summary>
        /// Set to Enable/Disable the ability to use a group of <see cref="IFDTile"/> belonging to a
        /// given <see cref="IFDGroupControlType"/> Enum; for example, 'Buildings'.
        /// </summary>
        /// <param name="groupControlTypeToDisable">Enum <see cref="IFDGroupControlType"/> to affect</param>
        /// <param name="tileIsUseable">To either Enable or Disable group</param>
        public static void SetAbilityToBuildGroupControlType(IFDGroupControlType groupControlTypeToDisable, bool tileIsUseable)
        {
            // verify NOT NULL
            if (_ifdTileRetrieveGroups == null)
                return;

            // 1st - Retrieve List of SubQueueKeys for given GroupControlType.
            List<SubQueueKey> subQueueKeys;
            if (!_ifdTileRetrieveGroups.TryGetValue((int)groupControlTypeToDisable, out subQueueKeys)) return;

            // iterate List of SubQueueKeys, then Tiles to Disable.
            var count = subQueueKeys.Count;
            for (var i = 0; i < count; i++)
            {
                // retrieve key
                var subQueueKey = subQueueKeys[i];

                // use key to retrieve IFD Group.
                List<IFDTile> group;
                if (!_ifdTileGroups.TryGetValue(subQueueKey.InstanceKey, out group))
                    continue;

                // iterate group
                var count1 = group.Count;
                for (var j = 0; j < count1; j++)
                {
                    // cache
                    var tile = group[j];

                    // make sure not Null
                    if (tile == null)
                        continue;

                    // Set Property 'TileIsUseable'
                    tile.TileIsUseable = tileIsUseable;

                    // update back to list
                    group[j] = tile;

                } // End ForLoop
            } // End ForLoop
        }

        // 10/11/2009
        /// <summary>
        /// Set to Enable/Disable a specific <see cref="ItemType"/> tile from being useable; for example,
        /// the 'PowerPlant' building.
        /// </summary>
        /// <param name="groupControlTypeToDisable">Enum <see cref="IFDGroupControlType"/> tile belongs to</param>
        /// <param name="itemType"><see cref="ItemType"/> Enum tile to affect</param>
        /// <param name="tileIsUseable">To either Enable or Disable tile</param>
        public static void SetAbilityToBuildSpecificItemType(IFDGroupControlType groupControlTypeToDisable, ItemType itemType, bool tileIsUseable)
        {
            // verify NOT NULL
            if (_ifdTileRetrieveGroups == null)
                return;

            // 1st - Retrieve List of SubQueueKeys for given GroupControlType.
            List<SubQueueKey> subQueueKeys;
            if (!_ifdTileRetrieveGroups.TryGetValue((int)groupControlTypeToDisable, out subQueueKeys)) return;

            // iterate List of SubQueueKeys, then Tiles to Disable.
            var count = subQueueKeys.Count;
            for (var i = 0; i < count; i++)
            {
                // retrieve key
                var subQueueKey = subQueueKeys[i];

                // use key to retrieve IFD Group.
                List<IFDTile> group;
                if (!_ifdTileGroups.TryGetValue(subQueueKey.InstanceKey, out group))
                    continue;

                // iterate group
                var count1 = group.Count;
                for (var j = 0; j < count1; j++)
                {
                    // cast as IFDTile_Placement
                    var tile = group[j] as IFDTilePlacement;

                    // make sure not Null
                    if (tile == null)
                        continue;

                    // check if the proper 'ItemType' user wants to disable
                    if (tile.ItemTypeToUse != itemType) continue;

                    // Set Property 'TileIsUseable'
                    tile.TileIsUseable = tileIsUseable;

                    // update back to list
                    group[j] = tile;
                } // End ForLoop

            } // End ForLoop
        }

        // 10/22/2009
        /// <summary>
        /// Set to Enable/Disable a specific <see cref="ItemType"/> tile to Flash; for example,
        /// the 'PowerPlant' building tile.
        /// </summary>
        /// <param name="groupControlTypeToDisable">Enum <see cref="IFDGroupControlType"/> tile belongs to</param>
        /// <param name="itemType"><see cref="ItemType"/> tile to affect</param>
        /// <param name="flashTile">To either Enable/Disable Flashing</param>
        public static void SetToFlashSpecificItemType(IFDGroupControlType groupControlTypeToDisable, ItemType itemType, bool flashTile)
        {
            // verify NOT NULL
            if (_ifdTileRetrieveGroups == null)
                return;

            // 1st - Retrieve List of SubQueueKeys for given GroupControlType.
            List<SubQueueKey> subQueueKeys;
            if (!_ifdTileRetrieveGroups.TryGetValue((int)groupControlTypeToDisable, out subQueueKeys)) return;

            // ONLY extract SubQueue#1, since used for Tutorial Use in scripting.
            // retrieve key
            var subQueueKey = subQueueKeys[0];

            // use key to retrieve IFD Group.
            List<IFDTile> group;
            if (!_ifdTileGroups.TryGetValue(subQueueKey.InstanceKey, out group)) return;

            // iterate group
            var count = group.Count;
            for (var j = 0; j < count; j++)
            {
                // cast as IFDTile_Placement
                var tile = group[j] as IFDTilePlacement;

                // make sure not Null
                if (tile == null)
                    continue;

                // check if the proper 'ItemType' user wants to FLASH
                if (tile.ItemTypeToUse != itemType) continue;

                // Set Property 'FlashTile'
                tile.FlashTile = flashTile;

                // update back to list
                group[j] = tile;
            } // End ForLoop
        }

        // 2/23/2011
        /// <summary>
        /// Sets the visibility for the given <paramref name="tileInstanceKey"/>.
        /// </summary>
        /// <param name="tileInstanceKey">Tile's instance key.</param>
        /// <param name="isVisible">Visibility setting.</param>
        public static void SetVisibility(int tileInstanceKey, bool isVisible)
        {
            // Finally - Check All Group Control tiles
            // Get Dictionary Keys
            var ifdTileGroupsCount = _ifdTileGroups.Keys.Count; // 7/27/2009
            if (_groupKeys.Length < ifdTileGroupsCount)
            {
                Array.Resize(ref _groupKeys, ifdTileGroupsCount);
            }
            _ifdTileGroups.Keys.CopyTo(_groupKeys, 0);

            // TODO: Is the J loop necessary?
            // Iterate Dictionary using Keys.
            for (var i = 0; i < ifdTileGroupsCount; i++)
            {
                // Loop through List of given Key
                for (var j = 0; j < _ifdTileGroups[_groupKeys[i]].Count; j++)
                {
                    var ifdTile = _ifdTileGroups[_groupKeys[i]][j]; // cache
                    if (ifdTile.TileInstanceKey != tileInstanceKey) continue;

                    ifdTile.DrawTile = isVisible;
                    return;
                }
            }
        }

        // 10/11/2009
        /// <summary>
        /// Retrieves a List of <see cref="SubQueueKey"/> structs for a Group, by the <see cref="IFDGroupControlType"/> Enum.  This is useful
        /// when you need to access a <see cref="IFDTile"/> group, but don't know the <see cref="SubQueueKey"/> structure.
        /// </summary>
        /// <param name="groupControlToGet"><see cref="IFDGroupControlType"/> Enum</param>
        /// <param name="subQueueKeys">(OUT) Collection of <see cref="SubQueueKey"/> for group</param>
        /// <returns>True/False of result</returns>
        public static bool GetGroupSubQueueKeyByGroupControlType(IFDGroupControlType groupControlToGet, out List<SubQueueKey> subQueueKeys)
        {
            // try to retrieve value from Dictionary
            return _ifdTileRetrieveGroups.TryGetValue((int)groupControlToGet, out subQueueKeys);
        }

        // 10/11/2009
        /// <summary>
        /// Helper method, which will add a given <see cref="IFDGroupControlType"/> Enum and given <see cref="SubQueueKey"/>, to the
        /// internal Dictionary <see cref="_ifdTileRetrieveGroups"/>, to create a 1-1 relationship between the Enum and Key.
        /// </summary>
        /// <param name="groupControl"><see cref="IFDGroupControlType"/> Enum</param>
        /// <param name="subQueueKey"><see cref="SubQueueKey"/> structure</param>
        private static void AddGroupSubQueueKeyToRetrievesDictionary(IFDGroupControlType groupControl, SubQueueKey subQueueKey)
        {
            // add key to Dictionary
            if (!_ifdTileRetrieveGroups.ContainsKey((int)groupControl))
            {
                // Create new List.
                var subQueueKeys = new List<SubQueueKey> { subQueueKey /* Add Key */};

                // Add List<SubQueueKey> to Dictionary
                _ifdTileRetrieveGroups.Add((int)groupControl, subQueueKeys);

            }
            else // Update List
            {
                // Retrieve List from Dictionary
                var subQueueKeys = _ifdTileRetrieveGroups[(int)groupControl];

                // Add new key
                subQueueKeys.Add(subQueueKey);

                // Store List back into Dictionary
                _ifdTileRetrieveGroups[(int)groupControl] = subQueueKeys;
            }
        }

        // 11/4/2008
        /// <summary>
        /// Adds a new <see cref="BuildingScene"/> Sub-Queue <see cref="IFDTileSubGroupControl"/> tab 
        /// for current <see cref="ItemGroupType"/> Enum.
        /// </summary>
        /// <param name="owner"><see cref="BuildingScene"/> owner reference</param>
        /// <param name="productionType">Production <see cref="ItemGroupType"/> Sub-Queue</param>
        /// <param name="subQueueKey">(OUT) <see cref="SubQueueKey"/> structure</param>
        public static void AddNewBuildingQueueTab(BuildingScene owner, ItemGroupType productionType,
                                                  out SubQueueKey subQueueKey)
        {
            // Add Building Queue to given GroupControl tab.
            _ifdGroupControlTiles[(int)productionType].AddNewBuildingQueueTab(owner, out subQueueKey);
        }

        // 12/9/2008 - Currently called from IFDtile_SubGroupControl.
        /// <summary>
        /// Removes a <see cref="BuildingScene"/> Sub-Queue tab from the current <see cref="ItemGroupType"/> Enum.
        /// </summary>
        /// <param name="subQueueKey"><see cref="SubQueueKey"/> structure</param>
        /// <param name="productionType">Production <see cref="ItemGroupType"/> Sub-Queue</param>
        public static void RemoveBuildingQueueTab(ref SubQueueKey subQueueKey, ItemGroupType productionType)
        {
            // Remove Building Queue from given GroupControl tab.
            _ifdGroupControlTiles[(int)productionType].RemoveBuildingQueueTab(ref subQueueKey);

            // Also need to remove the tiles themselves from view
            for (var i = 0; i < _ifdTileGroups[subQueueKey.InstanceKey].Count; i++)
            {
                // 7/27/2009 - Check 'TilesClickedCount' for each IFDtile, and remove from TotalCount in IFDTile_Placement class.
                var tile = (_ifdTileGroups[subQueueKey.InstanceKey][i] as IFDTilePlacement);
                if (tile != null && tile.TileClicksCount > 0)
                {
                    IFDTilePlacement.ReduceTotalQueuedCountForBuildingType(productionType, tile.TileClicksCount, TemporalWars3DEngine.SThisPlayer);
                }

                _ifdTileGroups[subQueueKey.InstanceKey][i].Dispose(false);
                // false = does not remove static variables when disposing!
            }
            _ifdTileGroups[subQueueKey.InstanceKey].Clear(); // Clear List
            _ifdTileGroups.Remove(subQueueKey.InstanceKey); // Clear From Dictionary
        }

        /// <summary>
        /// Manages rendering of all <see cref="IFDTile"/> groups, starting first with
        /// the 'Display-Always' group, then regular current group, followed with the 
        /// <see cref="InterFaceRoundMeter"/> draws.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private static void RenderInterFaceDisplay(GameTime gameTime)
        {
            // 1/6/2009 - Draw Placement-Overlay SelectionBoxes
            if (AttemptingItemPlacement)
                // XNA 4.0 Updates - Final 2 params updated.
                TriangleShapeHelper.DrawPrimitiveTriangle(ref _selectionBoxes, _rasterizerState, _depthStencilState);

            // Call Batch Begin
            IFDTile.SpriteBatchBegin();

            // 5/1/2009 - Reset Boolean flag.
            CursorInSomeIFDTile = false;

            // 10/6/2008 - Draw Perm tiles first            
            var ifdTilesPerm = _ifdTilesPerm; // 6/1/2010 - Cache
            if (ifdTilesPerm != null) // 11/9/2009
            {
                var ifdTilesPermCount = ifdTilesPerm.Count; // 11/9/2009
                for (var i = 0; i < ifdTilesPermCount; i++)
                {
                    // 11/9/2009 - cache
                    var tile = ifdTilesPerm[i];
                    if (tile == null) continue; // 11/9/2009 - check Null.

                    // 11/27/2009 - If XBOX & GroupControl, then skip drawing item.
                    if ((TemporalWars3DEngine.CurrentPlatform == PlatformID.Xbox) && (tile is IFDTileGroupControl))
                        continue;

                    // Render IFD Tile
                    tile.RenderInterFaceTile(gameTime);

                    // 5/1/2009 - Set global flag.
                    if (tile.CursorInsideIFDTile)
                        CursorInSomeIFDTile = true;
                }
            }

            // Iterate through all IFD Tiles and display them.
            if (_ifdTiles != null) // 11/9/2009
            {
                var ifdTiles = _ifdTiles; // 11/9/2009 - Cache to fix error when 'IFDTiles' is Nulled!
                var ifdTilesCount = ifdTiles.Count; // 11/9/2009

                for (var i = 0; i < ifdTilesCount; i++)
                {
                    // 11/9/2009 - cache
                    var tile = ifdTiles[i];

                    // 11/9/2009 - check Null.
                    if (tile == null)
                        continue;

                    // Render IFD Tile
                    tile.RenderInterFaceTile(gameTime);

                    // 5/1/2009 -  Set global flag.
                    if (tile.CursorInsideIFDTile)
                        CursorInSomeIFDTile = true;
                }
            } // End If IFDTile == null

            // Call Batch End
            IFDTile.SpriteBatchEnd();
            
            // 6/1/2010 - Refactor code to new method.
            RenderRoundMeters(); 
           
        }

        // 6/1/2010
        /// <summary>
        /// Iterates <see cref="IFDTiles"/>, calling render for the <see cref="IFDTilePlacement.RoundMeter"/>.
        /// </summary>
        private static void RenderRoundMeters()
        {
            if (_ifdTiles == null) return;

            var ifdTiles = _ifdTiles; // 11/9/2009 - Cache to fix error when 'IFDTiles' is Nulled!
            var ifdTilesCount = ifdTiles.Count; // 11/9/2009

            // 9/28/2008 - Draw Round Meter's
            for (var i = 0; i < ifdTilesCount; i++)
            {
                var ifdTilePlacement = ifdTiles[i] as IFDTilePlacement;
                if (ifdTilePlacement == null) continue;

                ifdTilePlacement.RoundMeter.Draw();
            }
        }

        // 10/31/2008; 4/8/2009: Updated to include the PlayerSide param.
        /// <summary>
        /// Creates the default set of <see cref="IFDTile"/> to be shown on screen.
        /// </summary>
        /// <param name="playerSide"><see cref="Player"/> side</param>
        public void CreateIFDTiles(int playerSide)
        {
            // 1/5/2009 - Wait until PreLoad Thread done! Otherwise, XNA crash can occur if we
            //            try to add a texture here, while the same texture is being added in 
            //            the PreLoad!
            //_preLoadTileTexturesThread.Join();            

            // Create the IFD Control Group Tile set.
            CreateIFDTiles(IFDGroupControlType.ControlGroup1, null, null, 0);

            // Initialize IFD Tiles 'Building' set
            // 11/4/2008 - Let's add at least one Production Queue for Buildings.
            SubQueueKey subQueueKey;


            // 12/24/2008 - Add Defensive 'Shields' set.
            AddNewBuildingQueueTab(null, ItemGroupType.Shields, out subQueueKey);
            CreateIFDTiles(IFDGroupControlType.Shields, subQueueKey, null, playerSide);

            // 4/21/2011 - Fixed spacing from edges from 0 to 40 to avoid screen clipping.
            // 4/6/2010: Updated to use 'ContentTexturesLoc' global var.
            // 1/5/2009 - Add cash card for player.
            _cashMessageTile = new IFDTileMessage(Game, "0", new Rectangle(80, 40, 200, 100), true)
            {
                SbMessageToDisplay = { Capacity = 10 },
                BackgroundTexture =
                    TemporalWars3DEngine.GameInstance.Content.Load<Texture2D>(
                    TemporalWars3DEngine.ContentTexturesLoc + @"\Textures\scoreBoard"),
                MessageOrigin = new Vector2(-50, -20)
            };
            // 2/22/2009 - Set 'scoreBoard' as background to use for message.
            AddInterFaceDisplayTile(_cashMessageTile);

            // 4/21/2011 - Fixed spacing from edges from 0 to 40 to avoid screen clipping.
            // 1/30/2009 - Add energy card for player.
            _energyMessageTile = new IFDTileMessage(Game, "0", new Rectangle(80, 40, 200, 100), true)
            {
                SbMessageToDisplay = { Capacity = 10 },
                MessageOrigin = new Vector2(-50, -65),
                DrawBackground = false
            };
            AddInterFaceDisplayTile(_energyMessageTile);

            // 4/30/2009 - Only set on PC, not XBOX.
#if !XBOX360
            // Set Building Set to render.            
            _ifdGroupControlTiles[(int)IFDGroupControlType.Buildings].SetAsCurrentGroupToDisplay();
#endif
        }

        // 10/11/2009: Updated to add to new Dictionary '_ifdTilesREtrieveGroups'.
        // 9/24/2008 - Creates a group of IFD Tiles; 4/7/2009: Updated to include the 'PlayerSide'.
        /// <summary>
        /// Creates the given <see cref="IFDGroupControlType"/> type; for example, the 'Buildings' set.
        /// </summary>
        /// <remarks>Some <see cref="IFDGroupControlType"/> types also require the <see cref="BuildingScene"/> to be set.</remarks>
        /// <param name="tileGroup"><see cref="IFDGroupControlType"/> Enum to create</param>
        /// <param name="subQueueKey"><see cref="SubQueueKey"/> structure (Optional ONLY for 'ControlGroup1' <see cref="IFDGroupControlType"/> Enum.)</param>
        /// <param name="buildingItem"><see cref="BuildingScene"/> instance</param>
        /// <param name="playerSide"><see cref="Player"/> side</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="subQueueKey"/> is Null for most <see cref="IFDGroupControlType"/> Enums.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="tileGroup"/> is not valid.</exception>
        internal void CreateIFDTiles(IFDGroupControlType tileGroup, SubQueueKey? subQueueKey, BuildingScene buildingItem,
                                     int playerSide)
        {
            switch (tileGroup)
            {
                case IFDGroupControlType.ControlGroup1:
                    CreateIFDTileControlGroup();
                    break;
                case IFDGroupControlType.Buildings:
                    // 4/19/2010 - Check if Required SubQueueKey is null?
                    if (subQueueKey == null)
                        throw new ArgumentNullException("subQueueKey", @"For given 'Buildings' type, the SubQueueKey is required.");

                    switch (playerSide)
                    {
                        case 1:
                            CreateIFDTileBuildingsGroup_Set1(subQueueKey.Value);
                            break;
                        case 2:
                            CreateIFDTileBuildingsGroup_Set2(subQueueKey.Value);
                            break;
                    }
                    break;
                case IFDGroupControlType.Shields:
                    // 4/19/2010 - Check if Required SubQueueKey is null?
                    if (subQueueKey == null)
                        throw new ArgumentNullException("subQueueKey", @"For given 'Shields' type, the SubQueueKey is required.");

                    switch (playerSide)
                    {
                        case 1:
                            CreateIFDTileShieldsGroup_Set1(subQueueKey.Value); // 12/24/2008
                            break;
                        case 2:
                            CreateIFDTileShieldsGroup_Set2(subQueueKey.Value);
                            break;
                    }
                    break;
                case IFDGroupControlType.People:
                    break;
                case IFDGroupControlType.Vehicles:
                    // 4/19/2010 - Check if Required SubQueueKey is null?
                    if (subQueueKey == null)
                        throw new ArgumentNullException("subQueueKey", @"For given 'Vehicles' type, the SubQueueKey is required.");


                    switch (playerSide)
                    {
                        case 1:
                            CreateIFDTileTanksGroup_Set1(subQueueKey.Value, buildingItem);
                            break;
                        case 2:
                            CreateIFDTileTanksGroup_Set2(subQueueKey.Value, buildingItem);
                            break;
                    }
                    break;
                case IFDGroupControlType.Airplanes:
                    // 4/19/2010 - Check if Required SubQueueKey is null?
                    if (subQueueKey == null)
                        throw new ArgumentNullException("subQueueKey", @"For given 'Airplanes' type, the SubQueueKey is required.");

                    switch (playerSide)
                    {
                        case 1:
                            CreateIFDTileAirplanesGroup_Set1(subQueueKey.Value, buildingItem);
                            break;
                        case 2:
                            CreateIFDTileAirplanesGroup_Set2(subQueueKey.Value, buildingItem);
                            break;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("tileGroup");
            }

            // 10/11/2009 - Add Relationship to internal Dictionary, between IFDGroup enum and SubQueueKey.
            if (subQueueKey != null)
                AddGroupSubQueueKeyToRetrievesDictionary(tileGroup, subQueueKey.Value);

        }

        // 2/3/2009 - 
        /// <summary>
        /// Specifically creates the <see cref="IFDTile"/> 'Airplane' Group, for side-1.
        /// </summary>
        /// <param name="subQueueKey"><see cref="SubQueueKey"/> structure</param>
        /// <param name="buildingItem"><see cref="BuildingScene"/> instance</param>
        private void CreateIFDTileAirplanesGroup_Set1(SubQueueKey subQueueKey, BuildingScene buildingItem)
        {
            var tileLocation = new Rectangle(5, 5, 1, 1);

            // Create Tile 1            
            var ifdTile = new IFDTilePlacement(Game, "SciFi_Heli01_Pic",
                                                TilePlacement.Pos1, ItemType.sciFiHeli01, subQueueKey, true);

            // 3/25/2009 - Attach a Message Tile.
            var ifdMessageTile = new IFDTileMessage(Game, ifdTile, ItemType.sciFiHeli01, tileLocation, false);
            ifdTile.AddMessageTile(ifdMessageTile);


            // Attach EventHandler for ItemCreated.
            ifdTile.ProductionBuilding = buildingItem;
            ifdTile.ItemCreateRequest += _thisPlayer.IFDPlacement_ItemCreated;
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);

            // Create Tile 2
            ifdTile = new IFDTilePlacement(Game, "SciFi_Bomber06_Pic",
                                            TilePlacement.Pos2, ItemType.sciFiBomber06, subQueueKey, true);

            // 3/25/2009 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, ifdTile, ItemType.sciFiBomber06, tileLocation, false);
            ifdTile.AddMessageTile(ifdMessageTile);

            // 5/25/2009 - Disable Tile, and attack to be triggered by 'SpecialTechBuilding' global event.
            ifdTile.TileState =
                Player.IsSpecialBuildingPlaced(_thisPlayer,
                                               "Research Center-B2")
                    ? TileState.None
                    : TileState.Disabled;
            ifdTile.SpecialBuildingName1 = "Research Center-B2";
            BuildingScene.SpecialBuildingCreated += ifdTile.SpecialBuildingCreated_EventHandler;
            BuildingScene.SpecialBuildingDestroyed += ifdTile.SpecialBuildingDestroyed_EventHandler;


            // Attach EventHandler for ItemCreated.
            ifdTile.ProductionBuilding = buildingItem;
            ifdTile.ItemCreateRequest += _thisPlayer.IFDPlacement_ItemCreated;
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);


            // Create Tile 3
            ifdTile = new IFDTilePlacement(Game, "SciFi_Gunship02_Pic",
                                            TilePlacement.Pos3, ItemType.sciFiGunShip02, subQueueKey, true);

            // 3/25/2009 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, ifdTile, ItemType.sciFiGunShip02, tileLocation, false);
            ifdTile.AddMessageTile(ifdMessageTile);

            // Attach EventHandler for ItemCreated.
            ifdTile.ProductionBuilding = buildingItem;
            ifdTile.ItemCreateRequest += _thisPlayer.IFDPlacement_ItemCreated;
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);

            // 4/30/2009 - XBOX Only - add the FlagMarker Tile.
#if XBOX360


#endif
            // Create Tile 8 (FlagMarker)
            ifdTile = new IFDTilePlacement(Game, "FlagMarker",
                                            TilePlacement.Pos8, ItemType.flagMarker,
                                            subQueueKey, false);

            // 3/25/2009 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, ifdTile, ItemType.flagMarker, tileLocation, false);
            ifdTile.AddMessageTile(ifdMessageTile);
            ifdTile.IsPlacementFlag = true; // 5/19/2009

            //Attach EventHandler for ItemCreated.       
            ifdTile.ProductionBuilding = buildingItem;
            ifdTile.CreateItemToPlace = CreateBuildingSceneItem;
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);
        }

        // 3/11/2009
        /// <summary>
        /// Specifically creates the <see cref="IFDTile"/> 'Airplane' Group for side-2.
        /// </summary>
        /// <param name="subQueueKey"><see cref="SubQueueKey"/> structure</param>
        /// <param name="buildingItem"><see cref="BuildingScene"/> instance</param>
        private void CreateIFDTileAirplanesGroup_Set2(SubQueueKey subQueueKey, BuildingScene buildingItem)
        {
            var tileLocation = new Rectangle(5, 5, 1, 1);


            // Create Tile 1
            var ifdTile = new IFDTilePlacement(Game, "SciFi_Heli02_Pic",
                                                TilePlacement.Pos1, ItemType.sciFiHeli02, subQueueKey, true);

            // 3/25/2009 - Attach a Message Tile.
            var ifdMessageTile = new IFDTileMessage(Game, ifdTile, ItemType.sciFiHeli02, tileLocation, false);
            ifdTile.AddMessageTile(ifdMessageTile);


            // Attach EventHandler for ItemCreated.
            ifdTile.ProductionBuilding = buildingItem;
            ifdTile.ItemCreateRequest += _thisPlayer.IFDPlacement_ItemCreated;
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);

            // Create Tile 2
            ifdTile = new IFDTilePlacement(Game, "SciFi_Bomber01_Pic",
                                            TilePlacement.Pos2, ItemType.sciFiBomber01, subQueueKey, true);

            // 3/25/2009 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, ifdTile, ItemType.sciFiBomber01, tileLocation, false);
            ifdTile.AddMessageTile(ifdMessageTile);

            // 5/25/2009 - Disable Tile, and attack to be triggered by 'SpecialTechBuilding' global event.
            ifdTile.TileState =
                Player.IsSpecialBuildingPlaced(_thisPlayer,
                                               "Research Center-B4")
                    ? TileState.None
                    : TileState.Disabled;
            ifdTile.SpecialBuildingName1 = "Research Center-B4";
            BuildingScene.SpecialBuildingCreated += ifdTile.SpecialBuildingCreated_EventHandler;
            BuildingScene.SpecialBuildingDestroyed += ifdTile.SpecialBuildingDestroyed_EventHandler;

            // Attach EventHandler for ItemCreated.
            ifdTile.ProductionBuilding = buildingItem;
            ifdTile.ItemCreateRequest += _thisPlayer.IFDPlacement_ItemCreated;
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);


            // Create Tile 3
            ifdTile = new IFDTilePlacement(Game, "SciFi_Bomber07_Pic",
                                            TilePlacement.Pos3, ItemType.sciFiBomber07, subQueueKey, true);

            // 3/25/2009 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, ifdTile, ItemType.sciFiBomber07, tileLocation, false);
            ifdTile.AddMessageTile(ifdMessageTile);


            // Attach EventHandler for ItemCreated.
            ifdTile.ProductionBuilding = buildingItem;
            ifdTile.ItemCreateRequest += _thisPlayer.IFDPlacement_ItemCreated;
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);


            // Create Tile 8 (FlagMarker)
            ifdTile = new IFDTilePlacement(Game, "FlagMarker",
                                            TilePlacement.Pos8, ItemType.flagMarker,
                                            subQueueKey, false);

            // 3/25/2009 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, ifdTile, ItemType.flagMarker, tileLocation, false);
            ifdTile.AddMessageTile(ifdMessageTile);
            ifdTile.IsPlacementFlag = true; // 5/19/2009

            //Attach EventHandler for ItemCreated.       
            ifdTile.ProductionBuilding = buildingItem;
            ifdTile.CreateItemToPlace = CreateBuildingSceneItem;
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);
        }

        // 12/24/2008 -
        /// <summary>
        /// Specifically creates the <see cref="IFDTile"/> 'Shields' Group for side-1.
        /// </summary>
        /// <param name="subQueueKey"><see cref="SubQueueKey"/> structure</param>
        private void CreateIFDTileShieldsGroup_Set1(SubQueueKey subQueueKey)
        {
            // 6/2/2009: Updated to set the tile positions to 7/8 if XBOX.
#if (!XBOX)
            // Create Tile 1            
            var ifdTile = new IFDTilePlacement(Game, "SciFi_AAGun01_Pic",
                                                TilePlacement.Pos1, ItemType.sciFiAAGun01, subQueueKey, false) { CreateItemToPlace = CreateBuildingSceneItem };

            //Attach EventHandler for ItemCreated.            
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);

            // Create Tile 2          
            ifdTile = new IFDTilePlacement(Game, "SciFi_AAGun02_Pic",
                                            TilePlacement.Pos2, ItemType.sciFiAAGun02, subQueueKey, false) { CreateItemToPlace = CreateBuildingSceneItem };

            //Attach EventHandler for ItemCreated.            
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);
#else
    // Create Tile 1            
            var ifdTile = new IFDTilePlacement(Game, "SciFi_AAGun01_Pic",
                                        TilePlacement.Pos7, ItemType.sciFiAAGun01, subQueueKey, false)
                              {CreateItemToPlace = CreateBuildingSceneItem};

            //Attach EventHandler for ItemCreated.            
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);

            // Create Tile 2          
            ifdTile = new IFDTilePlacement(Game, "SciFi_AAGun02_Pic",
                                        TilePlacement.Pos8, ItemType.sciFiAAGun02, subQueueKey, false)
                          {CreateItemToPlace = CreateBuildingSceneItem};

            //Attach EventHandler for ItemCreated.            
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);
#endif
        }

        /// <summary>
        /// Specifically creates the <see cref="IFDTile"/> 'Shields' Group for side-2.
        /// </summary>
        /// <param name="subQueueKey"><see cref="SubQueueKey"/> structure</param>
        private void CreateIFDTileShieldsGroup_Set2(SubQueueKey subQueueKey)
        {
            // 6/2/2009: Updated to set the tile positions to 7/8 if XBOX.
#if (!XBOX)
            // Create Tile 1            
            var ifdTile = new IFDTilePlacement(Game, "SciFi_AAGun04_Pic",
                                                TilePlacement.Pos1, ItemType.sciFiAAGun04, subQueueKey, false) { CreateItemToPlace = CreateBuildingSceneItem };

            //Attach EventHandler for ItemCreated.            
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);

            // Create Tile 2          
            ifdTile = new IFDTilePlacement(Game, "SciFi_AAGun05_Pic",
                                            TilePlacement.Pos2, ItemType.sciFiAAGun05, subQueueKey, false) { CreateItemToPlace = CreateBuildingSceneItem };

            //Attach EventHandler for ItemCreated.            
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);
#else

    // Create Tile 1            
            var ifdTile = new IFDTilePlacement(Game, "SciFi_AAGun04_Pic",
                                        TilePlacement.Pos7, ItemType.sciFiAAGun04, subQueueKey, false)
                              {CreateItemToPlace = CreateBuildingSceneItem};

            //Attach EventHandler for ItemCreated.            
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);

            // Create Tile 2          
            ifdTile = new IFDTilePlacement(Game, "SciFi_AAGun05_Pic",
                                        TilePlacement.Pos8, ItemType.sciFiAAGun05, subQueueKey, false)
                          {CreateItemToPlace = CreateBuildingSceneItem};

            //Attach EventHandler for ItemCreated.            
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);

#endif
        }

        // 10/2/2008 - Specifically creates the IFDTiles 'Tanks' Set-1 Group.
        // 11/5/2008 - Updated to include the new parameters 'SubQueueKey', used to connect to SubGroupControl Tab, and
        //             'BuildingScene', used to connect the Events.
        // 11/10/2008 - Updated to store BuildingScene 'Position' into IFDTile, and connect 'EventHandler' to Player class.
        /// <summary>
        /// Specifically creates the <see cref="IFDTile"/> 'Tanks' Group for side-1.
        /// </summary>
        /// <param name="subQueueKey"><see cref="SubQueueKey"/> structure</param>
        /// <param name="buildingItem"><see cref="BuildingScene"/> instance</param>
        private void CreateIFDTileTanksGroup_Set1(SubQueueKey subQueueKey, BuildingScene buildingItem)
        {
            var tileLocation = new Rectangle(5, 5, 1, 1);

            // Create Tile 1 (light tank)
            var ifdTile = new IFDTilePlacement(Game, "SciFi_Tank04_Pic",
                                                TilePlacement.Pos1, ItemType.sciFiTank04, subQueueKey, true);

            // 3/25/2009 - Attach a Message Tile.
            var ifdMessageTile = new IFDTileMessage(Game, ifdTile, ItemType.sciFiTank04, tileLocation, false);
            ifdTile.AddMessageTile(ifdMessageTile);

            // 11/10/2008 - Attach EventHandler for ItemCreated.
            ifdTile.ProductionBuilding = buildingItem;
            ifdTile.ItemCreateRequest += _thisPlayer.IFDPlacement_ItemCreated;
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);

            // Create Tile 2  (med tank - Strong vs Vehicles)         
            ifdTile = new IFDTilePlacement(Game, "SciFi_Tank01_Pic",
                                            TilePlacement.Pos2, ItemType.sciFiTank01, subQueueKey, true);

            // 3/25/2009 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, ifdTile, ItemType.sciFiTank01, tileLocation, false);
            ifdTile.AddMessageTile(ifdMessageTile);


            // 11/10/2008 - Attach EventHandler for ItemCreated.
            ifdTile.ProductionBuilding = buildingItem;
            ifdTile.ItemCreateRequest += _thisPlayer.IFDPlacement_ItemCreated;
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);

            // Create Tile 3 (med tank 2 - Strong vs Buildings)
            ifdTile = new IFDTilePlacement(Game, "SciFi_Tank06_Pic",
                                            TilePlacement.Pos3, ItemType.sciFiTank06, subQueueKey, true);

            // 3/25/2009 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, ifdTile, ItemType.sciFiTank06, tileLocation, false);
            ifdTile.AddMessageTile(ifdMessageTile);


            // 11/10/2008 - Attach EventHandler for ItemCreated.
            ifdTile.ProductionBuilding = buildingItem;
            ifdTile.ItemCreateRequest += _thisPlayer.IFDPlacement_ItemCreated;
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);

            // Create Tile 4 (strong tank)
            ifdTile = new IFDTilePlacement(Game, "SciFi_Tank09_Pic",
                                            TilePlacement.Pos4, ItemType.sciFiTank09, subQueueKey, true);

            // 3/25/2009 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, ifdTile, ItemType.sciFiTank09, tileLocation, false);
            ifdTile.AddMessageTile(ifdMessageTile);

            // 3/26/2009 - Disable Tile, and attack to be triggered by 'SpecialTechBuilding' global event.
            ifdTile.TileState =
                Player.IsSpecialBuildingPlaced(_thisPlayer,
                                               "Research Center-B2")
                    ? TileState.None
                    : TileState.Disabled; // 5/18/2009
            ifdTile.SpecialBuildingName1 = "Research Center-B2";
            BuildingScene.SpecialBuildingCreated += ifdTile.SpecialBuildingCreated_EventHandler;
            BuildingScene.SpecialBuildingDestroyed += ifdTile.SpecialBuildingDestroyed_EventHandler; // 5/18/2009

            // 11/10/2008 - Attach EventHandler for ItemCreated            
            ifdTile.ProductionBuilding = buildingItem;
            ifdTile.ItemCreateRequest += _thisPlayer.IFDPlacement_ItemCreated;
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);


            // Create Tile 5 (artillery)
            ifdTile = new IFDTilePlacement(Game, "SciFi_Tank10_Pic",
                                            TilePlacement.Pos5, ItemType.sciFiTank10,
                                            subQueueKey, true);

            // 3/25/2009 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, ifdTile, ItemType.sciFiTank10, tileLocation, false);
            ifdTile.AddMessageTile(ifdMessageTile);

            // 3/26/2009 - Disable Tile, and attack to be triggered by 'SpecialTechBuilding' global event.
            ifdTile.TileState =
                Player.IsSpecialBuildingPlaced(_thisPlayer,
                                               "Research Center-B2")
                    ? TileState.None
                    : TileState.Disabled; // 5/18/2009
            ifdTile.SpecialBuildingName1 = "Research Center-B2";
            BuildingScene.SpecialBuildingCreated += ifdTile.SpecialBuildingCreated_EventHandler;
            BuildingScene.SpecialBuildingDestroyed += ifdTile.SpecialBuildingDestroyed_EventHandler; // 5/18/2009


            // 11/10/2008 - Attach EventHandler for ItemCreated.           
            ifdTile.ProductionBuilding = buildingItem;
            ifdTile.ItemCreateRequest += _thisPlayer.IFDPlacement_ItemCreated;
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);

            // Create Tile 6 (Anti-Air)
            ifdTile = new IFDTilePlacement(Game, "SciFi_Jeep01_Pic",
                                            TilePlacement.Pos6, ItemType.sciFiJeep01, subQueueKey, true);

            // 3/25/2009 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, ifdTile, ItemType.sciFiJeep01, tileLocation, false);
            ifdTile.AddMessageTile(ifdMessageTile);


            // 11/10/2008 - Attach EventHandler for ItemCreated.
            ifdTile.ProductionBuilding = buildingItem;
            ifdTile.ItemCreateRequest += _thisPlayer.IFDPlacement_ItemCreated;
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);


            // 4/30/2009 - XBOX Only - add the FlagMarker Tile.
#if XBOX360


#endif
            // Create Tile 8 (FlagMarker)
            ifdTile = new IFDTilePlacement(Game, "FlagMarker",
                                            TilePlacement.Pos8, ItemType.flagMarker,
                                            subQueueKey, false);

            // 3/25/2009 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, ifdTile, ItemType.flagMarker, tileLocation, false);
            ifdTile.AddMessageTile(ifdMessageTile);
            ifdTile.IsPlacementFlag = true; // 5/19/2009

            //Attach EventHandler for ItemCreated.       
            ifdTile.ProductionBuilding = buildingItem;
            ifdTile.CreateItemToPlace = CreateBuildingSceneItem;
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);
        }

        // 3/24/2009 - Specifically creates the IFDTiles 'Tanks' Set-2 Group.
        /// <summary>
        /// Specifically creates the <see cref="IFDTile"/> 'Tanks' Group for side-2.
        /// </summary>
        /// <param name="subQueueKey"><see cref="SubQueueKey"/> structure</param>
        /// <param name="buildingItem"><see cref="BuildingScene"/> instance</param>
        private void CreateIFDTileTanksGroup_Set2(SubQueueKey subQueueKey, BuildingScene buildingItem)
        {
            var tileLocation = new Rectangle(5, 5, 1, 1);

            // Create Tile 1 (light tank) 
            var ifdTile = new IFDTilePlacement(Game, "SciFi_Tank02_Pic",
                                                TilePlacement.Pos1, ItemType.sciFiTank02, subQueueKey, true);

            // 3/25/2009 - Attach a Message Tile.
            var ifdMessageTile = new IFDTileMessage(Game, ifdTile, ItemType.sciFiTank02, tileLocation, false);
            ifdTile.AddMessageTile(ifdMessageTile);


            // 11/10/2008 - Attach EventHandler for ItemCreated.
            ifdTile.ProductionBuilding = buildingItem;
            ifdTile.ItemCreateRequest += _thisPlayer.IFDPlacement_ItemCreated;
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);

            // Create Tile 2 (med tank - Strong vs Vehicles) 
            ifdTile = new IFDTilePlacement(Game, "SciFi_Tank03_Pic",
                                            TilePlacement.Pos2, ItemType.sciFiTank03, subQueueKey, true);

            // 3/25/2009 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, ifdTile, ItemType.sciFiTank03, tileLocation, false);
            ifdTile.AddMessageTile(ifdMessageTile);


            // 11/10/2008 - Attach EventHandler for ItemCreated.
            ifdTile.ProductionBuilding = buildingItem;
            ifdTile.ItemCreateRequest += _thisPlayer.IFDPlacement_ItemCreated;
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);

            // Create Tile 3 (med tank 2 - Strong vs Buildings)
            ifdTile = new IFDTilePlacement(Game, "SciFi_Tank08_Pic",
                                            TilePlacement.Pos3, ItemType.sciFiTank08, subQueueKey, true);

            // 3/25/2009 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, ifdTile, ItemType.sciFiTank08, tileLocation, false);
            ifdTile.AddMessageTile(ifdMessageTile);


            // 11/10/2008 - Attach EventHandler for ItemCreated.
            ifdTile.ProductionBuilding = buildingItem;
            ifdTile.ItemCreateRequest += _thisPlayer.IFDPlacement_ItemCreated;
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);


            // Create Tile 4 (strong tank)
            ifdTile = new IFDTilePlacement(Game, "SciFi_Tank07_Pic",
                                            TilePlacement.Pos4, ItemType.sciFiTank07, subQueueKey, true);

            // 3/25/2009 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, ifdTile, ItemType.sciFiTank07, tileLocation, false);
            ifdTile.AddMessageTile(ifdMessageTile);

            // 3/26/2009 - Disable Tile, and attack to be triggered by 'SpecialTechBuilding' global event.
            ifdTile.TileState =
                Player.IsSpecialBuildingPlaced(_thisPlayer,
                                               "Research Center-B4")
                    ? TileState.None
                    : TileState.Disabled; // 5/18/2009
            ifdTile.SpecialBuildingName1 = "Research Center-B4";
            BuildingScene.SpecialBuildingCreated += ifdTile.SpecialBuildingCreated_EventHandler;
            BuildingScene.SpecialBuildingDestroyed += ifdTile.SpecialBuildingDestroyed_EventHandler; // 5/18/2009


            // 11/10/2008 - Attach EventHandler for ItemCreated.
            ifdTile.ProductionBuilding = buildingItem;
            ifdTile.ItemCreateRequest += _thisPlayer.IFDPlacement_ItemCreated;
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);

            // Create Tile 5 (artillery)
            ifdTile = new IFDTilePlacement(Game, "SciFi_Artilery01_Pic",
                                            TilePlacement.Pos5, ItemType.sciFiArtilery01, subQueueKey, true);

            // 3/25/2009 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, ifdTile, ItemType.sciFiArtilery01, tileLocation, false);
            ifdTile.AddMessageTile(ifdMessageTile);

            // 3/26/2009 - Disable Tile, and attack to be triggered by 'SpecialTechBuilding' global event.
            ifdTile.TileState =
                Player.IsSpecialBuildingPlaced(_thisPlayer,
                                               "Research Center-B4")
                    ? TileState.None
                    : TileState.Disabled; // 5/18/2009
            ifdTile.SpecialBuildingName1 = "Research Center-B4";
            BuildingScene.SpecialBuildingCreated += ifdTile.SpecialBuildingCreated_EventHandler;
            BuildingScene.SpecialBuildingDestroyed += ifdTile.SpecialBuildingDestroyed_EventHandler; // 5/18/2009


            // 11/10/2008 - Attach EventHandler for ItemCreated.
            ifdTile.ProductionBuilding = buildingItem;
            ifdTile.ItemCreateRequest += _thisPlayer.IFDPlacement_ItemCreated;
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);


            // Create Tile 6 (Anti-Air)
            ifdTile = new IFDTilePlacement(Game, "SciFi_Jeep03_Pic",
                                            TilePlacement.Pos6, ItemType.sciFiJeep03, subQueueKey, true);

            // 3/25/2009 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, ifdTile, ItemType.sciFiJeep03, tileLocation, false);
            ifdTile.AddMessageTile(ifdMessageTile);


            // 11/10/2008 - Attach EventHandler for ItemCreated.
            ifdTile.ProductionBuilding = buildingItem;
            ifdTile.ItemCreateRequest += _thisPlayer.IFDPlacement_ItemCreated;
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);

            // 4/30/2009 - XBOX Only - add the FlagMarker Tile.
#if XBOX360


#endif
            // Create Tile 8 (FlagMarker)
            ifdTile = new IFDTilePlacement(Game, "FlagMarker",
                                            TilePlacement.Pos8, ItemType.flagMarker,
                                            subQueueKey, false);

            // 3/25/2009 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, ifdTile, ItemType.flagMarker, tileLocation, false);
            ifdTile.AddMessageTile(ifdMessageTile);
            ifdTile.IsPlacementFlag = true; // 5/19/2009

            //Attach EventHandler for ItemCreated.       
            ifdTile.ProductionBuilding = buildingItem;
            ifdTile.CreateItemToPlace = CreateBuildingSceneItem;
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);
        }

        // 9/24/2008 - 
        /// <summary>
        /// Specifically creates the <see cref="IFDTile"/> 'Display-Always' <see cref="IFDTileGroupControl"/> group. This
        /// group are the icons which the user selects to show groups like the 'Buildings'.
        /// </summary>
        private void CreateIFDTileControlGroup()
        {
            var tileLocation = new Rectangle { X = 0, Y = 20, Width = 35, Height = 35 };

            // Create Tile 1
            _ifdTileGcBuildings = new IFDTileGroupControl(Game, "IFDTileGC_Buildings",
                                                          IFDGroupControlType.Buildings,
                                                          tileLocation);

            // Add to Perm Array / GroupControl Dictionary      
            AddGroupControlTile(_ifdTileGcBuildings);

            // Create Tile 2   
            tileLocation.X = 35;
            tileLocation.Y = 20;
            _ifdTileGcPeople = new IFDTileGroupControl(Game, "IFDTileGC_People", IFDGroupControlType.People,
                                                       tileLocation);

            // Add to Perm Array / GroupControl Dictionary        
            AddGroupControlTile(_ifdTileGcPeople);

            // Create Tile 3
            tileLocation.X = 70;
            tileLocation.Y = 20;
            _ifdTileGcVehicles = new IFDTileGroupControl(Game, "IFDTileGC_Vehicles", IFDGroupControlType.Vehicles,
                                                         tileLocation);

            // Add to Perm Array / GroupControl Dictionary        
            AddGroupControlTile(_ifdTileGcVehicles);

            // Create Tile 4
            tileLocation.X = 105;
            tileLocation.Y = 20;
            _ifdTileGcShields = new IFDTileGroupControl(Game, "IFDTileGC_Shields", IFDGroupControlType.Shields,
                                                        tileLocation);

            // Add to Perm Array / GroupControl Dictionary    
            AddGroupControlTile(_ifdTileGcShields);

            // Create Tile 5
            tileLocation.X = 140;
            tileLocation.Y = 20;
            _ifdTileGcAirplanes = new IFDTileGroupControl(Game, "IFDTileGC_Airplanes", IFDGroupControlType.Airplanes,
                                                          tileLocation);

            // Add to Perm Array / GroupControl Dictionary      
            AddGroupControlTile(_ifdTileGcAirplanes);
        }

        // 9/24/2008 - 
        /// <summary>
        /// Specifically creates the <see cref="IFDTile"/> 'Buildings', Group for side-1.
        /// </summary>
        /// <param name="subQueueKey"><see cref="SubQueueKey"/> structure</param>
        private void CreateIFDTileBuildingsGroup_Set1(SubQueueKey subQueueKey)
        {
            var tileLocation = new Rectangle(5, 5, 1, 1);

            //
            // 1st - Create all Instanced of Tiles needed.
            //

            // Create Tile 1 (War Factory)
            var ifdTile = new IFDTilePlacement(Game, "SciFi_BldB11_Pic",
                                                TilePlacement.Pos1, ItemType.sciFiBldb11, subQueueKey, false);

            // 9/25/2008 - Attach a Message Tile.
            var ifdMessageTile = new IFDTileMessage(Game, ifdTile, ItemType.sciFiBldb11, tileLocation, false);
            ifdTile.AddMessageTile(ifdMessageTile);


            // 9/23/2008 - Attach Delegate function to tile
            ifdTile.SpecialBuildingName1 = "War Factory-B11";
            ifdTile.TileState = TileState.Disabled; // 3/25/2009
            ifdTile.CreateItemToPlace = CreateBuildingSceneItem;

            // Create Tile 2 (Power)
            var ifdTile2 = new IFDTilePlacement(Game, "SciFi_BldB09_Pic",
                                                 TilePlacement.Pos2, ItemType.sciFiBldb09, subQueueKey, false);

            // 9/25/2008 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, ifdTile2, ItemType.sciFiBldb09, tileLocation, false);
            ifdTile2.AddMessageTile(ifdMessageTile);

            // 9/23/2008 - Attach Delegate function to tile
            ifdTile2.TileState = TileState.Disabled; // 3/25/2009
            ifdTile2.CreateItemToPlace = CreateBuildingSceneItem;

            // Create Tile 3 (Supply Depot)
            var ifdTile3 = new IFDTilePlacement(Game, "SciFi_BldB12_Pic",
                                                 TilePlacement.Pos3, ItemType.sciFiBldb12, subQueueKey, false);

            // 9/25/2008 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, ifdTile3, ItemType.sciFiBldb12, tileLocation, false);
            ifdTile3.AddMessageTile(ifdMessageTile);

            // 9/23/2008 - Attach Delegate function to tile
            ifdTile3.TileState = TileState.Disabled; // 3/25/2009
            ifdTile3.CreateItemToPlace = CreateBuildingSceneItem;


            // Create Tile 4 (Airport)
            var ifdTile4 = new IFDTilePlacement(Game, "SciFi_BldB13_Pic",
                                                 TilePlacement.Pos4, ItemType.sciFiBldb13, subQueueKey, false);

            // 9/25/2008 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, ifdTile4, ItemType.sciFiBldb13, tileLocation, false);
            ifdTile4.AddMessageTile(ifdMessageTile);

            // 9/23/2008 - Attach Delegate function to tile
            ifdTile4.SpecialBuildingName2 = "Airport-B13";
            ifdTile4.TileState = TileState.Disabled; // 3/25/2009
            ifdTile4.CreateItemToPlace = CreateBuildingSceneItem;


            // Create Tile 5 (Research Center)
            var ifdTile5 = new IFDTilePlacement(Game, "SciFi_BldB02_Pic",
                                                 TilePlacement.Pos5, ItemType.sciFiBldb02, subQueueKey, false);

            // 9/25/2008 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, ifdTile5, ItemType.sciFiBldb02, tileLocation, false);
            ifdTile5.AddMessageTile(ifdMessageTile);

            // 9/23/2008 - Attach Delegate function to tile            
            ifdTile5.TileState = TileState.Disabled; // 3/25/2009
            ifdTile5.CreateItemToPlace = CreateBuildingSceneItem;
            BuildingScene.SpecialBuildingDestroyed += ifdTile5.SpecialBuildingDestroyed_EventHandler;

            // 4/9/2009: Updated to use the STATIC variable 'HQSide1'.
            // Create Tile 6 (MCF)
            HQSide1 = new IFDTilePlacement(Game, "SciFi_BldB15_Pic",
                                            TilePlacement.Pos6, ItemType.sciFiBldb15, subQueueKey, false);

            // 9/25/2008 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, HQSide1, ItemType.sciFiBldb15, tileLocation, false);
            HQSide1.AddMessageTile(ifdMessageTile);

            // 6/2/2009 - Create the 2 Shield tiles in positions 7/8 for XBOX
#if (XBOX360)

            CreateIFDTileShieldsGroup_Set1(subQueueKey);
#endif


            // 9/23/2008 - Attach Delegate function to tile
            HQSide1.CreateItemToPlace = CreateBuildingSceneItem;


            //
            // 2nd - Attach Tiles which need to be Enabled, to the required Tile Enabler.
            //

            // Attach Tiles 3 & 4 to Tile-2, the enabler.
            ifdTile2.ItemPlaced += ifdTile3.ItemPlaced_EventHandler;
            ifdTile2.ItemPlaced += ifdTile4.ItemPlaced_EventHandler;

            // Attach Tile 5 to Tile-4 & Tile-1, the enablers. (Reqs 2 enablers)
            ifdTile5.RequiresTwoEnablers = true;
            ifdTile5.SpecialBuildingName1 = "War Factory-B11";
            ifdTile5.SpecialBuildingName2 = "Airport-B13";
            ifdTile4.ItemPlaced += ifdTile5.ItemPlaced_EventHandler;
            ifdTile.ItemPlaced += ifdTile5.ItemPlaced_EventHandler;

            // Attach Tiles 1 & 2 to Tile-6, the enabler.
            HQSide1.ItemPlaced += ifdTile.ItemPlaced_EventHandler;
            HQSide1.ItemPlaced += ifdTile2.ItemPlaced_EventHandler;

            //
            // 3rd - Add All Tile Instanced to the SubQueue
            //
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile2);
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile3);
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile4);
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile5);
            AddInterFaceDisplayTile(ref subQueueKey, HQSide1);
        }

        // 12/28/2008 - 
        /// <summary>
        /// Specifically creates the <see cref="IFDTile"/> 'Buildings', Group for side-2.
        /// </summary>
        /// <param name="subQueueKey"><see cref="SubQueueKey"/> structure</param>
        private void CreateIFDTileBuildingsGroup_Set2(SubQueueKey subQueueKey)
        {
            var tileLocation = new Rectangle(5, 5, 1, 1);

            //
            // 1st - Create all Instanced of Tiles needed.
            //

            // Create Tile 1 (War Factory)
            var ifdTile = new IFDTilePlacement(Game, "SciFi_BldB01_Pic",
                                                              TilePlacement.Pos1, ItemType.sciFiBldb01,
                                                              subQueueKey, false);

            // 9/25/2008 - Attach a Message Tile.
            var ifdMessageTile = new IFDTileMessage(Game, ifdTile, ItemType.sciFiBldb01, tileLocation, false);
            ifdTile.AddMessageTile(ifdMessageTile);


            // 9/23/2008 - Attach Delegate function to tile
            ifdTile.SpecialBuildingName1 = "War Factory-B1";
            ifdTile.TileState = TileState.Disabled; // 3/25/2009
            ifdTile.CreateItemToPlace = CreateBuildingSceneItem;

            // Create Tile 2 (Power Structure)
            var ifdTile2 = new IFDTilePlacement(Game, "SciFi_BldB05_Pic",
                                                               TilePlacement.Pos2, ItemType.sciFiBldb05, subQueueKey, false);

            // 9/25/2008 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, ifdTile2, ItemType.sciFiBldb05, tileLocation, false);
            ifdTile2.AddMessageTile(ifdMessageTile);

            // 9/23/2008 - Attach Delegate function to tile
            ifdTile2.TileState = TileState.Disabled; // 3/25/2009
            ifdTile2.CreateItemToPlace = CreateBuildingSceneItem;

            // Create Tile 3 (Supply Depot)
            var ifdTile3 = new IFDTilePlacement(Game, "SciFi_BldB07_Pic",
                                                               TilePlacement.Pos3, ItemType.sciFiBldb07, subQueueKey, false);

            // 9/25/2008 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, ifdTile3, ItemType.sciFiBldb07, tileLocation, false);
            ifdTile3.AddMessageTile(ifdMessageTile);

            // 9/23/2008 - Attach Delegate function to tile
            ifdTile3.TileState = TileState.Disabled; // 3/25/2009
            ifdTile3.CreateItemToPlace = CreateBuildingSceneItem;


            // Create Tile 4 (Airport)
            var ifdTile4 = new IFDTilePlacement(Game, "SciFi_BldB10_Pic",
                                                               TilePlacement.Pos4, ItemType.sciFiBldb10,
                                                               subQueueKey, false);

            // 9/25/2008 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, ifdTile4, ItemType.sciFiBldb10, tileLocation, false);
            ifdTile4.AddMessageTile(ifdMessageTile);

            // 9/23/2008 - Attach Delegate function to tile
            ifdTile4.SpecialBuildingName2 = "Airport-B10";
            ifdTile4.TileState = TileState.Disabled; // 3/25/2009
            ifdTile4.CreateItemToPlace = CreateBuildingSceneItem;


            // Create Tile 5 (Technology Building)
            var ifdTile5 = new IFDTilePlacement(Game, "SciFi_BldB04_Pic",
                                                               TilePlacement.Pos5, ItemType.sciFiBldb04, subQueueKey, false);

            // 9/25/2008 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, ifdTile5, ItemType.sciFiBldb04, tileLocation, false);
            ifdTile5.AddMessageTile(ifdMessageTile);

            // 9/23/2008 - Attach Delegate function to tile            
            ifdTile5.TileState = TileState.Disabled; // 3/25/2009
            ifdTile5.CreateItemToPlace = CreateBuildingSceneItem;
            BuildingScene.SpecialBuildingDestroyed += ifdTile5.SpecialBuildingDestroyed_EventHandler;

            // 4/9/2009: Updated to use the STATIC variable 'HQSide2'.
            // Create Tile 6 (MCF)
            HQSide2 = new IFDTilePlacement(Game, "SciFi_BldB14_Pic",
                                            TilePlacement.Pos6, ItemType.sciFiBldb14, subQueueKey, false);

            // 9/25/2008 - Attach a Message Tile.
            ifdMessageTile = new IFDTileMessage(Game, HQSide2, ItemType.sciFiBldb14, tileLocation, false);
            HQSide2.AddMessageTile(ifdMessageTile);

            // 6/2/2009 - Create the 2 Shield tiles in positions 7/8 for XBOX
#if (XBOX360)

            CreateIFDTileShieldsGroup_Set2(subQueueKey);
#endif

            // 9/23/2008 - Attach Delegate function to tile
            HQSide2.CreateItemToPlace = CreateBuildingSceneItem;

            //
            // 2nd - Attach Tiles which need to be Enabled, to the required Tile Enabler.
            //

            // Attach Tiles 3 & 4 to Tile-2, the enabler.
            ifdTile2.ItemPlaced += ifdTile3.ItemPlaced_EventHandler;
            ifdTile2.ItemPlaced += ifdTile4.ItemPlaced_EventHandler;

            // Attach Tile 5 to Tile-4 & Tile-1, the enablers. (Reqs 2 enablers)
            ifdTile5.RequiresTwoEnablers = true;
            ifdTile5.SpecialBuildingName1 = "War Factory-B1";
            ifdTile5.SpecialBuildingName2 = "Airport-B10";
            ifdTile4.ItemPlaced += ifdTile5.ItemPlaced_EventHandler;
            ifdTile.ItemPlaced += ifdTile5.ItemPlaced_EventHandler;

            // Attach Tiles 1 & 2 to Tile-6, the enabler.
            HQSide2.ItemPlaced += ifdTile.ItemPlaced_EventHandler;
            HQSide2.ItemPlaced += ifdTile2.ItemPlaced_EventHandler;

            //
            // 3rd - Add All Tile Instanced to the SubQueue
            //
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile);
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile2);
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile3);
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile4);
            AddInterFaceDisplayTile(ref subQueueKey, ifdTile5);
            AddInterFaceDisplayTile(ref subQueueKey, HQSide2);
        }

        // 3/25/2009 - Updated to include the last parameter of 'IFDTile' caller.
        // 9/23/2008 - IFDTile Delegate function, used to create the SceneItem for Placement
        // 11/4/2008 - Updated Delegate function by adding the 'IFDGroupControlType' Enum Parameter, which is used to know
        //             what GroupControlTile needs another Sub-Queue Tab.
        // 12/26/2008 - Updated to include the new '_itemGroupToAttack' parameter for defense structs.
        /// <summary>
        /// Helper delegate method, used to create the <see cref="SceneItem"/> for placement.
        /// </summary>
        /// <param name="buildingType"><see cref="ItemGroupType"/> Enum</param>
        /// <param name="productionType">Production <see cref="ItemGroupType"/> Enum</param>
        /// <param name="itemType"><see cref="ItemType"/> Enum of new <see cref="SceneItem"/></param>
        /// <param name="itemGroupToAttack"><see cref="ItemGroupType"/> Enum this <see cref="SceneItem"/> can attack</param>
        /// <param name="placeItemAt"><see cref="Vector3"/> location to place item</param>
        /// <param name="theIFDTile"><see cref="IFDTile"/> instance reference</param>
        private void CreateBuildingSceneItem(ItemGroupType buildingType, ItemGroupType? productionType,
                                             ItemType itemType,
                                             ItemGroupType? itemGroupToAttack, ref Vector3 placeItemAt,
                                             IFDTile theIFDTile)
        {
            _itemTypeToUse = itemType;
            _itemGroupToAttack = itemGroupToAttack;
            _buildingType = buildingType;
            _productionType = productionType;
            _ifdTileCaller = theIFDTile; // 3/25/2009

            // 12/24/2008 - Which class to create
            var player = _thisPlayer; // 10/9/2009 - Cache
            switch (buildingType)
            {
                case ItemGroupType.Buildings:
                    // Add to the Terrain scene
                    player.PoolManager.GetNode_IFD(out _buildingPoolNode);
                    _itemToPlace = _buildingPoolNode.SceneItemInstance;

                    // 10/9/2009 - Store Cast value
                    var buildingScene = (_itemToPlace as BuildingScene);

                    // 4/19/2010 - Check if null.
                    if (buildingScene != null)
                    {
                        buildingScene.ShapeItem.SetInstancedItemTypeToUse(itemType);
                        buildingScene.LoadPlayableAttributesForItem(
                            new ItemCreatedArgs {ItemType = itemType, PlaceItemAt = placeItemAt}, false);

                        _itemToPlace.Position = placeItemAt;
                        buildingScene.ShapeItem.PlayerNumber = player.PlayerNumber;
                        buildingScene.PlayerNumber = player.PlayerNumber;
                    }
                    else
                    {
                        Debug.WriteLine("Unable to create building, since Null cast error.");
                    }

                    break;
                case ItemGroupType.Shields:
                    // Add to the Terrain scene 

                    player.PoolManager.GetNode_IFD(out _defensePoolNode);
                    _itemToPlace = _defensePoolNode.SceneItemInstance;

                    // 10/9/2009 - Store Cast value
                    var defenseScene = (_itemToPlace as DefenseScene);

                    // 4/19/2010 - Check if null.
                    if (defenseScene != null)
                    {
                        defenseScene.ShapeItem.SetInstancedItemTypeToUse(itemType);
                        defenseScene.LoadPlayableAttributesForItem(
                            new ItemCreatedArgs
                                {
                                    ItemType = itemType,
                                    PlaceItemAt = placeItemAt,
                                    ItemGroupToAttack =
                                        (itemGroupToAttack == null) // 4/19/2010
                                            ? ItemGroupType.Vehicles
                                            : itemGroupToAttack.Value
                                }, false);

                        _itemToPlace.Position = placeItemAt;
                        defenseScene.ShapeItem.PlayerNumber =
                            player.PlayerNumber;
                        defenseScene.PlayerNumber =
                            player.PlayerNumber;
                    }
                    else
                    {
                        Debug.WriteLine("Unable to create defense, since Null cast error.");
                    }

                    break;
                case ItemGroupType.People:
                case ItemGroupType.Airplanes:
                case ItemGroupType.Vehicles:
                    break;
                default:
                    break;
            }

            // 6/26/2012 - Update the 'ItemToPlaced' flag to false.
            _itemToPlace.ItemIsPlaced = false;

            // 6/15/2010: Updated to use the Player classes Add method.
            // Add to the Selectable Items 
            // NOTE: 12/17/2008 - This should not be added to SelectableItemsDict Dictionary!
            //player.SelectableItems.Add(_itemToPlace as SceneItemWithPick);
            Player.AddSelectableItem(player, _itemToPlace as SceneItemWithPick, false);
        }

        private static int[] _groupKeys = new int[1];
        private static int[] _queueKeys = new int[1];


        // 5/27/2008; 8/21/2008
        // 12/8/2008: Updated to check the 'Clicked Queue, to process Tiles clicked in order of clicks, within
        //            each of their respective subQueues!
        /// <summary>
        /// Checks for mouse/gamepad input for the <see cref="IFDTileManager"/>, and activates the approriate functions
        /// where necessary; for example, a click on war-factor would allow user to place a war-factor
        /// on the <see cref="Terrain"/>.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private static void UpdateInterfaceDisplay(GameTime gameTime)
        {
            // 12/8/2008
            // Iterate through all SubQueue Group tiles and then DeQueue the top
            // Tile from iternal Queue, and then set TileState to CountDown!
            UpdateForQueueCountdown(); // 6/17/2010 - Refactored into separate method.

            // 9/29/2008
            // Iterate through all IFD Tiles and Calls Update; regardless, if they
            // are the Current Display group or not, since the progress still needs
            // to be updated.
            UpdateIFDTiles(gameTime); // 6/17/2010 - Refactored into separate method.

            // 1/5/2009 - Update the _cashMessageTile with player's current cash amount.
            UpdateCashValueTile(); // 6/17/2010 - Refactored into separate method.

            // 1/30/2009 - Update the _energyMessageTile with player's current energy amount.
            UpdateEnergyValueTile();
        }

        // 6/17/2010 - To reduce GC on heap from strings, add int->string conversions in dictionary for resuse.
        private static readonly Dictionary<int, string> StringHelper = new Dictionary<int, string>(100);
        

        // 6/17/2010
        /// <summary>
        ///  Update the <see cref="_energyMessageTile"/> with player's current energy amount.
        /// </summary>
        private static void UpdateEnergyValueTile()
        {
            try
            {
                // 5/25/2012 -Skips section when the ScriptingAction is using the scoreboard.
                if (_displayScoreboardForScripting)
                {
                    return;
                }

                if (!_thisPlayer.EnergyValueChanged)
                {
                    return;
                }

                var energyCalc = _thisPlayer.Energy -
                                 _thisPlayer.EnergyUsed;

                DoUpdateEnergyValueTile(energyCalc);

                _thisPlayer.EnergyValueChanged = false;
            }
            // Captures the NullRefExp, and checks if the _thisPlayer is null; if so, retrieves
            // updates instance directly from TWEngine class.  This avoids having to check if null every game cycle.
            catch (NullReferenceException)
            {
#if DEBUG
                Debug.WriteLine(
                    "UpdateEnergyValueTile method, in IFDTile_Manager class, threw the NullReferenceExp error.",
                    "NullReferenceException");
#endif

                if (_thisPlayer == null)
                {
                    // 6/15/2010 - Updated to use new GetPlayer method.
                    TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out _thisPlayer);
#if DEBUG
                    Debug.WriteLine(
                        "The '_thisPlayer' variable was null; however, fixed by retrieving reference from TWEngine class.");
#endif
                }
            }
            
        }

        // 5/25/2012 - Updated to allow ScriptingAction calls.
        /// <summary>
        /// Updates the <see cref="_energyMessageTile"/> with the given energy amount.
        /// </summary>
        /// <param name="energy"></param>
        public static void DoUpdateEnergyValueTile(int energy)
        {
            // 6/17/2010 - Builds strings into dictionary to reduce GC on heap.
            /*if (!StringHelper.ContainsKey(energy))
                StringHelper.Add(energy, energy.ToString());*/

            if (_energyMessageTile == null)
            {
                return;
            }

            // 5/13/2009 - Populate StringBuilder
            _energyMessageTile.SbMessageToDisplay.Length = 0;
            _energyMessageTile.SbMessageToDisplay.Append(energy); // StringHelper[energy]
        }

        // 6/17/2010
        /// <summary>
        /// Updates the <see cref="_cashMessageTile"/> with player's current cash amount.
        /// </summary>
        private static void UpdateCashValueTile()
        {
            try
            {
                // 5/25/2012 -Skips section when the ScriptingAction is using the scoreboard.
                if (_displayScoreboardForScripting)
                {
                    return;
                }

                if (!_thisPlayer.CashValueChanged)
                {
                    return;
                }

                DoUpdateCashValueTile(_thisPlayer.Cash);

                _thisPlayer.CashValueChanged = false;
            }
            // Captures the NullRefExp, and checks if the _thisPlayer is null; if so, retrieves
            // updates instance directly from TWEngine class.  This avoids having to check if null every game cycle.
            catch (NullReferenceException)
            {
#if DEBUG
                Debug.WriteLine(
                    "UpdateCashValueTile method, in IFDTile_Manager class, threw the NullReferenceExp error.",
                    "NullReferenceException");
#endif

                if (_thisPlayer == null)
                {
                    // 6/15/2010 - Updated to use new GetPlayer method.
                    TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out _thisPlayer);
#if DEBUG
                    Debug.WriteLine("The '_thisPlayer' variable was null; however, fixed by retrieving reference from TWEngine class.");
#endif
                }
            }
            
        }

        // 5/25/2012 - Updated to allow ScriptingAction calls.
        /// <summary>
        ///  Updates the <see cref="_cashMessageTile"/> with given cash amount.
        /// </summary>
        /// <param name="cash">New cash value.</param>
        public static void DoUpdateCashValueTile(int cash)
        {
            // Populate StringBuilder
            if (_cashMessageTile == null)
            {
                return;
            }

            _cashMessageTile.SbMessageToDisplay.Length = 0;
            _cashMessageTile.SbMessageToDisplay.Append(cash);
        }

        // 6/17/2010
        /// <summary>
        /// Iterate through all <see cref="IFDTiles"/> and call Update, regardless if they
        /// are the current display group or not, since the progress still needs
        /// to be updated.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private static void UpdateIFDTiles(GameTime gameTime)
        {
            // Get Dictionary Keys
            var ifdTileGroups = _ifdTileGroups; // 6/1/2010 - Cache
            var ifdTileGroupsCount = ifdTileGroups.Keys.Count; // 7/27/2009

            if (_groupKeys.Length != ifdTileGroupsCount)
                Array.Resize(ref _groupKeys, ifdTileGroupsCount);
            ifdTileGroups.Keys.CopyTo(_groupKeys, 0);

            var groupKeys = _groupKeys; // 6/1/2010 - Cache

            // Iterate Dictionary using Keys.                
            for (var i = 0; i < ifdTileGroupsCount; i++)
            {
                // 4/19/2010 - Cache
                var ifdTileGroup = ifdTileGroups[groupKeys[i]];
                if (ifdTileGroup == null) continue;

                // Loop through List of given Key
                var count = ifdTileGroup.Count;
                for (var j = 0; j < count; j++)
                {
                    // 4/19/2010 - Cache
                    var ifdTile = ifdTileGroup[j];
                    if (ifdTile == null) continue;

                    ifdTile.Update(gameTime);
                }
            }
        }

        // 6/17/2010
        /// <summary>
        /// Iterate through all SubQueue group tiles, dequeue the top
        /// <see cref="IFDTile"/> from iternal queue, and then set <see cref="TileState"/> 
        /// to CountDown.
        /// </summary>
        private static void UpdateForQueueCountdown()
        {
            // Get Dictionary Keys
            var ifdTilesClicked = _ifdTilesClicked; // 6/1/2010 - Cache
            var ifdTilesClickCount = ifdTilesClicked.Keys.Count; // 7/27/2009

            if (_queueKeys.Length < ifdTilesClickCount)
                Array.Resize(ref _queueKeys, ifdTilesClickCount);
            ifdTilesClicked.Keys.CopyTo(_queueKeys, 0);

            var queueKeys = _queueKeys; // 6/1/2010 - Cache

            // Iterate Dictionary using Keys.
            for (var i = 0; i < ifdTilesClickCount; i++)
            {
                // Get top Tile, in Queue, using Peek since we do not want the tile DeQueue until
                // it is done with it's CountDown phase!
                if (ifdTilesClicked[queueKeys[i]].Count == 0) continue;

                var currentTileDeQueued = ifdTilesClicked[queueKeys[i]].Peek();

                // 4/19/2010 - Convert from sloppy If to Switch construct.
                switch (currentTileDeQueued.TileState)
                {
                        // If 'Queued' state, then let's start CountDown.
                    case TileState.Queued:
                        currentTileDeQueued.RoundMeter.RunCountdown = true;
                        currentTileDeQueued.RoundMeter.StartCountdown();
                        currentTileDeQueued.TileState = TileState.Countdown;
                        break;
                    case TileState.Hovered:
                    case TileState.None:
                        if (currentTileDeQueued.TileClicksCount > 0)
                        {
                            // yes, then set to 'Queued' to create SceneItemOwner again.
                            currentTileDeQueued.TileState = TileState.Queued;
                        }
                        else
                        {
                            // no, then let's Dequeue this Tile
                            ifdTilesClicked[queueKeys[i]].Dequeue();
                        } // End if tileclickscount > 0
                        break;
                }
            } // End For QueueKeys
        }

        /// <summary>
        /// Helper Fn: Called when user is attempting to place a <see cref="SceneItem"/> owner on the <see cref="Terrain"/>.
        /// </summary>     
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>   
        /// <param name="interfaceDisplay"><see cref="IFDTileManager"/> manager reference</param>
        private static void UpdateCheckForItemPlacement(IFDTileManager interfaceDisplay, GameTime gameTime)
        {
            // Update Position of _itemToPlace
            if (_itemToPlace == null) return;


            // 8/1/2009: Updated to use the Vector3.Multiply, which is faster on the XBOX!
            // 1/5/2009: Updated to use 'AStarPathScale', to Snap Position to nearest pathnode.
            // Get Position of Mouse Cursor in World Space 
            TerrainPickingRoutines.GetCursorPosByPickedRay(PickRayScale.DivideByAStarPathScale, out PlaceItemAt);

            var height = PlaceItemAt.Y;
            Vector3.Multiply(ref PlaceItemAt, TemporalWars3DEngine._pathNodeStride, out PlaceItemAt); // 8/1/2009
            PlaceItemAt.Y = height;
            _itemToPlace.Position = PlaceItemAt;

            // 1/6/2009: Create the Boxes overlay on terrain, which shows passable/unpassable areas to player.
            CreatePlacementOverlay(ref PlaceItemAt, _itemToPlace);
           
            // If user Clicks on Terrain, then place SceneItemOwner at this Position
            if (!HandleInput.InputState.IFDPlaceItem) return;

            // 5/1/2009 - Check if within the BuildAble area of HQ, except the FlagMarker
            var isInPlacableArea = IsWithinBuildableArea(_itemToPlace, (int)PlaceItemAt.X, (int)PlaceItemAt.Z) &&
                                    _itemToPlace.RunPlacementCheck(ref PlaceItemAt);

            // 1/5/2009 - Check if SceneItemOwner can be placed at this Position?
            if (!isInPlacableArea) return;

            // 4/30/2009 - If FlagMarker, then just call ProdBuilding associated to update marker Position!
            if (_itemToPlace.ShapeItem.InstancedItemData.ItemType == ItemType.flagMarker)
            {
                var buildingScene = ((IFDTilePlacement)_ifdTileCaller).ProductionBuilding;
                buildingScene.ShapeItem.SetMarkerPosition(ref PlaceItemAt, buildingScene.NetworkItemNumber);

                // Turn Off Attempting SceneItemOwner Placement
                AttemptingItemPlacement = false;

                // 6/26/2012 - Set 'ItemIsPlaced' for flag markers (Scripting Conditions)
                _itemToPlace.ItemIsPlaced = true;

                // 8/1/2009 - make sure main building for flag is now deselected.
                Player.DeSelectSceneItem(buildingScene);

                // Let's now remove the temporary itemPlacement from InterfaceDisplay
                RemoveTempItemToPlace();
                return;
            }

            // 11/10/10028
            // Let's call 'IFDPlacement_ItemCreated' directly in player class, to correctly add building for both
            // single player and network games.

            // 1st - add SceneItemOwner via player class
            var itemArgs = new ItemCreatedArgs(_buildingType, _productionType, _itemTypeToUse, _itemGroupToAttack,
                                                           PlaceItemAt, 0, null, gameTime.TotalGameTime.TotalSeconds);
            // NOTE: NetworkID 0, because not assigned yet!!! 

            // 12/14/2008: This line MUST preceed the EventHandler call to 'IFDPlacement_ItemCreated'!
            // Turn Off Attempting SceneItemOwner Placement
            AttemptingItemPlacement = false;

            // 2nd - Call Player Classes ItemCreated directly to create final World sceneitem
            _thisPlayer.IFDPlacement_ItemCreated(interfaceDisplay, itemArgs);


            // 10/19/2008 - Fire Event of SceneItemOwner Placed.
            if (SceneItemPlaced != null)
                SceneItemPlaced(_itemToPlace, EventArgs.Empty);

            // 3/25/2009 - Fire Event of SceneItemOwner Placed, which specifically is used
            //             to enable other Tiles, when connected to an Enabler!
            ((IFDTilePlacement)_ifdTileCaller).OnItemPlaced();
        }

        // 5/1/2009
        /// <summary>
        /// Checks if the <see cref="SceneItem"/> <paramref name="itemToPlace"/> position 
        /// is within the <see cref="Player"/> buildable area.
        /// </summary>
        /// <returns>True/False if within builadable area</returns>
        private static bool IsWithinBuildableArea(SceneItem itemToPlace, int x, int y)
        {
            // 6/8/2009 - If FlagMarker, then return TRUE.
            if (itemToPlace.ShapeItem.InstancedItemData.ItemType == ItemType.flagMarker)
                return true;

            var location = new Point { X = x, Y = y };

            bool isInBuildableArea;
            var thisPlayer = _thisPlayer; // 8/14/2009
            thisPlayer.BuildableAreaRectangle.Contains(ref location,
                                                       out isInBuildableArea);

            return isInBuildableArea;
        }

        // 1/6/2009
        /// <summary>
        /// Creates the Placement helper Overlay to draw onto terrain.  This shows the
        /// colored grid of white and red boxes, clearly demonstrating the areas which are
        /// blocked and valid for <see cref="SceneItem"/> placement.
        /// </summary>
        /// <param name="placeItemAt"><see cref="Vector3"/> position to check for placement</param>
        /// <param name="itemToPlace"><see cref="SceneItem"/> instance</param>
        private static void CreatePlacementOverlay(ref Vector3 placeItemAt, SceneItem itemToPlace)
        {
            // Convert World Cords to Graph Stride Cords
            var aStarManagerNodeStride = (TemporalWars3DEngine.AStarGraph != null) ? TemporalWars3DEngine.AStarGraph.NodeStride : 0;
            var x = (int)(placeItemAt.X / aStarManagerNodeStride);
            var y = (int)(placeItemAt.Z / aStarManagerNodeStride);
            // Calculate half of AStar NodeStride, minus a slight margin to create the boxes.
            var halfNodeStride = (aStarManagerNodeStride / 2) - 5;

            _selectionBoxColor.A = 50; // Make partial translucent color!         

            var index = 0;
            for (var xRow = -2; xRow < 3; xRow++)
                for (var yRow = -2; yRow < 3; yRow++)
                {
                    // Check if current Node is blocked
                    var xValue = (x + xRow) * aStarManagerNodeStride;
                    var yValue = (y + yRow) * aStarManagerNodeStride;

                    // 8/17/2009 - Skip values below zero, or larger than mapSize.
                    if (xValue < 0 || yValue < 0)
                        continue;

                    if (TemporalWars3DEngine.AStarGraph != null) // 1/13/2010
                        if (TemporalWars3DEngine.AStarGraph.IsNodeBlocked(NodeScale.TerrainScale, xValue, yValue))
                        {
                            // Make Red
                            _selectionBoxColor.R = 255;
                            _selectionBoxColor.B = 0;
                            _selectionBoxColor.G = 0;
                        }
                        else if (itemToPlace.IsInPlacementZone(ref placeItemAt, xValue, yValue, 0)) // 6/8/2009
                        {
                            // Make Green
                            _selectionBoxColor.R = 0;
                            _selectionBoxColor.B = 0;
                            _selectionBoxColor.G = 255;
                        }
                        else
                        {
                            // Make White
                            _selectionBoxColor.R = 255;
                            _selectionBoxColor.B = 255;
                            _selectionBoxColor.G = 255;
                        }
                    else
                    {
                        // Make White
                        _selectionBoxColor.R = 255;
                        _selectionBoxColor.B = 255;
                        _selectionBoxColor.G = 255;
                    }

                    // 5/1/2009 - Also check if within Buildable area of HQ!
                    if (itemToPlace.ShapeItem.InstancedItemData.ItemType != ItemType.flagMarker)
                        if (!IsWithinBuildableArea(itemToPlace, xValue, yValue))
                        {
                            // Make Red
                            _selectionBoxColor.R = 255;
                            _selectionBoxColor.B = 0;
                            _selectionBoxColor.G = 0;
                        }

                    var calculationX = (x + xRow) * aStarManagerNodeStride;
                    var calculationY = (y + yRow) * aStarManagerNodeStride;

                    // Left Triangle
                    // Vertex 1                    
                    SetOverlayVertexPosition(index, calculationX - halfNodeStride, calculationY - halfNodeStride);
                    index++;

                    // Vertex 2      
                    SetOverlayVertexPosition(index, calculationX + halfNodeStride, calculationY - halfNodeStride);
                    index++;

                    // Vertex 3  
                    SetOverlayVertexPosition(index, calculationX - halfNodeStride, calculationY + halfNodeStride);
                    index++;

                    // Right Triangle
                    // Vertex 1
                    SetOverlayVertexPosition(index, calculationX + halfNodeStride, calculationY - halfNodeStride);
                    index++;

                    // Vertex 2  
                    SetOverlayVertexPosition(index, calculationX + halfNodeStride, calculationY + halfNodeStride);
                    index++;

                    // Vertex 3    
                    SetOverlayVertexPosition(index, calculationX - halfNodeStride, calculationY + halfNodeStride);
                    index++;
                } // End Loop            
        }

        // 7/29/2009
        /// <summary>
        /// Helper method, for the <see cref="CreatePlacementOverlay"/> method, which updates a
        /// specific selection box, with the proper color and position data given.
        /// </summary>
        /// <param name="index">Index value of selection box to update</param>
        /// <param name="calculationX">X value</param>
        /// <param name="calculationY">Y value</param>
        private static void SetOverlayVertexPosition(int index, int calculationX, int calculationY)
        {
            // 11/28/2009 - Cache
            var selectionBox = _selectionBoxes[index];

            selectionBox.Position.X = calculationX;
            selectionBox.Position.Y = TerrainData.GetTerrainHeight(calculationX, calculationY);
            selectionBox.Position.Z = calculationY;
            selectionBox.Color = _selectionBoxColor;

            // 11/28/2009 - Update
            _selectionBoxes[index] = selectionBox;
        }


        // 11/28/2008
        // 12/14/2008: Fixed _itemToPlace disappering error.
        /// <summary>
        /// Removes the '_itemToPlace' from the <see cref="Player"/> 'SelectableItems' array, and
        /// deletes it from memory.
        /// </summary>
        public static void RemoveTempItemToPlace()
        {
            // 12/14/2008: Updated check to Not remove '_itemToPlace', if 'AttemptingItemPlacement'; this elminates the error
            //             caused during MP games where the Host places an SceneItemOwner and tells the client to place
            //             the same SceneItemOwner in its copy; however, at the end of the 'AddSceneItem_MP' method, in 
            //             the 'Player' class, this method was called and would remove the SceneItemOwner the client player
            //             was trying to place!
            if (_itemToPlace == null || AttemptingItemPlacement) return;

            // 1st - remove temp _itemToPlace
            // 11/11/2008: Note: InstanceItems Transform deletes also taken care of with this setting!  
            _itemToPlace.DrawStatusBar = false;

            // 6/15/2010 - Updated to use Remove methods.
            //_thisPlayer.SelectableItems.Remove(_itemToPlace as SceneItemWithPick);
            //_thisPlayer.ItemsSelected.Remove(sceneItemWithPick);
            var sceneItemWithPick = _itemToPlace as SceneItemWithPick; // 6/15/2010 - cache
            Player.RemoveSelectableItem(_thisPlayer, sceneItemWithPick);
            Player.RemoveItemSelected(_thisPlayer, sceneItemWithPick);

            // 6/16/2009 - Return to PoolManager
            _itemToPlace.ReturnItemToPool(true);

            // 2nd - null ref               
            _itemToPlace = null;
        }

        /// <summary>
        ///  Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this final dispose?</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

                if (_itemToPlace != null)
                    _itemToPlace.Dispose(true);

                if (_tShapeHelper != null)
                    _tShapeHelper.Dispose();

                // Null Refs
                _itemToPlace = null;
                _tShapeHelper = null;

                // Clear out all List & Dictionaries of Tiles
                ClearIFDTiles(true);

                // Null Refs
                _ifdTilesPerm = null;
                _ifdTiles = null;
                _ifdGroupControlTiles = null;
                _ifdTileGroups = null;
                _ifdTilesClicked = null;
                _ifdTileRetrieveGroups = null;
            }

            base.Dispose(disposing);
        }
    }
}