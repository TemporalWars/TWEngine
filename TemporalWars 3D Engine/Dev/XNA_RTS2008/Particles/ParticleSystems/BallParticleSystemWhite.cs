#region File Description
//-----------------------------------------------------------------------------
// BallParticleSystemWhite.cs
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
    sealed class BallParticleSystemWhite : ParticleSystem
    {
        public BallParticleSystemWhite(Game game, ContentManager content) : base(game, content)
        {
        }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "smoke"; // particle

            settings.MaxParticles = 250;

            settings.Duration = TimeSpan.FromSeconds(1.5f);
            
            settings.DurationRandomness = 0;            

            settings.EmitterVelocitySensitivity = 1;

            settings.MinHorizontalVelocity = 1;
            settings.MaxHorizontalVelocity = 1;

            settings.MinVerticalVelocity = 1;
            settings.MaxVerticalVelocity = 1;
            
            settings.Gravity = new Vector3(0, 0, 0);

            settings.MinColor = new Color(255, 255, 255, 255);
            settings.MaxColor = new Color(255, 255, 255, 255);

            // 3/24/2009
            settings.MinRotateSpeed = -4;
            settings.MaxRotateSpeed = 4;

            settings.MinStartSize = 10;
            settings.MaxStartSize = 10;

            settings.MinEndSize = 10;
            settings.MaxEndSize = 10;

            // XNA 4.0 Updates
            // Use additive blending.
            //settings.SourceBlend = Blend.SourceAlpha;
            //settings.DestinationBlend = Blend.One;
            settings.BlendState = BlendState.Additive;
           
        }
    }
}


