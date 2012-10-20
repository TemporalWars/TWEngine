#region File Description
//-----------------------------------------------------------------------------
// InstancedModelPackedVector3Reader.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.InstancedModelLoader.Structs;
using Microsoft.Xna.Framework.Content;

namespace ImageNexus.BenScharbach.TWEngine.InstancedModelLoader
{
    /// <summary>
    /// Content pipeline support class for loading InstancedModel PackedVector3 structs.
    /// </summary>
    public class InstancedModelPackedVector3Reader : ContentTypeReader<PackedVector3>
    {
        /// <summary>
        /// Reads InstancedModel data from an XNB file, specifically for the PackedVector3.
        /// </summary>
        /// <param name="existingInstance">Instance of PackedVector3.</param>
        /// <param name="input">Instance of <see cref="Microsoft.Xna.Framework.Content.ContentReader"/>.</param>
        /// <returns>Instance of PackedVector3.</returns>
        protected sealed override PackedVector3 Read(ContentReader input, PackedVector3 existingInstance)
        {
            var packedVector3 = new PackedVector3
                                    {
                                        posX = {PackedValue = input.ReadUInt16()},
                                        posY = {PackedValue = input.ReadUInt16()},
                                        posZ = {PackedValue = input.ReadUInt16()}
                                    };

            return packedVector3;
        }
    }
}
