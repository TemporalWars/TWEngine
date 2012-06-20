#region File Description
//-----------------------------------------------------------------------------
// TerrainItemToolRoutines.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TWEngine.GameScreens;
using TWEngine.HandleGameInput;
using TWEngine.IFDTiles;
using TWEngine.InstancedModels;
using TWEngine.InstancedModels.Enums;
using TWEngine.ItemTypeAttributes;
using TWEngine.ItemTypeAttributes.Structs;
using TWEngine.Players;
using TWEngine.SceneItems;
using TWEngine.SceneItems.Structs;
using TWEngine.Terrain;
using TWEngine.Terrain.Enums;
using TWEngine.Utilities;
using TWTerrainTools_Interfaces.Structs;
using TWTerrainToolsWPF;

namespace TWEngine.TerrainTools
{
    // 7/1/2010
    /// <summary>
    /// The <see cref="TerrainItemToolRoutines"/> class contains all necessary methods and event handlers to
    /// connect to the WPF ItemTools form.
    /// </summary>
    public class TerrainItemToolRoutines : IDisposable
    {
        private const float ScaleMinValue = 0.01f; // 5/31/2012
        private const float ScaleMaxValue = 100.0f; // 5/31/2012
        private const float CollisionMinValue = 0.01f; // 5/31/2012
        private const float CollisionMaxValue = 100.0f; // 5/31/2012

        internal static ItemToolWindow ItemToolWindowI { get; private set; }

        // 7/4/2010 - Item Content locations to search when populating the TreeList.
        private static readonly List<string> ContentSearchLocations = new List<string>();

        // Players array and 'playerNumber' number.
        private static int _playerNumber; // 1/19/2011 - Set from PlayerNumber NUD control.
        private static Player[] _players; 

        private static SceneItem _itemToPlace; // 10/5/2009: Was 'ScenaryItemScene'
        private static int _itemToPlaceInstanceKey; // 4/14/2009
        private static ItemType _itemTypeToUse;
        private static bool _isPlayableItemType; // 10/5/2009
        private static bool _startItemPlacement; // 1/11/2011

        // PerlinNoise data
        private static List<float> _noiseData;
        private static Vector3 _placeItemAt;
        private static ScenaryItemScene _sceneItemForUndo;
        // TreeList of Positions, used in the Flood generator.
        private static readonly List<Vector3> TreeList = new List<Vector3>();

        // 3/30/2011 - ManualResetEvent
        private static ManualResetEvent _manualResetEventForClosing;

        // 4/3/2011 - Rotation values
        private static float _rotationValue;
        private static float _rotationDelta;
        private static float _oldRotationValue;
        private static SceneItem _currentSceneItem;
        // 4/3/2011 - Scale values
        private static float _scaleValue = 1.0f;
        private static float _scaleDelta;
        // 5/31/2012 - Collision values
        private static float _collisionValue = 1.0f;
        private static float _collisionDelta;

        /// <summary>
        /// constructor
        /// </summary>
        public TerrainItemToolRoutines()
        {
#if WithLicense
            var license = new LicenseHelper();
            license.Required();
#endif

            // Updated to use new GetPlayers.
            TemporalWars3DEngine.GetPlayers(out _players);

            // 11/19/2009 - Need to turn of FOW, otherwise, blinking will occur.
            var fogOfWar = (IFogOfWar)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(IFogOfWar));
            if (fogOfWar != null) fogOfWar.IsVisible = false;

            // 3/30/2011
            _manualResetEventForClosing = new ManualResetEvent(false);

            // 1/11/2011
            CreateItemToolWindow();
        }

        ///<summary>
        /// Needs to be called each time a <see cref="SceneItem"/> is updated in this Tool;
        /// when called, this will update all the Tools form values.
        ///</summary>
        ///<param name="sceneItem"><see cref="SceneItem"/> instance</param>
        ///<exception cref="ArgumentNullException">Thrown when <paramref name="sceneItem"/> instance is null.</exception>
        public void LinkSceneItemToTool(SceneItem sceneItem)
        {
            _currentSceneItem = sceneItem;
        }

        // 1/11/2011
        private static void CreateItemToolWindow()
        {
            ItemToolWindowI = new ItemToolWindow();

            // 7/4/2010 - Populate TreeView's content root search locations.
            PopulateContentSearchLocations(TemporalWars3DEngine.ContentSearchLocations);
            StartPopulateTreeContent();
            
            // 7/1/2010
            // Connect Events to event handlers.
            ConnectEventHandlers();
        }

        /// <summary>
        /// Connects all required event handlers to WPF form.
        /// </summary>
        private static void ConnectEventHandlers()
        {
            ItemToolWindowI.SelectedItemChanged += ItemToolWindowI_SelectedItemChanged;
            ItemToolWindowI.DoGenerateFloodList += ItemToolWindowI_DoGenerateFloodList;
            ItemToolWindowI.UndoGenerateFloodList += ItemToolWindowI_UndoGenerateFloodList;
            ItemToolWindowI.GenerateFloodList += ItemToolWindowI_GenerateFloodList;
            ItemToolWindowI.GeneratePerlinNoise += ItemToolWindowI_GeneratePerlinNoise;
            ItemToolWindowI.PlayerNumberChanged += ItemToolWindowI_PlayerNumberChanged;  // 1/19/2011

            // 1/10/2011 - Form Closed event.
            ItemToolWindowI.FormClosed += ItemToolWindowI_FormClosed;
            // 3/30/2011 - FormStartClose
            ItemToolWindowI.FormStartClose += ItemToolWindowI_FormStartClose;

        }
      
        /// <summary>
        /// Populates the search locations which to use to build the Items TreeList.
        /// </summary>
        /// <param name="contentSearchLocations">(Optional) Used to set the assets content search location; use 'Null' to use development paths.</param>
        private static void PopulateContentSearchLocations(IEnumerable<string> contentSearchLocations)
        {
            try // 6/22/2010
            {
                // 1/9/2011 - If null, then use default dev location.
                if (contentSearchLocations == null)
                {
                    var visualStudioDir = TemporalWars3DEngine.VisualStudioLocation;


                    ContentSearchLocations.Add(visualStudioDir + @"\\TemporalWars 3D Engine\\Dev\\ContentForResources\\ContentAlleyPack\\");
                    ContentSearchLocations.Add(visualStudioDir + @"\\TemporalWars 3D Engine\\Dev\\ContentForResources\\ContentDowntownDistrictPack\\");
                    ContentSearchLocations.Add(visualStudioDir + @"\\TemporalWars 3D Engine\\Dev\\ContentForResources\\ContentRTSPack\\");
                    ContentSearchLocations.Add(visualStudioDir + @"\\TemporalWars 3D Engine\\Dev\\ContentForResources\\ContentSticksNTwiggPack\\");
                    ContentSearchLocations.Add(visualStudioDir + @"\\TemporalWars 3D Engine\\Dev\\ContentForResources\\ContentUrbanPack\\");
                    ContentSearchLocations.Add(visualStudioDir + @"\\TemporalWars 3D Engine\\Dev\\ContentForResources\\ContentWarehouseDistrictPack\\");

                    // 10/5/2009 - Add also 'PlayableModels', used for scripting AI side.
                    ContentSearchLocations.Add(visualStudioDir + @"\\TemporalWars 3D Engine\\Dev\\ContentForResources\\ContentPlayableModels\\");
                    return;
                }

                // 1/9/2011 - Else, use the given parameter data.
                foreach (var contentSearchLocation in contentSearchLocations)
                {
                    ContentSearchLocations.Add(contentSearchLocation);
                }
                
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PopulateContentSearchLocations method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        #region EventHandlers


        /// <summary>
        /// Builds the an array of <see cref="DirectoryInfo"/> locations, to give to the
        /// WPF form's TreeView.
        /// </summary>
        static void StartPopulateTreeContent()
        {
            // 7/4/2010 - 
            var count = ContentSearchLocations.Count;
            var rootLocationsForTree = new DirectoryInfo[count];

            // Iterate the 'contentSearchList'.
            for (var i = 0; i < count; i++)
            {
                // get directory name
                var modelsDirPath = Path.GetDirectoryName(ContentSearchLocations[i]);
                // store into array 
                rootLocationsForTree[i] = new DirectoryInfo(modelsDirPath);
            }

            // 1/9/2011 - Retrieve path from TemporalWars3DEngine class.
            // set path to Icon asset images.
            //var imagesDirPath =Path.GetDirectoryName(Environment.GetEnvironmentVariable("VisualStudioDir") + @"\\Projects\\XNA_RTS2008\\Dev\\ItemToolPics\\");
            var directoryForItemToolPics = new DirectoryInfo(TemporalWars3DEngine.ItemToolsAssetPreviewPics); 
            
            // 1/9/2011 - Updated to pass new directory filter.
            // Set into WPF form
            ItemToolWindowI.CreateDataContextForTree(rootLocationsForTree, 
                directoryForItemToolPics, fi => ((fi.Extension == ".X" || fi.Extension == ".FBX") || fi.Extension == ".x") || fi.Extension == ".fbx",
                dir => (!dir.Name.Equals("obj") && !dir.Name.Equals("bin") && !dir.Name.Equals(".Thumbnails")));
        }

        // 7/4/2010
        /// <summary>
        /// Generates a NoiseMap, using the Perlin Noise Algorithm.  The noise data will be used
        /// to flood the terrain with the choose 'ItemType', using the perlin noise values (0-255) to determine
        /// the positions of the given items.
        /// </summary>
        static void ItemToolWindowI_GeneratePerlinNoise(object sender, EventArgs e)
        {
            try
            {
                // 7/4/2010 - Get attributes
                PerlinNoisePass perlinNoisePass;
                ItemToolWindowI.GetPerlinNoiseAttributes(out perlinNoisePass);

                // Read values in from 'PerlinNoise' Group controls            
                var randomSeedP1 = perlinNoisePass.RandomSeed;
                var perlinNoiseSizeP1 = perlinNoisePass.PerlinNoiseSize;
                var perlinPersistenceP1 = perlinNoisePass.PerlinPersistence;
                var perlinOctavesP1 = perlinNoisePass.PerlinOctaves;

                // Generate PerlinNoise '_noiseData'.
                _noiseData = TerrainData.CreatePerlinNoiseMap(randomSeedP1, perlinNoiseSizeP1, perlinPersistenceP1,
                                                              perlinOctavesP1);

                // Populate Color 'Bits', used to store into texture map
                // Set Image into PictureBox on Form.
                ItemToolWindowI.SetPictureBoxImage(TerrainData.CreateBitmapFromPerlinNoise(_noiseData));
            }
            catch (Exception err)
            {
#if DEBUG
                Debug.WriteLine("ItemToolWindowI_GeneratePerlinNoise method threw an exception; " + err.Message ?? "No Message.");
#endif
            }
        }

        /// <summary>
        /// Iterate through all terrain positions, comparing to the 'NoiseData' array to determine
        /// 'Positions' to use for flooding.  Returns a List of Positions.
        /// </summary>
        static void ItemToolWindowI_GenerateFloodList(object sender, EventArgs e)
        {
            try
            {
                // Check if noise data is null or zero Count
                if (_noiseData == null || _noiseData.Count == 0)
                {
                    ItemToolWindowI.SetErrorMessage(@"A 'Perlin-Noise' MUST be generated first before the automatic Flood generator can be run.");
                    return;
                }

                // make sure ItemType was chosen.
                if (ItemToolWindowI.InstanceNumber == -1)
                {
                    ItemToolWindowI.SetErrorMessage(@"A 'SceneItem' MUST be chosen first before the automatic Flood generator can be run.");
                    return;
                }

                // 7/4/2010 - Get attributes
                FloodConstraints floodConstraints;
                ItemToolWindowI.GetFloodConstraintAttributes(out floodConstraints);

                // Flood Terrain with chosen SceneItemOwner, using the _noiseData.            
                // Random 
                var spacing = floodConstraints.Spacing;
                var densitySpacing = floodConstraints.DensitySpacing;
                var random = new Random();
                TreeList.Clear();

                var mapWidth = TerrainData.MapWidth; // 11/21/09
                var mapHeight = TerrainData.MapHeight; // 11/21/09
                const int scale = TerrainData.cScale; // 11/21/09
                var minFlatness = (float)Math.Cos(MathHelper.ToRadians(15)); // 11/21/09

                for (var x = 0; x < mapWidth; x += spacing)
                {
                    for (var y = 0; y < mapHeight; y += spacing)
                    {
                        float terrainXPos = x * scale;
                        float terrainYPos = y * scale;

                        // retrieve Height for the given x/y map location
                        var terrainHeight = TerrainData.GetTerrainHeight(terrainXPos, terrainYPos);

                        // make sure within height limits set by user
                        if (terrainHeight <= floodConstraints.HeightMin || terrainHeight >= floodConstraints.HeightMax)
                            continue;

                        // get Normal for given location
                        Vector3 avgNormal;
                        TerrainData.GetNormal(terrainXPos, terrainYPos, out avgNormal);

                        // make sure at flatness set by user
                        var flatness = Vector3.Dot(avgNormal, Vector3.Up);

                        if (flatness <= minFlatness) continue;

                        // retrieve Noise value for given location
                        var noiseValueAtCurrentPosition = _noiseData[x + y * mapHeight];

                        // determine the 'Density' value
                        float treeDensity;
                        if (noiseValueAtCurrentPosition > floodConstraints.NoiseGreater_Lv3)
                            treeDensity = floodConstraints.Density_Lv3;
                        else if (noiseValueAtCurrentPosition > floodConstraints.NoiseGreater_Lv2)
                            treeDensity = floodConstraints.Density_Lv2;
                        else if (noiseValueAtCurrentPosition > floodConstraints.NoiseGreater_Lv1)
                            treeDensity = floodConstraints.Density_lv1;
                        else
                            treeDensity = 0;

                        // create random positions around given location, with given density
                        for (var currDetail = 0; currDetail < treeDensity; currDetail++)
                        {
                            var rand1 = random.Next(1000) / 1000.0f;
                            var rand2 = random.Next(1000) / 1000.0f;
                            rand1 *= densitySpacing;
                            rand2 *= densitySpacing;

                            var xPos = (x - rand1) * scale;
                            var yPos = (y - rand2) * scale;

                            var treePosition = new Vector3(xPos, 0, yPos) { Y = TerrainData.GetTerrainHeight(xPos, yPos) };

                            TreeList.Add(treePosition);
                        } // End For Density
                    } // End For X/Y loop
                }


                // Set Total Count into text window
                ItemToolWindowI.FloodListCount = TreeList.Count;
            }
            catch (Exception err)
            {
#if DEBUG
                Debug.WriteLine("btnGenerateFloodList_Click method threw an exception; " + err.Message ?? "No Message.");
#endif
            }
        }

        // 7/4/2010
        /// <summary>
        /// Undoes the last Terrain Flood operation, for the given 'TreeList'.
        /// </summary>
        static void ItemToolWindowI_UndoGenerateFloodList(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var game = TemporalWars3DEngine.GameInstance;

                // Check 'SceneITemForUndo' to make sure not null
                if (_sceneItemForUndo == null)
                {
                    ItemToolWindowI.SetErrorMessage(@"A 'Flood' ACTION must be generated first, before any 'Undo' operation can occur.");
                    return;
                }

                // Delete the 'SceneItemForUndo' from Terrain.
                var terrainScreen = (TerrainScreen)game.Services.GetService(typeof(TerrainScreen));
                terrainScreen.EditModeDeleteSpecificScenarySceneItem(_sceneItemForUndo);

                // set to null
                _sceneItemForUndo = null;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnUndoLastFlood_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/4/2010
        /// <summary>
        /// Uses the 'TreeList' of positions, generated in the FloodList operation, to flood the
        /// terrain with given Itemtype.
        /// </summary>
        static void ItemToolWindowI_DoGenerateFloodList(object sender, EventArgs e)
        {
            try
            {
                // Check TreeList to make sure not empty
                var count = TreeList.Count; // 11/21/09
                if (count == 0)
                {
                    ItemToolWindowI.SetErrorMessage(@"A 'FloodList' must be generated first, before the automatic Flood generator can be run.");
                    return;
                }

                // 11/20/2009 - Check if in PlayerItemTypeAtts Dictionary, which implies this item
                //             is a playable item, and not a scenery item!
                _isPlayableItemType = PlayableItemTypeAtts.ItemTypeAtts.ContainsKey(_itemTypeToUse);

                // 10/5/2009 - Make sure flooding is done with a ScenearyItemScene instance!
                if (_isPlayableItemType)
                {
                    ItemToolWindowI.SetErrorMessage(@"You can only flood with ScenaryItems, not PlayableItems.");
                    return;
                }

                // Create List<ScenaryITemData>, to use to populate the ScenaryItemScene.
                var scenaryItemsToAdd = new List<ScenaryItemData>(count);
                for (var i = 0; i < count; i++)
                {
                    var scenaryItemData = new ScenaryItemData
                    {
                        instancedItemData = { ItemType = _itemTypeToUse },
                        isPathBlocked = false,
                        pathBlockSize = 0,
                        position = TreeList[i]
                    };

                    // 7/2/2009 - Apply random rotations for each instance
                    var angle = MathUtils.RandomBetween(0, 360);
                    scenaryItemData.rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, angle);


                    scenaryItemsToAdd.Add(scenaryItemData);
                }

                // Call 'ScenaryItemScene' constructor, with the List of data to add.
                var gameInstance = TemporalWars3DEngine.GameInstance;

                // 3/30/2011 - Check if instance already exist
                ScenaryItemScene scenaryItemScene;
                if (!TerrainScreen.GetSceneItemInstance(_itemTypeToUse, out scenaryItemScene))
                {
                    scenaryItemScene = new ScenaryItemScene(gameInstance, _itemTypeToUse, scenaryItemsToAdd, 0);
                    
                }

                // Iterate transforms to add
                for (var i = 0; i < scenaryItemsToAdd.Count; i++)
                {
                    var transform = scenaryItemsToAdd[i].position;
                    scenaryItemScene.AddScenaryItemSceneInstance(_itemTypeToUse, ref transform, 0, scenaryItemsToAdd[i].rotation);
                }

                //scenaryItemScene.InitializeScenaryItemsWorldTransforms();
                //TerrainScreen.SceneCollection.Add(scenaryItemScene);
                //var terrainShape = (ITerrainShape)gameInstance.Services.GetService(typeof(ITerrainShape));
                //terrainShape.ScenaryItems.Add(scenaryItemScene);

                // Save ref to '_newItemScene', for Undo actions.
                _sceneItemForUndo = scenaryItemScene;

                // Clear any error messages from display
                ItemToolWindowI.SetErrorMessage(string.Empty);
            }
            catch (Exception err)
            {
#if DEBUG
                Debug.WriteLine("btnPerformFlood_Click method threw an exception; " + err.Message ?? "No Message.");
#endif
            }
        }

        // 7/4/2010
        static void ItemToolWindowI_SelectedItemChanged(object sender, EventArgs e)
        {
            // 3/30/2011
            ItemToolWindowI.InstanceNumber = (int)((ItemType)Enum.Parse(typeof(ItemType), ItemToolWindowI.CurrentSelectedFileName));

        }

        // 1/10/2011
        /// <summary>
        /// Updates proper settings when some WPF form closes.
        /// </summary>
        static void ItemToolWindowI_FormClosed(object sender, EventArgs e)
        {
           // 1/11/2011 - Recreate new instance; otherwise, calling 'Show' will crash WPF.
            ContentSearchLocations.Clear();

            // 3/30/2011 - Signal close complete
            _manualResetEventForClosing.Set();
           
        }

        // 3/30/2011
        /// <summary>
        /// Occurs when the WPF form is starting the close cycle.
        /// </summary>
        static void ItemToolWindowI_FormStartClose(object sender, EventArgs e)
        {
            // Set ToolType window to start close cycle
            TerrainWPFTools.ToolTypeToClose = ToolType.ItemTool;

            // Set State of StartcloseCycle to false
            ItemToolWindowI.StartCloseCycle = false;
        }

        #endregion

        // 7/1/2010
        /// <summary>
        /// Updates the current WPF tool.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            DoAddAssetUpdateCheck(); // 1/11/2011
            DoPlaceAssetUpdateCheck();
            DoRotationUpdateCheck(gameTime); // 4/3/2011
            DoScaleUpdateCheck(gameTime); // 4/3/2011
            DoCollisionUpdateCheck(gameTime); // 5/31/2012
        }

        // 3/30/2011
        public void CloseForm()
        {
            ItemToolWindowI.Close();

            // Wait for WPF Closed event to trigger before allowing exit of method call.
            _manualResetEventForClosing.WaitOne();
        }

        /// <summary>
        /// Adds the new selected asset as <see cref="SceneItem"/> to the <see cref="TerrainScene"/> collection,
        ///  and the <see cref="Player"/> collections if considered a 'playableItem'.
        /// </summary>
        private static void AddNewSceneItem()
        {
            try // 6/20/2010
            {
                var sceneItemToUseTag = ItemToolWindowI.CurrentSelectedFileName;// tvItems.SelectedNode.Tag;
                var game = TemporalWars3DEngine.GameInstance;
                if (sceneItemToUseTag == null) return;

                TerrainPickingRoutines.GetCursorPosByPickedRay(PickRayScale.NoChange, out _placeItemAt);
                
                // 7/31/2008 - Added Enum.Parse line below, which now directly takes the string
                //             name in the Tag, and uses it to become the ItemType Enum; thereby, 
                //             eliminating the need for a long Switch statement!!! :)
                _itemTypeToUse = (ItemType)Enum.Parse(typeof(ItemType), sceneItemToUseTag);

                // 10/5/2009 - Check if in PlayerItemTypeAtts Dictionary, which implies this item
                //             is a playable item, and not a scenery item!
                _isPlayableItemType = PlayableItemTypeAtts.ItemTypeAtts.ContainsKey(_itemTypeToUse);

                //
                // Add to the Terrain scene  
                //

                // 10/5/2009 - Create proper SceneItem type; sceneryItem or playableItem.
                if (_isPlayableItemType)
                {
                    // Which PlayableItem 'ItemGroupType' is this?
                    var player = _players[_playerNumber]; // 6/15/2010
                    CreatePlayableItem(_itemTypeToUse, ref _placeItemAt, out _itemToPlace);

                    // 6/15/2010 - Updated to use Add method.
                    Player.AddSelectableItem(player, _itemToPlace as SceneItemWithPick, false);

                    // 1/19/2011 - Transfer SceneItem to proper Player instance.
                    Player.TransferSelectableItem((SceneItemWithPick)_itemToPlace, (byte)_playerNumber);
                    
                }
                else
                {
                    ScenaryItemScene scenaryItemScene;
                    if (TerrainScreen.GetSceneItemInstance(_itemTypeToUse, out scenaryItemScene))
                    {
                        scenaryItemScene.AddScenaryItemSceneInstance(_itemTypeToUse, ref _placeItemAt, 0, null);
                        _itemToPlace = scenaryItemScene;
                    }
                    else
                    {
                        _itemToPlace = new ScenaryItemScene(game, _itemTypeToUse, ref _placeItemAt, 0);
                        _itemToPlaceInstanceKey = _itemToPlace.ShapeItem.ItemInstanceKey; // 4/14/2009
                    }

                    // 5/13/2008 - If PathBlocked, then let's update the A* GraphNodes
                    if (_itemToPlace.ShapeItem.IsPathBlocked)
                    {
                        // 12/9/2008 - Set AStarGraph Costs
                        ((ScenaryItemScene)_itemToPlace).SetAStarCostsForCurrentItem();

                        TerrainShape.PopulatePathNodesArray();
                    }

                    // 4/3/2011
                    TerrainQuadTree.UpdateSceneryCulledList = true;
                }

                // 7/1/2009 - Display item key; will be used by the Flood Generator.
                ItemToolWindowI.InstanceNumber = (int)_itemTypeToUse;
                
            }
            catch (Exception err)
            {
#if DEBUG
                Debug.WriteLine("AddNewSceneItem method threw an exception; " + err.Message ?? "No Message.");
#endif
            }

        }

        // 6/20/2010
        /// <summary>
        /// Removes old <see cref="_itemToPlace"/> from the <see cref="TerrainScreen"/> collection and
        /// the <see cref="Player"/> collections.
        /// </summary>
        /*private static void RemoveOldSceneItem()
        {
            try // 6/20/2010
            {
                if (_itemToPlace == null) return;

                TerrainScreen.SceneCollection.Remove(_itemToPlace);
                // 9/17/2008 - Need to Remove InstanceTransform too
                var sceneryItemToPlace = (_itemToPlace as ScenaryItemScene); // 10/5/2009
                if (sceneryItemToPlace != null)
                {
                    sceneryItemToPlace.ShapeItem.RemoveInstanceTransform(_itemToPlaceInstanceKey);
                    _itemToPlace = null;
                }
                else
                {
                    // 1st - remove temp _itemToPlace
                    // 11/11/2008: Note: InstanceItems Transform deletes also taken care of with this setting!  
                    _itemToPlace.DrawStatusBar = false;

                    // 6/15/2010 - Updated to use Remove methods.
                    //player.SelectableItems.Remove(_itemToPlace as SceneItemWithPick);
                    //player.ItemsSelected.Remove(sceneItemWithPick);
                    var player = _players[playerNumber]; // 6/15/2010
                    var sceneItemWithPick = _itemToPlace as SceneItemWithPick; // 6/15/2010 - cache
                    Player.RemoveSelectableItem(player, sceneItemWithPick);
                    Player.RemoveItemSelected(player, sceneItemWithPick);

                }
            }
            catch (Exception err)
            {
#if DEBUG
                Debug.WriteLine("RemoveOldSceneItem method threw an exception; " + err.Message ?? "No Message.");
#endif
            }

        }*/

        // 10/5/2009 - Creates the proper PlayableItem, and passes back via the OUT param.
        private static void CreatePlayableItem(ItemType itemTypeToUse, ref Vector3 placeItemAt, out SceneItem itemToPlace)
        {
            itemToPlace = null;
            var game = TemporalWars3DEngine.GameInstance;

            try // 6/20/2010
            {
                PlayableItemTypeAttributes playableAtts;
                if (!PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(itemTypeToUse, out playableAtts)) return;

                switch (playableAtts.ItemGroupType)
                {
                    case ItemGroupType.Buildings:
                        itemToPlace = new BuildingScene(game, itemTypeToUse, ref placeItemAt, 0);

                        var buildingScene = (_itemToPlace as BuildingScene);

                        if (buildingScene != null)
                        {
                            buildingScene.ShapeItem.SetInstancedItemTypeToUse(_itemTypeToUse);
                            buildingScene.LoadPlayableAttributesForItem(
                                new ItemCreatedArgs { ItemType = _itemTypeToUse, PlaceItemAt = _placeItemAt }, false);
                        }
                        _itemToPlace.Position = _placeItemAt;

                        break;
                    case ItemGroupType.Shields:

                        itemToPlace = new DefenseScene(game, itemTypeToUse, ItemGroupType.Vehicles, ref placeItemAt, 0);

                        var defenseScene = (_itemToPlace as DefenseScene);

                        if (defenseScene != null)
                        {
                            defenseScene.ShapeItem.SetInstancedItemTypeToUse(_itemTypeToUse);
                            defenseScene.LoadPlayableAttributesForItem(
                                new ItemCreatedArgs { ItemType = _itemTypeToUse, PlaceItemAt = _placeItemAt }, false);
                        }
                        _itemToPlace.Position = _placeItemAt;

                        break;
                    case ItemGroupType.Vehicles:
                        itemToPlace = new SciFiTankScene(game, itemTypeToUse, ItemGroupType.Vehicles,
                                                         ref placeItemAt, 0);

                        var tankScene = (_itemToPlace as SciFiTankScene);

                        if (tankScene != null)
                        {
                            tankScene.ShapeItem.SetInstancedItemTypeToUse(_itemTypeToUse);
                            tankScene.LoadPlayableAttributesForItem(
                                new ItemCreatedArgs { ItemType = _itemTypeToUse, PlaceItemAt = _placeItemAt }, false);
                        }
                        _itemToPlace.Position = _placeItemAt;

                        break;
                    case ItemGroupType.Airplanes:
                        itemToPlace = new SciFiAircraftScene(game, itemTypeToUse, ItemGroupType.Vehicles,
                                                             ref placeItemAt, 0);

                        var aircraftScene = (_itemToPlace as SciFiAircraftScene);

                        if (aircraftScene != null)
                        {
                            aircraftScene.ShapeItem.SetInstancedItemTypeToUse(_itemTypeToUse);
                            aircraftScene.LoadPlayableAttributesForItem(
                                new ItemCreatedArgs { ItemType = _itemTypeToUse, PlaceItemAt = _placeItemAt }, false);
                        }
                        _itemToPlace.Position = _placeItemAt;

                        break;
                } // End Switch
            }
            catch (Exception err)
            {
#if DEBUG
                Debug.WriteLine("CreatePlayableItem method threw an exception; " + err.Message ?? "No Message.");
#endif
            }

        }

        // 5/7/2008
        // Helper Function which checks if a Point given is within this Window Form's Client Rectangle.        
        // The 'PointToScreen' method is used to convert the Forms' MousePoint to Screen coordinates.  Finally,
        // this is compared using a rectangle, created with this Windows location, and the rectangle's 'Contain'
        // method is called.
        public static bool IsMouseInControl()
        {
            var isIn = false;
            try // 6/20/2010
            {
                /*_mousePoint.X = MousePosition.X;
                _mousePoint.Y = MousePosition.Y;

                // set this Form's ClientRectangle            
                _rectangle.X = Location.X + 5;
                _rectangle.Y = Location.Y + 5;
                _rectangle.Width = Width - 5;
                _rectangle.Height = Height - 5;

                _rectangle.Contains(ref _mousePoint, out isIn);*/

                if (ItemToolWindowI != null) isIn = ItemToolWindowI.IsMouseOver;
            }
            catch (Exception err)
            {
#if DEBUG
                Debug.WriteLine("TerrainItemToolRoutines classes IsMouseInControl method threw an exception; " + err.Message ?? "No Message.");
#endif
            }

            return isIn;

        }

        // 1/11/2011
        /// <summary>
        /// Starts the process of adding some sceneItem to the game engine when the user
        /// holds down the right mouse button.
        /// </summary>
        private static void DoAddAssetUpdateCheck()
        {
            // If user holds Right-Click on Terrain, then create asset for placement
            if (!HandleInput.InputState.RightMouseButton || IsMouseInControl() || _startItemPlacement) return;

            // Remove old SceneItemOwner from Scene  
            //RemoveOldSceneItem(); 

            // Add to the Terrain scene  
            AddNewSceneItem(); 

            // Load ArtWork.
            InstancedItemLoader.PreLoadInstanceItemsMethod();

            _startItemPlacement = true;

        }

        // 5/18/2010; 1/11/2011 - Renamed and removed redudant re-adding of scene items.
        /// <summary>
        /// Finishes the process of adding some sceneItem to the game engine when the user
        /// releases the right mouse button.
        /// </summary>
        private static void DoPlaceAssetUpdateCheck()
        {
            try // 6/22/2010
            {
                // Update Position of ItemToPlace
                if (_itemToPlace == null) return;

                // Get Position of Mouse Cursor in World Space 
                TerrainPickingRoutines.GetCursorPosByPickedRay(PickRayScale.NoChange, out _placeItemAt);
                _itemToPlace.Position = _placeItemAt;

                // 1/11/2010 - Updated to use the 'RightMouseButton Released'.
                // If user releases Right-Click on Terrain, then place SceneItem at this Position
                if (!HandleInput.InputState.RightMouseButtonReleased || IsMouseInControl()) return;

                _startItemPlacement = false;
                _itemToPlace = null;

               

            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("DoPlaceAssetUpdateCheck method threw the exception " + ex.Message ?? "No Message");
#endif
            }
           
        }

        // 4/3/2011
        /// <summary>
        /// Updates the item's rotation value when user holds 'R' down and moves
        /// the mouse cursor.
        /// </summary>
        private static void DoRotationUpdateCheck(GameTime gameTime)
        {
            if (!HandleInput.InputState.IsKeyPress(Keys.R)) return;

            if (_currentSceneItem == null) return;

            var quaternion = _currentSceneItem.Rotation;
            Vector3 rotationVector;
            MathUtils.QuaternionToEuler(ref quaternion, out rotationVector);
            _rotationValue = rotationVector.Y;

            if (HandleInput.InputState.MouseScrollRight)
            {
                _rotationDelta = 55f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else if (HandleInput.InputState.MouseScrollLeft)
            {
                _rotationDelta = -55f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            _rotationValue += _rotationDelta;
            TerrainQuadTree.UpdateSceneryCulledList = true;
            _rotationValue = MathHelper.Clamp(_rotationValue, -180, 180);

            RotateSceneItem();

            _oldRotationValue = _rotationValue;
        }

        // 4/3/2011
        /// <summary>
        /// Helper method to rotate the current <see cref="SceneItem"/>.
        /// </summary>
        private static void RotateSceneItem()
        {
            if (_currentSceneItem == null) return;
             
            if (Math.Abs(_oldRotationValue - _rotationValue) < float.Epsilon) return;

            var rotationAxis = Matrix.CreateRotationY(MathHelper.ToRadians(_rotationValue));

            Quaternion quaternion;
            Quaternion.CreateFromRotationMatrix(ref rotationAxis, out quaternion);

            _currentSceneItem.Rotation = quaternion;
        }

        // 4/3/2011
        /// <summary>
        /// Updates the item's scale value when user holds 'S' down and moves
        /// the mouse cursor.
        /// </summary>
        private static void DoScaleUpdateCheck(GameTime gameTime)
        {
            if (!HandleInput.InputState.IsKeyPress(Keys.S)) return;

            if (_currentSceneItem == null) return;

            var scale = _currentSceneItem.Scale;
            _scaleValue = scale.Y;

            if (HandleInput.InputState.MouseScrollRight)
            {
                _scaleDelta = 2f*(float) gameTime.ElapsedGameTime.TotalSeconds;
            }
            else if (HandleInput.InputState.MouseScrollLeft)
            {
                _scaleDelta = -2f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            _scaleValue += _scaleDelta;
            TerrainQuadTree.UpdateSceneryCulledList = true;
            MathHelper.Clamp(_scaleValue, ScaleMinValue, ScaleMaxValue);
           
            scale.X = scale.Y = scale.Z = _scaleValue;
            _currentSceneItem.Scale = scale;

        }

        // 5/31/2012
        /// <summary>
        /// Updates the item's collision value when user holds 'C' down and moves
        /// the mouse cursor.
        /// </summary>
        private static void DoCollisionUpdateCheck(GameTime gameTime)
        {
            if (!HandleInput.InputState.IsKeyPress(Keys.C)) return;

            if (_currentSceneItem == null) return;

            var collision = _currentSceneItem.CollisionRadius;
            _collisionValue = collision;

            if (HandleInput.InputState.MouseScrollRight)
            {
                _collisionDelta = 5f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else if (HandleInput.InputState.MouseScrollLeft)
            {
                _collisionDelta = -5f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            _collisionValue += _collisionDelta;
            TerrainQuadTree.UpdateSceneryCulledList = true;
            MathHelper.Clamp(_collisionValue, CollisionMinValue, CollisionMaxValue);

            collision = _collisionValue;
            _currentSceneItem.CollisionRadius = collision;

        }

        // 1/19/2011
        /// <summary>
        /// Occurs when the PlayerNumber NUD control is updated.
        /// </summary>
        private static void ItemToolWindowI_PlayerNumberChanged(object sender, EventArgs e)
        {
            // Set PlayerNumber
            _playerNumber = ItemToolWindowI.PlayerNumber;
        }

        // 7/9/2010
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            // Dispose of WPF window
            if (ItemToolWindowI != null)
            {
                ItemToolWindowI.Close();
                ItemToolWindowI = null;
            }                
        }
    }
}
