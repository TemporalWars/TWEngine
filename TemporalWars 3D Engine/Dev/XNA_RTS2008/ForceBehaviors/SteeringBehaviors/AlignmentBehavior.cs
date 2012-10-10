#region File Description
//-----------------------------------------------------------------------------
// AlignmentBehavior.cs
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
    /// The <see cref="AlignmentBehavior"/> attempts to keep a <see cref="SceneItem"/> heading aligned with its neighbors.
    ///</summary>
    public sealed class AlignmentBehavior : AbstractBehavior
    {
        private static readonly Vector3 Vector3Zero = Vector3.Zero; // 8/11/2009


        ///<summary>
        /// Constructor, which simply sets the <see cref="AbstractBehavior.UseBehavior"/> property to false; this
        /// forces this to be turn on from the Properites Tool window.
        ///</summary>
        public AlignmentBehavior()
            : base((int)BehaviorsEnum.Alignment, 1.0f)
        {
            // Default to Off, so user has to turn on in Properties Tool window
            UseBehavior = false;
        }

        /// <summary>
        ///  Using SceneItemOwner's Move-To-Position, this AbstractBehavior returns a steering force which will
        ///  direct the agent towards the target.
        /// </summary>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> as elapsed game time.</param>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        /// <param name="force">(OUT) calculated force as <see cref="Vector3"/></param>
        public override void Update(ref BehaviorsTimeSpan elapsedTime, SceneItem item, out Vector3 force)
        {
            // 5/20/2012 - Throw expection if not SceneItemWithPick.
            var sceneItemWithPick = (SceneItemWithPick) item;
            if (sceneItemWithPick == null)
            {
                throw new InvalidOperationException("AlignmentAbstractBehavior can ONLY be used with SceneItemWithPick types.");
            }

            // used to record the Average Heading of the neighbors
            DoUpdate(this, sceneItemWithPick, out force);
        }

        // 6/8/2010 - Stores references to Neighbor collections.
        private SceneItemWithPick[] _neighborsGround;

        // 6/8/2010
        /// <summary>
        /// Method helper, for the Update method.
        /// </summary>
        /// <param name="alignBehavior">Instance of <see cref="AlignmentBehavior"/>.</param>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        /// <param name="force">(OUT) calculated force as <see cref="Vector3"/></param>
        private static void DoUpdate(AlignmentBehavior alignBehavior, SceneItemWithPick item, out Vector3 force)
        {
            var averageHeading = Vector3Zero;
            force = default(Vector3); // 5/16/2010

            // used to Count the number of vehicles in the neighborhood
            var neighborCount = 0;

            // 5/16/2010 - Cache
            alignBehavior.ForceBehaviorManager.GetNeighborsGround(ref alignBehavior._neighborsGround);
            if (alignBehavior._neighborsGround == null) return;

            // 6/8/2010 - Retrieve Keys count.
            var keysCount = alignBehavior.ForceBehaviorManager.NeighborsGroundKeysCount;
            

            // iterate through all neighbors and sum their Heading vectors
            for (var i = 0; i < keysCount; i++)
            {
                // 5/16/2010 - Cache
                var neighbor = alignBehavior._neighborsGround[i];
                if (neighbor == null) continue;

                // make sure *this* agent isn't included in the calculations
                // *** also make sure it doesn't include the evade target ***
                if ((neighbor == item) ||
                    (neighbor == alignBehavior.ForceBehaviorManager.TargetItem1)) continue;

                // 12/18/2008 - Updated to remove Overload Ops, since this slows down XBOX.
                //averageHeading += ForceBehaviorManager.neighbors[i].aStarItem.Heading;
                Vector3.Add(ref averageHeading, ref neighbor.AStarItemI.Heading, out averageHeading);

                ++neighborCount;
            }

            // if the neighborhood contained one or more vehicles, Average their
            // Heading vectors.
            if (neighborCount > 0)
            {
                // 12/18/2008 - Updated to remove Overload Ops, since this slows down XBOX.
                //averageHeading /= (float)neighborCount;
                Vector3.Divide(ref averageHeading, neighborCount, out averageHeading);

                //averageHeading -= SceneItemOwner.aStarItem.Heading;
                Vector3.Subtract(ref averageHeading, ref item.AStarItemI.Heading, out averageHeading); 
            }

            // We don't want any Force on the Y-axis
            averageHeading.Y = 0;

            force = averageHeading;
        }
    }
}
