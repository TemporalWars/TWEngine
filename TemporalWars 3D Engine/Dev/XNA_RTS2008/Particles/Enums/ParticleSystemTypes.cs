#region File Description
//-----------------------------------------------------------------------------
// ParticleSystemTypes.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using ImageNexus.BenScharbach.TWTools.Particles3DComponentLibrary;

namespace ImageNexus.BenScharbach.TWEngine.Particles.Enums
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.Particles.Enums"/> namespace contains the enumerations
    /// which make up the entire <see cref="TWEngine.TWEngine.Particles.Enumsnent.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    // 10/16/2012: Updated enum to inherit from short value.
    // 7/7/2009 - Particles Enum list
    // Count = 18.
    /// <summary>
    /// The <see cref="ParticleSystemTypes"/> Enum is used to specify the type
    /// of <see cref="ParticleSystem"/> to generate.
    /// </summary>
    public enum ParticleSystemTypes : short
    {
        ///<summary>
        /// Creates the fiery part of the explosions.
        ///</summary>
        ExplosionParticleSystem,
        ///<summary>
        /// Creates a small smokey part of the explosions
        ///</summary>
        SmallExplosionSmokeParticleSystem,
        ///<summary>
        /// Creates a medium smokey part of the explosions
        ///</summary>
        MediumExplosionSmokeParticleSystem,
        ///<summary>
        /// Creates a large smokey part of the explosions
        ///</summary>
        LargeExplosionSmokeParticleSystem,
        ///<summary>
        /// Creates smoke trails behind the rocket projectiles
        ///</summary>
        ProjectileTrailParticleSystem,
        ///<summary>
        /// Creates a giant plume of long lasting smoke.
        ///</summary>
        SmokePlumeParticleSystem,
        ///<summary>
        /// Creates a flame effect
        ///</summary>
        FireParticleSystem,
        ///<summary>
        /// creating rain drops effect
        ///</summary>
        RainParticleSystem,
        ///<summary>
        /// Creates a round ball of white energy.
        ///</summary>
        BallParticleSystemWhite,
        ///<summary>
        /// Creates a round ball of red energy.
        ///</summary>
        BallParticleSystemRed,
        ///<summary>
        /// Creates a round ball of blue energy.
        ///</summary>
        BallParticleSystemBlue,
        ///<summary>
        /// Creates a round ball of orange energy.
        ///</summary>
        BallParticleSystemOrange,
        ///<summary>
        /// Creates a giant plume of Dust.
        ///</summary>
        DustPlumeParticleSystem,
        ///<summary>
        /// Creates a torch flame effect.
        ///</summary>
        TorchParticleSystem,
        ///<summary>
        /// Creates a quick gun flash
        ///</summary>
        FlashParticleSystem,
        ///<summary>
        /// Creates a black plume smoke.
        ///</summary>
        BlackSmokePlumeParticleSystem,
        ///<summary>
        /// Creates a blue plume smoke.
        ///</summary>
        BlueSmokePlumeParticleSystem,
        ///<summary>
        /// Creates a jet fuel fire effect.
        ///</summary>
        GunshipParticleSystem,
        /// <summary>
        /// Creates snow flake effect.
        /// </summary>
        SnowParticleSystem // 3/1/2011
    }
}


