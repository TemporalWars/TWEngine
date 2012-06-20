#region File Description
//-----------------------------------------------------------------------------
// ForceBehaviorsManager.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using PerfTimersComponent.Timers;
using PerfTimersComponent.Timers.Enums;
using TWEngine.ParallelTasks;

namespace TWEngine.ForceBehaviors
{
    // 5/22/2012 - Renamed from SteeringBehaviorsManager to ForceBehaviorsManager
    ///<summary>
    /// The <see cref="ForceBehaviorsManager"/> class is used to add each <see cref="ForceBehaviorsCalculator"/> into
    /// the internal <see cref="ForceBehaviorsParallelFor"/> collection; the collection is threaded to run
    /// the looping in a parallel mannor.
    ///</summary>
    public sealed class ForceBehaviorsManager : GameComponent
    {
        // 2/16/2010 - Instance of ForceBehavior ParallelFor
        private static ForceBehaviorsParallelFor _forceBehaviorParallelFor;

        // 8/12/2009 - 
        ///<summary>
        /// Flag to have component sort lists upon next Update cycle.
        ///</summary>
        public static bool SortItemsByInUseFlag { get; set; }

        // contructor
        /// <summary>
        /// Contructor, which creates an instance of the <see cref="ForceBehaviorsParallelFor"/> class.
        /// </summary>
        /// <param name="game">Instance of <see cref="game"/>.</param>
        public ForceBehaviorsManager(Game game) : base(game)
        {
#if DEBUG
            // 11/7/2008 - StopWatchTimers           
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.SteeringAIThread, false); // "SteeringAIThread1"
#endif

           // 2/16/2010
            _forceBehaviorParallelFor = new ForceBehaviorsParallelFor(TemporalWars3DEngine.UseDotNetThreadPool);
        }

        /// <summary>
        /// Add a <see cref="ForceBehaviorsCalculator"/> reference to the internal collection.  
        /// </summary>
        /// <param name="forceBehaviorToAdd"><see cref="ForceBehaviorsCalculator"/> to add</param>
        public static void Add(ForceBehaviorsCalculator forceBehaviorToAdd)
        {
            // 2/16/2010
            _forceBehaviorParallelFor.AddItem(forceBehaviorToAdd);
        }
        
        /// <summary>
        /// Sorts all internal List arrays by the 'InUse' flag, where True is sorted to the top of the list.
        /// </summary>       
        private static void DoSortItemsByInUseFlag()
        {
            // 2/16/2010
            _forceBehaviorParallelFor.DoSortItemsByInUseFlag();
        }
        
        ///<summary>
        /// Wake up threads to run for given update.  
        ///</summary>
        public static void PumpUpdateThreads()
        {
            // 5/29/2012 - Skip if game paused.
            if (TemporalWars3DEngine.GamePaused)
                return;

#if DEBUG
            // 2/16/2010 - Parallize For-Loop for AIDefense
            StopWatchTimers.StartStopWatchInstance(StopWatchName.SteeringAIThread); // "AIDefense-Thread1"
#endif

            _forceBehaviorParallelFor.ParallelFor();

#if DEBUG
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.SteeringAIThread);  //  "AIDefense-Thread1" 
#endif
        }
        
        ///<summary>
        /// Waits for each Thread AutoEvent to signal its finsihed
        /// working for the current frame.
        ///</summary>
        public static void WaitForThreadsToFinishCurrentFrame()
        {
            // Empty
        }
        
        /// <summary>
        /// Clears out all <see cref="ForceBehaviorsCalculator"/> in collection; called when exiting a level.
        /// </summary>
        public static void ClearSteeringBehaviorArrays()
        {
            // 2/16/2010
            _forceBehaviorParallelFor.ClearArrays();
        }

        // 8/12/2009
        /// <summary>
        /// Checks if sorting needs to be done for the internal lists.
        /// </summary>
        /// <remarks>This is set by the PoolManager events for 'Get' and 'Returning' pool items.</remarks>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public override void Update(GameTime gameTime)
        {
            // 5/29/2012 - Skip if game paused.
            if (TemporalWars3DEngine.GamePaused)
                return;

            // 8/12/2009 - Check if sorting needed.  This is set by the PoolManager Events, for Get and Returning pool items.
            if (SortItemsByInUseFlag)
            {
                DoSortItemsByInUseFlag();
                SortItemsByInUseFlag = false;
            }


            base.Update(gameTime);
        }
       
    }
}