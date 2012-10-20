#region File Description
//-----------------------------------------------------------------------------
// Deceleration.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.ForceBehaviors.SteeringBehaviors;
using ImageNexus.BenScharbach.TWEngine.SceneItems;

namespace ImageNexus.BenScharbach.TWEngine.ForceBehaviors.Enums
{
    // 10/16/2012: Updated enum to inherit from short value.
    ///<summary>
    /// The <see cref="ArriveBehavior"/> makes use of these Enums to determine how quickly a <see cref="SceneItem"/>
    /// should decelerate to its target
    ///</summary>
    public enum Deceleration : short
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
