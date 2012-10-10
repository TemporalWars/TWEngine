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
    /// change requests, using the given <see cref="InstancedDataCommunication"/> structure, to marshal
    /// data to the <see cref="InstancedModelPart"/> classes Change-Buffers, while processing requests
    /// by the <see cref="ChangeRequest"/> Enum type given for each structure.
    /// </summary>
    public class InstancedModelChangeRequests : IDisposable
    {
        
        // 8/27/2009 - Change Request for Updating the instance transforms.
        // Key = ItemInstanceKey.
        internal readonly Dictionary<int, InstancedDataCommunication> ChangeRequests = new Dictionary<int, InstancedDataCommunication>();

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
        /// Used to add a new <see cref="ChangeRequest"/> to the current <see cref="InstancedModel"/> item,
        /// using the given <see cref="InstancedDataCommunication"/> structure to pass the data.
        /// </summary>
        /// <param name="itemType"><see cref="ItemType"/> of this <see cref="InstancedModel"/></param>
        /// <param name="instancedDataCommunication">
        /// New <see cref="InstancedDataCommunication"/> node with the updated changes to occur in next batch.
        /// </param>
        /// <param name="changeRequest">
        /// <see cref="ChangeRequest"/> type to process; for example, to 'Update', or 'DeleteAllParts'.
        /// </param>
        /// <param name="instancedModel">
        /// <see cref="InstancedModel"/> instance to attach change request to
        /// </param>
        internal static void AddChangeRequestForInstanceTransforms(ItemType itemType, ref InstancedDataCommunication instancedDataCommunication, 
                                                                  ChangeRequest changeRequest, InstancedModel instancedModel)
        {
            // 11/6/2009
            try
            {
                // 2/3/2010 - Cache items from InstancedModel.
                var changeRequests = instancedModel.ChangeRequestManager.ChangeRequests;
                var modelParts = instancedModel.ModelParts;
                var modelPartsKeys = instancedModel.ModelPartsKeys;
                
                
                // Check ChangeRequest type.
                switch (changeRequest)
                {
                    case ChangeRequest.AddUpdatePart_InstanceItem:
                        // see if already have request for given instance.
                        InstancedDataCommunication currentWorldData;
                        if (changeRequests.TryGetValue(instancedDataCommunication.ItemInstanceKey, out currentWorldData))
                        {
                            // then update to new request value.
                            currentWorldData = instancedDataCommunication;

                            // store back into dictionary
                            changeRequests[instancedDataCommunication.ItemInstanceKey] = currentWorldData;
                        }
                        else
                        {
                            // add new request
                            changeRequests.Add(instancedDataCommunication.ItemInstanceKey, instancedDataCommunication); 
                        }
                        break;
                    case ChangeRequest.DeleteAllParts_InstanceItem:
                        // Create new ChangeRequestItem
                        var changeRequestItem = new ChangeRequestItem
                                                    {
                                                        ChangeRequest = ChangeRequest.DeleteAllParts_InstanceItem,
                                                        Transform = MatrixIdentity
                                                    };

                        var modelPartsCount = modelParts.Count; // 1/5/2010
                        for (var modelPartIndex = 0; modelPartIndex < modelPartsCount; modelPartIndex++)
                        {
                            EnterChangeRequestItemToCurrentChangeBuffer(modelPartIndex, instancedDataCommunication.ItemInstanceKey,
                                                                        ref changeRequestItem, modelParts);
                        }

                        break;
                    case ChangeRequest.DeleteAllCulledParts_InstanceItem: // 8/27/09
                        // Create new ChangeRequestItem
                        changeRequestItem = new ChangeRequestItem
                                                {
                                                    ChangeRequest = ChangeRequest.DeleteAllCulledParts_InstanceItem,
                                                    Transform = MatrixIdentity
                                                };

                        // iterate ONLY through normal parts.
                        var count = modelPartsKeys.Count; // 1/5/2010
                        for (var i = 0; i < count; i++)
                        {
                            EnterChangeRequestItemToCurrentChangeBuffer(modelPartsKeys[i], instancedDataCommunication.ItemInstanceKey,
                                                                        ref changeRequestItem, modelParts);
                        }
                        break;

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
        
        // 7/21/2009 - 
        /// <summary>
        /// Adds/Updates a <see cref="ChangeRequestItem"/> into the current ChangeBuffer.
        /// </summary>
        /// <param name="modelPartIndex">Index value of <see cref="InstancedModelPart"/> in <see cref="InstancedModel._modelParts"/> collection.</param>
        /// <param name="itemInstanceKey"><see cref="ItemType"/> unique key</param>
        /// <param name="changeRequestItem"><see cref="ChangeRequestItem"/> structure</param>
        /// <param name="modelParts">Collection of <see cref="InstancedModelPart"/>.</param>
        internal static void EnterChangeRequestItemToCurrentChangeBuffer(int modelPartIndex, int itemInstanceKey,
                                                                        ref ChangeRequestItem changeRequestItem, IList<InstancedModelPart> modelParts)
        {
            // 11/6/2009
            try
            {
                // 8/13/2009 - Cache
                var changeBuffers = modelParts[modelPartIndex].ChangeBuffers;

                // see if already have request for given instance.
                ChangeRequestItem currentChangeRequest;
                var currentUpdateBuffer = CurrentUpdateBuffer; // 1/5/2010; 6/1/2010 - Uses local now.
                if (changeBuffers[currentUpdateBuffer].TryGetValue(itemInstanceKey, out currentChangeRequest))
                {
                    // check if 'DeletePart' request, which takes priority.
                    if (currentChangeRequest.ChangeRequest == ChangeRequest.DeleteAllParts_InstanceItem)
                        return;

                    // then update new request value.
                    currentChangeRequest = changeRequestItem;

                    // store back into dictionary
                    changeBuffers[currentUpdateBuffer][itemInstanceKey] = currentChangeRequest;
                }
                else
                {
                    // Save request into Dictionary
                    changeBuffers[currentUpdateBuffer].Add(itemInstanceKey, changeRequestItem);
                }
            }
            // Note: The following ArgumentExpception error is thrown from the Dictionary!  It occurs, in this case,
            //       when the same 'ItemInstanceKey' is being Insert and Add at the same time, due to a Thread sync error!
            //       Normally, a Lock would be used, but this error doesn't seem to occur too frequently; therefore, it 
            //       seems to be faster just to capture it once in a while?!
            catch (ArgumentException)
            {
                // 11/6/2009 - Just log the error, to see how often this occurs!
                Debug.WriteLine("(EnterChangeRequestItemToCurrentChangeBuffer) threw the 'ArgumentException' error.");
                
            }
        }

        

        /// <summary>
        /// Updates the 'TransformsToDrawList' collections, based on the <see cref="ChangeRequestItem"/> given.  The 'TransformsToDrawList' to update, 
        /// depends on the <see cref="PartType"/> Enum, stored in the <see cref="ChangeRequestItem"/>.  For example, if <see cref="PartType"/> is of 'ExplosionPart',
        /// then the 'TransformsToDrawExpList' would be updated.
        /// </summary>
        /// <param name="instancedModelPart"><see cref="InstancedModelPart"/> instance</param>
        /// <param name="indexKey">Index position to affect in the given 'TransformsToDrawList' collection.</param>
        /// <param name="changeRequestItem"><see cref="ChangeRequestItem"/> structure</param>
        internal static void UpdateTransformsBasedOnChangeRequestItem(InstancedModelPart instancedModelPart, int indexKey, ref ChangeRequestItem changeRequestItem)
        {
           
            // Check Message type
            switch (changeRequestItem.ChangeRequest)
            {
                case ChangeRequest.AddUpdatePart_InstanceItem:

                    switch (changeRequestItem.PartType)
                    {
                        case PartType.NormalPart:
                            // Update 'Culled' list
                            UpdateGivenTransformList(instancedModelPart.TransformsToDrawList, indexKey, ref changeRequestItem);
                            // Update 'All' list
                            UpdateGivenTransformList(instancedModelPart.TransformsToDrawAllList, indexKey, ref changeRequestItem);
                            break;
                        case PartType.ExplosionPart:
                            // Update 'Explosion' list
                            UpdateGivenTransformList(instancedModelPart.TransformsToDrawExpList, indexKey, ref changeRequestItem);                            
                            break;                        
                    } 

                    break;
                    // 8/19/2009 - Adds a new Scenery culled part, while making sure the prior Culled parts are cleared!
                case ChangeRequest.AddUpdateSceneryPart_InstanceItem:
                    
                    // now add new culled part
                    {
                        // Update 'Culled' list
                        UpdateGivenTransformList(instancedModelPart.TransformsToDrawList, indexKey, ref changeRequestItem);
                        // Update 'All' list
                        UpdateGivenTransformList(instancedModelPart.TransformsToDrawAllList, indexKey, ref changeRequestItem);
                    }

                    break;
                case ChangeRequest.DeleteAllParts_InstanceItem:
                    // yes, so remove from all lists
                    instancedModelPart.TransformsToDrawList.Remove(indexKey);
                    instancedModelPart.TransformsToDrawAllList.Remove(indexKey);
                    instancedModelPart.TransformsToDrawExpList.Remove(indexKey);
                  
                    break;
                case ChangeRequest.DeleteAllCulledParts_InstanceItem:
                    // yes, so remove from all lists
                    instancedModelPart.TransformsToDrawList.Remove(indexKey);
                    instancedModelPart.TransformsToDrawExpList.Remove(indexKey);

                    break;
                case ChangeRequest.DeleteAllCulledParts_AllItems:
                    // yes, so remove items from ALL 'Culled' lists.
                    instancedModelPart.TransformsToDrawList.Clear();
                    instancedModelPart.TransformsToDrawExpList.Clear();
                   
                    break;
            }
        }

        /// <summary>
        /// Checks the given 'TransformsToDrawList' dictionary, at the given <paramref name="indexKey"/>, processing the 
        /// updates stored in the <see cref="ChangeRequestItem"/>, for example the updated <see cref="Matrix"/> transform.
        /// </summary>
        /// <param name="transformsToDrawList">The <see cref="InstancedModelPart"/> dictionary of transforms to draw</param>
        /// <param name="indexKey">Index position to affect in the given 'TransformsToDrawList' collection.</param>
        /// <param name="changeRequestItem"><see cref="ChangeRequestItem"/> providing updated data</param>
        private static void UpdateGivenTransformList(IDictionary<int, InstancedDataForDraw> transformsToDrawList, int indexKey, ref ChangeRequestItem changeRequestItem)
        {
            // check if entry exist in Culled List
            InstancedDataForDraw existingNode;
            if (transformsToDrawList.TryGetValue(indexKey, out existingNode))
            {
                // update value
                existingNode.Transform = changeRequestItem.Transform;
                // 6/6/2010 -  Stores the elapsed game time, used for shader explosions.
                existingNode.AccumElapsedTime = InstancedModel.AccumElapsedTime;
                // 6/6/2010 - Stores the Projectile's velocity, used for shader explosions.
                existingNode.ProjectileVelocity = changeRequestItem.ProjectileVelocity;

                // 8/28/2009 - The PlayerNumber is stored in the fractional portion of the float, while the FOW culling flag is stored in 
                //             the integer potion of the float.  The 'ModF' HLSL function is used to break these apart when needed.

                existingNode.PlayerNumberAndMaterialId = changeRequestItem.PlayerNumberAndMaterialId; // 8/28/2009 - Get combine PlayerNumber & FOW float.
                transformsToDrawList[indexKey] = existingNode;
            }
            else
            {
                // add new SceneItemOwner
                var newNode = new InstancedDataForDraw
                                  {
                                      Transform = changeRequestItem.Transform,
                                      AccumElapsedTime = InstancedModel.AccumElapsedTime, // 6/6/2010 -  Stores the elapsed game time, used for shader explosions.
                                      ProjectileVelocity = changeRequestItem.ProjectileVelocity, // 6/6/2010 - Stores the Projectile's velocity, used for shader explosions.
                                      PlayerNumberAndMaterialId = changeRequestItem.PlayerNumberAndMaterialId /* 8/28/2009 */
                                  };
                transformsToDrawList.Add(indexKey, newNode);
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
                if (ChangeRequestItemTypeKeys.TryGetValue(_changeRequestKeys[i], out index))
                {
                    //= ChangeRequestItemTypeKeys[_changeRequestKeys[i]];
                    DoChangeRequestForInstanceTransforms(InstancedItem.InstanceModels[index]);
                }
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
            // 1/7/2010
            if (instancedModel == null) return;
            try
            {
                // 4/21/2010 - Cache
                var changeRequests = instancedModel.InstancedModelChangeRequestManager.ChangeRequests;
                if (changeRequests == null) return;

                // 4/21/2010 - Cache
                var keys = changeRequests.Keys;
                if (keys == null) return;

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
                    InstancedDataCommunication instanceWorldData;
                    if (changeRequests.TryGetValue(_keys[i], out instanceWorldData))
                        instancedModel.UpdateInstanceTransforms(ref instanceWorldData);
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
