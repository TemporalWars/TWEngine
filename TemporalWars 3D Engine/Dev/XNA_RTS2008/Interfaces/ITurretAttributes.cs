#region File Description
//-----------------------------------------------------------------------------
// ITurretAttributes.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.SceneItems;

namespace ImageNexus.BenScharbach.TWEngine.Interfaces
{
    ///<summary>
    /// Holds attributes for some gun turret for any <see cref="SceneItem"/>
    ///</summary>
    public interface ITurretAttributes
    {
        ///<summary>
        /// Current desired angle for turret. 
        ///</summary>
        /// <remarks>Calculation comes from 'TurnToFace' method calls</remarks>
        float TurretDesiredAngle { get; set; }

        ///<summary>
        /// Current direction or angle turret is facing.
        ///</summary>
        float TurretFacingDirection { get; set; }

        ///<summary>
        /// Current turn speed for turret
        ///</summary>
        float TurretTurnSpeed { get; set; }
    }
}