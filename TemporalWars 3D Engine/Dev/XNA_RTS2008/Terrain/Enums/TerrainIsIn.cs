#region File Description
//-----------------------------------------------------------------------------
// TerrainIsIn.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace ImageNexus.BenScharbach.TWEngine.Terrain.Enums
{
    // 10/16/2012: Updated enum to inherit from short value.
    ///<summary>
    /// The <see cref="TerrainIsIn"/> Enum identifies the mode the current
    /// game is set in, which is Edit or Playabe modes.
    ///</summary>
    public enum TerrainIsIn : short
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
