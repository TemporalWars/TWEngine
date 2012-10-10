#region File Description
//-----------------------------------------------------------------------------
// IInstancedItem.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.InstancedModels;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Structs;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWEngine.Shapes.Enums;

namespace ImageNexus.BenScharbach.TWEngine.Interfaces
{
    /// <summary>
    /// The <see cref="InstancedItem"/> class is the manager for the <see cref="InstancedModel"/>
    /// items.  It's responsibilities include loading the <see cref="InstancedModel"/> into memory,
    /// and marshaling communication update requests between the <see cref="SceneItem"/> classes, like
    /// the <see cref="SciFiTankScene"/> class, and its required <see cref="InstancedModel"/> counterpart.
    /// For example, updates of <see cref="SceneItem"/> position data, for a specific instance, is communicate
    /// to this manager class, which in turn, will be updated to the proper <see cref="InstancedModel"/> item.
    /// </summary>
    internal interface IInstancedItem
    {
        ///<summary>
        /// <see cref="ModelType"/> Enum.
        ///</summary>
        ModelType IsModelType { get; set; }
        /// <summary>
        /// The <see cref="ItemType"/> Enum to use
        /// </summary>
        ItemType ItemType { get; }
        /// <summary>
        /// The <see cref="ItemGroupType"/> Enum this item belongs to.
        /// </summary>
        ItemGroupType ItemGroupType { get; }
        ///<summary>
        /// Item picked in edit mode?
        ///</summary>
        bool IsPickedInEditMode { get; set; }
        ///<summary>
        /// Does item contribute to path blocking for A*.
        ///</summary>
        bool IsPathBlocked { get; set; }
        ///<summary>
        /// Path block size area to affect?
        ///</summary>
        /// <remarks>Requires the <see cref="IsPathBlocked"/> to be TRUE.</remarks>
        int PathBlockSize { get; set; }
        /// <summary>
        /// The <see cref="InstancedItem"/> unique instance item key,
        /// stored in the <see cref="InstancedItemData"/> structure.
        /// </summary>
        int ItemInstanceKey { get; }
    }
}


