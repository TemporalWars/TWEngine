using System;
using System.Diagnostics;
using System.Threading;

namespace ParallelTasksComponent
{
    /// <summary>
    /// The <see cref="AbstractParallelFor"/> abstract class, is ued to parallize the 'For-Loop', using 4 internal threads.
    /// </summary>
    public abstract class AbstractParallelFor
    {
        // 5/27/2010 - MyThreadPool
        private static readonly MyThreadPool MyThreadPool = new MyThreadPool();

        // 5/28/2010 - AutoResetEvents, used for the ThreadPool version only.
        private readonly AutoResetEvent _processorThreadEnd1 = new AutoResetEvent(false);
        private readonly AutoResetEvent _processorThreadEnd2 = new AutoResetEvent(false);
        private readonly AutoResetEvent _processorThreadEnd3 = new AutoResetEvent(false);
        private readonly AutoResetEvent _processorThreadEnd4 = new AutoResetEvent(false);

        private readonly object _dataThreadLock1 = new object();
        private readonly object _dataThreadLock2 = new object();
        private readonly object _dataThreadLock3 = new object();
        private readonly object _dataThreadLock4 = new object();
       
        // 2/17/2010 - ThreadPool Test
        private readonly WaitCallback _waitCallBack1;
        private readonly WaitCallback _waitCallBack2;
        private readonly WaitCallback _waitCallBack3;
        private readonly WaitCallback _waitCallBack4;
       

        // Loop Ranges for each thread.
        // Starts
        private volatile int _loopStart1;
        private volatile int _loopStart2;
        private volatile int _loopStart3;
        private volatile int _loopStart4;
        // Ends
        private volatile int _loopEnd1;
        private volatile int _loopEnd2;
        private volatile int _loopEnd3;
        private volatile int _loopEnd4;

        #region Prooperties

        // 5/28/2010
        /// <summary>
        /// The <see cref="AbstractParallelFor"/> class defaults to using the custom <see cref="MyThreadPool"/>, which is
        /// designed to eliminate garbage collection on the Xbox.  However, you can use the .Net Frameworks <see cref="ThreadPool"/> by
        /// setting this to TRUE.
        /// </summary>
        public bool UseDotNetThreadPool { get; set; }

        #endregion


        /// <summary>
        /// Constructor, which sets the internal <see cref="WaitCallback"/> delegates.
        /// </summary>
        protected AbstractParallelFor()
        {
            UseDotNetThreadPool = false;
            // 2/17/2010 - Set ThreadPool WaitCallBack delegates.
            _waitCallBack1 = WaitCallBack1;
            _waitCallBack2 = WaitCallBack2;
            _waitCallBack3 = WaitCallBack3;
            _waitCallBack4 = WaitCallBack4;
            
        }

        // 5/28/2010: Updated to now check if use 'UseDotNetThreadPool' attribute.
        // 5/24/2010: Updated to be STATIC method.
        /// <summary>
        /// Used to Parallize a For-Loop, to run on 4 separate processors.
        /// </summary>
        /// <param name="abstractParallelFor"></param>
        /// <param name="inclusiveLowerBound">Starting index of For-Loop</param>
        /// <param name="exclusiveUpperBound">Ending index of For-Loop</param>
        public static void ParallelFor(AbstractParallelFor abstractParallelFor, int inclusiveLowerBound, int exclusiveUpperBound)
        {
            const int numProcs = 4;  // could use 'Environment.ProcessorCount'
            var size = exclusiveUpperBound - inclusiveLowerBound;
            var range = size / numProcs;

            // 2/18/2010 - Check if Size given fits the processor range?
            if (size >= numProcs)
            {
                //
                // create For-Loop range for each Thread processor
                //
                // Thread#1
                abstractParallelFor._loopStart1 = 0 * range + inclusiveLowerBound;
                abstractParallelFor._loopEnd1 = abstractParallelFor._loopStart1 + range;
                // Thread#2
                abstractParallelFor._loopStart2 = 1 * range + inclusiveLowerBound;
                abstractParallelFor._loopEnd2 = abstractParallelFor._loopStart2 + range;
                // Thread#3
                abstractParallelFor._loopStart3 = 2 * range + inclusiveLowerBound;
                abstractParallelFor._loopEnd3 = abstractParallelFor._loopStart3 + range;
                // Thread#4
                abstractParallelFor._loopStart4 = 3 * range + inclusiveLowerBound;
                abstractParallelFor._loopEnd4 = exclusiveUpperBound;
               
                // 5/28/2010 - Check which ThreadPool to use.
                if (!abstractParallelFor.UseDotNetThreadPool)
                {
                    // 2/18/2010 - Test using MyThreadPool
                    MyThreadPool.QueueUserWorkItem(abstractParallelFor._waitCallBack1, 0);
                    MyThreadPool.QueueUserWorkItem(abstractParallelFor._waitCallBack2, 1);
                    MyThreadPool.QueueUserWorkItem(abstractParallelFor._waitCallBack3, 2);
                    MyThreadPool.QueueUserWorkItem(abstractParallelFor._waitCallBack4, 3);

                    MyThreadPool.WaitOne(0);
                    MyThreadPool.WaitOne(1);
                    MyThreadPool.WaitOne(2);
                    MyThreadPool.WaitOne(3);
                }
                else
                {
                    // 2/17/2010 - Use .Net ThreadPool
                    ThreadPool.QueueUserWorkItem(abstractParallelFor._waitCallBack1);
                    ThreadPool.QueueUserWorkItem(abstractParallelFor._waitCallBack2);
                    ThreadPool.QueueUserWorkItem(abstractParallelFor._waitCallBack3);
                    ThreadPool.QueueUserWorkItem(abstractParallelFor._waitCallBack4);

                    // Wait for all Thread Processors to complete.
                    abstractParallelFor._processorThreadEnd1.WaitOne();
                    abstractParallelFor._processorThreadEnd2.WaitOne();
                    abstractParallelFor._processorThreadEnd3.WaitOne();
                    abstractParallelFor._processorThreadEnd4.WaitOne();
                }

            }
            else if (size >= 2 && size < numProcs)
            {
                // recalc range
                range = size/2;

                // Create For-Loop ranges for Threads 1-2.
                // Thread#1
                abstractParallelFor._loopStart1 = 0 * range + inclusiveLowerBound;
                abstractParallelFor._loopEnd1 = abstractParallelFor._loopStart1 + range;
                // Thread#2
                abstractParallelFor._loopStart2 = 1 * range + inclusiveLowerBound;
                abstractParallelFor._loopEnd2 = exclusiveUpperBound;

                 // 5/28/2010 - Check which ThreadPool to use.
                if (!abstractParallelFor.UseDotNetThreadPool)
                {
                    // 2/18/2010 - Use MyThreadPool
                    MyThreadPool.QueueUserWorkItem(abstractParallelFor._waitCallBack1, 0);
                    MyThreadPool.QueueUserWorkItem(abstractParallelFor._waitCallBack2, 1);

                    MyThreadPool.WaitOne(0);
                    MyThreadPool.WaitOne(1);
                }
                else
                {
                    // 2/17/2010 - Use .Net ThreadPool
                    ThreadPool.QueueUserWorkItem(abstractParallelFor._waitCallBack1);
                    ThreadPool.QueueUserWorkItem(abstractParallelFor._waitCallBack2);

                    // Wait for all Thread Processors to complete.
                    abstractParallelFor._processorThreadEnd1.WaitOne();
                    abstractParallelFor._processorThreadEnd2.WaitOne();
                }
               
            }
            else if (size < 2)
            {
                // Then just process in one thread.

                // Thread#1
                abstractParallelFor._loopStart1 = inclusiveLowerBound;
                abstractParallelFor._loopEnd1 = exclusiveUpperBound;

                 // 5/28/2010 - Check which ThreadPool to use.
                if (!abstractParallelFor.UseDotNetThreadPool)
                {
                    // 2/18/2010 - Use MyThreadPool
                    MyThreadPool.QueueUserWorkItem(abstractParallelFor._waitCallBack1, 0);

                    // Wait for all Thread Processors to complete.
                    MyThreadPool.WaitOne(0);
                }
                else
                {
                    // 2/17/2010 - Use .Net ThreadPool
                    ThreadPool.QueueUserWorkItem(abstractParallelFor._waitCallBack1);

                    // Wait for all Thread Processors to complete.
                    abstractParallelFor._processorThreadEnd1.WaitOne();
                }

            } // End if Size fits processor range.
            
        }

        // 2/17/2010
        private void WaitCallBack1(object item)
        {
            ProcessorThreadDelegateMethod1(this);
        }

        // 2/17/2010
        private void WaitCallBack2(object item)
        {
            ProcessorThreadDelegateMethod2(this);
        }

        // 2/17/2010
        private void WaitCallBack3(object item)
        {
            ProcessorThreadDelegateMethod3(this);
        }

        // 2/17/2010
        private void WaitCallBack4(object item)
        {
            ProcessorThreadDelegateMethod4(this);
        }

        // 5/24/2010: Updated to be STATIC method.
        /// <summary>
        /// Core method run for each thread iteration, which iterates
        /// the given loop range, while calling the <see cref="LoopBody"/> delegate method. 
        /// </summary>
        /// <param name="abstractParallelFor">this instance of <see cref="AbstractParallelFor"/></param>
        private static void ProcessorThreadDelegateMethod1(AbstractParallelFor abstractParallelFor)
        {
            try
            {
                lock (abstractParallelFor._dataThreadLock1)
                {
                    // iterate given range and process 'Body'.
                    var loopStart1 = abstractParallelFor._loopStart1; // 6/2/2010
                    var loopEnd1 = abstractParallelFor._loopEnd1; // 6/2/2010
                    for (var i = loopStart1; i < loopEnd1; i++)
                    {
                        abstractParallelFor.LoopBody(i);
                    }
                }

                // 5/28/2010 - Use ONLY For .Net ThreadPool
                if (abstractParallelFor.UseDotNetThreadPool)
                    abstractParallelFor._processorThreadEnd1.Set(); // Signal end of thread frame.


            } // Can occur periodically when exiting a level.
            catch (ArgumentOutOfRangeException)
            {
                Debug.WriteLine("(ProcessorThreadDelegateMethod1) in AbstractParallelFor threw the 'ArgumentOutOfRangeException' error.");
                //throw;
            }
            catch (IndexOutOfRangeException)
            {
                Debug.WriteLine("(ProcessorThreadDelegateMethod1) in AbstractParallelFor threw the 'IndexOutOfRangeException' error.");
                //throw;
            }
        }

        // 5/24/2010: Updated to be STATIC method.
        /// <summary>
        /// Core method run for each thread iteration, which iterates
        /// the given loop range, while calling the <see cref="LoopBody"/> delegate method. 
        /// </summary>
        /// <param name="abstractParallelFor">this instance of <see cref="AbstractParallelFor"/></param>
        private static void ProcessorThreadDelegateMethod2(AbstractParallelFor abstractParallelFor)
        {
            try
            {
                lock (abstractParallelFor._dataThreadLock2)
                {
                    // iterate given range and process 'Body'.
                    var loopStart2 = abstractParallelFor._loopStart2; // 6/2/2010
                    var loopEnd2 = abstractParallelFor._loopEnd2; // 6/2/2010
                    for (var i = loopStart2; i < loopEnd2; i++)
                    {
                        abstractParallelFor.LoopBody(i);
                    }
                }

                // 5/28/2010 - Use ONLY For .Net ThreadPool
                if (abstractParallelFor.UseDotNetThreadPool)
                    abstractParallelFor._processorThreadEnd2.Set(); // Signal end of thread frame.

            } // Can occur periodically when exiting a level.
            catch (ArgumentOutOfRangeException)
            {
                Debug.WriteLine("(ProcessorThreadDelegateMethod2) in AbstractParallelFor threw the 'ArgumentOutOfRangeException' error.");
                //throw;
            }
            catch (IndexOutOfRangeException)
            {
                Debug.WriteLine("(ProcessorThreadDelegateMethod1) in AbstractParallelFor threw the 'IndexOutOfRangeException' error.");
                //throw;
            }
        }

        // 5/24/2010: Updated to be STATIC method.
        /// <summary>
        /// Core method run for each thread iteration, which iterates
        /// the given loop range, while calling the <see cref="LoopBody"/> delegate method. 
        /// </summary>
        /// <param name="abstractParallelFor">this instance of <see cref="AbstractParallelFor"/></param>
        private static void ProcessorThreadDelegateMethod3(AbstractParallelFor abstractParallelFor)
        {
            try
            {
                lock (abstractParallelFor._dataThreadLock3)
                {
                    // iterate given range and process 'Body'.
                    var loopStart3 = abstractParallelFor._loopStart3; // 6/2/2010
                    var loopEnd3 = abstractParallelFor._loopEnd3; // 6/2/2010
                    for (var i = loopStart3; i < loopEnd3; i++)
                    {
                        abstractParallelFor.LoopBody(i);
                    }
                }

                // 5/28/2010 - Use ONLY For .Net ThreadPool
                if (abstractParallelFor.UseDotNetThreadPool)
                    abstractParallelFor._processorThreadEnd3.Set(); // Signal end of thread frame.

            } // Can occur periodically when exiting a level.
            catch (ArgumentOutOfRangeException)
            {
                Debug.WriteLine("(ProcessorThreadDelegateMethod3) in AbstractParallelFor threw the 'ArgumentOutOfRangeException' error.");
                //throw;
            }
            catch (IndexOutOfRangeException)
            {
                Debug.WriteLine("(ProcessorThreadDelegateMethod1) in AbstractParallelFor threw the 'IndexOutOfRangeException' error.");
                //throw;
            }
        }

        // 5/24/2010: Updated to be STATIC method.
        /// <summary>
        /// Core method run for each thread iteration, which iterates
        /// the given loop range, while calling the <see cref="LoopBody"/> delegate method. 
        /// </summary>
        /// <param name="abstractParallelFor">this instance of <see cref="AbstractParallelFor"/></param>
        private static void ProcessorThreadDelegateMethod4(AbstractParallelFor abstractParallelFor)
        {
            try
            {
                lock (abstractParallelFor._dataThreadLock4)
                {
                    // iterate given range and process 'Body'.
                    var loopStart4 = abstractParallelFor._loopStart4; // 6/2/2010
                    var loopEnd4 = abstractParallelFor._loopEnd4; // 6/2/2010
                    for (var i = loopStart4; i < loopEnd4; i++)
                    {
                        abstractParallelFor.LoopBody(i);
                    }
                }

                // 5/28/2010 - Use ONLY For .Net ThreadPool
                if (abstractParallelFor.UseDotNetThreadPool)
                    abstractParallelFor._processorThreadEnd4.Set(); // Signal end of thread frame.

            } // Can occur periodically when exiting a level.
            catch (ArgumentOutOfRangeException)
            {
                Debug.WriteLine("(ProcessorThreadDelegateMethod4) in AbstractParallelFor threw the 'ArgumentOutOfRangeException' error.");
                //throw;
            }
            catch (IndexOutOfRangeException)
            {
                Debug.WriteLine("(ProcessorThreadDelegateMethod1) in AbstractParallelFor threw the 'IndexOutOfRangeException' error.");
                //throw;
            }
        }

        /// <summary>
        /// Core method for the Body of the For-Loop.  Inheriting classes
        /// MUST override and provide the core <see cref="LoopBody"/> to the For-Loop logic.
        /// </summary>
        /// <param name="index"></param>
        protected abstract void LoopBody(int index);
       

       
    }
}