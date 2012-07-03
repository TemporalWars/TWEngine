#region File Description
//-----------------------------------------------------------------------------
// BuildingScenePoolItem.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using MemoryPoolComponent;
using TWEngine.InstancedModels.Enums;
using TWEngine.MemoryPool;
using TWEngine.MemoryPool.Interfaces;

namespace TWEngine.SceneItems
{
    // 2/24/2009 - 
    ///<summary>
    /// The <see cref="BuildingScenePoolItem"/> class, provides a wrapper around
    /// the <see cref="BuildingScene"/> class, allowing it to be used within the 
    /// <see cref="PoolManager"/>.
    ///</summary>
    public class BuildingScenePoolItem : PoolItem, IPoolNodeSceneItem
    {
        /// <summary>
        /// Instance of <see cref="SceneItemWithPick"/>.
        /// </summary>
        public SceneItemWithPick SceneItemInstance { get; set;} // 1/7/2010 - Updated to use base class.

        ///<summary>
        /// Constructor which instantiates and saves a reference to a <see cref="BuildingScene"/> class.
        ///</summary>
        public BuildingScenePoolItem()
        {
            // instantiate SceneItemOwner - PlayerNumber is set to zero for now, since it is not known; however, 'PoolManager' will
            //                    immediately set to proper number after instantiation of pool!
            SceneItemInstance = new BuildingScene(TemporalWars3DEngine.GameInstance, ItemType.sciFiBlda01, ref ZeroPosition, 0)
                                    {PoolItemWrapper = this};
        }
    }
}
