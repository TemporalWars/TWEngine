#region File Description
//-----------------------------------------------------------------------------
// ProjectilePoolItem.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using TWEngine.MemoryPool;
using TWEngine.MemoryPool.Interfaces;
using TWEngine.SceneItems;

namespace TWEngine.Particles
{
    // 5/13/2009 - Memory pool SceneItemOwner class wrapper
    /// <summary>
    /// The <see cref="ProjectilePoolItem"/> class, provides a wrapper around
    /// the <see cref="Projectile"/> class, allowing it to be used within the 
    /// <see cref="PoolManager"/>.
    /// </summary>
    public class ProjectilePoolItem : IPoolNodeItem, IPoolNodeSceneItem
    {
        /// <summary>
        /// Reference to Memory PoolNode parent.
        /// </summary>
        public PoolNode PoolNode { get; set;}
        /// <summary>
        /// Reference to PoolManager instance
        /// </summary>
        public PoolManager PoolManager { get; set; }
        /// <summary>
        /// Is <see cref="SceneItem"/> in use?
        /// </summary>
        public bool InUse { get; set;}
        // 1/7/2010
        /// <summary>
        /// Reduces the total queued count for the given <see cref="SceneItem"/>.
        /// </summary>
        public bool ReduceIFDCounter { get; set; }

        // Instance of Projectile class.
        /// <summary>
        /// A <see cref="Projectile"/> instance.
        /// </summary>
        public Projectile ProjectileItem;


        /// <summary>
        /// Constructor which instantiates and saves a reference to a <see cref="Projectile"/> class.
        /// </summary>
        public ProjectilePoolItem()
        {
            // instantiate SceneItemOwner - PlayerNumber is set to zero for now, since it is not known; however, 'PoolManager' will
            //                    immediately set to proper number after instantiation of pool!           
            ProjectileItem = new Projectile {PoolItemWrapper = this};
        }

        // 1/7/2010
        /// <summary>
        /// Instance of <see cref="SceneItemWithPick"/>.
        /// </summary>
        public SceneItemWithPick SceneItemInstance { get; set; }
        
    }
}


