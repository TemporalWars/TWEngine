#region File Description
//-----------------------------------------------------------------------------
// ScenaryItemScene.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using ImageNexus.BenScharbach.TWEngine.Audio.Enums;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.Common;
using ImageNexus.BenScharbach.TWEngine.Common.Extensions;
using ImageNexus.BenScharbach.TWEngine.GameCamera;
using ImageNexus.BenScharbach.TWEngine.GameScreens;
using ImageNexus.BenScharbach.TWEngine.IFDTiles;
using ImageNexus.BenScharbach.TWEngine.InstancedModels;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Structs;
using ImageNexus.BenScharbach.TWEngine.Interfaces;
using ImageNexus.BenScharbach.TWEngine.ItemTypeAttributes;
using ImageNexus.BenScharbach.TWEngine.Players;
using ImageNexus.BenScharbach.TWEngine.SceneItems.Enums;
using ImageNexus.BenScharbach.TWEngine.SceneItems.Structs;
using ImageNexus.BenScharbach.TWEngine.Shapes;
using ImageNexus.BenScharbach.TWEngine.Terrain;
using ImageNexus.BenScharbach.TWEngine.Utilities;
using ImageNexus.BenScharbach.TWLate.AStarInterfaces.AStarAlgorithm.Enums;
using Microsoft.Xna.Framework;

#if XBOX360

#endif

namespace ImageNexus.BenScharbach.TWEngine.SceneItems
{

    ///<summary>
    /// The <see cref="ScenaryItemScene"/> is used only for static scenary items, like
    /// trees, bushes, plants, or decorative buildings, such as wind-mills.
    ///</summary>
    public sealed class ScenaryItemScene : SceneItem
    { 
        // 10/29/2008
        private static ITerrainShape _terrainShape;

        // 4/14/2009 - List of instances for given ItemType; this now holds the 'Positions', 'Rotation', 'Scale' data.
        internal List<ScenaryItemData> ScenaryItems = new List<ScenaryItemData>();

        // 6/10/2012 - Additional collection to track ScenaryItems which emit sounds.
        private readonly List<ScenaryItemDataAudio> _scenaryItemsWithAudio = new List<ScenaryItemDataAudio>();

        // 10/8/2009 - Dictionary of the internal ScenaryItems, searchable by 'Name'; specifically added for scripting purposes.
        //             Key = 'Name' of item, and Value = Index of 'ScenaryItems' List above.
        internal Dictionary<string, int> ScenaryItemsByName = new Dictionary<string, int>();

        #region Properties

        // 5/31/2012
        /// <summary>
        /// Gets or sets if the <see cref="SceneItem"/> was spawned with some scripting action.  
        /// </summary>
        /// <remarks>This flag is used to remove item spawned dynamically when saving map data.</remarks>
        public override bool SpawnByScriptingAction
        {
            get
            {
                try
                {
                    // if not -1 index, then return item from array
                    return ShapeItem.InstancedItemPickedIndex != -1 && ScenaryItems[ShapeItem.InstancedItemPickedIndex].SpawnByScriptingAction;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("SpawnByScriptingAction property in ScenaryItemScene class threw the exception " + ex.Message);
                    return false;
                }
            }
            set
            {
                ScenaryItemData itemData;
                if (!GetScenaryItemData(out itemData)) return;

                itemData.SpawnByScriptingAction = value;

                // Set ItemData struct
                SetScenaryItemData(itemData);
            }
        }

        // 11/11/2008 - Override Delete so we can also delete Transform from InstanceItem.
        /// <summary>
        /// Should this <see cref="ScenaryItemScene"/> be deleted?
        /// </summary>
        public override bool Delete
        {
            get
            {
                return base.Delete;
            }
            set
            {
                // Delete Transform for InstanceItem, if True
                if (value)
                {
                    InstancedItem.RemoveInstanceTransform(ref ShapeItem.InstancedItemData);
                }

                // 7/2/2009 - All delete items form the 'ScenaryItems' list.
                var scenaryItems = ScenaryItems; // 4/26/2010 - Cache
                if (scenaryItems == null) return;

                var count = scenaryItems.Count; // 4/26/2010 - Cache
                if (count > 0)
                {
                    for (var i = 0; i < count; i++)
                    {
                        var scenaryItemData = scenaryItems[i];
                        InstancedItem.RemoveInstanceTransform(ref scenaryItemData.instancedItemData);
                    }
                    scenaryItems.Clear();
                }

                // 1/29/2009 - Set 'DoRemoveAllCheck' to True in the TerrainScreen class
                TerrainScreen.DoRemoveAllCheck = true;

                base.Delete = value;
            }
        }

        // 4/9/2009
        ///<summary>
        /// Get or Set the <see cref="ScenaryItemShape"/> instance
        ///</summary>
        public new ScenaryItemShape ShapeItem
        {
            get
            {
                return (base.ShapeItem as ScenaryItemShape);
            }
            set
            {
                ShapeItem = value;
            }

        }

        // 4/14/2009
        /// <summary>
        /// The <see cref="Vector3"/> position of this <see cref="ScenaryItemScene"/>
        /// </summary>
        public override Vector3 Position
        {
            get
            {
                try
                {
                    // 10/6/2009 - if not -1 index, then return item from array
                    return ShapeItem.InstancedItemPickedIndex == -1 ? Vector3.Zero : ScenaryItems[ShapeItem.InstancedItemPickedIndex].position;
                }
                catch (Exception ex)
                {
#if DEBUG
                    Debug.WriteLine("Position property in ScenaryItemScene class threw the exception " + ex.Message);
#endif
                    return Vector3.Zero;
                }
                
            }
            set
            {
                // 4/26/2010 - Retrieve ItemData struct.
                ScenaryItemData itemData;
                if (!GetScenaryItemData(out itemData)) return;

                itemData.position = value;

                // 6/7/2012 - Set position updated.
                itemData.instancedItemData.PositionUpdated = true;

                // 4/26/210 - Set ItemData struct
                SetScenaryItemData(itemData);
            }
        }

       

        // 4/14/2009
        /// <summary>
        /// The current rotation for this <see cref="SceneItem"/>
        /// </summary>
        public override Quaternion Rotation
        {
            get
            {
                try
                {
                    // 10/6/2009 - if not -1 index, then return item from array
                    return ShapeItem.InstancedItemPickedIndex == -1 ? Quaternion.Identity : ScenaryItems[ShapeItem.InstancedItemPickedIndex].rotation;
                }
                catch (Exception ex)
                {
#if DEBUG
                    Debug.WriteLine("Rotation property in ScenaryItemScene class threw the exception " + ex.Message);
#endif
                    return Quaternion.Identity;
                }
            }
            set
            {
                // 4/26/2010 - Retrieve ItemData struct.
                ScenaryItemData itemData;
                if (!GetScenaryItemData(out itemData)) return;

                itemData.rotation = value;

                // 4/26/210 - Set ItemData struct
                SetScenaryItemData(itemData);
            }
        }

        // 4/3/2011
        /// <summary>
        /// The current scale for this <see cref="SceneItem"/>
        /// </summary>
        public override Vector3 Scale
        {
            get
            {
                try
                {
                    // 10/6/2009 - if not -1 index, then return item from array
                    return ShapeItem.InstancedItemPickedIndex == -1 ? Vector3.One : ScenaryItems[ShapeItem.InstancedItemPickedIndex].scale;
                }
                catch (Exception ex)
                {
#if DEBUG
                    Debug.WriteLine("Scale property in ScenaryItemScene class threw the exception " + ex.Message);
#endif
                    return Vector3.Zero;
                }
            }
            set
            {
                // 4/26/2010 - Retrieve ItemData struct.
                ScenaryItemData itemData;
                if (!GetScenaryItemData(out itemData)) return;

                itemData.scale = value;

                // 4/26/210 - Set ItemData struct
                SetScenaryItemData(itemData);
            }
        } 

        // 6/6/2012
        public override Guid UniqueKey
        {
            get
            {
                try
                {
                    // 10/6/2009 - if not -1 index, then return item from array
                    return ShapeItem.InstancedItemPickedIndex == -1 ? base.UniqueKey : ScenaryItems[ShapeItem.InstancedItemPickedIndex].UniqueKey;
                }
                catch (Exception ex)
                {
#if DEBUG
                    Debug.WriteLine("UniqueKey property in ScenaryItemScene class threw the exception " + ex.Message);
#endif
                    return Guid.Empty;
                }
            }
        }
       
        
        // 10/6/2009
        /// <summary>
        /// User defined name, used to itentify this instance 
        /// within the Scripting conditions.
        /// </summary>
        public override string Name
        {
            get
            {
                try
                {
                    // 10/6/2009 - if not -1 index, then return item from array
                    return ShapeItem.InstancedItemPickedIndex == -1 ? base.Name : ScenaryItems[ShapeItem.InstancedItemPickedIndex].name;
                }
                catch (Exception ex)
                {
#if DEBUG
                    Debug.WriteLine("Name property in ScenaryItemScene class threw the exception " + ex.Message);
#endif
                    return string.Empty;
                }
            }
            set
            {
                // 4/26/2010 - Retrieve ItemData struct.
                ScenaryItemData itemData;
                if (!GetScenaryItemData(out itemData)) return;

                itemData.name = value;

                ScenaryItems[ShapeItem.InstancedItemPickedIndex] = itemData;

                // 5/18/2012 - Add index to ScenaryItemNames Dictionary
                AddToScenaryItemsByNameDictionary(value, ShapeItem.InstancedItemPickedIndex);

                // 5/17/2012 - Set also to Base, which sets into the Player's dictionary. (Scripting Purposes)
                base.Name = value;
            }
        }

        // 5/17/2012
        /// <summary>
        /// Sets or Gets the current PickedIndex value.
        /// </summary>
        public int InstancedItemPickedIndex
        {
            get
            {
                return (ShapeItem == null) ? -1 : ShapeItem.InstancedItemPickedIndex;
            }
            set { if (ShapeItem != null) ShapeItem.InstancedItemPickedIndex = value; }
        }


        // 4/27/2010
        /// <summary>
        /// Helper method, which returns the <see cref="ScenaryItemData"/> for
        /// the given picked index set in <see cref="ScenaryItemShape.InstancedItemPickedIndex"/>.
        /// </summary>
        /// <param name="itemData">(OUT) <see cref="ScenaryItemData"/> structure</param>
        /// <returns>true/false of result</returns>
        private bool GetScenaryItemData(out ScenaryItemData itemData)
        {
            itemData = default(ScenaryItemData);

            var scenaryItemShape = ShapeItem;
            if (scenaryItemShape == null) return false;

            // 4/26/2010 - Cache
            var scenaryItems = ScenaryItems;
            if (scenaryItems == null) return false;

            // if there is a picked index, then set value.
            if (scenaryItemShape.InstancedItemPickedIndex == -1) return false;
            itemData = scenaryItems[scenaryItemShape.InstancedItemPickedIndex];
            return true;
        }

        // 4/26/2010
        /// <summary>
        /// Helper method, which sets the given <see cref="ScenaryItemData"/> for
        /// the given picked index set in <see cref="ScenaryItemShape.InstancedItemPickedIndex"/>.
        /// </summary>
        /// <param name="itemData">Updated <see cref="ScenaryItemData"/> structure to set</param>
        private void SetScenaryItemData(ScenaryItemData itemData)
        {
            var scenaryItemShape = ShapeItem;
            if (scenaryItemShape == null) 
                throw new InvalidOperationException("The internal 'ShapeItem' instance is null, which is not allowed for the given method!");

            var scenaryItems = ScenaryItems;
            if (scenaryItems == null)
                throw new InvalidOperationException("The collection 'ScenaryItems' is null, which is not allowed for the given method!");

            // 10/6/2009: Updated to have 'CreateWorldMatrixTransforms' called BEFORE saving back into array!
            // Update with proper World Matrix
            CreateWorldMatrixTransforms(ref itemData);
            scenaryItems[scenaryItemShape.InstancedItemPickedIndex] = itemData;

            InstancedItem.UpdateScenaryModelTransform(scenaryItemShape, false, ref itemData.instancedItemData, 0);
        }

        #endregion

        #region constructors
       
        // NOTE: This constructor is ONLY called from the ItemsTool.
        /// <summary>
        /// Creates a <see cref="ScenaryItemScene"/>, for example bushes and rocks.
        /// </summary>
        /// <param name="game">Instance of game.</param> 
        /// <param name="initialPosition">The initial position to place the item in the game world referenced as <see cref="Vector3"/>.</param>
        /// <param name="itemType">The <see cref="itemType"/> to be used.</param>  
        /// <param name="playerNumber">The unique <see cref="Player"/>'s network number.</param>     
        public ScenaryItemScene(Game game, ItemType itemType, ref Vector3 initialPosition, byte playerNumber)
            : base(game, new ScenaryItemShape(game, playerNumber), initialPosition)
        {
            // 11/20/2009
            CommonInitilization(game, itemType, playerNumber);

            // 3/30/2011 Refactored
            AddScenaryItemSceneInstance(itemType, ref initialPosition, playerNumber, null);

            TerrainScreen.SceneCollection.Add(this);
            // 5/31/2012 - Add to ScenaryItems Array for use in Save Routine and Smoothing Algorithm.
            TerrainScreen.TerrainShapeInterface.ScenaryItems.Add(this);
        }

        // 4/14/2009: Overload version, used when loading multiple instances of same ItemType.
        /// <summary>
        /// Creates a <see cref="ScenaryItemScene"/>, for example bushes and rocks.
        /// </summary>
        /// <param name="game">Instance of game.</param>
        /// <param name="inScenaryItems">A collection of <see cref="inScenaryItems"/>.</param>  
        /// <param name="itemType">The <see cref="itemType"/> to be used.</param> 
        /// <param name="playerNumber">The unique <see cref="Player"/>'s network number.</param>     
        public ScenaryItemScene(Game game, ItemType itemType, IEnumerable<ScenaryItemData> inScenaryItems, byte playerNumber)
            : base(game, new ScenaryItemShape(game, playerNumber))
        {
            // 11/20/2009
            CommonInitilization(game, itemType, playerNumber);

            // 4/14/2009 - Copy List given and create World transforms.
            ScenaryItems.AddRange(inScenaryItems);

            // 4/14/2009 - Load Instanced Model once here, for this given ItemType
            InstancedItem.AddScenaryInstancedItem(itemType, this);

            // 6/24/2009 - Test 'PhysX' SoftBody for given Palm trees
#if !XBOX360
            //if (ItemType == ItemType.treePalmNew002c || ItemType == ItemType.treePalm002)
            if (itemType == ItemType.treePalmNew002c)
            {               
                //InstancedItemData InstancedItemData = ShapeItem.InstancedItemDatas[0];
                //InstancedItem.SetPhysXSoftBodyForBoneTransform(ref InstancedItemData, "Leafs", ShapeItem.ItemTypeAtts.modelLoadPathName);
            }
#endif
            

        }

        /// <summary>
        /// Common Initilization setup done for both constructors.
        /// </summary>
        private void CommonInitilization(Game game, ItemType itemType, byte playerNumber)
        {
            // 11/20/2009 - Check if in PlayerItemTypeAtts Dictionary, which implies this item
            //             is a playable item, and not a sceneryitem!
            var isPlayableItemType = PlayableItemTypeAtts.ItemTypeAtts.ContainsKey(itemType);
            if (isPlayableItemType)
                throw new ArgumentException(@"ItemType enum given MUST be a scenary type item!", "itemType");

            GameInstance = game;

            // 10/29/2008
            if (_terrainShape == null)
                _terrainShape = (ITerrainShape)game.Services.GetService(typeof(ITerrainShape));
          
            // Set Collision Radius 
            CollisionRadius = 60.0f;

            // Set View Radius
            if (ShapeItem.UseFogOfWar)
                ViewRadius = ShapeItem.FogOfWarHeight * TerrainData.cScale;
            else
                ViewRadius = 700.0f;

            // 4/14/2009 - Load ItemType Atts
            LoadItemTypeAttributes(itemType);

            // 11/7/2009
            ShapeItem.InstancedItemData.ItemType = itemType;
            PlayerNumber = playerNumber;
        }


        // 11/7/2009
        /// <summary>
        /// Initalizes the collection of <see cref="ScenaryItemData"/>, by updating the Scale and World transforms
        /// for each instance.  Currently, this method is called directly from the PreLoad thread.
        /// </summary>
        public void InitializeScenaryItemsWorldTransforms()
        {
            var itemType = ShapeItem.InstancedItemData.ItemType;

            var scenaryItemsCount = ScenaryItems.Count; // 11/7/2009
            for (var i = 0; i < scenaryItemsCount; i++)
            {
                ScenaryItemData itemData = ScenaryItems[i];
                
                itemData.instancedItemData.ItemType = itemType;
                itemData.instancedItemData.ItemInstanceKey = InstancedItem.GenerateItemInstanceKey(ref itemData.instancedItemData);

                // Get Scale
                // 5/31/2012 - If current Scale is zero, then try to get default from artwork.
                if (itemData.scale.Equals(Vector3.Zero))
                {
                    GetItemTypeScale(itemData.instancedItemData);
                    itemData.scale = scale;
                }

                // Update with proper World Matrix
                CreateWorldMatrixTransforms(ref itemData);
                ShapeItem.WorldP = itemData.world; // 8/27/2009
                InstancedItem.UpdateScenaryModelTransform(ShapeItem, true, ref itemData.instancedItemData, PlayerNumber);

                // SceneItemOwner does not use FOW Culling, so set to True to always make visible.
                InstancedItem.UpdateInstanceModelFogOfWarView(ref itemData.instancedItemData, PlayerNumber);

                // Store into Arrays
                ScenaryItems[i] = itemData;
                ShapeItem.InstancedItemDatas.Add(itemData.instancedItemData);

                // 5/17/2012 - Created new method call.
                AddToScenaryItemsByNameDictionary(itemData.name, i);
            }
        }

        // 6/11/2012
        /// <summary>
        /// Sets the Scale to all batch instances for this <see cref="ItemType"/>.
        /// </summary>
        /// <param name="newScale"><see cref="Vector3"/> as new scale value.</param>
        public void SetScaleToAllInstances(Vector3 newScale)
        {
            var scenaryItemsCount = ScenaryItems.Count; // 11/7/2009
            for (var i = 0; i < scenaryItemsCount; i++)
            {
                // index value
                ShapeItem.InstancedItemPickedIndex = i;

                Scale = newScale;
            }
        }

        // 6/11/2012
        /// <summary>
        /// Sets the Rotation to all batch instances for this <see cref="ItemType"/>.
        /// </summary>
        /// <param name="rotationAxis"><see cref="RotationAxisEnum"/> to affect.</param>
        /// <param name="rotationValue">>Rotation value to use</param>
        public void SetRotationToAllInstances(RotationAxisEnum rotationAxis, float rotationValue)
        {
            var scenaryItemsCount = ScenaryItems.Count; // 11/7/2009
            for (var i = 0; i < scenaryItemsCount; i++)
            {
                // index value
                ShapeItem.InstancedItemPickedIndex = i;
                SetRotationByValue(rotationAxis, rotationValue);
            }
        }

        // 6/11/2012
        /// <summary>
        /// Sets the Height to all batch instances for this <see cref="ItemType"/>.
        /// </summary>
        /// <param name="newHeight">float as new height value.</param>
        public void SetHeightToAllInstances(float newHeight)
        {
            var scenaryItemsCount = ScenaryItems.Count; // 11/7/2009
            for (var i = 0; i < scenaryItemsCount; i++)
            {
                // index value
                ShapeItem.InstancedItemPickedIndex = i;

                Vector3 currentPosition = Position;
                currentPosition.Y = newHeight;
                Position = currentPosition;
            }
        }

        /// <summary>
        /// Helper method which adds a new entry into the ScenaryItemsNames dictionary.
        /// </summary>
        /// <param name="name">Name to add.</param>
        /// <param name="index">Index value to associate.</param>
        private void AddToScenaryItemsByNameDictionary(string name, int index)
        {
            // 10/8/2009 - Check if 'Name' given, as if so, add to Names Dictionary.
            if (string.IsNullOrEmpty(name) || name == "$E") return;

            // make sure doen't already exist in Dictionary
            if (ScenaryItemsByName.ContainsKey(name))
                throw new InvalidOperationException("ScenaryItem 'Name' given already exists!");

            // Add new record using 'Name', and array Index 'i'.
            ScenaryItemsByName.Add(name, index);
        }

        // 4/14/2009
        private void CreateWorldMatrixTransforms(ref ScenaryItemData itemData)
        {
            Matrix inMatrix1, inMatrix2, inMatrix3, inMatrix4;

            var inCenter = (-Center);
            Matrix.CreateTranslation(ref inCenter, out inMatrix1);
            Matrix.CreateScale(ref itemData.scale, out inMatrix2);
            Matrix.CreateFromQuaternion(ref itemData.rotation, out inMatrix3);
            //Vector3.Add(ref Position, ref center, out inTranslate); //inTranslate = Position + center;
            Matrix.CreateTranslation(ref itemData.position, out inMatrix4);
            
            Matrix.Multiply(ref inMatrix1, ref inMatrix2, out inMatrix1);
            Matrix.Multiply(ref inMatrix1, ref inMatrix3, out inMatrix1);
            Matrix.Multiply(ref inMatrix1, ref inMatrix4, out inMatrix1);

            itemData.world = inMatrix1;
            ShapeItem.WorldP = itemData.world; // 10/5/2009
            
        }

        // 4/14/2009
        private void LoadItemTypeAttributes(ItemType itemType)
        {
            // 8/1/2008 - Create Model using ItemType Attributes
            //            Now all ScenaryItems are created by first looking
            //            at the 'ScenaryItemTypeAtts' Attribute class, which
            //            holds all attributes for each 'ItemType' Enum.  The
            //            data is now saved as an XML File!! - Ben

            // Retrieve Attributes from Dictionary               
            if (!ScenaryItemTypeAtts.ItemTypeAtts.TryGetValue(itemType, out ShapeItem.ItemTypeAtts)) return;

            // 9/11/2008 - Only create Instance of FOWItem, if required.
            if (!ShapeItem.ItemTypeAtts.useFogOfWar) return;

            // Create Instance of FogOfWarItem
            ShapeItem._fogOfWarItem = new FogOfWarItem
                                         {
                                             FogOfWarHeight = ShapeItem.ItemTypeAtts.FogOfWarHeight,
                                             FogOfWarWidth = ShapeItem.ItemTypeAtts.FogOfWarWidth
                                         };
            ShapeItem.UseFogOfWar = ShapeItem.ItemTypeAtts.useFogOfWar;
        }

        // 12/9/2008
        /// <summary>
        /// Sets the AStarGraph costs for the current <see cref="ScenaryItemScene"/>.
        /// </summary>
        public void SetAStarCostsForCurrentItem()
        {
            // 4/26/2010 - Cache
            var scenaryItemShape = ShapeItem; 
            if (scenaryItemShape == null) return;

            // 12/9/2008 - Update AStarGraph Costs for current Scenary SceneItemOwner, if necessary.
            if (!scenaryItemShape.IsPathBlocked) return;

            const int tmpInCost = -1; 
            var tmpInSize = scenaryItemShape.PathBlockSize;
            var inTmpX = (int)Position.X; 
            var inTmpY = (int)Position.Z;

            // 1/13/2010
            if (TemporalWars3DEngine.AStarGraph != null)
                TemporalWars3DEngine.AStarGraph.SetCostToPos(inTmpX, inTmpY, tmpInCost, tmpInSize);
        }

        #endregion
       

       


        /// <summary>
        /// Updates any values associated with this <see cref="SceneItem"/> and its children
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="time"><see cref="TimeSpan"/> structure for time</param>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> structure for elapsed game sime since last call</param>
        /// <param name="isClientCall">Is this the client-side update in a network game?</param>
        public override void Update(GameTime gameTime, ref TimeSpan time, ref TimeSpan elapsedTime, bool isClientCall)
        {   

            // 3/12/2009 - Only call once, since most ScenearyItems do not animate!
            //             This optimization shaved off almost 10MS for this update call
            //             on the XBOX alone!!!
            //Calculate new positions, speeds and other base class stuff
            /*if (!baseUpdateCompleted) 
            {
                base.Update(gameTime, ref Time, ref ElapsedTime);
                baseUpdateCompleted = true;
            }*/

            // 6/11/2012
            UpdateAudioEmitters();
        }

        // 5/28/2012
        /// <summary>
        /// Helper method which renders all the Collision spheres for debug purposes.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void RenderDebug(GameTime gameTime)
        {
            // iterate batch of 'this' ItemType instance of scenaryItems
            var count = ScenaryItems.Count;
            for (int i = 0; i < count; i++)
            {
               ScenaryItemData scenaryItemData = ScenaryItems[i];
               Vector3 itemPosition = scenaryItemData.position;
                
               // 5/27/2012 - Draw Debug Collision Spheres
               var sphere = new BoundingSphere(itemPosition, CollisionRadius); // Test - new Vector3(3461, 50, 967)
               DebugShapeRenderer.AddBoundingSphere(sphere, BoundingSphereDefaultColor);
               DebugShapeRenderer.Draw(gameTime);
            }
        }

        /// <summary>
        /// When a <see cref="SceneItem"/> is placed on the <see cref="TWEngine.Terrain"/>, via the <see cref="IFDTileManager"/>, this method
        /// is called in order to do specific <see cref="SceneItem"/> placement checks; for example, if the <see cref="SceneItem"/>
        /// requires A* blocking updated.
        /// </summary>
        /// <param name="placementPosition">The <see cref="Vector3"/> position to place item at</param>
        /// <returns>true/false of result</returns>     
        public override bool RunPlacementCheck(ref Vector3 placementPosition)
        {
            // 1/13/2010 - Check if AStarGraph interface is null.
            var aStarGraph = TemporalWars3DEngine.AStarGraph; // 4/26/2010
            if (aStarGraph == null) return true;

            // 4/26/2010 - cache
            var scenaryItemShape = ShapeItem; 
            if (scenaryItemShape == null) return true;

            // 4/26/2010 - Cache
            var terrainShape = _terrainShape;
            if (terrainShape == null) return true;

            // 1/5/2009 - Check if SceneItemOwner can be placed at the current location, given the 'PathBlockSize'.
            if (!aStarGraph.IsPathNodeSectionBlocked((int)placementPosition.X, 
                (int)placementPosition.Z, scenaryItemShape.PathBlockSize, BlockedType.Any))
            {
                // 5/13/2008 - If PathBlocked, then let's update the A* GraphNodes
                if (scenaryItemShape.IsPathBlocked)
                {
                    const int tmpInCost = -1; 
                    var tmpInSize = scenaryItemShape.PathBlockSize;
                    SetCostAtCurrentPosition(tmpInCost, tmpInSize);

                    terrainShape.ScenaryItems.Add(this);
                    TerrainShape.PopulatePathNodesArray();
                }

                return true;
            }
            return false;
            
        }

        // 10/8/2009
        /// <summary>
        /// Searches the internal dictionary for the given name, and when found, sets the
        /// ShapeItem.InstancedItemPickedIndex to the given index value of the internal 
        /// List().  This redirects the calls to the property values, like 'Positions' and 'Rotation'
        /// to given instance.
        /// </summary>
        /// <param name="name">Name to search for</param>
        /// <exception cref="InvalidOperationException">Thrown when internal Dictionary <see cref="ScenaryItemsByName"/> is null.</exception>
        /// <returns>True/False of success.</returns>
        public bool SearchByName(string name)
        {
            // 4/27/2010 - Cache
            var scenaryItemsByName = ScenaryItemsByName;
            if (scenaryItemsByName == null)
                throw new InvalidOperationException("Internal Dictionary 'ScenaryItemsByName' is null, which is not allowed with the current method.");

            // 4/27/2010 - Cache
            var scenaryItemShape = ShapeItem;
            if (scenaryItemShape == null) return false;

            // Search using internal Dictionary
            int index;
            if (scenaryItemsByName.TryGetValue(name, out index))
            {
                scenaryItemShape.InstancedItemPickedIndex = index;
                return true;
            }
            return false;
        }

        // 7/2/2009
        /// <summary>
        /// Iterates the internal <see cref="ScenaryItemScene"/> collection, 
        /// checking if the items are picked, and marking using the 'IsPickedInEditMode'.
        /// </summary>
        /// <returns>True or False if picked.</returns>
        public bool CheckForPickedItemsWithInternalList()
        {
            // 4/26/2010 - Cache
            var scenaryItems = ScenaryItems;
            if (scenaryItems == null) return false;

            // make sure not null or empty
            var count = scenaryItems.Count; // 11/21/09
            if (count == 0) return false;

            // 4/26/2010 - Cache
            var scenaryItemShape = ShapeItem;
            if (scenaryItemShape == null) return false;

            // itereate the list of instanceItems
            for (var i = 0; i < count; i++)
            {
                // get picked instances
                var scenaryItemData = scenaryItems[i];

                // 10/6/2009 - Update to this instance 'World' matrix!
                scenaryItemShape.WorldP = scenaryItemData.world;

                float? distance; // 2/2/2010
                if (!InstancedItem.IsMeshPicked(ref scenaryItemData.instancedItemData, out distance)) continue;
               
                // yes, so mark as picked.
                scenaryItemData.IsPickedInEditMode = true;
                // store update back to list
                scenaryItems[i] = scenaryItemData;

                // 10/6/2009 - Update PickedIndex, since used by the Position/Rotation Get Properties!
                scenaryItemShape.InstancedItemPickedIndex = i;

                return true; // 10/6/2009 - Break out of loop, since found answer!
            }

            return false;
        }

        // 7/2/2009 
        /// <summary>
        /// Iterates the internal <see cref="ScenaryItemScene"/> list, marking the 'IsPickedInEditMode' to false.
        /// </summary>
        public void DeselectAllPickedItemsInInternalList()
        {
            // 4/27/2010
            var scenaryItems = ScenaryItems;
            if (scenaryItems == null) return;

            // make sure not null or empty
            var count = scenaryItems.Count;
            if (count == 0) return;

            // iterate the list of instanceItems
            for (var i = 0; i < count; i++)
            {
                // get struct to update
                var scenaryItemData = scenaryItems[i];
                // set to false
                scenaryItemData.IsPickedInEditMode = false;
                // store update back to list
                scenaryItems[i] = scenaryItemData;
            }

            // 10/6/2009 - Update PickedIndex back to -1.
            if (ShapeItem != null) ShapeItem.InstancedItemPickedIndex = -1;
        }

        // 7/2/2009
        /// <summary>
        /// Iterates the internal <see cref="ScenaryItemScene"/> list, only removing the instances which
        /// are picked.
        /// </summary>
        public void RemovePickedItemsFromInternalList()
        {
            // 4/27/2010 - Cache
            var scenaryItems = ScenaryItems;
            if (scenaryItems == null) return;

            // make sure not null or empty
            var count = scenaryItems.Count; // 8/18/2009
            if (count == 0) return;

            // 4/27/2010 - Cache
            var scenaryItemShape = ShapeItem;
            if (scenaryItemShape == null) return;

            // itereate the list of instanceItems
            for (var i = 0; i < count; i++)
            {
                // get picked instances
                var scenaryItemData = scenaryItems[i];
                if (!scenaryItemData.IsPickedInEditMode) continue;

                // yes, so remove Transform
                InstancedItem.RemoveInstanceTransform(ref scenaryItemData.instancedItemData);
                // set instance to be deleted from list
                scenaryItemData.deleteItem = true;
                // store update back to list
                scenaryItems[i] = scenaryItemData;

                // also remove from ShapeItem list
                var itemData = scenaryItemShape.InstancedItemDatas[i];
                itemData.DeleteItem = true;
                scenaryItemShape.InstancedItemDatas[i] = itemData;
            }

            // delete all items marked with 'delete' in both lists
            scenaryItems.RemoveAll(DeleteItems);
            scenaryItemShape.InstancedItemDatas.RemoveAll(DeleteItems);

        }

        // 3/30/2011
        /// <summary>
        /// Adds a new position instance for the given <see cref="ItemType"/>.
        /// </summary>
        /// <param name="itemType">The <see cref="itemType"/> to be used.</param>
        /// <param name="initialPosition">The initial position to place the item in the game world referenced as <see cref="Vector3"/>.</param>
        /// <param name="playerNumber">The unique <see cref="Player"/>'s network number.</param>
        /// <param name="rotation1"></param>
        public int AddScenaryItemSceneInstance(ItemType itemType, ref Vector3 initialPosition, byte playerNumber, Quaternion? rotation1)
        {
            if (TerrainData.IsOnHeightmap(position.X, position.Z))
            {
                // now that we know we're on the heightmap, we need to know the correct
                // height and normal at this Position.
                Vector3 normal;
                position.Y = TerrainData.GetTerrainHeight(position.X, position.Z);
                TerrainData.GetNormal(position.X, position.Z, out normal);
            }

            // 4/14/2009 - Init InstanceItemData
            ShapeItem.InstancedItemData = new InstancedItemData { ItemType = itemType };
            ShapeItem.InstancedItemData.ItemInstanceKey = InstancedItem.GenerateItemInstanceKey(ref ShapeItem.InstancedItemData);
            InstancedItem.AddScenaryInstancedItem(itemType, this);

            // 3/30/2011 - Check if rotation passed in for optional param
            var useRotation = (rotation1 != null) ? rotation1.Value : rotation;

            // Get Scale
            var itemData = new ScenaryItemData(ref initialPosition, ref useRotation);
            GetItemTypeScale(ShapeItem.InstancedItemData);
            itemData.instancedItemData = ShapeItem.InstancedItemData;
            itemData.scale = scale;

            // Update with proper World Matrix
            CreateWorldMatrixTransforms(ref itemData);
            ShapeItem.WorldP = itemData.world; // 8/27/2009
            InstancedItem.UpdateScenaryModelTransform(ShapeItem, true, ref itemData.instancedItemData, playerNumber);

            // SceneItemOwner does not use FOW Culling, so set to True to always make visible.
            InstancedItem.UpdateInstanceModelFogOfWarView(ref itemData.instancedItemData, playerNumber);

            // Add to ScenaryItems array
            ScenaryItems.Add(itemData);
            ShapeItem.InstancedItemDatas.Add(ShapeItem.InstancedItemData);

            // 1/11/2010 - Since called from ItemsTool, and only instance, set the internal 'InstancedItemPickedIndex' 
            //             to be zero, the first index; otherwise, if left as -1, the Position will not be updatable!
            ShapeItem.InstancedItemPickedIndex = 0;

            // 5/18/2012 - Return the current Index value from the ScenaryItems collection
            return ScenaryItems.Count - 1;
        }
       

        // 7/2/2009 - 
        /// <summary>
        /// Predicate method used to delete items in <see cref="ScenaryItemScene"/> list
        /// </summary>
        /// <param name="scenaryItemData"><see cref="ScenaryItemData"/> structure</param>
        /// <returns>true/false if marked for delete</returns>
        private static bool DeleteItems(ScenaryItemData scenaryItemData)
        {
            return scenaryItemData.deleteItem;
        }

        // 7/2/2009 - 
        /// <summary>
        /// Predicate method used to delete items in <see cref="InstancedItemData"/> shapeItem list
        /// </summary>
        /// <param name="instancedItemData"><see cref="ScenaryItemData"/> structure</param>
        /// <returns>true/false if marked for delete</returns>
        private static bool DeleteItems(InstancedItemData instancedItemData)
        {
            return instancedItemData.DeleteItem;
        }

        // 10/29/2008
        ///<summary>
        /// Remove cost value from the AStarGraph at the current position. 
        ///</summary>
        public void RemoveCostAtCurrentPosition()
        {
            var inTmpX = (int)Position.X; var inTmpY = (int)Position.Z;

            // 1/13/2010
            var aStarGraph = TemporalWars3DEngine.AStarGraph; // 4/27/2010
            if (aStarGraph != null)
                aStarGraph.RemoveCostAtPos(inTmpX, inTmpY, ShapeItem.PathBlockSize);
        }

        // 10/29/2008
        ///<summary>
        /// Sets a cost value in the AStarGraph at the given map node.      
        ///</summary>
        ///<param name="cost">Cost value to apply</param>
        ///<param name="size">Size of area to affect</param>
        public void SetCostAtCurrentPosition(int cost, int size)
        {
            // Add new Point with Cost and Size Affected to A* Node Array
            var inTmpX = (int)Position.X; var inTmpY = (int)Position.Z;

            // 1/13/2010
            var aStarGraph = TemporalWars3DEngine.AStarGraph; // 4/27/2010
            if (aStarGraph != null)
                aStarGraph.SetCostToPos(inTmpX, inTmpY, cost, size);
        }

        // 10/8/2009
        /// <summary>
        /// Checks if the <see cref="ScenaryItemScene"/> or internal <see cref="ScenaryItemData"/> instances
        /// are within view of the given Item.
        /// </summary>
        /// <typeparam name="TType">Generic type to use</typeparam>
        /// <param name="item">Generic type item to check</param>
        /// <returns>true/false ig within view</returns>
        public override bool WithinView<TType>(TType item) 
        {
            // make sure item given is not null
            if (item == null) return false;

            // 4/27/2010 - Cache
            var scenaryItemShape = ShapeItem;
            if (scenaryItemShape == null) return false;

            // If there is NO picked index, then return false.
            if (scenaryItemShape.InstancedItemPickedIndex == -1) return false;

            // 4/27/2010 - Cache
            var scenaryItems = ScenaryItems;
            if (scenaryItems == null) return false;

            // cache strut
            var scenaryItemData = scenaryItems[scenaryItemShape.InstancedItemPickedIndex];
           
            Vector3 difference;
            var itemGivenPosition = item.Position;
            Vector3.Subtract(ref scenaryItemData.position, ref itemGivenPosition, out difference);
            var length = difference.Length();

            return length < (ViewRadius + item.CollisionRadius);
        }

        // 10/12/2009
        /// <summary>
        /// Flashes the given <see cref="ScenaryItemScene"/> 'White' for the 
        /// specified amount of time given in seconds.
        /// </summary>
        /// <param name="sceneItemName">Name of <see cref="ScenaryItemScene"/></param>
        /// <param name="timeInSeconds">Time in seconds to flash white</param>
        public void FlashItemWhite(string sceneItemName, int timeInSeconds)
        {
            // Set to proper index
            SearchByName(sceneItemName);

            // now call base
            FlashItemWhite(timeInSeconds);
        }

        // 5/28/2012
        /// <summary>
        /// Checks if there is a collision between the this and the passed in <see cref="SceneItem"/>
        /// </summary>
        /// <param name="item">A scene <see cref="SceneItem"/> to check</param>
        /// <returns>True if there is a collision</returns>
        public override bool Collide(SceneItem item)
        {
            var scenaryItem = item as ScenaryItemScene;
            if (scenaryItem == null)
            {
                return false;
            }

            //Until we get collision meshes sorted just do a simple sphere (well circle!) check
            return (Position - item.Position).Length() < CollisionRadius + item.CollisionRadius;
        }

        // 5/28/2012
        /// <summary>
        /// Checks if the given <see cref="Vector3"/> position, is within the <see cref="SceneItem.CollisionRadius"/> of this <see cref="SceneItem"/>.
        /// </summary>
        /// <param name="checkPosition"><see cref="Vector3"/> position to check</param>
        /// <returns>true/false if within collision radius</returns>
        public override bool WithinCollision(ref Vector3 checkPosition)
        {
            //Until we get collision meshes sorted just do a simple sphere (well circle!) check
            return (Position - checkPosition).Length() < CollisionRadius;
        }

        // 6/6/2012
        /// <summary>
        /// Helper method, which checks if the current <see cref="SceneItem"/> position is within
        /// the 'MoveToPosition', by the current 'WaypointSeekDistSq' value.
        /// </summary>
        /// <returns>True/False</returns>
        internal override bool HasReachedMoveToPosition(Vector3 moveToPosition)
        {
            // Sub-goal reached? 
            var tmpPosA = new Vector2 { X = Position.X, Y = Position.Z };
            var tmpPosB = new Vector2 { X = moveToPosition.X, Y = moveToPosition.Z };

            float result;
            Vector2.DistanceSquared(ref tmpPosB, ref tmpPosA, out result);
            return (result < SeekDistSq);
        }

        // 6/6/2012
        /// <summary>
        /// Helper method, which checks if the current <see cref="SceneItem"/> position is within
        /// the 'MoveToPosition', by the current 'WaypointSeekDistSq' value.
        /// </summary>
        /// <returns>True/False</returns>
        public override bool HasReachedMoveToPosition(ref Vector3 moveToPosition)
        {
            // Sub-goal reached? 
            var tmpPosA = new Vector2 { X = Position.X, Y = Position.Z };
            var tmpPosB = new Vector2 { X = moveToPosition.X, Y = moveToPosition.Z };

            float result;
            Vector2.DistanceSquared(ref tmpPosB, ref tmpPosA, out result);
            return (result < SeekDistSq);
        }

        // 6/10/2012
        /// <summary>
        /// Queues a new <see cref="Sounds"/> to be played for the current <see cref="ScenaryItemScene"/>.
        /// </summary>
        /// <param name="soundToPlay"><see cref="Sounds"/> to play</param>
        public void Play3DAudio(Sounds soundToPlay)
        {
            if (ShapeItem == null)
                throw new InvalidOperationException("The 'ShapeItem' is Null!");

            if (ShapeItem.InstancedItemPickedIndex == -1)
                throw new InvalidOperationException("InstancedItemPickedIndex must be set before calling this method.");

            try
            {
                // Retrieve ItemData struct.
                ScenaryItemData itemData;
                if (!GetScenaryItemData(out itemData)) return;

                // check if audio structure already exist.
                if (itemData.AttachedAudioStructIndex == -1)
                {
                    // no, so create ScenaryItemDataAudio structure
                    var scenaryItemDataAudio = new ScenaryItemDataAudio(ShapeItem.InstancedItemPickedIndex);
                    // add to Audio collection
                    _scenaryItemsWithAudio.Add(scenaryItemDataAudio);
                    // set index into ItemData struct
                    itemData.AttachedAudioStructIndex = _scenaryItemsWithAudio.Count - 1;
                    // Queue sound to play
                    scenaryItemDataAudio.AddSoundToPlay(soundToPlay);
                    // Set ItemData struct
                    SetScenaryItemData(itemData);

                    return;
                }

                // else, get ScenaryItemDataAudio structure and queue new request
                _scenaryItemsWithAudio[itemData.AttachedAudioStructIndex].AddSoundToPlay(soundToPlay);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("Play3DAudio method in ScenaryItemScene class threw the exception " + ex.Message ?? "No Message");
#endif
            }

        }

        // 6/10/2012
        /// <summary>
        /// Helper method to initalize the internal audio emitter and listner variables.
        /// </summary>
        protected override void UpdateAudioEmitters()
        {
            try
            {
                var count = _scenaryItemsWithAudio.Count;
                for (var i = 0; i < count; i++)
                {
                    // cache audio structure.
                    var scenaryItemDataAudio = _scenaryItemsWithAudio[i];
                    ShapeItem.InstancedItemPickedIndex = scenaryItemDataAudio.InstancedItemPickedIndex;

                    var scenaryItemPosition = Position;

                    // Verify NaN is not present.
                    if (float.IsNaN(scenaryItemPosition.X))
                        scenaryItemPosition.X = 0;

                    if (float.IsNaN(scenaryItemPosition.Y))
                        scenaryItemPosition.Y = 0;

                    if (float.IsNaN(scenaryItemPosition.Z))
                        scenaryItemPosition.Z = 0;

                    Position = scenaryItemPosition;

                    // skip null items
                    if (scenaryItemDataAudio.AudioEmitterI == null || scenaryItemDataAudio.AudioListenerI == null)
                        continue;

                    // Init 3D Position
                    scenaryItemDataAudio.AudioEmitterI.Position = scenaryItemPosition;
                    scenaryItemDataAudio.AudioEmitterI.Up = Vector3.Up;
                    scenaryItemDataAudio.AudioEmitterI.Forward = Vector3.Forward;
                    scenaryItemDataAudio.AudioListenerI.Position = Camera.CameraPosition;

                    // Check for Audio Play3D requests
                    scenaryItemDataAudio.CheckToPlaySound(UniqueKey);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("UpdateAudioEmitters method in ScenaryItemScene class threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/11/2008; 1/8/2009: Updated to get Scale from Content pipeline.
        /// <summary>
        /// Gets the 'Scale' for given <see cref="ItemType"/>.
        /// </summary>    
        /// <param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>   
        private void GetItemTypeScale(InstancedItemData instancedItemData)
        {
            // Default Scale
            scale = Vector3.One;

            // 6/8/2012 - Check for scale in ItemAtts
            LoadItemTypeAttributes(instancedItemData.ItemType);
            if (Math.Abs(ShapeItem.ItemTypeAtts.Scale - 0) > float.Epsilon)
            {
                scale.Y = scale.Z = scale.X = ShapeItem.ItemTypeAtts.Scale;
                return;
            }

            // 1/8/2009 - Get Scale directly from Content pipeline file!
            InstancedItem.GetScale(ref instancedItemData, out scale.X);
            scale.Y = scale.Z = scale.X;
        }

        // 8/15/2008
        ///<summary>
        /// Disposes of unmanaged resources.
        ///</summary>
        ///<param name="finalDispose">Is this final dispose?</param>
        public override void Dispose(bool finalDispose)
        {
            base.Dispose(finalDispose);

            if (!finalDispose) return;

            // 1/8/2010 - Clear Arrays
            if (ScenaryItems != null)
                ScenaryItems.Clear();
            if (ScenaryItemsByName != null)
                ScenaryItemsByName.Clear();

            // Null _terrainShape
            ScenaryItems = null;
            ScenaryItemsByName = null;
            _terrainShape = null;
        }
        
    }
}
