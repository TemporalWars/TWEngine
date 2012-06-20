#region File Description
//-----------------------------------------------------------------------------
// AttackState.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using TWEngine.AI.Enums;
using TWEngine.Common;
using TWEngine.SceneItems;
using TWEngine.SceneItems.Enums;

namespace TWEngine.AI.FSMStates
{
    /// <summary>
    /// Attack state, used in the FSM_Machine class; specifically makes a unit
    /// stop and attack some enemy unit.
    /// </summary>
    class AttackState : FSM_State
    {
        // 3/4/2011 - Optimization timers, used to reduce the # of times a method is called.
        private const float LocateSomeEnemyCheckTimeReset = 2000f;
        private float _locateSomeEnemyCheckTime = LocateSomeEnemyCheckTimeReset;

        /// <summary>
        /// Creates the Attack FSM_State, setting the
        /// FSM_StateType enum to 'Attack'.
        /// </summary>
        /// <param name="parent">Reference to the FSM_AIControl parent instance</param>
        public AttackState(FSM_AIControl parent)
            : base(parent)
        {
            Type = FSM_StateType.FSM_STATE_ATTACK;
            Parent = parent;
        }

        /// <summary>
        /// First called when the FSM_State is activated;  Calls
        /// the 'IssueAttackOrder' method, to start attack.
        /// </summary>
        public override void Enter()
        {
            // Issue AttackOrder.            
            IssueAttackOrder();
            
        }

        /// <summary>
        /// Is called last, as the FSM_State is exiting; Removes the attacking sceneItem
        /// event handler, sets the AttackOn mode to 'False', and tells any other units also
        /// attacking same unit to CeaseAttack.
        /// </summary>
        public override void Exit()
        {
            // 11/7/2009 - Check if 'Parent' is Null.
            if (Parent == null) return;

            // 11/7/2009 - cache 'SceneItemOwner', and check for 'Null'.
            var sceneItemOwner = Parent.SceneItemOwner;
            if (sceneItemOwner == null) return;

            // Remove EventHandler Reference
            if (sceneItemOwner.AttackSceneItem != null)
                sceneItemOwner.AttackSceneItem.RemoveEventHandler_SceneItemDestroyed(AttackSceneItemSceneItemDestroyed, sceneItemOwner);
            
            // Then stop trying to attack.     
            sceneItemOwner.AttackOn = false;                    
            sceneItemOwner.AIOrderIssued = AIOrderType.None;
           
            // Tell other player to cease attack.
            sceneItemOwner.SendCeaseAttackOrderToMPPlayer();

        }

        // 3/4/2011 - Updated to include GameTime param.
        /// <summary>
        /// Core logic for the FSM_State, which is called every cycle; currently, this
        /// checks if attackie is out of range, and if DefenseAIStance is set to 'Aggressive', 
        /// tells units to pathfind to nearest attacking range of fleeing unit.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Execute(GameTime gameTime)
        {
            base.Execute(gameTime);

            // 11/7/2009 - Check if 'Parent' is Null.
            if (Parent == null) return;

            // 11/7/2009 - cache 'SceneItemOwner', and check for 'Null'.
            var sceneItemOwner = Parent.SceneItemOwner;
            if (sceneItemOwner == null) return;

            // Copy 'AttackSceneItem' into local cache, to eliminate Thread Sync errors!
            var attackie = sceneItemOwner.AttackSceneItem;
            var aStarItem = sceneItemOwner.AStarItemI;
            var aiOrderIssued = sceneItemOwner.AIOrderIssued;
            
            // If NonAI order given, then skip AttackSomeNeighborItem check.
            if (aiOrderIssued == AIOrderType.NonAIAttackOrderRequest)
                return;

            // 6/3/2009 - If attackie out of range, and 'Aggressive' stance, then
            //            re-path to attackie; otherwise, just stop and forget about attackie.
            if (attackie == null || sceneItemOwner.IsAttackieInAttackRadius(attackie)) return;

            switch (Parent.CurrentDefenseState)
            {
                // 3/4/2011 - Updated to check if already found attackingPosition.
                case DefenseAIStance.Aggressive:
                    if (aStarItem.ItemState == ItemStates.Resting)
                    {
                        Vector3 newPosition;
                        DoFindAlternativeAttackPosition(sceneItemOwner);
                    }
                    break;
                case DefenseAIStance.Guard:
                case DefenseAIStance.HoldGround:
                case DefenseAIStance.HoldFire:
                    break;
            }
        }

        /// <summary>
        /// Checks current unit, and decides to either continue attacking, stop attacking, repair itself, or return
        /// to an Idle state.
        /// </summary>
        /// <returns>Current FSM_StateType to transition to</returns>
        public override FSM_StateType CheckTransitions()
        {
            // 11/7/2009 - Check if 'Parent' is Null.
            if (Parent == null) return FSM_StateType.FSM_STATE_IDLE;

            // 11/7/2009 - cache 'SceneItemOwner', and check for 'Null'.
            var sceneItemOwner = Parent.SceneItemOwner;
            if (sceneItemOwner == null) return FSM_StateType.FSM_STATE_IDLE;


            // Copy 'AttackSceneItem' into local cache, to eliminate Thread Sync errors!
            var attackie = sceneItemOwner.AttackSceneItem;

            // check if attackie null
            if (attackie == null)
            {
                // 10/19/2009 - Updated to now check if 'AttackMoveOrderIssued' is TRUE.
                return sceneItemOwner.AttackMoveOrderIssued ? FSM_StateType.FSM_STATE_ATTACKMOVE : FSM_StateType.FSM_STATE_IDLE;
            }

            // check if attackie dead.
            if (!attackie.IsAlive)
                // 10/19/2009 - Updated to now check if 'AttackMoveOrderIssued' is TRUE.
                return sceneItemOwner.AttackMoveOrderIssued ? FSM_StateType.FSM_STATE_ATTACKMOVE : FSM_StateType.FSM_STATE_IDLE;

            // 10/21/2009: Updated to check the new 'DoRepair' flag.
            // 10/19/2009 - Updated to NOT repair, if doing a AttackMove order.
            // 8/2/2009 - check if SceneItemOwner should repair itself
            if ((sceneItemOwner.CurrentHealthPercent <= 0.40f || sceneItemOwner.DoRepair)
                && !sceneItemOwner.AttackMoveOrderIssued)
            {             
                // store current Position
                Parent.PreviousPositionPriorRepair = sceneItemOwner.Position;

                return FSM_StateType.FSM_STATE_REPAIR;
            }

            // check if 'HoldFire' state.
            switch (Parent.CurrentDefenseState)
            {                
                case DefenseAIStance.HoldFire:                    
                    return FSM_StateType.FSM_STATE_IDLE;                    
            }

            // check if attackie out-of-range, and not aggressive stance, then stop attacking.
            if (!sceneItemOwner.IsAttackieInAttackRadius(attackie))
            {
                switch (Parent.CurrentDefenseState)
                {                   
                    case DefenseAIStance.Guard:
                    case DefenseAIStance.HoldGround:
                    case DefenseAIStance.HoldFire:
                        // 10/19/2009 - Updated to now check if 'AttackMoveOrderIssued' is TRUE.
                        return sceneItemOwner.AttackMoveOrderIssued ? FSM_StateType.FSM_STATE_ATTACKMOVE : FSM_StateType.FSM_STATE_IDLE;
                }
            }           

            // stay in attack state.
            return FSM_StateType.FSM_STATE_ATTACK;
        }

        // 7/31/2009
        /// <summary>
        /// Sets current unit to start an Attack on attackie.
        /// </summary>
        private void IssueAttackOrder()
        {
            // 11/7/2009 - Check if 'Parent' is Null.
            if (Parent == null) return;

            // 11/7/2009 - cache 'SceneItemOwner', and check for 'Null'.
            var sceneItemOwner = Parent.SceneItemOwner;
            if (sceneItemOwner == null) return;

            // Copy 'AttackSceneItem' into local cache, to eliminate Thread Sync errors!
            var attackie = (SceneItemWithPick)sceneItemOwner.AttackSceneItem;
            if (attackie == null) return;

            // Turn on Attack State
            sceneItemOwner.AttackOn = true;           

            // Connect EventHandler for destroyed event.
            attackie.AssignEventHandler_SceneItemDestroyed(AttackSceneItemSceneItemDestroyed, sceneItemOwner); 

            // 5/13/2009 - Send AttackOrder to other player.
            sceneItemOwner.SendStartAttackOrderToMPPlayer(attackie);

            // Get an attack Position near the attackie using the this unit's attacking range.
            // 1/1/2008: Add 'ItemMoveable' check, since items like Buildings or Defense units do not MOVE!
            if (!sceneItemOwner.ItemMoveable) return;

            //sceneItemOwner.PathToPositionWithinAttackingRange(attackie);
            
            Vector3 newPosition;
            DoFindAlternativeAttackPosition(sceneItemOwner);
        }

        // 3/4/2011; 5/30/2011 - Removed (OUT) param for Vector3 newPosition.
        /// <summary>
        /// Helper method which finds an alternative attack position.
        /// </summary>
        /// <param name="sceneItemOwner"></param>
        private void DoFindAlternativeAttackPosition(SceneItemWithPick sceneItemOwner)
        {
            if (GameTimeInstance == null) return;

            // 3/4/2011
            // Do check every few seconds
            _locateSomeEnemyCheckTime -= (float)GameTimeInstance.ElapsedGameTime.TotalMilliseconds;
            if (_locateSomeEnemyCheckTime >= 0) return;
            _locateSomeEnemyCheckTime = LocateSomeEnemyCheckTimeReset;

            // Set to 'PathFindingAI', which prevents multiply requests!
            sceneItemOwner.AStarItemI.ItemState = ItemStates.PathFindingAI;

            // 3/4/2011 - Set MoveToPosition
            sceneItemOwner.AStarItemI.MoveToPosition = sceneItemOwner.Position;

            // 3/4/2011
            Vector3 newPosition;
            if (!AStarItem.FindAlternativeGoalNodeForAttacking(sceneItemOwner.AStarItemI, out newPosition)) return;

            // Queue new Goal Position to PathFind to.
            sceneItemOwner.AStarItemI.AddWayPointGoalNode(ref newPosition);
        }

        /// <summary>
        /// EventHandler for when an attackie SceneItemOwner is destroyed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains no event data.</param>
        private void AttackSceneItemSceneItemDestroyed(object sender, EventArgs e)
        {
            // 11/7/2009
            try
            {
                // 11/7/2009 - Check if 'Parent' is Null.
                if (Parent == null) return;

                // 11/7/2009 - cache 'SceneItemOwner', and check for 'Null'.
                var sceneItemOwner = Parent.SceneItemOwner;
                if (sceneItemOwner == null) return;

                // Copy 'AttackSceneItem' into local cache, to eliminate Thread Sync errors!
                var attackie = sceneItemOwner.AttackSceneItem;

                // Set AIOrderIssued to None state.
                sceneItemOwner.AIOrderIssued = AIOrderType.None;

                // Remove EventHandler Reference
                if (attackie != null)
                    attackie.RemoveEventHandler_SceneItemDestroyed(AttackSceneItemSceneItemDestroyed, sceneItemOwner);

                // Remove Attackie Reference
                sceneItemOwner.AttackSceneItem = null;

                // If MP Game and Host, then send Client 'CeaseAttack' order.
                if (Parent.ThisPlayer != null && (Parent.ThisPlayer.NetworkSession != null &&
                                                  Parent.ThisPlayer.NetworkSession.IsHost))
                {
                    sceneItemOwner.SendCeaseAttackOrderToMPPlayer();
                }
            }
            catch (NullReferenceException err)
            {
#if DEBUG
                Debug.WriteLine("AttackSceneItemSceneItemDestroyed method threw NullRefExp.");
#endif

            }

#if DEBUG
            //System.Diagnostics.Debug.WriteLine("EventHandler triggered for AttackSceneItem_SceneItemDestroyed." + networkItemNumber.ToString());
#endif

        }

        
    }
}