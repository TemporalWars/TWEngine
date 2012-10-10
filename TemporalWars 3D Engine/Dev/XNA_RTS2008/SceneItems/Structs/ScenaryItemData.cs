#region File Description
//-----------------------------------------------------------------------------
// ScenaryItemData.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Structs;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.SceneItems.Structs
{
    // 4/14/2009
    ///<summary>
    /// The <see cref="ScenaryItemData"/> is used to load attributes for the given
    /// <see cref="ScenaryItemScene"/>.
    ///</summary>
    public struct ScenaryItemData
    {
        ///<summary>
        /// Constructor, which sets all internal attributes for given <see cref="ScenaryItemScene"/>.
        ///</summary>
        ///<param name="inPosition"><see cref="Vector3"/> position value</param>
        ///<param name="inRotation"><see cref="Quaternion"/> rotation value</param>
        public ScenaryItemData(ref Vector3 inPosition, ref Quaternion inRotation) : this()
        {
            position = inPosition;
            rotation = inRotation;
            scale = Vector3.One;
            instancedItemData = new InstancedItemData();
            UniqueKey = Guid.NewGuid(); // 6/6/2012
            AttachedAudioStructIndex = -1; // 6/10/2012
        }

        // 6/6/2012
        /// <summary>
        /// Gets the current <see cref="SceneItem"/> unique key GUID.
        /// </summary>
        public Guid UniqueKey;

        // 5/31/2012
        /// <summary>
        /// Gets or sets if the <see cref="SceneItem"/> was spawned with some scripting action.  
        /// </summary>
        /// <remarks>This flag is used to remove item spawned dynamically when saving map data.</remarks>
        public bool SpawnByScriptingAction;

        ///<summary>
        /// User defined name for scripting purposes.
        ///</summary>
// ReSharper disable InconsistentNaming
        public string name; // 10/6/2009 - 

        ///<summary>
        /// Current position for item.
        ///</summary>
        public Vector3 position;

        ///<summary>
        /// Current <see cref="Quaternion"/> rotation
        ///</summary>
        public Quaternion rotation;

        ///<summary>
        /// World <see cref="Matrix"/> structure
        ///</summary>
        public Matrix world;

        ///<summary>
        /// Current scale for item
        ///</summary>
        public Vector3 scale;

        ///<summary>
        /// Item blocks A* path?
        ///</summary>
        public bool isPathBlocked;

        ///<summary>
        /// The A* path block size to affect.
        ///</summary>
        /// <remarks>The <see cref="isPathBlocked"/> must be set to true.</remarks>
        public int pathBlockSize;

        ///<summary>
        /// Reference to the <see cref="InstancedItemData"/> structure.
        ///</summary>
        public InstancedItemData instancedItemData;

        internal bool IsPickedInEditMode; // 7/2/2009
        internal bool deleteItem; // 7/2/2009  

        // 6/10/2012
        /// <summary>
        /// This is set to the index of the collection containing the <see cref="ScenaryItemDataAudio"/> structure.
        /// When no index is set, the default value is -1.
        /// </summary>
        internal int AttachedAudioStructIndex; 
// ReSharper restore InconsistentNaming
    }
}


