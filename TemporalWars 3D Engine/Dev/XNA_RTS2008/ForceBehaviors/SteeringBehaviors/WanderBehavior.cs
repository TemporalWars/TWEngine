#region File Description
//-----------------------------------------------------------------------------
// WanderBehavior.cs
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
    /// the <see cref="WanderBehavior"/> class is used to create a steering force which will give the
    /// impression of a random walk through the agent's environment.
    ///</summary>
    public sealed class WanderBehavior : AbstractBehavior
    {      
        private Vector3 _wanderTarget;
        private Random _random = new Random();      

        //the radius of the constraining circle for the wander AbstractBehavior
        private float _wanderRadius = 5.2f;      
        //distance the wander circle is projected in front of the agent
        private float _wanderDistance;       
        //the maximum amount of displacement along the circle each frame
        private float _wanderJitter = 60.0f;
        private static readonly Vector3 Vector3Zero = Vector3.Zero; // 8/11/2009

        #region Properties

        /// <summary>
        /// The radius of the constraining circle for the <see cref="WanderBehavior"/>
        /// </summary>
        public float WanderRadius
        {
            get { return _wanderRadius; }
            set
            { 
                _wanderRadius = value;
                var theta = _random.NextDouble() * MathHelper.TwoPi;
                _wanderTarget = new Vector3((float)(_wanderRadius * Math.Cos(theta)), 0, (float)(_wanderRadius * Math.Sin(theta))); 
            }
        }

        /// <summary>
        /// Distance the wander circle is projected in front of the <see cref="SceneItem"/>
        /// </summary>
        public float WanderDistance
        {
            get { return _wanderDistance; }
            set { _wanderDistance = value; }
        }

        /// <summary>
        /// The maximum amount of displacement along the circle each frame
        /// </summary>
        public float WanderJitter
        {
            get { return _wanderJitter; }
            set { _wanderJitter = value; }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public WanderBehavior()
            : base((int)BehaviorsEnum.Wander, 1.0f)
        {
            
            //create a vector to a target Position on the wander circle  
            var theta = _random.NextDouble() * MathHelper.TwoPi;
            _wanderTarget = new Vector3((float)(_wanderRadius * Math.Cos(theta)), 0, (float)(_wanderRadius * Math.Sin(theta)));
           

        }

        /// <summary>
        /// This AbstractBehavior makes a <see cref="SceneItem"/> wander about randomly
        /// </summary>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> as elapsed game time.</param>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        /// <param name="force">(OUT) calculated force as <see cref="Vector3"/></param>
        public override void Update(ref BehaviorsTimeSpan elapsedTime, SceneItem item, out Vector3 force)
        {
            //this AbstractBehavior is dependent on the update rate, so this line must
            //be included when using Time independent framerate.
            DoUpdate(this, ref elapsedTime, item, out force);
        }

        // 5/17/2010; 6/12/2010: Updated to BehaviorsTimeSpan.
        /// <summary>
        /// Method helper, for the 'Update' method.
        /// </summary>
        /// <param name="wanderBehavior">Instance of <see cref="WanderBehavior"/>.</param>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> as elapsed game time.</param>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        /// <param name="force">(OUT) calculated force as <see cref="Vector3"/></param>
        private static void DoUpdate(WanderBehavior wanderBehavior, ref BehaviorsTimeSpan elapsedTime, SceneItem item, out Vector3 force)
        {

            var jitterThisTimeSlice = wanderBehavior._wanderJitter * (float)elapsedTime.TotalSeconds;

            // 12/18/2008 - Updated to remove Overload Ops, since this slows down XBOX.
            //first, add a small random vector to the target's Position
            var randomVector = new Vector3
                                   {
                                       X = (float) (wanderBehavior.RandomClamped()*jitterThisTimeSlice),
                                       Y = 0,
                                       Z = (float) (wanderBehavior.RandomClamped()*jitterThisTimeSlice)
                                   };
            //wanderTarget += randomVector;
            Vector3.Add(ref wanderBehavior._wanderTarget, ref randomVector, out wanderBehavior._wanderTarget);

            //reproject this new vector back on to a unit circle
            if (!wanderBehavior._wanderTarget.Equals(Vector3Zero)) wanderBehavior._wanderTarget.Normalize(); // 8/4/2009 - Avoid NaN errors, by avoiding Zero.

            // 12/18/2008 - Updated to remove Overload Ops, since this slows down XBOX.
            //increase the length of the vector to the same as the radius
            //of the wander circle
            //wanderTarget *= wanderRadius;
            Vector3.Multiply(ref wanderBehavior._wanderTarget, wanderBehavior._wanderRadius, out wanderBehavior._wanderTarget);

            // 12/18/2008 - Updated to remove Overload Ops, since this slows down XBOX.
            //move the target into a Position WanderDist in front of the agent
            var wanderDistV = Vector3Zero;
            wanderDistV.Z = wanderBehavior._wanderDistance;
            //target = wanderTarget + wanderDistV;

            Vector3 target;
            Vector3.Add(ref wanderBehavior._wanderTarget, ref wanderDistV, out target);
            
            // Using Orientation Matrix and current SceneItemOwner.Position, let's combine
            // the 'target' into 'Target'.
            var matTransform = item.ShapeItem.Orientation;
            matTransform.Translation = item.Position;
            var tempX = (matTransform.M11 * target.X) + (matTransform.M31 * target.Z) + (matTransform.M41);
            var tempZ = (matTransform.M13 * target.X) + (matTransform.M33 * target.Z) + (matTransform.M43);

            var finalTarget = new Vector3 { X = tempX, Y = 0 /* TerrainData.GetTerrainHeight(finalTarget.X, finalTarget.Z);*/, Z = tempZ };

            // 12/18/2008 - Updated to remove Overload Ops, since this slows down XBOX.
            //and steer towards it              
            //force = finalTarget - SceneItemOwner.Position;
            var tmpPos = item.Position;
            tmpPos.Y = 0;
            Vector3.Subtract(ref finalTarget, ref tmpPos, out force);
            
        }

        //returns a random double in the range -1 < n < 1
        private double RandomClamped()
        {
            return _random.NextDouble() - _random.NextDouble();
        }

        // 11/14/2008
        ///<summary>
        /// Release of unmanaged resources.
        ///</summary>
        public override void Dispose()
        {
            // null refs           
            _random = null;

            base.Dispose();
        }
    }
}
