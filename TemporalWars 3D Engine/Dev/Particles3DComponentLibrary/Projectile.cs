#region File Description
//-----------------------------------------------------------------------------
// Projectile.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Particles3DComponentLibrary
{
    /// <summary>
    /// This class demonstrates how to combine several different particle systems
    /// to build up a more sophisticated composite effect. It implements a rocket
    /// projectile, which arcs up into the sky using a ParticleEmitter to leave a
    /// steady stream of trail particles behind it. After a while it explodes,
    /// creating a sudden burst of explosion and smoke particles.
    /// </summary>
    class Projectile
    {
        #region Constants

        const float TrailParticlesPerSecond = 200;
        const int NumExplosionParticles = 30;
        const int NumExplosionSmokeParticles = 50;
        const float ProjectileLifespan = 1.5f;
        const float SidewaysVelocityRange = 60;
        const float VerticalVelocityRange = 40;
        const float Gravity = 15;

        #endregion

        #region Fields

        readonly ParticleSystem _explosionParticles;
        readonly ParticleSystem _explosionSmokeParticles;
        readonly ParticleEmitter _trailEmitter;

        Vector3 _position;
        Vector3 _velocity;
        float _age;

        static readonly Random RandomGenerator = new Random();

        #endregion


        /// <summary>
        /// Constructs a new projectile.
        /// </summary>
        public Projectile(ParticleSystem explosionParticles,
                          ParticleSystem explosionSmokeParticles,
                          ParticleSystem projectileTrailParticles)
        {
            _explosionParticles = explosionParticles;
            _explosionSmokeParticles = explosionSmokeParticles;

            // Start at the origin, firing in a random (but roughly upward) direction.
            _position = Vector3.Zero;

            _velocity.X = (float)(RandomGenerator.NextDouble() - 0.5) * SidewaysVelocityRange;
            _velocity.Y = (float)(RandomGenerator.NextDouble() + 0.5) * VerticalVelocityRange;
            _velocity.Z = (float)(RandomGenerator.NextDouble() - 0.5) * SidewaysVelocityRange;

            // Use the particle emitter helper to output our trail particles.
            _trailEmitter = new ParticleEmitter(projectileTrailParticles,
                                               TrailParticlesPerSecond, _position);
        }


        /// <summary>
        /// Updates the projectile.
        /// </summary>
        public bool Update(GameTime gameTime)
        {
            var elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Simple projectile physics.
            _position += _velocity * elapsedTime;
            _velocity.Y -= elapsedTime * Gravity;
            _age += elapsedTime;

            // Update the particle emitter, which will create our particle trail.
            _trailEmitter.Update(gameTime, _position);

            // If enough time has passed, explode! Note how we pass our velocity
            // in to the AddParticle method: this lets the explosion be influenced
            // by the speed and direction of the projectile which created it.
            if (_age > ProjectileLifespan)
            {
                for (var i = 0; i < NumExplosionParticles; i++)
                    _explosionParticles.AddParticle(_position, _velocity);

                for (var i = 0; i < NumExplosionSmokeParticles; i++)
                    _explosionSmokeParticles.AddParticle(_position, _velocity);

                return false;
            }
                
            return true;
        }
    }
}
