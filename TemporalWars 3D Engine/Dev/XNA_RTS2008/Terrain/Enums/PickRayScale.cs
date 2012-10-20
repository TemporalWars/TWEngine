#region File Description
//-----------------------------------------------------------------------------
// PickRayScale.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace ImageNexus.BenScharbach.TWEngine.Terrain.Enums
{
    // 10/16/2012: Updated enum to inherit from short value.
    ///<summary>
    /// The <see cref="PickRayScale"/> Enum specifies how to divide the picked ground location's return value.
    ///</summary>
    public enum PickRayScale : short
    {
        ///<summary>
        /// Returns the exact value at picked position.
        ///</summary>
        NoChange,
        ///<summary>
        /// Returns the picked position, divided by the set terrain scale value.
        ///</summary>
        DivideByTerrainScale,
        ///<summary>
        /// Returns the picked position, divided by the A* path scale value.
        ///</summary>
        DivideByAStarPathScale
    }
}
