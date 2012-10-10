#region File Description
//-----------------------------------------------------------------------------
// CohesionBehavior.cs
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
    /// The <see cref="CohesionBehavior"/> class produces a steering force which moves a <see cref="SceneItem"/>
    /// toward the center of mass of its neighbors.
    ///</summary>
    public sealed class CohesionBehavior : AbstractBehavior
    {
        SeekBehavior _seekBehavior;
        private static readonly Vector3 Vector3Zero = Vector3.Zero;

        // 8/4/2009
        /// <summary>
        /// Use Aircraft update?
        /// </summary>
        public bool UseAircraftUpdate { get; set; }
        

        ///<summary>
        /// Constructor, which creates an instance of the <see cref="SeekBehavior"/>.
        ///</summary>
        public CohesionBehavior()
            : base((int)BehaviorsEnum.Cohesion, 1.0f)
        {
            _seekBehavior = new SeekBehavior();

            // Default to Off, so user has to turn on in Properties Tool window
            //UseBehavior = false;
        }

        // 11/14/2008
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            // null refs
            _seekBehavior = null;

            base.Dispose();
        }

        /// <summary>
        /// Returns a steering force which attempts to move the agent towards the
        /// center of mass of the agents in its immediate area.
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
                throw new InvalidOperationException("CohesionAbstractBehavior can ONLY be used with SceneItemWithPick types.");
            }

            //first find the center of mass of all the agents
            DoUpdate(this, sceneItemWithPick, out force);
        }

        // 6/8/2010 - Stores references to Neighbor collections.
        private SceneItemWithPick[] _neighborsGround;
        private SceneItemWithPick[] _neighborsAir;

        // 6/8/2010
        /// <summary>
        /// Method helper for Update method.
        /// </summary>
        /// <param name="cohesionBehavior">Instance of <see cref="CohesionBehavior"/>.</param>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        /// <param name="force">(OUT) calculated force as <see cref="Vector3"/></param>
        private static void DoUpdate(CohesionBehavior cohesionBehavior, SceneItem item, out Vector3 force)
        {
            force = Vector3Zero;
            var centerOfMass = Vector3Zero; 
            var steeringForce = Vector3Zero;
            var neighborCount = 0;
            int keysCount; // 6/8/2010

            // 6/8/2010 - Get proper Neighbors list, depending on flag setting.
            SceneItemWithPick[] neighbors;
            if (cohesionBehavior.UseAircraftUpdate)
            {
                cohesionBehavior.ForceBehaviorManager.GetNeighborsAir(ref cohesionBehavior._neighborsAir);
                neighbors = cohesionBehavior._neighborsAir;
                keysCount = cohesionBehavior.ForceBehaviorManager.NeighborsAirKeysCount;
            }
            else
            {
                cohesionBehavior.ForceBehaviorManager.GetNeighborsGround(ref cohesionBehavior._neighborsGround);
                neighbors = cohesionBehavior._neighborsGround;
                keysCount = cohesionBehavior.ForceBehaviorManager.NeighborsGroundKeysCount;
            }

            if (neighbors == null) return;

            //iterate through the neighbors and sum up all the Position vectors
            for (var i = 0; i < keysCount; i++)
            {
                // 12/18/09 - Cache
                var neighbor = neighbors[i];
                if (neighbor == null) continue;

                // make sure *this* agent isn't included in the calculations
                // ***also make sure it doesn't include the evade target ***
                if ((neighbor == item) || (neighbor == cohesionBehavior.ForceBehaviorManager.TargetItem1)) continue;

                // 12/18/2008 - Updated to remove Overload Ops, since this slows down XBOX.
                //centerOfMass += ForceBehaviorManager.neighbors[i].Position;
                var tmpPos = neighbor.Position;
                Vector3.Add(ref centerOfMass, ref tmpPos, out centerOfMass);

                ++neighborCount;
            }

            if (neighborCount > 0)
            {
                // 12/18/2008 - Updated to remove Overload Ops, since this slows down XBOX.
                //the center of mass is the Average of the sum of positions
                //centerOfMass /= (float)neighborCount;
                Vector3.Divide(ref centerOfMass, neighborCount, out centerOfMass);

                //now _seekAbstract towards that Position
                cohesionBehavior._seekBehavior.Update(item, ref centerOfMass, out steeringForce);

                // The magnitude of cohesion is usually much larger than separation or
                // alignment so it usually helps to normalize it.
                if (!steeringForce.Equals(Vector3Zero)) steeringForce.Normalize(); // 8/5/2009: Avoid NaN errors, by not normalizing Zero values!

            }

            // We don't want any Force on the Y-axis
            steeringForce.Y = 0;

            force = steeringForce;
            
        }
    }
}
