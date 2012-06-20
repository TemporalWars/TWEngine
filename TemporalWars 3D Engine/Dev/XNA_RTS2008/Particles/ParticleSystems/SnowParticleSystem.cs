#region File Description
//-----------------------------------------------------------------------------
// SnowParticleSystem.cs
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
    // 3/1/2011
    /// <summary>
    /// Custom particle system for creating snow flake effect.
    /// </summary>
    sealed class SnowParticleSystem : ParticleSystem
    {
        public SnowParticleSystem(Game game, ContentManager content) : base(game, content)
        {
        }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "snowFlake";

            settings.MaxParticles = 20000; //   

            // Allows Random Position generation within EmitRange.
            // Must add 'ParticleFrequencyEmitter' instance in the
            // Particles class to use.
            settings.EmitPosition = new Vector3(2500, 500, 2500);
            settings.EmitRange = new Vector3(5000, 0, 5000);
            settings.EmitPerSecond = 250;

            settings.Duration = TimeSpan.FromSeconds(20.0f);

            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = -10;
            settings.MaxHorizontalVelocity = 10;

            settings.MinVerticalVelocity = -0.5f;
            settings.MaxVerticalVelocity = -2.0f;
            
            settings.EndVelocity = 5.9f;

            // Set gravity
            settings.Gravity = new Vector3(0, -9.6f, 0);

            settings.MinColor = Color.White;
            settings.MaxColor = Color.PowderBlue;

            settings.MinStartSize = 1;
            settings.MaxStartSize = 5;

            settings.MinEndSize = 5;
            settings.MaxEndSize = 10;

            // XNA 4.0 Updates
            // Use additive blending.
            //settings.SourceBlend = Blend.SourceAlpha;
            //settings.DestinationBlend = Blend.One;
            settings.BlendState = BlendState.Additive;
        }
    }
}


