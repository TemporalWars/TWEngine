using System;
using System.Diagnostics;
using System.Threading;

namespace ParallelTasksComponent
{
    /// <summary>
    /// Simple <see cref="ThreadPool"/>, for use in place of the XNA ThreadPool which creates
    /// garbage on the HEAP, and reduces XBOX-360 performance.
    /// </summary>
    public class MyThreadPool: IDisposable
    {
        // Pool of Thread processors.
        private const int ProcessorsToUse = 4; // 3/1/2010
        private static readonly MyThreadPoolItem[] ThreadProcessors = new MyThreadPoolItem[ProcessorsToUse];

        // XBOX Processors; Only 1,3,4 & 5 are avaiable.
        private enum XboxProcessor
        {
            Processor1,
// ReSharper disable UnusedMember.Local
            Processor2,
            Processor3,
            Processor4
// ReSharper restore UnusedMember.Local
        }

        // STATIC shared Xbox Affinity setting.
        private static XboxProcessor _xBoxProcessor = XboxProcessor.Processor1;
       
        // Constructor
        ///<summary>
        /// Constructor, which creates and starts the four internal threads.
        ///</summary>
        public MyThreadPool()
        {
           
            // Create the initial 4 Thread processors for the ThreadPool.
            for (var i = 0; i < ProcessorsToUse; i++)
            {
                // Create instance of MyThreadPoolItem
                var myThreadPoolItems = new MyThreadPoolItem(i, _xBoxProcessor);
                
                // Add to ThreadPool
                ThreadProcessors[i] = myThreadPoolItems;

                // Increase Affinity
                _xBoxProcessor++;
                if (_xBoxProcessor > XboxProcessor.Processor4)
                    _xBoxProcessor = XboxProcessor.Processor1;
            }
        }

        // 1/21/2011
        /// <summary>
        /// De-constructor, used to release the thread pool.
        /// </summary>
        ~MyThreadPool()
        {
            Dispose();
        }
        
        /// <summary>
        /// Sets a method for execution, attached to the specified <paramref name="threadProcessorToUse"/>.
        /// </summary>
        /// <param name="waitCallback">Method to execute within thread.</param>
        /// <param name="threadProcessorToUse">Assign thread number to use</param>
        /// <exception cref="ArgumentNullException">Thrown when <see cref="waitCallback"/> is null.</exception>
        public void QueueUserWorkItem(WaitCallback waitCallback, int threadProcessorToUse)
        {
            // check if null
            if (waitCallback == null)
                throw new ArgumentNullException("waitCallBack", @"Method to execute is null?!");

            // directly add to the given ThreaddProcessor
           MyThreadPoolItem.QueueUserWorkItem(ThreadProcessors[threadProcessorToUse], waitCallback);
           
        }

        // 2/18/2010
        /// <summary>
        /// Blocks until the WaitOne 'End' signal is received from
        /// given <see cref="Thread"/> processor number.
        /// </summary>
        /// <param name="threadProcessor"><see cref="Thread"/> number</param>
        public void WaitOne(int threadProcessor)
        {
            // Wait for End signal on given processor.
            const int millisecondsTimeout = 5; // 5/24/2010 - Updated to be only 5 ms intervals

            //ThreadProcessors[threadProcessor].ProcessorThreadEnd.WaitOne(millisecondsTimeout, false); 
            // XNA 4.0 Updates
            ThreadProcessors[threadProcessor].ProcessorThreadEnd.WaitOne(millisecondsTimeout); 
        }

        // 2/18/2010
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            for (var i = 0; i < ProcessorsToUse; i++)
            {
                var threadPoolItem = ThreadProcessors[i];

                // Dispose of ThreadPoolItem.
                if (threadPoolItem != null)
                    threadPoolItem.Dispose();
            }
           
        }

        // 2/18/2010 - MyThreadPoolItem class
        class MyThreadPoolItem : IDisposable
        {
            // Thread processor
            private readonly Thread _threadProcessor;
            private volatile bool _isStopping;
            private readonly AutoResetEvent _processorThreadStart = new AutoResetEvent(false);
            public readonly AutoResetEvent ProcessorThreadEnd = new AutoResetEvent(false);
#if XBOX
            private volatile int _xboxAffinity;
#endif

            // Delegate callBack item
            private WaitCallback _waitCallBack;

            // Lock
            private readonly object _lockObject = new object();
           

            // constructor
            /// <summary>
            /// Creates the internal Thread processor.
            /// </summary>
// ReSharper disable UnusedParameter.Local
            public MyThreadPoolItem(int threadNumber, XboxProcessor xBoxProcessor)
// ReSharper restore UnusedParameter.Local
            {
#if XBOX
                // Set XBOX Affinity
                switch (xBoxProcessor)
                {
                    case XboxProcessor.Processor1:
                        _xboxAffinity = 1;
                        break;
                    case XboxProcessor.Processor2:
                        _xboxAffinity = 3;
                        break;
                    case XboxProcessor.Processor3:
                        _xboxAffinity = 4;
                        break;
                    case XboxProcessor.Processor4:
                        _xboxAffinity = 5;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("xBoxProcessor");
                }
#endif

                // Create and start internal Thread processor
                // Create and start the Thread Processors.
                _threadProcessor = new Thread(ProcessorThreadMethod)
                {
                    Name = "ThreadPool processor#" + threadNumber,
                    IsBackground = true
                };
                _threadProcessor.Start();
            }

            // 5/24/2010 - Updated to be STATIC method.
            /// <summary>
            /// Sets a method for execution.  
            /// </summary>
            /// <param name="threadPoolItem">this instance of <see cref="MyThreadPoolItem"/></param>
            /// <param name="waitCallback">Method to execute within thread.</param>
            public static void QueueUserWorkItem(MyThreadPoolItem threadPoolItem, WaitCallback waitCallback)
            {
                // check if null
                if (waitCallback == null)
                    throw new ArgumentNullException("waitCallBack", @"Method to execute is null?!");

                lock (threadPoolItem._lockObject)
                {
                    // save delegate, and request thread to start.
                    threadPoolItem._waitCallBack = waitCallback;
                }
                threadPoolItem._processorThreadStart.Set(); // Send message to start processing.
            }

            // 5/24/2010
            /// <summary>
            /// Main processor method for <see cref="Thread"/>.
            /// </summary>
            private void ProcessorThreadMethod()
            {
                ProcessorThreadMethod(this); 
            }

            // 5/24/2010 - Updated to be STATIC method.
            /// <summary>
            /// Method helper, which is called by the <see cref="ProcessorThreadMethod()"/>.
            /// </summary>
            /// <param name="threadPoolItem">this instance of <see cref="MyThreadPoolItem"/></param>
            private static void ProcessorThreadMethod(MyThreadPoolItem threadPoolItem)
            {
                // Set XBOX-360 CPU Core for thread            
#if XBOX360
            Thread.CurrentThread.SetProcessorAffinity(threadPoolItem._xboxAffinity);
#endif


                while (!threadPoolItem._isStopping)
                {
                    // Add Try-Catch construct to capture the 'System.InvalidOperationException' Exception,
                    // thrown periodically when exiting the level!
                    try
                    {
                        // xna 4.0 updates
                        // 5/24/2010: Updated to now put 'WaitOne' call into While loop, with 5 ms checks!
                        // Wait for signal to start process
                        //while (!threadPoolItem._processorThreadStart.WaitOne(5, false))
                        while(!threadPoolItem._processorThreadStart.WaitOne(5))
                        {
                            // 7/2/2010 - Avoid deadlock.
                            if (threadPoolItem._isStopping) return;

                            Thread.Sleep(1);
                        }// 5/24/2010: Updated to 5 ms intervals.

                        lock (threadPoolItem._lockObject)
                        {
                            // Call Thread 'WaitCallBack' delegate, to begin work.
                            if (threadPoolItem._waitCallBack != null)
                                threadPoolItem._waitCallBack(null);
                            
                        }

                        // Signal 'Work' is complete.
                        threadPoolItem.ProcessorThreadEnd.Set();
                      

                    }
/*#pragma warning disable 168
                    catch (InvalidOperationException err)
#pragma warning restore 168
                    {
                        // Simply end thread when this occurs; seems to only occur when exiting level.
                        Debug.WriteLine("ProcessorThreadMethod method (MyThreadPool) Thread threw InvalidOpExp.");
                    }*/
                    catch (ThreadAbortException)
                    {
                        // 2/16/2010 - Normal; Exception thrown when forcefully shut down threads!
                        return;
                    }

#if DEBUG
/*#pragma warning disable 168
                    catch (Exception err)
#pragma warning restore 168
                    {
                        Debug.WriteLine("ProcessorThreadMethod method (MyThreadPool) Thread threw Exception.");
                        //Debugger.Break();
                    }*/
#endif

                } // End While

            }

            #region Implementation of IDisposable

            // Disposes of unmanaged resources.
            public void Dispose()
            {
                try
                {
                    // let's shutdown our thread if it hasn't
                    // shutdown already
                    _isStopping = true;
                    // Thread#1
                    if (_threadProcessor != null)
                    {
                        _processorThreadStart.Set();
                        _threadProcessor.Join(1000); // wait for the thread to shutdown
                        _threadProcessor.Abort(); // Terminate the Thread.
                    }
                }
                catch (ThreadAbortException)
                {
                    // Expected exception.
                }
                // 1/21/2011 - Thrown peridiocally on XBOX.
                catch (ObjectDisposedException) 
                {
                    // Expected exception.
                }
                
            }

            #endregion
        }

       
    }
}