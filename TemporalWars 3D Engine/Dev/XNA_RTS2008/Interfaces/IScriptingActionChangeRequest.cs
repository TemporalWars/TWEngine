#region File Description
//-----------------------------------------------------------------------------
// IScriptingActionChangeRequest.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using TWEngine.SceneItems;

namespace TWEngine.Interfaces
{
    /// <summary>
    /// The interface for the abstract ScriptingAction change request.
    /// </summary>
    public interface IScriptingActionChangeRequest: IDisposable
    {
        /// <summary>
        /// Gets or sets the current index value for ScenaryItems.
        /// </summary>
        int InstancedItemPickedIndex { get; }

        /// <summary>
        /// Gets or sets the current <see cref="SceneItem"/> to update.
        /// </summary>
        SceneItem SceneItemToUpdate { get; }

        /// <summary>
        /// Gets if the change request operation is completed.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Gets or sets the Delta magnitude. (Rate of change)
        /// </summary>
        /// <remarks>
        /// Default is 2F.
        /// </remarks>
        float DeltaMagnitude { get; set; }

        /// <summary>
        /// Gets ir sets the current Delta.
        /// </summary>
        float Delta { get; set; }

        /// <summary>
        /// Gets or sets the terminate action state.
        /// </summary>
        bool TerminateAction { get; set; }

        // 6/9/2012
        /// <summary>
        /// Gets the current <see cref="IScriptingActionChangeRequest"/> unique key GUID.
        /// </summary>
        Guid UniqueKey { get; }

        /// <summary>
        /// Updates the current change request.
        /// </summary>
        void Update(GameTime gameTime);
    }
}