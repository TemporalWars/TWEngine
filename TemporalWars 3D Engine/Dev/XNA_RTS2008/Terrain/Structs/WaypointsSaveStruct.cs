#region File Description
//-----------------------------------------------------------------------------
// WaypointsSaveStruct.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.Terrain.Structs
{
    // 10/16/2009
    // Waypoint Save Structure
    ///<summary>
    /// The <see cref="WaypointsSaveStruct"/> struct is used to save two collections;
    ///  1) The Waypoints locations
    ///  2) The Waypoints paths.
    ///</summary>
    public struct WaypointsSaveStruct
    {
        ///<summary>
        /// Collection of waypoint locations.
        ///</summary>
        public List<Vector3> Waypoints;

        ///<summary>
        /// Collection of waypoint paths.
        ///</summary>
        public List<WaypointPathsSaveStruct> WaypointPaths;
    }
}