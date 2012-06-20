#region File Description
//-----------------------------------------------------------------------------
// IVehicleSceneType.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using TWEngine.VehicleTypes;

namespace TWEngine.Interfaces
{
    ///<summary>
    /// The <see cref="VehicleSceneType"/> class is used to create vehicles which have four wheels.
    ///</summary>
    public interface IVehicleSceneType
    {
        ///<summary>
        /// Now we need to roll the vehicle's wheels "forward.", and to do this, we'll
        /// calculate how far they have rolled, and from there calculate how much
        /// they must have rotated.
        ///</summary>
        ///<param name="movement"><see cref="Vector3"/> movement</param>
        ///<param name="position"><see cref="Vector3"/> position</param>
        ///<param name="newPosition"><see cref="Vector3"/> new position</param>
        void UpdateWheelRoll(ref Microsoft.Xna.Framework.Vector3 movement, Microsoft.Xna.Framework.Vector3 position, ref Microsoft.Xna.Framework.Vector3 newPosition);

        ///<summary>
        /// The radius of the vehicle's wheels. This is used when we calculate how fast they
        /// should be rotating as the vehicle moves.
        ///</summary>
        float WheelRadius { get; set; }
    }
}