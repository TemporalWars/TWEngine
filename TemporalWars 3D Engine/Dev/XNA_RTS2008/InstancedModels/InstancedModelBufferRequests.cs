using System;
using System.Collections.Generic;
using System.Diagnostics;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Structs;
using ImageNexus.BenScharbach.TWTools.PerfTimersComponent.Timers;
using ImageNexus.BenScharbach.TWTools.PerfTimersComponent.Timers.Enums;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.InstancedModels
{
    // 10/16/2012
    /// <summary>
    /// This partial piece of the <see cref="InstancedModelPart"/> class extends the class
    /// with the inner class <see cref="InstancedModelBufferRequests"/>.
    /// </summary>
    internal sealed partial class InstancedModelPart
    {
        /// <summary>
        /// This inner class <see cref="InstancedModelBufferRequests"/> is used to process the BufferItem requests.
        /// </summary>
        internal class InstancedModelBufferRequests
        {
            ///<summary>
            /// Processes all changes requests within the ChangeBuffers for given
            /// <see cref="InstancedModel"/>.
            ///</summary>
            ///<param name="instancedModel"><see cref="InstancedModel"/> instance</param>
            public static void ProcessDoubleBuffers(InstancedModel instancedModel)
            {
#if DEBUG
                // 4/21/2010 - Debug Purposes           
                StopWatchTimers.StartStopWatchInstance(StopWatchName.IMPProcBuffs);
#endif
                //if (instancedModel.ItemTypeInUse == ItemType.treePalmNew002c)
                //  Debugger.Break();

                try
                {

                    //
                    // Process InstancedModelPart change buffers.
                    //

                    var instancedModelParts = instancedModel._modelParts;
                    var modelPartsKeys = instancedModel.ModelPartsKeys;

                    // 6/16/2010 - Check if null
                    if (modelPartsKeys == null) return;

                    // Iterate ModelParts
                    var modelPartsKeysCount = modelPartsKeys.Count; // 8/12/2009
                    for (var i = 0; i < modelPartsKeysCount; i++)
                    {
                        // Cache data 
                        var modelPartsIndex = modelPartsKeys[i];

                        // Cache modelPart 
                        var modelPart = instancedModelParts[modelPartsIndex];
                        if (modelPart == null) continue;

                        // Process DoubleBuffer at ModePart level.
                        ProcessDoubleBuffers(modelPart);

                    } // End For ModelParts Loop

                   
                }
                finally
                {
#if DEBUG
                    // 4/21/2010 - Debug Purposes
                    StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.IMPProcBuffs);
#endif
                }
            }

            /// <summary>
            /// Processes the double buffers, by iterating through the 'Current' double buffer, and processing
            /// all 'ChangeRequests' given.
            /// </summary>
            /// <param name="instancedModelPart"><see cref="instancedModelPart"/> to process</param>
            internal static void ProcessDoubleBuffers(InstancedModelPart instancedModelPart)
            {
                var changeBuffers = instancedModelPart.ChangeBuffers;

                // 6/1/2010 - Updated to use the 'PriorUpdateBuffer', since old way was causing high CPI of 17.0 in V-Tune!
                //var currentUpdateBuffer = InstancedModel.CurrentUpdateBuffer; // 4/20/2010 - Cache
                //var useDoubleBuffer = (currentUpdateBuffer == 0) ? 1 : 0;
                var useDoubleBuffer = PriorUpdateBuffer;

                // Get Keys to Dictionary
                var bufferRequestItems = changeBuffers[useDoubleBuffer]; // 6/1/2010 - Cache
                var changeBuffersCount = bufferRequestItems.Keys.Count;

                // 6/4/2010 - Track 'Dirty' flag - to know when changes take place per draw cycle.
                instancedModelPart._isDirty = false; // reset

                // don't waste time processing anything if empty changeBuffer!
                if (changeBuffersCount <= 0) return;

                // 6/4/2010 - There are changes, so set to 'Dirty'.
                instancedModelPart._isDirty = true;

                if (_keys.Length < changeBuffersCount)
                    Array.Resize(ref _keys, changeBuffersCount);
                bufferRequestItems.Keys.CopyTo(_keys, 0);

                // 6/1/2010 - Cache
                var keys = _keys;

                // Iterate through changeBuffer & update drawList
                for (var i = 0; i < changeBuffersCount; i++)
                {
                    // Cache key 
                    var instancedIndexKey = keys[i];

                    // 2/24/2011
                    try
                    {
                        // Retrieve ChangeRequestItem from internal Queue
                        var bufferRequestItem = bufferRequestItems[instancedIndexKey];

                        // Update TransformsLists (Culled/All).
                        UpdateInstancedModelBasedOnChangeRequestItem(instancedModelPart, instancedIndexKey, ref bufferRequestItem);

                    }
                    catch (KeyNotFoundException)
                    {
                        // Skip
                    }

                } // End For Loop

                // 7/21/2009 - Clear Current Buffers
                bufferRequestItems.Clear();

                // 10/15/2012 - To optimize, let's now copy the Dictionary transforms to a simple array.  This will eliminate the
                //              constanst calls to copy the data within the UpdateTransformsStream method call.
                DictionaryCopyToArray(ref instancedModelPart._instanceTransformsForDraw, instancedModelPart.TransformsToDrawList);
                //DictionaryCopyToArray(ref instancedModelPart._instanceTransformsForDrawAll, instancedModelPart.TransformsToDrawAllList);
            }


            /// <summary>
            /// Adds/Updates a <see cref="BufferRequestItem"/> into the current ChangeBuffer.
            /// </summary>
            /// <param name="modelPartIndex">Index value of <see cref="InstancedModelPart"/> in <see cref="InstancedModel._modelParts"/> collection.</param>
            /// <param name="itemInstanceKey"><see cref="ItemType"/> instance unique key</param>
            /// <param name="bufferRequestItem"><see cref="BufferRequestItem"/> structure</param>
            /// <param name="modelParts">Collection of <see cref="InstancedModelPart"/>.</param>
            internal static void EnterBufferRequestItemToCurrentChangeBuffer(int modelPartIndex, int itemInstanceKey,
                                                                             ref BufferRequestItem bufferRequestItem, IList<InstancedModelPart> modelParts)
            {
                // 11/6/2009
                try
                {
                    // 8/13/2009 - Cache
                    var changeBuffers = modelParts[modelPartIndex].ChangeBuffers;

                    // see if already have request for given instance.
                    BufferRequestItem currentBufferRequest;
                    var currentUpdateBuffer = InstancedModelChangeRequests.CurrentUpdateBuffer; // 1/5/2010; 6/1/2010 - Uses local now.
                    if (changeBuffers[currentUpdateBuffer].TryGetValue(itemInstanceKey, out currentBufferRequest))
                    {
                        // check if 'DeletePart' request, which takes priority.
                        if (currentBufferRequest.BufferRequest == BufferRequest.RemoveInstancedModelPart)
                            return;

                        // then update new request value.
                        currentBufferRequest = bufferRequestItem;

                        // store back into dictionary
                        changeBuffers[currentUpdateBuffer][itemInstanceKey] = currentBufferRequest;
                    }
                    else
                    {
                        // Save request into Dictionary
                        changeBuffers[currentUpdateBuffer].Add(itemInstanceKey, bufferRequestItem);
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
                catch (Exception e)
                {
                    Debug.WriteLine("EnterChangeRequestItemToCurrentChangeBuffer method threw the '{0}' error.", e.Message);
                }
            }

            /// <summary>
            /// Updates the <see cref="InstancedModel"/> 'TransformsToDrawList' collections, based on the <see cref="BufferRequestItem"/> given.
            /// </summary>
            /// <param name="instancedModelPart"><see cref="InstancedModelPart"/> instance</param>
            /// <param name="instanceIndexKey">Index position to affect in the given 'TransformsToDrawList' collection.</param>
            /// <param name="bufferRequestItem"><see cref="BufferRequestItem"/> structure</param>
            internal static void UpdateInstancedModelBasedOnChangeRequestItem(InstancedModelPart instancedModelPart, int instanceIndexKey, ref BufferRequestItem bufferRequestItem)
            {
                // Check Message type
                switch (bufferRequestItem.BufferRequest)
                {
                    case BufferRequest.ClearAllDrawTransformsForInstancedModel:
                        // 10/18/2012 - 
                        // Optimization: Updated to now call the new 'ClearInstancesCulledList' method, which clears all transforms
                        //               in all the modelParts with one call!  Prior, it was done with multiple BufferRequest calls for each modelPart.
                        if (instancedModelPart.Parent != null) 
                            instancedModelPart.Parent.ClearInstancesCulledList();
                        //instancedModelPart.TransformsToDrawList.Clear();
                        break;
                    case BufferRequest.AddOrUpdateInstancedModelPart:
                        // Update 'Culled' list
                        UpdateGivenTransformList(instancedModelPart, instanceIndexKey, ref bufferRequestItem);
                        break;
                    case BufferRequest.RemoveInstancedModelPart:
                        // yes, so remove from all lists
                        UpdateGivenTransformList(instancedModelPart, instanceIndexKey, ref bufferRequestItem);
                        if (instancedModelPart != null)
                        {
                            instancedModelPart.TransformsToDrawList.Remove(instanceIndexKey);
                            instancedModelPart.RemoveBufferRequestTransform(instanceIndexKey);
                        }
                        break;
                }
            }

            /// <summary>
            /// Checks the given 'TransformsToDrawList' dictionary, at the given <paramref name="instanceIndexKey"/>, processing the 
            /// updates stored in the <see cref="BufferRequestItem"/>, for example the updated <see cref="Matrix"/> transform.
            /// </summary>
            /// <param name="instancedModelPart"><see cref="InstancedModelPart"/> instance</param>
            /// <param name="instanceIndexKey">Index position to affect in the given 'TransformsToDrawList' collection.</param>
            /// <param name="bufferRequestItem"><see cref="BufferRequestItem"/> providing updated data</param>
            internal static void UpdateGivenTransformList(InstancedModelPart instancedModelPart, int instanceIndexKey, ref BufferRequestItem bufferRequestItem)
            {
                var transformsToDrawList = instancedModelPart.TransformsToDrawList;
                // 10/17/2012 - Get new BufferRequest transform
                var updatedTransform = instancedModelPart.GetBufferRequestTransform(instanceIndexKey);

                // check if entry exist in Culled List
                InstancedDataForDraw existingNode;
                if (transformsToDrawList.TryGetValue(instanceIndexKey, out existingNode))
                {
                    // update value
                    existingNode.Transform = updatedTransform; //bufferRequestItem.Transform; // 10/17/2012
                    // 6/6/2010 -  Stores the elapsed game time, used for shader explosions.
                    existingNode.AccumElapsedTime = InstancedModel.AccumElapsedTime;
                    // 8/28/2009 - The PlayerNumber is stored in the fractional portion of the float, while the FOW culling flag is stored in 
                    //             the integer potion of the float.  The 'ModF' HLSL function is used to break these apart when needed.
                    existingNode.PlayerNumberAndMaterialId = bufferRequestItem.PlayerNumberAndMaterialId; // 8/28/2009 - Get combine PlayerNumber & FOW float.

                    // 10/16/2012: Removed obsolete explosion code.
                    // 6/6/2010 - Stores the Projectile's velocity, used for shader explosions.
                    //existingNode.ProjectileVelocity = bufferRequestItem.ProjectileVelocity;

                    transformsToDrawList[instanceIndexKey] = existingNode;
                }
                else
                {
                    // add new SceneItemOwner
                    var newNode = new InstancedDataForDraw
                        {
                            Transform = updatedTransform, //bufferRequestItem.Transform, // 10/17/2012
                            AccumElapsedTime = InstancedModel.AccumElapsedTime, // 6/6/2010 -  Stores the elapsed game time, used for shader explosions.
                            PlayerNumberAndMaterialId = bufferRequestItem.PlayerNumberAndMaterialId, /* 8/28/2009 */
                        };

                    transformsToDrawList.Add(instanceIndexKey, newNode);
                }
            }

            /// <summary>
            /// Helper method which copies the given dictionary transforms to a simple array.
            /// </summary>
            private static void DictionaryCopyToArray(ref InstancedDataForDraw[] transformsToDrawArray, IDictionary<int, InstancedDataForDraw> transformsToDrawList)
            {
                var transformsCount = transformsToDrawList.Count;

                // NOTE: Do NOT try to cache the 'instancedModelPart._instanceTransforms', because will actually slow down the Pc, since causes HEAP garbage!
                // 4/21/2009; 5/14/2009: Updated to only Grow array, and not shrink.
                // Resize array, if necessary
                if (transformsToDrawArray.Length < transformsCount)
                    Array.Resize(ref transformsToDrawArray, transformsCount);

                // 10/15/2012: Optimization: Updated to now copy in the ProcessBuffers method call.
                transformsToDrawList.Values.CopyTo(transformsToDrawArray, 0);
            }
           
        }
    }
}