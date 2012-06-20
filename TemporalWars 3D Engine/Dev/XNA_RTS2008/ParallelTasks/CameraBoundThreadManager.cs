#region File Description
//-----------------------------------------------------------------------------
// CameraBoundThreadManager.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;
using ParallelTasksComponent;
using ParallelTasksComponent.LocklessQueue;
using TWEngine.ParallelTasks.Structs;
using TWEngine.SceneItems;

namespace TWEngine.ParallelTasks
{
    // 2/28/2011
    /// <summary>
    /// The <see cref="CameraBoundThreadManager{TAParam}"/> class manages the process of updating
    /// the camera bound area.
    /// </summary>
    public sealed class CameraBoundThreadManager<TAParam> : ThreadProcessor<CameraBoundItemStruct<TAParam>>
    {
        
        // 2/17/2010: Updated to use the new 'AutoBlocking' type.
        ///<summary>
        /// Constructor, which starts the <see cref="ThreadProcessor{KillSceneItemStruct}"/>, used to queue
        /// up <see cref="SceneItem"/> items to be removed from game play.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        public CameraBoundThreadManager(Game game)
            : base(game, "MiscThreadManager Thread", 5, ThreadMethodTypeEnum.AutoBlocking)
        {
            
            // Empty
           
        }

        /// <summary>
        /// The method which is called from the <see cref="Thread"/> each cycle.
        /// </summary>
        protected override void ProcessorThreadDelegateMethod()
        {
            // Refactored out code to new STATIC method.
            DoProcessorThreadDelegateMethod();
        }

        // 5/20/2010
        /// <summary>
        /// Helper method, which iterates the <see cref="LocklessQueue{T}"/> collection, and processes
        /// each <see cref="Action{TAParam}"/>.
        /// </summary>
        private static void DoProcessorThreadDelegateMethod()
        {
            try
            {
                CameraBoundItemStruct<TAParam> action;
                while (LocklessQueue.TryDequeue(out action))
                {
                    // Do Action
                    if (action.ActionMethod != null)
                        action.ActionMethod(action.MethodParam);

                    // Have Thread sleep a few ms.
                    Thread.Sleep(1);
                }
            } // NOTE: Error occurs when count says 1, but 'Dequeue' fails since count is zero. (Rare)
            catch (ArgumentNullException)
            {
                Debug.WriteLine("(DoProcessorThreadDelegateMethod) threw the 'ArgumentNullException' error.");
            }
        }
        
    }
}