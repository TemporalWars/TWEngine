#region File Description
//-----------------------------------------------------------------------------
// FSM_AIControl.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.AI.FSMStates;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.Players;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWEngine.SceneItems.Enums;
using TWEngine;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.AI
{
    ///<summary>
    /// This F.S.M. (Finite State Machine) class, specifically manages the AI
    /// for the in-game units; for example, Defense or Attack state AI.
    ///</summary>
// ReSharper disable InconsistentNaming
    public class FSM_AIControl
// ReSharper restore InconsistentNaming
    {
        
        private readonly FSM_Machine _machine;
        /// <summary>
        /// The <see cref="SceneItemOwner"/> instance who is owner of this <see cref="FSM_AIControl"/> instance.
        /// </summary>
        protected internal readonly SceneItemWithPick SceneItemOwner;

        
        // Perception data
        // (Public so all states can share)
        /// <summary>
        /// Stores the current <see cref="DefenseAIStance"/>.
        /// </summary>
        protected internal DefenseAIStance CurrentDefenseState = DefenseAIStance.Guard; // Set by HandleGameInput class.  
        /// <summary>
        /// Stores the previous position as <see cref="Vector3"/> prior to unit repair.
        /// </summary>
        protected internal Vector3 PreviousPositionPriorRepair = Vector3.Zero; // 8/2/2009
        /// <summary>
        /// Stores the instance of this <see cref="Player"/>.
        /// </summary>
        protected internal Player ThisPlayer; // 10/19/09

        // constructor
        ///<summary>
        /// Constructor for this F.S.M. (Finite State Machine) AI control class, which
        /// creates the main 'FSM_Machine' instance, and adds the basic states, like the
        /// 'Idle_state' and 'Attack_state'.
        ///</summary>
        ///<param name="sceneItemOwner"><see cref="SceneItemOwner"/> instance who is owner of this <see cref="FSM_AIControl"/> instance</param>
        public FSM_AIControl(SceneItemWithPick sceneItemOwner)
        {
            SceneItemOwner = sceneItemOwner;

            // 6/15/2010 - Updated to use new GetPlayer method.
            TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out ThisPlayer);

            // construct the state machine, and add necessary states
            _machine = new FSM_Machine(this);

            var idleState = new IdleState(this);

            _machine.AddState(idleState);
            _machine.AddState(new AttackState(this));
            //_machine.AddState(new RepairState(this)); // 8/2/2009
            _machine.SetDefaultState(idleState);
            _machine.AddState(new AttackMoveState(this)); // 10/19/2009

        }

        // constructor for DefenseScene Items
        ///<summary>
        /// Constructor for this F.S.M. (Finite State Machine) AI control class, which
        /// creates the main 'FSM_Machine' instance, and adds the basic states, like the
        /// 'Idle_state' and 'Attack_state'.
        /// NOTE: This overload version for the 'DefenseScene' instance owner, specifically 
        /// adds the 'DefenseIdle_State' to the FSM_Machine.
        ///</summary>
        ///<param name="sceneItemOwner">SceneItem instance who is owner of this <see cref="FSM_AIControl"/> instance</param>
// ReSharper disable SuggestBaseTypeForParameter
        public FSM_AIControl(DefenseScene sceneItemOwner)
// ReSharper restore SuggestBaseTypeForParameter
        {
            SceneItemOwner = sceneItemOwner;

            // construct the state machine, and add necessary states
            _machine = new FSM_Machine(this);

            var idleState = new DefenseIdleState(this);

            _machine.AddState(idleState);
            _machine.AddState(new DefenseAttackState(this));
            _machine.SetDefaultState(idleState);

        }

        // 3/4/2011 - Updated to include GameTime param.
        /// <summary>
        /// Updates this FSMAIControl, by calling the internal 
        /// UpdatePerceptions method, and the 'Update' method of
        /// the FSMMachine instance.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            if (SceneItemOwner == null)
            {
                _machine.Reset();
                return;
            }

            // 11/7/2009 - Make sure 'ThisPlayer' is not NULL.
            if (ThisPlayer == null)
            {
                // 6/15/2010 - Updated to use new GetPlayer method.
                TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out ThisPlayer);
            }

            UpdatePerceptions();
            _machine.UpdateMachine(gameTime);
        }
       
        /// <summary>
        /// Sets the current <see cref="DefenseAIStance"/>.
        /// </summary>
        private void UpdatePerceptions()
        {
            // 8/2/2009 - Set the CurrentDefenseState
            CurrentDefenseState = SceneItemOwner.DefenseAIStance;

            return;
        }


    }
}