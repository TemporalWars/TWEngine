
#region File Description
//-----------------------------------------------------------------------------
// ForceBehaviorsParallelFor.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using System.Threading;
using ImageNexus.BenScharbach.TWEngine.ForceBehaviors;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWTools.ParallelTasksComponent;

namespace ImageNexus.BenScharbach.TWEngine.ParallelTasks
{
    /// <summary>
    /// The <see cref="ForceBehaviorsParallelFor"/> class, threads the calling of the <see cref="ForceBehaviorsCalculator"/> logic, 
    /// into 4 parallize processors.
    /// </summary>
    class ForceBehaviorsParallelFor : AbstractParallelFor
    {
        // List arrays for the ForceBehaviors
        private static readonly List<ForceBehaviorsCalculator> SteeringBehaviors = new List<ForceBehaviorsCalculator>();

         // 5/28/2010
        /// <summary>
        /// Constructor; can use either the custom <see cref="MyThreadPool"/>, or the .Net Framework <see cref="ThreadPool"/>.
        /// </summary>
        /// <param name="useDotNetThreadPool">Use the .Net Framework <see cref="ThreadPool"/>?</param>
        public ForceBehaviorsParallelFor(bool useDotNetThreadPool)
        {
            // save
            UseDotNetThreadPool = useDotNetThreadPool;
        }


        /// <summary>
        /// Used to parallize a 'For-Loop', to run on 4 separate processors.
        /// </summary>
        public void ParallelFor()
        {
            ParallelFor(this, 0, SteeringBehaviors.Count);
        }

        /// <summary>
        /// Add a <see cref="ForceBehaviorsCalculator"/> reference to one of the internal List array.  
        /// </summary>
        /// <param name="itemToAdd"><see cref="ForceBehaviorsCalculator"/> instance to add</param>
        public void AddItem(ForceBehaviorsCalculator itemToAdd)
        {
            // Enqueuing a new record.
            SteeringBehaviors.Add(itemToAdd);

            // Sort list to have 'InUse' active nodes at top of list.
            SteeringBehaviors.Sort(CompareByInUseFlag);
        }

        /// <summary>
        /// Clears out the internal collection, used when exiting a level.
        /// </summary>
        public void ClearArrays()
        {
            if (SteeringBehaviors != null)
                SteeringBehaviors.Clear();
        }

        /// <summary>
        /// Sorts all internal collections by the 'InUse' flag, where true is sorted to the top of the list.
        /// </summary>       
        public void DoSortItemsByInUseFlag()
        {
            // Sort list to have 'InUse' active nodes at top of list.
            if (SteeringBehaviors != null) SteeringBehaviors.Sort(CompareByInUseFlag);
        }

        /// <summary>
        /// Core method for the <see cref="LoopBody"/> of the 'For-Loop'.  Inheriting classes
        /// MUST override and provide the core <see cref="LoopBody"/> to the 'For-Loop' logic.
        /// </summary>
        /// <param name="index">'For-Loop' index value</param>
        protected override void LoopBody(int index)
        {
            // 5/24/2010: Refactored code to new STATIC method.
            DoLoopBody(index);
        }

        // 5/24/2010
        /// <summary>
        /// Helper method, which does the core 'For-Loop' logic.
        /// </summary>
        /// <param name="index">'For-Loop' index value</param>
        private static void DoLoopBody(int index)
        {
            // Cache
            var behaviors = SteeringBehaviors[index];
            if (behaviors == null) return;

            // if 'InUse' is False, then break out of loop. This
            // is because the list is Sorted with the 'True' items
            // at the top!
            var sceneItemOwner = behaviors.SceneItemOwner as SceneItemWithPick; // 5/20/2012
            if (sceneItemOwner != null)
            {
                if (sceneItemOwner.PoolItemWrapper != null)
                    if (!sceneItemOwner.PoolItemWrapper.InUse)
                        return;
            }

            // Update Behaviors
            behaviors.Calculate();
        }


        // 4/13/2009
        /// <summary>
        /// Predicate method used for the <see cref="List{TDefault}"/> sort method.  This will sort all 
        /// items with the 'InUse' flag set to true first.
        /// </summary>
        /// <param name="item1"><see cref="ForceBehaviorsCalculator"/> instance</param>
        /// <param name="item2"><see cref="ForceBehaviorsCalculator"/> instance</param>
        /// <returns>Sort value of -1, 0, or 1.</returns>
        private static int CompareByInUseFlag(ForceBehaviorsCalculator item1, ForceBehaviorsCalculator item2)
        {
            if (item1 == null)
            {
                return item2 == null ? 0 : -1;
            }
            // If item1 is not null...
            //
            if (item2 == null)
            {
                // ...and item2 is null, item1 is greater.
                return 1;
            }
            // ...and item2 is not null, compare the 
            // 'InUse' flags.

            // 5/20/2012 - check if SceneItemWithPick
            var sceneItemWithPick = item1.SceneItemOwner as SceneItemWithPick;
            if (sceneItemWithPick != null)
            {
                // 8/15/2009 - Cache
                var poolItemWrapper = sceneItemWithPick.PoolItemWrapper;
                var poolItemWrapper2 = sceneItemWithPick.PoolItemWrapper;

                if (poolItemWrapper == null || poolItemWrapper2 == null)
                    return 0;

                // item2 greater
                if (poolItemWrapper.InUse && !poolItemWrapper2.InUse)
                    return -1;

                // item1 greater
                if (!poolItemWrapper.InUse && poolItemWrapper2.InUse)
                    return 1;

                // items equal
                if (poolItemWrapper.InUse && poolItemWrapper2.InUse)
                    return 0;

                // items equal
                if (!poolItemWrapper.InUse && !poolItemWrapper2.InUse)
                    return 0; 
            }

            return 0;
        }
      
    }
}