#region File Description
//-----------------------------------------------------------------------------
// TerrainDirectionalIcon.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.GameCamera;
using ImageNexus.BenScharbach.TWEngine.Utilities;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.Terrain
{
    // 3/2/2011
    ///<summary>
    /// The <see cref="TerrainDirectionalIcon"/> draws a directional icon on the terrain.
    ///</summary>
    public class TerrainDirectionalIcon
    {
        // 6/15/2012 - Acceleration value behavior 
        private readonly AccelerationValueBehavior _accelerationValueBehavior;

        public enum MovementDirection
        {
            Still,
            Right,
            Left,
            Up,
            Down,
        }

        // directional icon's attributes.
        private bool _visible;
        private Vector3 _directionalIconPosition;
        private int _directionalIconSize = 50;
        private float _directionalIconRotation;
        private Color _directionalIconColor = Color.White;
        private readonly int _index;

        // rotation attributes
        private bool _doAnimatedRotation;
        private float _timeMaxRotation;
        private int _timeElapsedRotation;
        private float _deltaMagnitudeRotation = 50.0f;

        // 10/24/2012
        private bool _applyCameraRotation;

        // movement attributes
        private MovementDirection _movementDirection = MovementDirection.Still;

        // Bound position movement
        private Vector3 _minBound = Vector3.Zero;
        private Vector3 _maxBound = Vector3.Zero;

        #region Properties

        /// <summary>
        /// Is Directional Icon Visible?
        /// </summary>
        public bool Visible
        {
            get { return _visible; }
            set
            {
                _visible = value;
                TerrainDirectionalIconManager.IconVisibles[_index] = value;
            }
        }

        ///<summary>
        /// Position for directional icon.
        ///</summary>
        public Vector3 DirectionalIconPosition
        {
            get { return _directionalIconPosition; }
            set
            {
                // Clamp position            
                Vector3.Clamp(ref value, ref _minBound, ref _maxBound, out value);

                _directionalIconPosition = value;
                TerrainDirectionalIconManager.IconPositions[_index] = value;
            }
        }

        ///<summary>
        /// Size of directional icon.
        ///</summary>
        public int DirectionalIconSize
        {
            get { return _directionalIconSize; }
            set
            {
                _directionalIconSize = value;
                TerrainDirectionalIconManager.IconSizes[_index] = value;
            }
        }

        ///<summary>
        /// Rotation of directional icon.
        ///</summary>
        public float DirectionalIconRotation
        {
            get { return _directionalIconRotation; }
            set
            {
                _directionalIconRotation = value;
                TerrainDirectionalIconManager.IconRotations[_index] = value;
            }
        }

        ///<summary>
        /// Color of directional Icon
        ///</summary>
        public Color DirectionalIconColor
        {
            get { return _directionalIconColor; }
            set
            {
                _directionalIconColor = value;
                TerrainDirectionalIconManager.IconColors[_index] = value.ToVector4();
            }
        }

        // 10/24/2012
        /// <summary>
        /// Gets or sets to apply the camera rotation to the current Icon's position.
        /// </summary>
        public bool ApplyCameraRotation
        {
            get { return _applyCameraRotation; }
            set
            {
                _applyCameraRotation = value;
                TerrainDirectionalIconManager.ApplyCameraRotation[_index] = value;
            }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="index">Index position into effect array.</param>
        public TerrainDirectionalIcon(int index)
        {
            _index = index;
            SetDefaultMovementBoundArea();

            // 6/15/2012 - Create instance of AccelerationValueBehavior class, then wrap with adapter.
            _accelerationValueBehavior = new AccelerationValueBehavior(750.0f, 0.20f);
        }

        // 6/6/2012
        /// <summary>
        /// Updates values, like rotation.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // Update Camera's target Position                       
            var vectorToAdd = Vector3.Zero;

            // check for rotation updates.
            DoAnimatedRotationCheck(gameTime);
  
            // check for movement requests.
            DoMovementCheck(gameTime, ref vectorToAdd);

            // 10/24/2012 - check for applying camera rotation. (Scripting Purposes)
            DoRotationCalculations(ref vectorToAdd);
        }

        // 6/6/2012
        /// <summary>
        /// Sets to move Directional Icon in <see cref="MovementDirection"/>.
        /// </summary>
        /// <param name="movementDirection"><see cref="MovementDirection"/> to move in.</param>
        /// <param name="deltaMagnitude">The Position's delta magnitude change value. (Rate of Change)</param>
        /// <returns>Current position of Directional Icon.</returns>
        internal Vector3 SetMovementDirection(MovementDirection movementDirection, float deltaMagnitude)
        {
            _movementDirection = movementDirection;
            _accelerationValueBehavior.DeltaMax = deltaMagnitude; // 6/15/2012

            // Convert to Terrain Scale
            Vector3 positionScaled;
            Vector3.Multiply(ref _directionalIconPosition, TerrainData.cScale, out positionScaled);

            return positionScaled;
        }

        // 6/6/2012
        ///<summary>
        /// Sets the area which the Directional Icon can move in.
        ///</summary>
        ///<param name="minBound">Minimum value as <see cref="Vector3"/>.</param>
        ///<param name="maxBound">Maximum value as <see cref="Vector3"/>.</param>
        ///<exception cref="ArgumentOutOfRangeException">Thrown when either the <paramref name="minBound"/>
        /// or <paramref name="maxBound"/> values are out of range.</exception>
        public void SetMovementBoundArea(ref Vector3 minBound, ref Vector3 maxBound)
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


            // Remove TerrainScale
            Vector3 minBoundWithoutScale;
            Vector3.Multiply(ref minBound, 0.10f, out minBoundWithoutScale);
            Vector3 maxBoundWithoutScale;
            Vector3.Multiply(ref maxBound, 0.10f, out maxBoundWithoutScale);
            
            // Set new values
            _minBound = minBoundWithoutScale;
            _maxBound = maxBoundWithoutScale;
        }

        // 6/6/2012
        /// <summary>
        /// Starts a rotation request.
        /// </summary>
        /// <param name="deltaMagnitude">The Rotation's delta magnitude change value. (Rate of Change)</param>
        /// <param name="rotationTimeMax">Set to length of given rotation in milliseconds; 0 implies infinite.</param>
        public void StartRotation(float deltaMagnitude, int rotationTimeMax)
        {
            _doAnimatedRotation = true;
            _timeElapsedRotation = 0;
            _deltaMagnitudeRotation = deltaMagnitude;
            _timeMaxRotation = rotationTimeMax;
        }

        // 6/6/2012
        /// <summary>
        /// Checks if there is an animated rotation to calculate.
        /// </summary>
        private void DoAnimatedRotationCheck(GameTime gameTime)
        {
            if (!_doAnimatedRotation)
                return;

            // Update rotation
            DirectionalIconRotation += (float)(_deltaMagnitudeRotation * gameTime.ElapsedGameTime.TotalSeconds);

            // If Non-Stop operation, then return.
            if (Math.Abs(_timeMaxRotation - 0) < float.Epsilon) return;

            // Otherwise, check if time is up
            _timeElapsedRotation += gameTime.ElapsedGameTime.Milliseconds;
            if (_timeElapsedRotation >= _timeMaxRotation)
            {
                _doAnimatedRotation = false;
            }
        }

        // 6/6/2012; 10/24/2012 - Add 2nd param which carries the new delta change to add to the final position.
        /// <summary>
        /// Checks for movement requests and updates the position as needed.
        /// </summary>
        private void DoMovementCheck(GameTime gameTime, ref Vector3 vectorToAdd)
        {
            // check movement direction
            switch (_movementDirection)
            {
                case MovementDirection.Still:
                    // 6/15/2012
                    _accelerationValueBehavior.IsStartState = false;
                    break;
                case MovementDirection.Right:
                    // 6/15/2012
                    _accelerationValueBehavior.IsStartState = true;
                    //_directionalIconPosition.X += _accelerationValueBehavior.GetDelta(gameTime); //  (int)(_deltaMagnitudeMovement * gameTime.ElapsedGameTime.TotalSeconds);
                    vectorToAdd.X += _accelerationValueBehavior.GetDelta(gameTime); //  (int)(_deltaMagnitudeMovement * gameTime.ElapsedGameTime.TotalSeconds);
                    break;
                case MovementDirection.Left:
                    // 6/15/2012
                    _accelerationValueBehavior.IsStartState = true;
                    //_directionalIconPosition.X -=  _accelerationValueBehavior.GetDelta(gameTime); // (int)(_deltaMagnitudeMovement * gameTime.ElapsedGameTime.TotalSeconds);
                    vectorToAdd.X -=  _accelerationValueBehavior.GetDelta(gameTime); // (int)(_deltaMagnitudeMovement * gameTime.ElapsedGameTime.TotalSeconds);
                    break;
                case MovementDirection.Up:
                    // 6/15/2012
                    _accelerationValueBehavior.IsStartState = true;
                    //_directionalIconPosition.Z -=  _accelerationValueBehavior.GetDelta(gameTime); // (int)(_deltaMagnitudeMovement * gameTime.ElapsedGameTime.TotalSeconds);
                    vectorToAdd.Z -=  _accelerationValueBehavior.GetDelta(gameTime); // (int)(_deltaMagnitudeMovement * gameTime.ElapsedGameTime.TotalSeconds);
                    break;
                case MovementDirection.Down:
                    // 6/15/2012
                    _accelerationValueBehavior.IsStartState = true;
                    //_directionalIconPosition.Z +=  _accelerationValueBehavior.GetDelta(gameTime); // (int)(_deltaMagnitudeMovement * gameTime.ElapsedGameTime.TotalSeconds);
                    vectorToAdd.Z +=  _accelerationValueBehavior.GetDelta(gameTime); // (int)(_deltaMagnitudeMovement * gameTime.ElapsedGameTime.TotalSeconds);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // update to effect
            DirectionalIconPosition = _directionalIconPosition;
            // Reset movement to still
            _movementDirection = MovementDirection.Still;
        }

        // 10/24/2012
        /// <summary>
        /// Calculates a rotation vector, using the current Camera's position and target locations.
        /// This rotation vector is then applied to this <see cref="TerrainDirectionalIcon"/> position vector.
        /// </summary>
        private void DoRotationCalculations(ref Vector3 vectorToAdd)
        {
            if (!_applyCameraRotation)
                return;

            // #1 - Calculate the Camera's directional vector
            var cameraTarget = Camera.CameraTarget;
            var cameraPosition = Camera.CameraPosition;
            Vector3 directionalVector;
            Vector3.Subtract(ref cameraTarget, ref cameraPosition, out directionalVector);

            // #2 - Retrieve the angle using the calculated directional vector.
            const float degrees90ToRadians = 90 * (MathHelper.Pi / 180);
            var desiredAngle = (float)Math.Atan2(-directionalVector.Z, directionalVector.X) + degrees90ToRadians;

            // #3 - Create the required rotation matrix, using the calculated desiredAngle.
            Matrix rotationMatrix;
            Matrix.CreateRotationY(desiredAngle, out rotationMatrix);
            Matrix.Multiply(ref rotationMatrix, -1, out rotationMatrix);

            // #4 - Apply the rototationMatrix to the new delta Vector change for this cycle.
            Vector3 rotatedVector;
            Vector3.Transform(ref vectorToAdd, ref rotationMatrix, out rotatedVector);

            // #5 - Finally, apply the rotatedVector to the Icon's position vector.
            Vector3.Add(ref _directionalIconPosition, ref rotatedVector, out _directionalIconPosition);
            DirectionalIconPosition = _directionalIconPosition;
        }

        // 6/6/2012
        /// <summary>
        /// Sets the Directional Icon's bound area to the size of the terrain map.
        /// </summary>
        private void SetDefaultMovementBoundArea()
        {
            _minBound.X = 0;
            _minBound.Z = 0;
            _maxBound.X = TerrainData.MapWidth;
            _maxBound.Z = TerrainData.MapHeight;
        }
        
    }
}
