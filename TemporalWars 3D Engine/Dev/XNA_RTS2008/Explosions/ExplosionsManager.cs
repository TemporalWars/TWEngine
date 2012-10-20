#region File Description
//-----------------------------------------------------------------------------
// ExplosionsManager.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.Explosions.Structs;
using ImageNexus.BenScharbach.TWEngine.InstancedModels;
using ImageNexus.BenScharbach.TWEngine.Particles;
using ImageNexus.BenScharbach.TWEngine.Particles.Enums;
using ImageNexus.BenScharbach.TWTools.ParallelTasksComponent;
using ImageNexus.BenScharbach.TWTools.ParallelTasksComponent.LocklessQueue;
using ImageNexus.BenScharbach.TWTools.Particles3DComponentLibrary;
using Microsoft.Xna.Framework;
using System.Threading;

namespace ImageNexus.BenScharbach.TWEngine.Explosions
{
    // 10/13/12012 - Obsolete
    /// <summary>
    /// Explosions Manager class, manages the calculations and updates for
    /// each <see cref="ExplosionItem"/>.
    /// </summary>
    /// <remarks>Inherits from the ThreadProcessor parallel class</remarks>
    [Obsolete]
    public sealed class ExplosionsManager : ThreadProcessor<ExplosionItem>
    {
        // 4/8/2009 - Particles Explosions
        private static ParticleSystem _explosionParticles;
        private static ParticleSystem _smallExplosionSmokeParticles;
        private static ParticleSystem _mediumExplosionSmokeParticles;
        private static ParticleSystem _largeExplosionSmokeParticles;

        private static readonly Queue<ExplosionItem> ItemsToProcess = new Queue<ExplosionItem>(100);

        // 8/10/2009 - StopWatch timer, used to keep the Explosions processing to a max time per cycle!
        private static readonly Stopwatch TimerToSleep = new Stopwatch();
        private static readonly TimeSpan TimerSleepMax = new TimeSpan(0, 0, 0, 0, 20);

        //private static ITerrainShape _terrainShape;
        //private static readonly Vector3 Vector3Zero = Vector3.Zero;

        //private const float Gravity = 9.8f * 10; // 10 frames per second.
        //private const float Friction = 0.2f;

        ///<summary>
        /// Constructor for ExplosionManager class, which creates the initial
        /// thread used in the calculations and updating of the <see cref="ExplosionItem"/>.
        /// Also creates several <see cref="ParticleSystem"/> instances, used to show special effects
        /// , like sparks and smoke.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance.</param>
        public ExplosionsManager(Game game)
            : base(game, "ExplosionManager Thread", 4, ThreadMethodTypeEnum.AutoBlocking)
        {
            // 1/16/2010: Updated to use the new 'GetParticleSystem' method.
            // 4/8/2009 - Get ParticleSystem Services
            ParticlesManager.GetParticleSystem(ParticleSystemTypes.ExplosionParticleSystem, 0, out _explosionParticles);
            ParticlesManager.GetParticleSystem(ParticleSystemTypes.SmallExplosionSmokeParticleSystem, 0, out _smallExplosionSmokeParticles);
            ParticlesManager.GetParticleSystem(ParticleSystemTypes.MediumExplosionSmokeParticleSystem, 0, out _mediumExplosionSmokeParticles);
            ParticlesManager.GetParticleSystem(ParticleSystemTypes.LargeExplosionSmokeParticleSystem, 0, out _largeExplosionSmokeParticles);
           
        }

        /// <summary>
        /// Iterates the current <see cref="LocklessQueue{T}"/> queue to pass the <see cref="ExplosionItem"/> piece into
        /// the <see cref="ItemsToProcess"/> queue for processing; once an item is marked for
        /// deletion, it is removed from the game.
        /// </summary>
        /// <remarks>The method which is called from the Thread each cycle</remarks>
        protected override void ProcessorThreadDelegateMethod()
        {
            // 6/7/2010 - Process Item-Request.
            ProcessRequestItems();

            // 5/24/2010: Refactored out the core code to new STATIC method.
            DoProcessorThreadDelegateMethod();

        }

        // 5/24/2010
        /// <summary>
        /// Method helper, which iterates the current <see cref="ItemsToProcess"/> queue and processes an <see cref="ExplosionItem"/>.
        /// </summary>
        private static void DoProcessorThreadDelegateMethod()
        {
            // 5/29/2012 - Skip if game paused.
            if (TemporalWars3DEngine.GamePaused)
                return;

            // 8/10/2009 - Start Stopwatch timer
            TimerToSleep.Reset();
            TimerToSleep.Start();

            // Iterate through current Explosion Items array to update
            var itemsToProcess = ItemsToProcess; // 5/24/2010
            while (itemsToProcess.Count > 0)
            {
                var explosionItem = itemsToProcess.Dequeue();
                
                // 6/8/2009 - Start StopWatch
                if (!explosionItem.StopWatchStarted)
                {
                    explosionItem.StopWatch.Start();
                    explosionItem.StopWatchStarted = true;
                }

                // 6/14/2010 - Start Explosion on shader.
                if (explosionItem.StartExplosion < 3)
                {
                    // 6/5/2010 - Trigger an update, to get the proper positioning update for part, then allow shader to finish with 
                    //            explosion animation.
                    var itemPieceAdjTransform = Matrix.Identity;
                    InstancedItem.SetAdjustingBoneTransform(ref explosionItem.InstancedItemData,
                                                            explosionItem.ItemPieceBoneName,
                                                            ref itemPieceAdjTransform);

                    explosionItem.StartExplosion++;
                    //UpdateExplodeItemPiece(ref ElapsedGameTime, ref explosionItem);
                }


                // 6/8/2009 - Check if at StopWatchLimit.
                if (explosionItem.StopWatch.Elapsed.Seconds >= explosionItem.StopWatchLimit.Seconds)
                {
                    explosionItem.StopWatch.Stop();

                    // clear all adjusting bone transforms.
                    InstancedItem.ResetAdjustingBoneTransform(ref explosionItem.InstancedItemData, explosionItem.ItemPieceBoneName);

                    explosionItem.Delete = true;

                    // Check if this piece is linked to sceneItem owner.
                    if (explosionItem.SceneItemOwner != null)
                    {
                        explosionItem.SceneItemOwner.FinishKillSceneItem(ref ElapsedGameTime,
                                                                         explosionItem.SceneItemOwner.PlayerNumber);

                        // Null SceneItem ref
                        explosionItem.SceneItemOwner = null;
                    }

                    continue;

                }

                // Explosion piece not done, so put back on queue.
                itemsToProcess.Enqueue(explosionItem);

                // 8/10/2009 - Sleep a few ms.
                if (TimerToSleep.Elapsed.TotalMilliseconds < TimerSleepMax.Milliseconds) continue;

                Thread.Sleep(1);
                TimerToSleep.Reset();
                TimerToSleep.Start();

                // 6/7/2010 - Check if more items need to be processed.
                ProcessRequestItems();
            }

            // Set to Zero
            ElapsedGameTime = TimeSpanZero;
        }

        // 2/6/2009
        /// <summary>
        /// Adds a new <see cref="ExplosionItem"/> to the <see cref="ExplosionsManager"/>.
        /// </summary>
        /// <param name="explosionItem">ExplosionItem to add</param>
        public void AddNewExplosionItem(ref ExplosionItem explosionItem)
        {
            AddItemRequest(explosionItem);
        }
        
        /// <summary>
        /// Processes the <see cref="ThreadProcessor{T}.LocklessQueue"/> collection, queuing them
        /// into the <see cref="ItemsToProcess"/> Queue.
        /// </summary>
        private static void ProcessRequestItems()
        {
            // make sure not empty
            var count = LocklessQueue.Count;
            if (count <= 0) return;

            // iterate requests list
            for (var i = 0; i < count; i++)
            {
                ExplosionItem explosionItem;
                LocklessQueue.TryDequeue(out explosionItem);

                ItemsToProcess.Enqueue(explosionItem);
            }
            
        }
        

        // 2/17/2010
        /*/// <summary>
        /// Override base Check, to also check if current 'Queue' is empty, before
        /// allowing thread to sleep.
        /// </summary>
        /// <param name="threadStart">Instance of <see cref="AutoResetEvent"/> thread to start.</param>
        protected override void CheckToBlock(AutoResetEvent threadStart)
        {
            // 5/24/2010: Updated to now put 'WaitOne' call into While loop, with 5 ms checks!
            if (ItemRequests.Count != 0 || _itemsToProcess.Count != 0) return;

            while (!threadStart.WaitOne(5, false))
            {
                Thread.Sleep(5);
            }
        }*/

        // 2/6/2009
        /*/// <summary>
        /// Updates the <see cref="ExplosionItem"/>, by calculating the proper translation values, depending on time, velocity,
        /// and gravity settings.  Once a piece hits the ground, it's velocity is stopped immediately.
        /// </summary>
        /// <param name="elapsedGameTime">Elapsed GameTime value</param>
        /// <param name="explosionItem">ExplosionItem to update</param>
        private static void UpdateExplodeItemPiece(ref TimeSpan elapsedGameTime, ref ExplosionItem explosionItem)
        {
            // 1/30/2010 - Skip all, if 'Delete' set True.
            if (explosionItem.Delete)
                return;

            // 1/30/2010 - Check if marked for 'StartDeletionCountdown'; if TRUE, then do countdown to deletion calc.
            if (explosionItem.StartDeletionCountdown)
            {
                // Check if Time to Delete From World
                if (explosionItem.SceneItemOwner != null)
                {
                    explosionItem.SceneItemOwner.DeleteFromWorldTime -= elapsedGameTime;
                    if (explosionItem.SceneItemOwner.DeleteFromWorldTime <= TimeSpan.Zero)
                    {
                        // Mark for explosion manager to finally delete entire sceneItem.
                        explosionItem.Delete = true;
                    }
                }
                return;
            }

            Matrix itemPieceAdjTransform;
            Matrix itemPieceRotAdjTransform;  // 4/3/2009

            // Update Turret Position
            var totalSeconds = elapsedGameTime.TotalSeconds; // 8/20/2009
            {
                // 2/6/2009: Updated by removing Ops Vector3 overloads, which are SLOW on XBOX!
                // apply velocity
                //newPosition += turretVelocity * (float)ElapsedTime.TotalSeconds;
                Vector3 itemPieceVeloctiyWithElapsed;
                Vector3.Multiply(ref explosionItem.ItemPieceVelocity, (float)totalSeconds, out itemPieceVeloctiyWithElapsed);
                Vector3.Add(ref explosionItem.ItemPieceNewPosition, ref itemPieceVeloctiyWithElapsed, out explosionItem.ItemPieceNewPosition);

                // apply Friction to velocity
                //turretVelocity *= (1 - Friction * (float)ElapsedTime.TotalSeconds);
                Vector3.Multiply(ref explosionItem.ItemPieceVelocity, (1 - Friction * (float)totalSeconds), out explosionItem.ItemPieceVelocity);

                // apply Gravity to velocity
                explosionItem.ItemPieceVelocity.Y -= Gravity * (float)totalSeconds;
            }
                     
            Matrix.CreateTranslation(ref explosionItem.ItemPieceNewPosition, out itemPieceAdjTransform);

            // 4/3/2009 - Let's also rotate the pieces!
            explosionItem.RotValue += (1.0f * explosionItem.RotSpeed) * (float)totalSeconds;
            explosionItem.RotValue = (explosionItem.RotValue > 360) ? 0 : explosionItem.RotValue;
            Matrix.CreateFromAxisAngle(ref explosionItem.RotAngle, explosionItem.RotValue, out itemPieceRotAdjTransform);
            Matrix.Multiply(ref itemPieceRotAdjTransform, ref itemPieceAdjTransform, out itemPieceAdjTransform);

            InstancedItem.SetAdjustingBoneTransform(ref explosionItem.InstancedItemData, explosionItem.ItemPieceBoneName, ref itemPieceAdjTransform);

            // Check if on ground yet?
            {
                // Make sure we have TerrainShape ref
                if (_terrainShape == null)
                    _terrainShape = (ITerrainShape)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(ITerrainShape));
                
                Matrix.Multiply(ref explosionItem.ItemPieceBoneTransform, ref itemPieceAdjTransform, out itemPieceAdjTransform);

                // 2/6/2009: Updated by removing Ops Matrix overloads, which are SLOW on XBOX!
                // Apply Orientation & World Transforms
                Matrix itemActualWorldPosition;
                Matrix.Multiply(ref itemPieceAdjTransform, ref explosionItem.OrientationXWorld, out itemActualWorldPosition);
                
                // 11/5/2009 - Item is falling?
                var isFalling = (explosionItem.ItemPieceVelocity.Y < 0);
                
                // If below ground level, then stop all velocity movement.
                var translation = itemActualWorldPosition.Translation; // 8/20/2009
                //if (translation.Y <= TerrainData.GetTerrainHeight(translation.X, translation.Z))
                if (translation.Y <= 0 && isFalling)
                {
                    explosionItem.ItemPieceVelocity = Vector3Zero;

                    // 6/8/2009 - reset the adjusting bone Transform for this piece!
                    InstancedItem.ResetAdjustingBoneTransform(ref explosionItem.InstancedItemData, explosionItem.ItemPieceBoneName);

                    // 1/30/2010 - Mark to start countdown to deletion from List, if this 'Piece' is linked to owner.
                    if (explosionItem.SceneItemOwner != null)
                        explosionItem.StartDeletionCountdown = true;
                    else 
                        explosionItem.Delete = true;
                }
                
            }

        }*/

        // 2/27/2009
        /*/// <summary>
        /// Clears out all <see cref="ExplosionItem"/>'s out of memory.
        /// </summary>
        public void ClearExplosionItems()
        {
            ClearItemsToProcess();
        }*/


        // 3/12/2009
        /// <summary>
        /// Releases the unmanaged resources used by the GameComponent and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _explosionParticles = null;
                _mediumExplosionSmokeParticles = null;
                _largeExplosionSmokeParticles = null;
            }

            base.Dispose(disposing);
        }
    }
}