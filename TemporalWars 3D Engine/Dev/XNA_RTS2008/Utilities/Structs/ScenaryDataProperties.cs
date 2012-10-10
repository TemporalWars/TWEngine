#region File Description
//-----------------------------------------------------------------------------
// ScenaryDataProperties.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.Utilities.Structs
{
    // Used in Struct 'SaveTerrainScenaryData'.

#pragma warning disable 1587
    ///<summary>
    /// The <see cref="ScenaryDataProperties"/> struct saves <see cref="ScenaryItemScene"/> attributes, like
    /// the position and rotation.
    ///</summary>
#pragma warning restore 1587
#if !XBOX360
    [Serializable] 
#endif
    public struct ScenaryDataProperties
    {
        ///<summary>
        /// <see cref="Vector3"/> position
        ///</summary>
// ReSharper disable InconsistentNaming
        public Vector3 position;


        ///<summary>
        /// <see cref="Quaternion"/> rotation
        ///</summary>
        public Quaternion rotation;

        ///<summary>
        /// Does this <see cref="ScenaryItemScene"/> set a path block for A*?
        ///</summary>
        public bool isPathBlocked;

        ///<summary>
        /// What is path block size?
        ///</summary>
        /// <remarks>Requires the <see cref="isPathBlocked"/> to be set to TRUE</remarks>
        public int pathBlockSize;

        ///<summary>
        /// User defined Name of this <see cref="ScenaryItemScene"/> for scripting purposes.
        ///</summary>
        public string name; // 10/6/2009 - 

        // 5/31/2012
        /// <summary>
        /// Scale of item.
        /// </summary>
        public Vector3 scale;
// ReSharper restore InconsistentNaming
    }
}