#region File Description
//-----------------------------------------------------------------------------
// SciFiAircraftScene.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using ImageNexus.BenScharbach.TWLate.RTS_FogOfWarInterfaces.FOW;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.SceneItems
{

    ///<summary>
    /// The <see cref="SciFiAircraftScene"/> is used for units which fly around in the
    /// air.
    ///</summary>
    public sealed class SciFiAircraftScene : SceneItemWithPick, ITurretAttributes
    {      

        // 11/13/2008 - Position of damage fire _particles, when health falls below 50%
        private Vector3 _damageFirePosition = Vector3.Zero;

        // 10/27/2008 - Add Dust Particle System       
        private Vector3 _tmpZero = Vector3.Zero;

        // 10/9/2008 - Turret Atts Class for Indirect Inheritance
        private TurretAttributes _turretAtts;

        // 10/9/2008 - Ref to TurnTurretAbstractBehavior
        private TurnTurretBehavior _turnTurretAbstractBehavior;
        // 2/3/2009 - Ref to UpdateOrientationAbstractBehavior
        private UpdateOrientationBehavior _updateOrientationAbstractBehavior;
        // 2/4/2009 - Ref to TurnToFaceAbstractBehavior
        private TurnToFaceBehavior _turnToFaceAbstractBehavior;

        // 2/17/2009 - List of _spawnBulletTransforms; ref retrieved in 'SetBulletPosition' method.
        private readonly List<InstancedItemTransform> _spawnBulletTransforms;

        // 2/26/2009 - Ref to SciFiAircraftScenePoolItem Wrapper class
        ///<summary>
        /// Reference to <see cref="SciFiAircraftScenePoolItem"/> wrapper class.
        ///</summary>
        public new SciFiAircraftScenePoolItem PoolItemWrapper;  

        // 8/5/2009 - BotHelper - Connect to leader attempt.
        private OffsetPursuitBehavior _offsetPursuitAbstractBehavior;
        private CohesionBehavior _cohesionAbstract;
        private SeparationBehavior _separationAbstract;
        private WanderBehavior _wanderAbstractBehavior;
        private bool _tryToConnectToLeader;
        private int _leaderUniqueNumber;
       

        #region Properties

        // 11/11/2008 - Override Delete so we can also delete Transform from InstanceItem.
        /// <summary>
        /// Should this <see cref="SciFiAircraftScene"/> be deleted?
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
        ///  Set or Get reference to the <see cref="SciFiAircraftShape"/> instance.
        ///</summary>
        public new SciFiAircraftShape ShapeItem
        {
            get
            {
                return (base.ShapeItem as SciFiAircraftShape);
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
                // 4/27/20010 - Make sure value given is in Radian measurement.
                const float pi = MathHelper.Pi;
                if (value < -pi || value > pi)
                    throw new ArgumentOutOfRangeException("value", @"The current value was not within the required range of -pi to pi!");

                
                _turretAtts.TurretFacingDirection = value;

                // 6/1/2009; 4/27/2010: cache
                var shapeItem = ShapeItem;
                if (shapeItem == null) return;
                    
                shapeItem.TurretRotation = _turretAtts.TurretFacingDirection;
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
                // 4/27/2010 - Make sure value given is in Radian measurement.
                const float pi = MathHelper.Pi;
                if (value < -pi || value > pi)
                    throw new ArgumentOutOfRangeException("value", @"The current value was not within the required range of -pi to pi!");

                _turretAtts.TurretDesiredAngle = value;
            }
        }

        #endregion



        #endregion


        ///<summary>
        /// Constructor, which creates the given <see cref="SciFiAircraftScene"/> item.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        ///<param name="itemType"><see cref="ItemType"/> to use</param>
        ///<param name="itemGroupToAttack"><see cref="ItemGroupType"/> Enum this item can attack</param>
        ///<param name="initialPosition"><see cref="Vector3"/> initial position</param>
        ///<param name="playerNumber">The player number this item belongs to</param>
        public SciFiAircraftScene(Game game, ItemType itemType, ItemGroupType itemGroupToAttack, ref Vector3 initialPosition, byte playerNumber)
            : base(game, new SciFiAircraftShape(game, itemType, playerNumber), ref initialPosition, playerNumber)
        {
            
            // Add AStarItem
            AStarItemI = new AStarItem(game, this);

            // 3/23/2009 - Add AstarItem to AIThreadManager
            AIManager.AddAStarItemAI(this);
           
            // 3/6/2009 - Save ItemGroupToAttack
            ItemGroupTypeToAttackE = itemGroupToAttack;

            // 5/13/2009 - Init the List<InstancedITemTransform>.
            _spawnBulletTransforms = new List<InstancedItemTransform>(4);

        }       

        // 1/30/2009
        /// <summary>
        /// Populates the <see cref="PlayableItemTypeAttributes"/> structure with the common attributes
        /// used by the given ItemType.
        /// </summary>
        /// <param name="e"><see cref="ItemCreatedArgs"/> instance</param>
        /// <param name="isFinalPosition">Is this final position?</param>
        public override void LoadPlayableAttributesForItem(ItemCreatedArgs e, bool isFinalPosition)
        {
            base.LoadPlayableAttributesForItem(e, isFinalPosition);

            if (e.ItemGroupToAttack != null)
                ItemGroupTypeToAttackE = e.ItemGroupToAttack.Value; // 3/6/2009 - Save ItemGroupToAttack

            // 2/26/2009
            CommonInitilization(e.PlaceItemAt);

            // 8/4/2009 - Check if BotHelper.
            CreateBotHelper(e);
        }

        // 8/9/2009
        /// <summary>
        /// Method helper, which creates a given bot helper for current <see cref="SciFiAircraftScene"/>.
        /// </summary>
        /// <param name="e"><see cref="ItemCreatedArgs"/> instance</param>
        private void CreateBotHelper(ItemCreatedArgs e)
        {
            if (!e.IsBotHelper) return;

            // 8/9/2009 - Make sure not added twice.
            if (IsBotHelper)
                return;

            // Set Internal flag
            IsBotHelper = true;
            ItemState = ItemStates.BotHelper;

            // Set scale to be smaller for this SceneItemOwner.
            Vector3.Multiply(ref scale, 0.40f, out scale);

            //
            // Create Behaviors for Bot
            //

            // OffsetPursuit
            _offsetPursuitAbstractBehavior = (OffsetPursuitBehavior)ForceBehaviors.Add(BehaviorsEnum.OffsetPursuit);
            _offsetPursuitAbstractBehavior.OffsetBy = new Vector3(0, 0, -100);

            // Wander
            _wanderAbstractBehavior = (WanderBehavior)ForceBehaviors.Add(BehaviorsEnum.Wander);
            _wanderAbstractBehavior.BehaviorWeight = 0.10f;
            _wanderAbstractBehavior.WanderJitter = 125;
            _wanderAbstractBehavior.WanderRadius = 6;
            _wanderAbstractBehavior.WanderDistance = 4;

            // Separation & Coehsion
            _cohesionAbstract = (CohesionBehavior)ForceBehaviors.Add(BehaviorsEnum.Cohesion);
            _separationAbstract = (SeparationBehavior)ForceBehaviors.Add(BehaviorsEnum.Separation);
            _separationAbstract.BehaviorWeight = 15.0f;
            _cohesionAbstract.BehaviorWeight = 0.1f;
            _cohesionAbstract.UseAircraftUpdate = true;
            _separationAbstract.UseAircraftUpdate = true;
            ForceBehaviors.PopulateNeighborsAir = true; // Set to true, otherwise Cohesion & Separation will not work!

            // Remove the FollowPath from this Bot
            ForceBehaviors.Remove(BehaviorsEnum.FollowPath);
            _turnToFaceAbstractBehavior.FaceAttackie = true;    
                
            // Set ForceBehavior manager to use Version-2 of GetNeighbors for BOTS!
            ForceBehaviors.UseGetNeighborsVersion2 = true;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(e.LeaderPlayerNumber, out player);

            // Set bot's leader      
            SceneItemWithPick selectableItem; // 5/20/2012
            if (Player.GetSelectableItem(player, e.LeaderUniqueNumber, out selectableItem))
            {
                ForceBehaviors.TargetItem1 = selectableItem;
                return;
            }


            // Leader was not found yet, so set to check in 'Update' method until found!
            // This occurs when the Leader has not been created yet, due to being out-of-sequence
            // in the network game.
            _tryToConnectToLeader = true;
            _leaderUniqueNumber = e.LeaderUniqueNumber;
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
            InstancedItem.GetScale(ref ((Shape) ShapeItem).InstancedItemData, out useScale);
            scale = Vector3.Zero;
            scale.X = scale.Y = scale.Z = useScale;
            
            // 12/9/2008 - Apply Default Rotation values to affect the Display only of tanks!
            InstancedItem.ApplyRotationValuesToRootTranform(ref ((Shape) ShapeItem).InstancedItemData);    
            

            // 2/23/2009 - Reset flags correctly
            IsAlive = true;
            Delete = false;
            ShapeItem.ExplodeAnimStarted = false;
            ThePickSelected = false; // 7/12/2009

            // 10/13/2012 - Obsolete.
            // 3/28/2009 - Tell InstanceModel to draw using Normal pieces, and not explosion pieces!
            //InstancedItem.UpdateInstanceModelToDrawExplosionPieces(ref ((Shape) ShapeItem).InstancedItemData, PlayerNumber, false);

            // Set pathNodePosition to Current PathNode Position            
            var tmpPathNodePos = PathNodePosition;
            tmpPathNodePos.X = (int)(initialPosition.X / TemporalWars3DEngine._pathNodeStride);
            tmpPathNodePos.Y = initialPosition.Y;
            tmpPathNodePos.Z = (int)(initialPosition.Z / TemporalWars3DEngine._pathNodeStride);
            PathNodePosition = tmpPathNodePos;

            LastPosition = initialPosition;
            // Speed turret can turn at
            _turretAtts.TurretTurnSpeed = PlayableItemAtts.TurretTurnSpeed;

            // Set A* OccupiedBy for Current PathNode
            if (!AStarItem.SetOccupiedByAtCurrentPosition(AStarItemI))
                throw new Exception("Set OccupiedBy At Current Position Failed!");                    


            // 10/14/2008 - Set FogOfWar Radius, based on SceneItem ViewRadius Property
            if (ShapeItem.UseFogOfWar)
            {
                ShapeItem.FogOfWarHeight = (int)ViewRadius / TerrainData.cScale;
                ShapeItem.FogOfWarWidth = (int)ViewRadius / TerrainData.cScale;
                
                // 1/14/2009 - Make sure it can be seen immediately!
                ShapeItem.IsFOWVisible = true;

                var fogOfWar = (IFogOfWar)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(IFogOfWar));
                if (fogOfWar != null) fogOfWar.UpdateSight = true;
            }            

            // 10/7/2008 - ForceBehavior
            if (ForceBehaviors == null)
            {
                ForceBehaviors = new ForceBehaviorsCalculator(GameInstance, this);
                ForceBehaviors.Add(BehaviorsEnum.FollowPath);
                _updateOrientationAbstractBehavior = (UpdateOrientationBehavior)ForceBehaviors.Add(BehaviorsEnum.UpdateOrientation);
                _turnToFaceAbstractBehavior = (TurnToFaceBehavior)ForceBehaviors.Add(BehaviorsEnum.TurnToFace);
                
            }

            
            if (_updateOrientationAbstractBehavior != null)
            {
                _updateOrientationAbstractBehavior.UseAircraftUpdate = true;
                _updateOrientationAbstractBehavior.DesiredAircraftHeight = 225.0f;

                // 11/28/2009 - Set Animation options for aircraft.
                _updateOrientationAbstractBehavior.UseAircraftUpDownAnimation = PlayableItemAtts.UseAircraftUpDownAnimation;
                _updateOrientationAbstractBehavior.UseRockLeftRightAnimation = PlayableItemAtts.UseRockLeftRightAnimation;
            }

            if (_turnToFaceAbstractBehavior != null)
            {
                _turnToFaceAbstractBehavior.FaceAttackie = PlayableItemAtts.FaceAttackie; // 4/1/2009
                _turnToFaceAbstractBehavior.FacingDirectionOffset = MathHelper.ToRadians(FacingDirectionOffset); // 8/11/2009  
            }

            // 5/29/2011 - check if astaritem is instantiated
            if (AStarItemI == null)
            {
                // Add AStarItem
                AStarItemI = new AStarItem(TemporalWars3DEngine.GameInstance, this);
            }
           
            // 6/1/2009 - Add DefenseBehavior AI to AIThreadManager.
            AIManager.AddDefenseAI(this);    

            // 2/3/2009 - Does aircraft have a turret bone.
            if (PlayableItemAtts.HasTurret)
                _turnTurretAbstractBehavior = (TurnTurretBehavior)ForceBehaviors.Add(BehaviorsEnum.TurnTurret);
           
            // 1/26/2009 - Add Ref to ForceBehaviorsManager class
            ForceBehaviorsManager.Add(ForceBehaviors);
           

            // 10/27/2008 - Add Particles Class
            // 8/15/2008 - Turn on use of ParticleSystem Projectiles
            UseProjectiles = true;

            // 5/6/2009 - Play Chopper Idle sound, if Helicopter.
            if (ShapeItem.ItemType == ItemType.sciFiHeli01 || ShapeItem.ItemType == ItemType.sciFiHeli02)
                AudioManager.Play3D(UniqueKey, Sounds.ChopperIdleLoop1, AudioListenerI, AudioEmitterI, false); // SceneItemNumber
            

        }


        /// <summary>
        /// Updates the current <see cref="SciFiAircraftScene"/>, by initially calling the base, then calling
        /// the <see cref="UpdateCircleOrientation"/> method to update aircraft which circle. 
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
          
            // 4/1/2009 - Check Circling SceneItemOwner Update
            UpdateCircleOrientation(ref elapsedTime, this);

            // 8/5/2009 - When set, will keep trying to locate the Bot's Leader.
            if (_tryToConnectToLeader && isClientCall)
            {
                // try to set bot's leader
                
                // 6/15/2010 - Updated to use new GetPlayer method.
                Player player;
                TemporalWars3DEngine.GetPlayer(PlayerNumber, out player);

                SceneItemWithPick selectableItem; // 5/20/2012
                if (Player.GetSelectableItem(player, _leaderUniqueNumber, out selectableItem))
                {
                    ForceBehaviors.TargetItem1 = selectableItem;
                    _tryToConnectToLeader = false;
                }
            }

            // 11/5/2009 - If Bot, then check if Leader is dead!?
            if (!IsBotHelper || ForceBehaviors.TargetItem1.IsAlive) return;

            // leader dead, so KILL this BOT!
            CurrentHealth = 0;
            StartKillSceneItem(ref elapsedTime, PlayerNumber);
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
        // 2/4/2009: Updated to include new 'SpawnBulletNumber' parameter.
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

            if (player.NetworkSession != null && !player.NetworkSession.IsHost)
            {
                // Client game, so Shoot Bullets without Turret Angel Check to stay in Sync!                
                base.ShootBullets(ref elapsedTime);
            }
            else // Do Regular check.
            {
                // 2/3/2009
                // Does aircraft have a turret bone?
                if (PlayableItemAtts.HasTurret)
                {
                    // Add Projectiles to fire, when Turret is facing within 1 degrees of target!
                    if (!IsFacingTargetWithin1Degrees(_turnTurretAbstractBehavior.AngleDifference))
                        return;
                    
                    // Shoot Projectile Bullet; must call Base to check for MP games!
                    base.ShootBullets(ref elapsedTime);
                    
                }
                else
                {
                    // Add Projectiles to fire, when Turret is facing within 1 degrees of target!
                    //if (Math.Abs(_turnToFaceAbstractBehavior.AngleDifference) <= _targetDifference)
                    if (!IsFacingTargetWithin1Degrees(_turnToFaceAbstractBehavior.AngleDifference))
                        return;

                    // Shoot Projectile Bullet; must call Base to check for MP games!
                    base.ShootBullets(ref elapsedTime);

                }
            }
        }

        // 10/26/2008
        
        
        /// <summary>
        /// Overrides base method call, to set the 'AngleToTarget' Angle.
        /// </summary>
        public override void AttackGroundOrder()
        {
            // 2/4/09
            // If aircraft has a turret, then turn it.           
            if (PlayableItemAtts.HasTurret)
            {
                // If aircraft has a turret, then turn it.    
                SetTurretAngleToTarget();
            }
           

            base.AttackGroundOrder();
        }

        // 10/26/2008
        /// <summary>
        /// Starts the <see cref="SciFiAircraftScene"/> attack order.
        /// </summary>
        public override void AttackOrder()
        {
            // 2/4/09
            // If aircraft has a turret, then turn it        
            if (PlayableItemAtts.HasTurret)
            {      
                // Let's set the TurnTurretAbstractBehavior's 'AngleToTarget'.   
                SetTurretAngleToTarget();         
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

            // 4/27/2010 - Cache
            var sciFiAircraftShape = ShapeItem;
            if (sciFiAircraftShape == null) return;

            // 4/27/2010 - Cache
            var itemType = sciFiAircraftShape.ItemType;

            // 5/12/2009 -
            if (itemType == ItemType.sciFiHeli01 || itemType == ItemType.sciFiHeli02)
            {
                // Remove or Pause any sounds for given SceneItemOwner
                AudioManager.Stop(UniqueKey, Sounds.ChopperIdleLoop1);
               
            }

            // 11/28/2009 - Remove 

            // 8/16/2009 - Remove any BotHelper behaviors when BotHelper.
            var steeringBehaviors = ForceBehaviors; // 4/27/2010
            if (IsBotHelper && steeringBehaviors != null)
            {
                steeringBehaviors.Remove(BehaviorsEnum.Wander);
                steeringBehaviors.Remove(BehaviorsEnum.Cohesion);
                steeringBehaviors.Remove(BehaviorsEnum.OffsetPursuit);
                steeringBehaviors.Remove(BehaviorsEnum.Separation);
                _wanderAbstractBehavior = null;
                _offsetPursuitAbstractBehavior = null;
                _cohesionAbstract = null;
                _separationAbstract = null;
                IsBotHelper = false;
                ItemState = ItemStates.Resting; // 11/27/2009
                steeringBehaviors.Add(BehaviorsEnum.FollowPath); // 11/27/2009
                steeringBehaviors.UseGetNeighborsVersion2 = false; // 11/28/2009 - Reset to default.
            }

            AudioManager.Remove(UniqueKey, PlayableItemAtts.SoundsToPlay.SoundToPlayPrimaryFire);
            AudioManager.Remove(UniqueKey, PlayableItemAtts.SoundsToPlay.SoundToPlaySecondaryFire);

            // Remove OccupiedBy at our Old Position
            AStarItem.RemoveOccupiedByAtOldPosition(AStarItemI);

            // 10/13/2012 - Obsolete.
            // 3/28/2009 - Tell InstanceModel to draw using Explosion Pieces!
            //InstancedItem.UpdateInstanceModelToDrawExplosionPieces(ref ((Shape) sciFiAircraftShape).InstancedItemData, PlayerNumber, true);

            // 10/13/2012 - Draw Explosion smoke
            var currentPosition = ShapeItem.World.Translation;
            var lastProjectileVelocity = ShapeItem.LastProjectileVelocity;
            ParticlesManager.DoParticles_MediumExplosion(ref currentPosition, ref lastProjectileVelocity);
        }

        // 2/26/2009
        /// <summary>
        /// Returns this instance back into the <see cref="PoolManager"/>, setting 'Active' to false.
        /// </summary>
        /// <param name="isInterfaceDisplayNode">Created by <see cref="IFDTile"/></param>
        public override void ReturnItemToPool(bool isInterfaceDisplayNode)
        {
            // 6/29/2009
            var sciFiAircraftScenePoolItem = PoolItemWrapper; // 4/27/2010 - Cache
            if (sciFiAircraftScenePoolItem == null) return;

            if (sciFiAircraftScenePoolItem.PoolNode != null) 
                sciFiAircraftScenePoolItem.PoolNode.ReturnToPool();
            else
                Debug.WriteLine("(ReturnItemToPool) failed to return pool node.");
        }


        // 11/13/2008
        /// <summary>
        /// When <see cref="SciFiAircraftScene"/>. health falls below 50%, this will draw the smoke <see cref="ParticleSystem"/>
        /// on the <see cref="SciFiAircraftScene"/>.
        /// </summary>
        /// <param name="elapsedTime">Elapsed game time given as <see cref="TimeSpan"/>.</param>
        protected override void Below50HealthParticleEffects(ref TimeSpan elapsedTime)
        {
            // 1/14/2009: Updated to show only when in FOW view and in Camera view!
            if (!ShapeItem.IsFOWVisible || !InstancedItem.IsInCameraView(ref ((Shape) ShapeItem).InstancedItemData))
                return;

            ParticlesManager.UpdateParticleSystem(ParticleSystemTypes.BlackSmokePlumeParticleSystem, ref position, ref _tmpZero);
        }

        // 11/13/2008
        /// <summary>
        /// When <see cref="SciFiAircraftScene"/>. health falls below 25%, this will draw the fire <see cref="ParticleSystem"/>
        /// on the <see cref="SciFiAircraftScene"/>.
        /// </summary>
        /// <param name="elapsedTime">Elapsed game time given as <see cref="TimeSpan"/>.</param>
        protected override void Below25HealthParticleEffects(ref TimeSpan elapsedTime)
        {
            // 1/14/2009: Updated to show only when in FOW view and in Camera view!
            if (!ShapeItem.IsFOWVisible || !InstancedItem.IsInCameraView(ref ((Shape) ShapeItem).InstancedItemData)) return;

            // Set Position of damage fire
            _damageFirePosition = position; _damageFirePosition.Y += 25;
            ParticlesManager.UpdateParticleSystem(ParticleSystemTypes.FireParticleSystem, ref _damageFirePosition, ref _tmpZero);
        }

        // 1/7/2009
        /// <summary>
        /// Plays some explosion sound when a <see cref="SciFiAircraftScene"/> is killed!
        /// </summary>
        protected override void Audio_KillSceneItem()
        {
            // 5/12/2009: Updated to use the SoundBanks 'PlayCue3D', rather than 'Play3D'; this is because when calling the 'Play3D' version,
            //            the Items shooting will still have their sounds loop forever, even though they have stopped attacking!!
            AudioManager.PlayCue(Sounds.Exp_MediumGroup);
            //SoundManager.Play3D(SceneItemNumber, SoundBankGroup.Explosions, Sounds.Exp_MediumGroup, AudioListenerI, AudioEmitterI, false);
        }

        // 1/8/2009
        /// <summary>
        /// Plays some shooting sound when a <see cref="SciFiAircraftScene"/> shoots is <see cref="Projectile"/>.
        /// </summary>
        /// <param name="spawnBullet">The SpawnBullet number</param>
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
        /// Plays some pick selected sound when a <see cref="SciFiAircraftScene"/> is selected.
        /// </summary>
        /// <param name="thePickSelected">The value of the <see cref="PickSelected"/>.</param>
        protected override void Audio_PickedSelected(bool thePickSelected)
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
            AudioManager.UpdateCues3DEmitters(UniqueKey, Sounds.ChopperIdleLoop1, AudioListenerI, AudioEmitterI);

            // Apply emitters to shooting sounds.
            AudioManager.UpdateCues3DEmitters(UniqueKey, PlayableItemAtts.SoundsToPlay.SoundToPlayPrimaryFire, AudioListenerI, AudioEmitterI);
            AudioManager.UpdateCues3DEmitters(UniqueKey, PlayableItemAtts.SoundsToPlay.SoundToPlaySecondaryFire, AudioListenerI, AudioEmitterI);
        }

        // 11/28/2009
        private Vector3 _torchEngineVelocity1 = Vector3.Down;
        private Vector3 _torchEngineVelocity2 = Vector3.Down;

        // 11/28/2009
        /// <summary>
        /// Does Particle Effects for the current <see cref="SciFiAircraftScene"/>.
        /// </summary>
        protected override void DoParticlesCheck()
        {
            // Do Torch particles for each GunShip's engine.
            if (ShapeItem.ItemType != ItemType.sciFiGunShip02 && ShapeItem.ItemType != ItemType.sciFiBomber07) return;

            // Create rotation transform
            Matrix adjTransform;
            Matrix.CreateRotationY(MathHelper.ToRadians(90), out adjTransform);

            // Get SpawnEngine-1
            Matrix tmpMatrix;
            Vector3 spawnEngine1;
            InstancedItem.GetInstanceItemAbsoluteBoneTransform(ref ((Shape)ShapeItem).InstancedItemData, "propLeft", out tmpMatrix);
            InstancedItem.GetWorldPositionFromBoneTransform(this, ref adjTransform, ref tmpMatrix, out spawnEngine1);

            // Get SpawnEngine-2
            Vector3 spawnEngine2;
            InstancedItem.GetInstanceItemAbsoluteBoneTransform(ref ((Shape)ShapeItem).InstancedItemData, "propRight", out tmpMatrix);
            InstancedItem.GetWorldPositionFromBoneTransform(this, ref adjTransform, ref tmpMatrix, out spawnEngine2);

            // Torch in Engines
            ParticlesManager.UpdateParticleSystem(ParticleSystemTypes.GunshipParticleSystem, ref spawnEngine1, ref _torchEngineVelocity1);
            ParticlesManager.UpdateParticleSystem(ParticleSystemTypes.GunshipParticleSystem, ref spawnEngine2, ref _torchEngineVelocity2);
        }

        #region Circling AI

        // 4/1/2009       
        Vector3 _currentIdleCirclePosition = Vector3.Zero;
        float _rotAngle; 
        bool _startCircling; 

        ///<summary>
        /// The current <see cref="SciFiAircraftScene"/> circle state.
        ///</summary>
        public enum CircleState
        {
            ///<summary>
            /// No circle state applied
            ///</summary>
            None,
            ///<summary>
            /// <see cref="SciFiAircraftScene"/> is idle circling.
            ///</summary>
            IdleCircling,
            ///<summary>
            /// <see cref="SciFiAircraftScene"/> is attack circling.
            ///</summary>
            AttackCircling
        }
        internal CircleState CircleState0 = CircleState.None;
      

        // 4/1/2009
        /// <summary>
        /// Updates the orientation, so <see cref="SciFiAircraftScene"/> will circle either its own position, or 
        /// the attackies Position.
        /// </summary>
        /// <param name="elapsedtime"><see cref="TimeSpan"/> structure as elapsed time</param>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        private void UpdateCircleOrientation(ref TimeSpan elapsedtime, SceneItemWithPick item)
        {
            // 7/16/2009 - Copy 'AttackSceneItem' into local cache, to eliminate Thread Sync errors!
            var attackie = item.AttackSceneItem;
            float circleRadius = 0;

            // 4/1/2009 - If 'AircraftMustCircle', then make aircraft circle its current Position, or attack Position.
            if (item.PlayableItemAtts.AircraftMustCircle)
                if (item.AttackOn && attackie != null)
                {
                    // if was just IdleCircling, then set to none.
                    if (CircleState0 == CircleState.IdleCircling)
                    {
                        // reset FacingDirectionOffset
                        item.FacingDirectionOffset = item.PlayableItemAtts.FacingDirectionOffset;
                        _startCircling = false;                        
                        CircleState0 = CircleState.None;
                    }

                    // check if aircraft within attacking distance
                    var tmpAttackPos = attackie.Position;                                     

                    // 7/16/2009  
                    // 1st - Calculate distance between current vector and center vector. 
                    float distance;
                    item.CalculateDistanceToSceneItem(attackie, out distance);

                    // 2nd - Check if within attacking distance, and start attack if TRUE.
                    if (Math.Abs(distance - item.AttackRadius) < 50 || (distance < item.AttackRadius))
                    {
                        var direction = Vector3.Zero;
                        var tmpPosition = item.Position;
                        if (!_startCircling)
                        {
                            // 7/16/2009
                            // get current angle of SceneItemOwner with attack item 
                            item.CalculateDesiredAngle(attackie, out _rotAngle);

                            // set FacingDirectionOffset to force gunship to face the direction its moving, rather than facing
                            // attack item; this is because the flag 'FaceAttackie' is set to True.
                            const float deg90AsRad = MathHelper.PiOver4; // 6/28/2010
                            item.FacingDirectionOffset = deg90AsRad; // 6/28/2010 - Updated to be in radian format.

                            // set proper circleRadius
                            circleRadius = Math.Abs(distance - item.AttackRadius) < 10 ? item.AttackRadius : distance;

                            _startCircling = true;
                            CircleState0 = CircleState.AttackCircling;
                        }

                        // create rotation      
                        _rotAngle += 0.5f * (float)elapsedtime.TotalSeconds;
                        _rotAngle = (_rotAngle > 360) ? 0 : _rotAngle;

                        // Get direction of angle                        
                        direction.X = (float)Math.Cos(_rotAngle);
                        direction.Z = (float)Math.Sin(_rotAngle);

                        // Calculate the circleRadius
                        if (circleRadius < item.AttackRadius)
                            circleRadius += 100f * (float)elapsedtime.TotalSeconds;
                        else
                            circleRadius = item.AttackRadius;

                        // calc new vector from attackItem's Position, using direction * circleRadius                        
                        Vector3.Multiply(ref direction, circleRadius, out direction);
                        Vector3 tmpNewPos;
                        Vector3.Add(ref tmpAttackPos, ref direction, out tmpNewPos);
                        tmpPosition.X = tmpNewPos.X; tmpPosition.Z = tmpNewPos.Z;
                        item.Position = tmpPosition;

                    }

                }
                else
                {
                    // if was just attacking, then set to idle.
                    if (CircleState0 == CircleState.AttackCircling)
                    {
                        // reset FacingDirectionOffset
                        item.FacingDirectionOffset = item.PlayableItemAtts.FacingDirectionOffset;
                        _startCircling = false;
                        CircleState0 = CircleState.None;
                    }

                    // if ItemState is resting, then see if plane can IdleCircle.
                    if (item.ItemState == ItemStates.Resting)
                    {
                        switch (CircleState0)
                        {
                            case CircleState.None:
                                {
                                    // 7/16/2009
                                    // 1st - Calculate distance between current Position and goal Position. 
                                    float distance;
                                    var otherPosition = item.AStarItemI.GoalPosition;
                                    item.CalculateDistanceToPosition(ref otherPosition, out distance);

                                    // 2nd - Check if within goal distance, before starting to circle.
                                    if (distance < 350)
                                    {
                                        var tmpPosition = item.Position;
                                        if (!_startCircling)
                                        {
                                            _currentIdleCirclePosition = item.AStarItemI.GoalPosition;

                                            // get current angle of SceneItemOwner with SceneItemOwner's goal Position    
                                            Vector3 direction;
                                            Vector3.Subtract(ref tmpPosition, ref _currentIdleCirclePosition, out direction);
                                            _rotAngle = (float)Math.Atan2(direction.Z, direction.X);

                                            // set FacingDirectionOffset to force gunship to face the direction its moving, rather than facing
                                            // attack item; this is because the flag 'FaceAttackie' is set to True.
                                            const float deg90AsRad = MathHelper.PiOver4; // 6/28/2010
                                            item.FacingDirectionOffset = deg90AsRad; // 6/28/2010 - Updated to be in radian format.
                                            _startCircling = true;
                                            CircleState0 = CircleState.IdleCircling;
                                        }

                                    } // Is with GoalDistance
                                }
                                break;
                            case CircleState.IdleCircling:
                                {
                                    var tmpPosition = item.Position;

                                    // create rotation      
                                    _rotAngle += 0.5f * (float)elapsedtime.TotalSeconds;
                                    _rotAngle = (_rotAngle > 360) ? 0 : _rotAngle;

                                    // Get direction of angle      
                                    var direction = Vector3.Zero;
                                    direction.X = (float)Math.Cos(_rotAngle);
                                    direction.Z = (float)Math.Sin(_rotAngle);

                                    // Calculate the circleRadius
                                    if (circleRadius < item.AttackRadius)
                                        circleRadius += 100f * (float)elapsedtime.TotalSeconds;

                                    // calc new vector from goalPosition, using direction * circleRadius                        
                                    Vector3.Multiply(ref direction, circleRadius, out direction);
                                    Vector3 tmpNewPos;
                                    Vector3.Add(ref _currentIdleCirclePosition, ref direction, out tmpNewPos);
                                    tmpPosition.X = tmpNewPos.X; tmpPosition.Z = tmpNewPos.Z;
                                    item.Position = tmpPosition;

                                }
                                break;
                        }

                    }
                    else // SceneItemOwner started to move.
                    {
                        // if was just IdleCircling, then set to none.
                        if (CircleState0 == CircleState.IdleCircling)
                        {
                            // reset FacingDirectionOffset
                            item.FacingDirectionOffset = item.PlayableItemAtts.FacingDirectionOffset;
                            _startCircling = false;                           
                            CircleState0 = CircleState.None;
                        }

                    }
                }
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
            _turnToFaceAbstractBehavior = null;
            _updateOrientationAbstractBehavior = null;

            base.Dispose(finalDispose);
        }
    }
}
