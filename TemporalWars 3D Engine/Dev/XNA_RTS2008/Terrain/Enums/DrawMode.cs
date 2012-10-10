#region File Description
//-----------------------------------------------------------------------------
// DrawMode.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWLate.RTS_MinimapInterfaces.Minimap;

namespace ImageNexus.BenScharbach.TWEngine.Terrain.Enums
{
    ///<summary>
    /// Use <see cref="DrawMode"/> Enum to set draw type.
    ///</summary>
    public enum DrawMode
    {
        ///<summary>
        /// Use to draw the paint or height brushes during editing.
        ///</summary>
        EditMode,
        ///<summary>
        /// Solid textured drawing mode.
        ///</summary>
        Solid,
        ///<summary>
        /// Wireframe non-textured drawing mode.
        ///</summary>
        WireFrame,
        ///<summary>
        /// Top-down view of landscape used to draw in <see cref="IMinimap"/>.
        ///</summary>
        MiniMap
    }
}
