#region File Description
//-----------------------------------------------------------------------------
// BuildingShape.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using AStarInterfaces.AStarAlgorithm.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TWEngine.Common;
using TWEngine.Explosions.Structs;
using TWEngine.GameCamera;
using TWEngine.InstancedModels.Enums;
using TWEngine.InstancedModels.Structs;
using TWEngine.Interfaces;
using TWEngine.ItemTypeAttributes.Structs;
using TWEngine.Players;
using TWEngine.rtsCommands.Enums;
using TWEngine.SceneItems.Enums;
using TWEngine.Utilities;
using TWEngine.Shadows;
using TWEngine.ItemTypeAttributes;
using TWEngine.InstancedModels;
using TWEngine.SceneItems;
using TWEngine.Explosions;
using TWEngine.Terrain;
using TWEngine.rtsCommands;
using TWEngine.Networking;
using TWEngine.MemoryPool;
using TWEngine.Shapes.Enums;

namespace TWEngine.Shapes
{
    ///<summary>
    /// The <see cref="BuildingShape"/> class holds the actual artwork reference, to
    /// either an XNA <see cref="Model"/> or the <see cref="InstancedItem"/> model.  It
    /// provides the ability to 'Pick' the item, retrieve the item's World <see cref="Matrix"/>, 
    /// set the <see cref="ItemType"/>, and start an explosion, to name few.
    ///</summary>
    public sealed class BuildingShape : ShapeWithPick, IShadowShapeItem
    {
        // 9/26/2008 - Add ShadowItem Class
        private ShadowItem _shadowItem;

        // 12/8/2008 - Add ItemToPosition bone references
        private bool _itemToPositionsSet;
        private Matrix _itemToPosition1 = Matrix.Identity;
        private Matrix _itemToPosition2 = Matrix.Identity;  
        
        internal ScenaryItemTypeAttributes ItemTypeAtts;

        // 2/24/2009
        /// <summary>
        /// Set when explosion animation started.
        /// </summary>
        internal bool ExplodeAnimStarted;

        // 8/20/2012 - Set when the 'SetMarkerPosition' method is called. (Scripting Conditions)
        private bool _setMarkerPositionCalled;
        // 8/20/2012 - Stores the current 'Flag' world position. (Scripting Conditions)
        private Vector3 _flagMarkerWorldPosition;

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
        /// <remarks>Requires the <see cref="IsPathBlocked"/> to be TRUE.</remarks>
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

        // Sets the Inherited Interface Property Model
        ///<summary>
        /// Set or get reference to XNA <see cref="Model"/>.
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
        /// Gets or Sets if model animates?
        /// </summary>
        public bool ModelAnimates
        {
            get { return _shadowItem.ModelAnimates; }
            set { _shadowItem.ModelAnimates = value; }
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
        public BuildingShape(Game game, ItemType itemType, byte playerNumber)
            : base(game)
        {
            ((Shape) this).InstancedItemData.ItemType = itemType;
            PlayerNumber = playerNumber; // 11/19/2008
            ((Shape) this).InstancedItemData.ItemInstanceKey = InstancedItem.GenerateItemInstanceKey(ref ((Shape) this).InstancedItemData);          
            

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

        // 2/24/2009
        /// <summary>
        /// Sets what <see cref="ItemType"/> to use for this <see cref="BuildingShape"/> instance; for
        /// example, 'SciFi-Building1' or 'SciFi-Building2' ItemType.
        /// </summary>
        /// <param name="itemType"><see cref="ItemType"/> Enum to use</param>
        public override void SetInstancedItemTypeToUse(ItemType itemType)
        {            
            ((Shape) this).InstancedItemData.ItemGroupType = ItemGroupType.Buildings; 

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
        
        ///<summary>
        /// Overrides the <see cref="Shape.World"/> property, to redirect calls here to also update
        /// the <see cref="InstancedModel"/> transforms.   
        ///</summary>
        public override Matrix WorldP
        {
            get { return base.WorldP; }
            set
            {                
                base.WorldP = value;
                
                InstancedItem.UpdatePlayableModelTransform(this, ref ((Shape) this).InstancedItemData, PlayerNumber);
                
            }
        }


        // 9/24/2008 -   
        /// <summary>
        /// Overrides Shape.IsMeshPicked method to now check for <see cref="InstancedModel"/>.
        /// </summary>
        /// <param name="intersectionDistance">(OUT) intersectoin distance</param>
        /// <returns>true/false result</returns>
        public override bool IsMeshPicked(out float? intersectionDistance)
        {
            return InstancedItem.IsMeshPicked(ref ((Shape)this).InstancedItemData, out intersectionDistance);            
        }

        /// <summary>
        /// Creates the <see cref="ShadowItem"/> structure.
        /// </summary>
        /// <param name="castShadow">Cast shadow?</param>
        private void CreateShadowItem(bool castShadow)
        {
            // ShadowItem            
            _shadowItem = new ShadowItem(Game, ItemTypeAtts.modelAnimates) {BoneTransforms = new Matrix[1]};

            // 1/6/2009 - If Building Animates, then set shadow to always update
            if (ItemTypeAtts.modelAnimates)
                InstancedItem.SetAlwaysDrawShadow(ref ((Shape) this).InstancedItemData, true);

            ModelCastShadow = castShadow;
        }
       
        // 8/10/2009 - Updated to now directly add the final goal Position into the Queue, rather
        //             than adding the 'PathMoveToCompleted' event.
        // 12/4/2008 - Updated to remove Overload ops, since this slows down XBOX!
        /// <summary>
        /// Moves the <paramref name="itemCreated"/>, like a tank, out of the <see cref="BuildingShape"/> Structure, to the 'ItemToPosition1'
        /// bone position, and then the 'ItemToPosition2' bone position.
        /// </summary>
        /// <remarks>'ItemToPosition2' is dynamic, allowing the user to set different queue targets.</remarks>
        /// <param name="itemCreated"><see cref="SceneItemWithPick"/> instance</param>
        public void MoveItemOut(SceneItemWithPick itemCreated)
        {
            // 12/8/2008
            if (!_itemToPositionsSet)
                CalculateItemToPositions();

            // 2/28/2011 - Set Seek value 
            itemCreated.DoSeekOutOfBuildingCheck = true;
            itemCreated.WarFactoryOwner = this;
            itemCreated.MoveToPosition = _itemToPosition1.Translation;

            // 11/9/2009 - Check if AStarItemI is null.
            var aStarItemI = itemCreated.AStarItemI; // 4/30/2010 - Cache
            if (aStarItemI == null)
            {
                System.Diagnostics.Debug.WriteLine("Method Error: AStarItemI instance is NULL, in 'MoveItemOut'.");
                return;
            }

            aStarItemI.MoveToPosition = _itemToPosition1.Translation;            
            aStarItemI.ItemState = ItemStates.PathFindingMoving;
           
        }

        // 2/28/2011
        /// <summary>
        /// Does the pathfinding to the Flag Marker's position.
        /// </summary>
        /// <param name="itemCreated">Instance of <see cref="SceneItemWithPick"/>.</param>
        internal void DoMoveToFlagMarker(SceneItemWithPick itemCreated)
        {
            // 11/9/2009 - Check if AStarItemI is null.
            var aStarItemI = itemCreated.AStarItemI; // 4/30/2010 - Cache
            if (aStarItemI == null)
            {
                itemCreated.AStarItemI = new AStarItem(TemporalWars3DEngine.GameInstance, itemCreated);

#if DEBUG
                System.Diagnostics.Debug.WriteLine("Method Error: AStarItemI instance is NULL, in 'MoveItemOut'.");
#endif
                return;
            }

            // 1/13/2010 - Check if 'IAstarGraph' is null.
            var aStarGraph = TemporalWars3DEngine.AStarGraph; // 4/30/2010 - Cache
            if (aStarGraph == null)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("Method Error: IAstarGraph instance is NULL, in 'MoveItemOut'.");
#endif
                return;
            }

            // 8/10/2009 - Add 2nd ItemToPosition for Queue.
            var flagGoalPosition = _itemToPosition2.Translation;

            // 11/15/2009 - Updated to use the new 'GetOccupiedByIndex'.
            // 11/10/2009 - Check if FlagMarker position is already occupied, and if so, find alt node to move to.
            var foundAltNode = false;
            var index = new Point { X = (int)flagGoalPosition.X, Y = (int)flagGoalPosition.Z};
            object occupiedByOut;
            var altGoalPosition = Vector3.Zero;

            if (aStarGraph.GetOccupiedByAtIndex(ref index, NodeScale.TerrainScale, aStarItemI.UsePathNodeType, out occupiedByOut) ||
                aStarGraph.IsNodeBlocked(NodeScale.TerrainScale, (int)flagGoalPosition.X, (int)flagGoalPosition.Z))
            {
                // then find alt node to move to.
                foundAltNode = AStarItem.GetClosestFreeNode(NodeScale.TerrainScale,
                                                            aStarItemI.UsePathNodeType, ref flagGoalPosition,
                                                            out altGoalPosition);
            }

            // 11/16/2009 - Let's set the 'Occupied' immediately, even before the item gets there.
            flagGoalPosition.Y = 0;
            var goalPosition = foundAltNode ? altGoalPosition : flagGoalPosition;
            aStarItemI.FlagGoalPosition = goalPosition; // 5/29/2011 - Store the Flag goal position to know when the 'InTransitiontoFlagMarker' state is complete.
            AStarItem.SetOccupiedByToGivenPosition(aStarItemI, ref goalPosition);
            
            
            // 5/29/2011 - Check if GroundItem and set 'CanPassOverBlockedAreas' to TRUE when unit comes out of building, avoiding
            //             the possibility of getting stuck.
            aStarItemI.CanPassOverBlockedAreas = (aStarItemI.UsePathNodeType == PathNodeType.GroundItem);
            aStarItemI.InTransitionToFlagMarker = true; // 5/29/2011
            aStarItemI.PathToQueue.Clear();
            aStarItemI.PathToQueue.Enqueue(goalPosition);
        }


        // 12/8/2008; 12/19/2008 - Updated to return True/False for success. 
        /// <summary>
        /// Calculates the <see cref="_itemToPosition1"/> and <see cref="_itemToPosition2"/> bone 
        /// references for the current <see cref="BuildingShape"/>, as World cordinates.  
        /// The <see cref="_itemToPosition1"/> and <see cref="_itemToPosition2"/> are 
        /// used to move the <see cref="SceneItem"/> out of a production building.
        /// </summary>
        /// <returns>true/false of result</returns>
        private bool CalculateItemToPositions()
        {
            // ***
            // Calculate ItemToPosition 1
            // ***
            if (InstancedItem.GetInstanceItemAbsoluteBoneTransform(ref ((Shape) this).InstancedItemData,
                                                                   "ItemToPosition1", out _itemToPosition1))
            {
                // 12/4/2008 - Updated to remove Overload ops, since this slows down XBOX!
                //finalTransform = boneTransform * World * Orientation;
                var tmpBoneTransform = _itemToPosition1;
                var tmpWorld = WorldP;
                var tmpOrientation = Orientation;

                Matrix.Multiply(ref tmpBoneTransform, ref tmpWorld, out tmpBoneTransform);
                Matrix.Multiply(ref tmpBoneTransform, ref tmpOrientation, out tmpBoneTransform);
                _itemToPosition1 = tmpBoneTransform;

                // Get Height.                   
                var tmpPosition = _itemToPosition1.Translation;
                tmpPosition.Y = TerrainData.GetTerrainHeight(tmpPosition.X, tmpPosition.Z);
                _itemToPosition1.Translation = tmpPosition;
               
            }
            else return false;

            // ***
            // Calculate ItemToPosition 2
            // ***
            // Move SceneItemOwner to Final 'ItemToPosition2' Bone Position.    
            if (InstancedItem.GetInstanceItemAbsoluteBoneTransform(ref ((Shape) this).InstancedItemData,
                                                                   "ItemToPosition2", out _itemToPosition2))
            {
                // 12/4/2008 - Updated to remove Overload ops, since this slows down XBOX!
                //finalTransform = boneTransform * World * Orientation;
                var tmpBoneTransform = _itemToPosition2;
                var tmpWorld = WorldP;
                var tmpOrientation = Orientation;

                Matrix.Multiply(ref tmpBoneTransform, ref tmpWorld, out tmpBoneTransform);
                Matrix.Multiply(ref tmpBoneTransform, ref tmpOrientation, out tmpBoneTransform);
                _itemToPosition2 = tmpBoneTransform;

                // Make sure Position is on a PathNode                      
                var tmpPosition = Vector3.Zero;
                const int pathNodeStride = TemporalWars3DEngine._pathNodeStride; // 11/10/09
                tmpPosition.X = ((int) (_itemToPosition2.Translation.X/pathNodeStride))*pathNodeStride;
                tmpPosition.Z = ((int) (_itemToPosition2.Translation.Z/pathNodeStride))*pathNodeStride;
                tmpPosition.Y = TerrainData.GetTerrainHeight(tmpPosition.X, tmpPosition.Z);

                _itemToPosition2.Translation = tmpPosition;
                
            }
            else return false;

            // Set flag to true
            _itemToPositionsSet = true;

            return true;
        }

        // 3/10/2009 - 
        ///<summary>
        /// Sets the Marker for the <see cref="SceneItem"/> to path to, using the <paramref name="newMarkerPosition"/> given.
        ///</summary>
        /// <remarks>This Overload version does not have the 'BuildingPosition' parameter.</remarks>
        ///<param name="newMarkerPosition"><see cref="Vector3"/> new marker position</param>
        ///<param name="networkItemNumber"><see cref="SceneItemWithPick.NetworkItemNumber"/> for <see cref="SceneItem"/></param>
        public void SetMarkerPosition(ref Vector3 newMarkerPosition, int networkItemNumber)
        {
            var worldPosition = World.Translation;
            SetMarkerPosition(ref worldPosition, ref newMarkerPosition, networkItemNumber);

            // 8/20/2012 - Set state that this flag was moved (Scripting Conditions)
            _setMarkerPositionCalled = true;
            _flagMarkerWorldPosition = newMarkerPosition;
        }

        // 8/20/2012
        /// <summary>
        /// Returns if the current 'Flag' was moved by user and returns this position in the OUT field.
        /// </summary>
        /// <remarks>
        /// Once called, the state of the 'SetMarkerPositionCalled' internally will be set to FALSE.
        /// </remarks>
        /// <param name="flagCurrentPosition">(OUT) Current flag postion as <see cref="Vector3"/></param>
        /// <returns>true/false of result.</returns>
        public bool IsMarkerPositionUpdated(out Vector3 flagCurrentPosition)
        {
            var isMarkerPositionCalled = _setMarkerPositionCalled;
            _setMarkerPositionCalled = false;
            flagCurrentPosition = _flagMarkerWorldPosition;

            return isMarkerPositionCalled;
        }

        // 12/8/2008; 3/10/2009: Updated to have the 'BuildingPosition' passed in.
        /// <summary>
        /// Sets the Marker for the <see cref="SceneItem"/> to path to, using the <paramref name="newMarkerPosition"/> given.
        /// </summary>
        /// <param name="buildingPosition">Current <see cref="BuildingShape"/> position</param>
        /// <param name="newMarkerPosition"><see cref="Vector3"/> new marker position</param>
        /// <param name="networkItemNumber"><see cref="SceneItemWithPick.NetworkItemNumber"/> for <see cref="SceneItem"/></param>
        public void SetMarkerPosition(ref Vector3 buildingPosition, ref Vector3 newMarkerPosition, int networkItemNumber)
        {
            // 12/19/2008
            // Get Updated copies of ItemToPositions, if possible; if building does not
            // have 'itemPositions' bones, then False will be returned, and the method just
            // return to caller without doing anything!
            if (!CalculateItemToPositions()) return;

            // Calculate the Adjustment required for Absolute bone Transform               
            var tmpValue = buildingPosition;

            Vector3 adjustingBone;
            Vector3.Subtract(ref newMarkerPosition, ref tmpValue, out adjustingBone);

            // Create Adjustment Matrix
            Matrix doTranslation;
            Matrix.CreateTranslation(ref adjustingBone, out doTranslation);

            // Adjust for Scaling of model by taking World (Scale) and using the Inverse!
            // Also, I am going to remove the original 'translation' values, since we don't need it.         

            // 1st - Inverse the World Matrix, to get the scaling values inverse!
            Matrix tmpWorld;
            Matrix.Invert(ref World, out tmpWorld);
            tmpWorld.Translation = Vector3.Zero;
            Matrix.Multiply(ref doTranslation, ref tmpWorld, out doTranslation);

            // 2nd - Let's recreate the Matrix again to remove the scaling, but keep the new translation values.
            tmpValue = doTranslation.Translation;
            Matrix.CreateTranslation(ref tmpValue, out doTranslation);
                

            // Set Adjusting Transform into model            
            InstancedItem.SetAdjustingBoneTransform(ref ((Shape) this).InstancedItemData, "ItemToPosition2", ref doTranslation);

            // (MP) If Client side of MP game, then we need to send new translation to server.
            
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(PlayerNumber, out player);

            // 6/15/2010 - Check if null.
            if (player != null)
                if (player.NetworkSession != null && !player.NetworkSession.IsHost)
                {
                    // 6/29/2009 - Create RTS Command                        
                    RTSCommQueueMarker queueMarker;
                    PoolManager.GetNode(out queueMarker);

                    queueMarker.Clear();
                    queueMarker.NetworkCommand = NetworkCommands.QueueMarker;
                    queueMarker.PlayerNumber = PlayerNumber;
                    queueMarker.NetworkItemNumber = networkItemNumber;
                    queueMarker.NewQueuePosition = newMarkerPosition;

                    // Send to Server using Guaranteed method.
                    NetworkGameComponent.AddCommandsForServerG(queueMarker);
                }

            // NOTE: This is required for the actual spot of the items to move to!
            // Update the 'ItemToPosition2' reference using the 'adjustingBone' without scaling.
            //_itemToPosition2.Translation += adjustingBone;
            tmpValue = _itemToPosition2.Translation;
            Vector3.Add(ref tmpValue, ref adjustingBone, out tmpValue);
            _itemToPosition2.Translation = tmpValue;
        }

        // 1/30/2010 - Updated to include the 'SceneItem' owner param.
        // 2/6/2009 - Updated to use the new ExplosionManager.
        // 11/14/2008 - ExplodeItem
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

            // 5/20/2010 - Refactored out core code to new STATIC method.
            DoStartExplosion(this, sceneItemOwner);

            ExplodeAnimStarted = true;
        }

        // 5/20/2010
        /// <summary>
        /// Method helper, which adds the <see cref="ExplosionItem"/> part with random velocity to the <see cref="ExplosionsManager"/>.
        /// </summary>
        /// <param name="buildingShape">this instance of <see cref="buildingShape"/></param>
        /// <param name="sceneItemOwner"><see cref="SceneItemWithPick"/> owner of this shape</param>
        private static void DoStartExplosion(BuildingShape buildingShape, SceneItemWithPick sceneItemOwner)
        {
            // 5/20/2010 - Const amount of velocity applied to Y access; 0-100%.
            // 1/18/2011 - Velocity Y Height has default 30x in shader.
            const float velocityYPower = 5 * 30 * 0.50f; // Height
            const float velocityXPower = 0.10f; // 1/17/2011
            const float velocityZPower = 0.10f; // 1/17/2011


            Vector3 itemPiece2Velocity, itemPiece3Velocity, itemPiece4Velocity;
            var itemPiece1Velocity = itemPiece2Velocity = itemPiece3Velocity = itemPiece4Velocity = Vector3.Zero;

            var tmpOrientation = buildingShape.Orientation;
            var explosionsManager = TemporalWars3DEngine.ExplosionManager; // 4/30/2010 - Cache
            { // SceneItemOwner Piece 1
                // Set to Projectile Velocity
                itemPiece1Velocity.X = buildingShape.LastProjectileVelocity.X * 4 * velocityXPower;
                itemPiece1Velocity.Z = buildingShape.LastProjectileVelocity.Z * 4 * velocityZPower;
                itemPiece1Velocity.Y = velocityYPower; // 60 frames per second.

                // 1/18/2011 - Add new Explosion Velocity
                InstancedItem.AddBoneExplosionVelocity(ref ((Shape)buildingShape).InstancedItemData, "Piece1", ref itemPiece1Velocity);

                // Create new ExplosionItem instance for each piece needed.                   
                var explosionItemPiece = new ExplosionItem(sceneItemOwner, "Piece1", ref itemPiece1Velocity, ref tmpOrientation, ref buildingShape.World, ref ((Shape)buildingShape).InstancedItemData)
                                             {
                                                 RotAngle = Vector3.Up /* y-axis*/,
                                                 RotSpeed = 1
                                             };
                // Add to ExplosionManager
                explosionsManager.AddNewExplosionItem(ref explosionItemPiece);
            }

            { // SceneItemOwner Piece 2
                // Set to Projectile Velocity
                itemPiece2Velocity.X = buildingShape.LastProjectileVelocity.X * MathUtils.RandomBetween(2, 8) * velocityXPower;
                itemPiece2Velocity.Z = buildingShape.LastProjectileVelocity.Z * MathUtils.RandomBetween(-8, 10) * velocityZPower;
                itemPiece2Velocity.Y = velocityYPower; // 60 frames per second.

                // 1/18/2011 - Add new Explosion Velocity
                InstancedItem.AddBoneExplosionVelocity(ref ((Shape)buildingShape).InstancedItemData, "Piece2", ref itemPiece1Velocity);

                // Create new ExplosionItem instance for each piece needed.                   
                var explosionItemPiece = new ExplosionItem(null, "Piece2", ref itemPiece2Velocity, ref tmpOrientation, ref buildingShape.World, ref ((Shape)buildingShape).InstancedItemData)
                                             {
                                                 RotAngle = Vector3.Backward /* z-axis*/,
                                                 RotSpeed = 2
                                             };
                // Add to ExplosionManager
                explosionsManager.AddNewExplosionItem(ref explosionItemPiece);
            }

            { // SceneItemOwner Piece 3
                // Set to Projectile Velocity
                itemPiece3Velocity.X = buildingShape.LastProjectileVelocity.X * MathUtils.RandomBetween(-8, 10) * velocityXPower;
                itemPiece3Velocity.Z = buildingShape.LastProjectileVelocity.Z * MathUtils.RandomBetween(2, 10) * velocityZPower;
                itemPiece3Velocity.Y = velocityYPower; // 60 frames per second.

                // 1/18/2011 - Add new Explosion Velocity
                InstancedItem.AddBoneExplosionVelocity(ref ((Shape)buildingShape).InstancedItemData, "Piece3", ref itemPiece1Velocity);

                // Create new ExplosionItem instance for each piece needed.                  
                var explosionItemPiece = new ExplosionItem(null, "Piece3", ref itemPiece3Velocity, ref tmpOrientation, ref buildingShape.World, ref ((Shape)buildingShape).InstancedItemData)
                                             {
                                                 RotAngle = Vector3.Right /* x-axis*/,
                                                 RotSpeed = 1
                                             };
                // Add to ExplosionManager
                explosionsManager.AddNewExplosionItem(ref explosionItemPiece);
            }

            { // SceneItemOwner Piece 4
                // Set to Projectile Velocity
                itemPiece4Velocity.X = buildingShape.LastProjectileVelocity.X * MathUtils.RandomBetween(-12, 8) * velocityXPower;
                itemPiece4Velocity.Z = buildingShape.LastProjectileVelocity.Z * MathUtils.RandomBetween(-15, 15) * velocityZPower;
                itemPiece4Velocity.Y = velocityYPower; // 60 frames per second.

                // 1/18/2011 - Add new Explosion Velocity
                InstancedItem.AddBoneExplosionVelocity(ref ((Shape)buildingShape).InstancedItemData, "Piece4", ref itemPiece1Velocity);

                // Create new ExplosionItem instance for each piece needed.                   
                var explosionItemPiece = new ExplosionItem(null, "Piece4", ref itemPiece4Velocity, ref tmpOrientation, ref buildingShape.World, ref ((Shape)buildingShape).InstancedItemData)
                                             {
                                                 RotAngle = Vector3.Right /* x-axis*/,
                                                 RotSpeed = 1.5f
                                             };
                // Add to ExplosionManager
                explosionsManager.AddNewExplosionItem(ref explosionItemPiece);
            }

            // 4/5/2009 - Show Particle Explosion Effects! 
            var currentPosition = buildingShape.World.Translation;
            var lastProjectileVelocity = buildingShape.LastProjectileVelocity;
            ExplosionsManager.DoParticles_LargeExplosion(ref currentPosition, ref lastProjectileVelocity);
        }

        #region IShadowItem Methods

        // 9/26/2008
        ///<summary>
        /// Draws the <see cref="BuildingShape"/> using the <see cref="ShadowMap"/> shader, which will project the shadow for this
        /// <see cref="SceneItem"/> onto the <see cref="ShadowMap"/>.        
        ///</summary>
        ///<param name="lightView">Light view <see cref="Matrix"/></param>
        ///<param name="lightProj">Light projection <see cref="Matrix"/></param>
        public void DrawForShadowMap(ref Matrix lightView, ref Matrix lightProj)
        {
            InstancedItem.DrawForShadowMap(ref ((Shape) this).InstancedItemData, ref lightView, ref lightProj);          

        }

        // 9/26/2008 - 
        ///<summary>
        /// Call the <see cref="ShadowItem"/> method <see cref="StoreModelEffect"/>.
        ///</summary>
        ///<param name="model">XNA <see cref="Model"/> instance</param>
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

            // 5/3/2009
            _shadowItem.Dispose();
            // free native resources
        }

        #endregion
    }
}
