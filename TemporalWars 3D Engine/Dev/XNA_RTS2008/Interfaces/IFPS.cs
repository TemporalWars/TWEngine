#region File Description
//-----------------------------------------------------------------------------
// IFPS.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using TWEngine.Common;

namespace TWEngine.Interfaces
{
    /// <summary>
    /// The <see cref="FPS"/> (Frames Per Second) component, used to show the
    /// games current FPS when running, for debug purposes.
    /// </summary>
    public interface IFPS
    {
        ///<summary>
        /// Color to draw header with
        ///</summary>
        Microsoft.Xna.Framework.Color HeaderDrawColor { get; set; }
        ///<summary>
        /// Header draw location
        ///</summary>
        Microsoft.Xna.Framework.Vector2 HeaderDrawLocation { get; set; }
        ///<summary>
        /// True/False to show <see cref="FPS"/> component on screen.
        ///</summary>
        bool IsVisible { get; set; }
    }
}