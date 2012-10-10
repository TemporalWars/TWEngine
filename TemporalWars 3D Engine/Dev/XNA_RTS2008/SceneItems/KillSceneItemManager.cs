#region File Description
//-----------------------------------------------------------------------------
// KillSceneItemManager.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Diagnostics;
using System.Threading;
using ImageNexus.BenScharbach.TWEngine.SceneItems.Structs;
using ImageNexus.BenScharbach.TWTools.ParallelTasksComponent;
using ImageNexus.BenScharbach.TWTools.ParallelTasksComponent.LocklessQueue;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.SceneItems
{
    // 10/3/2009 - KillSceneItemStruct

    /// <summary>
    /// The <see cref="KillSceneItemManager"/> class manages the process of killing a 
    /// <see cref="SceneItem"/>, which simply involves calling the item's <see cref="SceneItem.StartKillSceneItem"/> method.
    /// However, due to the lengthy time the kill process can take, this is now run within this threaded
    /// manager to eliminate any hicups in game play!
    /// </summary>
    public sealed class KillSceneItemManager : ThreadProcessor<KillSceneItemStruct>
    {
        //private volatile Queue<KillSceneItemStruct> _itemsToProcess = new Queue<KillSceneItemStruct>(50);

        // 2/17/2010: Updated to use the new 'AutoBlocking' type.
        ///<summary>
        /// Constructor, which starts the <see cref="ThreadProcessor{KillSceneItemStruct}"/>, used to queue
        /// up <see cref="SceneItem"/> items to be removed from game play.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        public KillSceneItemManager(Game game)
            : base(game, "KillSceneItemManager Thread", 3, ThreadMethodTypeEnum.AutoBlocking)
        {
            
            // Empty
           
        }

        /// <summary>
        /// The method which is called from the <see cref="Thread"/> each cycle.
        /// </summary>
        protected override void ProcessorThreadDelegateMethod()
        {
            // 5/20/2010: Refactored out code to new STATIC method.
            ProcessKillSceneItemList();
        }

        // 5/20/2010
        /// <summary>
        /// Helper method, which iterates the <see cref="LocklessQueue{T}"/> collection, and processes
        /// each <see cref="KillSceneItemStruct"/>.
        /// </summary>
        private static void ProcessKillSceneItemList()
        {
            try
            {
                // Iterate List, and call the 'KillSceneItem' method for each item.
                //var count = toProcess.Count; // 11/11/09
                //for (var i = 0; i < count; i++)
                KillSceneItemStruct killSceneItem;
                while (LocklessQueue.TryDequeue(out killSceneItem))
                {
                    // Dequeue item
                    //var killSceneItem = toProcess.Dequeue();

                    // 11/11/09 - Cache
                    var itemToKill = killSceneItem.ItemToKill;

                    // Call 'KillSceneITem' method on item
                    if (itemToKill != null)
                        itemToKill.StartKillSceneItem(ref ElapsedGameTime, killSceneItem.AttackerPlayerNumber);

                    // Have Thread sleep a few ms.
                    Thread.Sleep(1);
                }
            } // NOTE: Error occurs when count says 1, but 'Dequeue' fails since count is zero. (Rare)
            catch (ArgumentNullException)
            {
                Debug.WriteLine("(ProcessorThreadDelegateMethod) threw the 'ArgumentNullException' error.");
            }
        }

        // 8/21/2009
        /*/// <summary>
        /// Overrides base method, to process the <see cref="ThreadProcessor{T}.ItemRequests"/> and add
        /// them to the Queue '_itemsToProcess'.
        /// </summary>
        protected override void ProcessRequestItems()
        {
            // 5/20/2010: Refactored out code to new STATIC method.
            DoProcessRequestItems(this);
        }

        // 5/20/2010
        /// <summary>
        /// Method helper, which iterates the <see cref="ThreadProcessor{T}.ItemRequests"/> collection, and adds
        /// to the <see cref="_itemsToProcess"/> collection.
        /// </summary>
        /// <param name="killSceneItemManager">this instance of <see cref="KillSceneItemManager"/></param>
        private static void DoProcessRequestItems(KillSceneItemManager killSceneItemManager)
        {
            lock (killSceneItemManager.DataThreadLock)
            {
                // make sure not empty
                var itemRequests = ItemRequests; // 4/26/2010 - Cache
                var count = itemRequests.Count; // 11/11/09
                if (count <= 0) return;

                // iterate requests list
                var itemsToProcess = killSceneItemManager._itemsToProcess; // 4/26/2010 - Cache
                for (var i = 0; i < count; i++)
                {
                    itemsToProcess.Enqueue(itemRequests[i]);
                }
                itemRequests.Clear();
            }
        }

        // 2/17/2010
        /// <summary>
        /// Override base Check, to also check if current 'Queue' is empty, before
        /// allowing <see cref="Thread"/> to sleep.
        /// </summary>
        /// <param name="threadStart"><see cref="AutoResetEvent"/> instance</param>
        protected override void CheckToBlock(AutoResetEvent threadStart)
        {
            // 5/24/2010: Updated to now put 'WaitOne' call into While loop, with 5 ms checks!
            if (ItemRequests.Count != 0 || _itemsToProcess.Count != 0) return;

            while(!threadStart.WaitOne(5, false))
            {
                Thread.Sleep(5);
            }
        }*/
    }
}