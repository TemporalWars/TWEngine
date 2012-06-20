#region File Description
//-----------------------------------------------------------------------------
// DefenseIdleState.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System.Diagnostics;
using Microsoft.Xna.Framework;
using ParallelTasksComponent.LocklessDictionary;
using TWEngine.AI.Enums;
using TWEngine.InstancedModels.Enums;
using TWEngine.SceneItems;
using TWEngine.SceneItems.Enums;

namespace TWEngine.AI.FSMStates
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefenseIdleState"/>.
    /// </summary>
    sealed class DefenseIdleState : IdleState
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">Instance of <see cref="FSM_AIControl"/></param>.
        public DefenseIdleState(FSM_AIControl parent)
            : base(parent)
        {
            Type = FSM_StateType.FSM_STATE_IDLE;
            Parent = parent;
        }
        /// <summary>
        /// First called when the FSM_State is activated.
        /// </summary>
        public override void Enter()
        {
            return;
        }
        /// <summary>
        /// Called last as the FSM_State is exiting.  Any final cleanup logic is placed here.
        /// </summary>
        public override void Exit()
        {
            return;
        }

        // 3/4/2011 - Updated to include GameTime param.
        /// <summary>
        /// The core logic for the FSM_State, which is called every cycle until the 'CheckTransitions'
        /// method forces the 'Exit' state.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Execute(GameTime gameTime)
        {
            base.Execute(gameTime);

            // 6/15/2010 - Updated to use new GetPlayer method.
            Players.Player player;
            TemporalWars3DEngine.GetPlayer(Parent.SceneItemOwner.PlayerNumber, out player);

            // Check if EnergyOff for the player this SceneItemOwner belongs to
            if (player != null && player.EnergyOff) return;

            // Update random movement, since in idleState.
            ((DefenseScene)Parent.SceneItemOwner).TurretRandomMovementCheck();

            // Update the Turrets facing direction.
            ((DefenseScene) Parent.SceneItemOwner).UpdateTurretFacingDirection();
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

            // 6/15/2010 - Updated to use new GetPlayer method.
            Players.Player player;
            TemporalWars3DEngine.GetPlayer(Parent.SceneItemOwner.PlayerNumber, out player);

            // Check if EnergyOff for the player this SceneItemOwner belongs to
            if (player != null && player.EnergyOff) return FSM_StateType.FSM_STATE_IDLE;

            // 7/31/2009 - If some 'nonAI_AttackRequest', then switch state to AttackState.
            if (aiOrderIssued == AIOrderType.NonAIAttackOrderRequest)
                return FSM_StateType.FSM_STATE_ATTACK;


            // 3/23/2009: Updated to check using bitwise comparison, since enum is a bitwise enum!            
            if (steeringBehaviors != null)
            {
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
            }            

            return FSM_StateType.FSM_STATE_IDLE;
        }
       
        
    }
}