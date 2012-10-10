#region File Description
//-----------------------------------------------------------------------------
// ITerrainStorageRoutines.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.Terrain;

namespace ImageNexus.BenScharbach.TWEngine.Interfaces
{
    ///<summary>
    /// The <see cref="TerrainStorageRoutines"/> class provides basic save and load routines, used
    /// to save game data.
    ///</summary>
    public interface ITerrainStorageRoutines
    {  
#if !XBOX360
        /// <summary>
        /// Saves the <see cref="TWEngine.Terrain"/> meta-data, like heights, ground textures, waypoints, quads, etc.
        /// </summary>
        /// <param name="mapName">Map name</param>
        /// <param name="mapType">Map type; either SP or MP.</param>
        void SaveTerrainData(string mapName, string mapType);
#endif
    }
}