#region File Description
//-----------------------------------------------------------------------------
// ScriptingHelper.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using TWEngine.GameScreens;
using TWEngine.InstancedModels.Enums;
using TWEngine.ItemTypeAttributes;
using TWEngine.Players;
using TWEngine.SceneItems;
using TWEngine.Terrain;

namespace TWEngine.GameLevels
{
    // 5/27/2012
    /// <summary>
    /// The <see cref="ScriptingHelper"/> class holds common method calls that can be used between the <see cref="ScriptingActions"/> 
    /// and <see cref="ScriptingConditions"/>.
    /// </summary>
    public static class ScriptingHelper
    {
        /// <summary>
        /// Helper method which retrieves the given <see cref="SceneItem"/> based on the given <paramref name="sceneItemName"/>.
        /// Class types of <see cref="ScenaryItemScene"/> will automatically have their 'PickedIndex' set internally.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to retrieve.</param>
        /// <param name="pickedIndex">(OUT) The pickedIndex for a <see cref="ScenaryItemScene"/> type; -1 is default.</param>
        /// <returns>Instance of <see cref="SceneItem"/></returns>
        internal static SceneItem GetNamedItem(string sceneItemName, out int pickedIndex)
        {
            pickedIndex = -1;

            if (String.IsNullOrEmpty(sceneItemName))
            {
                throw new ArgumentNullException("sceneItemName");
            }

            // try get 'Named' sceneItem from Player class
            SceneItem namedSceneItem;
            if (Player.SceneItemsByName.TryGetValue(sceneItemName, out namedSceneItem))
            {
                // If ScenaryItem, then call the 'SearchByName' which sets the proper PickedIndex instance.
                var scenaryItemScene = namedSceneItem as ScenaryItemScene;
                if (scenaryItemScene != null)
                {
                    scenaryItemScene.SearchByName(sceneItemName);
                    pickedIndex = scenaryItemScene.InstancedItemPickedIndex;
                }

                return namedSceneItem;
            }

            throw new ArgumentException(@"Given sceneItem Name is NOT VALID!", "sceneItemName");
        }

        // 5/29/2012
        /// <summary>
        /// Gets an existing waypoint using the given <paramref name="waypointIndex"/>.
        /// </summary>
        /// <param name="waypointIndex">Index key for waypoint.</param>
        /// <returns><see cref="Vector3"/> as waypoint position.</returns>
        internal static Vector3 GetExistingWaypoint(int waypointIndex)
        {
            // get location for given waypoint index
            Vector3 position;
            if (!TerrainWaypoints.GetExistingWaypoint(waypointIndex, out position))
            {
                throw new ArgumentOutOfRangeException("waypointIndex", "Waypoint index given does not exist!");
            }

            return position;
        }

        // 5/29/2012
        /// <summary>
        /// Checks if the given waypoint exist and throws exception if it does not.
        /// </summary>
        /// <param name="waypointIndex">Index key for waypoint.</param>
        internal static void DoesWaypointExist(int waypointIndex)
        {
            // check if index exist.
            if (!TerrainWaypoints.Waypoints.ContainsKey(waypointIndex))
            {
                throw new ArgumentOutOfRangeException("waypointIndex", "Waypoint index given does not exist.");
            }
        }

        /// <summary>
        /// Helper method which spawns some ItemType to the game world.
        /// </summary>
        /// <param name="playerNumber"><see cref="Player"/> to add <see cref="ItemType"/> to</param>
        /// <param name="itemTypeToSpawn"><see cref="ItemType"/> enum to spawn; for example 'SciFiTank01' enum.</param>
        /// <param name="player">Instance of <see cref="Player"/></param>
        /// <param name="goalPosition">Position to spawn item at.</param>
        /// <param name="newScale">(Optional) Set to new 'Scale' value.</param>
        /// <param name="sceneItemName">(Optional) Name to give the SceneItem.</param>
        internal static void SpawnItemType(byte playerNumber, ItemType itemTypeToSpawn, Player player, Vector3 goalPosition, float newScale, string sceneItemName)
        {
            // 5/17/2012 - Check if playable item
            if (PlayableItemTypeAtts.ItemTypeAtts.ContainsKey(itemTypeToSpawn))
            {
                // Create SceneItem
                var sceneItemNumber = Player.AddSceneItem(player, itemTypeToSpawn, goalPosition);

                // Get Instance just created, to finish populating with data
                SceneItemWithPick newSceneItem;
                Player.GetSelectableItem(player, sceneItemNumber, out newSceneItem);

                // 5/17/2012
                if (newSceneItem == null)
                {
                    throw new NullReferenceException("Not a valid Selectable Item.");
                }

                // 5/31/2012 - set flag to designate this item was created dynamically with a scripting call.
                newSceneItem.SpawnByScriptingAction = true;

                // Update playerNumber
                newSceneItem.PlayerNumber = playerNumber;

                // Set SceneItemName
                newSceneItem.Name = sceneItemName;

                // Set Scale
                newSceneItem.Scale = new Vector3(newScale);
               
            }
            else // Add ScenaryItem
            {
                int indexLocation = 0;
                ScenaryItemScene scenaryItemScene;
                if (TerrainScreen.GetSceneItemInstance(itemTypeToSpawn, out scenaryItemScene))
                {
                    indexLocation = scenaryItemScene.AddScenaryItemSceneInstance(itemTypeToSpawn, ref goalPosition, 0, null);
                }
                else
                {
                    scenaryItemScene = new ScenaryItemScene(TemporalWars3DEngine.GameInstance, itemTypeToSpawn, ref goalPosition, 0);
                }

                // Set Index into ScenaryItem; otherwise, the update will not affect the proper item.
                scenaryItemScene.InstancedItemPickedIndex = indexLocation;

                // 5/31/2012 - set flag to designate this item was created dynamically with a scripting call.
                scenaryItemScene.SpawnByScriptingAction = true;

                // Set SceneItemName
                scenaryItemScene.Name = sceneItemName;

                // Set Scale
                scenaryItemScene.Scale = new Vector3(newScale);

                // If PathBlocked, then let's update the A* GraphNodes
                if (scenaryItemScene.ShapeItem.IsPathBlocked)
                {
                    // Set AStarGraph Costs
                    scenaryItemScene.SetAStarCostsForCurrentItem();

                    TerrainShape.PopulatePathNodesArray();
                }

                TerrainQuadTree.UpdateSceneryCulledList = true;
            }
        }
    }
}