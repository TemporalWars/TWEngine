#region File Description
//-----------------------------------------------------------------------------
// ICursor.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;

namespace TWEngine.Interfaces
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.Interfaces"/> namespace contains the classes
    /// which make up the entire <see cref="Interfaces"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    /// <summary>
    /// Cursor is a DrawableGameComponent that draws a Cursor on the screen. It works
    /// differently on Xbox and Windows. On windows, this will be a Cursor that is
    /// controlled using both the mouse and the gamepad. On Xbox, the Cursor will be
    /// controlled using only the gamepad.
    /// </summary>
    public interface ICursor : IDisposable
    {
        ///<summary>
        /// Cursor position in screen space.
        ///</summary>
        Microsoft.Xna.Framework.Vector2 Position { get; set; }
        
    }
}