#region File Description
//-----------------------------------------------------------------------------
// IPoolNodeItem.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using TWEngine.IFDTiles;

namespace TWEngine.MemoryPool.Interfaces
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.MemoryPool.Interfaces"/> namespace contains the classes
    /// which make up the entire <see cref="MemoryPool.Interfaces"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    ///<summary>
    /// Interface <see cref="IPoolNodeItem"/>.
    ///</summary>
    public interface IPoolNodeItem
    {
        ///<summary>
        /// Set or Get a reference to the <see cref="PoolNode"/> instance.
        ///</summary>
        PoolNode PoolNode { get; set; }

        ///<summary>
        /// Set or Get a reference to the <see cref="PoolManager"/> instance.
        ///</summary>
        PoolManager PoolManager { get; set; }

        ///<summary>
        /// Set or Get if <see cref="PoolNode"/> is in use.
        ///</summary>
        bool InUse { get; set; }

        ///<summary>
        /// Set or Set if this <see cref="PoolNode"/> instance reduces the <see cref="IFDTile"/> counter.
        ///</summary>
        bool ReduceIFDCounter { get; set; }
    }
}