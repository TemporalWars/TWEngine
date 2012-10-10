#region File Description
//-----------------------------------------------------------------------------
// InstancedModelAttsData.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.InstancedModels.Structs
{
    // 5/27/2009
    /// <summary>
    /// The <see cref="InstancedModelAttsData"/> structure is used to store
    /// the initial attributes data, stored with the given <see cref="InstancedModel"/> during
    /// creation, in the '.XNB' file, and read back into memory using the
    /// <see cref="InstancedModelAttsDataReader"/> class.
    /// </summary>
    public struct InstancedModelAttsData
    {
        /// <summary>
        /// Sets <see cref="InstancedModel"/> to oscillate the illumination effect.
        /// </summary>
// ReSharper disable InconsistentNaming
        public bool oscillateIllum;
        /// <summary>
        /// Sets <see cref="InstancedModel"/> to oscillate at a given speed.
        /// </summary>
        public float oscillateSpeed;
        /// <summary>
        /// Sets <see cref="InstancedModel"/> illumination color.
        /// </summary>
        public Color illumColor;
        /// <summary>
        /// Sets if <see cref="InstancedModel"/> has any spawn bullet transforms.
        /// </summary>
        public bool hasSpawnBulletMarkers; // SpawnBullet Marker Positions extracted
        /// <summary>
        /// Sets if <see cref="InstancedModel"/> was read from a FBX type format.
        /// </summary>
        /// <remarks>FBX format flips the Y and Z channels.</remarks>
        public bool isFBXFormat; // IsFBXFormat?
// ReSharper restore InconsistentNaming
    }
}


