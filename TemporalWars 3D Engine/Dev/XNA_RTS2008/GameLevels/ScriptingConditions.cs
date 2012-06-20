#region File Description
//-----------------------------------------------------------------------------
// ScriptingConditions.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using TWEngine.GameCamera;
using TWEngine.GameLevels.Delegates;
using TWEngine.IFDTiles;
using TWEngine.IFDTiles.Enums;
using TWEngine.InstancedModels.Enums;
using TWEngine.Players;
using TWEngine.SceneItems;
using TWEngine.SceneItems.Enums;
using TWEngine.Terrain;


namespace TWEngine.GameLevels
{
    // 10/3/2009
    ///<summary>
    /// The <see cref="ScriptingConditions"/> class, provides actionable real-time
    /// in game checks, like is <see cref="Camera"/> at position 'X', or does <see cref="TerrainTriggerAreas"/> contain
    /// the specific named <see cref="SceneItem"/>, and more, while abstracting the user from the lower engine level details in achieving
    /// these goals. Ideally, this class should be used within the <see cref="GameLevel"/>s and <see cref="GameLevelPart"/>s, 
    /// along with its companion <see cref="ScriptingActions"/> class, to create fully scripted game levels.
    ///</summary>
    public static class ScriptingConditions
    {

#if WithLicense
#if !XBOX
        // 5/10/2012 - License
        private static readonly LicenseHelper LicenseInstance;
#endif
#endif

        // 10/9/2009 - 
        ///<summary>
        /// UI State Enums, used for the method <see cref="ScriptingConditions.UserInterfaceStatePropositionIsTrue"/>.
        ///</summary>
        public enum UIStateProposition
        {
            ///<summary>
            /// Is Infantry <see cref="IFDTileManager"/> page open?
            ///</summary>
            InfantryBuildPageIsOpen,
            ///<summary>
            /// Is Vehicle <see cref="IFDTileManager"/> page open?
            ///</summary>
            VehicleBuildPageIsOpen,
            ///<summary>
            /// Is Aircraft <see cref="IFDTileManager"/> page open?
            ///</summary>
            AircraftBuildPageIsOpen,
            ///<summary>
            /// Is Buildings <see cref="IFDTileManager"/> page open?
            ///</summary>
            MainStructureBuildPageIsOpen, // Buildings.
            ///<summary>
            /// Is user currently placing some structure?
            ///</summary>
            PlacingAStructure,
        }

        /// <summary>
        /// constructor
        /// </summary>
        static ScriptingConditions()
        {

#if WithLicense
#if !XBOX
            // 5/10/2012 Check for Valid License.
            LicenseInstance = new LicenseHelper();
#endif
#endif
        }

        #region Misc

        // 1/19/2011
        /// <summary>
        /// Returns the status of the GameTrialOver flag.
        /// </summary>
        public static bool IsGameTrialOver
        {
            get
            {
#if WithLicense
#if !XBOX
                // 5/10/2012 - Return if trial.
                if (LicenseInstance.IsTrial)
                {
                    throw new InvalidOperationException("'IsGameTrialOver' Valid ONLY in FULL PAID Version!");
                }
#endif
#endif
                return TemporalWars3DEngine.IsGameTrialOver;
            }
        }

        // 1/19/2011
        /// <summary>
        /// Checks if the current game is a trial or fully purchased copy.
        /// </summary>
        /// <value>True/False of result.</value>
        public static bool IsGamePurchased
        {
            get
            {
#if WithLicense
#if !XBOX
                // 5/10/2012 - Return if trial.
                if (LicenseInstance.IsTrial)
                {
                    throw new InvalidOperationException("'IsGamePurchased' Valid ONLY in FULL PAID Version!");
                }
#endif
#endif
                return TemporalWars3DEngine.IsPurchasedGame;
            }
        }

        /// <summary>
        /// Checks if game is running on a Pc platform.
        /// </summary>
        /// <returns>True/False of result.</returns>
        public static bool IsGameRunningOnPlatformPc()
        {
            return TemporalWars3DEngine.CurrentPlatform != PlatformID.Xbox;
        }

        /// <summary>
        /// Checks if game is running on a Xbox-360 platform.
        /// </summary>
        /// <returns>True/False of result</returns>
        public static bool IsGameRunningOnPlatformXbox()
        {
            return TemporalWars3DEngine.CurrentPlatform == PlatformID.Xbox;
        }

        // 10/9/2009
        /// <summary>
        /// Checks if two given Named <see cref="SceneItem"/>s are some N Comparison distance apart.
        /// </summary>
        /// <param name="sceneItemName1">Name of sceneItem#1</param>
        /// <param name="sceneItemName2">Name of sceneItem#2</param>
        /// <param name="comparisonToDo">Lamba Func format: 'var => var > 5'; example would be 'distance => distance > 5'</param>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="sceneItemName1"/> is not valid.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="sceneItemName2"/> is not valid.</exception>
        /// <returns>True/False of result</returns>
        public static bool CompareDistanceBetweenTwoNamedItems(string sceneItemName1, string sceneItemName2, ScriptFunc<float, bool> comparisonToDo)
        {
            // 1st - retrieve 'Named' item from Dictionary
            SceneItem namedItemToCheck;
            if (Player.SceneItemsByName.TryGetValue(sceneItemName1, out namedItemToCheck))
            {
                // 1st
                // Check if 'namedItemToCheck#1' is a scenaryItem; if so, then it will contain
                // an internal array of instances, and therefore, the proper instance key needs
                // to be set, which affects what 'Position' will be returned at the base level Property
                // call!  This will affect the outcome of the 'WithinView' check below!
                var scenaryItem = (namedItemToCheck as ScenaryItemScene);
                if (scenaryItem != null)
                {
                    // Sets the internal PickedIndex to the correct position, when the 'Name' if found.
                    if (!scenaryItem.SearchByName(sceneItemName1))
                        throw new InvalidOperationException("SceneItem 'Name'#1 given does not exits!  Verify your map contains some SceneItem with this name.");
                }

                // 2nd - Do comparison to name-2.
                SceneItem namedItemToCheck2;
                if (Player.SceneItemsByName.TryGetValue(sceneItemName2, out namedItemToCheck2))
                {
                    // Check if 'namedItemToCheck#2' is a scenaryItem; if so, then it will contain
                    // an internal array of instances, and therefore, the proper instance key needs
                    // to be set, which affects what 'Position' will be returned at the base level Property
                    // call!  This will affect the outcome of the 'WithinView' check below!
                    var scenaryItem2 = (namedItemToCheck2 as ScenaryItemScene);
                    if (scenaryItem2 != null)
                    {
                        // Sets the internal PickedIndex to the correct position, when the 'Name' if found.
                        scenaryItem2.SearchByName(sceneItemName2);
                    }

                    // Get Position of item#1
                    var position1 = namedItemToCheck.Position;
                    // Get Position of item#2
                    var position2 = namedItemToCheck2.Position;

                    // Calculate distance between items
                    float distanceBetweenItems;
                    Vector3.Distance(ref position1, ref position2, out distanceBetweenItems);

                    // do lamba func check
                    return comparisonToDo(distanceBetweenItems);
                }

                throw new InvalidOperationException("SceneItem 'Name'#2 given does not exits!  Verify your map contains some SceneItem with this name.");
            }

            throw new InvalidOperationException("SceneItem 'Name'#1 given does not exits!  Verify your map contains some SceneItem with this name.");
        }


        // 10/9/2009
        /// <summary>
        /// Checks if given <see cref="Player"/>'s has the <see cref="UIStateProposition"/> enum TRUE; for example, could
        /// check for the enum 'VehicleBuildingIsOpen', or the 'PlacingAStructure' enum. (Tutorial Use)
        /// </summary>
        /// <param name="uiStateProposition"><see cref="UIStateProposition"/> Enum to check</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="uiStateProposition"/> is not valid.</exception>
        /// <returns>True/False of result</returns>
        public static bool UserInterfaceStatePropositionIsTrue(UIStateProposition uiStateProposition)
        {
            // check which UIState user wants to check
            switch (uiStateProposition)
            {
                case UIStateProposition.InfantryBuildPageIsOpen:
                    // then check 'ActiveGroup' setting of GroupControl
                    if (IFDTileGroupControl.ActiveGroup == IFDGroupControlType.People)
                        return true;
                    break;
                case UIStateProposition.VehicleBuildPageIsOpen:
                    // then check 'ActiveGroup' setting of GroupControl
                    if (IFDTileGroupControl.ActiveGroup == IFDGroupControlType.Vehicles)
                        return true;
                    break;
                case UIStateProposition.AircraftBuildPageIsOpen:
                    // then check 'ActiveGroup' setting of GroupControl
                    if (IFDTileGroupControl.ActiveGroup == IFDGroupControlType.Airplanes)
                        return true;
                    break;
                case UIStateProposition.MainStructureBuildPageIsOpen:
                    // then check 'ActiveGroup' setting of GroupControl
                    if (IFDTileGroupControl.ActiveGroup == IFDGroupControlType.Buildings)
                        return true;
                    break;
                case UIStateProposition.PlacingAStructure:
                    // then check if 'AttemptingItemPlacement' is TRUE, in the InterfaceDisplay class.
                    if (IFDTileManager.AttemptingItemPlacement)
                        return true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("uiStateProposition");
            }

            return false;
        }

        // 10/10/2009
        /// <summary>
        /// Checks if given <see cref="ItemType"/> is currently being selected.
        /// </summary>
        /// <param name="itemTypeToCheck"><see cref="ItemType"/> to check; for example, SciFiTank4.</param>
        /// <returns>True/False of result</returns>
        public static bool ItemTypeIsCurrentlySelected(ItemType itemTypeToCheck)
        {
            // 6/15/2010 - get Players array
            Player[] playersArray;
            TemporalWars3DEngine.GetPlayers(out playersArray);

            // make sure not NULL
            if (playersArray == null) return false;

            // Iterate 'Player' selectableItems, and check each ItemType for 'PickSelected'.
            var length = playersArray.Length; // 4/30/2010 - Cache
            for (var i = 0; i < length; i++)
            {
                // 6/15/2010 - Updated to use new GetPlayer method.
                Player player;
                if (!TemporalWars3DEngine.GetPlayer(i, out player))
                    break;

                // make sure not NULL
                if (player == null) continue;

                // 6/15/2010 - Updated to retrieve the ROC collection.
                ReadOnlyCollection<SceneItemWithPick> itemsSelected;
                Player.GetItemsSelected(player, out itemsSelected);

                // iterate 'itemsSelected' array
                var count = itemsSelected.Count; // 4/30/2010
                for (var j = 0; j < count; j++)
                {
                    // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                    SceneItemWithPick selected; 
                    if (!Player.GetItemsSelectedByIndex(player, j, out selected))
                        break;

                    // make sure not NULL
                    if (selected == null) continue;

                    // check 'PickedSelected' Property && ItemType, and if TRUE, stop search and return.
                    if (selected.PickSelected && selected.ShapeItem.ItemType == itemTypeToCheck)
                        return true;
                }
            } // End Loop Players

            return false;
        }

        #endregion

        #region Camera

        // 6/7/2012
        /// <summary>
        /// Checks if the camera has finished following some waypointPath.
        /// </summary>
        /// <param name="waypointPathName">A <see cref="TerrainWaypoints"/> path to check.</param>
        /// <returns>true/false of result.</returns>
        public static bool CameraFinishedFollowingWaypointPath(string waypointPathName)
        {
            if (CameraCinematics.CinematicSplinesCompleted.ContainsKey(waypointPathName))
            {
                return CameraCinematics.CinematicSplinesCompleted[waypointPathName];
            }

            return false;
        }

        /// <summary>
        /// Checks if <see cref="Camera.CameraPosition"/> is within the given <see cref="TerrainTriggerAreas"/>.
        /// </summary>
        /// <param name="triggerAreaName"><see cref="TerrainTriggerAreas"/> name</param>
        /// <returns>True/False of result</returns>
        public static bool CameraEnteredTriggerArea(string triggerAreaName)
        {
            return TerrainTriggerAreas.TriggerAreaContainsCamera(triggerAreaName);
        }

        // 10/9/2009
        /// <summary>
        /// Checks if the <see cref="Camera"/> was Reset by <see cref="Player"/>. (Tutorial use)
        /// </summary>
        /// <returns>True/False of result</returns>
        public static bool CameraWasResetByPlayer()
        {
            return Camera.CameraWasReset;
        }

        // 10/9/2009
        /// <summary>
        /// Checks if <see cref="Player"/> rotated the <see cref="Camera"/> N degrees to the right.  (Tutorial use)
        /// </summary>
        /// <remarks> 
        /// Measurement is defined by a constant continious movement.  Therefore, once user
        /// stops rotation, the counter internally is reset.
        /// </remarks>
        /// <param name="degreesRotated">Float value in range of 1-360</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="degreesRotated"/> is outside range of 1-360.</exception>
        /// <returns>True/False of result</returns>
        public static bool CameraWasRotatedRightByPlayerNDegrees(float degreesRotated)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'CameraWasRotatedRightByPlayerNDegrees' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif

            // make sure value is between 1 and 360.
            if (degreesRotated < 1 || degreesRotated > 360)
                throw new ArgumentOutOfRangeException("degreesRotated", @"Degrees must be a value between 1 and 360!");

            return MathHelper.ToDegrees(Camera.RotateRightCounter) >= degreesRotated;
        }

        // 10/9/2009
        /// <summary>
        /// Checks if <see cref="Player"/> rotated the <see cref="Camera"/> N degrees to the left.  (Tutorial use)
        /// </summary>
        /// <remarks> 
        /// Measurement is defined by a constant continious movement.  Therefore, once user
        /// stops rotation, the counter internally is reset.
        /// </remarks>
        /// <param name="degreesRotated">Float value in range of 1-360</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="degreesRotated"/> is outside range of 1-360.</exception>
        /// <returns>True/False of result</returns>
        public static bool CameraWasRotatedLeftByPlayerNDegrees(float degreesRotated)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'CameraWasRotatedLeftByPlayerNDegrees' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // make sure value is between 1 and 360.
            if (degreesRotated < 1 || degreesRotated > 360)
                throw new ArgumentOutOfRangeException("degreesRotated", @"Degrees must be a value between 1 and 360!");

            return  MathHelper.ToDegrees(Camera.RotateLeftCounter) >= degreesRotated;
        }

        // 10/9/2009
        /// <summary>
        /// Checks if <see cref="Player"/> rotated the <see cref="Camera"/> N degrees to the right or left.  (Tutorial use)
        /// </summary>
        /// <remarks> 
        /// Measurement is defined by a constant continious movement.  Therefore, once user
        /// stops rotation, the counter internally is reset.
        /// </remarks>
        /// <param name="degreesRotated">Float value in range of 1-360</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="degreesRotated"/> is outside range of 1-360.</exception>
        /// <returns>True/False of result</returns>
        public static bool CameraWasRotatedByPlayerNDegrees(float degreesRotated)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'CameraWasRotatedByPlayerNDegrees' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // make sure value is between 1 and 360.
            if (degreesRotated < 1 || degreesRotated > 360)
                throw new ArgumentOutOfRangeException("degreesRotated", @"Degrees must be a value between 1 and 360!");

            return  MathHelper.ToDegrees(Camera.RotateRightCounter) >= degreesRotated 
                ||  MathHelper.ToDegrees(Camera.RotateLeftCounter) >= degreesRotated;
        }

        // 10/9/2009
        /// <summary>
        /// Checks if <see cref="Player"/> scrolled the <see cref="Camera"/>, either forward, backward, left or right, N distance. (Tutorial use)
        /// </summary>
        /// <remarks> 
        /// Measurement is defined by a constant continious movement.  Therefore, once user
        /// stops rotation, the counter internally is reset.
        /// </remarks>
        /// <param name="distanceMoved">Float value 1 or greater</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="distanceMoved"/> is less than 1.</exception>
        /// <returns>True/False of result</returns>
        public static bool CameraWasScrolledByPlayerNDistance(float distanceMoved)
        {
            // make sure value is greater than zero.
            if (distanceMoved < 1)
                throw new ArgumentOutOfRangeException("distanceMoved", @"Distance given must be a value 1 or greater.");

            return Camera.ScrolledDistance >= distanceMoved;
        }

        // 10/9/2009
        /// <summary>
        /// Checks if <see cref="Player"/> zoomed OUT the <see cref="Camera"/> some N distance. (Tutorial use)
        /// </summary>
        /// <remarks> 
        /// Measurement is defined by a constant continious movement.  Therefore, once user
        /// stops rotation, the counter internally is reset.
        /// </remarks>
        /// <param name="distanceMoved">Float value 1 or greater</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="distanceMoved"/> is less than 1.</exception>
        /// <returns>True/False of result</returns>
        public static bool CameraWasZoomedOutByPlayerNDistance(float distanceMoved)
        {
            // make sure value is greater than zero.
            if (distanceMoved < 1)
                throw new ArgumentOutOfRangeException("distanceMoved", @"Distance given must be a value 1 or greater.");

            return Camera.ZoomOutCounter >= distanceMoved;
        }

        // 10/9/2009
        /// <summary>
        /// Checks if <see cref="Player"/> zoomed IN the <see cref="Camera"/> some N distance. (Tutorial use)
        /// </summary>
        /// <remarks> 
        /// Measurement is defined by a constant continious movement.  Therefore, once user
        /// stops rotation, the counter internally is reset.
        /// </remarks>
        /// <param name="distanceMoved">Float value 1 or greater</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="distanceMoved"/> is less than 1.</exception>
        /// <returns>True/False of result</returns>
        public static bool CameraWasZoomedInByPlayerNDistance(float distanceMoved)
        {
            // make sure value is greater than zero.
            if (distanceMoved < 1)
                throw new ArgumentOutOfRangeException("distanceMoved", @"Distance given must be a value 1 or greater.");

            return Camera.ZoomInCounter >= distanceMoved;
        }

        #endregion

        #region Player


        /// <summary>
        /// Checks if given <see cref="Player"/> has units within the given <see cref="TerrainTriggerAreas"/>.
        /// </summary>
        /// <param name="playerNumber">PlayerNumber of <see cref="Player"/> to check</param>
        /// <param name="triggerAreaName"><see cref="TerrainTriggerAreas"/> name</param>
        /// <returns>True/False of result</returns>
        public static bool PlayerHasUnitsInTriggerArea(int playerNumber, string triggerAreaName)
        {
            // 4th param uses the Lamba Fn: Var => var is BuildingScene, which will exlude the BuildingScene type from the results!
            return TerrainTriggerAreas.DoUnitItemsInTriggerAreaCheck(playerNumber, triggerAreaName, true, selectableItem => selectableItem is BuildingScene);
        }

        // 2/7/2011 - Overload method#2
        /// <summary>
        /// Checks if given <see cref="Player"/> has units within the given <see cref="TerrainTriggerAreas"/>.
        /// </summary>
        /// <param name="playerNumber">PlayerNumber of <see cref="Player"/> to check</param>
        /// <param name="triggerAreaName"><see cref="TerrainTriggerAreas"/> name</param>
        /// <param name="comparisonToDo">Lamba Func format: 'var => var > 5'; example would be 'cash => cash > 5'</param>
        /// <returns>True/False of result</returns>
        public static bool PlayerHasUnitsInTriggerArea(int playerNumber, string triggerAreaName, ScriptFunc<int, bool> comparisonToDo)
        {
            // 4th param uses the Lamba Fn: Var => var is BuildingScene, which will exlude the BuildingScene type from the results!
            return TerrainTriggerAreas.DoUnitItemsInTriggerAreaCheck(playerNumber, triggerAreaName,
                                                                    true, selectableItem => selectableItem is BuildingScene, comparisonToDo);
        }


        /// <summary>
        /// Checks if given <see cref="Player"/> has some sceneItem type within the given <see cref="TerrainTriggerAreas"/>.  
        /// For example, a SceneItem type of 'SciFiTankScene'.
        /// </summary>
        /// <param name="playerNumber">PlayerNumber of <see cref="Player"/> to check</param>
        /// <param name="triggerAreaName"><see cref="TerrainTriggerAreas"/> name</param>
        /// <param name="itemType">Lamba Fn: 'var => var is itemType', where itemType is some 'SceneItem' class type.</param>
        /// <returns>True/False of result</returns>
        public static bool PlayerHasSelectableItemTypeInTriggerArea(int playerNumber, string triggerAreaName, ScriptFunc<SceneItem, bool> itemType)
        {
            return TerrainTriggerAreas.DoUnitTypeInTriggerAreaCheck(playerNumber, triggerAreaName, itemType);
        }

        // 2/7/2011 - Overload method#2
        /// <summary>
        /// Checks if given <see cref="Player"/> has some sceneItem type within the given <see cref="TerrainTriggerAreas"/>.  
        /// For example, a SceneItem type of 'SciFiTankScene'.
        /// </summary>
        /// <param name="playerNumber">PlayerNumber of <see cref="Player"/> to check</param>
        /// <param name="triggerAreaName"><see cref="TerrainTriggerAreas"/> name</param>
        /// <param name="itemType">Lamba Fn: 'var => var is itemType', where itemType is some 'SceneItem' class type.</param>
        /// <param name="comparisonToDo">Lamba Func format: 'var => var > 5'; example would be 'cash => cash > 5'</param>
        /// <returns>True/False of result</returns>
        public static bool PlayerHasSelectableItemTypeInTriggerArea(int playerNumber, string triggerAreaName,
                                                                    ScriptFunc<SceneItem, bool> itemType, ScriptFunc<int, bool> comparisonToDo)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'PlayerHasSelectableItemTypeInTriggerArea' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            return TerrainTriggerAreas.DoUnitTypeInTriggerAreaCheck(playerNumber, triggerAreaName, itemType , comparisonToDo);
        }

        // 10/4/2009
        /// <summary>
        /// Helper method, which checks if the given PlayerNumber is valid within the
        /// Players array, and if the <see cref="Player"/> is not NULL!
        /// </summary>
        /// <param name="playerNumber">PlayerNumber to verify</param>
        /// <returns>True/False of result</returns>
        private static bool DoPlayerArgCheck(int playerNumber)
        {
            // 6/15/2010 - get Players array
            Player[] players;
            TemporalWars3DEngine.GetPlayers(out players);

            if (playerNumber >= players.Length)
                throw new ArgumentOutOfRangeException("playerNumber", @"Invalid PlayerNumber given!");

            // make sure not null
            return players[playerNumber] != null;
        }


        /// <summary>
        /// Checks if given <see cref="Player"/> has destroyed N (comparison) number of opponents <see cref="BuildingScene"/>s.
        /// </summary>
        /// <param name="playerNumber">PlayerNumber of <see cref="Player"/> to check</param>
        /// <param name="comparisonToDo">Lamba Func format: 'var => var > 5'; example would be 'cash => cash > 5'</param>
        /// <returns>True/False of result</returns>
        public static bool PlayerHasDestroyed_NComparison_OfOpponentsBuildings(int playerNumber, ScriptFunc<int, bool> comparisonToDo)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'PlayerHasDestroyed_NComparison_OfOpponentsBuildings' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return false;
            
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return false;

            if (player == null) return false;

            // retrieve given player's stats for Buildings destroyed.
            var valueToCheck = player.PlayerStats.EnemyBuildingsDestroyed;

            // do lamba func check
            return comparisonToDo(valueToCheck);
        }

        /// <summary>
        /// Checks if given <see cref="Player"/> has destroyed N (comparison) number of opponents 'Units'.
        /// </summary>
        /// <param name="playerNumber">PlayerNumber of <see cref="Player"/> to check</param>
        /// <param name="comparisonToDo">Lamba Func format: 'var => var > 5'; example would be 'cash => cash > 5'</param>
        /// <remarks>
        /// 'Units' can be either <see cref="SciFiTankScene"/> items or <see cref="SciFiAircraftScene"/> items, for example.
        /// </remarks>
        /// <returns>True/False of result</returns>
        public static bool PlayerHasDestroyed_NComparison_OfOpponentsUnits(int playerNumber, ScriptFunc<int, bool> comparisonToDo)
        {
            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return false;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return false;

            if (player == null) return false;

            // retrieve given player's stats for Buildings destroyed.
            var valueToCheck = player.PlayerStats.EnemyUnitsDestroyed;

            // do lamba func check
            return comparisonToDo(valueToCheck);
        }


        /// <summary>
        /// Checks if given <see cref="Player"/> has destroyed N (comparison) number of opponents <see cref="ItemType"/> enum.
        /// </summary>
        /// <param name="playerNumber">PlayerNumber of <see cref="Player"/> to check</param>
        /// <param name="itemTypeToRetrieve"><see cref="ItemType"/> enum to retrieve; for example 'SciFiTank01' enum.</param>
        /// <param name="comparisonToDo">Lamba Func format: 'var => var > 5'; example would be 'cash => cash > 5'</param>
        /// <returns>True/False of result</returns>
        public static bool PlayerHasDestroyed_NComparison_OfOpponentsItemType(int playerNumber, ItemType itemTypeToRetrieve, ScriptFunc<int, bool> comparisonToDo)
        {
            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return false;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return false;

            if (player == null) return false;

            // retrieve given player's stats for Buildings destroyed.
            var valueToCheck =
                PlayerStats.GetItemTypeKillStats(player.PlayerStats, itemTypeToRetrieve);

            // do lamba func check
            return comparisonToDo(valueToCheck);
        }


        /// <summary>
        /// Checks if given <see cref="Player"/> belongs to given faction side number.
        /// </summary>
        /// <param name="playerNumber">PlayerNumber of <see cref="Player"/> to check</param>
        /// <param name="playerSide"><see cref="Player"/> side to check (1 or 2)</param>
        /// <returns>True/False of result</returns>
        public static bool PlayerIsSide(int playerNumber, int playerSide)
        {
            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return false;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return false;
            
            if (player == null) return false;

            // check which side this player belongs to
            return player.PlayerSide == playerSide;
        }


        // 10/4/2009
        /// <summary>
        /// Checks if given <see cref="Player"/> has N (comparison) number of <see cref="Player.Cash"/> credits.
        /// </summary>
        /// <param name="playerNumber">PlayerNumber of <see cref="Player"/> to check</param>
        /// <param name="comparisonToDo">Lamba Func format: 'var => var > 5'; example would be 'cash => cash > 5'.</param>
        /// <returns>True/False of result</returns>
        public static bool PlayerHas_NComparison_NumberOfCredits(int playerNumber, ScriptFunc<int, bool> comparisonToDo)
        {
            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return false;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return false;

            if (player == null) return false;

            // retrieve players credits
            var valueToCheck = player.Cash;

            // do lamba func check
            return comparisonToDo(valueToCheck);
        }

        // 10/4/2009
        /// <summary>
        /// Checks if given <see cref="Player"/> has N (comparison) number of created <see cref="ItemType"/>s.
        /// </summary>
        /// <param name="playerNumber">PlayerNumber of <see cref="Player"/> to check</param>
        /// <param name="itemTypeToRetrieve"><see cref="ItemType"/> enum to retrieve; for example 'SciFiTank01' enum.</param>
        /// <param name="comparisonToDo">Lamba Func format: 'var => var > 5'; example would be 'cash => cash > 5'</param>
        /// <returns>True/False of result</returns>
        public static bool PlayerHas_NComparison_OfSelectableItem(int playerNumber, ItemType itemTypeToRetrieve, ScriptFunc<int, bool> comparisonToDo)
        {
            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return false;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return false;

            if (player == null) return false;

            // retrieve players 'itemType' create stat
            var valueToCheck =
                PlayerStats.GetItemTypeCreateStats(player.PlayerStats, itemTypeToRetrieve);

            // do lamba func check
            return comparisonToDo(valueToCheck);

        }

        // 10/5/2009
        /// <summary>
        /// Checks if given <see cref="Player"/> has built at least one <see cref="BuildingScene"/>.
        /// </summary>
        /// <param name="playerNumber">PlayerNumber of <see cref="Player"/> to check</param>
        /// <returns>True/False of result</returns>
        public static bool PlayerHasBuiltAtLeastOneBuilding(int playerNumber)
        {
            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return false;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return false;

            if (player == null) return false;

            return player.PlayerStats.BuildingsCreated >= 1;

        }

        // 10/9/2009
        /// <summary>
        /// Checks if given <see cref="Player"/> has built at least one of the given <see cref="ItemType"/>.
        /// </summary>
        /// <param name="playerNumber">PlayerNumber of <see cref="Player"/> to check</param>
        /// <param name="itemType"><see cref="ItemType"/> to check</param>
        /// <returns>True/False of result</returns>
        public static bool PlayerHasBuiltItemType(int playerNumber, ItemType itemType)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'PlayerHasBuiltItemType' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return false;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return false;

            if (player == null) return false;

            // check 'PlayerStats' to see if the item was built
            return (PlayerStats.GetItemTypeCreateStats(player.PlayerStats, itemType) > 0);
        }

        // 10/5/2009
        /// <summary>
        /// Checks if given <see cref="Player"/> has lost at least N <see cref="BuildingScene"/>s.
        /// </summary>
        /// <param name="playerNumber">PlayerNumber of <see cref="Player"/> to check</param>
        /// <param name="quantityToCheck">Quantity to check</param>
        /// <returns>True/False of result</returns>
        public static bool PlayerHasLostAtLeastNBuildings(int playerNumber, int quantityToCheck)
        {
            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return false;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return false;

            if (player == null) return false;

            return player.PlayerStats.BuildingsDestroyed >= quantityToCheck;

        }

        // 10/5/2009
        /// <summary>
        /// Checks if given <see cref="Player"/> has lost at least N 'units'.
        /// </summary>
        /// <param name="playerNumber">PlayerNumber of <see cref="Player"/> to check</param>
        /// <param name="quantityToCheck">Quantity to check</param>
        /// <remarks>
        /// 'Units' can be either <see cref="SciFiTankScene"/> items or <see cref="SciFiAircraftScene"/> items, for example.
        /// </remarks>
        /// <returns>True/False of result</returns>
        public static bool PlayerHasLostAtLeastNUnits(int playerNumber,int quantityToCheck)
        {
            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return false;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return false;

            if (player == null) return false;

            return player.PlayerStats.UnitsDestroyed >= quantityToCheck;

        }

        // 10/5/2009
        /// <summary>
        /// Checks if given <see cref="Player"/> has Lost N (comparison) number of <see cref="ItemType"/>s.
        /// </summary>
        /// <param name="playerNumber">PlayerNumber of <see cref="Player"/> to check</param>
        /// <param name="itemTypeToRetrieve"><see cref="ItemType"/> enum to retrieve; for example 'SciFiTank01' enum.</param>
        /// <param name="comparisonToDo">Lamba Func format: 'var => var > 5'; example would be 'cash => cash > 5'</param>
        /// <returns>True/False of result</returns>
        public static bool PlayerHasLost_NComparison_OfSelectableItem(int playerNumber, ItemType itemTypeToRetrieve, ScriptFunc<int, bool> comparisonToDo)
        {
            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return false;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return false;

            if (player == null) return false;

            // retrieve players 'itemType' destroyed stat
            var valueToCheck =
                PlayerStats.GetItemTypeDestroyedStats(player.PlayerStats, itemTypeToRetrieve);

            // do lamba func check
            return comparisonToDo(valueToCheck);

        }

        // 1/14/2011
        /// <summary>
        /// Checks if given <see cref="Player"/> has remaing N (comparison) number of <see cref="ItemType"/>s.
        /// </summary>
        /// <param name="playerNumber">PlayerNumber of <see cref="Player"/> to check</param>
        /// <param name="itemTypeToRetrieve"><see cref="ItemType"/> enum to retrieve; for example 'SciFiTank01' enum.</param>
        /// <param name="comparisonToDo">Lamba Func format: 'var => var > 5'; example would be 'cash => cash > 5'</param>
        /// <returns>True/False of result</returns>
        public static bool PlayerHasRemaing_NComparison_OfSelectableItem(int playerNumber, ItemType itemTypeToRetrieve, ScriptFunc<int, bool> comparisonToDo)
        {
            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return false;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return false;

            if (player == null) return false;

            // retrieve players 'itemType' destroyed stat
            var destroyedStats =
                PlayerStats.GetItemTypeDestroyedStats(player.PlayerStats, itemTypeToRetrieve);

            // retrieve players 'itemType' created stat
            var createdStats =
                PlayerStats.GetItemTypeCreateStats(player.PlayerStats, itemTypeToRetrieve);

            var valueToCheck = createdStats - destroyedStats;

            // do lamba func check
            return comparisonToDo(valueToCheck);
        }

        // 10/5/2009
        /// <summary>
        /// Checks if given <see cref="Player"/> has N (comparison) number of <see cref="Player.Energy"/> supply.
        /// </summary>
        /// <param name="playerNumber">PlayerNumber of <see cref="Player"/> to check</param>
        /// <param name="comparisonToDo">Lamba Func format: 'var => var > 5'; example would be 'power => power > 5'</param>
        /// <returns>True/False of result</returns>
        public static bool PlayerHas_NComparison_OfPowerSupply(int playerNumber, ScriptFunc<int, bool> comparisonToDo)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'PlayerHas_NComparison_OfPowerSupply' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return false;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return false;

            if (player == null) return false;
 
            // retrieve players 'power' supply
            var valueToCheck = player.Energy;

            // do lamba func check
            return comparisonToDo(valueToCheck);
        }


        // 10/5/2009
        /// <summary>
        /// Checks if given <see cref="Player"/> has N (comparison) of <see cref="Player.Energy"/> to Consumption percent.
        /// </summary>
        /// <param name="playerNumber">PlayerNumber of <see cref="Player"/> to check</param>
        /// <param name="comparisonToDo">Lamba Func format: 'var => var > 5'; example would be 'power => power > 5'</param>
        /// <returns>True/False of result</returns>
        public static bool PlayerHas_NComparison_OfPowerToConsumptionPercent(int playerNumber, ScriptFunc<int, bool> comparisonToDo)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'PlayerHas_NComparison_OfPowerToConsumptionPercent' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return false;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return false;

            if (player == null) return false;
       
            // retrieve players 'Power' & 'PowerUsed' values
            var powerSupply = (float)player.Energy;
            var powerUsed = (float)player.EnergyUsed;

            // Calc power ratio
            var powerRatio = (int)((powerUsed/powerSupply)*100.0f);

            // do lamba func check
            return comparisonToDo(powerRatio);
        }

        // 10/5/2009
        /// <summary>
        /// Checks if given <see cref="Player"/>'s base is not powered (<see cref="Player.EnergyOff"/>).
        /// </summary>
        /// <param name="playerNumber">PlayerNumber of <see cref="Player"/> to check</param>
        /// <returns>True/False of result</returns>
        public static bool PlayerBaseCurrentlyHasNoPower(int playerNumber)
        {
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return false;

            if (player == null) return false;

            // check if out of power
            // verify proper PlayerNumber given, and not NULL instance!
            return DoPlayerArgCheck(playerNumber) && player.EnergyOff;
            
        }

        // 10/5/2009
        /// <summary>
        /// Checks if given <see cref="Player"/>'s base is powered  (<see cref="Player.EnergyOff"/>).
        /// </summary>
        /// <param name="playerNumber">PlayerNumber of <see cref="Player"/> to check</param>
        /// <returns>True/False of result</returns>
        public static bool PlayerBaseCurrentlyHasPower(int playerNumber)
        {
            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return false;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return false;

            if (player == null) return false;

            // check if out of power
            return !player.EnergyOff;
        }

        // 10/5/2009
        /// <summary>
        /// Check if given <see cref="Player"/> sighted some enemy 'units' or <see cref="BuildingScene"/>s.
        /// </summary>
        /// <param name="playerNumber">PlayerNumber of <see cref="Player"/> to check</param>
        /// <remarks>
        /// 'Units' can be either <see cref="SciFiTankScene"/> items or <see cref="SciFiAircraftScene"/> items, for example.
        /// </remarks>
        /// <returns>True/False of result</returns>
        public static bool PlayerSightedEnemyPlayer(int playerNumber)
        {
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return false;

            if (player == null) return false;

            // verify proper PlayerNumber given, and not NULL instance!
            return DoPlayerArgCheck(playerNumber) && player.PlayerSightedEnemyPlayer;
        }

        // 10/8/2009
        /// <summary>
        /// Check if given <see cref="Player"/> sighted some Named <see cref="SceneItem"/>.
        /// </summary>
        /// <param name="playerNumber">PlayerNumber of <see cref="Player"/> to check</param>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to check; can be both a 'Selectable' or 'Scenary' sceneItem.</param>
        /// <remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
        /// <returns>True/False of result</returns>
        public static bool PlayerSightedNamedItem(int playerNumber, string sceneItemName)
        {
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return false;

            if (player == null) return false;

            // check if any of the Player's SelectableItems are within view of the Named item.
            // verify proper PlayerNumber given, and not NULL instance!
            return DoPlayerArgCheck(playerNumber) && Player.PlayerSightedNamedItem(player, sceneItemName);

            
        }

        #endregion


        #region NamedItems

        // 5/27/2012
        /// <summary>
        /// Checks if the given named <paramref name="sceneItemNameA"/> collided with the given named <paramref name="sceneItemNameB"/>.
        /// </summary>
        /// <param name="sceneItemNameA">Named <see cref="SceneItem"/> to check</param>
        /// <param name="sceneItemNameB">Named <see cref="SceneItem"/> to check</param>
        /// <returns>True/False of collision.</returns>
        public static bool NamedItemCollidedWithNamedItem(string sceneItemNameA, string sceneItemNameB)
        {
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItemA = ScriptingHelper.GetNamedItem(sceneItemNameA, out instancedItemPickedIndex);

            // try get 'Named sceneITem from Player class
            int instancedItemPickedIndex2;
            var namedSceneItemB = ScriptingHelper.GetNamedItem(sceneItemNameB, out instancedItemPickedIndex2);

            return namedSceneItemA.Collide(namedSceneItemB);
        }

        // 10/10/2009
        /// <summary>
        /// Checks if the given Named <see cref="SceneItem"/> is within the specified <see cref="TerrainTriggerAreas"/> name.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to check; can be both a 'Selectable' or 'Scenary' <see cref="SceneItem"/>.</param>
        /// <param name="triggerAreaName"><see cref="TerrainTriggerAreas"/> name to check</param>
        /// <remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
        /// <returns>True/False of result</returns>
        public static bool NamedItemIsInsideTriggerArea(string sceneItemName, string triggerAreaName)
        {
            return TerrainTriggerAreas.DoNamedItemInTriggerAreaCheck(sceneItemName, triggerAreaName);
        }

        // 10/10/2009
        /// <summary>
        /// Checks if the given Named <see cref="SceneItem"/> is outside the specified <see cref="TerrainTriggerAreas"/> name.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to check; can be both a 'Selectable' or 'Scenary' <see cref="SceneItem"/>.</param>
        /// <param name="triggerAreaName"><see cref="TerrainTriggerAreas"/> name to check</param>
        /// <remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
        /// <returns>True/False of result</returns>
        public static bool NamedItemIsOutsideTriggerArea(string sceneItemName, string triggerAreaName)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'NamedItemIsOutsideTriggerArea' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // check the negate of being inside triggerArea.
            return !TerrainTriggerAreas.DoNamedItemInTriggerAreaCheck(sceneItemName, triggerAreaName);
        }

        // 10/10/2009
        /// <summary>
        /// Checks if the given Named <see cref="SceneItem"/> has been attacked.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to check (Only Selectable SceneItems allowed)</param>
        /// <remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
        /// <returns>True/False of result</returns>
        public static bool NamedItemHasBeenAttacked(string sceneItemName)
        {
            // try get 'Named' sceneItem from Player class
            SceneItem namedSceneItem;
            if (Player.SceneItemsByName.TryGetValue(sceneItemName, out namedSceneItem))
            {
                // check if this is a ScenaryItem, which is not allowed for this method!
                SceneItem.DoCheckIfSceneItemIsScenaryItemType(namedSceneItem);

                // check item's health, to know if attacked.
                return namedSceneItem.CurrentHealthPercent < 1.0f;
            }

            throw new InvalidOperationException("Given sceneItem Name is NOT VALID!");
        }

        // 10/10/2009
        /// <summary>
        /// Checks if given Name <see cref="SceneItem"/> has destroyed N (comparison) number of opponents <see cref="ItemType"/> enum.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to check (Only Selectable SceneItems allowed)</param>
        /// <param name="itemTypeToRetrieve"><see cref="ItemType"/> enum to retrieve; for example 'SciFiTank01' enum.</param>
        /// <param name="comparisonToDo">Lamba Func format: 'var => var > 5'; example would be 'cash => cash > 5'</param>
        /// <remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="sceneItemName"/> is not valid.</exception>
        /// <returns>True/False of result</returns>
        public static bool NamedItemHasDestroyed_NComparison_OfOpponentsItemType(string sceneItemName, ItemType itemTypeToRetrieve, ScriptFunc<int, bool> comparisonToDo)
        {
            // try get 'Named' sceneItem from Player class
            SceneItem namedSceneItem;
            if (Player.SceneItemsByName.TryGetValue(sceneItemName, out namedSceneItem))
            {
                // check if this is a ScenaryItem, which is not allowed for this method!
                SceneItem.DoCheckIfSceneItemIsScenaryItemType(namedSceneItem);

                // verify proper PlayerNumber given, and not NULL instance!
                if (!DoPlayerArgCheck(namedSceneItem.PlayerNumber)) return false;

                // 6/15/2010 - Updated to use new GetPlayer method.
                Player player;
                if (!TemporalWars3DEngine.GetPlayer(namedSceneItem.PlayerNumber, out player))
                    return false;

                if (player == null) return false;

                // retrieve given player's stats for itemType destroyed.
                var valueToCheck =
                    PlayerStats.GetItemTypeKillStats(player.PlayerStats, itemTypeToRetrieve);

                // do lamba func check
                return comparisonToDo(valueToCheck);
                
            }

            throw new InvalidOperationException("Given sceneItem Name is NOT VALID!");
        }

        // 10/10/2009
        /// <summary>
        /// Checks if given Name <see cref="SceneItem"/> is currently being attacked by given <see cref="ItemType"/> enum; current being
        /// with the last 10 seconds.
        /// </summary>
        /// <param name="gameTime">Current GameTime</param>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to check (Only Selectable SceneItems allowed)</param>
        /// <param name="itemTypeToCheck"><see cref="ItemType"/> enum to check; for example 'SciFiTank01' enum.</param>
        /// <remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="sceneItemName"/> is not valid.</exception>
        /// <returns>True/False of result</returns>
        public static bool NamedItemIsAttackedByItemType(GameTime gameTime, string sceneItemName, ItemType itemTypeToCheck)
        {
            // try get 'Named' sceneItem from Player class
            SceneItem namedSceneItem;
            if (Player.SceneItemsByName.TryGetValue(sceneItemName, out namedSceneItem))
            {
                // check if this is a ScenaryItem, which is not allowed for this method!
                SceneItem.DoCheckIfSceneItemIsScenaryItemType(namedSceneItem);

                // must be a sceneItemWithPick, so do cast now.
                var namedSceneItemWithPick = (namedSceneItem as SceneItemWithPick);
                if (namedSceneItemWithPick != null)
                {
                    // check 'AttackBy' Property.
                    if (namedSceneItemWithPick.AttackBy != null)
                    {
                        // cache struct
                        var attackByLastItemType = namedSceneItemWithPick.AttackBy.Value;

                        // get time of Attack; must be at least within the last 10 seconds of game play.
                        var timeOfAttack = attackByLastItemType.TimeOfAttack.TotalGameTime.Seconds;

                        // check if attack was within the last 10 seconds.
                        var attackWithin = Math.Abs(gameTime.TotalGameTime.Seconds - timeOfAttack);

                        return (attackWithin <= 10 && attackByLastItemType.AttackedByItemType == itemTypeToCheck);
                        
                    } // End if 'AttackBy' is Null
                } // End if NOT SceneItemWithPick
                return false;
            }

            throw new InvalidOperationException("Given sceneItem Name is NOT VALID!");

        }

        // 10/10/2009
        /// <summary>
        /// Checks if given Name <see cref="SceneItem"/> is Destroyed by given <see cref="ItemType"/> enum.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to check (Only Selectable SceneItems allowed)</param>
        /// <param name="itemTypeToCheck"><see cref="ItemType"/> enum to check; for example 'SciFiTank01' enum.</param>
        /// <remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="sceneItemName"/> is not valid.</exception>
        /// <returns>True/False of result</returns>
        public static bool NamedItemIsDestroyedByItemType(string sceneItemName, ItemType itemTypeToCheck)
        {
            // try get 'Named' sceneItem from Player class
            SceneItem namedSceneItem;
            if (Player.SceneItemsByName.TryGetValue(sceneItemName, out namedSceneItem))
            {
                // check if this is a ScenaryItem, which is not allowed for this method!
                SceneItem.DoCheckIfSceneItemIsScenaryItemType(namedSceneItem);

                // must be a sceneItemWithPick, so do cast now.
                var namedSceneItemWithPick = (namedSceneItem as SceneItemWithPick);
                if (namedSceneItemWithPick != null)
                {
                    // check 'DestroyedBy' Property.
                    if (namedSceneItemWithPick.DestroyedBy != null)
                    {
                        // Check 'DestroyedBy' property
                        return (namedSceneItemWithPick.DestroyedBy == itemTypeToCheck);

                    } // End if 'DestroyedBy' is Null
                } // End if NOT SceneItemWithPick
                return false;
            }

            throw new InvalidOperationException("Given sceneItem Name is NOT VALID!");
        }

        // 10/10/2009
        /// <summary>
        /// Checks if given Name <see cref="SceneItem"/> is Destroyed.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to check (Only Selectable SceneItems allowed)</param>
        /// <remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="sceneItemName"/> is not valid.</exception>
        /// <returns>True/False of result</returns>
        public static bool NamedItemIsDestroyed(string sceneItemName)
        {
            // try get 'Named' sceneItem from Player class
            SceneItem namedSceneItem;
            if (Player.SceneItemsByName.TryGetValue(sceneItemName, out namedSceneItem))
            {
                // check if this is a ScenaryItem, which is not allowed for this method!
                SceneItem.DoCheckIfSceneItemIsScenaryItemType(namedSceneItem);

                // must be a sceneItemWithPick, so do cast now.
                var namedSceneItemWithPick = (namedSceneItem as SceneItemWithPick);
                if (namedSceneItemWithPick != null)
                {
                    // Check 'IsAlive' property and 'DestroyedBy' is not NULL.
                    return (!namedSceneItemWithPick.IsAlive && namedSceneItemWithPick.DestroyedBy != null);

                }// End if NOT SceneItemWithPick
                return false;
            }

            throw new InvalidOperationException("Given sceneItem Name is NOT VALID!");
        }

        // 10/10/2009
        /// <summary>
        /// Checks if given Name <see cref="SceneItem"/> current <see cref="SceneItem.CurrentHealth"/> value, is N comparison some percent value.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to check (Only Selectable SceneItems allowed)</param>
        /// <param name="comparisonToDo">Lamba Func format: 'var => var > 5'; example would be 'health => health > 5'</param>
        /// <remarks>
        /// 'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.
        ///  Note: Enter the percent value as a number 0 - 100.
        /// </remarks> 
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="sceneItemName"/> is not valid.</exception>
        /// <returns>True/False of result</returns>
        public static bool NamedItemHas_NComparison_OfHealthPercent(string sceneItemName, ScriptFunc<float, bool> comparisonToDo)
        {
            // try get 'Named' sceneItem from Player class
            SceneItem namedSceneItem;
            if (Player.SceneItemsByName.TryGetValue(sceneItemName, out namedSceneItem))
            {
                // check if this is a ScenaryItem, which is not allowed for this method!
                SceneItem.DoCheckIfSceneItemIsScenaryItemType(namedSceneItem);

                // get Health percent, and convert to 100s.
                var healthPercent = namedSceneItem.CurrentHealthPercent * 100;

                // do lamba fn comparison.
                return comparisonToDo(healthPercent);
            }

            throw new InvalidOperationException("Given sceneItem Name is NOT VALID!");
        }

        // 10/10/2009
        /// <summary>
        /// Checks if given Name <see cref="SceneItem"/> is currently selected.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to check (Only Selectable SceneItems allowed)</param>
        /// <remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="sceneItemName"/> is not valid.</exception>
        /// <returns>True/False of result</returns>
        public static bool NamedItemIsCurrentlySelected(string sceneItemName)
        {
            // try get 'Named' sceneItem from Player class
            SceneItem namedSceneItem;
            if (Player.SceneItemsByName.TryGetValue(sceneItemName, out namedSceneItem))
            {
                // check if this is a ScenaryItem, which is not allowed for this method!
                SceneItem.DoCheckIfSceneItemIsScenaryItemType(namedSceneItem);

                // must be a sceneItemWithPick, so do cast now.
                var namedSceneItemWithPick = (namedSceneItem as SceneItemWithPick);
                return namedSceneItemWithPick != null && (namedSceneItemWithPick.PickSelected);
            }

            throw new InvalidOperationException("Given sceneItem Name is NOT VALID!");
        }

        // 10/10/2009
        /// <summary>
        /// Checks if given Name <see cref="SceneItem"/> sighted the given <see cref="ItemType"/> of the given <see cref="Player"/>.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to check; can be both a 'Selectable' or 'Scenary' sceneItem.</param>
        /// <param name="itemTypeToCheck"><see cref="ItemType"/> enum to check; for example 'SciFiTank01' enum.</param>
        /// <param name="playerNumber">PlayerNumber of <see cref="Player"/> to check</param>
        /// <remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
        /// <returns>True/False of result</returns>
        public static bool NamedItemHasSightedItemTypeBelongingToPlayer(string sceneItemName, ItemType itemTypeToCheck, int playerNumber)
        {
            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return false;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return false;
            
            return player != null && Player.NamedSceneItemSightedItemType(player, sceneItemName, itemTypeToCheck);

            // do check
        }

        // 10/10/2009
        /// <summary>
        /// Checks if given Named <see cref="SceneItem"/> has the given <see cref="DefenseAIStance"/> stance. (Scripting purposes)
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to check (Only Selectable SceneItems allowed)</param>
        /// <param name="defenseAIStanceToCheck"><see cref="DefenseAIStance"/> stance to check; like 'Guard' stance.</param>
        /// <remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
        /// <returns>True/False of result</returns>
        public static bool NamedItemHasGivenDefenseStance(string sceneItemName, DefenseAIStance defenseAIStanceToCheck)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // do check
            return Player.NamedSceneItemIsUsingStance(sceneItemName, defenseAIStanceToCheck);
        }


        #endregion


        // 6/6/2012
        /// <summary>
        /// Used to unload resources during level loads.
        /// </summary>
        public static void UnloadContent()
        {
           // Empty
        }

    }
}
