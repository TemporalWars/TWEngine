#region File Description
//-----------------------------------------------------------------------------
// ParallelFor.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using TWEngine.ParallelTasks.Delegates;

namespace TWEngine.ParallelTasks
{
    /// <summary>
    /// The <see cref="ParallelFor{TDefault}"/> class, is used to parallize the 'For-Loop', using 4 internal threads.
    /// </summary>
    static class ParallelFor<T> 
    {
        // LoopBody to process each iteration
        private static volatile LoopBody<T> _loopBody;
        private static volatile IList<T> _loopArray;

        private static Thread _processorThread1;
        private static Thread _processorThread2;
        private static Thread _processorThread3;
        private static Thread _processorThread4;
        private static volatile bool _isStopping;
        private static readonly object DataThreadLock1 = new object();
        private static readonly object DataThreadLock2 = new object();
        private static readonly object DataThreadLock3 = new object();
        private static readonly object DataThreadLock4 = new object();

        // 2/15/2010 - AutoResetEvent Prims;
        //             To use, simply call 'Set()', which activates thread method, and 'WaitOne()' in thread,
        //             which makes thread go to sleep and wait for 'Set()' call!
        private static readonly AutoResetEvent ProcessorThreadStart1 = new AutoResetEvent(false);
        private static readonly AutoResetEvent ProcessorThreadStart2 = new AutoResetEvent(false);
        private static readonly AutoResetEvent ProcessorThreadStart3 = new AutoResetEvent(false);
        private static readonly AutoResetEvent ProcessorThreadStart4 = new AutoResetEvent(false);
        private static readonly AutoResetEvent ProcessorThreadEnd1 = new AutoResetEvent(false);
        private static readonly AutoResetEvent ProcessorThreadEnd2 = new AutoResetEvent(false);
        private static readonly AutoResetEvent ProcessorThreadEnd3 = new AutoResetEvent(false);
        private static readonly AutoResetEvent ProcessorThreadEnd4 = new AutoResetEvent(false);

        // Loop Ranges for each thread.
        private static volatile int _loopStart1;
        private static volatile int _loopStart2;
        private static volatile int _loopStart3;
        private static volatile int _loopStart4;
        private static volatile int _loopEnd1;
        private static volatile int _loopEnd2;
        private static volatile int _loopEnd3;
        private static volatile int _loopEnd4;
      

        // contructor
        /// <summary>
        /// Constructor, creating the 4 necessary <see cref="Thread"/> processors.
        /// </summary>
        static ParallelFor()
        {
            // Create and start the Thread Processors.
            _processorThread1 = new Thread(ProcessorThreadMethod1)
                                    {
                                        Name = "S-Parallel For-Loop Thread#1",
                                        IsBackground = true
                                    };
            _processorThread1.Start();

            _processorThread2 = new Thread(ProcessorThreadMethod2)
                                    {
                                        Name = "S-Parallel For-Loop Thread#2",
                                        IsBackground = true
                                    };
            _processorThread2.Start();

            _processorThread3 = new Thread(ProcessorThreadMethod3)
                                    {
                                        Name = "S-Parallel For-Loop Thread#3",
                                        IsBackground = true
                                    };
            _processorThread3.Start();

            _processorThread4 = new Thread(ProcessorThreadMethod4)
                                    {
                                        Name = "S-Parallel For-Loop Thread#4",
                                        IsBackground = true
                                    };
            _processorThread4.Start();
        }


        /// <summary>
        /// Used to parallize a 'For-Loop', to run on 4 separate processors.
        /// </summary>
        /// <param name="inclusiveLowerBound">Starting index of For-Loop</param>
        /// <param name="exclusiveUpperBound">Ending index of For-Loop</param>
        /// <param name="loopBody"><see cref="LoopBody{TDefault}"/> delegate method, which is required BODY of loop.</param>
        /// <param name="array">Collection to iterate</param>
        public static void For(int inclusiveLowerBound, int exclusiveUpperBound, LoopBody<T> loopBody, IList<T> array)
        {
            const int numProcs = 4;
            // could use 'Environment.ProcessorCount'; however, already know Xbox has 4 processors.
            var size = exclusiveUpperBound - inclusiveLowerBound;
            var range = size/numProcs;

            // 2/16/2010 - Assign delegate
            _loopBody = loopBody;
            // Assign Array to iterate
            _loopArray = array;

            //
            // create For-Loop range for each Thread processor
            //
            // Thread#1
            _loopStart1 = 0*range + inclusiveLowerBound;
            _loopEnd1 = _loopStart1 + range;
            // Thread#2
            _loopStart2 = 1*range + inclusiveLowerBound;
            _loopEnd2 = _loopStart2 + range;
            // Thread#3
            _loopStart3 = 2*range + inclusiveLowerBound;
            _loopEnd3 = _loopStart3 + range;
            // Thread#4
            _loopStart4 = 3*range + inclusiveLowerBound;
            _loopEnd4 = exclusiveUpperBound;

            // Trigger each Thread processor to start.
            ProcessorThreadStart1.Set();
            ProcessorThreadStart2.Set();
            ProcessorThreadStart3.Set();
            ProcessorThreadStart4.Set();

           
            // Wait for all Thread Processors to complete.
            ProcessorThreadEnd1.WaitOne();
            ProcessorThreadEnd2.WaitOne();
            ProcessorThreadEnd3.WaitOne();
            ProcessorThreadEnd4.WaitOne();
        }

        /// <summary>
        /// Main processor method for Thread#1.
        /// </summary>
        private static void ProcessorThreadMethod1()
        {
            // Set XBOX-360 CPU Core for thread            
#if XBOX360
            Thread.CurrentThread.SetProcessorAffinity(1);
#endif


            while (!_isStopping)
            {
                // Add Try-Catch construct to capture the 'System.InvalidOperationException' Exception,
                // thrown periodically when exiting the level!
                try
                {
                    // Wait for Set() call to start.
                    ProcessorThreadStart1.WaitOne();

                    // Call Thread Empty method, which will be overriden by the inheriting class.
                    ProcessorThreadDelegateMethod1();

                    // Signal end of thread frame.
                    ProcessorThreadEnd1.Set();

                }
#pragma warning disable 168
                catch (InvalidOperationException err)
#pragma warning restore 168
                {
                    // Simply end thread when this occurs; seems to only occur when exiting level.
                    Debug.WriteLine("ProcessorThreadMethod method (ThreadProcessor) Thread threw InvalidOpExp.");
                }
                catch (ThreadAbortException)
                {
                    // 2/16/2010 - Normal; Exception thrown when forcefully shut down threads!
                }

#if DEBUG
#pragma warning disable 168
                catch (Exception err)
#pragma warning restore 168
                {
                    Debug.WriteLine("ProcessorThreadMethod method (ThreadProcessor) Thread threw Exception.");
                    Debugger.Break();
                }
#endif

            } // End While


        }

        /// <summary>
        /// Main processor method for Thread#2.
        /// </summary>
        private static void ProcessorThreadMethod2()
        {
            // Set XBOX-360 CPU Core for thread            
#if XBOX360
            Thread.CurrentThread.SetProcessorAffinity(3);
#endif


            while (!_isStopping)
            {
                // Add Try-Catch construct to capture the 'System.InvalidOperationException' Exception,
                // thrown periodically when exiting the level!
                try
                {
                    // Wait for Set() call to start.
                    ProcessorThreadStart2.WaitOne();

                    // Call Thread Empty method, which will be overriden by the inheriting class.
                    ProcessorThreadDelegateMethod2();

                    // Signal end of thread frame.
                    ProcessorThreadEnd2.Set();

                }
#pragma warning disable 168
                catch (InvalidOperationException err)
#pragma warning restore 168
                {
                    // Simply end thread when this occurs; seems to only occur when exiting level.
                    //Thread.CurrentThread.Abort();
                    Debug.WriteLine("ProcessorThreadMethod method (ThreadProcessor) Thread threw InvalidOpExp.");
                }
                catch (ThreadAbortException)
                {
                    // 2/16/2010 - Normal; Exception thrown when forcefully shut down threads!
                }
#if DEBUG
#pragma warning disable 168
                catch (Exception err)
#pragma warning restore 168
                {
                    Debug.WriteLine("ProcessorThreadMethod method (ThreadProcessor) Thread threw Exception.");
                    Debugger.Break();
                }
#endif

            } // End While


        }

        /// <summary>
        /// Main processor method for Thread#3.
        /// </summary>
        private static void ProcessorThreadMethod3()
        {
            // Set XBOX-360 CPU Core for thread            
#if XBOX360
            Thread.CurrentThread.SetProcessorAffinity(4);
#endif


            while (!_isStopping)
            {
                // Add Try-Catch construct to capture the 'System.InvalidOperationException' Exception,
                // thrown periodically when exiting the level!
                try
                {
                    // Wait for Set() call to start.
                    ProcessorThreadStart3.WaitOne();

                    // Call Thread Empty method, which will be overriden by the inheriting class.
                    ProcessorThreadDelegateMethod3();

                    // Signal end of thread frame.
                    ProcessorThreadEnd3.Set();

                }
#pragma warning disable 168
                catch (InvalidOperationException err)
#pragma warning restore 168
                {
                    // Simply end thread when this occurs; seems to only occur when exiting level.
                    //Thread.CurrentThread.Abort();
                    Debug.WriteLine("ProcessorThreadMethod method (ThreadProcessor) Thread threw InvalidOpExp.");
                }
                catch (ThreadAbortException)
                {
                    // 2/16/2010 - Normal; Exception thrown when forcefully shut down threads!
                }
#if DEBUG
#pragma warning disable 168
                catch (Exception err)
#pragma warning restore 168
                {
                    Debug.WriteLine("ProcessorThreadMethod method (ThreadProcessor) Thread threw Exception.");
                    Debugger.Break();
                }
#endif

            } // End While


        }

        /// <summary>
        /// Main processor method for Thread#4.
        /// </summary>
        private static void ProcessorThreadMethod4()
        {
            // Set XBOX-360 CPU Core for thread            
#if XBOX360
            Thread.CurrentThread.SetProcessorAffinity(5);
#endif


            while (!_isStopping)
            {
                // Add Try-Catch construct to capture the 'System.InvalidOperationException' Exception,
                // thrown periodically when exiting the level!
                try
                {
                    // Wait for Set() call to start.
                    ProcessorThreadStart4.WaitOne();

                    // Call Thread Empty method, which will be overriden by the inheriting class.
                    ProcessorThreadDelegateMethod4();

                    // Signal end of thread frame.
                    ProcessorThreadEnd4.Set();

                }
#pragma warning disable 168
                catch (InvalidOperationException err)
#pragma warning restore 168
                {
                    // Simply end thread when this occurs; seems to only occur when exiting level.
                    //Thread.CurrentThread.Abort();
                    Debug.WriteLine("ProcessorThreadMethod method (ThreadProcessor) Thread threw InvalidOpExp.");
                }
                catch (ThreadAbortException)
                {
                    // 2/16/2010 - Normal; Exception thrown when forcefully shut down threads!
                }
#if DEBUG
#pragma warning disable 168
                catch (Exception err)
#pragma warning restore 168
                {
                    Debug.WriteLine("ProcessorThreadMethod method (ThreadProcessor) Thread threw Exception.");
                    Debugger.Break();
                }
#endif

            } // End While


        }


        /// <summary>
        /// Core method run for each thread iteration, which iterates
        /// the given loop range, while calling the 'Body' delegate method. 
        /// </summary>
        private static void ProcessorThreadDelegateMethod1()
        {
            try
            {
                lock (DataThreadLock1)
                {
                    // iterate given range and process 'Body'.
                    for (var i = _loopStart1; i < _loopEnd1; i++)
                    {
                        if (_loopBody != null)
                            _loopBody(_loopArray, i);
                    }
                }
            } // Can occur peridically when exiting a level.
            catch (ArgumentOutOfRangeException)
            {
                Debug.WriteLine("(ProcessorThreadDelegateMethod1) threw the 'ArgumentOutOfRangeException' error.");
            }
        }

        /// <summary>
        /// Core method run for each thread iteration, which iterates
        /// the given loop range, while calling the <see cref="LoopBody{TDefault}"/> delegate method. 
        /// </summary>
        private static void ProcessorThreadDelegateMethod2()
        {
            try
            {
                lock (DataThreadLock2)
                {
                    // iterate given range and process 'Body'.
                    for (var i = _loopStart2; i < _loopEnd2; i++)
                    {
                        if (_loopBody != null)
                            _loopBody(_loopArray, i);
                       
                    }
                }
            } // Can occur peridically when exiting a level.
            catch (ArgumentOutOfRangeException)
            {
                Debug.WriteLine("(ProcessorThreadDelegateMethod1) threw the 'ArgumentOutOfRangeException' error.");
            }
        }

        /// <summary>
        /// Core method run for each thread iteration, which iterates
        /// the given loop range, while calling the <see cref="LoopBody{TDefault}"/> delegate method. 
        /// </summary>
        private static void ProcessorThreadDelegateMethod3()
        {
            try
            {
                lock (DataThreadLock3)
                {
                    // iterate given range and process 'Body'.
                    for (var i = _loopStart3; i < _loopEnd3; i++)
                    {
                        if (_loopBody != null)
                            _loopBody(_loopArray, i);
                    }
                }
            } // Can occur peridically when exiting a level.
            catch (ArgumentOutOfRangeException)
            {
                Debug.WriteLine("(ProcessorThreadDelegateMethod1) threw the 'ArgumentOutOfRangeException' error.");
            }
        }

        /// <summary>
        /// Core method run for each thread iteration, which iterates
        /// the given loop range, while calling the <see cref="LoopBody{TDefault}"/> delegate method. 
        /// </summary>
        private static void ProcessorThreadDelegateMethod4()
        {
            try
            {
                lock (DataThreadLock4)
                {
                    // iterate given range and process 'Body'.
                    for (var i = _loopStart4; i < _loopEnd4; i++)
                    {
                        if (_loopBody != null)
                            _loopBody(_loopArray, i);
                    }
                }
            } // Can occur peridically when exiting a level.
            catch (ArgumentOutOfRangeException)
            {
                Debug.WriteLine("(ProcessorThreadDelegateMethod1) threw the 'ArgumentOutOfRangeException' error.");
            }
        }
       

        #region IDisposable Members

       
        // 2/15/2010
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        public static void Dispose()
        {

            // let's shutdown our thread if it hasn't
            // shutdown already
            _isStopping = true;
            // Thread#1
            if (_processorThread1 != null)
            {
                ProcessorThreadStart1.Set();
                _processorThread1.Join(); // wait for the thread to shutdown
                _processorThread1.Abort(); // Terminate the Thread.
                _processorThread1 = null;
            }
            // Thresd#2
            if (_processorThread2 != null)
            {
                ProcessorThreadStart2.Set();
                _processorThread2.Join(); // wait for the thread to shutdown
                _processorThread2.Abort(); // Terminate the Thread.
                _processorThread2 = null;
            }
            // Thresd#3
            if (_processorThread3 != null)
            {
                ProcessorThreadStart3.Set();
                _processorThread3.Join(); // wait for the thread to shutdown
                _processorThread3.Abort(); // Terminate the Thread.
                _processorThread3 = null;
            }
            // Thresd#4
            if (_processorThread4 != null)
            {
                ProcessorThreadStart4.Set();
                _processorThread4.Join(); // wait for the thread to shutdown
                _processorThread4.Abort(); // Terminate the Thread.
                _processorThread4 = null;
            }

        }

        #endregion
    }
}