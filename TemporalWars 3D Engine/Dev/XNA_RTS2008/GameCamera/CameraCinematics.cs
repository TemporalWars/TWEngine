#region File Description
//-----------------------------------------------------------------------------
// CameraCinematics.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TWEngine.Common;
using TWEngine.GameCamera.Enums;
using TWEngine.GameCamera.Structs;
using TWEngine.Players;
using TWEngine.SceneItems;
using TWEngine.Terrain;

namespace TWEngine.GameCamera
{
    // 10/24/2009
    /// <summary>
    /// The <see cref="CameraCinematics"/> class is used to move the <see cref="Camera"/> around in the game world.
    /// </summary>
    public static class CameraCinematics
    {
        // 6/7/2012 - Dictinary which tracks the completion of a camera spline operation. (Scripting Purposes)
        public static readonly Dictionary<string, bool> CinematicSplinesCompleted = new Dictionary<string, bool>(); 

        // 10/22/2009
        // ReSharper disable InconsistentNaming
        /// <summary>
        /// A Queue to hold the <see cref="Camera"/> <see cref="Curve3D"/> splines, specifically
        /// for camera's position.  These are used in scripting, to 
        /// cause the camera to move about the map. (Scripting Purposes)
        /// </summary>
        private static readonly Queue<Curve3D> CinematicSplines_Position;

        // 10/24/2009
        /// <summary>
        /// A Queue to hold the <see cref="Camera"/> <see cref="Curve3D"/>  splines, specifically
        /// for camera's target. These are used in scripting, to
        /// cause the camera to move about the map. (Scripting Purposes)
        /// </summary>
        private static readonly Queue<Curve3D> CinematicSplines_Target;

        // 10/22/2009 - Stores the current spline the camera is using.
        private static Curve3D _cinematicCurveSpline_Position;
        private static Curve3D _cinematicCurveSpline_Target; // 10/24/2009
        private static double _timePosition;
        private static double _timeTarget; // 10/24/2009
        // ReSharper restore InconsistentNaming

        // 10/26/2009
        /// <summary>
        /// Some <see cref="SceneItem"/> for the <see cref="Camera"/> to follow.  Set to Null to stop
        /// following item.
        /// </summary>
        private static SceneItemWithPick _sceneItemToFollow;

        /// <summary>
        /// <see cref="Camera"/> height above item to follow.
        /// </summary>
        private static float _sceneItemToFollowZoomHeight;

        // 10/26/2009 - Camera's Roll Atts.
        private static bool _useSmoothStepInterpolation;
        private static float _lastRollValue;
        private static float _adjustRollTo;
        private static float _adjustRollFrom;
        private static int _timeToCompleteRollInMilliSeconds;
        private static int _timeElapsedRollInMilliSeconds;

        // constructor
        /// <summary>
        /// Static constructor, used to create the two required internal
        /// queues, which are the <see cref="CinematicSplines_Position"/> and <see cref="CinematicSplines_Target"/>.
        /// </summary>
        static CameraCinematics()
        {
            // 10/22/2009 - Create the Queue for CinematicNodes. (Scripting Purposes)
            CinematicSplines_Position = new Queue<Curve3D>();

            // 10/24/2009 - Create the Queue for CinematicNodes. (Scripting Purposes)
            CinematicSplines_Target = new Queue<Curve3D>();
        }

        // 10/26/2009
        /// <summary>
        /// Update should be called once per frame, which allows checking for spline 
        /// movement, and other cinematic <see cref="Camera"/> processes.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance.</param>
        internal static void UpdateCinematics(GameTime gameTime)
        {
            // Process Splines, if any; however, skip when some item to follow.
            if (_sceneItemToFollow == null)
                ProcessAnyCinematicSplines(gameTime);
            else
            {
                // Else, update to follow sceneItem.
                FollowSceneItem();
            }

            // Check if Camera's Roll needs adjusting.
            ProcessCinematicRolls(gameTime);
        }

        // 10/27/2009
        /// <summary>
        /// Checks if there is some ROLL to process for the <see cref="Camera"/>.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance.</param>
        private static void ProcessCinematicRolls(GameTime gameTime)
        {
            if (_lastRollValue == _adjustRollTo || _timeElapsedRollInMilliSeconds >= _timeToCompleteRollInMilliSeconds) return;

            // 'Roll' change smoothly.
            _timeElapsedRollInMilliSeconds += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            var lerpAmount = (_timeElapsedRollInMilliSeconds / (float)_timeToCompleteRollInMilliSeconds);
                
            // Check if to Interpolate lineraly or smoothly.
            float rollAngle;
            if (_useSmoothStepInterpolation)
                rollAngle = MathHelper.SmoothStep(_adjustRollFrom, _adjustRollTo, lerpAmount);
                //float rollAngle = MathHelper.Lerp(_adjustRollFrom, _adjustRollTo, lerpAmount);
            else
                rollAngle = _adjustRollFrom + (_adjustRollTo - _adjustRollFrom) * lerpAmount;
                
            // calculate rollDelta adjustment
            var rollAdjustment = rollAngle - _lastRollValue;

            // Adjust Camera's Roll by delta
            Camera.AdjustRollBy(rollAdjustment);
            _lastRollValue = rollAngle;
        }

        // 10/26/2009
        /// <summary>
        /// Checks if there is some <see cref="SceneItem"/> for the <see cref="Camera"/> to follow.
        /// </summary>
        private static void FollowSceneItem()
        {
            // set lookAt to be item's position.
            Camera._cameraTarget = _sceneItemToFollow.Position;

            // update position
            Camera.UpdatePosition(null, _sceneItemToFollowZoomHeight);
        }

        // 10/24/2009
        /// <summary>
        /// Processes the Queue of splines, by checking if a current spline is being updated,
        /// and if not, to dequeue a new spline for the <see cref="Camera"/> to follow.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance.</param>
        private static void ProcessAnyCinematicSplines(GameTime gameTime)
        {
            // check if there are any 'CinematicSplines' to dequeue. (Scripting Purposes)
            {
                ProcessCinematicSplines_Position(gameTime);
            }

            // check if there is a current 'Cinematic' splines to process. (Scripting Purposes)
            {
                ProcessCinematicSplines_Target(gameTime);
            }
        }

        // 10/26/2009
        /// <summary>
        /// Processes the Queue of splines, specifically for the <see cref="Camera"/> target.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance.</param>
        private static void ProcessCinematicSplines_Target(GameTime gameTime)
        {
            // Check Target Queue
            if (CinematicSplines_Target.Count > 0 && _cinematicCurveSpline_Target == null)
            {
                // Setup new Curve Spline for camera to follow.
                _timeTarget = 0;
                _cinematicCurveSpline_Target = CinematicSplines_Target.Dequeue();

            }

            // Check for Target Spline
            if (_cinematicCurveSpline_Target == null) return;

            // Calculate the Target using the Curve3D Class.
            ProcessCurrentCinematicSpline_Target(gameTime);

            // Update the Camera' LookAt matrix.
            Camera.UpdateLookAt();
        }

        // 10/26/2009
        /// <summary>
        /// Processes the Queue of splines, specifically for the <see cref="Camera"/> position.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance.</param>
        private static void ProcessCinematicSplines_Position(GameTime gameTime)
        {
            // Check Position Queue
            if (CinematicSplines_Position.Count > 0 && _cinematicCurveSpline_Position == null)
            {
                // Setup new Curve Spline for camera to follow.
                _timePosition = 0;
                _cinematicCurveSpline_Position = CinematicSplines_Position.Dequeue();
            }

            // Check for Position Spline
            if (_cinematicCurveSpline_Position == null) return;

            // Calculate the Position using the Curve3D Class.
            ProcessCurrentCinematicSpline_Position(gameTime);

            // Update the Camera' LookAt matrix.
            Camera.UpdateLookAt();
        }


        // 10/24/2009
        /// <summary>
        /// Processes the current 'CinematicCurveSpline' for positions, by updating the <see cref="GameTime"/>
        /// and retrieving the point on the curve.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance.</param>
        private static void ProcessCurrentCinematicSpline_Position(GameTime gameTime)
        {
            var tmpTime = (float)_timePosition;

            // Is spline complete?
            if (_cinematicCurveSpline_Position.GetPointOnCurve(tmpTime, out Camera._cameraPosition))
            {
                // 6/7/2012 - Update Dictinary that this operation completed.
                UpdateCinematicSplinesCompletedDictionay(_cinematicCurveSpline_Position.Curve3DName, true);

                // yes, so clear current spline.
                _cinematicCurveSpline_Position = null;
                return;
            }

            _timePosition += gameTime.ElapsedGameTime.TotalMilliseconds;
        }

        // 6/7/2012
        /// <summary>
        /// Updates the internal dictionary for the given string name.
        /// </summary>
        /// <param name="splineName">Name of spline.</param>
        /// <param name="isCompleted">Value to set.</param>
        private static void UpdateCinematicSplinesCompletedDictionay(string splineName, bool isCompleted)
        {
            if (CinematicSplinesCompleted == null) return;

            if (CinematicSplinesCompleted.ContainsKey(splineName))
            {
                CinematicSplinesCompleted[splineName] = isCompleted;
            }
            else
            {
                CinematicSplinesCompleted.Add(splineName, isCompleted);
            }
        }

        // 10/24/2009
        /// <summary>
        /// Processes the current 'CinematicCurveSpline' for targets, by updating the <see cref="GameTime"/>
        /// and retrieving the point on the curve.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance.</param>
        private static void ProcessCurrentCinematicSpline_Target(GameTime gameTime)
        {
            var tmpTime = (float)_timeTarget;

            // Is spline complete?
            if (_cinematicCurveSpline_Target.GetPointOnCurve(tmpTime, out Camera._cameraTarget))
            {
                // 6/8/2012 - Update Dictinary that this operation completed.
                UpdateCinematicSplinesCompletedDictionay(_cinematicCurveSpline_Target.Curve3DName, true);

                // yes, so clear current spline.
                _cinematicCurveSpline_Target = null;
                return;
            }

            _timeTarget += gameTime.ElapsedGameTime.TotalMilliseconds;
        }
      

        // 10/24/2009
        /// <summary>
        /// Adds a single pair of <see cref="CinematicNode"/> to move the <see cref="Camera"/> from point A to point B.
        /// </summary>
        /// <param name="cinematicSplineName">Name of this cinematic spline.</param>
        /// <param name="cinematicNodeStart"><see cref="CinematicNode"/> starting position</param>
        /// <param name="cinematicNodeEnd"><see cref="CinematicNode"/> ending position</param>
        /// <param name="cameraMoveType">Affect <see cref="Camera"/> position or target (lookAt).</param>
        public static void AddNewCinematicNodePair(string cinematicSplineName, ref CinematicNode cinematicNodeStart, ref CinematicNode cinematicNodeEnd, CameraMoveType cameraMoveType)
        {
            // Create new instance of Curve3D
             var cinematicCurveSpline = new Curve3D();
            
            // create starting point with proper height zoom.
            {
                var startPos = new Vector3
                {
                    X = cinematicNodeStart.Position.X,
                    Z = cinematicNodeStart.Position.Z
                };

                // Get Proper ZoomHeight
                startPos.Y = Camera.GetZoomHeight(ref startPos, cinematicNodeStart.HeightZoom);
                cinematicCurveSpline.AddPoint(ref startPos, 0); // Add StartingPosition on Spline.
            }

            {
                // create ending point with proper height zoom.
                var endPos = new Vector3
                {
                    X = cinematicNodeEnd.Position.X,
                    Z = cinematicNodeEnd.Position.Z
                };

                // Get Proper ZoomHeight
                endPos.Y = Camera.GetZoomHeight(ref endPos, cinematicNodeEnd.HeightZoom);
                cinematicCurveSpline.AddPoint(ref endPos, cinematicNodeEnd.TimeToCompleteInMilliSeconds); // Add StartingPosition on Spline.

            }

            cinematicCurveSpline.MaxTimeAllowed = cinematicNodeEnd.TimeToCompleteInMilliSeconds; // 10/24/2009
            cinematicCurveSpline.SetTangents();

            // 6/24/2012 - Refactored.
            EnqueueCinematicCurveSpline(cinematicSplineName, cameraMoveType, cinematicCurveSpline);
        }

        // 10/24/2009
        /// <summary>
        /// Adds a cinematic spline, for the <see cref="Camera"/> to follow, using the given
        /// LinkedList of position nodes in the game world.  Also, the camera
        /// will interpolate the height between the start and ending values.
        /// </summary>
        /// <param name="cinematicSplineName">Name of this cinematic spline.</param>
        /// <param name="linkedList">LinkedList of position nodes to follow</param>
        /// <param name="startZoomHeight">Staring Zoom height value (0.0 to 1.0)</param>
        /// <param name="endZoomHeight">Ending Zoom height value (0.0 to 1.0)</param>
        /// <param name="totalTime">Total time to complete the movement in milliseconds.</param>
        /// <param name="cameraMoveType">Affect <see cref="Camera"/> position or target (lookAt).</param>
        public static void AddNewCinematicSplineFromLinkedList(string cinematicSplineName, LinkedList<int> linkedList, float startZoomHeight, float endZoomHeight, int totalTime, CameraMoveType cameraMoveType)
        {
            // 6/7/2012 - check if null
            if (string.IsNullOrEmpty(cinematicSplineName))
                throw new ArgumentNullException("cinematicSplineName");

            // Create new instance of Curve3D
            var cinematicCurveSpline = new Curve3D();

            // 1st - get first & last items in linkedList.
            var currentWaypoint = linkedList.First;
            var lastWaypoint = linkedList.Last;

            // 2nd - calc the zoom height difference between nodes.
            //       Example: If startZoom = 0.25 and endZoom = 0.75, then change = endZoom - startZoom = 0.50.
            //                If current LinkedList has 5 nodes, then adjustment = 0.5 / 5 = 0.10.
            var totalZoomDifference = endZoomHeight - startZoomHeight;
            var zoomAdjustmentPerNode = totalZoomDifference / linkedList.Count;

            // 3rd - calc the time adjustment between nodes.
            var timeAdjustmentPerNode = totalTime / linkedList.Count;

            // iterate list, and add positions for sceneItem to follow.
            // NOTE: The FORACH construct could be used here; however this causes garbage on the XBOX!

            // loop until linked item is last item in list.
            var isLast = false;
            var adjCounter = 0;
            while (!isLast)
            {
                // Get position for given waypoint index
                Vector3 position;
                TerrainWaypoints.GetExistingWaypoint(currentWaypoint.Value, out position);

                // Get Proper ZoomHeight
                position.Y = Camera.GetZoomHeight(ref position, startZoomHeight + (zoomAdjustmentPerNode*adjCounter));

                // Add point into Curve3D spline.
                cinematicCurveSpline.AddPoint(ref position, timeAdjustmentPerNode * adjCounter);

                // check if currentWaypoint was last one
                if (currentWaypoint == lastWaypoint)
                    isLast = true;

                // move to next item in linkedList
                currentWaypoint = currentWaypoint.Next;

                // increase counter
                adjCounter++;

            } // End while loop

            cinematicCurveSpline.MaxTimeAllowed = totalTime;
            cinematicCurveSpline.SetTangents();

            // 6/24/2012 - Refactored.
            EnqueueCinematicCurveSpline(cinematicSplineName, cameraMoveType, cinematicCurveSpline);
        }

        // 6/24/2012
        /// <summary>
        /// Helper method to add the given <paramref name="cinematicCurveSpline"/> to the internal queue.
        /// </summary>
        private static void EnqueueCinematicCurveSpline(string cinematicSplineName, CameraMoveType cameraMoveType,
                                                        Curve3D cinematicCurveSpline)
        {
            switch (cameraMoveType)
            {
                case CameraMoveType.Position:
                    // 6/7/2012 - Update Dictinary that this operation completed.
                    cinematicCurveSpline.Curve3DName = cinematicSplineName;
                    UpdateCinematicSplinesCompletedDictionay(cinematicSplineName, false);
                    // 10/24/2009 - Add to Queue
                    CinematicSplines_Position.Enqueue(cinematicCurveSpline);
                    break;
                case CameraMoveType.Target:
                    // 6/8/2012 - Update Dictinary that this operation completed.
                    cinematicCurveSpline.Curve3DName = cinematicSplineName;
                    UpdateCinematicSplinesCompletedDictionay(cinematicSplineName, false);
                    // 10/24/2009 - Add to Queue
                    CinematicSplines_Target.Enqueue(cinematicCurveSpline);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("cameraMoveType");
            } // End Switch
        }

        // 10/26/2009
        /// <summary>
        /// Sets <see cref="Camera"/> to follow the specific <see cref="SceneItem"/> around
        /// in the game world. (Scripting Purposes)
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to follow (Only Selectable SceneItems allowed)</param>
        /// <param name="zoomHeight">Zoom height value (0.0 to 1.0)</param>
        /// <remarks>**Selectable <see cref="SceneItem"/> are items like <see cref="BuildingScene"/> or <see cref="SciFiTankScene"/> types.</remarks>
        public static void SetCameraToFollowSpecificSceneItem(string sceneItemName, float zoomHeight)
        {
            // try get 'Named' sceneItem from Player class
            SceneItem namedSceneItem;
            if (Player.SceneItemsByName.TryGetValue(sceneItemName, out namedSceneItem))
            {
                // check if this is a ScenaryItem, which is not allowed for this 1st param!
                SceneItem.DoCheckIfSceneItemIsScenaryItemType(namedSceneItem);

                // cast SceneItem to SceneItemWithPick
                _sceneItemToFollow = (namedSceneItem as SceneItemWithPick);

                if (_sceneItemToFollow == null)
                    throw new InvalidOperationException("Cast of SceneItem to SceneItemWithPick failed!");

                // store Camera's height above sceneItem.
                _sceneItemToFollowZoomHeight = zoomHeight;

                return;

            }

            throw new ArgumentException(@"Given sceneItem Name is NOT VALID!", "sceneItemName");
  
        }

        // 10/26/2009
        /// <summary>
        /// Stops the <see cref="Camera"/> from following any SceneItems. (Scripting Purposes)
        /// </summary>
        public static void StopFollowingAnySceneItems()
        {
            // set to null, to stop the camera from following item.
            _sceneItemToFollow = null;
        }

        // 1/15/2010
        /// <summary>
        /// Stops the <see cref="Camera"/> from following any position or target splines. (Scripting Purposes)
        /// </summary>
        public static void StopAllCinematicSplines()
        {
            // Clear out any remaining items in Queues.
            CinematicSplines_Position.Clear();
            CinematicSplines_Target.Clear();

            // Null current splines
            _cinematicCurveSpline_Position = null;
            _cinematicCurveSpline_Target = null;
        }

        // 10/26/2009
        /// <summary>
        /// Sets the <see cref="Camera"/> to change its roll, smoothly over the total time given.
        /// </summary>
        /// <param name="newRoll">New roll value.</param>
        /// <param name="totalTime">Total time to complete the movement in milliseconds.</param>
        /// <param name="useSmoothStep">Default is to use linear interpolation; however if this is TRUE, smoothStep is used, which
        /// Interpolates smoothly by easing in and out.</param>
        public static void AdjustCameraRollTo(float newRoll, int totalTime, bool useSmoothStep)
        {
            // verify 'Pitch' angle given is in radians, within the range of -pi to pi.
            if (newRoll < -MathHelper.Pi || newRoll > MathHelper.Pi)
                throw new ArgumentOutOfRangeException("newRoll", @"Roll value given must fall in the allowable range of -pi to pi.");

            _adjustRollFrom = _lastRollValue; // save current roll
            _adjustRollTo = newRoll; // set new adjustment roll
            _timeToCompleteRollInMilliSeconds = totalTime; // time to complete adjustment
            _useSmoothStepInterpolation = useSmoothStep; // 10/27/2009
        }
    }
}
