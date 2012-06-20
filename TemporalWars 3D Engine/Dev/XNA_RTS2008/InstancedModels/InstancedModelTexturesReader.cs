#region File Description
//-----------------------------------------------------------------------------
// InstancedModelTexturesReader.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework.Content;

namespace TWEngine.InstancedModels
{
    /// <summary>
    /// Content pipeline support class for loading <see cref="InstancedModel"/> objects.
    /// </summary>
    public class InstancedModelTexturesReader : ContentTypeReader<InstancedModelTextures>
    {
        /// <summary>
        /// Reads <see cref="InstancedModel"/> data from an XNB file.
        /// </summary>
        /// <param name="existingInstance">Instance of <see cref="InstancedModel"/>.</param>
        /// <param name="input">Instance of <see cref="ContentReader"/>.</param>
        /// <returns>Returns an instance of <see cref="InstancedModel"/>.</returns>
        protected sealed override InstancedModelTextures Read(ContentReader input,
                                               InstancedModelTextures existingInstance)
        {
            return new InstancedModelTextures(input);
        }
    }
}
