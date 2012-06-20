#region File Description
//-----------------------------------------------------------------------------
// FSM_State.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System.Diagnostics;
using Microsoft.Xna.Framework;
using TWEngine.AI.Enums;
using TWEngine.Interfaces;
using TWEngine.SceneItems;

namespace TWEngine.AI
{
   
// ReSharper disable InconsistentNaming
    abstract class FSM_State
// ReSharper restore InconsistentNaming

    {
        // 3/4/2011 - GameTime instance
        protected GameTime GameTimeInstance;

        // 3/4/2011 - Optimization timers, used to reduce the # of times a method is called.
        private const float LocateSomeEnemyCheckTimeReset = 2000f;
        private float _locateSomeEnemyCheckTime = LocateSomeEnemyCheckTimeReset;

        // Constructor
        /// <summary>
        /// Constructor for the <see cref="FSM_State"/>.
        /// </summary>
        /// <param name="aiControl">Reference to the FSM_AIControl parent instance</param>
        protected FSM_State(FSM_AIControl aiControl)
        {
            Parent = aiControl;
            Type = FSM_StateType.FSM_STATE_NONE;
        }     

        /// <summary>
        /// Abstract method to be inherited from, which is first called when
        /// the FSM_State is activated.  Any inital logic is placed here.
        /// </summary>
        public abstract void Enter();

        // 3/4/2011 - Updated to include GameTime param.
        /// <summary>
        /// Abstract method to be inherited from, which is the core logic for the 
        /// FSM_State, which is called every cycle, until the 'CheckTransitions'
        /// method forces the 'Exit' state.
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Execute(GameTime gameTime)
        {
            GameTimeInstance = gameTime;
        }

        /// <summary>
        /// Abstract method to be interited from, which is called last as the
        /// FSM_State is exiting.  Any final cleanup logic is placed here.
        /// </summary>
        public abstract void Exit();

        /// <summary>
        /// Checks the FSM_States transitions; specifically used to transition
        /// from one state to another.
        /// </summary>
        /// <returns>Current FSM_StateType to transition to</returns>
        public abstract FSM_StateType CheckTransitions();

        /// <summary>
        /// Reference to the FSM_AIControl parent instance
        /// </summary>
        public FSM_AIControl Parent;

        /// <summary>
        /// The FSM StateType enumeration, refering to use
        /// of this FSM_State instance; for example, the 'AttackState' type.
        /// </summary>
        public FSM_StateType Type;

        

        /// <summary>
        /// Iterates the Neighbors list given, and attacks the first SceneItemOwner within attacking range.
        /// </summary>
        /// <param name="neighbors">'Read-Only-Collection' of the current neighbors</param>
        /// <param name="keysCount">Current count of neighbors to iterate in collection.</param>
        /// <returns>True/False of result.</returns>
        protected bool CanAttackSomeNeighborItem(SceneItemWithPick[] neighbors, int keysCount)
        {            
            SceneItemWithPick attackieLocated;
            if (LocateSomeEnemyNeighborToAttack(neighbors, out attackieLocated, keysCount))
            {
                // 7/31/2009 - set attackie
                Parent.SceneItemOwner.AttackSceneItem = attackieLocated;
                
                return true;
            }

            return false;
        }

        /// <summary>
        /// Iterates the Neighbors list in search of some neighbor to attack.
        /// </summary>
        /// <param name="neighbors">'Read-Only-Collection' of the current neighbors</param>
        /// <param name="attackieToAttack">(OUT) New attackie to attack</param>
        /// <param name="keysCount">countkeysCount</param>
        /// <returns>True/False of finding a new attackie to attack.</returns>
        protected bool LocateSomeEnemyNeighborToAttack(SceneItemWithPick[] neighbors, out SceneItemWithPick attackieToAttack, int keysCount)
        {
            attackieToAttack = null;

            // 3/4/2011
            if (GameTimeInstance == null) return false;

            // 3/4/2011
            // Do check every few seconds
            _locateSomeEnemyCheckTime -= (float)GameTimeInstance.ElapsedGameTime.TotalMilliseconds;
            if (_locateSomeEnemyCheckTime >= 0) return false;
            _locateSomeEnemyCheckTime = LocateSomeEnemyCheckTimeReset;

            // get neighbors array from steeringbehaviors
            if (keysCount > 0)
            {
                var itemGroupTypeToAttack = Parent.SceneItemOwner.ItemGroupTypeToAttack;

                // 6/8/2009 - Tracks distance to attackies.
                var lastDistance = float.MaxValue;
                
                // Iterate through neighbors list
                for (var i = 0; i < keysCount; i++)
                {
                    // 6/8/2010
                    var neighbor = neighbors[i];
                    if (neighbor == null) continue;

                    // Is SceneItemOwner alive? 
                    if (!neighbor.IsAlive) continue;

                    // Is it an enemy unit?
                    if (neighbor.PlayerNumber == Parent.SceneItemOwner.PlayerNumber ||
                        (neighbor.ShapeItem == null)) continue;

                    // 3/23/2009: Updated to check using bitwise comparison, since enum is a bitwise enum!
                    // Is it the itemGroupType this defense can shoot at?
                    //if (ItemGroupTypeToAttack == (neighbors[i].ShapeItem as IInstancedItem).ItemGroupType)
                    if (((int) (neighbor.ShapeItem as IInstancedItem).ItemGroupType & (int) itemGroupTypeToAttack) ==
                        0)
                        continue;

                    // 6/8/2009 - Calculate distance to attackie, and store if smaller than last one found.
                    float distance;
                    Parent.SceneItemOwner.CalculateDistanceToSceneItem(neighbor, out distance);
                    if (distance >= lastDistance) continue;

                    lastDistance = distance;
                    attackieToAttack = neighbor;

                } // End Loop Neighbors list
            } // End If Neighbors not empty        
            

            return (attackieToAttack == null) ? false : true;
        }
    }
}