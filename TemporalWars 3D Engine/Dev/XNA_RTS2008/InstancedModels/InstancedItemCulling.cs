#region File Description
//-----------------------------------------------------------------------------
// InstancedItemCulling.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ImageNexus.BenScharbach.TWEngine.GameCamera;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWTools.PerfTimersComponent.Timers;
using ImageNexus.BenScharbach.TWTools.PerfTimersComponent.Timers.Enums;

namespace ImageNexus.BenScharbach.TWEngine.InstancedModels
{
    // 2/8/2010
    /// <summary>
    /// Updates the InstancedItem models, by culling out items which are not within
    /// the camera's view fustrum.  (Threaded)
    /// </summary>
    class InstancedItemCulling
    {
        private static Thread _cameraCullingThread;
        private static volatile bool _cameraCullingIsStopping;
        private static readonly AutoResetEvent CameraCullingThreadStart = new AutoResetEvent(false);
        private static readonly AutoResetEvent CameraCullingThreadEnd = new AutoResetEvent(false);

        // StopWatch timer, used to keep the processing to a max time per cycle!
        private static readonly Stopwatch TimerToSleep = new Stopwatch();
        private static readonly TimeSpan TimerSleepMax = new TimeSpan(0, 0, 0, 0, 16);

        // 2/8/2010 - Constructor
        /// <summary>
        /// Creates the Culling Thread, and starts it in the background.
        /// </summary>
        public InstancedItemCulling()
        {
            // 8/6/2009 - Init the CameraCulling Thread
            if (_cameraCullingThread != null) return;

            // Start ForceBehavior Engine Thread 1
            _cameraCullingThread = new Thread(CameraCullingThreadMethod)
                                       {
                                           Name = "CameraCulling Thread",
                                           IsBackground = true
                                          
                                       };
            _cameraCullingThread.Start();
        }

        /// <summary>
        /// Used with the Double-Bufering technique, which is called once per
        /// frame, to pump the internal thread.
        /// </summary>
        public static void PumpUpdateThreads()
        {
            /*if (InstancedModelChangeRequests.ChangeRequestItemTypeKeys.Count > 0)
                CameraCullingThreadStart.Set();
            else
                CameraCullingThreadEnd.Set();*/
        }

        /// <summary>
        /// EventHandler triggered once the current Pumping is complete for this frame.
        /// </summary>
        public static void WaitForThreadsToFinishCurrentFrame()
        {
            // Wait For ChangeRequest Thread to end current frame.
            /*const int millisecondsTimeout = 1000 / 20;
            CameraCullingThreadEnd.WaitOne(millisecondsTimeout, false);*/
        }

        /// <summary>
        /// Instanced Item's Culling Thread method, used to specifically
        /// run in the background, and call the 'UpdateInstanceTransformsForCulling' method.
        /// </summary>
        private static void CameraCullingThreadMethod()
        {
            // Set XBOX-360 CPU Core for thread            
#if XBOX360
            Thread.CurrentThread.SetProcessorAffinity(4);
#endif

            while (!_cameraCullingIsStopping)
            {
                // Wait for Set() call to start.
                CameraCullingThreadStart.WaitOne();

                StopWatchTimers.StartStopWatchInstance(StopWatchName.InstancedItemCameraCulling); //"InstancedItem_CameraCulling"

                //if (_updateCameraEffectParams)
                {
                    // 7/25/2009 - Call Camera Culling
                    //InstancedItem.UpdateInstanceTransformsForCulling();
                }

                StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.InstancedItemCameraCulling); //"InstancedItem_CameraCulling"

                // Signal end of thread frame.
                CameraCullingThreadEnd.Set();
            }
        }



        /// <summary>
        /// Iterates through all InstanceModels, and then through the instanceTransforms List,
        /// only adding the ones which are in the Camera Frustum, for Culling Purposes.
        /// </summary>       
        /// <param name="instanceModelsKeys">Array of Keys, used to filter down to a specific set of 
        /// InstancedItem models; for example, 'AlphaMap' items.</param> 
        internal static void UpdateInstanceTransformsForCulling(IList<int> instanceModelsKeys)
        {
            // 8/6/2009 - Return if null.
            if (instanceModelsKeys == null)
                return;

            // 8/6/2009 - Return if Count zero.
            var instanceModelKeysCount = instanceModelsKeys.Count; // 8/13/2009
            if (instanceModelKeysCount == 0)
                return;

            // 2/8/2010 - Start Stopwatch timer
            TimerToSleep.Reset();
            TimerToSleep.Start();


            // Iterate through each ItemType
            int keyIndex;
            for (keyIndex = 0; keyIndex < instanceModelKeysCount; keyIndex++)
            {
                var itemIndex = instanceModelsKeys[keyIndex];

                // 8/13/2009 - Cache
                if (InstancedItem.InstanceModels == null) continue;

                var instanceModel = InstancedItem.InstanceModels[itemIndex];

                // Check if ItemType is being used; if not, Skip
                // to next SceneItemOwner by using 'Continue'.
                if (instanceModel == null)
                    continue;

                // 4/7/2009     
                var instanceWorldTransforms = instanceModel.InstanceWorldTransforms; // 8/13/2009
                int itemCount;
                lock (InstancedItem.InstanceModelsThreadLock)
                {
                    // 11/7/2009 - SpeedCollection Thread Lock
                    //lock (instanceWorldTransforms.ThreadLock)
                    {
                        itemCount = instanceWorldTransforms.Keys.Count;

                        // 12/18/2008 - Get Keys for Dictionary 
                        if (instanceModel.InstanceWorldTransformKeys.Length < itemCount)
                            Array.Resize(ref instanceModel.InstanceWorldTransformKeys, itemCount);
                        instanceWorldTransforms.Keys.CopyTo(instanceModel.InstanceWorldTransformKeys, 0);

                    } // End SpeedCollection Lock
                }

                // 5/6/2009 - Updated to use the new CollisionRadius BoundingSphere.
                // Check if in Camera Frustum before adding Transform                
                var tmpBoundingSphere = instanceModel.CollisionRadius;


                // Iterate through all Instances of this Model Type.               
                var keys = instanceModel.InstanceWorldTransformKeys; // 5/11/2009
                for (var loop1 = 0; loop1 < itemCount; loop1++)
                {
                    var tmpWorldTransform = instanceWorldTransforms[keys[loop1]].Transform;

                    bool isInFrustrum;
                    Camera.IsInCameraFrustrum(ref tmpWorldTransform, ref tmpBoundingSphere, out isInFrustrum);

                    // Retrieve InstanceData Node
                    var existingNode = instanceWorldTransforms[keys[loop1]];

                    // 7/14/2009
                    //UpdateForCulling(isInFrustrum, ref existingNode);
                    
                    // 8/27/2009: Delete All Normal Culled Parts, since item out of camera view.
                    if (!isInFrustrum && existingNode.InCameraView)
                    {
                        InstancedModelChangeRequests.AddChangeRequestForInstanceTransforms((ItemType)itemIndex, ref existingNode,
                                                                                           ChangeRequest.DeleteAllCulledParts_InstanceItem, instanceModel);
                    }

                    // 8/27/2009
                    existingNode.InCameraView = isInFrustrum;

                    // Save the InstanceData Node back into Array
                    instanceWorldTransforms[keys[loop1]] = existingNode;

                    // 8/27/2009 - ONLY process if in Camera view.
                    if (isInFrustrum)
                    {
                        // 7/21/2009: Updated to use the new 'AddChangeRequest' method, to batch entries.
                        InstancedModelChangeRequests.AddChangeRequestForInstanceTransforms((ItemType)itemIndex, ref existingNode,
                                                                                           ChangeRequest.AddUpdatePart_InstanceItem, instanceModel);
                    }

                    // 2/8/2010 - Sleep a few ms, if necessary.
                    if (TimerToSleep.Elapsed.TotalMilliseconds < TimerSleepMax.Milliseconds) continue;

                    Thread.Sleep(10);
                    TimerToSleep.Reset();
                    TimerToSleep.Start();
                } // End For each SceneItemOwner's Transform   

                // 7/25/2009
                instanceModel.UpdateInstanceTransforms();

                // 2/8/2010 - Sleep a few ms, if necessary.
                if (TimerToSleep.Elapsed.TotalMilliseconds < TimerSleepMax.Milliseconds) continue;

                Thread.Sleep(10);
                TimerToSleep.Reset();
                TimerToSleep.Start();
            } // End For each ItemType            
        }

        /// <summary>
        /// Stops the internal Threads from running; should be called ONLY when entire application is terminiating.
        /// </summary>
        public static void StopInstancedItemThreads()
        {
            // 8/6/2009 - Stop threads.
            _cameraCullingIsStopping = true;
           
            // let's shutdown our thread if it hasn't
            // shutdown already
            if (_cameraCullingThread == null) return;

            CameraCullingThreadStart.Set(); // 7/17/2009
            _cameraCullingThread.Join(); // wait for the thread to shutdown
            _cameraCullingThread.Abort(); // Terminate the Thread.
            _cameraCullingThread = null;
        }
    }
}
