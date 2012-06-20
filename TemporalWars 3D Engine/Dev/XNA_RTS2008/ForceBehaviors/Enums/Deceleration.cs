#region File Description
//-----------------------------------------------------------------------------
// Deceleration.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using TWEngine.SceneItems;
using TWEngine.ForceBehaviors.SteeringBehaviors;

namespace TWEngine.ForceBehaviors.Enums
{
    ///<summary>
    /// The <see cref="ArriveBehavior"/> makes use of these Enums to determine how quickly a <see cref="SceneItem"/>
    /// should decelerate to its target
    ///</summary>
    public enum Deceleration
    {
        ///<summary>
        /// Decelerates <see cref="SceneItem"/> slowly.
        ///</summary>
        Slow = 3,
        ///<summary>
        /// Decelerates <see cref="SceneItem"/> at normal gradual pace.
        ///</summary>
        Normal = 2,
        ///<summary>
        /// Decelerates <see cref="SceneItem"/> quickly.
        ///</summary>
        Fast = 1
    } ;
}
