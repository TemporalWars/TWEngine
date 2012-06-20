#region File Description
//-----------------------------------------------------------------------------
// ViewPortTexture.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework.Graphics;

namespace TWEngine.Water.Enums
{
    // NOTE: (8/17/2010) Anything changed or added to this ViewPortTexture, 
    // NOTE:  MUST be done to Enum ViewPortTexture in TWTerrainToolsWPF.
    ///<summary>
    /// The <see cref="ViewPortTexture"/> Enum identifies <see cref="RenderTarget"/> texture to
    /// show on screen, for debug purposes.
    ///</summary>
    public enum ViewPortTexture
    {
        ///<summary>
        /// Displays refraction texture.
        ///</summary>
        Refraction,
        ///<summary>
        /// Displays reflection texture.
        ///</summary>
        Reflection,
        ///<summary>
        /// Displays the bump-map texture.
        ///</summary>
        Bump
    }
}