#region File Description
//-----------------------------------------------------------------------------
// SmokePlumeParticleSystem.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWTools.Particles3DComponentLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ImageNexus.BenScharbach.TWEngine.Particles.ParticleSystems
{
    /// <summary>
    /// Custom particle system for creating a giant plume of long lasting smoke.
    /// </summary>
    public sealed class SmokePlumeParticleSystem : ParticleSystem
    {
        public SmokePlumeParticleSystem(Game game, ContentManager content) : base(game, content)
        {
        }
       

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "smoke";

            settings.MaxParticles = 600;

            settings.Duration = TimeSpan.FromSeconds(10);

            // 4/27/2009 - Add Frequency - Updated to use the 'ParticleFrequencyEmitter' class.
            settings.Frequency = TimeSpan.FromSeconds(4.0f);
            settings.ParticlesPerFrequency = 75;
            settings.UseFrequency = true;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 15;

            settings.MinVerticalVelocity = 10;
            settings.MaxVerticalVelocity = 20;

            // Create a wind effect by tilting the gravity vector sideways.
            settings.Gravity = new Vector3(-20, -5, 0);

            // 1/16/2010 - Test color
            settings.MinColor = Color.White;
            settings.MaxColor = Color.WhiteSmoke;

            settings.EndVelocity = 0.15f; // 0.75

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 5;
            settings.MaxStartSize = 10;

            settings.MinEndSize = 50;
            settings.MaxEndSize = 200;
        }
    }
}


