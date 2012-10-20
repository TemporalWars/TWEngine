#region File Description
//-----------------------------------------------------------------------------
// InstancedItem.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.BeginGame.Enums;
using ImageNexus.BenScharbach.TWEngine.Common;
using ImageNexus.BenScharbach.TWEngine.Explosions.Structs;
using ImageNexus.BenScharbach.TWEngine.ForceBehaviors;
using ImageNexus.BenScharbach.TWEngine.GameCamera;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Structs;
using ImageNexus.BenScharbach.TWEngine.Interfaces;
using ImageNexus.BenScharbach.TWEngine.ItemTypeAttributes.Structs;
using ImageNexus.BenScharbach.TWEngine.Players;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWEngine.ScreenManagerC;
using ImageNexus.BenScharbach.TWEngine.Shadows;
using ImageNexus.BenScharbach.TWEngine.Shapes;
using ImageNexus.BenScharbach.TWEngine.Terrain;
using ImageNexus.BenScharbach.TWEngine.Terrain.Enums;
using ImageNexus.BenScharbach.TWEngine.Terrain.Structs;
using ImageNexus.BenScharbach.TWEngine.Utilities;
using ImageNexus.BenScharbach.TWLate.RTS_FogOfWarInterfaces.FOW;
using ImageNexus.BenScharbach.TWTools.PerfTimersComponent.Timers;
using ImageNexus.BenScharbach.TWTools.PerfTimersComponent.Timers.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TWEngine;
using System.Diagnostics;

namespace ImageNexus.BenScharbach.TWEngine.InstancedModels
{
    // 7/24/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.InstancedModels"/> namespace contains the classes
    /// which make up the entire <see cref="TWEngine.InstancedModels"/>.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
    
    /// <summary>
    /// The <see cref="InstancedItem"/> class is the manager for the <see cref="InstancedModel"/>
    /// items.  It's responsibilities include loading the <see cref="InstancedModel"/> into memory,
    /// and marshaling communication update requests between the <see cref="SceneItem"/> classes, like
    /// the <see cref="SciFiTankScene"/> class, and its required <see cref="InstancedModel"/> counterpart.
    /// For example, updates of <see cref="SceneItem"/> position data, for a specific instance, is communicate
    /// to this manager class, which in turn, will be updated to the proper <see cref="InstancedModel"/> item.
    /// </summary>
    public class InstancedItem : GameComponent
    {
#if !XBOX360
        // 7/31/2008 - Sets the RunTime ReadOnly Variable with Total
        //             SceneItemOwner Count of the Enum 'ItemType'.
        internal static readonly int ItemTypeCount = GetEnumCount();
#else
        internal static readonly int ItemTypeCount = 506;

#endif

        // 11/1/2009 - Content Zip files
#if XBOX360
        internal const string ContentplayableXzb = @"1ContentPlayable\xbox360\";
        internal const string ContentscenaryXzb = @"1ContentScenary\xbox360\";
#else
        internal const string ContentplayableXzb = @"1ContentPlayable\x86\";
        internal const string ContentscenaryXzb = @"1ContentScenary\x86\";
#endif
        // 3/3/2009 - TerrinShape
        private static ITerrainShape _terrainShape;

        // 7/9/2009; 6/1/2010 - Updated to 100 intial array, to avoid calling Array.Resize as much!
        private static int[] _keys = new int[100];
        private static int[] _itemTypeInstancesCache = new int[100]; // 6/7/2012
        private static readonly List<int> ItemTypeInstancesList = new List<int>(100);

        // 2/8/2010 - InstancedItem Culling Thread.
// ReSharper disable UnaccessedField.Local
        //private InstancedItemCulling _instancedItemCulling;
// ReSharper restore UnaccessedField.Local

        // 7/18/2008 - InstancedModel Batches 
        internal static bool InitInstanceArrayListsCompleted;
        // The Enum 'ItemType' will be used as the Index Key for a specific model type.        
        internal static InstancedModel[] InstanceModels;
        
        // 1/29/2009 - This is a List<int> of the ItemType enum values, the _keys, used for the 'InstanceModels' which cast shadows! (STATIC)
        private static List<int> _shadowInstanceModelsKeysStatic;
        // 8/6/2009 - This is a List<int> of the ItemType enum values, the _keys, used for the 'InstanceModels' which cast shadows! (Dynamic)
        private static List<int> _shadowInstanceModelsKeysDynamic;
        // 2/19/2009 - This ia a List<int> of the ItemType enum value, the _keys, used for the 'InstanceModels' which use the AlphaMap drawing method!
        private static List<int> _alphaMapInstanceModelsKeys;
        // 2/19/2009 - This ia a List<int> of the ItemType enum value, the _keys, used for the 'InstanceModels' which use the Ilumination drawing method!
        private static List<int> _illumMapInstanceModelsKeys;
        // 3/12/2009 - This ia a List<int> of the ItemType enum value, the _keys, used for the 'InstanceModels' which are 'Selectable' Items!
        private static List<int> _selectableItemsInstanceModelsKeys;
        // 7/10/2009 - This ia a List<int> of the ItemType enum value, the _keys, used for the 'InstanceModels' which are 'Scenery' Items!
        private static List<int> _sceneryItemsInstanceModelsKeys;
        
        // 2/16/2010 - Set when the camera is moved.
        private static volatile bool _cameraWasUpdated;

        // 11/1/2009 - Updated to use the 'ZippedContent' manager.
        internal static ContentManager PlayableItemsContentManager; // XNA 4.0 - Updated to non-zip content manager.
        internal static ContentManager ScenaryItemsContentManager; // 4/22/2009

        // Since each Instance of a class will be called to render, we need to set a flag to know when a 'Batch' of InstanceModels
        // has already been drawn for the given Render cycle, thereby only drawing the batch once per cycle.  The Flag is then reset
        // in the 'Update' Method for the next 'Batch' Render cycle!       
// ReSharper disable UnaccessedField.Local
        private static bool[] _instanceModelsDrawn;
// ReSharper restore UnaccessedField.Local
        private static bool[] _instanceShadowModelsDrawn;

        // Tracks the Total Items Created, and the Specific SceneItemOwner's Instance#.
        private static List<int> _itemCounterPerItemType;
        private static int _itemCounter;

        // 4/7/2009 - ThreadLock
        internal static readonly object InstanceModelsThreadLock = new object();
        

        ///<summary>
        /// Constructor for <see cref="InstancedItem"/>, creates the internal required <see cref="ZippedContent"/> managers, used
        /// in loading the playable and scenary <see cref="InstancedItem"/>.  Also, all static List arrays are initialized here.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        public InstancedItem(Game game)
            : base(game)
        {
            // 11/18/2008 - Store GameInstance in 'STATIC' variable, so Thread has access to it.

            // 3/26/2011
            if (PlayableItemsContentManager == null)
                PlayableItemsContentManager = new ContentManager(game.Services);

            // 4/22/2009
            if (ScenaryItemsContentManager == null)
                ScenaryItemsContentManager = new ContentManager(game.Services, ContentscenaryXzb);
                      
            // 3/3/2009
            _terrainShape = (ITerrainShape) game.Services.GetService(typeof (ITerrainShape));

            // 7/18/2008 - Init List 'instanceTransforms' & 'instancePicks' for all ItemTypes.
            if (!InitInstanceArrayListsCompleted)
            {
                // Init Static Arrays                
                InitializeStaticArrays();
            }

            // 2/16/2010 - Remove the need to use the IN-Culling, since already done by FOW and ScenaryITems by Quad.
            //_instancedItemCulling = new InstancedItemCulling();
           
            // 11/9/2008 - StopWatchTimers            
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.InstancedItemDraw, false);//"InstancedItem_Draw"
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.InstancedItemUpdate, false);//"InstancedItem_Update"

            // 7/22/2009 - Attach to CameraUpdated event, which will be used to update the 'InstanceModel' 
            //             static effectParam's 'View' & 'Proj'.
            Camera.CameraUpdated += CameraUpdated;

            // 9/23/2008 - DrawOrder
            //DrawOrder = 101;
        }

        // 11/7/2009
        /// <summary>
        /// Initalizes all internal static arrays for the given class.
        /// </summary>
        internal static void InitializeStaticArrays()
        {
            if (InstanceModels == null)
                InstanceModels = new InstancedModel[ItemTypeCount];
            // 1/29/2009 - Init Shadow Keys (STATIC)
            if (_shadowInstanceModelsKeysStatic == null)
                _shadowInstanceModelsKeysStatic = new List<int>();
            // 8/6/2009 - Init Shadow Keys (Dynamic)
            if (_shadowInstanceModelsKeysDynamic == null)
                _shadowInstanceModelsKeysDynamic = new List<int>();
            // 2/19/2009 - Init AlphaMap Keys
            if (_alphaMapInstanceModelsKeys == null)
                _alphaMapInstanceModelsKeys = new List<int>();
            // 2/19/2009 - Init IllumMap Keys
            if (_illumMapInstanceModelsKeys == null)
                _illumMapInstanceModelsKeys = new List<int>();
            // 3/12/2009 - Init SelectableItems Keys
            if (_selectableItemsInstanceModelsKeys == null)
                _selectableItemsInstanceModelsKeys = new List<int>();
            // 7/10/2009 - Init SceneryItems Keys
            if (_sceneryItemsInstanceModelsKeys == null)
                _sceneryItemsInstanceModelsKeys = new List<int>();

            _instanceModelsDrawn = new bool[ItemTypeCount];
            _instanceShadowModelsDrawn = new bool[ItemTypeCount];
            _itemCounterPerItemType = new List<int>(ItemTypeCount);

            for (var i = 0; i < ItemTypeCount; i++)
            {
                _itemCounterPerItemType.Add(0);
            }
            // Keeps other instances from trying to re-init the List Array
            InitInstanceArrayListsCompleted = true;
        }

        // 7/22/2009
        /// <summary>
        /// EventHandler for the <see cref="Camera"/>'s Updated event.  This will set the <see cref="InstancedModel"/> EffectParam's
        /// 'View' & 'Projection'.
        /// </summary>
        private static void CameraUpdated(object sender, EventArgs e)
        {
            // Set to true, which signals the 'Update' method to 
            // call the SetupEffectParam static method on the next cycle.
            _cameraWasUpdated = true;
            
        }

        #region AddInstancedItem methods

        // 11/7/2009 - Updated to no longer load Item here, but Queue into Threads 'PreLoad'.  Also removed
        //             the need to pass the param 'IsSelectable' or the 'ItemTypeAtts'!
        // 8/6/2009 - Add new param 'SceneItemTypAtts', and removed 'modelToLoad' & 'twoPassDraw' params.
        // 3/12/2009 - Add new parameter 'IsSelectableItem'.
        // 9/23/2008 - Adds a new InstanceModel to memory for use.        
        ///<summary>
        /// Used to add an <see cref="InstancedItem"/> model, of <see cref="SceneItemWithPick"/> type, into memory.
        ///</summary>
        ///<param name="itemType"><see cref="ItemType"/> to load</param>
        public static void AddInstancedItem(ItemType itemType)
        {
            // 8/18/2008 - Check if Asset was already loaded, before trying to load again!
            if (InstanceModels[(int)itemType] == null)
            {
                // 11/7/2009 - Queue up loading into PreLoad thread.
                InstancedItemLoader.PreLoadPlayableInstancedItem(itemType);
            }
            
        }

        // 11/7/2009 - Updated to no longer load Item here, but Queue into Threads 'PreLoad'.  Also removed
        //             the need to pass the param 'IsSelectable' or the 'ItemTypeAtts'!
        // 4/22/2009 - Created.  
        ///<summary>
        /// Used to add an <see cref="InstancedItem"/> model, of <see cref="ScenaryItemScene"/> type, into memory.
        ///</summary>
        ///<param name="itemType"><see cref="ItemType"/> to load</param>
        ///<param name="sceneItemOwner"><see cref="SceneItem"/> owner</param>
        public static void AddScenaryInstancedItem(ItemType itemType, SceneItem sceneItemOwner)
        {
            // 8/18/2008 - Check if Asset was already loaded, before trying to load again!
            if (InstanceModels[(int) itemType] == null)
            {
                // 11/7/2009 - Queue up loading into PreLoad thread.
                InstancedItemLoader.PreLoadScenaryInstancedItem(itemType, sceneItemOwner);
            }
        }

        // 11/7/2009
        /// <summary>
        /// Sets the given <see cref="InstancedModel"/> rendering 'Technique'.
        /// </summary>
        /// <param name="itemTypeAtts"><see cref="ScenaryItemTypeAttributes"/> structure</param>
        /// <param name="instanceModel"><see cref="InstancedModel"/> to update</param>
        internal static void SetInstancedModelTechinque(ref ScenaryItemTypeAttributes itemTypeAtts,  ref InstancedModel instanceModel)
        {
            // 12/7/2009 - Check if 'InstancedModel' given is NULL?
            if (instanceModel == null) return;

            var twoPassDraw = itemTypeAtts.useTwoDrawMethod;

            if (twoPassDraw)
                instanceModel.SetInstancingTechnique(InstancingTechnique.HardwareInstancingAlphaDraw);
            // was ShaderInstancingTwoPass
            else
            {
                // 3/16/2009 - RenderingType
                switch (ScreenManager.RenderingType)
                {
                    case RenderingType.NormalRendering:
                    case RenderingType.NormalRenderingWithPostProcessEffects:
                        instanceModel.SetInstancingTechnique(InstancingTechnique.HardwareInstancing);
                        // HardwareInstancing
                        break;
                    default:
                        break;
                }
            }


            // 2/11/2010 - Set the Proper Shadow technique to use.
            //instanceModel.SetProperShadowTechnique();

        }


        // 7/8/2009
        /// <summary>
        /// Adds the <see cref="ItemType"/> given to the proper 'Keys' list.  Each 'Keys' list holds the <see cref="ItemType"/> number to draw; for example,
        /// 'TwoPassDraw' items are added to the 'AlphaMap-Keys' list, while 'IsSelectable' items are added to the
        /// 'SelectableKeys' list.
        /// </summary>
        /// <param name="itemType"><see cref="ItemType"/> Enum</param>
        /// <param name="itemTypeAtts"><see cref="ScenaryItemTypeAttributes"/> structure</param>
        /// <param name="isSceneryItem">This given item is of the ScenaryItem class type</param>
        /// <param name="instanceModel"><see cref="InstancedModel"/> for the <see cref="SceneItem"/> owner</param>
        internal static void AddItemTypeToProperKeyList(ItemType itemType, ref ScenaryItemTypeAttributes itemTypeAtts, bool isSceneryItem, InstancedModel instanceModel)
        {
            var twoPassDraw = itemTypeAtts.useTwoDrawMethod;

            // 12/9/2009 - Cache ItemType Int
            var itemType1 = (int)itemType;

            // 6/12/2012: TODO: Updated to remove the 'UseShadowCasting' for now; until STATIC MAP FIXED IN XNA 4.0
            // 8/6/2009: Updated to split the Shadow Keys between ALL and Dynamic.
            // 1/29/2009 - Add 'ItemType' to the Shadows Keys List, if it cast a shadow.
            // 6/12/2012: Removed: ScenaryItemTypeAtts.ItemTypeAtts[itemType].useShadowCasting
            if (!_shadowInstanceModelsKeysStatic.Contains(itemType1))
            {
                // 3/30/2011
                // TODO: Hack to add all items for shadowing until STATIC MAP FIXED IN XNA 4.0
                _shadowInstanceModelsKeysDynamic.Add(itemType1);

                // 8/6/2009: Check if Animates, which means Dynamic list
                /*if (itemTypeAtts.modelAnimates)
                    _shadowInstanceModelsKeysDynamic.Add(itemType1);
                else
                    // Also add to STATIC list
                    _shadowInstanceModelsKeysStatic.Add(itemType1);*/
            }

            // 2/19/2009 - Add 'ItemType' to the AlphaMap Keys List, if it uses the 2-pass AlphaMap draw method.
            if (twoPassDraw && !_alphaMapInstanceModelsKeys.Contains(itemType1))
                _alphaMapInstanceModelsKeys.Add(itemType1);

            // 2/19/2009 - Add 'ItemType' to the IllumMap Keys List, if it has an Illumination map.
            if (instanceModel.HasIlluminationsMapping() && !_illumMapInstanceModelsKeys.Contains(itemType1))
                _illumMapInstanceModelsKeys.Add(itemType1);

            // 3/12/2009 - Add 'ItemType' to the Selectable Keys List, if 'IsSceneryITem' is False.
            if (!isSceneryItem && !_selectableItemsInstanceModelsKeys.Contains(itemType1))
                _selectableItemsInstanceModelsKeys.Add(itemType1);

            // 7/102/009 - Add 'ItemType' to the SceneryItem Keys List, if 'IsSceneryITem' is True.
            if (isSceneryItem && !_sceneryItemsInstanceModelsKeys.Contains(itemType1))
                _sceneryItemsInstanceModelsKeys.Add(itemType1);
        }

        #endregion

        /// <summary>
        /// During each game cycle, this update method will process any 
        /// <see cref="InstancedModelChangeRequests"/>.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>       
        public override sealed void Update(GameTime gameTime)
        {
#if DEBUG
            StopWatchTimers.StartStopWatchInstance(StopWatchName.InstancedItemUpdate);//"InstancedItem_Update"
#endif

            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return;

            // 6/6/2010 - Store elapsedTime value for explosions on shader.
            InstancedModel.AccumElapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;           
            
            // 7/21/2009 - Apply ChangeRequests Batch, by iterating through List.
            InstancedModelChangeRequests.ProcessChangeRequests();

            // Updates the 'InstancedModel' classes EffectParams 'View' & 'Projection'.
            InstancedModel.SetCameraEffectParams();

            // 2/16/2010 - Update when camera moved.
            if (_cameraWasUpdated)
            {
                // 2/16/2010 - Update transforms for playable items...
                //InstancedItemCulling.UpdateInstanceTransformsForCulling(_selectableItemsInstanceModelsKeys);

                _cameraWasUpdated = false;
            }

            // Debug: Set all instancePicks to False
            {
                // Get Keyboard State                
                var keyState = Keyboard.GetState();

                if (keyState.IsKeyDown(Keys.LeftAlt)
                    && keyState.IsKeyDown(Keys.C))
                {
                    ClearAllPicksFromDictionary();
                }
            }
           
            base.Update(gameTime);

#if DEBUG
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.InstancedItemUpdate);//"InstancedItem_Update"
#endif
        }
       

        #region Draw Methods
        
        /*/// <summary>
        /// Used to process the DoubleBuffers each Draw cycle.
        /// </summary>
        /// <param name="gameTime"></param>
        public sealed override void Draw(GameTime gameTime)
        {
            // Not using the XNA Draw call, because the DrawOrder
            // would either be before ScreenManager, or after!  I need
            // the Drawing to occur during the ScreenManager 'TerrainScreen' update,
            // which then allows the ScreenManagers Menus to draw correctly over
            // the InstanceItems!

            // 6/13/2010 - Process current DoubleBuffers

        }*/
       

        // 2/18/2009; 2/19/2009: Updated to use the new 'alphaMapInstnaceModelKeys'.
        /// <summary>
        /// Draws the <see cref="InstancedItem"/> 2-pass AlphaMapping Models as a batch.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public static void DrawInstanceItems_AlphaMapDraw(GameTime gameTime)
        {

            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            var graphicsDevice = TemporalWars3DEngine.GameInstance.GraphicsDevice;
            if (instancedModels == null) return;
            if (instancedModels.Length == 0) return;

            // 8/13/2009 - Cache
            var alphaMapInstanceModelsKeys = _alphaMapInstanceModelsKeys;
            var count = alphaMapInstanceModelsKeys.Count;

            // 3/25/2011 - XNA 4.0 Updates - AlphaDraw settings
            graphicsDevice.BlendState = BlendState.AlphaBlend;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.RasterizerState = RasterizerState.CullNone;
            graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
           

            for (var item = 0; item < count; item++)
            {
                DrawInstanceModels(alphaMapInstanceModelsKeys[item], gameTime, ref instancedModels);
            }
           
        }

        // 2/19/2009
        /// <summary>
        /// Draws the <see cref="InstancedItem"/> Illumination Map items.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public static void DrawInstanceItems_IllumMapDraw(GameTime gameTime)
        {

            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            var graphicsDevice = TemporalWars3DEngine.GameInstance.GraphicsDevice;
            if (instancedModels == null) return;
            if (instancedModels.Length == 0) return;

            // 8/13/2009 - Cache
            var illumMapInstanceModelsKeys = _illumMapInstanceModelsKeys;
            var count = illumMapInstanceModelsKeys.Count;

            // 3/25/2011 - XNA 4.0 Updates
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            for (var item = 0; item < count; item++)
            {
                DrawInstanceModels(illumMapInstanceModelsKeys[item], gameTime, ref instancedModels);
            }
        }

        // 3/12/2009
        /// <summary>
        /// Draws the <see cref="InstancedItem"/> Selectable items.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public static void DrawInstanceItems_SelectablesDraw(GameTime gameTime)
        {
            
            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            var graphicsDevice = TemporalWars3DEngine.GameInstance.GraphicsDevice;
            if (instancedModels == null) return;
            if (instancedModels.Length == 0) return;

            // 8/13/2009 - Cache
            var selectableItemsInstanceModelsKeys = _selectableItemsInstanceModelsKeys;
            var count = selectableItemsInstanceModelsKeys.Count;

            // 3/25/2011 - XNA 4.0 Updates
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            for (var item = 0; item < count; item++)
            {
                DrawInstanceModels(selectableItemsInstanceModelsKeys[item], gameTime, ref instancedModels);
            }
        }

        // 5/24/2010
        /// <summary>
        /// Draws the <see cref="InstancedItem"/> scenary items.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public static void DrawInstanceItems_ScenaryDraw(GameTime gameTime)
        {

            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            var graphicsDevice = TemporalWars3DEngine.GameInstance.GraphicsDevice;
            if (instancedModels == null) return;
            if (instancedModels.Length == 0) return;

            // 8/13/2009 - Cache
            var sceneryItemsInstanceModelsKeys = _sceneryItemsInstanceModelsKeys;
            var count = sceneryItemsInstanceModelsKeys.Count;

            // 3/25/2011 - XNA 4.0 Updates
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            for (var item = 0; item < count; item++)
            {
                DrawInstanceModels(sceneryItemsInstanceModelsKeys[item], gameTime, ref instancedModels);
            }
        }

        // 5/24/2010: Updated to require the 'GameTime' param, and removed the use of the 'InstancedModelKeys' array.
        // 5/19/2009: Removed the params 'View', 'Projection', & 'LightPos' since these are avaible as STATIC variables!
        // 7/22/2008; 1/14/2009: Updated to use the Keys List instead.
        /// <summary>
        /// Batch Draws the <see cref="InstancedModel"/> for the <see cref="TWEngine.Water"/>'s ReflectionMap.
        /// Should be called from the <see cref="TWEngine.Water"/> Interface.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="reflectionView">Reflection view matrix</param>
        public static void DrawInstanceModelsForWaterRm(GameTime gameTime, ref Matrix reflectionView)
        {
            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return;
            if (instancedModels.Length == 0) return;

            // 7/1/2009 - Set Water ViewMatrix
            if (InstancedModelPart.ViewParam != null) InstancedModelPart.ViewParam.SetValue(reflectionView);

            // 5/24/2010 - Draw SelectableItems.
            DrawInstanceItems_SelectablesDraw(gameTime);

        }

        // 5/19/2009: Removed the params 'View', 'Projection', & 'LightPos' since these are avaible as STATIC variables!
        // 7/18/2008
        // 12/18/2008: Updated to add View,Proj, & lightPos Parameters, rather
        //             than having computer look up for ever draw call!
        /// <summary>
        /// Batch Draws the <see cref="InstancedModel"/> type
        /// </summary>     
        /// <param name="itemType"><see cref="ItemType"/> to draw</param>  
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="instanceModelsCollection"><see cref="InstancedModel"/> collection</param>
        private static void DrawInstanceModels(int itemType, GameTime gameTime, ref InstancedModel[] instanceModelsCollection)
        {
            // 4/20/2010 - Check if null
            if (instanceModelsCollection == null)
                throw new ArgumentNullException("instanceModelsCollection", @"The parameter 'InstanceModelsCollection' cannot be null!");

            // 8/13/2009 - Cache
            var instancedModel = instanceModelsCollection[itemType];

            // Check if ItemType is being used, otherwise exit method.
            if (instancedModel == null) return;

            // 6/13/2010 - Process the DoubleBuffers for this itemType.
            InstancedModelPart.InstancedModelBufferRequests.ProcessDoubleBuffers(instancedModel);

            // 5/27/2012 - Draw Debug Collision Spheres
            /*var sphere = new BoundingSphere(new Vector3(3461, 0, 967), 60.0f);
            DebugShapeRenderer.AddBoundingSphere(sphere, Color.Red);
            DebugShapeRenderer.Draw(gameTime);*/

            // 10/14/2012: TODO: Testing ALL call.
            InstancedModel.DrawCulledInstances(instancedModel, gameTime);
            //InstancedModel.DrawAllInstances(instancedModel, gameTime);
        }

        // 7/10/2009: Updated to have the 'culledItems' param.
        // 12/1/2008; 1/29/2009: Updated to use the Shadow Keys List instead.
        /// <summary>
        /// Draws the Shadows for all <see cref="InstancedItem"/> of all <see cref="ItemType"/> currently being used.
        /// </summary>        
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="lightView"><see cref="ShadowMap"/> classes LightView matrix.</param>
        /// <param name="lightProj"><see cref="ShadowMap"/> classes LightProj matrix.</param>
        /// <param name="culledItems">Do Culled items Only?</param>
        public static void DrawForShadowMap_AllItems(GameTime gameTime, ref Matrix lightView, ref Matrix lightProj,
                                                     bool culledItems)
        {
            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return;
            if (instancedModels.Length == 0) return;

            // 8/13/2009 - Cache
            var shadowInstanceModelsKeysAll = _shadowInstanceModelsKeysStatic;
            var count = shadowInstanceModelsKeysAll.Count;

            // 1/29/2009: Updated to use the Shadow Keys List instead.
            for (var item = 0; item < count; item++)
            {
                // 4/9/2009
                var index = shadowInstanceModelsKeysAll[item];

                // 8/13/2009 - Cache
                var instancedModel = instancedModels[index];

                // 6/11/2009
                if (instancedModel == null) continue;

                // 3/25/2011 - XNA 4.0 Updates - Skip Scenary with BakeTransforms==true.
                //if (instancedModel.IsScenaryItem && instancedModel.UseBakedTransforms) continue;

                // 7/10/2009
                if (culledItems)
                {
                    // 7/7/2009 - Skip draw call, if culling 'TransformToDrawList' is empty!
                    if (instancedModel.ModelParts[0].TransformsToDrawList.Count == 0)
                        continue;

                    InstancedModel.DrawShadowCulledInstances(instancedModel, ref lightView, ref lightProj, gameTime);
                }
                else
                    InstancedModel.DrawShadowAllInstances(instancedModel, ref lightView, ref lightProj, gameTime, false);

                if (_instanceShadowModelsDrawn != null)
                    _instanceShadowModelsDrawn[index] = true;
            }
        }

        // 12/1/2008; 1/29/2009: Updated to use the Shadow Keys List instead.
        /// <summary>
        /// Draws the Shadows for all <see cref="InstancedItem"/> for some <see cref="ItemType"/>, where the 'AlwaysDrawShadow=TRUE'.
        /// </summary>        
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="lightView"><see cref="ShadowMap"/> Dynamic LightView matrix.</param>
        /// <param name="lightProj"><see cref="ShadowMap"/> Dynamic LightProj matrix.</param>
        public static void DrawForShadowMap_DynamicItems(GameTime gameTime, ref Matrix lightView, ref Matrix lightProj)
        {
            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return;
            if (instancedModels.Length == 0) return;

            // 8/13/2009 - Cache
            var shadowInstanceModelsKeysDynamic = _shadowInstanceModelsKeysDynamic;
            var count = shadowInstanceModelsKeysDynamic.Count;

            for (var item = 0; item < count; item++)
            {
                // 4/9/2009
                var index = shadowInstanceModelsKeysDynamic[item];

                // 8/6/2009 - Cache to local value
                var instancedModel = instancedModels[index];

                // 6/11/2009
                if (instancedModel == null) continue;

                // Note: 5/28/2012: Removed, so all items cast shadows - this was used to remove 'Static' drawn items in XNA 3.1
                // 3/25/2011 - XNA 4.0 Updates - Skip Scenary with BakeTransforms==true.
                //if (instancedModel.IsScenaryItem && instancedModel.UseBakedTransforms) continue;

                // 7/7/2009 - Skip draw call, if culling 'TransformToDrawList' is empty!
                if (instancedModel.ModelParts[0].TransformsToDrawList.Count == 0)
                    continue;

                InstancedModel.DrawShadowCulledInstances(instancedModel, ref lightView, ref lightProj, gameTime);

                _instanceShadowModelsDrawn[index] = true;
            }
        }

        // 7/12/2009
        /// <summary>
        /// Draws the Shadows for all <see cref="InstancedItem"/> for some <see cref="ItemType"/>, where the 'AlwaysDrawShadow=FALSE'.
        /// </summary>        
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="lightView"><see cref="ShadowMap"/> Static LightView matrix.</param>
        /// <param name="lightProj"><see cref="ShadowMap"/> Static LightProj matrix.</param>
        public static void DrawForShadowMap_StaticItems(GameTime gameTime, ref Matrix lightView, ref Matrix lightProj)
        {
            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return;
            if (instancedModels.Length == 0) return;

            // 8/13/2009 - Cache
            var shadowInstanceModelsKeysAll = _shadowInstanceModelsKeysStatic;
            var count = shadowInstanceModelsKeysAll.Count;

            for (var item = 0; item < count; item++)
            {
                // 4/9/2009
                var index = shadowInstanceModelsKeysAll[item];

                // 8/13/2009 - Cache
                var instancedModel = instancedModels[index];

                // 6/11/2009
                if (instancedModel == null) continue;

                // 3/25/2011 - XNA 4.0 Updates - Skip Scenary with BakeTransforms==true.
                if (instancedModel.IsScenaryItem && instancedModel.UseBakedTransforms) continue;

                // 12/1/2008 - If NOT FALSE, then skip. (FALSE for 'STATIC' items)
                if (instancedModel.AlwaysDrawShadow) continue;

                InstancedModel.DrawShadowAllInstances(instancedModel, ref lightView, ref lightProj, gameTime, true);

                _instanceShadowModelsDrawn[index] = true;
            }
        }

        // 9/23/2008 - Draws the Shadows for all InstanceModels.
        /// <summary>
        /// Draws the Shadows for all <see cref="InstancedItem"/> of a specific ItemType requested.
        /// </summary>
        /// <param name="instancedItemData"><see cref="instancedItemData"/> structure</param>       
        /// <param name="lightView"><see cref="ShadowMap"/> classes LightView matrix.</param>
        /// <param name="lightProj"><see cref="ShadowMap"/> classes LightProj matrix.</param>
        public static void DrawForShadowMap(ref InstancedItemData instancedItemData, ref Matrix lightView,
                                            ref Matrix lightProj)
        {
            // Check if Batch already Drawn                       
            if (_instanceShadowModelsDrawn[(int) instancedItemData.ItemType]) return;

            InstancedModel.DrawShadowCulledInstances(InstanceModels[(int) instancedItemData.ItemType], ref lightView, ref lightProj, null);

            _instanceShadowModelsDrawn[(int) instancedItemData.ItemType] = true;
        }

        #endregion

        // 6/18/2010
        /// <summary>
        /// Used to set a specific effect parameter's value into the <see cref="InstancedModel"/> effect shaders.  If the 
        /// <paramref name="itemType"/> is left NULL, then all <see cref="InstancedModel"/> will be updated.
        /// </summary>
        /// <param name="effectParamName">Name of effectParam to update</param>
        /// <param name="valueToSet"><see cref="Vector3"/> value to set</param>
        /// <param name="itemType">(Optional) <see cref="ItemType"/> Enum</param>
        public static void SetSpecificEffectParam(string effectParamName, Vector3 valueToSet, ItemType? itemType)
        {
            var instancedModels = InstanceModels;

            // if 'itemType' given is null, then iterate all models.
            if (itemType == null)
            {
                var length = instancedModels.Length;
                for (var i = 0; i < length; i++)
                {
                    // cache
                    var instancedModel = instancedModels[i];
                    if (instancedModel == null) continue;

                    // set effectParam value.
                    instancedModel.SetSpecificEffectParam(effectParamName, valueToSet);
                }

                return;
            }

            // else update just the specific itemType given
            var specificModelType = instancedModels[(int) itemType.Value];
            if (specificModelType == null) return;

            specificModelType.SetSpecificEffectParam(effectParamName, valueToSet);
        }

        // 7/9/2009; 10/15/2012 - Optimized by using ScenaryItemTypesQuadContainer.
        /// <summary>
        /// This method is called directly from the <see cref="TerrainQuadTree"/> draw method, during each draw
        /// cycle.  The <see cref="TerrainQuadTree"/> then passes the internal Dictionary of scenary <see cref="ItemType"/> which are
        /// associated with given quad.  Furthermore, each <see cref="ItemType"/> entry in the dictionary, is 
        /// accompanied with an internal list of 'instanceItemKeys'.  This list is passed directly
        /// to the <see cref="InstancedModel"/> class, to create the proper 'Culled' list for the next draw call.
        /// </summary>
        /// <param name="itemTypes">Collection of <see cref="ItemType"/></param>
        public static void CreateSceneryInstancesCulledList(Dictionary<int, ScenaryItemTypesQuadContainer> itemTypes)
        {
            if (itemTypes == null) return;
            if (itemTypes.Count == 0) return;

#if DEBUG
            StopWatchTimers.StartStopWatchInstance(StopWatchName.InstancedItemCameraCulling); //"InstancedItem_CameraCulling"
#endif

            // Resize _keys array, if too small.
            var keyCollection = itemTypes.Keys; // 8/13/2009
            var itemTypesKeysCount = keyCollection.Count; // 8/13/2009

            if (_keys.Length < itemTypesKeysCount)
                Array.Resize(ref _keys, itemTypesKeysCount);
            keyCollection.CopyTo(_keys, 0);

            // 6/1/2010 - Cache
            var keys = _keys;

            // 4/20/2010 - Cache
            var instancedModels = InstanceModels; 
            if (instancedModels == null) return;

            // iterate list of ItemTypes, passing in the internal 'List<int>' of instanceItemKeys to affect.
            for (var i = 0; i < itemTypesKeysCount; i++)
            {
                // 4/20/2010 - Cache
                var itemTypeIndexKey = keys[i]; // 6/7/2012
                var instancedModel = instancedModels[itemTypeIndexKey];
                if (instancedModel == null) continue;

                // 10/15/2012 - Updated to retrieve structure container.
                // 6/7/2012 - Retrieve itemkeys from internal dictionary values.
                var scenaryItemTypesQuadContainer = itemTypes[itemTypeIndexKey];
                //var itemTypeInstancesCount = itemTypeInstances.Count;

                // 10/15/2012: Note: This work is now done in the TerrainQuadTree's ConnectScenaryItemToGivenQuad/DisconnectScenaryItemFromGivenQuad
                // 6/7/2012 - Resize array and copy to List<int>
                /*if (_itemTypeInstancesCache.Length < itemTypeInstancesCount)
                    Array.Resize(ref _itemTypeInstancesCache, itemTypeInstancesCount);
                itemTypeInstances.CopyTo(_itemTypeInstancesCache, 0);

                ItemTypeInstancesList.Clear();
                ItemTypeInstancesList.AddRange(_itemTypeInstancesCache);*/

                // _keys[] are the ItemType cast as an 'int'.
                instancedModel.CreateScenaryInstancesCulledList(ref scenaryItemTypesQuadContainer.ItemKeysArray);
            }

#if DEBUG
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.InstancedItemCameraCulling); //"InstancedItem_CameraCulling"
#endif
           
        }

        // 7/10/2009
        /// <summary>
        /// This method is called once per 'Draw' cycle, to clear out the 'Culled' lists of
        /// all <see cref="ScenaryItemScene"/> items.  
        /// </summary>
        public static void ClearSceneryInstancesCulledList()
        {
            try
            {
                // 8/13/2009 - Cache
                var sceneryItemsInstanceModelsKeys = _sceneryItemsInstanceModelsKeys;
                var count = sceneryItemsInstanceModelsKeys.Count;

                // iterate using the 'SceneryItemsKey' list.
                var instancedModels = InstanceModels; // 4/20/2010
                for (var item = 0; item < count; item++)
                {
                    var itemTypeKey = sceneryItemsInstanceModelsKeys[item];

                    // 8/13/2009 - Cache
                    var instancedModel = instancedModels[itemTypeKey];
                    if (instancedModel == null) continue;
                    
                    // Call clear scenary instances for given instancedModel artwork.
                    instancedModel.CreateBufferRequestToClearScenaryInstancesCulledList();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("ClearSceneryInstancesCulledList method threw the '{0}' error.", e.Message);
            }
        }

        // 12/1/2008
        /// <summary>
        /// Sets the 'AlwaysDrawShadow' internal flag inside the <see cref="InstancedModel"/> <see cref="ItemType"/>.
        /// When set to True, the call to the 'DrawForShadowMap_SomeItems' method will
        /// only batch draw the instances set to TRUE for the <see cref="ItemType"/>.
        /// </summary>
        /// <param name="instancedItemData"><see cref="ItemType"/> to set flag</param>
        /// <param name="alwaysDrawShadow">True/False</param>
        public static void SetAlwaysDrawShadow(ref InstancedItemData instancedItemData, bool alwaysDrawShadow)
        {
            // Set internal flag
            var index = (int) instancedItemData.ItemType;

            // 5/10/2009 - Verify within array bounds.
            if (index < 0 || index >= InstanceModels.Length) return;

            // 8/13/2009 - Cache
            var instancedModel = InstanceModels[index];

            if (instancedModel != null)
                instancedModel.AlwaysDrawShadow = alwaysDrawShadow;
        }


        /*// 10/18/2008
        /// <summary>
        /// Resets the ShadowModel Drawn flag for all Itemtypes.
        /// </summary>
        public static void ResetShadowModelsDrawnFlag()
        {
            // 1/6/2009 - Make sure not null
            if (_instanceShadowModelsDrawn == null)
                return;

            for (int i = 0; i < _instanceShadowModelsDrawn.Length; i++)
            {
                _instanceShadowModelsDrawn[i] = false;   
            }

        }*/

        // 10/19/2008
        /// <summary>
        /// Resets the ShadowModel Drawn flag for a specific <see cref="ItemType"/>.
        /// </summary>
        /// <param name="instancedItemData"><see cref="ItemType"/> to Set Flag</param>
        public static void ResetShadowModelsDrawnFlag(ref InstancedItemData instancedItemData)
        {
            _instanceShadowModelsDrawn[(int) instancedItemData.ItemType] = false;
        }


        // 8/12/2008
        /// <summary>
        /// Clears out all the internal Static Dictionaries/List which track
        /// the <see cref="InstancedModel"/> transforms.  Primarily called when loading
        /// a new map from the TerrainStorageRoutine class.
        /// </summary>
        public static void ClearAllInstanceModelsTransforms()
        {
            // Set _itemCounter List Array to zeros for all itemTypes           
            if (_itemCounterPerItemType != null)
            {
                var count = _itemCounterPerItemType.Count; // 8/13/2009

                for (var loop1 = 0; loop1 < count; loop1++)
                {
                    _itemCounterPerItemType[loop1] = 0;
                }
            }

            // 5/27/2009 - Clear all InstanceModels instances
            if (InstanceModels != null)
            {
                var length = InstanceModels.Length; // 8/13/2009
                for (var i = 0; i < length; i++)
                {
                    // 8/13/2009 - Cache
                    var instancedModel = InstanceModels[i];

                    if (instancedModel == null) continue;

                    instancedModel.ChangeRequestItemsTransforms.Clear();
                    Array.Clear(instancedModel.InstanceWorldTransformKeys, 0,
                                instancedModel.InstanceWorldTransformKeys.Length);
                }
            }

            // Reset back to 0
            _itemCounter = 0;
        }

        // 5/24/2010: Updated by refactoring out core code to new method called 'DoClearPicks', and removed old '_instancedModelKeys' collection.
        // 9/12/2008: Updated to use the List Array, so the following does not apply anymore.
        // 7/22/2008
        // 1/14/2009: Updated to use the Keys List instead.
        // Ben: The following solution was derived due to the following constraints of the Dictionary Struct;
        //
        //      1) The inner Dictionary Struct holds instances of Keys, where the instance is the class instance when created.
        //      2) The Count of the inner Dictionary does not match the 'Key' Instance value, and therefore cannot be used
        //         as a way to update values directly!  For example, if inner dictionary had 177 records, but the first record 'Key'
        //         value was '6', then 0-5 in the for-loop would not access the proper index!
        //      3) You can get the Keys Collection from Dictionary, but then you have to use the ForEach Enumeration, 
        //         which DOES NOT allow you to update values in the Dictionary while you are looping through the collection!        
        //
        // Solution: 
        //         Therefore, solution is to create a temporary List Array of KeyValuePair<> items, which is populated as you
        //         iterate through the original inner Dictionary Items using the ForEach Collection, and then make the changes
        //         inside the ForEach loop, create a new KeyValuePair<> SceneItemOwner with new changes, and add it to the temporary Array List.
        //         Once the ForEach loop is complete, you call the Clear() method on the inner Dictionary, and then loop
        //         through the List Array of the KeyValuePair<> items, and add them back to the Dictionary as new nodes.
        //
        /// <summary>
        /// The following Clears the 'IsPicked' Boolean value to be False for all <see cref="ItemType"/> Models and
        /// all the instances of each Model.
        /// </summary>
        public static void ClearAllPicksFromDictionary()
        {
            // 4/20/2010 - Cache
            if (_selectableItemsInstanceModelsKeys == null) return;

            // 5/24/2010 - Refactored core code, for reuse.
            DoClearPicks(_selectableItemsInstanceModelsKeys); // Clear Selectables.

            // 5/24/2010 - Check for scenary picks.
            DoClearPicks(_sceneryItemsInstanceModelsKeys); 
        }

        // 5/24/2010 
        /// <summary>
        /// Method helper, which iterates the given collection and clears out any picks.
        /// </summary>
        /// <param name="instanceModelsKeys">Collection of <see cref="int"/> values</param>
        private static void DoClearPicks(IList<int> instanceModelsKeys)
        {
            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return;

            var count = instanceModelsKeys.Count;
            for (var loop1 = 0; loop1 < count; loop1++)
            {
                // 8/13/2009 - Cache
                var instancedModel = instancedModels[instanceModelsKeys[loop1]];
                if (instancedModel == null) continue;

                // 12/18/2008 - Get Keys for Dictionary
                var keys = new int[1];
                var instanceWorldTransforms = instancedModel.ChangeRequestItemsTransforms; // 8/13/2009
                var keyCollection = instanceWorldTransforms.Keys;

                if (keys.Length != keyCollection.Count)
                    Array.Resize(ref keys,
                                 keyCollection.Count);
                keyCollection.CopyTo(keys, 0);

                var length = keys.Length; // 4/20/2010
                for (var loop2 = 0; loop2 < length; loop2++)
                {
                    var item = instanceWorldTransforms[keys[loop2]];

                    // Remove picking by setting M44 in the Matrix to be 1.0f.
                    //item.Transform.M44 = 1;
                    var world = item.ShapeItem.WorldP;
                    world.M44 = 1;
                    item.ShapeItem.WorldP = world;

                    item.IsPicked = false;

                    // Store back into Array
                    instanceWorldTransforms[keys[loop2]] = item;
                }
            } // End For ItemType

            // Update All
            //UpdateInstanceTransformsForCulling();
        }

        
#if !XBOX360
        // 7/31/2008
        /// <summary>
        /// Gets the Total Items contain in the <see cref="ItemType"/> Enum.
        /// Specifically used to set the 'ItemTypeCount' ReadOnly variable.
        /// </summary>
        /// <returns>Int as Total</returns>
        private static int GetEnumCount()
        {
            var tmpNames = Enum.GetNames(typeof (ItemType));
            return tmpNames.Length;
        }
#endif

        // 2/3/2010 - Updated to use the new refactored methods for 'InstanceData' node from the InstancedModel class.
        // 7/10/2009 - Add 'IsSceneryItem' param.
        // 9/23/2008 - Updates an InstanceItem Transform.
        // 11/19/2008 - Updated to not use the 'HasItemInstanceKey' predicate, since this causes additional garbage!       
        ///<summary>
        /// Used to create the <see cref="ChangeRequestItem"/> structure, which is populated with the
        /// given attributes, and updated into the <see cref="InstancedModelChangeRequests"/> class.
        ///</summary>
        /// <remarks>When initially called, and of <see cref="ScenaryItemScene"/> type, the item will be 
        /// connected to a specific <see cref="TerrainQuadPatch"/>, using the method call <see cref="TerrainQuadTree.ConnectScenaryItemToGivenQuad"/>.
        /// This is used for culling purposes during game play, to improve performance.
        /// </remarks>
        ///<param name="shapeItemOwner">Reference to <see cref="Shape"/> item owner</param>
        ///<param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        ///<param name="playerNumber"><see cref="Player"/>'s number</param>
        public static void UpdatePlayableModelTransform(Shape shapeItemOwner, ref InstancedItemData instancedItemData, int playerNumber)
        {
            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return;

            // 8/13/2009 - Cache
            var itemType = (int)instancedItemData.ItemType;
            var instancedModel = instancedModels[itemType];

            // 5/25/2009
            if (instancedModel == null) return;

            // 2/3/2010 - Updated to use the new refactored methods for 'InstanceData' node from the InstancedModel class.
            ChangeRequestItem existingNode;
            if (instancedModel.GetInstanceDataNode(instancedItemData.ItemInstanceKey, out existingNode))
            {
                existingNode.ShapeItem = shapeItemOwner; // 8/27/2009
                existingNode.SceneryTransform = Matrix.Identity; // 10/5/2009 - Req to see PropertiesTool updates!
                existingNode.PlayerNumber = playerNumber; // 2/23/2009
            }
            else
            {
                // Update new Node
                existingNode.ShapeItem = shapeItemOwner;
                existingNode.SceneryTransform = Matrix.Identity; // 8/27/2009
                existingNode.InCameraView = true;
                existingNode.ItemInstanceKey = instancedItemData.ItemInstanceKey;
                existingNode.PlayerNumber = playerNumber;
                existingNode.DrawWithExplodePieces = false;
                existingNode.IsSceneryItem = false; 
            }

            // 2/3/2010 - Save back into Dictionary
            instancedModel.UpdateInstanceDataNode(instancedItemData.ItemInstanceKey, ref existingNode);

            // Update InstanceTransforms, since World Matrix was updated or new one added.
            UpdateInstanceTransforms(ref instancedItemData);
        }

        // 3/29/2011
        ///<summary>
        /// Used to create the <see cref="ChangeRequestItem"/> structure, which is populated with the
        /// given attributes, and updated into the <see cref="InstancedModelChangeRequests"/> class.
        ///</summary>
        /// <remarks>When initially called, and of <see cref="ScenaryItemScene"/> type, the item will be 
        /// connected to a specific <see cref="TerrainQuadPatch"/>, using the method call <see cref="TerrainQuadTree.ConnectScenaryItemToGivenQuad"/>.
        /// This is used for culling purposes during game play, to improve performance.
        /// </remarks>
        ///<param name="shapeItemOwner">Reference to <see cref="Shape"/> item owner</param>
        ///<param name="isEditToolAdd">Was added using the ItemTool edit mode?</param>
        ///<param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        ///<param name="playerNumber"><see cref="Player"/>'s number</param>
        public static void UpdateScenaryModelTransform(Shape shapeItemOwner, bool isEditToolAdd, 
                                                    ref InstancedItemData instancedItemData, int playerNumber)
        {
            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return;

            // 8/13/2009 - Cache
            var itemType = (int)instancedItemData.ItemType;
            var instancedModel = instancedModels[itemType];

            // 5/25/2009
            if (instancedModel == null) return;

            // 2/3/2010 - Updated to use the new refactored methods for 'InstanceData' node from the InstancedModel class.
            ChangeRequestItem existingNode;
            if (instancedModel.GetInstanceDataNode(instancedItemData.ItemInstanceKey, out existingNode))
            {
                existingNode.ShapeItem = shapeItemOwner; // 8/27/2009
                existingNode.SceneryTransform = shapeItemOwner.WorldP; // 10/5/2009 - Req to see PropertiesTool updates!
                existingNode.PlayerNumber = playerNumber; // 2/23/2009

                // 6/7/2012 - Check to update Quad connection, if Position updated.
                if (instancedItemData.PositionUpdated)
                {
                    // Reset the position change flag.
                    instancedItemData.PositionUpdated = false;

                    // Optimization: Check if different QuadKey, before doing any updates.
                    TerrainQuadTree terrainQuadTree;
                    if (GetQuadKeyBasedOnItemPosition(shapeItemOwner, out terrainQuadTree))
                    {
                        if (terrainQuadTree.QuadKeyInstance != instancedItemData.QuadKey)
                        {
                            //
                            // Remove from old Quad
                            //

                            // Found quad, so store the scenary 'ItemType' into the quad.
                            var terrainQuadTreeRoot = TerrainShape.RootQuadTree; // 5/19/2010 - Cache
                            TerrainQuadTree.DisconnectScenaryItemFromGivenQuad(terrainQuadTreeRoot, ref instancedItemData);

                            //
                            // Add to new Quad
                            //

                            // 6/7/2012 - Refactored out.
                            AddScenaryItemToGivenQuad(shapeItemOwner, ref instancedItemData);
                        }
                    }
                }

            }
            else
            {
                // 6/7/2012 - Refactored out.
                AddScenaryItemToGivenQuad(shapeItemOwner, ref instancedItemData);

                // Update new Node
                existingNode.ShapeItem = shapeItemOwner;
                existingNode.SceneryTransform = shapeItemOwner.WorldP; // 8/27/2009
                existingNode.InCameraView = true;
                existingNode.ItemInstanceKey = instancedItemData.ItemInstanceKey;
                existingNode.PlayerNumber = playerNumber;
                existingNode.DrawWithExplodePieces = false;
                existingNode.IsSceneryItem = true; // 3/28/2009 
            }

            // 2/3/2010 - Save back into Dictionary
            instancedModel.UpdateInstanceDataNode(instancedItemData.ItemInstanceKey, ref existingNode);

            // Update InstanceTransforms, since World Matrix was updated or new one added.
            if (isEditToolAdd) UpdateInstanceTransforms(ref instancedItemData);
        }

        // 6/7/2012
        /// <summary>
        /// Gets the proper QuadKey for the given <see cref="shapeItemOwner"/> and adds this instance to the proper <see cref="TerrainQuadTree"/>.
        /// </summary>
        private static void AddScenaryItemToGivenQuad(Shape shapeItemOwner, ref InstancedItemData instancedItemData)
        {
            TerrainQuadTree quad;
            if (!GetQuadKeyBasedOnItemPosition(shapeItemOwner, out quad)) return;   

            instancedItemData.QuadKey = quad.QuadKeyInstance; // 6/7/2012

            // Found quad, so store the scenary 'ItemType' into the quad.
            var terrainQuadTree = TerrainShape.RootQuadTree;
            TerrainQuadTree.ConnectScenaryItemToGivenQuad(terrainQuadTree, quad.QuadKeyInstance, ref instancedItemData);
        }

        // 6/7/2012
        /// <summary>
        /// Gets the QuadKey from the <see cref="TerrainQuadTree"/> based on the 
        /// location of the given <see cref="Shape"/>'s position.
        /// </summary>
        private static bool GetQuadKeyBasedOnItemPosition(Shape shapeItemOwner, out TerrainQuadTree quad)
        {
            // 7/10/2009 - Only connect for scenery items.
            // 7/9/200 - Create ray from current Position pointing down, then get the terrain quad the
            //           ray intersects.
            var ray = new Ray(shapeItemOwner.WorldP.Translation, Vector3.Down);
           
            var terrainQuadTree = TerrainShape.RootQuadTree;
            return TerrainQuadTree.GetQuadForGivenRayIntersection(terrainQuadTree, ref ray, out quad);
        }

        // 2/3/2010 - Updated to use the new refactored methods for 'InstanceData' node from the InstancedModel class.
        // 1/30/2010: Removed the need to pass the 'IsFOWVisible' flag.
        // 7/20/2090: Updated to now only call the 'UpdateInstanceTransforms' method, when the FOWView flag differs from stored value.
        // 1/14/2009 - Updates the FOW IsFOWVisible flag.       
        ///<summary>
        /// Specifically used to communicate <see cref="IFogOfWar"/> updates for a given <see cref="SceneItem"/>.  This method
        /// will then create an <see cref="ChangeRequestItem"/> structure, and add it to the <see cref="InstancedModelChangeRequests"/> class
        /// for processing.
        ///</summary>
        ///<param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        ///<param name="playerNumber"><see cref="Player"/>'s number.</param>
        public static void UpdateInstanceModelFogOfWarView(ref InstancedItemData instancedItemData, int playerNumber)
        {
            // 7/24/2009 - Cache ItemType index, so done only cast once!
            var itemIndex = (int) instancedItemData.ItemType;


            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return;

            // 8/13/2009 - Cache
            var instancedModel = instancedModels[itemIndex];

            // Make sure not NULL
            if (instancedModel == null) return;

            // 2/3/2010 - Updated to use the new refactored methods for 'InstanceData' node from the InstancedModel class.
            ChangeRequestItem existingNode;
            instancedModel.GetInstanceDataNode(instancedItemData.ItemInstanceKey, out existingNode);
            
            // 8/28/2009: Updated to use the new 'AddChangeRequest' method, to batch entries.
            existingNode.ChangeRequest = ChangeRequestEnum.AddOrUpdateTransform; // 10/16/2012
            InstancedModelChangeRequests.AddChangeRequestForInstanceTransforms(instancedItemData.ItemType, ref existingNode, instancedModel);
           
            
        }

        // 2/3/2010 - Updated to use the new refactored methods for 'InstanceData' node from the InstancedModel class.
        // 3/28/2009 - Updates the InstaceModel to Draw using the EXPLOSION Pieces!
        ///<summary>
        /// Specifically used to communicate ExplosionItem updates for a given <see cref="SceneItem"/>.  This method
        /// will then create an <see cref="ChangeRequestItem"/> structure, and add it to the <see cref="InstancedModelChangeRequests"/> class
        /// for processing.
        ///</summary>
        ///<param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        ///<param name="playerNumber"><see cref="Player"/>'s number.</param>
        ///<param name="drawPieces">Start/Stop drawing pieces for ExplosionItem.</param>
        [Obsolete]
        public static void UpdateInstanceModelToDrawExplosionPieces(ref InstancedItemData instancedItemData,
                                                                    int playerNumber, bool drawPieces)
        {
            // 7/24/2009 - Cache ItemType index, so done only cast once!
            /*var itemIndex = (int) instancedItemData.ItemType;

            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return;

            // 8/13/2009 - Cache
            var instancedModel = instancedModels[itemIndex];

            // Make sure not NULL
            if (instancedModel == null) return;
           
            // 2/3/2010 - Updated to use the new refactored methods for 'InstanceData' node from the InstancedModel class.
            ChangeRequestItem existingNode;
            if (!instancedModel.GetInstanceDataNode(instancedItemData.ItemInstanceKey, out existingNode)) return;

            // update 'PlayerNumber'
            instancedModel.UpdateInstanceDataNodeSpecificParameter(instancedItemData.ItemInstanceKey,
                                                                   InstanceDataParam.PlayerNumber, playerNumber);
           
            // Update 'ExplodePieces' flag
            instancedModel.UpdateInstanceDataNodeSpecificParameter(instancedItemData.ItemInstanceKey,
                                                                  InstanceDataParam.DrawWithExplodePieces, drawPieces);

            // 7/24/2009: Delete All Normal Culled Parts, since now drawing exploding parts.
            InstancedModelChangeRequests.AddChangeRequestForInstanceTransforms(instancedItemData.ItemType, ref existingNode,
                                                                               ChangeRequest.RemovesASingleInstancedModelPart, instancedModel);

            // 2/15/2010 - Sets to allow drawing of Explosion pieces too.
            instancedModels[(int)instancedItemData.ItemType].SetDrawExplosionPiecesFlag();

            // Update InstanceTransforms, since Explosion value changed.
            UpdateInstanceTransforms(ref instancedItemData);*/
        }

        // 2/3/2010 - Updated to use the new refactored methods for 'InstanceData' node from the InstancedModel class.
        // 10/12/2009
        /// <summary>
        /// Specifically used to communicate 'Flash' white updates for a given <see cref="SceneItem"/>. (Scripting Purposes) 
        /// This method will then create an <see cref="ChangeRequestItem"/> structure, and add it to the <see cref="InstancedModelChangeRequests"/> class
        /// for processing.
        /// </summary>
        ///<param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        ///<param name="playerNumber"><see cref="Player"/>'s number.</param>
        /// <param name="doFlashWhite">Set to True/False</param>
        public static void UpdateInstanceModelToFlashWhite(ref InstancedItemData instancedItemData,
                                                                   int playerNumber, bool doFlashWhite)
        {
            // Cache ItemType index, so done only cast once!
            var itemIndex = (int) instancedItemData.ItemType;


            // Make sure not NULL
            var instancedModels = InstanceModels; // 4/20/2010
            if (instancedModels == null) return;

            // Cache
            var instancedModel = instancedModels[itemIndex];

            // Make sure not NULL
            if (instancedModel == null) return;

            // 2/3/2010 - Updated to use the new refactored methods for 'InstanceData' node from the InstancedModel class.
            ChangeRequestItem existingNode;
            instancedModel.GetInstanceDataNode(instancedItemData.ItemInstanceKey, out existingNode);

            // update 'PlayerNumber'
            instancedModel.UpdateInstanceDataNodeSpecificParameter(instancedItemData.ItemInstanceKey,
                                                                   InstanceDataParam.PlayerNumber, playerNumber);

            // Update 'ExplodePieces' flag
            instancedModel.UpdateInstanceDataNodeSpecificParameter(instancedItemData.ItemInstanceKey,
                                                                  InstanceDataParam.ShowFlashWhite, doFlashWhite);

            // Save back into Dictionary
            instancedModel.UpdateInstanceDataNode(instancedItemData.ItemInstanceKey, ref existingNode);

            // Updated to use the new 'AddChangeRequest' method, to batch entries.
            existingNode.ChangeRequest = ChangeRequestEnum.AddOrUpdateFlashWhite; // 10/16/2012
            InstancedModelChangeRequests.AddChangeRequestForInstanceTransforms(instancedItemData.ItemType, ref existingNode, instancedModel);
        }

        // 5/22/2009
        /// <summary>
        /// Checks if the given InstanceModel was imported using the 'FBXImporter' method.
        /// </summary>
        /// <param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        /// <returns>True/False</returns>
        public static bool IsFBXImport(ref InstancedItemData instancedItemData)
        {
            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return false;

            // 8/13/2009 - Cache
            var instancedModel = instancedModels[(int)instancedItemData.ItemType];

            // 5/25/2009
            return instancedModel != null && instancedModel.IsFBXImported;
        }

        // 5/22/2009 - 
        ///<summary>
        /// Applies a 'Rotation' bone adjustment Transform to a specific <see cref="InstancedModel"/> bone.
        ///</summary>
        ///<param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        ///<param name="boneName">Bone name to affect</param>
        ///<param name="rotationAxis">Enum <see cref="RotationAxis"/> to use</param>
        ///<param name="rotationValue">Rotation value of some value 0 or greater.</param>
        public static void SetAdjustingBoneTransform(ref InstancedItemData instancedItemData, string boneName,
                                                     RotationAxis rotationAxis, float rotationValue)
        {
            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return;

            // 8/13/2009 - Cache
            var instancedModel = instancedModels[(int)instancedItemData.ItemType];

            // 5/25/2009
            if (instancedModel == null) return;

            instancedModel.SetAdjustingBoneTransform(boneName, instancedItemData.ItemInstanceKey, rotationAxis, rotationValue);
        }

        // 10/1/2008 -      
        ///<summary>
        /// Applies a bone adjustment Transform to a specific <see cref="InstancedModel"/> bone. 
        ///</summary>
        ///<param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        ///<param name="boneName">Bone name to affect</param>
        ///<param name="adjTransform">New adjustment transform to apply</param>
        public static void SetAdjustingBoneTransform(ref InstancedItemData instancedItemData, string boneName, ref Matrix adjTransform)
        {
            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return;

            // 8/13/2009 - Cache
            var instancedModel = instancedModels[(int)instancedItemData.ItemType];

            // 5/25/2009
            if (instancedModel == null) return;

            instancedModel.SetAdjustingBoneTransform(boneName, instancedItemData.ItemInstanceKey, ref adjTransform);
        }

        // 1/17/2011
        /// <summary>
        /// Add or updates the given <paramref name="velocity"/> for the given <paramref name="boneName"/>.
        /// </summary>
        ///<param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        ///<param name="boneName">Bone name to affect</param>
        /// <param name="velocity">New explosion velocity value as <see cref="Vector3"/>.</param>
        public static void AddBoneExplosionVelocity(ref InstancedItemData instancedItemData, string boneName, ref Vector3 velocity)
        {
            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return;

            // 8/13/2009 - Cache
            var instancedModel = instancedModels[(int)instancedItemData.ItemType];

            // 5/25/2009
            if (instancedModel == null) return;

            instancedModel.AddBoneExplosionVelocity(boneName, instancedItemData.ItemInstanceKey, ref velocity);
        }

        // 1/17/2011
        /// <summary>
        /// Retrieves the explosion velocity for the given <paramref name="boneName"/>.
        /// </summary>
         ///<param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        ///<param name="boneName">Bone name to affect</param>
        /// <returns>Explosion velocity as <see cref="Vector3"/>.</returns>
        public Vector3 RetrieveBoneExplosionVelocity(ref InstancedItemData instancedItemData, string boneName)
        {
            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return Vector3.Zero;

            // 8/13/2009 - Cache
            var instancedModel = instancedModels[(int)instancedItemData.ItemType];

            // 5/25/2009
            return instancedModel == null ? Vector3.Zero : 
                instancedModel.RetrieveBoneExplosionVelocity(boneName, instancedItemData.ItemInstanceKey);
        }

#if !XBOX360

        // 6/22/2009 -
        ///<summary>
        /// PhysX Cloth setting for some bone Transform. (Non Production)
        ///</summary>
        ///<param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        ///<param name="boneName">Bone name to affect</param>
        public static void SetPhysXClothForBoneTransform(ref InstancedItemData instancedItemData, string boneName)
        {
            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return;

            // 8/13/2009 - Cache
            var instancedModel = instancedModels[(int)instancedItemData.ItemType];

            // 5/25/2009
            if (instancedModel == null) return;

            instancedModel.SetPhysXClothForBoneTransform(boneName);
        }

        // 6/24/2009 - 
        ///<summary>
        /// PhysX SoftBody setting for some bone Transform.
        ///</summary>
        ///<param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        ///<param name="boneName">Bone name to affect</param>
        ///<param name="modelPathName">Relative directory path name</param>
        public static void SetPhysXSoftBodyForBoneTransform(ref InstancedItemData instancedItemData, string boneName,
                                                            string modelPathName)
        {
            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return;

            // 8/13/2009 - Cache
            var instancedModel = instancedModels[(int)instancedItemData.ItemType];

            // 5/25/2009
            if (instancedModel == null) return;

            instancedModel.SetPhysXSoftBodyForBoneTransform(boneName, modelPathName);
        }

#endif
        // 6/8/2009 - 
        ///<summary>
        /// Resets a AdjustingBone Transform, back to the original 'Model' value.
        ///</summary>
        ///<param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        ///<param name="boneName">Bone name to affect</param>
        public static void ResetAdjustingBoneTransform(ref InstancedItemData instancedItemData, string boneName)
        {
            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return;

            // 8/13/2009 - Cache
            var instancedModel = instancedModels[(int)instancedItemData.ItemType];

            // 5/25/2009
            if (instancedModel == null) return;

            instancedModel.ResetAdjustingBoneTransform(boneName,instancedItemData.ItemInstanceKey);
        }

        // 2/23/2009 - 
        ///<summary>
        /// Removes an Adjusting Bone Entry from internal dicitonary.
        ///</summary>
        ///<param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        public static void RemoveAdjustingBoneTransform(ref InstancedItemData instancedItemData)
        {
            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return;

            // 8/13/2009 - Cache
            var instancedModel = instancedModels[(int)instancedItemData.ItemType];

            // 5/25/2009
            if (instancedModel == null) return;

            instancedModel.RemoveAdjustingBoneTransform(instancedItemData.ItemInstanceKey);
        }

        // 9/23/2008 - 
        ///<summary>
        /// Generates a unique ItemInstance Key
        ///</summary>
        ///<param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        ///<returns>New ItemInstance key</returns>
        public static int GenerateItemInstanceKey(ref InstancedItemData instancedItemData)
        {
            // 7/22/2008
            if (_itemCounterPerItemType != null) 
                _itemCounterPerItemType[(int) instancedItemData.ItemType] += 1;

            _itemCounter += 1;
            return (_itemCounter - 1);
        }

        // 9/12/2008
        /// <summary>
        /// Removes a Model's Instance Transform from the instanceTransforms List,
        /// located in <see cref="InstancedModel"/> class, so it no longer Draws during the Batch calls.
        /// </summary>
        ///<param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        public static void RemoveInstanceTransform(ref InstancedItemData instancedItemData)
        {
            // 7/21/2009 - Cache ItemType index, so done only cast once!
            var itemIndex = (int) instancedItemData.ItemType;

            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return;

            // 8/13/2009 - Cache
            var instancedModel = instancedModels[itemIndex];

            // 5/25/2009
            if (instancedModel == null) return;

            // 7/21/2009 - TODO: WHY does this get called twice? 
            ChangeRequestItem changeRequestItem;
            var instanceWorldTransforms = instancedModel.ChangeRequestItemsTransforms; // 8/13/2009
            if (!instanceWorldTransforms.TryGetValue(instancedItemData.ItemInstanceKey, out changeRequestItem)) return;
            
            // 10/18/2012 - Updated to call 'RemoveTransform'
            // Updated to use the new 'AddChangeRequest' method, to batch entries.
            changeRequestItem.ChangeRequest = ChangeRequestEnum.RemoveTransform;
            InstancedModelChangeRequests.AddChangeRequestForInstanceTransforms(instancedItemData.ItemType,
                                                                               ref changeRequestItem,
                                                                               instancedModel);

            // 12/18/2008 - Remove Node.      
            instanceWorldTransforms.Remove(instancedItemData.ItemInstanceKey);

            // 7/24/2009 - Update the ExplosionsFlag
            //instancedModel.SetDrawExplosionPiecesFlag();
        }


        // 9/12/2008
        // 12/18/2008: Added the ItemInstanceKey; 1/14/2009: Fixed Culling error, & also check for Fog-Of-War Culling.
        /// <summary>
        /// Updates the List Array 'instanceTransforms' in <see cref="InstancedModel"/> class, by checking if 
        /// the current 'itemInstance' of the <see cref="ItemType"/> is in <see cref="Camera"/> view. Should be called
        /// anytime a new instance is added, or the instance World Matrix changes! 
        /// </summary>   
        /// <remarks>Currently called from Property 'World' Set in this class.</remarks>
        ///<param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        private static void UpdateInstanceTransforms(ref InstancedItemData instancedItemData)
        {
            // 7/21/2009 - Cache ItemType index, so done only cast once!
            var itemIndex = (int) instancedItemData.ItemType;

            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return;

            // 8/13/2009 - Cache
            var instancedModel = instancedModels[itemIndex];

            // Check if ItemType is being used, otherwise exit method.
            if (instancedModel == null) return;

            // 12/22/2008 Make sure InstanceKey exist, otherwise exit method.
            ChangeRequestItem existingNode;
            var instanceWorldTransforms = instancedModel.ChangeRequestItemsTransforms; // 8/13/2009
            if (!instanceWorldTransforms.TryGetValue(instancedItemData.ItemInstanceKey, out existingNode)) return;

            // 5/6/2009 - Updated to use the new CollisionRadius BoundingSphere.
            // Check if in Camera Frustum before adding Transform               
            var tmpBoundingSphere = instancedModel.CollisionRadius;

            var tmpWorldTransform = instanceWorldTransforms[instancedItemData.ItemInstanceKey].Transform;

            // 8/27/2009: TEST removing the check of Camera InFrustrum?
            bool isInFrustrum;
            Camera.IsInCameraFrustrum(ref tmpWorldTransform, ref tmpBoundingSphere, out isInFrustrum);
           
            // 8/27/2009
            existingNode.InCameraView = isInFrustrum;

            // Save the InstanceData Node back into Array
            instanceWorldTransforms[instancedItemData.ItemInstanceKey] = existingNode;

            // 8/27/2009 - ONLY process if in Camera view.
            //if (isInFrustrum)
            {
                // 7/21/2009: Updated to use the new 'AddChangeRequest' method, to batch entries.
                existingNode.ChangeRequest = ChangeRequestEnum.AddOrUpdateTransform; // 10/16/2012
                InstancedModelChangeRequests.AddChangeRequestForInstanceTransforms(instancedItemData.ItemType, ref existingNode, instancedModel);
               
            }
            /*else  // 8/27/2009: Delete All Normal Culled Parts, since item out of camera view.
            {
                InstancedModelChangeRequests.AddChangeRequestForInstanceTransforms(itemIndex, ref existingNode,
                                                                     ChangeRequest.DeleteAllCulledParts_InstanceItem, instancedModel);
            }*/

        }
      

        // 1/6/2009
        /// <summary>
        /// Checks if a given <see cref="ItemType"/> Instance is marked as in <see cref="Camera"/> View.
        /// </summary>
        /// <param name="instancedItemData"><see cref="ItemType"/> to Check</param>        
        /// <returns>True/False of result</returns>
        public static bool IsInCameraView(ref InstancedItemData instancedItemData)
        {
            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return false;

            // 8/13/2009 - Cache
            var instancedModel = instancedModels[(int)instancedItemData.ItemType];

            // 5/25/2009
            if (instancedModel == null) return false;

            ChangeRequestItem tmpInstanceData;
            if (instancedModel.ChangeRequestItemsTransforms.TryGetValue(instancedItemData.ItemInstanceKey, out tmpInstanceData))
            {
                if (tmpInstanceData.InCameraView)
                    return true;
            }
            //System.Console.WriteLine(string.Format("The InstancedItem {0} is NOT in camera view.", instancedItemData.ItemType));
            return false;
        }

        // 10/16/2008
        /// <summary>
        /// For any given Instanced <see cref="ItemType"/>, this will return the Total <see cref="BoundingSphere"/> Radius,
        /// which encompasses all internal <see cref="InstancedModelPart"/> BoundingSphere.  It will be adjusted by the
        /// given <paramref name="adjustCollisionRadius"/> value.
        /// </summary>
        /// <param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        /// <param name="adjustCollisionRadius">Amount to scale given collision radius value</param>
        /// <returns>Radius as Float</returns>
        public static float GetInstanceItemCollisionRadius(ref InstancedItemData instancedItemData,
                                                           float adjustCollisionRadius)
        {
            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return 0;

            // 8/13/2009 - Cache
            var instancedModel = instancedModels[(int)instancedItemData.ItemType];

            // 5/25/2009
            if (instancedModel == null) return 0;


            // Need to Iterate through all Model's parts, since each one
            // has its own 'BoundingSphere'.
            var boundingSphere = new BoundingSphere();
            var modelParts = instancedModel.ModelParts; // 8/13/2009
            var modelPartsCount = modelParts.Count; // 8/13/2009
            for (var loop1 = 0; loop1 < modelPartsCount; loop1++)
            {
                // 4/20/2010 - Cache
                var instancedModelPart = modelParts[loop1];
                if (instancedModelPart == null) continue;

                var boundingSphereAdd = instancedModelPart.BoundingSphere;
                BoundingSphere.CreateMerged(ref boundingSphere, ref boundingSphereAdd, out boundingSphere);
            }

            // 5/6/2009 - Adjust Radius by the Scale for this SceneItemOwner.
            float useScale;
            GetScale(ref instancedItemData, out useScale);

            // 5/6/2009 - Now Adjust BoundingSphere by given Scale
            Matrix scaleMatrix;
            Matrix.CreateScale(useScale, out scaleMatrix);
            boundingSphere.Transform(ref scaleMatrix, out boundingSphere);

            // 5/6/2009 - Now apply the 'Adjust' collisionRadius value given           
            Matrix.CreateScale(adjustCollisionRadius, out scaleMatrix);
            boundingSphere.Transform(ref scaleMatrix, out boundingSphere);

            // 5/7/2009 - Store the AdjustCollisionRadius Matrix, for use in Picking.
            instancedModel.AdjustCollisionRadiusMatrix = scaleMatrix;
            instancedModel.AdjustCollisionRadiusSet = true;

            // 5/6/2009 - Store final CollisionRadius result
            instancedModel.CollisionRadius = boundingSphere;

            // Return Final CollisionRadius result
            return boundingSphere.Radius;
        }

        // 10/23/2008; 2/17/2009: Updated to pass back the InstancedItemTransform class reference, rather than Matrix struct.
        /// <summary>
        /// Retrieves the Absolute Bone Transform, which is combine with the adjusting Transform, of a given <see cref="InstancedModel"/> bone.
        /// </summary>
        /// <param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        /// <param name="boneName">Model Bone name</param>
        /// <param name="transform">(OUT) <see cref="InstancedItemTransform"/> structure reference</param>
        public static void GetInstanceItemCombineAbsoluteBoneTransform(ref InstancedItemData instancedItemData,
                                                                       string boneName,
                                                                       out InstancedItemTransform transform)
        {
            transform = new InstancedItemTransform();

            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return;

            // 8/13/2009 - Cache
            var instancedModel = instancedModels[(int)instancedItemData.ItemType];

            // 5/25/2009
            if (instancedModel == null) return;

            instancedModel.GetModelPartCombineAbsoluteTransform(boneName,
                                                                instancedItemData.
                                                                    ItemInstanceKey,
                                                                out transform);
        }

        // 2/6/2009
        /*/// <summary>
        /// Removes an <see cref="ItemType"/>'s Instance animation from the Animation Dictionary.
        /// </summary>
        /// <param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        public static void RemoveInstanceModelAnimation(ref InstancedItemData instancedItemData)
        {
            // 2/10/2009
            if (InstanceModels == null)
                return;

            // 8/13/2009
            var instancedModel = InstanceModels[(int)instancedItemData.ItemType];

            // 5/25/2009
            if (instancedModel == null)
                return;

            instancedModel.RemoveInstanceModelAnimation(instancedItemData.ItemInstanceKey);
        }*/

        // 1/6/2009; 2/17/2009: Updated to fix the ref 'Transform' parameter from being affected!
        /// <summary>
        /// Given a Bone Transform and <see cref="SceneItem"/>, this will convert the given Bone Position from Model Space into
        /// World Space.
        /// </summary>
        /// <param name="sceneItem"><see cref="SceneItem"/> instance</param>
        /// <param name="transform">Bone Transform to convert</param>
        /// <param name="position">(OUT) Converted Bone Position into World Space</param>
        public static void GetWorldPositionFromBoneTransform(SceneItemWithPick sceneItem, ref Matrix transform,
                                                             out Vector3 position)
        {
            // Get SceneItem's Orientation and World translations
            var shapeItem = sceneItem.ShapeItem; // 8/13/2009
            var tmpOrienation = shapeItem.Orientation;
            var tmpWorld = shapeItem.WorldP;
            Matrix tmpTransform;

            // 1/6/2009 - Updated to remove Ops overload, which is slow on XBOX!
            //Transform *= (ShapeItem as SciFiTankShape).Orientation * (ShapeItem as SciFiTankShape).World;
            Matrix.Multiply(ref transform, ref tmpOrienation, out tmpTransform);
            Matrix.Multiply(ref tmpTransform, ref tmpWorld, out tmpTransform);

            // Set Final Position
            position = tmpTransform.Translation;
        }

        // 11/28/2009 - Overload version, with additional 'Adjustment' tranform.
        /// <summary>
        /// Given a Bone Transform and <see cref="SceneItem"/>, this will convert the given Bone Position from Model Space into
        /// World Space.
        /// </summary>
        /// <param name="sceneItem"><see cref="SceneItem"/> instance</param>
        /// <param name="adjTransform">Transform to Adjust by</param>
        /// <param name="transform">Bone Transform to convert</param>
        /// <param name="position">(OUT) Converted Bone Position into World Space</param>
        public static void GetWorldPositionFromBoneTransform(SceneItemWithPick sceneItem, ref Matrix adjTransform,
                                                             ref Matrix transform, out Vector3 position)
        {
            // Get SceneItem's Orientation and World translations
            var shapeItem = sceneItem.ShapeItem; // 8/13/2009
            var tmpOrienation = shapeItem.Orientation;
            var tmpWorld = shapeItem.WorldP;
            Matrix tmpTransform;

            // 11/28/2009 - Adjust by given Adjustment Transform.
            Matrix.Multiply(ref tmpOrienation, ref adjTransform, out tmpOrienation);

            // 1/6/2009 - Updated to remove Ops overload, which is slow on XBOX!
            //Transform *= (ShapeItem as SciFiTankShape).Orientation * (ShapeItem as SciFiTankShape).World;
            Matrix.Multiply(ref transform, ref tmpOrienation, out tmpTransform);
            Matrix.Multiply(ref tmpTransform, ref tmpWorld, out tmpTransform);

            // Set Final Position
            position = tmpTransform.Translation;
        }

        // 12/8/2008; 12/19/2008 - Updated to return True/False for success.
        /// <summary>
        /// Retrieves the Pure Absolute Bone Transform of a given <see cref="InstancedModel"/> bone.
        /// </summary>
        /// <param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        /// <param name="boneName">Model Bone name</param>
        /// <param name="transform">(OUT) Absolute Bone Transform</param>
        /// <returns>True or False as result.</returns>
        public static bool GetInstanceItemAbsoluteBoneTransform(ref InstancedItemData instancedItemData, string boneName,
                                                                out Matrix transform)
        {
            transform = Matrix.Identity;

            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return false;

            // 8/13/2009 - Cache
            var instancedModel = instancedModels[(int)instancedItemData.ItemType];

            // 5/25/2009
            if (instancedModel == null) return false;

            return instancedModel.GetModelPartAbsoluteTransform(boneName,
                                                                out transform);
        }

        // 12/9/2008
        /// <summary>
        /// Retrieves the Default RotationX value for the given <see cref="InstancedModel"/> Type; this is the value
        /// stored in the Model's content pipeline file.
        /// </summary>
        /// <param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        /// <param name="rotX">(OUT) New rotation X value</param>
        public static void GetRotationX(ref InstancedItemData instancedItemData, out float rotX)
        {
            rotX = 0;

            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return;

            // 8/13/2009 - Cache
            var instancedModel = instancedModels[(int)instancedItemData.ItemType];

            // 5/25/2009
            if (instancedModel == null) return;

            rotX = instancedModel.RotX;
        }

        // 12/9/2008
        /// <summary>
        /// Retrieves the Default RotationY value for the given <see cref="InstancedModel"/> Type; this is the value
        /// stored in the Model's content pipeline file.
        /// </summary>
        /// <param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        /// <param name="rotY">(OUT) New rotation Y value</param>
        public static void GetRotationY(ref InstancedItemData instancedItemData, out float rotY)
        {
            rotY = 0;

            // 4/20/2010
            var instancedModels = InstanceModels;
            if (instancedModels == null) return;

            // 8/13/2009
            var instancedModel = instancedModels[(int)instancedItemData.ItemType];

            // 5/25/2009
            if (instancedModel == null) return;

            rotY = instancedModel.RotY;
        }

        // 12/9/2008
        /// <summary>
        /// Retrieves the Default RotationZ value for the given <see cref="InstancedModel"/> Type; this is the value
        /// stored in the Model's content pipeline file.
        /// </summary>
        /// <param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        /// <param name="rotZ">(OUT) New rotation Z value</param>
        public static void GetRotationZ(ref InstancedItemData instancedItemData, out float rotZ)
        {
            rotZ = 0;

            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return;

            // 8/13/2009 - Cache
            var instancedModel = instancedModels[(int)instancedItemData.ItemType];

            // 5/25/2009
            if (instancedModel == null) return;

            rotZ = instancedModel.RotZ;
        }

        // 1/1/2009
        /// <summary>
        /// Retrieves the Default Scale value for the given <see cref="InstancedModel"/> Type; this is the value
        /// stored in the Model's content pipeline file.
        /// </summary>
        /// <param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        /// <param name="scale">(OUT) New Scale value</param>
        public static void GetScale(ref InstancedItemData instancedItemData, out float scale)
        {
            scale = 1.0f;

            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return;

            // 8/13/2009 - Cache
            var instancedModel = instancedModels[(int)instancedItemData.ItemType];

            // 5/25/2009
            if (instancedModel == null) return;

            // 11/7/2009 - Updated to NOT allow scale of 0!
            scale = (instancedModel.Scale > 0) ? instancedModel.Scale : 1;
        }

        // 7/13/2009
        /// <summary>
        /// Retrieves the internal <see cref="ChangeRequestItem"/> structure, for the given <see cref="InstancedModel"/> Type.
        /// </summary>
        /// <param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        /// <param name="instanceDataCommunication">(OUT) <see cref="ChangeRequestItem"/> structure</param>
        public static void GetInstancedModel_InstanceDataNode(ref InstancedItemData instancedItemData,
                                                              out ChangeRequestItem? instanceDataCommunication)
        {
            instanceDataCommunication = null;

            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return;

            // 8/13/2009 - Cache
            var instancedModel = instancedModels[(int) instancedItemData.ItemType];

            if (instancedModel == null) return;

            instanceDataCommunication =instancedModel.ChangeRequestItemsTransforms[instancedItemData.ItemInstanceKey];
        }

       

        // 2/11/2009; 2/17/2009: Updated to pass back the 'InstancedItemTransform' class ref, rather than Matrix struct.
        /// <summary>
        /// Returns a reference to the internal List for the SpawnBullet position transforms, for the
        /// given <see cref="ItemType"/>.
        /// </summary>
        /// <param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        /// <param name="spawnBulletTransforms">List array of <see cref="InstancedItemTransform"/>.</param>
        public static void GetInstancedModelSpawnBulletPositions(ref InstancedItemData instancedItemData,
                                                                 List<InstancedItemTransform> spawnBulletTransforms)
        {
            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return;

            // 8/13/2009 - Cache
            var instancedModel = instancedModels[(int)instancedItemData.ItemType];

            // 5/25/2009
            if (instancedModel == null) return;

            instancedModel.GetInstancedModelSpawnBulletTransforms(instancedItemData.ItemInstanceKey, spawnBulletTransforms);
        }

        // 12/9/2008
        /// <summary>
        /// Call this method if you want to affect the rotation of the <see cref="InstancedModel"/> 
        /// on the Display only!  In other words, this will not be used in the
        /// World transform within the <see cref="SceneItem"/>.  This is useful if you need
        /// to rotate the models front to correct a rotation error, but don't want this
        /// rotation value to be included in any Orientation <see cref="AbstractBehavior"/>!
        /// </summary>
        /// <param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        public static void ApplyRotationValuesToRootTranform(ref InstancedItemData instancedItemData)
        {
            // 4/20/2010
            var instancedModels = InstanceModels;
            if (instancedModels == null) return;

            // 8/13/2009 - Cache
            var instancedModel = instancedModels[(int)instancedItemData.ItemType];

            // 5/25/2009
            if (instancedModel == null) return;

            instancedModel.ApplyRotationValuesToRootTranform();
        }

        // 2/2/2010 - Updated to return the 'Ray' hit distance as (OUT) param.
        // 9/23/2008 - Checks if a given InstanceModel is being picked by the mouse Cursor.
        // 11/19/2008 - Updated to NOT use the 'HasItemInstanceKey' predicate, since this causes additional garbage!
        // 12/18/2008 - Updated to use Dictionary.
        ///<summary>
        /// Checks if a given <see cref="InstancedItem"/> model has been picked by the user mouse.
        ///</summary>
        /// <param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        ///<param name="intersectionDistance">(OUT) Intersection value of pick, or Null if no hit.</param>
        ///<returns>True/False of result</returns>
        public static bool IsMeshPicked(ref InstancedItemData instancedItemData, out float? intersectionDistance)
        {
            intersectionDistance = null; // 2/2/2010

            Ray cursorRay;
            Cursor.CalculateCursorRay(out cursorRay);

            // 4/20/2010 - Cache
            var instancedModels = InstanceModels;
            if (instancedModels == null) return false;

            // 8/13/2009 - Cache
            var itemType = (int)instancedItemData.ItemType; // 6/13/2010
            var instancedModel = instancedModels[itemType];

            if (instancedModel == null) return false;

            var instanceWorldTransforms = instancedModel.ChangeRequestItemsTransforms; // 8/13/2009
            if (instanceWorldTransforms.ContainsKey(instancedItemData.ItemInstanceKey))
            {
                var instanceDataItem = instanceWorldTransforms[instancedItemData.ItemInstanceKey];

                // check to see if the cursorRay intersects the model....
                if (instanceDataItem.ShapeItem != null)
                {
                    //var worldTransform = instanceDataItem.ShapeItem.WorldP; // 8/27/2009
                    int closestModelPartIndex; // 2/3/2010
                    intersectionDistance = RayIntersectsModel(ref cursorRay, instancedModel, ref instanceDataItem, out closestModelPartIndex); // 2/2/2010

                    if (intersectionDistance != null)
                    {
#if !XBOX360
                        // 3/3/2009
                        // Display in Red for Editing purposes.
                        if (TerrainShape.TerrainIsIn == TerrainIsIn.EditMode)
                        {
                            //instanceDataItem.IsPicked = true;
                            //instanceWorldTransforms[instancedItemData.ItemInstanceKey] = instanceDataItem;
                            
                            // 2/3/2010 - Update the specific modelpart to be Red material.
                            ChangeRequestItem existingNode;
                            instancedModel.GetInstanceDataNode(instancedItemData.ItemInstanceKey, out existingNode);
                            
                            // Update the 'ProceduralMaterialId' param to be 5 for 'Red' matieral.
                            instancedModel.UpdateInstanceDataNodeSpecificParameter(instanceDataItem.ItemInstanceKey,
                                                                                   InstanceDataParam.ProceduralMaterialId, 5);

                            // Update the 'ModelPartIndexKey' param to the current picked part.
                            instancedModel.UpdateInstanceDataNodeSpecificParameter(instanceDataItem.ItemInstanceKey,
                                                                                   InstanceDataParam.ModelPartIndexKey, closestModelPartIndex);

                            // Add to ChangeRequests.
                            existingNode.ChangeRequest = ChangeRequestEnum.AddOrUpdateProcedureId; // 10/16/2012
                            InstancedModelChangeRequests.AddChangeRequestForInstanceTransforms(instancedItemData.ItemType, ref existingNode, instancedModel);
                        }
#endif

                        return true;
                    } // Hit?

                } // ShapeItem not null.
                
            } // end If Contains ItemKey.

            return false;
        }


        // 2/3/2010 - Updated to retrieve the ModelParts 'Normal' keys, which elivates the amount
        //            of records to itereate through!  Also, updated the signature, by removing the
        //            'WorldParam', and instead passing in the 'InstanceData' param.
        // 8/26/2008: Updated to optimize memory.   
        /// <summary>
        /// This helper function checks to see if a ray will intersect with a <see cref="InstancedModel"/>.
        /// The model's <see cref="BoundingSphere"/> are used, and the model is transformed using
        /// the matrix specified in the world transform argument.
        /// </summary>
        /// <param name="ray">The ray to perform the intersection check with</param>
        /// <param name="model">The <see cref="InstancedModel"/> to perform the intersection check with;
        /// the model's <see cref="BoundingSphere"/> will be used.</param>
        /// <param name="instanceDataCommunication"><see cref="ChangeRequestItem"/> structure</param>
        /// <param name="closestModelPartIndex">(OUT) index of the closest model part hit</param>
        /// <returns>True/False of result</returns>
        private static float? RayIntersectsModel(ref Ray ray, InstancedModel model,
                                               ref ChangeRequestItem instanceDataCommunication, out int closestModelPartIndex)
        {

            var worldTransform = instanceDataCommunication.ShapeItem.WorldP; // 2/3/2010
            var modelPartsKeys = model.ModelPartsKeys; // 2/3/2010
            var modelParts = model.ModelParts; // 8/13/2009
            var modelPartsCount = model.ModelPartsCount; // 2/3/2010 - Count of ONLY the 'Normal' type model-parts.

            // 2/2/2010 - Keep track of the closest modelPart we have seen so far, so we can
            // choose the closest one if there are several parts under the cursor.
            float? closestIntersection = null;
            closestModelPartIndex = -1;

            // Each ModelMesh in a Model has a bounding sphere, so to check for an
            // intersection in the Model, we have to check every mesh.
            // 8/26/2008: Updated to For-Loop, rather than ForEach.
            BoundingSphere sphere;
            for (var i = 0; i < modelPartsCount; i++)
            {
                // 2/3/2010 - Retrieve specific 'Normal' modelparts index.
                var modelPartIndex = modelPartsKeys[i];
                // 2/3/2010 - Get the specific 'ModelPart' using index key.
                var modelPart = modelParts[modelPartIndex];

                // the mesh's BoundingSphere is stored relative to the mesh itself.
                // (Mesh space). We want to get this BoundingSphere in terms of World
                // coordinates. To do this, we calculate a matrix that will Transform
                // from coordinates from mesh space into World space....
                //Matrix World = absoluteBoneTransforms[mesh.ParentBone.Index] * worldTransform;
                var tmpMeshBoundingSphere = modelPart.BoundingSphere;

                // 5/7/2009 - Let's adjust the BoundingSphere by the 'AdjustCollisionRadius' value.
                if (model.AdjustCollisionRadiusSet)
                    tmpMeshBoundingSphere.Transform(ref model.AdjustCollisionRadiusMatrix, out tmpMeshBoundingSphere);

                // ... and then Transform the BoundingSphere using that matrix.               
                TransformBoundingSphere(ref tmpMeshBoundingSphere, ref worldTransform, out sphere);

                // 2/2/2010 - Reset IsPicked to false.
                if (modelPart.IsMeshPicked)
                {
                    modelPart.IsMeshPicked = false;
                    // Update the 'ProceduralMaterialId' param to be 0, which forces shader to use default.
                    model.UpdateInstanceDataNodeSpecificParameter(instanceDataCommunication.ItemInstanceKey,
                                                                 InstanceDataParam.ProceduralMaterialId, 0);
                }
               

                // 2/2/2010: Updated to set the 'IsMeshPicked' property in the ModelPart.
                // now that the we have a sphere in World coordinates, we can just use
                // the BoundingSphere class's Intersects function. Intersects returns a
                // nullable float (float?). This value is the distance at which the ray
                // intersects the BoundingSphere, or null if there is no intersection.
                // so, if the value is not null, we have a collision.
                float? intersection;
                sphere.Intersects(ref ray, out intersection);
                if (intersection == null) continue;

                // 2/2/2010
                // If so, is it closer than any other model we might have
                // previously intersected?
                if ((closestIntersection != null) && (intersection >= closestIntersection)) continue;

                // Yes, so store information about this model.
                closestIntersection = intersection.Value;
                closestModelPartIndex = modelPartIndex;
            }

            // 2/3/2010
            if (closestModelPartIndex > -1)
                modelParts[closestModelPartIndex].IsMeshPicked = true;

            // if we've gotten this far, we've made it through every BoundingSphere, and
            // none of them intersected the ray. This means that there was no collision,
            // and we should return false.
            return closestIntersection;
        }

        /// <summary>
        /// This helper function takes a <see cref="BoundingSphere"/> and a transform matrix, and
        /// returns a transformed version of that <see cref="BoundingSphere"/>.
        /// </summary>
        /// <param name="sphere">The <see cref="BoundingSphere"/> to transform</param>
        /// <param name="transform">Transform matrix to use</param>
        /// <param name="transformedSphere">(OUT) The transformed <see cref="BoundingSphere"/></param>
        private static void TransformBoundingSphere(ref BoundingSphere sphere,
                                                              ref Matrix transform,
                                                              out BoundingSphere transformedSphere)
        {

            // the Transform can contain different scales on the x, y, and z components.
            // this has the effect of stretching and squishing our bounding sphere along
            // different axes. Obviously, this is no good: a bounding sphere has to be a
            // SPHERE. so, the transformed sphere's radius must be the maximum of the 
            // scaled x, y, and z radii.

            // to calculate how the Transform matrix will affect the x, y, and z
            // components of the sphere, we'll create a vector3 with x y and z equal
            // to the sphere's radius...
            var scale3 = new Vector3(sphere.Radius, sphere.Radius, sphere.Radius);

            // then Transform that vector using the Transform matrix. we use
            // TransformNormal because we don't want to take translation into account.
            //scale3 = Vector3.TransformNormal(scale3, Transform);
            Vector3.TransformNormal(ref scale3, ref transform, out scale3);

            // scale3 contains the x, y, and z radii of a squished and stretched sphere.
            // we'll set the finished sphere's radius to the maximum of the x y and z
            // radii, creating a sphere that is large enough to contain the original 
            // squished sphere.
            //transformedSphere.Radius = Math.Max(scale3.X, Math.Max(scale3.Y, scale3.Z));
            var maxValue1 = scale3.X > scale3.Y ? scale3.X : scale3.Y;
            transformedSphere.Radius = maxValue1 > scale3.Z ? maxValue1 : scale3.Z;

            // transforming the center of the sphere is much easier. we can just use 
            // Vector3.Transform to Transform the center vector. notice that we're using
            // Transform instead of TransformNormal because in this case we DO want to 
            // take translation into account.
            var sphereCenter = sphere.Center;
            Vector3 tmpCenter2;
            //transformedSphere.Center = Vector3.Transform(sphere.Center, Transform);
            Vector3.Transform(ref sphereCenter, ref transform, out tmpCenter2);
            transformedSphere.Center = tmpCenter2;
           
        }

        #region Dispose

        // 11/18/2009
        /// <summary>
        /// Clears out some of the arrays for level reloading.
        /// </summary>
        internal static void ClearForLevelReload()
        {
            // 1/7/2010
            // Dispose of Only Scenary InstanceModels
            if (InstanceModels != null)
            {
                // cache
                var scenaryItemsLoaded = InstancedItemLoader.ScenaryItemsLoaded;
                var count = scenaryItemsLoaded.Count;

                // Iterate the ScenaryItemsLoaded list.
                for (var i = 0; i < count; i++)
                {
                    var itemType = scenaryItemsLoaded[i];
                    var itemTypeIndex = (int)itemType;

                    if (InstanceModels[itemTypeIndex] == null) continue;

                    InstanceModels[itemTypeIndex].Dispose();
                    InstanceModels[itemTypeIndex] = null;
                }
            }

            // 1/7/2010
            // Clear all internal Change Requests & internal Arrays
            if (InstanceModels != null)
            {
                var length = InstanceModels.Length;
                for (var i = 0; i < length; i++)
                {
                    var instancedModel = InstanceModels[i];

                    if (instancedModel == null) continue;

                    instancedModel.InstancedModelChangeRequestManager.Dispose();
                    instancedModel.ClearForLevelReloads(); // 1/8/2010
                }
            }

            // Dispose of List
            if (_itemCounterPerItemType != null)
            {
                _itemCounterPerItemType.Clear();
                _itemCounterPerItemType = null;
            }

            // 11/18/2009 - Clear List
            InstancedItemLoader.ClearForLevelReload();

            // Clear all Static Arrays               
            _instanceModelsDrawn = null;
            _instanceShadowModelsDrawn = null;

            // 5/27/2009 - Clear All InstanceModels Instances
            ClearAllInstanceModelsTransforms();

            // 1/5/2010 - Call UnLoad for ContentManagers
            //PlayableItemsContentManager.Unload(); // Playable items
            ScenaryItemsContentManager.Unload(); // Scenary items

            // set back to false.
            InitInstanceArrayListsCompleted = false;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the GameComponent and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 11/18/2009
                ClearForLevelReload();
            }

            base.Dispose(disposing);
            GC.SuppressFinalize(this);
        }

        // 8/18/2008 - Static method to Dispose of all InstanceModels
        //             in memory, only when entire App Shut Down.
        ///<summary>
        /// Static method to Dispose of all <see cref="InstancedModel"/> in memory:
        /// only when entire App Shut Down.
        ///</summary>
        public static void DisposeInstanceModels()
        {
            // Dispose of all InstanceModels
            if (InstanceModels != null)
            {
                for (var i = 0; i < InstanceModels.Length; i++)
                {
                    if (InstanceModels[i] != null)
                        InstanceModels[i].Dispose();

                    InstanceModels[i] = null;
                }
            }
            InstanceModels = null;

            // Dispose of ShadowInstanceModelsKeys
            if (_shadowInstanceModelsKeysStatic != null)
            {
                _shadowInstanceModelsKeysStatic.Clear();
                _shadowInstanceModelsKeysStatic = null;
            }

            // 2/19/2009
            // Dispose of AlphaInstanceModelKeys
            if (_alphaMapInstanceModelsKeys != null)
            {
                _alphaMapInstanceModelsKeys.Clear();
                _alphaMapInstanceModelsKeys = null;
            }

            // 2/19/2009
            // Dispose of IllumInstanceModelKeys
            if (_illumMapInstanceModelsKeys != null)
            {
                _illumMapInstanceModelsKeys.Clear();
                _illumMapInstanceModelsKeys = null;
            }

            // 7/10/2009
            // Dispose of Selectable Keys
            if (_selectableItemsInstanceModelsKeys != null)
            {
                _selectableItemsInstanceModelsKeys.Clear();
                _selectableItemsInstanceModelsKeys = null;
            }

            // 7/10/2009
            // Dispose of Scenery _keys
            if (_sceneryItemsInstanceModelsKeys != null)
            {
                _sceneryItemsInstanceModelsKeys.Clear();
                _sceneryItemsInstanceModelsKeys = null;
            }

            // Dispose of InstanceModels ContentManager
            if (PlayableItemsContentManager != null)
            {
                PlayableItemsContentManager.Unload();
                PlayableItemsContentManager.Dispose();
                PlayableItemsContentManager = null;
            }
        }

        #endregion
    }
}