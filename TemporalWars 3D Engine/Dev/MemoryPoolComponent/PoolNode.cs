#region File Description
//-----------------------------------------------------------------------------
// PoolNode.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using MemoryPoolComponent.Interfaces;

namespace MemoryPoolComponent
{
    // 1/07/2010: Note: This was a STRUCT; however, made it a class to eliminate any referencing problems!

    /// <summary>
    /// Represents an entry in a <see cref="Pool{TDefault}"/> collections.
    /// </summary>
    public class PoolNode
    {
        // 1/7/2010
        /// <summary>
        /// Occurs when the 'ReturnToPool' is called, and the current
        /// <see cref="PoolNode"/> is ready to return to the original <see cref="Pool{TDefault}"/> owner.
        /// </summary>
        public event EventHandler ReturnToPoolEvent;

        /// <summary>
        /// Used internally to track which entry in the <see cref="Pool{TDefault}"/>
        /// is associated with this <see cref="PoolNode"/>.
        /// </summary>
        internal int NodeIndex;

        /// <summary>
        /// <see cref="IPoolNodeItem"/> stored in <see cref="Pool{TDefault}"/>.
        /// </summary>
        public IPoolNodeItem Item;
        

        // 4/16/2009; 1/7/2010: Updated to use the new Event.
        /// <summary>
        /// Triggers the <see cref="ReturnToPoolEvent"/> event.
        /// </summary>
        public void ReturnToPool()
        {
            if (ReturnToPoolEvent != null)
                ReturnToPoolEvent(this, EventArgs.Empty);
            
        }
           
    }
}