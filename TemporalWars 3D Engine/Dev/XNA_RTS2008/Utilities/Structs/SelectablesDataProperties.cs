#region File Description
//-----------------------------------------------------------------------------
// SelectablesDataProperties.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using TWEngine.SceneItems;

namespace TWEngine.Utilities.Structs
{
    // 10/7/2009
    // Used in Struct 'SaveTerrainSelectablesData'.

#pragma warning disable 1587
    ///<summary>
    /// The <see cref="SelectablesDataProperties"/> struct saves <see cref="SceneItemWithPick"/> attributes, like
    /// the player number, position, rotation, etc.
    ///</summary>
#pragma warning restore 1587
#if !XBOX360
    [Serializable]
#endif
    public struct SelectablesDataProperties
    {
        ///<summary>
        /// Player number this <see cref="SceneItemWithPick"/> belongs to.
        ///</summary>
// ReSharper disable InconsistentNaming
        public byte playerNumber; // 10/20/2009

        ///<summary>
        /// <see cref="Vector3"/> position
        ///</summary>
        public Vector3 position;

        ///<summary>
        /// <see cref="Quaternion"/> rotation
        ///</summary>
        public Quaternion rotation;

        ///<summary>
        /// Does this <see cref="SceneItemWithPick"/> set a path block for A*?
        ///</summary>
        public bool isPathBlocked;

        ///<summary>
        /// What is path block size?
        ///</summary>
        /// <remarks>Requires the <see cref="isPathBlocked"/> to be set to TRUE</remarks>
        public int pathBlockSize;

        ///<summary>
        /// User defined Name of this <see cref="SceneItemWithPick"/> for scripting purposes.
        ///</summary>
        public string name;
 // ReSharper restore InconsistentNaming
    }
}