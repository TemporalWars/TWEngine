#region File Description
//-----------------------------------------------------------------------------
// TexturesAtlasData.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System.Text;

namespace TWEngine.Terrain.Structs
{
    // 1/20/2009;  5/19/2009: Updated to use 'StringBuilder' instances, rather than direct strings.
    ///<summary>
    /// The <see cref="TexturesAtlasData"/> struct, is used to hold a single texture atlas name, using
    /// a <see cref="StringBuilder"/> to reduce heap garbage.
    ///</summary>
    public struct TexturesAtlasData
    {
        private StringBuilder _textureAtlasName;

        ///<summary>
        /// Get or set the texture's atlas name.
        ///</summary>
// ReSharper disable InconsistentNaming
        public string textureAtlasName
// ReSharper restore InconsistentNaming
        {
            get { return _textureAtlasName.ToString(); }
            set
            {
                if (_textureAtlasName == null)
                    _textureAtlasName = new StringBuilder(value);
                else
                {
                    // remove previous chars
                    _textureAtlasName.Remove(0, _textureAtlasName.Length);

                    // add new string value
                    _textureAtlasName.Insert(0, value);
                }
            }
        }
    }
}
