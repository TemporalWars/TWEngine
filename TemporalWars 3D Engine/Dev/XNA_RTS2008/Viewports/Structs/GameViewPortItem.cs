#region File Description
//-----------------------------------------------------------------------------
// GameViewPortItem.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWEngine.Viewports.Structs
{
    // 6/26/2009
    ///<summary>
    /// The <see cref="GameViewPortItem"/> struct is used to store the <see cref="Texture2D"/> for display, and
    /// then <see cref="Rectangle"/> size to draw in.
    ///</summary>
    public struct GameViewPortItem
    {
        ///<summary>
        /// Rectangle ViewPort size used to show textures
        ///</summary>
        public Rectangle RectSize;
        
        ///<summary>
        /// Texture to show in viewPort
        ///</summary>
        public Texture2D Texture;

        internal bool InUse;
        internal int IndexInArray;

    }
}
