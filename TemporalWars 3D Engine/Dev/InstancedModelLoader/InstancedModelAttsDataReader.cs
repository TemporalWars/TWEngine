#region File Description
//-----------------------------------------------------------------------------
// InstancedModelAttsDataReader.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.InstancedModelLoader.Loaders;
using Microsoft.Xna.Framework.Content;

namespace ImageNexus.BenScharbach.TWEngine.InstancedModelLoader
{
    /// <summary>
    /// Content pipeline support class for loading InstancedModelAttsDataLoader objects.
    /// </summary>
    public class InstancedModelAttsDataReader : ContentTypeReader<InstancedModelAttsDataLoader>
    {
        /// <summary>
        /// Reads instanced model data from an XNB file.
        /// </summary>
        /// <param name="existingInstance">Instance of InstancedModelAttsDataLoader.</param>
        /// <param name="input">Instance of <see cref="Microsoft.Xna.Framework.Content.ContentReader"/>.</param>
        /// <returns>Returns an instance of InstancedModelAttsDataLoader.</returns>
        protected sealed override InstancedModelAttsDataLoader Read(ContentReader input,
                                               InstancedModelAttsDataLoader existingInstance)
        {
            var partData = new InstancedModelAttsDataLoader
                               {
                                   oscillateIllum = input.ReadBoolean(), 
                                   oscillateSpeed = input.ReadSingle(),
                                   illumColor = input.ReadColor(),
                                   hasSpawnBulletMarkers = input.ReadBoolean(),
                                   isFBXFormat = input.ReadBoolean()
                               };
            return partData;
        }
    }
}
