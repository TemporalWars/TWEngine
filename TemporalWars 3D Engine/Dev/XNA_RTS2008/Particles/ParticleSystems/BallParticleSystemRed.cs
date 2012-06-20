#region File Description
//-----------------------------------------------------------------------------
// BallParticleSystemRed.cs
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
    /// Custom particle system for creating a round ball of energy.
    /// </summary>
    sealed class BallParticleSystemRed : ParticleSystem
    {
        public BallParticleSystemRed(Game game, ContentManager content) : base(game, content)
        {
        }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "smoke"; // particle

            settings.MaxParticles = 250;

            settings.Duration = TimeSpan.FromSeconds(2);            

            settings.DurationRandomness = 1;

            settings.EmitterVelocitySensitivity = 1f;

            settings.MinHorizontalVelocity = 0.8f;
            settings.MaxHorizontalVelocity = 1;

            settings.MinVerticalVelocity = 0;
            settings.MaxVerticalVelocity = 0;
            
            settings.Gravity = Vector3.Zero;

            settings.MinColor = Color.Red;
            settings.MaxColor = Color.Red;

            settings.MinStartSize = 20;
            settings.MaxStartSize = 20;

            settings.MinEndSize = 20;
            settings.MaxEndSize = 20;

            // XNA 4.0 Updates
            // Use additive blending.
            //settings.SourceBlend = Blend.SourceAlpha;
            //settings.DestinationBlend = Blend.One;
            settings.BlendState = BlendState.Additive;
           
        }
    }
}


