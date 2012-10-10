using System;
using ImageNexus.BenScharbach.TWTools.MemoryPoolComponent.Interfaces;

namespace ImageNexus.BenScharbach.TWTools.MemoryPoolComponent
{
    // 7/2/2012
    /// <summary>
    /// The <see cref="PoolManagerBase"/> class is used to implement the PoolManager, responsible for creating
    /// and returning instances of <see cref="IPoolNodeItem"/>s.
    /// </summary>
    public abstract class PoolManagerBase : IDisposable
    {
        /// <summary>
        /// Generic type method helper, which takes a <see cref="Pool{TDefault}"/> type, and 
        /// updates the internal <see cref="PoolNode"/> attributes.
        /// </summary>
        /// <typeparam name="TU">Some class of type <see cref="IPoolNodeItem"/>.</typeparam>
        /// <param name="pool">
        /// Some instance of <see cref="Pool{TDefault}"/>.
        /// </param>
        protected static void UpdatePoolNodesAtts<TU>(Pool<TU> pool) where TU : class, IPoolNodeItem, new()
        {
            foreach (var node in pool.AllNodes)
            {
                var item = (TU)node.Item;
                
                item.PoolNode = node;
                pool.SetItemValue(node);
            }
        }

        /// <summary>
        /// a Generic type method helper, which takes a <see cref="Pool{TDefault}"/> type, and 
        /// updates the internal <see cref="PoolNode"/> attributes.
        /// </summary>
        /// <typeparam name="TU">Some Class of type <see cref="IPoolNodeItem"/></typeparam>
        /// <param name="pool"> Some instance of <see cref="Pool{TDefault}"/> </param>
        /// <param name="poolManager"><see cref="PoolManagerBase"/> owner.</param>
        /// <param name="playerNumber">Player classes number</param>
        protected static void UpdatePoolNodesAtts<TU>(Pool<TU> pool, PoolManagerBase poolManager, byte playerNumber) where TU : class, IPoolNodeItem, new()
        {
            foreach (var node in pool.AllNodes)
            {
                var item = (TU)node.Item;
               
                item.PoolManager = poolManager;
                item.PoolNode = node;

                pool.SetItemValue(node);
            }
        }

        /// <summary>
        /// Generic Type Helper, which iterates through a <see cref="Pool{TDefault}"/>, disposing of <see cref="IPoolNodeItem"/>.
        /// </summary>
        /// <typeparam name="TU">Set Generic Type.</typeparam>
        /// <param name="pool"><see cref="Pool{TDefault}"/> instance.</param>
        public static void DisposePoolNodesAttributes<TU>(Pool<TU> pool) where TU : class, IPoolNodeItem, new()
        {
            // Dispose of Pool Nodes
            foreach (var node in pool.AllNodes)
            {
                var item = (TU)node.Item;

                item.InUse = false;
                item.PoolManager = null;
                
            } // End ForEach
        }

        /// <summary>
        /// Returns the 'AvailableCount' of the given PoolItem type.
        /// </summary>
        /// <param name="itemType">typeof(PoolItem) to check</param>
        /// <returns>Number of 'AvailableCount'</returns>
        /// <remarks>
        /// Parameter <paramref name="itemType"/> must be given as typeof(PoolItem) type class,
        /// where the class is inherited from the base class PoolItem.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown when <paramref name="itemType"/> is not of the valid PoolItem base.</exception>
        public abstract int GetPoolItemAvailableCount(Type itemType);

        /// <summary>
        /// Returns the 'Capacity' of the given PoolItem type.
        /// </summary>
        /// <param name="itemType">typeOf(PoolItem) to check</param>
        /// <returns>Number of 'Capacity'</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="itemType"/> is not of the valid PoolItem base.</exception>
        public abstract int GetPoolItemCapacity(Type itemType);

        /// <summary>
        /// Dispose of <see cref="Pool{Default}"/> collections.
        /// </summary>
        public abstract void Dispose();
    }
}