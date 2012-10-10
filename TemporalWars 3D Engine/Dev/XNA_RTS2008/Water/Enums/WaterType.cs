#region File Description
//-----------------------------------------------------------------------------
// WaterType.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace ImageNexus.BenScharbach.TWEngine.Water.Enums
{
    // NOTE: (8/17/2010) Anything changed or added to this WaterType, 
    // NOTE:  MUST be done to Enum WaterType in TWTerrainToolsWPF.
    ///<summary>
    /// The <see cref="WaterType"/> Enum to use.
    ///</summary>
    public enum WaterType
    {
        /// <summary>
        /// Set when no water type needs to be used.
        /// </summary>
        None, // 6/1/2010
        ///<summary>
        /// Creates the Lake simple water type
        ///</summary>
        Lake,
        ///<summary>
        /// Creates the Ocean complex water type
        ///</summary>
        Ocean
    }
}