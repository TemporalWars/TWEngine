#region File Description
//-----------------------------------------------------------------------------
// IdleState.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.AI.Enums;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWEngine.SceneItems.Enums;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.AI.FSMStates
{
    /// <summary>
    /// Idle state, used in the FSM_Machine class; in the Idle state, the 
    /// given unit checks for any attackies to attack, repairs for itself, or
    /// for any Attack move orders.
    /// </summary>
    class IdleState : FSM_State
    {
        /// <summary>
        /// Creates the Idle FSM_State, setting the
        /// FSM_StateType enum to 'Idle'.
        /// </summary>
        /// <param name="parent">Reference to the FSM_AIControl parent instance</param>
        public IdleState(FSM_AIControl parent)
            : base(parent)
        {
            Type = FSM_StateType.FSM_STATE_IDLE;
            Parent = parent;
        }

        /// <summary>
        /// First called when the FSM_State is activated; currently, just
        /// sets the 'AttackMoveOrderIssues' flag to false.
        /// </summary>
        public override void Enter()
        {
            // 10/19/2009 - Clear flag
            Parent.SceneItemOwner.AttackMoveOrderIssued = false;

            return;
        }

        // 3/4/2011 - Updated to include GameTime param.
        /// <summary>
        /// Abtract method to be inherited from, which is the core logic for the 
        /// FSM_State, which is called every cycle, until the 'CheckTransitions'
        /// method forces the 'Exit' state.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Execute(GameTime gameTime)
        {
            base.Execute(gameTime);
        }

        /// <summary>
        /// Abstract method to be interited from, which is called last, as the
        /// FSM_State is exiting.  Any final cleanup logic is placed here.
        /// </summary>
        public override void Exit()
        {
            return;
        }

        // 6/8/2010 - Stores references to Neighbor collections.
        private SceneItemWithPick[] _neighborsGround;
        private SceneItemWithPick[] _neighborsAir;

        /// <summary>
        /// Checks if there are any enemy attackies to pursue, or if
        /// it is time to return to base for a repair.
        /// </summary>
        /// <returns>Current FSM_StateType to transition to</returns>
        public override FSM_StateType CheckTransitions()
        {
            var aiOrderIssued = Parent.SceneItemOwner.AIOrderIssued;
            var itemGroupTypeToAttack = Parent.SceneItemOwner.ItemGroupTypeToAttack;
            var steeringBehaviors = Parent.SceneItemOwner.ForceBehaviors;

            // 10/19/2009 - Check if AttackMove issued.
            if (Parent.SceneItemOwner.AttackMoveOrderIssued)
                return FSM_StateType.FSM_STATE_ATTACKMOVE;

            // 10/21/2009: Updated to check the new 'DoRepair' flag.
            // 8/2/2009 - check if SceneItemOwner should repair itself
            if (Parent.SceneItemOwner.CurrentHealthPercent <= 0.40f ||
                Parent.SceneItemOwner.DoRepair)
            {
                // store current Position
                Parent.PreviousPositionPriorRepair = Parent.SceneItemOwner.Position;

                return FSM_StateType.FSM_STATE_REPAIR;
            }


            // 8/3/2009
            // If some 'nonAI_AttackRequest', then skip issuing attackOrder, since already done via 
            // HandleGameInput class to the Player class.
            if (aiOrderIssued == AIOrderType.NonAIAttackOrderRequest)
                return FSM_StateType.FSM_STATE_IDLE;


            // 3/23/2009: Updated to check using bitwise comparison, since enum is a bitwise enum!            
            if (steeringBehaviors != null)
            {
                switch (Parent.CurrentDefenseState)
                {
                    case DefenseAIStance.Aggressive:
                        // Aggressive attack will also consider buildings/shields too.
                        if (((int)itemGroupTypeToAttack & (int)(ItemGroupType.Buildings | ItemGroupType.Shields | ItemGroupType.Vehicles)) != 0)
                        {
                            // 6/8/2010
                            steeringBehaviors.GetNeighborsGround(ref _neighborsGround);
                            if (CanAttackSomeNeighborItem(_neighborsGround, Parent.SceneItemOwner.ForceBehaviors.NeighborsGroundKeysCount))
                                return FSM_StateType.FSM_STATE_ATTACK;
                        }

                        if (((int)itemGroupTypeToAttack & (int)ItemGroupType.Airplanes) != 0)
                        {
                            // 6/8/2010
                            steeringBehaviors.GetNeighborsAir(ref _neighborsAir);
                            if (CanAttackSomeNeighborItem(_neighborsAir, Parent.SceneItemOwner.ForceBehaviors.NeighborsAirKeysCount))
                                return FSM_StateType.FSM_STATE_ATTACK;
                        }

                        break;
                    case DefenseAIStance.Guard:
                    case DefenseAIStance.HoldGround:
                        // Guard and HoldGround only attack vehicles/aircraft.
                        if (((int)itemGroupTypeToAttack & (int)(ItemGroupType.Vehicles)) != 0)
                        {
                            // 6/8/2010
                            steeringBehaviors.GetNeighborsGround(ref _neighborsGround);
                            if (CanAttackSomeNeighborItem(_neighborsGround, Parent.SceneItemOwner.ForceBehaviors.NeighborsGroundKeysCount))
                            {
                                // check if within attack range.
                                if (Parent.SceneItemOwner.IsAttackieInAttackRadius(Parent.SceneItemOwner.AttackSceneItem))
                                    return FSM_StateType.FSM_STATE_ATTACK;
                            }
                        }

                        if (((int)itemGroupTypeToAttack & (int)ItemGroupType.Airplanes) != 0)
                        {
                            // 6/8/2010
                            steeringBehaviors.GetNeighborsAir(ref _neighborsAir);
                            if (CanAttackSomeNeighborItem(_neighborsAir, Parent.SceneItemOwner.ForceBehaviors.NeighborsAirKeysCount))
                            {
                                // check if within attack range.
                                if (Parent.SceneItemOwner.IsAttackieInAttackRadius(Parent.SceneItemOwner.AttackSceneItem))
                                    return FSM_StateType.FSM_STATE_ATTACK;
                            }
                        }

                        break;
                    case DefenseAIStance.HoldFire:
                        break;
                    default:
                        break;
                }
            }            

            return FSM_StateType.FSM_STATE_IDLE;
        }
      
    }
}