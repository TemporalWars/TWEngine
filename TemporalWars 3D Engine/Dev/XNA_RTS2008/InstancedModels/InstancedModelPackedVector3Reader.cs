#region File Description
//-----------------------------------------------------------------------------
// InstancedModelPackedVector3Reader.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework.Content;
using TWEngine.InstancedModels.Structs;

#endregion

// 5/28/2009
namespace TWEngine.InstancedModels
{
    /// <summary>
    /// Content pipeline support class for loading <see cref="InstancedModel"/> <see cref="PackedVector3"/> structs.
    /// </summary>
    public class InstancedModelPackedVector3Reader : ContentTypeReader<PackedVector3>
    {
        /// <summary>
        /// Reads <see cref="InstancedModel"/> data from an XNB file, specifically for the <see cref="PackedVector3"/>.
        /// </summary>
        /// <param name="existingInstance">Instance of <see cref="PackedVector3"/>.</param>
        /// <param name="input">Instance of <see cref="ContentReader"/>.</param>
        /// <returns>Instance of <see cref="PackedVector3"/>.</returns>
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
