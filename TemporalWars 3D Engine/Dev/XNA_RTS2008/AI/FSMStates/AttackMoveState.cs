#region File Description
//-----------------------------------------------------------------------------
// AttackMoveState.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.AI.Enums;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.Players;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWEngine.SceneItems.Enums;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.AI.FSMStates
{
    /// <summary>
    /// This namespace contains all the classes for the FSM States; for example <see cref="IdleState"/>
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    /// <summary>
    /// AttackMove state, used in the FSM_Machine class; the AttackMove state allows a 
    /// in-game unit to move to some user-goal with the intent to stop and attack any enemy
    /// units found along the way.
    /// </summary>
    class AttackMoveState : FSM_State
    {
        // 10/19/2009
        /// <summary>
        /// To use, store a value greater than 0, which is reduced each cycle.  This
        /// is to eliminate the constant checking and calling of the Player
        /// UnitMoveOrder, until it has had time to process the initial request!
        /// </summary>
        private int _checkForPathfinding;

        /// <summary>
        /// Creates the AttackMove FSM_State, setting the
        /// FSM_StateType enum to 'AttackMove'.
        /// </summary>
        /// <param name="parent">Reference to the FSM_AIControl parent instance</param>
        public AttackMoveState(FSM_AIControl parent)
            : base(parent)
        {
            Type = FSM_StateType.FSM_STATE_ATTACKMOVE;
            Parent = parent;
        }

        /// <summary>
        /// First called when the FSM_State is activated.  Any inital logic is placed here.
        /// </summary>
        public override void Enter()
        {
            return;
        }

        /// <summary>
        /// Is called last, as the FSM_State is exiting.  Any final cleanup logic is placed here.
        /// </summary>
        public override void Exit()
        {
            return;
        }

        // 3/4/2011 - Updated to include GameTime param.
        /// <summary>
        /// Core logic for this AttackMove state, which checks if unit
        /// has reached intended goal, or to continue the 'UnitMoveOrder' call
        /// to the Player class.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Execute(GameTime gameTime)
        {
            base.Execute(gameTime);

            // make sure still pathfinding to goalPosition!
            if (Parent.SceneItemOwner.ItemState == ItemStates.Resting && _checkForPathfinding == 0)
            {
                var attackMoveGoalPos = Parent.SceneItemOwner.AttackMoveGoalPosition;
                Player.UnitMoveOrder(Parent.SceneItemOwner, ref attackMoveGoalPos, true, 0, false);
                _checkForPathfinding = 20; // set for 20 iterations.
            }

            if (_checkForPathfinding > 0)
                _checkForPathfinding--;
        }

        // 6/8/2010 - Stores references to Neighbor collections.
        private SceneItemWithPick[] _neighborsGround;
        private SceneItemWithPick[] _neighborsAir;

        // 10/19/2009
        /// <summary>
        /// Checks if there are any items in the area for the units to attack.
        /// </summary>
        /// <returns>True/False</returns>
        private bool DoStartAttackCheck()
        {
           
            var itemGroupTypeToAttack = Parent.SceneItemOwner.ItemGroupTypeToAttack;
            var steeringBehaviors = Parent.SceneItemOwner.ForceBehaviors;
              
            if (steeringBehaviors == null) return false;

            switch (Parent.CurrentDefenseState)
            {
                case DefenseAIStance.Aggressive:
                    // Aggressive attack will also consider buildings/shields too.
                    if (((int)itemGroupTypeToAttack & (int)(ItemGroupType.Buildings | ItemGroupType.Shields | ItemGroupType.Vehicles)) != 0)
                    {
                        // 6/8/2010
                        steeringBehaviors.GetNeighborsGround(ref _neighborsGround);
                        if (CanAttackSomeNeighborItem(_neighborsGround, Parent.SceneItemOwner.ForceBehaviors.NeighborsGroundKeysCount)) return true;
                    }

                    if (((int)itemGroupTypeToAttack & (int)ItemGroupType.Airplanes) != 0)
                    {
                        // 6/8/2010
                        steeringBehaviors.GetNeighborsAir(ref _neighborsAir);
                        if (CanAttackSomeNeighborItem(_neighborsAir, Parent.SceneItemOwner.ForceBehaviors.NeighborsAirKeysCount))
                            return true;
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
                                return true;
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
                                return true;
                        }
                    }

                    break;
                case DefenseAIStance.HoldFire:
                    break;
                default:
                    break;
            }

            return false;
        }

        /// <summary>
        /// Checks current unit, and decides if transitioning to
        /// Attack state, Idle state, or stay in current state.
        /// </summary>
        /// <returns>Current FSM_StateType to transition to</returns>
        public override FSM_StateType CheckTransitions()
        {
            // Do Attack Check
            if (DoStartAttackCheck())
            {
                // if pathfinding, then stop.
                Parent.SceneItemOwner.AStarItemI.PathToQueue.Clear();
                if (Parent.SceneItemOwner.ItemState != ItemStates.Resting)
                    Parent.SceneItemOwner.ItemState = ItemStates.Resting;

                return FSM_StateType.FSM_STATE_ATTACK;
            }
       
            // Check if reached AttackMove GoalPosition, and return the IDLE state if true.
            var attackMoveGoalPos = Parent.SceneItemOwner.AttackMoveGoalPosition;
            if (Parent.SceneItemOwner.AStarItemI.SceneItemOwner.HasReachedGivenPosition(ref attackMoveGoalPos))
            {
                // 10/21/2009 - Check if there is another GoalPosition to move to in the Queue?
                if (Parent.SceneItemOwner.AttackMoveQueue.Count > 0)
                {
                    // yes, so dequeue new goalPosition.
                    Parent.SceneItemOwner.AttackMoveGoalPosition = Parent.SceneItemOwner.AttackMoveQueue.Dequeue();

                    return FSM_StateType.FSM_STATE_ATTACKMOVE;
                }

                // no, so stop AttackMove state and return to IDLE state.
                Parent.SceneItemOwner.AttackMoveOrderIssued = false;
                return FSM_StateType.FSM_STATE_IDLE;
            }

            // stay in AttackMove state.
            return FSM_StateType.FSM_STATE_ATTACKMOVE;
        }
       
    }
}