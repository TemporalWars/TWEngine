#region File Description
//-----------------------------------------------------------------------------
// PoolItem.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWTools.MemoryPoolComponent.Interfaces;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWTools.MemoryPoolComponent
{
    ///<summary>
    /// An item in the <see cref="PoolManager"/>.
    ///</summary>
    public abstract class PoolItem : IPoolNodeItem
    {
        /// <summary>
        /// The <see cref="PoolNode"/> instance reference to pool node parent
        /// </summary>
        public PoolNode PoolNode { get; set; } // 1/7/2009
        
        /// <summary>
        /// Reference to the <see cref="PoolManager"/>
        /// </summary>
        public PoolManagerBase PoolManager { get; set; }

        /// <summary>
        /// Is this Node in use?
        /// </summary>
        public bool InUse { get; set; }

        /// <summary>
        /// Get or Set this <see cref="ReduceIFDCounter"/>.
        /// </summary>
        public bool ReduceIFDCounter
        {
            get { return _reduceIFDCounter; }
            set { _reduceIFDCounter = value; }
        }

        private bool _reduceIFDCounter = true; // 11/28/2009
        /// <summary>
        /// Returns a <see cref="Vector3"/> with all of its components set to zero.
        /// </summary>
        protected Vector3 ZeroPosition = Vector3.Zero;
        
    }
}