#region File Description
//-----------------------------------------------------------------------------
// ScenaryItemShape.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TWEngine.GameCamera;
using TWEngine.InstancedModels.Enums;
using TWEngine.InstancedModels.Structs;
using TWEngine.Interfaces;
using TWEngine.ItemTypeAttributes.Structs;
using TWEngine.Players;
using TWEngine.SceneItems;
using TWEngine.Shadows;
using TWEngine.InstancedModels;
using TWEngine.Shapes.Enums;
using TWEngine.Terrain.Enums;
using TWEngine.TerrainTools;

namespace TWEngine.Shapes
{
    ///<summary>
    /// The <see cref="ScenaryItemShape"/> class holds the actual artwork reference, to
    /// either an XNA <see cref="Model"/> or the <see cref="InstancedItem"/> model.  This
    /// particular class is special, in that was designed to hold specifically scenary type
    /// artwork, like bushes and trees, or misc artwork, like bricks, wheels, fences, etc.
    ///</summary>
    public sealed class ScenaryItemShape : ShapeWithPick, IShadowShapeItem
    {
        /// <summary>
        /// <see cref="Game"/> instance
        /// </summary>
        private Game _gameInstance;

        // 8/4/2008 - ShadowMap Interface; used to get LightPos
        private static IShadowMap _shadowMap;
        private ShadowItem _shadowItem; // 7/8/2008 - Add ShadowItem Class

        // 1/29/2009
        ///<summary>
        /// <see cref="InstancedItemData"/> structure.
        ///</summary>
        internal new InstancedItemData InstancedItemData;
        /// <summary>
        /// Holds a collection of <see cref="InstancedItemData"/>, which is used to batch entire <see cref="ItemType"/> at once.
        /// </summary>
        /// <remarks>This concept only applies to the <see cref="ScenaryItemShape"/> class.</remarks>
        internal readonly List<InstancedItemData> InstancedItemDatas = new List<InstancedItemData>(); // 4/14/2009
        /// <summary>
        /// Holds the current picked index value into the collection <see cref="InstancedItemDatas"/>.
        /// </summary>
        internal int InstancedItemPickedIndex = -1; // 4/14/2009

        // 11/19/2008 - 
        /// <summary>
        /// The team this <see cref="Shape"/> belongs to (MP)
        /// </summary>
        private readonly int _playerNumber;

        /// <summary>
        /// The <see cref="ScenaryItemTypeAttributes"/> structure
        /// </summary>
        internal ScenaryItemTypeAttributes ItemTypeAtts;

        #region Properties


        /// <summary>
        /// Overrides the base World <see cref="Matrix"/>, allowing PlayerNumber to be passed into the method
        /// <see cref="InstancedItem.UpdatePlayableModelTransform"/>.
        /// </summary>
        public override Matrix WorldP
        {
            get { return base.WorldP; }
            set
            {
                //var oldWorld = base.WorldP;
                base.WorldP = value;

                // Update if 'InstanceModel' & when World Matrix value changes.
                //if (ItemTypeAtts.modelType == ModelType.InstanceModel)// && oldWorld != value)
                {
                    var isInEditMode = (TerrainEditRoutines.ToolInUse == ToolType.ItemTool ||
                                        TerrainEditRoutines.ToolInUse == ToolType.PropertiesTool);

                    InstancedItem.UpdateScenaryModelTransform(this, isInEditMode, ref InstancedItemData, _playerNumber);
                }

            }
        }

        /// <summary>
        /// Gets or sets the animation rotation amount.
        /// </summary>
        public float AnimatedRotation { get; set; }

        ///<summary>
        /// Overrides <see cref="Shape.IsFOWVisible"/> property, allowing PlayerNumber to be passed into the method
        /// <see cref="InstancedItem.UpdateInstanceModelFogOfWarView"/>.
        ///</summary>
        public new bool IsFOWVisible
        {
            get { return _fogOfWarItem.IsFOWVisible; }
            set
            {
                _fogOfWarItem.IsFOWVisible = value;

                // 1/14/2009 - also need to store into InstanceItem array.
                InstancedItem.UpdateInstanceModelFogOfWarView(ref InstancedItemData, _playerNumber);
            }
        }

        #region IShadowItem Interface Properties

        // Sets the Inherited Interface Property Model
        ///<summary>
        /// Set or get reference to XNA <see cref="IShadowShapeItem.Model"/>.
        ///</summary>
        public Model Model
        {
            get { return _shadowItem.Model; }
            set { _shadowItem.Model = value; }
        }

        ///<summary>
        /// Item in <see cref="Camera"/> frustrum?
        ///</summary>
        public bool InCameraFrustrum
        {
            get { return _shadowItem.InCameraFrustrum; }
            set { _shadowItem.InCameraFrustrum = value; }
        }

        ///<summary>
        /// Item cast shadow?
        ///</summary>
        public bool ModelCastShadow
        {
            get { return _shadowItem.ModelCastShadow; }
            set { _shadowItem.ModelCastShadow = value; }
        }

        /// <summary>
        /// Gets or Sets the Inherted ModelAnimates from the Interface.
        /// </summary>
        public bool ModelAnimates
        {
            get { return _shadowItem.ModelAnimates; }
            set { _shadowItem.ModelAnimates = value; }
        }

        #endregion

        #region IInstancedItem Interface Properties

        /// <summary>
        /// The <see cref="Shape.ItemGroupType"/> Enum this item belongs to.
        /// </summary>
        public override ItemGroupType ItemGroupType
        {
            get
            {
                // 11/21/2009 - Check if Array null or empty 1st.
                if (InstancedItemDatas == null) return ItemGroupType.People;

                return InstancedItemDatas.Count == 0 ? ItemGroupType.People : InstancedItemDatas[0].ItemGroupType;
            }
        }


        ///<summary>
        /// Item picked in edit mode?
        ///</summary>
        public override bool IsPickedInEditMode { get; set; }

        ///<summary>
        /// <see cref="ModelType"/> Enum.
        ///</summary>
        public override ModelType IsModelType
        {
            get { return ItemTypeAtts.modelType; }
            set { ItemTypeAtts.modelType = value; }
        }

        ///<summary>
        /// Does item contribute to path blocking for A*.
        ///</summary>
        public override bool IsPathBlocked
        {
            get { return ItemTypeAtts.usePathBlocking; }
            set { ItemTypeAtts.usePathBlocking = value; }
        }

        ///<summary>
        /// Path block size area to affect?
        ///</summary>
        /// <remarks>Requires the <see cref="Shape.IsPathBlocked"/> to be TRUE.</remarks>
        public override int PathBlockSize
        {
            get { return ItemTypeAtts.pathBlockValue; }
            set { ItemTypeAtts.pathBlockValue = value; }
        }

        /// <summary>
        /// The <see cref="InstancedItem"/> unique instance item key,
        /// stored in the <see cref="Shape.InstancedItemData"/> structure.
        /// </summary>
        public override int ItemInstanceKey
        {
            get
            {
                // 11/21/2009 - Check if Array null or empty 1st.
                if (InstancedItemDatas == null) return -1;
                if (InstancedItemDatas.Count == 0) return -1;

                // 10/6/2009 - Check if 'PickedIndex' set, which implies to return the key from the 'InstancedItemDatas' array.
                return InstancedItemPickedIndex == -1 ? InstancedItemDatas[0].ItemInstanceKey :  InstancedItemDatas[InstancedItemPickedIndex].ItemInstanceKey;
            }
        }

        // 6/18/2010
        /// <summary>
        /// The <see cref="Shape.ItemType"/> Enum to use
        /// </summary>
        public override ItemType ItemType
        {
            get
            {
                // 11/21/2009 - Check if Array null or empty 1st.
                if (InstancedItemDatas == null) return base.ItemType;
                if (InstancedItemDatas.Count == 0) return base.ItemType;

                // 10/6/2009 - Check if 'PickedIndex' set, which implies to return the key from the 'InstancedItemDatas' array.
                return InstancedItemPickedIndex == -1 ? InstancedItemDatas[0].ItemType : InstancedItemDatas[InstancedItemPickedIndex].ItemType;
            }
        }

        #endregion
        
        #endregion

        ///<summary>
        /// Constructor, which saves the given params, and calls 
        /// the internal <see cref="InitializeModelTypes"/> method.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        ///<param name="playerNumber"><see cref="Player"/> number</param>
        public ScenaryItemShape(Game game, int playerNumber)
            : base(game)
        {
            // 8/1/2008
            _gameInstance = game;
       
            // 11/19/2008
            _playerNumber = playerNumber;

            // 8/1/2008
            InitializeModelTypes();

        }        

        // 8/1/2008
        /// <summary>
        /// Checks <see cref="ItemType"/> Enum to initialize the proper model type.
        /// </summary>
        private void InitializeModelTypes()
        {
            // 8/4/2008 - Get ShadowMap Interface
            if (_shadowMap == null)
                _shadowMap = (IShadowMap)_gameInstance.Services.GetService(typeof(IShadowMap));

            // Create ShadowItem
            CreateShadowItem(ItemTypeAtts.useShadowCasting);
           

            // 8/1/2008 - Create Model using ItemType Attributes
            //            Now all ScenaryItems are created by first looking
            //            at the 'ScenaryItemTypeAtts' Attribute class, which
            //            holds all attributes for each 'ItemType' Enum.  The
            //            data is now saved as an XML File!! - Ben

            // Retrieve Attributes from Dictionary               
            /*if (ScenaryItemTypeAtts.ItemTypeAtts.TryGetValue(InstancedItemData.ItemType, out ItemTypeAtts))
            {
                // 9/11/2008 - Only create Instance of FOWItem, if required.
                if (ItemTypeAtts.useFogOfWar)
                {
                    // Create Instance of FogOfWarItem
                    FogOfWarItem = new FogOfWarItem();
                    FogOfWarItem.FogOfWarHeight = ItemTypeAtts.FogOfWarHeight;
                    FogOfWarItem.FogOfWarWidth = ItemTypeAtts.FogOfWarWidth;
                    useFogOfWar = ItemTypeAtts.useFogOfWar;

                }

                // Create Model, using the Attributes info gathered for specific ItemType Enum.
                CreateModelType(IsModelType, ItemTypeAtts.modelLoadPathName, ItemTypeAtts.useShadowCasting);

                if (ItemTypeAtts.modelAnimates && ItemTypeAtts.modelType == ModelType.XNAModel)
                {
                    animatedBone = model.Bones[ItemTypeAtts.modelAnimateBoneName];
                    animatedTransform = animatedBone.Transform;
                    BoneTransforms = new Matrix[model.Bones.Count];
                }
            }*/                   
           

        }
        

        #region CreateModelType Methods
                 
       

        // 8/1/2008
        /// <summary>
        /// Creates the <see cref="ShadowItem"/> structure
        /// </summary>
        private void CreateShadowItem(bool castShadow)
        {            
            _shadowItem = new ShadowItem(Game, ItemTypeAtts.modelAnimates) {BoneTransforms = new Matrix[1]};

            ModelCastShadow = castShadow;
        }

        #endregion

        /// <summary>
        /// Overrides base render and left empty, since <see cref="InstancedModels"/> are
        /// rendered in batches.
        /// </summary>
        public override void Render()
        {      
            // Empty; drawing is done in InstnaceItem class.
            return;
        }

        // 9/23/2008 - 
        ///<summary>
        /// Calls the <see cref="InstancedItem.RemoveInstanceTransform"/> method.
        ///</summary>
        ///<param name="itemInstancedKey">Instanced item's unique key</param>
        public void RemoveInstanceTransform(int itemInstancedKey)
        {
            InstancedItemData.ItemInstanceKey = itemInstancedKey; // 4/14/2009

            InstancedItem.RemoveInstanceTransform(ref InstancedItemData);
        }  

        #region IShadowItem Methods
        
        // 9/12/2008
        ///<summary>
        /// Draws the <see cref="IShadowShapeItem"/> using the <see cref="ShadowMap"/> shader, 
        /// which will project the shadow for this <see cref="SceneItem"/> onto the <see cref="ShadowMap"/>.        
        ///</summary>
        ///<param name="lightView">Light view <see cref="Matrix"/></param>
        ///<param name="lightProjection">Light projection <see cref="Matrix"/></param>
        public void DrawForShadowMap(ref Matrix lightView, ref Matrix lightProjection)
        {
            if (ItemTypeAtts.modelType == ModelType.InstanceModel)
            {
                InstancedItem.DrawForShadowMap(ref InstancedItemData, ref lightView, ref lightProjection);
            }
            else
            {
                // Set ShadowItem Attributes
                _shadowItem.WorldP = WorldP;

                // Call ShadowItem DrawForShadowMap method
                _shadowItem.DrawForShadowMap(ref lightView, ref lightProjection);
            }

        }

        // 7/8/2008 -
        ///<summary>
        /// Call the <see cref="ShadowItem"/> method <see cref="IShadowShapeItem.StoreModelEffect"/>.
        ///</summary>
        ///<param name="model">XNA <see cref="IShadowShapeItem.Model"/> instance</param>
        ///<param name="isBasicEffect">Is <see cref="BasicEffect"/>?</param>
        public void StoreModelEffect(ref Model model, bool isBasicEffect)
        {
            _shadowItem.StoreModelEffect(ref model, isBasicEffect);
        }

        #endregion


        /// <summary>
        /// Overrides base Shape.IsMeshPicked method, to now check for <see cref="InstancedModel"/> picks. 
        /// However, with the <see cref="ScenaryItemShape"/> class, there is a collection of <see cref="InstancedItemData"/> items
        /// to iterate through, since this class batches them together for the same <see cref="ItemType"/>.
        /// </summary>
        /// <param name="intersectionDistance">(OUT) intersection distance</param>
        /// <returns>True/False of result.</returns>      
        public override bool IsMeshPicked(out float? intersectionDistance)
        {
            // 2/2/2010
            intersectionDistance = null;

            // If not InstanceModel, then call base 'IsMeshPicked' Method.
            if (ItemTypeAtts.modelType != ModelType.InstanceModel)
                return base.IsMeshPicked(out intersectionDistance);

            // 4/14/2009 - iterate through all instances of current ItemType
            var count = InstancedItemDatas.Count; // 8/18/2009
            for (var i = 0; i < count; i++)
            {
                var itemData = InstancedItemDatas[i];
                var result = InstancedItem.IsMeshPicked(ref itemData, out intersectionDistance);

                if (!result) continue;

                InstancedItemPickedIndex = i;
                InstancedItemData = itemData;
                return true;
            }
            return false;
        }     
        
      
        #region Dispose

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public override void Dispose()
        {
            Dispose(true);           
            GC.SuppressFinalize(this);
            base.Dispose();
        }
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this final dispose?</param>
        private void Dispose(bool disposing)
        {
            if (!disposing) return;

            // Dispose of Resources 
            _shadowItem.Dispose();

            // null refs
            _gameInstance = null;
            _shadowMap = null;
            // free native resources
        }      

        #endregion
        
    }
}
