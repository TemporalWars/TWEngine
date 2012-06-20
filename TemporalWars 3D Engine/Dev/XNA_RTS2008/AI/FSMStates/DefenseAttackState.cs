#region File Description
//-----------------------------------------------------------------------------
// DefenseAttackState.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using TWEngine.AI.Enums;
using TWEngine.SceneItems;
using TWEngine.SceneItems.Enums;

namespace TWEngine.AI.FSMStates
{
    /// <summary>
    /// DefenseAttackMove state, used in the FSM_Machine class; the DefenseAttackMove state, is
    /// specficially designed to be used with the 'Defense' stationary items; for example, the base
    /// turrets.
    /// </summary>
    sealed class DefenseAttackState : AttackState
    {
        /// <summary>
        /// Creates the DefenseAttackMove FSM_State, setting the
        /// FSM_StateType enum to 'AttackMove'.
        /// </summary>
        /// <param name="parent">Reference to the FSM_AIControl parent instance</param>
        public DefenseAttackState(FSM_AIControl parent)
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
            // 11/6/2009 - Check if 'Parent' is Null.
            if (Parent == null) return;

            // 11/6/2009 - cache 'SceneItemOwner', and check for 'Null'.
            var sceneItemOwner = Parent.SceneItemOwner;
            if (sceneItemOwner == null) return;

            // 8/3/2009
            // If some 'nonAI_AttackRequest', then skip issuing attackOrder, since already done via 
            // HandleGameInput class to the Player class.
            if (sceneItemOwner.AIOrderIssued == AIOrderType.NonAIAttackOrderRequest)
                return;

            // Issue AttackOrder.            
            IssueAttackOrder();
        }

        // 3/4/2011 - Updated to include GameTime param.
        /// <summary>
        /// Core logic for the FSM_State, which is called every cycle; currently, this
        /// checks direction where attackie is, and moves the turret to face in proper
        /// direction.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Execute(GameTime gameTime)
        {
            base.Execute(gameTime);

            // 11/6/2009 - Check if 'Parent' is Null.
            if (Parent == null) return;

            // 11/6/2009 - cache 'SceneItemOwner', and check for 'Null'.
            var sceneItemOwner = Parent.SceneItemOwner;
            if (sceneItemOwner == null) return;

            // Copy 'AttackSceneItem' into local cache, to eliminate Thread Sync errors!
            var attackie = sceneItemOwner.AttackSceneItem; 

            // 7/16/2009 - Calculate new FacingDirection
            float desiredAngle;
            sceneItemOwner.CalculateDesiredAngle(attackie, out desiredAngle);
            ((DefenseScene) sceneItemOwner).DesiredAngle = desiredAngle;

            // Update the Turrets facing direction.
            ((DefenseScene)sceneItemOwner).UpdateTurretFacingDirection();
        }

        /// <summary>
        /// Is called last, as the FSM_State is exiting; Removes the attacking sceneItem
        /// event handler, sets the AttackOn mode to 'False', and tells any other units also
        /// attacking same unit to CeaseAttack.
        /// </summary>
        public override void Exit()
        {
            // 11/6/2009 - Check if 'Parent' is Null.
            if (Parent == null) return;

            // 11/6/2009 - cache 'SceneItemOwner', and check for 'Null'.
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

        /// <summary>
        /// Checks if attackie is dead, or out-of-range, and stops the attack.
        /// </summary>
        /// <returns>Current FSM_StateType to transition to</returns>
        public override FSM_StateType CheckTransitions()
        {
            // 11/6/2009 - Check if 'Parent' is Null.
            if (Parent == null) return FSM_StateType.FSM_STATE_IDLE;

            // 11/6/2009 - cache 'SceneItemOwner', and check for 'Null'.
            var sceneItemOwner = Parent.SceneItemOwner;
            if (sceneItemOwner == null) return FSM_StateType.FSM_STATE_IDLE;

            // Copy 'AttackSceneItem' into local cache, to eliminate Thread Sync errors!
            var attackie = sceneItemOwner.AttackSceneItem;            

            // check if attackie null
            if (attackie == null)
                return FSM_StateType.FSM_STATE_IDLE;

            // check if attackie dead.
            if (!attackie.IsAlive)
                return FSM_StateType.FSM_STATE_IDLE;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Players.Player player;
            TemporalWars3DEngine.GetPlayer(sceneItemOwner.PlayerNumber, out player);

            // Check if EnergyOff for the player this SceneItemOwner belongs to
            if (player != null && player.EnergyOff) return FSM_StateType.FSM_STATE_IDLE;

            // check if attackie out-of-range, then stop attacking.
            if (!sceneItemOwner.IsAttackieInAttackRadius(attackie))
            {
                // Make sure sound is stopped
                return FSM_StateType.FSM_STATE_IDLE;
            }

            return FSM_StateType.FSM_STATE_ATTACK;
        }

        // 7/31/2009
        /// <summary>
        /// Sets current unit to start an Attack on attackie.
        /// </summary>
        private void IssueAttackOrder()
        {
            // 11/6/2009 - Check if 'Parent' is Null.
            if (Parent == null) return;

            // 11/6/2009 - cache 'SceneItemOwner', and check for 'Null'.
            var sceneItemOwner = Parent.SceneItemOwner;
            if (sceneItemOwner == null) return;

            // Copy 'AttackSceneItem' into local cache, to eliminate Thread Sync errors!
            var attackie = (SceneItemWithPick)sceneItemOwner.AttackSceneItem;

            if (attackie == null)
                return;

            // Turn on Attack State
            sceneItemOwner.AttackOn = true;           

            // Connect EventHandler for destroyed event.
            attackie.AssignEventHandler_SceneItemDestroyed(AttackSceneItemSceneItemDestroyed, sceneItemOwner); 

            // 5/13/2009 - Send AttackOrder to other player.
            sceneItemOwner.SendStartAttackOrderToMPPlayer(attackie);

            // Issue Attack Order for this Server side or SP game.
            // **
            sceneItemOwner.AttackOrder();

            // 7/16/2009 - Calculate new FacingDirection
            sceneItemOwner.CalculateDesiredAngle(attackie, out ((DefenseScene)sceneItemOwner).DesiredAngle);

        }

        /// <summary>
        /// EventHandler for when an attackie SceneItemOwner is destroyed.
        /// </summary>
        private void AttackSceneItemSceneItemDestroyed(object sender, EventArgs e)
        {
            // 11/6/2009
            try 
            {
                // 11/6/2009 - Check if 'Parent' is Null.
                if (Parent == null) return;

                // 11/6/2009 - cache 'SceneItemOwner', and check for 'Null'.
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
                if (Parent.ThisPlayer != null)
                    if (Parent.ThisPlayer.NetworkSession != null &&
                        // was - ImageNexusRTSGameEngine.Players[ImageNexusRTSGameEngine.ThisPlayer]
                        Parent.ThisPlayer.NetworkSession.IsHost)
                    {
                        sceneItemOwner.SendCeaseAttackOrderToMPPlayer();
                    }
            }
            catch (NullReferenceException err)
            {
                Debug.WriteLine("AttackSceneItemSceneItemDestroyed method threw NullRefExp.");
               
            }

#if DEBUG
            //System.Diagnostics.Debug.WriteLine("EventHandler triggered for AttackSceneItem_SceneItemDestroyed." + networkItemNumber.ToString());
#endif

        }
    }
}