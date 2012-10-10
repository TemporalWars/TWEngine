using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using ImageNexus.BenScharbach.TWEngine.IFDTiles;
using ImageNexus.BenScharbach.TWEngine.SceneItems.Structs;
using ImageNexus.BenScharbach.TWLate.RTS_FogOfWarInterfaces.FOW;
using Microsoft.Xna.Framework;
using TWEngine.GameScreens;
using TWEngine.HandleGameInput;
using TWEngine.InstancedModels.Enums;
using TWEngine.ItemTypeAttributes.Structs;
using TWEngine.Players;
using TWEngine.SceneItems;
using TWEngine.SceneItems.Structs;
using TWEngine.Terrain;
using TWEngine.Terrain.Enums;
using TWEngine.TerrainTools.Enums;
using TWEngine.Utilities;
using TWEngine.ItemTypeAttributes;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace TWEngine.TerrainTools
{
    internal partial class ItemsTools : Form
    {
        // 7/22/2008
        private readonly ITerrainShape _terrainShape;

// ReSharper disable UnaccessedField.Local
        private ItemsTool _currentTool = ItemsTool.Select;
// ReSharper restore UnaccessedField.Local

        private readonly Game _game;

        private SceneItem _itemToPlace; // 10/5/2009: Was 'ScenaryItemScene'
        private int _itemToPlaceInstanceKey; // 4/14/2009
        private ItemType _itemTypeToUse;
        private bool _isPlayableItemType; // 10/5/2009
        private Point _mousePoint;
        private SceneItem _newItemScene; // 10/5/2009: Was 'ScenaryItemScene'

        // 7/1/2009 - PerlinNoise data
        private List<float> _noiseData;
        private Vector3 _placeItemAt;
        private Rectangle _rectangle;
        private ScenaryItemScene _sceneItemForUndo;
        // 7/1/2009 - TreeList of Positions, used in the Flood generator.
        private readonly List<Vector3> _treeList = new List<Vector3>();

        // 9/23/2009 - Item Content locations to search when populating the TreeList.
        private readonly List<string> _contentSearchLocations = new List<string>();

        // 10/5/2009 - Players array & 'ThisPlayer' number.
        private readonly int _thisPlayer = TemporalWars3DEngine.SThisPlayer;
        private readonly Player[] _players; 

        public ItemsTools(Game game)
        {

            try // 6/22/2010
            {
                InitializeComponent();

                // 6/15/2010 - Updated to use new GetPlayers.
                TemporalWars3DEngine.GetPlayers(out _players);

                _game = game;

                // 11/20/2009 - Need to turn of FOW, otherwise, blinking will occur.
                var fogOfWar = (IFogOfWar)game.Services.GetService(typeof(IFogOfWar));
                if (fogOfWar != null) fogOfWar.IsVisible = false;

                // 9/23/2009 - Populate Content search locations.
                PopulateContentSearchLocations();

                // Get TerrainShape Interface
                _terrainShape = (ITerrainShape)game.Services.GetService(typeof(ITerrainShape));

                // 3/3/2009 - Set in EditMode for TerrainShape
                TerrainShape.TerrainIsIn = TerrainIsIn.EditMode;


                TerrainPickingRoutines.GetCursorPosByPickedRay(PickRayScale.NoChange, out _placeItemAt);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("ItemsTool constructor threw the exception " + ex.Message ?? "No Message");
#endif
            }           
        }

        // 9/23/2009
        /// <summary>
        /// Populates the search locations which to use to build the Items TreeList.
        /// </summary>
        private void PopulateContentSearchLocations()
        {
            try // 6/22/2010
            {
                var visualStudioDir = Environment.GetEnvironmentVariable("VisualStudioDir");

                _contentSearchLocations.Add(visualStudioDir + @"\\Projects\\XNA_RTS2008\\Dev\\ContentForResources\\ContentAlleyPack\\");
                _contentSearchLocations.Add(visualStudioDir + @"\\Projects\\XNA_RTS2008\\Dev\\ContentForResources\\ContentDowntownDistrictPack\\");
                _contentSearchLocations.Add(visualStudioDir + @"\\Projects\\XNA_RTS2008\\Dev\\ContentForResources\\ContentRTSPack\\");
                _contentSearchLocations.Add(visualStudioDir + @"\\Projects\\XNA_RTS2008\\Dev\\ContentForResources\\ContentSticksNTwiggPack\\");
                _contentSearchLocations.Add(visualStudioDir + @"\\Projects\\XNA_RTS2008\\Dev\\ContentForResources\\ContentUrbanPack\\");
                _contentSearchLocations.Add(visualStudioDir + @"\\Projects\\XNA_RTS2008\\Dev\\ContentForResources\\ContentWarehouseDistrictPack\\");

                // 10/5/2009 - Add also 'PlayableModels', used for scripting AI side.
                _contentSearchLocations.Add(visualStudioDir + @"\\Projects\\XNA_RTS2008\\Dev\\ContentForResources\\ContentPlayableModels\\");
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PopulateContentSearchLocations method threw the exception " + ex.Message ?? "No Message"); 
#endif
            }
        }

// ReSharper disable UnusedMember.Local
        private void SelectTool(ItemsTool tool)
// ReSharper restore UnusedMember.Local
        {
            try // 6/22/2010
            {
                switch (tool)
                {
                    case ItemsTool.Select:
                        _currentTool = ItemsTool.Select;
                        break;
                    case ItemsTool.Fill:
                        _currentTool = ItemsTool.Fill;
                        break;
                    case ItemsTool.Unfill:
                        _currentTool = ItemsTool.Unfill;
                        break;
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("SelectTool method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }


        private void ItemsTools_Load(object sender, EventArgs e)
        {
            // Start Timer Tick
            timer1.Start();
        }


        protected override void OnClosed(EventArgs e)
        {
            // Remove old SceneItemOwner from Scene  
            RemoveOldSceneItem(); // 6/20/2010

            base.OnClosed(e);
        }


        // Check User Selection in TreeView
// ReSharper disable InconsistentNaming
        private void tvItems_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try // 6/22/2010
            {
                // Show Larger Image in ListView
                listView1.Items.Clear();
                listView1.Items.Add("SceneItemOwner", tvItems.SelectedNode.SelectedImageKey);

                // Remove old SceneItemOwner from Scene  
                RemoveOldSceneItem(); // 6/20/2010

                // Add to the Terrain scene  
                AddNewSceneItem(); // 6/20/2010

                // 1/11/2009 - Load ArtWork.
                InstancedItemLoader.PreLoadInstanceItemsMethod();
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("tvItems_AfterSelect method threw the exception " + ex.Message ?? "No Message");  
#endif
            }
        }

        // 6/20/2010
        /// <summary>
        /// Adds the new selected asset as <see cref="SceneItem"/> to the <see cref="TerrainScene"/> collection,
        ///  and the <see cref="Player"/> collections if considered a 'playableItem'.
        /// </summary>
        private void AddNewSceneItem()
        {
            try // 6/20/2010
            {
                // 6/20/2010
                var sceneItemToUseTag = tvItems.SelectedNode.Tag;
                if (sceneItemToUseTag == null) return;

                TerrainPickingRoutines.GetCursorPosByPickedRay(PickRayScale.NoChange, out _placeItemAt);

                // Check Tag in TreeView to see which SceneItemOwner user wants.
                var nodeValue = sceneItemToUseTag.ToString();

                // 7/31/2008 - Added Enum.Parse line below, which now directly takes the string
                //             name in the Tag, and uses it to become the ItemType Enum; thereby, 
                //             eliminating the need for a long Switch statement!!! :)
                _itemTypeToUse = (ItemType)Enum.Parse(typeof(ItemType), nodeValue);

                // 10/5/2009 - Check if in PlayerItemTypeAtts Dictionary, which implies this item
                //             is a playable item, and not a sceneryitem!
                _isPlayableItemType = PlayableItemTypeAtts.ItemTypeAtts.ContainsKey(_itemTypeToUse);

                //
                // Add to the Terrain scene  
                //

                // 10/5/2009 - Create proper SceneItem type; sceneryItem or playableItem.
                if (_isPlayableItemType)
                {
                    // Which PlayableItem 'ItemGroupType' is this?
                    var player = _players[_thisPlayer]; // 6/15/2010
                    CreatePlayableItem(_itemTypeToUse, ref _placeItemAt, out _itemToPlace);

                    // 6/15/2010 - Updated to use Add method.
                    //_players[_thisPlayer].SelectableItems.Add(_itemToPlace as SceneItemWithPick);
                    Player.AddSelectableItem(player, _itemToPlace as SceneItemWithPick, false);
                }
                else
                {
                    _itemToPlace = new ScenaryItemScene(_game, _itemTypeToUse, ref _placeItemAt, 0);
                    _terrainShape.ScenaryItems.Add(_newItemScene as ScenaryItemScene); // 1/11/2010
                    _itemToPlaceInstanceKey = _itemToPlace.ShapeItem.ItemInstanceKey; // 4/14/2009

                    TerrainScreen.SceneCollection.Add(_itemToPlace);
                }

                // 7/1/2009 - Display itemkey; will be used by the Flood Generator.
                var itemTypeKey = (int)_itemTypeToUse;
                tbInstanceNumber.Text = itemTypeKey.ToString();
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
        private void RemoveOldSceneItem()
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
                    var player = _players[_thisPlayer]; // 6/15/2010
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
            
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            try // 6/20/2010
            {
                // Update Position of ItemToPlace
                if (_itemToPlace == null) return;

                // Get Position of Mouse Cursor in World Space 
                TerrainPickingRoutines.GetCursorPosByPickedRay(PickRayScale.NoChange, out _placeItemAt);
                _itemToPlace.Position = _placeItemAt;

                // 1/11/2010 - Updated to use the 'RightMouseButton Released'.
                // If user Right-Clicks on Terrain, then place SceneItemOwner at this Position
                if (!HandleInput.InputState.RightMouseButtonReleased || IsMouseInControl()) return;

                // 10/5/2009 - Create proper SceneItem type; sceneryItem or playableItem.
                if (_isPlayableItemType)
                {
                    // 10/7/2009
                    Player.AddSceneItem(_players[_thisPlayer], _itemTypeToUse, _placeItemAt);
                    return;
                }

                // Else, SceneryItem to add.
                _newItemScene = new ScenaryItemScene(_game, _itemTypeToUse, ref _placeItemAt, 0);
                _terrainShape.ScenaryItems.Add(_newItemScene as ScenaryItemScene);

                // 5/13/2008 - If PathBlocked, then let's update the A* GraphNodes
                if (_newItemScene.ShapeItem.IsPathBlocked)
                {
                    // 12/9/2008 - Set AStarGraph Costs
                    ((ScenaryItemScene)_newItemScene).SetAStarCostsForCurrentItem();

                    TerrainShape.PopulatePathNodesArray();
                }

                TerrainScreen.SceneCollection.Add(_newItemScene);
            }
            catch (Exception err)
            {
#if DEBUG
                Debug.WriteLine("timer1_Tick method threw an exception; " + err.Message ?? "No Message.");
#endif
            }
            
        }

        // 10/5/2009 - Creates the proper PlayableItem, and passes back via the OUT param.
        private void CreatePlayableItem(ItemType itemTypeToUse, ref Vector3 placeItemAt, out SceneItem itemToPlace)
        {
            itemToPlace = null;

            try // 6/20/2010
            {
                PlayableItemTypeAttributes playableAtts;
                if (!PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(itemTypeToUse, out playableAtts)) return;

                switch (playableAtts.ItemGroupType)
                {
                    case ItemGroupType.Buildings:
                        itemToPlace = new BuildingScene(_game, itemTypeToUse, ref placeItemAt, 0);

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

                        itemToPlace = new DefenseScene(_game, itemTypeToUse, ItemGroupType.Vehicles, ref placeItemAt, 0);

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
                        itemToPlace = new SciFiTankScene(_game, itemTypeToUse, ItemGroupType.Vehicles,
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
                        itemToPlace = new SciFiAircraftScene(_game, itemTypeToUse, ItemGroupType.Vehicles,
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
        private bool IsMouseInControl()
        {
            var isIn = false;
            try // 6/20/2010
            {
                _mousePoint.X = MousePosition.X;
                _mousePoint.Y = MousePosition.Y;

                // set this Form's ClientRectangle            
                _rectangle.X = Location.X + 5;
                _rectangle.Y = Location.Y + 5;
                _rectangle.Width = Width - 5;
                _rectangle.Height = Height - 5;
                
                _rectangle.Contains(ref _mousePoint, out isIn);
               
            }
            catch (Exception err)
            {
#if DEBUG
                Debug.WriteLine("IsMouseInControl method threw an exception; " + err.Message ?? "No Message.");
#endif
            }

            return isIn;
            
        }

        // 7/1/2009
        /// <summary>
        /// Generates a NoiseMap, using the Perlin Noise Algorithm.  The noise data will be used
        /// to flood the terrain with the choose 'ItemType', using the perlin noise values (0-255) to determine
        /// the positions of the given items.
        /// </summary>
        private void btnGeneratePerlinNoise_Click(object sender, EventArgs e)
        {
            try
            {
                // Read values in from 'PerlinNoise' Group controls            
                var randomSeedP1 = (int)nudRandomSeedValue.Value;
                var perlinNoiseSizeP1 = (float)nudNoiseSize.Value;
                var perlinPersistenceP1 = (float)nudPersistence.Value;
                var perlinOctavesP1 = (int)nudOctaves.Value;

                // Generate PerlinNoise '_noiseData'.
                _noiseData = TerrainData.CreatePerlinNoiseMap(randomSeedP1, perlinNoiseSizeP1, perlinPersistenceP1,
                                                              perlinOctavesP1);

                // Populate Color 'Bits', used to store into texture map
                // Set Image into PictureBox on Form.
                pictureBox.BackgroundImage = TerrainData.CreateBitmapFromPerlinNoise(_noiseData); 
            }
            catch (Exception err)
            {
#if DEBUG
                Debug.WriteLine("btnGeneratePerlinNoise_Click method threw an exception; " + err.Message ?? "No Message.");
#endif
            }
            
        }
        

        // 7/1/2009
        /// <summary>
        /// Iterate through all terrain positions, comparing to the 'NoiseData' array to determine
        /// 'Positions' to use for flooding.  Returns a List of Positions.
        /// </summary>
        private void btnGenerateFloodList_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if noise data is null or zero Count
                if (_noiseData == null || _noiseData.Count == 0)
                {
                    txtFloodErrorMessages.Text =
                        @"A Perlin Noise MUST be generated first, before the automatic Flood generator can be run.";
                    return;
                }

                // make sure ItemType was chosen.
                if (string.IsNullOrEmpty(tbInstanceNumber.Text))
                {
                    txtFloodErrorMessages.Text =
                        @"An 'SceneItemOwner' MUST be chosen first, before the automatic Flood generator can be run.";
                    return;
                }

                // Flood Terrain with choosen SceneItemOwner, using the _noiseData.            
                // Random 
                var spacing = (int)nupSpacing.Value;
                var densitySpacing = (int)nupDensitySpacing.Value;
                var random = new Random();
                _treeList.Clear();

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
                        if (terrainHeight <= (int)nupHeightMin.Value || terrainHeight >= (int)nupHeightMax.Value)
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
                        if (noiseValueAtCurrentPosition > (float)nupNoiseGreater_Lv3.Value)
                            treeDensity = (int)nupDensity_Lv3.Value;
                        else if (noiseValueAtCurrentPosition > (float)nupNoiseGreater_Lv2.Value)
                            treeDensity = (int)nupDensity_Lv2.Value;
                        else if (noiseValueAtCurrentPosition > (float)nupNoiseGreater_Lv1.Value)
                            treeDensity = (int)nupDensity_Lv1.Value;
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

                            _treeList.Add(treePosition);
                        } // End For Density
                    } // End For X/Y loop
                }


                // Set Total Count into text window
                txtTotalCountInFloodList.Text = _treeList.Count.ToString();
            }
            catch (Exception err)
            {
#if DEBUG
                Debug.WriteLine("btnGenerateFloodList_Click method threw an exception; " + err.Message ?? "No Message.");
#endif
            }
            
        }

        // 7/1/2009
        /// <summary>
        /// Uses the 'TreeList' of positions, generated in the FloodList operation, to flood the
        /// terrain with given Itemtype.
        /// </summary>
        private void btnPerformFlood_Click(object sender, EventArgs e)
        {
            try
            {
                // Check TreeList to make sure not empty
                var count = _treeList.Count; // 11/21/09
                if (count == 0)
                {
                    txtFloodErrorMessages.Text =
                        @"A 'FloodList' must be generated first, before the automatic Flood generator can be run.";
                    return;
                }


                // 11/20/2009 - Check if in PlayerItemTypeAtts Dictionary, which implies this item
                //             is a playable item, and not a sceneryitem!
                _isPlayableItemType = PlayableItemTypeAtts.ItemTypeAtts.ContainsKey(_itemTypeToUse);

                // 10/5/2009 - Make sure flooding is done with a ScenearyItemScene instance!
                if (_isPlayableItemType)
                {
                    txtFloodErrorMessages.Text =
                        @"You can only flood with ScenaryItems, not PlayableItems.";
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
                        position = _treeList[i]
                    };

                    // 7/2/2009 - Apply random rotations for each instance
                    var angle = MathUtils.RandomBetween(0, 360);
                    scenaryItemData.rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, angle);


                    scenaryItemsToAdd.Add(scenaryItemData);
                }

                // Call 'ScenaryItemScene' constructor, with the List of data to add.
                _newItemScene = new ScenaryItemScene(_game, _itemTypeToUse, scenaryItemsToAdd, 0);
                TerrainScreen.SceneCollection.Add(_newItemScene);
                _terrainShape.ScenaryItems.Add(_newItemScene as ScenaryItemScene);

                // Save ref to '_newItemScene', for Undo actions.
                _sceneItemForUndo = _newItemScene as ScenaryItemScene;

                // Clear any error messages from display
                txtFloodErrorMessages.Text = string.Empty;
            }
            catch (Exception err)
            {
#if DEBUG
                Debug.WriteLine("btnPerformFlood_Click method threw an exception; " + err.Message ?? "No Message.");
#endif
            }
            
        }

        // 7/2/2009
        /// <summary>
        /// Undoes the last Terrain Flood operation, for the given 'TreeList'.
        /// </summary>
        private void btnUndoLastFlood_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Check 'SceneITemForUndo' to make sure not null
                if (_sceneItemForUndo == null)
                {
                    txtFloodErrorMessages.Text =
                        @"A 'Flood' ACTION must be generated first, before any 'Undo' operation can occur.";
                    return;
                }

                // Delete the 'SceneItemForUndo' from Terrain.
                var terrainScreen = (TerrainScreen)_game.Services.GetService(typeof(TerrainScreen));
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

        // 7/4/2009
        /// <summary>
        /// Rebuilds the tvITems TreeView, by iterating through the entire 'Models' directory, and adding
        /// every folder, subfolder, & file, to the treeView.  Also, the ImageList used for the treeView will
        /// be populated by retrieving all bitmaps from the 'ItemToolPics' folder.
        /// </summary>
        private void btnPopulate_Click(object sender, EventArgs e)
        {
            // Show ProgressBar
            populateProgressBar.Show();

            // clear tree first
            tvItems.Nodes.Clear();

            // Populate ImageList, using the 'ItemToolPics' folder to retrieve pics.
            {
                var imagesDirPath =
                    Path.GetDirectoryName(Environment.GetEnvironmentVariable("VisualStudioDir") +
                                          @"\\Projects\\XNA_RTS2008\\Dev\\ItemToolPics\\");

                var directoryForItemToolPics = new DirectoryInfo(imagesDirPath);
                BuildImageList(this, directoryForItemToolPics);
            }


            // 9/23/2009 - Iterate the 'contentSearchList'.
            var count = _contentSearchLocations.Count; // 5/28/2010
            for (var i = 0; i < count; i++)
            {
                var modelsDirPath = Path.GetDirectoryName(_contentSearchLocations[i]);

                var directoryForModels = new DirectoryInfo(modelsDirPath);
               
                // Populate tree recursively
                BuildTreeList(directoryForModels, tvItems.Nodes);
            }
           
            // Hide ProgressBar
            populateProgressBar.Hide();
        }

        // 7/4/2009; 6/22/2010 - Made Static.
        /// <summary>
        /// Populates the ImageList with Bitmap images, contained in the 'ItemToolPics' folder, where
        /// the name of the image ends with 'Pic'.
        /// </summary>
        /// <param name="itemsTools"></param>
        /// <param name="directoryForItemToolPics"></param>
        private static void BuildImageList(ItemsTools itemsTools, DirectoryInfo directoryForItemToolPics)
        {
            try // 6/22/2010
            {
                // Get all files from directory
                var files = directoryForItemToolPics.GetFiles("*Pic.*", SearchOption.AllDirectories);

                // Clear all items from 'ImageList', except first two, which are the 'Folder' icons.
                var folderIcon1 = itemsTools.TextureIcons.Images[0];
                var folderIcon2 = itemsTools.TextureIcons.Images[1];
                
                // 7/4/2010 - Save copy of folder icons
                folderIcon1.Save("folderIcon1.png", ImageFormat.Png);
                folderIcon2.Save("folderIcon2.png", ImageFormat.Png);

                var folderIcon1KeyName = itemsTools.TextureIcons.Images.Keys[0];
                var folderIcon2KeyName = itemsTools.TextureIcons.Images.Keys[1];
                itemsTools.TextureIcons.Images.Clear();
                itemsTools.TextureIcons.Images.Add(folderIcon1KeyName, folderIcon1);
                itemsTools.TextureIcons.Images.Add(folderIcon2KeyName, folderIcon2);

                itemsTools.TextureIconsBig.Images.Clear();

                // iterate the 'Files' list, and add each Bitmap to the ImageList            
                itemsTools.populateProgressBar.Maximum = files.Length;
                itemsTools.populateProgressBar.Step = 1;
                foreach (var file in files)
                {
                    var bitmapPic = new Bitmap(file.FullName);

                    // Get filename without extension, to use as key for collection
                    var keyName = Path.GetFileNameWithoutExtension(file.FullName);

                    itemsTools.TextureIcons.Images.Add(keyName, bitmapPic);
                    itemsTools.TextureIconsBig.Images.Add(keyName, bitmapPic);

                    // Update progessBar
                    itemsTools.populateProgressBar.PerformStep();
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("BuildImageList method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/4/2009
        /// <summary>
        /// Recursively builds the 'TreeView' folder list, using the given directory and the
        /// some TreeNodeCollection to populate.  
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="childNodes"></param>
        private void BuildTreeList(DirectoryInfo directory, TreeNodeCollection childNodes)
        {
            try // 6/22/2010
            {
                // Populate subDirectories for given directory
                foreach (var sub in directory.GetDirectories())
                {
                    // 9/23/2009 - Skip folders with names of 'obj' or 'bin'.
                    if (sub.Name == "bin" || sub.Name == "obj")
                        continue;

                    var subNode = new TreeNode(sub.Name)
                                      {
                                          ImageKey = @"Re_Folder-closed_24.bmp",
                                          SelectedImageKey = @"Re_Folder-open_24.bmp"
                                      };

                    // Set Folder Pics

                    // If another level of directories/folders, then call method again recursively.
                    if (sub.GetDirectories().Length > 0 || sub.GetFiles().Length > 0)
                    {
                        BuildTreeList(sub, subNode.Nodes);
                    }
                    childNodes.Add(subNode);
                }

                // Populate Files for given directory
                var files = directory.GetFiles();
                populateProgressBar.Maximum = files.Length;
                populateProgressBar.Step = 1;
                foreach (var fi in files)
                {
                    // Only add files with the '.x' or '.fbx' extensions.
                    if (((fi.Extension != ".X" && fi.Extension != ".FBX") && fi.Extension != ".x") && fi.Extension != ".fbx")
                        continue;

                    // Remove Extension from name
                    var fileName = Path.GetFileNameWithoutExtension(fi.FullName);

                    var subNode = new TreeNode(fileName);

                    // verify pic in imageLists
                    if (TextureIconsBig.Images.ContainsKey(fileName + "Pic"))
                    {
                        // set SceneItemOwner pics
                        subNode.ImageKey = fileName + @"Pic";
                        subNode.SelectedImageKey = fileName + @"Pic";
                    }
                    else
                        Debug.WriteLine(String.Format("FileName '{0}' does not exist in 'TextureIconsBig' ImageList.", fileName));

                    // verify in list, or write to debugger the error
                    try
                    {
#pragma warning disable 168
                        var itemTypeExist = (ItemType)Enum.Parse(typeof(ItemType), fileName);
#pragma warning restore 168
                    }
                    catch (ArgumentException)
                    {
                        Debug.WriteLine(String.Format("FileName '{0}' does not match any 'ItemType' Enum.", fileName));
                    }

                    // set 'Tag' to Enum of SceneItemOwner, which should be the same file name.
                    subNode.Tag = fileName;

                    childNodes.Add(subNode);

                    // Update progessBar
                    populateProgressBar.PerformStep();
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("BuildTreeList method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }
// ReSharper restore InconsistentNaming
        // 12/7/2009
        /// <summary>
        /// Captures the close event, and cancels the close action; instead, the form is simply hidden
        /// from view by setting the 'Visible' flag to False.  This fixes the issue of the HEAP crash 
        /// when trying to re instantiate a form, after it was already created in the 'TerrainEditRoutines' class.
        /// </summary>
        private void ItemsTools_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Visible = false;

            // 6/28/2010 - Remove any still attached 'ItemToPlace' asset.
            RemoveOldSceneItem();
        }
    }
}