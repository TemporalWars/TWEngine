#region File Description
//-----------------------------------------------------------------------------
// BehaviorsEnum.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.SceneItems;

namespace ImageNexus.BenScharbach.TWEngine.ForceBehaviors.Enums
{
    // 10/16/2012: Updated enum to inherit from short value.
    ///<summary>
    /// Behaviors, where the value given is the sortOrder key.
    ///</summary>
    /// <remarks>
    /// All values of 1 - 99 do not return steeringForce values, which
    /// are put in front so they WILL run.  This is because the 'Calculate'
    /// method in the <see cref="TWEngine.ForceBehaviors"/> class, breaks out of the loop when
    /// the Accumulative Force is reached; consequently, if they were at the end
    /// of the list, they might not get called each game cycle!
    /// </remarks>
    public enum BehaviorsEnum : short
    {
        ///<summary>
        /// Similar to Seek, but arrives to some position using a decelartion.
        ///</summary>
        Arrive = 111,
        ///<summary>
        /// Applies force to make a <see cref="SceneItem"/> Flee from some other position or <see cref="SceneItem"/>
        ///</summary>
        Flee = 104,
        ///<summary>
        /// Applies force to cause seperation from neighbors.
        ///</summary>
        Separation = 105,
        ///<summary>
        /// Applies force to attempt to align with neighbors.
        ///</summary>
        Alignment = 106,
        ///<summary>
        /// Applies force to attempt to stick to some neighbor.
        ///</summary>
        Cohesion = 107,
        ///<summary>
        /// AbstractBehavior which uses the Seek and Arrive behaviors to follow a given A* path.
        ///</summary>
        FollowPath = 125,
        ///<summary>
        /// Applies force to attempt to avoid obstacles.
        ///</summary>
        ObstacleAvoidance = 102,
        ///<summary>
        /// Applies force to offset pursuit at some distance, another <see cref="SceneItem"/>
        ///</summary>
        OffsetPursuit = 114,
        ///<summary>
        /// Applies force to Seek to some position or <see cref="SceneItem"/>.
        ///</summary>
        Seek = 110,
        ///<summary>
        /// AbstractBehavior which turns a <see cref="SceneItem"/> toward some facing position.
        ///</summary>
        TurnToFace = 1,
        ///<summary>
        /// AbstractBehavior which turns a <see cref="SceneItem"/> turret toward some facing position.
        ///</summary>
        TurnTurret = 2,
        ///<summary>
        /// AbstractBehavior which updates the orientation of a <see cref="SceneItem"/> to match the ground it is position at.
        ///</summary>
        UpdateOrientation = 3,
        ///<summary>
        /// AbstractBehavior which updates a <see cref="DefenseScene"/> turret to face toward some attackie.
        ///</summary>
        DefenseTurretBehavior = 130,
        ///<summary>
        /// Applies force to Wander around randomly for a <see cref="SceneItem"/>
        ///</summary>
        Wander = 112,
    }
}
