#region File Description
//-----------------------------------------------------------------------------
// ScriptingActionMovementOnPathsRequest.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using TWEngine.GameLevels.ChangeRequests.Enums;
using TWEngine.GameLevels.ChangeRequests.Structs;
using TWEngine.Interfaces;
using TWEngine.SceneItems;

namespace TWEngine.GameLevels.ChangeRequests
{
    // 5/29/2012
    /// <summary>
    /// This <see cref="ScriptingActionMovementOnPathsRequest"/> class is used to have some <see cref="SceneItem"/> follow a given Waypoint path,
    /// using different ScriptingAction change requests for each edge; like a rotation request on Edge-1 while moving.
    /// </summary>
    public class ScriptingActionMovementOnPathsRequest : ScriptingActionChangeRequestAbstract
    {
        private readonly Queue<MovementOnPathAttributes> _edgeMovementQueue = new Queue<MovementOnPathAttributes>();

        private readonly Queue<MovementOnPathAttributes> _edgeMovementQueueUsed = new Queue<MovementOnPathAttributes>();
        private readonly Stack<MovementOnPathAttributes> _edgeMovementStackUsed = new Stack<MovementOnPathAttributes>();

        private IScriptingActionChangeRequest _currentMovementAction;
        private IScriptingActionChangeRequest _currentRotationAction;
        private IScriptingActionChangeRequest _currentScaleAction;

        private int _repeatCount;

        #region Properties

        /// <summary>
        /// Gets if this is a 'Close-Loop' waypoint path.
        /// </summary>
        public bool IsCloseLoop { get; private set; }

        /// <summary>
        /// Gets or sets the repetition of the overall operation.
        /// </summary>
        /// <remarks>( -1 = Continous; 0 = No-Repeat; 1+ = Repetition )</remarks>
        public int Repeat { get; set; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sceneItem"></param>
        /// <param name="instancedItemPickedIndex"></param>
        public ScriptingActionMovementOnPathsRequest(SceneItem sceneItem, int instancedItemPickedIndex) 
            : base(sceneItem, instancedItemPickedIndex)
        {
          
        }

        /// <summary>
        /// Updates the current change request.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            if (TerminateAction || IsCompleted)
            {
                return;
            }

            // check if overall operation completed
            if (_edgeMovementQueue.Count == 0)
            {
                // if 'repeat' set to zero, then operation completed.
                if (Repeat == 0)
                {
                    IsCompleted = true;
                    return;
                }

                // if 'repeat' set to -1, then non-stop continuous operation
                if (Repeat == -1 || Repeat > 0)
                {
                    _repeatCount++;
                    if (Repeat != -1 && _repeatCount == Repeat)
                    {
                        IsCompleted = true;
                        return;
                    }

                    // Moves the used nodes back into production.
                    MoveUsedToProduction();
                }
               
            }

            // check if new item needs to be dequeued
            if (_currentMovementAction == null)
            {
                var edgeMovementRequest = _edgeMovementQueue.Dequeue();

                // Add back to 'used' collections, required for continous repetition.
                AddToProperUsedCollection(edgeMovementRequest);

                // iterate internal collection of ScriptingAction request
                var count = edgeMovementRequest.ScriptingActions.Count;
                for (var i = 0; i < count; i++)
                {
                    var scriptingAction = edgeMovementRequest.ScriptingActions[i];

                    // create the new scripting action request
                    CreateScriptingActionRequest(ref scriptingAction);
                }
            }

            // check if currentMovementAction has completed.
            if (_currentMovementAction != null && _currentMovementAction.IsCompleted)
            {
                // then set this ref to null, so the next edge will be dequeued.
                _currentMovementAction = null;
            }
           
        }

        /// <summary>
        /// Helper method which moves the used <see cref="MovementOnPathAttributes"/> nodes back into
        /// the production Queue.
        /// </summary>
        private void MoveUsedToProduction()
        {
            if (IsCloseLoop)
            {
                // move the nodes from the 'Used', and add back into production
                while (_edgeMovementQueueUsed.Count != 0)
                {
                    var edgeMovementRequest = _edgeMovementQueueUsed.Dequeue();
                    _edgeMovementQueue.Enqueue(edgeMovementRequest);
                }
                return;
            }

            // move the nodes from the 'Used' stack to reverse order,
            // and add back into production
            while (_edgeMovementStackUsed.Count != 0)
            {
                var edgeMovementRequest = _edgeMovementStackUsed.Pop();

                // since reversing direction, MUST reverse order of 'Start' and 'Goal' nodes per edge
                edgeMovementRequest.ReverseOperations();

                _edgeMovementQueue.Enqueue(edgeMovementRequest);
            }
        }

        /// <summary>
        /// Helper method which adds the given <see cref="MovementOnPathAttributes"/> to the proper
        /// used collection, depending on the <see cref="IsCloseLoop"/> state.
        /// </summary>
        private void AddToProperUsedCollection(MovementOnPathAttributes edgeMovementRequest)
        {
            if (IsCloseLoop)
            {
                _edgeMovementQueueUsed.Enqueue(edgeMovementRequest);
            }
            else
            {
                _edgeMovementStackUsed.Push(edgeMovementRequest);
            }
        }

        /// <summary>
        /// Helper method which creates the proper change request based on given <see cref="ScriptingActionChangeRequestEnum"/>.
        /// </summary>
        /// <param name="scriptingAction">The <see cref="ScriptingActionAttributes"/> structure.</param>
        private void CreateScriptingActionRequest(ref ScriptingActionAttributes scriptingAction)
        {
            // create request using enum set
            switch (scriptingAction.ChangeRequestEnum)
            {
                case ScriptingActionChangeRequestEnum.RotationRequest:
                    // Populate attributes for given creation request.
                    {
                        var attributes = ScriptingActionChangeRequestManager.ChangeRequestAttributes;
                        attributes.SceneItem = scriptingAction.ChangeRequestAttributes.SceneItem;
                        attributes.RotationType = scriptingAction.ChangeRequestAttributes.RotationType;
                        attributes.RotationDirection = scriptingAction.ChangeRequestAttributes.RotationDirection;
                        attributes.RotationTimeMax = scriptingAction.ChangeRequestAttributes.RotationTimeMax;
                        attributes.InstancedItemPickedIndex =
                            scriptingAction.ChangeRequestAttributes.InstancedItemPickedIndex;
                        attributes.DeltaMagnitude = scriptingAction.ChangeRequestAttributes.DeltaMagnitude;
                        ScriptingActionChangeRequestManager.ChangeRequestAttributes = attributes;

                        // if older request still exist, kill it off.
                        if (_currentRotationAction != null)
                        {
                            _currentRotationAction.TerminateAction = true;
                        }

                        // Create new ScriptingActionRotationRequest instance and queue.
                        _currentRotationAction =
                            ScriptingActionChangeRequestManager.CreateActionChangeRequest(
                                ScriptingActionChangeRequestEnum.RotationRequest);
                    }
                    break;
                case ScriptingActionChangeRequestEnum.ScaleRequest:
                    {
                        // Populate attributes for given creation request.
                        var attributes = ScriptingActionChangeRequestManager.ChangeRequestAttributes;
                        attributes.SceneItem = scriptingAction.ChangeRequestAttributes.SceneItem;
                        attributes.Scale = scriptingAction.ChangeRequestAttributes.Scale;
                        attributes.ScaleType = scriptingAction.ChangeRequestAttributes.ScaleType;
                        attributes.InstancedItemPickedIndex = scriptingAction.ChangeRequestAttributes.InstancedItemPickedIndex;
                        attributes.DeltaMagnitude = scriptingAction.ChangeRequestAttributes.DeltaMagnitude;
                        ScriptingActionChangeRequestManager.ChangeRequestAttributes = attributes;

                        // if older request still exist, kill it off.
                        if (_currentScaleAction != null)
                        {
                            _currentScaleAction.TerminateAction = true;
                        }

                        // Create new ScriptingActionRotationRequest instance and queue.
                        _currentScaleAction = ScriptingActionChangeRequestManager.CreateActionChangeRequest(
                            ScriptingActionChangeRequestEnum.ScaleRequest);
                    }
                    break;
                case ScriptingActionChangeRequestEnum.TossMovementRequest:
                    {
                        // Populate attributes for given creation request.
                        var attributes = ScriptingActionChangeRequestManager.ChangeRequestAttributes;

                        attributes.SceneItem.MoveToWayPosition = scriptingAction.ChangeRequestAttributes.GoalPosition;
                        attributes.SceneItem = scriptingAction.ChangeRequestAttributes.SceneItem;
                        attributes.WaypointIndex = scriptingAction.ChangeRequestAttributes.WaypointIndex;
                        attributes.MaxVelocity = scriptingAction.ChangeRequestAttributes.MaxVelocity;
                        attributes.InstancedItemPickedIndex = scriptingAction.ChangeRequestAttributes.InstancedItemPickedIndex;
                        attributes.AccuracyPercent = scriptingAction.ChangeRequestAttributes.AccuracyPercent;
                        attributes.ErrorDistanceOffset = scriptingAction.ChangeRequestAttributes.ErrorDistanceOffset;
                        attributes.UpForce = scriptingAction.ChangeRequestAttributes.UpForce;

                        ScriptingActionChangeRequestManager.ChangeRequestAttributes = attributes;

                        // Create new ScriptingActionRotationRequest instance and queue.
                        _currentMovementAction =
                            ScriptingActionChangeRequestManager.CreateActionChangeRequest(
                                ScriptingActionChangeRequestEnum.TossMovementRequest);
                    }
                    break;
                case ScriptingActionChangeRequestEnum.MovementRequest:
                    {
                        // Populate attributes for given creation request.
                        var attributes = ScriptingActionChangeRequestManager.ChangeRequestAttributes;
                        
                        attributes.SceneItem = scriptingAction.ChangeRequestAttributes.SceneItem;
                        attributes.StartPosition = scriptingAction.ChangeRequestAttributes.StartPosition;
                        attributes.SceneItem.MoveToWayPosition = scriptingAction.ChangeRequestAttributes.GoalPosition;
                        attributes.MaxVelocity = scriptingAction.ChangeRequestAttributes.MaxVelocity;
                        attributes.InstancedItemPickedIndex = scriptingAction.ChangeRequestAttributes.InstancedItemPickedIndex;
                        attributes.KeepOnGround = scriptingAction.ChangeRequestAttributes.KeepOnGround;

                        ScriptingActionChangeRequestManager.ChangeRequestAttributes = attributes;

                        // Create new ScriptingActionRotationRequest instance and queue.
                        _currentMovementAction =
                            ScriptingActionChangeRequestManager.CreateActionChangeRequest(
                                ScriptingActionChangeRequestEnum.MovementRequest);
                    }
                    break;
                case ScriptingActionChangeRequestEnum.PathMovementRequest:
                    // Skip for now.
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Populates the internal queue with given collection.
        /// </summary>
        /// <param name="edgeMovements">Read-Only-Collection of <see cref="MovementOnPathAttributes"/> edge requests.</param>
        internal void PopulateEdgeMovements(List<MovementOnPathAttributes> edgeMovements)
        {
            // check if collection
            if (edgeMovements == null)
            {
                throw new ArgumentNullException("waypointPathIndexes");
            }

            var count = edgeMovements.Count;
            if (count == 0)
            {
                throw new InvalidOperationException("The given waypoint collection CANNOT be empty!");
            }

            // set 'IsCloseLoop' flag; just take first edge, since flag setting same on all edges.
            IsCloseLoop = edgeMovements[0].IsCloseLoop;

            // iterate and populate the queue with the given waypoints
            for (var index = 0; index < count; index++)
            {
                var movement = edgeMovements[index];

                // verify there is at least 1 'Movement' type request.
                VerifyEdgeMovements(movement.ScriptingActions);

                // queue movement.
                _edgeMovementQueue.Enqueue(movement);
            }
        }

        /// <summary>
        /// Helper method which verifies the given collection has at least 1 movement type enum request.
        /// </summary>
        /// <param name="scriptingActions">Collection of <see cref="ScriptingActionAttributes"/></param>
        private static void VerifyEdgeMovements(IList<ScriptingActionAttributes> scriptingActions)
        {
            // iterate internal ScriptingAction request to check for 1 'Movement' type.
            var movementEnumCount = 0;
            var count = scriptingActions.Count;
            for (var j = 0; j < count; j++)
            {
                var scriptingAction = scriptingActions[j];

                if (scriptingAction.ChangeRequestEnum == ScriptingActionChangeRequestEnum.MovementRequest ||
                    scriptingAction.ChangeRequestEnum == ScriptingActionChangeRequestEnum.TossMovementRequest)
                {
                    movementEnumCount++;
                }
            }

            // throw excpetion if no movements found
            if (movementEnumCount == 0)
                throw new InvalidOperationException("The current ScriptingAction request requires at least 1 movement type request; like a 'ScriptingActionChangeRequestEnum.MovementRequest' for example.");
        }
    }
}