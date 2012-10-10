#region File Description
//-----------------------------------------------------------------------------
// SciFiTankScene.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using ImageNexus.BenScharbach.TWEngine.AI;
using ImageNexus.BenScharbach.TWEngine.Audio;
using ImageNexus.BenScharbach.TWEngine.Audio.Enums;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.Common;
using ImageNexus.BenScharbach.TWEngine.ForceBehaviors;
using ImageNexus.BenScharbach.TWEngine.ForceBehaviors.Enums;
using ImageNexus.BenScharbach.TWEngine.ForceBehaviors.SteeringBehaviors;
using ImageNexus.BenScharbach.TWEngine.ForceBehaviors.TurretBehaviors;
using ImageNexus.BenScharbach.TWEngine.IFDTiles;
using ImageNexus.BenScharbach.TWEngine.InstancedModels;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Structs;
using ImageNexus.BenScharbach.TWEngine.Interfaces;
using ImageNexus.BenScharbach.TWEngine.ItemTypeAttributes.Structs;
using ImageNexus.BenScharbach.TWEngine.MemoryPool;
using ImageNexus.BenScharbach.TWEngine.MemoryPool.PoolItems;
using ImageNexus.BenScharbach.TWEngine.Particles;
using ImageNexus.BenScharbach.TWEngine.Particles.Enums;
using ImageNexus.BenScharbach.TWEngine.Players;
using ImageNexus.BenScharbach.TWEngine.SceneItems.Enums;
using ImageNexus.BenScharbach.TWEngine.SceneItems.Structs;
using ImageNexus.BenScharbach.TWEngine.Shapes;
using ImageNexus.BenScharbach.TWEngine.Terrain;
using ImageNexus.BenScharbach.TWEngine.VehicleTypes;
using ImageNexus.BenScharbach.TWLate.RTS_FogOfWarInterfaces.FOW;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.SceneItems
{

    ///<summary>
    ///  The <see cref="SciFiTankScene"/> is used for the sci-fi tank item.
    ///</summary>
    public sealed class SciFiTankScene : SceneItemWithPick, IVehicleSceneType, ITurretAttributes
    {
      
        // 11/13/2008 - Position of damage fire particles, when health falls below 50%
        private Vector3 _damageFirePosition = Vector3.Zero;
              
        private Vector3 _tmpZero = Vector3.Zero;

        // 10/9/2008 - Turret Atts Class for Indirect Inheritance
        private TurretAttributes _turretAtts;

        // 10/9/2008 - Ref to TurnTurretAbstractBehavior
        private TurnTurretBehavior _turnTurretAbstractBehavior;
       
        // 7/11/2008 - VehicleSceneType Class for Indirect Inheritance
        private VehicleSceneType _vehicleSceneType;

        // 2/17/2009 - List of _spawnBulletTransforms; ref retrieved in 'SetBulletPosition' method.
        private readonly List<InstancedItemTransform> _spawnBulletTransforms;    
    
        // 2/23/2009 - Ref to SciFiTankScenePoolItem Wrapper class
        ///<summary>
        /// Reference for the <see cref="SciFiTankScenePoolItem"/> instance
        ///</summary>
        public new SciFiTankScenePoolItem PoolItemWrapper;          

        #region Properties

        // 11/11/2008 - Override Delete so we can also delete Transform from InstanceItem.
        /// <summary>
        /// Should this <see cref="SciFiTankScene"/> be deleted?
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
        /// Get or Set the <see cref="SciFiTankShape"/> instance
        ///</summary>
        public new SciFiTankShape ShapeItem
        {
            get
            {
                return (base.ShapeItem as SciFiTankShape);
            }
            set
            {
                ShapeItem = value;
            }

        }

        #region IVehicleSceneType Interface Properties

        ///<summary>
        /// Vehicle wheel radius
        ///</summary>
        public float WheelRadius
        {
            get { return _vehicleSceneType.WheelRadius; }
            set { _vehicleSceneType.WheelRadius = value; }
        }

        #endregion

        #region ITurretAttributes

        ///<summary>
        /// Set or Get the turret's facing direction.
        ///</summary>
        ///<exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not within the required range of -pi to pi.</exception>
        public float TurretFacingDirection
        {
            get {
                return _turretAtts.TurretFacingDirection;
            }
            set 
            {
                // 8/23/2009 - Make sure value given is in Radian measurement.
                const float pi = MathHelper.Pi;
                if (value < -pi || value > pi)
                    throw new ArgumentOutOfRangeException("value", @"Angle must be in Radian measurement; - pi to pi.");

                _turretAtts.TurretFacingDirection = value;
                
                // 6/1/2009
                var sciFiTankShape = ShapeItem; // 4/27/2010
                if (sciFiTankShape == null) return;
                    
                sciFiTankShape.TurretRotation = _turretAtts.TurretFacingDirection;
            }
        }

        ///<summary>
        /// Get or Set the turret's turn speed.
        ///</summary>
        public float TurretTurnSpeed
        {
            get {
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
            get {
                return _turretAtts.TurretDesiredAngle;
            }
            set
            {

                // 8/23/2009 - Make sure value given is in Radian measurement.
                const float pi = MathHelper.Pi;
                if (value < -pi || value > pi)
                    throw new ArgumentOutOfRangeException("value", @"Angle must be in Radian measurement; - pi to pi.");
               
                    
                _turretAtts.TurretDesiredAngle = value;
            }
        }

        #endregion



        #endregion

        // 3/1/2009 - Add ItemGroupToAttack parameter.
        ///<summary>
        /// Constructor, which creates the given <see cref="SciFiTankScene"/> item.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        ///<param name="itemType"><see cref="ItemType"/> to use</param>
        ///<param name="itemGroupToAttack"><see cref="ItemGroupType"/> Enum this item can attack</param>
        ///<param name="initialPosition"><see cref="Vector3"/> initial position</param>
        ///<param name="playerNumber">The player number this item belongs to</param>
        public SciFiTankScene(Game game, ItemType itemType, ItemGroupType itemGroupToAttack, ref Vector3 initialPosition, byte playerNumber)
            : base(game, new SciFiTankShape(game, itemType, playerNumber), ref initialPosition, playerNumber)
        {
            // 3/1/2009 - Save ItemGroupToAttack
            ItemGroupTypeToAttackE = itemGroupToAttack;

            // Add AStarItem
            AStarItemI = new AStarItem(game, this);

            // 3/23/2009 - Add a SceneItemWithPick reference to AIThreadManager
            AIManager.AddAStarItemAI(this);   

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

            if (e.ItemGroupToAttack != null)
                ItemGroupTypeToAttackE = e.ItemGroupToAttack.Value; // 3/6/2009 - Save ItemGroupToAttack

/*#if !XBOX360
            // 6/22/2009 - Set 'PhysX' Vehicle for Tank-1 flag.
            if (ItemType == ItemType.sciFiTank1)
            {
                // Create PhysX Vehicle with Mass of 2000.
                PhysX.PhysXVehicle.CreateVehicle(ref InitPosition, 2000);
            }
#endif*/

            
            // 2/23/2009
            CommonInitilization(e.PlaceItemAt);

            
        }

        // 8/4/2009
        /// <summary>
        /// Allows <see cref="SciFiTankScene"/> to create 'Bot' <see cref="SceneItem"/> helpers; other items, which 
        /// follow and help defend the main sceneItemm.
        /// </summary>
        /// <param name="e"><see cref="ItemCreatedArgs"/> instance</param>
        protected internal override void CreateBotHelpers(ItemCreatedArgs e)
        {
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player thisPlayer;
            TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out thisPlayer);

            // 8/5/2009 - If MP game, then ONLY the HOST can run this section!
            var isNetworkGame = (thisPlayer.NetworkSession != null);
            if (isNetworkGame && !thisPlayer.NetworkSession.IsHost)
                return;

            // 8/4/2009 - Add Bot helpers?
            if (e.ItemType == ItemType.sciFiTank01)
            {   
                // 1st - Create ItemCreateArgs for new Bot helper
                var itemCreatedArgs = new ItemCreatedArgs
                                          {
                    BuildingType = ItemGroupType.Airplanes,
                    ItemType = ItemType.sciFiGunShip02,
                    ItemGroupToAttack = ItemGroupType.Vehicles,
                    IsBotHelper = true,
                    PlaceItemAt = e.PlaceItemAt,
                    LeaderUniqueNumber = (isNetworkGame) ? NetworkItemNumber : SceneItemNumber,
                    LeaderPlayerNumber = PlayerNumber
                };

                // 6/15/2010 - Updated to use new GetPlayer method.
                Player player;
                TemporalWars3DEngine.GetPlayer(PlayerNumber, out player);

                // ADD a pair
                if (player != null) 
                    for (var i = 0; i < 2; i++)
                    {
                        // 2nd - Call Player Classes ItemCreated directly to create sceneitem
                        player.IFDPlacement_ItemCreated(null, itemCreatedArgs);
                    }              
                
            }

            base.CreateBotHelpers(e);
        }

        /// <summary>
        /// The <see cref="CommonInitilization"/> method sets internal tweakable flags
        /// back to there defaults, retrieves the current rotation value, updates the proper
        /// <see cref="IFogOfWar"/> settings if required, and obtains the current <see cref="TWEngine.Terrain"/>
        /// height for the given position.
        /// </summary>
        /// <param name="initialPosition"><see cref="Vector3"/> position to place item</param>
        private void CommonInitilization(Vector3 initialPosition)
        {
            // 1/1/2009 - Get the default Scale value, contain in the ItemType's content pipeline file.
            float useScale;
            var sciFiTankShape = ShapeItem; // 4/27/2010 - Cache
            var instancedItemData = ((Shape) sciFiTankShape).InstancedItemData; // 4/27/2010 - Cache
            InstancedItem.GetScale(ref instancedItemData, out useScale);
            scale = Vector3.Zero;
            scale.X = scale.Y = scale.Z = useScale;
            
            // 12/9/2008 - Apply Default Rotation values to affect the Display only of tanks!
            InstancedItem.ApplyRotationValuesToRootTranform(ref instancedItemData);        
           

            // 2/23/2009 - Reset flags correctly
            IsAlive = true;
            Delete = false;
            sciFiTankShape.ExplodeAnimStarted = false;
            ThePickSelected = false; // 7/12/2009

            // 3/28/2009 - Tell InstanceModel to draw using Normal pieces, and not explosion pieces!
            InstancedItem.UpdateInstanceModelToDrawExplosionPieces(ref instancedItemData, PlayerNumber, false);

            // Speed turret can turn at
            _turretAtts.TurretTurnSpeed = PlayableItemAtts.TurretTurnSpeed;            

            // Set pathNodePosition to Current PathNode Position            
            var tmpPathNodePos = PathNodePosition;
            const int pathNodeStride = TemporalWars3DEngine._pathNodeStride; // 4/27/2010
            tmpPathNodePos.X = (int)(initialPosition.X / pathNodeStride);
            tmpPathNodePos.Y = initialPosition.Y;
            tmpPathNodePos.Z = (int)(initialPosition.Z / pathNodeStride);
            PathNodePosition = tmpPathNodePos;

            LastPosition = initialPosition;

            // Set A* OccupiedBy for Current PathNode
            if (!AStarItem.SetOccupiedByAtCurrentPosition(AStarItemI))
                throw new Exception("Set OccupiedBy At Current Position Failed!");                    


            // 10/14/2008 - Set FogOfWar Radius, based on SceneItem ViewRadius Property
            if (sciFiTankShape.UseFogOfWar)
            {
                const int cScale = TerrainData.cScale; // 4/27/2010
                sciFiTankShape.FogOfWarHeight = (int)ViewRadius / cScale;
                sciFiTankShape.FogOfWarWidth = (int)ViewRadius / cScale;
                
                // 1/14/2009 - Make sure it can be seen immediately!
                sciFiTankShape.IsFOWVisible = true;

                var fogOfWar = (IFogOfWar)GameInstance.Services.GetService(typeof(IFogOfWar));
                if (fogOfWar != null) fogOfWar.UpdateSight = true;
            }            

            // 7/11/2008 - Instantiate Class 
            if (_vehicleSceneType == null)
                _vehicleSceneType = new VehicleSceneType(sciFiTankShape.VehicleShapeType);

            // 10/7/2008 - ForceBehavior
            if (ForceBehaviors == null)
            {
                ForceBehaviors = new ForceBehaviorsCalculator(GameInstance, this);
                ForceBehaviors.Add(BehaviorsEnum.FollowPath);
                ForceBehaviors.Add(BehaviorsEnum.UpdateOrientation);
                _turnTurretAbstractBehavior = (TurnTurretBehavior)ForceBehaviors.Add(BehaviorsEnum.TurnTurret);
                var turnToFaceBehavior = (TurnToFaceBehavior)ForceBehaviors.Add(BehaviorsEnum.TurnToFace);
                turnToFaceBehavior.FacingDirectionOffset = MathHelper.ToRadians(FacingDirectionOffset); // 8/11/2009
                // 1/26/2009 - Add Ref to ForceBehaviorsManager class
                ForceBehaviorsManager.Add(ForceBehaviors);
            }

            // 5/29/2011 - check if astaritem is instantiated
            if (AStarItemI == null)
            {
                // Add AStarItem
                AStarItemI = new AStarItem(TemporalWars3DEngine.GameInstance, this);     
            }

            // 6/1/2009 - Add DefenseBehavior AI to AIThreadManager.
            AIManager.AddDefenseAI(this);           

            // 10/27/2008 - Add Particles Class
            // 8/15/2008 - Turn on use of ParticleSystem Projectiles
            UseProjectiles = true;           

        }

        /// <summary>
        /// Updates the current <see cref="SciFiTankScene"/>, by calling the base.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="time"><see cref="TimeSpan"/> structure for time</param>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> structure for elapsed game sime since last call</param>
        /// <param name="isClientCall">Is this the client-side update in a network game?</param>
        public override void Update(GameTime gameTime, ref TimeSpan time, ref TimeSpan elapsedTime, bool isClientCall)
        {
            base.Update(gameTime, ref time, ref elapsedTime, isClientCall);           

            // 1/30/2009 - Make sure isAlive first.
            if (!IsAlive) return;

            // 3/24/2009 - If artilery unit, then lower turret if moving.
            if (ShapeItem.ItemType != ItemType.sciFiArtilery01) return;

            // Lower turret if SceneItemOwner is moving.
            if (AStarItemI.ItemState == ItemStates.Resting) return;

            if (!ShapeItem.RaiseTurretUp) return;

            ShapeItem.RaiseTurretUp = false;
            _turnTurretAbstractBehavior.ParkTurret = true;
                       
        }

        // 10/16/2008
        /// <summary>
        /// Sets the collision radius.
        /// </summary>
        protected override void SetCollisionRadius()
        {
            CollisionRadius = InstancedItem.GetInstanceItemCollisionRadius(ref ((Shape) ShapeItem).InstancedItemData, PlayableItemAtts.AdjustCollisionRadius);              
            
        }

        
        // 10/23/2008; 1/6/2009: Updated to use new InstanceItem method to get World Position of bone.
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

            // 1/6/2009 -Updated to use new InstanceItem method to get World Position of bone.
            var instancedItemTransform = spawnBulletTransforms[spawnBulletNumber - 1];
            InstancedItem.GetWorldPositionFromBoneTransform(this, ref instancedItemTransform.AbsoluteTransform, out bulletSpawnPosition);
            
        }

        
        // 7/4/2008
        /// <summary>
        /// Will either just reduce health of enemy unit, or shoot projectiles depending
        /// on defense type.
        /// </summary>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> structure</param>    
        protected override void ShootBullets(ref TimeSpan elapsedTime)
        {
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out player);

            if (player == null) return;

            // 12/16/2008 - Is Client side of MP Game?
            if (player.NetworkSession != null && !player.NetworkSession.IsHost)
            {
                // Client game, so Shoot Bullets without Turret Angel Check to stay in Sync!                
                base.ShootBullets(ref elapsedTime);
            }
            else // Do Regular check.
            {
                // Add Projectiles to fire, when Turret is facing within 1 degrees of target!
                if (!IsFacingTargetWithin1Degrees(_turnTurretAbstractBehavior.AngleDifference))
                    return;

                // 3/24/2009 - If artilery unit, then do check to make sure turret up!
                if (ShapeItem.ItemType == ItemType.sciFiArtilery01)
                {
                    if (Math.Abs(ShapeItem.TurretRotationUp) < 60.0)
                        return;
                }

                // Shoot Projectile Bullet; must call Base to check for MP games!
                base.ShootBullets(ref elapsedTime);
                
            }
        }


        /// <summary>
        /// Overrides base method call, to set the 'AngleToTarget' Angle.
        /// </summary>
        public override void AttackGroundOrder()
        {              
            // 1/1/2009
            if (_turnTurretAbstractBehavior != null)
            {
                // Let's set the TurnTurretAbstractBehavior's 'AngleToTarget'. 
                SetTurretAngleToTarget();
            }

            base.AttackGroundOrder();
        }

        // 10/26/2008
        /// <summary>
        /// Starts the <see cref="SciFiTankScene"/> attack order.
        /// </summary>
        public override void AttackOrder()
        {
            // 1/1/2009
            if (_turnTurretAbstractBehavior != null)
            {               
                // Let's set the TurnTurretAbstractBehavior's 'AngleToTarget'.   
                SetTurretAngleToTarget();

                // TODO: Have this abstracted out, and not hard coded.
                // 3/24/2009 - If artilery unit, then deploy turret.
                if (ShapeItem.ItemType == ItemType.sciFiArtilery01)
                {
                    // Only raise when SceneItemOwner is not moving!
                    if (AStarItemI.ItemState == ItemStates.Resting)
                        ShapeItem.RaiseTurretUp = true;                      
                }                
            }           

            base.AttackOrder();
        }   

       

        // 6/2/2009
        /// <summary>
        /// Sets the TurnTurretAbstractBehavior's 'AngleToTarget' to the correct angle.
        /// </summary>
        private void SetTurretAngleToTarget()
        {
            // 7/16/2009 - Copy 'AttackSceneItem' into local cache, to eliminate Thread Sync errors!
            var attackie = AttackSceneItem;

            // 7/16/2009 - Calculate DesiredAngle.
            TurretDesiredAngle = CalculateDesiredAngle(attackie);
            _turnTurretAbstractBehavior.AngleDifference = Math.Abs(TurretDesiredAngle) - Math.Abs(TurretFacingDirection);
        }

        // 7/4/2008
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
            AudioManager.Pause(UniqueKey, Sounds.TankMove);
            AudioManager.Remove(UniqueKey, PlayableItemAtts.SoundsToPlay.SoundToPlayPrimaryFire);
            AudioManager.Remove(UniqueKey, PlayableItemAtts.SoundsToPlay.SoundToPlaySecondaryFire);

#if DEBUG
            // Remove OccupiedBy at our Old Position
            if (!AStarItem.RemoveOccupiedByAtOldPosition(AStarItemI)) 
                Debug.WriteLine("KillSceneItem method: AStar Remove Occupied failed. ");

#else
            AStarItem.RemoveOccupiedByAtOldPosition(AStarItemI);
#endif

            
            // 3/28/2009 - Tell InstanceModel to draw using Explosion Pieces!
            InstancedItem.UpdateInstanceModelToDrawExplosionPieces(ref ((Shape) ShapeItem).InstancedItemData, PlayerNumber, true);
           

            // 3/1/2010 -  Have Thread sleep a few ms.
            Thread.Sleep(1);

            
        }

        // 2/24/2009
        /// <summary>
        /// Returns this instance back into the <see cref="PoolManager"/>, setting 'Active' to false.
        /// </summary>
        /// <param name="isInterfaceDisplayNode">Created by <see cref="IFDTile"/></param>
        public override void ReturnItemToPool(bool isInterfaceDisplayNode)
        {
            // 6/29/2009
            var sciFiTankScenePoolItem = PoolItemWrapper; // 4/27/2010 - Cache
            if (sciFiTankScenePoolItem == null) return;

            if (sciFiTankScenePoolItem.PoolNode != null)
            {
                sciFiTankScenePoolItem.PoolNode.ReturnToPool();
#if DEBUG
                Debug.WriteLine("(ReturnItemToPool) succesfully return pool node for " + SceneItemNumber);
#endif
            }
#if DEBUG
            else
                Debug.WriteLine("(ReturnItemToPool) failed to return pool node for " + SceneItemNumber);
#endif
        }


        // 11/13/2008
        /// <summary>
        /// When <see cref="SciFiTankScene"/> health falls below 50%, this will draw the smoke <see cref="ParticleSystem"/>
        /// on the <see cref="SciFiTankScene"/>.
        /// </summary>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> structure</param>
        protected override void Below50HealthParticleEffects(ref TimeSpan elapsedTime)
        {
            // 1/14/2009: Updated to show only when in FOW view and in Camera view!
            if (!ShapeItem.IsFOWVisible || !InstancedItem.IsInCameraView(ref ((Shape) ShapeItem).InstancedItemData))
                return;

            ParticlesManager.UpdateParticleSystem(ParticleSystemTypes.BlackSmokePlumeParticleSystem, ref position, ref _tmpZero);
        }

        // 11/13/2008
        /// <summary>
        /// When <see cref="SciFiTankScene"/> health falls below 25%, this will draw the fire <see cref="ParticleSystem"/>
        /// on the <see cref="SciFiTankScene"/>.
        /// </summary>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> structure</param>
        protected override void Below25HealthParticleEffects(ref TimeSpan elapsedTime)
        {
            // 1/14/2009: Updated to show only when in FOW view and in Camera view!
            if (!ShapeItem.IsFOWVisible || !InstancedItem.IsInCameraView(ref ((Shape) ShapeItem).InstancedItemData))
                return;

            // Set Position of damage fire
            _damageFirePosition = position; _damageFirePosition.Y += 25;
            ParticlesManager.UpdateParticleSystem(ParticleSystemTypes.FireParticleSystem, ref _damageFirePosition, ref _tmpZero);
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

            // Flash Particle effect
            var tmpZero = Vector3.Zero;
            ParticlesManager.UpdateParticleSystem(ParticleSystemTypes.FlashParticleSystem, ref bulletStartPosition, ref tmpZero);
        }

        // 5/5/2009
        private bool _startTankMoveSound;

        // 5/5/2009
        /// <summary>
        /// When <see cref="SciFiTankScene"/> is moving, will show dust particles, and play proper sounds.
        /// </summary>
        /// <returns>True/False if item is moving.</returns>
        protected override bool ItemIsMovingCheck()
        {
            var result = base.ItemIsMovingCheck();

            // if SceneItemOwner is moving, then show Dust particles and play moving tank sound.
            if (result)
            {
                // Set Position of tank for dust
                // 1/14/2009: Updated to show only when in FOW view and in Camera view!
                if (ShapeItem.IsFOWVisible &&
                    InstancedItem.IsInCameraView(ref ((Shape) ShapeItem).InstancedItemData))
                    ParticlesManager.UpdateParticleSystem(ParticleSystemTypes.DustPlumeParticleSystem, ref position, ref _tmpZero);               

                if (!_startTankMoveSound)
                {
                    AudioManager.Play3D(UniqueKey, Sounds.TankMove, AudioListenerI, AudioEmitterI, true);
                    _startTankMoveSound = true;
                }

                return true;
            }
                           
            // pause sound
            if (_startTankMoveSound)
            {
                AudioManager.Pause(UniqueKey, Sounds.TankMove);
                _startTankMoveSound = false;
            }

            return false;
        }
       

        #region Audio Methods        

        // 1/7/2009
        /// <summary>
        /// Plays some explosion sound when a <see cref="SciFiTankScene"/> is killed!
        /// </summary>
        protected override void Audio_KillSceneItem()
        {
            // 5/12/2009: Updated to use the SoundBanks 'PlayCue3D', rather than 'Play3D'; this is because when calling the 'Play3D' version,
            //            the Items shooting will still have their sounds loop forever, even though they have stopped attacking!!
            AudioManager.PlayCue(Sounds.Exp_MediumGroup);
            //SoundManager.Play3D(SceneItemNumber, SoundBankGroup.Explosions, Sounds.Exp_MediumGroup, AudioListenerI, AudioEmitterI, false);
        }        
        

        // 1/8/2009; 3/27/2009: Updated to get the SoundToPlay from the PlayableAtts instance.
        /// <summary>
        /// Plays some shooting sound when a <see cref="SciFiTankScene"/> shoots is <see cref="Projectile"/>.
        /// </summary>
        /// <param name="playSound">Play sound?</param>
        /// <param name="spawnBullet">The SpawnBullet number.</param>
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
                        //if (!StartShootProjectileSound)
                        {
                            AudioManager.Play3D(UniqueKey, PlayableItemAtts.SoundsToPlay.SoundToPlayPrimaryFire, AudioListenerI, AudioEmitterI, false);
                            StartShootProjectileSound = true;
                        }

                    }
                    else
                    {
                        if (StartShootProjectileSound)
                        {
                            //SoundManager.Pause(SceneItemNumber, PlayableItemAtts.SoundsToPlay.SoundToPlayPrimaryFire);
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
                        //if (!StartShootProjectileSound)
                        {
                            AudioManager.Play3D(UniqueKey, PlayableItemAtts.SoundsToPlay.SoundToPlaySecondaryFire, AudioListenerI, AudioEmitterI, false);
                            StartShootProjectileSound = true;
                        }

                    }
                    else
                    {
                        if (StartShootProjectileSound)
                        {
                            //SoundManager.Pause(SceneItemNumber, PlayableItemAtts.SoundsToPlay.SoundToPlaySecondaryFire);
                            StartShootProjectileSound = false;
                        }

                    }

                    break;
                default:
                    break;

            }                

        }

        // 5/5/2009
        /// <summary>
        /// Plays some pick selected sound when a <see cref="SciFiTankScene"/> is selected.
        /// </summary>
        /// <param name="pickSelected">The value of the <see cref="PickSelected"/>.</param>
        protected override void Audio_PickedSelected(bool pickSelected)
        {           
            // TODO: Play some pick selected sound.
            return;
        }

        // 5/6/2009
        /// <summary>
        /// Overrides the base method, to apply the new Emitter changes to the given Cue, for
        /// a given sound.
        /// </summary>
        protected override void UpdateAudioEmitters()
        {
            base.UpdateAudioEmitters();

            // Now apply new emitter atts to cue.
            AudioManager.UpdateCues3DEmitters(UniqueKey, Sounds.TankMove, AudioListenerI, AudioEmitterI);
            
            // Apply emitters to shooting sounds.
            AudioManager.UpdateCues3DEmitters(UniqueKey, PlayableItemAtts.SoundsToPlay.SoundToPlayPrimaryFire, AudioListenerI, AudioEmitterI);
            AudioManager.UpdateCues3DEmitters(UniqueKey, PlayableItemAtts.SoundsToPlay.SoundToPlaySecondaryFire, AudioListenerI, AudioEmitterI);
        }

        
        #endregion

        #region IVehicleSceneType Interface Wrapper Methods

        ///<summary>
        /// Updates the wheel roll
        ///</summary>
        ///<param name="movement"><see cref="Vector3"/> movement</param>
        ///<param name="inPosition"><see cref="Vector3"/> position</param>
        ///<param name="newPosition"><see cref="Vector3"/> new position</param>
        public void UpdateWheelRoll(ref Vector3 movement, Vector3 inPosition, ref Vector3 newPosition)
        {
            _vehicleSceneType.UpdateWheelRoll(ref movement, inPosition, ref newPosition);
        }

        #endregion

        // 12/16/2008 - Dispose
        ///<summary>
        /// Disposes of unmanaged resources.
        ///</summary>
        ///<param name="finalDispose">Is this final dispose?</param>
        public override void Dispose(bool finalDispose)
        {
            // 12/19/2008
            // Disable _turnTurretAbstractBehavior, otherwise, sometimes the
            // ForceBehavior thread will try to access the '_turretAtts'
            // while it is being nulled out below!
            if (_turnTurretAbstractBehavior != null)
                _turnTurretAbstractBehavior.UseBehavior = false;


            // null refs
            _turretAtts = default(TurretAttributes);
            _turnTurretAbstractBehavior = null;
            _vehicleSceneType = null;

            base.Dispose(finalDispose);
        }
    }
}
