#region File Description
//-----------------------------------------------------------------------------
// TerrainTextures.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace ImageNexus.BenScharbach.TWEngine.BeginGame.Enums
{
    // 10/16/2012: Updated enum to inherit from short value.
    ///<summary>
    /// The <see cref="TerrainTextures"/> Enum is used to specify the quality 
    /// of the terrain textures to use.
    ///</summary>
    public enum TerrainTextures : short
    {
        ///<summary>
        /// Textures at 128x128 size.
        ///</summary>
        Tex128X,
        ///<summary>
        /// Textures at 256x256 size.
        ///</summary>
        Tex256X,
       
    }
}