#region Copyright (c) 2007 Thomas H. Aylesworth
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"), 
// to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
//
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using ParallelTasksComponent.LocklessQueue;
using TWEngine.MemoryPool.Delegates;
using TWEngine.MemoryPool.Interfaces;

namespace TWEngine.MemoryPool
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.MemoryPool"/> namespace contains the classes
    /// which make up the entire <see cref="MemoryPool"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
    
    
    /// <summary>
    /// Represents a fixed-size pool of available items that can be removed
    /// as needed and returned when finished.
    /// </summary>
    /// <typeparam name="T">Where <typeparamref name="T"/> is a class of type <see cref="IPoolNodeItem"/>.</typeparam>
    public class Pool<T> : IEnumerable<T> where T : class, IPoolNodeItem, new()
    {
        // 3/23/2009 - 'Get' & 'Return' Events.
        ///<summary>
        /// Occurs when some <see cref="Pool{TValue}"/> item is retrieved.
        ///</summary>
        public event PoolMemoryEventHandler PoolItemGetCalled;

        ///<summary>
        /// Occurs when some <see cref="Pool{TValue}"/> item is returned.
        ///</summary>
        public event PoolMemoryEventHandler PoolItemReturnCalled;

        /// <summary>
        /// Fixed collection of <see cref="PoolNode"/> instances.
        /// </summary>
        internal PoolNode[] PoolNodes;


        /// <summary>
        /// Array containing the 'active' state for each <see cref="PoolNode"/> instance 
        /// in the <see cref="Pool{TValue}"/>.
        /// </summary>
        private bool[] _active;

        // 6/18/2010 - Updated to a LocklessQueue.
        /// <summary>
        /// Queue of 'available' <see cref="PoolNode"/> indices.
        /// </summary>
        private LocklessQueue<int> _available;


        /// <summary>
        /// Gets the number of 'available' items in the <see cref="Pool{TValue}"/>.
        /// </summary>
        /// <remarks>
        /// Retrieving this property is an O(1) operation.
        /// </remarks>
        public int AvailableCount
        {
            get { return _available.Count; }
        }


        /// <summary>
        /// Gets the number of 'active' items in the <see cref="Pool{TValue}"/>.
        /// </summary>
        /// <remarks>
        /// Retrieving this property is an O(1) operation.
        /// </remarks>
        public int ActiveCount
        {
            get { return PoolNodes.Length - _available.Count; }
        }


        /// <summary>
        /// Gets the total number of items in the <see cref="Pool{TValue}"/>.
        /// </summary>
        /// <remarks>
        /// Retrieving this property is an O(1) operation.
        /// </remarks>
        public int Capacity
        {            
            get { return PoolNodes.Length; }
        }

        // 4/16/2009
        /// <summary>
        /// Allow capacity increases?  When set, this
        /// will increase the internal capacity by the 
        /// <see cref="IncreaseCapacityBy"/> value, when the 'available'
        /// nodes runs out!
        /// </summary>
        public bool AllowCapacityIncreases { get; private set; }

        // 4/16/2009
        /// <summary>
        /// When <see cref="AllowCapacityIncreases"/> is TRUE, the
        /// internal capacity will be increased by the given value.
        /// </summary>
        public int IncreaseCapacityBy { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pool{TValue}"/> class.
        /// </summary>        
        public Pool()
        {
           // Empty
            return;
        }

        // 6/29/2009
        /// <summary>
        /// Sets the capacity for the <see cref="PoolNode"/>, and the allowed and increased capacity limits.
        /// </summary>
        /// <param name="capacity">Initial capacity of pool</param>
        /// <param name="allowCapIncreases">When capacity reached, if increases are allowed?</param>
        /// <param name="increaseCapacityBy">If <paramref name="allowCapIncreases"/> set, by how much to increase?</param>
        public void SetPoolNodeCapacities(int capacity, bool allowCapIncreases, int increaseCapacityBy)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException("capacity");
            }

            // 4/16/2009 - Set 'AllowCap' & 'CapIncrease' values.
            AllowCapacityIncreases = allowCapIncreases;
            IncreaseCapacityBy = increaseCapacityBy;

            PoolNodes = new PoolNode[capacity];
            _active = new bool[capacity];
            _available = new LocklessQueue<int>(); // 6/18/2010 - Updated to LocklessQueue

            try
            {
                // 4/16/2009
                PopulatePoolNodes(0, capacity);
            }
            catch (System.Reflection.TargetInvocationException err)
            {
                TemporalWars3DEngine.GameInstance.Window.Title = err.ToString();
            }
        }

        // 4/16/2009
        /// <summary>
        /// Populates the internal arrays with new <see cref="PoolNode"/> instances.
        /// </summary>
        private void PopulatePoolNodes(int startIndex, int endIndex)
        {
            for (var i = startIndex; i < endIndex; i++)
            {
                PoolNodes[i] = new PoolNode {NodeIndex = i, Item = new T()};

                _active[i] = false;
                _available.Enqueue(i);
            }
        }


        /// <summary>
        /// Makes all items in the <see cref="Pool{TValue}"/> 'available'.
        /// </summary>
        /// <remarks>
        /// This method is an O(n) operation, where n is Capacity.
        /// </remarks>
        public void Clear()
        {
            // 6/18/2010 - Clear manually, since LocklessQueue doesn't have a clear.
            //_available.Clear();
            while (!_available.IsEmpty)
            {
                int result;
                _available.TryDequeue(out result);
            }

            var length = PoolNodes.Length;
            for (var i = 0; i < length; i++)
            {
                _active[i] = false;
                _available.Enqueue(i);
            }
        }       

        // 2/23/2009: Updated to return 'boolean' and then return 'PoolNode' via parameter.
        /// <summary>
        /// Removes an _available <see cref="PoolNode"/> from the <see cref="Pool{TValue}"/> and makes it 'active'.
        /// </summary>
        /// <param name="poolNode">(OUT) <see cref="PoolNode"/> instance</param>
        /// <returns>True/False of result</returns>       
        /// <remarks>
        /// This method is an O(1) operation.
        /// </remarks>
        public bool Get(out PoolNode poolNode) // was PoolNode?
        {
            poolNode = null;
         
            // 2/23/2009 - Check if Queue empty first
            if (_available.Count > 0)
            {
                if (!GetAvailablePoolNode(out poolNode))
                    return false; // 6/18/2010

                // 6/18/2010 - Check if null still
                if (poolNode == null) return false;

                // 1/7/2010 - Attach EventHandler for the Return event.
                poolNode.ReturnToPoolEvent += PoolNode_ReturnToPool;
                
                return true;
            }

            // check if allowed to increase capacity
            if (AllowCapacityIncreases)
            {
                // set new size
                var currentCapacity = Capacity;
                var newCapacity = Capacity + IncreaseCapacityBy;

                // resize internal arrays
                Array.Resize(ref PoolNodes, newCapacity);
                Array.Resize(ref _active, newCapacity);                    

                // populate with new nodes
                PopulatePoolNodes(currentCapacity, newCapacity);      

                // Now get new node for caller.
                if (!GetAvailablePoolNode(out poolNode))
                    return false; // 6/18/2010

                // 6/18/2010 - Check if null still
                if (poolNode == null) return false;

                // 1/7/2010 - Attach EventHandler for the Return event.
                poolNode.ReturnToPoolEvent += PoolNode_ReturnToPool;

                return true;
                    
            } // End If AllowCapIncreases

            return false;
        }

        // 1/7/2010
        /// <summary>
        /// <see cref="EventHandler"/>, which captures the <see cref="PoolNode"/> return call.
        /// </summary>
        void PoolNode_ReturnToPool(object sender, EventArgs e)
        {
            // Get PoolNode, and call the internal Return method.
            var poolNode = (PoolNode) sender;
            Return(poolNode);
        }

        // 4/16/2009; 6/18/2010 - Add Bool return value.
        /// <summary>
        /// Gets an 'available' node to use from the 'Available' Queue.
        /// </summary>
        /// <param name="poolNode">(OUT) <see cref="PoolNode"/> instance</param>
        /// <returns>true/false of result</returns>
        private bool GetAvailablePoolNode(out PoolNode poolNode)
        {
            poolNode = null;

            // 6/18/2010 - Update to use TryDequeue.
            int nodeIndex;
            if (!_available.TryDequeue(out nodeIndex))
                return false;

            _active[nodeIndex] = true;
            poolNode = PoolNodes[nodeIndex];

            // 3/23/2009 - Trigger Event.
            if (PoolItemGetCalled == null) return true;

            var poolEventArgs = new PoolEventArgs { NodeIndex = nodeIndex };
            PoolItemGetCalled(this, poolEventArgs);

            return true;
        }

        /// <summary>
        /// Returns an 'active' <see cref="PoolNode"/> to the 'available' <see cref="Pool{TDefault}"/>.
        /// </summary>
        /// <param name="item">The node to return to the 'availabl' Pool.</param>
        /// <exception cref="ArgumentException">
        /// The node being returned is invalid.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The node being returned was not 'active'.
        /// This probably means the node was previously returned.
        /// </exception>
        /// <remarks>
        /// This method is an O(1) operation.
        /// </remarks>
        private void Return(PoolNode item)
        {
            if ((item.NodeIndex < 0) || (item.NodeIndex > PoolNodes.Length))
            {
                throw new ArgumentException("Invalid SceneItemOwner node.");
            }

            if (!_active[item.NodeIndex])
            {
                //throw new InvalidOperationException("Attempt to return an inactive node.");
                // 4/16/2009 - just return when this occurs!
                return;
            } 
          
            // 1/7/2010 - Remove EventHandler.
            item.ReturnToPoolEvent -= PoolNode_ReturnToPool;


            _active[item.NodeIndex] = false;
            _available.Enqueue(item.NodeIndex);

            // 3/23/2009 - Trigger Event.
            if (PoolItemReturnCalled == null) return;
           

            var poolEventArgs = new PoolEventArgs
                                    {
                                        NodeIndex = item.NodeIndex
                                        
                                    };

            PoolItemReturnCalled(this, poolEventArgs);
        }


        /// <summary>
        /// Sets the value of the <see cref="PoolNode"/> in the <see cref="Pool{TDefault}"/> associated with the 
        /// given node.
        /// </summary>
        /// <param name="item">The node whose <see cref="PoolNode"/> value is to be set.</param>
        /// <exception cref="ArgumentException">
        /// The node being returned is invalid.
        /// </exception>
        /// <remarks>
        /// This method is necessary to modify the value of a value type stored
        /// in the <see cref="Pool{TDefault}"/>.  It copies the value of the node's 
        /// <see cref="PoolNode"/> field into the Pool.
        /// This method is an O(1) operation.
        /// </remarks>
        public void SetItemValue(PoolNode item)
        {
            if ((item.NodeIndex < 0) || (item.NodeIndex > PoolNodes.Length))
            {
                throw new ArgumentException("Invalid SceneItemOwner node.");
            }

            PoolNodes[item.NodeIndex].Item = item.Item;
        }


        /// <summary>
        /// Copies the 'active' items to an existing one-dimensional collection, 
        /// starting at the specified array index. 
        /// </summary>
        /// <param name="array">
        /// The one-dimensional collection to which 'active' <see cref="PoolNode"/> items will be 
        /// copied.
        /// </param>
        /// <param name="arrayIndex">
        /// The index in array at which copying begins.
        /// </param>
        /// <returns>The number of items copied.</returns>
        /// <remarks>
        /// This method is an O(n) operation, where n is the smaller of 
        /// capacity or the array length.
        /// </remarks>
        public int CopyTo(T[] array, int arrayIndex)
        {
            var index = arrayIndex;

            var length = PoolNodes.Length;
            for (var i = 0; i < length; i++)
            {
                var item = PoolNodes[i];
                if (!_active[item.NodeIndex]) continue;

                array[index++] = item.Item as T;

                if (index == array.Length)
                {
                    return index - arrayIndex;
                }
            }

            return index - arrayIndex;
        }


        /// <summary>
        /// Gets an enumerator that iterates through the 'active' <see cref="PoolNode"/> 
        /// instances in the <see cref="Pool{TDefault}"/>.
        /// </summary>
        /// <returns>Enumerator for the 'active' items.</returns>
        /// <remarks>
        /// This method is an O(n) operation, 
        /// where n is Capacity divided by ActiveCount. 
        /// </remarks>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in PoolNodes)
            {
                if (_active[item.NodeIndex])
                {
                    yield return item.Item as T;
                }
            }
        }


/*
        /// <summary>
        /// Gets an enumerator that iterates through the _active nodes 
        /// in the Pool.
        /// </summary>
        /// <remarks>
        /// This method is an O(n) operation, 
        /// where n is Capacity divided by ActiveCount. 
        /// </remarks>
        public IEnumerable<PoolNode> ActiveNodes
        {
            get
            {
                foreach (PoolNode item in PoolNodes)
                {
                    if (_active[item.NodeIndex])
                    {
                        yield return item;
                    }
                }
            }
        }
*/


        /// <summary>
        /// Gets an enumerator that iterates through all of the <see cref="PoolNode"/> 
        /// instances in the <see cref="Pool{TDefault}"/>.
        /// </summary>
        /// <remarks>
        /// This method is an O(1) operation. 
        /// </remarks>
        public IEnumerable<PoolNode> AllNodes
        {
            get
            {
                foreach (var item in PoolNodes)
                {
                    yield return item;
                }
            }
        }


        /// <summary>
        /// Implementation of the IEnumerable interface.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
