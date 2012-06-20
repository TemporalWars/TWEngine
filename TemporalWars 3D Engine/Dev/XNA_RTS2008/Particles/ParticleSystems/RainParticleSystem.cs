#region File Description
//-----------------------------------------------------------------------------
// RainParticleSystem.cs
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
    /// Custom particle system for creating rain drops effect.
    /// </summary>
    sealed class RainParticleSystem : ParticleSystem
    {
        public RainParticleSystem(Game game, ContentManager content) : base(game, content)
        {
        }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "raindrop";

            settings.MaxParticles = 5000; //   

            // Allows Random Position generation within EmitRange.
            // Must add 'ParticleFrequencyEmitter' instance in the
            // Particles class to use.
            settings.EmitPosition = new Vector3(2500, 500, 2500);
            settings.EmitRange = new Vector3(5000, 0, 5000);
            settings.EmitPerSecond = 1100;
            
            settings.Duration = TimeSpan.FromSeconds(5.0f);

            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 0;

            settings.MinVerticalVelocity = -50;
            settings.MaxVerticalVelocity = -100;

            settings.EndVelocity = 10.0f;

            // Set gravity
            settings.Gravity = new Vector3(0, -10, 0);

            settings.MinColor = Color.CornflowerBlue;
            settings.MaxColor = Color.DarkBlue;

            settings.MinStartSize = 5;
            settings.MaxStartSize = 10;

            settings.MinEndSize = 15;
            settings.MaxEndSize = 20;

            // XNA 4.0 Updates
            // Use additive blending.
            //settings.SourceBlend = Blend.SourceAlpha;
            //settings.DestinationBlend = Blend.One;
            settings.BlendState = BlendState.Additive;
        }
    }
}


