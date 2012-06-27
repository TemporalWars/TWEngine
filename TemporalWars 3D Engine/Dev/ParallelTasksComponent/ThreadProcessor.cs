using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;
using ParallelTasksComponent.LocklessQueue;

namespace ParallelTasksComponent
{
    ///<summary>
    /// The <see cref="ThreadProcessor{TDefault}"/> generic abstract class, is used to automated the
    /// handling of generic item requests and processing.  The creation and processing of the <see cref="Thread"/> 
    /// are abstracted at this base level.  The inherting classes should then provide the final logic, which includes
    /// the generic type to handle, and the logic when processing this generic type. 
    ///</summary>
    ///<typeparam name="T">Generic type to handle</typeparam>
    public abstract class ThreadProcessor<T> : GameComponent
    {
        // 2/17/2010 - Thread MethodType to use.
        protected enum ThreadMethodTypeEnum
        {
            ManualPumping,
            AutoBlocking
        }

        private static ThreadMethodTypeEnum _threadMethodType;

        // 8/21/2009 - Thread members
        protected Thread ProcessorThread;
        protected volatile bool IsStopping;
        protected readonly object DataThreadLock = new object();
        protected static TimeSpan ElapsedGameTime = TimeSpan.Zero;
// ReSharper disable UnaccessedField.Local
        private readonly int _processorAffinityXbox = 3;
// ReSharper restore UnaccessedField.Local

        // 8/21/2009 - AutoResetEvent Prims;
        //             To use, simply call 'Set()', which activates thread method, and 'WaitOne()' in thread,
        //             which makes thread go to sleep and wait for 'Set()' call!
        private static readonly AutoResetEvent ProcessorThreadStart = new AutoResetEvent(false);
        private static readonly AutoResetEvent ProcessorThreadEnd = new AutoResetEvent(false);
        protected static readonly TimeSpan TimeSpanZero = TimeSpan.Zero;

        //protected static volatile List<T> ItemRequests = new List<T>(50);
        //protected static volatile List<T> ItemsToProcess = new List<T>(50);
        // 6/3/2010 - Test LocklessQueue
        protected static LocklessQueue<T> LocklessQueue = new LocklessQueue<T>();

        /// <summary>
        /// Constructor, which initializes the internal <see cref="Thread"/> for use.
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        /// <param name="threadName">Name of this <see cref="Thread"/></param>
        /// <param name="processorAffinityXbox">Xbox affinity to use</param>
        /// <param name="threadMethodType"><see cref="ThreadMethodTypeEnum"/> Enum</param>
        protected ThreadProcessor(Game game, string threadName, int processorAffinityXbox, ThreadMethodTypeEnum threadMethodType)
            : base(game)
        {
            // 2/17/2010 - Save _threadMethodType
            _threadMethodType = threadMethodType;

            // add to game components
            AddToGameComponents();

            // Start Processor Thread
            if (ProcessorThread != null) return;

            // set the threadProcessor# for the XBOX.
            _processorAffinityXbox = processorAffinityXbox;

            switch (threadMethodType)
            {
                case ThreadMethodTypeEnum.ManualPumping:
                    ProcessorThread = new Thread(ProcessorThreadMethod1)
                                          {
                                              Name = String.IsNullOrEmpty(threadName) ? "Processor Thread" : threadName,
                                              IsBackground = true
                                          };
                    ProcessorThread.Start();
                    break;
                case ThreadMethodTypeEnum.AutoBlocking:
                    ProcessorThread = new Thread(ProcessorThreadMethod2)
                                          {
                                              Name = String.IsNullOrEmpty(threadName) ? "Processor Thread" : threadName,
                                              IsBackground = true
                                          };
                    ProcessorThread.Start();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("threadMethodType");
            } // End Switch
            
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // Save GameTime
            lock (DataThreadLock)
            {
                ElapsedGameTime = gameTime.ElapsedGameTime;
            }            
        }

        /// <summary>
        /// Pumps the internal <see cref="Thread"/>, by waking it up.
        /// </summary>
        public void PumpUpdateThreads()
        {
            // 2/17/2010 - Return if set to AutoBlocking
            if (_threadMethodType == ThreadMethodTypeEnum.AutoBlocking)
                throw new InvalidOperationException("Calls to this method are only valid when _threadMethodType is set to 'ManualPumping'.");
            
            ProcessorThreadStart.Set();
            
        }

        /// <summary>
        /// Waits for the <see cref="Thread"/> to return the 'End' message.
        /// </summary>
        public void WaitForThreadsToFinishCurrentFrame()
        {
            // 2/17/2010 - Return if set to AutoBlocking
            if (_threadMethodType == ThreadMethodTypeEnum.AutoBlocking)
                throw new InvalidOperationException("Calls to this method are only valid when _threadMethodType is set to 'ManualPumping'.");

            // Wait For ALL Thread to end current frame.
            const int millisecondsTimeout = 1000 / 20;

            // xna 4.0 updates
            //ProcessorThreadEnd.WaitOne(millisecondsTimeout, false);
            ProcessorThreadEnd.WaitOne(millisecondsTimeout);   
        }

        /// <summary>
        /// Main processor method for <see cref="Thread"/>.
        /// </summary>
        /// <remarks>
        /// Version 1: This version will block itself every cycle, waiting for the UnBlock command via the <see cref="PumpUpdateThreads"/> call.
        /// </remarks>
        protected void ProcessorThreadMethod1()
        {
            // Set XBOX-360 CPU Core for thread            
#if XBOX360
            Thread.CurrentThread.SetProcessorAffinity(_processorAffinityXbox);
#endif

            // 5/24/2010 - Refactored out core code to new STATIC method.
            DoProcessorThreadMethod1(this);

        }

        // 5/24/2010 - 
        /// <summary>
        /// Method helper for version 1, which blocks itself every cycle, waiting for the UnBlock command via the <see cref="PumpUpdateThreads"/> call.
        /// </summary>
        /// <param name="threadProcessor">this instance of <see cref="ThreadProcessor{T}"/></param>
        private static void DoProcessorThreadMethod1(ThreadProcessor<T> threadProcessor)
        {
            while (!threadProcessor.IsStopping)
            {
                // Add Try-Catch construct to capture the 'System.InvalidOperationException' Exception,
                // thrown periodically when exiting the level!
                try
                {
                    // Wait for Set() call to start.
                    ProcessorThreadStart.WaitOne();

                    // Process all Requests
                    //threadProcessor.ProcessRequestItems();

                    // Call Thread Empty method, which will be overriden by the inheriting class.
                    threadProcessor.ProcessorThreadDelegateMethod();

                    // Signal end of thread frame.
                    ProcessorThreadEnd.Set();

                }
                /*catch (InvalidOperationException)
                {
                    // Simply end thread when this occurs; seems to only occur when exiting level.
                    //Thread.CurrentThread.Abort();
                    Debug.WriteLine("ProcessorThreadMethod method (ThreadProcessor) Thread threw InvalidOpExp.");
                }*/
                catch (ThreadAbortException) // 6/11/2010
                {
                    return;
                }
#if DEBUG
                /*catch (Exception)
                {
                    Debug.WriteLine("ProcessorThreadMethod method (ThreadProcessor) Thread threw Exception.");
                    Debugger.Break();
                }*/
#endif

            } // End While   
        }

        // 2/17/2010 - Version 2, which blocks itself ONLY when empty array.
        /// <summary>
        /// Main processor method for <see cref="Thread"/>.
        /// </summary>
        /// <remarks>
        ///  Version 2: This version will ONLY block itself when the internal collection
        /// is empty.  It is then woken up by the 'Add' action.
        /// </remarks>
        protected void ProcessorThreadMethod2()
        {
            // Set XBOX-360 CPU Core for thread            
#if XBOX360
            Thread.CurrentThread.SetProcessorAffinity(_processorAffinityXbox);
#endif


            // 5/24/2010 - Refactored out core code to new STATIC method.
            DoProcessorThreadMethod2(this);

        }

        // 5/24/2010
        /// <summary>
        /// Method helper for version 2, which ONLY blocks itself when the internal collection
        /// is empty.  It is then woken up by the 'Add' action.
        /// </summary>
        /// <param name="threadProcessor">this instance of <see cref="ThreadProcessor{T}"/></param>
        private static void DoProcessorThreadMethod2(ThreadProcessor<T> threadProcessor)
        {
            while (!threadProcessor.IsStopping)
            {
                // Add Try-Catch construct to capture the 'System.InvalidOperationException' Exception,
                // thrown periodically when exiting the level!
                try
                {
                    // Wait for Set() call to start.
                    ProcessorThreadStart.WaitOne();

                    // Process all Requests
                    //threadProcessor.ProcessRequestItems();

                    // Call Thread Empty method, which will be overriden by the inheriting class.
                    threadProcessor.ProcessorThreadDelegateMethod();
                   

                }
                /*catch (InvalidOperationException)

                {
                    // Simply end thread when this occurs; seems to only occur when exiting level.
                    //Thread.CurrentThread.Abort();
                    Debug.WriteLine("ProcessorThreadMethod method (ThreadProcessor) Thread threw InvalidOpExp.");
                }*/
                catch (ThreadAbortException) // 6/11/2010
                {
                    return;
                }
#if DEBUG
                /*catch (Exception)

                {
                    Debug.WriteLine("ProcessorThreadMethod method (ThreadProcessor) Thread threw Exception.");
                    Debugger.Break();
                }*/
#endif

            } // End While
        }

        // 2/17/2010
        /*/// <summary>
        /// Used to check if the current <see cref="Thread"/> can be put to sleep.
        /// </summary>
        /// <param name="threadStart"><see cref="AutoResetEvent"/> instance</param>
        protected virtual void CheckToBlock(AutoResetEvent threadStart)
        {
            // 2/17/2010: Block itself ONLY when empty arrays.
            //if (ItemRequests.Count != 0 || ItemsToProcess.Count != 0) return;

            // 5/24/2010: Updated to now put 'WaitOne' call into While loop, with 10 ms checks!
            while (!threadStart.WaitOne(10, false))
            {
                Thread.Sleep(5);
            }
        }*/

        // 2/17/2010
        /// <summary>
        /// Used to wake-up the current <see cref="Thread"/>.  Normally, this is automatically
        /// handled by the 'AddRequestItem'; however, if the inheriting class is
        /// not calling this method, you can manually wake up the thread instead.
        /// </summary>
        public static void WakeUpThread()
        {
            // 2/17/2010 - Wake-Up thread.
            if (_threadMethodType == ThreadMethodTypeEnum.AutoBlocking)
                ProcessorThreadStart.Set();
        }

        // 8/21/2009
        /*/// <summary>
        ///Processes the <see cref="ThreadProcessor{T}.ItemRequests"/> collection.
        /// </summary>
        protected virtual void ProcessRequestItems()
        {
            // 5/24/2010: Refactored out core code to new STATIC method.
            DoProcessRequestItems(DataThreadLock);
        }

        // 5/24/2010 - 
        /// <summary>
        /// Method helper, which processes the <see cref="ThreadProcessor{T}.ItemRequests"/> collection.
        /// </summary>
        /// <param name="dataThreadLock">Thread lock</param>
        private static void DoProcessRequestItems(object dataThreadLock)
        {
            lock (dataThreadLock)
            {
                // make sure not empty
                var count = ItemRequests.Count;
                if (count <= 0)
                    return;

                // iterate requests list
                for (var i = 0; i < count; i++)
                {
                    ItemsToProcess.Add(ItemRequests[i]);
                }
                ItemRequests.Clear();
            }
        }*/


        // 8/21/2009
        /// <summary>
        /// Adds an <paramref name="itemRequest"/> object to the internal collection.
        /// </summary>
        /// <param name="itemRequest"><paramref name="itemRequest"/> object to add</param>
        public virtual void AddItemRequest(T itemRequest)
        {
            // 5/24/2010: Refactored out core code to new STATIC method.
            DoAddItemRequest(itemRequest, DataThreadLock);
        }

        // 5/24/2010
        /// <summary>
        /// Method helper, which adds an <paramref name="itemRequest"/> object to the internal collection.
        /// </summary>
        /// <param name="itemRequest"><paramref name="itemRequest"/> object to add</param>
        /// <param name="dataThreadLock"></param>
        private static void DoAddItemRequest(T itemRequest, object dataThreadLock)
        {
            //lock (dataThreadLock)
            {
                // Add new item to internal request List
                //ItemRequests.Add(itemRequest);
                // 6/3/2010 - Add to LocklessQueue
                LocklessQueue.Enqueue(itemRequest);

                // Wake up
                WakeUpThread();
            }
        }


        /*/// <summary>
        /// Clears out all items out of memory.
        /// </summary>
        public virtual void ClearItemsToProcess()
        {
            lock (DataThreadLock)
            {
                ItemsToProcess.Clear();
            }
        }*/

        // 8/21/2009
        protected abstract void ProcessorThreadDelegateMethod();

        // 8/21/2009
        /// <summary>
        /// Adds the class to the <see cref="Game"/> components collection.
        /// </summary>
        private void AddToGameComponents()
        {
            Game.Components.Add(this);
        }

        // 8/21/2009
        /// <summary>
        /// Removes the class from the <see cref="Game"/> Components collection.
        /// </summary>
        private void RemoveFromGameComponents()
        {
            Game.Components.Remove(this);
        }


        /// <summary>
        /// Releases the unmanaged resources used by the GameComponent and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // remove itself from the game components
                RemoveFromGameComponents();

                // let's shutdown our thread if it hasn't
                // shutdown already
                IsStopping = true;
                if (ProcessorThread != null)
                {
                    ProcessorThreadStart.Set();
                    ProcessorThread.Abort(); // Terminate the Thread.
                    //ProcessorThread.Join(); // wait for the thread to shutdown
                    ProcessorThread = null;
                }

            }

            base.Dispose(disposing);
        }
    }
}