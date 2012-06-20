#region File Description
//-----------------------------------------------------------------------------
// PlayableItemTypeAttributes.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using AStarInterfaces.AStarAlgorithm.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TWEngine.IFDTiles;
using TWEngine.InstancedModels.Enums;
using TWEngine.Particles.Enums;
using TWEngine.SceneItems;
using TWEngine.Shapes;

namespace TWEngine.ItemTypeAttributes.Structs
{
#pragma warning disable 1587
    ///<summary>
    /// The <see cref="PlayableItemTypeAttributes"/> structure, stores ALL the specific 
    /// attributes for a given <see cref="ItemType"/> item in the game world.
    ///</summary>
#pragma warning restore 1587
#if !XBOX360
    [Serializable] 
#endif
    public struct PlayableItemTypeAttributes
    {
        ///<summary>
        /// The <see cref="ItemType"/> Enum these <see cref="PlayableItemTypeAttributes"/> are for.
        ///</summary>
        public ItemType ItemType;
        ///<summary>
        /// The <see cref="ItemGroupType"/> Enum the this item belongs to; for example, 'Buildings', 'Vehicles', 'Defense', 'Aircraft'.
        ///</summary>
        public ItemGroupType ItemGroupType; // 10/5/2009 - 
        ///<summary>
        /// The <see cref="ItemGroupType"/> Enum this item should attack; for example, 'Buildings', 'Vehicles', 'Defense', 'Aircraft'.
        ///</summary>
        public ItemGroupType? ItemGroupToAttack; // 11/6/2009 - 
        ///<summary>
        /// <see cref="ItemGroupType"/> Enum <see cref="IFDTileSubGroupControl"/> tab this <see cref="SceneItem"/> 
        /// owner belongs to; for example, 'Buildings', 'Vehicles', 'Defense', 'Aircraft'.
        ///</summary>
        public ItemGroupType? ProductionType; // 11/6/2009 - 
        ///<summary>
        /// <see cref="SceneItem"/> owner spawns at exact <see cref="BuildingScene"/> position; 
        /// however, you can offset by some amount if needed (Like for aircraft).
        ///</summary>
        public Vector3 ItemSpawnOffset; // 
        ///<summary>
        /// <see cref="SceneItem"/> owner will move to this marker offset position; 
        /// indirectly, this will set the 'ItemToPosition2' in the <see cref="Shape"/> class.
        ///</summary>
        public Vector3 ItemMarkerOffset; // 
        ///<summary>
        ///  Draw <see cref="IStatusBar"/>?
        ///</summary>
        public bool DrawStatusBar;  //
        ///<summary>
        /// <see cref="IStatusBar"/> offset position
        ///</summary>
        public Vector2 StatusBarOffsetPosition2D; // 
        ///<summary>
        /// Correction offset, if necessary, for given <see cref="ItemType"/>.
        ///</summary>
        public float FacingDirectionOffset; // 
        ///<summary>
        /// Speed <see cref="SceneItem"/> owner can turn at
        ///</summary>
        public float ItemTurnSpeed; // 
        ///<summary>
        /// Does <see cref="SceneItem"/> owner have a turret?
        ///</summary>
        public bool HasTurret; // 
        ///<summary>
        /// Does <see cref="SceneItem"/> owner have a spawnBullet position?
        ///</summary>
        public bool[] HasSpawnBullet; //
        
        ///<summary>
        /// Speed turret can turn at
        ///</summary>
        public float TurretTurnSpeed; // 
        ///<summary>
        /// Max velocity <see cref="SceneItem"/> owner can move at 
        ///</summary>
        public float MaxSpeed; // 
        ///<summary>
        /// Set view radius
        ///</summary>
        public float ViewRadius; //
        ///<summary>
        /// Distance <see cref="SceneItem"/> owner can shoot
        ///</summary>
        public float AttackRadius;  // 
        ///<summary>
        /// Amount of damage spawnBullet can apply to others
        ///</summary>
        public float[] AttackDamage; // 
       
        ///<summary>
        /// Bias by x amount; attacking vehicles.
        ///</summary>
        public float AttackDamageBiasVehicles; //
        ///<summary>
        /// Bias by x amount; attacking buildings
        ///</summary>
        public float AttackDamageBiasBuildings; // .
        ///<summary>
        /// Bias by x amount; attacking airplanes.
        ///</summary>
        public float AttackDamageBiasAircraft; //
        ///<summary>
        /// Bias by x amount; attacking other.
        ///</summary>
        public float AttackDamageBias4; // 
        ///<summary>
        /// <see cref="SceneItem"/> owner health
        ///</summary>
        public float StartingHealth; // 
        ///<summary>
        /// Set spawnBullet type of <see cref="PathShape"/> Enum Curve to use for projectiles
        ///</summary>
        public PathShape[] CurvePathShape; // 
       
        ///<summary>
        /// Set spawnBullet Curve's Magnitude
        ///</summary>
        public float[] CurveMagnitude; // 
       
        ///<summary>
        /// Set spawnBullet projectile's Speed
        ///</summary>
        public float[] ProjectileSpeed; // 
        
        ///<summary>
        /// Set spawnBullet projectiles ParticlesPerSecond
        ///</summary>
        public float[] ProjectileParticlesPerSecond; // 
       
        ///<summary>
        /// The spawnBullet <see cref="ProjectileType"/> Enum to use (Red Ball for example)
        ///</summary>
        public ProjectileType[] ProjectileTypeToUse; // 
        
        ///<summary>
        /// Set spawnBullet rate of fire
        ///</summary>
        public float[] RateOfFire; // 
        
        ///<summary>
        /// Set if this <see cref="SceneItem"/> owner should ignore the OccupiedBy flag when A*
        ///</summary>
        public IgnoreOccupiedBy IgnoreOccupiedByFlag; // 
        ///<summary>
        /// Set <see cref="SceneItem"/> owner cost
        ///</summary>
        public int Cost; // 
        ///<summary>
        /// Set build time
        ///</summary>
        public int TimeToBuild; // 
        ///<summary>
        /// Amount of energy needed to run
        ///</summary>
        public int EnergyNeeded; // 
        ///<summary>
        /// Does <see cref="SceneItem"/> owner generate revenue?
        ///</summary>
        public bool GeneratesRevenue; // 
        ///<summary>
        /// Does Building have a drop-ship, which brings supplies?
        ///</summary>
        public bool RevenueComesFromSupplyShip; // 
        ///<summary>
        /// Amount of Time between cash cycles. (Seconds)
        ///</summary>
        public int TimeToNextDeposit; // 
        ///<summary>
        /// Amount to increase per cycle.
        ///</summary>
        public int DepositAmount; // 
        ///<summary>
        /// Amount to increase per cycle when the energy is off.
        ///</summary>
        public int ReducedDepositAmount; // 
        ///<summary>
        /// Does <see cref="SceneItem"/> owner generate energy?
        ///</summary>
        public bool GeneratesEnergy; // 
        ///<summary>
        /// Amount of energy <see cref="SceneItem"/> owner generates.
        ///</summary>
        public int EnergyAmount; // 
        ///<summary>
        /// If energy goes off, this will show a <see cref="Texture2D"/> hovering over <see cref="SceneItem"/> owner in game.
        ///</summary>
        public bool ShowEnergyOffSymbol; // 
        ///<summary>
        /// If set to True, then the 'SpecialBuildingCreated' event will trigger when created. 
        ///</summary>
        public bool IsSpecialEnablerBuilding; //       
        ///<summary>
        /// Set to some name, like 'Research Center', for example; this is checked from the <see cref="EventHandler"/>.  
        ///</summary>
        public string SpecialBuildingName; //      
        ///<summary>
        /// Set the sounds to play for this unit 
        ///</summary>
        public SoundToPlay SoundsToPlay; //        
        ///<summary>
        /// Set to make an <see cref="SceneItem"/> owner face attackie when firing upon; for example, the helicopters should face attackie!
        ///</summary>
        public bool FaceAttackie; // 
        ///<summary>
        /// Set to make an aircraft <see cref="SceneItem"/> owner circle around when attacking or in rest position; for example, gunships!
        ///</summary>
        public bool AircraftMustCircle; // 
        ///<summary>
        /// Set Amount to adjust the pre-calulated collision radius, if necessary; otherwise set to 1 for 100%.
        ///</summary>
        public float AdjustCollisionRadius; //
        ///<summary>
        /// Set to have aircraft move up and down slightly in the air.
        ///</summary>
        public bool UseAircraftUpDownAnimation; // (11/28/2009) - 
        ///<summary>
        /// Set to have item move Up-axis, slightly left then right.
        ///</summary>
        public bool UseRockLeftRightAnimation; // (11/28/2009) - 
    }
}