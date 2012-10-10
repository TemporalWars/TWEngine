#region File Description
//-----------------------------------------------------------------------------
// InstancedModelTextures.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System.Collections.Generic;
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
        public InstancedModelTextures(ContentReader input)
        {
            // Read Count value
            var count = input.ReadInt32();

            // Create Dictionary
            _texturesDictionary = new Dictionary<string, Texture>(count);

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
