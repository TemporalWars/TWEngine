#region File Description
//-----------------------------------------------------------------------------
// CreateItemToPlace.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.IFDTiles.Delegates
{
    // 7/24/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.IFDTiles.Delegates"/> namespace contains the classes
    /// which make up the entire <see cref="IFDTiles.Delegates"/>.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
       

    /// <summary>
    /// Delegate used to place a created item into the game world.
    /// </summary>
    /// <param name="buildingType"><see cref="ItemGroupType"/> Enum of the created item</param>
    /// <param name="productionType">(Optional) <see cref="ItemGroupType"/> Enum of the production type created from this item</param>
    /// <param name="itemType"><see cref="ItemType"/> Enum to create</param>
    /// <param name="itemGroupToAttack">(Optional) <see cref="ItemGroupType"/> Enum which can be attacked by this item</param>
    /// <param name="placeItemAt">The <see cref="Vector3"/> location to place the item at</param>
    /// <param name="theIFDTile">The <see cref="IFDTile"/> instance which requested creation</param>
    public delegate void CreateItemToPlace(ItemGroupType buildingType, ItemGroupType? productionType, ItemType itemType, 
                                           ItemGroupType? itemGroupToAttack, ref Vector3 placeItemAt, IFDTile theIFDTile);
}