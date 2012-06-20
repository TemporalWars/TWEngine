#region File Description
//-----------------------------------------------------------------------------
// LargeExplosionSmokeParticleSystem.cs
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
    /// Custom particle system for creating the smokey part of the explosions.
    /// </summary>
    sealed class LargeExplosionSmokeParticleSystem : ParticleSystem
    {
        public LargeExplosionSmokeParticleSystem(Game game, ContentManager content) : base(game, content)
        {
        }

        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "smoke";

            settings.MaxParticles = 200;

            settings.Duration = TimeSpan.FromSeconds(4);

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 50;

            settings.MinVerticalVelocity = -10;
            settings.MaxVerticalVelocity = 50;

            settings.Gravity = new Vector3(0, -5, 0);

            settings.EndVelocity = 0;

            settings.MinColor = Color.DarkGray;
            settings.MaxColor = Color.Black;

            settings.MinRotateSpeed = -2;
            settings.MaxRotateSpeed = 2;

            settings.MinStartSize = 100;
            settings.MaxStartSize = 100;

            settings.MinEndSize = 1000;
            settings.MaxEndSize = 2000;
        }
    }
}


