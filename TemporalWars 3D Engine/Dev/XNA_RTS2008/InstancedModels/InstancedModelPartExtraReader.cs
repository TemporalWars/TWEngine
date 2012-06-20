#region File Description
//-----------------------------------------------------------------------------
// InstancedModelPartExtraReader.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework.Content;

namespace TWEngine.InstancedModels
{
    /// <summary>
    /// Content pipeline support class for loading <see cref="InstancedModelPartExtra"/> objects.
    /// </summary>
    public class InstancedModelPartExtraReader : ContentTypeReader<InstancedModelPartExtra>
    {
        /// <summary>
        /// Reads <see cref="InstancedModelExtra"/> data from an XNB file.
        /// </summary>
        /// <param name="existingInstance">Instance of <see cref="InstancedModelPartExtra"/>.</param>
        /// <param name="input">Instance of <see cref="ContentReader"/>.</param>
        /// <returns>Returns an instance of <see cref="InstancedModelPartExtra"/>.</returns>
        protected sealed override InstancedModelPartExtra Read(ContentReader input,
                                               InstancedModelPartExtra existingInstance)
        {
            
            return new InstancedModelPartExtra(input);
        }
    }
}
