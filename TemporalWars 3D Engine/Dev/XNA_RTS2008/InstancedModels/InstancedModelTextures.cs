#region File Description
//-----------------------------------------------------------------------------
// InstancedModelTextures.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using ImageNexus.BenScharbach.TWEngine.InstancedModelLoader.Loaders;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWEngine.InstancedModels
{

    ///<summary>
    /// The <see cref="InstancedModelTextures"/> class stores the 'String'/<see cref="Texture"/> pair.
    ///</summary>
    public class InstancedModelTextures
    {
        private readonly Dictionary<string, Texture> _texturesDictionary;

        #region Properties

        ///<summary>
        /// Gets the current Dictionary of <see cref="Texture"/>.
        ///</summary>
        public Dictionary<string, Texture> TexturesDictionary
        {
            get { return _texturesDictionary; }
        }

        #endregion
        
        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="input"></param>
        public InstancedModelTextures(InstancedModelTexturesLoader input)
        {
            // Create Dictionary
            _texturesDictionary = new Dictionary<string, Texture>(input.TexturesDictionary.Count);

            foreach (var texture in input.TexturesDictionary)
            {
                TexturesDictionary.Add(texture.Key, texture.Value);
            }
        }
    }
}
