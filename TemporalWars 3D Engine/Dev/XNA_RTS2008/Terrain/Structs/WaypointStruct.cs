#region File Description
//-----------------------------------------------------------------------------
// WaypointStruct.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TWEngine.Terrain.Structs
{
    // 10/14/2009
    // Waypoint structure

    ///<summary>
    /// The <see cref="WaypointStruct"/> struct holds the data for a single waypoint.
    ///</summary>
    public struct WaypointStruct
    {
        ///<summary>
        /// Location of waypoint in game world.
        ///</summary>
        public Vector3 Location;
        ///<summary>
        /// <see cref="Rectangle"/> area of waypoint.
        ///</summary>
        public Rectangle RectangleArea;
        ///<summary>
        /// Collection of <see cref="VertexPositionColor"/>, used to draw the rectangles.
        ///</summary>
        public VertexPositionColor[] VisualRectangleArea;
       
    }
}