#region File Description
//-----------------------------------------------------------------------------
// ParticleFrequencyEmitter.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using Particles3DComponentLibrary;

namespace TWEngine.Particles
{
    // 4/27/2009
    /// <summary>
    /// Emits <see cref="ParticleSystem"/> particles at a specified time frequency, or allows emits of particles
    /// within a random specified range!
    /// </summary>
    public class ParticleFrequencyEmitter
    {
        #region Fields

        private readonly ParticleSystem _particleSystem;
        private float _particlesPerSecond;
        private TimeSpan _elapsedFrequencyTime = new TimeSpan(0, 0, 0);
        private TimeSpan _frequencyBetweenEmits;

// ReSharper disable UnaccessedField.Local
        private Vector3 _emitPosition;
        private Vector3 _emitRange;
// ReSharper restore UnaccessedField.Local
        private Vector3 _minPosition;
        private Vector3 _maxPosition;
        private readonly bool _useRandomEmitRange;
        private static readonly Random Rand = new Random();
        private int _particlesToEmitThisFrame;

        // 1/16/2010 - For AutoEmit, need to store EmitPosition, EmitVelocity
        private bool _autoEmitVarsSet;
        private Vector3 _autoEmitPosition;
        private Vector3 _autoEmitVeloctiy;
        
        #endregion

        /// <summary>
        /// Constructs a new particle emitter object.
        /// </summary>
        /// <param name="particleSystem"><see cref="ParticleSystem"/> instance</param>
        /// <param name="particlesPerSecond">particles per second to emit</param>
        /// <param name="frequency"><see cref="TimeSpan"/> of frequecy to emit</param>
        public ParticleFrequencyEmitter(ParticleSystem particleSystem, float particlesPerSecond, TimeSpan frequency)             
        {
            _particleSystem = particleSystem;
           
            // 6/2/2012 - Refactored
            SetFrequencyAttributes(particlesPerSecond, frequency);
        }

        /// <summary>
        /// Constructs a new particle emitter object, with a random emitting range.
        /// </summary>
        /// <param name="particleSystem"><see cref="ParticleSystem"/> instance</param>
        /// <param name="particlesPerSecond">particles per second to emit</param>
        /// <param name="frequency"><see cref="TimeSpan"/> of frequecy to emit</param>
        /// <param name="emitPosition"><see cref="Vector3"/> initial emit position</param>
        /// <param name="emitRange"><see cref="Vector3"/> skew value to add to <paramref name="emitPosition"/>. Useful for emiting particles like rain.</param>
        public ParticleFrequencyEmitter(ParticleSystem particleSystem, float particlesPerSecond, TimeSpan frequency, Vector3 emitPosition, Vector3 emitRange) 
            : this (particleSystem, particlesPerSecond, frequency)
        {            
            // set Random Emit Attributes
            _useRandomEmitRange = true;
            _emitPosition = emitPosition;
            _emitRange = emitRange;

            _minPosition = emitPosition - (emitRange * 0.5f);
            _maxPosition = emitPosition + (emitRange * 0.5f);
        }

        /// <summary>
        /// Reduces the <see cref="_elapsedFrequencyTime"/> by the given <see cref="GameTime"/> instance.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public void Update(GameTime gameTime)
        {
            // reduce by elapsed Time.
            _elapsedFrequencyTime -= gameTime.ElapsedGameTime;
            // Calc Particles to emit per frame.
            _particlesToEmitThisFrame = (int)(_particlesPerSecond * gameTime.ElapsedGameTime.TotalSeconds + 0.99f);
        }        

        // Overload 1.
        /// <summary>
        /// Updates the emitter, creating the appropriate number of particles
        /// in the appropriate positions.
        /// </summary>
        /// <param name="position"><see cref="Vector3"/> position</param>  
        /// <param name="velocity"><see cref="Vector3"/> velocity</param>           
        public void AddParticles(ref Vector3 position, ref Vector3 velocity)
        {
            
            // Time to add new particle?
            if (_elapsedFrequencyTime.Milliseconds > 0.0f)
                return;  // No, so return.

            // Yes, so reset to frequency.
            _elapsedFrequencyTime = _frequencyBetweenEmits;

                
            if (_useRandomEmitRange)
            {
                // Emit only calculated particles for this frame.
                for (var i = 0; i < _particlesToEmitThisFrame; i++)
                {
                    Vector3 newPosition;
                    GetRandomVector3(ref _minPosition, ref _maxPosition, out newPosition);

                    // Create the particle.
                    _particleSystem.AddParticle(newPosition, velocity);
                }
                return;
            }

            // Emit all particle for this frame.
            for (var i = 0; i < _particlesPerSecond; i++)
            {
                // Create the particle.
                _particleSystem.AddParticle(position, velocity);
            }
        }

        // Overload 2.
        // 1/16/2010
        /// <summary>
        /// Updates the emitter, creating the appropriate number of particles
        /// in the appropriate positions.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the method <see cref="SetAutoEmitVectors"/> was not called first.</exception>
        public void AddParticles()
        {
            if (!_autoEmitVarsSet)
                throw new InvalidOperationException("Must set Auto-Emit variables first, by calling the 'SetAutoEmitVectors' method.");

            // Emit new particles
            AddParticles(ref _autoEmitPosition, ref _autoEmitVeloctiy);
        }

        // 6/2/2012
        /// <summary>
        /// Sets the frequency attributes.
        /// </summary>
        /// <param name="particlesPerSecond">particles per second to emit</param>
        /// <param name="frequency"><see cref="TimeSpan"/> of frequecy to emit</param>
        public void SetFrequencyAttributes(float particlesPerSecond, TimeSpan frequency)
        {
            _particlesPerSecond = particlesPerSecond;
            _frequencyBetweenEmits = frequency;
        }

        // 1/16/2010
        /// <summary>
        /// Used to set the Auto-Emitter varables, required for the Auto-Emit.
        /// </summary>
        /// <param name="emitPosition">Static Position to emit particles from</param>
        /// <param name="emitVelocity">Velocity of particles</param>
        public void SetAutoEmitVectors(Vector3 emitPosition, Vector3 emitVelocity)
        {
            _autoEmitPosition = emitPosition;
            _autoEmitVeloctiy = emitVelocity;

            _autoEmitVarsSet = true;
        }

        /// <summary>
        /// Gets a random float value, between the given <paramref name="min"/> and <paramref name="max"/> values.
        /// </summary>
        /// <param name="min">Min value of range</param>
        /// <param name="max">Max value of range</param>
        /// <returns>random float value within given range</returns>
        public static float GetRandomFloat(float min, float max)
        {
            var randNum = Rand.NextDouble();

            var result = (((float)randNum * (max - min)) + min);

            return result;
        }

        /// <summary>
        /// Gets a random <see cref="Vector3"/> structure, between the given <paramref name="min"/> and <paramref name="max"/> value.
        /// </summary>
        /// <param name="min">Min value of range</param>
        /// <param name="max">Max value of range</param>
        /// <param name="randomVector3">(OUT) New random <see cref="Vector3"/> structure</param>
        public static void GetRandomVector3(ref Vector3 min, ref Vector3 max, out Vector3 randomVector3)
        {
            randomVector3 = new Vector3(GetRandomFloat(min.X, max.X), GetRandomFloat(min.Y, max.Y), GetRandomFloat(min.Z, max.Z));
        }
    }
}


