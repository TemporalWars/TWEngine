#region File Description
//-----------------------------------------------------------------------------
// TurnTurretBehavior.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using TWEngine.ForceBehaviors.SteeringBehaviors;
using TWEngine.ForceBehaviors.Structs;
using TWEngine.Utilities;
using TWEngine.Interfaces;
using TWEngine.SceneItems;

namespace TWEngine.ForceBehaviors.TurretBehaviors
{
    ///<summary>
    /// The <see cref="TurnTurretBehavior"/> is used to turn some gun turret the proper direction, depending on its target.
    ///</summary>
    public sealed class TurnTurretBehavior : AbstractBehavior
    {    
        // 3/24/2009 - ParkTurret
        ///<summary>
        /// Set to have turret park itself.
        ///</summary>
        public bool ParkTurret;

        /// <summary>
        /// Current <see cref="Vector2"/> position to face towards.
        /// </summary>
        private Vector2 _faceThis = Vector2.Zero;

        // 6/3/2009
        private readonly System.Diagnostics.Stopwatch _timeToRandomTurretMoveStopWatch = new System.Diagnostics.Stopwatch();
        private readonly float _maxTimeToMove = MathUtils.RandomBetween(4000, 6000); // between 4-6 seconds.
        private bool _stopWatchStarted;
        private readonly Random _rndGenerator = new Random();
        
        private TurnToFaceBehavior _turnToFaceAbstractBehavior;
        
        private float _realTurretDirection, _turretDesiredAngle;
        private static readonly Vector3 Vector3Zero = Vector3.Zero; // 8/11/2009

        private float _angleDifference;
        private static readonly float OneRadian = MathHelper.ToRadians(1.0f);

        /// <summary>
        /// Stores value of currentAngle/DesiredAngle from TurnToFace method.
        /// </summary>
        public float AngleDifference
        {
            get { return _angleDifference; }
            set { _angleDifference = value; }
        }

        ///<summary>
        /// Constructor, which creates an instance of <see cref="TurnToFaceBehavior"/>.
        ///</summary>
        public TurnTurretBehavior()
            : base((int)Enums.BehaviorsEnum.TurnTurret, 0.0f)
        {
            _turnToFaceAbstractBehavior = new TurnToFaceBehavior();
        }

        /// <summary>
        /// Starts to turn the <see cref="SceneItem"/> turret using position of itself 
        /// and internal <see cref="_faceThis"/> value as direction to turn to.
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
                throw new InvalidOperationException("TurnTurretAbstractBehavior can ONLY be used with SceneItemWithPick types.");
            }

            // 5/16/2010 - refactored core code to new STATIC method.
            DoUpdate(this, sceneItemWithPick, ref elapsedTime, out force);
        }

        // 5/16/2010; 6/12/2010: Updated to BehaviorsTimeSpan.
        /// <summary>
        /// Helper method, for the Update method.
        /// </summary>
        /// <param name="turnTurretBehavior">Instance of <see cref="TurnTurretBehavior"/>.</param>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> as elapsed game time.</param>
        /// <param name="force">(OUT) calculated force as <see cref="Vector3"/></param>
        private static void DoUpdate(TurnTurretBehavior turnTurretBehavior, SceneItemWithPick item, 
                                    ref BehaviorsTimeSpan elapsedTime, out Vector3 force)
        {
            force = Vector3Zero;

            // 6/3/2009
            if (!turnTurretBehavior._stopWatchStarted)
            {
                turnTurretBehavior._timeToRandomTurretMoveStopWatch.Start();
                turnTurretBehavior._stopWatchStarted = true;
            }

            if (item == null) return;

            // Make sure Class has proper interface
            var itemTurretAtts = (item as ITurretAttributes); // 8/12/2009
            if (itemTurretAtts == null) return;

            var tmpPosition = new Vector2 { X = item.Position.X, Y = item.Position.Z };


            // 3/24/2009 - If not Park Turret, then do normal calculations.
            if (!turnTurretBehavior.ParkTurret)
            {
                try
                {
                    // 7/7/2009 - Get 'AttackSceneItem' value, and store locally.
                    var attackSceneItem = item.AttackSceneItem;

                    // 6/1/2009 - If no AttackItem, then do random movement.
                    if (attackSceneItem == null)
                    {
                        // If Time elapsed, then get new angle
                        if (turnTurretBehavior._timeToRandomTurretMoveStopWatch.ElapsedMilliseconds >= turnTurretBehavior._maxTimeToMove)
                        {
                            // Get new angle to move defense turret to 
                            float desiredAngle = turnTurretBehavior._rndGenerator.Next(-180, 180);
                            // 8/23/09 - Convert angle to radians for Cos/Sin function below.
                            desiredAngle = MathHelper.ToRadians(desiredAngle);
                            

                            // Get direction of angle      
                            var direction = new Vector2
                                                {
                                                    X = (float) Math.Cos(desiredAngle),
                                                    Y = (float) Math.Sin(desiredAngle)
                                                };

                            // Create FaceThis pos, by taking direction * 500 from current Position.
                            Vector2.Multiply(ref direction, 500, out direction);
                            Vector2.Add(ref tmpPosition, ref direction, out turnTurretBehavior._faceThis);

                            // Reset timer                                                       
                            turnTurretBehavior._timeToRandomTurretMoveStopWatch.Reset();
                            turnTurretBehavior._timeToRandomTurretMoveStopWatch.Start();

                        } // End if Time Expired
                    }
                    else
                    {
                        turnTurretBehavior._faceThis.X = attackSceneItem.Position.X;
                        turnTurretBehavior._faceThis.Y = attackSceneItem.Position.Z;

                    }
                }
                catch (NullReferenceException)
                {
                    System.Diagnostics.Debug.WriteLine("Method Error: Update threw Null in TurnTurrretBehavior.");
                }
            }
            else
            {
                tmpPosition = new Vector2 { X = item.Position.X, Y = item.Position.Z };

                // using Heading, make new _faceThis Position.
                var heading = item.AStarItemI.Heading;
                var tmpForwardDir = new Vector2 {X = heading.X, Y = heading.Z};
               

                Vector2 tmpNewPositionAdj;
                Vector2 tmpNewPosition;
                Vector2.Multiply(ref tmpForwardDir, 100, out tmpNewPositionAdj);
                Vector2.Add(ref tmpPosition, ref tmpNewPositionAdj, out tmpNewPosition);

                turnTurretBehavior._faceThis.X = tmpNewPosition.X;
                turnTurretBehavior._faceThis.Y = tmpNewPosition.Y;

            }


            turnTurretBehavior._turnToFaceAbstractBehavior.FacingDirectionOffset = item.FacingDirectionOffset;

            // 10/2/2008 - Calculate the RealTurretDirection, taking into account also the Tank rotation.
            turnTurretBehavior._realTurretDirection = MathHelper.WrapAngle(itemTurretAtts.TurretFacingDirection + item.FacingDirection);

            // 8/10/2009 - Multiple TurretTurnSpeed, by the game elapsedTime.
            var turretAttributesTurretTurnSpeed = itemTurretAtts.TurretTurnSpeed * (float)elapsedTime.TotalSeconds; 

            // Call TurnToFace AbstractBehavior
            itemTurretAtts.TurretFacingDirection = SceneItemWithPick.TurnToFace(ref tmpPosition,
                                                                                ref turnTurretBehavior._faceThis,
                                                                                turnTurretBehavior._realTurretDirection,
                                                                                turretAttributesTurretTurnSpeed,
                                                                                turnTurretBehavior._turnToFaceAbstractBehavior.
                                                                                    FacingDirectionOffset,
                                                                                out turnTurretBehavior._turretDesiredAngle,
                                                                                out turnTurretBehavior._angleDifference);

            itemTurretAtts.TurretDesiredAngle = turnTurretBehavior._turretDesiredAngle;


            turnTurretBehavior.AngleDifference = turnTurretBehavior._turnToFaceAbstractBehavior.AngleDifference;

            // 3/24/2009 - Park Turret check
            // If reach goal, then turn off ParkTurret.
            if (turnTurretBehavior.ParkTurret && (Math.Abs(turnTurretBehavior.AngleDifference) <= OneRadian))
            {
                turnTurretBehavior.ParkTurret = false;
            }

            // 10/2/2008 - Take back out the tank rotation adjustment, because we don't want to distort
            //             the actual value for the turretFacingDirection.
            itemTurretAtts.TurretFacingDirection = MathHelper.WrapAngle(itemTurretAtts.TurretFacingDirection - item.FacingDirection);
           
        }

        // 11/14/2008
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            // null refs
            _turnToFaceAbstractBehavior = null;

            base.Dispose();
        }
     
    }
}
