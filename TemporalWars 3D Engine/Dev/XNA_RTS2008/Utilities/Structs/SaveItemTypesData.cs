#region File Description
//-----------------------------------------------------------------------------
// SaveItemTypesData.cs
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
    // 8/1/2008 - Used to save ItemTypeAttributes.
    // 9/26/2008: Updated to use Generics.

#pragma warning disable 1587
    ///<summary>
    /// The <see cref="SaveItemTypesData{T}"/> struct is used to save
    /// the <see cref="ItemType"/> attributes.
    ///</summary>
    ///<typeparam name="T">Generic type</typeparam>
#pragma warning restore 1587
#if !XBOX360
    [Serializable] 
#endif
    public struct SaveItemTypesData<T>
    {
        ///<summary>
        /// Collection of <see cref="ItemType"/> attributes.
        ///</summary>
        public List<T> itemAttributes; // was = ScenaryItemTypeAttributes
    }
}