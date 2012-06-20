#region File Description
//-----------------------------------------------------------------------------
// TerrainIsIn.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace TWEngine.Terrain.Enums
{
    ///<summary>
    /// The <see cref="TerrainIsIn"/> Enum identifies the mode the current
    /// game is set in, which is Edit or Playabe modes.
    ///</summary>
    public enum TerrainIsIn
    {
        ///<summary>
        /// Set in 'Edit' mode, which allows use of the Terrain edit tools, like the paint tool.
        ///</summary>
        EditMode,
        ///<summary>
        /// Set in 'Playable' mode, which is the default game play mode.
        ///</summary>
        PlayableMode
    }
}
