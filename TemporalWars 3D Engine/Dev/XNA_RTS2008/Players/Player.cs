#region File Description
//-----------------------------------------------------------------------------
// Player.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Net;
using PerfTimersComponent.Timers;
using PerfTimersComponent.Timers.Enums;
using SpeedCollectionComponent;
using TWEngine.Audio;
using TWEngine.Audio.Enums;
using TWEngine.GameCamera;
using TWEngine.GameScreens;
using TWEngine.HandleGameInput;
using TWEngine.IFDTiles.Enums;
using TWEngine.IFDTiles.Structs;
using TWEngine.InstancedModels.Enums;
using TWEngine.Interfaces;
using TWEngine.ItemTypeAttributes;
using TWEngine.ItemTypeAttributes.Structs;
using TWEngine.ParallelTasks;
using TWEngine.rtsCommands;
using TWEngine.Networking;
using TWEngine.InstancedModels;
using TWEngine.IFDTiles;
using TWEngine.rtsCommands.Enums;
using TWEngine.SceneItems;
using TWEngine.SceneItems.Enums;
using TWEngine.Shapes;
using TWEngine.Terrain;
using TWEngine.MemoryPool;
using TWEngine.ScreenManagerC;
using TWEngine.Shadows;

#if !XBOX360
using TWEngine.Console.Enums;
using System.Windows.Forms;
#else
using TWEngine.Common.Extensions;
#endif

namespace TWEngine.Players
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.Players"/> namespace contains the classes
    /// which make up the entire <see cref="Players"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
    
    /// <summary>
    /// Represents the current state of each <see cref="Player"/> in the game
    /// </summary>
    public sealed class Player : IDisposable, IFOWPlayer, IMinimapPlayer, IStatusBarPlayer
    {
        // 6/10/2012
        private readonly Guid[] _playerUniqueKeys = new[] {Guid.NewGuid(), Guid.NewGuid()};

        private int _cash = 30000;
        internal bool CashValueChanged = true; // 4/21/2009
       
        private int _energy;
        internal bool EnergyValueChanged = true; // 4/21/2009
       
        private int _energyUsed; 
        private int _health = 5;

        // 8/14/2009; // 4/24/2011 - Updated from 25 to 40 for default value.
        /// <summary>
        /// Maximum population the <see cref="Player"/> can have.
        /// </summary>
        private static int _populationMax = 40; 
       
        /// <summary>
        /// Collection of <see cref="SceneItemWithPick"/> currently selected by player.
        /// </summary>
        public readonly List<SceneItemWithPick> _itemsSelected;
        
        // 4/15/2008 -
        /// <summary>
        ///  Collection of <see cref="SceneItemWithPick"/> selectable items; made public for access from <see cref="Terrain"/> class.
        /// </summary>
        public readonly List<SceneItemWithPick> _selectableItems;
        
        // 6/15/2010
        /// <summary>
        /// Collection of <see cref="ReadOnlyCollection{T}"/> of selectable items.
        /// </summary>
        private readonly ReadOnlyCollection<SceneItemWithPick> _readOnlySelectableItems;

        // 6/15/2010
        /// <summary>
        /// Collection of <see cref="ReadOnlyCollection{T}"/> of items selected.
        /// </summary>
        private readonly ReadOnlyCollection<SceneItemWithPick> _readOnlyItemsSelected;

        // 2/15/2010 - ParallelFor Threaded instance
        private static ClosestItemParallelFor _parallelfor;

        // 8/2/2009 - Ref to Players 'HQ' Building.
        private static BuildingScene _hqBuilding1;
        private static BuildingScene _hqBuilding2;

        // 10/7/2009 - Keeps ref to the last SceneItem created!
        private SceneItemWithPick _lastSceneItemCreated;

        // 1/29/2009 - Set when an SceneItemOwner is ready to Delete; when True, this will force a RemoveAll call 
        //             on the ItemsSelected & SelectableItems arrays.
        internal bool DoRemoveAllCheck;
 
        // 8/24/2009  - Updated to use the SpeedCollection.
        // 12/17/2008 - 
        ///<summary>
        /// Dictionary to store references to the <see cref="_selectableItems"/> for quick retrieval during MP games.
        ///</summary>
        public readonly SpeedCollection<SceneItemWithPick> SelectableItemsDict = new SpeedCollection<SceneItemWithPick>(250);
        
        ///<summary>
        /// Dictionary to store references to the <see cref="_selectableItems"/> and <see cref="ScenaryItemScene"/> by 'Name'; specifically used for quick retrieval.
        ///</summary>
        /// <remarks>This is a STATIC method, since given 'Name' MUST BE UNIQUE for ALL PLAYERS!</remarks>
        public static Dictionary<string, SceneItem> SceneItemsByName = new Dictionary<string, SceneItem>();
        
        // 10/3/2008 - Array of Selectable Minimap Rectangles; populated by the Minimap.  However, it is stored
        //             in the Player class, since it relates to the Player's own units.  This allows reuse of the
        //             rectangles structs, without the need to constantly create and delete them within the minimap.
        private Rectangle[] _selectableMinimapRects = new Rectangle[1];

        // 5/1/2009 -  Buildable Rectangle Area, situated around some center
        //             map position; for example, the HQ.
        private Rectangle _buildableAreaRectangle = new Rectangle(0, 0, 2000, 2000);

        // 11/11/2008
        private readonly IFDTileManager _interfaceDisplay;     
  
        // 2/23/2009 - Memory PoolManager
        internal readonly PoolManager PoolManager;

        // 1/2/2010
        private static IMinimap _miniMap;
             

        // 7/15/2008 - Add IGameConsole Interface
#if !XBOX360
        private IGameConsole _gameConsole;
#endif

        // 9/3/2008 - In Network Game?
        //            Will be used in the HandleInput section to redirect
        //            Move orders, Attack Orders, etc, to the proper Method
        //            'MP' (Multiple Player) call.  This ensures the AI is only
        //            carried out on the Server, and the Position type data is
        //            relayed back to the clients.
        private static bool _isNetworkGame;
        internal readonly NetworkSession NetworkSession;        
       
        // 12/2/2008 - NetworkGameSyncer Class
        private readonly NetworkGameSyncer _networkGameSyncer;
       

        #region Properties

        // 6/8/2010
        /// <summary>
        /// Returns if this is a network game.
        /// </summary>
        public static bool IsNetworkGame
        {
            get { return _isNetworkGame; }
        }

        // 10/4/2009 - PlayerStats
        /// <summary>
        /// The Player statistics.
        /// </summary>
        public PlayerStats PlayerStats { get; set; }

        /// <summary>
        /// The side the player is using (1 or 2).
        /// </summary>
        public int PlayerSide { get; set; }

        /// <summary>
        /// The amount of cash the current <see cref="Player"/> has to spend
        /// </summary>       
        public int Cash
        {
            get{ return _cash; }
            set
            { 
                // 5/6/2009 - Play Sounds, depending on change (up/down)
                /*if (value > _cash)
                    SoundManager.Play(0, SoundBankGroup.Interface, Sounds.Interface_Cash_Up);
                else if (value < _cash)
                    SoundManager.Play(0, SoundBankGroup.Interface, Sounds.Interface_Cash_Down);*/

                _cash = value;
                CashValueChanged = true; // 4/21/2009
            }
        }

        /// <summary>
        /// The amount of energy the current <see cref="Player"/> has to use
        /// </summary>
        public int Energy
        {
            get { return _energy; }
            set 
            { 
                _energy = value;
                EnergyValueChanged = true; // 4/21/2009
            }
        }

        /// <summary>
        /// The amount of energy being used by current <see cref="Player"/>
        /// </summary>
        public int EnergyUsed
        {
            get { return _energyUsed; }
            set 
            { 
                _energyUsed = value;
                EnergyValueChanged = true; // 4/21/2009            
            }
        }

        /// <summary>
        /// Set when energy amount falls below zero.
        /// </summary>
        public bool EnergyOff { get; private set; }

        /// <summary>
        /// The current score for this <see cref="Player"/>
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// The current health level for this <see cref="Player"/>
        /// </summary>
        public int Health
        {
            get{ return _health; }  
            set{ _health = value; }
        }

        /// <summary>
        /// The current <see cref="Player"/> <see cref="Color"/>.
        /// </summary>
        public Color PlayerColor { get; private set; }

        /// <summary>
        /// The unique <see cref="Player"/>'s network number.
        /// </summary>
        public byte PlayerNumber { get; private set; }

        // 8/14/2009
        /// <summary>
        /// The <see cref="Player"/> current population value.
        /// </summary>
        public int Population { get; set; }

        // 10/5/2009
        /// <summary>
        /// True when this <see cref="Player"/> sighted an enemy <see cref="Player"/> units or buildings.
        /// </summary>
        public bool PlayerSightedEnemyPlayer { get; set; }

        /// <summary>
        /// Collection of <see cref="Rectangle"/> for <see cref="IMinimap"/>.
        /// </summary>
        public Rectangle[] SelectableMinimapRects
        {
            get { return _selectableMinimapRects; }
            set { _selectableMinimapRects = value; }
        }

        // 1/6/2010 - 
        /// <summary>
        /// Buildable <see cref="Rectangle"/> area, situated around some center
        /// map position; for example, the HeadQuarters.
        /// </summary>
        public Rectangle BuildableAreaRectangle
        {
            get { return _buildableAreaRectangle; }
        }

        // 4/24/2011
        /// <summary>
        /// Maximum population the <see cref="Player"/> can have.
        /// </summary>
        public static int PopulationMax
        {
            get { return _populationMax; }
            set { _populationMax = value; }
        }

        #endregion

        #region IFOWPlayer Members

       
        // 2/2/2010
        /*/// <summary>
        /// Populates the List of IFOWSceneItem types, by casting the current
        /// SceneItemWithPick types, and adding to the 'FowSceneItems' array.
        /// </summary>
        /// <param name="fowSceneItems">(OUT) List of IFowSceneItems.</param>
        public void GetSelectableItems(out List<IFOWSceneItem> fowSceneItems)
        {
            // 2/15/2010 - Put lock on 'SelectableItems' object itself.
            var selectableItems = _selectableItems; // 5/21/2010 - Cache
            lock (selectableItems)
            {
                var count = selectableItems.Count;

                if (_fowSceneItems == null)
                    _fowSceneItems = new List<IFOWSceneItem>(count);

                _fowSceneItems.Clear();

                // populate list to give out.
                for (var i = 0; i < count; i++)
                {
                    // cache
                    var selectableItem = selectableItems[i];
                    // skip NULL items
                    if (selectableItem == null) continue;

                    // cast to new IFowSceneItem type.
                    var fowSceneItem = selectableItem as IFOWSceneItem;
                    // Add to FOW list
                    _fowSceneItems.Add(fowSceneItem);

                } // End For
            } // End Lock

            // Pass new list over to FOW component.
            fowSceneItems = _fowSceneItems;
        }*/
       
        // 12/31/2009
        /// <summary>
        /// Helper method used to convert the <see cref="SceneItemWithPick"/> instance to a <see cref="IFOWSceneItem"/> type.
        /// </summary>
        /// <param name="sceneItemWithPick">Instance of <see cref="SceneItemWithPick"/>.</param>
        /// <returns>Instance of <see cref="IFOWSceneItem"/>.</returns>
        public static IFOWSceneItem SceneItemToFOWItem(SceneItemWithPick sceneItemWithPick)
        {
            return sceneItemWithPick;
        }

        #endregion

        #region IMinimapPlayer Members
        
        // 6/17/2010; 6/18/2010 - Add 'ActualCount' param.
        /// <summary>
        /// Helper method used to retrieve the <see cref="IMinimapSceneItem"/> collections type.
        /// </summary>
        /// <param name="minimapSceneItems">Returns a collection of <see cref="IMinimapSceneItem"/>.</param> 
        /// <param name="actualCount">The actual count value within the collection.</param> 
        public void GetSelectableItems(ref IMinimapSceneItem[] minimapSceneItems, out int actualCount)
        {
            //minimapSceneItems = _selectableItems.ConvertAll(new Converter<SceneItemWithPick, IMinimapSceneItem>(SceneItemToMinimapItem));

            // check if null
            if (minimapSceneItems == null)
                throw new ArgumentNullException("minimapSceneItems", @"Collection given cannot be Null!");

            // check for resizing
            actualCount = _selectableItems.Count;
            if (minimapSceneItems.Length < actualCount)
                Array.Resize(ref minimapSceneItems, actualCount);

            // iterate 'SelectableItems' and populate into _miniMapSceneItems.
            for (var i = 0; i < actualCount; i++)
            {
                try
                {
                    minimapSceneItems[i] = _selectableItems[i];
                }
                catch (ArgumentOutOfRangeException)
                {
#if DEBUG
                    Debug.WriteLine("GetSelectableItems method in Player class threw the ArgOutOfRangeExp");
#endif
                    break;
                }
            }
            
        }
        

        // 1/02/2010
        /// <summary>
        /// Helper method used to convert the <see cref="SceneItemWithPick"/> instance to a <see cref="IFOWSceneItem"/> type.
        /// </summary>
        /// <param name="sceneItemWithPick">Instance of <see cref="SceneItemWithPick"/>.</param>
        /// <returns>Instance of <see cref="IMinimapSceneItem"/>.</returns>
        public static IMinimapSceneItem SceneItemToMinimapItem(SceneItemWithPick sceneItemWithPick)
        {
            return sceneItemWithPick;
        }

       
        #endregion
       
        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        ///<param name="playerSide">Player side</param>
        ///<param name="playerColor">Player <see cref="Color"/></param>
        ///<param name="networkSession"><see cref="NetworkSession"/> instance</param>
        ///<param name="playerNumber">Player number</param>
        public Player(Game game, int playerSide, Color playerColor, 
                      NetworkSession networkSession, byte playerNumber)
        {            
            
            // 2/23/2009 - Create PoolManager
            PoolManager = new PoolManager(playerNumber);

            // 10/4/2009 - Create this PlayerStats instance
            PlayerStats = new PlayerStats(this);

            // 1/6/2010 - Init Arrays here; otherwise in level reloads, these will be null!
            _itemsSelected = new List<SceneItemWithPick>();
            _selectableItems = new List<SceneItemWithPick>();

            // 6/15/2010 - Create ReadOnlyCollection wrappers
            _readOnlySelectableItems = new ReadOnlyCollection<SceneItemWithPick>(_selectableItems);
            _readOnlyItemsSelected = new ReadOnlyCollection<SceneItemWithPick>(_itemsSelected);

            // 2/15/2010 - Init the ParallelFor loop.
            _parallelfor = new ClosestItemParallelFor(TemporalWars3DEngine.UseDotNetThreadPool);

            // 12/18/2008 - TODO: StopWatchTimers            
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.PlayerUpdate, false);//"PlayerUpdate"
           
            // 9/8/2008 - Set PlayerNumber
            PlayerNumber = playerNumber;

            // 11/11/2008 - Get IFD Service
            _interfaceDisplay = (IFDTileManager)game.Services.GetService(typeof(IFDTileManager));

            // 9/7/2008
            if (networkSession != null)
            {
                NetworkSession = networkSession;
                _isNetworkGame = true;

                // 12/2/2008 - Create instance of NetworkGameSyncer
                _networkGameSyncer = new NetworkGameSyncer(game, networkSession);

            }
                // 1/12/2010
            else
                _isNetworkGame = false;
            
                       
            PlayerSide = playerSide; //ImageNexusRTSGameEngine.ThisPlayer = (teamNo - 1);
            PlayerColor = playerColor;

            // 8/13/2008 - Get GameConsole Interface
#if !XBOX360
            _gameConsole = (IGameConsole)game.Services.GetService(typeof(IGameConsole));
#endif
            
            // 1/2/2010 - Get Minimap Interface
            _miniMap = (IMinimap) game.Services.GetService(typeof (IMinimap));

        }   
        
        // 9/3/2008 - Updated to include the 3 parameters.
        // 9/8/2008 - Updated to include the additional parameters of Latency, EnablePrediction,
        //            EnableSmoothing, & FramesBetween Packets; used for Interpolation on Client side.       
        ///<summary>
        /// Processes the <see cref="Player"/>, by iterating the internal collection of <see cref="SceneItem"/>,
        /// calling the <see cref="Update"/> method for each <see cref="SceneItem"/>.
        ///</summary>
        ///<param name="gameTime"><see cref="GameTime"/> instance</param>
        public void Update(GameTime gameTime)
        {

#if DEBUG
            // 12/18/2008 - Debug purposes
            StopWatchTimers.StartStopWatchInstance(StopWatchName.PlayerUpdate);//"PlayerUpdate"
#endif

            // 1/30/2008 - Extract TimeSpan components needed.
            var totalGameTime = gameTime.TotalGameTime;
            var elapsedGameTime = gameTime.ElapsedGameTime;

            var thisPlayer = TemporalWars3DEngine.SThisPlayer; // 8/12/2009

            // 1/30/2009 - Erase Energy values, since it rebuilt every cycle.
            _energy = 0;
            _energyUsed = 0;

            // 9/7/2008 - If Single Player game
            if (!IsNetworkGame)
            {
                // Called here if SP; otherwise, called from NetworkGameComponent's "UpdateLocalGamer()" method.
                DoHandleInput(gameTime);

                // 9/3/2008 - Add Updating for the Selectable Items.
                //            This use to be updated with the TerrainScreen's 'Scene' List, but is now updated
                //            here within the Player class.  This change is for the Networking Game Model.
                var selectableItemsCount = _selectableItems.Count; // 8/12/2009
                for (var i = 0; i < selectableItemsCount; i++)
                {
                    // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                    SceneItemWithPick selectableItem;
                    if (!GetSelectableItemByIndex(this, i, out selectableItem))
                        break;

                    if (selectableItem == null) continue;

                    // 5/24/2009 - 
                    selectableItem.Update(gameTime, ref totalGameTime, ref elapsedGameTime, false);
                } // End For
            } // Else, this is Network game
            else
            {
                // 12/2/2008 - Call update on NetworkGameSyncer class
                _networkGameSyncer.Update(gameTime);

                // 12/10/2008 - If other player-state, then do HoverPick check.
                if (PlayerNumber != thisPlayer)
                    PlayerHoverPickCheck(this); // then do HoverPick check.

                // 4/26/2010 - Determine if client-side call of MP game.
                var isClientCall = !NetworkSession.IsHost;

                // Call Network 'Update' Full method
                var selectableItemsCount = _selectableItems.Count; // 8/12/2009
                for (var i = 0; i < selectableItemsCount; i++)
                {
                    // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                    SceneItemWithPick selectableItem;
                    if (!GetSelectableItemByIndex(this, i, out selectableItem))
                        break;

                    if (selectableItem == null) continue;

                    // 5/24/2009 
                    selectableItem.Update(gameTime, ref totalGameTime, ref elapsedGameTime, isClientCall);
                } // End For
            } // End if MP or SP game

            // 1/29/2009: Updated to only call the 'RemoveAll' when an SceneItemOwner sets the 'DoRemoveAllCheck'.
            //            This is to reduce the Predicate garbage created by calling this List<> method.
            //      Note: ScenaryItem are added to TerrainScreen, which are removed in the TerrainScreen classes 'Update'.
            // 1/2/2009 - Call 'RemoveAll' to remove any items marked 'Delete'.
            if (DoRemoveAllCheck)
            {
                _selectableItems.RemoveAll(IsDeleted);
                _itemsSelected.RemoveAll(IsDeleted);
                DoRemoveAllCheck = false;
            }

            // 5/6/2009 - Do EnergyOff Check
            EnergyOnOffCheck();

#if DEBUG
            // 12/18/2008 - Debug purposes
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.PlayerUpdate);//"PlayerUpdate"
#endif

        }

        // 5/6/2009
        /// <summary>
        /// Check the total <see cref="Energy"/> VS. <see cref="EnergyUsed"/>, and sets the <see cref="EnergyOff"/> flag accordingly; also,
        /// will play the proper 'PowerUp/Down' sounds.
        /// </summary>
        private void EnergyOnOffCheck()
        {
            // 8/12/2009 - Cache
            var thisPlayer = TemporalWars3DEngine.SThisPlayer;

            // 6/10/2012 - Get players' UniqueKey
            var uniqueKey = _playerUniqueKeys[thisPlayer];

            if (_energy - _energyUsed < 0)
            {
                if (!EnergyOff)
                {
                    EnergyOff = true;

                    // 5/6/2009 - Play 'PowerDown' sound
                    if (PlayerNumber == thisPlayer)
                    {
                        AudioManager.Play(uniqueKey, Sounds.PowerDown3);
                    }
                }


            }
            else
            {
                if (EnergyOff)
                {
                    EnergyOff = false;

                    // 5/6/2009 - Play 'PowerUp' sound
                    if (PlayerNumber == thisPlayer)
                    {
                        AudioManager.Play(uniqueKey, Sounds.PowerUp3);
                    }
                }

            }
        }

        // 1/1/2009
        /// <summary>
        /// Predicate used in the RemoveAll method, of the <see cref="List{T}"/>, which removes any items
        /// with the 'delete' set to TRUE.
        /// </summary>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        /// <returns>true/false of results.</returns>
        private static bool IsDeleted(SceneItemWithPick item)
        {            
            return (item != null) && item.Delete;
        }

        // 9/3/2008 - Add this Draw command to draw the 'SelectableItems'.
        ///<summary>
        /// Iterates the collection of <see cref="_selectableItems"/>, calling the 'Render' method for each item.
        ///</summary>
        /// <remarks>
        /// This use to be updated with the <see cref="TerrainScreen"/> 'Scene' List, but is now updated
        /// here within the <see cref="Player"/> class.  This change is for the Networking Game Model.
        /// </remarks>
        public void DrawSelectionBoxes()
        {
            var selectableItems = _selectableItems; // 5/21/2010 - Cache

            // Draw SelectableItems
            var selectableItemsCount = selectableItems.Count; // 8/12/2009
            for (var i = 0; i < selectableItemsCount; i++)
            {
                // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                SceneItemWithPick selectableItem;
                if (!GetSelectableItemByIndex(this, i, out selectableItem))
                    break;

                if (selectableItem != null)
                    selectableItem.Render();
            } // End For
        }

        // 6/15/2010
        ///<summary>
        /// Adds the given <see cref="IEnumerable{SceneItemWithPick}"/> collection to the internal
        /// <see cref="_itemsSelected"/> collection.
        ///</summary>
        ///<param name="player"><see cref="Player"/> instance</param>
        ///<param name="collectionToAdd"><see cref="IEnumerable{SceneItemWithPick}"/> collection to add</param>
        public static void AddItemsSelectedRange(Player player, IEnumerable<SceneItemWithPick> collectionToAdd)
        {
            // check if null
            if (player == null) return;
            if (player._itemsSelected == null) return;

            // add collection to internal 'ItemsSelected' collection.
            player._itemsSelected.AddRange(collectionToAdd);
        }

       
        // 12/17/2008 // 6/15/2010: Updated to STATIC method, and add new 'addToMPDictionary' param.
        /// <summary>
        /// Adds a <see cref="SceneItemWithPick"/> class to the <see cref="_selectableItems"/> collection
        /// and the <see cref="SelectableItemsDict"/> dictionary for MP games.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the 'Name' or 'NetworkItemNumber' given is already in use.</exception>
        /// <param name="player"><see cref="Player"/> instance</param>
        /// <param name="itemToAdd"><see cref="SceneItemWithPick"/> to add</param>
        /// <param name="addToMpDictionary">If Network game, do you want to add <see cref="SceneItemWithPick"/> to internal MP dictionary?</param>
        public static void AddSelectableItem(Player player, SceneItemWithPick itemToAdd, bool addToMpDictionary)
        {
            // 6/15/2010 - Check if nulls
            if (player == null) return;
            if (player._selectableItems == null) return;

            player._selectableItems.Add(itemToAdd);

            // 6/10/2010 - Add to FogOfWar, if connected.
            var fogOfWar = (IFogOfWar) TemporalWars3DEngine.GameInstance.Services.GetService(typeof (IFogOfWar));
            if (fogOfWar != null)
            {
                fogOfWar.AddSelectableItem(itemToAdd);
            }

            // 10/8/2009 - If 'Name' exist, add to SelectableItemsName Dictionary; 'Name' used for scripting.
            if (!string.IsNullOrEmpty(itemToAdd.Name) && itemToAdd.Name != "$E")
            {
                // Make sure same name not already used.
                if (!SceneItemsByName.ContainsKey(itemToAdd.Name))
                    SceneItemsByName.Add(itemToAdd.Name, itemToAdd);
                else
                    throw new InvalidOperationException("Same Name is being used!");
            }

            // 6/15/2010 - Check if caller wants to add to MP Dictionary.
            // If MP game, add to Dictionary.
            if (player.NetworkSession == null || !addToMpDictionary) return;

            // 7/19/2009 - Make sure same key not already used.
            if (player.SelectableItemsDict.ContainsKey(itemToAdd.NetworkItemNumber))
                throw new InvalidOperationException("Same NetworkItemNumber is being used!");

            player.SelectableItemsDict.Add(itemToAdd.NetworkItemNumber, itemToAdd);
        }

        // 1/16/2011
        ///<summary>
        /// Transfers the given <paramref name="itemToTransfer"/> from its original <see cref="Player"/> instance, to
        /// the new <paramref name="newPlayerNumber"/>.
        ///</summary>
        ///<param name="itemToTransfer">Instance of <see cref="SceneItemWithPick"/> to transfer.</param>
        ///<param name="newPlayerNumber"><see cref="PlayerNumber"/> to transfer item to.</param>
        ///<exception cref="ArgumentNullException">Thrown when <paramref name="itemToTransfer"/> is null.</exception>
        ///<exception cref="InvalidOperationException">Thrown when either the new <see cref="PlayerNumber"/> is the same as 
        /// the current item to transfer, or the <see cref="Player"/> instances are null.</exception>
        public static void TransferSelectableItem(SceneItemWithPick itemToTransfer, byte newPlayerNumber)
        {
            // Check if null
            if (itemToTransfer == null)
                throw new ArgumentNullException("itemToTransfer");

            // Check if old PlayerNumber is equal to new PlayerNumber
            if (itemToTransfer.PlayerNumber == newPlayerNumber)
                throw new InvalidOperationException(
                    "New player number given is the same as the old player number for the given item to transfer.");

            // Get Player old instance
            Player player;
            TemporalWars3DEngine.GetPlayer(itemToTransfer.PlayerNumber, out player);

            // Check if player instance is null
            if (player == null)
                throw new InvalidOperationException(string.Format("Player number {0} instance is null.", itemToTransfer.PlayerNumber));

            // Remove marked item
            itemToTransfer.Delete = true;
            lock (player._selectableItems)
            {
                player._selectableItems.RemoveAll(IsDeleted); 
            }

            // Get Player new instance
            TemporalWars3DEngine.GetPlayer(itemToTransfer.PlayerNumber, out player);

            // Check if player instance is null
            if (player == null)
                throw new InvalidOperationException(string.Format("Player number {0} instance is null.", newPlayerNumber));

            // Add item to new player instance
            itemToTransfer.Delete = false;
            lock (player._selectableItems)
            {
                player._selectableItems.Add(itemToTransfer);
            }

            // Set new PlayerNumber value.
            itemToTransfer.PlayerNumber = newPlayerNumber;

        }

        

        // 6/15/2010
        ///<summary>
        /// Removes all <see cref="SceneItemWithPick"/> in the internal <see cref="_itemsSelected"/> collection, using
        /// the given <see cref="Predicate{T}"/> function.
        ///</summary>
        ///<param name="player"><see cref="Player"/> instance</param>
        ///<param name="predicate"><see cref="Predicate{T}"/> function used for removal</param>
        public static void RemoveAllItemsSelected(Player player, Predicate<SceneItemWithPick> predicate)
        {
            // check if null
            if (player == null) return;
            if (player._itemsSelected == null) return;

            // Remove using given predicate.
            player._itemsSelected.RemoveAll(predicate);

        }

        // 6/15/2010
        ///<summary>
        /// Iterates the internal <see cref="_selectableItems"/> collection, removing all
        /// items where the given <paramref name="itemInstanceKey"/> is valid.
        ///</summary>
        /// <remarks>This is generally used to remove scenary items, which use this unique instance key.</remarks>
        ///<param name="player"><see cref="Player"/> instance</param>
        ///<param name="itemInstanceKey">instance key to search for in collection</param>
        public static void RemoveAllSelectableItemsByItemInstanceKey(Player player, int itemInstanceKey)
        {
            // check if player null
            if (player == null) return;

            // check if collection null
            var selectableItems = player._selectableItems;
            if (selectableItems == null) return;
 
            // call RemoveAll predicate removal method
            _matchItemKey = itemInstanceKey;
            selectableItems.RemoveAll(HasItemInstanceKey);

        }

        // 6/15/2010
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

        // 12/17/2008; // 6/15/2010 - Updated to STATIC method.
        /// <summary>
        /// Removes a <see cref="SceneItemWithPick"/> class from the <see cref="_selectableItems"/> collection
        /// and the <see cref="SelectableItemsDict"/> dictionary for MP Games.
        /// </summary>
        /// <param name="player"><see cref="Player"/> instance</param>
        /// <param name="itemToRemove"><see cref="SceneItemWithPick"/> to remove</param>
        public static void RemoveSelectableItem(Player player, SceneItemWithPick itemToRemove)
        {
            // 6/15/2010 - Check if player null
            if (player == null) return;

            // 1/2/2009 - Set SceneItemOwner to Delete, which removed from '_selectableItems'.            
            itemToRemove.Delete = true;

            // 6/10/2010 - Remove from FogOfWar, if connected.
            var fogOfWar = (IFogOfWar)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(IFogOfWar));
            if (fogOfWar != null)
            {
                fogOfWar.RemoveSelectableItem(itemToRemove);
            }

            // check if MP network game.
            if (player.NetworkSession == null) return;

            player.SelectableItemsDict.Remove(itemToRemove.NetworkItemNumber);
        }

        // 6/15/2010 -
        ///<summary>
        /// Clears the internal <see cref="_selectableItems"/> collection.
        ///</summary>
        ///<param name="player"><see cref="Player"/> instance</param>
        public static void ClearSelectableItems(Player player)
        {
            if (player == null) return;

            player._selectableItems.Clear();
        }

        // 6/15/2010
        ///<summary>
        /// Clears the internal <see cref="_itemsSelected"/> collection.
        ///</summary>
        ///<param name="player"><see cref="Player"/> instance</param>
        public static void ClearItemsSelected(Player player)
        {
            if (player == null) return;

            player._itemsSelected.Clear();
        }

        // 6/15/2010
        ///<summary>
        /// Removes a <see cref="SceneItemWithPick"/> class from the <see cref="_itemsSelected"/> collection
        ///</summary>
        ///<param name="player"><see cref="Player"/> instance</param>
        /// <param name="itemToRemove"><see cref="SceneItemWithPick"/> to remove</param>
        public static void RemoveItemSelected(Player player, SceneItemWithPick itemToRemove)
        {
            // 6/15/2010 - Check if player null
            if (player == null || player._itemsSelected == null) return;

            player._itemsSelected.Remove(itemToRemove);
        }

        // 10/10/2009
        /// <summary>
        /// Checks if given <see cref="SceneItemWithPick"/> <paramref name="sceneItemName"/> has sighted the <see cref="Player"/> <see cref="_selectableItems"/> of
        /// the given <see cref="ItemType"/>. (Scripting purposes)
        /// </summary>
        /// <remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
        /// <param name="player">this instance of <see cref="Player"/></param>
        /// <param name="sceneItemName"><see cref="SceneItemWithPick"/> name to check; can be both a 'Selectable' or 'Scenary' <see cref="SceneItem"/>.</param>
        /// <param name="itemTypeToCheck"><see cref="ItemType"/> Enum</param>
        /// <exception cref="ArgumentException">Thrown when given <paramref name="sceneItemName"/> is not valid.</exception>
        /// <returns>true/false of result</returns>
        public static bool NamedSceneItemSightedItemType(Player player, string sceneItemName, ItemType itemTypeToCheck)
        {
            // 1st - retrieve 'Named' item from Dictionary
            SceneItem namedSceneItem;
            if (SceneItemsByName.TryGetValue(sceneItemName, out namedSceneItem))
            {
                // check if Scenary item; if so, then MUST get proper instance within item.
                var scenaryItem = (namedSceneItem as ScenaryItemScene);
                if (scenaryItem != null && !scenaryItem.SearchByName(sceneItemName))
                    throw new ArgumentException(@"Given sceneItem Name is NOT VALID!", "sceneItemName");

               
                var selectableItems = player._selectableItems; // 5/21/2010 - Cache
                // 2nd - iterate Player's 'SelectableItems', and check if within View range for any of them.
                var count = selectableItems.Count;
                for (var i = 0; i < count; i++)
                {
                    // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                    SceneItemWithPick sceneItem;
                    if (!GetSelectableItemByIndex(player, i, out sceneItem))
                        break;

                    // skip if null
                    if (sceneItem == null) continue;

                    // skip if not proper ItemType to check
                    if (sceneItem.ShapeItem.ItemType != itemTypeToCheck) continue;

                    // if found item within view, return immediately
                    if (namedSceneItem.WithinView(sceneItem)) return true;

                    // Also check 'ScenaryItem' cast version; this is necessary, since
                    // the 'Position' value will differ in the 'Overload' version of 
                    // scenaryItem, compared to its base version of 'SceneItem'.
                    if (scenaryItem != null && scenaryItem.WithinView(sceneItem)) return true;
                } // End For Loop

                return false;
            }

            throw new ArgumentException(@"Given sceneItem Name is NOT VALID!", "sceneItemName");
        }

        // 10/10/2009
        /// <summary>
        /// Checks if given <see cref="SceneItemWithPick"/> <paramref name="sceneItemName"/> 
        /// has the given <see cref="DefenseAIStance"/> stance. (Scripting purposes)
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when given <paramref name="sceneItemName"/> is not valid, or is not a <see cref="SceneItemWithPick"/> instance.</exception>
        /// <param name="sceneItemName"><see cref="SceneItemWithPick"/> to check (Only Selectable SceneItems allowed)</param>
        /// <param name="defenseAIStanceToCheck"><see cref="DefenseAIStance"/> stance to check; like 'Guard' stance.</param>
        /// <returns>True/False of result</returns>
        public static bool NamedSceneItemIsUsingStance(string sceneItemName, DefenseAIStance defenseAIStanceToCheck)
        {
            // 1st - retrieve 'Named' item from Dictionary
            SceneItem namedSceneItem;
            if (SceneItemsByName.TryGetValue(sceneItemName, out namedSceneItem))
            {
                // make sure item is a Selectable SceneItemWithPick.
                var selectableItem = (namedSceneItem as SceneItemWithPick);
                if (selectableItem != null)
                {
                    return (selectableItem.DefenseAIStance == defenseAIStanceToCheck);
                    
                } // End if SceneITemWithPick
                
                throw new ArgumentException(@"Named sceneItem MUST be a SelectableItem, and NOT a scenaryItem!", "sceneItemName");
            }

            throw new ArgumentException(@"Given sceneItem Name is NOT VALID!", "sceneItemName");
        }

        // 10/12/2009
        /// <summary>
        /// Flashes a <see cref="SceneItem"/> <paramref name="sceneItemName"/> white, for a specified amount of time in seconds. (Scripting Purposes)
        /// </summary>
        /// <remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
        /// <exception cref="ArgumentException">Thrown when given <paramref name="sceneItemName"/> is not valid.</exception>
        /// <param name="sceneItemName"><see cref="SceneItem"/> name to check; can be both a 'Selectable' or 'Scenary' sceneItem.</param>
        /// <param name="timeInSeconds">How long to flash item (Seconds)</param>
        public static void FlashNamedSceneItemWhiteForSpecifiedAmountOfTime(string sceneItemName, int timeInSeconds)
        {
            // 1st - retrieve 'Named' item from Dictionary
            SceneItem namedSceneItem;
            if (SceneItemsByName.TryGetValue(sceneItemName, out namedSceneItem))
            {
                // check if Scenary item.
                var scenaryItem = (namedSceneItem as ScenaryItemScene);
                if (scenaryItem != null)
                    // Tell item to Flash White.
                    scenaryItem.FlashItemWhite(sceneItemName, timeInSeconds);
                else
                    // Tell item to Flash White.
                    namedSceneItem.FlashItemWhite(timeInSeconds);

                return;
            }

            throw new ArgumentException(@"Given sceneItem Name is NOT VALID!", "sceneItemName");
        }

        // 10/8/2009
        /// <summary>
        /// Checks if <see cref="Player"/> has sighted the <see cref="SceneItem"/> <paramref name="sceneItemName"/>.  A 'Named' item can
        /// either be a 'SelectableItem' or 'ScenaryItem'. (Scripting purposes)
        /// </summary>
        /// <remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
        /// <exception cref="ArgumentException">Thrown when given <paramref name="sceneItemName"/> is not valid.</exception>
        /// <param name="player">this instance of <see cref="Player"/></param>
        /// <param name="sceneItemName">Name of <see cref="SceneItem"/> to check</param>
        /// <returns>True/False of result</returns>
        public static bool PlayerSightedNamedItem(Player player, string sceneItemName)
        {
            // 1st - retrieve 'Named' item from Dictionary
            SceneItem namedItemToCheck;
            if (SceneItemsByName.TryGetValue(sceneItemName, out namedItemToCheck))
            {
                // Check if 'namedItemToCheck' is a scenaryItem; if so, then it will contain
                // an internal array of instances, and therefore, the proper instance key needs
                // to be set, which affects what 'Position' will be returned at the base level Property
                // call!  This will affect the outcome of the 'WithinView' check below!
                var scenaryItem = (namedItemToCheck as ScenaryItemScene);
                if (scenaryItem != null && !scenaryItem.SearchByName(sceneItemName))
                    throw new ArgumentException(@"Given sceneItem Name is NOT VALID!", "sceneItemName");
                
                var selectableItems = player._selectableItems; // 5/21/2010 - Cache

                // 2nd - iterate Player's 'SelectableItems', and check if within View range for any of them.
                var count = selectableItems.Count;
                for (var i = 0; i < count; i++)
                {
                    // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                    SceneItemWithPick sceneItem;
                    if (!GetSelectableItemByIndex(player, i, out sceneItem))
                        break;

                    // skip if null
                    if (sceneItem == null) continue;

                    // if found item within view, return immediately
                    if (sceneItem.WithinView(namedItemToCheck)) return true;

                    // Also check 'ScenaryItem' cast version; this is necessary, since
                    // the 'Position' value will differ in the 'Overload' version of 
                    // scenaryItem, compared to its base version of 'SceneItem'.
                    if (sceneItem.WithinView(scenaryItem)) return true;
                } // End For
                return false;
            }

            throw new ArgumentException(@"Given sceneItem Name is NOT VALID!", "sceneItemName");
        }

        // 10/8/2009
        /// <summary>
        /// Adds a given <see cref="SceneItem"/> to the internal <see cref="SceneItemsByName"/> Dictionary, which allows 
        /// searching by user-defined names via scripting conditions.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sceneItemToAdd"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when given <paramref name="sceneItemName"/> already exist.</exception>
        /// <param name="sceneItemName">Name for <see cref="SceneItem"/></param>
        /// <param name="sceneItemToAdd"><see cref="SceneItem"/> instance to add</param>
        /// <returns>True/False</returns>
        public static bool AddSceneItemToNamesDictionary(string sceneItemName, SceneItem sceneItemToAdd)
        {
            // make sure name given is not null or contains '$E'.
            if (string.IsNullOrEmpty(sceneItemName) || sceneItemName == "$E")
                return false;

            // make sure 'sceneItem' to add is not NULL
            if (sceneItemToAdd == null)
                throw new ArgumentNullException("sceneItemToAdd", @"The SceneItem to add CAN NOT BE NULL!");

            // check if 'Name' already exist
            if (!SceneItemsByName.ContainsKey(sceneItemName))
            {              

                // Add new record to Dictionary
                SceneItemsByName.Add(sceneItemName, sceneItemToAdd);
                return true;
            }

            // else error, because name already exist!
            throw new ArgumentException(@"Given sceneItem Name is NOT VALID!", "sceneItemName");
        }

        // 10/8/2009
        /// <summary>
        /// Adds the <see cref="List{SceneItem}"/> to the internal  <see cref="SceneItemsByName"/> Dictionary, which allows 
        /// searching by user-defined names via scripting conditions.
        /// </summary>
        /// <param name="itemsToAdd"><see cref="List{SceneItem}"/> to add</param>
        /// <typeparam name="TType">Type of <see cref="SceneItem"/> class.</typeparam>
        public static void AddSceneItemToNamesDictionary<TType>(IList<TType> itemsToAdd) where TType : SceneItem
        {
            // iterate given List, and add those which are necessary
            var count = itemsToAdd.Count; // 5/21/2010 - Cache
            for (var i = 0; i < count; i++)
            {
                // cache
                var sceneItemToAdd = itemsToAdd[i];

                // skip if NULL
                if (sceneItemToAdd == null) continue;

                // 1st - Check if ScenarySceneItem, with batch of internal instances?
                var scenarySceneItem = sceneItemToAdd as ScenaryItemScene;
                if (scenarySceneItem != null)
                {
                    // does this scenarySceneItem contain an internal batch of instances?
                    var scenaryItemsCount = scenarySceneItem.ScenaryItems.Count; // cache
                    if (scenaryItemsCount > 0)
                    {
                        // yes, then iterate iternal array, and check each record.
                        for (var j = 0; j < scenaryItemsCount; j++)
                        {
                            // cache scenaryItemData struct
                            var scenaryItemData = scenarySceneItem.ScenaryItems[j];

                            // 6/7/2012: NOTE: Removed logic 'break' below; otherwise, names missing for same ItemType batch.
                            // If even a single record was added successfully, then break
                            // out of iteration, since what is stored is the instance of the 
                            // parent 'ScenarySceneItem'!
                            AddSceneItemToNamesDictionary(scenaryItemData.name, scenarySceneItem);
                        }
                    }
                }
                    // Else, this is a 'SelectableItem'.
                else
                    AddSceneItemToNamesDictionary(sceneItemToAdd.Name, sceneItemToAdd);


            } // End For loop
        }

        #region HandleInput Methods

        // 2/5/2009: Add 'GameTime' parameter.
        // 9/3/2008 - HandleInput for the Player's Class       
        ///<summary>
        /// Handles input for given <see cref="Player"/>
        ///</summary>
        ///<param name="gameTime"><see cref="GameTime"/> instance</param>
        public void DoHandleInput(GameTime gameTime)
        {
            

#if !XBOX360
    // 9/9/2008 - Only HandleInput when GameConsole Closed.
            if (_gameConsole != null)
                if (_gameConsole.ConsoleState != ConsoleState.Closed)
                    return;
#endif

            // 2/5/2009 - SpecialGroup Selection check; for example, the Ctrl-1 or 1 key is pressed.
            HandleInput.SpecialGroupSelectionCheck(this); // 6/15/2010 - Updated to pass ROC.

            // 4/28/2009 - Check Player GamerInput
            HandleInput.PlayerInputCheck(this, gameTime);

        }


        // 11/10/2008
        /// <summary>
        /// <see cref="EventHandler"/> for <see cref="SceneItem"/> created event, which is connected to the IFD_Placement tiles.
        /// </summary>
        ///<param name="sender"><see cref="Object"/> instance as sender event</param>
        ///<param name="e"><see cref="ItemCreatedArgs"/> instance</param>
        public void IFDPlacement_ItemCreated(object sender, ItemCreatedArgs e)
        {
            // 5/21/2010 - Refactored out core code to new STATIC method.
            StartAddSceneItem(this, e);
        }   

        // 5/21/2010
        ///<summary>
        /// Method helper, which checks if SP or MP game type, and calls the appropriate 'AddSceneItem' methods.
        ///</summary>
        ///<param name="player">this instance of <see cref="Player"/></param>
        ///<param name="e"><see cref="ItemCreatedArgs"/> instance</param>
        private static void StartAddSceneItem(Player player, ItemCreatedArgs e)
        {
            // is Network Game?
            if (player.NetworkSession == null)
            {
                // No, then add directly for single-player games
                AddSceneItem(player, 0, e);
            }
            else
            {

                // Is this Host?
                if (player.NetworkSession.IsHost)
                {
                    // Increase NetworkItemNumber Counter
                    var newNetworkItemNumber =  NetworkGameComponent.IncreaseNetworkItemCount();

                    // Add Host's SceneItem                   
                    AddSceneItem(player, newNetworkItemNumber, e);
                    

                    // Send Command for Client to also Add Host's Tank in it's copy.                  
                    RTSCommAddSceneItem addSceneItem;
                    PoolManager.GetNode(out addSceneItem);

                    addSceneItem.Clear();
                    addSceneItem.NetworkCommand = NetworkCommands.AddSceneItem;
                    addSceneItem.ItemType = e.ItemType;
                    addSceneItem.BuildingType = e.BuildingType;
                    addSceneItem.ItemGroupToAttack = e.ItemGroupToAttack;
                    addSceneItem.BuildingNetworkItemNumber = e.BuildingProducerNetworkItemNumber;
                    addSceneItem.ProductionType = e.ProductionType; // 4/30/2009
                    addSceneItem.AtPosition = e.PlaceItemAt;
                    addSceneItem.NetworkItemNumber = newNetworkItemNumber;
                    addSceneItem.PlayerNumber = player.PlayerNumber; // was ImageNexusRTSGameEngine.ThisPlayer
                    addSceneItem.IsBotHelper = e.IsBotHelper; // 8/4/2009
                    addSceneItem.LeaderNetworkItemNumber = e.LeaderUniqueNumber; // 8/4/2009
                    addSceneItem.LeaderPlayerNumber = e.LeaderPlayerNumber; // 8/5/2009


                    NetworkGameComponent.AddCommandsForClientG(addSceneItem); // 12/2/2008 - Updated to 'ReliableInOrder' Queue.

                }
                    // Else, this is Client, so send Request to Server first.
                else
                {
                    // Create Add RTSCommand for Server and Add to Queue

                    RTSCommAddSceneItem addSceneItem;
                    PoolManager.GetNode(out addSceneItem);

                    addSceneItem.Clear();
                    addSceneItem.NetworkCommand = NetworkCommands.ReqAddSceneItem;
                    addSceneItem.ItemType = e.ItemType;
                    addSceneItem.BuildingType = e.BuildingType;
                    addSceneItem.ItemGroupToAttack = e.ItemGroupToAttack;
                    addSceneItem.BuildingNetworkItemNumber = e.BuildingProducerNetworkItemNumber;
                    addSceneItem.ProductionType = e.ProductionType;
                    addSceneItem.AtPosition = e.PlaceItemAt;
                    addSceneItem.PlayerNumber = player.PlayerNumber; // was ImageNexusRTSGameEngine.ThisPlayer
                    addSceneItem.IsBotHelper = e.IsBotHelper; // 8/4/2009
                    addSceneItem.LeaderNetworkItemNumber = e.LeaderUniqueNumber; // 8/4/2009
                    addSceneItem.LeaderPlayerNumber = e.LeaderPlayerNumber; // 8/5/2009

                    NetworkGameComponent.AddCommandsForServerG(addSceneItem); // 12/2/2008 - Updated to 'ReliableInOrder' Queue.
                }
            } // Is NetworkGame?
        }
    
        // 10/7/2009; 5/21/2010: Updated to be STATIC method.
        /// <summary>
        /// This overload version adds a single <see cref="SceneItem"/> to the game world; currently
        /// can be called from the 'ItemTool' form and the <see cref="TerrainStorageRoutines"/> class
        /// when loading back the 'SeletableItems' used for scripting. (Scripting Purposes)
        /// </summary>
        /// <param name="player">this instance of <see cref="Player"/></param>
        /// <param name="itemTypeToUse"><see cref="ItemType"/> to add</param>
        /// <param name="placeItemAt"><see cref="Vector3"/> location to add item</param>
        /// <returns>The unique <see cref="SceneItemWithPick.SceneItemNumber"/></returns>
        public static int AddSceneItem(Player player, ItemType itemTypeToUse, Vector3 placeItemAt)
        {
            // 11/4/2009 - Check if Network game.
            if (player.NetworkSession != null) return 0;
           
            // Which PlayableItem 'ItemGroupType' is this?
            PlayableItemTypeAttributes playableAtts;
            if (!PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(itemTypeToUse, out playableAtts)) return -1;

            // 2/6/2011 - Set ProductionType.
            // 1st - add SceneItemOwner via player class
            var itemArgs = new ItemCreatedArgs(playableAtts.ItemGroupType, playableAtts.ProductionType, itemTypeToUse,
                                               null, placeItemAt, 0, null, 0);

            // 2nd - Call ItemCreated 'EventHandler' directly to create final World sceneitem
            player.IFDPlacement_ItemCreated(null, itemArgs);

            // return ref to the SceneItem, via the unique 'SceneItemNumber' int.
            return player._lastSceneItemCreated.SceneItemNumber;
        }

        // 11/10/2008
        /// <summary>
        /// Adds a single <see cref="SceneItem"/> to game world, for either SP or MP games.
        /// For SP games, the <paramref name="networkItemNumber"/> will be 0.
        /// </summary>
        /// <param name="player">this instance of <see cref="Player"/></param>
        /// <param name="networkItemNumber">Unique <see cref="SceneItemWithPick.NetworkItemNumber"/> for MP games.</param>
        /// <param name="e"><see cref="ItemCreatedArgs"/> instance</param>
        public static void AddSceneItem(Player player, int networkItemNumber, ItemCreatedArgs e)
        {
            // Select Proper ProductionType
            switch (e.BuildingType)
            {
                case ItemGroupType.Buildings:
                    // Create new instance of SceneItemOwner, and assign network number                   
                    BuildingScenePoolItem poolNode1;
                    player.PoolManager.GetNode(out poolNode1);
                    var buildingToPlace = (BuildingScene) poolNode1.SceneItemInstance;
                    player._lastSceneItemCreated = buildingToPlace; // 10/7/2009

                    CreateSelectableItemFromPoolManager(player, buildingToPlace, networkItemNumber, e, true);                               

                    break;
                case ItemGroupType.Shields:
                    // Create new instance of SceneItemOwner, and assign network number                  
                    DefenseScenePoolItem poolNode2;
                    player.PoolManager.GetNode(out poolNode2);
                    var defenseToPlace = (DefenseScene) poolNode2.SceneItemInstance;
                    player._lastSceneItemCreated = defenseToPlace; // 10/7/2009

                    CreateSelectableItemFromPoolManager(player, defenseToPlace, networkItemNumber, e, true);                   

                    break;
                case ItemGroupType.People:
                    break;
                case ItemGroupType.Vehicles:
                    // Create new instance of SceneItemOwner, and assign network number
                    SciFiTankScenePoolItem poolNode3;
                    player.PoolManager.GetNode(out poolNode3);
                    var itemToPlace = (SciFiTankScene) poolNode3.SceneItemInstance;
                    player._lastSceneItemCreated = itemToPlace; // 10/7/2009

                    // 11/28/2009 - Check if 'IsBotHelper', and turn OFF counter reduction if TRUE.
                    if (e.IsBotHelper) poolNode3.ReduceIFDCounter = false;

                    CreateSelectableItemFromPoolManager(player, itemToPlace, networkItemNumber, e, false);

                    // Open Door, which will in turn trigger the Move Command
                    DoMoveItemOutCheck(player, e, itemToPlace);


                    break;
                case ItemGroupType.Airplanes: // 2/3/2009
                    // Create new instance of SceneItemOwner, and assign network number 
                    SciFiAircraftScenePoolItem poolNode4;
                    player.PoolManager.GetNode(out poolNode4);
                    var aircraftToPlace = (SciFiAircraftScene) poolNode4.SceneItemInstance;
                    player._lastSceneItemCreated = aircraftToPlace; // 10/7/2009

                    // 11/28/2009 - Check if 'IsBotHelper', and turn OFF counter reduction if TRUE.
                    if (e.IsBotHelper) poolNode4.ReduceIFDCounter = false;

                    CreateSelectableItemFromPoolManager(player, aircraftToPlace, networkItemNumber, e, false);

                    // Open Door, which will in turn trigger the Move Command
                    DoMoveItemOutCheck(player, e, aircraftToPlace);

                    break;
                default:
                    break;
            } // End Switch
        }

        // 8/3/2009
        /// <summary>
        /// Adds some <see cref="SceneItemWithPick"/> type instance, to the <see cref="_selectableItems"/> collection.
        /// </summary>
        /// <typeparam name="T"><see cref="SceneItemWithPick"/> base type</typeparam>
        /// <param name="player"></param>
        /// <param name="itemToAdd"><see cref="SceneItemWithPick"/> base type to add</param>
        /// <param name="networkItemNumber">Unique <see cref="SceneItemWithPick.NetworkItemNumber"/></param>
        /// <param name="itemsArgs"><see cref="ItemCreatedArgs"/> instance</param>
        /// <param name="isFinalPosition">Is this final position?</param>
        private static void CreateSelectableItemFromPoolManager<T>(Player player, T itemToAdd, int networkItemNumber, 
                                                                    ItemCreatedArgs itemsArgs, bool isFinalPosition) where T : SceneItemWithPick
        {

            itemToAdd.ShapeItem.SetInstancedItemTypeToUse(itemsArgs.ItemType);
            itemToAdd.LoadPlayableAttributesForItem(itemsArgs, isFinalPosition);
            itemToAdd.Position = itemsArgs.PlaceItemAt;
            ((ShapeWithPick)itemToAdd.ShapeItem).PlayerNumber = player.PlayerNumber;
            itemToAdd.PlayerNumber = player.PlayerNumber;
            itemToAdd.NetworkItemNumber = networkItemNumber;

            // Check if Building or Defense scene types
            Type type = typeof(T);
            var thisPlayer = TemporalWars3DEngine.SThisPlayer; // 8/12/2009
            switch (type.Name)
            {
                case "BuildingScene":
                    itemToAdd.SetPlacement(ref itemsArgs.PlaceItemAt); // DefenseScene?
                    itemToAdd.TimePlacedAt = itemsArgs.TotalSeconds; // DefenseScene?     

                    // Let's now remove the temporary itemPlacement from InterfaceDisplay
                    IFDTileManager.RemoveTempItemToPlace();

                    // 1/6/2009 - Make sure ShadowMap captures SceneItemOwner                    
                    ShadowMap.DoPreShadowMapTextures = true;

                    // 5/1/2009
                    // Add Building SubQueue to GroupControl Tile, if our player.
                    if (player.PlayerNumber == thisPlayer)
                        AddBuildingProductionSubQueue(player, itemsArgs, itemToAdd);

                    // 4/30/2009 - Call OnItemPlaced for HQs, to activate the proper initial tiles.
                    if (player.PlayerNumber == thisPlayer)
                        AddRelevantPlayerSideTiles(itemsArgs);

                    // 8/2/2009 - Save ref to Player HQs
                    player.SaveReferenceToPlayersHQ(itemToAdd);
                    break;
                case "DefenseScene":
                    itemToAdd.SetPlacement(ref itemsArgs.PlaceItemAt); // DefenseScene?
                    itemToAdd.TimePlacedAt = itemsArgs.TotalSeconds; // DefenseScene?     

                    // Let's now remove the temporary itemPlacement from InterfaceDisplay
                    IFDTileManager.RemoveTempItemToPlace();

                    // 1/6/2009 - Make sure ShadowMap captures SceneItemOwner                    
                    ShadowMap.DoPreShadowMapTextures = true;
                    break;
                default:
                    break;
            }

            // Add to the Selectable Items                    
            AddSelectableItem(player, itemToAdd, true);

            // CreateBotHelpers, if necessary.
            itemToAdd.CreateBotHelpers(itemsArgs);

        }

        // 8/4/2009
        /// <summary>
        /// Checks if the building given has a ProductionType; if true, then the
        /// proper <see cref="IFDTiles"/> Subqueue is added to the players window.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when given <paramref name="toPlace"/> is null.</exception>
        /// <param name="player">this instance of <see cref="Player"/></param>
        /// <param name="e"><see cref="ItemCreatedArgs"/> instance</param>
        /// <param name="toPlace"><see cref="SceneItemWithPick"/> to place</param>
        private static void AddBuildingProductionSubQueue(Player player, ItemCreatedArgs e, SceneItemWithPick toPlace)
        {
            if (toPlace == null) throw new ArgumentNullException("toPlace");

            if (e.ProductionType == null) return;

            // 8/12/2009 - Cache
            var buildingToPlace = (BuildingScene)toPlace;

            // 10/9/2009 - Store ProductionType into BuildingScene
            buildingToPlace.ProductionType = e.ProductionType;

            SubQueueKey subQueueKey;
            IFDTileManager.AddNewBuildingQueueTab(buildingToPlace, e.ProductionType.Value, out subQueueKey);
            player._interfaceDisplay.CreateIFDTiles((IFDGroupControlType)e.ProductionType.Value, subQueueKey, buildingToPlace, player.PlayerSide);

            // Save SubQueueKey into Building.
            buildingToPlace.SubQueueKeyIFDTiles = subQueueKey;
        }

        // 8/4/2009
        /// <summary>
        /// Adds the revelant PlayerSide <see cref="IFDTiles"/>.
        /// </summary>
        /// <param name="e"><see cref="ItemCreatedArgs"/> instance</param>
        private static void AddRelevantPlayerSideTiles(ItemCreatedArgs e)
        {
            // TODO: Abstract the building types from the HQ-1/2 types?!

            switch (e.ItemType)
            {
                case ItemType.sciFiBldb15:
                    if (IFDTileManager.HQSide1 != null) IFDTileManager.HQSide1.OnItemPlaced();
                    break;
                case ItemType.sciFiBldb14:
                    if (IFDTileManager.HQSide2 != null) IFDTileManager.HQSide2.OnItemPlaced();
                    break;
            }
        }

        // 8/4/2009
        /// <summary>
        /// Simply saves the reference to the HeadQuarters <see cref="BuildingScene"/>, 
        /// for the given <see cref="Player"/>.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when given <paramref name="toPlace"/> is not a castable <see cref="BuildingScene"/> item.</exception>
        /// <param name="toPlace"><see cref="SceneItemWithPick"/> to place</param>
        private void SaveReferenceToPlayersHQ(SceneItemWithPick toPlace)
        {
            // 8/12/2009 - Cache.
            var buildingToPlace = (BuildingScene)toPlace;
            if (buildingToPlace == null)
                throw new ArgumentException(@"SceneItem given MUST be a BuildingScene type item.","toPlace");
           
            switch (PlayerNumber)
            {
                case 0:
                    _hqBuilding1 = buildingToPlace;
                    break;
                case 1:
                    _hqBuilding2 = buildingToPlace;
                    break;
            }
        }


        // 8/3/2009 
        /// <summary>
        /// During games, this will do a 'MoveItemOut' check, ultimately, 
        /// starting the Move command for the given <see cref="SceneItemWithPick"/>.
        /// </summary>
        /// <param name="player">this instance of <see cref="Player"/></param>
        /// <param name="e"><see cref="ItemCreatedArgs"/> instance</param>
        /// <param name="itemToCheck"><see cref="SceneItemWithPick"/> instance to check</param>
        private static void DoMoveItemOutCheck(Player player, ItemCreatedArgs e, SceneItemWithPick itemToCheck)
        {
            // is Network Game?
            if (player.NetworkSession == null)
            {
                // ***
                // SP Version... 
                // ***

                if (e.BuildingProducer != null && e.BuildingProducer.ShapeItem != null)
                    e.BuildingProducer.ShapeItem.MoveItemOut(itemToCheck);

                return;
            }

            // ***
            // MP version...
            // ***

            // 8/5/2009: Updated to use the Dictionary, instead of iterating through the entire array.
           
            SceneItemWithPick sceneItemWithPick;
            if (!player.SelectableItemsDict.TryGetValue(e.BuildingProducerNetworkItemNumber, out sceneItemWithPick)) return;

            var buildingSceneItem = sceneItemWithPick as BuildingScene;

            if (buildingSceneItem == null)
            {
                System.Diagnostics.Debug.WriteLine("Method Error: Not BuildingScene type in 'DoWarFactoryDoorCheck'");
                return;
            }

            var buildingShapeItem = buildingSceneItem.ShapeItem;

            if (buildingShapeItem != null)
                buildingShapeItem.MoveItemOut(itemToCheck);
        }  


        #endregion

        // 10/19/2009
        /// <summary>
        /// Helper method, which issues a Move order to a single unit, using the 
        /// given <paramref name="goalPosition"/> (given in PathNode scale), which is translated by the GoalNodeTranformations by
        /// the given iteration <paramref name="loopNumber"/>.
        /// </summary>
        /// <remarks>If this method is a direct call, then pass in 0 for the <paramref name="loopNumber"/>.</remarks>
        /// <param name="sceneItemWithPick"><see cref="SceneItemWithPick"/> to move</param>
        /// <param name="goalPosition">The goal location to move the unit to (given in PathNode scale)</param>
        /// <param name="isAttackMoveOrder">Was an AttackMove order given?</param>
        /// <param name="loopNumber">Translates the given Goal position for multiple units</param>
        /// <param name="shouldSkipAStar">Should it skip the A* pathfinding call? (For client MP games, it should)</param>
        /// <returns>Returns the <paramref name="goalPosition"/> transformed by the <see cref="TerrainShape.GoalNodeTransformations"/>.</returns>
        public static Vector3 UnitMoveOrder(SceneItemWithPick sceneItemWithPick, ref Vector3 goalPosition, 
                                            bool isAttackMoveOrder, int loopNumber, bool shouldSkipAStar)
        {
            // Cache
            const int pathNodeStride = TemporalWars3DEngine._pathNodeStride;
            const int scale = TerrainData.cScale;
            var goalNodeTransformations = TerrainShape.GoalNodeTransformations;
            var mapWidth = TerrainData.MapWidth;
            var mapHeight = TerrainData.MapHeight;

            if (sceneItemWithPick == null)
                return Vector3.Zero;

            // 12/30/2008 - Skip items not moveable!
            if (!sceneItemWithPick.ItemMoveable)
                return Vector3.Zero;

            // Only set for items NOT in a "PathFinding" state.
            if (sceneItemWithPick.ItemState == ItemStates.PathFindingReady) return Vector3.Zero;

            // 1st - Set scene SceneItemOwner's Final goalPosition using PickedRay with PathNode Scale,
            // and Transform the Position depending on what group number it is, using the 
            // goalNodeTransformations Array.   
            // 12/24/2008: Updated to *2 the 'GoalNodeTrans', so the units are not right next to eachother at there destination!
            var goalNodeTransformation = goalNodeTransformations[loopNumber]; // 8/16/2009

            // 10/19/2009 - Convert goal position into pathNode scale.
            var goalPositionScaled = new Vector3
                                         {
                                             X = (int)(goalPosition.X / pathNodeStride),
                                             Y = goalPosition.Y,
                                             Z = (int)(goalPosition.Z / pathNodeStride)
                                         };

            var goalPosTrans = new Vector3
                                   {
                                       X = (goalPositionScaled.X + goalNodeTransformation.X * 2) * pathNodeStride,
                                       Y = goalPositionScaled.Y,
                                       Z = (goalPositionScaled.Z + goalNodeTransformation.Y * 2) * pathNodeStride
                                   };

            // 2/12/2009 - Verify Position calculated is actually on the map!
            if (!TerrainData.IsOnHeightmap(ref goalPosTrans))
            {
                // Keep object from going off the edge of the map
                if ((goalPosTrans.X / scale) > mapWidth)
                {
                    // Then put back on map, 10 spaces from edge
                    goalPosTrans.X = (mapWidth * 10) - (10 * pathNodeStride);
                }
                else if ((goalPosTrans.X / scale) < 0)
                {
                    // Then put back on map, 10 spaces from edge
                    goalPosTrans.X = (10 * pathNodeStride);
                }

                // Keep object from going off the edge of the map
                if ((goalPosTrans.Z / scale) > mapHeight)
                {
                    // Then put back on map, 10 spaces from edge
                    goalPosTrans.Z = (mapHeight * 10) - (10 * pathNodeStride);
                }
                else if ((goalPosTrans.Z / scale) < 0)
                {
                    // Then put back on map, 10 spaces from edge
                    goalPosTrans.Z = (10 * pathNodeStride);
                }

            }

            // 10/19/2009 - Set if AttackMoveOrder given.
            sceneItemWithPick.AttackMoveOrderIssued = isAttackMoveOrder;
            sceneItemWithPick.AttackMoveGoalPosition = goalPosTrans; 

            // 7/3/2008
            // Make sure to turn off 'attackOn', otherwise unit will not move if it was just
            // in attacking mode.
            sceneItemWithPick.AttackOn = false;

            // 6/2/2009 - Set AIOrderIssued state to 'None'.
            sceneItemWithPick.AIOrderIssued = AIOrderType.None;

            // ***********************************************************
            // The A* Call is removed from Client side, since Server will
            // perform this and return results to clients.
            // ***********************************************************
            if (!shouldSkipAStar)
            {
                // 12/11/2008: Updated to use PathTo Queue.
                // 2nd - Call FindPath to kick off A* algorithm
                sceneItemWithPick.UseSmoothingOnPath = false;
                sceneItemWithPick.AStarItemI.AddWayPointGoalNode(ref goalPosTrans);
            }

            return goalPosTrans;
        }

        // 12/1/2008
        // 1/15/2009: Updated to be a Static method, which should optimize memory.
        /// <summary>
        /// Issues the Units Move Order, to all units in the <see cref="_itemsSelected"/> collection, using the
        /// given <paramref name="goalPosition"/> and translating the <paramref name="goalPosition"/> per unit when groups 
        /// are moved alltogether.
        /// </summary>
        /// <remarks>
        /// This should be the only method called for either SP/MP games, since this 
        /// method will redirect to the proper version of UnitsMoveOrder.
        /// </remarks>
        /// <param name="playerNumber">Player number</param>
        /// <param name="goalPosition">The goal location to move the unit(s) to.</param>
        /// <param name="isAttackMoveOrder">Was an AttackMove order given?</param>
        public static void UnitsMoveOrder(int playerNumber, ref Vector3 goalPosition, bool isAttackMoveOrder)
        {
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return;

            if (player == null) return;

            // Call proper version depending on SP or MP game.
            if (!IsNetworkGame)
                UnitsMoveOrder_SP(player, ref goalPosition, isAttackMoveOrder);
            else
                UnitsMoveOrder_MP(player, ref goalPosition, isAttackMoveOrder);
        }

        // 6/14/2010 - Updated to pass in 'Player' instance param.
        // 8/17/2009 - Optimized by caching values.
        // 5/30/2008
        // 12/11/2008: Updated to use PathTo Queue.
        // 1/15/2009: Updated to be a Static method, which should optimize memory.
        /// <summary>
        /// Issues the Units Move Order, to all units in the <see cref="_itemsSelected"/> collection, using the
        /// given <paramref name="goalPosition"/> (given in PathNode scale) and translating the <paramref name="goalPosition"/> per unit when groups 
        /// are moved alltogether.
        /// </summary>
        /// <param name="player"><see cref="Player"/> instance</param>
        /// <param name="goalPosition">The goal location to move the unit(s) to.  (given in PathNode scale)</param>
        /// <param name="isAttackMoveOrder">Was an AttackMove order given?</param>
// ReSharper disable InconsistentNaming
        private static void UnitsMoveOrder_SP(Player player, ref Vector3 goalPosition, bool isAttackMoveOrder)
// ReSharper restore InconsistentNaming
        {
            var itemsSelected = player._itemsSelected; // 6/14/2010
            // 2/11/2009 - Make sure doesn't go over Max of 49.           
            var itemCount = itemsSelected.Count > 49 ? 49 : itemsSelected.Count;

            // Iterate through only Selected Items using the ItemsSelected Array.
            for (var i = 0; i < itemCount; i++)
            {
                // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                SceneItemWithPick selected;
                if (!GetItemsSelectedByIndex(player, i, out selected))
                    break;

                // 11/11/2009 - Skip items not moveable!
                if (!selected.ItemMoveable) continue;

                // 10/19/2009 - Refactored into new method.
                UnitMoveOrder(selected, ref goalPosition, isAttackMoveOrder, i, false);

            } // End For Loop
        }

        // 10/19/2009 - Updated to include 3rd param 'IsAttackMoveOrder'.
        // 8/17/2009 - Optimized by caching values.
        // 9/8/2008 - MultiPlayer Version of UnitsMoveOrder, called by Server to start the MoveOrder Request
        //            for Client.
        // 12/11/2008: Updated to use PathTo Queue.
        // 1/1/2009: Updated to use the SelectableItems Dictionary, which finds items at almost O(1).
        ///<summary>
        /// MultiPlayer version of UnitsMoveOrder, called by server to start the MoveOrder Request
        /// for Client.
        ///</summary>
        ///<param name="networkItemNumber">Unique <see cref="SceneItemWithPick.NetworkItemNumber"/></param>
        ///<param name="goalPosition"><see cref="Vector3"/> as goal position</param>
        ///<param name="isAttackMoveOrder">Is AttackMove order?</param>
        public void UnitsMoveOrder(ref int networkItemNumber, ref Vector3 goalPosition, bool isAttackMoveOrder)
        {
            // 1/2/2009 - Update to use SelectableItems Dictionary
            // find SceneItemOwner to move using NetworkItemNumber
            if (!SelectableItemsDict.ContainsKey(networkItemNumber)) return;

            // 8/17/2009 - Cache
            var sceneItemWithPick = SelectableItemsDict[networkItemNumber];

            // 1/2/2009 - Make sure not giving order for an SceneItemOwner which just died!
            if (!sceneItemWithPick.IsAlive) return;

            // 10/19/2009 - Set if AttackMoveOrder given.
            sceneItemWithPick.AttackMoveOrderIssued = isAttackMoveOrder;
            sceneItemWithPick.AttackMoveGoalPosition = goalPosition; 

            // Make sure to turn off 'attackOn', otherwise unit will not move if it was just
            // in attacking mode.
            sceneItemWithPick.AttackOn = false;

            // 6/2/2009 - Set AIOrderIssued state to 'None'.
            sceneItemWithPick.AIOrderIssued = AIOrderType.None;

            // 2nd - Call FindPath to kick off A* algorithm
            sceneItemWithPick.UseSmoothingOnPath = false;
            sceneItemWithPick.AStarItemI.AddWayPointGoalNode(ref goalPosition);
        }

        // 6/14/2010 - Updated to pass in 'Player' instance param.
        // 8/17/2009 - Optimized by caching values.
        //  9/3/2008 - MultiPlayer Version of UnitsMoveOrder.
        // 1/15/2009: Updated to be a Static method, which should optimize memory.
        private static void UnitsMoveOrder_MP(Player player, ref Vector3 goalPosition, bool isAttackMoveOrder)
        {
            var itemsSelected = player._itemsSelected; // 6/14/2010
            var networkSession = player.NetworkSession; // 6/14/2010
            var playerNumber = player.PlayerNumber; // 6/14/2010

            // Is this Host?
            if (networkSession.IsHost)
            {
                // Then we can call the normal UnitsMoveOrder.
                UnitsMoveOrder_SP(player, ref goalPosition, isAttackMoveOrder);
            }
                // Else, this is Client; therefore, Server will Process A* AI
            else
            {
                // 2/11/2009 - Make sure doesn't go over Max of 49.                 
                var itemCount = itemsSelected.Count > 49 ? 49 : itemsSelected.Count;

                // Iterate through only Selected Items using the ItemsSelected Array.
                for (var i = 0; i < itemCount; i++)
                {
                    // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                    SceneItemWithPick selected;
                    if (!GetItemsSelectedByIndex(player, i, out selected))
                        break;

                    // 11/11/2009 - Skip items not moveable!
                    if (!selected.ItemMoveable) continue;

                    // 10/19/2009 - Refactored into new method.
                    var goalPosTrans = UnitMoveOrder(selected, ref goalPosition, isAttackMoveOrder, i, true);
                   
                    // Create Move RTSCommand for Server and Add to Queue                        
                    RTSCommMoveSceneItem commMoveItem;
                    PoolManager.GetNode(out commMoveItem);

                    commMoveItem.Clear();
                    commMoveItem.NetworkCommand = NetworkCommands.ReqUnitMoveOrder;
                    commMoveItem.Position = goalPosTrans;
                    commMoveItem.Velocity = selected.Velocity; // 11/12/2008
                    commMoveItem.SmoothHeading = (selected.AStarItemI != null) ? selected.AStarItemI.SmoothHeading : Vector3.Zero; // 12/16/2008; 11/5/2009: Fix.                       
                    commMoveItem.PlayerNumber = playerNumber;
                    commMoveItem.NetworkItemNumber = selected.NetworkItemNumber;
                    commMoveItem.IsAttackMoveOrder = isAttackMoveOrder; // 10/19/2009

                    NetworkGameComponent.AddCommandsForServer(commMoveItem);
                } // End For Loop
            } // End If NetworkSession.IsHost
        }

        // 8/17/2009 - Optimized by caching values.
        // 11/24/2008 (MP)        
        /// <summary>
        /// Called by <see cref="NetworkGameComponent"/> to search the <see cref="_selectableItems"/> for the 'attacker', and
        /// then set it to start an attack on 'attackie' <see cref="SceneItem"/>.
        /// </summary>
        /// <param name="player">this instance of <see cref="Player"/></param>
        /// <param name="startAttackItem"><see cref="RTSCommStartAttackSceneItem"/> instance</param>
        public static void UnitsStartAttackItemOrder(Player player, RTSCommStartAttackSceneItem startAttackItem)
        {
            // Set Attack Order           

            // 12/17/2008 - Update to use SelectableItems Dictionary
            // Return if no Attacker found
            if (!player.SelectableItemsDict.ContainsKey(startAttackItem.SceneItemAttackerNetworkNumber)) return;
            
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player attackiePlayerNumber;
            if (!TemporalWars3DEngine.GetPlayer(startAttackItem.SceneItemAttackiePlayerNumber, out attackiePlayerNumber))
                return;

            if (attackiePlayerNumber == null) return;

            // Now we need to locate the Attackie SceneItem using its networkItemNumber
            if (!attackiePlayerNumber.SelectableItemsDict.ContainsKey(startAttackItem.SceneItemAttackieNetworkNumber)) return;

            // 8/17/2009 - Cache
            var sceneItemWithPick = player.SelectableItemsDict[startAttackItem.SceneItemAttackerNetworkNumber];

            // Make sure not giving order for an SceneItemOwner which just died!
            if (!sceneItemWithPick.IsAlive) return;

            // Set SceneItem into Attacker SceneItemOwner
            sceneItemWithPick.AttackSceneItem = attackiePlayerNumber.SelectableItemsDict[startAttackItem.SceneItemAttackieNetworkNumber];

            // 2/27/2009 - If DefenseItem, then also save in its behavior turret Queue.
            var itemWithPick = sceneItemWithPick as DefenseScene;
            if (itemWithPick != null)
                itemWithPick.StoreItemToAttackInBehaviorQueue(attackiePlayerNumber.SelectableItemsDict[startAttackItem.SceneItemAttackieNetworkNumber]);

            // 7/31/2009 - Set AIOrderType enum, and call RequestAttack in FSMAIControl!
            sceneItemWithPick.AIOrderIssued = startAttackItem.AIOrderIssued;
            // 7/31/2009 - Always call AttackOrder directly, since client requests to server, will not have a FSMAIControl instance,
            //             AND client processes from server also will not have an FSMAIControl!
            sceneItemWithPick.AttackOrder();
        }

        // 7/3/2008; // 6/14/2010: Updated to pass in 'Player' instance param.
        // 1/15/2009: Updated to be a Static method, which should optimize memory.
        /// <summary>
        /// Issues the Attack ground order, to all units in the <see cref="_itemsSelected"/> collection, using the
        /// given <paramref name="goalPosition"/> as the place to fire at.
        /// </summary>
        /// <param name="player"><see cref="Player"/> instance</param>
        /// <param name="goalPosition">The goal location to Attack</param>
        public static void UnitsAttackGroundOrder(Player player, ref Vector3 goalPosition)
        {
            var itemsSelected = player._itemsSelected; // 6/14/2010

            // 8/17/2009 - Cache
            const int pathNodeStride = TemporalWars3DEngine._pathNodeStride;
            var count = itemsSelected.Count;

            // Iterate through only Selected Items using the ItemsSelected Array.
            for (var i = 0; i < count; i++)
            {
                // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                SceneItemWithPick itemSelected;
                if (!GetItemsSelectedByIndex(player, i, out itemSelected))
                    break;

                // 11/13/2008 - Skip BuildingScenes
                if (itemSelected is BuildingScene) continue;

                goalPosition.X *= pathNodeStride;
                goalPosition.Z *= pathNodeStride;

                // Call 'AttackGroundOrder' for given SceneItemOwner.
                itemSelected.AttackGroundOrder();    
    
                // Turn off "PathFinding" state, since attacking.
                if (itemSelected.AStarItemI != null)
                {
                    itemSelected.ItemState = ItemStates.Resting;
                }
               

            } // End For Loop
        }

        static bool _pickSelectionInProgress;

        // 6/14/2010 - Updated to pass in 'Player' instance param.
        // 10/11/2008: Updated to 'update' Properties Tools window, if open. Also added
        //             GamePad Picking check for XBOX.
        // 10/13/2008: Removed the need to call 'PropertiesTools' form Link method.  The
        //             'PropetiesTools' form now takes care of this on its own within 'Tick' event.
        // 1/15/2009: Updated to be a Static method, which should optimize memory.
        /// <summary>
        /// Iterates through all <see cref="_selectableItems"/> to check if Picked by cursor.
        /// If <see cref="SceneItemWithPick"/> is picked and user selects with mouse or gamepad, it is automatically 
        /// marked as "PickSelected".
        /// </summary>
        /// <param name="player"><see cref="Player"/> instance</param>
        /// <param name="inputState"><see cref="InputState"/> instance</param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        internal static void SceneItemsPickCheck(Player player, InputState inputState, GameTime gameTime)
        {
            // 5/1/2009 - Only check if Cursor is not inside some IFD tile or Minimap.
            var miniMapContainsCursor = _miniMap != null && _miniMap.MiniMapContainsCursor; // 1/2/2010
            if (IFDTileManager.CursorInSomeIFDTile || miniMapContainsCursor)
                return;

            // 2/2/2010 - Updated to use the new refactored method.
            int itemIndex;
            if (GetClosestPickedSceneItem(player, out itemIndex))
            {
                // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                SceneItemWithPick selectableItem;
                if (!GetSelectableItemByIndex(player, itemIndex, out selectableItem))
                    return;

                if (selectableItem != null)
                {
                    selectableItem.PickHovered = true; // PickHovered check

                    // 2/23/2011 - Skip check for enemy players
                    if (player.PlayerNumber != TemporalWars3DEngine.SThisPlayer)
                        return;

                    // SelectAction check
                    PickWithSelectAction(player, selectableItem, inputState);
                }
            }

            // 7/12/2009
            if (inputState.SelectActionFinshed && _pickSelectionInProgress)
            {
                _pickSelectionInProgress = false; // 7/12/2009     
            }
           
            // 2/5/2009 - Same ItemType PickCheck.
            SameItemsPickCheck(player, inputState);
        }

        // 2/2/2010; // 6/14/2010 - Updated to pass in 'Player' instance param.
        /// <summary>
        /// Helper method, to set the <see cref="SceneItemWithPick"/> which are 'PickSelected' while the 'SelectAction' occurs, to TRUE.
        /// </summary>
        /// <param name="player"><see cref="Player"/> instance</param>
        /// <param name="selectableItem"><see cref="SceneItemWithPick"/> as selectable item</param>
        /// <param name="inputState"><see cref="InputState"/> instance</param>
        private static void PickWithSelectAction(Player player, SceneItemWithPick selectableItem, InputState inputState)
        {
            if (!selectableItem.PickHovered || !inputState.SelectAction) return;

            var itemsSelected = player._itemsSelected; // 6/14/2010

            // 2/2/2010
            var shapeWithPick = selectableItem.ShapeItem as ShapeWithPick;
            if (shapeWithPick == null) return;

            // 11/7/2009: Updated to be here, rather than inside the 'If-PickedSelected' below.
            // 10/9/2009: Updated to use new overload version of 'SetAsCurrentGroupToDisplay'.
            // 5/3/2009: FXCop - Cast to SceneItemOwner, then check if null, to avoid multiple casts!
            // 4/30/2009 - Check if BuildingScene
            var buildingSceneItem = selectableItem as BuildingScene;
            if (buildingSceneItem != null)
                // then display the IFDTiles associated to this building.                            
                IFDTileManager.SetAsCurrentGroupToDisplay(buildingSceneItem);

            if (selectableItem.PickSelected) return;

            selectableItem.PickSelected = true;
            _pickSelectionInProgress = true; // 7/12/2009     

            // 11/6/2009: Updated to also skip if DoubleClick true.
            // 6/16/2009 - If not 'ShiftSelected', then do normal DeSelect ALL prior units.
            if (!inputState.ShiftSelected)
                DeSelectAll(player);
#if DEBUG
            // 8/12/2009
            var instancedItem = (shapeWithPick as IInstancedItem);

            if (!instancedItem.IsPickedInEditMode)
                instancedItem.IsPickedInEditMode = true;
#endif

            // Add SceneItemOwner to Selected Array
            itemsSelected.Add(selectableItem);
            
        }

        // 6/14/2010 - Updated to pass in 'Player' instance param.
        // 2/2/2010 - Updated to use the new refactored method.
        // 2/5/2009
        /// <summary>
        /// Selects all items in the <see cref="_selectableItems"/> collection, which has the same <see cref="ItemType"/> as the
        /// current selected <see cref="SceneItemWithPick"/>.
        /// </summary>
        /// <param name="player"><see cref="Player"/> instance</param>
        /// <param name="inputState"><see cref="InputState"/> instance</param>
// ReSharper disable UnusedMethodReturnValue.Local
        private static bool SameItemsPickCheck(Player player, InputState inputState)
// ReSharper restore UnusedMethodReturnValue.Local
        {
            // If SameItemsPick, then multi-select current SceneItemOwner.
            if (!inputState.DoubleClick) return false;

            var itemsSelected = player._itemsSelected; // 6/14/2010
            var selectableItems = player._selectableItems; // 6/14/2010

            // 11/6/2009 - If 'ItemsSelected' is empty, then check for some item being hovered as selection.
            SceneItemWithPick selectableItem = null;
            if (itemsSelected.Count == 0)
            {
                // 2/2/2010 - Updated to use the new refactored method.
                int itemIndex;
                if (GetClosestPickedSceneItem(player, out itemIndex))
                {
                    //var itemWithPick = selectableItems[itemIndex];
                    // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                    SceneItemWithPick itemWithPick;
                    if (!GetSelectableItemByIndex(player, itemIndex, out itemWithPick))
                        return false;

                    if (itemWithPick != null)
                    {
                        // found item.
                        selectableItem = itemWithPick;
                        selectableItem.PickSelected = true;
                        itemsSelected.Add(itemWithPick);
                    }
                }
                
            }
            else
                selectableItem = itemsSelected[0];  

            // 8/12/2009 - Skip check, if null
            if (selectableItem == null)
                return false;

            // 11/6/2009 - Skip if building type.
            if (selectableItem is BuildingScene)
                return false;

            // Itereate through the 'SelectableItems' array.
            var selectableItemsCount = selectableItems.Count; // 8/12/2009
            for (var i = 0; i < selectableItemsCount; i++)
            {
                // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                SceneItemWithPick selectableItemToCompare;
                if (!GetSelectableItemByIndex(player, i, out selectableItemToCompare))
                    break;

                // 5/25/2009
                if (selectableItemToCompare == null) continue;

                // 2/24/2009 - Only Check Alive objects
                if (!selectableItemToCompare.IsAlive) continue;

                // 2/24/2009 - Skip items not moveable!
                if (!selectableItemToCompare.ItemMoveable) continue;
                
                // 11/6/2009 - Updated to check 'SceneItemNumber' to make sure adding itself!
                // Does current selectableItem have same ItemType of Double-click SceneItemOwner?
                if (selectableItemToCompare.PlayableItemAtts.ItemType != selectableItem.PlayableItemAtts.ItemType
                    || selectableItem.SceneItemNumber == selectableItemToCompare.SceneItemNumber) continue;

                // yes, so set as picked.
                selectableItemToCompare.PickSelected = true;
                // Add SceneItemOwner to Selected Array
                itemsSelected.Add(selectableItemToCompare);
            } // End For Loop    

            return itemsSelected.Count > 1;
        }

        // 4/28/2009; // 6/14/2010 - Updated to pass in 'Player' instance param.
        /// <summary>
        /// Selects all <see cref="SceneItemWithPick"/> which are withn the <see cref="Camera"/> frustrum.
        /// </summary>
        /// <param name="player"><see cref="Player"/> instance</param>
        internal static void SelectLocalUnits(Player player)
        {
            // 2/23/2011 - Skip check for enemy players
            if (player.PlayerNumber != TemporalWars3DEngine.SThisPlayer)
                return;

            var itemsSelected = player._itemsSelected; // 6/14/2010
            var selectableItems = player._selectableItems; // 6/14/2010

            // 1st - Deselect all units
            DeSelectAll(player);

            // Itereate through the 'SelectableItems' array.
            var selectableItemsCount = selectableItems.Count; // 8/12/2009
            for (var i = 0; i < selectableItemsCount; i++)
            {
                // 8/12/2009 - Cache
                var selectableItem = selectableItems[i];

                // 5/25/2009
                if (selectableItem == null) continue;

                // Only Check Alive objects
                if (!selectableItem.IsAlive) continue;

                // Skip items not moveable!
                if (!selectableItem.ItemMoveable) continue;

                // Is SceneItemOwner within camera view?
                if (!InstancedItem.IsInCameraView(ref selectableItem.ShapeItem.InstancedItemData)) continue;

                // yes, so set as picked.
                selectableItem.PickSelected = true;
                // Add SceneItemOwner to Selected Array
                itemsSelected.Add(selectableItem);
            } // End For Loop
        }

        // 4/28/2009; // 6/14/2010 - Updated to pass in 'Player' instance param.
        /// <summary>
        /// Selects all <see cref="SceneItemWithPick"/> on the entire map.
        /// </summary>
        /// <param name="player"><see cref="Player"/> instance</param>
        internal static void SelectAllUnits(Player player)
        {
            // 2/23/2011 - Skip check for enemy players
            if (player.PlayerNumber != TemporalWars3DEngine.SThisPlayer)
                return;

            var itemsSelected = player._itemsSelected; // 6/14/2010
            var selectableItems = player._selectableItems; // 6/14/2010

            // 1st - Deselect all units
            DeSelectAll(player);

            // Itereate through the 'SelectableItems' array.
            var selectableItemsCount = selectableItems.Count; // 8/12/2009
            for (var i = 0; i < selectableItemsCount; i++)
            {
                // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                SceneItemWithPick selectableItem;
                if (!GetSelectableItemByIndex(player, i, out selectableItem))
                    break;

                // 5/25/2009
                if (selectableItem == null) continue;

                // Only Check Alive objects
                if (!selectableItem.IsAlive) continue;

                // Skip items not moveable!
                if (!selectableItem.ItemMoveable) continue;
                
                // yes, so set as picked.
                selectableItem.PickSelected = true;
                // Add SceneItemOwner to Selected Array
                itemsSelected.Add(selectableItem);                

            } // End For Loop
        }

        // 6/14/2010 - Updated to pass in 'Player' instance param.
        // 2/2/2010 - Updated to use the new refactored method.
        // 12/10/2008
        // 1/15/2009: Updated to be a Static method, which should optimize memory.
        /// <summary>
        /// Iterates through all selectable <see cref="SceneItemWithPick"/> and checks if PickHovered by cursor.
        /// This is only called during MP games, and will be only called for a Player's other
        /// 'Player' state, and not their own.   
        /// </summary>  
        /// <remarks>This allows a <see cref="Player"/> to see the Status of their enemy units!</remarks>
        /// <param name="player">this instance of <see cref="Player"/></param>
        private static void PlayerHoverPickCheck(Player player)
        {
            // 5/1/2009 - Only check if Cursor is not inside some IFD tile, or in Minimap.
            var miniMapContainsCursor = _miniMap != null && _miniMap.MiniMapContainsCursor; // 1/2/2010
            if (IFDTileManager.CursorInSomeIFDTile || miniMapContainsCursor)
                return;

            // 2/2/2010 - Updated to use the new refactored method.
            int itemIndex;
            if (!GetClosestPickedSceneItem(player, out itemIndex)) return;

            //
            // 6/11/2010  - Added important missing code which sets the PickHovered!
            //

            // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
            SceneItemWithPick selectableItem;
            if (!GetSelectableItemByIndex(player, itemIndex, out selectableItem))
                return;

            if (selectableItem != null)
                selectableItem.PickHovered = true; // PickHovered check
        }

        // 8/3/2009
        /// <summary>
        /// Returns the proper HeadQuarters <see cref="BuildingScene"/> for the given <paramref name="playerNumber"/>.
        /// </summary>
        /// <param name="playerNumber"><paramref name="playerNumber"/> to retrieve</param>
        /// <param name="buildingHQ">(OUT) <see cref="BuildingScene"/> HQ</param>
        public static void GetPlayersHeadQuarters(int playerNumber, out BuildingScene buildingHQ)
        {
            buildingHQ = null;

            switch (playerNumber)
            {
                case 0:
                    buildingHQ = _hqBuilding1;
                    break;
                case 1:
                    buildingHQ = _hqBuilding2;
                    break;
            }
        }

        #region Attacking Methods

        // 7/4/2008; 6/14/2010: Updated to pass 'Player' as param.
        // 11/13/2008: Removed the (ShapeWithPick) type check, since all selectable items 
        //             will be attackable!
        // 1/15/2009: Updated to be a Static method, which should optimize memory.
        /// <summary>
        /// Iterates through all <see cref="SceneItemWithPick"/> in scene and checks for <see cref="InputState.ItemAttack"/>;
        /// when TRUE, the given <see cref="SceneItemWithPick"/> is set as 'Attackie' for all other selectable items.
        /// </summary>
        /// <param name="player"><see cref="Player"/> instance</param>
        /// <param name="inputState"><see cref="InputState"/> instance</param>
        internal static bool SceneItemsAttackCheck(Player player, InputState inputState)
        {
            // 5/1/2009 - Only check if Cursor is not inside some IFD tile, or in Minimap.
            var miniMapContainsCursor = _miniMap != null && _miniMap.MiniMapContainsCursor; // 1/2/2010
            if (IFDTileManager.CursorInSomeIFDTile || miniMapContainsCursor)
                return false;

            var playerNumber = player.PlayerNumber; // 6/14/2010

            // 8/12/2009
            const int maxPlayers = TemporalWars3DEngine._maxAllowablePlayers;

            // 1st - go through all SelectableItems contain in Enemy Player, to see which SceneItemOwner
            //       we are attacking.
            var itemToAttackIndex = -1;
            var enemyPlayerNumber = -1;

            // Loop through all 'Players', skipping ourselves.
            for (var i = 0; i < maxPlayers; i++)
            {
                // 6/15/2010 - Updated to use new GetPlayer method.
                Player playerToCheck;
                if (!TemporalWars3DEngine.GetPlayer(i, out playerToCheck))
                    break;

                if (playerToCheck == null) continue;

                // If not ourselves, then do check.
                if (playerToCheck.PlayerNumber == playerNumber) continue;

                if (!inputState.ItemAttack) continue;

                if (GetClosestPickedSceneItem(playerToCheck, out itemToAttackIndex))
                    enemyPlayerNumber = i;
            }

            // 1/26/2009: Add check for 'PlayerIndex' to avoid crashes.
            // 2nd - if enemy SceneItemOwner found, then go through all ItemsSelected, and set AttackSceneItem to the current enemy SceneItemOwner found.
            if (itemToAttackIndex != -1 && enemyPlayerNumber != -1)
            {
                SetItemsSelectedToAttackSelectedItem(player, itemToAttackIndex, enemyPlayerNumber);

                return true;
            } 

            // **
            // 3rd - If enemy SceneItemOwner was not found, then let's check if force attacking our own units:
            // **
            // if 'Ctrl' key pressed, or 'GamePad X' key, then check Player's own SelectableItems, to
            // see if attacking one of our own units.
            if (inputState.ForceItemAttack)
                GetClosestPickedSceneItem(player, out itemToAttackIndex);
            
            // 4th - if enemy SceneItemOwner found, then go through all ItemsSelected, and set AttackSceneItem to the current enemy SceneItemOwner found.
            if (itemToAttackIndex != -1)
            {
                SetItemsSelectedToAttackSelectedItem(player, itemToAttackIndex, enemyPlayerNumber);

                return true;
            }

            // Else, nothing found to attack
            return false;

        }

        // 1/16/2011 - Updated to move the Retrieval of the EnemyUnit to be outside of the MP section,
        //             since the SP also requires this information.
        // 11/27/2008; 6/14/2010: Updated to pass 'Player' as param.
        // 12/19/2008 - Skip Buildings, since they should not attack other items.
        // 1/15/2009: Updated to be a Static method, which should optimize memory.
        /// <summary>
        /// Iterates through this Player's <see cref="_itemsSelected"/> collection and sets all the
        /// <see cref="SceneItemWithPick"/> to attack a given selected <see cref="SceneItemWithPick"/>.
        /// </summary>
        /// <param name="player"><see cref="Player"/> instance</param>
        /// <param name="itemToAttackIndex">Index into collection of <see cref="SceneItemWithPick"/> to attack</param>
        /// <param name="enemyPlayerNumber"><see cref="Player"/> enemy number</param>
        private static void SetItemsSelectedToAttackSelectedItem(Player player, int itemToAttackIndex, int enemyPlayerNumber)
        {
            var itemsSelected = player._itemsSelected; // 6/14/2010
            var networkSession = player.NetworkSession; // 6/14/2010
            var playerNumber = player.PlayerNumber; // 6/14/2010

            // 1/29/2009 - If PlayerIndex == -1, then most likely a force attack, so set to this player's number.
            if (enemyPlayerNumber == -1)
                enemyPlayerNumber = playerNumber;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player enemyPlayer;
            if (!TemporalWars3DEngine.GetPlayer(enemyPlayerNumber, out enemyPlayer))
                return;

            if (enemyPlayer == null) return;

            // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
            SceneItemWithPick enemyPlayerSelectableItem;
            if (!GetSelectableItemByIndex(enemyPlayer, itemToAttackIndex, out enemyPlayerSelectableItem))
                return;

            var itemSelectedCount = itemsSelected.Count; // 8/12/2009
            for (var i = 0; i < itemSelectedCount; i++)
            {
                // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                SceneItemWithPick selectedItem;
                if (!GetItemsSelectedByIndex(player, i, out selectedItem))
                    break;

                if (selectedItem == null) continue;

                // 12/19/2008 - Skip Buildings, since they should not attack other items.
                if (selectedItem is BuildingScene) continue;

                // 11/20/2008
                // is Network Game?
                if (networkSession == null)
                {
                    // SP game, then just set attack info and start attack.
                    // Set Attackie SceneItemOwner into Attacker selected.
                    selectedItem.AttackSceneItem = enemyPlayerSelectableItem; // 1/16/2011 - selectableItems[itemToAttackIndex];
                    // Issue Attack Order                   
                    selectedItem.AIOrderIssued = AIOrderType.NonAIAttackOrderRequest;
                    selectedItem.AttackOrder();
                }
                else  // Yes, Network Game
                {
                    // Is Host?
                    if (networkSession.IsHost)
                    {
                        // Create StartAttack RTSCommand for Client and Add to Queue                        
                        RTSCommStartAttackSceneItem startAttackCommand;
                        PoolManager.GetNode(out startAttackCommand);

                        startAttackCommand.Clear();
                        startAttackCommand.NetworkCommand = NetworkCommands.StartAttackSceneItem;
                        startAttackCommand.SceneItemAttackerNetworkNumber = selectedItem.NetworkItemNumber;
                        startAttackCommand.SceneItemAttackerPlayerNumber = playerNumber;
                        startAttackCommand.SceneItemAttackieNetworkNumber = enemyPlayerSelectableItem.NetworkItemNumber;
                        startAttackCommand.SceneItemAttackiePlayerNumber = enemyPlayerSelectableItem.PlayerNumber;
                        startAttackCommand.AIOrderIssued = AIOrderType.NonAIAttackOrderRequest; // 6/3/2009

                        // Add to Queue to send to Client
                        NetworkGameComponent.AddCommandsForClientG(startAttackCommand); // 12/2/2008 - Updated to 'ReliableInOrder' queue.

                        // **
                        // Issue Attack Order for this Server side
                        // **
                        // Set Attackie SceneItemOwner into Attacker selected.
                        selectedItem.AttackSceneItem = enemyPlayerSelectableItem;
                        // Issue Attack Order                        
                        selectedItem.AIOrderIssued = AIOrderType.NonAIAttackOrderRequest;
                        selectedItem.AttackOrder();

                    }
                    else // Client
                    {
                        // Create StartAttackRequest RTSCommand for Server and Add to Queue                        
                        RTSCommStartAttackSceneItem reqStartAttackCommand;
                        PoolManager.GetNode(out reqStartAttackCommand);

                        reqStartAttackCommand.Clear();
                        reqStartAttackCommand.NetworkCommand = NetworkCommands.ReqStartAttackSceneItem;
                        reqStartAttackCommand.SceneItemAttackerNetworkNumber = selectedItem.NetworkItemNumber;
                        reqStartAttackCommand.SceneItemAttackerPlayerNumber = playerNumber;
                        reqStartAttackCommand.SceneItemAttackieNetworkNumber = enemyPlayerSelectableItem.NetworkItemNumber;
                        reqStartAttackCommand.SceneItemAttackiePlayerNumber = enemyPlayerSelectableItem.PlayerNumber;
                        reqStartAttackCommand.AIOrderIssued = AIOrderType.NonAIAttackOrderRequest; // 6/3/2009

                        // Add to Queue to send to Server
                        NetworkGameComponent.AddCommandsForServerG(reqStartAttackCommand); // 12/2/2008 - Updated to 'ReliableInOrder' queue.

                        // **
                        // Issue Attack Order for this Client side
                        // **
                        // Set Attackie SceneItemOwner into Attacker selected.
                        selectedItem.AttackSceneItem = enemyPlayerSelectableItem;
                        // Issue Attack Order                       
                        selectedItem.AIOrderIssued = AIOrderType.NonAIAttackOrderRequest;
                        selectedItem.AttackOrder();
                    }
                }

            } // End For Loop
        }

        // 2/25/2011
        /// <summary>
        /// Sets the <see cref="DefenseAIStance"/> for the given <paramref name="sceneItemName"/>.
        /// </summary>
        /// <param name="sceneItemName"></param>
        /// <param name="defenseAIStance"></param>
        public static void SetDefenseAIStance(string sceneItemName, DefenseAIStance defenseAIStance)
        {
            // 1st - retrieve 'Named' item from Dictionary
            SceneItem namedSceneItem;
            if (SceneItemsByName.TryGetValue(sceneItemName, out namedSceneItem))
            {
                // make sure item is a Selectable SceneItemWithPick.
                var selectableItem = (namedSceneItem as SceneItemWithPick);
                if (selectableItem != null)
                {
                    // set new defense stance.
                    selectableItem.DefenseAIStance = defenseAIStance;

                } // End if SceneITemWithPick

                throw new ArgumentException(@"Named sceneItem MUST be a SelectableItem, and NOT a scenaryItem!", "sceneItemName");
            }

            throw new ArgumentException(@"Given sceneItem Name is NOT VALID!", "sceneItemName");
        }

        // 10/12/2009
        /// <summary>
        /// Iterates the current Players <see cref="_selectableItems"/> collection, 
        /// killing all items by giving them a lethal amount of damage! (Scripting purposes)
        /// </summary>
        /// <param name="player">this instance of <see cref="Player"/></param>
        public static void KillAllPlayersSelectableItems(Player player)
        {
            // 2/15/2010 - Lock
            var selectableItems = player._selectableItems; // 5/21/2010 - CAche
            lock (selectableItems)
            {
                // 1st
                // iterate players 'SelectableItems', and deal a lethal amount of damage.
                var count = selectableItems.Count;
                for (var i = 0; i < count; i++)
                {
                    // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                    SceneItemWithPick selectableItem;
                    if (!GetSelectableItemByIndex(player, i, out selectableItem))
                        break;

                    // make sure not NULL
                    if (selectableItem == null) continue;

                    // get current health of item
                    var currentHealth = selectableItem.CurrentHealth;

                    // Reduce item's health by currentHealth + 100 to make sure it is dead!
                    selectableItem.ReduceHealth(currentHealth + 100, 0);
                } // End For
            } // End Lock
        }


        // 2/2/2010; // 6/14/2010: Updated to pass in 'Player' instance param.
        /// <summary>
        /// Iterates the given <see cref="_selectableItems"/> collection checking for 'Picked' items.  Only the 
        /// closest picked item's array index is return, determine by the intersection ray distance.
        /// </summary>
        /// <param name="player"><see cref="Player"/> instance</param>
        /// <param name="closestModelIndex">(OUT) index in array of closest match</param>
        /// <returns>True/False of finding any 'Picked' items.</returns>
        public static bool GetClosestPickedSceneItem(Player player, out int closestModelIndex)
        {
            var selectableItems = player._selectableItems; // 6/14/2010

            // Run the 'ParallelFor' threaded version
            closestModelIndex = _parallelfor.ParallelFor(selectableItems, 0, selectableItems.Count);
            return (closestModelIndex != -1);

        }

        // 6/15/2010
        ///<summary>
        /// Returns to caller a <see cref="ReadOnlyCollection{T}"/> for the <see cref="_selectableItems"/> internal
        /// collection.  To retrieve an item for changes, use the <see cref="Player.GetSelectableItemByIndex"/> method.
        ///</summary>
        ///<param name="player"><see cref="Player"/> instance</param>
        ///<param name="readOnlyCollection"><see cref="ReadOnlyCollection{T}"/> of <see cref="_selectableItems"/></param>
        public static void GetSelectableItems(Player player, out ReadOnlyCollection<SceneItemWithPick> readOnlyCollection)
        {
            readOnlyCollection = null;
            // check if player or collection null
            if (player == null) return;
            if (player._readOnlySelectableItems == null) return;

            // return ROC to caller.
            readOnlyCollection = player._readOnlySelectableItems;
        }

        // 6/15/2010
        ///<summary>
        /// Returns to caller a <see cref="ReadOnlyCollection{T}"/> for the <see cref="_itemsSelected"/> internal
        /// collection.  To retrieve an item for changes, use the <see cref="Player.GetItemsSelectedByIndex"/> method.
        ///</summary>
        ///<param name="player"><see cref="Player"/> instance</param>
        ///<param name="readOnlyCollection"><see cref="ReadOnlyCollection{T}"/> of <see cref="_itemsSelected"/></param>
        public static void GetItemsSelected(Player player, out ReadOnlyCollection<SceneItemWithPick> readOnlyCollection)
        {
            readOnlyCollection = null;
            // check if player or collection null
            if (player == null) return;
            if (player._readOnlyItemsSelected == null) return;

            // return ROC to caller.
            readOnlyCollection = player._readOnlyItemsSelected;

        }

        // 6/15/2010
        ///<summary>
        /// Returns the <see cref="SceneItemWithPick"/> from the Player's <see cref="_selectableItems"/> collection, using the given
        /// index value; return 'False' if index not valid.
        ///</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="index"/> is outside allowable range.</exception>
        ///<param name="player"><see cref="Player"/> instance</param>
        ///<param name="index">Collection index</param>
        ///<param name="sceneItem">(OUT) <see cref="SceneItemWithPick"/> instance</param>
        ///<returns>True/False of result</returns>
        public static bool GetSelectableItemByIndex(Player player, int index, out SceneItemWithPick sceneItem)
        {
            sceneItem = null;

            // if index less than 0, throw exception.
            if (index < 0)
                throw new ArgumentOutOfRangeException("index", @"Given index value must be greater than zero!");

            // check if player null
            if (player == null) return false;
            
            var selectableItems = player._selectableItems;

            // check if collection null
            if (selectableItems == null) return false;

            // check if valid index; if greater than count, then just return 'False', rather than throwing
            // an exception.  This is because the count could change due to Threading.
            if (index >= selectableItems.Count) return false;

            try
            {
                // Retrieve item from collection
                sceneItem = selectableItems[index];
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }

            return true;
        }

        // 6/15/2010
        ///<summary>
        /// Returns the <see cref="SceneItemWithPick"/> from the Player's <see cref="_itemsSelected"/> collection, using the given
        /// index value; return 'False' if index not valid.
        ///</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="index"/> is outside allowable range.</exception>
        ///<param name="player"><see cref="Player"/> instance</param>
        ///<param name="index">Collection index</param>
        ///<param name="sceneItem">(OUT) <see cref="SceneItemWithPick"/> instance</param>
        ///<returns>True/False of result</returns>
        public static bool GetItemsSelectedByIndex(Player player, int index, out SceneItemWithPick sceneItem)
        {
            sceneItem = null;

            // if index less than 0, throw exception.
            if (index < 0)
                throw new ArgumentOutOfRangeException("index", @"Given index value must be greater than zero!");

            // check if player null
            if (player == null) return false;
            
            var itemsSelected = player._itemsSelected;

            // check if collection null
            if (itemsSelected == null) return false;

            // check if valid index; if greater than count, then just return 'False', rather than throwing
            // an exception.  This is because the count could change due to Threading.
            if (index >= itemsSelected.Count) return false;

            try
            {
                // Retrieve item from collection
                sceneItem = itemsSelected[index];
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }

            return true;
        }

        // 8/4/2009
        /// <summary>
        /// Returns the <see cref="SceneItemWithPick"/> from the Player's <see cref="_selectableItems"/> collection, using the given
        /// <paramref name="uniqueItemNumber"/>, which can either be the <see cref="SceneItemWithPick.SceneItemNumber"/> for SP games, or
        /// the <see cref="SceneItemWithPick.NetworkItemNumber"/> for MP Games.
        /// </summary>
        /// <param name="player">This instance of <see cref="Player"/></param>
        /// <param name="uniqueItemNumber"><see cref="SceneItemWithPick.SceneItemNumber"/> for SP or <see cref="SceneItemWithPick.NetworkItemNumber"/> for MP</param>
        /// <param name="sceneItem">(OUT) <see cref="SceneItemWithPick"/> instance</param>
        /// <returns>True/False of result</returns>
        public static bool GetSelectableItem(Player player, int uniqueItemNumber, out SceneItemWithPick sceneItem)
        {
            // check if MP or SP game
            return player.NetworkSession == null ? GetSelectableItemSp(player, uniqueItemNumber, out sceneItem) : GetSelectableItem_MP(player, uniqueItemNumber, out sceneItem);
        }

        // 8/4/2009 (MP)
        /// <summary>
        /// Returns the <see cref="SceneItemWithPick"/> from the Player's <see cref="SelectableItemsDict"/> dictionary, using the
        /// <see cref="SceneItemWithPick.NetworkItemNumber"/> as key, in MP games.
        /// </summary>
        /// <param name="player">This instance of <see cref="Player"/></param>
        /// <param name="networkItemNumber"><see cref="SceneItemWithPick.NetworkItemNumber"/> key for dictionary</param>
        /// <param name="sceneItem">(OUT) <see cref="SceneItemWithPick"/> instance found.</param>
        /// <returns>True/False of result</returns>
        private static bool GetSelectableItem_MP(Player player, int networkItemNumber, out SceneItemWithPick sceneItem)
        {
            // Try to get given networkItemNumber SceneItemOwner.
            return player.SelectableItemsDict.TryGetValue(networkItemNumber, out sceneItem);
        }

        // 8/4/2009 (SP)
        /// <summary>
        /// Returns the <see cref="SceneItemWithPick"/> from the Player's <see cref="_selectableItems"/> collection, using the <paramref name="sceneItemNumber"/>
        /// to find a match, in SP games.
        /// </summary>
        /// <param name="player">This instance of <see cref="Player"/></param>
        /// <param name="sceneItemNumber">Unique <see cref="SceneItemWithPick.SceneItemNumber"/></param>
        /// <param name="sceneItem">(OUT) <see cref="SceneItemWithPick"/> instance</param>
        /// <returns>True/False of result</returns>
        private static bool GetSelectableItemSp(Player player, int sceneItemNumber, out SceneItemWithPick sceneItem)
        {
            
            var selectableItems = player._selectableItems; // 5/21/2010 - Cache
            var selectableItemsCount = selectableItems.Count; // 8/12/2009

            // iterate SelectableItems, searching for the given 'SceneItemNumber'.
            for (var i = 0; i < selectableItemsCount; i++)
            {
                // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                SceneItemWithPick selectableItem;
                if (!GetSelectableItemByIndex(player, i, out selectableItem))
                    break;

                if (selectableItem.SceneItemNumber != sceneItemNumber) continue;

                // found SceneItemOwner, so set to out param and return true.
                sceneItem = selectableItem;
                return true;
            } // End For

            sceneItem = null;

            // no result so return false
            return false;
        }

        #endregion


        // 4/15/2008: Draws the AreaSelect Rectangle using the mouse cordinates to create
        //            the rectangle size.
        // 8/26/2008: Updated to Optimize memory.    
        // 1/15/2009: Updated to be a Static method, which should optimize memory.
        // 2/5/2009: Updated to only draw the AreaSelect Rectangle in this method.  Also updated to NOT deselect items when
        //           the user is holding down the LeftShift key.
        /// <summary>
        /// Draws the <see cref="TerrainAreaSelect"/> rectangle using the <see cref="Common.Cursor"/> cordinates to create
        /// the rectangle size.
        /// </summary>
        /// <param name="player"><see cref="Player"/> instance</param>
        /// <param name="cursorX">Cursor X</param>
        /// <param name="cursorY">Cursor Y</param>
        internal static void AreaSelect_DrawRect(Player player, int cursorX, int cursorY)
        {
            // 7/12/2009: Added the flag check for '_pickSelectionInProgress'.
            //
            // 2nd - Area Select
            //            
            if (!TerrainAreaSelect.AreaSelect && !_pickSelectionInProgress)
            {
                // 1st - Deselect all
                DeSelectAll(player);

                // Set Up AreaSelect Rectangle
                TerrainAreaSelect.AreaSelect = true;
                TerrainAreaSelect.StartSelect = new Vector2 {X = cursorX, Y = cursorY};

                TerrainAreaSelect.CursorPos = new Vector2 {X = cursorX, Y = cursorY};
            }
            else
            {
                // Continue updating AreaSelect Rectangle
                TerrainAreaSelect.CursorPos = new Vector2 {X = cursorX, Y = cursorY};                
                

            } // End If AreaSelect = true
        }

        // 4/28/2009; // 6/14/2010 - Updated to pass in 'Player' instance param.
        /// <summary>
        /// Deselects all <see cref="SceneItemWithPick"/> by marking 'PickSelected' to false.
        /// </summary>
        /// <param name="player"><see cref="Player"/> instance</param>
        public static void DeSelectAll(Player player)
        {
            // 6/15/2010 - Check if null
            if (player == null) return;

            var itemsSelected = player._itemsSelected; // 6/14/2010

            // go through all ItemsSelected Array
            var itemsSelectedCount = itemsSelected.Count; // 8/12/2009
            for (var i = 0; i < itemsSelectedCount; i++)
            {
                // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                SceneItemWithPick selected;
                if (!GetItemsSelectedByIndex(player, i, out selected))
                    break;

                if (selected == null) continue;

                selected.PickSelected = false;
            } // End ForLoop   

            // 2/5/2009 - Clear Array
            itemsSelected.Clear();
        }

        // 7/7/2009 - Create instance of IComparer for BinarySearch
        static readonly SceneItemWithPickComparer BinarySearchComparer = new SceneItemWithPickComparer();
       

        // 6/16/2009
        /// <summary>
        /// Deselects the given items, and marks 'PickSelected' to false.
        /// </summary>        
        /// <param name="itemSelected"><see cref="SceneItemWithPick"/> to update</param>
        /// <returns>true/false of result</returns>
        internal static bool DeSelectSceneItem(SceneItemWithPick itemSelected)
        {
            // If not set PickSelected, then DeSelect action unnecessary.
            if (!itemSelected.PickSelected) return true;
            
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(itemSelected.PlayerNumber, out player))
                return false;

            if (player == null) return false;

            // search for given SceneItemOwner in array, and remove.
            var itemsSelected = player._itemsSelected; // 6/15/2010 - Cache
            var index = itemsSelected.BinarySearch(itemSelected, BinarySearchComparer);

            if (index >= 0)
            {
                // Turn off pick, and remove from list
                // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                SceneItemWithPick selected;
                if (!GetItemsSelectedByIndex(player, index, out selected))
                    return false;

                selected.PickSelected = false;
                itemsSelected.RemoveAt(index);
                return true;
            }

            return false;            
        }
        

        // 2/5/2009
        /// <summary>
        /// Using the <see cref="TerrainAreaSelect"/> rectangle created, iterates through the <see cref="_selectableItems"/> collection, selecting any
        /// <see cref="SceneItemWithPick"/> contained within the <see cref="TerrainAreaSelect"/> rectangle.
        /// </summary>
        /// <param name="player"><see cref="Player"/> instance</param>
        internal static void AreaSelect_SelectItems(Player player)
        {
            var selRect = TerrainAreaSelect.SelectionBox;
            var itemsSelected = player._itemsSelected; // 6/14/2010
            var selectableItems = player._selectableItems; // 6/14/2010


            // Check 'Selectable' Items List Array                
            var selectableItemsCount = selectableItems.Count; // 8/12/2009
            for (var i = 0; i < selectableItemsCount; i++)
            {
                // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                SceneItemWithPick selectableItem;
                if (!GetSelectableItemByIndex(player, i, out selectableItem))
                    break;

                // 5/25/2009
                if (selectableItem == null) continue;

                // 11/13/2008 - Only Check Alive objects
                if (!selectableItem.IsAlive) continue;

                // 12/30/2008 - Skip items not moveable!
                if (!selectableItem.ItemMoveable) continue;

                Point itemPos;
                selectableItem.GetScreenPos(out itemPos);

                // Check if selectableItem is in AreaSelect Rectangle
                if (!selRect.Contains(itemPos) || selectableItem.PickSelected)
                    continue;

                selectableItem.PickSelected = true;

                // Add SceneItemOwner to Selected Array
                itemsSelected.Add(selectableItem);
            } // End For Loop

            
        }

        // 8/14/2009; 5/21/2010: Updated to be STATIC method.
        /// <summary>
        /// To know if current player has reached the Population max.
        /// </summary>
        /// <param name="player">this instance of <see cref="Player"/></param>
        /// <returns>true/false of result</returns>
        public static bool IsAtPopulationMax(Player player)
        {
            return PopulationMax - player.Population <= 0;
        }

       
        // 5/18/2009 // 6/14/2010: Updated to pass in 'Player' instance param.
        /// <summary>
        /// Searches the <see cref="_selectableItems"/> collection for a <see cref="BuildingScene"/> with the following two
        /// conditions TRUE;
        ///  1) 'IsSpecialTechBuilding' set to True?
        ///  2) Does given <paramref name="specialBuildingName"/> equal to the PlayableItemAtts 'SpecialBuildingName'?
        /// </summary>
        /// <remarks>
        /// This is used in the <see cref="IFDTileManager"/> class when a <see cref="BuildingScene"/> is added, to see if the 'Special' enabler building
        /// was already created!  If so, then the TileState is immediately changed to the 'None' state, rather than the 'Disabled' state.
        /// </remarks>
        /// <param name="player"><see cref="Player"/> instance</param>
        /// <param name="specialBuildingName">Special building name</param>
        /// <returns>true/false of result</returns>
        internal static bool IsSpecialBuildingPlaced(Player player, string specialBuildingName)
        {
            var selectableItems = player._selectableItems; // 6/14/2010

            // Check 'Selectable' Items List Array                
            var selectableItemsCount = selectableItems.Count; // 8/12/2009
            for (var i = 0; i < selectableItemsCount; i++)
            {
                // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                SceneItemWithPick selectableItem;
                if (!GetSelectableItemByIndex(player, i, out selectableItem))
                    break;  

                // 5/25/2009
                if (selectableItem == null) continue;

                if (!(selectableItem is BuildingScene)) continue;

                if (selectableItem.PlayableItemAtts.IsSpecialEnablerBuilding &&
                    specialBuildingName == selectableItem.PlayableItemAtts.SpecialBuildingName)
                {
                    return true;
                }
            }

            return false;
        }

        // 5/18/2009
        /// <summary>
        /// Searches the <see cref="_selectableItems"/> collection for a <see cref="BuildingScene"/> with the following two
        /// conditions TRUE;
        ///  1) 'IsSpecialTechBuilding' set to True?
        ///  2) Does given <paramref name="specialBuildingName"/> equal to the PlayableItemAtts 'SpecialBuildingName'?
        ///  3) Is not equal to <paramref name="skipSceneItem"/> instance.
        /// </summary>
        /// <remarks>
        /// This is used in the <see cref="IFDTileManager"/> class when a <see cref="BuildingScene"/> is added, to see if the 'Special' enabler building
        /// was already created!  If so, then the TileState is immediately changed to the 'None' state, rather than the 'Disabled' state.
        /// </remarks>
        /// <param name="player"><see cref="Player"/> instance</param>
        /// <param name="skipSceneItem"><see cref="SceneItemWithPick"/> instance which should be skippied during checks.</param>
        /// <param name="specialBuildingName">Special building name</param>
        /// <returns>true/false of result</returns>
        internal static bool IsSpecialBuildingPlaced(Player player, SceneItemWithPick skipSceneItem, string specialBuildingName)
        {
            var selectableItems = player._selectableItems; // 6/14/2010

            // Check 'Selectable' Items List Array                
            var selectableItemsCount = selectableItems.Count; // 8/12/2009
            for (var i = 0; i < selectableItemsCount; i++)
            {
                // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                SceneItemWithPick selectableItem;
                if (!GetSelectableItemByIndex(player, i, out selectableItem))
                    break;

                // 5/25/2009
                if (selectableItem == null) continue;

                // skip if same 'SceneItemNumber'.
                if (selectableItem.SceneItemNumber == skipSceneItem.SceneItemNumber)
                    continue;

                if (!(selectableItem is BuildingScene)) continue;

                if (selectableItem.PlayableItemAtts.IsSpecialEnablerBuilding &&
                    specialBuildingName == selectableItem.PlayableItemAtts.SpecialBuildingName)
                {
                    return true;
                }
            }

            return false;
        }

        // 1/6/2010
        /// <summary>
        /// Sets the Buildable rectangle area, situated around the given
        /// <paramref name="centerPosition"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="distanceFromCenter"/> is less or equal to zero.</exception>
        /// <param name="centerPosition"><see cref="Vector3"/> as center position</param>
        /// <param name="distanceFromCenter">Distance from center position</param>
        public void SetBuildableAreaRectangle(Vector3 centerPosition, int distanceFromCenter)
        {
            // make sure distance is greater than 0.
            if (distanceFromCenter <= 0)
                throw new ArgumentOutOfRangeException("distanceFromCenter", @"Distance from center MUST be greater than zero.");

            _buildableAreaRectangle.X = (int)(centerPosition.X - distanceFromCenter);
            _buildableAreaRectangle.Y = (int)(centerPosition.Z - distanceFromCenter);

        }

        #region Dispose
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this final dispose?</param>
        private void Dispose(bool disposing)
        {
            if (!disposing) return;

            // Note: 1/6/2010: Do not NULL ItemsSelected.
            // dispose managed resources
            // Dispose of Resources
            var count = _itemsSelected.Count; // 5/21/2010
            for (var i = 0; i < count; i++)
            {
                _itemsSelected[i] = null;
            }
            _itemsSelected.Clear();

            // 
            // 1/5/2010 - Note: Up to this point, no InternalDriverError will be thrown in the SpriteBatch.
            //          - Note: Discovered, the error is coming from the call to '_selectableItems' dispose!

            // Note: 1/6/2010: Do not NULL _selectableItems.
            var count2 = _selectableItems.Count; // 5/21/2010
            for (var j = 0; j < count2; j++)
            {
                var sceneItemWithPick = _selectableItems[j]; // 5/21/2010
                if (sceneItemWithPick != null)
                    sceneItemWithPick.Dispose(true);

                _selectableItems[j] = null;
            }
            _selectableItems.Clear();
           
            // 11/17/2009
            if (SceneItemsByName != null)
                SceneItemsByName.Clear();

            // 1/7/2010 - Dispose of the current PoolNodes in PoolManager.
            if (PoolManager != null) 
                PoolManager.Dispose();
           

#if !XBOX360
            _gameConsole = null;
#endif
            // free native resources
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        
    }
}