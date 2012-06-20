#region File Description
//-----------------------------------------------------------------------------
// TurretAttributes.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using TWEngine.Interfaces;

namespace TWEngine.SceneItems.Structs
{
    // 4/27/2010 - Update to be a structure, rather than class.
    ///<summary>
    /// Holds attributes for some gun turret for any <see cref="SceneItem"/>
    ///</summary>
    public struct TurretAttributes : ITurretAttributes
    {
        ///<summary>
        /// Current direction or angle turret is facing.
        ///</summary>
        public float TurretFacingDirection { get; set; }

        ///<summary>
        /// Current turn speed for turret
        ///</summary>
        public float TurretTurnSpeed { get; set; }

        ///<summary>
        /// Current desired angle for turret. 
        ///</summary>
        /// <remarks>Calculation comes from 'TurnToFace' method calls</remarks>
        public float TurretDesiredAngle { get; set; }
    }
}
