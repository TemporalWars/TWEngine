#region File Description
//-----------------------------------------------------------------------------
// DefenseShape.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TWEngine.Common;
using TWEngine.Explosions;
using TWEngine.Explosions.Structs;
using TWEngine.GameCamera;
using TWEngine.InstancedModels;
using TWEngine.InstancedModels.Enums;
using TWEngine.InstancedModels.Structs;
using TWEngine.Interfaces;
using TWEngine.ItemTypeAttributes;
using TWEngine.ItemTypeAttributes.Structs;
using TWEngine.Players;
using TWEngine.SceneItems;
using TWEngine.Shadows;
using TWEngine.Shapes.Enums;

namespace TWEngine.Shapes
{
    // 12/25/2008
    ///<summary>
    /// The <see cref="DefenseShape"/> class holds the actual artwork reference, to
    /// either an XNA <see cref="Model"/> or the <see cref="InstancedItem"/> model.  It
    /// provides the ability to 'Pick' the item, retrieve the item's World <see cref="Matrix"/>, 
    /// set the <see cref="ItemType"/>, and start an explosion, to name few.
    ///</summary>
    public sealed class DefenseShape : ShapeWithPick, IShadowShapeItem
    {
        // Add ShadowItem Class
        private ShadowItem _shadowItem;

        // 2/26/2009
        /// <summary>
        /// Set when explosion animation started.
        /// </summary>
        internal bool ExplodeAnimStarted;

        /// <summary>
        /// The <see cref="ScenaryItemTypeAttributes"/> structure
        /// </summary>
        internal ScenaryItemTypeAttributes ItemTypeAtts;

        #region Properties

        ///<summary>
        /// Set or Get the <see cref="InstancedItemData"/> structure.
        ///</summary>
        public new InstancedItemData InstancedItemData
        {
            get { return ((Shape) this).InstancedItemData; }
            set { ((Shape) this).InstancedItemData = value; }
        }

        // 12/31/2009 -  
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
                InstancedItem.UpdateInstanceModelFogOfWarView(ref ((Shape)this).InstancedItemData, PlayerNumber);
            }
        }

        #region IInstancedItem Interface Properties

        ///<summary>
        /// Item picked in edit mode?
        ///</summary>
        public override bool IsPickedInEditMode { get; set; }

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

        ///<summary>
        /// <see cref="ModelType"/> Enum.
        ///</summary>
        public override ModelType IsModelType
        {
            get { return ItemTypeAtts.modelType; }
            set { ItemTypeAtts.modelType = value; }
        }

        #endregion

        #region IShadowItem Interface Properties
       
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

        #region IShadowShapeItem Members

        /// <summary>
        /// Overrides the base World <see cref="Matrix"/>, allowing PlayerNumber to be passed into the method
        /// <see cref="InstancedItem.UpdatePlayableModelTransform"/>.
        /// </summary>
        public override Matrix WorldP
        {
            get { return base.WorldP; }
            set
            {
                base.WorldP = value;

                InstancedItem.UpdatePlayableModelTransform(this, ref ((Shape)this).InstancedItemData, PlayerNumber);

            }
        }

        #endregion

       
        #endregion

        ///<summary>
        /// Constructor, which sets the given <see cref="ItemType"/> and
        /// creates a <see cref="FogOfWarItem"/>, if necessary.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        ///<param name="itemType"><see cref="ItemType"/> instance</param>
        ///<param name="playerNumber"><see cref="Player"/> number</param>
        public DefenseShape(Game game, ItemType itemType, byte playerNumber)
            : base(game)
        {
            ((Shape) this).InstancedItemData.ItemType = itemType;
            ((Shape) this).InstancedItemData.ItemInstanceKey = InstancedItem.GenerateItemInstanceKey(ref ((Shape) this).InstancedItemData);

            PlayerNumber = playerNumber;

            // Retrieve Attributes from Dictionary               
            if (!ScenaryItemTypeAtts.ItemTypeAtts.TryGetValue(itemType, out ItemTypeAtts)) return;

            // Only create Instance of FOWItem, if required.
            if (ItemTypeAtts.useFogOfWar)
            {
                // Create Instance of FogOfWarItem
                _fogOfWarItem = new FogOfWarItem
                                    {
                                        FogOfWarHeight = ItemTypeAtts.FogOfWarHeight,
                                        FogOfWarWidth = ItemTypeAtts.FogOfWarWidth
                                    };
                UseFogOfWar = ItemTypeAtts.useFogOfWar;
            }

            // Create ShadowItem
            CreateShadowItem(ItemTypeAtts.useShadowCasting);
            
        }

        // 2/26/2009
        /// <summary>
        /// Sets what <see cref="ItemType"/> to use for this <see cref="DefenseShape"/> instance; for
        /// example, 'SciFi_AAGun01' or 'SciFi_AAGun02' ItemType.
        /// </summary>
        /// <param name="itemType"><see cref="ItemType"/> Enum to use</param>
        public override void SetInstancedItemTypeToUse(ItemType itemType)
        {
            ((Shape) this).InstancedItemData.ItemGroupType = ItemGroupType.Shields;

            // call base version to fill ItemTypeAtts Struct
            SetInstancedItemTypeToUse(itemType, out ItemTypeAtts);


            // Only create Instance of FOWItem, if required.
            if (ItemTypeAtts.useFogOfWar)
            {
                // Create Instance of FogOfWarItem
                //if (_fogOfWarItem == null)
                _fogOfWarItem = new FogOfWarItem
                                    {
                                        FogOfWarHeight = ItemTypeAtts.FogOfWarHeight,
                                        FogOfWarWidth = ItemTypeAtts.FogOfWarWidth
                                    };

                UseFogOfWar = ItemTypeAtts.useFogOfWar;
            }

            // Create ShadowItem
            CreateShadowItem(ItemTypeAtts.useShadowCasting);

            // 8/27/2009 - Connect this ShapeItem Ref, to the InstanceItem.
            InstancedItem.UpdatePlayableModelTransform(this, ref ((Shape)this).InstancedItemData, PlayerNumber);
        }


        /// <summary>
        /// Creates the vertex buffers etc. This routine is called on object creation and on Device reset etc
        /// </summary>
        public override void Create()
        {
            // Left Empty
            return;
        }

             
        /// <summary>
        /// Overrides base Shape.IsMeshPicked method, to now check for <see cref="InstancedModel"/> picks.  
        /// </summary>
        /// <param name="intersectionDistance">(OUT) intersection distance</param>
        public override bool IsMeshPicked(out float? intersectionDistance)
        {
            return InstancedItem.IsMeshPicked(ref ((Shape)this).InstancedItemData, out intersectionDistance);
        }

        /// <summary>
        /// Creates the <see cref="ShadowItem"/> structure
        /// </summary>
        /// <param name="castShadow">Cast shadow?</param>
        private void CreateShadowItem(bool castShadow)
        {
            // ShadowItem            
            _shadowItem = new ShadowItem(Game, ItemTypeAtts.modelAnimates) {BoneTransforms = new Matrix[1]};


            // 1/16/2009 - If Building Animates, then set shadow to always update
            if (ItemTypeAtts.modelAnimates)
                InstancedItem.SetAlwaysDrawShadow(ref ((Shape) this).InstancedItemData, true);

            ModelCastShadow = castShadow;
        }

        /// <summary>
        /// Overrides base render and left empty, since <see cref="InstancedModels"/> are
        /// rendered in batches.
        /// </summary>
        public override void Render()
        {
            // Empty
            return;
        }

        // 2/26/2009 - ExplodeItem; 1/30/2010 - Updated with 'SceneItemOwner'.     
        /// <summary>
        /// When a <see cref="SceneItem"/> is killed, the base class <see cref="Shape"/> will automatically call
        /// this method.  This overriding method will provide an Exploding animation
        /// depending on the <see cref="ItemType"/>.  The <see cref="SceneItem"/> owner MUST be set to have at least one 
        /// <see cref="ExplosionItem"/> part being; this is so the <see cref="ExplosionsManager"/> can call the <see cref="SceneItemWithPick.FinishKillSceneItem"/>
        /// metohd to complete the death process.
        /// </summary>
        /// <param name="sceneItemOwner"><see cref="SceneItemWithPick"/> owner of this shape</param>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> structure as elapsed time</param>
        public override void StartExplosion(SceneItemWithPick sceneItemOwner, ref TimeSpan elapsedTime)
        {
            // 1/30/2010
            // NOTE: MUST set at least one ExplosionItem to have the 'SceneItemOwner' reference below; but ONLY one, otherwise multiply calls will occur.

            if (ExplodeAnimStarted) return;

            // 5/20/2010 - Const amount of velocity applied to Y access; 0-100%.
            // 1/18/2011 - Velocity Y Height has default 30x in shader.
            const float velocityYPower = 4 * 30 * 0.50f; // Height // Height
            const float velocityXPower = 0.10f; // 1/17/2011
            const float velocityZPower = 0.10f; // 1/17/2011

            var tmpOrientation = Orientation;
            var explosionsManager = TemporalWars3DEngine.ExplosionManager; // 4/30/2010 - Cache
            {
                // SceneItemOwner Turret
                // Set to Projectile Velocity
                var itemPiece0Velocity = new Vector3
                                             {
                                                 X = LastProjectileVelocity.X * 4 * velocityXPower,
                                                 Z = LastProjectileVelocity.Z * 4 * velocityZPower,
                                                 Y = velocityYPower
                                             };

                // 1/18/2011 - Add new Explosion Velocity
                InstancedItem.AddBoneExplosionVelocity(ref ((Shape)this).InstancedItemData, "turret", ref itemPiece0Velocity);
                
                // Create new ExplosionItem instance for each piece needed.                   
                var explosionItemPiece = new ExplosionItem(sceneItemOwner, "turret", ref itemPiece0Velocity, ref tmpOrientation,
                                                           ref World, ref ((Shape) this).InstancedItemData);
                // Add to ExplosionManager
                explosionsManager.AddNewExplosionItem(ref explosionItemPiece);
            }

            {
                // SceneItemOwner Turret-Barrels
                // Set to Projectile Velocity
                var itemPiece1Velocity = new Vector3
                                             {
                                                 X = LastProjectileVelocity.X * -4 * velocityXPower,
                                                 Z = LastProjectileVelocity.Z * -4 * velocityZPower,
                                                 Y = velocityYPower
                                             };

                // 1/18/2011 - Add new Explosion Velocity
                InstancedItem.AddBoneExplosionVelocity(ref ((Shape)this).InstancedItemData, "turret-barrels", ref itemPiece1Velocity);

                // Create new ExplosionItem instance for each piece needed.                   
                var explosionItemPiece = new ExplosionItem(null, "turret-barrels", ref itemPiece1Velocity,
                                                           ref tmpOrientation, ref World, ref ((Shape) this).InstancedItemData);
                // Add to ExplosionManager
                explosionsManager.AddNewExplosionItem(ref explosionItemPiece);
            }

            {
                // SceneItemOwner Piece 1
                // Set to Projectile Velocity
                var itemPiece2Velocity = new Vector3
                                             {
                                                 X = LastProjectileVelocity.X * 3 * velocityXPower,
                                                 Z = LastProjectileVelocity.Z * 2 * velocityZPower,
                                                 Y = velocityYPower
                                             };

                // 1/18/2011 - Add new Explosion Velocity
                InstancedItem.AddBoneExplosionVelocity(ref ((Shape)this).InstancedItemData, "Piece1", ref itemPiece2Velocity);

                // Create new ExplosionItem instance for each piece needed.                   
                var explosionItemPiece = new ExplosionItem(null, "Piece1", ref itemPiece2Velocity, ref tmpOrientation,
                                                           ref World, ref ((Shape) this).InstancedItemData);
                // Add to ExplosionManager
                explosionsManager.AddNewExplosionItem(ref explosionItemPiece);
            }

            {
                // SceneItemOwner Piece 2
                // Set to Projectile Velocity
                var itemPiece3Velocity = new Vector3
                                             {
                                                 X = LastProjectileVelocity.X * -3 * velocityXPower,
                                                 Z = LastProjectileVelocity.Z * 5 * velocityZPower,
                                                 Y = velocityYPower
                                             };

                // 1/18/2011 - Add new Explosion Velocity
                InstancedItem.AddBoneExplosionVelocity(ref ((Shape)this).InstancedItemData, "Piece2", ref itemPiece3Velocity);

                // Create new ExplosionItem instance for each piece needed.                  
                var explosionItemPiece = new ExplosionItem(null, "Piece2", ref itemPiece3Velocity, ref tmpOrientation,
                                                           ref World, ref ((Shape) this).InstancedItemData);
                // Add to ExplosionManager
                explosionsManager.AddNewExplosionItem(ref explosionItemPiece);
            }

            {
                // SceneItemOwner Piece 3
                // Set to Projectile Velocity
                var itemPiece4Velocity = new Vector3
                                             {
                                                 X = LastProjectileVelocity.X * 3 * velocityXPower,
                                                 Z = LastProjectileVelocity.Z * -2 * velocityZPower,
                                                 Y = velocityYPower
                                             };

                // 1/18/2011 - Add new Explosion Velocity
                InstancedItem.AddBoneExplosionVelocity(ref ((Shape)this).InstancedItemData, "Piece3", ref itemPiece4Velocity);

                // Create new ExplosionItem instance for each piece needed.                   
                var explosionItemPiece = new ExplosionItem(null, "Piece3", ref itemPiece4Velocity, ref tmpOrientation,
                                                           ref World, ref ((Shape) this).InstancedItemData);
                // Add to ExplosionManager
                explosionsManager.AddNewExplosionItem(ref explosionItemPiece);
            }

            // 4/7/2009
            var currentPosition = World.Translation;
            var lastProjectileVelocity = LastProjectileVelocity;
            ExplosionsManager.DoParticles_MediumExplosion(ref currentPosition, ref lastProjectileVelocity);


            ExplodeAnimStarted = true;
        }

        #region IShadowItem Methods

        // Draws the SceneItemOwner using the ShadowMap Shader, which will project the shadow for this
        // SceneItemOwner onto the ShadowMap.        
        ///<summary>
        /// Draws the <see cref="IShadowShapeItem"/> using the <see cref="ShadowMap"/> shader, 
        /// which will project the shadow for this <see cref="SceneItem"/> onto the <see cref="ShadowMap"/>.        
        ///</summary>
        ///<param name="lightView">Light view <see cref="Matrix"/></param>
        ///<param name="lightProjection">Light projection <see cref="Matrix"/></param>
        public void DrawForShadowMap(ref Matrix lightView, ref Matrix lightProjection)
        {
            InstancedItem.DrawForShadowMap(ref ((Shape) this).InstancedItemData, ref lightView, ref lightProjection);
        }

        // ShadowItem Class's Method 
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

            // dispose managed resources
            _shadowItem.Dispose();
            // free native resources
        }

        #endregion
    }
}