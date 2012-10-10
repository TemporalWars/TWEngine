#region File Description
//-----------------------------------------------------------------------------
// LOD.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace ImageNexus.BenScharbach.TWEngine.Terrain.Enums
{
    ///<summary>
    /// Level of detail or <see cref="LOD"/>.
    ///</summary>
    public enum LOD
    {
        ///<summary>
        /// Lowest detail - divide by 16.
        ///</summary>
        DetailMinimum16 = 16,
        ///<summary>
        /// Low detail - divide by 8.
        ///</summary>
        DetailLow8 = 8,
        ///<summary>
        /// Medium detail - divide by 4.
        ///</summary>
        DetailMedium4 = 4,
        ///<summary>
        /// High detail - divide by 2.
        ///</summary>
        DetailHigh2 = 2,
        ///<summary>
        /// Ultra detail - divide by 1.
        ///</summary>
        DetailUltra1 = 1
    }
}
