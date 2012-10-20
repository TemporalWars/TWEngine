#region File Description
//-----------------------------------------------------------------------------
// PackedVector3.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace ImageNexus.BenScharbach.TWEngine.InstancedModelLoader.Structs
{
    /// <summary>
    /// The <see cref="PackedVector3"/> structure, stores a regular
    /// <see cref="Vector3"/> structure in a compressed format.
    /// </summary>
    public struct PackedVector3
    {
// ReSharper disable InconsistentNaming
        /// <summary>
        /// Stores the float X value as <see cref="HalfSingle"/>.
        /// </summary>
        public HalfSingle posX;
        /// <summary>
        /// Stores the float Y value as <see cref="HalfSingle"/>.
        /// </summary>
        public HalfSingle posY;
        /// <summary>
        /// Stores the float Z value as <see cref="HalfSingle"/>.
        /// </summary>
        public HalfSingle posZ;
// ReSharper restore InconsistentNaming
        // Packs the Vector3
        /// <summary>
        /// Given a regular <see cref="Vector3"/>, this will seperate
        /// each channel into the <see cref="HalfSingle"/> format.
        /// </summary>
        /// <param name="inVector3"><see cref="Vector3"/> to compress</param>
        public PackedVector3(ref Vector3 inVector3)
        {
            // Store 'inVector3' using 3 halfSingle (50%)
            posX = new HalfSingle(inVector3.X);
            posY = new HalfSingle(inVector3.Y);
            posZ = new HalfSingle(inVector3.Z);
        }

        // Unpacks the Vector3
        /// <summary>
        /// Extracts all 3 components, stored as the <see cref="HalfSingle"/> format, and
        /// returns as a <see cref="Vector3"/> un-compressed format.
        /// </summary>
        /// <param name="outVector3">(OUT) uncompressed <see cref="Vector3"/> struct</param>
        public void UnPackVector3(out Vector3 outVector3)
        {
            outVector3 = new Vector3 {X = posX.ToSingle(), Y = posY.ToSingle(), Z = posZ.ToSingle()};
        }
       
    }
}