#region File Description
//-----------------------------------------------------------------------------
// FireParticleSystem.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Particles3DComponentLibrary;

namespace TWEngine.Particles.ParticleSystems
{
    /// <summary>
    /// Custom particle system for creating a flame effect.
    /// </summary>
    sealed class FireParticleSystem : ParticleSystem
    {
        public FireParticleSystem(Game game, ContentManager content) : base(game, content)
        {
        }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "fire";

            settings.MaxParticles = 500; // was 2400

            settings.Duration = TimeSpan.FromSeconds(2);

            // Add Frequency - Updated to use the 'ParticleFrequencyEmitter' class.
            settings.Frequency = TimeSpan.FromSeconds(0.250f);
            settings.ParticlesPerFrequency = 20;
            settings.UseFrequency = true;

            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 15;

            settings.MinVerticalVelocity = -10;
            settings.MaxVerticalVelocity = 10;

            // Set gravity upside down, so the flames will 'fall' upward.
            settings.Gravity = new Vector3(0, 15, 0);

            settings.MinColor = new Color(255, 255, 255, 10);
            settings.MaxColor = new Color(255, 255, 255, 40);

            settings.MinStartSize = 5;
            settings.MaxStartSize = 10;

            settings.MinEndSize = 10;
            settings.MaxEndSize = 40;

            // XNA 4.0 Updates
            // Use additive blending.
            //settings.SourceBlend = Blend.SourceAlpha;
            //settings.DestinationBlend = Blend.One;
            settings.BlendState = BlendState.Additive;
        }
    }
}


