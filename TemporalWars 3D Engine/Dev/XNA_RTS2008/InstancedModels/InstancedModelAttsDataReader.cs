#region File Description
//-----------------------------------------------------------------------------
// InstancedModelAttsDataReader.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework.Content;
using TWEngine.InstancedModels.Structs;

// 5/27/2009
namespace TWEngine.InstancedModels
{

    /// <summary>
    /// Content pipeline support class for loading <see cref="InstancedModelAttsData"/> objects.
    /// </summary>
    public class InstancedModelAttsDataReader : ContentTypeReader<InstancedModelAttsData>
    {
        /// <summary>
        /// Reads instanced model data from an XNB file.
        /// </summary>
        /// <param name="existingInstance">Instance of <see cref="InstancedModelAttsData"/>.</param>
        /// <param name="input">Instance of <see cref="ContentReader"/>.</param>
        /// <returns>Returns an instance of<see cref="InstancedModelAttsData"/>.</returns>
        protected sealed override InstancedModelAttsData Read(ContentReader input,
                                               InstancedModelAttsData existingInstance)
        {
            var partData = new InstancedModelAttsData
                               {
                                   oscillateIllum = input.ReadBoolean(), // oscillateGlow, oscillateSpeed, & illumColor
                                   oscillateSpeed = input.ReadSingle(),
                                   illumColor = input.ReadColor(),
                                   hasSpawnBulletMarkers = input.ReadBoolean(),
                                   isFBXFormat = input.ReadBoolean()
                               };
           

            return partData;
        }
    }
}
