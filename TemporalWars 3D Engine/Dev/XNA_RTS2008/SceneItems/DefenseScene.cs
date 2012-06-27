#region File Description
//-----------------------------------------------------------------------------
// DefenseScene.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics;
using AStarInterfaces.AStarAlgorithm.Enums;
using Microsoft.Xna.Framework;
using Particles3DComponentLibrary;
using TWEngine.AI;
using TWEngine.Audio;
using TWEngine.HandleGameInput;
using TWEngine.InstancedModels.Enums;
using TWEngine.ItemTypeAttributes.Structs;
using TWEngine.MemoryPool;
using TWEngine.Players;
using TWEngine.SceneItems.Structs;
using TWEngine.Terrain.Structs;
using TWEngine.Utilities;
using TWEngine.InstancedModels;
using TWEngine.InstancedModels.Structs;
using TWEngine.Interfaces;
using TWEngine.Particles;
using TWEngine.Particles.Enums;
using TWEngine.Shapes;
using TWEngine.Terrain;
using TWEngine.ForceBehaviors;
using TWEngine.IFDTiles;

namespace TWEngine.SceneItems
{

    // 12/25/2008
    ///<summary>
    /// The <see cref="DefenseScene"/> is used as a static structure, which can shoot 
    /// other <see cref="SceneItem"/> in the game; for example, the <see cref="DefenseScene"/> could
    /// be a mounted turret for some base.
    ///</summary>
    public sealed class DefenseScene : SceneItemWithPick, ITurretAttributes, IFOWPlaceableItem
    {
        private Vector3 _tmpZero = Vector3Zero;

        // Turret Atts Class for Indirect Inheritance
        private TurretAttributes _turretAtts;

        // 2/17/2009 - List of _spawnBulletTransforms; ref retrieved in 'SetBulletPosition' method.
        private readonly List<InstancedItemTransform> _spawnBulletTransforms;

        // 2/26/2009 - Ref to DefenseScenePoolItem Wrapper class
        ///<summary>
        /// Reference for the <see cref="DefenseScenePoolItem"/> instance
        ///</summary>
        public new DefenseScenePoolItem PoolItemWrapper;  

        // 2/27/2009 - Queue of Items to attack; needed, since the MP games can be slightly out of sync!  In other words,
        //             the Client could be attacking an SceneItemOwner, which is almost dead, but the Server is already attacking the
        //             next SceneItemOwner.  When the server sent the attack command, the client lost it, since it was still busy
        //             attacking the last SceneItemOwner.  This Queue guarantees the client will complete all Attack Commands sent to it.
        private readonly Queue<SceneItemWithPick> _itemsToAttack = new Queue<SceneItemWithPick>();       

        // Position of damage fire particles, when health falls below 50%
        private Vector3 _damageFirePosition = Vector3Zero;

        // 4/11/2009 - VisualCircle Index
        private int _visualCircleIndex;
        private VisualCircleRadius _visualCircle;       

        #region Properties

        // Override Delete so we can also delete Transform from InstanceItem.
        /// <summary>
        /// Should this <see cref="DefenseScene"/> be deleted?
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
                    // 1/18/2011
                    if (ShapeItem != null)
                        InstancedItem.RemoveInstanceTransform(ref ((Shape) ShapeItem).InstancedItemData);
                }

                base.Delete = value;
            }
        }

        // 2/5/2009
        ///<summary>
        /// Get or Set the <see cref="DefenseShape"/> instance
        ///</summary>
        public new DefenseShape ShapeItem
        {
            get
            {
                return (base.ShapeItem as DefenseShape);
            }
            set
            {
                ShapeItem = value;
            }

        }
      

        #region ITurretAttributes

        ///<summary>
        /// Set or Get the turret's facing direction.
        ///</summary>
        ///<exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not within the required range of -pi to pi.</exception>
        public float TurretFacingDirection
        {
            get
            {
                return _turretAtts.TurretFacingDirection;
            }
            set
            {
                // 8/23/2009 - Make sure value given is in Radian measurement.
                const float pi = MathHelper.Pi;
                if (value < -pi || value > pi)
                    throw new ArgumentOutOfRangeException("value", @"The current value was not within the required range of -pi to pi!");

                _turretAtts.TurretFacingDirection = value;
            }
        }

        ///<summary>
        /// Get or Set the turret's turn speed.
        ///</summary>
        public float TurretTurnSpeed
        {
            get
            {
                return _turretAtts.TurretTurnSpeed;
            }
            set
            {
                _turretAtts.TurretTurnSpeed = value;
            }
        }

        ///<summary>
        /// Get or Set the turret's desired angle.
        ///</summary>
        ///<exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not within the required range of -pi to pi.</exception>
        public float TurretDesiredAngle
        {
            get
            {
                return _turretAtts.TurretDesiredAngle;
            }
            set
            {
                // 8/23/2009 - Make sure value given is in Radian measurement.
                const float pi = MathHelper.Pi;
                if (value < -pi || value > pi)
                    throw new ArgumentOutOfRangeException("value", @"The current value was not within the required range of -pi to pi!");

                _turretAtts.TurretDesiredAngle = value;
            }
        }

        #endregion


        #endregion

        ///<summary>
        /// Constructor, which creates the given <see cref="DefenseScene"/> item.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        ///<param name="itemType"><see cref="ItemType"/> to use</param>
        ///<param name="itemGroupToAttack"><see cref="ItemGroupType"/> Enum this item can attack</param>
        ///<param name="initialPosition"><see cref="Vector3"/> initial position</param>
        ///<param name="playerNumber">The player number this item belongs to</param>
        public DefenseScene(Game game, ItemType itemType, ItemGroupType itemGroupToAttack, ref Vector3 initialPosition, byte playerNumber)
            : base(game, new DefenseShape(game, itemType, playerNumber), ref initialPosition, playerNumber)
        {     
            // 12/26/2008 - Save ItemGroupToAttack
            ItemGroupTypeToAttackE = itemGroupToAttack;

            // Set ItemMovable to False
            ItemMoveable = false;            

            // 5/13/2009 - Init the List<InstancedITemTransform>.
            _spawnBulletTransforms = new List<InstancedItemTransform>(4);
           
        }       

        // 1/30/2009
        /// <summary>
        /// Populates the <see cref="PlayableItemTypeAttributes"/> struct with the common attributes
        /// used by the given <see cref="ItemType"/>.
        /// </summary>
        /// <param name="e"><see cref="ItemCreatedArgs"/> instance</param>
        /// <param name="isFinalPosition">Position to place <see cref="SciFiTankScene"/></param>
        public override void LoadPlayableAttributesForItem(ItemCreatedArgs e, bool isFinalPosition)
        {
            base.LoadPlayableAttributesForItem(e, isFinalPosition);

            // 4/26/2010: Updated to check if 'ItemGroupToAttack' is null.
            // 12/26/2008 - Save ItemGroupToAttack
            if (e.ItemGroupToAttack != null) ItemGroupTypeToAttackE = e.ItemGroupToAttack.Value;

            // 2/26/2009
            CommonInitilization(GameInstance, e.PlaceItemAt);

        }

        // 2/26/2009
        /// <summary>
        /// The <see cref="CommonInitilization"/> method sets internal tweakable flags
        /// back to there defaults, retrieves the current rotation value, updates the proper
        /// <see cref="IFogOfWar"/> settings if required, and obtains the current <see cref="Terrain"/>
        /// height for the given position.
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        /// <param name="initialPosition"><see cref="Vector3"/> position to place item</param>
        private void CommonInitilization(Game game, Vector3 initialPosition)
        {
            
            // 2/23/2009 - Reset flags correctly
            IsAlive = true;
            Delete = false;
            ShapeItem.ExplodeAnimStarted = false;
            ThePickSelected = false; // 7/12/2009

            // 3/28/2009 - Tell InstanceModel to draw using Normal pieces, and not explosion pieces!
            InstancedItem.UpdateInstanceModelToDrawExplosionPieces(ref ((Shape) ShapeItem).InstancedItemData, PlayerNumber, false);

            // Speed turret can turn at
            _turretAtts.TurretTurnSpeed = PlayableItemAtts.TurretTurnSpeed;

            // Set FogOfWar Radius, based on SceneItem ViewRadius Property
            if (ShapeItem.UseFogOfWar)
            {
                ShapeItem.FogOfWarHeight = (int)ViewRadius / TerrainData.cScale;
                ShapeItem.FogOfWarWidth = (int)ViewRadius / TerrainData.cScale;
                
                // 1/14/2009 - Make sure it can be seen immediately!
                ShapeItem.IsFOWVisible = true;

                var fogOfWar = (IFogOfWar) game.Services.GetService(typeof (IFogOfWar));
                if (fogOfWar != null) fogOfWar.UpdateSight = true;
            }

            // 2/10/2009 - Get the default Scale value, contain in the ItemType's content pipeline file.
            float useScale;
            InstancedItem.GetScale(ref ((Shape) ShapeItem).InstancedItemData, out useScale);
            scale = Vector3Zero;
            scale.X = scale.Y = scale.Z = useScale;

            // Retrieve the ITerrainShape Interface
            TerrainShape = (ITerrainShape)game.Services.GetService(typeof(ITerrainShape));

            // Set to appropiate height on map.
            if (TerrainData.IsOnHeightmap(initialPosition.X, initialPosition.Z))
            {
                initialPosition.Y = TerrainData.GetTerrainHeight(initialPosition.X, initialPosition.Z);

                Position = initialPosition;

            }

            // Get the default Rotation values, contain in the ItemType's content pipeline file.
            float rotX, rotY, rotZ;
            InstancedItem.GetRotationX(ref ((Shape) ShapeItem).InstancedItemData, out rotX);
            InstancedItem.GetRotationY(ref ((Shape) ShapeItem).InstancedItemData, out rotY);
            InstancedItem.GetRotationZ(ref ((Shape) ShapeItem).InstancedItemData, out rotZ);
            Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(rotY), MathHelper.ToRadians(rotX), MathHelper.ToRadians(rotZ), out rotation);
             

            // ForceBehavior
            if (ForceBehaviors == null)
            {
                ForceBehaviors = new ForceBehaviorsCalculator(game, this);
                //defenseTurretRestBehavior = (DefenseTurretAbstractBehavior)ForceBehaviors.Add(Behaviors.DefenseTurretAbstractBehavior);
                // 1/26/2009 - Add Ref to ForceBehaviorsManager class
                ForceBehaviorsManager.Add(ForceBehaviors);
            }

            //if (((Shape)ShapeItem).InstancedItemData.ItemType == ItemType.sciFiAAGun05)
                //Debugger.Break(); // 1/15/2011

            // 6/1/2009 - Add DefenseBehavior AI to AIThreadManager.
            AIManager.AddDefenseAI(this);

            // Add Particles Class
            // Turn on use of ParticleSystem Projectiles
            UseProjectiles = true;

            // 1/15/2009 - Tell Minimap to update the unit positions.
            var miniMap = (IMinimap)game.Services.GetService(typeof(IMinimap)); // 1/2/2010
            if (miniMap != null) miniMap.DoUpdateMiniMap = true;
        }


        /// <summary>
        /// Updates the current <see cref="DefenseScene"/>, by initially calling the base, then sets the
        /// <see cref="TWEngine.ForceBehaviors"/> elasped time value, and finally calls the <see cref="HandleGameInput"/>.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="time">The <see cref="TimeSpan"/> structure as game time</param>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> structure as elapsed time</param>
        /// <param name="isClientCall">Is this the client-side update in a network game?</param>
        public override void Update(GameTime gameTime, ref TimeSpan time, ref TimeSpan elapsedTime, bool isClientCall)
        {
            base.Update(gameTime, ref time, ref elapsedTime, isClientCall);
           

            // 1/30/2009 - Make sure isAlive first.
            if (!IsAlive)
                return;

            // Set ThreadElapsedTime HERE, since the DefenseScene does not have an AstarItem!
            if (ForceBehaviors != null)
                ForceBehaviors.ThreadElapsedTime = elapsedTime;

            // 1/16/2011 - Fixed: Moved from Render to Update.
            if (ShapeItem != null)
                // Update Rotation Matrix when RotValue changes
                InstancedItem.SetAdjustingBoneTransform(ref ((Shape)ShapeItem).InstancedItemData, "turret", RotationAxis.Y, TurretFacingDirection);
            

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out player);

            if (player == null) return;

            // Is Network Game?
            if (player.NetworkSession != null)
            {
                // HandleInput - (MP games only check their own player)
                if (player.PlayerNumber == PlayerNumber)
                    HandleInput.DefenseSceneInputCheck(this);


                // 8/17/2009 - Updated the DefenseScene Client AI, since client does not have a FSMAIControl state machine.
                if (isClientCall)
                    UpdateDefenseBehavior_Client();
            }
            else
                HandleInput.DefenseSceneInputCheck(this); // SP Game 
            
        }

        #region AI Methods        

        private readonly Random _rndGenerator = new Random();
        internal float DesiredAngle;
// ReSharper disable UnaccessedField.Local
        private float _angleDifference; // Stores value of currentAngle/DesiredAngle from TurnToFace method.      
// ReSharper restore UnaccessedField.Local
        private readonly Stopwatch _timeToRandomTurretMoveStopWatch = new Stopwatch();
        private readonly float _maxTimeToMove = MathUtils.RandomBetween(4000, 10000); // between 4-10 seconds.
        private bool _stopWatchStarted;


        /// <summary>
        /// Turret basic <see cref="DefenseScene"/> logic is contained in this method.  This will be called
        /// from the <see cref="AIManager"/> thread engines.
        /// </summary>
        /// <param name="gameTime"></param>
        internal override void UpdateDefenseBehavior(GameTime gameTime)
        {
            //if (ShapeItem.ItemType == ItemType.sciFiAAGun05)
                //Debugger.Break(); // 1/16/2011

            // 8/18/2009- Add FSMAIControl component.
            // ***
            // NOTE: This MUST be created here, to force the FSMAIControl to
            // call the proper 'DefenseScene' constructor, which will then add the proper
            // DefenseScene AI States!
            // ***
            if (_FSMAIControl == null)
                _FSMAIControl = new FSM_AIControl(this);

            // 7/31/2009 - Update FSMAIControl.
            _FSMAIControl.Update(gameTime);

            #region OldCode
            // 8/7/2009 : TODO: Need to replace this with the FSMSection.
            /*if (ImageNexusRTSGameEngine.Players[ImageNexusRTSGameEngine.ThisPlayer].NetworkSession != null)
                UpdateDefenseBehavior_MP();

            force = Vector3.Zero;

            // 7/16/2009 - Copy 'AttackSceneItem' into local cache, to eliminate Thread Sync errors!
            SceneItem attackie = AttackSceneItem;

            // 1/30/2009 - Check if EnergyOff for the player this SceneItemOwner belongs to
            if (ImageNexusRTSGameEngine.Players[PlayerNumber].EnergyOff)
            {
                AttackOn = false;
                return;
            }

            // 6/3/2009 - If attackie out of range, then stop trying to attack that SceneItemOwner!
            if (AttackOn && attackie != null && !IsAttackieInAttackRadius(attackie))
            {
                // 6/3/2009 - Remove EventHandler Reference                
                attackie.RemoveEventHandler_SceneItemDestroyed(AttackSceneItemSceneItemDestroyed, this);

                // Then stop trying to attack.
                AttackOn = false;
                AttackSceneItem = null;
                AIOrderIssued = AIOrderType.None;
            }

            // 1/8/2008
            //TurretRandomMovementCheck();

            if (ForceBehaviors != null)
            {
                // If Attack Ground units.
                if (ItemGroupTypeToAttack == ItemGroupType.Vehicles)
                    AttackSomeNeighborItemSp(ForceBehaviors.NeighborsGround);

                // If Attack Air units.
                if (ItemGroupTypeToAttack == ItemGroupType.Airplanes)
                    AttackSomeNeighborItemSp(ForceBehaviors.NeighborsAir);
            }


            // 6/1/2009
            //UpdateTurretFacingDirection();*/
            #endregion

        }
        
        /// <summary>
        /// Attacks the 'AttackSceneItem' set by Host; also checks the _itemsToAttack Queue, for any other items to attack.
        /// </summary>
        private void UpdateDefenseBehavior_Client()
        {
            // 7/16/2009 - Copy 'AttackSceneItem' into local cache, to eliminate Thread Sync errors!
            var attackie = AttackSceneItem;

            //     - if no SceneItemOwner to attack, then get a random angle for the turret to move to; otherwise,
            //       calculate angle based on attackie's Position.
            if (!AttackOn || attackie == null || !attackie.IsAlive) // was !SceneItemOwner.Attackon
            {
                // 2/27/2009 - check Queue to see if any other items to attack
                if (_itemsToAttack.Count > 0)
                {
                    // dequeue SceneItemOwner, and see if it is valid to attack
                    var itemToAttack = _itemsToAttack.Dequeue();

                    // is alive?
                    if (itemToAttack.IsAlive)
                    {
                        AttackSceneItem = itemToAttack;
                        AttackOrder();
                    }
                }
            }
            else
            {
                // 7/16/2009 - Calculate new FacingDirection
                CalculateDesiredAngle(attackie, out DesiredAngle);

            }// End if ClientDoAttack
        } 
       
        /// <summary>
        /// Does a random turret movement when not attacking a <see cref="SceneItem"/>.
        /// </summary>
        internal void TurretRandomMovementCheck()
        {
            // 7/16/2009 - Copy 'AttackSceneItem' into local cache, to eliminate Thread Sync errors!
            // 7/18/2009: Was = (AttackSceneItem != null) ? AttackSceneItem : null;
            var attackie = AttackSceneItem;

            // 6/3/2009
            if (!_stopWatchStarted)
            {
                _timeToRandomTurretMoveStopWatch.Start();
                _stopWatchStarted = true;
            }

            if ((AttackOn && attackie != null) && attackie.IsAlive) return;

            // *****
            // Then just calculate a random movement
            // *****
            // If Time elapsed, then get new angle
            //if (timeToRandomTurretMove.TotalMilliseconds <= 0.0f)
            // 3/11/2009
            if (_timeToRandomTurretMoveStopWatch.ElapsedMilliseconds < _maxTimeToMove) return;

            // Get new angle to move defense turret to 
            DesiredAngle = _rndGenerator.Next(-180, 180);
            // 8/23/09 - Convert angle to radians for Cos/Sin function below.
            DesiredAngle = MathHelper.ToRadians(DesiredAngle);

            // Reset timer
            //timeToRandomTurretMove = TimeSpan.FromSeconds(5);
            _timeToRandomTurretMoveStopWatch.Reset();
            _timeToRandomTurretMoveStopWatch.Start();
        }

        // 6/1/2009
        /// <summary>
        /// Updates the Turret's facing direction.
        /// </summary>
        internal void UpdateTurretFacingDirection()
        {
            // Call TurnToFace AbstractBehavior
            TurretFacingDirection = TurnToFace(DesiredAngle, TurretFacingDirection, TurretTurnSpeed, FacingDirectionOffset, out _angleDifference);

            TurretDesiredAngle = DesiredAngle;
                
        }

        #endregion       


        /// <summary>
        /// When a <see cref="SceneItem"/> is placed on the <see cref="Terrain"/>, via the <see cref="IFDTileManager"/>, this method
        /// is called in order to do specific <see cref="SceneItem"/> placement checks; for example, if the <see cref="SceneItem"/>
        /// requires A* blocking updated.
        /// </summary>
        /// <param name="placementPosition">The <see cref="Vector3"/> position to place item at</param>
        /// <returns>true/false of result</returns>
        public override bool RunPlacementCheck(ref Vector3 placementPosition)
        {
            // 1/13/2010
            var aStarGraph = TemporalWars3DEngine.AStarGraph; // 4/26/2010
            if (aStarGraph == null) return true;

            // 1/5/2009 - Check if SceneItemOwner can be placed at the current location, given the 'PathBlockSize'.            
            return (!aStarGraph.IsPathNodeSectionBlocked((int)placementPosition.X, (int)placementPosition.Z, ShapeItem.PathBlockSize, BlockedType.Any));
        }

        /// <summary>
        /// When a <see cref="SceneItem"/> is placed on the <see cref="Terrain"/>, via the <see cref="IFDTileManager"/>, this method
        /// is called to check if the x/y values given, are within this sceneItem's <paramref name="pathBlockSize"/> zone.
        /// </summary>
        /// <param name="placementPosition">The <see cref="Vector3"/> position to place item at</param>
        /// <param name="x">X-value</param>
        /// <param name="y">Y-value</param>
        /// <param name="pathBlockSize">The scene item <paramref name="pathBlockSize"/>.</param>
        /// <returns>true/false of result</returns>
        public override bool IsInPlacementZone(ref Vector3 placementPosition, int x, int y, int pathBlockSize)
        {
            // call base version, passing in the 'pathBlockSize' from the ShapeItem.
            return base.IsInPlacementZone(ref placementPosition, x, y, ShapeItem.PathBlockSize);

        }

        /// <summary>
        /// Once a <see cref="SceneItem"/> is placed on the <see cref="Terrain"/>, this method is called
        /// in order to set its placement in the AStarGraph component, using the PathBlockSize.
        /// </summary>
        /// <param name="placementPosition">The <see cref="Vector3"/> position to place item at</param>
        public override void SetPlacement(ref Vector3 placementPosition)
        {
            // 6/26/2012 - Call base
            base.SetPlacement(ref placementPosition);

            // Get Items Path Blocking Size and set in AStarGraph.
            if (ShapeItem.IsPathBlocked)
            {
                //  5/18/2009 - Updated to set the cost to a value of -2, rather than -1, which is reserved for 'Blocked'
                //              sections of the map!  The difference is important, because it affects how the Cursor 'Blocked'
                //              image is displayed, which is only done when it reads the -1 cost value!
                var aStarGraph = TemporalWars3DEngine.AStarGraph; // 4/26/2010
                if (aStarGraph != null) // 1/13/2010
                    aStarGraph.SetCostToPos((int)placementPosition.X, (int)placementPosition.Z, -2, ShapeItem.PathBlockSize);

                Terrain.TerrainShape.PopulatePathNodesArray();
            }

            // 4/11/2009 - Add VisualCircle
            _visualCircle = new VisualCircleRadius(position, AttackRadius);
            _visualCircleIndex = Terrain.TerrainShape.TerrainVisualCircles.AddVisualCircle(ref _visualCircle);
        }

        /// <summary>
        /// This overrides the base method by setting the <see cref="SceneItem.CollisionRadius"/>
        /// using the <see cref="InstancedItem"/> model <see cref="BoundingSphere"/>, rather than the
        /// XNA model.
        /// </summary>
        protected override void SetCollisionRadius()
        {
            CollisionRadius = InstancedItem.GetInstanceItemCollisionRadius(ref ((Shape) ShapeItem).InstancedItemData, PlayableItemAtts.AdjustCollisionRadius);           
           
        }

        // 4/11/2009
        /// <summary>
        /// When <paramref name="pickSelected"/> true, this method calls the <see cref="TerrainVisualCircles"/> class
        /// to show the visual circles.
        /// </summary>
        /// <param name="pickSelected">Is pick selected?</param>
        protected override void OnPickSelected(bool pickSelected)
        {
            // 4/11/2009 - Update displaying of visual circle.
            Terrain.TerrainShape.TerrainVisualCircles.ShowVisualCircle(_visualCircleIndex, pickSelected);            
            
        }

        // 2/27/2009
        ///<summary>
        /// Adds the given <see cref="SceneItemWithPick"/> to attack into this <see cref="DefenseScene"/> item
        /// queue, called <see cref="_itemsToAttack"/>.
        ///</summary>
        ///<param name="itemToAttack"><see cref="SceneItemWithPick"/> instance to queue</param>
        public void StoreItemToAttackInBehaviorQueue(SceneItemWithPick itemToAttack)
        {
            // 2/27/2009 - Add to AbstractBehavior's internal Queue.
            //defenseTurretRestBehavior._itemsToAttack.Enqueue(itemToAttack);
            _itemsToAttack.Enqueue(itemToAttack);
        }

        float _rotTime;
        private static readonly Vector3 Vector3Zero = Vector3.Zero;

        /// <summary>
        /// Will either just reduce health of enemy unit, or shoot projectiles depending
        /// on defense type.
        /// </summary>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> structure</param>
        protected override void ShootBullets(ref TimeSpan elapsedTime)
        {
            // 1/30/2009            
            // Add Projectiles to fire     
            
            // If AA-Gun02, then continuosly rotate barrels
            if (ShapeItem.ItemType == ItemType.sciFiAAGun02)
            {
                // 5/22/2009 - Updated to use the new overload version of 'SetAdjustingBoneTransform'.
                _rotTime += (float)elapsedTime.TotalSeconds;
                InstancedItem.SetAdjustingBoneTransform(ref ((Shape) ShapeItem).InstancedItemData, "turret-barrels", RotationAxis.Z, _rotTime * 8.0f);
            }

            base.ShootBullets(ref elapsedTime);
        }
       
        // 2/17/2009: Updated to get the List of SpawnBulletTransforms only once.
        /// <summary>
        /// Returns the SpawnBullet position for the given <paramref name="spawnBulletNumber"/> value.
        /// </summary> 
        /// <param name="spawnBulletNumber">SpawnBullet Number to retrieve</param>
        /// <param name="bulletSpawnPosition">(OUT) World position for SpawnBullet</param>
        protected override void GetBulletStartPosition(int spawnBulletNumber, out Vector3 bulletSpawnPosition)
        {
            bulletSpawnPosition = default(Vector3);

            InstancedItem.GetInstancedModelSpawnBulletPositions(ref ((Shape)ShapeItem).InstancedItemData, _spawnBulletTransforms);

            // 1/21/2010 - Check if '_spawnBulletTransforms' is empty.
            var spawnBulletTransforms = _spawnBulletTransforms; // 4/27/2010
            if (spawnBulletTransforms.Count == 0) return;

            // Apply Orientation & World Transforms
            //Transform *= (ShapeItem as DefenseShape).Orientation * (ShapeItem as DefenseShape).World;
            // 2/17/2009 - Updated to use new InstanceItem method to get World Position of bone.
            var instancedItemTransform = spawnBulletTransforms[spawnBulletNumber - 1];
            InstancedItem.GetWorldPositionFromBoneTransform(this, ref instancedItemTransform.AbsoluteTransform, out bulletSpawnPosition);
            
        }


        /// <summary>
        /// Removes <see cref="SceneItem"/> from game, while calling some random death animation.
        /// </summary>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> structure</param>
        /// <param name="attackerPlayerNumber">Attacker's player number</param>
        public override void StartKillSceneItem(ref TimeSpan elapsedTime, int attackerPlayerNumber)
        {
            // Added 'KillSceneItemCalled' check to make sure code is not executed twice, since during MP games,
            // the Server will make sure client kills the unit by calling this too!
            if (KillSceneItemCalled) return;

            // 5/20/2010: Moved to top, rather than bottom of method call to have 'IsAlive' applied first!
            base.StartKillSceneItem(ref elapsedTime, attackerPlayerNumber);
           
            // 5/12/2009 - Remove or Pause any sounds for given SceneItemOwner                
            AudioManager.Remove(UniqueKey, PlayableItemAtts.SoundsToPlay.SoundToPlayPrimaryFire);
            AudioManager.Remove(UniqueKey, PlayableItemAtts.SoundsToPlay.SoundToPlaySecondaryFire);

            // Remove AStar Costs at our Position          
            var aStarGraph = TemporalWars3DEngine.AStarGraph; // 4/26/2010
            if (aStarGraph != null) // 1/13/2010
                aStarGraph.RemoveCostAtPos((int)position.X, (int)position.Z, ShapeItem.PathBlockSize);

            // 4/11/2009 - Remove VisualCircle
            Terrain.TerrainShape.TerrainVisualCircles.RemoveVisualCircle(_visualCircleIndex);

            // 3/28/2009 - Tell InstanceModel to draw using Explosion Pieces!
            InstancedItem.UpdateInstanceModelToDrawExplosionPieces(ref ((Shape) ShapeItem).InstancedItemData,
                                                                   PlayerNumber, true);

            // Start Explosion Animations
        }

        // 2/26/2009
        /// <summary>
        /// Returns this instance back into the <see cref="PoolManager"/>, setting 'Active' to false.
        /// </summary>
        /// <param name="isInterfaceDisplayNode">Created by <see cref="IFDTile"/></param>
        public override void ReturnItemToPool(bool isInterfaceDisplayNode)
        {
            // 6/29/2009
            var defenseScenePoolItem = PoolItemWrapper; // 4/27/2010
            if (defenseScenePoolItem == null) return;

            if (defenseScenePoolItem.PoolNode != null) 
                defenseScenePoolItem.PoolNode.ReturnToPool();
            else
                Debug.WriteLine("(ReturnItemToPool) failed to return pool node.");
        }


        /// <summary>
        /// When <see cref="DefenseScene"/> health falls below 50%, this will draw the smoke <see cref="ParticleSystem"/>
        /// on the <see cref="DefenseScene"/>.
        /// </summary>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> structure</param>
        protected override void Below50HealthParticleEffects(ref TimeSpan elapsedTime)
        {
            // 1/15/2009: Updated to show only when in FOW view and in Camera view!
            if (!ShapeItem.IsFOWVisible || !InstancedItem.IsInCameraView(ref ((Shape) ShapeItem).InstancedItemData))
                return;

            ParticlesManager.UpdateParticleSystem(ParticleSystemTypes.BlackSmokePlumeParticleSystem, ref position, ref _tmpZero);
        }

        /// <summary>
        /// When <see cref="DefenseScene"/> health falls below 25%, this will draw the fire <see cref="ParticleSystem"/>
        /// on the <see cref="DefenseScene"/>.
        /// </summary>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> structure</param>
        protected override void Below25HealthParticleEffects(ref TimeSpan elapsedTime)
        {
             // 1/15/2009: Updated to show only when in FOW view and in Camera view!
            if (!ShapeItem.IsFOWVisible || !InstancedItem.IsInCameraView(ref ((Shape) ShapeItem).InstancedItemData))
                return;

            // Set Position of damage fire
            _damageFirePosition = position; _damageFirePosition.Y += 25;
            ParticlesManager.UpdateParticleSystem(ParticleSystemTypes.FireParticleSystem, ref _damageFirePosition, ref _tmpZero);
        }

        /// <summary>
        /// Triggers the event for 'SceneItemCreated', passing 'This' as the <see cref="DefenseScene"/> instance.
        /// </summary>
        /// <param name="item">The <see cref="SceneItemWithPick"/> instance</param>
        protected override void FireEventHandler_Created(SceneItemWithPick item)
        {
            base.FireEventHandler_Created(this);
        }
       
        // 4/10/2009
        /// <summary>
        /// Captures the event from the base class, and displays a 'Flash' <see cref="ParticleSystem"/> effect.
        /// </summary>
        /// <param name="spawnBulletNumber">SpawnBullet number</param>
        /// <param name="bulletStartPosition"><see cref="Vector3"/> bullet start position</param>
        protected override void ProjectileReleased(int spawnBulletNumber, ref Vector3 bulletStartPosition)
        {
            // Updated to show only when in FOW view and in Camera view!
            if (!ShapeItem.IsFOWVisible || !InstancedItem.IsInCameraView(ref ((Shape) ShapeItem).InstancedItemData) ||
                !AttackOn) return;

            // AA-Guns, do FlashEffect.
            switch (ShapeItem.ItemType)
            {
                case ItemType.sciFiAAGun05:
                case ItemType.sciFiAAGun04:
                case ItemType.sciFiAAGun02:
                case ItemType.sciFiAAGun01:
                    {

                        // Flash Particle effect
                        var tmpZero = Vector3Zero;
                        ParticlesManager.UpdateParticleSystem(ParticleSystemTypes.FlashParticleSystem, ref bulletStartPosition, ref tmpZero);                   


                    } // End If AA-Guns
                    break;
            }
        }

        #region Audio Methods

        // 1/8/2009
        /// <summary>
        /// Plays some shooting sound when a <see cref="DefenseScene"/> turret shoots is <see cref="Projectile"/>.
        /// </summary>
        /// <param name="spawnBullet">SpawnBullet number</param>
        /// <param name="playSound">Play sound?</param>
        protected override void Audio_ShootProjectile(int spawnBullet, bool playSound)
        {
            switch (spawnBullet)
            {
                // if Primary Guns (1/2)
                case 1:
                case 2:
                    // 5/12/2009
                    if (playSound)
                    {
                        if (!StartShootProjectileSound)
                        {
                            AudioManager.Play3D(UniqueKey,PlayableItemAtts.SoundsToPlay.SoundToPlayPrimaryFire, AudioListenerI, AudioEmitterI, true);
                            StartShootProjectileSound = true;
                        }

                    }
                    else
                    {
                        if (StartShootProjectileSound)
                        {
                            AudioManager.Pause(UniqueKey, PlayableItemAtts.SoundsToPlay.SoundToPlayPrimaryFire);
                            StartShootProjectileSound = false;
                        }

                    }

                    break;
                // if Secondary Guns (3/4)
                case 3:
                case 4:
                    // 5/12/2009
                    if (playSound)
                    {
                        if (!StartShootProjectileSound)
                        {
                            AudioManager.Play3D(UniqueKey,PlayableItemAtts.SoundsToPlay.SoundToPlaySecondaryFire, AudioListenerI, AudioEmitterI, true);
                            StartShootProjectileSound = true;
                        }

                    }
                    else
                    {
                        if (StartShootProjectileSound)
                        {
                            AudioManager.Pause(UniqueKey, PlayableItemAtts.SoundsToPlay.SoundToPlaySecondaryFire);
                            StartShootProjectileSound = false;
                        }
                    }

                    break;
                default:
                    break;
            }     
        }

        // 5/12/2009
        /// <summary>
        /// Overrides the base method, to apply the new Emitter changes to the given Cue, for
        /// a given sound.
        /// </summary>
        protected override void UpdateAudioEmitters()
        {
            base.UpdateAudioEmitters();
           
            // Apply emitters to shooting sounds.
            AudioManager.UpdateCues3DEmitters(UniqueKey, PlayableItemAtts.SoundsToPlay.SoundToPlayPrimaryFire, AudioListenerI, AudioEmitterI);
            AudioManager.UpdateCues3DEmitters(UniqueKey, PlayableItemAtts.SoundsToPlay.SoundToPlaySecondaryFire, AudioListenerI, AudioEmitterI);

        }

        #endregion

    }
}
