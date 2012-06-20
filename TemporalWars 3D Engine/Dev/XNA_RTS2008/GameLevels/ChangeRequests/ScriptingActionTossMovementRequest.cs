#region File Description
//-----------------------------------------------------------------------------
// ScriptingActionTossMovementRequest.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using TWEngine.SceneItems;
using TWEngine.Terrain;

namespace TWEngine.GameLevels.ChangeRequests
{
    // 5/20/2012
    /// <summary>
    ///  The <see cref="ScriptingActionTossMovementRequest"/> class is used to update the Position of the given ItemType.
    /// </summary>
    public class ScriptingActionTossMovementRequest : ScriptingActionMovementRequestAbstract
    {
        // AbstractBehavior variables
        private readonly float _accuracyPercent;
        private readonly float _errorDistanceOffset;

        // 'THROW' attributes
        private float _upForce;
        private readonly float _objectWeight;
        private const float WeightFactor = 0.40f;

        // 6/6/2012
        /// <summary>
        /// Constructor to create a new position of a <see cref="SceneItem"/>
        /// </summary>
        /// <param name="sceneItem">Instance of <see cref="SceneItem"/></param>
        /// <param name="goalPosition"><see cref="Vector3"/> as goal position.</param>
        /// <param name="throwForce">Set to the 'Throw' force, used in the direction the <see cref="SceneItem"/> is thrown.</param>
        /// <param name="instancedItemPickedIndex">Index value to the correct scenaryItem instance.</param>
        /// <param name="accuracyPercent">Set to accuracy percent 0 - 100.</param>
        /// <param name="errorDistanceOffset">Maximum size of the error circle radius.</param>
        /// <param name="upForce">Set to the maxium 'UP' force.</param>
        /// <param name="objectWeight"> </param>
        public ScriptingActionTossMovementRequest(SceneItem sceneItem, Vector3 goalPosition, float throwForce, int instancedItemPickedIndex,
                                                  int accuracyPercent, float errorDistanceOffset, float upForce, float objectWeight)
            : this(sceneItem, throwForce, instancedItemPickedIndex, accuracyPercent, errorDistanceOffset, upForce, objectWeight)
        {
            if (objectWeight < 1)
            {
                throw new ArgumentOutOfRangeException("objectWeight", "Weight of object CANNOT be less than zero.");
            }

            _goalPosition = goalPosition;

        }

        /// <summary>
        /// Constructor to create a new position of a <see cref="SceneItem"/>
        /// </summary>
        /// <param name="sceneItem">Instance of <see cref="SceneItem"/></param>
        /// <param name="waypointGoalIndex">Waypoint goal index value.</param>
        /// <param name="throwForce">Set to the 'Throw' force, used in the direction the <see cref="SceneItem"/> is thrown.</param>
        /// <param name="instancedItemPickedIndex">Index value to the correct scenaryItem instance.</param>
        /// <param name="accuracyPercent">Set to accuracy percent 0 - 100.</param>
        /// <param name="errorDistanceOffset">Maximum size of the error circle radius.</param>
        /// <param name="upForce">Set to the maxium 'UP' force.</param>
        /// <param name="objectWeight"> </param>
        public ScriptingActionTossMovementRequest(SceneItem sceneItem, int waypointGoalIndex, float throwForce, int instancedItemPickedIndex, 
                                                  int accuracyPercent, float errorDistanceOffset, float upForce, float objectWeight) 
            : this(sceneItem, throwForce, instancedItemPickedIndex, accuracyPercent, errorDistanceOffset, upForce, objectWeight)
        {
            if (objectWeight < 1)
            {
                throw new ArgumentOutOfRangeException("objectWeight", "Weight of object CANNOT be less than zero.");
            }

            // get location for given waypoint index
            TerrainWaypoints.GetExistingWaypoint(waypointGoalIndex, out _goalPosition);
        }

        /// <summary>
        /// Constructor Helper, to create a new position of a <see cref="SceneItem"/>
        /// </summary>
        /// <param name="sceneItem">Instance of <see cref="SceneItem"/></param>
        /// <param name="throwForce">Set to the 'Throw' force, used in the direction the <see cref="SceneItem"/> is thrown.</param>
        /// <param name="instancedItemPickedIndex">Index value to the correct scenaryItem instance.</param>
        /// <param name="accuracyPercent">Set to accuracy percent 0 - 100.</param>
        /// <param name="errorDistanceOffset">Maximum size of the error circle radius.</param>
        /// <param name="upForce">Set to the maxium 'UP' force.</param>
        /// <param name="objectWeight"> </param>
        private ScriptingActionTossMovementRequest(SceneItem sceneItem, float throwForce, int instancedItemPickedIndex,
                                                  int accuracyPercent, float errorDistanceOffset, float upForce, float objectWeight)
            : base(sceneItem, instancedItemPickedIndex, throwForce)
        {
            if (objectWeight < 1)
            {
                throw new ArgumentOutOfRangeException("objectWeight", "Weight of object CANNOT be less than zero.");
            }

            // Clamp accuracy to 0 - 100.
            _accuracyPercent = MathHelper.Clamp(accuracyPercent, 0, 100);
            _errorDistanceOffset = errorDistanceOffset;

            // calculate accuracy of toss
            CreateAccuracyGoalPosition(_accuracyPercent, _errorDistanceOffset, out _goalPosition);

            // Get height at this position
            _goalPosition.Y = TerrainData.GetTerrainHeight(_goalPosition.X, _goalPosition.Z);

            // Zero Velocity
            sceneItem.Velocity = Vector3Zero;

            // Note: Throw force gets stored in the 'DeltaMag' at the base level.

            // Calc start 'throw' velocity
            _upForce = upForce;

            // Set start position
            var currentPosition = sceneItem.Position;
            _startPosition = new Vector3(currentPosition.X, 0, currentPosition.Z);

            // Set objects weight
            _objectWeight = objectWeight;

            // Turn on the Life-Span check
            UseLifeSpanCheck = true;
        }

        /// <summary>
        /// Updates the current change request.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
           base.Update(gameTime);
        }

        /// <summary>
        /// Helper method which should be overriden to update the 'Force' and provide a current velocity.
        /// </summary>
        /// <param name="scriptingActionMovementRequest">Instance of <see cref="ScriptingActionMovementRequestAbstract"/></param>
        /// <param name="gameTime">Instance of <see cref="GameTime"/></param>
        /// <param name="currentVelocity">(OUT) <see cref="Vector3"/> as current velocity</param>
        protected override void DoForceUpdateCheck(ScriptingActionMovementRequestAbstract scriptingActionMovementRequest, GameTime gameTime, out Vector3 currentVelocity)
        {
            currentVelocity = Vector3Zero;
            var sceneItemToUpdate = scriptingActionMovementRequest.SceneItemToUpdate;
            if (sceneItemToUpdate == null) return;

            // If ScenaryItem, then set to the proper index value.
            var scenaryItemScene = sceneItemToUpdate as ScenaryItemScene;
            if (scenaryItemScene != null)
            {
                scenaryItemScene.InstancedItemPickedIndex = scriptingActionMovementRequest.InstancedItemPickedIndex;
            }

            // Call calculate to get total Throw force
            UpdateVelocityWithThrowForce((ScriptingActionTossMovementRequest)scriptingActionMovementRequest, gameTime, out currentVelocity);
        }

        /// <summary>
        /// Updates the velocity using the throw force result.
        /// </summary>
        /// <param name="scriptingActionTossMovementRequest">Instance of THIS class.</param>
        /// <param name="gameTime">Instance of <see cref="GameTime"/></param>
        /// <param name="desiredForce">(OUT) updated desired force.</param>
        private static void UpdateVelocityWithThrowForce(ScriptingActionTossMovementRequest scriptingActionTossMovementRequest, GameTime gameTime, out Vector3 desiredForce)
        {
            var elapsedGameTime = gameTime.ElapsedGameTime;
            var friction = scriptingActionTossMovementRequest.ApplyGroundFriction ? GroundFriction : AirFriction;

            // Calculate 'MOVE' Force
            UpdateVelocityWithMoveForce(scriptingActionTossMovementRequest, gameTime, out desiredForce);

            // Calculate 'THROW' Force
            GetThrowForce(scriptingActionTossMovementRequest, gameTime, ref desiredForce);

            // Apply friction to DeltaMagnitude.
            var totalSeconds = (float) elapsedGameTime.TotalSeconds * TimeMultipler;
            var tmpFrictionResult = (1 - (friction * totalSeconds));
            scriptingActionTossMovementRequest.DeltaMagnitude *= tmpFrictionResult;
        }

        /// <summary>
        /// Gets the 'Throw' force.
        /// </summary>
        private static void GetThrowForce(ScriptingActionTossMovementRequest scriptingActionTossMovementRequest, GameTime gameTime, ref Vector3 force)
        {
            // apply gravity
            var elapsedGameTime = gameTime.ElapsedGameTime;
            var totalSeconds = (float)elapsedGameTime.TotalSeconds * TimeMultipler;
            scriptingActionTossMovementRequest._upForce -= (Gravity * (scriptingActionTossMovementRequest._objectWeight * WeightFactor)) * totalSeconds;
            // update vector
            force.Y = scriptingActionTossMovementRequest._upForce * totalSeconds;
        }

        /// <summary>
        /// Helper method which adjust the given <see cref="ScriptingActionMovementRequestAbstract.GoalPosition"/> to a 
        /// new position which adjustments for accuracy and error distance offset.
        /// </summary>
        /// <param name="accruacyPercent"></param>
        /// <param name="errorDistanceOffset"></param>
        /// <param name="newGoalPosition"></param>
        private void CreateAccuracyGoalPosition(float accruacyPercent, float errorDistanceOffset, out Vector3 newGoalPosition)
        {
            // Adj percent to 0.0 - 1.0f
            float accuracyPercentAdj = accruacyPercent / 100.0f;

            // 1st - Calculate some random angle between 0-360
            var randomGenerator = new Random();
            var randomAngle = randomGenerator.Next(0, 360);

            // 2nd - Calculate the outer perimeter position, using the errorDistance offset for radius and angle.
            // Apply Rotation Matrix to Vector direction.
            Vector3 goalPosition = _goalPosition;
            Vector3 outerCirclePosition;

            // Convert angle to radians for Cos/Sin function below.
            var desiredAngle = MathHelper.ToRadians(randomAngle);

            // Create direction of angle      
            var direction = new Vector3
            {
                X = (float)Math.Cos(desiredAngle),
                Y = 0,
                Z = (float)Math.Sin(desiredAngle),
            };

            Vector3.Multiply(ref direction, errorDistanceOffset, out direction);
            Vector3.Add(ref goalPosition, ref direction, out outerCirclePosition);

            // 3rd - Lerp a position between the goalPosition and output position using the percent accuracy.
            Vector3.Lerp(ref goalPosition, ref outerCirclePosition, 1 - accuracyPercentAdj, out newGoalPosition);
        }

    }
}