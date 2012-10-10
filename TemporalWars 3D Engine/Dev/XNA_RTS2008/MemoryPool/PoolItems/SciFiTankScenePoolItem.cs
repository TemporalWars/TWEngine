#region File Description
//-----------------------------------------------------------------------------
// SciFiTankScenePoolItem.cs
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

    ///<summary>
    /// The <see cref="SciFiTankScenePoolItem"/> class, provides a wrapper around
    /// the <see cref="SciFiTankScene"/> class, allowing it to be used within the 
    /// <see cref="PoolManager"/>.
    ///</summary>
    public class SciFiTankScenePoolItem : PoolItem, IPoolNodeSceneItem
    {
        /// <summary>
        /// Instance of <see cref="SceneItemWithPick"/>.
        /// </summary>
        public SceneItemWithPick SceneItemInstance { get; set; } // 1/7/2010 - Updated to use base class.


        ///<summary>
        /// Constructor which instantiates and saves a reference to a <see cref="SciFiTankScene"/> class.
        ///</summary>
        public SciFiTankScenePoolItem()
        {
            // instantiate SceneItemOwner - PlayerNumber is set to zero for now, since it is not known; however, 'PoolManager' will
            //                    immediately set to proper number after instantiation of pool!
            SceneItemInstance = new SciFiTankScene(TemporalWars3DEngine.GameInstance, ItemType.sciFiTank01, ItemGroupType.Vehicles, ref ZeroPosition, 0)
                                     {PoolItemWrapper = this};
        }
       
    }
}
