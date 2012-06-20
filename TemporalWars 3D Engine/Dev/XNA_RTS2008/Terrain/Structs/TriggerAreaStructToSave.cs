#region File Description
//-----------------------------------------------------------------------------
// TriggerAreaStructToSave.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;

namespace TWEngine.Terrain
{
    ///<summary>
    /// Structure used to specifically store the data used to save
    /// the <see cref="TerrainTriggerAreas"/> data.
    ///</summary>
    public struct TriggerAreaStructToSave
    {
        ///<summary>
        /// Trigger area name
        ///</summary>
        public string Name;

        ///<summary>
        /// TriggerArea's rectangle.
        ///</summary>
        public Rectangle RectangleArea;
    }
}