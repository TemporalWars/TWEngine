#region File Description
//-----------------------------------------------------------------------------
// IVehicleShapeType.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;

namespace TWEngine.Interfaces
{
    ///<summary>
    /// The <see cref="IVehicleShapeType"/> Interfaces adds the required properties and methods
    /// to define the bones required for a vehicle item.
    ///</summary>
    public interface IVehicleShapeType
    {
        /// <summary>
        /// Calculates the Bone Transforms for the Wheel Bones.
        /// </summary>
        void CalculateBoneTransforms();
        
        /// <summary>
        /// Stores the given back <paramref name="leftWheelName"/> and <paramref name="rightWheelName"/> turret bones.
        /// </summary>
        /// <param name="leftWheelName">Turret back left wheel name</param>
        /// <param name="rightWheelName">Turret back right wheel name</param>
        void SetupBackWheelBones(ref string leftWheelName, ref string rightWheelName);
        
        /// <summary>
        /// Stores the given front <paramref name="leftWheelName"/> and <paramref name="rightWheelName"/> turret bones.
        /// </summary>
        /// <param name="leftWheelName">Turret front left wheel name</param>
        /// <param name="rightWheelName">Turret front right wheel name</param>
        void SetupFrontWheelBones(ref string leftWheelName, ref string rightWheelName);
        
        /// <summary>
        /// Stores the given <paramref name="leftSteerName"/> and <paramref name="rightSteerName"/> turret bones.
        /// </summary>
        /// <param name="leftSteerName">Turret left steer name</param>
        /// <param name="rightSteerName">Turret right steer name</param>
        void SetupSteeringBones(ref string leftSteerName, ref string rightSteerName);
        
        /// <summary>
        /// Stores the given <paramref name="turretBoneName"/> for the turret bone.
        /// </summary>
        /// <param name="turretBoneName">Turret bone name</param>
        void SetupTurretBone(ref string turretBoneName);
        
        /// <summary>
        /// Sets or gets the steering rotation value
        /// </summary>
        float SteerRotation { get; set; }

        /// <summary>
        /// Sets or get the turret rotation value.
        /// </summary>
        float TurretRotation { get; set; }

        /// <summary>
        /// Sets or gets the wheel roll <see cref="Matrix"/>
        /// </summary>
        Matrix WheelRollMatrix { get; set; }

        /// <summary>
        /// Sets or gets the wheel rotation value.
        /// </summary>
        float WheelRotation { get; set; }
    }
}