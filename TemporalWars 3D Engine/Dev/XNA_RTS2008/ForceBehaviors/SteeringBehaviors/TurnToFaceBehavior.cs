#region File Description
//-----------------------------------------------------------------------------
// TurnToFaceBehavior.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.ForceBehaviors.Enums;
using ImageNexus.BenScharbach.TWEngine.ForceBehaviors.Structs;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.ForceBehaviors.SteeringBehaviors
{
    ///<summary>
    /// The <see cref="TurnToFaceBehavior"/> is used to calculate the proper angles, to turn some <see cref="SceneItem"/> towards
    /// some facing direction position.  This is accomplished by calculating the angle that an object should face, given its current
    /// position, its target position, its current angle, and its maximum turning speed.
    ///</summary>
    public sealed class TurnToFaceBehavior : AbstractBehavior
    {
        // Direction SceneItemOwner is facing in Radians.        
        private float _facingDirection;

        private Vector2 _pos; 
        private Vector2 _faceThis;
        private Vector2 _forwardDir;       
// ReSharper disable UnaccessedField.Local
        private float _desiredAngle;
// ReSharper restore UnaccessedField.Local

        // 10/14/2008 - Use Forward Velocity as Desired Angle?
        private bool _useForwardVelocity = true;
        private static readonly Vector3 Vector3Zero = Vector3.Zero; // 8/11/2009

        #region Properties
        // 1/30/2009 - 
        private float _angleDifference;

        /// <summary>
        /// Stores value of currentAngle/desiredAngle from TurnToFace method.
        /// </summary>
        public float AngleDifference
        {
            get { return _angleDifference; }
            set { _angleDifference = value; }
        }

        /// <summary>
        /// Direction offset adjustment in radians
        /// </summary>
        public float FacingDirectionOffset { get; set; }

        /// <summary>
        /// <see cref="SceneItem"/> will face in the direction of the Forward velocity.
        /// </summary>
        public bool UseForwardVelocity
        {
            get { return _useForwardVelocity; }
            set { _useForwardVelocity = value; }
        }

        /// <summary>
        /// <see cref="SceneItem"/> will face in the direction of the <see cref="SceneItem"/> it is attacking.
        /// </summary>
        public bool FaceAttackie { get; set; }

        // 10/14/2009
        /// <summary>
        /// <see cref="SceneItem"/> will Face in the direction to face the given Waypoint position. (Scripting Purposes)
        /// </summary>
        public bool FaceWaypoint { get; set; }

        // 10/14/2009 
        /// <summary>
        /// Waypoint position item must face towards. (Scripting Purposes)
        /// </summary>
        /// <remarks><see cref="FaceAttackie"/> property must be
        /// set to TRUE, in order to use this.</remarks>
        public Vector3 WaypointPosition { get; set; }

        #endregion

        ///<summary>
        /// Constructor
        ///</summary>
        public TurnToFaceBehavior()
            : base((int)BehaviorsEnum.TurnToFace, 0.0f)
        {
            
            
        }

        /// <summary>
        /// Calculates the angle that an object should face, given its position, its
        /// target's position, its current angle, and its maximum turning speed.
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
                throw new InvalidOperationException("TurnToFaceAbstractBehavior can ONLY be used with SceneItemWithPick types.");
            }

            // Since this method doesn't calculate any SteeringForce, let's return Zero.
            force = Vector3Zero;

            // 11/12/2008 - Check if AbstractBehavior on; turned off for MP games Client side. 
            if (!UseBehavior)
                return;

            // 10/14/2009 - Check if 'FaceWaypoint' is active, which overrides any other 'Turning' for this item.
            if (FaceWaypoint)
            {
                //System.Diagnostics.Debugger.Break();

                // Yes, so have item Turn to Waypoint position.
                DoFacingDirectionUsingAPosition(this, sceneItemWithPick, ref elapsedTime, WaypointPosition);

                var difference = MathHelper.WrapAngle(_angleDifference); // Return as Radians.

                // Once facing waypoint within 1 degrees, stop.
                if (SceneItemWithPick.IsFacingTargetWithin1Degrees(difference))
                    FaceWaypoint = false;

                return;
            }

            // 10/15/2008
            // If 'UseForwardVelocity' is False, then DesiredAngle is calculated using the SceneItemOwner's
            // current 'Position' and the 'MoveToPosition'; otherwise, the 'smoothHeader' Vector
            // in SceneItemOwner is used to determine the 'DesiredAngle'.
            if (_useForwardVelocity)
            {
                // 1/26/2009 - Check 'AstarItem' is Null
                DoFaceDirectionUsingForwardVelocity(this, sceneItemWithPick, ref elapsedTime);
                
            }
            else
            {
                // Face SceneItem toward given 'MoveToPosition'.
                DoFacingDirectionUsingAPosition(this, sceneItemWithPick, ref elapsedTime, sceneItemWithPick.MoveToPosition);
            }


        }

        // 10/14/2009; 6/12/2010: Updated to BehaviorsTimeSpan.
        /// <summary>
        /// Uses a given world position, for the <see cref="SceneItem"/>, as the facing direction.
        /// </summary>
        /// <param name="turnToFaceBehavior">Instance of <see cref="TurnToFaceBehavior"/>.</param>
        /// <param name="item"><see cref="SceneItem"/> to turn</param>
        /// <param name="elapsedTime">Game <see cref="TimeSpan"/> as elapsedTime</param>
        /// <param name="facePosition">World position to face towards.</param>
        private static void DoFacingDirectionUsingAPosition(TurnToFaceBehavior turnToFaceBehavior, SceneItemWithPick item, 
                                                            ref BehaviorsTimeSpan elapsedTime, Vector3 facePosition)
        {
            turnToFaceBehavior._pos.X = item.Position.X;
            turnToFaceBehavior._pos.Y = item.Position.Z;

            turnToFaceBehavior._faceThis.X = facePosition.X;
            turnToFaceBehavior._faceThis.Y = facePosition.Z;

            // 10/14/2009: Skip this section if 'FaceWaypoint' active!
            // 7/7/2009: Updated to use the refactored 'GetAttackiePosition'.
            // 2/4/2009 - If 'FaceAttackie' set, then override the
            //            faceThis with Attackie Position.
            if (!turnToFaceBehavior.FaceWaypoint)
            {
                Vector3 attackPosition;
                if (GetAttackiePosition(turnToFaceBehavior, item, out attackPosition))
                {
                    turnToFaceBehavior._faceThis.X = attackPosition.X;
                    turnToFaceBehavior._faceThis.Y = attackPosition.Z;
                }
            } // End if NOT FaceWaypoint

            // 8/10/2009 - Multiple TurretTurnSpeed, by the game elapsedTime.
            var itemItemTurnSpeed = item.ItemTurnSpeed * (float)elapsedTime.TotalSeconds;

            // Update the FacingDirectionOffset
            turnToFaceBehavior.FacingDirectionOffset = item.FacingDirectionOffset;

            // Call TurnToFace AbstractBehavior
            turnToFaceBehavior._facingDirection = SceneItemWithPick.TurnToFace(ref turnToFaceBehavior._pos, ref turnToFaceBehavior._faceThis, item.FacingDirection, itemItemTurnSpeed,
                                                            turnToFaceBehavior.FacingDirectionOffset, out turnToFaceBehavior._desiredAngle, out turnToFaceBehavior._angleDifference);

#if DEBUG
            // 7/20/2009 - Debugging
            if (float.IsNaN(turnToFaceBehavior._facingDirection))
                System.Diagnostics.Debugger.Break();
#endif

            item.FacingDirection = turnToFaceBehavior._facingDirection;
        }

        // 10/14/2009; 6/12/2010: Updated to BehaviorsTimeSpan.
        /// <summary>
        /// Uses the forward velocity of the <see cref="SceneItem"/> as the facing direction.
        /// </summary>
        /// <param name="turnToFaceBehavior">Instance of <see cref="TurnToFaceBehavior"/>.</param>
        /// <param name="item"><see cref="SceneItem"/> to turn</param>
        /// <param name="elapsedTime">Game <see cref="TimeSpan"/> as elapsedTime</param>
        private static void DoFaceDirectionUsingForwardVelocity(TurnToFaceBehavior turnToFaceBehavior, SceneItemWithPick item, ref BehaviorsTimeSpan elapsedTime)
        {
            if (item.AStarItemI != null)
            {
                turnToFaceBehavior._forwardDir.X = item.AStarItemI.SmoothHeading.X;
                turnToFaceBehavior._forwardDir.Y = item.AStarItemI.SmoothHeading.Z;

            }
            else
                return;

            // 7/7/2009: Updated to use the refactored 'GetAttackiePosition'.
            // 2/4/2009 - If 'FaceAttackie' set, then override the
            //            forwardDir calculation using the Attackie Position.
            Vector3 attackPosition;
            if (GetAttackiePosition(turnToFaceBehavior, item, out attackPosition))
            {
                turnToFaceBehavior._forwardDir.X = attackPosition.X - item.Position.X;
                turnToFaceBehavior._forwardDir.Y = attackPosition.Z - item.Position.Z;
            }

            // 4/1/2009 - if 'FaceAttackie' set and circleState is IdleCircling, then
            //            override the forwardDir calculation using the GoalPosition.
            var itemAircraft = (item as SciFiAircraftScene);
            if (turnToFaceBehavior.FaceAttackie && itemAircraft != null && itemAircraft.PlayableItemAtts.AircraftMustCircle
                && itemAircraft.CircleState0 == SciFiAircraftScene.CircleState.IdleCircling)
            {
                // Then set direction using goal Position
                if (itemAircraft.AStarItemI != null)
                {
                    turnToFaceBehavior._forwardDir.X = itemAircraft.AStarItemI.GoalPosition.X - itemAircraft.Position.X;
                    turnToFaceBehavior._forwardDir.Y = itemAircraft.AStarItemI.GoalPosition.Z - itemAircraft.Position.Z;
                }
            }

            // 8/10/2009 - Multiple TurretTurnSpeed, by the game elapsedTime.
            var itemItemTurnSpeed = item.ItemTurnSpeed * (float)elapsedTime.TotalSeconds;

            // Update the FacingDirectionOffset
            turnToFaceBehavior.FacingDirectionOffset = item.FacingDirectionOffset;
                
            // Call TurnToFace AbstractBehavior
            turnToFaceBehavior._facingDirection = SceneItemWithPick.TurnToFace(item.FacingDirection, itemItemTurnSpeed, turnToFaceBehavior._forwardDir,
                                                            turnToFaceBehavior.FacingDirectionOffset, out turnToFaceBehavior._desiredAngle);

#if DEBUG
            // 7/20/2009 - Debugging
            if (float.IsNaN(turnToFaceBehavior._facingDirection))
                System.Diagnostics.Debugger.Break();
#endif

            item.FacingDirection = turnToFaceBehavior._facingDirection;
        }

        // 7/7/2009
        /// <summary>
        /// Gets the <see cref="SceneItemWithPick.AttackSceneItem"/> position from the given <see cref="SceneItemWithPick"/>, 
        /// if <see cref="FaceAttackie"/> and <see cref="SceneItemWithPick.AttackOn"/> values are true.
        /// </summary>
        /// <param name="turnToFaceBehavior">Instance of <see cref="TurnToFaceBehavior"/></param>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        /// <param name="attackPosition">(OUT) Attack position</param>
        /// <returns>True/False of result</returns>
        private static bool GetAttackiePosition(TurnToFaceBehavior turnToFaceBehavior, SceneItemWithPick item, out Vector3 attackPosition)
        {
            if (turnToFaceBehavior.FaceAttackie && item.AttackOn)
            {
                // 7/7/2009 - Error can arise by checking if 'AttackSceneItem' is null, and then
                //            on the next line, trying to retrieve a value from it!  Why?  Because it can be
                //            changed by the another thread, making it Null!  Therefore, by retrieving the value
                //            and storing it here, this will guarantee the same value.
                var attackSceneItem = item.AttackSceneItem;

                // Then set direction using attackie Position                    
                if (attackSceneItem != null)
                {
                    attackPosition = attackSceneItem.Position;
                    return true;
                }
            }

            attackPosition = Vector3Zero;

            return false;
        }
        
       
       
    }
}
