#region File Description
//-----------------------------------------------------------------------------
// VehicleShape.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.Common;
using ImageNexus.BenScharbach.TWEngine.GameCamera;
using ImageNexus.BenScharbach.TWEngine.InstancedModels;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.Interfaces;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWEngine.SceneItems.Enums;
using ImageNexus.BenScharbach.TWEngine.Shadows;
using ImageNexus.BenScharbach.TWEngine.Shapes.Enums;
using ImageNexus.BenScharbach.TWEngine.VehicleTypes;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.Shapes
{
    ///<summary>
    /// The <see cref="VehicleShape"/> class holds the actual artwork reference, to
    /// either an XNA <see cref="Model"/> or the <see cref="InstancedItem"/> model.  It
    /// provides the ability to 'Pick' the item, retrieve the item's World <see cref="Matrix"/>, 
    /// set the <see cref="ItemType"/>, and start an explosion, to name few.
    ///</summary>
    sealed class VehicleShape : ShapeWithPick, IVehicleShapeType, IShadowShapeItem
    {
        // DEBUG: 
        /// <summary>
        /// Holds <see cref="ItemStates"/>, which is set from <see cref="SceneItemWithPick"/> 'Render' method.
        /// Used in the Render method below to show <see cref="SceneItem"/> in red when pathfinding!
        /// </summary>
        /// <remarks>Debug purposes only.</remarks>
        public ItemStates ItemState;
        
        /// <summary>
        /// Store original diffuse colors for <see cref="VehicleShape"/>
        /// </summary>
        private List<Vector3> _diffuseColor = new List<Vector3>();

        // 8/27/2008 - Bone Names
        string _backLeftWheelName;
        string _backRightWheelName;
        string _frontLeftWheelName;
        string _frontRightWheelName;
        string _leftSteerName;
        string _rightSteerName;
        string _turretName;

        // XNA 4.0 Updates - RenderState replaced with 4 new states; BlendState, RasterizerState, DepthStencilState, or SampleState.
        private DepthStencilState _depthStencilState;

        // Array holding all the bone Transform matrices for the entire model.
        // We could just allocate this locally inside the Draw method, but it
        // is more efficient to reuse a single array, as this avoids creating
        // unnecessary garbage.
        Matrix[] _boneTransforms;

        // 7/8/2008 - Add ShadowItem Class for Indirect Inheritance
        private ShadowItem _shadowItem;
       
// ReSharper disable UnaccessedField.Local
        private bool _useFogOfWar;
// ReSharper restore UnaccessedField.Local

        // 7/11/2008 - Add VehicleShapeType Class for Indirect Inheritance
        private VehicleShapeType _vehicleShapeType;

        #region Properties

        #region IVehicleShapeType Interface Properties

        /// <summary>
        /// Sets or gets the wheel roll <see cref="Matrix"/>
        /// </summary>
        public Matrix WheelRollMatrix
        {
            get { return _vehicleShapeType.WheelRollMatrix; }
            set { _vehicleShapeType.WheelRollMatrix = value; }
        }

        /// <summary>
        /// Gets or sets the wheel rotation amount.
        /// </summary>
        public float WheelRotation
        {
            get { return _vehicleShapeType.WheelRotation; }
            set { _vehicleShapeType.WheelRotation = value; }
        }

        /// <summary>
        /// Gets or sets the steering rotation amount.
        /// </summary>
        public float SteerRotation
        {
            get { return _vehicleShapeType.SteerRotation; }
            set { _vehicleShapeType.SteerRotation = value; }
        }

        /// <summary>
        /// Gets or sets the turret rotation amount.
        /// </summary>
        public float TurretRotation
        {
            get { return _vehicleShapeType.TurretRotation; }
            set { _vehicleShapeType.TurretRotation = value; }
        }

        /// <summary>
        /// Gets reference to the <see cref="VehicleShapeType"/> 
        /// </summary>
        public VehicleShapeType VehicleShapeType
        {
            get { return _vehicleShapeType; }
        }

        #endregion

        /// <summary>
        /// Collection holding all the bone Transform matrices for the entire model.
        /// We could just allocate this locally inside the Draw method, but it
        /// is more efficient to reuse a single array, as this avoids creating
        /// unnecessary heap garbage.
        /// </summary>
        public Matrix[] BoneTransforms
        {
            get { return _boneTransforms; }
            set { _boneTransforms = value; }
        }
        
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

        /// <summary>
        /// Constructor, which creates a new <see cref="ContentManager"/>, then
        /// calls the internal <see cref="LoadModel"/> method while passing in the given
        /// <see cref="VehicleType"/> Enum.
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        /// <param name="vehicleType"><see cref="VehicleType"/> Enum to load</param>
        public VehicleShape(Game game, VehicleType vehicleType)
            : base(game)
        {
            LoadModel(vehicleType);
        }

        /// <summary>
        /// Constructor, which creates a new <see cref="ContentManager"/>.
        /// </summary>
        /// <param name="game"></param>
        public VehicleShape(Game game)
            : base(game)
        {
           // Empty
            
        }
       

        // 7/8/2008
        /// <summary>
        /// Loads the given <see cref="VehicleType"/> type
        /// </summary>
        /// <param name="vehicleType"><see cref="VehicleType"/> Enm to Load</param>
        public void LoadModel(VehicleType vehicleType)
        {
            switch (vehicleType)
            {
                case VehicleType.Humvee:
                    ModelInstance = TemporalWars3DEngine.ContentMisc.Load<Model>(@"Models\Vehicles\Humvee\humveeTest");
                    break;

                default:
                    break;
            }

            // 7/11/2008 - Instatiate the VehicleShapeType Inherited Interface Class
            _vehicleShapeType = new VehicleShapeType(ref ModelInstance);

            _backLeftWheelName = "LeftBackWheelBone";
            _backRightWheelName = "RightBackWheelBone";
            _frontLeftWheelName = "LeftFrontWheelBone";
            _frontRightWheelName = "RightFrontWheelBone";
            _leftSteerName = "LeftSteerBone";
            _rightSteerName = "RightSteerBone";
            _turretName = "turret";

            SetupBackWheelBones(ref _backLeftWheelName, ref _backRightWheelName);
            SetupFrontWheelBones(ref _frontLeftWheelName, ref _frontRightWheelName);
            SetupSteeringBones(ref _leftSteerName, ref _rightSteerName);
            SetupTurretBone(ref _turretName);

            // Allocate the Transform matrix array.
            _boneTransforms = new Matrix[ModelInstance.Bones.Count];


            // 7/8/2008 - ShadowItem
            {
                // Create Instance of ShadowItem
                _shadowItem = new ShadowItem(Game, true);
                // Store Original Effect for Current Model into ShadowItem Class
                _shadowItem.StoreModelEffect(ref ModelInstance, false);
                // Store ref to Model
                _shadowItem.Model = ModelInstance;
                // Does Model Cast Shadow
                _shadowItem.ModelCastShadow = true;
                // Allocate the Transform matrix array.
                _shadowItem.BoneTransforms = new Matrix[ModelInstance.Bones.Count];
            }

            // 7/9/2008 - FogOfWarItem
            {
                // Create Instance of FogOfWarItem
                _fogOfWarItem = new FogOfWarItem();
                _useFogOfWar = true;
                _fogOfWarItem.FogOfWarWidth = 30; _fogOfWarItem.FogOfWarHeight = 30;
            }

            // XNA 4.0 Updates
            _depthStencilState = new DepthStencilState { DepthBufferEnable = true, DepthBufferWriteEnable = true };

        }


        /// <summary>
        /// Renders the <see cref="VehicleShape"/>. 
        /// </summary>
        public override void Render()
        {
            // XNA 4.0 Updates - RenderState replaced with 4 new states; BlendState, RasterizerState, DepthStencilState, or SampleState.
            // 3/5/2008 : Needed if SpriteBatch done to keep Shape from being see-through!
            //TemporalWars3DEngine.GameInstance.GraphicsDevice.RenderState.DepthBufferEnable = true;
            //TemporalWars3DEngine.GameInstance.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            TemporalWars3DEngine.GameInstance.GraphicsDevice.DepthStencilState = _depthStencilState;

            // XNA 4.0 Updates - These states no longer required.
            //TemporalWars3DEngine.GameInstance.GraphicsDevice.RenderState.AlphaBlendEnable = false;
            //TemporalWars3DEngine.GameInstance.GraphicsDevice.RenderState.AlphaTestEnable = false;

            CalculateBoneTransforms();

            // now that we've updated the wheels' transforms, we can create an array
            // of absolute transforms for all of the bones, and then use it to draw.
            ModelInstance.CopyAbsoluteBoneTransformsTo(_boneTransforms);

        }

        #region IShadowItem Methods
           
        ///<summary>
        /// Draws the <see cref="IShadowShapeItem"/> using the <see cref="ShadowMap"/> shader, 
        /// which will project the shadow for this <see cref="SceneItem"/> onto the <see cref="ShadowMap"/>.        
        ///</summary>
        ///<param name="lightView">Light view <see cref="Matrix"/></param>
        ///<param name="lightProjection">Light projection <see cref="Matrix"/></param>
        public void DrawForShadowMap(ref Matrix lightView, ref Matrix lightProjection)
        {
            CalculateBoneTransforms();

            // Set ShadowItem Attributes
            _shadowItem.WorldP = (Orientation * World);
            // Call ShadowItem DrawForShadowMap method
            _shadowItem.DrawForShadowMap(ref lightView, ref lightProjection);
        }

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


        #region IVehicleShapeType Interface Wrapper Methods

        /// <summary>
        /// Calculates the Bone Transforms for the Wheel Bones.
        /// </summary>
        public void CalculateBoneTransforms()
        {
            _vehicleShapeType.CalculateBoneTransforms();
        }

        /// <summary>
        /// Stores the given front <paramref name="leftWheelName"/> and <paramref name="rightWheelName"/> turret bones.
        /// </summary>
        /// <param name="leftWheelName">Turret front left wheel name</param>
        /// <param name="rightWheelName">Turret front right wheel name</param>
        public void SetupFrontWheelBones(ref string leftWheelName, ref string rightWheelName)
        {
            _vehicleShapeType.SetupFrontWheelBones(ref leftWheelName, ref rightWheelName);
        }

        /// <summary>
        /// Stores the given back <paramref name="leftWheelName"/> and <paramref name="rightWheelName"/> turret bones.
        /// </summary>
        /// <param name="leftWheelName">Turret back left wheel name</param>
        /// <param name="rightWheelName">Turret back right wheel name</param>
        public void SetupBackWheelBones(ref string leftWheelName, ref string rightWheelName)
        {
            _vehicleShapeType.SetupBackWheelBones(ref leftWheelName, ref rightWheelName);
        }

        /// <summary>
        /// Stores the given <paramref name="leftSteerName"/> and <paramref name="rightSteerName"/> turret bones.
        /// </summary>
        /// <param name="leftSteerName">Turret left steer name</param>
        /// <param name="rightSteerName">Turret right steer name</param>
        public void SetupSteeringBones(ref string leftSteerName, ref string rightSteerName)
        {
            _vehicleShapeType.SetupSteeringBones(ref leftSteerName, ref rightSteerName);
        }

        /// <summary>
        /// Stores the given <paramref name="turretBoneName"/> for the turret bone.
        /// </summary>
        /// <param name="turretBoneName">Turret bone name</param>
        public void SetupTurretBone(ref string turretBoneName)
        {
            _vehicleShapeType.SetupTurretBone(ref turretBoneName);
        }

        #endregion

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public override void Dispose()
        {
            Dispose(true);
            base.Dispose();
        }

        // 4/5/2009 - Dispose of resources
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="all">Is this final dispose?</param>
        private void Dispose(bool all)
        {
            if (!all) return;

            // 5/3/2009
            _shadowItem.Dispose();

            // Dispose of Resources              
            if (_vehicleShapeType != null)
                _vehicleShapeType.Dispose();

            // Arrays
            if (_diffuseColor != null)
                _diffuseColor.Clear();

            // Null Refs
            _diffuseColor = null;

            _vehicleShapeType = null;
            _boneTransforms = null;
        }
    }
}
