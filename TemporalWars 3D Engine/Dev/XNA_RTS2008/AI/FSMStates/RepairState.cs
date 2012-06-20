#region File Description
//-----------------------------------------------------------------------------
// RepairState.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using TWEngine.AI.Enums;
using TWEngine.Players;
using TWEngine.SceneItems;
using Microsoft.Xna.Framework;

namespace TWEngine.AI.FSMStates
{
    /// <summary>
    /// Repair state, used in the FSM_Machine class; in the Repair state, the
    /// current unit pathfinds it way back to HQ (Head Quarters), and then 
    /// self-repairs.  Once repaired, then returns back to the original position
    /// it was at, before attempting the repair.
    /// </summary>
    sealed class RepairState : FSM_State
    {
        // 8/2/2009
        BuildingScene _hqBuilding;

        /// <summary>
        /// Creates the Repair FSM_State, setting the
        /// FSM_StateType enum to 'Repair'.
        /// </summary>
        /// <param name="parent">Reference to the FSM_AIControl parent instance</param>
        public RepairState(FSM_AIControl parent)
            : base(parent)
        {
            Type = FSM_StateType.FSM_STATE_REPAIR;
            Parent = parent;
        }

        /// <summary>
        /// First called when the FSM_State is activated; currently, starts the pathfinding
        /// to the HQ order.
        /// </summary>
        public override void Enter()
        {
            //
            // Pathfind to the HQ to get repaired.
            //     
            Player.GetPlayersHeadQuarters(Parent.SceneItemOwner.PlayerNumber, out _hqBuilding);

            // move to HQ for repairs
            //Parent._sceneItem.PathToPositionWithinAttackingRange(_hqBuilding);
            var tmpPosition = _hqBuilding.Position + new Vector3(0, 0, 500);
            Parent.SceneItemOwner.AStarItemI.AddWayPointGoalNode(ref tmpPosition);
           
        }

        // 3/4/2011 - Updated to include GameTime param.
        /// <summary>
        /// Core logic for the FSM_State, which is called every cycle; currently, this
        /// checks if the unit is within the HQ repair range to start the repairs.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Execute(GameTime gameTime)
        {
            base.Execute(gameTime);

            // check if SceneItemOwner is within HQ repair range, so a self-repair can start.
            if (!Parent.SceneItemOwner.IsWithinRepairRadius(_hqBuilding) || Parent.SceneItemOwner.StartSelfRepair)
                return;

            // then start repairs.
            Parent.SceneItemOwner.StartSelfRepair = true;
             
            // send other MP player same order.
            Parent.SceneItemOwner.SendStartSelfRepairToMPPlayer();
        }

        /// <summary>
        /// Is called last, as the FSM_State is exiting; currently, sets the
        /// DoRepair flag to 'False'.
        /// </summary>
        public override void Exit()
        {
            // 10/21/2009 - Turn of 'DoRepair'.
            Parent.SceneItemOwner.DoRepair = false;
        }

        /// <summary>
        /// Checks if the current unit is at least 95% repaired; if true, then
        /// sets pathfinding order back to original location, and returns to Idle
        /// state.
        /// </summary>
        /// <returns>Current FSM_StateType to transition to</returns>
        public override FSM_StateType CheckTransitions()
        {
            // Check if almost fully repaired, at least 95%.
            if (Parent.SceneItemOwner.IsHealthAtOrAbovePercentile(0.95f))
            {
                // return to previous Position
                Parent.SceneItemOwner.AStarItemI.AddWayPointGoalNode(ref Parent.PreviousPositionPriorRepair);

                // then transtion back to the IdleState
                return FSM_StateType.FSM_STATE_IDLE;
            }

            return FSM_StateType.FSM_STATE_REPAIR;
        }

       
    }
}