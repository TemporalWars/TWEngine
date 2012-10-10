#region File Description
//-----------------------------------------------------------------------------
// ParticleSettings.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace ImageNexus.BenScharbach.TWTools.Particles3DComponentLibrary
{
    /// <summary>
    /// Settings class describes all the tweakable options used
    /// to control the appearance of a particle system.
    /// </summary>
    public class ParticleSettings
    {
        #region Properties

        /// <summary>
        /// Range of values controlling the particle color and alpha. Values for
        /// individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        /// <remarks> Works in conjuction with the MaxColor.</remarks>
        public Color MinColor
        {
            get { return _minColor; }
            set
            {
                _minColor = value;

                // 1/16/2010 - Update Effect
                if (Parent == null || Parent.ParticleEffect == null) return;

                Parent.ParticleEffect.Parameters["MinColor"].SetValue(_minColor.ToVector4());

                // XNA 4.0 Updates - Obsolete CommiteChanges()
                //_parent.ParticleEffect.CommitChanges();
            }
        }

        /// <summary>
        /// Range of values controlling the particle color and alpha. Values for
        /// individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        /// <remarks> Works in conjuction with the MinColor.</remarks>
        public Color MaxColor
        {
            get { return _maxColor; }
            set
            {
                _maxColor = value;

                // 1/16/2010 - Update Effect
                if (Parent == null || Parent.ParticleEffect == null) return;

                Parent.ParticleEffect.Parameters["MaxColor"].SetValue(_maxColor.ToVector4());

                // XNA 4.0 Updates - Obsolete CommiteChanges()
                //_parent.ParticleEffect.CommitChanges();
            }
        }

        // 6/1/2012
        /// <summary>
        /// Color multiplier, used to reduce or increase the color range.
        /// </summary>
        /// <remarks> Works in conjuction with the MinColor.</remarks>
        public Vector4 ColorMultiplier
        {
            get { return _colorMultiplier; }
            set
            {
                _colorMultiplier = value;

                // 1/16/2010 - Update Effect
                if (Parent == null || Parent.ParticleEffect == null) return;

                Parent.ParticleEffect.Parameters["ColorMultiplier"].SetValue(_colorMultiplier);

                // XNA 4.0 Updates - Obsolete CommiteChanges()
                //_parent.ParticleEffect.CommitChanges();
            }
        }

        // 6/1/2012
        /// <summary>
        /// Direction and strength of the gravity effect. Note that this can point in any
        /// direction, not just down! The fire effect points it upward to make the flames
        /// rise, and the smoke plume points it sideways to simulate wind.
        /// </summary>
        public Vector3 Gravity
        {
            get { return _gravity; }
            set
            {
                _gravity = value;

                // 1/16/2010 - Update Effect
                if (Parent == null || Parent.ParticleEffect == null) return;

                Parent.ParticleEffect.Parameters["Gravity"].SetValue(_gravity);

                // XNA 4.0 Updates - Obsolete CommiteChanges()
                //_parent.ParticleEffect.CommitChanges();
            }
        }

        // 6/2/2012
        /// <summary>
        /// How long these particles will last.
        /// </summary>
        public TimeSpan Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;

                // 1/16/2010 - Update Effect
                if (Parent == null || Parent.ParticleEffect == null) return;

                Parent.ParticleEffect.Parameters["Duration"].SetValue((float)_duration.TotalSeconds);

                // XNA 4.0 Updates - Obsolete CommiteChanges()
                //_parent.ParticleEffect.CommitChanges();
            }
        }
        
        /// <summary>
        /// Parent instance of <see cref="ParticleSystem"/>
        /// </summary>
        public ParticleSystem Parent { get; set; }

        #endregion

        // Name of the texture used by this particle system.
        public string TextureName;

        // Maximum number of particles that can be displayed at one time.
        public int MaxParticles = 100;

        // How long these particles will last.
        protected TimeSpan _duration = TimeSpan.FromSeconds(1);

        // 3/24/2011
        ///<summary>
        /// Frequency of adding particles option.
        ///</summary>
        public bool UseFrequency;
        ///<summary>
        /// Number of particles to add per frequency tick.
        ///</summary>
        /// <remarks>Requires <see cref="UseFrequency"/> to be set to true.</remarks>
        public float ParticlesPerFrequency = 1.0f;
        ///<summary>
        /// The <see cref="TimeSpan"/> between each frequency tick.
        ///</summary>
        /// <remarks>Requires <see cref="UseFrequency"/> to be set to true.</remarks>
        public TimeSpan Frequency = TimeSpan.FromSeconds(0);

        // 3/24/2011 - Emitter Attributes
        ///<summary>
        /// Emitter position
        ///</summary>
        public Vector3 EmitPosition;
        ///<summary>
        /// Emitter range
        ///</summary>
        public Vector3 EmitRange;
        ///<summary>
        /// Emit particles per second value
        ///</summary>
        public float EmitPerSecond;

        // If greater than zero, some particles will last a shorter time than others.
        public float DurationRandomness;

        // Controls how much particles are influenced by the velocity of the object
        // which created them. You can see this in action with the explosion effect,
        // where the flames continue to move in the same direction as the source
        // projectile. The projectile trail particles, on the other hand, set this
        // value very low so they are less affected by the velocity of the projectile.
        public float EmitterVelocitySensitivity = 1;

        // Range of values controlling how much X and Z axis velocity to give each
        // particle. Values for individual particles are randomly chosen from somewhere
        // between these limits.
        public float MinHorizontalVelocity;
        public float MaxHorizontalVelocity;

        // Range of values controlling how much Y axis velocity to give each particle.
        // Values for individual particles are randomly chosen from somewhere between
        // these limits.
        public float MinVerticalVelocity;
        public float MaxVerticalVelocity;

        // Direction and strength of the gravity effect. Note that this can point in any
        // direction, not just down! The fire effect points it upward to make the flames
        // rise, and the smoke plume points it sideways to simulate wind.
        protected Vector3 _gravity = Vector3.Zero;

        // Controls how the particle velocity will change over their lifetime. If set
        // to 1, particles will keep going at the same speed as when they were created.
        // If set to 0, particles will come to a complete stop right before they die.
        // Values greater than 1 make the particles speed up over time.
        public float EndVelocity = 1;

        // Range of values controlling the particle color and alpha. Values for
        // individual particles are randomly chosen from somewhere between these limits.
        protected Color _minColor = Color.White;
        protected Color _maxColor = Color.White;

        // 6/1/2012 - Color multiplier, used to reduce or increase the color range.
        public Vector4 _colorMultiplier = Vector4.One; 

        // Range of values controlling how fast the particles rotate. Values for
        // individual particles are randomly chosen from somewhere between these
        // limits. If both these values are set to 0, the particle system will
        // automatically switch to an alternative shader technique that does not
        // support rotation, and thus requires significantly less GPU power. This
        // means if you don't need the rotation effect, you may get a performance
        // boost from leaving these values at 0.
        public float MinRotateSpeed;
        public float MaxRotateSpeed;

        // Range of values controlling how big the particles are when first created.
        // Values for individual particles are randomly chosen from somewhere between
        // these limits.
        public float MinStartSize = 100;
        public float MaxStartSize = 100;

        // Range of values controlling how big particles become at the end of their
        // life. Values for individual particles are randomly chosen from somewhere
        // between these limits.
        public float MinEndSize = 100;
        public float MaxEndSize = 100;

        // Alpha blending settings.
        public BlendState BlendState = BlendState.NonPremultiplied;
       
    }
}
