#region File Description
//-----------------------------------------------------------------------------
// PlayerStats.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System.Collections.Generic;
using TWEngine.InstancedModels.Enums;
using TWEngine.SceneItems;

namespace TWEngine.Players
{
    ///<summary>
    /// The <see cref="PlayerStats"/> class is used to track player stats, like number of enemy units killed or
    /// number of items created.
    ///</summary>
    public class PlayerStats
    {
        // 10/4/2009 - Player Owner of these stats.
        private readonly Player _playerOwner;

        // 10/3/2009 - ItemType Enemy-Kill stats Dictionary, used to track the enemy kills per enum 'ItemType'.
        //             Key = (int) of ItemType, and value is the count.
        private readonly Dictionary<int, int> _itemTypeEnemyKillStats = new Dictionary<int, int>();

        // 10/3/2009 - ItemType Create stats Dictionary, used to track the creation per enum 'ItemType'.
        //             Key = (int) of ItemType, and value is the count.
        private readonly Dictionary<int, int> _itemTypeCreateStats = new Dictionary<int, int>();

        // 10/5/2009 - ItemType Destroyed stats Dictionary, used to track our item's Destroyed per enum 'ItemType'.
        //             Key = (int) of ItemType, and value is the count.
        private readonly Dictionary<int, int> _itemTypeDestroyedStats = new Dictionary<int, int>();

        #region Properties

        // 10/3/2009
        /// <summary>
        /// The number of enemy buildings destroyed.
        /// </summary>
        public int EnemyBuildingsDestroyed { get; set; }

        // 10/3/2009
        /// <summary>
        /// The number of enemy units destroyed.
        /// </summary>
        public int EnemyUnitsDestroyed { get; set; }

        // 10/5/2009
        /// <summary>
        /// The number of buildings destroyed; lost to enemy fire.
        /// </summary>
        public int BuildingsDestroyed { get; set; }

        // 10/5/2009
        /// <summary>
        /// The number of units destroyed; lost to enemy fire.
        /// </summary>
        public int UnitsDestroyed { get; set; }

        // 10/4/2009
        /// <summary>
        /// The number of buildings created.
        /// </summary>
        public int BuildingsCreated { get; set; }

        // 10/4/2009
        /// <summary>
        /// The number of units created.
        /// </summary>
        public int UnitsCreated { get; set; }

        #endregion


        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="playerOwner"><see cref="Player"/> instance who is owner</param>
        public PlayerStats(Player playerOwner)
        {
            // store Player's Owner to this PlayerStats instance.
            _playerOwner = playerOwner;
        }

        // 10/3/2009
        /// <summary>
        /// Called when a <see cref="Player"/> killed some <see cref="SceneItem"/>, which in turn updates
        /// the internal stats for either 'EnemyBuildingDestroyed' or 'EnemyUnitsDestroyed'.
        /// </summary>
        /// <param name="playerStats">This instance of <see cref="PlayerStats"/></param>
        /// <param name="itemKilled"><see cref="SceneItem"/> instance as killed item</param>
        public static void UpdatePlayersKillStats(PlayerStats playerStats, SceneItem itemKilled)
        {
            // If Buildingitem, increase Building, else it is a unit.
            if (itemKilled is BuildingScene)
            {
                playerStats.EnemyBuildingsDestroyed++;
            }
            else
            {
                playerStats.EnemyUnitsDestroyed++;
            }

            // Now update the individual 'ItemType' enum stats
            var itemType = (int)itemKilled.ShapeItem.ItemType;
            if (playerStats._itemTypeEnemyKillStats.ContainsKey(itemType))
            {
                // yes, so increase count
                playerStats._itemTypeEnemyKillStats[itemType]++;
            }
            else
            {
                // no, so add 'ItemType' to Dictionary
                playerStats._itemTypeEnemyKillStats.Add(itemType, 1);
            }
        }

        // 10/3/2009
        /// <summary>
        /// Called when a <see cref="Player"/> created some <see cref="SceneItem"/>.
        /// </summary>
        /// <param name="playerStats">This instance of <see cref="PlayerStats"/></param>
        /// <param name="itemCreated"><see cref="SceneItem"/> instance created</param>
        public static void UpdatePlayersCreateStats(PlayerStats playerStats, SceneItem itemCreated)
        {
            // If Buildingitem, increase Building, else it is a unit.
            if (itemCreated is BuildingScene)
            {
                playerStats.BuildingsCreated++;
            }
            else
            {
                playerStats.UnitsCreated++;
            }

            // Now update the individual 'ItemType' enum stats
            var itemType = (int)itemCreated.ShapeItem.ItemType;
            if (playerStats._itemTypeCreateStats.ContainsKey(itemType))
            {
                // yes, so increase count
                playerStats._itemTypeCreateStats[itemType]++;
            }
            else
            {
                // no, so add 'ItemType' to Dictionary
                playerStats._itemTypeCreateStats.Add(itemType, 1);
            }
        }

        // 10/5/2009
        /// <summary>
        /// Called when a <see cref="Player"/> lost some <see cref="SceneItem"/>.
        /// </summary>
        /// <param name="playerStats">This instance of <see cref="PlayerStats"/></param>
        /// <param name="itemDestroyed"><see cref="SceneItem"/> instance destroyed</param>
        public static void UpdatePlayersDestroyedStats(PlayerStats playerStats, SceneItem itemDestroyed)
        {
            // If Buildingitem, increase Building, else it is a unit.
            if (itemDestroyed is BuildingScene)
            {
                playerStats.BuildingsDestroyed++;
            }
            else
            {
                playerStats.UnitsDestroyed++;
            }

            // Now update the individual 'ItemType' enum stats
            var itemType = (int)itemDestroyed.ShapeItem.ItemType;
            if (playerStats._itemTypeDestroyedStats.ContainsKey(itemType))
            {
                // yes, so increase count
                playerStats._itemTypeDestroyedStats[itemType]++;
            }
            else
            {
                // no, so add 'ItemType' to Dictionary
                playerStats._itemTypeDestroyedStats.Add(itemType, 1);
            }
        }

        // 10/3/2009
        /// <summary>
        /// For the given <see cref="ItemType"/> Enum, will return the given 'KillCount' stat.
        /// </summary>
        /// <param name="playerStats">This instance of <see cref="PlayerStats"/></param>
        /// <param name="itemTypeToRetrieve"><see cref="ItemType"/> to retrieve</param>
        /// <returns>Kill count</returns>
        public static int GetItemTypeKillStats(PlayerStats playerStats, ItemType itemTypeToRetrieve)
        {
            // check internal Dictionary
            int itemTypeKillCount;
            return playerStats._itemTypeEnemyKillStats.TryGetValue((int)itemTypeToRetrieve, out itemTypeKillCount) ? itemTypeKillCount : 0;
        }

        // 10/4/2009
        /// <summary>
        /// For the given <see cref="ItemType"/> Enum, will return the given 'CreateCount' stat.
        /// </summary>
        /// <param name="playerStats">This instance of <see cref="PlayerStats"/></param>
        /// <param name="itemTypeToRetrieve"><see cref="ItemType"/> to retrieve</param>
        /// <returns>Create count</returns>
        public static int GetItemTypeCreateStats(PlayerStats playerStats, ItemType itemTypeToRetrieve)
        {
            // check internal Dictionary
            int itemTypeCreateCount;
            return playerStats._itemTypeCreateStats.TryGetValue((int)itemTypeToRetrieve, out itemTypeCreateCount) ? itemTypeCreateCount : 0;
        }

        // 10/5/2009
        /// <summary>
        /// For the given <see cref="ItemType"/> Enum, will return the given 'DestroyedCount' stat.
        /// </summary>
        /// <param name="playerStats">This instance of <see cref="PlayerStats"/></param>
        /// <param name="itemTypeToRetrieve"><see cref="ItemType"/> to retrieve</param>
        /// <returns>Create count</returns>
        public static int GetItemTypeDestroyedStats(PlayerStats playerStats, ItemType itemTypeToRetrieve)
        {
            // check internal Dictionary
            int itemTypeDestroyedCount;
            return playerStats._itemTypeDestroyedStats.TryGetValue((int)itemTypeToRetrieve, out itemTypeDestroyedCount) ? itemTypeDestroyedCount : 0;
        }


    }
}