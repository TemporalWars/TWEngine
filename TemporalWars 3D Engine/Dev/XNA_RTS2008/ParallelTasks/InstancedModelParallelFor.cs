#region File Description
//-----------------------------------------------------------------------------
// InstancedModelParallelFor.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;
using ParallelTasksComponent;
using TWEngine.InstancedModels;
using TWEngine.InstancedModels.Enums;
using TWEngine.InstancedModels.Structs;

namespace TWEngine.ParallelTasks
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
        private volatile PartType _partType;
        private InstancedDataCommunication _instanceData;

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
        /// <param name="partType"><see cref="PartType"/> Enum</param>
        /// <param name="instanceData"><see cref="InstancedDataCommunication"/> structure</param>
        /// <param name="modelPartsKeys">List of <paramref name="modelPartsKeys"/> to check</param>
        /// <param name="inclusiveLowerBound">Starting index of For-Loop</param>
        /// <param name="exclusiveUpperBound">Ending index of For-Loop</param>
        public void ParallelFor(InstancedModel instancedModel, PartType partType, ref InstancedDataCommunication instanceData,
                                IList<int> modelPartsKeys, int inclusiveLowerBound, int exclusiveUpperBound)
        {
            // Save ref to IList
            _modelPartsKeys = modelPartsKeys;

            // Save refs to other required data.
            _instancedModel = instancedModel;
            _partType = partType;
            _instanceData = instanceData;

            // Start Parallel For-Loop process
            ParallelFor(this, inclusiveLowerBound, exclusiveUpperBound);

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

            var modelPartIndex = instancedModelParallelFor._modelPartsKeys[index];

            // 2/3/2010 - Check if restricted to one modelpart.
            // Note: Its important to note, the check MUST be done using the internal '_modelPartIndexKey', and NOT the Property!
            var restrictToModelPart = (instancedModelParallelFor._instanceData._modelPartIndexKey > 0 && 
                instancedModelParallelFor._instanceData.ModelPartIndexKey != modelPartIndex);

            // Create new ChangeRequestItem
            var changeRequestItem = new ChangeRequestItem
                                        {
                                            ChangeRequest = ChangeRequest.AddUpdatePart_InstanceItem,
                                            PlayerNumber = instancedModelParallelFor._instanceData.PlayerNumber, // 8/28/2009
                                            ProceduralMaterialId = (restrictToModelPart) ? 0 : instancedModelParallelFor._instanceData.ProceduralMaterialId, // 2/3/2010
                                            ShowFlashWhite = instancedModelParallelFor._instanceData.ShowFlashWhite, // 10/12/2009 (Scripting Purposes)
                                            PartType = instancedModelParallelFor._partType // 7/24/2009
                                        };

            // Only add if in camera view.
            if (instancedModelParallelFor._instanceData.InCameraView)
            {
                // Only Calc if NOT using BakedTransforms
                Matrix tmpTransformResult;
                if (!useBakedTransforms)
                {
                    InstancedModel.CopyAbsoluteBoneTranformsTo(instancedModelParallelFor._instancedModel, instancedModelParallelFor._instanceData.ItemInstanceKey);

                    // Optimize by removing Matrix Overload operations, which are slow on XBOX!                        
                    var tmpTransform = instancedModelParallelFor._instanceData.Transform;
                    Matrix.Multiply(ref ((IDictionary<int, InstancedItemTransform[]>)instancedModelParallelFor._instancedModel._absoluteBoneTransforms)[instancedModelParallelFor._instanceData.ItemInstanceKey]
                                        [((IList<InstancedModelPart>)instancedModelParallelFor._instancedModel._modelParts)[modelPartIndex].BoneOffsetIndex].AbsoluteTransform,
                                    ref tmpTransform, out tmpTransformResult); // was 'out tmpTransformsToDraw[i]
                }
                else
                    tmpTransformResult = instancedModelParallelFor._instanceData.Transform;

                // Update ChangeRequestItem                    
                changeRequestItem.Transform = tmpTransformResult;
            }
            else // Update ChangeRequestItem                                
                changeRequestItem.Transform = instancedModelParallelFor._instanceData.Transform;

            // 1/30/2010 - Set Transform to zero, if FOW=false.
            if (!instancedModelParallelFor._instanceData.ShapeItem.IsFOWVisible)
                changeRequestItem.Transform = new Matrix();
          
            InstancedModelChangeRequests.EnterChangeRequestItemToCurrentChangeBuffer(modelPartIndex, instancedModelParallelFor._instanceData.ItemInstanceKey,
                                                                                     ref changeRequestItem, instancedModelParallelFor._instancedModel._modelParts);
        }
      
    }
}