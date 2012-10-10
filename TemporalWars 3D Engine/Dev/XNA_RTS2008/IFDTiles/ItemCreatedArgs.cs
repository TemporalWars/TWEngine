#region File Description
//-----------------------------------------------------------------------------
// ItemCreatedArgs.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.Players;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.IFDTiles
{
    // 11/5/2008; 12/26/2008: Add ItemGroupToAttack Enum; 4/6/209: Add 'GameTime' value.
    /// <summary>
    /// ItemCreated event args
    /// </summary>
    public class ItemCreatedArgs : EventArgs
    {
        /// <summary>
        /// <see cref="ItemGroupType"/> Enum who produced <see cref="SceneItem"/> owner
        /// </summary>
        public ItemGroupType BuildingType;
        /// <summary>
        /// <see cref="ItemGroupType"/> Enum which this <see cref="BuildingScene"/> produces
        /// </summary>
        public ItemGroupType? ProductionType;
        /// <summary>
        /// <see cref="ItemType"/> to create
        /// </summary>
        public ItemType ItemType;
        /// <summary>
        /// <see cref="ItemGroupType"/> Enum To attack for <see cref="DefenseScene"/> items.
        /// </summary>
        public ItemGroupType? ItemGroupToAttack;
        /// <summary>
        /// Where to place <see cref="SceneItem"/> owner
        /// </summary>
        public Vector3 PlaceItemAt;
        /// <summary>
        /// Building Producer's <see cref="SceneItemWithPick.NetworkItemNumber"/>
        /// </summary>
        public int BuildingProducerNetworkItemNumber;
        /// <summary>
        /// Building Producer's Pointer Ref, used in Single-Player games, since
        /// <see cref="SceneItemWithPick.NetworkItemNumber"/> is not availble.
        /// </summary>
        public BuildingScene BuildingProducer;
        /// <summary>
        /// <see cref="GameTime"/> (TotalSeconds) <see cref="SceneItem"/> owner was placed at.
        /// </summary>
        public double TotalSeconds;

        // 8/4/2009
        /// <summary>
        /// Set if <see cref="SceneItem"/> owner is to be the Bot Helper of some leader <see cref="SceneItem"/> owner
        /// </summary>
        public bool IsBotHelper;

        /// <summary>
        /// The Leader the bot will serve. 
        /// </summary>
        /// <remarks>Set to either <see cref="SceneItemWithPick.SceneItemNumber"/> for SP games, 
        /// or <see cref="SceneItemWithPick.NetworkItemNumber"/> for MP Games.</remarks>
        public int LeaderUniqueNumber;

        /// <summary>
        /// The Leader's <see cref="Player.PlayerNumber"/>.
        /// </summary>
        public int LeaderPlayerNumber; 

        /// <summary>
        /// Constructor for <see cref="ItemCreatedArgs"/>.
        /// </summary>
        /// <param name="buildingType"><see cref="ItemGroupType"/> Enum</param>
        /// <param name="productionType"><see cref="ItemGroupType"/> Enum as productin type</param>
        /// <param name="itemType"><see cref="ItemType"/> enum to create</param>
        /// <param name="itemGroupToAttack"><see cref="ItemGroupType"/> this <see cref="ItemType"/> can attack</param>
        /// <param name="placeItemAt"><see cref="Vector3"/> location to place item</param>
        /// <param name="buildingProducerNetworkItemNumber"><see cref="SceneItemWithPick.NetworkItemNumber"/> for MP Games</param>
        /// <param name="buildingProducer"><see cref="BuildingScene"/> instance reference to who produced this <see cref="ItemType"/>.</param>
        /// <param name="totalSeconds"><see cref="GameTime"/> (TotalSeconds) <see cref="SceneItem"/> owner was placed at; used for F.O.W. logic.</param>
        public ItemCreatedArgs(ItemGroupType buildingType, ItemGroupType? productionType, ItemType itemType, ItemGroupType? itemGroupToAttack,
                            Vector3 placeItemAt, int buildingProducerNetworkItemNumber, BuildingScene buildingProducer, double totalSeconds)
        {
            BuildingType = buildingType;
            ProductionType = productionType;
            ItemType = itemType;
            ItemGroupToAttack = itemGroupToAttack;
            PlaceItemAt = placeItemAt;
            BuildingProducerNetworkItemNumber = buildingProducerNetworkItemNumber;
            BuildingProducer = buildingProducer;
            TotalSeconds = totalSeconds; // 4/6/2009
        }

        /// <summary>
        /// Default empty constructor for <see cref="ItemCreatedArgs"/>.
        /// </summary>
        public ItemCreatedArgs()
        {
            return;
        }
         
    }
}
