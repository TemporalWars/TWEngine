#region File Description
//-----------------------------------------------------------------------------
// TorchParticleSystem.cs
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
    /// Custom particle system for creating a torch flame effect.
    /// </summary>
    sealed class TorchParticleSystem : ParticleSystem
    {
        public TorchParticleSystem(Game game, ContentManager content) : base(game, content)
        {
        }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "fire";

            settings.MaxParticles = 500; // was 2400

            settings.Duration = TimeSpan.FromSeconds(2.5f);

            // Add Frequency - Updated to use the 'ParticleFrequencyEmitter' class.
            settings.Frequency = TimeSpan.FromSeconds(2.500f);
            settings.ParticlesPerFrequency = 75;
            settings.UseFrequency = true;

            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 5;

            settings.MinVerticalVelocity = 1;
            settings.MaxVerticalVelocity = 10;

            // Set gravity upside down, so the flames will 'fall' upward.
            settings.Gravity = new Vector3(0, -15, 0);

            settings.MinColor = new Color(255, 255, 255, 10);
            settings.MaxColor = new Color(255, 255, 255, 40);

            settings.MinStartSize = 25;
            settings.MaxStartSize = 25;

            settings.MinEndSize = 5;
            settings.MaxEndSize = 5;

            // XNA 4.0 Updates
            // Use additive blending.
            //settings.SourceBlend = Blend.SourceAlpha;
            //settings.DestinationBlend = Blend.One;
            settings.BlendState = BlendState.Additive;
        }
    }
}


