#region File Description
//-----------------------------------------------------------------------------
// DefenseScenePoolItem.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.MemoryPool.Interfaces;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWTools.MemoryPoolComponent;

namespace ImageNexus.BenScharbach.TWEngine.MemoryPool.PoolItems
{
    // 2/26/2009 - Memory pool SceneItemOwner class wrapper
    ///<summary>
    /// The <see cref="DefenseScenePoolItem"/> class, provides a wrapper around
    /// the <see cref="DefenseScene"/> class, allowing it to be used within the 
    /// <see cref="PoolManager"/>.
    ///</summary>
    public class DefenseScenePoolItem : PoolItem, IPoolNodeSceneItem
    {
        /// <summary>
        /// Instance of <see cref="SceneItemWithPick"/>.
        /// </summary>
        public SceneItemWithPick SceneItemInstance { get; set; } // 1/7/2010 - Updated to use base class.
     
        ///<summary>
        /// Constructor which instantiates and saves a reference to a <see cref="DefenseScene"/> class.
        ///</summary>
        public DefenseScenePoolItem()
        {
            // instantiate SceneItemOwner - PlayerNumber is set to zero for now, since it is not known; however, 'PoolManager' will
            //                    immediately set to proper number after instantiation of pool!
            SceneItemInstance = new DefenseScene(TemporalWars3DEngine.GameInstance, ItemType.sciFiAAGun01, ItemGroupType.Airplanes, ref ZeroPosition, 0)
                                   {PoolItemWrapper = this};
        }
    }
}
