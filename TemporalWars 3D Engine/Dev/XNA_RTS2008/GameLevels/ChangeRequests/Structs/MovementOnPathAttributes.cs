#region File Description
//-----------------------------------------------------------------------------
// MovementOnPathAttributes.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TWEngine.GameLevels.ChangeRequests.Enums;

namespace TWEngine.GameLevels.ChangeRequests.Structs
{
    /// <summary>
    /// The <see cref="MovementOnPathAttributes"/> structure holds a collection of scripting action request along a single waypoint edge.
    /// </summary>
    public struct MovementOnPathAttributes
    {
        /// <summary>
        /// Gets the 'Starting' position for this edge movement.
        /// </summary>
        public Vector3 StartPosition { get; private set; }
        /// <summary>
        /// Gets the 'Goal' position for this edge movement.
        /// </summary>
        public Vector3 GoalPosition { get; private set; }
        /// <summary>
        /// Gets the edge name; like 'Edge 1'.
        /// </summary>
        public string EdgeName { get; private set; }
        /// <summary>
        /// Gets the collection of <see cref="ScriptingActionAttributes"/>.
        /// </summary>
        public List<ScriptingActionAttributes> ScriptingActions { get; private set; }

        /// <summary>
        /// Gets if this edge is part of closed-loop waypoint path.
        /// </summary>
        public bool IsCloseLoop { get; set; }

        /// <summary>
        /// Constructor to initialize a single edge movement request.
        /// </summary>
        /// <param name="startPosition">Set the edge's starting position</param>
        /// <param name="goalPosition">Set the edge's goal position</param>
        /// <param name="edgeName">Set the edge's name</param>
        public MovementOnPathAttributes(Vector3 startPosition, Vector3 goalPosition, string edgeName) : this()
        {
            StartPosition = startPosition;
            GoalPosition = goalPosition;
            EdgeName = edgeName;
            ScriptingActions = new List<ScriptingActionAttributes>();
        }

        /// <summary>
        /// Updates attributes to be in reverse order, like the 'Goal' and 'Start' positions flipping order or the Scale
        /// operation changing the scale operation from growth to shrinkage.
        /// </summary>
        internal void ReverseOperations()
        {
            {
                var goalNode = GoalPosition;
                GoalPosition = StartPosition;
                StartPosition = goalNode;
            }

            // iterate actions
            var count = ScriptingActions.Count;
            for (var i = 0; i < count; i++)
            {
                var scriptingAction = ScriptingActions[i];
                
                // reverse positions
                if (scriptingAction.ChangeRequestEnum == ScriptingActionChangeRequestEnum.MovementRequest ||
                    scriptingAction.ChangeRequestEnum == ScriptingActionChangeRequestEnum.TossMovementRequest)
                {
                    var attributes = scriptingAction.ChangeRequestAttributes;

                    var goalNode = attributes.GoalPosition;
                    attributes.GoalPosition = attributes.StartPosition;
                    attributes.StartPosition = goalNode;

                    scriptingAction.ChangeRequestAttributes = attributes;
                }

                // reverse scale operation
                if (scriptingAction.ChangeRequestEnum == ScriptingActionChangeRequestEnum.ScaleRequest)
                {
                    var attributes = scriptingAction.ChangeRequestAttributes;

                    if (attributes.ScaleType == ScaleTypeEnum.Grow)
                    {
                        attributes.ScaleType = ScaleTypeEnum.Shrink;
                    }

                    if (attributes.ScaleType == ScaleTypeEnum.Shrink)
                    {
                        attributes.ScaleType = ScaleTypeEnum.Grow;
                    }

                    scriptingAction.ChangeRequestAttributes = attributes;
                }

                // reverse rotation operation
                if (scriptingAction.ChangeRequestEnum == ScriptingActionChangeRequestEnum.RotationRequest)
                {
                    var attributes = scriptingAction.ChangeRequestAttributes;

                    if (attributes.RotationDirection == RotationDirectionEnum.Forward)
                    {
                        attributes.RotationDirection = RotationDirectionEnum.Reverse;
                    }

                    if (attributes.RotationDirection == RotationDirectionEnum.Reverse)
                    {
                        attributes.RotationDirection = RotationDirectionEnum.Forward;
                    }
                }

                ScriptingActions[i] = scriptingAction;
            }
        }
    }
}