#region File Description
//-----------------------------------------------------------------------------
// TerrainStorageRoutines.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
#if !XBOX360
using System.Windows.Forms;
#endif
using AStarInterfaces.AStarAlgorithm.Structs;
using Microsoft.Xna.Framework;
using TWEngine.GameScreens;
using TWEngine.GameScreens.Generic;
using TWEngine.IFDTiles;
using TWEngine.InstancedModels;
using TWEngine.InstancedModels.Enums;
using TWEngine.Interfaces;
using TWEngine.ItemTypeAttributes;
using TWEngine.ItemTypeAttributes.Structs;
using TWEngine.Players;
using TWEngine.SceneItems;
using TWEngine.Shadows;
using TWEngine.Shadows.Structs;
using TWEngine.Terrain.Structs;
using TWEngine.TerrainTools;
using TWEngine.Utilities;
using TWEngine.Utilities.Enums;
using TWEngine.Utilities.Structs;
using TWEngine.Water;
using TWEngine.Water.Structs;


namespace TWEngine.Terrain
{
    ///<summary>
    /// The <see cref="TerrainStorageRoutines"/> class provides basic save and load routines, used
    /// to save game data.
    ///</summary>
    public class TerrainStorageRoutines : ITerrainStorageRoutines, IDisposable
    {
        // 8/13/2008 - Game Ref
        private static Game _gameInstance;

        // 10/31/2008 - Thread members for loading textures
        private static volatile List<TexturesGroupData> _tmpGroupData1;
        private static volatile List<TexturesGroupData> _tmpGroupData2;
        private static volatile List<TexturesAtlasData> _tmpGroupData3;
        internal static Thread LoadDataThread;
        internal static Thread LoadDataThread2;
        internal static string LoadMapName;
        internal static string LoadMapType; // 11/17/09 - MP or SP type.

       

        ///<summary>
        /// Contructor
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        public TerrainStorageRoutines(Game game)
        {
            // 8/13/2008
            _gameInstance = game;

            // Get Interface to ITerrainScreen
        }

#if !XBOX360
        // 4/23/2008 - Saves Terrain Data using the Terrain Struct and passing it to the
        //             Storage Class.

        // 9/16/2008: Add new 'ProjectSavePath' Attribute, which allows the Storage class to move the BMP file
        //            to the final location, which should be inside the Project's Content folder.  This allows
        //            the changing of the final location by simply changing the attribute below! - Ben
        // 1/8/2009: Updated to save to the 'ContentMaps' folder.

        #region Save Method Routines

        /// <summary>
        /// Saves the <see cref="Terrain"/> meta-data, like heights, ground textures, waypoints, quads, etc.
        /// </summary>
        /// <param name="mapName">Map name</param>
        /// <param name="mapType">Map type; either SP or MP.</param>
        public void SaveTerrainData(string mapName, string mapType)
        {
            // 8/12/2008 - 

            // Create the data to save; XML will can only save simple values, like Ints and Floats,
            // but not some Structs, like Texture2D.
            var data = new SaveTerrainData
                           {
                               MapHeight = TerrainData.MapHeight,
                               MapWidth = TerrainData.MapWidth,
                               AmbientColorLayer1 = TerrainScreen.TerrainShapeInterface.AmbientColorLayer1 /* 4/24/2009*/,
                               AmbientPowerLayer1 = TerrainScreen.TerrainShapeInterface.AmbientPowerLayer1 /* 4/24/2009*/,
                               SpecularColorLayer1 = TerrainScreen.TerrainShapeInterface.SpecularColorLayer1 /* 4/24/2009*/,
                               SpecularPowerLayer1 = TerrainScreen.TerrainShapeInterface.SpecularPowerLayer1 /* 4/24/2009*/,
                               AmbientColorLayer2 = TerrainScreen.TerrainShapeInterface.AmbientColorLayer2 /* 5/8/2009*/,
                               AmbientPowerLayer2 = TerrainScreen.TerrainShapeInterface.AmbientPowerLayer2 /* 5/8/2009*/,
                               SpecularColorLayer2 = TerrainScreen.TerrainShapeInterface.SpecularColorLayer2 /* 5/8/2009*/,
                               SpecularPowerLayer2 = TerrainScreen.TerrainShapeInterface.SpecularPowerLayer2 /* 5/8/2009*/,
                               AlphaLy1Percent = TerrainScreen.TerrainShapeInterface.AlphaMaps.AlphaLy1Percent,
                               AlphaLy2Percent = TerrainScreen.TerrainShapeInterface.AlphaMaps.AlphaLy2Percent,
                               AlphaLy3Percent = TerrainScreen.TerrainShapeInterface.AlphaMaps.AlphaLy3Percent,
                               AlphaLy4Percent = TerrainScreen.TerrainShapeInterface.AlphaMaps.AlphaLy4Percent,
                               quadParentsTessellated = TerrainScreen.TerrainShapeInterface.QuadParentsTessellated,
                               lightPosition = TerrainShape.LightPosition /* 6/5/2009*/,
                               IsRaining = TerrainScreen.IsRaining /* 6/5/2009*/,
                               SaveSelectableItemsWithMap = TerrainScreen.SaveSelectableItemsWithMap, /* 10/7/2009 */
                               quadLOD3 = TerrainData.GetQuadKeysOfLOD3(), // Save Ref to which Quad's are LOD-3.
                               textureGroupData1 = ConvertDictionaryToList(TerrainScreen.TerrainShapeInterface.TextureGroupData1),// Save TextureGroupData1 Array which contains the textures selected for Group-1 in PaintTool.
                               textureGroupData2 = ConvertDictionaryToList(TerrainScreen.TerrainShapeInterface.TextureGroupData2), // 10/7/2009 - Updated to use Generic method.
                               texturesAtlasData = TerrainShape.GetVolumeTextureDataAsList(), // 1/20/2009 - Save Volume Texture Data
                           };
           

            // 1/21/2009 - Save Water Data Attributes
            WaterData waterData;
            WaterManager.GetWaterDataAttributes(out waterData);
            data.waterData = waterData;

            // 6/5/2009 - Save ShadowMap Data Attributes
            ShadowMapData shadowMapData;
            ShadowMap.GetShadowMapDataAttributes(out shadowMapData);
            data.shadowMapData = shadowMapData;
            
            // 3/4/2009 - Save AStarGraph Blocking nodes.
            if (TemporalWars3DEngine.AStarGraph != null)
                data.blockingData = TemporalWars3DEngine.AStarGraph.GetPathfindingGraph();

            // 5/15/2009 - Save PerlinNoise Data
            TerrainShape.GetPerlinNoiseData(ref data);

            // Create Storage Class and pass Stuct to it
            var storageTool = new Storage();
           
            // 10/7/2009 - Save QuadMetaData
            TerrainData.SaveQuadMetaData(ref data, storageTool, mapName, mapType);

            // 4/9/2009 - Save the MapMarkersPosition data
            TerrainShape.SaveMapMarkersPositionData(storageTool, mapName, mapType);

            // 9/28/2009 - Save the TriggerAreas data
            TerrainTriggerAreas.SaveTriggerAreas(storageTool, mapName, mapType);

            // 10/14/2009 - Save the Waypoints data
            TerrainWaypoints.SaveWaypoints(storageTool, mapName, mapType);

            // Save HeightMap Data
            TerrainData.SaveHeightMapData(storageTool, mapName, mapType);

            // We now need to save the AlphaMaps            
            TerrainAlphaMaps.SaveAlphaMaps(storageTool, mapName, mapType);

            // Save Terrain Scenary Items
            SaveTerrainScenaryItems(mapName, mapType);

            // 10/7/2009
            // Save Terrain Selectable Items (Used for Scripting)
            if (TerrainScreen.SaveSelectableItemsWithMap)
                SaveTerrainSelectableItems(mapName, mapType);

            // Done Message            
            _gameInstance.Window.Title = "Save operation completed successfully.";
            
            // 1/10/2011
            MessageBox.Show("Save operation completed successfully.", "Save Map Data", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

        }

        // 4/30/2008:
        // 9/16/2008: Add new 'ProjectSavePath' Attribute, which allows the Storage class to move the BMP file
        //            to the final location, which should be inside the Project's Content folder.  This allows
        //            the changing of the final location by simply changing the attribute below! - Ben
        // 10/29/2008: Updated to use the new method 'StartSave_ScenaryItemOperation', which saves directly as binary file.
        // 1/8/2009: Updated to save to the 'ContentMaps' folder.
        /// <summary>
        /// Saves the <see cref="Terrain"/> <see cref="ScenaryItemScene"/> items, like houses, trees, bushes, etc.
        /// </summary>
        /// <param name="mapName">Map name</param>
        /// <param name="mapType">Map type; either SP or MP.</param>
        private static void SaveTerrainScenaryItems(string mapName, string mapType)
        {
            // extract data from TerrainShape 'ScenaryItems' array.
            List<ScenaryDataProperties> tmpItemProperties;
            var tmpItemTypes = TerrainShape.GetScenaryItemsData(out tmpItemProperties);

            // Create the data to save
            var data = new SaveTerrainScenaryData { itemTypes = tmpItemTypes, itemProperties = tmpItemProperties };

            // 4/6/2010: Updated to 'ContentMapsLoc' global var.
            // Create Storage Class and pass Stuct to it
            var storageTool = new Storage();

            // 1/8/2009: Updated to save to the 'ContentMaps' folder.
            int errorCode;
            if (!storageTool.StartSave_ScenaryItemOperation(data, "tdScenaryMetaData.tsd",
                                                            String.Format(@"{0}\{1}\{2}\",TemporalWars3DEngine.ContentMapsLoc, mapType, mapName), out errorCode))
            {
                // 4/8/2010 - Error occured, so check which one.
                if (errorCode == 1)
                {
                    MessageBox.Show(@"Locked files detected for 'ScenaryItems' save.  Unlock files, and try again.",
                                    @"Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (errorCode == 2)
                {
                    MessageBox.Show(@"Directory location for 'ScenaryItems' save, not found.  Verify directory exist, and try again.",
                                    @"Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                throw new InvalidOperationException("The Save Struct 'scenary' data Operation Failed.");
            }

            // Release Resources
            tmpItemTypes.Clear();
            tmpItemProperties.Clear();
        }
       

        // 10/7/2009
        /// <summary>
        /// Saves the <see cref="Terrain"/> Selectables, like Tanks, Aircraft and Buildings, which are specifically used
        /// in scripting levels.  (Scripting Purposes)
        /// </summary>
        /// <param name="mapName">Map name</param>
        /// <param name="mapType">Map type; either SP or MP.</param>
        private static void SaveTerrainSelectableItems(string mapName, string mapType)
        {
            List<SelectablesDataProperties> tmpItemProperties;
            var tmpItemTypes = TerrainShape.GetSelectableItemsData(out tmpItemProperties);

            // Create the data to save
            var data = new SaveTerrainSelectablesData { itemTypes = tmpItemTypes, itemProperties = tmpItemProperties };

            // 4/6/2010: Updated to use 'ContentMapsLoc' global var.
            // Create Storage Class and pass Stuct to it
            var storageTool = new Storage();
            int errorCode;
            if (!storageTool.StartSaveOperation(data, "tdSelectablesMetaData.tsd",
                                                            String.Format(@"{0}\{1}\{2}\", TemporalWars3DEngine.ContentMapsLoc, mapType, mapName), out errorCode))
            {
                // 4/7/2010 - Error occured, so check which one.
                if (errorCode == 1)
                {
                    MessageBox.Show(@"Locked files detected for 'SelectablesMetaData' (tdSelectablesMetaData.tsd) save.  Unlock files, and try again.",
                                    @"Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (errorCode == 2)
                {
                    MessageBox.Show(@"Directory location for 'SelectablesMetaData' (tdSelectablesMetaData.tsd) save, not found.  Verify directory exist, and try again.",
                                    @"Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                throw new InvalidOperationException("The Save Struct 'Selectables' data Operation Failed.");
            }

            // Release Resources
            tmpItemTypes.Clear();
            tmpItemProperties.Clear();
        }



        // 10/7/2009
        /// <summary>
        /// Takes a dictionary, and return the items as a List collection.
        /// </summary>
        /// <typeparam name="TList">Generic value</typeparam>
        /// <param name="dictionary">Dictionary to convert</param>
        /// <returns><see cref="List{TList}"/> collection</returns>
        private static List<TList> ConvertDictionaryToList<TList>(Dictionary<int, TList> dictionary)
        {
            return dictionary.Select(kvp => kvp.Value).ToList();
        }

        #endregion

#endif

        #region Load Method Routines

        // 9/15/2008 - Will start the Loading in seperate threads.        
        ///<summary>
        /// Starts an internal <see cref="Thread"/>, which loads the terrain data.
        ///</summary>
        /// <param name="mapName">Map name</param>
        /// <param name="mapType">Map type; either SP or MP.</param>
        public static void LoadTerrainData(string mapName, string mapType)
        {
            // Start LoadTerrain THREAD...
            LoadMapType = mapType; // 11/17/09
            LoadMapName = mapName;
            LoadDataThread = new Thread(LoadTerrainDataThreadMethod) {Name = "LoadTerrainData", IsBackground = false};
            LoadDataThread.Start();

            // 7/16/2009 - Start IFD Tiles THREAD...
            IFDTileTextureLoader.PreLoadIFDTileSet(ItemGroupType.Airplanes);
            IFDTileTextureLoader.PreLoadIFDTileSet(ItemGroupType.Vehicles);
        }

        // Thread Method
        // 9/15/2008 -  
        /// <summary>
        /// Loads terrain data and stores into structs, like <see cref="TexturesGroupData"/> or <see cref="TexturesAtlasData"/>.  
        /// Then passes it to the storage Class.   
        /// </summary>
        private static void LoadTerrainDataThreadMethod()
        {
            // Set XBOX-360 CPU Core (3) for thread            
#if XBOX360
            Thread.CurrentThread.SetProcessorAffinity(3);
#endif

            

            var mapName = LoadMapName;
            var mapType = LoadMapType; // 11/17/09

            // 8/12/2008 - Clear out all Dictionaries/List in TerrainShape
            var terrainShape = TerrainScreen.TerrainShapeInterface; // 8/12/2009
            terrainShape.TerrainBoundingBoxes.Clear();
            terrainShape.QuadParentsTessellated.Clear();
            terrainShape.TextureGroupData1.Clear();
            terrainShape.TextureGroupData2.Clear();

            // Create the Struct to hold the return data
            SaveTerrainData data;

            // Show Loading Message            
            //GameInstance.Window.Title = "Loading Quad Data...";
            LoadingScreen.LoadingMessage = "Loading Quad Data";

            // 4/6/2010: Updated to use the new 'ContentMapsLoc' global var.
            // Create Storage Class and pass Struct to it
            var storageTool = new Storage();
            List<int> tmpParentsTessellated; // = new List<int>();
            List<int> tmpQuadLOD3; // = new List<int>();
            _tmpGroupData1 = new List<TexturesGroupData>();
            _tmpGroupData2 = new List<TexturesGroupData>();
            _tmpGroupData3 = new List<TexturesAtlasData>(); // 1/20/2009
            WaterData waterData; // = new WaterData(); // 1/21/2009
            ShadowMapData shadowMapData; // = new ShadowMapData(); // 6/5/2009
            List<PathNodeForSaving> tmpBlockingData; // = new List<pathNodeForSaving>(); // 3/4/2009
            // 1/8/2009: Updated to save to the 'ContentMaps' folder.
            if (storageTool.StartLoadOperation(out data, "tdQuadMetaData.sav",
                                               String.Format(@"{0}\{1}\{2}\",TemporalWars3DEngine.ContentMapsLoc, mapType, mapName),
                                               StorageLocation.TitleStorage))
            {
                // Assign Values from Data Struct back into Terrain Class
                TerrainData.MapHeight = data.MapHeight;
                TerrainData.MapWidth = data.MapWidth;
                terrainShape.AmbientColorLayer1 = data.AmbientColorLayer1; // 4/24/2009
                terrainShape.AmbientPowerLayer1 = data.AmbientPowerLayer1; // 4/24/2009
                terrainShape.SpecularColorLayer1 = data.SpecularColorLayer1; // 4/24/2009
                terrainShape.SpecularPowerLayer1 = data.SpecularPowerLayer1; // 4/24/2009
                terrainShape.AmbientColorLayer2 = data.AmbientColorLayer2; // 5/8/2009
                terrainShape.AmbientPowerLayer2 = data.AmbientPowerLayer2; // 5/8/2009
                terrainShape.SpecularColorLayer2 = data.SpecularColorLayer2; // 5/8/2009
                terrainShape.SpecularPowerLayer2 = data.SpecularPowerLayer2; // 5/8/2009
                terrainShape.AlphaMaps.AlphaLy1Percent = data.AlphaLy1Percent;
                terrainShape.AlphaMaps.AlphaLy2Percent = data.AlphaLy2Percent;
                terrainShape.AlphaMaps.AlphaLy3Percent = data.AlphaLy3Percent;
                terrainShape.AlphaMaps.AlphaLy4Percent = data.AlphaLy4Percent;
                TerrainShape.PerlinNoiseDataTexture1To2MixLayer1 = data.perlinNoiseDataTexture1To2Mix_Layer1;
                // 5/15/2009
                TerrainShape.PerlinNoiseDataTexture1To2MixLayer2 = data.perlinNoiseDataTexture1To2Mix_Layer2;
                // 5/15/2009
                TerrainShape.LightPosition = data.lightPosition; // 6/5/2009
                TerrainScreen.IsRaining = data.IsRaining; // 6/5/2009
                tmpParentsTessellated = data.quadParentsTessellated;
                tmpQuadLOD3 = data.quadLOD3;
                _tmpGroupData1 = data.textureGroupData1;
                _tmpGroupData2 = data.textureGroupData2;
                _tmpGroupData3 = data.texturesAtlasData; // 1/20/2009
                waterData = data.waterData; // 1/21/2009
                shadowMapData = data.shadowMapData; // 6/5/2009
                tmpBlockingData = data.blockingData; // 3/4/2009
                TerrainScreen.SaveSelectableItemsWithMap = data.SaveSelectableItemsWithMap; // 10/7/2009
            }
            else
                //_gameInstance.Window.Title = "The Load Struct data Operation Failed.";
                throw new InvalidOperationException("The Load Struct data Operation Failed.");


            // 4/8/2009 - Load MapMarkerPositions Struct data
            MapMarkerPositions mapMarkerData;
            TerrainShape.LoadMapMarkerPositionsData(storageTool, mapName, mapType, out mapMarkerData);
            terrainShape.MapMarkerPositions = mapMarkerData;
           

            // 9/29/2009 - Load TriggerAreas data
            TerrainTriggerAreas.LoadTriggerAreas(storageTool, mapName, mapType);

            // 10/14/2009 - Load Waypoints data
            TerrainWaypoints.LoadWaypoints(storageTool, mapName, mapType);

            TerrainData.SetupTerrainVertexBuffer();

#if !XBOX360
            TerrainData.SetupVertexDataAndVertexLookup();
#endif

            // We now need to load the AlphaMaps
            //LoadingScreen.LoadingMessage = "Creating AlphaMaps";
            TerrainAlphaMaps.LoadAlphaMaps(mapName, mapType);

            // 8/11/2008 - Creating new QuadTree            
            LoadingScreen.LoadingMessage = "Creating QuadTree";
            if (TerrainShape.RootQuadTree != null)
                TerrainShape.RootQuadTree.ClearQuadTree();
            TerrainShape.RootQuadTree = new TerrainQuadTree(_gameInstance, TerrainData.TerrainNormals.Count);
            // 5/19/2009 - Store the Total Count 'TreeLeafList' Array, used in the Draw method.
            TerrainQuadTree.TreeLeafCount = TerrainQuadTree.TreeLeafList.Count;

            // 7/10/2009
            GC.Collect();

            // 7/16/2009 - Start Scenary Items THREAD...
            LoadDataThread2 = new Thread(LoadTerrainSceneryItems) {Name = "LoadScenaryItems", IsBackground = false};
            LoadDataThread2.Start();

            //GameInstance.Window.Title = "Updating Terrain...";
            LoadingScreen.LoadingMessage = "Updating Terrain";
            
            // Tessellate Parents Quad's where necessary
            TerrainEditRoutines.TessellateQuads(tmpParentsTessellated, tmpQuadLOD3);

            LoadingScreen.LoadingMessage = "Updating Effect Textures";

            // 10/7/2009 - Load TextureData back into memory
            TerrainShape.LoadMapTextureData(terrainShape, mapName, mapType, _tmpGroupData1, _tmpGroupData2, _tmpGroupData3);

            // 1/21/2009 - Load Water Data Attributes back into memory.
            WaterManager.SetWaterDataAttributes(ref waterData);

            // 6/5/2009 - Load ShadowMap Data Attributes back into memory.
            ShadowMap.SetShadowMapDataAttributes(ref shadowMapData);

            // 3/4/2009 - Load AStarGraph PathNode Blocking Data
            var aStarGraph = TemporalWars3DEngine.AStarGraph; // 5/19/2010
            if (aStarGraph != null) // 1/13/2010
                aStarGraph.LoadAStarGraphBlockingData(tmpBlockingData, TemporalWars3DEngine._pathNodeStride);

            // Apply Textures into Effect Class.
            TerrainShape.UpdateEffectDiffuseTextures();
            // 7/31/2008 - Apply BumpMap Textures into Effect Class.
            TerrainShape.UpdateEffectBumpMapTextures();

            // Normalize Terrain
            LoadingScreen.LoadingMessage = "Normalizing Terrain";
            var rootQuadTree = TerrainShape.RootQuadTree;
            TerrainData.RebuildNormals(ref rootQuadTree);

            // 10/7/2009 - Load Selectable Items, used for Scripting.
            if (TerrainScreen.SaveSelectableItemsWithMap)
                LoadTerrainSelectableItems(storageTool, mapName, mapType);
            
            LoadingScreen.LoadingMessage = null;

            // 8/21/2008 - Release Resources
            tmpQuadLOD3.Clear();
            if (_tmpGroupData1 != null)
                _tmpGroupData1.Clear();
            if (_tmpGroupData2 != null)
                _tmpGroupData2.Clear();
            if (_tmpGroupData3 != null)
                _tmpGroupData3.Clear();
            _tmpGroupData1 = null;
            _tmpGroupData2 = null;
            _tmpGroupData3 = null;
            

            Thread.CurrentThread.Abort();
        }
      

        // 4/30/2008: 
        // 8/26/2008: Updated to optimize memory.
        // 9/15/2008: Changed into a Thread Method.
        // 10/30/2008: Updated to use the new Content Pipeline method of loading the 'ScenaryData'.        
        ///<summary>
        /// Loads the <see cref="Terrain"/> <see cref="ScenaryItemScene"/> items, like houses, trees, bushes, etc.
        ///</summary>
        public static void LoadTerrainSceneryItems()
        {
            // Set XBOX-360 CPU Core (5) for thread            
#if XBOX360
            Thread.CurrentThread.SetProcessorAffinity(5);
#endif

            LoadingScreen.LoadingMessage = "Loading Scenary Items";
           
            var mapName = LoadMapName;
            var mapType = LoadMapType; // 11/17/09

            // 8/11/2008 - Delete all current ScenaryItems
            TerrainScreen.TerrainShapeInterface.ScenaryItems.Clear();
            // 8/12/2008 - Clear all Instance Models World Transforms            
            InstancedItem.ClearAllInstanceModelsTransforms();

            // 10/30/2008 - Load using the Content Pipeline '.xnb' method.
            var items = TemporalWars3DEngine.ContentMaps.Load<List<ScenaryItemScene>>(
                String.Format(@"{0}\{1}\tdScenaryMetaData", mapType, mapName));

            LoadingScreen.LoadingMessage = "Copying Scenary Items into Terrain";

            // Add to Scene to be shown on screen           
            TerrainScreen.SceneCollection.AddRange(items.ToArray());
            // Add to ScenaryItems Array for use in Save Routine and Smoothing Algorithm.
            TerrainScreen.TerrainShapeInterface.ScenaryItems.AddRange(items);

            // 10/8/2009 - Add Array to 'Names' Dictionary, if necessary.
            Player.AddSceneItemToNamesDictionary(items);

            // DEBUG: Populate the pathNodes Array
            TerrainShape.PopulatePathNodesArray();

            // Release Resources
            LoadingScreen.LoadingMessage = "Scenary Items Thread Finished";

            Thread.CurrentThread.Abort();
        }

        // 10/7/2009; 1/15/2011 - Updated to iterate Player colletion.
        /// <summary>
        /// Loads the <see cref="Terrain"/> Selectable items, like Tanks, Aircraft and Buildings, which are specifically used for single player
        /// levels, via scripting conditions.
        /// </summary>
        /// <param name="storageTool"><see cref="Storage"/> instance</param>
        /// <param name="mapName">Map name</param>
        /// <param name="mapType">Map type; either SP or MP.</param>
        private static void LoadTerrainSelectableItems(Storage storageTool, string mapName, string mapType)
        {
            // 11/4/2009 - If Network game, then skip loading this section!
            
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player thisPlayer;
            TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out thisPlayer);

            if (thisPlayer == null) return;

            if (thisPlayer.NetworkSession != null) return;

            LoadingScreen.LoadingMessage = "Loading Selectable Items";
            
            // 4/6/2010: Updated to use new 'ContentMapsLoc' global var.
            // 4/8/2009 - Load MapMarkerPositions Struct data
            SaveTerrainSelectablesData terrainSelectablesData;
            if (!storageTool.StartLoadOperation(out terrainSelectablesData, "tdSelectablesMetaData.tsd",
                                                String.Format(@"{0}\{1}\{2}\",TemporalWars3DEngine.ContentMapsLoc, mapType, mapName),
                                                StorageLocation.TitleStorage)) return;

            // set with values loaded back into memory.
            var itemTypes = terrainSelectablesData.itemTypes; // 5/19/2010 - Cache
            var selectablesDataPropertieses = terrainSelectablesData.itemProperties; // 5/19/2010 - Cache

            var count = itemTypes.Count;
            for (var i = 0; i < count; i++)
            {
                // get ItemType
                var itemType = itemTypes[i];

                // Get ItemTypeProperties
                var itemTypeProperties = selectablesDataPropertieses[i];

                // 1/15/2011 - Retrieve PlayerNumber from ItemTypeProperties
                Player player;
                TemporalWars3DEngine.GetPlayer(itemTypeProperties.playerNumber, out player);
                
                // Create SceneItem
                var sceneItemNumber = Player.AddSceneItem(player, itemType, itemTypeProperties.position);

                // Get Instance just created, to finish populating with data
                SceneItemWithPick newSceneItem;
                Player.GetSelectableItem(player, sceneItemNumber, out newSceneItem);

                // 11/6/2009 - Get ItemType atts.
                PlayableItemTypeAttributes itemAtts;
                if (PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(itemType, out itemAtts))
                {
                    // Set Group to attack.
                    newSceneItem.ItemGroupTypeToAttack = itemAtts.ItemGroupToAttack != null
                                                             ? itemAtts.ItemGroupToAttack.Value
                                                             : newSceneItem.ItemGroupTypeToAttack;

                    // try casting to buildingScene.
                    var buildingSceneItem = (newSceneItem as BuildingScene);

                    // if BuildingScene, then updates its ProductionType.
                    if (buildingSceneItem != null)
                        buildingSceneItem.ProductionType = itemAtts.ProductionType;
                }

                // Apply Properties
                newSceneItem.PlayerNumber = itemTypeProperties.playerNumber; // 10/20/2009
                newSceneItem.Rotation = itemTypeProperties.rotation;
                newSceneItem.Name = itemTypeProperties.name;
                newSceneItem.ShapeItem.PathBlockSize = itemTypeProperties.pathBlockSize;
                newSceneItem.ShapeItem.IsPathBlocked = itemTypeProperties.isPathBlocked;
                
            }
        }

        #endregion

        #region Dispose

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this final dispose?</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            // 1/8/2010 - Clear Arrays
            if (_tmpGroupData1 != null) _tmpGroupData1.Clear();
            if (_tmpGroupData2 != null) _tmpGroupData2.Clear();
            if (_tmpGroupData3 != null) _tmpGroupData3.Clear();

            // Clear Threads
            if (LoadDataThread != null) LoadDataThread.Abort();
            if (LoadDataThread2 != null) LoadDataThread2.Abort();
            LoadDataThread = null;
            LoadDataThread2 = null;

            // dispose managed resources
            _gameInstance = null;
        }

        #endregion
    }
}