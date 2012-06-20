#region File Description
//-----------------------------------------------------------------------------
// WaypointPathsSaveStruct.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System.Collections.Generic;

namespace TWEngine.Terrain.Structs
{
    // 10/16/2009
    // WaypointPath Save Structure

    ///<summary>
    /// The <see cref="WaypointPathsSaveStruct"/> struct is used to store a PathName, and 
    /// a collection of waypoint indexes which defines the path.
    ///</summary>
    public struct WaypointPathsSaveStruct
    {
        ///<summary>
        /// Path name
        ///</summary>
        public string PathName;

        ///<summary>
        /// Collection of <see cref="int"/> as waypoint indexes which defines the path.
        ///</summary>
        public List<int> PathConnections;
    }
}