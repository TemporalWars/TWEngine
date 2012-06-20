#region File Description
//-----------------------------------------------------------------------------
// VehicleShapeType.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using TWEngine.Interfaces;

namespace TWEngine.VehicleTypes
{
    // 7/11/2008 - The VehicleShape Interface Type
    ///<summary>
    /// The <see cref="VehicleShapeType"/> class is where the actual calculation for
    /// each of the bones, like the <see cref="AnimAttsVehicleShape.LeftBackWheelBone"/> or <see cref="AnimAttsVehicleShape.RightSteerBone"/>, take place. 
    ///</summary>
    public class VehicleShapeType : IVehicleShapeType, IDisposable
    {
        // we'll use this value when making the wheels roll. It's calculated based on 
        // the distance moved.
        Matrix _wheelRollMatrix = Matrix.Identity;

        // Reference to the Model
        Model _model;

        
        ///<summary>
        /// Keeps track of the <see cref="ModelBone"/> that control the wheels, and will manually
        /// set their transforms. 
        ///</summary>
        public struct AnimAttsVehicleShape
        {
#pragma warning disable 1591
            public ModelBone LeftBackWheelBone;

            public ModelBone RightBackWheelBone;
            public ModelBone LeftFrontWheelBone;
            public ModelBone RightFrontWheelBone;
            public ModelBone LeftSteerBone;
            public ModelBone RightSteerBone;
            public ModelBone TurretBone;

            public Matrix SteerRotation;
            public Matrix TurretRotation;

            public Matrix LeftBackWheelTransform;
            public Matrix RightBackWheelTransform;
            public Matrix LeftFrontWheelTransform;
            public Matrix RightFrontWheelTransform;
            public Matrix LeftSteerTransform;
            public Matrix RightSteerTransform;
            public Matrix TurretTransform;

            // Current animation positions.
            public float WheelRotationValue;
            public float OldWheelRotationValue;
            public float SteerRotationValue;
            public float OldSteerRotationValue;
            public float TurretRotationValue;
            public float OldTurretRotationValue;
#pragma warning restore 1591
        }
        internal AnimAttsVehicleShape AnimAttsVehicle;


        #region Properties

        /// <summary>
        /// Sets or gets the wheel roll <see cref="Matrix"/>
        /// </summary>
        public Matrix WheelRollMatrix
        {
            get { return _wheelRollMatrix; }
            set { _wheelRollMatrix = value; }

        }

        /// <summary>
        /// Sets or gets the wheel rotation value.
        /// </summary>
        public float WheelRotation
        {
            get { return AnimAttsVehicle.WheelRotationValue; }
            set { AnimAttsVehicle.WheelRotationValue = value; }
        }

        /// <summary>
        /// Gets or sets the steering rotation amount.
        /// </summary>
        public float SteerRotation
        {
            get { return AnimAttsVehicle.SteerRotationValue; }
            set { AnimAttsVehicle.SteerRotationValue = value; }
        }

        /// <summary>
        /// Gets or sets the turret rotation amount.
        /// </summary>
        public float TurretRotation
        {
            get { return AnimAttsVehicle.TurretRotationValue; }
            set { AnimAttsVehicle.TurretRotationValue = value; }
        }

        #endregion

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="inmodel"><see cref="Model"/> instance</param>
        public VehicleShapeType(ref Model inmodel)
        {
            _model = inmodel;
        }       

       
        /// <summary>
        /// Calcuates the bone transforms for the wheel and steering bones.
        /// </summary>
        public void CalculateBoneTransforms()
        {
            // 8/27/2008: Updated to use Matrix Ref Version to optimize memory.
            // Calculate matrices based on the current animation Position.            
            Matrix.CreateRotationY(AnimAttsVehicle.SteerRotationValue, out AnimAttsVehicle.SteerRotation);
            Matrix.CreateRotationY(AnimAttsVehicle.TurretRotationValue, out AnimAttsVehicle.TurretRotation);

            // Apply matrices to the relevant bones, as discussed in the Simple 
            // Animation Sample.
            if (AnimAttsVehicle.LeftBackWheelBone != null)
                AnimAttsVehicle.LeftBackWheelBone.Transform = _wheelRollMatrix * AnimAttsVehicle.LeftBackWheelTransform;
            if (AnimAttsVehicle.RightBackWheelBone != null)
                AnimAttsVehicle.RightBackWheelBone.Transform = _wheelRollMatrix * AnimAttsVehicle.RightBackWheelTransform;
            if (AnimAttsVehicle.LeftFrontWheelBone != null)
                AnimAttsVehicle.LeftFrontWheelBone.Transform = _wheelRollMatrix * AnimAttsVehicle.LeftFrontWheelTransform;
            if (AnimAttsVehicle.RightFrontWheelBone != null)
                AnimAttsVehicle.RightFrontWheelBone.Transform = _wheelRollMatrix * AnimAttsVehicle.RightFrontWheelTransform;
            if (AnimAttsVehicle.LeftSteerBone != null)
                AnimAttsVehicle.LeftSteerBone.Transform = AnimAttsVehicle.SteerRotation * AnimAttsVehicle.LeftSteerTransform;
            if (AnimAttsVehicle.RightSteerBone != null)
                AnimAttsVehicle.RightSteerBone.Transform = AnimAttsVehicle.SteerRotation * AnimAttsVehicle.RightSteerTransform;
            if (AnimAttsVehicle.TurretBone != null)
                AnimAttsVehicle.TurretBone.Transform = AnimAttsVehicle.TurretRotation * AnimAttsVehicle.TurretTransform;
        }
        
        /// <summary>
        /// Setup the references to the front wheel bones
        /// </summary>
        /// <param name="leftWheelName">Left front wheel bone</param>
        /// <param name="rightWheelName">Right front wheel bone</param>
        public void SetupFrontWheelBones(ref string leftWheelName, ref string rightWheelName)
        {
            AnimAttsVehicle.LeftFrontWheelBone = _model.Bones[leftWheelName];
            AnimAttsVehicle.RightFrontWheelBone = _model.Bones[rightWheelName];

            // Also, we'll store the original Transform matrix for each animating bone.
            AnimAttsVehicle.LeftFrontWheelTransform = AnimAttsVehicle.LeftFrontWheelBone.Transform;
            AnimAttsVehicle.RightFrontWheelTransform = AnimAttsVehicle.RightFrontWheelBone.Transform;
        }
        
        /// <summary>
        /// Setup the references to the back wheel bones
        /// </summary>
        /// <param name="leftWheelName">Left back wheel bone</param>
        /// <param name="rightWheelName">Right back wheel bone</param>
        public void SetupBackWheelBones(ref string leftWheelName, ref string rightWheelName)
        {
            AnimAttsVehicle.LeftBackWheelBone = _model.Bones[leftWheelName];
            AnimAttsVehicle.RightBackWheelBone = _model.Bones[rightWheelName];

            // Also, we'll store the original Transform matrix for each animating bone.
            AnimAttsVehicle.LeftBackWheelTransform = AnimAttsVehicle.LeftBackWheelBone.Transform;
            AnimAttsVehicle.RightBackWheelTransform = AnimAttsVehicle.RightBackWheelBone.Transform;
        }
        
        /// <summary>
        /// Setup the references to the steering bones
        /// </summary>
        /// <param name="leftSteerName">Left steering bone</param>
        /// <param name="rightSteerName">Right steering bone</param>
        public void SetupSteeringBones(ref string leftSteerName, ref string rightSteerName)
        {
            AnimAttsVehicle.LeftSteerBone = _model.Bones[leftSteerName];
            AnimAttsVehicle.RightSteerBone = _model.Bones[rightSteerName];

            // Also, we'll store the original Transform matrix for each animating bone.
            AnimAttsVehicle.LeftSteerTransform = AnimAttsVehicle.LeftSteerBone.Transform;
            AnimAttsVehicle.RightSteerTransform = AnimAttsVehicle.RightSteerBone.Transform;
        }
        
        /// <summary>
        /// Setup the references to the turret bones
        /// </summary>
        /// <param name="turretBoneName"></param>
        public void SetupTurretBone(ref string turretBoneName)
        {
            AnimAttsVehicle.TurretBone = _model.Bones[turretBoneName];

            // Also, we'll store the original Transform matrix for each animating bone.
            AnimAttsVehicle.TurretTransform = AnimAttsVehicle.TurretBone.Transform;
        }

        #region Dispose

        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this final dispose?</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources

                // Null Refs
                _model = null;
                AnimAttsVehicle.LeftBackWheelBone = null;
                AnimAttsVehicle.RightBackWheelBone = null;
                AnimAttsVehicle.LeftFrontWheelBone = null;
                AnimAttsVehicle.RightFrontWheelBone = null;
                AnimAttsVehicle.LeftSteerBone = null;
                AnimAttsVehicle.RightSteerBone = null;
                AnimAttsVehicle.TurretBone = null;
            }
            // free native resources
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
        
    }
}
