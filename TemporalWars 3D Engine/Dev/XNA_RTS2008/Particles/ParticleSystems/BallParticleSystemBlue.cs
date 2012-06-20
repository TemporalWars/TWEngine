#region File Description
//-----------------------------------------------------------------------------
// BallParticleSystemBlue.cs
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
    sealed class BallParticleSystemBlue : ParticleSystem
    {
        public BallParticleSystemBlue(Game game, ContentManager content) : base(game, content)
        {
        }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "smoke"; // particle

            settings.MaxParticles = 250;

            settings.Duration = TimeSpan.FromSeconds(1.5f);           

            settings.DurationRandomness = 0;

            settings.EmitterVelocitySensitivity = 1;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 0;

            settings.MinVerticalVelocity = 0;
            settings.MaxVerticalVelocity = 0;

            settings.Gravity = Vector3.Zero;

            settings.MinColor = Color.Blue;
            settings.MaxColor = Color.Blue;

            // 3/24/2009
            settings.MinRotateSpeed = -4;
            settings.MaxRotateSpeed = 4;

            settings.MinStartSize = 15;
            settings.MaxStartSize = 15;

            settings.MinEndSize = 15;
            settings.MaxEndSize = 15;

            // XNA 4.0 Updates
            // Use additive blending.
            //settings.SourceBlend = Blend.SourceAlpha;
            //settings.DestinationBlend = Blend.One;
            settings.BlendState = BlendState.Additive;
           
        }
    }
}


