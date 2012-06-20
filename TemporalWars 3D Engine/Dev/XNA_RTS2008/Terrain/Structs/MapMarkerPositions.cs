#region File Description
//-----------------------------------------------------------------------------
// MapMarkerPositions.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;

namespace TWEngine.Terrain.Structs
{
    // 4/8/2009 
    ///<summary>
    /// Holds the maps current starting 'Marker' locations, which identify where the
    /// starting HeadQuarters will be placed at game start.
    ///</summary>
    /// <remarks>The Markers are set using the properties Tool form.</remarks>
    public struct MapMarkerPositions
    {
// ReSharper disable InconsistentNaming
        ///<summary>
        /// Map marker location 1.
        ///</summary>
        public Vector3 markerLoc1;
        ///<summary>
        /// Map marker location 2.
        ///</summary>
        public Vector3 markerLoc2;
        ///<summary>
        /// Map size - default is 512.
        ///</summary>
        public int mapSize;
// ReSharper restore InconsistentNaming
    }
}
