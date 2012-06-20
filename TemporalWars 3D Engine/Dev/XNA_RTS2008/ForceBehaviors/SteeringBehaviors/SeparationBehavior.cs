#region File Description
//-----------------------------------------------------------------------------
// SeparationBehavior.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using TWEngine.ForceBehaviors.Structs;
using TWEngine.SceneItems;

namespace TWEngine.ForceBehaviors.SteeringBehaviors
{
    ///<summary>
    /// The <see cref="SeparationBehavior"/> class creates a force that steers a <see cref="SceneItem"/> away
    /// from those in its neighborhood region.
    ///</summary>
    public sealed class SeparationBehavior : AbstractBehavior
    {
        private static readonly Vector3 Vector3Zero = Vector3.Zero; // 8/11/2009

        // 8/4/2009
        /// <summary>
        /// Use Aircraft update?
        /// </summary>
        public bool UseAircraftUpdate { get; set; }
      

        ///<summary>
        /// Constructor
        ///</summary>
        public SeparationBehavior()
            : base((int)Enums.BehaviorsEnum.Separation, 4.0f)
        {
            // Default to Off, so user has to turn on in Properties Tool window
            //UseBehavior = false;
        }

        /// <summary>
        /// This calculates a force repelling from the other neighbors.
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
                throw new InvalidOperationException("SeparationAbstractBehavior can ONLY be used with SceneItemWithPick types.");
            }

            // 5/16/2010 - refactored our code into STATIC method.
            DoUpdate(this, sceneItemWithPick, out force);
        }
        
        // 6/8/2010 - Stores references to Neighbor collections.
        private SceneItemWithPick[] _neighborsGround;
        private SceneItemWithPick[] _neighborsAir;

        // 5/16/2010
        /// <summary>
        /// Method helper, for Update method.
        /// </summary>
        /// <param name="separationBehavior">Instance of <see cref="SeparationBehavior"/></param>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        /// <param name="force">(OUT) calculated force as <see cref="Vector3"/></param>
        private static void DoUpdate(SeparationBehavior separationBehavior, SceneItem item, out Vector3 force)
        {
            force = Vector3Zero;
            var steeringForce = Vector3Zero;
            int keysCount; // 6/8/2010

            // 6/8/2010 - Get proper Neighbors list, depending on flag setting.
            SceneItemWithPick[] neighbors;
            if (separationBehavior.UseAircraftUpdate)
            {
                separationBehavior.ForceBehaviorManager.GetNeighborsAir(ref separationBehavior._neighborsAir);
                neighbors = separationBehavior._neighborsAir;
                keysCount = separationBehavior.ForceBehaviorManager.NeighborsAirKeysCount;
            }
            else
            {
                separationBehavior.ForceBehaviorManager.GetNeighborsGround(ref separationBehavior._neighborsGround);
                neighbors = separationBehavior._neighborsGround;
                keysCount = separationBehavior.ForceBehaviorManager.NeighborsGroundKeysCount;
            }

            if (neighbors == null) return;
            

            // Iterate through our neighbors list 
            var targetItem1 = separationBehavior.ForceBehaviorManager.TargetItem1; // 5/16/2010 - Cache
            for (var i = 0; i < keysCount; i++)
            {
                // 5/16/2010 - Cache
                var neighbor = neighbors[i];
                if (neighbor == null) continue;

                // make sure *this* agent isn't included in the calculations
                // ***also make sure it doesn't include the evade target ***
                if ((neighbor == item) || (neighbor == targetItem1)) continue;

                // 12/18/2008 - Updated to remove Overload Ops, since this slows down XBOX.
                //toAgent = SceneItemOwner.Position - ForceBehaviorManager.neighbors[i].Position;
                var itemPosition = item.Position; 
                var neighborsPosition = neighbor.Position;

                Vector3 toAgent;
                Vector3.Subtract(ref itemPosition, ref neighborsPosition, out toAgent);

                //scale the force inversely proportional to the agents distance  
                //from its neighbor.
                var length = toAgent.Length();
                if (length != 0) toAgent.Normalize(); // 8/4/2009: Avoid NaN with Zero check.

                // 12/18/2008 - Updated to remove Overload Ops, since this slows down XBOX.
                //steeringForce += toAgent / length;
                Vector3.Divide(ref toAgent, length, out toAgent);
                Vector3.Add(ref steeringForce, ref toAgent, out steeringForce);
            }

            // We don't want any Force on the Y-axis
            steeringForce.Y = 0;
            force = steeringForce;
        }
    }
}
