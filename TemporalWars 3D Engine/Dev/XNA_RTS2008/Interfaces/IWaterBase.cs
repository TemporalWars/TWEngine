#region File Description
//-----------------------------------------------------------------------------
// IWaterBase.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using TWEngine.Water;

namespace TWEngine.Interfaces
{
    ///<summary>
    /// The <see cref="WaterBase"/> class is the base class for creating any water-type component.
    ///</summary>
    public interface IWaterBase
    {
        /// <summary>
        /// Set water table height.
        /// </summary>
        float WaterHeight { get; set; }
    }
}
