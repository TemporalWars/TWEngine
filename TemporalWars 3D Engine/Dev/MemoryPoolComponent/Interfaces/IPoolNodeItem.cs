#region File Description
//-----------------------------------------------------------------------------
// IPoolNodeItem.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

namespace ImageNexus.BenScharbach.TWTools.MemoryPoolComponent.Interfaces
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="MemoryPoolComponent.Interfaces"/> namespace contains the classes
    /// which make up the entire <see cref="MemoryPoolComponent.Interfaces"/> component.
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
        /// Set or Get a reference to the <see cref="PoolManagerBase"/> instance.
        ///</summary>
        PoolManagerBase PoolManager { get; set; }

        ///<summary>
        /// Set or Get if <see cref="PoolNode"/> is in use.
        ///</summary>
        bool InUse { get; set; }

        ///<summary>
        /// Set or Set if this <see cref="PoolNode"/> instance reduces the IFDTile counter.
        ///</summary>
        bool ReduceIFDCounter { get; set; }
    }
}