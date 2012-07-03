#region File Description
//-----------------------------------------------------------------------------
// PoolManager.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using MemoryPoolComponent;
using MemoryPoolComponent.Interfaces;
using TWEngine.IFDTiles;
using TWEngine.InstancedModels.Enums;
using TWEngine.MemoryPool.Interfaces;
using TWEngine.Particles;
using TWEngine.Players;
using TWEngine.rtsCommands;
using TWEngine.SceneItems;
using TWEngine.ForceBehaviors;

namespace TWEngine.MemoryPool
{
    /// <summary>
    /// The <see cref="PoolManager"/> class, creates multiple <see cref="Pool{TDefault}"/> instances for
    /// each of the different playable <see cref="SceneItem"/>, like the <see cref="SciFiTankScene"/> class.
    /// Ultimately, the retrieving and returning of each of these <see cref="PoolNode"/> instances are managed
    /// here.
    /// </summary>
    public class PoolManager : PoolManagerBase
    {
        // 8/17/2009 - Store PlayerNumber who owns this instance of PoolManager.
        private readonly int _playerNumberOwner;


        // 8/14/2009 - Max Population constants.
        /// <summary>
        /// <see cref="SciFiTankScene"/> max population setting.
        /// </summary>
        private static int _tankItemsMaxPopulation = 20;
        /// <summary>
        /// <see cref="DefenseScene"/> max population setting.
        /// </summary>
        private static int _defenseItemsMaxPopulation = 5;
        /// <summary>
        /// <see cref="SciFiAircraftScene"/> max population setting.
        /// </summary>
        private static int _aircraftItemsMaxPopulation = 20;
        /// <summary>
        /// <see cref="BuildingScene"/> max population setting.
        /// </summary>
        private static int _buildingItemsMaxPopulation = 20;


        // NetworkGame RTS Commands
        private static Pool<RTSCommAddSceneItem> _rtsCommAddSceneItems; // 5/13/2009
        private static Pool<RTSCommAttackSceneItem> _rtsCommAttackSceneItems; // 5/13/2009
        private static Pool<RTSCommCeaseAttackSceneItem> _rtsCommCeaseAttackSceneItems; // 6/2/2009 
        private static Pool<RTSCommDelayTime> _rtsCommDelayTimeItems; // 5/13/2009
        private static Pool<RTSCommGameSlow> _rtsCommGameSlowItems; // 5/13/2009
        private static Pool<RTSCommGameTurn> _rtsCommGameTurnItems; // 5/13/2009
        private static Pool<RTSCommIsReady> _rtsCommIsReadyItems; // 5/13/2009
        private static Pool<RTSCommKillSceneItem> _rtsCommKillSceneItems; // 5/13/2009
        private static Pool<RTSCommLobbyData> _rtsCommLobbyDataItems; // 5/13/2009
        private static Pool<RTSCommMoveSceneItem2> _rtsCommMoveScene2Items; // 5/13/2009
        private static Pool<RTSCommMoveSceneItem> _rtsCommMoveSceneItems; // 5/13/2009
        private static Pool<RTSCommQueueMarker> _rtsCommQueueMarkerItems; // 5/13/2009
        private static Pool<RTSCommSceneItemHealth> _rtsCommSceneItemHealthItems; // 8/3/2009
        private static Pool<RTSCommSceneItemStance> _rtsCommSceneItemStanceItems; // 6/2/2009
        private static Pool<RTSCommStartAttackSceneItem> _rtsCommStartAttackSceneItems; // 5/13/2009
        private static Pool<RTSCommSyncTime> _rtsCommSyncTimeItems; // 5/13/2009
        private static Pool<RTSCommValidator> _rtsCommValidator; // 6/16/2010
        private readonly Pool<BuildingScenePoolItem> _buildingSceneItems; // 2/24/2009
        private readonly Pool<DefenseScenePoolItem> _defenseSceneItems; // 2/26/2009
        private readonly Pool<BuildingScenePoolItem> _ifdBuildingSceneItems; // 2/24/2009
        private readonly Pool<DefenseScenePoolItem> _ifdDefenseSceneItems; // 2/26/2009  
        private readonly Pool<ProjectilePoolItem> _projectileItems; // 5/13/2009
        private readonly Pool<SciFiAircraftScenePoolItem> _sciFiAircraftSceneItems; // 2/26/2009
        private readonly Pool<SciFiTankScenePoolItem> _sciFiTankSceneItems; // 2/23/2009

        #region Properties

        // 4/24/2011
        /// <summary>
        /// <see cref="SciFiTankScene"/> max population setting.
        /// </summary>
        public static int TankItemsMaxPopulation
        {
            get { return _tankItemsMaxPopulation; }
            set { _tankItemsMaxPopulation = value; }
        }

        // 4/24/2011
        /// <summary>
        /// <see cref="DefenseScene"/> max population setting.
        /// </summary>
        public static int DefenseItemsMaxPopulation
        {
            get { return _defenseItemsMaxPopulation; }
            set { _defenseItemsMaxPopulation = value; }
        }

        // 4/24/2011
        /// <summary>
        /// <see cref="SciFiAircraftScene"/> max population setting.
        /// </summary>
        public static int AircraftItemsMaxPopulation
        {
            get { return _aircraftItemsMaxPopulation; }
            set { _aircraftItemsMaxPopulation = value; }
        }

        // 4/24/2011
        /// <summary>
        /// <see cref="BuildingScene"/> max population setting.
        /// </summary>
        public static int BuildingItemsMaxPopulation
        {
            get { return _buildingItemsMaxPopulation; }
            set { _buildingItemsMaxPopulation = value; }
        }

        #endregion

        // constructor
        /// <summary>
        /// Constructor for the <see cref="PoolManager"/> class, which creates the
        /// <see cref="Pool{TDefault}"/> instances for each <see cref="SceneItem"/> type, and
        /// populates with capacity amounts given for each <see cref="SceneItem"/> type.
        /// </summary>
        /// <param name="playerNumber">The player's number represented as a byte.</param>
        public PoolManager(byte playerNumber)
        {
            // 8/17/2009 - Store the Player number to who owns this.
            _playerNumberOwner = playerNumber;

            // Set Player's atts into class
            {
                // Populate Game SciFi-Tanks
                _sciFiTankSceneItems = new Pool<SciFiTankScenePoolItem>();
                _sciFiTankSceneItems.SetPoolNodeCapacities(_tankItemsMaxPopulation, false, 0);

                // 3/23/2009 - Attach EventHandler to the Pool Get/Return Events.
                _sciFiTankSceneItems.PoolItemGetCalled += SciFiTankSceneItemsPoolItemGetCalled;
                _sciFiTankSceneItems.PoolItemReturnCalled += SciFiTankSceneItemsPoolItemReturnCalled;

                // Now get new Pool enumerator, and set the 'player' atts.
                UpdatePoolNodesAtts(_sciFiTankSceneItems, this, playerNumber); // 1/7/2010
                /*foreach (var node in _sciFiTankSceneItems.AllNodes)
                {
                    var item = (SciFiTankScenePoolItem)node.Item;

                    item.SceneItemInstance.PlayerNumber = playerNumber;
                    item.PoolManager = this;
                    item.PoolNode = node;
                    //item.PoolNode.PoolOwner = _sciFiTankSceneItems; // 6/29/2009

                    _sciFiTankSceneItems.SetItemValue(node);
                }*/
            }

            {
                // Populate Game Defense items
                _defenseSceneItems = new Pool<DefenseScenePoolItem>();
                _defenseSceneItems.SetPoolNodeCapacities(_defenseItemsMaxPopulation, false, 0);

                // 4/6/2009 - Attach EventHandler to the Pool Get/Return Events.
                _defenseSceneItems.PoolItemGetCalled += DefenseSceneItemsPoolItemGetCalled;
                _defenseSceneItems.PoolItemReturnCalled += DefenseSceneItemsPoolItemReturnCalled;

                // Now get new Pool enumerator, and set the 'player' atts.
                UpdatePoolNodesAtts(_defenseSceneItems, this, playerNumber); // 1/7/2010
            }

            {
                // Populate Game SciFi-Aircrafts
                _sciFiAircraftSceneItems = new Pool<SciFiAircraftScenePoolItem>();
                _sciFiAircraftSceneItems.SetPoolNodeCapacities(_aircraftItemsMaxPopulation, false, 0);

                // 3/23/2009 - Attach EventHandler to the Pool Get/Return Events.
                _sciFiAircraftSceneItems.PoolItemGetCalled += SciFiAircraftSceneItemsPoolItemGetCalled;
                _sciFiAircraftSceneItems.PoolItemReturnCalled += SciFiAircraftSceneItemsPoolItemReturnCalled;

                // Now get new Pool enumerator, and set the 'player' atts.
                UpdatePoolNodesAtts(_sciFiAircraftSceneItems, this, playerNumber); // 1/7/2010
            }

            {
                // Populate Game Buildings

                _buildingSceneItems = new Pool<BuildingScenePoolItem>();
                _buildingSceneItems.SetPoolNodeCapacities(_buildingItemsMaxPopulation, false, 0);

                // 7/27/2009 - Attach EventHandler to the Pool Get/Return Events.
                _buildingSceneItems.PoolItemGetCalled += BuildingSceneItemsPoolItemGetCalled;
                _buildingSceneItems.PoolItemReturnCalled += BuildingSceneItemsPoolItemReturnCalled;

                // Now get new Pool enumerator, and set the 'player' atts.
                UpdatePoolNodesAtts(_buildingSceneItems, this, playerNumber); // 1/7/2010
            }

            {
                // Populate IFD Buildings

                _ifdBuildingSceneItems = new Pool<BuildingScenePoolItem>();
                _ifdBuildingSceneItems.SetPoolNodeCapacities(1, false, 0);

                // Now get new Pool enumerator, and set the 'player' atts.
                UpdatePoolNodesAtts(_ifdBuildingSceneItems, this, playerNumber); // 1/7/2010
            }

            {
                // Populate IFD Defense items
                _ifdDefenseSceneItems = new Pool<DefenseScenePoolItem>();
                _ifdDefenseSceneItems.SetPoolNodeCapacities(1, false, 0);

                // Now get new Pool enumerator, and set the 'player' atts.
                UpdatePoolNodesAtts(_ifdDefenseSceneItems, this, playerNumber); // 1/7/2010
                
            }

            // 5/13/2009
            {
                // Populate Projectile Items

                _projectileItems = new Pool<ProjectilePoolItem>();
                _projectileItems.SetPoolNodeCapacities(500, false, 0);

                // Now get new Pool enumerator, and set some atts.
                foreach (var node in _projectileItems.AllNodes)
                {
                    var item = (ProjectilePoolItem)node.Item;

                    item.PoolManager = this;
                    item.PoolNode = node;
                    //item.PoolNode.PoolOwner = _projectileItems; // 6/29/2009

                    _projectileItems.SetItemValue(node);
                }
            }
        }

        /// <summary>
        /// Default empty <see cref="PoolManager"/> constructor.
        /// </summary>
        public PoolManager() : this(0)
        {
        }

        // 5/13/2009
        /// <summary>
        /// Initalizes the <see cref="RTSCommand"/> pools into memory, used
        /// for network games.  Should be called at the very start of the game.
        /// </summary>
        public static void InitializeNetworkRTSCommands()
        {
            // 5/13/2009 - NetworkGame RTS Commands
            {
                // AddSceneItem

                _rtsCommAddSceneItems = new Pool<RTSCommAddSceneItem>();
                _rtsCommAddSceneItems.SetPoolNodeCapacities(100, true, 10);

                // Now get new Pool enumerator, and set some atts.
                UpdatePoolNodesAtts(_rtsCommAddSceneItems);
                /*foreach (var node in _rtsCommAddSceneItems.AllNodes)
                {
                    var item = (RTSCommAddSceneItem)node.Item;

                    item.PoolNode = node;
                    //item.PoolNode.PoolOwner = _rtsCommAddSceneItems; // 6/29/2009

                    _rtsCommAddSceneItems.SetItemValue(node);
                }*/
            }

            {
                // AttackSceneItem

                _rtsCommAttackSceneItems = new Pool<RTSCommAttackSceneItem>();
                _rtsCommAttackSceneItems.SetPoolNodeCapacities(100, true, 10);

                // Now get new Pool enumerator, and set some atts.
                UpdatePoolNodesAtts(_rtsCommAttackSceneItems);
            }

            {
                // DelayTime

                _rtsCommDelayTimeItems = new Pool<RTSCommDelayTime>();
                _rtsCommDelayTimeItems.SetPoolNodeCapacities(100, true, 10);

                // Now get new Pool enumerator, and set some atts.
                UpdatePoolNodesAtts(_rtsCommDelayTimeItems);
            }

            {
                // GameSlow

                _rtsCommGameSlowItems = new Pool<RTSCommGameSlow>();
                _rtsCommGameSlowItems.SetPoolNodeCapacities(100, true, 10);

                // Now get new Pool enumerator, and set some atts.
                UpdatePoolNodesAtts(_rtsCommGameSlowItems);
            }

            {
                // GameTurn

                _rtsCommGameTurnItems = new Pool<RTSCommGameTurn>();
                _rtsCommGameTurnItems.SetPoolNodeCapacities(100, true, 10);

                // Now get new Pool enumerator, and set some atts.
                UpdatePoolNodesAtts(_rtsCommGameTurnItems);
            }

            {
                // IsReady

                _rtsCommIsReadyItems = new Pool<RTSCommIsReady>();
                _rtsCommIsReadyItems.SetPoolNodeCapacities(100, true, 10);

                // Now get new Pool enumerator, and set some atts.
                UpdatePoolNodesAtts(_rtsCommIsReadyItems);
            }

            {
                // KillSceneItem

                _rtsCommKillSceneItems = new Pool<RTSCommKillSceneItem>();
                _rtsCommKillSceneItems.SetPoolNodeCapacities(100, true, 10);

                // Now get new Pool enumerator, and set some atts.
                UpdatePoolNodesAtts(_rtsCommKillSceneItems);
            }

            {
                // LobbyData

                _rtsCommLobbyDataItems = new Pool<RTSCommLobbyData>();
                _rtsCommLobbyDataItems.SetPoolNodeCapacities(100, false, 0);

                // Now get new Pool enumerator, and set some atts.
                UpdatePoolNodesAtts(_rtsCommLobbyDataItems);
            }

            {
                // MoveSceneItem

                _rtsCommMoveSceneItems = new Pool<RTSCommMoveSceneItem>();
                _rtsCommMoveSceneItems.SetPoolNodeCapacities(100, true, 10);

                // Now get new Pool enumerator, and set some atts.
                UpdatePoolNodesAtts(_rtsCommMoveSceneItems);
            }

            {
                // MoveSceneItem2

                _rtsCommMoveScene2Items = new Pool<RTSCommMoveSceneItem2>();
                _rtsCommMoveScene2Items.SetPoolNodeCapacities(100, true, 10);

                // Now get new Pool enumerator, and set some atts.
                UpdatePoolNodesAtts(_rtsCommMoveScene2Items);
            }

            {
                // QueueMarker

                _rtsCommQueueMarkerItems = new Pool<RTSCommQueueMarker>();
                _rtsCommQueueMarkerItems.SetPoolNodeCapacities(100, true, 10);

                // Now get new Pool enumerator, and set some atts.
                UpdatePoolNodesAtts(_rtsCommQueueMarkerItems);
            }

            {
                // StartAttackSceneItem

                _rtsCommStartAttackSceneItems = new Pool<RTSCommStartAttackSceneItem>();
                _rtsCommStartAttackSceneItems.SetPoolNodeCapacities(100, true, 10);

                // Now get new Pool enumerator, and set some atts.
                UpdatePoolNodesAtts(_rtsCommStartAttackSceneItems);
            }

            {
                // SyncTime

                _rtsCommSyncTimeItems = new Pool<RTSCommSyncTime>();
                _rtsCommSyncTimeItems.SetPoolNodeCapacities(100, true, 10);

                // Now get new Pool enumerator, and set some atts.
                UpdatePoolNodesAtts(_rtsCommSyncTimeItems);
            }

            {
                // SceneItemStance

                _rtsCommSceneItemStanceItems = new Pool<RTSCommSceneItemStance>();
                _rtsCommSceneItemStanceItems.SetPoolNodeCapacities(100, true, 10);

                // Now get new Pool enumerator, and set some atts.
                UpdatePoolNodesAtts(_rtsCommSceneItemStanceItems);
            }

            {
                // Cease AttackSceneItem

                _rtsCommCeaseAttackSceneItems = new Pool<RTSCommCeaseAttackSceneItem>();
                _rtsCommCeaseAttackSceneItems.SetPoolNodeCapacities(100, true, 10);

                // Now get new Pool enumerator, and set some atts.
                UpdatePoolNodesAtts(_rtsCommCeaseAttackSceneItems);
            }

            {
                // SceneItemHealth

                _rtsCommSceneItemHealthItems = new Pool<RTSCommSceneItemHealth>();
                _rtsCommSceneItemHealthItems.SetPoolNodeCapacities(100, true, 10);

                // Now get new Pool enumerator, and set some atts.
                UpdatePoolNodesAtts(_rtsCommSceneItemHealthItems);
            }

            { // 6/16/2010
                // Validator

                _rtsCommValidator = new Pool<RTSCommValidator>();
                _rtsCommValidator.SetPoolNodeCapacities(100, true, 10);

                // Now get new Pool enumerator, and set some atts.
                UpdatePoolNodesAtts(_rtsCommValidator);
            }
        }

        // 1/7/2010 - Overload v2.
        /// <summary>
        /// a Generic type method helper, which takes a <see cref="Pool{TDefault}"/> type, and 
        /// updates the internal <see cref="PoolNode"/> attributes.
        /// </summary>
        /// <typeparam name="TU">Some Class of type <see cref="IPoolNodeItem"/></typeparam>
        /// <param name="pool"> Some instance of <see cref="Pool{TDefault}"/> </param>
        /// <param name="poolManager"><see cref="PoolManagerBase"/> owner.</param>
        /// <param name="playerNumber"><see cref="Player"/> number</param>
        protected new static void UpdatePoolNodesAtts<TU>(Pool<TU> pool, PoolManagerBase poolManager, byte playerNumber) where TU : class, IPoolNodeItem, IPoolNodeSceneItem, new()
        {
            foreach (var node in pool.AllNodes)
            {
                var item = (TU)node.Item;

                item.SceneItemInstance.PlayerNumber = playerNumber;
                item.PoolManager = poolManager;
                item.PoolNode = node;

                pool.SetItemValue(node);
            }
        }

        // 7/27/2009
        /// <summary>
        /// Returns the 'AvailableCount' of the given <see cref="PoolItem"/> type.
        /// </summary>
        /// <param name="itemType">typeof(<see cref="PoolItem"/>) to check</param>
        /// <returns>Number of 'AvailableCount'</returns>
        /// <remarks>
        /// Parameter <paramref name="itemType"/> must be given as typeof(PoolItem) type class,
        /// where the class is inherited from the base class <see cref="PoolItem"/>.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown when <paramref name="itemType"/> is not of the valid <see cref="PoolItem"/> base.</exception>
        public override int GetPoolItemAvailableCount(Type itemType)
        {
            switch (itemType.Name)
            {
                case "SciFiTankScenePoolItem":
                    return _sciFiTankSceneItems.AvailableCount;
                case "SciFiAircraftScenePoolItem":
                    return _sciFiAircraftSceneItems.AvailableCount;
                case "BuildingScenePoolItem":
                    return _buildingSceneItems.AvailableCount;
                case "DefenseScenePoolItem":
                    return _defenseSceneItems.AvailableCount;
                default:
                    throw new ArgumentException("The given type is not supported for the 'GetPoolItem' method.");
            }
        }

        // 7/27/2009
        /// <summary>
        /// Returns the 'Capacity' of the given <see cref="PoolItem"/> type.
        /// </summary>
        /// <param name="itemType">typeOf(<see cref="PoolItem"/>) to check</param>
        /// <returns>Number of 'Capacity'</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="itemType"/> is not of the valid <see cref="PoolItem"/> base.</exception>
        public override int GetPoolItemCapacity(Type itemType)
        {
            switch (itemType.Name)
            {
                case "SciFiTankScenePoolItem":
                    return _sciFiTankSceneItems.Capacity;
                case "SciFiAircraftScenePoolItem":
                    return _sciFiAircraftSceneItems.Capacity;
                case "BuildingScenePoolItem":
                    return _buildingSceneItems.Capacity;
                case "DefenseScenePoolItem":
                    return _defenseSceneItems.Capacity;
                default:
                    throw new ArgumentException("The given type is not supported for the 'GetPoolItem' method.");
            }
        }

        #region Get PoolNode Methods

        // 6/29/2009
        /// <summary>
        /// Returns an instance of <see cref="SciFiAircraftScenePoolItem"/>.
        /// </summary>
        /// <param name="commSceneItem">(OUT) <see cref="SciFiAircraftScenePoolItem"/>. instance.</param>
        public void GetNode(out SciFiAircraftScenePoolItem commSceneItem)
        {
            // Try to retrieve a PoolNode, if fail, create an instance anyway!   
            PoolNode poolNode;
            commSceneItem = (_sciFiAircraftSceneItems.Get(out poolNode) ? poolNode.Item : new SciFiAircraftScenePoolItem()) as SciFiAircraftScenePoolItem;
        }

        // 6/29/2009
        /// <summary>
        /// Returns an instance of <see cref="SciFiTankScenePoolItem"/>.
        /// </summary>
        /// <param name="commSceneItem">(OUT) <see cref="SciFiTankScenePoolItem"/>. instance.</param>
        public void GetNode(out SciFiTankScenePoolItem commSceneItem)
        {
            // Try to retrieve a PoolNode, if fail, create an instance anyway!   
            PoolNode poolNode;
            commSceneItem = (_sciFiTankSceneItems.Get(out poolNode) ? poolNode.Item : new SciFiTankScenePoolItem()) as SciFiTankScenePoolItem;
        }

        // 6/29/2009
        /// <summary>
        /// Returns an instance of <see cref="BuildingScenePoolItem"/>.
        /// </summary>
        /// <param name="commSceneItem">(OUT) <see cref="BuildingScenePoolItem"/>. instance.</param>
        public void GetNode(out BuildingScenePoolItem commSceneItem)
        {
            // Try to retrieve a PoolNode, if fail, create an instance anyway!   
            PoolNode poolNode;
            commSceneItem = (_buildingSceneItems.Get(out poolNode) ? poolNode.Item : new BuildingScenePoolItem()) as BuildingScenePoolItem;
        }

        // 6/29/2009
        /// <summary>
        /// Returns an instance of <see cref="DefenseScenePoolItem"/>.
        /// </summary>
        /// <param name="commSceneItem">(OUT) <see cref="DefenseScenePoolItem"/>. instance.</param>
        public void GetNode(out DefenseScenePoolItem commSceneItem)
        {
            // Try to retrieve a PoolNode, if fail, create an instance anyway!   
            PoolNode poolNode;
            commSceneItem = (_defenseSceneItems.Get(out poolNode) ? poolNode.Item : new DefenseScenePoolItem()) as DefenseScenePoolItem;
        }

        // 6/29/2009
        /// <summary>
        /// Returns an instance of <see cref="ProjectilePoolItem"/>.
        /// </summary>
        /// <param name="commSceneItem">(OUT) <see cref="ProjectilePoolItem"/>. instance.</param>
        public void GetNode(out ProjectilePoolItem commSceneItem)
        {
            // Try to retrieve a PoolNode, if fail, create an instance anyway!   
            PoolNode poolNode;
            commSceneItem = (_projectileItems.Get(out poolNode) ? poolNode.Item : new ProjectilePoolItem()) as ProjectilePoolItem;
        }

        // 6/29/2009
        /// <summary>
        /// Returns an instance of <see cref="BuildingScenePoolItem"/>.
        /// </summary>
        /// <param name="commSceneItem">(OUT) <see cref="BuildingScenePoolItem"/>. instance.</param>
        public void GetNode_IFD(out BuildingScenePoolItem commSceneItem)
        {
            // Try to retrieve a PoolNode, if fail, create an instance anyway!   
            PoolNode poolNode;
            commSceneItem = (_ifdBuildingSceneItems.Get(out poolNode) ? poolNode.Item : new BuildingScenePoolItem()) as BuildingScenePoolItem;
        }

        // 6/29/2009
        /// <summary>
        /// Returns an instance of <see cref="DefenseScenePoolItem"/>.
        /// </summary>
        /// <param name="commSceneItem">(OUT) <see cref="DefenseScenePoolItem"/>. instance.</param>
        public void GetNode_IFD(out DefenseScenePoolItem commSceneItem)
        {
            // Try to retrieve a PoolNode, if fail, create an instance anyway!   
            PoolNode poolNode;
            commSceneItem = (_ifdDefenseSceneItems.Get(out poolNode) ? poolNode.Item : new DefenseScenePoolItem()) as DefenseScenePoolItem;
        }


        // 6/29/2009
        /// <summary>
        /// Returns an instance of <see cref="RTSCommAddSceneItem"/>.
        /// </summary>
        /// <param name="commSceneItem">(OUT) <see cref="RTSCommAddSceneItem"/>. instance.</param>
        public static void GetNode(out RTSCommAddSceneItem commSceneItem)
        {
            // Try to retrieve a PoolNode, if fail, create an instance anyway!   
            PoolNode poolNode;
            commSceneItem = (_rtsCommAddSceneItems.Get(out poolNode) ? poolNode.Item : new RTSCommAddSceneItem()) as RTSCommAddSceneItem;
        }

        // 6/29/2009
        /// <summary>
        /// Returns an instance of <see cref="RTSCommAttackSceneItem"/>.
        /// </summary>
        /// <param name="commSceneItem">(OUT) <see cref="RTSCommAttackSceneItem"/>. instance.</param>
        public static void GetNode(out RTSCommAttackSceneItem commSceneItem)
        {
            // Try to retrieve a PoolNode, if fail, create an instance anyway!   
            PoolNode poolNode;
            commSceneItem = (_rtsCommAttackSceneItems.Get(out poolNode) ? poolNode.Item : new RTSCommAttackSceneItem()) as RTSCommAttackSceneItem;
        }

        // 6/29/2009
        /// <summary>
        /// Returns an instance of <see cref="RTSCommCeaseAttackSceneItem"/>.
        /// </summary>
        /// <param name="commSceneItem">(OUT) <see cref="RTSCommCeaseAttackSceneItem"/>. instance.</param>
        public static void GetNode(out RTSCommCeaseAttackSceneItem commSceneItem)
        {
            // Try to retrieve a PoolNode, if fail, create an instance anyway!   
            PoolNode poolNode;
            commSceneItem = (_rtsCommCeaseAttackSceneItems.Get(out poolNode) ? poolNode.Item : new RTSCommCeaseAttackSceneItem()) as RTSCommCeaseAttackSceneItem;
        }

        // 6/29/2009
        /// <summary>
        /// Returns an instance of <see cref="RTSCommDelayTime"/>.
        /// </summary>
        /// <param name="commSceneItem">(OUT) <see cref="RTSCommDelayTime"/>. instance.</param>
        public static void GetNode(out RTSCommDelayTime commSceneItem)
        {
            // Try to retrieve a PoolNode, if fail, create an instance anyway!   
            PoolNode poolNode;
            commSceneItem = (_rtsCommDelayTimeItems.Get(out poolNode) ? poolNode.Item : new RTSCommDelayTime()) as RTSCommDelayTime;
        }

        // 6/29/2009
        /// <summary>
        /// Returns an instance of <see cref="RTSCommGameSlow"/>.
        /// </summary>
        /// <param name="commSceneItem">(OUT) <see cref="RTSCommGameSlow"/>. instance.</param>
        public static void GetNode(out RTSCommGameSlow commSceneItem)
        {
            // Try to retrieve a PoolNode, if fail, create an instance anyway!   
            PoolNode poolNode;
            commSceneItem = (_rtsCommGameSlowItems.Get(out poolNode) ? poolNode.Item : new RTSCommGameSlow()) as RTSCommGameSlow;
        }

        // 6/29/2009
        /// <summary>
        /// Returns an instance of <see cref="RTSCommGameTurn"/>.
        /// </summary>
        /// <param name="commSceneItem">(OUT) <see cref="RTSCommGameTurn"/>. instance.</param>
        public static void GetNode(out RTSCommGameTurn commSceneItem)
        {
            // Try to retrieve a PoolNode, if fail, create an instance anyway!   
            PoolNode poolNode;
            commSceneItem = (_rtsCommGameTurnItems.Get(out poolNode) ? poolNode.Item : new RTSCommGameTurn()) as RTSCommGameTurn;
        }

        // 6/29/2009
        /// <summary>
        /// Returns an instance of <see cref="RTSCommIsReady"/>.
        /// </summary>
        /// <param name="commSceneItem">(OUT) <see cref="RTSCommIsReady"/>. instance.</param>
        public static void GetNode(out RTSCommIsReady commSceneItem)
        {
            // Try to retrieve a PoolNode, if fail, create an instance anyway!   
            PoolNode poolNode;
            commSceneItem = (_rtsCommIsReadyItems.Get(out poolNode) ? poolNode.Item : new RTSCommIsReady()) as RTSCommIsReady;
        }

        // 6/29/2009
        /// <summary>
        /// Returns an instance of <see cref="RTSCommKillSceneItem"/>.
        /// </summary>
        /// <param name="commSceneItem">(OUT) <see cref="RTSCommKillSceneItem"/>. instance.</param>
        public static void GetNode(out RTSCommKillSceneItem commSceneItem)
        {
            // Try to retrieve a PoolNode, if fail, create an instance anyway!   
            PoolNode poolNode;
            commSceneItem = (_rtsCommKillSceneItems.Get(out poolNode) ? poolNode.Item : new RTSCommKillSceneItem()) as RTSCommKillSceneItem;
        }

        // 6/29/2009
        /// <summary>
        /// Returns an instance of <see cref="RTSCommLobbyData"/>.
        /// </summary>
        /// <param name="commSceneItem">(OUT) <see cref="RTSCommLobbyData"/>. instance.</param>
        public static void GetNode(out RTSCommLobbyData commSceneItem)
        {
            // Try to retrieve a PoolNode, if fail, create an instance anyway!   
            PoolNode poolNode;
            commSceneItem = (_rtsCommLobbyDataItems.Get(out poolNode) ? poolNode.Item : new RTSCommLobbyData()) as RTSCommLobbyData;
        }

        // 6/29/2009
        /// <summary>
        /// Returns an instance of <see cref="RTSCommMoveSceneItem"/>.
        /// </summary>
        /// <param name="commSceneItem">(OUT) <see cref="RTSCommMoveSceneItem"/>. instance.</param>
        public static void GetNode(out RTSCommMoveSceneItem commSceneItem)
        {
            // Try to retrieve a PoolNode, if fail, create an instance anyway!   
            PoolNode poolNode;
            commSceneItem = (_rtsCommMoveSceneItems.Get(out poolNode) ? poolNode.Item : new RTSCommMoveSceneItem()) as RTSCommMoveSceneItem;
        }

        // 6/29/2009
        /// <summary>
        /// Returns an instance of <see cref="RTSCommMoveSceneItem2"/>.
        /// </summary>
        /// <param name="commSceneItem">(OUT) <see cref="RTSCommMoveSceneItem2"/>. instance.</param>
        public static void GetNode(out RTSCommMoveSceneItem2 commSceneItem)
        {
            // Try to retrieve a PoolNode, if fail, create an instance anyway!   
            PoolNode poolNode;
            commSceneItem = (_rtsCommMoveScene2Items.Get(out poolNode) ? poolNode.Item : new RTSCommMoveSceneItem2()) as RTSCommMoveSceneItem2;
        }

        // 6/29/2009
        /// <summary>
        /// Returns an instance of <see cref="RTSCommQueueMarker"/>.
        /// </summary>
        /// <param name="commSceneItem">(OUT) <see cref="RTSCommQueueMarker"/>. instance.</param>
        public static void GetNode(out RTSCommQueueMarker commSceneItem)
        {
            // Try to retrieve a PoolNode, if fail, create an instance anyway!   
            PoolNode poolNode;
            commSceneItem = (_rtsCommQueueMarkerItems.Get(out poolNode) ? poolNode.Item : new RTSCommQueueMarker()) as RTSCommQueueMarker;
        }

        // 6/29/2009
        /// <summary>
        /// Returns an instance of <see cref="RTSCommSceneItemStance"/>.
        /// </summary>
        /// <param name="commSceneItem">(OUT) <see cref="RTSCommSceneItemStance"/>. instance.</param>
        public static void GetNode(out RTSCommSceneItemStance commSceneItem)
        {
            // Try to retrieve a PoolNode, if fail, create an instance anyway!   
            PoolNode poolNode;
            commSceneItem = (_rtsCommSceneItemStanceItems.Get(out poolNode) ? poolNode.Item : new RTSCommSceneItemStance()) as RTSCommSceneItemStance;
        }

        // 6/29/2009
        /// <summary>
        /// Returns an instance of <see cref="RTSCommStartAttackSceneItem"/>.
        /// </summary>
        /// <param name="commSceneItem">(OUT) <see cref="RTSCommStartAttackSceneItem"/>. instance.</param>
        public static void GetNode(out RTSCommStartAttackSceneItem commSceneItem)
        {
            // Try to retrieve a PoolNode, if fail, create an instance anyway!   
            PoolNode poolNode;
            commSceneItem = (_rtsCommStartAttackSceneItems.Get(out poolNode) ? poolNode.Item : new RTSCommStartAttackSceneItem()) as RTSCommStartAttackSceneItem;
        }

        // 8/3/2009
        /// <summary>
        /// Returns an instance of <see cref="RTSCommSceneItemHealth"/>.
        /// </summary>
        /// <param name="commSceneItem">(OUT) <see cref="RTSCommSceneItemHealth"/>. instance.</param>
        public static void GetNode(out RTSCommSceneItemHealth commSceneItem)
        {
            // Try to retrieve a PoolNode, if fail, create an instance anyway!   
            PoolNode poolNode;
            commSceneItem = (_rtsCommSceneItemHealthItems.Get(out poolNode) ? poolNode.Item : new RTSCommSceneItemHealth()) as RTSCommSceneItemHealth;
        }

        // 6/29/2009
        /// <summary>
        /// Returns an instance of <see cref="RTSCommSyncTime"/>.
        /// </summary>
        /// <param name="commSceneItem">(OUT) <see cref="RTSCommSyncTime"/>. instance.</param>
        public static void GetNode(out RTSCommSyncTime commSceneItem)
        {
            // Try to retrieve a PoolNode, if fail, create an instance anyway!   
            PoolNode poolNode;
            commSceneItem = (_rtsCommSyncTimeItems.Get(out poolNode) ? poolNode.Item : new RTSCommSyncTime()) as RTSCommSyncTime;
        }

        // 6/16/2010
        /// <summary>
        /// Returns an instance of <see cref="RTSCommValidator"/>.
        /// </summary>
        /// <param name="commSceneItem">(OUT) <see cref="RTSCommValidator"/>. instance.</param>
        public static void GetNode(out RTSCommValidator commSceneItem)
        {
            // Try to retrieve a PoolNode, if fail, create an instance anyway!   
            PoolNode poolNode;
            commSceneItem = (_rtsCommValidator.Get(out poolNode) ? poolNode.Item : new RTSCommValidator()) as RTSCommValidator;
        }


        #endregion

        #region Event Handlers

        // 7/27/2009
        /// <summary>
        /// <see cref="EventHandler"/> for the <see cref="BuildingScene"/> get, which sets the InUse to TRUE.
        /// </summary>
        private void BuildingSceneItemsPoolItemGetCalled(object sender, PoolEventArgs e)
        {
            _buildingSceneItems.PoolNodes[e.NodeIndex].Item.InUse = true;
        }

        // 7/27/2009
        /// <summary>
        /// <see cref="EventHandler"/> for the <see cref="BuildingScene"/> return, which sets the InUse to FALSE, 
        /// and reduces the <see cref="IFDTilePlacement"/> counter for given item.
        /// </summary>
        private void BuildingSceneItemsPoolItemReturnCalled(object sender, PoolEventArgs e)
        {
            _buildingSceneItems.PoolNodes[e.NodeIndex].Item.InUse = false;
            var reduceIFDCounter = (_buildingSceneItems.PoolNodes[e.NodeIndex].Item).ReduceIFDCounter; // 11/28/2009

            // 7/27/2009 - Call IFDTile_Placement method, to reduce the totalCount for given SceneItemOwner.
            if (reduceIFDCounter)
                IFDTilePlacement.ReduceTotalQueuedCountForBuildingType(ItemGroupType.Buildings, _playerNumberOwner);

            // 11/28/2009 - Reset to False.
            (_buildingSceneItems.PoolNodes[e.NodeIndex].Item).ReduceIFDCounter = true;
        }

        // 4/6/2009
        /// <summary>
        /// <see cref="EventHandler"/> for the <see cref="DefenseScene"/> get, which sets the InUse to TRUE.
        /// </summary>
        private void DefenseSceneItemsPoolItemGetCalled(object sender, PoolEventArgs e)
        {
            (_defenseSceneItems.PoolNodes[e.NodeIndex].Item).InUse = true;
           
            // 4/13/2009 - call sort in ForceBehaviorsManager
            ForceBehaviorsManager.SortItemsByInUseFlag = true;
        }

        // 4/6/2009
        /// <summary>
        /// <see cref="EventHandler"/> for the <see cref="DefenseScene"/> return, which sets the InUse to FALSE, 
        /// and reduces the <see cref="IFDTilePlacement"/> counter for given item.
        /// </summary>
        private void DefenseSceneItemsPoolItemReturnCalled(object sender, PoolEventArgs e)
        {
            (_defenseSceneItems.PoolNodes[e.NodeIndex].Item).InUse = false;
            var reduceIFDCounter = (_defenseSceneItems.PoolNodes[e.NodeIndex].Item).ReduceIFDCounter; // 11/28/2009
           
            // 4/13/2009 - call sort in ForceBehaviorsManager
            ForceBehaviorsManager.SortItemsByInUseFlag = true;

            // 7/27/2009 - Call IFDTile_Placement method, to reduce the totalCount for given SceneItemOwner.
            if (reduceIFDCounter)
                IFDTilePlacement.ReduceTotalQueuedCountForBuildingType(ItemGroupType.Shields, _playerNumberOwner);

            // 11/28/2009 - Reset to False.
            (_defenseSceneItems.PoolNodes[e.NodeIndex].Item).ReduceIFDCounter = true;
        }

        // 3/23/23009
        /// <summary>
        /// <see cref="EventHandler"/> for the <see cref="SciFiAircraftScene"/> get, which sets the InUse to TRUE.
        /// </summary>
        private void SciFiAircraftSceneItemsPoolItemGetCalled(object sender, PoolEventArgs e)
        {
            (_sciFiAircraftSceneItems.PoolNodes[e.NodeIndex].Item).InUse = true;
           
            // 4/13/2009 - call sort in ForceBehaviorsManager
            ForceBehaviorsManager.SortItemsByInUseFlag = true;
        }

        // 3/23/2009
        /// <summary>
        /// <see cref="EventHandler"/> for the <see cref="SciFiAircraftScene"/> return, which sets the InUse to FALSE, 
        /// and reduces the <see cref="IFDTilePlacement"/> counter for given item.
        /// </summary>
        private void SciFiAircraftSceneItemsPoolItemReturnCalled(object sender, PoolEventArgs e)
        {
            (_sciFiAircraftSceneItems.PoolNodes[e.NodeIndex].Item).InUse = false;
            var reduceIFDCounter = (_sciFiAircraftSceneItems.PoolNodes[e.NodeIndex].Item).ReduceIFDCounter; // 11/28/2009
           
            // 4/13/2009 - call sort in ForceBehaviorsManager
            ForceBehaviorsManager.SortItemsByInUseFlag = true;

            // 7/27/2009 - Call IFDTile_Placement method, to reduce the totalCount for given SceneItemOwner.
            if (reduceIFDCounter)
                IFDTilePlacement.ReduceTotalQueuedCountForBuildingType(ItemGroupType.Airplanes, _playerNumberOwner);

            // 11/28/2009 - Reset to False.
            (_sciFiAircraftSceneItems.PoolNodes[e.NodeIndex].Item).ReduceIFDCounter = true;
        }

        // 3/23/2009
        /// <summary>
        /// <see cref="EventHandler"/> for the <see cref="SciFiTankScene"/> get, which sets the InUse to TRUE.
        /// </summary>
        private void SciFiTankSceneItemsPoolItemGetCalled(object sender, PoolEventArgs e)
        {
            (_sciFiTankSceneItems.PoolNodes[e.NodeIndex].Item).InUse = true;
            
            // 4/13/2009 - call sort in ForceBehaviorsManager
            ForceBehaviorsManager.SortItemsByInUseFlag = true;
        }

        // 3/23/2009
        /// <summary>
        /// <see cref="EventHandler"/> for the <see cref="SciFiTankScene"/> return, which sets the InUse to FALSE, 
        /// and reduces the <see cref="IFDTilePlacement"/> counter for given item.
        /// </summary>
        private void SciFiTankSceneItemsPoolItemReturnCalled(object sender, PoolEventArgs e)
        {
            (_sciFiTankSceneItems.PoolNodes[e.NodeIndex].Item).InUse = false;
            var reduceIFDCounter = (_sciFiTankSceneItems.PoolNodes[e.NodeIndex].Item).ReduceIFDCounter; // 11/28/2009
            
            // 4/13/2009 - call sort in ForceBehaviorsManager
            ForceBehaviorsManager.SortItemsByInUseFlag = true;

            // 7/27/2009 - Call IFDTile_Placement method, to reduce the totalCount for given SceneItemOwner.
            if (reduceIFDCounter)
                IFDTilePlacement.ReduceTotalQueuedCountForBuildingType(ItemGroupType.Vehicles, _playerNumberOwner);

            // 11/28/2009 - Reset to False.
            (_sciFiTankSceneItems.PoolNodes[e.NodeIndex].Item).ReduceIFDCounter = true;
        }

        #endregion

        // 1/7/2010
        /// <summary>
        /// Dispose of <see cref="Pool{Default}"/> collections.
        /// </summary>
        public override void Dispose()
        {
            // Dispose of items.
            DisposePoolNodesAttributes(_buildingSceneItems);
            DisposePoolNodesAttributes(_defenseSceneItems);
            DisposePoolNodesAttributes(_ifdBuildingSceneItems);
            DisposePoolNodesAttributes(_ifdDefenseSceneItems);
            DisposePoolNodesAttributes(_projectileItems);
            DisposePoolNodesAttributes(_sciFiAircraftSceneItems);
            DisposePoolNodesAttributes(_sciFiTankSceneItems);
        }

        // 1/7/2010
        /// <summary>
        /// Generic Type Helper, which iterates through a <see cref="Pool{TDefault}"/> instance, and 
        /// calls the dispose for all <see cref="SceneItem"/> references types.
        /// </summary>
        /// <typeparam name="TU">Set Generic Type.</typeparam>
        /// <param name="pool"><see cref="Pool{TDefault}"/> instance.</param>
        public new static void DisposePoolNodesAttributes<TU>(Pool<TU> pool) where TU : class, IPoolNodeItem, IPoolNodeSceneItem, new()
        {
            // Dispose of Pool Nodes
            foreach (var node in pool.AllNodes)
            {
                var item = (TU)node.Item;

                item.InUse = false;
                item.PoolManager = null;

                // Call Dispose on SceneItem, if one exist!
                if (item.SceneItemInstance == null) continue;

                item.SceneItemInstance.Dispose(true);
                item.SceneItemInstance = null;
            } // End ForEach
        }
    }
}