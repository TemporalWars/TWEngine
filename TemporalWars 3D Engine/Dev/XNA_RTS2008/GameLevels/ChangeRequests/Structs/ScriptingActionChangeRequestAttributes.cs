#region File Description
//-----------------------------------------------------------------------------
// ScriptingActionChangeRequestAttributes.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using TWEngine.GameLevels.ChangeRequests.Enums;
using TWEngine.Interfaces;
using TWEngine.SceneItems;

namespace TWEngine.GameLevels.ChangeRequests.Structs
{
    // 5/29/2012
    /// <summary>
    /// This structure <see cref="ScriptingActionChangeRequestAttributes"/> holds attributes used during the creation
    /// of the <see cref="ScriptingActionChangeRequestManager"/> calls in the factory method.
    /// </summary>
    public struct ScriptingActionChangeRequestAttributes
    {
        /// <summary>
        /// Gets or sets the current <see cref="SceneItem"/>.
        /// </summary>
        public SceneItem SceneItem { get; set; }

        /// <summary>
        /// Gets or sets the current starting position for the <see cref="SceneItem"/>.
        /// </summary>
        public Vector3 StartPosition { get; set; }

        /// <summary>
        /// Gets or sets the current goal position for the <see cref="SceneItem"/>.
        /// </summary>
        public Vector3 GoalPosition { get; set; }

        /// <summary>
        /// Gets or set to keep the item on the ground during the movement.
        /// </summary>
        public bool KeepOnGround { get; set; }

        /// <summary>
        /// Gets or sets the waypoint goal index value.
        /// </summary>
        public int WaypointIndex { get; set; }

        /// <summary>
        /// Gets or sets the maximum velocity for the <see cref="SceneItem"/>.
        /// </summary>
        public float MaxVelocity { get; set; }

        /// <summary>
        /// Gets or sets the Index value to the correct scenaryItem instance.
        /// </summary>
        public int InstancedItemPickedIndex { get; set; }

        /// <summary>
        /// Gets or sets accuracy percent 0 - 100.
        /// </summary>
        public int AccuracyPercent { get; set; }

        /// <summary>
        /// Gets or sets the maximum size of the error circle radius.
        /// </summary>
        public float ErrorDistanceOffset { get; set; }

        /// <summary>
        /// Gets or sets the maxium 'UP' force.
        /// </summary>
        public float UpForce { get; set; }

        // 6/4/2012
        /// <summary>
        /// Gets or sets the weight of the object.
        /// </summary>
        public float ObjectWeight { get; set; }

        /// <summary>
        /// Gets or sets the rotation force to use.
        /// </summary>
        public Vector3 RotationForce { get; set; }

        /// <summary>
        /// Gets or sets the rotation type to use.
        /// </summary>
        public RotationTypeEnum RotationType { get; set; }

        /// <summary>
        /// Gets or sets the rotation direction to use.
        /// </summary>
        public RotationDirectionEnum RotationDirection { get; set; }

        /// <summary>
        /// Gets or sets to length of given rotation in milliseconds; 0 implies infinite.
        /// </summary>
        public float RotationTimeMax { get; set; }

        /// <summary>
        /// Gets or sets the Delta magnitude. (Rate of change)
        /// </summary>
        public float DeltaMagnitude { get; set; }

        /// <summary>
        /// Gets or sets the Scale for the <see cref="SceneItem"/>.
        /// </summary>
        public float Scale { get; set; }

        /// <summary>
        /// Scale type to use; grow or shrink.
        /// </summary>
        public ScaleTypeEnum ScaleType { get; set; }

        // 6/13/2012
        /// <summary>
        /// Gets or sets the total life span for this instance.  Once the
        /// life span is reached, the item is terminated.
        /// </summary>
        public int MaxLifeSpan { get; set; }

        // 6/13/2012
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
       
    }
}