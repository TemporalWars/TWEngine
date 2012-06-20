#region File Description
//-----------------------------------------------------------------------------
// DefenseTurretBehavior.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TWEngine.Common.Extensions;
using TWEngine.ForceBehaviors.SteeringBehaviors;
using TWEngine.ForceBehaviors.Structs;
using TWEngine.InstancedModels.Enums;
using TWEngine.Interfaces;
using TWEngine.Players;
using TWEngine.rtsCommands.Enums;
using TWEngine.SceneItems;
using TWEngine.rtsCommands;
using TWEngine.Networking;
using TWEngine.MemoryPool;

namespace TWEngine.ForceBehaviors.TurretBehaviors
{
    // 1/16/2011
    // NOTE: DefenseScene class no longer uses this class; instead, the 'DefenseIdleState' and 'DefenseAttackState'
    // NOTE: are used.  However, this class remains since it can be applied by scripting levels to some other asset
    // NOTE: via the properties tool window.
    ///<summary>
    /// The <see cref="DefenseTurretBehavior"/> class is used to provide defense shooting capabilities to some
    /// <see cref="DefenseScene"/>, which has a mounted gun turret.
    ///</summary>
    public sealed class DefenseTurretBehavior : AbstractBehavior
    {        
        // Direction Offset Adjustment in Radians
        private float _facingDirectionOffset;
       
        private float _realTurretDirection;

        // 1/8/2009 - Enable Attacking? - If not, then AbstractBehavior can be used just for random movement of turret.
        private bool _enableAttacking = true;       
        // 2/27/2009 - Queue of Items to attack; needed, since the MP games can be slightly out of sync!  In other words,
        //             the Client could be attacking an SceneItemOwner, which is almost dead, but the Server is already attacking the
        //             next SceneItemOwner.  When the server sent the attack command, the client lost it, since it was still busy
        //             attacking the last SceneItemOwner.  This Queue guarantees the client will complete all Attack Commands sent to it.
        private readonly Queue<SceneItem> _itemsToAttack = new Queue<SceneItem>();

        private const int MaxSeconds = 10;
        private TimeSpan _timeToRandomTurretMove = new TimeSpan(0, 0, MaxSeconds); 
        private Random _rndGenerator = new Random();
        private float _desiredAngle;

        // 1/30/2009 - Stores value of currentAngle/DesiredAngle from TurnToFace method.
        private float _angleDifference;
        private static readonly Vector3 Vector3Zero = Vector3.Zero;

        #region Properties

        
        // 1/8/2009 
        /// <summary>
        /// Allows turning off the Attacking AbstractBehavior; this can be useful if you want to attach
        /// the AbstractBehavior to other items, like a WindMill, to be able to use the random turning
        /// AbstractBehavior!
        /// </summary>
        public bool EnableAttacking
        {
            get { return _enableAttacking; }
            set { _enableAttacking = value; }
        }

        // 2/27/2009
        /// <summary>
        /// Is attacking some <see cref="SceneItem"/>.
        /// </summary>
        public bool IsAttacking { get; set; }

        #endregion

        ///<summary>
        /// Constructor
        ///</summary>
        public DefenseTurretBehavior()
            : base((int)Enums.BehaviorsEnum.DefenseTurretBehavior, 0.0f)
        {
            
        }

        // 6/8/2010 - Stores references to Neighbor collections.
        private SceneItemWithPick[] _neighborsGround;
        private SceneItemWithPick[] _neighborsAir;

        /// <summary>
        /// Starts to turn the SceneItemOwner's Turret using Position of itself 
        /// and faceThis as direction to turn to.
        /// </summary>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> as elapsed game time.</param>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        /// <param name="force">(OUT) calculated force as <see cref="Vector3"/></param>
        public override void Update(ref BehaviorsTimeSpan elapsedTime, SceneItem item, out Vector3 force)
        {
            // 5/20/2012 - Throw expection if not SceneItemWithPick.
            var sceneItemWithPick = (SceneItemWithPick)item;
            if (sceneItemWithPick == null)
            {
                throw new InvalidOperationException("DefenseTurretAbstractBehavior can ONLY be used with SceneItemWithPick types.");
            }

            force = Vector3Zero;
            
            // reduce Time
            //_timeToRandomTurretMove -= elapsedTime; 
            _timeToRandomTurretMove = _timeToRandomTurretMove.Subtract(elapsedTime); // 6/12/2010 - New Extension method.

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(item.PlayerNumber, out player);

            // 1/30/2009 - Check if EnergyOff for the player this SceneItemOwner belongs to
            if (player != null && player.EnergyOff)
            {
                sceneItemWithPick.AttackOn = false;
                return;
            }

            // 1/8/2008 - Check if Attacking is disabled for this AbstractBehavior.
            if (!_enableAttacking)
            {
                // *****
                // Then just calculate a random movement
                // *****
                // If Time elapsed, then get new angle
                if (_timeToRandomTurretMove.Milliseconds <= 0.0f)
                {
                    // Get new angle to move defense turret to                
                    _desiredAngle = MathHelper.ToRadians(_rndGenerator.Next(-180, 180));
                    // Reset timer
                    _timeToRandomTurretMove = TimeSpan.FromSeconds(MaxSeconds);
                } // End if Time Expired
            }
            else
            {
                //  // is Network Game?
                if (ForceBehaviorManager.NetworkSession == null)
                {
                    // SP game

                    // If Attack Ground units.
                    if (sceneItemWithPick.ItemGroupTypeToAttack == ItemGroupType.Vehicles)
                    {
                        // 6/8/2010
                        ForceBehaviorManager.GetNeighborsGround(ref _neighborsGround);
                        UpdateSp(this, item, _neighborsGround, ForceBehaviorManager.NeighborsGroundKeysCount);
                    }

                    // If Attack Air units.
                    if (sceneItemWithPick.ItemGroupTypeToAttack == ItemGroupType.Airplanes)
                    {
                        // 6/8/2010
                        ForceBehaviorManager.GetNeighborsAir(ref _neighborsAir);
                        UpdateSp(this, item, _neighborsAir, ForceBehaviorManager.NeighborsAirKeysCount);
                    }


                }
                else // Yes, Network Game
                {
                    // Is Host?
                    if (ForceBehaviorManager.NetworkSession.IsHost)
                    {
                        // If Attack Ground units.
                        if (sceneItemWithPick.ItemGroupTypeToAttack == ItemGroupType.Vehicles)
                        {
                            // 6/8/2010
                            ForceBehaviorManager.GetNeighborsGround(ref _neighborsGround);
                            Update_Host(this, item, _neighborsGround, ForceBehaviorManager.NeighborsGroundKeysCount);
                        }

                        // If Attack Air units.
                        if (sceneItemWithPick.ItemGroupTypeToAttack == ItemGroupType.Airplanes)
                        {
                            // 6/8/2010
                            ForceBehaviorManager.GetNeighborsAir(ref _neighborsAir);
                            Update_Host(this, item, _neighborsAir, ForceBehaviorManager.NeighborsAirKeysCount);
                        }

                    }
                    else // Client
                    {
                        Update_Client(this, item);
                    }
                } // End Is MP Game.

            } // End If _enableAttacking.

            // 8/12/2009 - Cache
            var itemTurretAtts = (item as ITurretAttributes);

            // Make sure Class has proper interface
            if (itemTurretAtts == null) return;

            _facingDirectionOffset = sceneItemWithPick.FacingDirectionOffset;
            
            // Calculate the RealTurretDirection, taking into account also the Tank rotation.
            _realTurretDirection = itemTurretAtts.TurretFacingDirection + sceneItemWithPick.FacingDirection;               

            // Call TurnToFace AbstractBehavior
            itemTurretAtts.TurretFacingDirection = TurnToFace(_desiredAngle, _realTurretDirection, itemTurretAtts.TurretTurnSpeed);

            itemTurretAtts.TurretDesiredAngle = _desiredAngle;               

            // Take back out the tank rotation adjustment, because we don't want to distort
            // the actual value for the turretFacingDirection.
            itemTurretAtts.TurretFacingDirection -= sceneItemWithPick.FacingDirection;
        }
       
        // 6/8/2009: Updated to use ReadOnlyCollection.
        // 12/30/2008; 2/27/2009 - Updated to use the 'IsAttacking' flag & connect the eventHandler.
        private static void UpdateSp(DefenseTurretBehavior defenseTurretBehavior, SceneItem item, SceneItem[] neighbors, int keysCount)
        {
            // 5/20/2012
            var sceneItemWithPick = (SceneItemWithPick)item;

            // 1st - Check if GetNeighbors found any units within firing range
            if (!defenseTurretBehavior.IsAttacking) // Only attack 1 SceneItemOwner at a Time.
            {
                var sThisPlayer = TemporalWars3DEngine.SThisPlayer; // 5/16/2010
                if (keysCount > 0)
                {
                    // Iterate through neighbors list
                    for (var i = 0; i < keysCount; i++)
                    {
                        var neighbor = neighbors[i];
                        if (neighbor == null) continue; // 5/16/2010

                        var instancedItem = (neighbor.ShapeItem as IInstancedItem);

                        // Is it an enemy unit?
                        if (neighbor.PlayerNumber == sThisPlayer || instancedItem == null)
                            continue;

                        // Is it the itemGroupType this defense can shoot at?
                        if (sceneItemWithPick.ItemGroupTypeToAttack != instancedItem.ItemGroupType)
                            continue;

                        // Then set defense to shoot at this SceneItemOwner.
                        item.AttackSceneItem = neighbor;                               

                        // 2/27/2009 - Connect EventHandler for destroyed event.
                        //SceneItemOwner.AttackSceneItem.sceneItemDestroyed += new EventHandler(AttackSceneItemSceneItemDestroyed);
                        item.AssignEventHandler_SceneItemDestroyed(defenseTurretBehavior.AttackSceneItemSceneItemDestroyed, item);

                        defenseTurretBehavior.IsAttacking = true;
                        break;
                    } // End Loop Neighbors list
                } // End If Neighbors not empty
            } // End If itemToAttack Null

            // 7/16/2009 - Copy 'AttackSceneItem' into local cache, to eliminate Thread Sync errors!
            var attackie = item.AttackSceneItem;
           

            // 2nd - if no SceneItemOwner to attack, then get a random angle for the turret to move to; otherwise,
            //       calculate angle based on attackie's Position.
            if (!defenseTurretBehavior.IsAttacking || attackie == null || !attackie.IsAlive)
            {
                defenseTurretBehavior.IsAttacking = false;
                item.AttackSceneItem = null;

                // If Time elapsed, then get new angle
                if (defenseTurretBehavior._timeToRandomTurretMove.Milliseconds <= 0.0f)
                {
                    // Get new angle to move defense turret to                
                    defenseTurretBehavior._desiredAngle = MathHelper.ToRadians(defenseTurretBehavior._rndGenerator.Next(-180, 180));
                    // Reset timer
                    defenseTurretBehavior._timeToRandomTurretMove = TimeSpan.FromSeconds(5);
                } // End if Time Expired
            } // End if itemToAttack Null
            else
            {               
                // 7/16/2009 - Make sure within attack range.
                float distance;
                sceneItemWithPick.CalculateDistanceToSceneItem(attackie, out distance);

                if (distance < item.AttackRadius) // We are in Attack Radius!
                {
                    // 7/16/2009 - calc turn Position based on attackie's location.
                    sceneItemWithPick.CalculateDesiredAngle(item.AttackSceneItem, out defenseTurretBehavior._desiredAngle);
                    
                    // Issue Attack Order for the Defense Turret                    
                    sceneItemWithPick.AttackOrder();
                    return;
                }

                item.AttackSceneItem = null;
                sceneItemWithPick.AttackOn = false;
                defenseTurretBehavior.IsAttacking = false; // 2/27/2009
            }
        }

        // 6/8/2009: Updated to use ReadOnlyCollection.
        // 12/30/2008;  2/27/2009 - Updated to use the 'IsAttacking' flag & connect the eventHandler.
        private static void Update_Host(DefenseTurretBehavior defenseTurretBehavior, SceneItem item, SceneItem[] neighbors, int keysCount)
        {
            // 5/20/2012
            var sceneItemWithPick = (SceneItemWithPick)item;

            // 1st - Check if GetNeighbors found any units within firing range
            if (!defenseTurretBehavior.IsAttacking) // Only attack 1 SceneItemOwner at a Time.
            {
                if (keysCount > 0)
                {
                    // Iterate through neighbors list
                    for (var i = 0; i < keysCount; i++)
                    {
                        var neighbor = neighbors[i];
                        if (neighbor == null) continue; // 5/16/2010

                        var instancedItem = (neighbor.ShapeItem as IInstancedItem);

                        // Is it an enemy unit?
                        if (neighbor.PlayerNumber == item.PlayerNumber || instancedItem == null) continue;

                        // Is it the itemGroupType this defense can shoot at?
                        if (sceneItemWithPick.ItemGroupTypeToAttack != instancedItem.ItemGroupType)
                            continue;

                        // Create StartAttack RTSCommand for Client and Add to Queue
                        RTSCommStartAttackSceneItem startAttackCommand;
                        PoolManager.GetNode(out startAttackCommand);

                        startAttackCommand.Clear();
                        startAttackCommand.NetworkCommand = NetworkCommands.StartAttackSceneItem;
                        startAttackCommand.SceneItemAttackerNetworkNumber = sceneItemWithPick.NetworkItemNumber;
                        startAttackCommand.SceneItemAttackerPlayerNumber = item.PlayerNumber;
                        startAttackCommand.SceneItemAttackieNetworkNumber = ((SceneItemWithPick)neighbor).NetworkItemNumber;
                        startAttackCommand.SceneItemAttackiePlayerNumber = neighbor.PlayerNumber;
                        startAttackCommand.AIOrderIssued = sceneItemWithPick.AIOrderIssued; // 6/3/2009

                        // Add to Queue to send to Client
                        NetworkGameComponent.AddCommandsForClientG(startAttackCommand);

                        // Then set defense to shoot at this SceneItemOwner.
                        item.AttackSceneItem = neighbor;

                        // 2/27/2009 - Connect EventHandler for destroyed event.
                        //SceneItemOwner.AttackSceneItem.sceneItemDestroyed += new EventHandler(AttackSceneItemSceneItemDestroyed);
                        item.AssignEventHandler_SceneItemDestroyed(defenseTurretBehavior.AttackSceneItemSceneItemDestroyed, item); // 6/15/2009
                        defenseTurretBehavior.IsAttacking = true;

                        break;
                    } // End Loop Neighbors list
                } // End If Neighbors not empty
            } // End If itemToAttack Null

            // 7/16/2009 - Copy 'AttackSceneItem' into local cache, to eliminate Thread Sync errors!
            var attackie = item.AttackSceneItem;

            // 2nd - if no SceneItemOwner to attack, then get a random angle for the turret to move to; otherwise,
            //       calculate angle based on attackie's Position.
            if (!defenseTurretBehavior.IsAttacking || attackie == null || !attackie.IsAlive)
            {
                defenseTurretBehavior.IsAttacking = false;
                item.AttackSceneItem = null;

                // If Time elapsed, then get new angle
                if (defenseTurretBehavior._timeToRandomTurretMove.Milliseconds <= 0.0f)
                {
                    // Get new angle to move defense turret to                
                    defenseTurretBehavior._desiredAngle = MathHelper.ToRadians(defenseTurretBehavior._rndGenerator.Next(-180, 180));
                    // Reset timer
                    defenseTurretBehavior._timeToRandomTurretMove = TimeSpan.FromSeconds(5);
                } // End if Time Expired
            } // End if itemToAttack Null
            else
            {
                // 7/16/2009 - Make sure within attack range.
                float distance;
                sceneItemWithPick.CalculateDistanceToSceneItem(attackie, out distance);

                if (distance < item.AttackRadius) // We are in Attack Radius!
                {                    
                    // 7/16/2009 - calc turn Position based on attackie's location.
                    sceneItemWithPick.CalculateDesiredAngle(item.AttackSceneItem, out defenseTurretBehavior._desiredAngle);

                    // Issue Attack Order for the Defense Turret                    
                    sceneItemWithPick.AttackOrder();
                    return;
                }

                item.AttackSceneItem = null;
                sceneItemWithPick.AttackOn = false;
                defenseTurretBehavior.IsAttacking = false; // 2/27/2009
            }
        }
       
        // 12/30/2008
        private static void Update_Client(DefenseTurretBehavior defenseTurretBehavior, SceneItem item)
        {
            // 5/20/2012
            var sceneItemWithPick = (SceneItemWithPick)item;

            // 7/16/2009 - Copy 'AttackSceneItem' into local cache, to eliminate Thread Sync errors!
            var attackie = item.AttackSceneItem;
           
            //     - if no SceneItemOwner to attack, then get a random angle for the turret to move to; otherwise,
            //       calculate angle based on attackie's Position.
            if (!defenseTurretBehavior.IsAttacking || attackie == null || !attackie.IsAlive) // was !SceneItemOwner.Attackon
            {
                // 2/27/2009 - check Queue to see if any other items to attack
                if (defenseTurretBehavior._itemsToAttack.Count > 0)
                {
                    // dequeue SceneItemOwner, and see if it is valid to attack
                    var itemToAttack = defenseTurretBehavior._itemsToAttack.Dequeue();

                    // is alive?
                    if (itemToAttack.IsAlive)
                    {
                        item.AttackSceneItem = itemToAttack;
                        sceneItemWithPick.AttackOrder();
                        return;
                    }                    
                }

                // If Time elapsed, then get new angle
                if (defenseTurretBehavior._timeToRandomTurretMove.Milliseconds <= 0.0f)
                {
                    // Get new angle to move defense turret to                
                    defenseTurretBehavior._desiredAngle = MathHelper.ToRadians(defenseTurretBehavior._rndGenerator.Next(-180, 180));
                    // Reset timer
                    defenseTurretBehavior._timeToRandomTurretMove = TimeSpan.FromSeconds(5);
                } // End if Time Expired
            } 
            else
            {                
                // 7/16/2009                
                sceneItemWithPick.CalculateDesiredAngle(attackie, out defenseTurretBehavior._desiredAngle);

            }// End if ClientDoAttack
        }      


        // 2/27/2009
        /// <summary>
        /// <see cref="EventHandler"/> for when an attackie <see cref="SceneItem"/> is destroyed.
        /// </summary>
        void AttackSceneItemSceneItemDestroyed(object sender, EventArgs e)
        {
            // Stop this turret from attacking current SceneItemOwner.
            IsAttacking = false;

        }

        /// <summary>
        /// Calculates the angle that an object should face, given its position, its
        /// target's position, its current angle, and its maximum turning speed.
        /// </summary>        
        private float TurnToFace(float desiredAngle, float currentAngle, float turnSpeed)
        {
            // Ben - Added a Constant Shift in Degrees to the DesiredAngle formula
            //       to compensate for where the front of the SceneItemOwner Shape Image is.
            desiredAngle += _facingDirectionOffset;


            // so now we know where we WANT to be facing, and where we ARE facing...
            // if we weren't constrained by turnSpeed, this would be easy: we'd just 
            // return DesiredAngle.
            // instead, we have to calculate how much we WANT to turn, and then make
            // sure that's not more than turnSpeed.

            // first, figure out how much we want to turn, using WrapAngle to get our
            // result from -Pi to Pi ( -180 degrees to 180 degrees )
            _angleDifference = desiredAngle - currentAngle;
            var difference = MathHelper.WrapAngle(_angleDifference);

            // clamp that between -turnSpeed and turnSpeed.
            difference = MathHelper.Clamp(difference, -turnSpeed, turnSpeed);

            // so, the closest we can get to our target is currentAngle + difference.
            // return that, using WrapAngle again.
            var currentAngleDiff = currentAngle + difference;
            var newDirection = MathHelper.WrapAngle(currentAngleDiff);

            return newDirection;
        }

        // 11/14/2008
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            // Null Refs
            _rndGenerator = null;

            base.Dispose();
        }
    }
}
