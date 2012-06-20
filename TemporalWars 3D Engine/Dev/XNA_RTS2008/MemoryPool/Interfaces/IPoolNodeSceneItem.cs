#region File Description
//-----------------------------------------------------------------------------
// IPoolNodeSceneItem.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using TWEngine.SceneItems;

namespace TWEngine.MemoryPool.Interfaces
{
    /// <summary>
    /// Interface <see cref="IPoolNodeSceneItem"/>, for a <see cref="PoolNode"/> 
    /// which requires a <see cref="SceneItemWithPick"/> reference.
    /// </summary>
    public interface IPoolNodeSceneItem
    {
        /// <summary>
        /// Get or Set an instance of <see cref="SceneItemWithPick"/>.
        /// </summary>
        SceneItemWithPick SceneItemInstance { get; set; }
    }
}