#region File Description
//-----------------------------------------------------------------------------
// ITerrainScreen.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using TWEngine.GameScreens;
using TWEngine.Players;
using TWEngine.SceneItems;
using TWEngine.Terrain;
using TWEngine.TerrainTools;

namespace TWEngine.Interfaces
{
    /// <summary>
    /// This <see cref="TerrainScreen"/> implements the actual RTS game logic, which includes
    /// the creation of the <see cref="Player"/> instances, creation of the <see cref="Terrain"/>,
    /// loading the current level map, as well as continually calling update and draw throughout 
    /// the game cycle.
    /// </summary>
    public interface ITerrainScreen
    {
        ///<summary>
        /// The <see cref="TerrainShape"/> class is a manager, which uses the other terrain classes to create and manage
        /// the <see cref="Terrain"/>.  For example, the drawing of the terrain is intiated in this class, but the actual drawing is
        /// done in the <see cref="TerrainQuadTree"/> class.  This class also loads the <see cref="SceneItem"/> into memory at the
        /// beginning of a level load.  This class also used the <see cref="TerrainAlphaMaps"/>, <see cref="TerrainPickingRoutines"/>, and
        /// the <see cref="TerrainEditRoutines"/> classes.
        ///</summary>
// ReSharper disable InconsistentNaming
        ITerrainShape ITerrainShape { get; }
// ReSharper restore InconsistentNaming

#if DEBUG
        /// <summary>
        /// Turns off all Sounds playing in <see cref="SoundManager"/>.  This method call is ONLY for the GameConsole access, since Python 
        /// can't seem to access STATIC classes directly.
        /// </summary>
        void SoundsOff(); // 5/16/2009
#endif
        
    }
}