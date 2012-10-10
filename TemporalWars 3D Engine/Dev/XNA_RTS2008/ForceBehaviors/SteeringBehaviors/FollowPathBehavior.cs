#region File Description
//-----------------------------------------------------------------------------
// FollowPathBehavior.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.ForceBehaviors.Enums;
using ImageNexus.BenScharbach.TWEngine.ForceBehaviors.Structs;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWEngine.SceneItems.Enums;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.ForceBehaviors.SteeringBehaviors
{
    ///<summary>
    /// The <see cref="FollowPathBehavior"/> class is used to follow some given node path, for example 
    /// an A* solution path, using the steering behaviors <see cref="SeekBehavior"/> and <see cref="ArriveBehavior"/>.
    ///</summary>
    public sealed class FollowPathBehavior : AbstractBehavior
    {
        private SeekBehavior _seekBehavior;
        private ArriveBehavior _arriveBehavior;
        private static readonly Vector3 Vector3Zero = Vector3.Zero;

        ///<summary>
        /// Constructor, which creates instances for the <see cref="ArriveBehavior"/> and <see cref="SeekBehavior"/>.
        ///</summary>
        public FollowPathBehavior()
            : base((int)BehaviorsEnum.FollowPath, 0.05f)
        {
            _arriveBehavior = new ArriveBehavior();
            _seekBehavior = new SeekBehavior();
        }

        /// <summary>
        /// The <see cref="SceneItem"/> uses the <see cref="SeekBehavior"/> to move to the next waypoint - unless it is the last
        ///  waypoint, in which case it uses the <see cref="ArriveBehavior"/>.
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
                throw new InvalidOperationException("FollowPathAbstractBehavior can ONLY be used with SceneItemWithPick types.");
            }

            force = Vector3Zero;

            if (sceneItemWithPick.ItemState == ItemStates.Resting || sceneItemWithPick.ItemState == ItemStates.PausePathfinding)
                return;                       

            // Is Last Node
            var aStarItemI = sceneItemWithPick.AStarItemI; // 5/16/2010 - Cache
            if (aStarItemI != null)
                if (aStarItemI.SolutionFinal.Count > 1)
                {
                    _seekBehavior.Update(ref elapsedTime, item, out force);
                }
                else
                {
                    _arriveBehavior.Update(ref elapsedTime, item, out force);
                }


        }

        // 11/14/2008
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            // null refs
            _seekBehavior = null;
            _arriveBehavior = null;

            base.Dispose();
        }
    }
}
