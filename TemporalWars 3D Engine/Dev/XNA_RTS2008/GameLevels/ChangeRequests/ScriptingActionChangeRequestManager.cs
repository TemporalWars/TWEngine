#region File Description
//-----------------------------------------------------------------------------
// ScriptingActionChangeRequestManager.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TWEngine.GameLevels.ChangeRequests.Enums;
using TWEngine.GameLevels.ChangeRequests.Structs;
using TWEngine.Interfaces;
using TWEngine.SceneItems;

namespace TWEngine.GameLevels.ChangeRequests
{
    // 5/22/2012
    /// <summary>
    /// The <see cref="ScriptingActionChangeRequestManager"/> class is used to generate new <see cref="IScriptingActionChangeRequest"/> actions, which
    /// affect the given <see cref="SceneItem"/>; for example, scale and rotation requests.
    /// </summary>
    public static class ScriptingActionChangeRequestManager
    {
        private static Guid[] _dictionaryKeys = new Guid[1];
        private static Dictionary<Guid, IScriptingActionChangeRequest>[] _dictionaryValues = new Dictionary<Guid, IScriptingActionChangeRequest>[1];
        private static IScriptingActionChangeRequest[] _internalDictionaryValues = new IScriptingActionChangeRequest[1];

        // 5/17/2012 - Stores a Queue of ScriptingAction change requests.
        //private static readonly Queue<IScriptingActionChangeRequest> ChangeRequestQueue = new Queue<IScriptingActionChangeRequest>();

        // 5/22/2012 - Stores a GUID ref between the 'SceneItem' and the ScriptingAction change request assigned to that item.
        private static readonly
            Dictionary<Guid, Dictionary<Guid, IScriptingActionChangeRequest>>
            ChangeRequestDictionary =
                new Dictionary<Guid, Dictionary<Guid, IScriptingActionChangeRequest>>();


        #region Properties

        /// <summary>
        /// Gets or sets the attributes required for the <see cref="CreateActionChangeRequest"/> factory method call.
        /// </summary>
        public static ScriptingActionChangeRequestAttributes ChangeRequestAttributes { get; set; }

        #endregion

        // 5/17/2012
        /// <summary>
        /// Checks the current <see cref="ScriptingActionScaleRequest"/> Queue for any current updates to complete.
        /// </summary>
        /// <remarks>The <see cref="GameLevelManager"/> calls this class during the 'Update' cycles.</remarks>
        /// <param name="gameTime">Instance of the <see cref="GameTime"/></param>
        internal static void DoChangeRequestUpdates(GameTime gameTime)
        {
            // Copy ChangeRequest keys
            var keysCount = ChangeRequestDictionary.Keys.Count;
            if (_dictionaryKeys.Length != keysCount)
                Array.Resize(ref _dictionaryKeys, keysCount);
            ChangeRequestDictionary.Keys.CopyTo(_dictionaryKeys, 0);

            // Copy ChangeRequest values
            var valuesCount = ChangeRequestDictionary.Values.Count;
            if (_dictionaryValues.Length != valuesCount)
                Array.Resize(ref _dictionaryValues, valuesCount);
            ChangeRequestDictionary.Values.CopyTo(_dictionaryValues, 0);

            // iterate outer keys collection
            var keysLength = _dictionaryKeys.Length;
            for (var i = 0; i < keysLength; i++)
            {
                var internalChangeRequestValue = _dictionaryValues[i];

                // Copy internal values
                var internalValuesCount = internalChangeRequestValue.Values.Count;
                if (_internalDictionaryValues.Length != internalValuesCount)
                    Array.Resize(ref _internalDictionaryValues, internalValuesCount);
                internalChangeRequestValue.Values.CopyTo(_internalDictionaryValues, 0);

                // iterate internal values collection
                var count = internalChangeRequestValue.Count;
                for (var j = 0; j < count; j++)
                {
                    var changeRequest = _internalDictionaryValues[j];
                    if (changeRequest == null)
                    {
                        continue;
                    }

                    if (changeRequest.IsCompleted || changeRequest.TerminateAction)
                    {
                        RemoveFromDictionary(changeRequest);
                        continue;
                    }

                    // update current ChangeRequest.
                    changeRequest.Update(gameTime);
                }
            }

        }

        // 6/9/2012
        /// <summary>
        /// Terminates all scripting actions for a specific sceneItem based on its <paramref name="uniqueKey"/>.
        /// </summary>
        /// <param name="uniqueKey"> </param>
        internal static void TerminateAllScriptingActions(Guid uniqueKey)
        {
            Dictionary<Guid, IScriptingActionChangeRequest> scriptingActionRequests;
            if (!ChangeRequestDictionary.TryGetValue(uniqueKey, out scriptingActionRequests))
            {
                return;
            }

            // Copy internal values
            var internalValuesCount = scriptingActionRequests.Values.Count;
            if (_internalDictionaryValues.Length != internalValuesCount)
                Array.Resize(ref _internalDictionaryValues, internalValuesCount);
            scriptingActionRequests.Values.CopyTo(_internalDictionaryValues, 0);

            // iterate internal values collection
            var count = scriptingActionRequests.Count;
            for (var j = 0; j < count; j++)
            {
                var changeRequest = _internalDictionaryValues[j];
                if (changeRequest == null)
                {
                    continue;
                }

                changeRequest.TerminateAction = true;
            }
        }

        // 5/22/2012
        /// <summary>
        /// Creates the requested <see cref="IScriptingActionChangeRequest"/> action and returns to caller.
        /// </summary>
        /// <param name="scriptingActionChangeRequestEnum">Type of scripting action to create.</param>
        /// <returns>Instance of <see cref="IScriptingActionChangeRequest"/>.</returns>
        internal static IScriptingActionChangeRequest CreateActionChangeRequest(ScriptingActionChangeRequestEnum scriptingActionChangeRequestEnum)
        {
            IScriptingActionChangeRequest changeRequest = null;

            switch (scriptingActionChangeRequestEnum)
            {
                case ScriptingActionChangeRequestEnum.RotationRequest:

                    // Create new ScriptingActionRotationRequest instance and queue.
                    changeRequest = new ScriptingActionRotationRequest(ChangeRequestAttributes.SceneItem, ChangeRequestAttributes.RotationType,
                                                                           ChangeRequestAttributes.RotationDirection,
                                                                           ChangeRequestAttributes.RotationTimeMax, ChangeRequestAttributes.InstancedItemPickedIndex) { DeltaMagnitude = ChangeRequestAttributes.DeltaMagnitude };

                    // Update the dictionary tracker
                    AddToDictionary(changeRequest.UniqueKey, changeRequest);

                    break;
                case ScriptingActionChangeRequestEnum.ScaleRequest:

                    // Create new ScriptingActionScaleRequest instance and queue.
                    changeRequest = new ScriptingActionScaleRequest(ChangeRequestAttributes.SceneItem, ChangeRequestAttributes.Scale,
                                                                    ChangeRequestAttributes.ScaleType,
                                                                    ChangeRequestAttributes.InstancedItemPickedIndex) { DeltaMagnitude = ChangeRequestAttributes.DeltaMagnitude };

                    // Update the dictionary tracker
                    AddToDictionary(changeRequest.UniqueKey, changeRequest);

                    break;
                case ScriptingActionChangeRequestEnum.PathMovementRequest:

                    // Create new ScriptingActionMovementRequest instance and queue.
                    changeRequest = new ScriptingActionMovementOnPathsRequest(ChangeRequestAttributes.SceneItem, ChangeRequestAttributes.InstancedItemPickedIndex);

                    // Update the dictionary tracker
                    AddToDictionary(changeRequest.UniqueKey, changeRequest);

                    break;
                case ScriptingActionChangeRequestEnum.MovementRequest:

                    // Create new ScriptingActionMovementRequest instance and queue.
                    changeRequest = new ScriptingActionMovementRequest(ChangeRequestAttributes.SceneItem, ChangeRequestAttributes.InstancedItemPickedIndex, ChangeRequestAttributes.MaxVelocity,
                                                                          ChangeRequestAttributes.StartPosition) { RotationForce = ChangeRequestAttributes.RotationForce, KeepOnGround = ChangeRequestAttributes.KeepOnGround };

                    // Update the dictionary tracker
                    AddToDictionary(changeRequest.UniqueKey, changeRequest);

                    break;
                case ScriptingActionChangeRequestEnum.TossMovementRequest:

                    // 6/6/2012 - Check if using GoalPosition or WaypointIndex constructor call
                    if (ChangeRequestAttributes.WaypointIndex == -1)
                    {
                        // Create new ScriptingActionTossMovementRequest instance and queue.
                        changeRequest = new ScriptingActionTossMovementRequest(ChangeRequestAttributes.SceneItem, ChangeRequestAttributes.GoalPosition, ChangeRequestAttributes.MaxVelocity,
                                                                               ChangeRequestAttributes.InstancedItemPickedIndex, ChangeRequestAttributes.AccuracyPercent,
                                                                               ChangeRequestAttributes.ErrorDistanceOffset, ChangeRequestAttributes.UpForce, ChangeRequestAttributes.ObjectWeight) 
                                                                               { RotationForce = ChangeRequestAttributes.RotationForce, 
                                                                                   UseLifeSpanCheck = ChangeRequestAttributes.UseLifeSpanCheck, MaxLifeSpan = ChangeRequestAttributes.MaxLifeSpan};
                    }
                    else
                    {
                        // Create new ScriptingActionTossMovementRequest instance and queue.
                        changeRequest = new ScriptingActionTossMovementRequest(ChangeRequestAttributes.SceneItem, ChangeRequestAttributes.WaypointIndex, ChangeRequestAttributes.MaxVelocity,
                                                                               ChangeRequestAttributes.InstancedItemPickedIndex, ChangeRequestAttributes.AccuracyPercent,
                                                                               ChangeRequestAttributes.ErrorDistanceOffset, ChangeRequestAttributes.UpForce, ChangeRequestAttributes.ObjectWeight) 
                                                                               { RotationForce = ChangeRequestAttributes.RotationForce, 
                                                                                   UseLifeSpanCheck = ChangeRequestAttributes.UseLifeSpanCheck, MaxLifeSpan = ChangeRequestAttributes.MaxLifeSpan };
                    }

                    // Update the dictionary tracker
                    AddToDictionary(changeRequest.UniqueKey, changeRequest);

                    break;
                default:
                    throw new ArgumentOutOfRangeException("scriptingActionChangeRequestEnum");
            }

            return changeRequest;
        }

        /// <summary>
        /// Adds a <see cref="IScriptingActionChangeRequest"/> for a given <see cref="SceneItem"/>.
        /// </summary>
        private static void AddToDictionary(Guid uniqueKey, IScriptingActionChangeRequest scriptingActionChangeRequest)
        {
            Dictionary<Guid, IScriptingActionChangeRequest> scriptingActionRequests;
            if (ChangeRequestDictionary.TryGetValue(ChangeRequestAttributes.SceneItem.UniqueKey, out scriptingActionRequests))
            {
                // check if given scriptingAction already exist for this SceneItem
                IScriptingActionChangeRequest changeRequest;
                if (scriptingActionRequests.TryGetValue(uniqueKey, out changeRequest))
                {
                    // then stop current operation
                    changeRequest.TerminateAction = true;

                    // override this ref with new ref; this is ok, since the Queue has its own ref to the original item.
                    scriptingActionRequests[uniqueKey] = scriptingActionChangeRequest;
                    
                }
                else
                {
                    // else, add new changeRequest
                    scriptingActionRequests.Add(uniqueKey, scriptingActionChangeRequest);
                }
            }
            else
            {
                // else, add new item.
                scriptingActionRequests = new Dictionary<Guid, IScriptingActionChangeRequest>
                                              {{uniqueKey, scriptingActionChangeRequest}};
                ChangeRequestDictionary.Add(ChangeRequestAttributes.SceneItem.UniqueKey, scriptingActionRequests);
            }
        }

        /// <summary>
        /// Removes the given instance of <paramref name="scriptingActionChangeRequest"/> from the dictionary.
        /// </summary>
        /// <param name="scriptingActionChangeRequest">Instance of <see cref="IScriptingActionChangeRequest"/> to remove.</param>
        private static void RemoveFromDictionary(IScriptingActionChangeRequest scriptingActionChangeRequest)
        {
            { // 6/6/2012 - Make sure the correct 'InstancedItemPickedIndex' is set!
                var sceneItemToUpdate = scriptingActionChangeRequest.SceneItemToUpdate;
                if (sceneItemToUpdate == null) return;

                // If ScenaryItem, then set to the proper index value.
                var scenaryItemScene = sceneItemToUpdate as ScenaryItemScene;
                if (scenaryItemScene != null)
                {
                    scenaryItemScene.InstancedItemPickedIndex = scriptingActionChangeRequest.InstancedItemPickedIndex;
                }
            }

            Dictionary<Guid, IScriptingActionChangeRequest> scriptingActionRequests;
            if (!ChangeRequestDictionary.TryGetValue(scriptingActionChangeRequest.SceneItemToUpdate.UniqueKey,
                                                     out scriptingActionRequests))
            {
                return;
            }


            // check if Rotation type.
            if (scriptingActionChangeRequest is ScriptingActionRotationRequest)
            {
                if (scriptingActionRequests.ContainsKey(scriptingActionChangeRequest.UniqueKey))
                {
                    scriptingActionRequests.Remove(scriptingActionChangeRequest.UniqueKey);
                }
            }

            // check if Scale type.
            if (scriptingActionChangeRequest is ScriptingActionScaleRequest)
            {
                if (scriptingActionRequests.ContainsKey(scriptingActionChangeRequest.UniqueKey))
                {
                    scriptingActionRequests.Remove(scriptingActionChangeRequest.UniqueKey);
                }
            }

            // 6/9/2012 - check if movement type.
            if (scriptingActionChangeRequest is ScriptingActionMovementRequest)
            {
                if (scriptingActionRequests.ContainsKey(scriptingActionChangeRequest.UniqueKey))
                {
                    scriptingActionRequests.Remove(scriptingActionChangeRequest.UniqueKey);
                }
            }

            // check if toss movement type.
            if (scriptingActionChangeRequest is ScriptingActionTossMovementRequest)
            {
                if (scriptingActionRequests.ContainsKey(scriptingActionChangeRequest.UniqueKey))
                {
                    scriptingActionRequests.Remove(scriptingActionChangeRequest.UniqueKey);
                }
            }
        }

        // 6/6/2012
        /// <summary>
        /// Used to unload resources during level loads.
        /// </summary>
        public static void UnloadContent()
        {
            if (ChangeRequestDictionary != null)
            {
                ChangeRequestDictionary.Clear();
            }
        }
    }
}