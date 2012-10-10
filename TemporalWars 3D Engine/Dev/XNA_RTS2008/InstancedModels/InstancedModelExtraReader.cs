#region File Description
//-----------------------------------------------------------------------------
// InstancedModelExtraReader.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using Microsoft.Xna.Framework.Content;

namespace ImageNexus.BenScharbach.TWEngine.InstancedModels
{
    /// <summary>
    /// Content pipeline support class for loading <see cref="InstancedModel"/> objects.
    /// </summary>
    public class InstancedModelExtraReader : ContentTypeReader<InstancedModelExtra>
    {
        /// <summary>
        /// Reads <see cref="InstancedModelExtra"/> data from an XNB file.
        /// </summary>
        /// <param name="existingInstance">Instance of <see cref="InstancedModel"/>.</param>
        /// <param name="input">Instance of <see cref="ContentReader"/>.</param>
        /// <returns>Returns an instance of <see cref="InstancedModel"/>.</returns>
        protected sealed override InstancedModelExtra Read(ContentReader input,
                                               InstancedModelExtra existingInstance)
        {
            return new InstancedModelExtra(input);
        }
    }
}
