#region File Description
//-----------------------------------------------------------------------------
// ScenaryItemSceneLoader.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImageNexus.BenScharbach.TWEngine.ScenaryDataLoader.Enums;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.ScenaryDataLoader.Loaders
{
    // 10/11/2012
    /// <summary>
    /// 
    /// </summary>
    public class ScenaryItemSceneLoader
    {
        // 4/14/2009: Overload version, used when loading multiple instances of same ItemType.
        /// <summary>
        /// Creates a <see cref="ScenaryItemSceneLoader"/>, for example bushes and rocks.
        /// </summary>
        /// <param name="inScenaryItems">A collection of <see cref="inScenaryItems"/>.</param>  
        /// <param name="itemType">The <see cref="itemType"/> to be used.</param> 
        /// <param name="playerNumber">The unique Player's network number.</param>     
        public ScenaryItemSceneLoader(ItemType itemType, IEnumerable<ScenaryItemDataLoader> inScenaryItems, byte playerNumber)
        {
            ItemType = itemType;
            PlayerNumber = playerNumber;

            // Copy List
            ScenaryItemData.AddRange(inScenaryItems);
        }

        public ItemType ItemType;
        public List<ScenaryItemDataLoader> ScenaryItemData = new List<ScenaryItemDataLoader>();
        public byte PlayerNumber;
    }
}
