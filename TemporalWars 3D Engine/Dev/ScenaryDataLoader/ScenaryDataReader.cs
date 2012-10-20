#region File Description
//-----------------------------------------------------------------------------
// ScenaryDataReader.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using ImageNexus.BenScharbach.TWEngine.ScenaryDataLoader.Enums;
using ImageNexus.BenScharbach.TWEngine.ScenaryDataLoader.Loaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ImageNexus.BenScharbach.TWEngine.ScenaryDataLoader
{
    /// <summary>
    /// Content pipeline support class for loading <see cref="ScenaryItemDataLoader"/>.
    /// </summary>
    public class ScenaryDataReader : ContentTypeReader<List<ScenaryItemSceneLoader>>
    {
        // 4/9/2009
        private static List<ScenaryItemDataLoader> _tmpItemProperties;
        private List<ScenaryItemSceneLoader> _scenaryItems;
        private static Vector3 _tmpZero = Vector3.Zero;
        private static Quaternion _tmpZeroQuat = Quaternion.Identity;
        private int _arrayLength;

        #region Delegates

        /// <summary>
        /// Delegate which passes some message to display.
        /// </summary>
        public static Action<string> DisplayMessage;

        #endregion

        /// <summary>
        /// Reads <see cref="ScenaryItemDataLoader"/> from an XNB file.
        /// </summary>
        /// <param name="input">Instance of <see cref="ContentReader"/>.</param>
        /// <param name="existingInstance">A collection of <see cref="ScenaryItemSceneLoader"/>.</param>
        /// <returns>A collection of <see cref="ScenaryItemSceneLoader"/>.</returns>
        protected sealed override List<ScenaryItemSceneLoader> Read(ContentReader input,
                                               List<ScenaryItemSceneLoader> existingInstance)
        {
            // Todo
            //LoadingScreen.LoadingMessage = "Loading Scenary Items";
            if (DisplayMessage != null)
                DisplayMessage("Loading Scenary Items");

            // Init Array
            _scenaryItems = new List<ScenaryItemSceneLoader>(_arrayLength);

            // Read File data first.
            // Read Count of List of ItemTypes           
            _arrayLength = input.ReadInt32();

            // 4/18/2009
            if (_arrayLength == 0)
                return _scenaryItems;

            // 4/9/2009            
            _tmpItemProperties = new List<ScenaryItemDataLoader>(_arrayLength);

            // 1st - Load Values back into memory.            
            for (var i = 0; i < _arrayLength; i++)
            {
                // 10/6/2009: Note: Fields are saved in the 'Storage' classes 'DoSaveScenaryItemData' method.
                var tmpScenaryDataProperties = new ScenaryItemDataLoader(ref _tmpZero, ref _tmpZeroQuat)
                                                   {
                                                       ItemType = (ItemType) input.ReadInt32(),
                                                       Rotation = input.ReadQuaternion(),
                                                       Position = input.ReadVector3(),
                                                       PathBlockSize = input.ReadInt32(),
                                                       IsPathBlocked = input.ReadBoolean(),
                                                       Name = input.ReadString(),
                                                   };
                // Add to List
                _tmpItemProperties.Add(tmpScenaryDataProperties);
            }

            // Todo
            //LoadingScreen.LoadingMessage = "Creating Scenary Items";
            if (DisplayMessage != null)
                DisplayMessage("Creating Scenary Items");

            // 1st - Sort by ItemType
            _tmpItemProperties.Sort(CompareByItemType);

            // 2nd - iterate list and create each group of ScenaryItems by ItemType
            var scenaryItemGroup = new List<ScenaryItemDataLoader>();
            var oldItemType = _tmpItemProperties[0].ItemType; // set to 1st record.            
            var count = _tmpItemProperties.Count; // 8/18/2009
           
            for (var i = 0; i < count; i++)
            {
                // if ItemType same as last record, then add to current group.
                var itemProperty = _tmpItemProperties[i]; // 8/18/2009
                if (itemProperty.ItemType == oldItemType)
                {
                    scenaryItemGroup.Add(itemProperty);
                }
                else
                {
                    // group retrieved, so create sceneItems
                    var scenaryItem = new ScenaryItemSceneLoader(oldItemType, scenaryItemGroup, 0);
                    // add to ScenaryItems List
                    _scenaryItems.Add(scenaryItem);
                    // clear group.
                    scenaryItemGroup.Clear();

                    // add new record to next group
                    scenaryItemGroup.Add(itemProperty);
                }

                oldItemType = itemProperty.ItemType;
            }

            // 7/11/2009 - Check 'ScenaryItemsGroup' Count, to see if one last group to add.
            var count1 = scenaryItemGroup.Count; // 8/18/2009
            if (count1 > 0)
            {
                // group retrieved, so create sceneItems
                var scenaryItem = new ScenaryItemSceneLoader(oldItemType, scenaryItemGroup, 0);
                // add to ScenaryItems List
                _scenaryItems.Add(scenaryItem);
                // clear group.
                scenaryItemGroup.Clear();
            }

            return _scenaryItems;
        }

        /// <summary>
        /// Predicate method used for the <see cref="List{T}.Sort()" /> all items by the <see cref="ItemType"/> enum.
        /// </summary>
        /// <param name="item1"><see cref="ScenaryItemDataLoader"/> structure</param>
        /// <param name="item2"><see cref="ScenaryItemDataLoader"/> structure</param>
        /// <returns>Sort value of -1, 0, or 1.</returns>
        private static int CompareByItemType(ScenaryItemDataLoader item1, ScenaryItemDataLoader item2)
        {
            // item2 greater
            if (item1.ItemType < item2.ItemType)
                return -1;

            // item1 greater
            if (item1.ItemType > item2.ItemType)
                return 1;

            // items equal
            if (item1.ItemType == item2.ItemType)
                return 0;

            return 0;
        }

    }
}
