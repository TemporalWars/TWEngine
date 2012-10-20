#region File Description
//-----------------------------------------------------------------------------
// InstancedModelChangeRequests.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Structs;
using ImageNexus.BenScharbach.TWTools.ParallelTasksComponent;
using ImageNexus.BenScharbach.TWTools.PerfTimersComponent.Timers;
using ImageNexus.BenScharbach.TWTools.PerfTimersComponent.Timers.Enums;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.InstancedModels
{
    // 8/28/2009
    /// <summary>
    /// The <see cref="InstancedModelChangeRequests"/> class is used to manage the
    /// change requests, using the given <see cref="ChangeRequestItem"/> structure, to marshal
    /// data to the <see cref="InstancedModelPart"/> classes Change-Buffers, while processing requests
    /// by the <see cref="BufferRequest"/> Enum type given for each structure.
    /// </summary>
    public class InstancedModelChangeRequests : IDisposable
    {
        
        // 8/27/2009 - Change Request for Updating the instance transforms.
        // Key = ItemInstanceKey.
        internal readonly Dictionary<int, ChangeRequestItem> ChangeRequests = new Dictionary<int, ChangeRequestItem>();

        // 6/1/2010
        ///<summary>
        /// The current Buffer value, for double buffering; this should reflect the same value as in 'InstancedModel'.
        ///</summary>
        /// <remarks>Can only be set to two values; either 0 or 1.</remarks>
        internal static int CurrentUpdateBuffer;

        /// <summary>
        /// Stores the <see cref="Matrix.Identity"/> transform for quick retrieval.
        /// </summary>
        private static readonly Matrix MatrixIdentity = Matrix.Identity;

        /// <summary>
        /// Used in the <see cref="DoChangeRequestForInstanceTransforms"/> method to hold the current changeRequest keys.
        /// </summary>
        /// <remarks>This is to eliminate garbage on HEAP, which slows Xbox.</remarks>
        private static int[] _keys = new int[50];

        /// <summary>
        /// Used in the <see cref="ProcessChangeRequests"/> method to hold the current array keys.
        /// </summary>
        /// <remarks>This is to eliminate garbage on HEAP, which slows Xbox.</remarks>
        private static int[] _changeRequestKeys = new int[50];

        /// <summary>
        /// Used to add a new <see cref="BufferRequest"/> to the current <see cref="InstancedModel"/> item,
        /// using the given <see cref="ChangeRequestItem"/> structure to pass the data.
        /// </summary>
        /// <param name="itemType"><see cref="ItemType"/> of this <see cref="InstancedModel"/></param>
        /// <param name="changeRequestItem">New <see cref="ChangeRequestItem"/> node with the updated changes to occur in next batch.</param>
        /// <param name="instancedModel"><see cref="InstancedModel"/> instance to attach change request to.</param>
        internal static void AddChangeRequestForInstanceTransforms(ItemType itemType, ref ChangeRequestItem changeRequestItem, InstancedModel instancedModel)
        {
            // 11/6/2009
            try
            {
                // 2/3/2010 - Cache items from InstancedModel.
                var changeRequests = instancedModel.ChangeRequestManager.ChangeRequests;
                //var modelParts = instancedModel.ModelParts;

                // Check ChangeRequest type.
                switch (changeRequestItem.ChangeRequest)
                {
                    case ChangeRequestEnum.AddOrUpdateTransform:
                    case ChangeRequestEnum.AddOrUpdateFlashWhite: // 10/16/2012
                    case ChangeRequestEnum.AddOrUpdateProcedureId: // 10/16/2012
                    case ChangeRequestEnum.RemoveTransform: // 10/18/2012
                        // see if already have request for given instance.
                        ChangeRequestItem currentChangeRequestItem;
                        if (changeRequests.TryGetValue(changeRequestItem.ItemInstanceKey, out currentChangeRequestItem))
                        {
                            // then update to new request value.
                            currentChangeRequestItem = changeRequestItem;

                            // store back into dictionary
                            changeRequests[changeRequestItem.ItemInstanceKey] = currentChangeRequestItem;
                        }
                        else
                        {
                            // add new request
                            changeRequests.Add(changeRequestItem.ItemInstanceKey, changeRequestItem);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                        // TODO: 10/16/2012 - Should move below code to the BufferRequest class.
                        /*case ChangeRequest.RemovesASingleInstancedModelPart: // 8/27/09
                    // Create new bufferRequestItem
                    var bufferRequestItem = new BufferRequestItem
                    {
                        ChangeRequest = ChangeRequest.RemovesASingleInstancedModelPart,
                        Transform = MatrixIdentity
                    };

                    var modelPartsCount = modelParts.Count; // 1/5/2010
                    for (var modelPartIndex = 0; modelPartIndex < modelPartsCount; modelPartIndex++)
                    {
                        InstancedModelPart.InstancedModelBufferRequests.EnterBufferRequestItemToCurrentChangeBuffer(modelPartIndex, changeRequestItem.ItemInstanceKey,
                                                                    ref bufferRequestItem, modelParts);
                    }
                    break;*/
                }

                // 2/3/2010 - Moved inside this method; rather than being called from the 'InstancedItem' methods.
                // 11/10/2009: Updated to use new 'SafeAdd' for Thread saftey.
                // 7/21/2009 - Add to ChangeRequests List, the ItemType which made the request.
                var itemTypeInt = (int) itemType; // 4/16/2010
                ChangeRequestItemTypeKeys.SafeAdd(itemTypeInt, itemTypeInt);
            }
            // Note: The following ArgumentExpception error is thrown from the Dictionary!  It occurs, in this case,
            //       when the same 'ItemInstanceKey' is being Insert and Add at the same time, due to a Thread sync error!
            //       Normally, a Lock would be used, but this error doesn't seem to occur too frequently; therefore, it 
            //       seems to be faster just to capture it once in a while?!
            catch (ArgumentException)
            {
                // 11/6/2009 - Just log the error, to see how often this occurs!
                Debug.WriteLine("(AddChangeRequestForInstanceTransforms) threw the 'ArgumentException' error.");
                
            }
        }

        // 11/10/2009 - Updated to use my new ThreadSafeDictionary.
        internal static readonly ThreadSafeDictionary<int, int> ChangeRequestItemTypeKeys = new ThreadSafeDictionary<int, int>(25);

        /// <summary>
        /// Processes ChangeRequests by iterating through the <see cref="ChangeRequestItemTypeKeys"/> collection, which
        /// holds the integer values for each <see cref="ItemType"/> to process.  This method
        /// then calls the <see cref="DoChangeRequestForInstanceTransforms"/> method, passing in the <see cref="InstancedModel"/>
        /// to update, for the specific <see cref="ItemType"/> model, during each iteration.
        /// </summary>
        internal static void ProcessChangeRequests()
        {
            // 12/18/2009 - Updated to use new 'KeysSafeCopyTo' method.
            int changeRequestCount;
            ChangeRequestItemTypeKeys.KeysSafeCopyTo(ref _changeRequestKeys, 0, out changeRequestCount);

            // skip any processing, if empty.
            if (changeRequestCount <= 0) return;

            // 2/16/2010
            StopWatchTimers.StartStopWatchInstance(StopWatchName.DoChangeRequests);

            for (var i = 0; i < changeRequestCount; i++)
            {
                // 11/10/2009 - Make sure key exist - I know... shoudn't occur, but it did on XBOX!
                int index;
                if (!ChangeRequestItemTypeKeys.TryGetValue(_changeRequestKeys[i], out index)) 
                    continue;

                var instancedModel = InstancedItem.InstanceModels[index];
                if (instancedModel == null) continue;
                    
                DoChangeRequestForInstanceTransforms(instancedModel);
            }
            ChangeRequestItemTypeKeys.Clear();

            // 2/16/2010
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.DoChangeRequests);
        }

        /// <summary>
        /// Processes ChangeRequests by iterating through the 'ChangeRequests' for this specific
        /// <see cref="InstancedModel"/>, and calling the <see cref="InstancedModel.UpdateInstanceTransforms"/>.
        /// </summary>
        /// <param name="instancedModel">Specific <see cref="InstancedModel"/> type to process</param>
        private static void DoChangeRequestForInstanceTransforms(InstancedModel instancedModel)
        {
            try
            {
                // 4/21/2010 - Cache
                var changeRequests = instancedModel.InstancedModelChangeRequestManager.ChangeRequests;
                if (changeRequests == null) return;

                // 4/21/2010 - Cache
                var keys = changeRequests.Keys;

                // Get Keys to Dictionary
                var changeRequestCount = keys.Count;

                // 8/20/2009 - skip any processing, if empty.
                if (changeRequestCount <= 0) return;

                if (_keys.Length < changeRequestCount)
                    Array.Resize(ref _keys, changeRequestCount);
                keys.CopyTo(_keys, 0);

                // iterate dictionary, and apply requests.
                for (var i = 0; i < changeRequestCount; i++)
                {
                    // 6/17/2010 - Make sure key exist - I know... shoudn't occur, but it did on XBOX!
                    ChangeRequestItem changeRequestItem;
                    if (!changeRequests.TryGetValue(_keys[i], out changeRequestItem)) 
                        continue;

                    // 10/16/2012
                    // Check ChangeRequest type.
                    switch (changeRequestItem.ChangeRequest)
                    {
                        case ChangeRequestEnum.AddOrUpdateTransform:
                            InstancedModel.UpdateInstanceTransforms(instancedModel, ref changeRequestItem);
                            break;
                        case ChangeRequestEnum.AddOrUpdateFlashWhite: // 10/16/2012
                            InstancedModel.UpdateInstanceFlashWhite(instancedModel, ref changeRequestItem);
                            break;
                        case ChangeRequestEnum.AddOrUpdateProcedureId: // 10/16/2012
                            InstancedModel.UpdateProceduralId(instancedModel, ref changeRequestItem);
                            break;
                        case ChangeRequestEnum.RemoveTransform: // 10/18/2012
                            InstancedModel.RemoveInstanceTransform(instancedModel, ref changeRequestItem);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                // finish, so clear requests.
                changeRequests.Clear();
            }
            catch (NullReferenceException)
            {
                Debug.WriteLine("(DoChangeRequestForInstanceTransforms) threw the 'NullReferenceException' error.");
            }
        }

        // 1/6/2010
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Clear values.
            if (ChangeRequests != null)
            {
                ChangeRequests.Clear();
            }

            Array.Clear(_keys, 0, _keys.Length);
            Array.Clear(_changeRequestKeys, 0, _changeRequestKeys.Length);
        }
    }
}
