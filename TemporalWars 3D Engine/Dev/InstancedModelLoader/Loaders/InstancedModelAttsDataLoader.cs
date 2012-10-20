#region File Description
//-----------------------------------------------------------------------------
// InstancedModelAttsDataLoader.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.InstancedModelLoader.Loaders
{
    /// <summary>
    /// The <see cref="InstancedModelAttsDataLoader"/> structure is used to store
    /// the initial attributes data, stored with the given InstancedModel during
    /// creation, in the '.XNB' file, and read back into memory using the
    /// InstancedModelAttsDataReader class.
    /// </summary>
    public struct InstancedModelAttsDataLoader
    {
        /// <summary>
        /// Sets InstancedModel to oscillate the illumination effect.
        /// </summary>
        // ReSharper disable InconsistentNaming
        public bool oscillateIllum;
        /// <summary>
        /// Sets InstancedModel to oscillate at a given speed.
        /// </summary>
        public float oscillateSpeed;
        /// <summary>
        /// Sets InstancedModel illumination color.
        /// </summary>
        public Color illumColor;
        /// <summary>
        /// Sets if InstancedModel has any spawn bullet transforms.
        /// </summary>
        public bool hasSpawnBulletMarkers; // SpawnBullet Marker Positions extracted
        /// <summary>
        /// Sets if InstancedModel was read from a FBX type format.
        /// </summary>
        /// <remarks>FBX format flips the Y and Z channels.</remarks>
        public bool isFBXFormat; // IsFBXFormat?
        // ReSharper restore InconsistentNaming
    }
}
