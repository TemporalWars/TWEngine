#region File Description
//-----------------------------------------------------------------------------
// ScenaryItemDataLoader.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImageNexus.BenScharbach.TWEngine.ScenaryDataLoader.Enums;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.ScenaryDataLoader.Loaders
{
    // 10/11/2012
    /// <summary>
    /// 
    /// </summary>
    public struct ScenaryItemDataLoader
    {
         ///<summary>
        /// Constructor, which sets all internal attributes for given <see cref="ScenaryItemDataLoader"/>.
        ///</summary>
        ///<param name="inPosition"><see cref="Vector3"/> position value</param>
        ///<param name="inRotation"><see cref="Quaternion"/> rotation value</param>
        public ScenaryItemDataLoader(ref Vector3 inPosition, ref Quaternion inRotation)
            : this()
        {
            Position = inPosition;
            Rotation = inRotation;
            Scale = Vector3.One;
            UniqueKey = Guid.NewGuid(); 
            AttachedAudioStructIndex = -1; 
        }

        public Guid UniqueKey;
        public ItemType ItemType;
        public Quaternion Rotation;
        public Vector3 Position;
        public Vector3 Scale;
        public int PathBlockSize;
        public bool IsPathBlocked;
        public string Name;
        public int AttachedAudioStructIndex;
    }
}
