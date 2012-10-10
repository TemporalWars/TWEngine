#region File Description
//-----------------------------------------------------------------------------
// SaveTerrainScenaryData.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;

namespace ImageNexus.BenScharbach.TWEngine.Utilities.Structs
{
#pragma warning disable 1587
    ///<summary>
    /// The <see cref="SaveTerrainScenaryData"/> struct is used to save two
    /// collections; <see cref="ItemType"/> and <see cref="ScenaryDataProperties"/>.
    ///</summary>
#pragma warning restore 1587
#if !XBOX360
    [Serializable] 
#endif
    public struct SaveTerrainScenaryData
    {
        ///<summary>
        /// Collection of <see cref="ItemType"/> Enums
        ///</summary>
// ReSharper disable InconsistentNaming
        public List<ItemType> itemTypes;

        ///<summary>
        /// Collection of <see cref="ScenaryDataProperties"/>
        ///</summary>
        public List<ScenaryDataProperties> itemProperties;
// ReSharper restore InconsistentNaming
    }
}