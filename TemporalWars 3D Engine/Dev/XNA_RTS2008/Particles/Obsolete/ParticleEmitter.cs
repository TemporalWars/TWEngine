#region File Description
//-----------------------------------------------------------------------------
// ParticleEmitter.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;

namespace TWEngine.Particles
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.Particles"/> namespace contains the classes
    /// which make up the entire <see cref="Particles"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
    
    /// <summary>
    /// Helper for objects that want to leave particles behind them as they
    /// move around the World. This emitter implementation solves two related
    /// problems:
    /// 
    /// If an object wants to create particles very slowly, less than once per
    /// frame, it can be a pain to keep track of which updates ought to create
    /// a new particle versus which should not.
    /// 
    /// If an object is moving quickly and is creating many particles per frame,
    /// it will look ugly if these particles are all bunched up together. Much
    /// better if they can be spread out along a line between where the object
    /// is now and where it was on the previous frame. This is particularly
    /// important for leaving trails behind fast moving objects such as rockets.
    /// 
    /// This emitter class keeps track of a moving object, remembering its
    /// previous Position so it can calculate the velocity of the object. It
    /// works out the perfect locations for creating particles at any frequency
    /// you specify, regardless of whether this is faster or slower than the
    /// game update rate.
    /// </summary>
    public class ParticleEmitter : IDisposable
    {
        #region Fields
        /// <summary>
        /// <see cref="ParticleSystem"/> instance.
        /// </summary>
        protected ParticleSystem ParticleSystem;
        /// <summary>
        /// The time between the next particle creation.
        /// </summary>
        protected float TimeBetweenParticles;
        /// <summary>
        /// The previous of the particle expressed as a <see cref="Vector3"/>.
        /// </summary>
        protected Vector3 PreviousPosition;
        /// <summary>
        /// The time left for the current particle before death.
        /// </summary>
        protected float TimeLeftOver;

        #endregion

        /// <summary>
        /// Constructs a new particle emitter object.
        /// </summary>
        /// <param name="particleSystem"><see cref="ParticleSystem"/> instance</param>
        /// <param name="particlesPerSecond">particles per second to emit</param>
        /// <param name="initialPosition">The <see cref="Vector3"/> initial position</param>
        public ParticleEmitter(ParticleSystem particleSystem,
                               float particlesPerSecond, Vector3 initialPosition)
        {
            ParticleSystem = particleSystem;

            TimeBetweenParticles = 1.0f / particlesPerSecond;
            
            PreviousPosition = initialPosition;
        }

        // 5/13/2009 - Allows Init particle emitter
        ///<summary>
        /// Constructs a new particle emitter object.
        ///</summary>
        ///<param name="particleSystem"><see cref="ParticleSystem"/> instance</param>
        ///<param name="particlesPerSecond">particles per second to emit</param>
        ///<param name="initialPosition">The <see cref="Vector3"/> initial position</param>
        public void ParticleEmitterInitialization(ParticleSystem particleSystem,
                                                  float particlesPerSecond, Vector3 initialPosition)
        {
            ParticleSystem = particleSystem;

            TimeBetweenParticles = 1.0f / particlesPerSecond;

            PreviousPosition = initialPosition;

            // 5/18/2009 - Reset Time
            TimeLeftOver = 0;
        }       

        /// <summary>
        /// Updates the emitter, creating the appropriate number of particles
        /// in the appropriate positions.
        /// </summary>        
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>   
        /// <param name="newPosition">The <see cref="Vector3"/> new position</param>  
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="gameTime"/> is null.</exception>
        public void Update(GameTime gameTime, Vector3 newPosition)
        {
            if (gameTime == null)
                throw new ArgumentNullException("gameTime", @"The parameter cannot be null.");

            // Work out how much Time has passed since the previous update.
            var elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (elapsedTime > 0)
            {
                // 2/9/2009: Updated to remove Ops overload of Vector3, since these are slow on XBOX!
                // Work out how fast we are moving.               
                Vector3 velocity;
                Vector3.Subtract(ref newPosition, ref PreviousPosition, out velocity);
                Vector3.Divide(ref velocity, elapsedTime, out velocity);

                // If we had any Time left over that we didn't use during the
                // previous update, add that to the current elapsed Time.
                var timeToSpend = TimeLeftOver + elapsedTime;
                
                // Counter for looping over the Time interval.
                var currentTime = -TimeLeftOver;

                // Create particles as long as we have a big enough Time interval.
                while (timeToSpend > TimeBetweenParticles)
                {
                    currentTime += TimeBetweenParticles;
                    timeToSpend -= TimeBetweenParticles;

                    // Work out the optimal Position for this particle. This will produce
                    // evenly spaced particles regardless of the object speed, particle
                    // creation frequency, or game update rate.
                    var mu = currentTime / elapsedTime;
                    
                    Vector3 position;
                    Vector3.Lerp(ref PreviousPosition, ref newPosition, mu, out position);

                    // Create the particle.
                    ParticleSystem.AddParticle(position, velocity);
                }

                // Store any Time we didn't use, so it can be part of the next update.
                TimeLeftOver = timeToSpend;
            }

            PreviousPosition = newPosition;
        }

        #region Dispose
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this final dispose?</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                ParticleSystem = null;
            }
            // free native resources
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}


