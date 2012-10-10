#region File Description
//-----------------------------------------------------------------------------
// FSM_Machine.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using ImageNexus.BenScharbach.TWEngine.AI.Enums;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.AI
{
    /// <summary>
    /// This F.S.M. (Finite State Machine) Machine class, manages
    /// all FSM states throughout the game cycle.  
    /// </summary>
// ReSharper disable InconsistentNaming
    class FSM_Machine : FSM_State
// ReSharper restore InconsistentNaming
    {
        // Private
        readonly List<FSM_State> _states;
        FSM_State _currentState;
        FSM_State _defaultState;
        FSM_State _goalState;
        FSM_StateType _goalID;

        // constructor  
        /// <summary>
        /// Constructor for the FSM_Machine, which
        /// creates the internal List of FSM_States.
        /// </summary>
        /// <param name="parent">instance of <see cref="FSM_AIControl"/></param>.
        public FSM_Machine(FSM_AIControl parent)
            : base(parent)
        {
            // Initalize the List collection
            _states = new List<FSM_State>();            
        }

        // 3/4/2011 - Updated to include GameTime param.
        /// <summary>
        /// Each cycle this UpdateMachine method is called, which in turn updates the
        /// current state by calling the appropriate 'Enter', 'Execute', or 
        /// 'Exit' methods for that state.
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void UpdateMachine(GameTime gameTime)
        {
            // don't do anything, if no states.
            if (_states == null || _states.Count == 0)
                return;

            // don't do anything, if no currentState or defaulState
            if (_currentState == null)
                _currentState = _defaultState;
            if (_currentState == null)
                return;

            // update currentState, and check for transition
            var oldStateID = _currentState.Type;
            _goalID = _currentState.CheckTransitions();

            if (_goalID != oldStateID)
            {
                if (TransitionState(_goalID))
                {
                    _currentState.Exit();
                    _currentState = _goalState;
                    _currentState.Enter();
                }

            }
            _currentState.Execute(gameTime);
        }

        /// <summary>
        /// Adds a new FSM_State instance to the internal List array.
        /// </summary>
        /// <param name="stateToAdd">FSM_State instance to add</param>
        public virtual void AddState(FSM_State stateToAdd)
        {
            // make sure not null
            if (stateToAdd == null) return;

            // only add if new state
            if (!_states.Contains(stateToAdd))
                _states.Add(stateToAdd);
        }

        /// <summary>
        /// Used to set the 'Default' FSM_State for the current machine.
        /// </summary>
        /// <param name="stateToSet">FSM_State instance</param>
        public virtual void SetDefaultState(FSM_State stateToSet)
        {
            // set default state
            _defaultState = stateToSet;
        }

        /// <summary>
        /// New FSM_State to transition to during next cycle.
        /// </summary>
        /// <param name="goal">FSM_StateType enum to transition to</param>
        public virtual void SetGoalID(FSM_StateType goal)
        {
            _goalID = goal;
        }

        /// <summary>
        /// Iterates the current List of FSM_States, checking for
        /// the given 'Goal' FSM_StateType enum.  If found, this is set
        /// as the new 'Goal' state.
        /// </summary>
        /// <param name="goal">FSM_StateType enum to set as goal</param>
        /// <returns>True/False of finding goal</returns>
        public virtual bool TransitionState(FSM_StateType goal)
        {
            // iterate the states list, to verify the given stateID exist.
            var count = _states.Count;
            for (var i = 0; i < count; i++)
            {
                if (_states[i].Type != goal) continue;

                // set goalState
                _goalState = _states[i];
                return true;
            }

            return false;
        }

        /// <summary>
        /// Override in inheriting methods to reset some FSM state value; 
        /// for example, the health value in an attack state. 
        /// </summary>
        public virtual void Reset()
        {
            return;
        }
        
        // Abstract Overridden methods.

        /// <summary>
        /// First called when the FSM_State is activated.
        /// </summary>
        public override void Enter()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Is called last, as the FSM_State is exiting.
        /// </summary>
        public override void Exit()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Core logic for the FSM_State, which is called every cycle.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Execute(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks the FSM_States transitions; specifically used to transition
        /// from one state to another.
        /// </summary>
        /// <returns>Current FSM_StateType to transition to</returns>
        public override FSM_StateType CheckTransitions()
        {
            return FSM_StateType.FSM_STATE_IDLE;
        }
    }
}