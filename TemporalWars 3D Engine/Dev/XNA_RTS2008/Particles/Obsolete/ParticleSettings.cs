#region File Description
//-----------------------------------------------------------------------------
// ParticleSettings.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TWEngine.Particles
{
    /// <summary>
    /// The <see cref="ParticleSettings"/> class describes all the tweakable options used
    /// to control the appearance of a particle system.
    /// </summary>
    public class ParticleSettings
    {
        // 1/16/2010 - Save Refence to parent 'ParticleSystem'.
        private ParticleSystem _parent;

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

        // 3/24/2011
        /// <summary>
        /// Parent instance of <see cref="ParticleSystem"/>
        /// </summary>
        public ParticleSystem Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        #endregion

      

        // 1/16/2010 - Add Constructor
        /*///<summary>
        /// Constructor for <see cref="ParticleSettings"/>
        ///</summary>
        ///<param name="particleSystem"><see cref="ParticleSystem"/> instance</param>
        public ParticleSettings(ParticleSystem particleSystem)
        {
            _parent = particleSystem;
        }*/
        
        /// <summary>
        /// Name of the <see cref="Texture2D"/> used by this <see cref="ParticleSystem"/>.
        /// </summary>
        public string TextureName;

         
        /// <summary>
        /// Maximum number of particles that can be displayed at one Time.
        /// </summary>
        public int MaxParticles = 100;

        
        /// <summary>
        /// How long these particles will last.
        /// </summary>
        public TimeSpan Duration = TimeSpan.FromSeconds(2);

        // 4/27/2009 -
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

        // 4/27/2009 - Emitter Attributes
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
        
        /// <summary>
        /// If greater than zero, some particles will last a shorter Time than others.
        /// </summary>
        public float DurationRandomness;
      
        /// <summary>
        /// Controls how much particles are influenced by the velocity of the object
        /// which created them. You can see this in action with the explosion effect,
        /// where the flames continue to move in the same direction as the source
        /// <see cref="Projectile"/>. The <see cref="Projectile"/> trail particles, on the other hand, set this
        /// value very low so they are less affected by the velocity of the <see cref="Projectile"/>.        
        /// </summary>
        public float EmitterVelocitySensitivity = 1;


        
        /// <summary>
        /// Range of values controlling how much X and Z axis velocity to give each
        /// particle. Values for individual particles are randomly chosen from somewhere
        /// between these limits.
        /// </summary>
        /// <remarks>Works in conjuction with the MaxHorizontalVelocity.</remarks>
        public float MinHorizontalVelocity;
        /// <summary>
        /// Range of values controlling how much X and Z axis velocity to give each
        /// particle. Values for individual particles are randomly chosen from somewhere
        /// between these limits.
        /// </summary>
        /// <remarks>Works in conjuction with the MinHorizontalVelocity.</remarks>
        public float MaxHorizontalVelocity;


        
        /// <summary>
        /// Range of values controlling how much Y axis velocity to give each particle.
        /// Values for individual particles are randomly chosen from somewhere between
        /// these limits.
        /// </summary>
        /// <remarks>Works in conjuction with the MaxVerticalVelocity.</remarks>
        public float MinVerticalVelocity;
        /// <summary>
        /// Range of values controlling how much Y axis velocity to give each particle.
        /// Values for individual particles are randomly chosen from somewhere between
        /// these limits.
        /// </summary>
        /// <remarks>Works in conjuction with the MinVerticalVelocity. </remarks>
        public float MaxVerticalVelocity;
        
        /// <summary>
        /// Direction and strength of the gravity effect. 
        /// </summary>
        /// <remarks> This can point in any direction, not just down! The fire effect points it upward to make the flames
        /// rise, and the smoke plume points it sideways to simulate wind. 
        /// </remarks>
        public Vector3 Gravity = Vector3.Zero;

        
        /// <summary>
        /// Controls how the particle velocity will change over their lifetime. If set
        /// to 1, particles will keep going at the same speed as when they were created.
        /// If set to 0, particles will come to a complete stop right before they die.
        /// Values greater than 1 make the particles speed up over Time.
        /// </summary>
        public float EndVelocity = 1;


        
        /// <summary>
        /// Range of values controlling the particle color and alpha. Values for
        /// individual particles are randomly chosen from somewhere between these limits. 
        /// </summary>
        /// <remarks>Works in conjuction with the MaxColor.</remarks>
        private Color _minColor = Color.White;

        /// <summary>
        /// Range of values controlling the particle color and alpha. Values for
        /// individual particles are randomly chosen from somewhere between these limits.
        /// </summary>
        /// <remarks>Works in conjuction with the MinColor.</remarks>
        private Color _maxColor = Color.White;
        
        /// <summary>
        /// Range of values controlling how fast the particles rotate. Values for
        /// individual particles are randomly chosen from somewhere between these
        /// limits. If both these values are set to 0, the <see cref="ParticleSystem"/> will
        /// automatically switch to an alternative shader technique that does not
        /// support rotation, and thus requires significantly less GPU power. This
        /// means if you don't need the rotation effect, you may get a performance
        /// boost from leaving these values at 0.
        /// </summary>
        /// <remarks>Works in conjuction with the MaxRotateSpeed.</remarks>
        public float MinRotateSpeed;

        /// <summary>
        /// Range of values controlling how fast the particles rotate. Values for
        /// individual particles are randomly chosen from somewhere between these
        /// limits. If both these values are set to 0, the <see cref="ParticleSystem"/> will
        /// automatically switch to an alternative shader technique that does not
        /// support rotation, and thus requires significantly less GPU power. This
        /// means if you don't need the rotation effect, you may get a performance
        /// boost from leaving these values at 0.
        /// </summary>
        /// <remarks>Works in conjuction with the MinRotateSpeed.</remarks>
        public float MaxRotateSpeed;
        
        /// <summary>
        /// Range of values controlling how big the particles are when first created.
        /// Values for individual particles are randomly chosen from somewhere between
        /// these limits. 
        /// </summary>
        /// <remarks>Works in conjuction with the MaxStartSize.</remarks>
        public float MinStartSize = 100;

        /// <summary>
        /// Range of values controlling how big the particles are when first created.
        /// Values for individual particles are randomly chosen from somewhere between
        /// these limits. 
        /// </summary>
        /// <remarks>Works in conjuction with the MinStartSize.</remarks>
        public float MaxStartSize = 100;


        
        /// <summary>
        /// Range of values controlling how big particles become at the end of their
        /// life. Values for individual particles are randomly chosen from somewhere
        /// between these limits.
        /// </summary>
        /// <remarks>Works in conjuction with the MaxEndSide.</remarks>
        public float MinEndSize = 100;

        /// <summary>
        /// Range of values controlling how big particles become at the end of their
        /// life. Values for individual particles are randomly chosen from somewhere
        /// between these limits.
        /// </summary>
        /// <remarks>Works in conjuction with the MinEndSide.</remarks>
        public float MaxEndSize = 100;

        #region OLDcode
        /*/// <summary>
        /// Alpha blending Settings.
        /// </summary>
        public Blend SourceBlend = Blend.SourceAlpha;

        /// <summary>
        /// Alpha blending Settings.
        /// </summary>
        public Blend DestinationBlend = Blend.InverseSourceAlpha;*/
        #endregion

        // XNA 4.0 Updates - Use BlendState for Alphablend now.
        ///<summary>
        /// Alpha blending settings.
        ///</summary>
        public BlendState BlendState = BlendState.NonPremultiplied;

      
    }
}


