#region File Description
//-----------------------------------------------------------------------------
// VehicleSceneType.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.Interfaces;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.VehicleTypes
{
    ///<summary>
    /// The <see cref="VehicleSceneType"/> class is used to create vehicles which have four wheels.
    ///</summary>
    public class VehicleSceneType : IVehicleSceneType, IDisposable
    {
        /// <summary>
        /// The radius of the vehicle's wheels. This is used when we calculate how fast they
        /// should be rotating as the vehicle moves.
        /// </summary>
        private float _wheelRadius = 18;
        
        /// <summary>
        /// Reference to <see cref="VehicleShapeType"/>
        /// </summary>
        private VehicleShapeType _vehicleShapeType;

        ///<summary>
        /// The radius of the vehicle's wheels. This is used when we calculate how fast they
        /// should be rotating as the vehicle moves.
        ///</summary>
        public float WheelRadius
        {
            get { return _wheelRadius; }
            set { _wheelRadius = value; }
        }

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="inShapeItem"><see cref="VehicleShapeType"/> instance</param>
        public VehicleSceneType(VehicleShapeType inShapeItem)
        {
            _vehicleShapeType = inShapeItem;
        }               
        
        ///<summary>
        /// Now we need to roll the vehicle's wheels "forward.", and to do this, we'll
        /// calculate how far they have rolled, and from there calculate how much
        /// they must have rotated.
        ///</summary>
        ///<param name="movement"><see cref="Vector3"/> movement</param>
        ///<param name="position"><see cref="Vector3"/> position</param>
        ///<param name="newPosition"><see cref="Vector3"/> new position</param>
        public void UpdateWheelRoll(ref Vector3 movement, Vector3 position, ref Vector3 newPosition)
        {
            float distanceMoved;
            Vector3.Distance(ref position, ref newPosition, out distanceMoved);
            var theta = distanceMoved / _wheelRadius;
            var rollDirection = movement.Z > 0 ? 1 : -1;

            _vehicleShapeType.WheelRollMatrix *= Matrix.CreateRotationX(theta * rollDirection);
        }

        #region Dispose

        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this final dispose?</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            // dispose managed resources
            if (_vehicleShapeType != null)
                _vehicleShapeType.Dispose();

            // Null Refs
            _vehicleShapeType = null;
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
