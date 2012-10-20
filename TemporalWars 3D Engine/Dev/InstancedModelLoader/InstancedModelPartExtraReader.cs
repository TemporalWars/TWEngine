#region File Description
//-----------------------------------------------------------------------------
// InstancedModelPartExtraReader.cs
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
    /// Content pipeline support class for loading <see cref="InstancedModelPartExtraLoader"/> objects.
    /// </summary>
    public class InstancedModelPartExtraReader : ContentTypeReader<InstancedModelPartExtraLoader>
    {
        /// <summary>
        /// Reads <see cref="InstancedModelExtraLoader"/> data from an XNB file.
        /// </summary>
        /// <param name="existingInstance">Instance of <see cref="InstancedModelPartExtraLoader"/>.</param>
        /// <param name="input">Instance of <see cref="Microsoft.Xna.Framework.Content.ContentReader"/>.</param>
        /// <returns>Returns an instance of <see cref="InstancedModelPartExtraLoader"/>.</returns>
        protected sealed override InstancedModelPartExtraLoader Read(ContentReader input,
                                               InstancedModelPartExtraLoader existingInstance)
        {

            return new InstancedModelPartExtraLoader(input);
        }
    }
}
