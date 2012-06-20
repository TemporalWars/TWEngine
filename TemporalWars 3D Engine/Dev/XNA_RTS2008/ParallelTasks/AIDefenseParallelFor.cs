#region File Description
//-----------------------------------------------------------------------------
// AIDefenseParallelFor.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using ParallelTasksComponent;
using TWEngine.SceneItems;

namespace TWEngine.ParallelTasks
{
    /// <summary>
    /// The <see cref="AIDefenseParallelFor"/> class, threads the calling of the FSM_Machine logic, 
    /// for each <see cref="SceneItem"/>, into 4 parallize processors.
    /// </summary>
    class AIDefenseParallelFor : AbstractParallelFor
    {
        // List arrays for the Defense AI of SceneItems.
        private static readonly List<SceneItemWithPick> SceneItems = new List<SceneItemWithPick>();
        
        // 3/4/2011 - GameTime
        private volatile GameTime _gameTime;

        // 5/28/2010
        /// <summary>
        /// Constructor; can use either the custom <see cref="MyThreadPool"/>, or the .Net Framework <see cref="ThreadPool"/>.
        /// </summary>
        /// <param name="useDotNetThreadPool">Use the .Net Framework <see cref="ThreadPool"/>?</param>
        public AIDefenseParallelFor(bool useDotNetThreadPool)
        {
            // save
            UseDotNetThreadPool = useDotNetThreadPool;
        }

        /// <summary>
        /// Used to Parallize a For-Loop, to run on 4 separate processors.
        /// </summary>
        /// <param name="gameTime"></param>
        public void ParallelFor(GameTime gameTime)
        {
            // Store current gameTime
            _gameTime = gameTime;

            ParallelFor(this, 0, SceneItems.Count);
        }

        /// <summary>
        /// Add a <see cref="SceneItemWithPick"/> reference to one of the internal List array.  
        /// </summary>
        /// <param name="itemToAdd"><see cref="SceneItemWithPick"/> instance</param>
        public void AddSceneItem(SceneItemWithPick itemToAdd)
        {
            lock (SceneItems)
            {
                // Enqueuing a new record.
                SceneItems.Add(itemToAdd);

                // Sort list to have 'InUse' active nodes at top of list.
                SceneItems.Sort(CompareByInUseFlag);
            }

        }

        /// <summary>
        /// Clears out the internal collection, used when exiting a level.
        /// </summary>
        public void ClearAIArrays()
        {
            if (SceneItems != null)
                SceneItems.Clear();
        }

        /// <summary>
        /// Core method for the <see cref="LoopBody"/> of the 'For-Loop'.  Inheriting classes
        /// MUST override and provide the core  <see cref="LoopBody"/> to the 'For-Loop' logic.
        /// </summary>
        protected override void LoopBody(int index)
        {
            // 5/24/2010: Refactored out core code to new STATIC method.
            DoLoopBody(index, _gameTime);
        }

        // 5/24/2010; 3/4/2011 - Updated to include GameTime param.
        /// <summary>
        /// Helper method, which does the core 'For-Loop' logic.
        /// </summary>
        /// <param name="index">'For-Loop' index value</param>
        /// <param name="gameTime"></param>
        private static void DoLoopBody(int index, GameTime gameTime)
        {
            var sceneItemWithPick = SceneItems[index];
            if (sceneItemWithPick == null) return;

            // if 'InUse' is False, then break out of loop. This
            // is because the list is Sorted with the 'True' items
            // at the top!
            if (sceneItemWithPick.PoolItemWrapper != null)
                if (!sceneItemWithPick.PoolItemWrapper.InUse)
                    return;

            // Update AI
            sceneItemWithPick.UpdateDefenseBehavior(gameTime);
        }

        // 3/23/2009
        /// <summary>
        /// Predicate method used for the <see cref="List{TDefault}"/> sort method.  This will sort all 
        /// items with the 'InUse' flag set to true first.
        /// </summary>
        /// <param name="item1"><see cref="SceneItemWithPick"/> instance</param>
        /// <param name="item2"><see cref="SceneItemWithPick"/> instance</param>
        /// <returns></returns>
        private static int CompareByInUseFlag(SceneItemWithPick item1, SceneItemWithPick item2)
        {
            if (item1 == null)
            {
                if (item2 == null)
                {
                    // If item1 is null and item2 is null, they're
                    // equal. 
                    return 0;
                }

                // If item1 is null and item2 is not null, item2
                // is greater. 
                return -1;
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


            // 8/15/2009 - Cache
            var poolItemWrapper = item1.PoolItemWrapper;
            var poolItemWrapper2 = item2.PoolItemWrapper;

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

            return 0;
        }
      
    }
}