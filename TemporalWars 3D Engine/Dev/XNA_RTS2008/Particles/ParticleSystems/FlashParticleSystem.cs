#region File Description
//-----------------------------------------------------------------------------
// FlashParticleSystem.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWTools.Particles3DComponentLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWEngine.Particles.ParticleSystems
{
    /// <summary>
    /// Custom particle system for creating a quick gun flash.
    /// </summary>
    sealed class FlashParticleSystem : ParticleSystem
    {
        public FlashParticleSystem(Game game, ContentManager content) : base(game, content)
        {
        }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "particle";

            settings.MaxParticles = 900; // was 100

            settings.Duration = TimeSpan.FromSeconds(0.2f);
            settings.DurationRandomness = 1;

            // Add Frequency - Updated to use the 'ParticleFrequencyEmitter' class.
            settings.Frequency = TimeSpan.FromSeconds(0.0f);
            settings.ParticlesPerFrequency = 5;
            settings.UseFrequency = true;

            settings.MinHorizontalVelocity = -10;
            settings.MaxHorizontalVelocity = 10;

            settings.MinVerticalVelocity = -20;
            settings.MaxVerticalVelocity = 20;

            settings.EndVelocity = 0;

            settings.MinColor = Color.Goldenrod;
            settings.MaxColor = Color.OrangeRed;

            settings.MinRotateSpeed = 0;
            settings.MaxRotateSpeed = 0;

            settings.MinStartSize = 40;
            settings.MaxStartSize = 45;

            settings.MinEndSize = 100;
            settings.MaxEndSize = 125;

            // XNA 4.0 Updates
            // Use additive blending.
            //settings.SourceBlend = Blend.SourceAlpha;
            //settings.DestinationBlend = Blend.One;
            settings.BlendState = BlendState.Additive;
        }
    }
}


