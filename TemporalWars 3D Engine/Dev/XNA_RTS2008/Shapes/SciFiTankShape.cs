#region File Description
//-----------------------------------------------------------------------------
// SciFiTankShape.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.Common;
using ImageNexus.BenScharbach.TWEngine.Explosions;
using ImageNexus.BenScharbach.TWEngine.Explosions.Structs;
using ImageNexus.BenScharbach.TWEngine.GameCamera;
using ImageNexus.BenScharbach.TWEngine.InstancedModels;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Structs;
using ImageNexus.BenScharbach.TWEngine.Interfaces;
using ImageNexus.BenScharbach.TWEngine.ItemTypeAttributes.Structs;
using ImageNexus.BenScharbach.TWEngine.Players;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWEngine.Shadows;
using ImageNexus.BenScharbach.TWEngine.Shapes.Enums;
using ImageNexus.BenScharbach.TWEngine.Utilities;
using ImageNexus.BenScharbach.TWEngine.VehicleTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWEngine.Shapes
{
    ///<summary>
    /// The <see cref="SciFiTankShape"/> class holds the actual artwork reference, to
    /// either an XNA <see cref="Model"/> or the <see cref="InstancedItem"/> model.  It
    /// provides the ability to 'Pick' the item, retrieve the item's World <see cref="Matrix"/>, 
    /// set the <see cref="ItemType"/>, and start an explosion, to name few.
    ///</summary>
    public sealed class SciFiTankShape : ShapeWithPick, IVehicleShapeType, IShadowShapeItem
    {
        // Add ShadowItem Class
        private ShadowItem _shadowItem;

        // Basic Tank Bones
        private string _turretName;
        private string _turretBaseName; // 3/24/2009

        // 3/24/2009 - Allows Artilery units to raise turret up.
        private float _turretRotationUp;

        /// <summary>
        /// Set when explosion animation started.
        /// </summary>
        internal bool ExplodeAnimStarted;

        /// <summary>
        /// The <see cref="ScenaryItemTypeAttributes"/> structure
        /// </summary>    
        private ScenaryItemTypeAttributes _itemTypeAtts;


        #region Properties

        /// <summary>
        /// Gets reference to the <see cref="VehicleShapeType"/> 
        /// </summary>
        public VehicleShapeType VehicleShapeType { get; private set; }

        ///<summary>
        /// Set or Get the <see cref="InstancedItemData"/> structure.
        ///</summary>
        public new InstancedItemData InstancedItemData
        {
            get { return ((Shape) this).InstancedItemData; }
            set { ((Shape) this).InstancedItemData = value; }
        }

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
        /// <see cref="ModelType"/> Enum.
        ///</summary>
        public override ModelType IsModelType
        {
            get { return _itemTypeAtts.modelType; }
            set { _itemTypeAtts.modelType = value; }
        }

        ///<summary>
        /// Does item contribute to path blocking for A*.
        ///</summary>
        public override bool IsPathBlocked
        {
            get { return _itemTypeAtts.usePathBlocking; }
            set { _itemTypeAtts.usePathBlocking = value; }
        }

        ///<summary>
        /// Path block size area to affect?
        ///</summary>
        /// <remarks>Requires the <see cref="Shape.IsPathBlocked"/> to be TRUE.</remarks>
        public override int PathBlockSize
        {
            get { return _itemTypeAtts.pathBlockValue; }
            set { _itemTypeAtts.pathBlockValue = value; }
        }

        #endregion

        #region IVehicleShapeType Interface Properties

        public Matrix WheelRollMatrix
        {
            get { return VehicleShapeType.WheelRollMatrix; }
            set { VehicleShapeType.WheelRollMatrix = value; }

        }

        /// <summary>
        /// Gets or sets the wheel rotation amount.
        /// </summary>
        public float WheelRotation
        {
            get { return VehicleShapeType.WheelRotation; }
            set { VehicleShapeType.WheelRotation = value; }
        }

        /// <summary>
        /// Gets or sets the steering rotation amount.
        /// </summary>
        public float SteerRotation
        {
            get { return VehicleShapeType.SteerRotation; }
            set { VehicleShapeType.SteerRotation = value; }
        }

        /// <summary>
        /// Gets or sets the turret rotation amount.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is outside allowable range of -pi to pi.</exception>
        public float TurretRotation
        {
            get { return VehicleShapeType.TurretRotation; }
            set
            {
                // 8/23/2009 - Make sure value given is in Radian measurement.
                const float pi = MathHelper.Pi;
                if (value < -pi || value > pi)
                    throw new ArgumentOutOfRangeException("value", @"Angle must be in Radian measurement; - pi to pi.");


                VehicleShapeType.TurretRotation = value;
            }
        }

        // 3/24/2009
        /// <summary>
        /// Gets or sets the raising of the turret for some units.
        /// </summary>
        public bool RaiseTurretUp { get; set; }

        // 3/24/2009 -
        ///<summary>
        /// Angle of turret on the x-axis.
        ///</summary>
        public float TurretRotationUp
        {
            get { return _turretRotationUp; }
        }

        #endregion

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

        ///<summary>
        /// Item in <see cref="Camera"/> frustrum?
        ///</summary>
        public bool InCameraFrustrum
        {
            get { return _shadowItem.InCameraFrustrum; }
            set { _shadowItem.InCameraFrustrum = value; }
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
        public SciFiTankShape(Game game, ItemType itemType, byte playerNumber)
            : base(game)
        {
            // Loads and Setups Model Atts
            SetupModelAnimAtts();           

            // Set InstanceKey
            ((Shape) this).InstancedItemData.ItemType = itemType;
            ((Shape) this).InstancedItemData.ItemInstanceKey = InstancedItem.GenerateItemInstanceKey(ref ((Shape) this).InstancedItemData);
            // Set PlayerNumber
            PlayerNumber = playerNumber;

            
            // 8/27/2009: Believe this to be a waste now!  This because the default 'ItemType' is always 334 or Tank#1, and it is 
            //            official created when the item 'SetInstancedITemTypeToUse' is called below.
            // Retrieve Attributes from Dictionary               
            /*if (ScenaryItemTypeAtts.ItemTypeAtts.TryGetValue(itemType, out _itemTypeAtts))
            { 
                // Create Model, using the Attributes info gathered for specific ItemType Enum.
                ((Shape) this).InstancedItemData.InstancedModel = InstancedItem.AddInstancedItem(itemType, ref _itemTypeAtts, true);
            }*/
           
        }

        // 2/23/2009
        /// <summary>
        /// Sets what <see cref="ItemType"/> to use for this <see cref="SciFiTankShape"/> instance; for
        /// example, 'SciFiTank1' or 'SciFiTank2' ItemType.
        /// </summary>
        /// <param name="itemType"><see cref="ItemType"/> to use</param>
        public override void SetInstancedItemTypeToUse(ItemType itemType)
        {
            // call base version to fill ItemTypeAtts Struct
            SetInstancedItemTypeToUse(itemType, out _itemTypeAtts);

            // 8/27/2009 - Connect this ShapeItem Ref, to the InstanceItem.
            InstancedItem.UpdatePlayableModelTransform(this, ref ((Shape)this).InstancedItemData, PlayerNumber);
        }


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

                /*Matrix tmpValue;
                Matrix tmpOrientation = Orientation;
                Matrix.Multiply(ref tmpOrientation, ref value, out tmpValue);*/
                
                InstancedItem.UpdatePlayableModelTransform(this, ref ((Shape) this).InstancedItemData, PlayerNumber);
               
            }
        }

        /// <summary>
        /// Loads and setups the <see cref="VehicleShapeType"/> animation attributes, 
        /// and creates the two structures; <see cref="ShadowItem"/> and <see cref="FogOfWarItem"/>.
        /// </summary>
        private void SetupModelAnimAtts()
        { 
            // Instantiate the VehicleShapeType Inherited Interface Class
            VehicleShapeType = new VehicleShapeType(ref ModelInstance);

            _turretName = "turret";
            _turretBaseName = "turretBase";

            //SetupBackWheelBones(ref backLeftWheelName, ref backRightWheelName);
            //SetupFrontWheelBones(ref frontLeftWheelName, ref frontRightWheelName);
            //SetupSteeringBones(ref leftSteerName, ref rightSteerName);
            //SetupTurretBone(ref _turretName);           

            // 7/8/2008 - ShadowItem
            {
                // ShadowItem            
                _shadowItem = new ShadowItem(Game, _itemTypeAtts.modelAnimates)
                                  {
                                      BoneTransforms = new Matrix[1],
                                      ModelCastShadow = true
                                  };
            }

            // 7/9/2008 - FogOfWarItem
            {
                // Create Instance of FogOfWarItem
                _fogOfWarItem = new FogOfWarItem();
                UseFogOfWar = true;
                _fogOfWarItem.FogOfWarWidth = 70; _fogOfWarItem.FogOfWarHeight = 70;
            }           
           
        }


        /// <summary>
        /// Updates the <see cref="SciFiTankShape"/> by calling the following method;
        /// InstancedItem.SetAdjustingBoneTransform.
        /// </summary>
        /// <param name="time"><see cref="TimeSpan"/> struct with time</param>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> struct with elapsed time since last call</param>
        public override void Update(ref TimeSpan time, ref TimeSpan elapsedTime)
        {
            
            // 11/18/2008 - Update Rotation Matrix when RotValue changes
            InstancedItem.SetAdjustingBoneTransform(ref ((Shape) this).InstancedItemData, _turretBaseName, RotationAxis.Y, VehicleShapeType.TurretRotation);

            // TODO: This should not be hard-coded.
            // 3/24/2009 - Raise Turret up for some units.
            if (ItemType == ItemType.sciFiArtilery01)
            {
                if (RaiseTurretUp)
                {
                    // Yes, so let's raise the turret up to 65 degrees angle.
                    if (_turretRotationUp > -65.0f)
                    {
                        _turretRotationUp -= 15.0f * (float)elapsedTime.TotalSeconds;

                        // 5/22/2009 - Updated to use the new overload version of 'SetAdjustingBoneTransform'.
                        //Matrix RotValue;
                        //Matrix.CreateRotationX(MathHelper.ToRadians(_turretRotationUp), out RotValue);
                        //InstancedItem.SetAdjustingBoneTransform(ref InstancedItemData, _turretName, ref RotValue);
                        InstancedItem.SetAdjustingBoneTransform(ref ((Shape) this).InstancedItemData, _turretName, RotationAxis.X, MathHelper.ToRadians(_turretRotationUp));
                    }

                }
                else
                {
                    // No, so lower it down, if already up.
                    if (_turretRotationUp < 0)
                    {
                        _turretRotationUp += 15.0f * (float)elapsedTime.TotalSeconds;

                        // 5/22/2009 - Updated to use the new overload version of 'SetAdjustingBoneTransform'.
                        //Matrix RotValue;
                        //Matrix.CreateRotationX(MathHelper.ToRadians(_turretRotationUp), out RotValue);
                        //InstancedItem.SetAdjustingBoneTransform(ref InstancedItemData, _turretName, ref RotValue);
                        InstancedItem.SetAdjustingBoneTransform(ref ((Shape) this).InstancedItemData, _turretName, RotationAxis.X, MathHelper.ToRadians(_turretRotationUp));
                    }

                }

            }

            base.Update(ref time, ref elapsedTime);
            
        }

        /// <summary>
        /// Overrides base render and left empty, since <see cref="InstancedModels"/> are
        /// rendered in batches.
        /// </summary>
        public override void Render()
        {
           return;
        }

        /// <summary>
        /// Overrides base Shape.IsMeshPicked method, to now check for <see cref="InstancedModel"/> picks.  
        /// </summary>
        /// <param name="intersectionDistance">(OUT) intersection distance</param>      
        public override bool IsMeshPicked(out float? intersectionDistance)
        {
            // If not InstanceModel, then call base 'IsMeshPicked' Method.
            return _itemTypeAtts.modelType != ModelType.InstanceModel ? base.IsMeshPicked(out intersectionDistance)
                : InstancedItem.IsMeshPicked(ref ((Shape)this).InstancedItemData, out intersectionDistance);
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
        /// <param name="scifiTankShape">this instance of <see cref="SciFiTankShape"/></param>
        /// <param name="sceneItemOwner"><see cref="SceneItemWithPick"/> owner of this shape</param>
        private static void DoStartExplosion(SciFiTankShape scifiTankShape, SceneItemWithPick sceneItemOwner)
        {
            // 5/20/2010 - Const amount of velocity applied to Y access; 0-100%.
            // 1/18/2011 - Velocity Y Height has default 30x in shader.
            const float velocityYPower = 6 * 30 * 0.50f; // Height
            const float velocityXPower = 0.10f; // 1/17/2011
            const float velocityZPower = 0.10f; // 1/17/2011

            var tmpOrientation = scifiTankShape.Orientation;
            var explosionsManager = TemporalWars3DEngine.ExplosionManager; // 4/30/2010 - Cache
            { // SceneItemOwner Turret
                // Set to Projectile Velocity
                var itemPiece1Velocity = new Vector3
                                             {
                                                 X = scifiTankShape.LastProjectileVelocity.X * 4 * velocityXPower,
                                                 Z = scifiTankShape.LastProjectileVelocity.Z * 4 * velocityZPower,
                                                 Y = velocityYPower
                                             };

                // 1/17/2011 - Add new Explosion Velocity
                InstancedItem.AddBoneExplosionVelocity(ref ((Shape)scifiTankShape).InstancedItemData, "turret", ref itemPiece1Velocity);

                // Create new ExplosionItem instance for each piece needed.                   
                var explosionItemPiece = new ExplosionItem(sceneItemOwner, "turret", ref itemPiece1Velocity, ref tmpOrientation, ref scifiTankShape.World, ref ((Shape)scifiTankShape).InstancedItemData);
                // Add to ExplosionManager
                explosionsManager.AddNewExplosionItem(ref explosionItemPiece);
            }

            { // SceneItemOwner Piece 1
                // Set to Projectile Velocity
                var itemPiece2Velocity = new Vector3
                                             {
                                                 X = scifiTankShape.LastProjectileVelocity.X * MathUtils.RandomBetween(2, 8) * velocityXPower,
                                                 Z = scifiTankShape.LastProjectileVelocity.Z * MathUtils.RandomBetween(-8, 18) * velocityZPower,
                                                 Y = velocityYPower
                                             };
                
                // 1/17/2011 - Add new Explosion Velocity
                InstancedItem.AddBoneExplosionVelocity(ref ((Shape)scifiTankShape).InstancedItemData, "Piece1", ref itemPiece2Velocity);

                // Create new ExplosionItem instance for each piece needed.                   
                var explosionItemPiece = new ExplosionItem(null, "Piece1", ref itemPiece2Velocity, ref tmpOrientation, ref scifiTankShape.World, ref ((Shape)scifiTankShape).InstancedItemData);
                // Add to ExplosionManager
                explosionsManager.AddNewExplosionItem(ref explosionItemPiece);
            }

            { // SceneItemOwner Piece 2
                // Set to Projectile Velocity
                var itemPiece3Velocity = new Vector3
                                             {
                                                 X = scifiTankShape.LastProjectileVelocity.X * MathUtils.RandomBetween(-8, 10) * velocityXPower,
                                                 Z = scifiTankShape.LastProjectileVelocity.Z * MathUtils.RandomBetween(2, 10) * velocityZPower,
                                                 Y = velocityYPower
                                             };
                
                // 1/17/2011 - Add new Explosion Velocity
                InstancedItem.AddBoneExplosionVelocity(ref ((Shape)scifiTankShape).InstancedItemData, "Piece2", ref itemPiece3Velocity);

                // Create new ExplosionItem instance for each piece needed.                  
                var explosionItemPiece = new ExplosionItem(null, "Piece2", ref itemPiece3Velocity, ref tmpOrientation, ref scifiTankShape.World, ref ((Shape)scifiTankShape).InstancedItemData);
                // Add to ExplosionManager
                explosionsManager.AddNewExplosionItem(ref explosionItemPiece);
            }

            { // SceneItemOwner Piece 3
                // Set to Projectile Velocity
                var itemPiece4Velocity = new Vector3
                                             {
                                                 X = scifiTankShape.LastProjectileVelocity.X * MathUtils.RandomBetween(-12, 8) * velocityXPower,
                                                 Z = scifiTankShape.LastProjectileVelocity.Z * MathUtils.RandomBetween(-15, 15) * velocityZPower,
                                                 Y = velocityYPower
                                             };
                
                // 1/17/2011 - Add new Explosion Velocity
                InstancedItem.AddBoneExplosionVelocity(ref ((Shape)scifiTankShape).InstancedItemData, "Piece3", ref itemPiece4Velocity);

                // Create new ExplosionItem instance for each piece needed.                   
                var explosionItemPiece = new ExplosionItem(null, "Piece3", ref itemPiece4Velocity, ref tmpOrientation, ref scifiTankShape.World, ref ((Shape)scifiTankShape).InstancedItemData);
                // Add to ExplosionManager
                explosionsManager.AddNewExplosionItem(ref explosionItemPiece);
            }

            // 4/7/2009
            var currentPosition = scifiTankShape.World.Translation;
            var lastProjectileVelocity = scifiTankShape.LastProjectileVelocity;
            ExplosionsManager.DoParticles_MediumExplosion(ref currentPosition, ref lastProjectileVelocity);
        }


        // 7/8/2008
        // To get around the C# Limitation of MultiInheritance, you inherit instead by calling the Interface Class
        // of the Class you want to use.  In this example, I inherited from the Interface Class 'IVehicleShape'.
        // C# will then force you to have all the same Method Implementations from the 'VehicleShape' 
        // Class to be contain inside this class, which I did below; however, rather than re-enter
        // all the code within each Method, you instead keep an Instance of the Actual 'VehicleShape'
        // Class, which is passed in through the constructor!
        // Then you can simply call the original methods inside these wrapper methods! - Ben

        #region IVehicleShape Interface Methods

        /// <summary>
        /// Calculates the bone transforms for the wheel bones.
        /// </summary>
        public void CalculateBoneTransforms()
        {
            // Call VehicleShape's Class method 'CalculateBoneTransforms'
            VehicleShapeType.CalculateBoneTransforms();
        }

        /// <summary>
        /// Stores the given front <paramref name="leftWheelName"/> and <paramref name="rightWheelName"/> turret bones.
        /// </summary>
        /// <param name="leftWheelName">Turret front left wheel name</param>
        /// <param name="rightWheelName">Turret front right wheel name</param>
        public void SetupFrontWheelBones(ref string leftWheelName, ref string rightWheelName)
        {
            VehicleShapeType.SetupFrontWheelBones(ref leftWheelName, ref rightWheelName);
        }

        /// <summary>
        /// Stores the given back <paramref name="leftWheelName"/> and <paramref name="rightWheelName"/> turret bones.
        /// </summary>
        /// <param name="leftWheelName">Turret back left wheel name</param>
        /// <param name="rightWheelName">Turret back right wheel name</param>
        public void SetupBackWheelBones(ref string leftWheelName, ref string rightWheelName)
        {
            VehicleShapeType.SetupBackWheelBones(ref leftWheelName, ref rightWheelName);
        }

        /// <summary>
        /// Stores the given <paramref name="leftSteerName"/> and <paramref name="rightSteerName"/> turret bones.
        /// </summary>
        /// <param name="leftSteerName">Turret left steer name</param>
        /// <param name="rightSteerName">Turret right steer name</param>
        public void SetupSteeringBones(ref string leftSteerName, ref string rightSteerName)
        {
            VehicleShapeType.SetupSteeringBones(ref leftSteerName, ref rightSteerName);
        }

        /// <summary>
        /// Stores the given <paramref name="turretBoneName"/> for the turret bone.
        /// </summary>
        /// <param name="turretBoneName">Turret bone name</param>
        public void SetupTurretBone(ref string turretBoneName)
        {
            VehicleShapeType.SetupTurretBone(ref turretBoneName);
        }

        #endregion

        #region IShadowItem Methods

        // 10/19/2008 - Override the ShadowMap Draw
        ///<summary>
        /// Draws the <see cref="IShadowShapeItem"/> using the <see cref="ShadowMap"/> shader, 
        /// which will project the shadow for this <see cref="SceneItem"/> onto the <see cref="ShadowMap"/>.        
        ///</summary>
        ///<param name="lightView">Light view <see cref="Matrix"/></param>
        ///<param name="lightProjection">Light projection <see cref="Matrix"/></param>
        public void DrawForShadowMap(ref Matrix lightView, ref Matrix lightProjection)
        {
            InstancedItem.DrawForShadowMap(ref ((Shape)this).InstancedItemData, ref lightView, ref lightProjection);

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

            if (VehicleShapeType != null)
                VehicleShapeType.Dispose();

            // Null refs
            VehicleShapeType = null;
            // free native resources
        }

        #endregion

               
    }
}
