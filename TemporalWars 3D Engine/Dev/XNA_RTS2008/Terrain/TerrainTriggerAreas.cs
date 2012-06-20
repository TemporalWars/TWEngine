#region File Description
//-----------------------------------------------------------------------------
// TerrainTriggerAreas.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
#if !XBOX360
using System.Windows.Forms;
using TWEngine.TerrainTools;
#endif
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PerfTimersComponent.Timers;
using PerfTimersComponent.Timers.Enums;
using TWEngine.GameCamera;
using TWEngine.HandleGameInput;
using TWEngine.Players;
using TWEngine.SceneItems;
using TWEngine.Terrain.Enums;
using TWEngine.Terrain.Structs;
using TWEngine.Utilities;
using TWEngine.Utilities.Enums;
using TWEngine.GameLevels.Delegates;

namespace TWEngine.Terrain
{
    // 9/28/2009
    /// <summary>
    /// The <see cref="TerrainTriggerAreas"/> class is used by the 
    /// PropertiesTool window to define 'Trigger' areas in the game world.
    /// </summary>
    public sealed class TerrainTriggerAreas : DrawableGameComponent
    {
        private static BasicEffect _lineEffect;
        private static VertexDeclaration _lineVertexDeclaration;

        private static Vector3 _startSelect = Vector3.Zero;
        private static Vector3 _cursorPosition = Vector3.Zero;
        private static Rectangle _rectangleArea;

        private static VertexPositionColor[] _currentVisualRectangleArea;

        // 6/2/2012
        private static Vector3 _updateTriggerAreaPriorPosition;
        private static Vector3 _updateTriggerAreaPositionDelta;

        // 10/1/2009 - Dictionary keys
        private static string[] _keys = new string[1];
        private static readonly Random RandomGenerator = new Random(99);

        // XNA 4.0 Updates - RenderState replaced with 4 new states; BlendState, RasterizerState, DepthStencilState, or SampleState.
        private static RasterizerState _rasterizerState1;
        private static RasterizerState _rasterizerState2;
        private static DepthStencilState _depthStencilState1;
        private static DepthStencilState _depthStencilState2;

        #region Properties

// ReSharper disable UnusedAutoPropertyAccessor.Local
        private static Game GameInstance { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Local


        ///<summary>
        /// Set to 'True' the moment a user starts the creation of a <see cref="TerrainTriggerAreas"/> rectangle,
        /// and set to 'False' when user finshes creation of <see cref="TerrainTriggerAreas"/>.
        ///</summary>
        public static bool AreaSelect { get; set; }

        ///<summary>
        /// Turn On/Off ability to create a new <see cref="TerrainTriggerAreas"/> rectangle.
        /// (Updated via the PropertiesTool form)
        ///</summary>
        public static bool DoDefineArea { get; set; }

        ///<summary>
        /// Start position during the creation of a new <see cref="TerrainTriggerAreas"/>.
        ///</summary>
        public static Vector3 StartSelect
        {
            set { _startSelect = value; }
        }

        /// <summary>
        /// Continious position, during the creation of a new <see cref="TerrainTriggerAreas"/>.  This value, coupled with
        /// the <see cref="StartSelect"/> position, determines the total area for the <see cref="TerrainTriggerAreas"/> rectangle.
        /// </summary>
        public static Vector3 CursorPosition
        {
            get { return _cursorPosition; }
            set { _cursorPosition = value; }
        }

        /// <summary>
        /// <see cref="TerrainTriggerAreas"/> visual rectangle.
        /// </summary>
        public static Rectangle RectangleArea
        {
            get { return _rectangleArea; }
        }
       
        ///<summary>
        /// Dictionary of <see cref="TerrainTriggerAreas"/>, where Key = Area's Name.
        ///</summary>
        public static Dictionary<string, TriggerAreaStruct> TriggerAreas { get; private set; }

        #endregion

        ///<summary>
        /// Constructor for <see cref="TerrainTriggerAreas"/>, which initializes the required 'TriggerAreas'
        /// dictionary, and the <see cref="BasicEffect"/> for drawing the visual lines of the trigger rectangles.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        public TerrainTriggerAreas(Game game)
            : base(game)
        {
            // Set static instance
            GameInstance = game;

            // create & populate current rectangle struct with init color.
            _currentVisualRectangleArea = new VertexPositionColor[6];

            // XNA 4.0 Updates - Remove 2nd param.
            // Line effect is used for rendering area rectangle
            //_lineEffect = new BasicEffect(game.GraphicsDevice, null) { VertexColorEnabled = true };
            _lineEffect = new BasicEffect(game.GraphicsDevice) { VertexColorEnabled = true };

            // XNA 4.0 Updates
            //_lineVertexDeclaration = new VertexDeclaration(game.GraphicsDevice, VertexPositionColor.VertexElements);

            //Iniitliaze the Selection box's rectangle. Currently no selection box is drawn
            //so set it's x and y Position to -1 and it's height and width to 0
            _rectangleArea = new Rectangle(-1, -1, 0, 0);

            // Init Dictionary of TriggerArea's
            TriggerAreas = new Dictionary<string, TriggerAreaStruct>();

            // Set 'DoDefineArea' to false, for off.  This is set via the 'PropertiesTool' form 'Areas' tab.
            DoDefineArea = false;

            // Draworder
            DrawOrder = 125;

            // XNA 4.0 Updates
            _rasterizerState1 = new RasterizerState { FillMode = FillMode.WireFrame, CullMode = CullMode.None };
            _rasterizerState2 = new RasterizerState { FillMode = FillMode.Solid, CullMode = CullMode.CullClockwiseFace };
            _depthStencilState1 = new DepthStencilState { DepthBufferEnable = false };
            _depthStencilState2 = new DepthStencilState { DepthBufferEnable = true };
        }



        /// <summary>
        /// Performs regular updates for the class.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public override void Update(GameTime gameTime)
        {

#if !XBOX360
            // When propertiesTool form is open, checks for user creation of new 'TriggerArea' rectangle.
            DoPropertiesToolCheck();
#endif
           
            // check for selectable items within some trigger area
            DoSelectableItemsInTriggerAreaCheck();


            base.Update(gameTime);
        }

        // 10/2/2009
        /// <summary>
        /// Given a specific <see cref="TerrainTriggerAreas"/> name, will return if there are any
        /// selectable items contain within its rectangle area.
        /// </summary>
        /// <param name="triggerAreaName"><see cref="TerrainTriggerAreas"/> name</param>
        /// <returns>True/False of success</returns>
        public static bool TriggerAreaContainsSomeItem(string triggerAreaName)
        {
            // do search in dictionary
            TriggerAreaStruct triggerAreaItem;
            return TriggerAreas.TryGetValue(triggerAreaName, out triggerAreaItem) && triggerAreaItem.ContainsSomeSelectableItem;
        }

        // 10/3/2009; // 1/15/2010 - Fixed to directly check the name given, and not iterate the dictionary!
        /// <summary>
        /// Given a specific <see cref="TerrainTriggerAreas"/> name, will return if the <see cref="Camera"/> is
        /// within its rectangle area.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when <paramref name="triggerAreaName"/> was not found, or is not valid.</exception>
        /// <param name="triggerAreaName"><see cref="TerrainTriggerAreas"/> name</param>
        /// <returns>True/False of success</returns>
        public static bool TriggerAreaContainsCamera(string triggerAreaName)
        {
            // 1/15/2010 - Check if Name is correct
            // Try to retrieve 'TriggerArea' name
            TriggerAreaStruct triggerAreaItem;
            if (TriggerAreas.TryGetValue(triggerAreaName, out triggerAreaItem))
            {
                // check if Camera within 'TriggerArea' rectangle
                return triggerAreaItem.RectangleArea.Contains((int)Camera.CameraPosition.X, (int)Camera.CameraPosition.Z);
            }

            throw new ArgumentException(@"Given TriggerArea Name is NOT VALID!", triggerAreaName);
        }

        // 10/3/2009
        /// <summary>
        /// Checks the given Player classes 'SelectableItems' arrays, 
        /// for selectable items of given SceneItem type, which are within
        /// the specified 'TriggerArea' name.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when <paramref name="triggerAreaName"/> was not found, or is not valid.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="playerNumber"/> is not valid.</exception>
        /// <param name="playerNumber">PlayerNumber of player to check</param>
        /// <param name="triggerAreaName">TriggerArea name</param>
        /// <param name="itemType">Lamba Fn: 'var => var is itemType', where itemType is some 'SceneItem' class type.</param>
        public static bool DoUnitTypeInTriggerAreaCheck(int playerNumber, string triggerAreaName, ScriptFunc<SceneItem, bool> itemType)
        {
            // make sure playerNumber is valid
            if (playerNumber >= TemporalWars3DEngine._maxAllowablePlayers)
                throw new ArgumentOutOfRangeException("playerNumber", @"PlayerNumber given is not valid!");
            
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return false;

            // make sure player not null
            if (player == null) return false;

            // Try to retrieve 'TriggerArea' name
            TriggerAreaStruct triggerAreaItem;
            if (TriggerAreas.TryGetValue(triggerAreaName, out triggerAreaItem))
            {
                // 6/15/2010 - Updated to retrieve the ROC collection.
                // cache selectables
                ReadOnlyCollection<SceneItemWithPick> selectableItems; 
                Player.GetSelectableItems(player, out selectableItems);

                // iterate selectableItems for given player, and check if within 'TriggerArea' rectangle.
                var selectableItemsCount = selectableItems.Count;
                for (var k = 0; k < selectableItemsCount; k++)
                {
                    // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                    // cache selectable item
                    SceneItemWithPick selectableItem;
                    if (!Player.GetSelectableItemByIndex(player, k, out selectableItem))
                        break;

                    // make sure not null
                    if (selectableItem == null) continue;

                    // Only check for the given ItemType specified by user
                    if (itemType(selectableItem))
                        // check if within 'TriggerArea' rectangle
                        if (triggerAreaItem.RectangleArea.Contains((int)selectableItem.Position.X,
                                                                   (int)selectableItem.Position.Z))
                        {
                            // found an item, so stop search and return true
                            return true;
                        }

                } // End For selectables

                return false;

            } // End TryGetValue

            throw new ArgumentException(@"Given TriggerArea Name is NOT VALID!", "triggerAreaName");
        }

        // 2/7/2011: Overload method#2
        /// <summary>
        /// Checks the given Player classes 'SelectableItems' arrays, 
        /// for selectable items of given SceneItem type for the given quantity comparison, which are within
        /// the specified 'TriggerArea' name.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when <paramref name="triggerAreaName"/> was not found, or is not valid.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="playerNumber"/> is not valid.</exception>
        /// <param name="playerNumber">PlayerNumber of player to check</param>
        /// <param name="triggerAreaName">TriggerArea name</param>
        /// <param name="itemType">Lamba Fn: 'var => var is itemType', where itemType is some 'SceneItem' class type.</param>
        /// <param name="comparisonToDo">Lamba Func format: 'var => var > 5'; example would be 'cash => cash > 5'</param>
        public static bool DoUnitTypeInTriggerAreaCheck(int playerNumber, string triggerAreaName, ScriptFunc<SceneItem, bool> itemType, ScriptFunc<int, bool> comparisonToDo)
        {
            // make sure playerNumber is valid
            if (playerNumber >= TemporalWars3DEngine._maxAllowablePlayers)
                throw new ArgumentOutOfRangeException("playerNumber", @"PlayerNumber given is not valid!");

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return false;

            // make sure player not null
            if (player == null) return false;

            // Try to retrieve 'TriggerArea' name
            TriggerAreaStruct triggerAreaItem;
            if (TriggerAreas.TryGetValue(triggerAreaName, out triggerAreaItem))
            {
                // 6/15/2010 - Updated to retrieve the ROC collection.
                // cache selectables
                ReadOnlyCollection<SceneItemWithPick> selectableItems;
                Player.GetSelectableItems(player, out selectableItems);

                // iterate selectableItems for given player, and check if within 'TriggerArea' rectangle.
                var itemsWithinAreaCount = 0;
                var selectableItemsCount = selectableItems.Count;
                for (var k = 0; k < selectableItemsCount; k++)
                {
                    // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                    // cache selectable item
                    SceneItemWithPick selectableItem;
                    if (!Player.GetSelectableItemByIndex(player, k, out selectableItem))
                        break;

                    // make sure not null
                    if (selectableItem == null) continue;

                    // Only check for the given ItemType specified by user
                    if (itemType(selectableItem))
                        // check if within 'TriggerArea' rectangle
                        if (triggerAreaItem.RectangleArea.Contains((int)selectableItem.Position.X,
                                                                   (int)selectableItem.Position.Z))
                        {
                            // found an item, so increase count
                            itemsWithinAreaCount++;
                        }

                } // End For selectables

                // do lamba func check
                return comparisonToDo(itemsWithinAreaCount);

            } // End TryGetValue

            throw new ArgumentException(@"Given TriggerArea Name is NOT VALID!", "triggerAreaName");
        }

        // 10/10/2009
        /// <summary>
        /// Checks if given Named sceneItem, is within the specified <see cref="TerrainTriggerAreas"/> name.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when <paramref name="triggerAreaName"/> was not found, or is not valid.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sceneItemName"/> was not found, or is not valid.</exception>
        /// <param name="sceneItemName">Named SceneItem to check; can be both a 'Selectable' or 'Scenary' sceneItem.</param>
        /// <param name="triggerAreaName"><see cref="TerrainTriggerAreas"/> name</param>
        /// <returns>True/False</returns>
        public static bool DoNamedItemInTriggerAreaCheck(string sceneItemName, string triggerAreaName)
        {
            // Get Named item
            SceneItem namedSceneItem;
            if (Player.SceneItemsByName.TryGetValue(sceneItemName, out namedSceneItem))
            {
                // Try to retrieve 'TriggerArea' name
                TriggerAreaStruct triggerAreaItem;
                if (TriggerAreas.TryGetValue(triggerAreaName, out triggerAreaItem))
                {

                    // now check if named item is a 'ScenaryItem', which implies the need to check the internal array of items.
                    var scenaryItem = (namedSceneItem as ScenaryItemScene);
                    if (scenaryItem != null)
                    {
                        // use the internal Dictionary, to find the instance contain in the internal array.
                        int instanceArrayIndex;
                        if (scenaryItem.ScenaryItemsByName.TryGetValue(sceneItemName, out instanceArrayIndex))
                        {
                            // retrieve the scenaryItem instance, using index.
                            var scenaryInstance = scenaryItem.ScenaryItems[instanceArrayIndex];

                            // check if within 'TriggerArea' rectangle
                            return triggerAreaItem.RectangleArea.Contains((int)scenaryInstance.position.X,
                                                                          (int)scenaryInstance.position.Z);
                        } // End if name exist for instances
                    } // end if ScenaryItem

                    // check if within 'TriggerArea' rectangle
                    return triggerAreaItem.RectangleArea.Contains((int)namedSceneItem.Position.X,
                                                                  (int)namedSceneItem.Position.Z);
                }

                throw new ArgumentException(@"Given TriggerArea Name is NOT VALID!", triggerAreaName);
            }

            throw new ArgumentException(@"Given sceneItem Name is NOT VALID!", sceneItemName);
        }

        // 10/3/2009
        /// <summary>
        /// Checks the given Player classes 'SelectableItems' arrays, for selectable items which are within
        /// the specified <see cref="TerrainTriggerAreas"/> name; if 'ItemType' param is specified, then this will be excluded 
        /// from search.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="playerNumber"/> is not valid.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="triggerAreaName"/> was not found, or is not valid.</exception>
        /// <param name="playerNumber">PlayerNumber of player to check</param>
        /// <param name="triggerAreaName"><see cref="TerrainTriggerAreas"/> name</param>
        /// <param name="doItemTypeFilter">Do an exlusion of some SceneItem itemType</param>
        /// <param name="itemType">Lamba Fn: 'var => var is itemType', where itemType is some 'SceneItem' class type.</param>
        /// <returns>True/False</returns>
        public static bool DoUnitItemsInTriggerAreaCheck(int playerNumber, string triggerAreaName, bool doItemTypeFilter, ScriptFunc<SceneItem, bool> itemType)
        {
            // make sure playerNumber is valid
            if (playerNumber >= TemporalWars3DEngine._maxAllowablePlayers)
                throw new ArgumentOutOfRangeException("playerNumber", @"PlayerNumber given is not valid!");
           
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return false;

            // make sure player not null
            if (player == null) return false;
            
            // Try to retrieve 'TriggerArea' name
            TriggerAreaStruct triggerAreaItem;
            if (TriggerAreas.TryGetValue(triggerAreaName, out triggerAreaItem))
            {
                // 6/15/2010 - Updated to retrieve the ROC collection.
                // cache selectables
                ReadOnlyCollection<SceneItemWithPick> selectableItems;
                Player.GetSelectableItems(player, out selectableItems);

                // iterate selectableItems for given player, and check if within 'TriggerArea' rectangle.
                var selectableItemsCount = selectableItems.Count;
                for (var k = 0; k < selectableItemsCount; k++)
                {
                    // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                    // cache selectable item
                    SceneItemWithPick selectableItem;
                    if (!Player.GetSelectableItemByIndex(player, k, out selectableItem))
                        break;
                   
                    // make sure not null
                    if (selectableItem == null) continue;

                    // do lamba func itemType check, if 'DoItemTypeFilter' is on!
                    if (itemType(selectableItem) && doItemTypeFilter)
                        continue;

                    // check if within 'TriggerArea' rectangle
                    if (triggerAreaItem.RectangleArea.Contains((int) selectableItem.Position.X,
                                                               (int) selectableItem.Position.Z))
                    {
                        // found an item, so stop search and return true
                        return true;
                    }
                } // End For selectables

                return false;
                
            } // End TryGetValue

            throw new ArgumentException(@"Given TriggerArea name is NOT VALID!", "triggerAreaName");
            
        }

        // 2/7/2011 - Overload method#2
        /// <summary>
        /// Checks the given Player classes 'SelectableItems' arrays, for selectable items which are within
        /// the specified <see cref="TerrainTriggerAreas"/> name; if 'ItemType' param is specified, then this will be excluded 
        /// from search.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="playerNumber"/> is not valid.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="triggerAreaName"/> was not found, or is not valid.</exception>
        /// <param name="playerNumber">PlayerNumber of player to check</param>
        /// <param name="triggerAreaName"><see cref="TerrainTriggerAreas"/> name</param>
        /// <param name="doItemTypeFilter">Do an exlusion of some SceneItem itemType</param>
        /// <param name="itemType">Lamba Fn: 'var => var is itemType', where itemType is some 'SceneItem' class type.</param>
        /// <param name="comparisonToDo">Lamba Func format: 'var => var > 5'; example would be 'cash => cash > 5'</param>
        /// <returns>True/False</returns>
        public static bool DoUnitItemsInTriggerAreaCheck(int playerNumber, string triggerAreaName, bool doItemTypeFilter,
                                                        ScriptFunc<SceneItem, bool> itemType, ScriptFunc<int, bool> comparisonToDo)
        {
            // make sure playerNumber is valid
            if (playerNumber >= TemporalWars3DEngine._maxAllowablePlayers)
                throw new ArgumentOutOfRangeException("playerNumber", @"PlayerNumber given is not valid!");

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return false;

            // make sure player not null
            if (player == null) return false;

            // Try to retrieve 'TriggerArea' name
            TriggerAreaStruct triggerAreaItem;
            if (TriggerAreas.TryGetValue(triggerAreaName, out triggerAreaItem))
            {
                // 6/15/2010 - Updated to retrieve the ROC collection.
                // cache selectables
                ReadOnlyCollection<SceneItemWithPick> selectableItems;
                Player.GetSelectableItems(player, out selectableItems);

                // iterate selectableItems for given player, and check if within 'TriggerArea' rectangle.
                var itemsWithinAreaCount = 0;
                var selectableItemsCount = selectableItems.Count;
                for (var k = 0; k < selectableItemsCount; k++)
                {
                    // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                    // cache selectable item
                    SceneItemWithPick selectableItem;
                    if (!Player.GetSelectableItemByIndex(player, k, out selectableItem))
                        break;

                    // make sure not null
                    if (selectableItem == null) continue;

                    // do lamba func itemType check, if 'DoItemTypeFilter' is on!
                    if (itemType(selectableItem) && doItemTypeFilter)
                        continue;

                    // check if within 'TriggerArea' rectangle
                    if (triggerAreaItem.RectangleArea.Contains((int)selectableItem.Position.X,
                                                               (int)selectableItem.Position.Z))
                    {
                        // found an item, so increase count
                        itemsWithinAreaCount++;
                    }
                } // End For selectables

                // do lamba func check
                return comparisonToDo(itemsWithinAreaCount);

            } // End TryGetValue

            throw new ArgumentException(@"Given TriggerArea name is NOT VALID!", "triggerAreaName");

        }

        // 1/13/2011
        /// <summary>
        /// Gets the rectangle area for a given <paramref name="triggerAreaName"/>.
        /// </summary>
        /// <param name="triggerAreaName">TriggerArea name</param>
        /// <param name="rectangleArea">(OUT) <see cref="TerrainTriggerAreas"/> visual rectangle.</param>
        /// <returns>True/False</returns>
        public static bool GetTriggerAreaRectangle(string triggerAreaName, out Rectangle rectangleArea)
        {
            // Try to retrieve 'TriggerArea' name
            TriggerAreaStruct triggerAreaItem;
            if (TriggerAreas.TryGetValue(triggerAreaName, out triggerAreaItem))
            {
                rectangleArea = triggerAreaItem.RectangleArea;

                return true;
            }

            throw new ArgumentException(@"Given TriggerArea name is NOT VALID!", "triggerAreaName");
        }

        // 10/18/2009
        /// <summary>
        /// Checks the given Player classes 'SelectableItems' arrays, and returns a List of selectables
        /// which are within the given <see cref="TerrainTriggerAreas"/> name.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="playerNumber"/> is not valid.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="triggerAreaName"/> was not found, or is not valid.</exception>
        /// <param name="playerNumber">PlayerNumber of player to check</param>
        /// <param name="triggerAreaName">TriggerArea name</param>
        /// <param name="selectablesWithinArea">(OUT) List of Selectables within the <see cref="TerrainTriggerAreas"/>.</param>
        /// <returns>If any selectables found. True/False</returns>
        public static bool GetAllSelectablesWithinTriggerArea(int playerNumber, string triggerAreaName, out List<SceneItemWithPick> selectablesWithinArea)
        {
            // Create List to hold selectables
            selectablesWithinArea = new List<SceneItemWithPick>();

            // make sure playerNumber is valid
            if (playerNumber >= TemporalWars3DEngine._maxAllowablePlayers)
                throw new ArgumentOutOfRangeException("playerNumber", @"PlayerNumber given is not valid!");
            
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return false;

            // make sure player not null
            if (player == null) return false;

            // Try to retrieve 'TriggerArea' name
            TriggerAreaStruct triggerAreaItem;
            if (TriggerAreas.TryGetValue(triggerAreaName, out triggerAreaItem))
            {
                // 6/15/2010 - Updated to retrieve the ROC collection.
                // cache selectables
                ReadOnlyCollection<SceneItemWithPick> selectableItems;
                Player.GetSelectableItems(player, out selectableItems);

                // iterate selectableItems for given player, and check if within 'TriggerArea' rectangle.
                var selectableItemsCount = selectableItems.Count;
                var anySelectablesFound = false;
                for (var k = 0; k < selectableItemsCount; k++)
                {
                    // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                    // cache selectable item
                    SceneItemWithPick selectableItem;
                    if (!Player.GetSelectableItemByIndex(player, k, out selectableItem))
                        break;

                    // make sure not null
                    if (selectableItem == null) continue;

                    // check if within 'TriggerArea' rectangle
                    if (!triggerAreaItem.RectangleArea.Contains((int) selectableItem.Position.X,
                                                                (int) selectableItem.Position.Z)) continue;
                    // found an item, so add to List
                    anySelectablesFound = true;
                    selectablesWithinArea.Add(selectableItem);
                } // End For selectables

                return anySelectablesFound;

            } // End TryGetValue

            throw new ArgumentException(@"Given TriggerArea name is NOT VALID!", "triggerAreaName");
                
        }

        // 10/18/2009
        /// <summary>
        /// Checks the given Player classes 'SelectableItems' array, and returns some random selectable
        /// sceneItem which is within the given <see cref="TerrainTriggerAreas"/> name.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="playerNumber"/> is not valid.</exception>
        /// <param name="playerNumber">PlayerNumber of player to check</param>
        /// <param name="triggerAreaName"><see cref="TerrainTriggerAreas"/> name</param>
        /// <param name="selectableWithinArea">(OUT) Some random sceneItem within area</param>
        /// <returns>True/False if found any items to return.</returns>
        public static bool GetSomeSelectableWithinTriggerArea(int playerNumber, string triggerAreaName, out SceneItemWithPick selectableWithinArea)
        {
            selectableWithinArea = null;

            // make sure playerNumber is valid
            if (playerNumber >= TemporalWars3DEngine._maxAllowablePlayers)
                throw new ArgumentOutOfRangeException("playerNumber", @"PlayerNumber given is not valid!");

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return false;

            // make sure player not null
            if (player == null) return false;

            // 1st - Get all Selectables within TriggerArea
            List<SceneItemWithPick> selectablesWithinArea;
            if (GetAllSelectablesWithinTriggerArea(playerNumber, triggerAreaName, out selectablesWithinArea))
            {
                // Pick some selectableItem from list to return at Random.
                var randomIndex = RandomGenerator.Next(0, selectablesWithinArea.Count);

                // return random selectable.
                selectableWithinArea = selectablesWithinArea[randomIndex];
                return true;

            } // End Get all selectables with area

            return false;
        }


        // 10/1/2009
        /// <summary>
        /// Checks the Player classes 'SelectableItems' arrays, for items which are within
        /// one of the <see cref="TerrainTriggerAreas"/> rectangles.  If any item is found within the area, the
        /// 'ContainsSomeSelectableItem' is set to TRUE for the given <see cref="TerrainTriggerAreas"/>. 
        /// </summary>
        private static void DoSelectableItemsInTriggerAreaCheck()
        {
            // Get Dictionary keys
            var triggerAreasCount = TriggerAreas.Keys.Count;
            if (_keys.Length < triggerAreasCount)
                Array.Resize(ref _keys, triggerAreasCount);
            TriggerAreas.Keys.CopyTo(_keys, 0);

            // 6/15/2010 - Updated to use new GetPlayers.
            Player[] players;
            TemporalWars3DEngine.GetPlayers(out players);

            // iterate Dictionary list
            for (var i = 0; i < triggerAreasCount; i++)
            {
                // get 'TriggerArea' from Dictionary
                var triggerAreaItem = TriggerAreas[_keys[i]];

                // iterate selectableItems for each player, and check if within 'TriggerArea' rectangle.
                var playersCount = players.Length;
                var containsAnItem = false;
                for (var j = 0; j < playersCount; j++)
                {
                    // make sure player not null
                    
                    // 6/15/2010 - Updated to use new GetPlayer method.
                    Player player;
                    if (!TemporalWars3DEngine.GetPlayer(j, out player))
                        break;

                    if (player == null) continue;

                    // 6/15/2010 - Updated to retrieve the ROC collection.
                    // cache selectables
                    ReadOnlyCollection<SceneItemWithPick> selectableItems;
                    Player.GetSelectableItems(player, out selectableItems);

                    var selectableItemsCount = selectableItems.Count;
                    for (var k = 0; k < selectableItemsCount; k++)
                    {
                        // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                        // cache selectable item
                        SceneItemWithPick selectableItem;
                        if (!Player.GetSelectableItemByIndex(player, i, out selectableItem))
                            break;

                        // make sure not null
                        if (selectableItem == null) continue;

                        // check if within 'TriggerArea' rectangle
                        if (triggerAreaItem.RectangleArea.Contains((int)selectableItem.Position.X, (int)selectableItem.Position.Z))
                        {
                            containsAnItem = true;
                        }
                    } // End For selectables
                } // End for Players

                // Update 'ContainsSomeSelectableItem'
                triggerAreaItem.ContainsSomeSelectableItem = containsAnItem;
                TriggerAreas[_keys[i]] = triggerAreaItem;

            } // End For TriggerAreas
        }

        // 10/12/2009
        /// <summary>
        /// Kills all 'SelectableItems' belonging to the given player, 
        /// in the given <see cref="TerrainTriggerAreas"/> name. (Scripting purposes)
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="playerNumber"/> is not valid.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="triggerAreaName"/> was not found, or is not valid.</exception>
        /// <param name="playerNumber">PlayerNumber of player to check</param>
        /// <param name="triggerAreaName"><see cref="TerrainTriggerAreas"/> name</param>
        public static void KillAllUnitsBelongingToPlayerInTriggerArea(int playerNumber, string triggerAreaName)
        {
            // make sure playerNumber is valid
            if (playerNumber >= TemporalWars3DEngine._maxAllowablePlayers)
                throw new ArgumentOutOfRangeException("playerNumber", @"PlayerNumber given is not valid!");
            
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return;

            // make sure player not null
            if (player == null) return;
            
            // Try to retrieve 'TriggerArea' name
            TriggerAreaStruct triggerAreaItem;
            if (TriggerAreas.TryGetValue(triggerAreaName, out triggerAreaItem))
            {
                // 6/15/2010 - Updated to retrieve the ROC collection.
                // cache selectables
                ReadOnlyCollection<SceneItemWithPick> selectableItems;
                Player.GetSelectableItems(player, out selectableItems);

                // iterate selectableItems for given player, and check if within 'TriggerArea' rectangle.
                var selectableItemsCount = selectableItems.Count;
                for (var k = 0; k < selectableItemsCount; k++)
                {
                    // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                    // cache selectable item
                    SceneItemWithPick selectableItem;
                    if (!Player.GetSelectableItemByIndex(player, k, out selectableItem))
                        break;

                    // make sure not null
                    if (selectableItem == null) continue;

                    // check if within 'TriggerArea' rectangle
                    if (!triggerAreaItem.RectangleArea.Contains((int) selectableItem.Position.X,
                                                                (int) selectableItem.Position.Z)) continue;
                    // **
                    // found an item in area, so KILL it!
                    // **

                    // get current health
                    var currentHealth = selectableItem.CurrentHealth;

                    // Reduce item's health by currentHealth + 100 to make sure it is dead!
                    selectableItem.ReduceHealth(currentHealth + 100, 0);
                } // End For selectables

                return;
            }

            throw new ArgumentException(@"Given TriggerArea name is NOT VALID!", "triggerAreaName");
        }

#if !XBOX360

        // 4/13/2010 - Stores the current Selected TriggerArea name
        private static string _selectedTriggerAreaName;

        // 4/13/2010 - Stores the DragMove vars.
        private static string _dragMoveTriggerAreaName;
        private static bool _dragMoveStarted;

        // 10/1/2009
        /// <summary>
        /// When propeties tool window is open, checks for creation of new <see cref="TerrainTriggerAreas"/> and checks if cursor is
        /// within an existing <see cref="TerrainTriggerAreas"/> rectangle.
        /// </summary>
        private static void DoPropertiesToolCheck()
        {
            // make sure not in the control itself; otherwise skip check
            var propertiesTools = TerrainEditRoutines.PropertiesTools; // 11/21/2009
            if (propertiesTools == null || propertiesTools.IsMouseInControl())
                return;

            // Check if Index tab 2, for "Areas"
            if (!propertiesTools.IsTabIndexActive(2))
                return;

            // Get Picked cursor position
            Vector3 cursorPosition;
            TerrainPickingRoutines.GetCursorPosByPickedRay(PickRayScale.NoChange, out cursorPosition);

            // 4/13/2010 - Check for the start of a new TriggerArea rectangle.
            CheckForTriggerAreaCreation(ref cursorPosition);

            // 4/13/2010 - Check for TriggerArea dragMove.
            Vector3 newPosition;
            if (!string.IsNullOrEmpty(_dragMoveTriggerAreaName) && HandleInput.CheckForItemDragMove(ref _dragMoveStarted, out newPosition))
                UpdateTriggerArea(_dragMoveTriggerAreaName, ref newPosition);
            else
            {
                _dragMoveTriggerAreaName = string.Empty;
                _updateTriggerAreaPositionDelta = Vector3.Zero; // 6/2/2012
                _updateTriggerAreaPriorPosition = Vector3.Zero; // 6/2/2012
            }

            // 4/13/2010 - Check for the selection of some TriggerArea rectangle.
            CheckForTriggerAreaSelection(propertiesTools, ref cursorPosition);

            // 4/13/2010 - Check for Start of DragMove operation.
            if (HandleInput.DoDragMoveCheck(ref _dragMoveStarted))
                _dragMoveTriggerAreaName = _selectedTriggerAreaName;
        }

        // 4/13/2010
        /// <summary>
        /// Checks for a current <see cref="TerrainTriggerAreas"/> selection, which is caused by the
        /// user selecting with a left-click on screen.
        /// </summary>
        /// <param name="propertiesTools">PropertiesTool form instance</param>
        /// <param name="cursorPosition">Current cursor position</param>
        private static void CheckForTriggerAreaSelection(PropertiesTools propertiesTools, ref Vector3 cursorPosition)
        {
            // 4/13/2010 - Reset TriggerArea selection key - Updated to ONLY clear when 'DragMoveStarted' is false.
            if (!_dragMoveStarted) _selectedTriggerAreaName = string.Empty;

            // do check to see which TriggerAreas the cursor is in.
            // ** Normally, would not use the ForEach construct, since this causes garbage due to enumeration; however
            //    since this is only used during editing and not during critical game play, it will be allowed - Ben.
            foreach (var triggerArea in TriggerAreas)
            {
                // If contains cursor, then make Red, else Orange.
                var visualRectangleArea = triggerArea.Value.VisualRectangleArea; // 5/19/2010 - Cache
                var length = visualRectangleArea.Length; // 5/19/2010 - Cache
                if (triggerArea.Value.RectangleArea.Contains((int)cursorPosition.X, (int)cursorPosition.Z))
                {
                    // Update color
                    for (var i = 0; i < length; i++)
                        visualRectangleArea[i].Color = Color.Red;

                    // Check if user pressed left mouse button, to select.
                    if (HandleInput.InputState.LeftMouseButton)
                    {
                        // then show selection in ListView of Areas tab in 'PropetiesTool' form.
                        propertiesTools.SelectTriggerAreaInListView(triggerArea.Key);

                        // 4/13/2010 - Store selected TriggerArea name
                        _selectedTriggerAreaName = triggerArea.Key;
                    }
                }
                else
                {
                    // Update color
                    for (var i = 0; i < length; i++)
                        visualRectangleArea[i].Color = Color.Orange;
                }
            }
        }

        // 4/13/2010
        /// <summary>
        /// Checks for the start of a new <see cref="TerrainTriggerAreas"/> rectangle.
        /// </summary>
        /// <param name="cursorPosition">Current Cursor position</param>
        private static void CheckForTriggerAreaCreation(ref Vector3 cursorPosition)
        {
            // 12/17/2009 - Updated to also check the 'DoDefineArea'.
            // check for start of creation of new TriggerArea
            if (HandleInput.InputState.StartAreaSelect && DoDefineArea)
            {
                if (!AreaSelect)
                {
                    // Set Up AreaSelect Rectangle
                    AreaSelect = true;
                    StartSelect = new Vector3 { X = cursorPosition.X, Z = cursorPosition.Z };

                    CursorPosition = new Vector3 { X = cursorPosition.X, Z = cursorPosition.Z };
                }
                else
                {
                    // Continue updating AreaSelect Rectangle
                    CursorPosition = new Vector3 { X = cursorPosition.X, Z = cursorPosition.Z };

                } // End If AreaSelect = true
            } // End if StartAreaSelect
            else
                AreaSelect = false;
        }

#endif

        /// <summary>
        /// Draws the <see cref="TerrainTriggerAreas"/> rectangle into the game world.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public override void Draw(GameTime gameTime)
        {
#if DEBUG
            // 5/26/2010 - DEBUG
            StopWatchTimers.StartStopWatchInstance(StopWatchName.GameDrawLoop_Main_TriggerAreas);
#endif

#if !XBOX360
            // 9/29/2009 - Only draw when PropertiesTool form is open.
            if (TerrainEditRoutines.PropertiesTools == null
                || TerrainEditRoutines.PropertiesTools.Visible == false)
                return;
            
            // Create Area Rectangle
            CreateAreaRectangle(Color.Magenta);

            // Draw Current Area Rectangle
            DrawCurrentAreaRectangle(GraphicsDevice);

            // Draw Dictionary Area Rectangles
            DrawDictionaryTriggerAreas(GraphicsDevice);
#endif

            base.Draw(gameTime);

#if DEBUG
            // 5/26/2010 - DEBUG
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.GameDrawLoop_Main_TriggerAreas);
#endif
        }


        // 9/28/2009
        /// <summary>
        /// Adds the current <see cref="TerrainTriggerAreas"/> defined, to the internal
        /// dictionary of <see cref="TerrainTriggerAreas"/>, with the given name as key.
        /// </summary>
        /// <param name="triggerAreaName"><see cref="TerrainTriggerAreas"/> name</param>
        /// <returns>True/False of result</returns>
        public static bool AddNewTriggerArea(string triggerAreaName)
        {
            // make sure name not already used
            if (TriggerAreas.ContainsKey(triggerAreaName))
                return false;

            // create copy
            var visualRectangeArea = new VertexPositionColor[6];

            // Update Color for current visualRectangle, to now be orange.
            var currentVisualRectangleArea = _currentVisualRectangleArea; // 5/19/2010 - Cache
            var length = currentVisualRectangleArea.Length;
            for (var i = 0; i < length; i++)
            {
                visualRectangeArea[i].Position = currentVisualRectangleArea[i].Position;
                visualRectangeArea[i].Color = Color.Orange;
            }

            // create new TriggerAreaStruct
            var triggerArea = new TriggerAreaStruct
                                  {
                                      Name = triggerAreaName,
                                      RectangleArea = new Rectangle(_rectangleArea.X, _rectangleArea.Y, _rectangleArea.Width, _rectangleArea.Height),
                                      VisualRectangleArea = visualRectangeArea,
                                  };
            
            // else, add current item as new trigger area
            TriggerAreas.Add(triggerAreaName, triggerArea);
   
            return true;
        }

        // 5/25/2012
        /// <summary>
        /// Adds the current <see cref="TerrainTriggerAreas"/> defined as <paramref name="rectangleArea"/>, to the internal
        /// dictionary of <see cref="TerrainTriggerAreas"/>, with the given name as key. (Scripting Purposes)
        /// </summary>
        /// <param name="triggerAreaName"><see cref="TerrainTriggerAreas"/> name</param>
        /// <param name="rectangleArea">Rectangle defined as trigger area.</param>
        /// <returns>True/False of result</returns>
        public static bool AddNewTriggerArea(string triggerAreaName, ref Rectangle rectangleArea)
        {
            // store the rectangeArea
            _rectangleArea = rectangleArea;

            // Create the visual triangle points, to draw rectangle.
            CreateVisualRectangle(ref _rectangleArea, ref _currentVisualRectangleArea, Color.Orange);

            // call to create TriggerArea
            var success = AddNewTriggerArea(triggerAreaName);

            // 6/3/2012 - Update flag to designate created by ScriptingAction - this removes from save operations.
            var triggerAreaStruct = TriggerAreas[triggerAreaName];
            triggerAreaStruct.SpawnWithScriptingAction = true;
            TriggerAreas[triggerAreaName] = triggerAreaStruct;

            return success;
        }

        // 4/13/2010
        /// <summary>
        /// Helper method, which updates the <see cref="TerrainTriggerAreas"/> with new position.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when <paramref name="triggerAreaName"/> was not found, or is not valid.</exception>
        /// <param name="triggerAreaName"><see cref="TerrainTriggerAreas"/> name to update</param>
        /// <param name="newPosition"><see cref="Vector3"/> new position value</param>
        private static void UpdateTriggerArea(string triggerAreaName, ref Vector3 newPosition)
        {
            // Retrieve TriggerArea by name
            TriggerAreaStruct triggerAreaStruct;
            if (TriggerAreas.TryGetValue(triggerAreaName, out triggerAreaStruct))
            {
                // update the start positions of rectangle
                triggerAreaStruct.RectangleArea.X += (int)_updateTriggerAreaPositionDelta.X; // (int) newPosition.X;
                triggerAreaStruct.RectangleArea.Y += (int)_updateTriggerAreaPositionDelta.Z; //(int) newPosition.Z;

                // Create the visual triangle points, to draw rectangle.
                CreateVisualRectangle(ref triggerAreaStruct.RectangleArea, ref triggerAreaStruct.VisualRectangleArea, Color.Magenta);

                // Update back to dictionary
                TriggerAreas[triggerAreaName] = triggerAreaStruct;

                // 6/2/2012 - Caclulate positino delta
                if (!_updateTriggerAreaPriorPosition.Equals(Vector3.Zero))
                    Vector3.Subtract(ref newPosition, ref _updateTriggerAreaPriorPosition, out _updateTriggerAreaPositionDelta);
                _updateTriggerAreaPriorPosition = newPosition;

                return;
            }

            throw new ArgumentException("UpdateTriggerArea method failed, because TriggerArea Name given is not valid.");
        }
        
        /// <summary>
        /// Creates the rectangle used to draw the <see cref="TerrainTriggerAreas"/> rectangle on screen.
        /// </summary>
        private static void CreateAreaRectangle(Color colorToUse)
        {
            // Check if AreaSelect ON
            if (!AreaSelect) return;
           
            // Create initial rectangle area, using mousePosition.
            var recWidth = (int)(_cursorPosition.X - _startSelect.X);
            var recHeight = (int)(_cursorPosition.Z - _startSelect.Z);               
            _rectangleArea.X = (int)_startSelect.X; _rectangleArea.Y = (int)_startSelect.Z;
            _rectangleArea.Width = recWidth; _rectangleArea.Height = recHeight;

            // Create the visual triangle points, to draw rectangle.
            CreateVisualRectangle(ref _rectangleArea, ref _currentVisualRectangleArea, colorToUse);
        }

        /// <summary>
        /// Creates the boundingRectangleArea, used to show graphical rectangle, using the
        /// given rectangle data.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="visualRectangle"/> length is less than 6.</exception>
        /// <param name="rectangle"><see cref="Rectangle"/> struct</param>
        /// <param name="visualRectangle">Collection of <see cref="VertexPositionColor"/></param>
        /// <param name="colorToUse"><see cref="Color"/> to use</param>
        private static void CreateVisualRectangle(ref Rectangle rectangle, ref VertexPositionColor[] visualRectangle, Color colorToUse)
        {
            // make sure array exist
            if (visualRectangle == null)
                visualRectangle = new VertexPositionColor[6];

            // make sure proper size array
            if (visualRectangle.Length < 6)
                throw new ArgumentOutOfRangeException("visualRectangle", @"Array must be a size of 6.");

            // Set Positions
            visualRectangle[0].Position = new Vector3(rectangle.X, 50, rectangle.Y);
            visualRectangle[1].Position = new Vector3(rectangle.Right, 50, rectangle.Bottom);
            visualRectangle[2].Position = new Vector3(rectangle.X, 50, rectangle.Bottom);

            visualRectangle[3].Position = new Vector3(rectangle.X, 50, rectangle.Y);
            visualRectangle[4].Position = new Vector3(rectangle.Right, 50, rectangle.Top);
            visualRectangle[5].Position = new Vector3(rectangle.Right, 50, rectangle.Bottom);

            // Set Color
            for (var i = 0; i < 6; i++)
                visualRectangle[i].Color = colorToUse;

        }

        /// <summary>
        /// Draws the 'Current' <see cref="TerrainTriggerAreas"/> rectangle, into the game world.
        /// </summary>
        private static void DrawCurrentAreaRectangle(GraphicsDevice graphicsDevice)
        {
            // XNA 4.0 Updates - RenderState replaced with 4 new states; BlendState, RasterizerState, DepthStencilState, or SampleState.
            /*graphicsDevice.RenderState.FillMode = FillMode.WireFrame;
            graphicsDevice.RenderState.CullMode = CullMode.None;
            graphicsDevice.RenderState.DepthBufferEnable = false;*/
            graphicsDevice.RasterizerState = _rasterizerState1;
            graphicsDevice.DepthStencilState = _depthStencilState1;

            var basicEffect = _lineEffect; // 5/19/2010 - CAche
            basicEffect.View = Camera.View;
            basicEffect.Projection = Camera.Projection;

            // XNA 4.0 updates - Begin() and End() obsolete.
            //basicEffect.Begin();
            basicEffect.CurrentTechnique.Passes[0].Apply();

            // XNA 4.0 Updates - VertexDeclaration obsolete.
            // Draw the triangle.
            //graphicsDevice.VertexDeclaration = _lineVertexDeclaration;
            graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _currentVisualRectangleArea, 0, 2);

            // XNA 4.0 updates - Begin() and End() obsolete.
            //basicEffect.CurrentTechnique.Passes[0].End();
            //basicEffect.End();

            // XNA 4.0 Updates - RenderState replaced with 4 new states; BlendState, RasterizerState, DepthStencilState, or SampleState.
            //graphicsDevice.RenderState.FillMode = FillMode.Solid;
            graphicsDevice.RasterizerState = _rasterizerState2;
            graphicsDevice.DepthStencilState = _depthStencilState2;
        }

        /// <summary>
        /// Draws all instances of <see cref="TerrainTriggerAreas"/> contain in the dictionary, into
        /// the game world.
        /// </summary>
        /// <param name="graphicsDevice"><see cref="GraphicsDevice"/> instance</param>
        private static void DrawDictionaryTriggerAreas(GraphicsDevice graphicsDevice)
        {
            // XNA 4.0 Updates - RenderState replaced with 4 new states; BlendState, RasterizerState, DepthStencilState, or SampleState.
            /*graphicsDevice.RenderState.FillMode = FillMode.WireFrame;
            graphicsDevice.RenderState.CullMode = CullMode.None;
            graphicsDevice.RenderState.DepthBufferEnable = false;*/
            graphicsDevice.RasterizerState = _rasterizerState1;
            graphicsDevice.DepthStencilState = _depthStencilState1;

            var basicEffect = _lineEffect; // 5/19/2010 - CAche
            basicEffect.View = Camera.View;
            basicEffect.Projection = Camera.Projection;

            // XNA 4.0 updates - Begin() and End() obsolete.
            //basicEffect.Begin();
            basicEffect.CurrentTechnique.Passes[0].Apply();

            // XNA 4.0 Updates - VertexDeclaration obsolete.
            // Draw the triangle.
            //graphicsDevice.VertexDeclaration = _lineVertexDeclaration;

            // Iterate Dictionary to draw each VisualRectangle.
            foreach (var triggerArea in TriggerAreas)
            {
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, triggerArea.Value.VisualRectangleArea, 0, 2);
            }

            // XNA 4.0 updates - Begin() and End() obsolete.
            //basicEffect.CurrentTechnique.Passes[0].End();
            //basicEffect.End();

            // XNA 4.0 Updates - RenderState replaced with 4 new states; BlendState, RasterizerState, DepthStencilState, or SampleState.
            //graphicsDevice.RenderState.FillMode = FillMode.Solid;
            graphicsDevice.RasterizerState = _rasterizerState2;
            graphicsDevice.DepthStencilState = _depthStencilState2;
        }

#if !XBOX360


        /// <summary>
        /// Saves the internal <see cref="TerrainTriggerAreas"/>, as a list; this should be called
        /// from the 'TerrainStorageRoutine' class.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the save operation fails.</exception>
        /// <param name="storageTool"><see cref="Storage"/> instance</param>
        /// <param name="mapName">Map name</param>
        /// <param name="mapType">Map type; SP for single or MP for Multi-player</param>
        public static void SaveTriggerAreas(Storage storageTool, string mapName, string mapType)
        {
            //
            // iterate dictionary and transfer to list for saving
            //

            // 6/3/2012 - Filter out triggerArea's which were created with a Scripting-Action.
            var fltTriggerAreas =
                TriggerAreas.Where(triggerArea => !triggerArea.Value.SpawnWithScriptingAction);

            // Create final 'TriggerAreaStructToSave' List collection.
            var tmpTriggerAreas = fltTriggerAreas.Select(triggerArea => new TriggerAreaStructToSave
                                                                         {
                                                                             Name = triggerArea.Value.Name, RectangleArea = triggerArea.Value.RectangleArea,
                                                                         }).ToList();

            // 4/7/2010: Updated to use 'ContentMapsLoc' global var.
            int errorCode;
            if (storageTool.StartSaveOperation(tmpTriggerAreas, "tdTriggerAreas.ttd",
                                               String.Format(@"{0}\{1}\{2}\", TemporalWars3DEngine.ContentMapsLoc,
                                                             mapType, mapName), out errorCode)) return;

            // 4/7/2010 - Error occured, so check which one.
            if (errorCode == 1)
            {
                MessageBox.Show(@"Locked files detected for 'TriggerAreas' (tdTriggerAreas.ttd) save.  Unlock files, and try again.",
                                @"Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (errorCode == 2)
            {
                MessageBox.Show(@"Directory location for 'TriggerAreas' (tdTriggerAreas.ttd) save, not found.  Verify directory exist, and try again.",
                                @"Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            throw new InvalidOperationException("The Save Struct TriggerAreas Operation Failed.");
        }
#endif

        // 9/29/2009; 11/19/2009: Updated with 'mapType' param.
        /// <summary>
        /// Loads the internal <see cref="TerrainTriggerAreas"/>, as a list; this should be called
        /// from the 'TerrainStorageRoutine' class.
        /// </summary>
        /// <param name="storageTool"><see cref="Storage"/> instance</param>
        /// <param name="mapName">Map name</param>
        /// <param name="mapType">Map type; SP for single or MP for Multi-player</param>
        public static void LoadTriggerAreas(Storage storageTool, string mapName, string mapType)
        {
            // make sure dictionary is not null
            if (TriggerAreas == null)
                TriggerAreas = new Dictionary<string, TriggerAreaStruct>();

            // 4/7/2010: Updated to use 'ContentMapsLoc' global var.
            // Load MapMarkerPositions Struct data
            List<TriggerAreaStructToSave> tmpTriggerAreas;
            if (!storageTool.StartLoadOperation(out tmpTriggerAreas, "tdTriggerAreas.ttd",
                                                String.Format(@"{0}\{1}\{2}\",TemporalWars3DEngine.ContentMapsLoc, mapType, mapName),
                                                StorageLocation.TitleStorage))
            {
#if DEBUG
                Debug.WriteLine("LoadTriggerAreas method, of TerrainTriggerAreas, failed to load 'tdTriggerAreas.ttd' file.");
#endif
                return; // TriggerAreas are not required, so just return with failed.
            }

            // repopulate the dictionary with given list
            TriggerAreas.Clear();
            var count = tmpTriggerAreas.Count;
            for (var i = 0; i < count; i++)
            {
                // Retrieve Struct
                var triggerAreaItem = tmpTriggerAreas[i];

                // Create TriggerAreaStruct for Dictionary
                var triggerArea = new TriggerAreaStruct
                                      {
                                         Name = triggerAreaItem.Name,
                                         RectangleArea = triggerAreaItem.RectangleArea,
                                      };

                // Re-create the VisualRectangle
                CreateVisualRectangle(ref triggerArea.RectangleArea, ref triggerArea.VisualRectangleArea, Color.Orange);
                

                // Add record to Dictionary; make sure record doesn't already exist, incase file was changed!
                if (!TriggerAreas.ContainsKey(triggerAreaItem.Name))
                    TriggerAreas.Add(triggerAreaItem.Name, triggerArea);
            }
        }

        // Dispose of resources
        /// <summary>
        /// Releases the unmanaged resources used by the DrawableGameComponent and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;

            if (_lineEffect != null)
                _lineEffect.Dispose();
            if (_lineVertexDeclaration != null)
                _lineVertexDeclaration.Dispose();

            // 1/8/2010
            if (_currentVisualRectangleArea != null)
                Array.Clear(_currentVisualRectangleArea, 0 ,_currentVisualRectangleArea.Length);
        }
    }
}