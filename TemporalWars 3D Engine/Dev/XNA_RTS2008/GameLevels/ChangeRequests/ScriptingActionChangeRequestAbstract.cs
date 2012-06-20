#region File Description
//-----------------------------------------------------------------------------
// ScriptingActionChangeRequestAbstract.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using TWEngine.Interfaces;
using TWEngine.SceneItems;

namespace TWEngine.GameLevels.ChangeRequests
{
    // 5/19/2012
    /// <summary>
    /// This abstract class <see cref="ScriptingActionChangeRequestAbstract"/> is used to create ItemType change requests, like a scale operation, time-sliced.
    /// </summary>
    public abstract class ScriptingActionChangeRequestAbstract : IScriptingActionChangeRequest
    {
        // 6/9/2012 - Unique GUID for this class instance
        private readonly Guid _uniqueKey = Guid.NewGuid();

        // Apply some simple physics
        protected const float Gravity = 9.8f; 
        protected const float AirFriction = 0.05f;
        protected const float GroundFriction = 0.95f;
        protected const float TimeMultipler = 1.0f;
        protected static readonly Vector3 Vector3Zero = Vector3.Zero;
        private float _deltaMagnitude = 2f;

        // Total life-span for this scriptingAction
        private int _maxLifeSpan = 10000; // defaults to 10 seconds.
        private double _lifeSpanElapsed;

        #region Properties

        /// <summary>
        /// Gets or sets to use the life span check, which terminates the
        /// instance of the <see cref="IScriptingActionChangeRequest"/> when
        /// then <see cref="MaxLifeSpan"/> is reached.
        /// </summary>
        /// <remarks>
        /// This is useful for scripting actions which might never reach completion; for example,
        /// a check for a goal position which is never reached.
        /// </remarks>
        public bool UseLifeSpanCheck { get; set; }

        /// <summary>
        /// Gets or sets the total life span for this instance.  Once the
        /// life span is reached, the item is terminated.
        /// </summary>
        public int MaxLifeSpan
        {
            get { return _maxLifeSpan; }
            set { _maxLifeSpan = value; }
        }

        // 6/9/2012
        /// <summary>
        /// Gets the current <see cref="IScriptingActionChangeRequest"/> unique key GUID.
        /// </summary>
        public Guid UniqueKey
        {
            get { return _uniqueKey; }
        }

        /// <summary>
        /// Gets or sets the current index value for ScenaryItems.
        /// </summary>
        public virtual int InstancedItemPickedIndex { get; private set; }

        /// <summary>
        /// Gets or sets the current <see cref="SceneItem"/> to update.
        /// </summary>
        public virtual SceneItem SceneItemToUpdate { get; private set; }

        /// <summary>
        /// Gets if the change request operation is completed.
        /// </summary>
        public virtual bool IsCompleted { get; protected set; }

        /// <summary>
        /// Gets or sets the Delta magnitude. (Rate of change)
        /// </summary>
        /// <remarks>
        /// Default is 2F.
        /// </remarks>
        public float DeltaMagnitude
        {
            get { return _deltaMagnitude; }
            set { _deltaMagnitude = value; }
        }

        /// <summary>
        /// Gets ir sets the current Delta.
        /// </summary>
        public float Delta { get; set; }

        /// <summary>
        /// Gets or sets the terminate action state.
        /// </summary>
        public bool TerminateAction { get; set; }

        #endregion


        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sceneItem">Instance of <see cref="SceneItem"/>.</param>
        /// <param name="instancedItemPickedIndex">Index value to the correct scenaryItem instance.</param>
        protected ScriptingActionChangeRequestAbstract(SceneItem sceneItem, int instancedItemPickedIndex)
        {
            SceneItemToUpdate = sceneItem;

            if (!(sceneItem is ScenaryItemScene)) return;

            if (instancedItemPickedIndex == -1)
                throw new InvalidOperationException("InstancedItemPickedItem for the ScenaryItems MUST be greater than -1.");

            InstancedItemPickedIndex = instancedItemPickedIndex;
        }

        #endregion

        /// <summary>
        /// Updates the current change request.
        /// </summary>
        public virtual void Update(GameTime gameTime)
        {
            if (!UseLifeSpanCheck) return;

            _lifeSpanElapsed += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_lifeSpanElapsed < MaxLifeSpan) return;

            TerminateAction = true;
            IsCompleted = true;
        }

        /// <summary>
        /// Does a single Delta value update for the given time-slice.
        /// </summary>
        /// <param name="scriptingActionChangeRequest">Instance of this base <see cref="ScriptingActionChangeRequestAbstract"/>.</param>
        /// <param name="gameTime">Instance of <see cref="GameTime"/>.</param>
        /// <param name="inReverse">Apply update in reverse; in other words, multiply by -1.</param>
        protected static void DoDeltaUpdate(ScriptingActionChangeRequestAbstract scriptingActionChangeRequest, GameTime gameTime, bool inReverse)
        {
            scriptingActionChangeRequest.Delta = inReverse
                                                     ? -scriptingActionChangeRequest.DeltaMagnitude*
                                                       (float) gameTime.ElapsedGameTime.TotalSeconds
                                                     : scriptingActionChangeRequest.DeltaMagnitude*
                                                       (float) gameTime.ElapsedGameTime.TotalSeconds;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public virtual void Dispose()
        {
            SceneItemToUpdate = null;
        }
    }
}