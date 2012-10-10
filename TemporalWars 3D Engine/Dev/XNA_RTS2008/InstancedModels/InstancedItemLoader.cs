#region File Description
//-----------------------------------------------------------------------------
// InstancedItemLoader.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.ItemTypeAttributes;
using ImageNexus.BenScharbach.TWEngine.ItemTypeAttributes.Structs;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWEngine.InstancedModels
{
    /// <summary>
    /// The <see cref="InstancedItemLoader"/> class handles the responbility for
    /// loading the <see cref="InstancedModel"/> assets into memory.
    /// </summary>
    public static class InstancedItemLoader
    {
        // 11/7/2009 - PreLoadItem struct.
        /// <summary>
        /// The <see cref="PreLoadItem"/> structure is specifically used for the
        /// internal Queue of items to load into memory.
        /// </summary>
        public struct PreLoadItem
        {
            /// <summary>
            /// The <see cref="ItemType"/> Enum to load.
            /// </summary>
            public ItemType ItemTypeToLoad;
            /// <summary>
            /// The <see cref="SceneItem"/> owner.
            /// </summary>
            public SceneItem SceneItemOwner; // used to callback for further setup, after model loaded.
            /// <summary>
            /// Tracks number of attempts to reload the artwork.
            /// </summary>
            public int FailedAttempts; // 3/23/2010 - Tracks number of attempts to reload the artwork.

            // 6/3/2012
            /// <summary>
            /// Set when several attempts to load a ZippedContent item failed, and another attempt will be made using the NonZip method.
            /// </summary>
            public bool TryScenaryNonZipLoad; 
        }

        private static Game _gameInstance;

        // 5/29/2009 - Thread method for Pre-Load of some InstanceItem Models, 
        //              for example, Buildings and Tanks.
        private static readonly Queue<PreLoadItem> PreLoadInstanceItemsQueue = new Queue<PreLoadItem>();
        private static List<bool> _preLoadInstanceItemComplete;

        // 1/7/2010 - List of ScenaryItems loaded; used to unload during level reloads, while keeping the playable items intact!
// ReSharper disable InconsistentNaming
        private static readonly List<ItemType> _scenaryItemsLoaded = new List<ItemType>();
        // ReSharper restore InconsistentNaming

        #region Properties

        // 6/3/2012
        /// <summary>
        /// Gets or sets to do the preloading of the R.T.S. playable items into memory at the start
        /// of the game engine.
        /// </summary>
        public static bool DoPreloadPlayableRtsItems { get;  set; }

        // 1/6/2010 - PreLoad Playable items completed.
        public static bool PreloadItemsCompleted { get; set; }

        // 1/7/2010 - 
        /// <summary>
        /// List of <see cref="ItemType"/>, specifically of the <see cref="ScenaryItemScene"/> type, which were loaded during the current level.
        /// </summary>
        public static List<ItemType> ScenaryItemsLoaded
        {
            get { return _scenaryItemsLoaded; }
        }

        #endregion

        // 11/20/2009 - Static Constructor
        /// <summary>
        /// Constructor for the <see cref="InstancedItemLoader"/>, which creates
        /// the internal List and Queue structures.
        /// </summary>
        static InstancedItemLoader()
        {
            // 11/20/2009
            InitializePreloadCompleteArray();
        }

        // 6/28/2012
        /// <summary>
        /// To save time during game play, some of the <see cref="InstancedModel"/> items are preloaded during game bootup by
        /// calling this method.  
        /// </summary>
        internal static void PreLoadSomeInstanceItems(Game game, List<ItemType> playableItemsToLoad)
        {
            // 6/3/2012 - Skip loading RTS items if not set to TRUE.
            if (!DoPreloadPlayableRtsItems)
                return;

            // 6/28/2012
            if (playableItemsToLoad == null)
                throw new ArgumentNullException("playableItemsToLoad");

            // 6/28/2012
            if (playableItemsToLoad.Count == 0)
                throw new ArgumentOutOfRangeException("playableItemsToLoad", "Count must be greater than 0!");

            // Set Game Instance
            _gameInstance = game;

            // 6/28/2012 - 
            var count = playableItemsToLoad.Count;
            for (var i = 0; i < count; i++)
            {
                // retrieve item
                var itemTypeToLoad = playableItemsToLoad[i];
                
                // Add to preload queue
                PreLoadPlayableInstancedItem(itemTypeToLoad);
            }

            // Flag-Marker
            PreLoadPlayableInstancedItem(ItemType.flagMarker);

            // 11/7/2009 - Start Thread
            PreLoadInstanceItemsMethod(); // 1/7/2010

            // 1/6/2010 - Mark Playable Items loaded.
            PreloadItemsCompleted = true;

        }

        /// <summary>
        /// To save time during game play, some of the <see cref="InstancedModel"/> items are preloaded during game bootup by
        /// calling this method.  
        /// </summary>
        internal static void PreLoadSomeInstanceItems(Game game)
        {
            // 6/3/2012 - Skip loading RTS items if not set to TRUE.
            if (!DoPreloadPlayableRtsItems)
                return;

            // Set Game Instance
            _gameInstance = game;

            // Pre-Load Tank Models
            PreLoadPlayableInstancedItem(ItemType.sciFiTank01);
            PreLoadPlayableInstancedItem(ItemType.sciFiTank02);
            PreLoadPlayableInstancedItem(ItemType.sciFiTank03);
            PreLoadPlayableInstancedItem(ItemType.sciFiTank04);
            PreLoadPlayableInstancedItem(ItemType.sciFiTank05);
            PreLoadPlayableInstancedItem(ItemType.sciFiTank06);
            PreLoadPlayableInstancedItem(ItemType.sciFiTank07);
            PreLoadPlayableInstancedItem(ItemType.sciFiTank08);
            PreLoadPlayableInstancedItem(ItemType.sciFiTank09);
            PreLoadPlayableInstancedItem(ItemType.sciFiTank10);
            PreLoadPlayableInstancedItem(ItemType.sciFiTank11);
            PreLoadPlayableInstancedItem(ItemType.sciFiJeep01);
            PreLoadPlayableInstancedItem(ItemType.sciFiJeep03);

            // Pre-Load SciFi-Defense Models
            PreLoadPlayableInstancedItem(ItemType.sciFiAAGun01);
            PreLoadPlayableInstancedItem(ItemType.sciFiAAGun02);
            PreLoadPlayableInstancedItem(ItemType.sciFiAAGun04);
            PreLoadPlayableInstancedItem(ItemType.sciFiAAGun05);

            // Pre-Load SciFi-Aircraft Models
            PreLoadPlayableInstancedItem(ItemType.sciFiHeli01);
            PreLoadPlayableInstancedItem(ItemType.sciFiHeli02);
            PreLoadPlayableInstancedItem(ItemType.sciFiBomber01);
            PreLoadPlayableInstancedItem(ItemType.sciFiBomber06);
            PreLoadPlayableInstancedItem(ItemType.sciFiBomber07);
            PreLoadPlayableInstancedItem(ItemType.sciFiGunShip01);

            //
            // Pre-Load Buildings
            //
            //PreLoadPlayableInstancedItem(ItemType.sciFiBldb03);
            //PreLoadPlayableInstancedItem(ItemType.sciFiBldb06);
            //PreLoadPlayableInstancedItem(ItemType.sciFiBldb08);
            // Side-1
            {
                PreLoadPlayableInstancedItem(ItemType.sciFiBldb11); // war factory
                PreLoadPlayableInstancedItem(ItemType.sciFiBldb09); // Power Structure
                PreLoadPlayableInstancedItem(ItemType.sciFiBldb12); // Supply Depot
                PreLoadPlayableInstancedItem(ItemType.sciFiBldb13); // Airport
                PreLoadPlayableInstancedItem(ItemType.sciFiBldb02); // Technology Building
                PreLoadPlayableInstancedItem(ItemType.sciFiBldb15); // HQ
            }

            // Side-2
            {
                PreLoadPlayableInstancedItem(ItemType.sciFiBldb01); // war factory
                PreLoadPlayableInstancedItem(ItemType.sciFiBldb05); // Power Structure
                PreLoadPlayableInstancedItem(ItemType.sciFiBldb07); // Supply Depot
                PreLoadPlayableInstancedItem(ItemType.sciFiBldb10); // Airport
                PreLoadPlayableInstancedItem(ItemType.sciFiBldb04); // Technology Building
                PreLoadPlayableInstancedItem(ItemType.sciFiBldb14); // HQ
            }

            // Flag-Marker
            PreLoadPlayableInstancedItem(ItemType.flagMarker);

            // 11/7/2009 - Start Thread
            //PreLoadInstanceItemsStart.Set();
            PreLoadInstanceItemsMethod(); // 1/7/2010

            // 1/6/2010 - Mark Playable Items loaded.
            PreloadItemsCompleted = true;

        }

        /// <summary>
        /// Adds the playable <see cref="ItemType"/>, like a tank for example, to the internal Queue for loading.
        /// </summary>
        /// <param name="itemTypeToLoad"><see cref="ItemType"/> Enum</param>
        internal static void PreLoadPlayableInstancedItem(ItemType itemTypeToLoad)
        {
            // 11/20/2009
            if (_preLoadInstanceItemComplete == null)
                InitializePreloadCompleteArray();

            // Skip already preLoaded items!
// ReSharper disable PossibleNullReferenceException
            if (!_preLoadInstanceItemComplete[(int)itemTypeToLoad])
// ReSharper restore PossibleNullReferenceException
            {
                // 11/7/2009 - Create the PreLoadItem struct.
                var preLoadItem = new PreLoadItem
                                      {
                                          ItemTypeToLoad = itemTypeToLoad,
                                          SceneItemOwner = null
                                      };

                // Enqueue new request
                PreLoadInstanceItemsQueue.Enqueue(preLoadItem);
               
            }
            
        }

        /// <summary>
        /// Clears out some of the arrays for level reloading.
        /// </summary>
        internal static void ClearForLevelReload()
        {
            // 1/7/2010 - Iterate ScenaryItemsLoaded list, and mark 'False' for all Scenary items loaded.
            if (_preLoadInstanceItemComplete == null) return;

            var scenaryItemsLoaded = ScenaryItemsLoaded; // 5/24/2010 - Cache
            var count = scenaryItemsLoaded.Count; // 5/24/2010 - Cache
            for (var i = 0; i < count; i++)
            {
                var itemTypeLoaded = scenaryItemsLoaded[i];

                // Mark 'False' in the PreLoad list.
                _preLoadInstanceItemComplete[(int)itemTypeLoaded] = false;
            }
        }

        /// <summary>
        /// Adds the scenary <see cref="ItemType"/>, like a tree for example, to the internal queue for loading.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sceneItem"/> given is null.</exception>
        /// <param name="itemTypeToLoad"><see cref="ItemType"/> to preLoad</param>
        /// <param name="sceneItem"><see cref="SceneItem"/> used to call the initialize routines for current item.</param>
        internal static void PreLoadScenaryInstancedItem(ItemType itemTypeToLoad, SceneItem sceneItem)
        {
            // 11/20/2009 - Make sure 'SceneItemOwner' is Not NULL.
            if (sceneItem == null)
                throw new ArgumentNullException("sceneItem", @"SceneItem cannot be NULL!");

            // 11/20/2009
            if (_preLoadInstanceItemComplete == null)
                InitializePreloadCompleteArray();

            // Skip already preLoaded items!
// ReSharper disable PossibleNullReferenceException
            if (!_preLoadInstanceItemComplete[(int)itemTypeToLoad])
// ReSharper restore PossibleNullReferenceException
            {
                // 11/7/2009 - Create the PreLoadItem struct.
                var preLoadItem = new PreLoadItem
                                      {
                                          ItemTypeToLoad = itemTypeToLoad,
                                          SceneItemOwner = sceneItem
                                      };

                // Enqueue new request
                PreLoadInstanceItemsQueue.Enqueue(preLoadItem);

                // 1/7/2010 - Track ScenaryItems loaded.
                ScenaryItemsLoaded.Add(itemTypeToLoad);

                return;
            }

            // 11/20/2009 - Then just call the WorldTransforms setup method!
            // Callback the Initialize WorldTransforms method for the current model.
           
            // cast to ScenaryItem
            var scenaryItemScene = (sceneItem as ScenaryItemScene);
            if (scenaryItemScene != null) 
                scenaryItemScene.InitializeScenaryItemsWorldTransforms();
           
        }

        /// <summary>
        /// Processes the current queue, loading all queued items into memory.
        /// </summary>
        internal static void PreLoadInstanceItemsMethod()
        {
            // 11/7/2009 - Check if any of the init arrays need to be created.
            if (!InstancedItem.InitInstanceArrayListsCompleted)
            {
                // Init Static Arrays                
                InstancedItem.InitializeStaticArrays();
            }

            // 12/12/2009 - Fix, since sometimes this can be null!
            if (_gameInstance == null)
                _gameInstance = TemporalWars3DEngine.GameInstance;

            // 3/26/2011
            if (InstancedItem.PlayableItemsContentManager == null)
                InstancedItem.PlayableItemsContentManager = new ContentManager(_gameInstance.Services);

            if (InstancedItem.ScenaryItemsContentManager == null)
                InstancedItem.ScenaryItemsContentManager = new ContentManager(_gameInstance.Services, InstancedItem.ContentscenaryXzb);
           

            // Check if Playable Queue has SceneItemOwner
            while (PreLoadInstanceItemsQueue.Count > 0)
            {
                var preLoadItem = PreLoadInstanceItemsQueue.Dequeue();

                PreLoadInstanceItem(ref preLoadItem);

                // Set as preLoad completed.
                if (_preLoadInstanceItemComplete != null)
                    _preLoadInstanceItemComplete[(int) preLoadItem.ItemTypeToLoad] = true;
            }

            // 6/13/2010 - Process ChangeRequests immediately, to get Transforms 'ALL' lists populated.
            InstancedModelChangeRequests.ProcessChangeRequests();
        }

        /// <summary>
        /// Helper function to load the given <see cref="InstancedItem"/> type, using the
        /// <see cref="ScenaryItemTypeAttributes"/> Dictionary to locate the modelLoadPath.
        /// </summary>
        /// <param name="preLoadItem"><see cref="PreLoadItem"/> structure, containing the <see cref="ItemType"/> to load.</param>
        private static void PreLoadInstanceItem(ref PreLoadItem preLoadItem)
        {
            // 1/26/2009: Catches any random errors thrown by the internal
            //            XNA framework, which is thrown sometimes when
            //            the same ContentManager is being used in the Main
            //            thread to load the same SceneItemOwner?!
            var itemType = preLoadItem.ItemTypeToLoad;

            try
            {
                // Retrieve Attributes from Dictionary      
                ScenaryItemTypeAttributes itemTypeAtts;
                if (ScenaryItemTypeAtts.ItemTypeAtts.TryGetValue(itemType, out itemTypeAtts))
                {
                    // Pre-load the model into the static ContentManager.

                    // 3/23/2011 - XNA 4.0 Updates
                    var itemTypeLoadPath = itemType.ToString();

#if DEBUG

// ReSharper disable RedundantToStringCall
                    Debug.WriteLine(String.Format("PreLoadInstanceItem ItemType={0}; Total Mem={1}", itemTypeLoadPath,
                                                  GC.GetTotalMemory(false)));
// ReSharper restore RedundantToStringCall
#endif
                    // 6/3/2012 - Updated to also use the new 'NonZip' scenary loader logic below.
                    // 1/6/2010 - Load from proper ContentManager.
                    var isScenaryItem = preLoadItem.SceneItemOwner != null; // 2/16/2010
                    Model xnaModel = isScenaryItem
                                         ? InstancedItem.ScenaryItemsContentManager.Load<Model>(itemTypeAtts.modelLoadPathName) // itemTypeLoadPath
                                         : InstancedItem.PlayableItemsContentManager.Load<Model>(
                                             itemTypeAtts.modelLoadPathName);


                    // 3/23/2010 - IF null, then re-queue to try to load again.
                    if (xnaModel == null) //throw new ContentLoadException();
                    {
                        // 3/23/2010 - Add back to Queue to try loading again.
                        ContentLoadExceptionTryAgain(ref preLoadItem);
                        return;
                    }

                    // InstancedModel
                    var instancedModel = new InstancedModel(xnaModel, itemType, isScenaryItem);

                    // 11/7/2009 - Store new model into array
                    var index0 = (int) itemType; // 6/13/2010
                    InstancedItem.InstanceModels[index0] = instancedModel;

                    // 2/25/2011 - Set Scale
                    if (Math.Abs(itemTypeAtts.Scale - 0) > float.Epsilon)
                    {
                        instancedModel.Scale = itemTypeAtts.Scale;
                    } 

                    // 6/18/2010 - Set ItemType in use.
                    //instancedModel.ItemTypeInUse = itemType;

                    // TODO: TESTING
                    //if (isScenaryItem)
                        //instanceModel.AssignProceduralMaterialId(ShaderToUseEnum.Glossy, null);

                    // 11/7/2009 - Set the Rendering Technqiue
                    InstancedItem.SetInstancedModelTechinque(ref itemTypeAtts, ref instancedModel);

                    // 2/16/2010: Updated to use 'isScenaryItem'.
                    // 11/7/2009 - Add proper Keys for ItemType
                    InstancedItem.AddItemTypeToProperKeyList(itemType, ref itemTypeAtts, isScenaryItem, instancedModel);

                    // 11/7/2009 - Callback the Initialize WorldTransforms method for the current model.
                    if (isScenaryItem)
                    {
                        // cast to ScenaryItem
                        var scenaryItemScene = (preLoadItem.SceneItemOwner as ScenaryItemScene);
                        if (scenaryItemScene != null)
                        {
                            scenaryItemScene.InitializeScenaryItemsWorldTransforms();

                            // 2/15/2010 - Set to transforms to draw.
                            instancedModel.SetDrawExplosionPiecesFlag();
                        }
                    }
                }
            }
            catch (ContentLoadException ex)
            {
                // 3/23/2010 - Add back to Queue to try loading again.
                ContentLoadExceptionTryAgain(ref preLoadItem);

                // empty
                Debug.WriteLine(
                    String.Format(
                        "(ContentLdExp) Unable to Load Content in PreLoad InstanceItems for ItemType={0} with error {1}",
// ReSharper disable RedundantToStringCall
                        itemType.ToString(), ex.Message));
// ReSharper restore RedundantToStringCall
            }
            // 7/9/2009 - Seems to only occur when the same SceneItemOwner is trying to be loaded in 2 seperate Threads!
            catch (ArgumentException ex)
            {
                // 3/23/2010 - Add back to Queue to try loading again.
                ContentLoadExceptionTryAgain(ref preLoadItem);

                // empty
                Debug.WriteLine(
                    String.Format("(ArgExp) Unable to Load Content in PreLoad InstanceItems for ItemType={0}",
// ReSharper disable RedundantToStringCall
                                  itemType.ToString()));
// ReSharper restore RedundantToStringCall
            }
            catch(Exception ex)
            {
                // 3/23/2010 - Add back to Queue to try loading again.
                ContentLoadExceptionTryAgain(ref preLoadItem);

                // empty
                Debug.WriteLine(
                    String.Format("Unable to Load Content in PreLoad InstanceItems for ItemType={0} with exception {1}",
                    // ReSharper disable RedundantToStringCall
                                  itemType.ToString(), ex.Message));
            }
        }

        // 3/23/2010
        /// <summary>
        /// When content fails load, this method re-queues for another attempt; however, after
        /// 5 failed attempts, the artwork is skipped, to avoid an endless loop.
        /// </summary>
        /// <param name="preLoadItem">ref <see cref="PreLoadItem"/> struct.</param>
        private static void ContentLoadExceptionTryAgain(ref PreLoadItem preLoadItem)
        {
            // Update counter, and check if tried too many times.
            preLoadItem.FailedAttempts++;
            if (preLoadItem.FailedAttempts >= 2)
            {
                // 6/3/2012
                if (!preLoadItem.TryScenaryNonZipLoad)
                {
                    preLoadItem.TryScenaryNonZipLoad = true;
                    // Re-Queue for another attempt.
                    PreLoadInstanceItemsQueue.Enqueue(preLoadItem);
                }

                return;
            }

#if DEBUG
            Debug.WriteLine("ContentLoadExceptionTryAgain method, of InstancedItemLoader, is attempting a reload of artwork.");
#endif

            // Re-Queue for another attempt.
            PreLoadInstanceItemsQueue.Enqueue(preLoadItem);

        }

        // 11/20/2009
        /// <summary>
        /// Initalizes the PreLoad 'Complete' array with all FALSEs.
        /// </summary>
        private static void InitializePreloadCompleteArray()
        {
            var itemTypeCount = InstancedItem.ItemTypeCount; // 1/5/2010
            _preLoadInstanceItemComplete = new List<bool>(itemTypeCount);
            // Populate List
            if (_preLoadInstanceItemComplete.Count >= itemTypeCount) return;

            for (var i = 0; i < itemTypeCount; i++)
            {
                _preLoadInstanceItemComplete.Add(false);
            }
        }
    }
}
