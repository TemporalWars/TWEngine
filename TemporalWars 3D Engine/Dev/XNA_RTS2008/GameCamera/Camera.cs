#region File Description
//-----------------------------------------------------------------------------
// Camera.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.GameCamera.Delegates;
using ImageNexus.BenScharbach.TWEngine.GameCamera.Enums;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWEngine.TemporalWarInterfaces.Interfaces;
using ImageNexus.BenScharbach.TWEngine.Terrain;
using ImageNexus.BenScharbach.TWEngine.Utilities;
using ImageNexus.BenScharbach.TWLate.RTS_FogOfWarInterfaces.FOW;
using ImageNexus.BenScharbach.TWLate.RTS_MinimapInterfaces.Minimap;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.GameCamera
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.GameCamera"/> namespace contains the common classes
    /// which make up the entire camera component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
   
      
    // 10/22/2009: Updated to inherit from XNA GameComponent.
    ///<summary>
    /// The <see cref="Camera"/> class is used to show the game world
    /// in R.T.S. games.  
    ///</summary>
    public class Camera : GameComponent, ICamera
    {
        // 4/22/2010 - static ref to game instance
        private static Game _gameInstance;

        // 8/25/2008 - 
        ///<summary>
        /// <see cref="EventHandler"/> for <see cref="CameraUpdated"/>, which is triggered when the <see cref="Camera"/> is updated.
        ///</summary>
        public static event EventHandler CameraUpdated;

        // 8/27/2009
        private static readonly Vector3 Vector3Zero = Vector3.Zero;

        /// <summary>
        /// A global projection matrix since it never changes
        /// </summary>
        private static Matrix _projection;

        /// <summary>
        /// A global view matrix since it never changes
        /// </summary>
        private static Matrix _view;

        /// <summary>
        /// The <see cref="Camera"/> position
        /// </summary>
// ReSharper disable InconsistentNaming
        internal static Vector3 _cameraPosition = Vector3Zero;
// ReSharper restore InconsistentNaming

        /// <summary>
        /// The <see cref="Camera"/> position for Orthogonal view
        /// </summary>
        private static Vector3 _cameraPositionOrtho = Vector3Zero;

        /// <summary>
        ///  This value controls the point the <see cref="Camera"/> will aim at.
        /// </summary>
// ReSharper disable InconsistentNaming
        internal static Vector3 _cameraTarget = Vector3Zero;
// ReSharper restore InconsistentNaming

        /// <summary>
        ///  The <see cref="Camera"/> target for Orthogonal view.
        /// </summary>
        private static Vector3 _cameraTargetOrtho = Vector3Zero;

        /// <summary>
        /// The <see cref="Camera"/> rotation <see cref="Matrix"/>.
        /// </summary>
        private static Matrix _cameraRotation;       

        /// <summary>
        /// The <see cref="Camera"/> radius, or distance from target position.
        /// </summary>
        private static float _radius;

        /// <summary>
        /// The <see cref="Camera"/> height set by user mouse action
        /// </summary>
        private static float _userDesiredHeight;
        private static float _currentHeight;
        private static CameraDirectionEnum _cameraDirection = CameraDirectionEnum.None;
        private static bool _result;
        private static bool _doMoveCheck;
        private static bool _doRotationCheck;
        private static float _maxHeight;
        // 4/13/2010 - Used when the Properties 'Position' or 'Target' are changed!
        private static bool _updatePositionNextCycle;
        /// <summary>
        /// The <see cref="Camera"/> velocity 
        /// </summary>
        private const float Velocity = 30;

        // Orthogonal: Center (eye & focus in the _center of the terrain)
        private static Vector2 _center = Vector2.Zero;

#pragma warning disable 649
        /// <summary>
        /// The Terrain Height At <see cref="Camera"/> position
        /// </summary>
        private static float _terrainHeightAtCameraPos;
#pragma warning restore 649

        // 4/20/2009 - Bound CameraTarget movement
        private static Vector3 _minBound = Vector3Zero;
        private static Vector3 _maxBound = Vector3Zero;
        private static bool _doCameraBoundInit = true;        

        // 5/2/2009 - Camera speed / acceleration
        private const float CameraMaxSpeed = 500.0f;
        private const float CameraDefaultAcceleration = 0.1f;
        private static float _cameraAcceleration = CameraDefaultAcceleration;

        // 10/9/2009 - Camera Scrolled distance (Scripting purposes)
        private static float _scrolledDistance;
        private static bool _startedScroll;
        private static Vector3 _scrollStartPosition;
        private static Vector3 _scrollCurrentPosition;

        // 10/26/2009 - Camera's UP vector.
        private static Vector3 _cameraUp = Vector3.Up;

        // 10/27/2009 - Camera's Current Roll & Max Roll allowed.
        private const float TotalRollAllowed = 70.0f * (MathHelper.Pi / 180);
        private static float _currentRoll;

        // 6/15/2012
        private static AccelerationValueBehavior _accelerationValueBehavior;

        #region Properties

        // 6/15/2012
        /// <summary>
        /// 
        /// </summary>
        public static CameraDirectionEnum CameraDirection
        {
            get { return _cameraDirection; }
            set
            {
                switch (value)
                {
                    case CameraDirectionEnum.None:
                        // 6/15/2012
                        _accelerationValueBehavior.IsStartState = false;
                        break;
                    case CameraDirectionEnum.ScrollForward:
                    case CameraDirectionEnum.ScrollBackward:
                    case CameraDirectionEnum.ScrollLeft:
                    case CameraDirectionEnum.ScrollRight:
                    case CameraDirectionEnum.Up:
                    case CameraDirectionEnum.Down:
                        _doMoveCheck = true;
                        // 6/15/2012
                        _accelerationValueBehavior.IsStartState = true;
                        break;
                    case CameraDirectionEnum.RotateRight:
                    case CameraDirectionEnum.RotateLeft:
                        _doRotationCheck = true;
                        // 6/15/2012
                        _accelerationValueBehavior.IsStartState = true;
                        break;
                }

                _cameraDirection = value;
            }
        }

        // 10/21/2009
        /// <summary>
        /// Lock or Unlock <see cref="Camera"/> rotation (Scripting Purposes)
        /// </summary>
        public static bool LockRotation { get; set; }

        // 10/21/2009
        /// <summary>
        /// Lock or Unlock <see cref="Camera"/> scroll (Scripting Purposes)
        /// </summary>
        public static bool LockScroll { get; set; }

        // 10/21/2009
        /// <summary>
        /// Lock or Unlock <see cref="Camera"/> zoom (Scripting Purposes)
        /// </summary>
        public static bool LockZoom { get; set; }

        // 10/21/2009
        /// <summary>
        /// Lock or Unlock <see cref="Camera"/> reset (Scripting Purposes)
        /// </summary>
        public static bool LockReset { get; set; }

        // 10/21/2009
        /// <summary>
        /// Lock or Unlock All <see cref="Camera"/> movement. (Scripting Purposes)
        /// </summary>
        public static bool LockAll
        {
            set
            {
                LockRotation = value;
                LockScroll = value;
                LockZoom = value;
                LockReset = value;
            }
        }

        // 10/9/2009 
        /// <summary>
        /// Set when <see cref="Camera"/> was just reset.
        /// </summary>
        public static bool CameraWasReset { get; set; }

        // 10/9/2009 - 
        /// <summary>
        /// Tracks total continious rotation of <see cref="Camera"/>
        /// to right in degrees; used for Scripting purposes.
        /// </summary>
        public static float RotateRightCounter { get; set; }

        // 10/9/2009 - 
        /// <summary>
        /// Tracks total continious rotation of <see cref="Camera"/> 
        /// to left in degrees; used for Scripting purposes.
        /// </summary>
        public static float RotateLeftCounter { get; set; }

        // 10/9/2009
        /// <summary>
        /// Tracks total continious distance of <see cref="Camera"/>
        /// zooming out; used for Scripting purposes.
        /// </summary>
        public static float ZoomOutCounter { get; set; }

        // 10/9/2009
        /// <summary>
        /// Tracks total continous distance of <see cref="Camera"/>
        /// zooming in; used for Scripting purposes.
        /// </summary>
        public static float ZoomInCounter { get; set; }

        // 10/9/2009 - This value is reset in the 'ResetAcceleration' method, since 
        //             this is called when no scrolling is occuring!
        /// <summary>
        /// Tracks total continious <see cref="Camera"/> movement in some direction;
        /// used for Scripting purposes.
        /// </summary>
        public static float ScrolledDistance
        {
            get { return _scrolledDistance; }
            set { _scrolledDistance = value; }
        }

        // 6/30/2009
        ///<summary>
        /// Set to tell <see cref="Camera"/> class to do the bound intilization.
        ///</summary>
        public static bool DoCameraBoundInit
        {
            set { _doCameraBoundInit = value; }
        }

        // 4/22/2010 - Non-Static for Interface ref.
        Matrix ICamera.View
        {
            get { return View; }
        }

        Matrix ICamera.Projection
        {
            get { return Projection; }
        }

        ///<summary>
        /// Returns the <see cref="Camera"/> projection <see cref="Matrix"/>.
        ///</summary>
        public static Matrix Projection
        {
            get{ return _projection; }
        }

        /// <summary>
        /// Returns the <see cref="Camera"/> view <see cref="Matrix"/>.
        /// </summary>
        public static Matrix View
        {
            get{ return _view; }
        }

        // 7/25/2008 - Delegate assinee
        /// <summary>
        /// When Functions are assigned, they will be called from within
        /// the <see cref="UpdatePosition"/> method of the <see cref="Camera"/> Class.
        /// </summary>
        private static UpdateDueToCameraMovement UpdateDueToCameraMovement { get; set; }

        // 4/22/2010 - Non-Static for Interface ref.
        void ICamera.SetNormalRTSView()
        {
            SetNormalRTSView();
        }

        // 4/22/2010 - Non-Static for Interface ref.
        BoundingFrustum ICamera.CameraFrustum
        {
            get
            {
                return CameraFrustum;
            }
        }

        /// <summary>
        /// <see cref="Camera"/> view <see cref="BoundingFrustum"/>.
        /// </summary>
        public static BoundingFrustum CameraFrustum { get; private set; }


        ///<summary>
        /// <see cref="Camera"/> position in game world.
        ///</summary>
        public static Vector3 CameraPosition
        {
            get{ return _cameraPosition; }
            set
            {
                _cameraPosition = value;
                _cameraPosition.Y = _userDesiredHeight;

                // 4/13/2010 - Set flag to update in next cycle, rather than call 'UpdatePosition' directly!
                _updatePositionNextCycle = true;
            }
        }

        ///<summary>
        /// <see cref="Camera"/> target position, or where it is looking.
        ///</summary>
        public static Vector3 CameraTarget
        {
            get{ return _cameraTarget; }
            set
            {
                _cameraTarget = value;

                // 4/13/2010 - Set flag to update in next cycle, rather than call 'UpdatePosition' directly!
                _updatePositionNextCycle = true;
            }
        }

        // 7/1/2009
        ///<summary>
        /// <see cref="Camera"/> rotation <see cref="Matrix"/>.
        ///</summary>
        public static Matrix CameraRotation
        {
            get { return _cameraRotation; }            
        }

        // 10/26/2009
        /// <summary>
        /// Returns the <see cref="Camera"/> forward vector.
        /// </summary>
        public static Vector3 CameraForward
        {
            get { return GetCameraForwardVector(); }
        }

        /// <summary>
        /// The <see cref="AlphaAngle"/> determines the top rotation of the <see cref="Camera"/> around the y-axis.
        /// </summary>
        public static float AlphaAngle { get; set; }

        /// <summary>
        /// The <see cref="BetaAngle"/> determines the eye's Position from the side view and the elevation
        /// from the ground.
        /// </summary>
        public static float BetaAngle { get; set; }

        /// <summary>
        /// The <see cref="Camera"/> aspect ratio
        /// </summary>
        private static float AspectRatio { get; set; }

        /// <summary>
        /// The <see cref="Camera"/> field of view
        /// </summary>
        private static float FOV { get; set; }

        /// <summary>
        /// The <see cref="Camera"/> near plane
        /// </summary>
        private static float NearPlane { get; set; }

        /// <summary>
        /// The <see cref="Camera"/> far plane
        /// </summary>
        private static float FarPlane { get; set; }

        /// <summary>
        /// The <see cref="Camera"/> max height
        /// </summary>
        public static float MaxHeight
        {
            get { return _maxHeight; }
            set 
            { 
                _maxHeight = value;
                _userDesiredHeight = _terrainHeightAtCameraPos + _maxHeight;
            }
        }

        /// <summary>
        /// The <see cref="Camera"/> min height
        /// </summary>
        private static float MinHeight { get; set; }

        // 1/7/2009
        /// <summary>
        /// The <see cref="Camera"/> current height
        /// </summary>
        public static float CurrentHeight
        {
            get { return _userDesiredHeight; }
            set
            {
                _userDesiredHeight = value;

                if (_userDesiredHeight < (_terrainHeightAtCameraPos + MinHeight))
                    _userDesiredHeight = _terrainHeightAtCameraPos + MinHeight;
                if (_userDesiredHeight > (_terrainHeightAtCameraPos + _maxHeight))
                    _userDesiredHeight = _terrainHeightAtCameraPos + _maxHeight;
            }
            
        }

        // 10/26/2009
        /// <summary>
        /// <see cref="Camera"/> UP (<see cref="Vector3.Up"/>) vector
        /// </summary>
        public static Vector3 CameraUp
        {
            get { return _cameraUp; }
        }

        #endregion

        ///<summary>
        /// Creates the main game <see cref="Camera"/>, using the given attributes.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        ///<param name="fov">Field-of-View</param>
        ///<param name="aspectRatio">Aspect ratio for the <see cref="Camera"/></param>
        ///<param name="nearPlane">Near plane distance</param>
        ///<param name="farPlane">Far plane distance</param>
        public Camera(Game game, float fov, float aspectRatio, float nearPlane, float farPlane) : base(game)
        {   
            // 4/22/2010 - save ref to game in static var.
            _gameInstance = game;

            Matrix.CreatePerspectiveFieldOfView(fov, aspectRatio, nearPlane, farPlane, out _projection);
            AspectRatio = aspectRatio;
            FOV = fov;
            NearPlane = nearPlane;
            FarPlane = farPlane;

            // 5/29/2008
            AlphaAngle = MathHelper.ToRadians(90);
            BetaAngle = MathHelper.ToRadians(57); // 57 degrees is what C&C3 uses! - Ben
            _radius = 900.0f; // determines height 
            
            // 1/7/2009
            _currentHeight = (float)(_radius * Math.Sin(BetaAngle));
            _userDesiredHeight = _currentHeight; // 6/19/2009
            _maxHeight = 1000.0f;
            MinHeight = 600.0f; 

            // 6/15/2012 - Create new Acceleration value behavior
            _accelerationValueBehavior = new AccelerationValueBehavior(CameraMaxSpeed, 0.25f);

            // 7/23/2008
            UpdatePosition(null, null);
        }

        // 10/22/2009
        /// <summary>
        /// Allows checking if the <see cref="Camera"/> has any <see cref="CameraCinematics"/> to process,
        /// like following splines or following <see cref="SceneItem"/> for example. (Scripting Purposes)
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // 5/29/2012 - Enter Pause check here.
            if (TemporalWars3DEngine.GamePaused)
                return;

            // check if there are any 'Cinematics' to process. (Scripting Purposes)
            CameraCinematics.UpdateCinematics(gameTime);

            // Update Camera's target Position                       
            Vector3 vectorToAdd = Vector3Zero;

            // 6/15/2012 - check if movement updated required.
            if (_doMoveCheck)
            {
                // 4/20/2009 - CameraTarget Bounds
                if (_doCameraBoundInit)
                    SetDefaultCameraBoundArea(); // 1/13/2011 - Refactored to method.

                MoveCamera(gameTime, ref vectorToAdd);
            }

            // 6/15/2012 - check if rotation updated required.
            if (_doRotationCheck)
                RotateCamera(gameTime);

            // 4/13/2010 - Check if Camera Position need updating.
            if (_updatePositionNextCycle)
            {
                UpdatePosition(null, null);
                _updatePositionNextCycle = false;
            }

            // 6/15/2012; // 10/16/2012 - Fixed by adding check if '_doRotationCheck' or '_doMoveCheck'.
            if (_doRotationCheck || _doMoveCheck) 
                DoCameraCalculations(ref vectorToAdd);

            // 6/15/2012 - Reset values each tick.
            _doMoveCheck = false;
            _doRotationCheck = false;
        }
  
        // 5/1/2009
        /// <summary>
        /// Resets the <see cref="Camera"/> acceleration back to the default value, and
        /// resets the <see cref="ScrolledDistance"/> to zero.
        /// </summary>
        public static void ResetAcceleration()
        {
            _cameraAcceleration = CameraDefaultAcceleration;
           
            // 6/15/2012 - Reset acceleration.
            _accelerationValueBehavior.IsStartState = false;

            // 10/9/2009 - Reset ScrolledDistance (Scripting purposes)
            _startedScroll = false;
            ScrolledDistance = 0;
        }

        // 4/20/2009
        /// <summary>
        /// Updates the <see cref="CameraTarget"/> vector, then calls the <see cref="UpdatePosition"/> method, which
        /// internally updates the <see cref="CameraPosition"/> automatically.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="vectorToAdd"> </param>
        private static void MoveCamera(GameTime gameTime, ref Vector3 vectorToAdd)
        {
            // 6/15/2012 - Check if Lock set (Scripting Purposes)
            if (LockScroll)
                return;

            // Apply Accelartion
            var scrolledDistance = _accelerationValueBehavior.GetDelta(gameTime);

            // 6/28/2012 - Updated to use [Flags] on enum, which allows diagonal moving! - :) Ben
            switch (_cameraDirection)
            {   
                case CameraDirectionEnum.ScrollForward:
                case CameraDirectionEnum.ScrollBackward:
                case CameraDirectionEnum.ScrollLeft:
                case CameraDirectionEnum.ScrollRight:
                    break;
                case CameraDirectionEnum.Up:
                    vectorToAdd.Y = scrolledDistance;
                    break;
                case CameraDirectionEnum.Down:
                    vectorToAdd.Y = -scrolledDistance;
                    break;
            }

            // 6/28/2012
            if (((int)_cameraDirection & (int)CameraDirectionEnum.ScrollForward) != 0)
            {
                vectorToAdd.Z = -scrolledDistance;
                ScrolledDistance += scrolledDistance * 25f; // 10/9/2009 (Scripting purposes)
            }

            // 6/28/2012
            if (((int)_cameraDirection & (int)CameraDirectionEnum.ScrollBackward) != 0)
            {
                vectorToAdd.Z = scrolledDistance;
                ScrolledDistance += scrolledDistance * 25f; // 10/9/2009 (Scripting purposes)
            }

            // 6/28/2012
            if (((int)_cameraDirection & (int)CameraDirectionEnum.ScrollLeft) != 0)
            {
                vectorToAdd.X = -scrolledDistance;
                ScrolledDistance += scrolledDistance * 25f; // 10/9/2009 (Scripting purposes)
            }

            // 6/28/2012
            if (((int)_cameraDirection & (int)CameraDirectionEnum.ScrollRight) != 0)
            {
                vectorToAdd.X = scrolledDistance;
                ScrolledDistance += scrolledDistance * 25f; // 10/9/2009 (Scripting purposes)
            }

        }

        /// <summary>
        /// Updates the <see cref="CameraTarget"/> vector, then calls the <see cref="UpdatePosition"/> method, which
        /// internally updates the <see cref="CameraPosition"/> automatically.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private static void RotateCamera(GameTime gameTime)
        {
            // Skip if lock is on. (Scripting Purposes)
            if (LockRotation)
                return;

            // cache values
            var elapsedGameTime = gameTime.ElapsedGameTime;
            var totalSeconds = (float)elapsedGameTime.TotalSeconds;

            switch (_cameraDirection)
            {
                case CameraDirectionEnum.RotateRight:
                    AlphaAngle += 2.5f*totalSeconds;
                    RotateRightCounter += 2.5f * totalSeconds; // 10/9/2009 (Scripting purposes)
                    break;
                case CameraDirectionEnum.RotateLeft:
                    AlphaAngle -= 2.5f*totalSeconds;
                    RotateLeftCounter += 2.5f * totalSeconds; // 10/9/2009 (Scripting purposes)
                    break;
            }
        }

        // 6/15/2012
        /// <summary>
        /// Updates the camera rotation and position values.
        /// </summary>
        private static void DoCameraCalculations(ref Vector3 vectorToAdd)
        {
            // 6/15/2012 - Check if Lock set (Scripting Purposes)
            if (LockScroll)
                return;

            // Apply Camera Rotation Vector and movement.
            const float degrees90ToRadians = 90*(MathHelper.Pi/180);
            var inAngle = AlphaAngle + degrees90ToRadians; // MathHelper.ToRadians(90)

            Matrix.CreateRotationY(-inAngle, out _cameraRotation);
            //Vector3 rotatedVector = Vector3.Transform(vectorToAdd, -_cameraRotation);
            Matrix.Multiply(ref _cameraRotation, -1, out _cameraRotation);
            Vector3 rotatedVector;
            Vector3.Transform(ref vectorToAdd, ref _cameraRotation, out rotatedVector);
            //_cameraTarget += (Velocity * rotatedVector);
            Vector3.Multiply(ref rotatedVector, Velocity, out rotatedVector);
            Vector3.Add(ref _cameraTarget, ref rotatedVector, out _cameraTarget);

            // Clamp CameraTarget in bounds of map            
            Vector3.Clamp(ref _cameraTarget, ref _minBound, ref _maxBound, out _cameraTarget);

            // 10/9/2009 - If 'CameraWasReset', then set back to false. (Used for Scripting)
            if (CameraWasReset) CameraWasReset = false;

            // 7/23/2008 - Updated the Camera Position, using the new Target calcs.
            UpdatePosition(null, null);

            // 10/9/2009 - Update 'Scrolled' distance (Used for Scripting)
            UpdateScrolledPosition();
        }


        // 1/13/2011
        /// <summary>
        /// Sets the camera's bound area to the size of the terrain map.
        /// </summary>
        public static void SetDefaultCameraBoundArea()
        {
            _minBound.X = 0;
            _minBound.Z = 0;
            _maxBound.X = TerrainData.MapWidthToScale;
            _maxBound.Z = TerrainData.MapHeightToScale;

            _doCameraBoundInit = false;
        }

        // 10/9/2009
        /// <summary>
        /// Helper method, which calculates the distance the <see cref="Camera"/> has continiously
        /// scrolled in some direction; used for Scripting purposes.
        /// </summary>
        private static void UpdateScrolledPosition()
        {
            // 10/21/2009 - Skip if lock is on. (Scripting Purposes)
            if (LockScroll) return;

            switch (_cameraDirection)
            {
                case CameraDirectionEnum.ScrollForward:
                case CameraDirectionEnum.ScrollBackward:
                case CameraDirectionEnum.ScrollLeft:
                case CameraDirectionEnum.ScrollRight:

                    // 10/9/2009 - Track Scrolled distance (Scriping purposes)
                    if (!_startedScroll)
                    {
                        _startedScroll = true;

                        // set start camera position
                        _scrollStartPosition = CameraPosition;
                    }

                    // save camera current position
                    _scrollCurrentPosition = CameraPosition;

                    // calculate the distance scrolled
                    Vector3.Distance(ref _scrollStartPosition, ref _scrollCurrentPosition, out _scrolledDistance);

                    break;
            }
        }
                
        /*///<summary>
        /// Updates <see cref="Camera"/> movement, while speed of movement is
        /// regulated with the <paramref name="delta"/> parameter.  
        ///</summary>
        ///<param name="delta"><see cref="Vector2"/> delta</param>
        public static void MoveCamera(ref Vector2 delta)
        {
            // down on the thumbstick is -1. however, in screen coordinates, values
            // increase as they go down the screen. so, we have to flip the sign of the
            // y component of delta.
            delta.Y *= -1;

            // Normalize Delta
            delta.Normalize();
            var tmpDelta = Vector3Zero;
            tmpDelta.X = delta.X; tmpDelta.Z = delta.Y;

            // 1/7/2009: Updated to remove Ops overload, which is slow on XBOX!
            //_cameraTarget += tmpDelta * Velocity;  // TODO: Times by game elapsedtime
            
            Vector3.Multiply(ref tmpDelta, Velocity, out tmpDelta);
            Vector3.Add(ref _cameraTarget, ref tmpDelta, out _cameraTarget);
           

            // 1/7/2009: Fixed the 2nd line to be Z axis, and not the Y axis.
            _cameraTarget.X = MathHelper.Clamp(_cameraTarget.X, 0, TerrainData.MapWidthToScale);
            _cameraTarget.Z = MathHelper.Clamp(_cameraTarget.Z, 0, TerrainData.MapHeightToScale);            

            // 7/23/2008
            UpdatePosition(null, null);

        }*/

        // 10/22/2009
        /// <summary>
        /// Sets the <see cref="Camera"/> at some position, looking at the given <paramref name="lookAtPosition"/> position, with
        /// the given <paramref name="zoom"/> 0-1.0.  (Scripting Purposes)
        /// </summary>
        /// <param name="position"><see cref="Vector3"/> position to place <see cref="Camera"/> at.</param>
        /// <param name="lookAtPosition">Direction <see cref="Camera"/> is looking.</param>
        /// <param name="zoom">Zoom value of 0.0 to 1.0</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="zoom"/> is outside allowable range of 0.0 - 1.0.</exception>
        public static void SetUpCamera(ref Vector3 position, ref Vector3 lookAtPosition, float zoom)
        {
            // verify the zoom value is between 0.0 - 1.0
            if (zoom < 0.0f || zoom > 1.0f)
                throw new ArgumentOutOfRangeException("zoom", @"Zoom value given is outside the allowable range of 0.0 - 1.0.");

            // set Camera's Target to be the LookAtPosition.
            _cameraTarget = lookAtPosition;

            // calculate what the AlphaAngle is (SideRadius), used to position the camera 360 around
            // the given LookAt position.  
            {
                // 1st - get direction from Camera position, to lookAt position.
                Vector3 direction;
                Vector3.Subtract(ref position, ref lookAtPosition, out direction);

                // 2nd - we'll use the Atan2 function. Atan will calculates the arc tangent of 
                // y / x for us, and has the added benefit that it will use the signs of x
                // and y to determine what cartesian quadrant to put the result in.
                // http://msdn2.microsoft.com/en-us/library/system.math.atan2.aspx
                AlphaAngle = (float)Math.Atan2(direction.Z, direction.X); // Return as Radians.
               
            }

            // 3rd - calculate distance between positions.
            float distance;
            Vector3.Distance(ref position, ref lookAtPosition, out distance);

            // 4th - calculate the new zoom value, using the Maxheight.
            //float zoomHeight = GetZoomHeight(ref position, zoom);
            //_currentHeight = zoomHeight;

            // UpdatePosition, using new values.
            UpdatePosition(distance, zoom);

        }

        // 1/13/2011
        ///<summary>
        /// Sets the area which the camera can move in.
        ///</summary>
        ///<param name="minBound">Minimum value as <see cref="Vector3"/>.</param>
        ///<param name="maxBound">Maximum value as <see cref="Vector3"/>.</param>
        ///<exception cref="ArgumentOutOfRangeException">Thrown when either the <paramref name="minBound"/>
        /// or <paramref name="maxBound"/> values are out of range.</exception>
        public static void SetCameraBoundArea(ref Vector3 minBound, ref Vector3 maxBound)
        {
            if (minBound.X < 0 || minBound.Z < 0)
                throw new ArgumentOutOfRangeException("minBound", @"MinBound value MUST be greater than zero.");

            if (minBound.X > TerrainData.MapWidthToScale || minBound.Z > TerrainData.MapHeightToScale)
                throw new ArgumentOutOfRangeException("minBound", @"MinBound value MUST be less than the maximum size of the terrain map.");

            if (maxBound.X < 0 || maxBound.Z < 0)
                throw new ArgumentOutOfRangeException("maxBound", @"MaxBound value MUST be greater than zero.");

            if (maxBound.X > TerrainData.MapWidthToScale || maxBound.Z > TerrainData.MapHeightToScale)
                throw new ArgumentOutOfRangeException("maxBound", @"MaxBound value MUST be less than the maximum size of the terrain map.");

            if (minBound.X > maxBound.X || minBound.Z > maxBound.Z)
                throw new ArgumentOutOfRangeException("minBound", @"MinBound value CANNOT be greater than the given MaxBound value.");

            // Set new values
            _doCameraBoundInit = false;
            _minBound = minBound;
            _maxBound = maxBound;

        }

        // 10/22/2009
        /// <summary>
        /// Method helper, which when given a position, will extract the 
        /// proper height value for the position, and multiple it by the given <paramref name="zoom"/> 
        /// percent (0.0 - 1.0) of <see cref="MaxHeight"/> allowed.
        /// </summary>
        /// <param name="zoom">Zoom height from ground (0.0 - 1.0)</param>
        /// <param name="position"><see cref="Camera"/> position</param>
        /// <returns>Proper zoom height for given position</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="zoom"/> is outside allowable range of 0.0 - 1.0.</exception>
        internal static float GetZoomHeight(ref Vector3 position, float zoom)
        {
            // verify the zoom value is between 0.0 - 1.0
            if (zoom < 0.0f || zoom > 1.0f)
                throw new ArgumentOutOfRangeException("zoom", @"Zoom value given is outside the allowable range of 0.0 - 1.0.");

            var terrainHeight = TerrainData.GetTerrainHeight(position.X, position.Z);
            var zoomHeight = (terrainHeight + _maxHeight) * zoom;
            MathHelper.Clamp(zoomHeight, terrainHeight + MinHeight, terrainHeight + _maxHeight);
            return zoomHeight;
        }

        // 4/10/2008; 1/7/2009
        /// <summary>
        /// Raises the <see cref="Camera"/> by some calculated height value. (Zoom Out)
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public static void RaiseCameraHeight(GameTime gameTime)
        {
            // 10/21/2009 - Skip if lock is on. (Scripting Purposes)
            if (LockZoom) return;

            // 4/29/2009
            var elapsedGameTime = gameTime.ElapsedGameTime;           

            // Raise Camera Up
            if (_currentHeight < (_terrainHeightAtCameraPos + _maxHeight))
                _radius += 2500.0f * (float)elapsedGameTime.TotalSeconds;
            else
                _currentHeight = (_terrainHeightAtCameraPos + _maxHeight);

            // Calc new height value.
            var newHeightValue = (float)(_radius * Math.Sin(BetaAngle));

            // 10/9/2009 - Track ZoomOutCounter distance (Scripting purposes)
            ZoomOutCounter +=  Math.Abs(_currentHeight - newHeightValue);

            // 1/7/2009 - Assign new Height value.
            _currentHeight = newHeightValue;
            _userDesiredHeight = _currentHeight; // 6/19/2009

            // 8/27/2009 - Conversion of const degrees to radians.
            const float degrees57ToRadians = 57.0f*(MathHelper.Pi/180);
            const float degrees5ToRadians = 5.0f*(MathHelper.Pi/180);

            // 1/7/2009 - Also increase betaAngle.
            if (BetaAngle < degrees57ToRadians) // MathHelper.ToRadians(57.0f)
                BetaAngle += degrees5ToRadians; // MathHelper.ToRadians(5.0f)
            else
                BetaAngle = degrees57ToRadians;           

            // 7/23/2008
            UpdatePosition(null, null);
           

        }

        // 4/10/2008; 1/7/2009
        /// <summary>
        /// Lowers the <see cref="Camera"/> by some calculated height value. (Zoom In)
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public static void LowerCameraHeight(GameTime gameTime)
        {
            // 10/21/2009 - Skip if lock is on. (Scripting Purposes)
            if (LockZoom) return;

            // 4/29/2009
            var elapsedGameTime = gameTime.ElapsedGameTime;            

            // Lower Camera Down
            if (_currentHeight > (_terrainHeightAtCameraPos + MinHeight))
                _radius -= 2500.0f * (float)elapsedGameTime.TotalSeconds;
            else
                _currentHeight = (_terrainHeightAtCameraPos + MinHeight);

            // Calc new height value.
            var newHeightValue = (float)(_radius * Math.Sin(BetaAngle));

            // 10/9/2009 - Track ZoomInCounter distance (Scripting purposes)
            ZoomInCounter += Math.Abs(_currentHeight - newHeightValue);

            // 1/7/2009 - Assign new Height value.
            _currentHeight = newHeightValue;
            _userDesiredHeight = _currentHeight; // 6/19/2009

            // 8/27/2009 - Conversion of const degrees to radians.
            const float degrees30ToRadians = 30.0f * (MathHelper.Pi / 180);
            const float degrees5ToRadians = 5.0f * (MathHelper.Pi / 180);

            // 1/7/2009 - Also increase betaAngle.
            if (BetaAngle > degrees30ToRadians) // MathHelper.ToRadians(30.0f)
                BetaAngle -= degrees5ToRadians;
            else
                BetaAngle = degrees30ToRadians;            

            // 7/23/2008
            UpdatePosition(null, null);           

        }       

        // 5/28/2008
        /// <summary>
        /// Set the <see cref="Camera"/> in Orthogonal view; used for <see cref="IFogOfWar"/>
        /// and <see cref="IMinimap"/>.
        /// </summary>
        /// <param name="mapHeight">Current height of terrain map.</param>   
        /// <param name="mapWidth">Current width of terrain map.</param>     
        public static void SetOrthogonalView(int mapWidth, int mapHeight)
        {

            _center.X = ((float)mapWidth / 2);
            _center.Y = ((float)mapHeight / 2);

            _cameraPositionOrtho.X = _center.X;
            _cameraPositionOrtho.Y = 1000.0f;
            _cameraPositionOrtho.Z = _center.Y;

            _cameraTargetOrtho.X = _center.X;
            _cameraTargetOrtho.Y = 0.0f;
            _cameraTargetOrtho.Z = _center.Y;

            var inForward = Vector3.Forward;
            Matrix.CreateLookAt(ref _cameraPositionOrtho, ref _cameraTargetOrtho, ref inForward, out _view);

            Matrix.CreateOrthographic(mapWidth, mapHeight, 0.1f, 2000.0f, out _projection);

            // 7/10/2009
            UpdateCameraFrustum();

        }


        // 4/22/2010 - Non-Static for Interface ref.
        void ICamera.SetOrthogonalView(int mapWidth, int mapHeight)
        {
            SetOrthogonalView(mapWidth, mapHeight);
        }

        /// <summary>
        /// Set the <see cref="Camera"/> in normal R.T.S. (Real Time Strategy) view
        /// </summary>
        public static void SetNormalRTSView()
        {
            // Update Camera Position
            // with those values, we'll calculate the viewMatrix.  
            Matrix.CreateLookAt(ref _cameraPosition, ref _cameraTarget, ref _cameraUp, out _view);
            
            Matrix.CreatePerspectiveFieldOfView(FOV, AspectRatio, NearPlane, FarPlane, out _projection);

            // 7/10/2009
            UpdateCameraFrustum();

        }

        // 9/12/2008: Updated to optimize memory, by change to the Pass by Reference versions of the 
        //            XNA Methods.
        // 7/23/2008: 
        /// <summary>
        /// Checks if current <see cref="BoundingSphere"/> is in <see cref="Camera"/> frustrum.
        /// </summary>
        /// <param name="worldTransform">World transform to apply</param>
        /// <param name="boundingSphere"><see cref="BoundingSphere"/> to check</param>
        /// <param name="inFrustrum">(OUT) Is in frustum?</param>
        public static void IsInCameraFrustrum(ref Matrix worldTransform, ref BoundingSphere boundingSphere, out bool inFrustrum)
        {
            try
            {
                // Transform by World Matrix
                BoundingSphere tmpBoundingSphere;
                boundingSphere.Transform(ref worldTransform, out tmpBoundingSphere);

                CameraFrustum.Intersects(ref tmpBoundingSphere, out _result);
               
                inFrustrum = _result;
            }
            catch
            {
                // Empty
                inFrustrum = false;
            }

        }

        // 10/26/2009: Updated to allow overriding the 'zoomHeight' value.
        // 2/25/2008 - Ben; 10/22/2009: Updated to allow overriding the 'SideRadius' value.
        /// <summary>
        /// Update <see cref="Camera"/> position by polling Keyboard/Mouse Input.
        /// </summary>
        /// <remarks>
        /// - If <paramref name="sideRadius"/> is set to NULL value, then it will automatically calculate 
        /// the <paramref name="sideRadius"/> using the 'Radius' and <see cref="BetaAngle"/>; otherwise, it will use the
        /// given parameter value.
        /// - If <paramref name="zoomHeight"/> is set to NULL value, then it will automatically calculate
        /// the <paramref name="zoomHeight"/> using the current terrain height; otherwise, it will use the
        /// given param value.
        /// </remarks>
        /// <param name="sideRadius">Allows overriding the SideRadius of <see cref="Camera"/>; distance from target. (Set to NULL for Auto-Calc)</param>
        /// <param name="zoomHeight">Allows overriding the ZoomHeight of <see cref="Camera"/>. (Set to NULL for Auto-Calc)</param>
        public static void UpdatePosition(float? sideRadius, float? zoomHeight)
        {
            // Update Camera's Position based on target Position!
            if (sideRadius == null)
                sideRadius = (float)(_radius * Math.Cos(BetaAngle));
            //height = (float)(_radius * Math.Sin(betaAngle));

            _cameraPosition.X = (float) (_cameraTarget.X + sideRadius*Math.Cos(AlphaAngle));
            _cameraPosition.Y = _cameraTarget.Y + _currentHeight;
            _cameraPosition.Z = (float) (_cameraTarget.Z + sideRadius*Math.Sin(AlphaAngle));

            // 10/26/20009 - Check if adjusting ZoomHeight given.
            if (zoomHeight != null)
                _cameraPosition.Y = GetZoomHeight(ref _cameraPosition, zoomHeight.Value);

            // with those values, we'll calculate the viewMatrix.    
            UpdateLookAt();
        }

        // 10/22/2009
        /// <summary>
        /// Updates the <see cref="Camera"/> LookAt matrix and camera frustrum.
        /// </summary>
        internal static void UpdateLookAt()
        {
            Matrix.CreateLookAt(ref _cameraPosition, ref _cameraTarget, ref _cameraUp, out _view);
           
            // 7/10/2009
            UpdateCameraFrustum();

            // 7/25/2008 - Call Update Delegate for any attached method
            if (UpdateDueToCameraMovement != null)
                UpdateDueToCameraMovement();

            // 8/25/2008 - Fire CameraUpdated Event
            if (CameraUpdated != null)
                CameraUpdated(null, EventArgs.Empty);
        }

        // 7/10/2009
        /// <summary>
        /// Updates the <see cref="Camera"/> frustum, using the new <see cref="View"/>/<see cref="Projection"/> attributes.
        /// </summary>
        private static void UpdateCameraFrustum()
        {
            // 7/23/2008; 1/7/2009: Updated by removing the Matrix Ops overload, which is SLOW on XBOX!
            Matrix newViewProjectionMatrix;
            Matrix.Multiply(ref _view, ref _projection, out newViewProjectionMatrix);

            // 1/28/2009
            if (CameraFrustum == null)
                CameraFrustum = new BoundingFrustum(newViewProjectionMatrix);
            else
                CameraFrustum.Matrix = newViewProjectionMatrix;
        }

        // 1/7/2009
        /// <summary>
        /// Returns the current <see cref="TWEngine.Terrain"/> height, using the given <see cref="Vector3"/> (X/Z) values.
        /// </summary>
        /// <param name="inVector">A <see cref="Vector3"/> to use for height search</param>
        /// <param name="terrainHeight">(OUT) Height Result</param>
// ReSharper disable UnusedMember.Local
        private static void GetTerrainHeightForGivenVector(ref Vector3 inVector, out float terrainHeight)
// ReSharper restore UnusedMember.Local
        {
            // Default
            terrainHeight = 0; 

            // Only Get TerrainHeight for positions inside the Terrain Map
            if (inVector.X <= 0 || inVector.X >= TerrainData.MapWidthToScale || inVector.Z <= 0 ||
                inVector.Z >= TerrainData.MapHeightToScale) return;

            terrainHeight = TerrainData.GetTerrainHeight(inVector.X, inVector.Z);
           
            // Is it a valid height
            if (Math.Abs(terrainHeight - -10000) < float.Epsilon)
                terrainHeight = 0;
        }

        // 1/7/2009
        /// <summary>
        /// Resets the <see cref="Camera"/> <see cref="BetaAngle"/> (Height) 
        /// and <see cref="AlphaAngle"/> (rotation) to initial values.
        /// </summary>
        public static void ResetCameraPosition()
        {
            // 10/21/2009 - Skip if lock is on. (Scripting Purposes)
            if (LockReset) return;
           
            // Reset values back to their originals.
            ResetCameraAlphaAngle(); // 2/1/2010
            ResetCameraBetaAngle(); // 2/1/2010
      
            // 10/9/2009 - Set for Scripting purposes
            CameraWasReset = true;

            UpdatePosition(null, null);
           
        }

        // 2/1/2010
        /// <summary>
        /// Resets the <see cref="Camera"/> rotation around the look-at point; Y-axis (UP). (Scripting Purposes)
        /// </summary>
        public static void ResetCameraAlphaAngle()
        {
            // Conversion of const degrees to radians.
            const float degrees90ToRadians = 90.0f * (MathHelper.Pi / 180);

            AlphaAngle = degrees90ToRadians;
           
        }

        // 2/1/2010
        /// <summary>
        /// Resets the <see cref="Camera"/> rotation around the look-at point; X-axis.  (Scripting Purposes)
        /// </summary>
        public static void ResetCameraBetaAngle()
        {
            // 8/27/2009 - Conversion of const degrees to radians.
            const float degrees57ToRadians = 57.0f * (MathHelper.Pi / 180);

            BetaAngle = degrees57ToRadians;
            _cameraUp = Vector3.Up;
            _radius = 900.0f;
            _currentHeight = (float)(_radius * Math.Sin(BetaAngle));
            _userDesiredHeight = _currentHeight; // 6/19/2009 
            
        }

        // 10/26/2009
        /// <summary>
        /// Adjusts the <see cref="Camera"/> roll by some angle delta. (Scripting Purposes)
        /// </summary>
        /// <param name="roll">Roll angle to adjust by (-pi to pi)</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <see cref="roll"/> is outside allowable range of -pi to pi.</exception>
        public static void AdjustRollBy(float roll)
        {
            // verify 'Roll' angle given is in radians, within the range of -pi to pi.
            if (roll < -MathHelper.Pi || roll > MathHelper.Pi)
                throw new ArgumentOutOfRangeException("roll", @"Roll value given must fall in the allowable range of -pi to pi.");
            
            // Make sure not past the TotalRoll allowed!
            if (Math.Abs(_currentRoll + roll) >= TotalRollAllowed) return;

            // Get Camera Forward vector.
            var forwardDirection = CameraForward;

            // Calculate new Up vector to roll
            Matrix matrixTranform;
            Matrix.CreateFromAxisAngle(ref forwardDirection, roll, out matrixTranform);
            Vector3.Transform(ref _cameraUp, ref matrixTranform, out _cameraUp);

            // update currentRoll value.
            _currentRoll += roll;

            // Update the LookAt View matrix.
            UpdateLookAt();
        }

        // 10/26/2009
        /// <summary>
        /// Helper method, which calculates the <see cref="Camera"/> forward vector.
        /// </summary>
        /// <returns><see cref="Camera"/> forward <see cref="Vector3"/></returns>
        private static Vector3 GetCameraForwardVector()
        {
            Vector3 forwardDirection;
            Vector3.Subtract(ref _cameraTarget, ref _cameraPosition, out forwardDirection);
            
            // normalize direction
            if (!forwardDirection.Equals(Vector3.Zero))
                forwardDirection.Normalize();

            return forwardDirection;
        }

        // 4/22/2010
        ///<summary>
        /// Given some world <see cref="Vector3"/> position, converts from object space to screen space.  The
        /// final <see cref="Vector2"/> screen position is returned via the <paramref name="screenPosition"/>.
        ///</summary>
        ///<param name="worldPosition">World <see cref="Vector3"/> position to convert</param>
        ///<param name="screenPosition">(OUT) Converted <see cref="Vector2"/> screen position</param>
        public static void ProjectToScreen(ref Vector3 worldPosition, out Vector2 screenPosition)
        {
            // Using ViewPort.Project, we will convert the SceneItemOwner's World Position to Screen Cordinates.
            var projectedPosition = _gameInstance.GraphicsDevice.Viewport.Project(worldPosition, Projection, View, Matrix.Identity);
            // save as screen 2D coordinates.
            screenPosition = new Vector2 { X = projectedPosition.X, Y = projectedPosition.Y };

        }

        // 4/22/2010 - Private explicit implementation of the ICamera interface method.
        void ICamera.ProjectToScreen(ref Vector3 worldPosition, out Vector2 screenPosition)
        {
            ProjectToScreen(ref worldPosition, out screenPosition);
        }

        // 10/22/2009
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this final dispose?</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Null Refs            
                UpdateDueToCameraMovement = null;
                CameraFrustum = null;
            }

            base.Dispose(disposing);
        }

       
    }
}

