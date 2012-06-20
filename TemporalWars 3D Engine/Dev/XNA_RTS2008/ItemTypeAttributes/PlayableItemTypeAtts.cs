#region File Description
//-----------------------------------------------------------------------------
// PlayableItemTypeAtts.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System.Collections.Generic;
using AStarInterfaces.AStarAlgorithm.Enums;
using Microsoft.Xna.Framework;
using TWEngine.Audio.Enums;
using TWEngine.InstancedModels;
using TWEngine.InstancedModels.Enums;
using TWEngine.ItemTypeAttributes.Structs;
using TWEngine.Particles.Enums;

namespace TWEngine.ItemTypeAttributes
{
    /// <summary>
    /// The <see cref="PlayableItemTypeAtts"/> class, inheriting from <see cref="ItemTypeAtts"/> base class, is
    /// used to save and load the <see cref="PlayableItemTypeAttributes"/> structure, for a given <see cref="ItemType"/>.
    /// For example, you could have an <see cref="ItemType"/> which is a 'Tank'; all attributes would then be retrieved 
    /// from this class, like the speed the tank could move at in the game.
    /// </summary>
    public class PlayableItemTypeAtts : ItemTypeAtts
    {
        // Playable ItemType Attributes
        ///<summary>
        /// Internal Dictionary, used to store the <see cref="PlayableItemTypeAttributes"/> structure, using the <see cref="ItemType"/> as the key.
        ///</summary>
        public static Dictionary<ItemType, PlayableItemTypeAttributes> ItemTypeAtts = new Dictionary<ItemType, PlayableItemTypeAttributes>(InstancedItem.ItemTypeCount);

        // 3/2/2009 - 
        /// <summary>
        /// Set TRUE to force recreation of atts file!
        /// </summary>
        private static bool _forceRebuildOfXMLFile;

        // 5/1/2009 - Create Private Constructor, per FXCop
        PlayableItemTypeAtts()
        {
            // Empty
            _forceRebuildOfXMLFile = false;
        }

        // 12/23/2008
        /// <summary>
        /// Creates the <see cref="PlayableItemTypeAttributes"/> structure, used for each specific <see cref="ItemType"/>,
        /// and saves the data to disk.  This file is used when loading items back into memory. 
        /// This allows for changing of the attributes quickly, just by updating the XML file for a specific <see cref="ItemType"/>.
        /// </summary>
        /// <remarks>This method should only me called to create the file for the first Time,
        /// or if the file is lost or destroyed.</remarks>
        /// <param name="game"><see cref="Game"/> instance</param>
        public static void CreateItemTypeAttributesAndSave(Game game)
        {
            // 8/20/2008 - Save Game Ref
            GameInstance = game;

            var itemAttsDefault = new PlayableItemTypeAttributes(); // 3/25/2009

            // Set General Building's atts first
            itemAttsDefault.ItemGroupType = ItemGroupType.Buildings; // 10/5/2009
            itemAttsDefault.ItemSpawnOffset = Vector3.Zero;
            itemAttsDefault.ItemMarkerOffset = Vector3.Zero;
            itemAttsDefault.DrawStatusBar = true;
            itemAttsDefault.StatusBarOffsetPosition2D = new Vector2(-50, -50);
            itemAttsDefault.FacingDirectionOffset = 0;
            itemAttsDefault.ItemTurnSpeed = 0;
            itemAttsDefault.HasTurret = false;
            itemAttsDefault.HasSpawnBullet = new bool[4];
            itemAttsDefault.TurretTurnSpeed = 0;
            itemAttsDefault.MaxSpeed = 0.0f;
            itemAttsDefault.ViewRadius = 700.0f;
            itemAttsDefault.AttackRadius = 0;
            itemAttsDefault.AttackDamage = new float[4];
            itemAttsDefault.AttackDamageBiasVehicles = 0;
            itemAttsDefault.AttackDamageBiasBuildings = 0;
            itemAttsDefault.AttackDamageBiasAircraft = 0;
            itemAttsDefault.AttackDamageBias4 = 0;
            itemAttsDefault.StartingHealth = 1000.0f;
            itemAttsDefault.CurvePathShape = new[] { PathShape.Straight, PathShape.Straight, PathShape.Straight, PathShape.Straight };
            itemAttsDefault.CurveMagnitude = new[] {1.0f, 1.0f, 1.0f, 1.0f};
            itemAttsDefault.RateOfFire = new[] {0f, 0f, 0f, 0f};
            itemAttsDefault.ProjectileSpeed = new[] {480.0f, 0, 0, 0};
            itemAttsDefault.ProjectileParticlesPerSecond = new[] {10f, 10f, 10f, 10f};
            itemAttsDefault.ProjectileTypeToUse = new[]
                                                       {
                                                           ProjectileType.WhiteBall, ProjectileType.WhiteBall,
                                                           ProjectileType.WhiteBall, ProjectileType.WhiteBall
                                                       };
            itemAttsDefault.IgnoreOccupiedByFlag = IgnoreOccupiedBy.Off;
            itemAttsDefault.Cost = 1000;
            itemAttsDefault.EnergyNeeded = 15;
            itemAttsDefault.TimeToBuild = 5;
            itemAttsDefault.GeneratesRevenue = false;
            itemAttsDefault.RevenueComesFromSupplyShip = false;
            itemAttsDefault.TimeToNextDeposit = 0;
            itemAttsDefault.DepositAmount = 0;
            itemAttsDefault.ReducedDepositAmount = 0;
            itemAttsDefault.GeneratesEnergy = false;
            itemAttsDefault.EnergyAmount = 0;
            itemAttsDefault.ShowEnergyOffSymbol = false;
            itemAttsDefault.IsSpecialEnablerBuilding = false;            
            itemAttsDefault.SpecialBuildingName = "None";
            itemAttsDefault.SoundsToPlay.SoundToPlayPrimaryFire = Sounds.Cannon1;
            itemAttsDefault.SoundsToPlay.SoundToPlaySecondaryFire = Sounds.ElectroGun1;
            itemAttsDefault.FaceAttackie = false;
            itemAttsDefault.AircraftMustCircle = false;
            itemAttsDefault.AdjustCollisionRadius = 1.0f;


            var itemAtts = itemAttsDefault;

            #region SciFiBuildingSet_1

            // Add Building 1
            itemAtts.ItemType = ItemType.sciFiBlda01;
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Building 2
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiBlda02;
            AddItemTypeAttributeToArray(ref itemAtts);
          
            // Add Building 3   
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiBlda03;
            AddItemTypeAttributeToArray(ref itemAtts);           

            // Add Building 4
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiBlda04;
            AddItemTypeAttributeToArray(ref itemAtts);
            
            // Add Building 5
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiBlda05;
            AddItemTypeAttributeToArray(ref itemAtts);           

            // Add Building 6     
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiBlda06;
            itemAtts.GeneratesRevenue = true;
            itemAtts.TimeToNextDeposit = 5; // seconds
            itemAtts.DepositAmount = 125;
            itemAtts.ReducedDepositAmount = 50;
            AddItemTypeAttributeToArray(ref itemAtts);          

            // Add Building 7
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiBlda07;
            itemAtts.GeneratesRevenue = false;
            itemAtts.TimeToNextDeposit = 0;
            itemAtts.DepositAmount = 0;
            itemAtts.ReducedDepositAmount = 0;
            AddItemTypeAttributeToArray(ref itemAtts);
            
            // Add Building 8
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiBlda08;            
            AddItemTypeAttributeToArray(ref itemAtts);           

            // Add Building 9  
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiBlda09;
            itemAtts.GeneratesRevenue = true;
            itemAtts.TimeToNextDeposit = 5; // seconds
            itemAtts.DepositAmount = 125;
            itemAtts.ReducedDepositAmount = 50;
            AddItemTypeAttributeToArray(ref itemAtts);
           

            #endregion

            #region SciFiBuildingSet_2

            // Add Building 1
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiBldb01;
            itemAtts.ProductionType = ItemGroupType.Vehicles; // 11/6/2009
            itemAtts.ItemMarkerOffset = new Vector3(-50, 0, 200);
            itemAtts.GeneratesEnergy = false;
            itemAtts.GeneratesRevenue = false;
            itemAtts.TimeToNextDeposit = 0;
            itemAtts.DepositAmount = 0;
            itemAtts.ReducedDepositAmount = 0;
            itemAtts.EnergyAmount = 0;
            itemAtts.IsSpecialEnablerBuilding = true;
            itemAtts.SpecialBuildingName = "War Factory-B1";            
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Building 2
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiBldb02;
            itemAtts.ItemMarkerOffset = Vector3.Zero;
            itemAtts.IsSpecialEnablerBuilding = true;
            itemAtts.SpecialBuildingName = "Research Center-B2";           
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Building 3   
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiBldb03;           
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Building 4
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiBldb04;
            itemAtts.IsSpecialEnablerBuilding = true;
            itemAtts.SpecialBuildingName = "Research Center-B4";            
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Building 5
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiBldb05;
            itemAtts.GeneratesEnergy = true;
            itemAtts.EnergyAmount = 100;            
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Building 6  
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiBldb06;
            itemAtts.GeneratesEnergy = false;
            itemAtts.EnergyAmount = 0;           
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Building 7
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiBldb07;
            itemAtts.GeneratesRevenue = true;
            itemAtts.TimeToNextDeposit = 5; // seconds
            itemAtts.DepositAmount = 250;
            itemAtts.ReducedDepositAmount = 125;            
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Building 8
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiBldb08;
            itemAtts.GeneratesRevenue = false;
            itemAtts.TimeToNextDeposit = 0;
            itemAtts.DepositAmount = 0;
            itemAtts.ReducedDepositAmount = 0;            
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Building 9   
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiBldb09;
            itemAtts.GeneratesEnergy = true;
            itemAtts.EnergyAmount = 100;            
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Building 10   
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiBldb10;
            itemAtts.ProductionType = ItemGroupType.Airplanes; // 11/6/2009
            itemAtts.GeneratesEnergy = false;
            itemAtts.EnergyAmount = 0;
            itemAtts.ItemSpawnOffset = new Vector3(0, 200, 0);
            itemAtts.ItemMarkerOffset = new Vector3(200, 0, 0);
            itemAtts.IsSpecialEnablerBuilding = true;
            itemAtts.SpecialBuildingName = "Airport-B10";        
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Building 11   
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiBldb11;
            itemAtts.ProductionType = ItemGroupType.Vehicles; // 11/6/2009
            itemAtts.ItemSpawnOffset = new Vector3(25, 0, 50);
            itemAtts.ItemMarkerOffset = new Vector3(-50, 0, 300);
            itemAtts.IsSpecialEnablerBuilding = true;
            itemAtts.SpecialBuildingName = "War Factory-B11";            
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Building 12 
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiBldb12;
            itemAtts.ItemSpawnOffset = Vector3.Zero;
            itemAtts.ItemMarkerOffset = Vector3.Zero;
            itemAtts.GeneratesRevenue = true;
            itemAtts.RevenueComesFromSupplyShip = true;
            itemAtts.TimeToNextDeposit = 5; // seconds
            itemAtts.DepositAmount = 250;
            itemAtts.ReducedDepositAmount = 125;           
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Building 13 
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiBldb13;
            itemAtts.ProductionType = ItemGroupType.Airplanes; // 11/6/2009
            itemAtts.ItemSpawnOffset = new Vector3(10, 20, 0);
            itemAtts.ItemMarkerOffset = new Vector3(200, 0, 0);
            itemAtts.GeneratesRevenue = false;        
            itemAtts.IsSpecialEnablerBuilding = true;
            itemAtts.SpecialBuildingName = "Airport-B13";
            itemAtts.AdjustCollisionRadius = 0.1f;
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Building 14 (MCF)
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiBldb14;
            itemAtts.ProductionType = ItemGroupType.Buildings; // 11/6/2009
            itemAttsDefault.ViewRadius = 1500.0f;
            itemAtts.ItemSpawnOffset = new Vector3(10, 20, 0);
            itemAtts.ItemMarkerOffset = new Vector3(200, 0, 0);
            itemAtts.GeneratesEnergy = true;
            itemAtts.EnergyAmount = 50;  
            itemAtts.GeneratesRevenue = false;
            itemAtts.Cost = 2000;
            itemAtts.StartingHealth = 20000.0f;
            itemAtts.EnergyNeeded = 25;
            itemAtts.TimeToBuild = 1;            
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Building 15 (MCF)
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiBldb15;
            itemAtts.ProductionType = ItemGroupType.Buildings; // 11/6/2009
            itemAttsDefault.ViewRadius = 1500.0f;
            itemAtts.ItemSpawnOffset = new Vector3(10, 20, 0);
            itemAtts.ItemMarkerOffset = new Vector3(200, 0, 0);
            itemAtts.GeneratesEnergy = true;
            itemAtts.EnergyAmount = 50;  
            itemAtts.GeneratesRevenue = false;
            itemAtts.Cost = 2000;
            itemAtts.StartingHealth = 20000.0f;
            itemAtts.EnergyNeeded = 25;
            itemAtts.TimeToBuild = 1;
            AddItemTypeAttributeToArray(ref itemAtts); 


            #endregion            

            #region Flag Marker

            // Add Flag Marker
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.flagMarker;
            itemAttsDefault.ViewRadius = 150.0f;
            itemAtts.ItemSpawnOffset = Vector3.Zero;
            itemAtts.ItemMarkerOffset = Vector3.Zero;
            itemAtts.GeneratesRevenue = false;
            itemAtts.Cost = 0;
            itemAtts.StartingHealth = 0.0f;
            itemAtts.EnergyNeeded = 0;
            itemAtts.TimeToBuild = 0;
            AddItemTypeAttributeToArray(ref itemAtts); 

            #endregion

            #region SciFiTankSet

            // Set General Tanks's atts first
            itemAttsDefault.ItemGroupType = ItemGroupType.Vehicles; // 10/5/2009
            itemAttsDefault.ItemGroupToAttack = ItemGroupType.Buildings | ItemGroupType.Vehicles | ItemGroupType.Shields; // 11/6/2009
            itemAttsDefault.ItemSpawnOffset = Vector3.Zero;
            itemAttsDefault.ItemMarkerOffset = Vector3.Zero;
            itemAttsDefault.StatusBarOffsetPosition2D = new Vector2(-50, -50);
            itemAttsDefault.ItemTurnSpeed = 2.25f;
            itemAttsDefault.TurretTurnSpeed = 2.5f;
            itemAttsDefault.HasTurret = true;
            itemAttsDefault.HasSpawnBullet = new[] {true, false, false, false};
            itemAttsDefault.MaxSpeed = 150.0f;
            itemAttsDefault.ViewRadius = 700.0f;
            itemAttsDefault.AttackRadius = 500.0f;
            itemAttsDefault.AttackDamage = new[] {25.0f, 0f, 0f, 0f};
            itemAttsDefault.AttackDamageBiasVehicles = 1;
            itemAttsDefault.AttackDamageBiasBuildings = 1;
            itemAttsDefault.AttackDamageBiasAircraft = 1;
            itemAttsDefault.AttackDamageBias4 = 1;
            itemAttsDefault.StartingHealth = 500.0f;
            itemAttsDefault.CurvePathShape = new[] { PathShape.Straight, PathShape.Straight, PathShape.Straight, PathShape.Straight }; // 1/14//2011
            itemAttsDefault.CurveMagnitude = new[] { 1.0f, 0.2f, 1.0f, 1.0f }; // 1/14/2011
            itemAttsDefault.RateOfFire = new[] {3.0f, 0, 0, 0};
            itemAttsDefault.ProjectileSpeed = new[] {500.0f, 500.0f, 500.0f, 500.0f};
            itemAttsDefault.ProjectileParticlesPerSecond = new[] {10f, 10f, 10f, 10f};
            itemAttsDefault.ProjectileTypeToUse = new[]
                                                       {
                                                           ProjectileType.WhiteBall, ProjectileType.WhiteBall,
                                                           ProjectileType.WhiteBall, ProjectileType.WhiteBall
                                                       }; // 1/14/2011
            itemAttsDefault.IgnoreOccupiedByFlag = IgnoreOccupiedBy.On;
            itemAttsDefault.Cost = 500;
            itemAttsDefault.EnergyNeeded = 0;
            itemAttsDefault.TimeToBuild = 5;

            // Add Tank 1
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiTank01;
            itemAtts.ItemTurnSpeed = 3.25f;
            itemAtts.TurretTurnSpeed = 2.5f; // was 0.05
            itemAtts.MaxSpeed = 150.0f;
            itemAtts.ViewRadius = 700.0f;
            itemAtts.AttackRadius = 500.0f;
            itemAtts.AttackDamage = new[] { 50.0f, 0.0f, 0.0f, 0.0f }; // 1/14/2011
            itemAtts.AttackDamageBiasVehicles = 1;
            itemAtts.AttackDamageBiasBuildings = 0.3f;
            itemAtts.AttackDamageBiasAircraft = 0;
            itemAtts.AttackDamageBias4 = 1;
            itemAtts.RateOfFire = new[] { 3.0f, 0f, 0f, 0f }; // 1/14/2011
            itemAtts.ProjectileSpeed = new[] { 600.0f, 500.0f, 500.0f, 500.0f }; // 1/14/2011
            itemAtts.ProjectileParticlesPerSecond = new[] {10.0f, 0f, 0f, 0f};
            itemAtts.StartingHealth = 500.0f;
            itemAtts.SoundsToPlay.SoundToPlayPrimaryFire = Sounds.Cannon1;
            itemAtts.AdjustCollisionRadius = 1.0f;
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Tank 2
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiTank02;
            itemAtts.ItemTurnSpeed = 4.25f;
            itemAtts.TurretTurnSpeed = 1.2f;
            itemAtts.MaxSpeed = 400.0f;
            itemAtts.ViewRadius = 500.0f;
            itemAtts.AttackRadius = 400.0f;
            itemAtts.AttackDamage = new[] { 20.0f, 0.0f, 0.0f, 0.0f }; // 1/14/2011
            itemAtts.AttackDamageBiasVehicles = 1;
            itemAtts.AttackDamageBiasBuildings = 0.5f;
            itemAtts.AttackDamageBiasAircraft = 0;
            itemAtts.AttackDamageBias4 = 1;
            itemAtts.RateOfFire = new[] { 0.5f, 0f, 0f, 0f }; // 1/14/2011
            itemAtts.ProjectileSpeed = new[] { 900.0f, 500.0f, 500.0f, 500.0f }; // 1/14/2011
            itemAtts.ProjectileParticlesPerSecond = new[] { 10.0f, 0f, 0f, 0f };
            itemAtts.StartingHealth = 200.0f;
            itemAtts.IgnoreOccupiedByFlag = IgnoreOccupiedBy.Off;
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Tank 3  
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiTank03;
            itemAtts.ItemTurnSpeed = 4.15f;
            itemAtts.TurretTurnSpeed = 0.9f;
            itemAtts.MaxSpeed = 250.0f;
            itemAtts.ViewRadius = 900.0f;
            itemAtts.AttackRadius = 750.0f;
            itemAtts.AttackDamage = new[] { 75.0f, 0.0f, 0.0f, 0.0f }; // 1/14/2011
            itemAtts.AttackDamageBiasVehicles = 1;
            itemAtts.AttackDamageBiasBuildings = 0.3f;
            itemAtts.AttackDamageBiasAircraft = 0;
            itemAtts.AttackDamageBias4 = 1;
            itemAtts.RateOfFire = new[] { 4.0f, 0f, 0f, 0f }; // 1/14/2011
            itemAtts.ProjectileSpeed = new[] { 800.0f, 500.0f, 500.0f, 500.0f }; // 1/14/2011
            itemAtts.ProjectileParticlesPerSecond = new[] { 10.0f, 0f, 0f, 0f };
            itemAtts.ProjectileTypeToUse = new[]
                                                       {
                                                           ProjectileType.WhiteBall, ProjectileType.WhiteBall,
                                                           ProjectileType.WhiteBall, ProjectileType.WhiteBall
                                                       }; // 1/14/2011
            itemAtts.StartingHealth = 500.0f;
            itemAtts.IgnoreOccupiedByFlag = IgnoreOccupiedBy.On;
            itemAtts.SoundsToPlay.SoundToPlayPrimaryFire = Sounds.Cannon2;
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Tank 4
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiTank04;
            itemAtts.HasSpawnBullet = new[] {true, true, true, true};
            itemAtts.CurveMagnitude = new[] { 0.4f, 1.0f, 1.0f, 1.0f }; // 1/14/2011                     
            itemAtts.ItemTurnSpeed = 10.25f;
            itemAtts.TurretTurnSpeed = 3.2f;
            itemAtts.MaxSpeed = 400.0f;
            itemAtts.ViewRadius = 500.0f;
            itemAtts.AttackRadius = 400.0f;
            itemAtts.AttackDamage = new[] {10.0f, 2.0f, 2.0f, 10.0f};
            itemAtts.AttackDamageBiasVehicles = 1;
            itemAtts.AttackDamageBiasBuildings = 0.5f;
            itemAtts.AttackDamageBiasAircraft = 0;
            itemAtts.AttackDamageBias4 = 1;
            itemAtts.RateOfFire = new[] {3.5f, 1.0f, 1.0f, 3.5f };
            itemAtts.ProjectileSpeed = new[] {900.0f, 600.0f, 600.0f, 900.0f};
            itemAtts.ProjectileParticlesPerSecond = new[] {5f, 5f, 5f, 5f};
            itemAtts.ProjectileTypeToUse = new[]
                                                       {
                                                           ProjectileType.RedBall, ProjectileType.OrangeBall,
                                                           ProjectileType.OrangeBall, ProjectileType.RedBall
                                                       }; // 1/14/2011
            itemAtts.StartingHealth = 250.0f;
            itemAtts.SoundsToPlay.SoundToPlayPrimaryFire = Sounds.PulseGun4;
            itemAtts.AdjustCollisionRadius = 1.0f;
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Tank 5
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiTank05;
            itemAtts.HasSpawnBullet = new[] { true, false, false, false }; // 1/14/2011
            itemAtts.AttackDamage = new[] {10.0f, 0f, 0f, 0f};
            itemAtts.RateOfFire = new[] { 3.0f, 0f, 0f, 0f }; // 1/14/2011
            itemAtts.ProjectileSpeed = new[] { 500.0f, 0.0f, 0.0f, 0.0f }; // 1/14/2011
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Tank 6   
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiTank06;
            itemAtts.ItemTurnSpeed = 1.15f;
            itemAtts.TurretTurnSpeed = 0.9f;
            itemAtts.MaxSpeed = 300.0f;
            itemAtts.ViewRadius = 700.0f;
            itemAtts.AttackRadius = 650.0f;
            itemAtts.HasSpawnBullet = new[] {true, true, false, false};
            itemAtts.AttackDamage = new[] { 20.0f, 1.0f, 0.0f, 0.0f }; // 1/14/2011
            itemAtts.RateOfFire = new[] { 3.0f, 0.5f, 0f, 0f }; // 1/14/2011
            itemAtts.ProjectileSpeed = new[] { 500.0f, 800.0f, 500.0f, 500.0f }; // 1/14/2011
            itemAtts.ProjectileParticlesPerSecond = new[] { 5.0f, 5.0f, 0f, 0f };
            itemAtts.ProjectileTypeToUse = new[]
                                                       {
                                                           ProjectileType.OrangeBall, ProjectileType.RedBall,
                                                           ProjectileType.WhiteBall, ProjectileType.WhiteBall
                                                       }; // 1/14/2011
            itemAtts.StartingHealth = 1000.0f;
            itemAtts.AdjustCollisionRadius = 0.6f;
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Tank 7
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiTank07;
            itemAtts.AttackDamage = new[] { 20.0f, 0.0f, 0.0f, 0.0f }; // 1/14/2011
            itemAtts.AttackDamageBiasVehicles = 1;
            itemAtts.AttackDamageBiasBuildings = 1;
            itemAtts.AttackDamageBiasAircraft = 0;
            itemAtts.AttackDamageBias4 = 1;
            itemAtts.StartingHealth = 600.0f;
            itemAtts.Cost = 2000;
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Tank 8
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiTank08;
            itemAtts.AttackDamage = new[] { 50.0f, 0.0f, 0.0f, 0.0f }; // 1/14/2011
            itemAtts.AttackDamageBiasVehicles = 0.5f;
            itemAtts.AttackDamageBiasBuildings = 1;
            itemAtts.AttackDamageBiasAircraft = 0;
            itemAtts.AttackDamageBias4 = 1;
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Tank 9  
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiTank09;
            itemAtts.ItemTurnSpeed = 1.5f;
            itemAtts.TurretTurnSpeed = 0.9f;
            itemAtts.MaxSpeed = 100.0f;
            itemAtts.ViewRadius = 500.0f;
            itemAtts.AttackRadius = 450.0f;
            itemAtts.AttackDamage = new[] { 100.0f, 0.0f, 0.0f, 0.0f }; // 1/14/2011
            itemAtts.AttackDamageBiasVehicles = 1;
            itemAtts.AttackDamageBiasBuildings = 1;
            itemAtts.AttackDamageBiasAircraft = 0;
            itemAtts.AttackDamageBias4 = 1;
            itemAtts.StartingHealth = 2000.0f;
            itemAtts.Cost = 2000;
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Tank 10
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiTank10;
            itemAtts.ItemGroupToAttack = ItemGroupType.Vehicles | ItemGroupType.Shields; // 11/6/2009
            itemAtts.CurvePathShape = new[] { PathShape.ArchUp, PathShape.Straight, PathShape.Straight, PathShape.Straight }; // 1/14//2011
            itemAtts.CurveMagnitude = new[] { 2.0f, 1.0f, 1.0f, 1.0f }; // 1/14/2011
            itemAtts.ProjectileSpeed = new[] { 940.0f, 500.0f, 500.0f, 500.0f }; // 1/14/2011
            itemAtts.ViewRadius = 1100.0f;
            itemAtts.AttackRadius = 950.0f;
            itemAtts.AttackDamage = new[] { 100.0f, 0.0f, 0.0f, 0.0f }; // 1/14/2011
            itemAtts.AttackDamageBiasVehicles = 0.5f;
            itemAtts.AttackDamageBiasBuildings = 1;
            itemAtts.AttackDamageBiasAircraft = 0;
            itemAtts.StartingHealth = 800.0f;
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Tank 11   
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiTank11;
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Artilery 1
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiArtilery01;
            itemAtts.CurvePathShape = new[] { PathShape.ArchUp, PathShape.Straight, PathShape.Straight, PathShape.Straight }; // 1/14//2011
            itemAtts.ProjectileTypeToUse = new[]
                                                       {
                                                           ProjectileType.OrangeBall, ProjectileType.WhiteBall,
                                                           ProjectileType.WhiteBall, ProjectileType.WhiteBall
                                                       }; // 1/14/2011
            itemAtts.CurveMagnitude = new[] { 2.0f, 1.0f, 1.0f, 1.0f }; // 1/14/2011
            itemAtts.ItemTurnSpeed = 0.9f;
            itemAtts.TurretTurnSpeed = 1.9f;
            itemAtts.MaxSpeed = 70.0f;
            itemAtts.ViewRadius = 2000.0f;
            itemAtts.AttackRadius = 1900.0f;
            itemAtts.AttackDamage = new[] { 250.0f, 0.0f, 0.0f, 0.0f }; // 1/14/2011
            itemAtts.AttackDamageBiasVehicles = 0.5f;
            itemAtts.AttackDamageBiasBuildings = 1;
            itemAtts.AttackDamageBiasAircraft = 0;
            itemAtts.AttackDamageBias4 = 1;
            itemAtts.RateOfFire = new[] { 10.0f, 0f, 0f, 0f }; // 1/14/2011
            itemAtts.ProjectileSpeed = new[] { 350.0f, 500.0f, 500.0f, 500.0f }; // 1/14/2011
            itemAtts.ProjectileParticlesPerSecond = new[] { 5.0f, 0f, 0f, 0f };
            itemAtts.AdjustCollisionRadius = 0.5f;
            AddItemTypeAttributeToArray(ref itemAtts);

            #endregion

            #region SciFiJeeps

            // Set General Jeep's atts first
            itemAttsDefault.ItemGroupType = ItemGroupType.Vehicles; // 10/5/2009
            itemAttsDefault.ItemGroupToAttack = ItemGroupType.Airplanes; // 11/6/2009
            itemAttsDefault.ItemSpawnOffset = Vector3.Zero;
            itemAttsDefault.ItemMarkerOffset = Vector3.Zero;
            itemAttsDefault.StatusBarOffsetPosition2D = new Vector2(-50, -50);
            itemAttsDefault.ItemTurnSpeed = 10.25f;
            itemAttsDefault.TurretTurnSpeed = 1.5f;
            itemAttsDefault.HasSpawnBullet = new[] { true, true, true, false }; // 1/14/2011
            itemAttsDefault.MaxSpeed = 150.0f;
            itemAttsDefault.ViewRadius = 700.0f;
            itemAttsDefault.AttackRadius = 500.0f;
            itemAttsDefault.AttackDamage = new[] { 5f, 5f, 5f, 5f};
            itemAttsDefault.AttackDamageBiasVehicles = 0;
            itemAttsDefault.AttackDamageBiasBuildings = 1;
            itemAttsDefault.AttackDamageBiasAircraft = 1;
            itemAttsDefault.AttackDamageBias4 = 1;
            itemAttsDefault.StartingHealth = 200.0f;
            itemAttsDefault.CurvePathShape = new[] { PathShape.Straight, PathShape.Straight, PathShape.Straight, PathShape.Straight }; // 1/14//2011
            itemAttsDefault.CurveMagnitude = new[] { 0.2f, 1.0f, 1.0f, 1.0f }; // 1/14/2011
            itemAttsDefault.RateOfFire = new[] {3.0f, 2.0f, 2.0f, 3.0f};
            itemAttsDefault.ProjectileSpeed = new[] {500.0f, 400.0f, 400.0f, 500.0f};
            itemAttsDefault.ProjectileParticlesPerSecond = new[] {5.0f, 5.0f, 5.0f, 5.0f};
            itemAttsDefault.ProjectileTypeToUse = new[]
                                                       {
                                                           ProjectileType.WhiteBall, ProjectileType.WhiteBall,
                                                           ProjectileType.WhiteBall, ProjectileType.WhiteBall
                                                       };
            itemAttsDefault.IgnoreOccupiedByFlag = IgnoreOccupiedBy.On;
            itemAttsDefault.Cost = 500;
            itemAttsDefault.EnergyNeeded = 0;
            itemAttsDefault.TimeToBuild = 5;

            // Add Jeep 1
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiJeep01;
            itemAtts.ItemTurnSpeed = 10.15f;
            itemAtts.TurretTurnSpeed = 5.5f;
            itemAtts.MaxSpeed = 250.0f;
            itemAtts.ViewRadius = 900.0f;
            itemAtts.AttackRadius = 900.0f;            
            itemAtts.AttackDamageBiasVehicles = 0;
            itemAtts.AttackDamageBiasBuildings = 0;
            itemAtts.AttackDamageBiasAircraft = 1;
            itemAtts.AttackDamageBias4 = 1;         
            itemAtts.StartingHealth = 300.0f;
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Jeep 3
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiJeep03;
            itemAtts.ItemTurnSpeed = 10.15f;
            itemAtts.TurretTurnSpeed = 6.5f;
            itemAtts.MaxSpeed = 250.0f;
            itemAtts.ViewRadius = 900.0f;
            itemAtts.AttackRadius = 900.0f;
            itemAtts.AttackDamageBiasVehicles = 0;
            itemAtts.AttackDamageBiasBuildings = 0;
            itemAtts.AttackDamageBiasAircraft = 1;
            itemAtts.AttackDamageBias4 = 1;
            itemAtts.StartingHealth = 300.0f;
            AddItemTypeAttributeToArray(ref itemAtts);


            #endregion

            #region SciFiAircraftSet

            // Set General Airfract atts first
            itemAttsDefault.ItemGroupType = ItemGroupType.Airplanes; // 10/5/2009
            itemAttsDefault.StatusBarOffsetPosition2D = new Vector2(-50, -50);
            itemAttsDefault.ItemTurnSpeed = 1.25f;
            itemAttsDefault.TurretTurnSpeed = 0.0f;
            itemAttsDefault.HasTurret = false;
            itemAttsDefault.HasSpawnBullet = new[] { true, false, false, false }; // 1/14/2011
            itemAttsDefault.MaxSpeed = 150.0f;
            itemAttsDefault.ViewRadius = 700.0f;
            itemAttsDefault.AttackRadius = 500.0f;
            itemAttsDefault.AttackDamage = new[] {25.0f, 0f, 0f, 0f};
            itemAttsDefault.AttackDamageBiasVehicles = 1;
            itemAttsDefault.AttackDamageBiasBuildings = 1;
            itemAttsDefault.AttackDamageBiasAircraft = 1;
            itemAttsDefault.AttackDamageBias4 = 1;
            itemAttsDefault.StartingHealth = 200.0f;
            itemAttsDefault.CurvePathShape = new[]
                                                  {
                                                      PathShape.Straight, PathShape.Straight, PathShape.Straight,
                                                      PathShape.Straight
                                                  };
            itemAttsDefault.CurveMagnitude = new[] {0.2f, 0.2f, 0.2f, 0.2f};
            itemAttsDefault.RateOfFire = new[] {3.0f, 0f, 0f, 0f};
            itemAttsDefault.ProjectileSpeed = new[] {480.0f, 0f, 0f, 0f};
            itemAttsDefault.ProjectileParticlesPerSecond = new[] {1f, 1f, 1f, 1f};
            itemAttsDefault.ProjectileTypeToUse = new[]
                                                       {
                                                           ProjectileType.BlueBall, ProjectileType.BlueBall,
                                                           ProjectileType.BlueBall, ProjectileType.BlueBall
                                                       };
            itemAttsDefault.IgnoreOccupiedByFlag = IgnoreOccupiedBy.On;
            itemAttsDefault.Cost = 500;
            itemAttsDefault.EnergyNeeded = 0;
            itemAttsDefault.TimeToBuild = 5;
            itemAttsDefault.FaceAttackie = true;
            itemAttsDefault.UseAircraftUpDownAnimation = true; // 11/28/2009
            itemAttsDefault.UseRockLeftRightAnimation = true; // 11/28/2009

            // Add Heli 1
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiHeli01;
            itemAtts.ItemGroupToAttack = ItemGroupType.Vehicles; // 11/6/2009
            itemAtts.ItemTurnSpeed = 2.25f;
            itemAtts.TurretTurnSpeed = 0.0f;
            itemAtts.HasSpawnBullet = new[] { true, true, true, true};
            itemAtts.MaxSpeed = 250.0f;
            itemAtts.ViewRadius = 1000.0f;
            itemAtts.AttackRadius = 600.0f;
            itemAtts.AttackDamage = new[] {10.0f, 10.0f, 25.0f, 25.0f};
            itemAtts.AttackDamageBiasVehicles = 1;
            itemAtts.AttackDamageBiasBuildings = 1;
            itemAtts.AttackDamageBiasAircraft = 1;
            itemAtts.AttackDamageBias4 = 1;
            itemAtts.RateOfFire = new[] {0.5f, 0.5f, 7.5f, 7.5f};
            itemAtts.ProjectileSpeed = new[] {180.0f, 180.0f, 300.0f, 300.0f};
            itemAtts.ProjectileTypeToUse = new[]
                                                {
                                                    ProjectileType.BlueBall, ProjectileType.BlueBall,
                                                    ProjectileType.BlueBall, ProjectileType.BlueBall
                                                };
            itemAtts.StartingHealth = 400.0f;
            itemAtts.IgnoreOccupiedByFlag = IgnoreOccupiedBy.On;
            itemAtts.SoundsToPlay.SoundToPlayPrimaryFire = Sounds.MachineGunA_Group;
            itemAtts.SoundsToPlay.SoundToPlaySecondaryFire = Sounds.LaserMissle_Group;
            itemAtts.UseRockLeftRightAnimation = false; // 11/28/2009
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Heli 2
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiHeli02;
            itemAtts.ItemGroupToAttack = ItemGroupType.Vehicles; // 11/6/2009
            itemAtts.ItemTurnSpeed = 2.5f;
            itemAtts.TurretTurnSpeed = 0.0f;
            itemAtts.HasSpawnBullet = new[] { true, true, true, true };
            itemAtts.MaxSpeed = 350.0f;
            itemAtts.ViewRadius = 1500.0f;
            itemAtts.AttackRadius = 800.0f;
            itemAtts.AttackDamage = new[] { 25.0f, 25.0f, 75.0f, 75.0f };
            itemAtts.AttackDamageBiasVehicles = 1;
            itemAtts.AttackDamageBiasBuildings = 1;
            itemAtts.AttackDamageBiasAircraft = 1;
            itemAtts.AttackDamageBias4 = 1;
            itemAtts.RateOfFire = new[] { 0.8f, 0.8f, 8.5f, 8.5f };
            itemAtts.ProjectileSpeed = new[] { 180.0f, 180.0f, 200.0f, 220.0f };
            itemAtts.ProjectileTypeToUse = new[]
                                                {
                                                    ProjectileType.BlueBall, ProjectileType.BlueBall,
                                                    ProjectileType.RedBall, ProjectileType.RedBall
                                                };
            itemAtts.StartingHealth = 100.0f;
            itemAtts.IgnoreOccupiedByFlag = IgnoreOccupiedBy.On;
            itemAtts.SoundsToPlay.SoundToPlayPrimaryFire = Sounds.MachineGunC;
            itemAtts.SoundsToPlay.SoundToPlaySecondaryFire = Sounds.RocketMissle_Group;
            itemAtts.UseRockLeftRightAnimation = false; // 11/28/2009
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add GunShip 1
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiGunShip01;
            itemAtts.ItemTurnSpeed = 2.5f;
            itemAtts.TurretTurnSpeed = 0.0f;
            itemAtts.HasSpawnBullet = new[] { false, false, false, false };
            itemAtts.MaxSpeed = 150.0f;
            itemAtts.ViewRadius = 1000.0f;
            itemAtts.StartingHealth = 200.0f;
            itemAtts.IgnoreOccupiedByFlag = IgnoreOccupiedBy.On;
            itemAtts.FaceAttackie = false;
            itemAtts.UseAircraftUpDownAnimation = false; // 11/28/2009
            itemAtts.UseRockLeftRightAnimation = false; // 11/28/2009
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add GunShip 2
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiGunShip02;
            itemAtts.ItemGroupToAttack = ItemGroupType.Airplanes; // 11/6/2009
            itemAtts.ItemTurnSpeed = 2.5f;
            itemAtts.TurretTurnSpeed = 0.0f;
            itemAtts.HasSpawnBullet = new[] { true, true, true, true };
            itemAtts.MaxSpeed = 150.0f;
            itemAtts.ViewRadius = 1000.0f;
            itemAtts.AttackRadius = 400.0f;
            itemAtts.AttackDamage = new[] { 25.0f, 25.0f, 50.0f, 50.0f };
            itemAtts.AttackDamageBiasVehicles = 1;
            itemAtts.AttackDamageBiasBuildings = 0;
            itemAtts.AttackDamageBiasAircraft = 1;
            itemAtts.AttackDamageBias4 = 1;
            itemAtts.RateOfFire = new[] { 0.8f, 0.8f, 3.0f, 3.0f };
            itemAtts.ProjectileSpeed = new[] { 180.0f, 180.0f, 320.0f, 320.0f };
            itemAtts.ProjectileTypeToUse = new[]
                                                {
                                                    ProjectileType.WhiteBall, ProjectileType.WhiteBall,
                                                    ProjectileType.BlueBall, ProjectileType.BlueBall
                                                };
            itemAtts.StartingHealth = 200.0f;
            itemAtts.IgnoreOccupiedByFlag = IgnoreOccupiedBy.On;
            AddItemTypeAttributeToArray(ref itemAtts);


            // Add Bomber 1
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiBomber01;
            itemAtts.ItemGroupType = ItemGroupType.Buildings | ItemGroupType.Shields; // 11/6/2009
            itemAtts.ItemTurnSpeed = 0.5f;
            itemAtts.TurretTurnSpeed = 0.0f;
            itemAtts.HasSpawnBullet = new[] { true, true, true, true };
            itemAtts.MaxSpeed = 250.0f;
            itemAtts.ViewRadius = 700.0f;
            itemAtts.AttackRadius = 600.0f;
            itemAtts.AttackDamage = new[] { 20.0f, 20.0f, 35.0f, 35.0f };
            itemAtts.AttackDamageBiasVehicles = 0.20f;
            itemAtts.AttackDamageBiasBuildings = 1;
            itemAtts.AttackDamageBiasAircraft = 0;
            itemAtts.AttackDamageBias4 = 0;
            itemAtts.RateOfFire = new[] { 1.8f, 1.0f, 1.0f, 1.8f };
            itemAtts.ProjectileSpeed = new[] { 1500.0f, 1400.0f, 1400.0f, 1500.0f };
            itemAtts.ProjectileTypeToUse = new[]
                                                {
                                                    ProjectileType.RedBall, ProjectileType.RedBall,
                                                    ProjectileType.OrangeBall, ProjectileType.OrangeBall
                                                };
            itemAtts.StartingHealth = 2000.0f;
            itemAtts.IgnoreOccupiedByFlag = IgnoreOccupiedBy.On;
            itemAtts.FaceAttackie = true;
            itemAtts.Cost = 3000;
            itemAtts.AircraftMustCircle = true;
            itemAtts.UseAircraftUpDownAnimation = false; // 11/28/2009
            itemAtts.UseRockLeftRightAnimation = false; // 11/28/2009
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Bomber 6
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiBomber06;
            itemAtts.ItemGroupToAttack = ItemGroupType.Buildings | ItemGroupType.Shields; // 11/6/2009
            itemAtts.ItemTurnSpeed = 0.5f;
            itemAtts.TurretTurnSpeed = 0.0f;
            itemAtts.HasSpawnBullet = new[] { true, true, true, true };
            itemAtts.MaxSpeed = 200.0f;
            itemAtts.ViewRadius = 600.0f;
            itemAtts.AttackRadius = 500.0f;
            itemAtts.AttackDamage = new[] { 20.0f, 20.0f, 35.0f, 35.0f };
            itemAtts.AttackDamageBiasVehicles = 0.20f;
            itemAtts.AttackDamageBiasBuildings = 1;
            itemAtts.AttackDamageBiasAircraft = 0;
            itemAtts.AttackDamageBias4 = 0;
            itemAtts.RateOfFire = new[] { 1.8f, 1.0f, 1.0f, 1.8f };
            itemAtts.ProjectileSpeed = new[] { 1480.0f, 1280.0f, 1280.0f, 1480.0f };
            itemAtts.ProjectileTypeToUse = new[]
                                                {
                                                    ProjectileType.OrangeBall, ProjectileType.WhiteBall,
                                                    ProjectileType.WhiteBall, ProjectileType.OrangeBall
                                                };
            itemAtts.StartingHealth = 2000.0f;
            itemAtts.IgnoreOccupiedByFlag = IgnoreOccupiedBy.On;
            itemAtts.FaceAttackie = true;
            itemAtts.Cost = 3000;
            itemAtts.AircraftMustCircle = true;
            itemAtts.UseAircraftUpDownAnimation = false; // 11/28/2009
            itemAtts.UseRockLeftRightAnimation = false; // 11/28/2009
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Bomber 7
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiBomber07;
            itemAtts.ItemGroupToAttack = ItemGroupType.Airplanes; // 11/6/2009
            itemAtts.ItemTurnSpeed = 0.5f;
            itemAtts.TurretTurnSpeed = 0.0f;
            itemAtts.HasSpawnBullet = new[] { true, true, true, true };
            itemAtts.MaxSpeed = 350.0f;
            itemAtts.ViewRadius = 1500.0f;
            itemAtts.AttackRadius = 800.0f;
            itemAtts.AttackDamage = new[] { 25.0f, 25.0f, 75.0f, 75.0f };
            itemAtts.AttackDamageBiasVehicles = 1;
            itemAtts.AttackDamageBiasBuildings = 1;
            itemAtts.AttackDamageBiasAircraft = 1;
            itemAtts.AttackDamageBias4 = 1;
            itemAtts.RateOfFire = new[] { 0.8f, 0.8f, 8.5f, 8.5f };
            itemAtts.ProjectileSpeed = new[] { 180.0f, 180.0f, 200.0f, 200.0f };
            itemAtts.ProjectileTypeToUse = new[]
                                                {
                                                    ProjectileType.WhiteBall, ProjectileType.WhiteBall,
                                                    ProjectileType.RedBall, ProjectileType.RedBall
                                                };
            itemAtts.StartingHealth = 100.0f;
            itemAtts.IgnoreOccupiedByFlag = IgnoreOccupiedBy.On;
            itemAtts.FaceAttackie = true;
            itemAtts.AircraftMustCircle = false;
            AddItemTypeAttributeToArray(ref itemAtts);



            #endregion

            #region Defense Set

            // Set General Defense's atts first
            itemAttsDefault.ItemGroupType = ItemGroupType.Shields; // 10/5/2009
            itemAttsDefault.HasTurret = true;
            itemAttsDefault.HasSpawnBullet = new[] { true, false, false, false }; // 1/14/2011
            itemAttsDefault.StatusBarOffsetPosition2D = new Vector2(-50, -50);
            itemAttsDefault.FacingDirectionOffset = MathHelper.ToRadians(-90);
            itemAttsDefault.ItemTurnSpeed = 0.0f;
            itemAttsDefault.TurretTurnSpeed = 0.015f;
            itemAttsDefault.MaxSpeed = 0.0f;
            itemAttsDefault.ViewRadius = 500.0f;
            itemAttsDefault.AttackRadius = 500.0f;
            itemAttsDefault.AttackDamage= new[] { 15.0f, 15.0f, 0.0f, 0.0f }; // 1/14/2011
            itemAttsDefault.AttackDamageBiasVehicles = 1;
            itemAttsDefault.AttackDamageBiasBuildings = 1;
            itemAttsDefault.AttackDamageBiasAircraft = 1;
            itemAttsDefault.AttackDamageBias4 = 1;
            itemAttsDefault.StartingHealth = 1000.0f;
            itemAttsDefault.CurvePathShape = new[] { PathShape.Straight, PathShape.Straight, PathShape.Straight, PathShape.Straight }; // 1/14//2011
            itemAttsDefault.CurveMagnitude = new[] {0.2f, 0.2f, 1.0f, 1.0f}; // 1/14/2011
            itemAttsDefault.RateOfFire = new[] { 0.5f, 0.8f, 0f, 0f }; // 1/14/2011
            itemAttsDefault.ProjectileSpeed = new[] { 900.0f, 900f, 0f, 0f }; // 1/14/2011
            itemAttsDefault.ProjectileParticlesPerSecond = new[] { 5.0f, 5.0f, 0f, 0f };
            itemAttsDefault.ProjectileTypeToUse = new[]
                                                       {
                                                           ProjectileType.RedBall, ProjectileType.RedBall,
                                                           ProjectileType.WhiteBall, ProjectileType.WhiteBall
                                                       }; // 1/14/2011
            itemAttsDefault.IgnoreOccupiedByFlag = IgnoreOccupiedBy.On;
            itemAttsDefault.Cost = 800;
            itemAttsDefault.EnergyNeeded = 25;
            itemAttsDefault.ShowEnergyOffSymbol = true;
            itemAttsDefault.TimeToBuild = 20;           

            // Add Defense Turret 1  
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiAAGun01;
            itemAtts.ItemGroupToAttack = ItemGroupType.Airplanes; // 11/6/2009
            itemAtts.ProjectileTypeToUse = new[]
                                                       {
                                                           ProjectileType.OrangeBall, ProjectileType.WhiteBall,
                                                           ProjectileType.WhiteBall, ProjectileType.WhiteBall
                                                       }; // 1/14/2011
            itemAtts.TurretTurnSpeed = 0.15f;
            itemAtts.SoundsToPlay.SoundToPlayPrimaryFire = Sounds.PulseGun1;
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Defense Turret 2    
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiAAGun02;
            itemAtts.ItemGroupToAttack = ItemGroupType.Vehicles; // 11/6/2009
            itemAtts.ProjectileTypeToUse = new[]
                                                       {
                                                           ProjectileType.RedBall, ProjectileType.WhiteBall,
                                                           ProjectileType.WhiteBall, ProjectileType.WhiteBall
                                                       }; // 1/14/2011
            itemAtts.TurretTurnSpeed = 0.15f;
            itemAtts.SoundsToPlay.SoundToPlayPrimaryFire = Sounds.MachineGunB_Group;
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Defense Turret 4     
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiAAGun04;
            itemAtts.ItemGroupToAttack = ItemGroupType.Airplanes; // 11/6/2009
            itemAtts.TurretTurnSpeed = 0.15f;
            itemAtts.SoundsToPlay.SoundToPlayPrimaryFire = Sounds.PulseGun1;
            AddItemTypeAttributeToArray(ref itemAtts);

            // Add Defense Turret 5   
            itemAtts = itemAttsDefault; // Set default values.
            itemAtts.ItemType = ItemType.sciFiAAGun05;
            itemAtts.ItemGroupToAttack = ItemGroupType.Vehicles; // 11/6/2009
            itemAtts.TurretTurnSpeed = 0.15f;
            itemAtts.SoundsToPlay.SoundToPlayPrimaryFire = Sounds.PulseGun2;
            AddItemTypeAttributeToArray(ref itemAtts);


            #endregion

#if !XBOX360
            // Call Base Level Method to Save
            CreateItemTypeAttributesAndSave(game, "PlayableItemTypeAtts.sav",
                                                                        ItemTypeAtts);
#endif

        }


        /// <summary>
        /// Loads the <see cref="PlayableItemTypeAttributes"/> structures back into memory, from the XML file.
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        public static void LoadItemTypeAttributes(Game game)
        {

            // If XBOX360, then instead of loading data, which is incredibly slow due to the slow Serializing 
            // on the Combact framework, we will simply create the attributes directly.
#if XBOX360
            CreateItemTypeAttributesAndSave(game);
#else

            // 3/2/2009 - Check if forced rebuild wanted?
            if (_forceRebuildOfXMLFile)
            {
                CreateItemTypeAttributesAndSave(game);
                return;
            }

            // Call Base Level Method to Load
            List<PlayableItemTypeAttributes> tmpItemTypeAtts;
            if (LoadItemTypeAttributes(game, "PlayableItemTypeAtts.sav", out tmpItemTypeAtts, 49))
            {
                // Add each record back into the Dictionary Array
                var count = tmpItemTypeAtts.Count;
                for (var loop1 = 0; loop1 < count; loop1++)
                {
                    ItemTypeAtts.Add(tmpItemTypeAtts[loop1].ItemType, tmpItemTypeAtts[loop1]);
                }
            }
                // Load Failed, so let's recreate XML file.
            else
                CreateItemTypeAttributesAndSave(game);
#endif

        }


        // 12/23/2008 -  
        //              This is currently called from the 'CreateItemTypeAttributesAndSave' Method.
        /// <summary>
        /// Helper function to add <see cref="PlayableItemTypeAttributes"/> structs to the internal dictionary.
        /// </summary>
        /// <param name="itemTypeAttsToAdd"><see cref="PlayableItemTypeAttributes"/> structure to add</param>
        private static void AddItemTypeAttributeToArray(ref PlayableItemTypeAttributes itemTypeAttsToAdd)
        {

            ItemTypeAtts.Add(itemTypeAttsToAdd.ItemType, itemTypeAttsToAdd);

        }


        // Dispose 
        ///<summary>
        /// Disposes of unmanaged resources.
        ///</summary>
        public new static void Dispose()
        {
            if (ItemTypeAtts != null)
                ItemTypeAtts.Clear();

            ItemTypeAttributes.ItemTypeAtts.Dispose();
        }

    }
}
