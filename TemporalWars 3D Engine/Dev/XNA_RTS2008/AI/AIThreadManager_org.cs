#region File Description
//-----------------------------------------------------------------------------
// AIThreadManager.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;
using Spacewar.SceneItems;
using Spacewar.Utilities;

namespace Spacewar.AIManager
{
    public sealed class AIThreadManager : GameComponent
    {
        // List arrays for the Defense AI of SceneItems.
        private static volatile List<SceneItemWithPick> _sceneItems1 = new List<SceneItemWithPick>();
        private static volatile List<SceneItemWithPick> _sceneItems2 = new List<SceneItemWithPick>();
        private static volatile List<SceneItemWithPick> _sceneItems3 = new List<SceneItemWithPick>();

        // List arrays for the AstarItem AI update of SceneItems.
        private static volatile List<SceneItemWithPick> _aStarItems1 = new List<SceneItemWithPick>();
        private static volatile List<SceneItemWithPick> _aStarItems2 = new List<SceneItemWithPick>();
        private static volatile List<SceneItemWithPick> _aStarItems3 = new List<SceneItemWithPick>();
        

        // Enum of what AI Thread engine to use.  This is alternated
        // to each one for each new request!
        enum AIThreadEngine
        {
            Engine1 = 1,
            Engine2 = 2,
            Engine3 = 3
        }

        private static AIThreadEngine _aiThreadEngineToUse = AIThreadEngine.Engine1;
        private static AIThreadEngine _aiaStarItemsThreadEngineToUse = AIThreadEngine.Engine1;

        // Thread members - Defense AI
        private static Thread _aiDefenseThread1;
        private static Thread _aiDefenseThread2;
        private static Thread _aiDefenseThread3;
        private static volatile bool _isStopping;
        private static readonly object ListThreadLock1 = new object();
        private static readonly object ListThreadLock2 = new object();
        private static readonly object ListThreadLock3 = new object();

        // Thread members - AStarItems AI
        private static Thread _aiaStarItemThread1;
        private static Thread _aiaStarItemThread2;
        private static Thread _aiaStarItemThread3;        
        private static readonly object ListAStarThreadLock1 = new object();
        private static readonly object ListAStarThreadLock2 = new object();
        private static readonly object ListAStarThreadLock3 = new object();        

        // 7/17/2009 - AutoResetEvent Prims;
        //             To use, simply call 'Set()', which activates thread method, and 'WaitOne()' in thread,
        //             which makes thread go to sleep and wait for 'Set()' call!
        // Defense AI
        private static readonly AutoResetEvent AIDefenseThread1Start = new AutoResetEvent(false);
        private static readonly AutoResetEvent AIDefenseThread2Start = new AutoResetEvent(false);
        private static readonly AutoResetEvent AIDefenseThread3Start = new AutoResetEvent(false);
        private static readonly AutoResetEvent AIDefenseThread1End = new AutoResetEvent(false);// 7/20/2009
        private static readonly AutoResetEvent AIDefenseThread2End = new AutoResetEvent(false);// 7/20/2009
        private static readonly AutoResetEvent AIDefenseThread3End = new AutoResetEvent(false); // 7/20/2009
        // AStarItems AI
        private static readonly AutoResetEvent AIAStarItemThread1Start = new AutoResetEvent(false);
        private static readonly AutoResetEvent AIAStarItemThread2Start = new AutoResetEvent(false);
        private static readonly AutoResetEvent AIAStarItemThread3Start = new AutoResetEvent(false);
        private static readonly AutoResetEvent AIAStarItemThread1End = new AutoResetEvent(false); // 7/20/2009
        private static readonly AutoResetEvent AIAStarItemThread2End = new AutoResetEvent(false);// 7/20/2009
        private static readonly AutoResetEvent AIAStarItemThread3End = new AutoResetEvent(false); // 7/20/2009

        // 8/10/2009
        private static readonly Stopwatch TimerToSleep1 = new Stopwatch();
        private static readonly Stopwatch TimerToSleep2 = new Stopwatch();
        private static readonly Stopwatch TimerToSleep3 = new Stopwatch();
        private static readonly Stopwatch TimerToSleep4 = new Stopwatch();
        private static readonly Stopwatch TimerToSleep5 = new Stopwatch();
        private static readonly Stopwatch TimerToSleep6 = new Stopwatch();
        private static readonly TimeSpan TimerSleepMax = new TimeSpan(0, 0, 0, 0, 20);

        #region Properties

        // 6/1/2009 - 
        /// <summary>
        /// Is MP/SP game.
        /// </summary>
        public static bool IsNetworkGame { get; set; }

        // 8/12/2009 - Flag to have component sort lists upon next Update cycle.
        public static bool SortItemsByInUseFlag { get; set; }

        #endregion

        // contructor
        public AIThreadManager(Game game) : base(game)
        {
            // 11/7/2008 - StopWatchTimers           
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.AIDefenseThread1, false); // "AIDefense-Thread1"
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.AIDefenseThread2, false); // "AIDefense-Thread2"
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.AIDefenseThread3, false); // "AIDefense-Thread3"
            // 7/20/2009 - StopWatchTimers
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.AStarItemThread1, false); // "AStarItem-Thread1"
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.AStarItemThread2, false); // "AStarItem-Thread2"
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.AStarItemThread3, false); // "AStarItem-Thread3"

            { // Start Defense AI Threads

                // Start AI Defense Engine Thread 1
                _aiDefenseThread1 = new Thread(AIDefenseThreadMethod1)
                                        {
                                            Name = "AI Defense Engine Thread-1",
                                            IsBackground = true
                                        };
                _aiDefenseThread1.Start();

                // Start AI Defense Engine Thread 2
                _aiDefenseThread2 = new Thread(AIDefenseThreadMethod2)
                                        {
                                            Name = "AI Defense Engine Thread-2",
                                            IsBackground = true
                                        };
                _aiDefenseThread2.Start();

                // Start AI Defense Engine Thread 3
                _aiDefenseThread3 = new Thread(AIDefenseThreadMethod3)
                                        {
                                            Name = "AI Defense Engine Thread-3",
                                            IsBackground = true
                                        };
                _aiDefenseThread3.Start();
            }

            { // Start AStarItem AI Threads

                // Start AI Astar Engine Thread 1
                _aiaStarItemThread1 = new Thread(AIAStarItemThreadMethod1)
                                          {
                                              Name = "AI AStarItem Engine Thread-1",
                                              IsBackground = true
                                          };
                _aiaStarItemThread1.Start();

                // Start AI Astar Engine Thread 2
                _aiaStarItemThread2 = new Thread(AIAStarItemThreadMethod2)
                                          {
                                              Name = "AI AStarItem Engine Thread-2",
                                              IsBackground = true
                                          };
                _aiaStarItemThread2.Start();

                // Start AI Astar Engine Thread 3
                _aiaStarItemThread3 = new Thread(AIAStarItemThreadMethod3)
                                          {
                                              Name = "AI AStarItem Engine Thread-3",
                                              IsBackground = true
                                          };
                _aiaStarItemThread3.Start();

            }
        }       

           

        /// <summary>
        /// Add a SceneItemWithPick reference to one of the internal 3 List arrays.  
        /// </summary>
        public static void AddDefenseAI(SceneItemWithPick itemToAdd)
        {
            // 6/1/2009 - For MP games, ONLY the Host player will have the DefenseAI added and running.
            //            However, DefenseScenes are the excpetion!
            if (TemporalWars3DEngine.SPlayers[TemporalWars3DEngine.SThisPlayer].NetworkSession != null)            
            {
                 if (!(itemToAdd is DefenseScene) && !TemporalWars3DEngine.SPlayers[TemporalWars3DEngine.SThisPlayer].NetworkSession.IsHost)
                    return;
            }

            // Which Thread Engine to use for this request.
            switch (_aiThreadEngineToUse)
            {
                case AIThreadEngine.Engine1:
                    // Lock Critical section to make sure Queue is not updated while
                    // Enqueuing a new record.
                    lock (ListThreadLock1)
                    {
                        _sceneItems1.Add(itemToAdd);

                        // Sort list to have 'InUse' active nodes at top of list.
                        _sceneItems1.Sort(CompareByInUseFlag);
                    }
                    break;
                case AIThreadEngine.Engine2:
                    // Lock Critical section to make sure Queue is not updated while
                    // Enqueuing a new record.
                    lock (ListThreadLock2)
                    {
                        _sceneItems2.Add(itemToAdd);

                        // Sort list to have 'InUse' active nodes at top of list.
                        _sceneItems2.Sort(CompareByInUseFlag);
                    }
                    break;
                case AIThreadEngine.Engine3:
                    // Lock Critical section to make sure Queue is not updated while
                    // Enqueuing a new record.
                    lock (ListThreadLock3)
                    {
                        _sceneItems3.Add(itemToAdd);

                        // Sort list to have 'InUse' active nodes at top of list.
                        _sceneItems3.Sort(CompareByInUseFlag);
                    }
                    break;
                default:
                    break;
            }

            // Alternate Thread Engine to use for next request.
            _aiThreadEngineToUse++;
            if (_aiThreadEngineToUse > AIThreadEngine.Engine3)
            {                
                _aiThreadEngineToUse = AIThreadEngine.Engine1;
            }           

        }

        /// <summary>
        /// Add a SceneItemWithPick reference to one of the internal 3 List arrays.
        /// </summary>
        public static void AddAStarItemAI(SceneItemWithPick itemToAdd)
        {
            // 8/5/2009 - Debug test of one thread
            //_aiaStarItemsThreadEngineToUse = AIThreadEngine.Engine1;

            // Which Thread Engine to use for this request.
            switch (_aiaStarItemsThreadEngineToUse)
            {
                case AIThreadEngine.Engine1:
                    // Lock Critical section to make sure Queue is not updated while
                    // Enqueuing a new record.
                    lock (ListAStarThreadLock1)
                    {
                        _aStarItems1.Add(itemToAdd);

                        // Sort list to have 'InUse' active nodes at top of list.
                        _aStarItems1.Sort(CompareByInUseFlag);
                    }
                    break;
                case AIThreadEngine.Engine2:
                    // Lock Critical section to make sure Queue is not updated while
                    // Enqueuing a new record.
                    lock (ListAStarThreadLock2)
                    {
                        _aStarItems2.Add(itemToAdd);

                        // Sort list to have 'InUse' active nodes at top of list.
                        _aStarItems2.Sort(CompareByInUseFlag);
                    }
                    break;
                case AIThreadEngine.Engine3:
                    // Lock Critical section to make sure Queue is not updated while
                    // Enqueuing a new record.
                    lock (ListAStarThreadLock3)
                    {
                        _aStarItems3.Add(itemToAdd);

                        // Sort list to have 'InUse' active nodes at top of list.
                        _aStarItems3.Sort(CompareByInUseFlag);
                    }
                    break;
                default:
                    break;
            }

            // Alternate Thread Engine to use for next request.
            _aiaStarItemsThreadEngineToUse++;
            if (_aiaStarItemsThreadEngineToUse > AIThreadEngine.Engine3)            
                _aiaStarItemsThreadEngineToUse = AIThreadEngine.Engine1;
            

        }

        // 3/23/2009
        /// <summary>
        /// Sorts all internal List arrays by the 'InUse' flag, where True is sorted to the top of the list.
        /// </summary>       
        private static void DoSortItemsByInUseFlag()
        {
            // Sort list to have 'InUse' active nodes at top of list.
            _sceneItems1.Sort(CompareByInUseFlag);
            // Sort list to have 'InUse' active nodes at top of list.
            _sceneItems2.Sort(CompareByInUseFlag);
            // Sort list to have 'InUse' active nodes at top of list.
            _sceneItems3.Sort(CompareByInUseFlag);

            // Sort list to have 'InUse' active nodes at top of list.
            _aStarItems1.Sort(CompareByInUseFlag);
            // Sort list to have 'InUse' active nodes at top of list.
            _aStarItems2.Sort(CompareByInUseFlag);
            // Sort list to have 'InUse' active nodes at top of list.
            _aStarItems3.Sort(CompareByInUseFlag);
        }
       

        // 3/23/2009
        /// <summary>
        /// Predicate method used for the List<>.Sort() method.  This will sort all 
        /// items with the InUse flag set to True first.
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
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

        // 7/17/2009
        public override void Update(GameTime gameTime)
        {
            // 8/12/2009 - Check if sorting needed.  This is set by the PoolManager Events, for Get & Returning pool items.
            if (SortItemsByInUseFlag)
            {
                DoSortItemsByInUseFlag();
                SortItemsByInUseFlag = false;
            }

            base.Update(gameTime);
        }

        // 7/24/2009 - Flag used to update AIDefense thread every couple of frames.              
        private static int _processFrameCounter; 

        // 7/18/2009
        // Check if any AI Threads need to be woken up.
        public static void PumpUpdateThreads()
        { 
            // 7/24/2009 - Update every 2nd frame.
            _processFrameCounter++;
            if (_processFrameCounter > 1)
                _processFrameCounter = 0;

            // AI Defense Threads
            if (_sceneItems1.Count > 0 && _processFrameCounter == 1)
                AIDefenseThread1Start.Set();
            else
                AIDefenseThread1End.Set();

            if (_sceneItems2.Count > 0 && _processFrameCounter == 1)
                AIDefenseThread2Start.Set();
            else
                AIDefenseThread2End.Set();

            if (_sceneItems3.Count > 0 && _processFrameCounter == 1)
                AIDefenseThread3Start.Set();
            else
                AIDefenseThread3End.Set();


            // AStarItem Threads
            if (_aStarItems1.Count > 0)
                AIAStarItemThread1Start.Set();
            else
                AIAStarItemThread1End.Set();

            if (_aStarItems2.Count > 0)
                AIAStarItemThread2Start.Set();
            else
                AIAStarItemThread2End.Set();

            if (_aStarItems3.Count > 0)
                AIAStarItemThread3Start.Set();
            else
                AIAStarItemThread3End.Set();
            
        }

        // 7/20/2009
        // Waits for each Thread AutoEvent to signal its finsihed
        // working for the current frame.
        public static void WaitForThreadsToFinishCurrentFrame()
        {
            // Wait For ALL AI-Defense Threads to end current frame.
            const int millisecondsTimeout = 1000 / 20;
            AIDefenseThread1End.WaitOne(millisecondsTimeout, false);
            AIDefenseThread2End.WaitOne(millisecondsTimeout, false);
            AIDefenseThread3End.WaitOne(millisecondsTimeout, false);

            // Wait For ALL AI-AStar Threads to end current frame.
            AIAStarItemThread1End.WaitOne(millisecondsTimeout, false);
            AIAStarItemThread2End.WaitOne(millisecondsTimeout, false);
            AIAStarItemThread3End.WaitOne(millisecondsTimeout, false);
           
        }

        // 7/17/2009
        /// <summary>
        /// Clears out ALL internal arrays for both the 'AIDefense' & 'AIAstarItem' arrays.
        /// Called when exiting a level.
        /// </summary>
        public static void ClearAIArrays()
        {
            if (_sceneItems1 != null)
                _sceneItems1.Clear();
            if (_sceneItems2 != null)
                _sceneItems2.Clear();
            if (_sceneItems3 != null)
                _sceneItems3.Clear();

            if (_aStarItems1 != null)
                _aStarItems1.Clear();
            if (_aStarItems2 != null)
                _aStarItems2.Clear();
            if (_aStarItems3 != null)
                _aStarItems3.Clear();
        }

        // AI Defense thread method 1       
        private static void AIDefenseThreadMethod1()
        {
            // Set XBOX-360 CPU Core for thread            
#if XBOX360
            Thread.CurrentThread.SetProcessorAffinity(1);            
#endif
                       
            while (!_isStopping)
            {               
                // 7/17/2009 - Wait for Set() call to start.
                AIDefenseThread1Start.WaitOne();

                // 8/10/2009 - Start StopWatch timer
                TimerToSleep1.Reset();
                TimerToSleep1.Start();

                StopWatchTimers.StartStopWatchInstance(StopWatchName.AIDefenseThread1); // "AIDefense-Thread1"

                // Iterate through sceneitems and call update.
                var count = _sceneItems1.Count; // 8/15/2009
                for (int i = 0; i < count; i++)
                {
                    // 8/15/2009 - Cache
                    var sceneItemWithPick = _sceneItems1[i];

                    // if 'InUse' is False, then break out of loop. This
                    // is because the list is Sorted with the 'True' items
                    // at the top!
                    if (sceneItemWithPick.PoolItemWrapper != null)
                        if (!sceneItemWithPick.PoolItemWrapper.InUse)
                            break;

                    sceneItemWithPick.UpdateDefenseBehavior();

                    // 8/10/2009 - Sleep every few ms.
                    if (TimerToSleep1.Elapsed.TotalMilliseconds >= TimerSleepMax.Milliseconds)
                    {
                        Thread.Sleep(20);
                        TimerToSleep1.Reset();
                        TimerToSleep1.Start();
                    }
                    
                }

                StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.AIDefenseThread1);  //  "AIDefense-Thread1"                       

                // 7/20/2009 - Signal end of thread frame.
                AIDefenseThread1End.Set();

            } // End While

        }

        // AI Defense thread method 2      
        private static void AIDefenseThreadMethod2()
        {
            // Set XBOX-360 CPU Core for thread            
#if XBOX360
            Thread.CurrentThread.SetProcessorAffinity(3);            
#endif
            
            while (!_isStopping)
            {                
                // 7/17/2009 - Wait for Set() call to start.
                AIDefenseThread2Start.WaitOne();

                // 8/10/2009 - Start StopWatch timer
                TimerToSleep2.Reset();
                TimerToSleep2.Start();

                StopWatchTimers.StartStopWatchInstance(StopWatchName.AIDefenseThread2); // "AIDefense-Thread2"

                // Iterate through sceneitems and call update.
                var count = _sceneItems2.Count; // 8/15/2009
                for (int i = 0; i < count; i++)
                {
                    // 8/15/2009 - Cache
                    var sceneItemWithPick = _sceneItems2[i];

                    // if 'InUse' is False, then break out of loop. This
                    // is because the list is Sorted with the 'True' items
                    // at the top!
                    if (sceneItemWithPick.PoolItemWrapper != null)
                        if (!sceneItemWithPick.PoolItemWrapper.InUse)
                            break;

                    sceneItemWithPick.UpdateDefenseBehavior();

                    // 8/10/2009 - Sleep every few ms.
                    if (TimerToSleep2.Elapsed.TotalMilliseconds >= TimerSleepMax.Milliseconds)
                    {
                        Thread.Sleep(20);
                        TimerToSleep2.Reset();
                        TimerToSleep2.Start();
                    }  
                   
                }

                StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.AIDefenseThread2); // "AIDefense-Thread2"        
                

                // 7/20/2009 - Signal end of thread frame.
                AIDefenseThread2End.Set();

            } // End While

        }

        // AI Defense thread method 3       
        private static void AIDefenseThreadMethod3()
        {
            // Set XBOX-360 CPU Core for thread            
#if XBOX360
            Thread.CurrentThread.SetProcessorAffinity(5);            
#endif
           
            while (!_isStopping)
            {
               
                // 7/17/2009 - Wait for Set() call to start.
                AIDefenseThread3Start.WaitOne();

                // 8/10/2009 - Start StopWatch timer
                TimerToSleep3.Reset();
                TimerToSleep3.Start();

                StopWatchTimers.StartStopWatchInstance(StopWatchName.AIDefenseThread3); //"AIDefense-Thread3"

                // Iterate through sceneitems and call update.
                var count = _sceneItems3.Count; // 8/15/2009
                for (int i = 0; i < count; i++)
                {
                    // 8/15/2009 - Cache
                    var sceneItemWithPick = _sceneItems3[i];

                    // if 'InUse' is False, then break out of loop. This
                    // is because the list is Sorted with the 'True' items
                    // at the top!
                    if (sceneItemWithPick.PoolItemWrapper != null)
                        if (!sceneItemWithPick.PoolItemWrapper.InUse)
                            break;

                    sceneItemWithPick.UpdateDefenseBehavior();

                    // 8/10/2009 - Sleep every few ms.
                    if (TimerToSleep3.Elapsed.TotalMilliseconds >= TimerSleepMax.Milliseconds)
                    {
                        Thread.Sleep(20);
                        TimerToSleep3.Reset();
                        TimerToSleep3.Start();
                    }  
                    
                }

                StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.AIDefenseThread3); //"AIDefense-Thread3"

                // 7/20/2009 - Signal end of thread frame.
                AIDefenseThread3End.Set();
               
            } // End While

        }

        
        // AI AStarItem thread method 1       
        private static void AIAStarItemThreadMethod1()
        {
            // Set XBOX-360 CPU Core for thread            
#if XBOX360
            Thread.CurrentThread.SetProcessorAffinity(3);            
#endif

            while (!_isStopping)
            {               
                // 7/17/2009 - Wait for Set() call to start.
                AIAStarItemThread1Start.WaitOne();

                // 8/10/2009 - Start StopWatch timer
                TimerToSleep4.Reset();
                TimerToSleep4.Start();

                StopWatchTimers.StartStopWatchInstance(StopWatchName.AStarItemThread1); //"AStarItem-Thread1"

                // Iterate through AStarItems and call update.
                var count = _aStarItems1.Count; // 8/15/2009
                for (int i = 0; i < count; i++)
                {
                    // 8/15/2009 - Cache
                    var sceneItemWithPick = _aStarItems1[i];

                    // if 'InUse' is False, then break out of loop. This
                    // is because the list is Sorted with the 'True' items
                    // at the top!
                    if (sceneItemWithPick.PoolItemWrapper != null)
                        if (!sceneItemWithPick.PoolItemWrapper.InUse)
                            break;

                    if (sceneItemWithPick.AStarItemI != null)
                        sceneItemWithPick.AStarItemI.Update();

                    // 8/10/2009 - Sleep every few ms.
                    if (TimerToSleep4.Elapsed.TotalMilliseconds >= TimerSleepMax.Milliseconds)
                    {
                        Thread.Sleep(1);
                        TimerToSleep4.Reset();
                        TimerToSleep4.Start();
                    }  
                    
                }

                StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.AStarItemThread1); //"AStarItem-Thread1"

                // 7/20/2009 - Signal end of thread frame.
                AIAStarItemThread1End.Set();

            } // End While

        }

        // AI AStarItem thread method 2      
        private static void AIAStarItemThreadMethod2()
        {
            // Set XBOX-360 CPU Core for thread            
#if XBOX360
            Thread.CurrentThread.SetProcessorAffinity(4);            
#endif

            while (!_isStopping)
            {                
                // 7/17/2009 - Wait for Set() call to start.
                AIAStarItemThread2Start.WaitOne();

                // 8/10/2009 - Start StopWatch timer
                TimerToSleep5.Reset();
                TimerToSleep5.Start();

                StopWatchTimers.StartStopWatchInstance(StopWatchName.AStarItemThread2);//"AStarItem-Thread2"


                // Iterate through AStarItems and call update.
                var count = _aStarItems2.Count; // 8/15/2009
                for (int i = 0; i < count; i++)
                {
                    // 8/15/2009 - Cache
                    var sceneItemWithPick = _aStarItems2[i];

                    // if 'InUse' is False, then break out of loop. This
                    // is because the list is Sorted with the 'True' items
                    // at the top!
                    if (sceneItemWithPick.PoolItemWrapper != null)
                        if (!sceneItemWithPick.PoolItemWrapper.InUse)
                            break;

                    if (sceneItemWithPick.AStarItemI != null)
                        sceneItemWithPick.AStarItemI.Update();

                    // 8/10/2009 - Sleep every few ms.
                    if (TimerToSleep5.Elapsed.TotalMilliseconds >= TimerSleepMax.Milliseconds)
                    {
                        Thread.Sleep(1);
                        TimerToSleep5.Reset();
                        TimerToSleep5.Start();
                    }  
                   
                }

                StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.AStarItemThread2);//"AStarItem-Thread2"


                // 7/20/2009 - Signal end of thread frame.
                AIAStarItemThread2End.Set();

            } // End While

        }

        // AI AStarItem thread method 3       
        private static void AIAStarItemThreadMethod3()
        {
            // Set XBOX-360 CPU Core for thread            
#if XBOX360
            Thread.CurrentThread.SetProcessorAffinity(5);            
#endif

            while (!_isStopping)
            {                
                // 7/17/2009 - Wait for Set() call to start.
                AIAStarItemThread3Start.WaitOne();

                // 8/10/2009 - Start StopWatch timer
                TimerToSleep6.Reset();
                TimerToSleep6.Start();

                StopWatchTimers.StartStopWatchInstance(StopWatchName.AStarItemThread3);//"AStarItem-Thread3"

                // Iterate through AStarItems and call update.
                var count = _aStarItems3.Count; // 8/15/2009
                for (int i = 0; i < count; i++)
                {
                    // 8/15/2009 - Cache
                    var sceneItemWithPick = _aStarItems3[i];

                    // if 'InUse' is False, then break out of loop. This
                    // is because the list is Sorted with the 'True' items
                    // at the top!
                    if (sceneItemWithPick.PoolItemWrapper != null)
                        if (!sceneItemWithPick.PoolItemWrapper.InUse)
                            break;

                    if (sceneItemWithPick.AStarItemI != null)
                        sceneItemWithPick.AStarItemI.Update();

                    // 8/10/2009 - Sleep every few ms.
                    if (TimerToSleep6.Elapsed.TotalMilliseconds >= TimerSleepMax.Milliseconds)
                    {
                        Thread.Sleep(1);
                        TimerToSleep6.Reset();
                        TimerToSleep6.Start();
                    }     
                    
                }

                StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.AStarItemThread3);//"AStarItem-Thread3"

                // 7/20/2009 - Signal end of thread frame.
                AIAStarItemThread3End.Set();

            } // End While

        }

        // 7/17/2009
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this the final dispose?</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _isStopping = true;

                // let's shutdown our thread if it hasn't
                // shutdown already
                if (_aiDefenseThread1 != null)
                {
                    AIDefenseThread1Start.Set(); // 7/17/2009
                    _aiDefenseThread1.Join(); // wait for the thread to shutdown
                    _aiDefenseThread1.Abort(); // Terminate the Thread.
                    _aiDefenseThread1 = null;
                }

                if (_aiDefenseThread2 != null)
                {
                    AIDefenseThread2Start.Set(); // 7/17/2009
                    _aiDefenseThread2.Join(); // wait for the thread to shutdown
                    _aiDefenseThread2.Abort(); // Terminate the Thread.
                    _aiDefenseThread2 = null;
                }

                if (_aiDefenseThread3 != null)
                {
                    AIDefenseThread3Start.Set(); // 7/17/2009
                    _aiDefenseThread3.Join(); // wait for the thread to shutdown
                    _aiDefenseThread3.Abort(); // Terminate the Thread.
                    _aiDefenseThread3 = null;
                }

                if (_aiaStarItemThread1 != null)
                {
                    AIAStarItemThread1Start.Set(); // 7/17/2009
                    _aiaStarItemThread1.Join(); // wait for the thread to shutdown
                    _aiaStarItemThread1.Abort(); // Terminate the Thread.
                    _aiaStarItemThread1 = null;
                }

                if (_aiaStarItemThread2 != null)
                {
                    AIAStarItemThread2Start.Set(); // 7/17/2009
                    _aiaStarItemThread2.Join(); // wait for the thread to shutdown
                    _aiaStarItemThread2.Abort(); // Terminate the Thread.
                    _aiaStarItemThread2 = null;
                }

                if (_aiaStarItemThread3 != null)
                {
                    AIAStarItemThread3Start.Set(); // 7/17/2009
                    _aiaStarItemThread3.Join(); // wait for the thread to shutdown
                    _aiaStarItemThread3.Abort(); // Terminate the Thread.
                    _aiaStarItemThread3 = null;
                }

                // Dispose of Lists
                _sceneItems1.Clear();
                _sceneItems2.Clear();
                _sceneItems3.Clear();
                _aStarItems1.Clear();
                _aStarItems2.Clear();
                _aStarItems3.Clear();
            }

            base.Dispose(disposing);
        }     
       
        
    }
}
