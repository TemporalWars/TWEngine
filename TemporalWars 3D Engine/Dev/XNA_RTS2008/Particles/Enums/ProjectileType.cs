#region File Description
//-----------------------------------------------------------------------------
// ProjectileType.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.Particles.ParticleSystems;

namespace ImageNexus.BenScharbach.TWEngine.Particles.Enums
{
    // 12/27/2008
    ///<summary>
    /// The <see cref="ProjectileType"/> Enum allows settings the type of energy ball to use.
    ///</summary>
    public enum ProjectileType
    {
        ///<summary>
        /// Uses the <see cref="BallParticleSystemWhite"/> type.
        ///</summary>
        WhiteBall,
        ///<summary>
        /// Uses the <see cref="BallParticleSystemRed"/> type.
        ///</summary>
        RedBall,
        ///<summary>
        /// Uses the <see cref="BallParticleSystemBlue"/> type.
        ///</summary>
        BlueBall,
        ///<summary>
        /// Uses the <see cref="BallParticleSystemOrange"/> type.
        ///</summary>
        OrangeBall
    }
}


