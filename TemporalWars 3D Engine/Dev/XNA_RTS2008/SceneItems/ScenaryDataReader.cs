#region File Description
//-----------------------------------------------------------------------------
// ScenaryDataReader.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.GameScreens.Generic;
using ImageNexus.BenScharbach.TWEngine.InstancedModels;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.SceneItems.Structs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ImageNexus.BenScharbach.TWEngine.SceneItems
{
    /// <summary>
    /// Content pipeline support class for loading <see cref="ScenaryItemData"/>.
    /// </summary>
    public class ScenaryDataReader : ContentTypeReader<List<ScenaryItemScene>>
    {
        // 4/9/2009
        private static List<ScenaryItemData> _tmpItemProperties;
        private List<ScenaryItemScene> _scenaryItems;
        private static Vector3 _tmpZero = Vector3.Zero;
        private static Quaternion _tmpZeroQuat = Quaternion.Identity;
        private int _arrayLength;

        /// <summary>
        /// Reads <see cref="ScenaryItemData"/> from an XNB file.
        /// </summary>
        /// <param name="input">Instance of <see cref="ContentReader"/>.</param>
        /// <param name="existingInstance">A collection of <see cref="ScenaryItemScene"/>.</param>
        /// <returns>A collection of <see cref="ScenaryItemScene"/>.</returns>
        protected sealed override List<ScenaryItemScene> Read(ContentReader input,
                                               List<ScenaryItemScene> existingInstance)
        {
            LoadingScreen.LoadingMessage = "Loading Scenary Items";

            // Init Array
            _scenaryItems = new List<ScenaryItemScene>(_arrayLength);

            // Read File data first.
            // Read Count of List of ItemTypes           
            _arrayLength = input.ReadInt32();

            // 4/18/2009
            if (_arrayLength == 0)
                return _scenaryItems;

            // 4/9/2009            
            _tmpItemProperties = new List<ScenaryItemData>(_arrayLength);

            // 1st - Load Values back into memory.            
            for (var i = 0; i < _arrayLength; i++)
            {
                // 10/6/2009: Note: Fields are saved in the 'Storage' classes 'DoSaveScenaryItemData' method.
                var tmpScenaryDataProperties = new ScenaryItemData(ref _tmpZero, ref _tmpZeroQuat)
                                                   {
                                                       instancedItemData = {ItemType = (ItemType) input.ReadInt32()},
                                                       rotation = input.ReadQuaternion(),
                                                       position = input.ReadVector3(),
                                                       pathBlockSize = input.ReadInt32(),
                                                       isPathBlocked = input.ReadBoolean(),
                                                       name = input.ReadString(), // 10/6/2009 
                                                       
                                                   };
                // Add to List
                _tmpItemProperties.Add(tmpScenaryDataProperties);
            }

            LoadingScreen.LoadingMessage = "Creating Scenary Items";

            // 1st - Sort by ItemType
            _tmpItemProperties.Sort(CompareByItemType);

            // 2nd - iterate list and create each group of ScenaryItems by ItemType
            var scenaryItemGroup = new List<ScenaryItemData>();
            var oldItemType = _tmpItemProperties[0].instancedItemData.ItemType; // set to 1st record.            
            var count = _tmpItemProperties.Count; // 8/18/2009
            var gameInstance = TemporalWars3DEngine.GameInstance; // 4/26/2010
            for (var i = 0; i < count; i++)
            {
                // if ItemType same as last record, then add to current group.
                var itemProperty = _tmpItemProperties[i]; // 8/18/2009
                if (itemProperty.instancedItemData.ItemType == oldItemType)
                {
                    scenaryItemGroup.Add(itemProperty);
                }
                else
                {
                    // group retrieved, so create sceneItems
                    var scenaryItem = new ScenaryItemScene(gameInstance, oldItemType, scenaryItemGroup, 0);
                    // add to ScenaryItems List
                    _scenaryItems.Add(scenaryItem);
                    // clear group.
                    scenaryItemGroup.Clear();

                    // add new record to next group
                    scenaryItemGroup.Add(itemProperty);
                }

                oldItemType = itemProperty.instancedItemData.ItemType;
            }

            // 7/11/2009 - Check 'ScenaryItemsGroup' Count, to see if one last group to add.
            var count1 = scenaryItemGroup.Count; // 8/18/2009
            if (count1 > 0)
            {
                // group retrieved, so create sceneItems
                var scenaryItem = new ScenaryItemScene(gameInstance, oldItemType, scenaryItemGroup, 0);
                // add to ScenaryItems List
                _scenaryItems.Add(scenaryItem);
                // clear group.
                scenaryItemGroup.Clear();
            }

            // 1/7/2010 - Trigger ItemsLoader Thread to start.
            InstancedItemLoader.PreLoadInstanceItemsMethod();

            return _scenaryItems;
        }

        /// <summary>
        /// Predicate method used for the <see cref="List{t}.Sort()"/> method.  This will sort all 
        /// items by the <see cref="ItemType"/> enum.
        /// </summary>
        /// <param name="item1"><see cref="ScenaryItemData"/> structure</param>
        /// <param name="item2"><see cref="ScenaryItemData"/> structure</param>
        /// <returns>Sort value of -1, 0, or 1.</returns>
        private static int CompareByItemType(ScenaryItemData item1, ScenaryItemData item2)
        {
            // item2 greater
            if (item1.instancedItemData.ItemType < item2.instancedItemData.ItemType)
                return -1;

            // item1 greater
            if (item1.instancedItemData.ItemType > item2.instancedItemData.ItemType)
                return 1;

            // items equal
            if (item1.instancedItemData.ItemType == item2.instancedItemData.ItemType)
                return 0;

            return 0;
        }

    }
}
