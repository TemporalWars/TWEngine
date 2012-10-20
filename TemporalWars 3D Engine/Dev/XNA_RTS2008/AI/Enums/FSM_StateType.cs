#region File Description
//-----------------------------------------------------------------------------
// FSM_StateType.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.SceneItems;

namespace ImageNexus.BenScharbach.TWEngine.AI.Enums
{
    // 8/21/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.AI.Enums"/> namespace contains the classes
    /// which make up the entire <see cref="FSM_StateType"/>.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    // 10/16/2012: Updated enum to inherit from short value.
// ReSharper disable InconsistentNaming
    /// <summary>
    /// The Enum <see cref="FSM_StateType"/> is used to set the 
    /// FMS_State of some <see cref="SceneItem"/>.
    /// </summary>
    enum FSM_StateType : short
    {
        /// <summary>
        /// No state given.
        /// </summary>
        FSM_STATE_NONE,
        /// <summary>
        /// In the Idle state, the given unit checks for any attackies to attack,
        ///  repairs for itself, or for any Attack move orders.
        /// </summary>
        FSM_STATE_IDLE,
        /// <summary>
        /// In the Repair state, the current unit pathfinds it way back to HQ (Head Quarters),
        /// and then self-repairs.  Once repaired, then returns back to the original position
        /// it was at, before attempting the repair.
        /// </summary>
        FSM_STATE_REPAIR,
        /// <summary>
        /// Specifically makes a unit stop and attack some enemy unit.
        /// </summary>
        FSM_STATE_ATTACK,
        /// <summary>
        /// The AttackMove state allows a in-game unit to move to some user-goal 
        /// with the intent to stop and attack any enemy units found along the way.
        /// </summary>
        FSM_STATE_ATTACKMOVE // 10/19/2009
// ReSharper restore InconsistentNaming
    }
}