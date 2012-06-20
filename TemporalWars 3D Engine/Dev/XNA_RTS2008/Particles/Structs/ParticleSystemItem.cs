#region File Description
//-----------------------------------------------------------------------------
// ParticleSystemItem.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Particles3DComponentLibrary;

namespace TWEngine.Particles.Structs
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.Particles.Structs"/> namespace contains the classes
    /// which make up the entire <see cref="Particles.Structs"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
    
    
    
    ///<summary>
    /// The <see cref="ParticleSystemItem"/> structure holds the reference to a single instance
    /// of a particle.
    ///</summary>
    public struct ParticleSystemItem
    {
        ///<summary>
        /// Assigned to location in list.
        ///</summary>
        public int InstanceKey; // 
        /// <summary>
        /// Allows system to automatically create new partices per the emitter. (Scripting Purposes)
        /// </summary>
        public bool AutoEmit; //  - 
        /// <summary>
        /// <see cref="ParticleSystem"/> instance
        /// </summary>
        public ParticleSystem ParticleSystem;
        /// <summary>
        /// <see cref="ParticleFrequencyEmitter"/> instance
        /// </summary>
        public ParticleFrequencyEmitter ParticleEmitters;
    }
}