#region File Description
//-----------------------------------------------------------------------------
// TriggerAreaStruct.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TWEngine.Terrain.Structs
{
    ///<summary>
    /// Stores the attributes to what makes up a single <see cref="TerrainTriggerAreas"/>
    /// rectangle.
    ///</summary>
    public struct TriggerAreaStruct
    {
        ///<summary>
        /// TriggerArea's name.
        ///</summary>
        public string Name;
        ///<summary>
        /// TriggerArea's rectangle.
        ///</summary>
        public Rectangle RectangleArea;

        ///<summary>
        /// Collection of custom vertex data, used in drawing the
        /// visual rectangle for the trigger area.
        ///</summary>
        public VertexPositionColor[] VisualRectangleArea;
        
        ///<summary>
        /// For a specific trigger area, if any selectable items are
        /// contained within the trigger's rectangle, this is set to True.
        ///</summary>
        public bool ContainsSomeSelectableItem; // 10/2/2009

        // 6/3/2012
        /// <summary>
        /// Was TriggerArea spawned from a scripting action?  If so,
        /// these will be skipped during the save operation.
        /// </summary>
        public bool SpawnWithScriptingAction;
    }
}