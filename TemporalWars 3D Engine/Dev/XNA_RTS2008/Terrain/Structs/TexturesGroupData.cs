#region File Description
//-----------------------------------------------------------------------------
// TexturesGroupData.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System.Text;

namespace TWEngine.Terrain.Structs
{
    // 5/7/2008; 5/19/2009: Updated to use 'StringBuilder' instances, rather than direct strings.
    // Custom 
    ///<summary>
    /// The <see cref="TexturesGroupData"/> struct, is used to hold each texture 
    /// added to the PaintTool Group 1 or 2 containers; uses <see cref="StringBuilder"/> to reduce heap garbage.
    ///</summary>
    public struct TexturesGroupData
    {
        private StringBuilder _imageKey;
        private StringBuilder _selectedImageKey;
        private StringBuilder _textureImagePath;

        #region Properties
        // ReSharper disable InconsistentNaming

        ///<summary>
        /// Image key name
        ///</summary>
        public string imageKey

        {
            get { return _imageKey.ToString(); }
            set
            {
                if (_imageKey == null)
                    _imageKey = new StringBuilder(value);
                else
                {
                    // remove previous chars
                    _imageKey.Remove(0, _imageKey.Length);

                    // add new string value
                    _imageKey.Insert(0, value);
                }

            }
        }

        ///<summary>
        /// Selected image key name
        ///</summary>
        public string selectedImageKey
        {
            get { return _selectedImageKey.ToString(); }
            set
            {
                if (_selectedImageKey == null)
                    _selectedImageKey = new StringBuilder(value);
                else
                {
                    // remove previous chars
                    _selectedImageKey.Remove(0, _selectedImageKey.Length);

                    // add new string value
                    _selectedImageKey.Insert(0, value);
                }
            }
        }


        ///<summary>
        /// Texture image path.
        ///</summary>
        public string textureImagePath
        {
            get
            {
                return _textureImagePath.ToString();
            }
            set
            {
                if (_textureImagePath == null)
                    _textureImagePath = new StringBuilder(value);
                else
                {
                    // remove previous chars
                    _textureImagePath.Remove(0, _textureImagePath.Length);

                    // add new string value
                    _textureImagePath.Insert(0, value);
                }
            }
        }
// ReSharper restore InconsistentNaming
        #endregion

    }
}
