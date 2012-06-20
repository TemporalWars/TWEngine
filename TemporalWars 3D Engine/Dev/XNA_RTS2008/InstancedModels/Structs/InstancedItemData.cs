#region File Description
//-----------------------------------------------------------------------------
// InstancedItemData.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using TWEngine.InstancedModels.Enums;

namespace TWEngine.InstancedModels.Structs
{
    // 1/29/2009
    /// <summary>
    /// Stores the InstancedItem Data, like ItemType, ItemGroup, and InstanceKey,
    /// for example, which is needed when calling up any of the InstancedItem class methods.
    /// </summary>
    public struct InstancedItemData
    {
        ///<summary>
        /// Constructor for the <see cref="InstancedItemData"/> structure, which
        /// sets the given <see cref="ItemType"/>.
        ///</summary>
        ///<param name="itemType"><see cref="ItemType"/> Enum to use</param>
        public InstancedItemData(ItemType itemType) : this()
        {
            ItemType = itemType;
            ItemGroupType = ItemGroupType.Vehicles;
            ItemInstanceKey = 0;
            DeleteItem = false; // 7/2/2009
        }

        // 6/7/2012
        /// <summary>
        /// Set to tell TerrainQuadTree to update the ScenaryItems connection to the quad.
        /// </summary>
        public bool PositionUpdated;

        ///<summary>
        /// <see cref="ItemType"/> Enum to use
        ///</summary>
        public ItemType ItemType;
        ///<summary>
        /// <see cref="ItemGroupType"/> Enum group
        ///</summary>
        public ItemGroupType ItemGroupType; 
        ///<summary>
        /// <see cref="InstancedItem"/> unique key.
        ///</summary>
        public int ItemInstanceKey;

        // 6/7/2012
        /// <summary>
        /// The TerrainQuad patch this item is attached to.
        /// </summary>
        public int QuadKey;

        // 7/2/2009
        /// <summary>
        /// Set to delete item.
        /// </summary>
        internal bool DeleteItem;
    }
}


