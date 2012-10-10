#region File Description
//-----------------------------------------------------------------------------
// BallParticleSystemOrange.cs
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
    /// Custom particle system for creating a round ball of energy.
    /// </summary>
    sealed class BallParticleSystemOrange : ParticleSystem
    {
        public BallParticleSystemOrange(Game game, ContentManager content) : base(game, content)
        {
        }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "smoke"; // particle

            settings.MaxParticles = 250;

            settings.Duration = TimeSpan.FromSeconds(1.5f);            

            settings.DurationRandomness = 0;

            settings.EmitterVelocitySensitivity = 1f;

            settings.MinHorizontalVelocity = 1;
            settings.MaxHorizontalVelocity = 1;

            settings.MinVerticalVelocity = 1;
            settings.MaxVerticalVelocity = 1;
            
            settings.Gravity = new Vector3(0, 0, 0);

            settings.MinColor = new Color(255, 166, 5, 255);
            settings.MaxColor = new Color(255, 166, 5, 255);

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


