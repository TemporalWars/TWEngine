#region File Description
//-----------------------------------------------------------------------------
// InstancedModelParallelFor.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using ImageNexus.BenScharbach.TWEngine.InstancedModels;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Structs;
using ImageNexus.BenScharbach.TWEngine.Terrain;
using ImageNexus.BenScharbach.TWEngine.Terrain.Enums;
using ImageNexus.BenScharbach.TWTools.ParallelTasksComponent;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.ParallelTasks
{
    // 1/13/2011 : NOTE: NonProduction code.
    ///<summary>
    /// The <see cref="InstancedModelParallelFor"/> class, threads the calling of the player logic, 
    /// for each <see cref="InstancedModel"/>, into 4 parallize processors.
    ///</summary>
    public class InstancedModelParallelFor : AbstractParallelFor
    {
        // Collection of items to iterate
        private volatile IList<int> _modelPartsKeys;
        private volatile InstancedModel _instancedModel;
        private ChangeRequestItem _changeRequestItem;
        private BufferRequestItem[] _bufferRequestItems = new BufferRequestItem[1];

         // 5/28/2010
        /// <summary>
        /// Constructor; can use either the custom <see cref="MyThreadPool"/>, or the .Net Framework <see cref="ThreadPool"/>.
        /// </summary>
        /// <param name="useDotNetThreadPool">Use the .Net Framework <see cref="ThreadPool"/>?</param>
        public InstancedModelParallelFor(bool useDotNetThreadPool)
        {
            // save
            UseDotNetThreadPool = useDotNetThreadPool;
        }

        /// <summary>
        /// Used to parallize a 'For-Loop', to run on 4 separate processors.
        /// </summary>
        /// <param name="instancedModel"><see cref="InstancedModel"/> instance</param>
        /// <param name="changeRequestItem"><see cref="ChangeRequestItem"/> structure</param>
        /// <param name="modelPartsKeys">List of <paramref name="modelPartsKeys"/> to check</param>
        /// <param name="inclusiveLowerBound">Starting index of For-Loop</param>
        /// <param name="exclusiveUpperBound">Ending index of For-Loop</param>
        public void ParallelFor(InstancedModel instancedModel, ref ChangeRequestItem changeRequestItem,
                                IList<int> modelPartsKeys, int inclusiveLowerBound, int exclusiveUpperBound)
        {
            // Save ref to IList
            _modelPartsKeys = modelPartsKeys;

            // Save refs to other required data.
            _instancedModel = instancedModel;
            _changeRequestItem = changeRequestItem;

            // 10/17/2012 - Resize output array, if necessary
            var count = _modelPartsKeys.Count;
            if (_bufferRequestItems.Length < count)
                Array.Resize(ref _bufferRequestItems, count);

            // Start Parallel For-Loop process
            ParallelFor(this, inclusiveLowerBound, exclusiveUpperBound);

            // 10/17/2012 - Add BufferRequest to requests manager.
            for (var modelPartIndex = 0; modelPartIndex < _modelPartsKeys.Count; modelPartIndex++)
            {
                // get bufferRequest
                var bufferRequestItem = _bufferRequestItems[modelPartIndex];

                // Create Change Request
                InstancedModelPart.InstancedModelBufferRequests.EnterBufferRequestItemToCurrentChangeBuffer(
                    modelPartIndex, changeRequestItem.ItemInstanceKey,
                    ref bufferRequestItem, instancedModel._modelParts);
            }
        }

        /// <summary>
        /// Core method for the <see cref="LoopBody"/> of the 'For-Loop'.  Inheriting classes
        /// MUST override and provide the core <see cref="LoopBody"/> to the 'For-Loop' logic.
        /// </summary>
        /// <param name="index">The index value for the current collection.</param>
        protected override void LoopBody(int index)
        {
            UpdateTransforms(this, index);
        }
       

        // 2/16/2010; 5/24/2010: Updated to be STATIC method.
        /// <summary>
        /// Helper method, which does the updating of each <see cref="InstancedModel"/> transforms, and
        /// adds to the <see cref="InstancedModelChangeRequests"/> for processing.
        /// </summary>
        /// <param name="instancedModelParallelFor">this instance of <see cref="InstancedModelParallelFor"/></param>
        /// <param name="index">Loop index position</param>
        private static void UpdateTransforms(InstancedModelParallelFor instancedModelParallelFor, int index)
        {
            var useBakedTransforms = instancedModelParallelFor._instancedModel.UseBakedTransforms;

            var changeRequestItem = instancedModelParallelFor._changeRequestItem;
            var instancedModel = instancedModelParallelFor._instancedModel;
            var modelPartIndex = instancedModelParallelFor._modelPartsKeys[index];
            var modelPart = instancedModel.ModelParts[modelPartIndex]; // 10/17/2012

            // Only add if in camera view.
            if (changeRequestItem.InCameraView)
            {
                // Only Calc if NOT using BakedTransforms
                var transformResult = changeRequestItem.Transform;
                if (!useBakedTransforms)
                {
                    InstancedModel.CopyAbsoluteBoneTranformsTo(instancedModel, changeRequestItem.ItemInstanceKey);

                    // Optimize by removing Matrix Overload operations, which are slow on XBOX!                        
                    var tmpTransform = changeRequestItem.Transform;
                    // 6/17/2010 - was cast to (IDictionary<int, InstancedItemTransform[]>)
                    Matrix.Multiply(ref instancedModel._absoluteBoneTransforms[changeRequestItem.ItemInstanceKey][((IList<InstancedModelPart>)instancedModel._modelParts)[modelPartIndex].BoneOffsetIndex].AbsoluteTransform,
                        ref tmpTransform, out transformResult); // was 'out tmpTransformsToDraw[i]
                }

                // Update ChangeRequestItem                    
                //bufferRequestItem.Transform = transformResult;
                modelPart.StoreBufferRequestTransform(changeRequestItem.ItemInstanceKey, ref transformResult); // 10/17/2012
            }
            else // Update ChangeRequestItem                                
            {
                //bufferRequestItem.Transform = changeRequestItem.Transform;
                var transform = changeRequestItem.Transform;
                modelPart.StoreBufferRequestTransform(changeRequestItem.ItemInstanceKey, ref transform); // 10/17/2012
            }

            // 1/16/2011 - Updated to check if 'TerrainIsIn' playableMode before hiding units for FOW.
            // 4/21/2010 - Updated to check if 'ShapeItem' is null.
            // 1/30/2010 - Set Transform to zero, if FOW=false; 6/13/2010 - Not for Scenary items.
            if (TerrainShape.TerrainIsIn == TerrainIsIn.PlayableMode && !changeRequestItem.ShapeItem.IsFOWVisible)
            {
                //bufferRequestItem.Transform = new Matrix();
                var transform = Matrix.Identity;
                modelPart.StoreBufferRequestTransform(changeRequestItem.ItemInstanceKey, ref transform); // 10/17/2012
            }

            // NOTE: DEBUG
            /*if (instancedModel.ItemTypeInUse == ItemType.treePalmNew002c)
                System.Console.WriteLine(string.Format("The UpdateTransforms = {0}.", changeRequestItem.Transform));*/

            // Create new ChangeRequestItem
            var bufferRequestItem = new BufferRequestItem
            {
                BufferRequest = BufferRequest.AddOrUpdateInstancedModelPart,
                PlayerNumber = (short)changeRequestItem.PlayerNumber,
            };

            // 10/17/2012 - Add to output array
            instancedModelParallelFor._bufferRequestItems[index] = bufferRequestItem;
        }
      
    }
}