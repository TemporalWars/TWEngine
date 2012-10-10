#region File Description
//-----------------------------------------------------------------------------
// TerrainEditRoutines.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.GameCamera;
using ImageNexus.BenScharbach.TWEngine.GameScreens;
using ImageNexus.BenScharbach.TWEngine.HandleGameInput;
using ImageNexus.BenScharbach.TWEngine.Interfaces;
using ImageNexus.BenScharbach.TWEngine.Players;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWEngine.Terrain;
using ImageNexus.BenScharbach.TWEngine.Terrain.Enums;
using ImageNexus.BenScharbach.TWEngine.Terrain.Structs;
using ImageNexus.BenScharbach.TWLate.RTS_MinimapInterfaces.Minimap;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DrawMode = ImageNexus.BenScharbach.TWEngine.Terrain.Enums.DrawMode;
using Keys = Microsoft.Xna.Framework.Input.Keys;

#if !XBOX360
using TWEngine.TerrainTools;
using System.Windows.Forms;
#endif

namespace ImageNexus.BenScharbach.TWEngine.TerrainTools
{
    // 7/29/2008
    /// <summary>
    /// The <see cref="TerrainEditRoutines"/> class holds all the terrain editing algorithms, like
    /// the texture splatting or terrain deformation actions.  Primarily, this is used by the Terrain
    ///  Tools, like the Paint tool.
    /// </summary>
    public class TerrainEditRoutines : GameComponent
    {

#if !XBOX360
        // 7/1/2010 - Instance of WPF tool.
        private static TerrainWPFTools _terrainWPFTools;
#endif

        // Game Ref
        private static Game _gameInstance;
        // Ref to TerrainShape Class
        private static TerrainShape _terrainShape;
      
        // Changes Shader file during Draw to use V1 or V2; V2 has groundcursor for editing. 
        internal static ToolType _toolInUse = ToolType.None;
        private KeyboardState _keyState;

        // Window Tools Form Editors
        #if XBOX360
        #else
        #endif

        private static Vector3 _groundCursorPosition = Vector3.Zero;
        private static int _groundCursorSize = 50;
        private static int _groundCursorStrength = 20;
        private static int _paintCursorSize = 5;
        private static int _paintCursorStrength = 20;
        private static float _constantFeetToAdd = 1;

        // 4/25/2008 - This locks the Call to Method 'TessellateToLowerLOD' from being constantly called until it is finished.
        //             Currently, this is unlocked when the mouse button is released in the TerrainShape
        //             HeightTools Section of calling Raise or Lower Editing methods.

        #region Properties

        // 7/1/2010
        /// <summary>
        /// During edit-mode, stores which VertexBuffer to use for Stream-1.
        /// </summary>
        /// <remarks>false = 1, true = 2</remarks>
        public static bool CurrentVertexBuffer { get; private set; }

       

        #if XBOX360
        #else

        ///<summary>
        /// Get or Set the <see cref="PropertiesTools"/>
        ///</summary>
        public static PropertiesTools PropertiesTools { get; set; }

         ///<summary>
        /// Gets the instance of <see cref="TerrainWPFTools"/>.
        ///</summary>
        public static TerrainWPFTools TerrainWpfTools
        {
            get { return _terrainWPFTools; }
        }

#endif

        ///<summary>
        /// Get or Set the <see cref="Texture2D"/> ground cursor.
        ///</summary>
        public static Texture2D GroundCursorTex { get; set; }

        ///<summary>
        /// Get or Set the <see cref="Texture2D"/> paint cursor.
        ///</summary>
        public static Texture2D PaintCursorTex { get; set; }

        ///<summary>
        /// Get or Set the <see cref="EffectParameter"/> for ground cursor position.
        ///</summary>
        public static EffectParameter GroundCursorPositionParam { get; set; }

        ///<summary>
        /// Get or Set the <see cref="EffectParameter"/> for ground cursor texture.
        ///</summary>
        public static EffectParameter GroundCursorTextureParam { get; set; }

        ///<summary>
        /// Get or Set the <see cref="EffectParameter"/> for paint cursor texture.
        ///</summary>
        public static EffectParameter PaintCursorTextureParam { get; set; }

        ///<summary>
        /// Get or Set the <see cref="EffectParameter"/> for ground cursor size.
        ///</summary>
        public static EffectParameter GroundCursorSizeParam { get; set; }

        ///<summary>
        /// Get or Set the <see cref="EffectParameter"/> for paint cursor size.
        ///</summary>
        public static EffectParameter PaintCursorSizeParam { get; set; }

        ///<summary>
        /// Get or Set the <see cref="EffectParameter"/> to show height cursor.
        ///</summary>
        public static EffectParameter ShowHeightCursorParam { get; set; }

        ///<summary>
        /// Get or Set the <see cref="EffectParameter"/> to show paint cursor.
        ///</summary>
        public static EffectParameter ShowPaintCursorParam { get; set; }

        ///<summary>
        /// Get or Set if in 'EditMode'.
        ///</summary>
        public static bool EditMode { get; set; }

        ///<summary>
        /// Get or Set <see cref="Vector3"/> ground cursor position.
        ///</summary>
        public static Vector3 GroundCursorPosition
        {
            get { return _groundCursorPosition; }
            set { _groundCursorPosition = value; }
        }

        ///<summary>
        /// Get or Set ground cursor size.
        ///</summary>
        public static int GroundCursorSize
        {
            get { return _groundCursorSize; }
            set { _groundCursorSize = value; }
        }

        ///<summary>
        /// Get or Set ground cursor strength.
        ///</summary>
        public static int GroundCursorStrength
        {
            get { return _groundCursorStrength; }
            set { _groundCursorStrength = value; }
        }

        ///<summary>
        /// Get or Set paint cursor size.
        ///</summary>
        public static int PaintCursorSize
        {
            get { return _paintCursorSize; }
            set { _paintCursorSize = value; }
        }

        ///<summary>
        /// Get or Set paint cursor strength.
        ///</summary>
        public static int PaintCursorStrength
        {
            get { return _paintCursorStrength; }
            set { _paintCursorStrength = value; }
        }

        ///<summary>
        /// Get or Set to show height cursor.
        ///</summary>
        public static bool ShowHeightCursor { get; set; }

        ///<summary>
        /// Get or Set to show paint cursor.
        ///</summary>
        public static bool ShowPaintCursor { get; set; }

        ///<summary>
        /// Get or Set to use constant feet.
        ///</summary>
        public static bool UseConstantFeet { get; set; }

        ///<summary>
        /// Get or Set constant feet to add.
        ///</summary>
        /// <remarks>Requires the <see cref="UseConstantFeet"/> to be set to TRUE</remarks>
        public static float ConstantFeetToAdd
        {
            get { return _constantFeetToAdd; }
            set { _constantFeetToAdd = value; }
        }

        ///<summary>
        /// Get or Set constant feet value
        ///</summary>
        /// <remarks>Requires the <see cref="UseConstantFeet"/> to be set to TRUE</remarks>
        public static float ConstantFeetValue { get; set; }

        ///<summary>
        /// Get or Set to flatten height.
        ///</summary>
        public static float FlattenHeight { get; set; }

        ///<summary>
        /// Get or Set to tessellate to a lower level-of-detail for quad.
        ///</summary>
        public static bool TessellateToLowerLODLocked { get; set; }

        ///<summary>
        /// Get or Set the <see cref="ToolType"/> in use.
        ///</summary>
        public static ToolType ToolInUse
        {
            get { return _toolInUse; }
            set { _toolInUse = value; }
        }

        #endregion


        
        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        ///<param name="terrainShape"><see cref="TerrainShape"/> instance</param>
        public TerrainEditRoutines(Game game, TerrainShape terrainShape) : base(game)
        {

            // 1/2/2010
            _gameInstance = game;

            // Save Ref to TerrainShape
            _terrainShape = terrainShape;

#if !XBOX360
            // 7/1/2010 - Create instance of WPF form.
            _terrainWPFTools = new TerrainWPFTools();
#endif
        }
       
#if !XBOX360
       

#endif
        
        /// <summary>
        /// Calls the <see cref="HandleInput"/> each game cycle.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public sealed override void Update(GameTime gameTime)
        {            
            //HandleInput(); // 2/5/2011 - Removed, now called from HandleInput.

#if !XBOX360
            // 7/1/2010 - Call update for WPFTools component.
            if (TerrainWpfTools != null)
                TerrainWpfTools.Update(gameTime);
#endif

            base.Update(gameTime);
        }


        // 11/20/2008
        /// <summary>
        /// Handles user input
        /// </summary>
        internal static void HandleInput(InputState inputState)
        {

#if !XBOX360
            // 6/2/2012
            DoTerrainEditModePicking(inputState);

            // 12/6/2009
            // Turn on NavigationTool
            //if (_keyState.IsKeyDown(Keys.OemTilde)) ActivateTool(ToolType.MainMenuTool);
            if (inputState.IsNewKeyPress(Keys.OemTilde)) ActivateTool(ToolType.MainMenuTool);

            // 3/18/2008
            // Turn on HeightTool Editor
            //if (_keyState.IsKeyDown(Keys.F1)) ActivateTool(ToolType.HeightTool);
            if (inputState.IsNewKeyPress(Keys.F1)) ActivateTool(ToolType.HeightTool);

            // 4/22/2008
            // Turn on PaintTool Editor
            //if (_keyState.IsKeyDown(Keys.F2)) ActivateTool(ToolType.PaintTool);
            if (inputState.IsNewKeyPress(Keys.F2)) ActivateTool(ToolType.PaintTool);


            // 4/30/2008
            // Turn on ItemsTool Editor
            //if (_keyState.IsKeyDown(Keys.F3)) ActivateTool(ToolType.ItemTool);
            if (inputState.IsNewKeyPress(Keys.F3)) ActivateTool(ToolType.ItemTool);

            // 5/9/2008
            // Turn on PropertiesTool Editor
            //if (_keyState.IsKeyDown(Keys.F4)) ActivateTool(ToolType.PropertiesTool);
            if (inputState.IsNewKeyPress(Keys.F4)) ActivateTool(ToolType.PropertiesTool);

#endif
        }

#if !XBOX360

        // 6/2/2012
        /// <summary>
        /// Helper method which does the terrain editing for <see cref="SceneItem"/>s.
        /// </summary>
        private static void DoTerrainEditModePicking(InputState input)
        {
            // If (TerrainIsIn == EditMode), then we check all items in the Screen Scene 
            // for pickable, since in EditMode, we want the ability to be able to delete
            // any SceneItemOwner placed on the Terrain.
            if (TerrainShape.TerrainIsIn != TerrainIsIn.EditMode) return;

            // Make sure Properties Tool does not have mouse in control
            if (ToolInUse == ToolType.PropertiesTool
                || ToolInUse == ToolType.ItemTool)
            {
                // Only call when mouse not in control
                if (PropertiesTools != null &&
                    !PropertiesTools.IsMouseInControl())
                    EditModePicking(input);

                // 4/3/2011
                // Only call when mouse not in control
                if (!TerrainItemToolRoutines.IsMouseInControl())
                    EditModePicking(input);
            }

            // 5/5/2008 - Delete ScenaryItems if in EditMode               
            if (input.Delete)
            {
                EditModeDeleting();
            }
        }

        private static bool _dragMoveStarted;

        /// <summary>
        /// If (TerrainIsIn == EditMode), then we check all items in the <see cref="TerrainScreen"/> 
        /// for pickable <see cref="ScenaryItemScene"/>, because in EditMode, we want the ability to be 
        /// able to delete any <see cref="SceneItem"/> placed on the <see cref="Terrain"/>.  
        /// </summary>
        /// <param name="input"><see cref="InputState"/> instance</param>
        internal static void EditModePicking(InputState input)
        {
            // 6/2/2012 - check if Properties tool open and using either "Areas" or "Waypoints" tabs.
            var propertiesTools = PropertiesTools;
            if (propertiesTools != null)
            {
                // Check if Index tab 2, for "Areas"
                if (propertiesTools.IsTabIndexActive(2))
                    return;

                // 6/2/2012 - Check if Index tab 3, for "Waypoints"
                if (propertiesTools.IsTabIndexActive(3))
                    return;
            }

            // 11/21/2009 - Check for SceneItem DragMove.
            // NOTE: MUST PROCEED the follow 'IsPicked' check; otherwise will fail!
            Vector3 newPosition;
            if (_sceneItemDragMove != null && ImageNexus.BenScharbach.TWEngine.HandleGameInput.HandleInput.CheckForItemDragMove(ref _dragMoveStarted, out newPosition))
                _sceneItemDragMove.Position = newPosition;
            else
                _sceneItemDragMove = null;

            // 4/28/2009 - Check if 'IsPicked'
            if (!input.IsPicked || TerrainScreen.SceneCollection == null) return;

            // 10/6/2009 - Check ScenaryItems for picks
            CheckScenaryItemsForPicks();

            // 10/6/2009 - Check SelectableItems for picks
            CheckSelectableItemsForPicks();
        }

        // 1/15/2011 - Updated to now iterate the Player collection.
        // 2/2/2010 - Updated to use the new refactored method.
        // 10/6/2009

        /// <summary>
        /// Iterates the <see cref="Player._selectableItems"/> collection, while checking for picks; items picked, 
        /// are then updated to the ProperitesTool form. (EditMode ONLY).
        /// </summary>
        private static void CheckSelectableItemsForPicks()
        {
            //Debugger.Break();

            // 1/15/2011 - Iterate Player collection
            const int maxAllowablePlayers = TemporalWars3DEngine._maxAllowablePlayers; // 6/2/2012
            for (var i = 0; i < maxAllowablePlayers; i++)
            {
                // 6/15/2010 - Updated to use new GetPlayer method.
                Player player;
                TemporalWars3DEngine.GetPlayer(i, out player);
                if (player == null) continue;

                // 2/2/2010 - Updated to use the new refactored method.
                int itemIndex;
                if (!Player.GetClosestPickedSceneItem(player, out itemIndex)) continue;

                // 6/15/2010 - Updated to use new get method.
                SceneItemWithPick selectableItem;
                Player.GetSelectableItemByIndex(player, itemIndex, out selectableItem);
                if (selectableItem == null) continue;

                // 11/21/2009 - Do DragMove check.
                if (ImageNexus.BenScharbach.TWEngine.HandleGameInput.HandleInput.DoDragMoveCheck(ref _dragMoveStarted))
                {
                    // set item to drag move.
                    _sceneItemDragMove = selectableItem;
                    // TODO: (1/15/2011) - Updated to use the new InstanceModel FlashWhite material.
                    selectableItem.FlashItemWhite(5); // 11/21/2009 - Have item blink white for a few seconds.
                }

                // If Left-Control Mouse-click, then selection is for AbstractBehavior's 'TargetItem' selection.
                var propertiesTools = PropertiesTools; // cache
                if (propertiesTools == null) continue; // 11/3/2009

                if (!ImageNexus.BenScharbach.TWEngine.HandleGameInput.HandleInput.InputState.LeftMouseButton) continue;

                // 6/11/2012: Updated to a 'Right-Click' for setting the Behaviors in the Properties tool.
                if (ImageNexus.BenScharbach.TWEngine.HandleGameInput.HandleInput.InputState.IsKeyPress(Keys.RightControl))
                {
                    // Store Current Position of SceneItemOwner clicked on.
                    // Store Current SceneItemOwner Ref
                    propertiesTools.BehaviorsTargetItem = selectableItem;

                    // Call Delegate Function
                    if (propertiesTools.PopulateUsingTargetItemDelegate != null)
                        propertiesTools.PopulateUsingTargetItemDelegate();

                    continue;
                }

                // 6/11/2012 - Updated to only link item when the Left-Control key clicked.
                if (ImageNexus.BenScharbach.TWEngine.HandleGameInput.HandleInput.InputState.IsKeyPress(Keys.LeftControl))
                {
                    // Else, Regular Left-Mouse Click selection
                    // Call Updating the Links
                    propertiesTools.LinkSceneItemToTool(selectableItem);
                }

            } // End Loop Player Collection
        }
#endif

        // 10/6/2009

        /// <summary>
        /// Iterates the <see cref="TerrainScreen.SceneCollection"/>, while checking for picks; items picked, 
        /// are then updated to the ProperitesTool form.
        /// </summary>
        private static void CheckScenaryItemsForPicks()
        {
            // 11/25/2009
            var sceneItems = TerrainScreen.SceneCollection;
            if (sceneItems == null) return;

            // 1st - Deselect all
            var count = sceneItems.Count; // 11/21/2009
            for (var i = 0; i < count; i++)
            {
                // 7/2/2009 - Check if ScenaryItemScene, with multiple items, and call internal method.
                var scenaryItemScene = (sceneItems[i] as ScenaryItemScene);
                if (scenaryItemScene == null) continue;

                scenaryItemScene.DeselectAllPickedItemsInInternalList();
            }

            // Check 'Scene' Items
            for (var i = 0; i < count; i++)
            {
                // 7/2/2009 - Check if ScenaryItemScene, with multiple items, and call internal method.
                var scenaryItemScene = (sceneItems[i] as ScenaryItemScene);
                if (scenaryItemScene == null) continue;

                // 10/6/2009 - Updated to check for boolean value
                if (!scenaryItemScene.CheckForPickedItemsWithInternalList()) continue;

#if !XBOX360
                
                // Check if Properties Windows is Open and update attributes.
                switch (ToolInUse)
                {
                    case ToolType.PropertiesTool:
                        // 6/11/2012 - Updated to only link item when the Left-Control key clicked.
                        if (ImageNexus.BenScharbach.TWEngine.HandleGameInput.HandleInput.InputState.IsKeyPress(Keys.LeftControl))
                        {
                            PropertiesTools.LinkSceneItemToTool(scenaryItemScene);
                        }
                        break;
                    case ToolType.ItemTool:
                        TerrainWPFTools.TerrainItemToolRoutines.LinkSceneItemToTool(scenaryItemScene);
                        break;
                }

                // 11/21/2009 - Do DragMove check.
                if (ImageNexus.BenScharbach.TWEngine.HandleGameInput.HandleInput.DoDragMoveCheck(ref _dragMoveStarted))
                {
                    // set item to drag move.
                    _sceneItemDragMove = scenaryItemScene;
                    scenaryItemScene.FlashItemWhite(5); // 11/21/2009 - Have item blink white for a few seconds.
                }

                // 10/6/2009 - Break out of loop, since found pick!
                break;
#endif
            } // End For Loop Scene
        }

        private static SceneItem _sceneItemDragMove;

        // 5/3/2009: FXCop: Updated to not recast 'IIntancedItem' multiple times.
        // 5/5/2008
        // 9/24/2008: Update Class to from 'ScenaryItem' to 'InstancedItem'.

        /// <summary>
        /// If (TerrainIsIn == EditMode) and user press 'Delete' Key, then this will delete
        /// any <see cref="ScenaryItemScene"/> which are currently Picked.
        /// </summary>
        internal static void EditModeDeleting()
        {
            // 11/25/2009 - Cache
            var sceneItems = TerrainScreen.SceneCollection;
            if (sceneItems == null) return;

            // 1st - Iterate through Scene and remove
            var count = sceneItems.Count; // 11/21/09
            for (var i = 0; i < count; i++)
            {
                // 7/2/2009 - If 'ScenaryItemScene', then must call internal Remove method, since
                //            scenary items classes contain multiple transforms in one class.  Therefore, if
                //            the regular 'Delete is used, all transforms would be erroneuosly deleted!
                var scenaryItemScene = (sceneItems[i] as ScenaryItemScene);

                if (scenaryItemScene != null)
                {
                    scenaryItemScene.RemovePickedItemsFromInternalList();

                    // if internal list empty, then delete entire class instance
                    if (scenaryItemScene.ScenaryItems != null && scenaryItemScene.ScenaryItems.Count == 0)
                    {
                        // 7/2/2009
                        TerrainScreen.DeleteSceneItemFromList(i, scenaryItemScene.ShapeItem.ItemInstanceKey);
                    } // End if ScenaryItems List Empty
                }

                // Make sure we are only checking ScenaryItemShape Classes
                var instancedItemPick = (sceneItems[i].ShapeItem as IInstancedItem); // Extract InstancedItem for quick access

                if (instancedItemPick == null) continue;

                // If InstancedItem is Picked, then remove it from scene.
                if (scenaryItemScene != null && scenaryItemScene.ScenaryItems != null &&
                    (instancedItemPick.IsPickedInEditMode && scenaryItemScene.ScenaryItems.Count == 0))
                {
                    // 7/2/2009
                    TerrainScreen.DeleteSceneItemFromList(i, instancedItemPick.ItemInstanceKey);
                } // End If IsPicked
            } // End For Scene Loop
        }

#if !XBOX360

        // 12/7/2009
        /// <summary>
        /// Will activate the given <see cref="ToolType"/> Form.
        /// </summary>
        /// <param name="toolType"><see cref="ToolType"/> to activate.</param>
        public static void ActivateTool(ToolType toolType)
        {
            try
            {
#if WithLicense
                // 2/22/2011
                // Check for Valid License.
                var license = new LicenseHelper();
                if (!license.Check(false))
                    return;
#endif

                var game = TemporalWars3DEngine.GameInstance;

                // 1/13/2011 - Unlock Camera for free movement.
                Camera.LockAll = false;
                Camera.SetDefaultCameraBoundArea();

                // TODO: 6/18/2012: Add ScriptingActions call, once service.
                // Restore original blocking data set.
                //RestoreOriginalBlockingDataSet();

                switch (toolType)
                {
                    case ToolType.None:
                        return;
                    case ToolType.MainMenuTool:
                        TerrainWPFTools.MainMenuToolShow(); // 8/18/2010
                        return;
                    case ToolType.HeightTool:
                        // 7/8/2010
                        TerrainWPFTools.HeightToolShow();
                       
                        ShowHeightCursor = true; // 7/1/2010
                        ToolInUse = ToolType.HeightTool;
                        // Tell Terrain Class to use V2 Shader
                        TurnOnEditMode();

                        break;
                    case ToolType.PaintTool:

                        // 7/8/2010
                        TerrainWPFTools.PaintToolShow();

                        ShowPaintCursor = true; // 5/8/2011
                        ToolInUse = ToolType.PaintTool;
                        // Tell Terrain Class to use V2 Shader
                        TurnOnEditMode();

                        break;
                    case ToolType.ItemTool:
                        // 7/8/2010
                        TerrainWPFTools.ItemToolShow();
                        
                        ToolInUse = ToolType.ItemTool;

                        break;
                    case ToolType.PropertiesTool:

                        if (PropertiesTools == null)
                        {
                            PropertiesTools = new PropertiesTools(game) {Visible = true};
                            // 8/28/2008 - Add Event Handler for Form Closing Event.
                            PropertiesTools.FormClosing += TerrainTools_FormClosed;
                        }
                        else
                        {
                            PropertiesTools.Visible = true;
                        }

                        ToolInUse = ToolType.PropertiesTool;

                        break;
                    default:
                        throw new ArgumentOutOfRangeException("toolType");
                } // End Switch

                // 12/17/2009
                TerrainShape.TerrainIsIn = TerrainIsIn.EditMode; 

            }
            catch (Exception)
            {
                Debug.WriteLine("(ActivateTool) threw the 'GeneralExp' error.");
            }
        }

        // 7/1/2010 - Dirty flag to track changes
        private static bool _isDirty;

       

        // 7/1/2010
        /// <summary>
        /// Called from 'EndDraw' in main class, to check if VertexBuffer data needs to be
        /// updated for current frame.  This is done to eliminate the errors caused by trying to 
        /// call the VertexBuffer's 'SetData' operation, which throws the InvalidOp exception due
        /// to the resource being locked during a draw call.
        /// </summary>
        public static void DoUpdateCheckForVertexBuffers()
        {
            if (!_isDirty) return;

            if (TerrainData.VertexBufferDataStream1 == null) return;

            try
            {
                // update current VB to use.
                CurrentVertexBuffer = !CurrentVertexBuffer;

                // else, update with new VB data.
                if (!CurrentVertexBuffer)
                {
                    TerrainData.TerrainVertexBufferStream1.SetData(TerrainData.VertexBufferDataStream1);
                }
                else 
                {
                    TerrainData.TerrainVertexBufferStream1A.SetData(TerrainData.VertexBufferDataStream1);
                }

                _isDirty = false;
            }
            catch (InvalidOperationException)
            {

                Debug.WriteLine("(DoUpdateCheckForVertexBuffers) threw the 'InvalidOpExp' error.");
            }
        }
        

        // 4/10/2009: Updated to be STATIC.
        /// <summary>
        /// Raises <see cref="Terrain"/> Quad up, by increasing the Y height values.
        /// </summary>
        /// <remarks>Update the <see cref="GroundCursorStrength"/> to affect the increases</remarks>
        public static void QuadRaiseHeight()
        {                       

            QuadMoveVertices(GroundCursorStrength * .1f);
        }

        // 4/10/2009: Updated to be STATIC.
        /// <summary>
        /// Lowers <see cref="Terrain"/> Quad down, by decreasing the Y height values.
        /// </summary>
        /// <remarks>Update the <see cref="GroundCursorStrength"/> to affect the increases</remarks>
        public static void QuadLowerHeight()
        {                          

            QuadMoveVertices(-GroundCursorStrength * .1f);
        }

        // 8/18/2009: Updated by removing the need to pass the 'PickedTriangle', since can
        //            get access to it since STATIC property!
        // 4/10/2009: Updated to be STATIC.
        // 9/12/2008: Optimize for memory.        
        private static void QuadMoveVertices(float height)
        {
            try
            {
                var vertexBufferDataStream1 = TerrainData.VertexBufferDataStream1; // 5/18/2010 - Cache
                var heightData = TerrainData.HeightData; // 5/18/2010 - Cache

                var inZero = Vector2.Zero;
                var mapHeight = TerrainData.MapHeight; // 8/18/2009
                var mapWidth = TerrainData.MapWidth; // 8/18/2009
                const int mapScale = TerrainData.cScale;
                var vertexDataLookup = TerrainData.VertexDataLookup; // 12/17/2009

                var pickedTriangle = TerrainPickingRoutines.PickedTriangle;
                var x = (int)pickedTriangle.Triangle[0].Position.X;
                var y = (int)pickedTriangle.Triangle[0].Position.Z;
               
                var areaAffected = _groundCursorSize / 10;

                // Increase other points around height determine by _groundCursorSize
                for (var loopV = -areaAffected; loopV < areaAffected; loopV++)
                    for (var loopW = -areaAffected; loopW < areaAffected; loopW++)
                    {
                        var inVecVw = new Vector2
                                          {
                                              X = loopV,
                                              Y = loopW
                                          };

                        float length;
                        Vector2.Distance(ref inZero, ref inVecVw, out length);

                        if (length >= areaAffected) continue;

                        var amount = (int) (height*(areaAffected - length)/2f);

                        var key = (x + loopV*mapScale) + ((y + loopW*mapScale)*mapWidth);

                        int arrayInt;
                        if (!vertexDataLookup.TryGetValue(key, out arrayInt)) continue;

                        var positionUnPacked = vertexBufferDataStream1[arrayInt].Position; // 1/11/2010
                        if (UseConstantFeet)
                        {
                            if (positionUnPacked.Y < ConstantFeetValue)
                            {
                                positionUnPacked.Y += (ConstantFeetToAdd*(areaAffected - length)/2f);
                                heightData[(x/mapScale + loopV) + (y/mapScale + loopW)*mapHeight] += amount;
                                vertexBufferDataStream1[arrayInt].Position = positionUnPacked; // 1/11/2010
                            }
                        }
                        else
                        {
                            positionUnPacked.Y += amount;
                            heightData[(x/mapScale + loopV) + (y/mapScale + loopW)*mapHeight] += amount;
                            vertexBufferDataStream1[arrayInt].Position = positionUnPacked; // 1/11/2010
                        }
                    } // End For Loop
                
                // 7/1/2010 - Updated to use double-buffer VB, which is to eliminate the InvalidOp exception.
                _isDirty = true;
                //TerrainData.TerrainVertexBufferStream1.SetData(vertexBufferDataStream1);
                
            }
            catch (InvalidOperationException)
            {
                
                Debug.WriteLine("(QuadMoveVertices) threw the 'InvalidOpExp' error.");
            }


        }

        // 8/18/2009: Updated by removing the need to pass the 'PickedTriangle', since can
        //            get access to it since STATIC property!
        // 4/10/2009: Updated to be STATIC.
        // 9/12/2008 - Optimize for memory.
        ///<summary>
        /// Flattens the vertices within a <see cref="Terrain"/> quad.
        ///</summary>
        public static void QuadFlattenVertices()
        {
            try
            {

                // 5/18/2010 - Cache
                var vertexBufferDataStream1 = TerrainData.VertexBufferDataStream1;
                var heightData = TerrainData.HeightData;

                var inZero = Vector2.Zero; 
                var mapWidth = TerrainData.MapWidth; // 8/18/2009
                var mapHeight = TerrainData.MapHeight; // 8/18/2009
                const int mapScale = TerrainData.cScale;
                var vertexDataLookup = TerrainData.VertexDataLookup; // 12/17/2009

                var pickedTriangle = TerrainPickingRoutines.PickedTriangle;
                var x = (int)pickedTriangle.Triangle[0].Position.X;
                var y = (int)pickedTriangle.Triangle[0].Position.Z;
                var areaAffected = _groundCursorSize / 10;


                for (var loopV = -areaAffected; loopV < areaAffected; loopV++)
                    for (var loopW = -areaAffected; loopW < areaAffected; loopW++)
                    {
                        var inVecVw = new Vector2
                                          {
                                              X = loopV,
                                              Y = loopW
                                          };

                        float length;
                        Vector2.Distance(ref inZero, ref inVecVw, out length);
                        if (length >= areaAffected) continue;

                        var key = (x + loopV*mapScale) + ((y + loopW*mapScale)*mapWidth);
                        int arrayInt;
                        if (!vertexDataLookup.TryGetValue(key, out arrayInt)) continue;

                        vertexBufferDataStream1[arrayInt].Position.Y = FlattenHeight;

                        heightData[(x/mapScale + loopV) + (y/mapScale + loopW)*mapHeight] = FlattenHeight;

                    } // End For Loop

                // 7/1/2010 - Updated to use double-buffer VB, which is to eliminate the InvalidOp exception.
                _isDirty = true;
                //TerrainData.TerrainVertexBufferStream1.SetData(vertexBufferDataStream1);
            }
            catch (InvalidOperationException)
            {
                Debug.WriteLine("(QuadFlattenVertices) threw the 'InvalidOpExp' error.");
            }

        }

        // 8/18/2009: Updated by removing the need to pass the 'PickedTriangle', since can
        //            get access to it since STATIC property!
        // 4/10/2009: Updated to be STATIC.
        // 9/12/2008 - Optimize for memory        
        ///<summary>
        /// Smooths the vertices within a <see cref="Terrain"/> quad, by averaging their heights among 
        /// the surrounding neighbors.
        ///</summary>
        public static void QuadSmooth()
        {

            try
            {
                // 5/18/2010 - Cache
                var vertexBufferDataStream1 = TerrainData.VertexBufferDataStream1;
                var heightData = TerrainData.HeightData;

                var inZero = Vector2.Zero; 
                var pixHeight = new float?[4];
                var mapWidth = TerrainData.MapWidth; // 8/18/2009
                var mapHeight = TerrainData.MapHeight; // 8/18/2009
                const int mapScale = TerrainData.cScale;
                var vertexDataLookup = TerrainData.VertexDataLookup; // 12/17/2009

                var pickedTriangle = TerrainPickingRoutines.PickedTriangle;
                var x = (int)pickedTriangle.Triangle[0].Position.X;
                var y = (int)pickedTriangle.Triangle[0].Position.Z;
                var areaAffected = _groundCursorSize / 10;

                for (var loopV = -areaAffected; loopV < areaAffected; loopV++)
                    for (var loopW = -areaAffected; loopW < areaAffected; loopW++)
                    {
                        var inVecVw = new Vector2
                                          {
                                              X = loopV,
                                              Y = loopW
                                          };

                        float length;
                        Vector2.Distance(ref inZero, ref inVecVw, out length);
                        if (length >= areaAffected) continue;

                        var iCalc = (y + loopW*mapScale); // 12/17/2009
                        var key1 = (x + ((loopV + 1)*mapScale)) + (iCalc*mapWidth);
                        var key2 = (x + ((loopV - 1)*mapScale)) + (iCalc*mapWidth);
                        var jCalc = (x + loopV*mapScale); // 12/17/2009
                        var key3 = jCalc + ((y + ((loopW + 1)*mapScale))*mapWidth);
                        var key4 = jCalc + ((y + ((loopW - 1)*mapScale))*mapWidth);

                        int arrayInt;
                        if (vertexDataLookup.TryGetValue(key1, out arrayInt))
                            pixHeight[0] = vertexBufferDataStream1[arrayInt].Position.Y;
                        if (vertexDataLookup.TryGetValue(key2, out arrayInt))
                            pixHeight[1] = vertexBufferDataStream1[arrayInt].Position.Y;
                        if (vertexDataLookup.TryGetValue(key3, out arrayInt))
                            pixHeight[2] = vertexBufferDataStream1[arrayInt].Position.Y;
                        if (vertexDataLookup.TryGetValue(key4, out arrayInt))
                            pixHeight[3] = vertexBufferDataStream1[arrayInt].Position.Y;


                        var averageHeight = 0f;
                        var numpix = 0;
                        for (var i = 0; i < 4; i++)
                        {
                            var thisPixHeight = pixHeight[i];
                            if (thisPixHeight == null) continue;
                            
                            averageHeight += thisPixHeight.Value;
                            numpix++;
                        }

                        averageHeight = averageHeight/numpix;

                        var key = jCalc + (iCalc*mapWidth);
                        if (!vertexDataLookup.TryGetValue(key, out arrayInt)) continue;

                        vertexBufferDataStream1[arrayInt].Position.Y = averageHeight;

                        heightData[(x/mapScale + loopV) + (y/mapScale + loopW)*mapHeight] = averageHeight;

                    } // End For Loop

                // 7/1/2010 - Updated to use double-buffer VB, which is to eliminate the InvalidOp exception.
                _isDirty = true;
                //TerrainData.TerrainVertexBufferStream1.SetData(vertexBufferDataStream1);
            }
            catch (InvalidOperationException)
            {
                Debug.WriteLine("(QuadSmooth) threw the 'InvalidOpExp' error.");
            }

        }

#endif

        // 4/10/2009: Updated to be STATIC.
        /// <summary>
        /// Smoothes out the <see cref="Terrain"/> using averages of the height.       
        /// </summary>
        /// <param name="passes">Number of smoothing passes to make</param>
        public static void SmoothTerrain(int passes)
        {
            // 8/18/2009 - Cache
            var mapWidth = TerrainData.MapWidth;
            var mapHeight = TerrainData.MapHeight;
            var heightData = TerrainData.HeightData;
            var newHeightData = new float[mapWidth * mapHeight];

            while (passes > 0)
            {
                passes--;

                for (var loopX = 0; loopX < mapWidth; loopX++)
                {
                    for (var loopY = 0; loopY < mapHeight; loopY++)
                    {
                        var adjacentSections = 0;
                        var sectionsTotal = 0.0f;

                        // 11/19/2009 - cache
                        var loopXMinus1 = (loopX - 1); 
                        var loopYMinus1 = (loopY - 1);
                        var loopYAdd1 = (loopY + 1);
                        var loopXAdd1 = (loopX + 1);
                        // 12/17/2009 - cache
                        var loopYMultMapHeight = loopY * mapHeight;
                        var loopYMinus1MultMapHeight = loopYMinus1 * mapHeight;
                        var loopYAdd1MultMapHeight = loopYAdd1 * mapHeight;

                        if (loopXMinus1 > 0)            // Check to left
                        {
                            sectionsTotal += heightData[loopXMinus1 + loopYMultMapHeight];
                            adjacentSections++;

                            if (loopYMinus1 > 0)        // Check up and to the left
                            {
                                sectionsTotal += heightData[loopXMinus1 + loopYMinus1MultMapHeight];
                                adjacentSections++;
                            }

                            if (loopYAdd1 < mapHeight)        // Check down and to the left
                            {
                                sectionsTotal += heightData[loopXMinus1 + loopYAdd1MultMapHeight];
                                adjacentSections++;
                            }
                        }

                        
                        if (loopXAdd1 < mapWidth)     // Check to right
                        {
                            sectionsTotal += heightData[loopXAdd1 + loopYMultMapHeight];
                            adjacentSections++;

                            if (loopYMinus1 > 0)        // Check up and to the right
                            {
                                sectionsTotal += heightData[loopXAdd1 + loopYMinus1MultMapHeight];
                                adjacentSections++;
                            }

                            if (loopYAdd1 < mapHeight)        // Check down and to the right
                            {
                                sectionsTotal += heightData[loopXAdd1 + loopYAdd1MultMapHeight];
                                adjacentSections++;
                            }
                        }

                        if (loopYMinus1 > 0)            // Check above
                        {
                            sectionsTotal += heightData[loopX + loopYMinus1MultMapHeight];
                            adjacentSections++;
                        }

                        if (loopYAdd1 < mapHeight)    // Check below
                        {
                            sectionsTotal += heightData[loopX + loopYAdd1MultMapHeight];
                            adjacentSections++;
                        }

                        var index0 = loopX + loopYMultMapHeight; // 11/19/2009

                        newHeightData[index0] = (heightData[index0] + (sectionsTotal / adjacentSections)) * 0.5f;
                    }
                }

                // Overwrite the HeightData info with our new smoothed info
                for (var loopX = 0; loopX < mapWidth; loopX++)
                {
                    for (var loopY = 0; loopY < mapHeight; loopY++)
                    {
                        var index = loopX + loopY * mapHeight;
                        heightData[index] = newHeightData[index];
                    }
                }
            }
            
        }

        // 4/10/2009: Updated to be STATIC.
        // 4/8/2008 - Tessellate current Quad patch one LOD deeper
        // 4/25/2008: Updated Method to use the Dictionary 'quadList'.
        private static void TessellateCurrentQuad(int quadInstance, TessellateLevel changeToLevel)
        {           

            // Find Quad Instance using quadList Dictionary
            TerrainQuadTree quad;
            if (TerrainData.QuadList.TryGetValue(quadInstance, out quad))
            {
                TerrainQuadTessellater.TessellateCurrentQuad(quad, quadInstance, changeToLevel);

            }

        }

        // 4/10/2009: Updated to be STATIC.
        // 4/24/2008 - Given Current PickedTriangle, we extract the current quadInstanceKey
        //             and check if we can Tessellate Deeper.
        //             This is primarily called from the Raise and Lower Terrain Methods.
        ///<summary>
        /// Given current PickedTriangle, extracts the current <see cref="TerrainPickingRoutines.PickedTriangle"/> quad key
        /// and checks if <see cref="TessellateLevel"/> Enum can go deeper.
        ///</summary>
        public static void TessellateToLowerLOD()
        {
            TessellateToLowerLODLocked = true;

            var quadKey = TerrainPickingRoutines.PickedTriangle.QuadInstanceKey;
            switch (TerrainData.GetQuadLOD(quadKey))
            {
                case TessellateLevel.Level1:
                    TessellateCurrentQuad(quadKey, TessellateLevel.Level2);
                    break;
                case TessellateLevel.Level2:
                    TessellateCurrentQuad(quadKey, TessellateLevel.Level3);
                    break;
            }

        }

        // 4/11/2008 - 
        // 4/25/2008: Updated Method to use the Dictionary 'quadList'.
        ///<summary>
        /// Adds triangles to <see cref="TerrainQuadPatch"/> to eliminate terrain cracks.
        ///</summary>
        ///<param name="quadInstance">quad instance</param>
        ///<param name="quadAdjacent"><see cref="QuadAdjacent"/> Enum</param>
        ///<param name="section"><see cref="QuadSection"/> Enum</param>
        ///<param name="detailLevel"><see cref="LOD"/> Enum</param>
        public static void CrackFixCurrentQuad(int quadInstance, QuadAdjacent quadAdjacent, QuadSection section, LOD detailLevel)
        {            

            // Find Quad Instance using quadList Dictionary
            TerrainQuadTree quad;
            if (TerrainData.QuadList.TryGetValue(quadInstance, out quad))
            {
                TerrainQuadTessellater.CrackFixCurrentQuad(quad, quadInstance, quadAdjacent, section, detailLevel);

            }

        }

        // 4/21/2008 - 
        // 11/20/2008 - Updated to set 'TerrainShape' Enum DrawMode to 'EditMode'; also removed
        //              old code which had to reset all shader parameters for the textures.
        ///<summary>
        /// Turns on edit mode, by setting the <see cref="TerrainShape.DrawMode"/> to <see cref="Terrain.Enums.DrawMode.EditMode"/>.
        ///</summary>
        public static void TurnOnEditMode()
        {
            EditMode = true;

            // 11/20/2008 - Set to EditMode for Draw enum
            TerrainShape.DrawMode = DrawMode.EditMode;

            GroundCursorTex = TemporalWars3DEngine.ContentGroundTextures.Load<Texture2D>(@"Terrain\groundCursor");  
            PaintCursorTex = TemporalWars3DEngine.ContentGroundTextures.Load<Texture2D>(@"Terrain\paintCursor"); 

            GroundCursorPositionParam = _terrainShape.Effect.Parameters["xGroundCursorPosition"];
            GroundCursorTextureParam = _terrainShape.Effect.Parameters["xGroundCursorTex"];
            PaintCursorTextureParam = _terrainShape.Effect.Parameters["xPaintCursorTex"];
            GroundCursorSizeParam = _terrainShape.Effect.Parameters["xGroundCursorSize"];
            PaintCursorSizeParam = _terrainShape.Effect.Parameters["xPaintCursorSize"];
            ShowHeightCursorParam = _terrainShape.Effect.Parameters["xShowHeightCursor"];
            ShowPaintCursorParam = _terrainShape.Effect.Parameters["xShowPaintCursor"]; 
        
        }

        // 10/7/2009
        /// <summary>
        /// When loading a map, this should be called to Tessellate the Quads to the previous
        /// levels; this should be called from the 'TerrainStorageRoutines'.
        /// </summary>
        /// <param name="tmpParentsTessellated">Collection of parent quads</param>
        /// <param name="tmpQuadLOD3">Collection of LOD-3 quads</param>
        public static void TessellateQuads(List<int> tmpParentsTessellated, List<int> tmpQuadLOD3)
        {
            var count = tmpParentsTessellated.Count; // 5/18/2010 - Cache
            for (var loop1 = 0; loop1 < count; loop1++)
            {
                TessellateToLowerLOD();
            }
            TessellateToLowerLODLocked = false;

            // Tessellate Child Quad's to LOD3 where necessary
            var i = tmpQuadLOD3.Count; // 5/18/2010 - Cache
            for (var loop1 = 0; loop1 < i; loop1++)
            {
                TessellateToLowerLOD();
            }
        }
       
#if !XBOX360
        // 8/28/3008 - Forms Tools Closed EventHandler, which turns off the EditMode.
        // 11/20/2008 - Updated to set the TerrainShape Enum 'DrawMode' to Solid.
        internal static void TerrainTools_FormClosed(object sender, FormClosingEventArgs e)
        {
            try
            {
                EditMode = false;
                TerrainShape.DrawMode = DrawMode.Solid;
                TerrainShape.TerrainIsIn = TerrainIsIn.PlayableMode; // 3/3/2009
                ToolInUse = ToolType.None;

                // 9/15/2008
                TerrainAlphaMaps.SetAlphaMapsTextureEffect();

                // 5/15/2009 - Make sure Layer InUse are all off.
                TerrainAlphaMaps.InUseLayer1 = false;
                TerrainAlphaMaps.InUseLayer2 = false;           

              
                ShowPaintCursor = false;
                ShowHeightCursor = false;  

                // Update Effect Textures
                TerrainShape.UpdateEffectDiffuseTextures();

                // 7/30/2008 - Update Bump map Textures
                TerrainShape.UpdateEffectBumpMapTextures();

                // 1/2/2010
                // Update MiniMap Landscape.      
                var miniMap = (IMinimap) _gameInstance.Services.GetService(typeof (IMinimap));
                if (miniMap != null) miniMap.RenderLandscapeForMiniMap();
            }
            catch
            {
                Debug.WriteLine("Method Error: TerrainTools_FormClosed Event handler.");
            }

        }

#endif

        // 4/5/2009
        /// <summary>
        /// Releases the unmanaged resources used by the GameComponent and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
#if XBOX360
#else
                if (PropertiesTools != null)
                    PropertiesTools.Dispose();

                // 7/2/2010
                if (TerrainWpfTools != null)
                    TerrainWpfTools.Dispose();
#endif

                if (GroundCursorTex != null)
                    GroundCursorTex.Dispose();
                if (PaintCursorTex != null)
                    PaintCursorTex.Dispose();

                GroundCursorPositionParam = null;
                GroundCursorTextureParam = null;
                PaintCursorTextureParam = null;
                GroundCursorSizeParam = null;
                PaintCursorSizeParam = null;
                ShowHeightCursorParam = null;
                ShowPaintCursorParam = null;
                _terrainShape = null;

                
            }

            base.Dispose(disposing);
        }
    }
}
