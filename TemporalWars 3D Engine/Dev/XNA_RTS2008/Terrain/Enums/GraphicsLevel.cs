#region File Description
//-----------------------------------------------------------------------------
// GraphicsLevel.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace ImageNexus.BenScharbach.TWEngine.Terrain.Enums
{
    // 10/16/2012: Updated enum to inherit from short value.
    ///<summary>
    /// The <see cref="GraphicsLevel"/> Enum.
    ///</summary>
    public enum GraphicsLevel : short
    {
        ///<summary>
        /// Apply low quality type graphics.
        ///</summary>
        Low = 0,
        ///<summary>
        ///  Apply medium quality type graphics.
        ///</summary>
        Med = 1,
        ///<summary>
        ///  Apply high quality type graphics.
        ///</summary>
        High = 2
        
    }
}
