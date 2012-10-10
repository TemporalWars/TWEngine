#region File Description
//-----------------------------------------------------------------------------
// SaveTerrainSelectablesData.cs
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
    // 10/7/2009

#pragma warning disable 1587
    ///<summary>
    /// The <see cref="SaveTerrainSelectablesData"/> struct is used to save two
    /// collections; <see cref="ItemType"/> Enums and <see cref="SelectablesDataProperties"/>.
    ///</summary>
#pragma warning restore 1587
#if !XBOX360
    [Serializable] 
#endif
    public struct SaveTerrainSelectablesData
    {
        ///<summary>
        /// Collection of <see cref="ItemType"/> Enums
        ///</summary>
// ReSharper disable InconsistentNaming
        public List<ItemType> itemTypes;

        ///<summary>
        /// Collection of <see cref="SelectablesDataProperties"/>
        ///</summary>
        public List<SelectablesDataProperties> itemProperties;
// ReSharper restore InconsistentNaming
    }
}