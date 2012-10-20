#region File Description
//-----------------------------------------------------------------------------
// InstancedModelTexturesLoader.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWEngine.InstancedModelLoader.Loaders
{
    /// <summary>
    /// The <see cref="InstancedModelTexturesLoader"/> is used to abtract loading of the InstancedModel artwork, separate from the game engine.
    /// This allows changes to the main Game Engine's versions number and strong name, without breaking the loading of the arwork.
    /// </summary>
    public class InstancedModelTexturesLoader
    {
       public Dictionary<string, Texture> TexturesDictionary;

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="input"></param>
        public InstancedModelTexturesLoader(ContentReader input)
        {
            // Read Count value
            var count = input.ReadInt32();

            // Create Dictionary
            TexturesDictionary = new Dictionary<string, Texture>(count);

            // Iterate using count
            for (var i = 0; i < count; i++)
            {
                var textureName = input.ReadString();
                var texture = input.ReadExternalReference<Texture>();

                TexturesDictionary.Add(textureName, texture);
            }
        }
    }
}
