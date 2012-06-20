#region File Description
//-----------------------------------------------------------------------------
// VehicleScene.cs
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
using TWEngine.Interfaces;
using TWEngine.Shapes.Enums;
using TWEngine.Shapes;
using TWEngine.ForceBehaviors.Enums;
using TWEngine.Terrain;
using TWEngine.VehicleTypes;

namespace TWEngine.SceneItems
{
    sealed class VehicleScene : SceneItemWithPick, IVehicleSceneType
    {
        // 8/22/2008 - Hold Ref to ItemShape
        private readonly VehicleShape _vehicleShape;       
     
        // 7/4/2008
        private const float TurretFacingDirection = 0;
        private const float TurretDesiredAngle = 0;
       
        // Pointer to Terrain Scene
        private TerrainScene _terrain;       

        // 7/11/2008 - VehicleSceneType Class for Indirect Inheritance
        private VehicleSceneType _vehicleSceneType;
       

        #region Properties

        public float WheelRadius
        {
            get { return _vehicleSceneType.WheelRadius; }
            set { _vehicleSceneType.WheelRadius = value; }
        }

        
        #endregion

        #region Constructors
               

        /// <summary>
        /// Overload: This constructor creates an SceneItemOwner with a particular shape at a particular point
        /// </summary>
        public VehicleScene(Game game, VehicleType vehicleType, ref Vector3 initialPosition, ref Vector3 scaleFactor, 
                            ref TerrainScene terrainScene, byte playerNumber)
            : base(game, new VehicleShape(game, vehicleType), ref initialPosition, playerNumber)
        {
            _terrain = terrainScene;
            GameInstance = game;
           
            TerrainShape = (ITerrainShape)_terrain.ShapeItem;

            // Set pathNodePosition to Current PathNode Position
            Vector3 tmpPathNodePos = PathNodePosition;
            tmpPathNodePos.X = (int)(initialPosition.X / TemporalWars3DEngine._pathNodeStride);
            tmpPathNodePos.Y = initialPosition.Y;
            tmpPathNodePos.Z = (int)(initialPosition.Z / TemporalWars3DEngine._pathNodeStride);
            PathNodePosition = tmpPathNodePos;

            LastPosition = initialPosition;

            // Set A* OccupiedBy for Current PathNode
            if (!AStarItem.SetOccupiedByAtCurrentPosition(AStarItemI))
                throw new Exception("Set OccupiedBy At Current Position Failed!");

            // 11/24/2008 - Add new StatusBarItem instance
            if (StatusBar != null) StatusBar.AddNewStatusBarItem(this, out StatusBarItem);

            // Draw StatusBar?
            DrawStatusBar = true;
            // StatusBar OffsetPosition
            StatusBarOffsetPosition2D = new Vector2(-50, -50);
            // Speed tank can turn at
            ItemTurnSpeed = 0.25f;
            // Speed turret can turn at
            // Velocity tank can move at           
            MaxSpeed = 1.5f;
            // Set SceneItemOwner Facing Direction Offset in Degrees
            FacingDirectionOffset = -90; 
            // Set Collision Radius 
            CollisionRadius = 60.0f;
            // Set View Radius
            ViewRadius = 700.0f;
            // Set Attack Radius
            AttackRadius = 500.0f;
            // Set Attack Damage
            //AttackDamage = 20.0f;
            // Set Health
            StartingHealth = 200.0f;           
            // Set Type of Curve Path to use for Projectiles
            //curvePathShape = Particle3D.PathShape.ZigZagLeftRight;
            // Set Curve's Magnitude
            //curveMagnitude = 0.2f;
            // Set Rate of Fire
            //rateOfFire = 6.0f; // 6 seconds
            // Set Scale Factor
            scale = scaleFactor;
            // Set Selection Box Color
            SelectionBoxColor = Color.Yellow;
            // Set if we should Ignore the OccupiedBy Flag when A*
            AStarItemI.IgnoreOccupiedByFlag = IgnoreOccupiedBy.On;

            // 10/14/2008 - Set FogOfWar Radius, based on SceneItem ViewRadius Property
            if (ShapeItem.UseFogOfWar)
            {
                ShapeItem.FogOfWarHeight = (int)ViewRadius / TerrainData.cScale; 
                ShapeItem.FogOfWarWidth = (int)ViewRadius / TerrainData.cScale; 
            }
            
            // 8/15/2008 - Turn on use of ParticleSystem Projectiles
            UseProjectiles = true;
            //InitParticleSystems();
            
            // 7/11/2008 - Instantiate Class           
            _vehicleSceneType = new VehicleSceneType(((VehicleShape)ShapeItem).VehicleShapeType);

            // 8/22/2008 - Get Ref Vehicle Shape
            _vehicleShape = ((VehicleShape)ShapeItem);

            // 10/9/2008 - Add Orientation AbstractBehavior
            ForceBehaviors.Add(BehaviorsEnum.UpdateOrientation);
            

        }
        #endregion

        // 8/15/2008 - Dispose of Resourcses
        ///<summary>
        /// Disposes of unmanaged resources.
        ///</summary>
        ///<param name="finalDispose">Is this final dispose?</param>
        public override void Dispose(bool finalDispose)
        {
            // Dispose
            if (_vehicleSceneType != null)
                _vehicleSceneType.Dispose();

            // Null Refs           
            _terrain = null;           
            _vehicleSceneType = null;

            base.Dispose(finalDispose);
        }
       

        // 7/4/2008
        /// <summary>
        /// verrides base ShootBullets Method, by checking if Turret is facing right direction
        /// before shooting bullet.
        /// </summary>
        protected override void ShootBullets(ref TimeSpan elapsedTime)
        {
            // Turn Turret Toward Target
            float degreeDiff = TurnTurretToFace();
            
            // Apply the new TurretFacingDirection, while removing the FacingDirection of the tank, 
            // since the Tank can be rotating as well! - Ben
            _vehicleShape.TurretRotation = TurretFacingDirection - FacingDirection;
            
            // Add Projectiles to fire, when Turret is facing within 1 degrees of target!
            if (Math.Abs(degreeDiff) <= MathHelper.ToRadians(1.0f))
            {
                // Shoot Projectile Bullet
                UpdateProjectilesCreation(ref elapsedTime);                

            }

        }

        // 7/4/2008
        /// <summary>
        /// Removes SceneItem from Game, while calling some random Death Animation.
        /// </summary>
        public override void StartKillSceneItem(ref TimeSpan elapsedTime, int attackerPlayerNumber)
        {
            // TODO: Need to make a death animation for tank?
            //ChooseRandomDeathAnimationClip(SpacewarGame.ElapsedTime);

            IsAlive = false;
            // Remove OccupiedBy at our Old Position
            AStarItem.RemoveOccupiedByAtOldPosition(AStarItemI);
            
        }

        // 7/4/2008
        /// <summary>
        /// Starts to turn the SceneItemOwner's Turret using Position of itself 
        /// and faceThis as direction to turn to.
        /// </summary>
        private static float TurnTurretToFace()
        {
            // Ben - Needed to multiply the Z access by *-1 to flip the sign, which fixed
            //       the angle movement going in the wrong direction.
            //var pos = new Vector2 {X = position.X, Y = position.Z*-1};
            //var faceThis = new Vector2 {X = AttackSceneItem.Position.X, Y = AttackSceneItem.Position.Z*-1};

            return Math.Abs(TurretDesiredAngle) - Math.Abs(TurretFacingDirection);
        }


        #region IVehicleSceneType Interface Wrapper Methods

        public void UpdateWheelRoll(ref Vector3 movement, Vector3 inPosition, ref Vector3 newPosition)
        {
            _vehicleSceneType.UpdateWheelRoll(ref movement, inPosition, ref newPosition);
        }

        #endregion

    }
}
