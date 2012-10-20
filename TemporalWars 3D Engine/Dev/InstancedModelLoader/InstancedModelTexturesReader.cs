#region File Description
//-----------------------------------------------------------------------------
// InstancedModelTexturesReader.cs
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
    /// Content pipeline support class for loading InstancedModel objects.
    /// </summary>
    public class InstancedModelTexturesReader : ContentTypeReader<InstancedModelTexturesLoader>
    {
        /// <summary>
        /// Reads InstancedModel data from an XNB file.
        /// </summary>
        /// <param name="existingInstance">Instance of InstancedModel.</param>
        /// <param name="input">Instance of <see cref="Microsoft.Xna.Framework.Content.ContentReader"/>.</param>
        /// <returns>Returns an instance of InstancedModel.</returns>
        protected sealed override InstancedModelTexturesLoader Read(ContentReader input,
                                               InstancedModelTexturesLoader existingInstance)
        {
            return new InstancedModelTexturesLoader(input);
        }
    }
}
