#region File Description
//-----------------------------------------------------------------------------
// HandleInput.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AStarInterfaces.AStarAlgorithm.Enums;
using PerfTimersComponent.Timers;
using ScreenTextDisplayer.ScreenText;
using TWEngine.Common;
using TWEngine.Common.Enums;
using TWEngine.Common.Extensions;
using TWEngine.GameCamera.Enums;
using TWEngine.IFDTiles.Enums;
using TWEngine.InstancedModels.Enums;
using TWEngine.Interfaces;
using TWEngine.Players;
using TWEngine.rtsCommands.Enums;
using TWEngine.SceneItems.Enums;
using TWEngine.ScreenManagerC;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TWEngine.GameCamera;
using TWEngine.Shadows.Enums;
using TWEngine.Terrain;
using TWEngine.SceneItems;
using TWEngine.Shadows;
using TWEngine.IFDTiles;
using TWEngine.rtsCommands;
using TWEngine.MemoryPool;
using TWEngine.Networking;
using TWEngine.Terrain.Enums;
using TWEngine.TerrainTools;

#if !XBOX360
using TWEngine.Console.Enums;
#else
using Microsoft.Xna.Framework.Graphics;
#endif

namespace TWEngine.HandleGameInput
{
    // 4/30/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.HandleGameInput"/> namespace contains a static class 
    /// <see cref="HandleInput"/>.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    // 4/28/2009
    /// <summary>
    /// The <see cref="HandleGameInput"/>, handles input for the <see cref="IFDTile"/> selection,
    /// <see cref="IMinimap"/> selection, <see cref="Cursor"/> selection, <see cref="Camera"/> selection,
    /// <see cref="ShadowMap"/> debug selection, <see cref="Player"/> selection, and special control-group
    /// selection.
    /// </summary>
    static class HandleInput
    {
        /// <summary>
        /// Debug Purposes, used in TerrainShapeInputCheck
        /// </summary>
        private static int _useItem; 
 
        // 1/2/2010 - 
        /// <summary>
        /// Reference for <see cref="IMinimap"/>
        /// </summary>
        private static readonly IMinimap MiniMap;
     
        // constructor
        /// <summary>
        /// Constructor, which retrieves the <see cref="Cursor"/> and <see cref="IMinimap"/> services.
        /// </summary>
        static HandleInput()
        {
            // Set Minimap Ref
            MiniMap = (IMinimap)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(IMinimap));
        }

        #region Properties

        /// <summary>
        /// Returns reference to <see cref="InputState"/>.
        /// </summary>
        public static InputState InputState { get; private set; }
      

        #endregion
        

        // Handles all input for the game
        /// <summary>
        /// Updates game input, by calling the following internal methods;
        /// <see cref="HandleCameraInput"/>, <see cref="TerrainShapeInputCheck"/>, <see cref="MiniMapInputCheck"/>,
        /// <see cref="ShadowMapInputCheck"/> and <see cref="CursorInputCheck"/>.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="inputState"><see cref="InputState"/> instance</param>
        public static void UpdateInput(GameTime gameTime, InputState inputState)
        {
            InputState = inputState;

            // Check for Camera Input
            HandleCameraInput(gameTime);

            // Check for TerrainShape Input
            TerrainShapeInputCheck();

            // Check for MiniMap Input
            MiniMapInputCheck();

            // Check for ShadowMap Input
            ShadowMapInputCheck();

            // Note: 6/15/2012: Moved to the Cursor class.
            // Check for Cursor Input
            //CursorInputCheck(gameTime);

#if !XBOX360
            // 2/5/2011 - Check EditRoutines Input
            TerrainEditRoutines.HandleInput(inputState);
#endif

        }

        // 8/13/2008  
        /// <summary>
        /// Process input for the <see cref="Camera"/>.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private static void HandleCameraInput(GameTime gameTime)
        {

            // 11/27/2009 - When IFDTile displaying, then skip updating Camera position.  This should fix the
            //              error of the user selecting an item on XBOX, and then exiting menu and experiencing
            //              the jump off screen position; since the cursor position below, was continously being
            //              updated as the user held the stick to choose a menu item!
            if (IFDTileManager.IFDTileSetIsDisplaying)
                return;

            // 6/15/2012
            // 5/1/2009 - If No Camera movement, then reset camera acceleration
            if (!InputState.MoveCameraLeft && !InputState.MoveCameraRight && !InputState.MoveCameraForward && !InputState.MoveCameraBackward)
            {
                Camera.ResetAcceleration();
                Camera.CameraDirection = CameraDirectionEnum.None;
            }

            // 6/15/2012
            // Move Left and Right
            if (InputState.MoveCameraLeft)
                Camera.CameraDirection = CameraDirectionEnum.ScrollLeft; 

            // 6/15/2012
            if (InputState.MoveCameraRight)
                Camera.CameraDirection = CameraDirectionEnum.ScrollRight; 

            // 6/15/2012
            // Move Forward and Back
            if (InputState.MoveCameraForward)
                Camera.CameraDirection = CameraDirectionEnum.ScrollForward; 

            // 6/15/2012
            if (InputState.MoveCameraBackward)
                Camera.CameraDirection = CameraDirectionEnum.ScrollBackward; 

            // 6/28/2012
            // Move Diagonlly up/right
            if (InputState.MoveCameraForward && InputState.MoveCameraRight)
                Camera.CameraDirection = CameraDirectionEnum.ScrollForward | CameraDirectionEnum.ScrollRight;

            // 6/28/2012
            // Move Diagonlly down/right
            if (InputState.MoveCameraBackward && InputState.MoveCameraRight)
                Camera.CameraDirection = CameraDirectionEnum.ScrollBackward | CameraDirectionEnum.ScrollRight; 

            // 6/28/2012
            // Move Diagonlly down/left
            if (InputState.MoveCameraBackward && InputState.MoveCameraLeft)
                Camera.CameraDirection = CameraDirectionEnum.ScrollBackward | CameraDirectionEnum.ScrollLeft; 

            // 6/28/2012
            // Move Diagonlly up/left
            if (InputState.MoveCameraForward && InputState.MoveCameraLeft)
                Camera.CameraDirection = CameraDirectionEnum.ScrollForward | CameraDirectionEnum.ScrollLeft; 

            // 4/18/2009
            // Rotate Camera Right or Left
            if (InputState.RotateCameraLeft)
                Camera.CameraDirection = CameraDirectionEnum.RotateLeft; // 6/15/2012
            else // 10/9/2009 (Scripting Purposes)
                Camera.RotateLeftCounter = 0; // reset value

            if (InputState.RotateCameraRight)
                Camera.CameraDirection = CameraDirectionEnum.RotateRight; // 6/15/2012
            else // 10/9/2009 (Scripting Purposes)
                Camera.RotateRightCounter = 0; // reset value

            // Zoom Up and Down - Keyboard or MouseWheel
            if (InputState.MoveCameraHigher)
                Camera.RaiseCameraHeight(gameTime);
            else // 10/9/2009 (Scripting Purposes)
                Camera.ZoomOutCounter = 0; // reset value

            if (InputState.MoveCameraLower)
                Camera.LowerCameraHeight(gameTime);
            else // 10/9/2009 (Scripting Purposes)
                Camera.ZoomInCounter = 0; // reset value

            // 1/7/2009 - If middle mouse button pressed, reset Camera's Position to initial
            if (InputState.ResetCameraHeight)
                Camera.ResetCameraPosition();

        }

        /// <summary>
        /// Checks for Gamepad/Mouse Input for PC. This is called from the <see cref="IFDTile"/> render method.
        /// </summary>     
        /// <param name="ifdTile"><see cref="IFDTile"/> instance</param>   
        public static void IFDInputCheckForPc(IFDTile ifdTile)
        {
            if (InputState == null)
                return;

            var position = Cursor.Position; // 8/14/20009
            var tmpCursor = new Point { X = (int)position.X, Y = (int)position.Y };

            // 11/9/2009 - Refactored method.
            DoIFDTileSelectionCheck(ifdTile, ref tmpCursor);
        }

        /// <summary>
        /// Checks For Gamepad Input for XBOX. This is called from the <see cref="IFDTile"/> render method.
        /// </summary>       
        /// <param name="ifdTile"><see cref="IFDTile"/> instance</param>    
        public static void IFDInputCheckForXbox(IFDTile ifdTile)
        {
            if (InputState == null)
                return;

            // 4/30/2009: Only check if there is ifdTile set displaying!
            if (!IFDTileManager.IFDTileSetIsDisplaying)
                return;

            // 8/13/2009 - Cache
            const int constantShiftValue = (75 + 37);
            var middleScreenX = IFDTileManager.MiddleScreenX;
            var middleScreenY = IFDTileManager.MiddleScreenY;
            
            var tmpCursor = Point.Zero;            

            // For XBOX, let's set the Cursor to the specific tile positions, depending on the direction
            // the user is pushing the GamePad's RightStick.
            
            if (InputState.IFDTileSelectedPos1)
            {
                tmpCursor.X = middleScreenX - constantShiftValue;
                tmpCursor.Y = middleScreenY - constantShiftValue;
            }
            else if (InputState.IFDTileSelectedPos2)
            {
                tmpCursor.X = middleScreenX - constantShiftValue;
                tmpCursor.Y = middleScreenY + constantShiftValue;
            }
            else if (InputState.IFDTileSelectedPos3)
            {
                tmpCursor.X = middleScreenX + constantShiftValue;
                tmpCursor.Y = middleScreenY - constantShiftValue;
            }
            else if (InputState.IFDTileSelectedPos4)
            {
                tmpCursor.X = middleScreenX + constantShiftValue;
                tmpCursor.Y = middleScreenY + constantShiftValue;
            }
            else if (InputState.IFDTileSelectedPos5)
            {
                tmpCursor.X = middleScreenX - constantShiftValue;
                tmpCursor.Y = middleScreenY;
            }
            else if (InputState.IFDTileSelectedPos6)
            {
                tmpCursor.X = middleScreenX + constantShiftValue;
                tmpCursor.Y = middleScreenY;
            }
            else if (InputState.IFDTileSelectedPos7)
            {
                tmpCursor.X = middleScreenX;
                tmpCursor.Y = middleScreenY - constantShiftValue;
            }
            else if (InputState.IFDTileSelectedPos8)
            {
                tmpCursor.X = middleScreenX;
                tmpCursor.Y = middleScreenY + constantShiftValue;
            }

            // 11/9/2009 - Refactored method.
            DoIFDTileSelectionCheck(ifdTile, ref tmpCursor);

        }

        // 11/9/2009
        /// <summary>
        /// Checks if the given <see cref="Cursor"/> position is within an <see cref="IFDTile"/>, and
        /// if some selection or cancellation was done.
        /// </summary>
        /// <param name="ifdTile"><see cref="IFDTile"/> to check</param>
        /// <param name="tmpCursor"><see cref="Cursor"/> position</param>
        private static void DoIFDTileSelectionCheck(IFDTile ifdTile, ref Point tmpCursor)
        {
            ifdTile.TileRectCheck.Contains(ref tmpCursor, out ifdTile.CursorInsideIFDTile);
            if (ifdTile.CursorInsideIFDTile)
            {
                // 4/2/2009 - Skip if TileState is 'Disabled'
                if (ifdTile.TileState == TileState.Disabled)
                    return;

                // Tile is Hovered
                ifdTile.TileHovered(true);

                // 11/9/2009 - cast to IFDTile_Placement type.
                var tilePlacement = (ifdTile as IFDTilePlacement);
                if (tilePlacement != null)
                {
                    // If TileSelected (Released) AND Buildings/Defense itemtype.            
                    if (InputState.IFDTileSelectedWhenReleased &&
                        (tilePlacement.BuildingType == ItemGroupType.Buildings || tilePlacement.BuildingType == ItemGroupType.Shields))
                    {
                        // Call 'TileClicked' method.
                        ifdTile.TileSelected();

                    } // End If TileSelected
                    // If TileSelected (Pressed) AND NOT Buildings/Defense itemtype.    
                    else if (InputState.IFDTileSelectedWhenPressed &&
                             (tilePlacement.BuildingType != ItemGroupType.Buildings && tilePlacement.BuildingType != ItemGroupType.Shields))
                    {
                        // Call 'TileClicked' method.
                        ifdTile.TileSelected();

                    } // End If TileSelected
                    // If Right-Mouse Clicked, then call TileRightClicked.                         
                    else if (InputState.IFDTileCanceled)
                    {
                        // Call 'TileRightClicked' method
                        ifdTile.TileCanceled();

                    } // End If Right Click 
                    return;
                }

               // Check for other tiles, like the GroupControls
               if (InputState.IFDTileSelectedWhenPressed)
               {
                   // Call 'TileClicked' method.
                   ifdTile.TileSelected();
               }

               return;
            }

#if XBOX360
            // 4/30/2009 - Cancel Display, if B button pressed on gamepad.
            if (InputState.IsNewButtonPress(Buttons.B))
            {
                // 11/9/2009 - Updated to use this 'Deactive' method.
                IFDTileManager.DeActivateCurrentDisplayGroup();
            }
#endif

            ifdTile.TileHovered(false);
        }

        /// <summary>
        /// Check input for <see cref="BuildingScene"/> items.
        /// </summary>
        /// <param name="buildingScene"><see cref="BuildingScene"/> instance</param>
        public static void BuildingSceneInputCheck(BuildingScene buildingScene)
        {
            // Update ItemToPosition Marker, if PlaceBuildingMarkerFlag action
            if (!buildingScene.PickSelected || !InputState.PlaceBuildingMarker) return;

            // Get PickedRay Position in PathNodes Cordinates                   
            Vector3 newMarkerPosition;
            TerrainPickingRoutines.GetCursorPosByPickedRay(PickRayScale.NoChange, out newMarkerPosition);

            // Set new ItemToPosition Marker.
            var tmpPos = buildingScene.Position;
            buildingScene.ShapeItem.SetMarkerPosition(ref tmpPos, ref newMarkerPosition, buildingScene.NetworkItemNumber);
        }

        /// <summary>
        /// Check input for <see cref="DefenseScene"/> items.
        /// </summary>
        /// <param name="defenseScene"><see cref="DefenseScene"/> instance</param>
        public static void DefenseSceneInputCheck(DefenseScene defenseScene)
        {
            return;
        }

        
        /// <summary>
        /// Check input for <see cref="ShadowMap"/>.
        /// </summary>
        private static void ShadowMapInputCheck()
        {
            // 6/5/2009 - Skip if 'DebugValues' false.
            if (!ShadowMap._DebugValues)
                return;

#if !XBOX360
            // 10/31/2008 - If GameConsole Null, try to get from Game.Services.
            if (TerrainShape.GameConsole == null)
                TerrainShape.GameConsole = (IGameConsole)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(IGameConsole));
            
            if (TerrainShape.GameConsole.ConsoleState != ConsoleState.Closed)
                return;
#endif
            if (InputState.IsNewKeyPress(Keys.F12))
            {
                ShadowMap.DebugIsFor++;

                if (ShadowMap.DebugIsFor > DebugIsFor.ShadowTexelOffsetBottomRight)
                    ShadowMap.DebugIsFor = DebugIsFor.LightPosition;
            }

            // DEBUG: Test moving LightPos Height for shadows
            var lightPos = TerrainShape.LightPosition; // 2/13/2009
            var lightTar = ShadowMap.LightTarget; // 4/28/2009
            if (InputState.IsKeyPress(Keys.A) && ShadowMap.DebugIsFor == DebugIsFor.LightPosition)
            {
                lightPos.Y += 10;
                TerrainShape.LightPosition = lightPos;
            }

            if (InputState.IsKeyPress(Keys.Z) && ShadowMap.DebugIsFor == DebugIsFor.LightPosition)
            {
                lightPos.Y -= 10;
                TerrainShape.LightPosition = lightPos;
            }

            // DEBUG: Test moving LightTarget Height for shadows
            if (InputState.IsKeyPress(Keys.A) && ShadowMap.DebugIsFor == DebugIsFor.LightTarget)
            {
                lightTar.Y += 10;
                ShadowMap.LightTarget = lightTar;
            }

            if (InputState.IsKeyPress(Keys.Z) && ShadowMap.DebugIsFor == DebugIsFor.LightTarget)
            {
                lightTar.Y -= 10;
                ShadowMap.LightTarget = lightTar;
            }

            // 6/10/2009 - Adjust DepthBias by the thousands
            if (InputState.IsKeyPress(Keys.Add) && (InputState.IsKeyPress(Keys.LeftShift) || InputState.IsKeyPress(Keys.RightShift)))
            {
                ShadowMap.ShadowMapDepthBias += 0.0001f;
            }
            // 6/4/2009 - Adjust DepthBias
            else if (InputState.IsKeyPress(Keys.Add))
            {
                ShadowMap.ShadowMapDepthBias += 0.01f;
            }

            // 6/10/2009 - Adjust DepthBias by the thousands
            if (InputState.IsKeyPress(Keys.Subtract) && (InputState.IsKeyPress(Keys.LeftShift) || InputState.IsKeyPress(Keys.RightShift)))
            {
                ShadowMap.ShadowMapDepthBias -= 0.0001f;
            }
            // 6/4/2009 - Adjust DepthBias
            else if (InputState.IsKeyPress(Keys.Subtract))
            {
                ShadowMap.ShadowMapDepthBias -= 0.01f;
            }

            // DEBUG: Test moving LightPos's Position for shadows
            if (InputState.IsKeyPress(Keys.NumPad8) && ShadowMap.DebugIsFor == DebugIsFor.LightPosition)
            {
                lightPos.Z += 10;
                TerrainShape.LightPosition = lightPos;
            }

            if (InputState.IsKeyPress(Keys.NumPad2) && ShadowMap.DebugIsFor == DebugIsFor.LightPosition)
            {
                lightPos.Z -= 10;
                TerrainShape.LightPosition = lightPos;
            }

            if (InputState.IsKeyPress(Keys.NumPad4) && ShadowMap.DebugIsFor == DebugIsFor.LightPosition)
            {
                lightPos.X -= 10;
                TerrainShape.LightPosition = lightPos;
            }

            if (InputState.IsKeyPress(Keys.NumPad6) && ShadowMap.DebugIsFor == DebugIsFor.LightPosition)
            {
                lightPos.X += 10;
                TerrainShape.LightPosition = lightPos;
            }

            // DEBUG: Test moving LightTarget's Position for shadows
            if (InputState.IsKeyPress(Keys.NumPad8) && ShadowMap.DebugIsFor == DebugIsFor.LightTarget)
            {
                lightTar.Z += 10;
                ShadowMap.LightTarget = lightTar;
            }

            if (InputState.IsKeyPress(Keys.NumPad2) && ShadowMap.DebugIsFor == DebugIsFor.LightTarget)
            {
                lightTar.Z -= 10;
                ShadowMap.LightTarget = lightTar;
            }

            if (InputState.IsKeyPress(Keys.NumPad4) && ShadowMap.DebugIsFor == DebugIsFor.LightTarget)
            {
                lightTar.X -= 10;
                ShadowMap.LightTarget = lightTar;
            }

            if (InputState.IsKeyPress(Keys.NumPad6) && ShadowMap.DebugIsFor == DebugIsFor.LightTarget)
            {
                lightTar.X += 10;
                ShadowMap.LightTarget = lightTar;
            }

        }
        
        // 8/12/2009: Optimized.
        /// <summary>
        /// Check for input on <see cref="IMinimap"/>
        /// </summary>                
        private static void MiniMapInputCheck()
        {
            // 1/2/2010 - If Null, then just return.
            if (MiniMap == null) return;

            // 8/12/2009 - Cache
            var mmWidth = MiniMap.MMWidth; // 8/12/2009
            var mmHeight = MiniMap.MMHeight; // 8/12/2009
            var mapWidthToScale = TerrainData.MapWidthToScale;
            var mapHeightToScale = TerrainData.MapHeightToScale;
            var miniMapDest = MiniMap.MiniMapDestination; // 8/12/2009
            var thisPlayer = TemporalWars3DEngine.SThisPlayer; // 8/12/2009
            var cursorPosition = Cursor.Position; // 8/12/2009

            // Check if Cursor inside Minimap
            bool minimapMiniMapContainsCursor;
            var cursorPoint = new Point {X = (int) cursorPosition.X, Y = (int) cursorPosition.Y};
            miniMapDest.Contains(ref cursorPoint, out minimapMiniMapContainsCursor);
            MiniMap.MiniMapContainsCursor = minimapMiniMapContainsCursor;

            // Move camera to location on minimap, if MinimapMoveCamera action. 
            if (minimapMiniMapContainsCursor && InputState.MinimapMoveCamera)
            {
                var x = ((cursorPosition.X - miniMapDest.Left) / mmWidth) * mapWidthToScale;
                var y = ((cursorPosition.Y - miniMapDest.Top) / mmHeight) * mapHeightToScale;

                var tmpCameraTarget = new Vector3 {X = x, Y = 0, Z = y};
                Camera.CameraTarget = tmpCameraTarget;

            }

            // Move Units selected to location on minimap, if MinimapMoveUnits action.
            if (!minimapMiniMapContainsCursor || !InputState.MinimapMoveUnits) return;
                
            var tmpGoalPosition = new Vector3
                                      {
                                          X = ((cursorPosition.X - miniMapDest.Left) / mmWidth) * mapWidthToScale,
                                          Y = 0,
                                          Z = ((cursorPosition.Y - miniMapDest.Top) / mmHeight) * mapHeightToScale
                                      };

            Player.UnitsMoveOrder(thisPlayer, ref tmpGoalPosition, false);
        }

        /// <summary>
        /// Check for input on <see cref="TerrainShape"/>
        /// </summary>
        private static void TerrainShapeInputCheck()
        {

#if !XBOX360
            // 9/9/2008 - Only HandleInput when GameConsole Closed.
            if (TerrainShape.GameConsole.ConsoleState != ConsoleState.Closed)
                return;
#endif


#if EditMode // 8/14/2009 - ONLY needed when in Edit mode.
            
            // 4/17/2008 - Update Ground Cursor Position used for editing the terrain.
            Vector3 cursorPos;
            TerrainPickingRoutines.GetCursorPosByPickedRay(PickRayScale.DivideByTerrainScale, out cursorPos);
            TerrainEditRoutines.GroundCursorPosition = cursorPos;

            // Rebuild Normals in QuadTerrain.
            if (InputState.IsKeyPress(Keys.N) && InputState.IsKeyPress(Keys.LeftControl))
            {
                var rootQuadTree = TerrainShape.RootQuadTree;
                TerrainData.RebuildNormals(ref rootQuadTree);
            }


            // 5/13/2008 DEBUG: Changes showing PathNodes - On or Off
            if (InputState.IsNewKeyPress(Keys.F5))                         
                TerrainShape.DisplayPathNodes = !TerrainShape.DisplayPathNodes;
            
#endif

#if DEBUG

#if !XBOX360
            // 5/14/2008 - DEBUG: Updates the PathNodes Array
            if (InputState.IsNewKeyPress(Keys.F6))
                TerrainShape.PopulatePathNodesArray();

            // 5/18/2008 - DEBUG: Changes showing VisualTestNodes - On or Off
            if (InputState.IsNewKeyPress(Keys.F7))
            {
                AStarItem.ShowVisualTestedNodes = !AStarItem.ShowVisualTestedNodes;
            }

            // 5/25/2008 - DEBUG: Changes showing VisualPathNodes - On or Off
            if (InputState.IsNewKeyPress(Keys.F8))
            {
                AStarItem.ShowVisualPathNodes = !AStarItem.ShowVisualPathNodes;
            }

            // 4/10/2008 - Changes Draw Mode between Solid and WireFrame
            // 8/21/2008 - Add Gamepad check
            if (InputState.IsNewKeyPress(Keys.F9))
            {
                TerrainShape.DrawMode = TerrainShape.DrawMode == DrawMode.Solid ? DrawMode.WireFrame : DrawMode.Solid;
            }

            // 8/10/2009 - If 'T' for Timers; // 4/12/2010 - Updated to make combo with 'Left-Control'.
            if (InputState.IsKeyPress(Keys.T) && InputState.IsKeyPress(Keys.LeftControl))
            {
                var timers = (StopWatchTimers)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(StopWatchTimers));
                timers.IsVisible = !timers.IsVisible;
            }

            // 7/30/2008 - Test Enabling BumpMap in Shader
            if (InputState.IsNewKeyPress(Keys.F10))
                TerrainShape.EnableNormalMap = !TerrainShape.EnableNormalMap;  
  
#endif

            const int inputs = InputState.MaxInputs; // 11/6/2009
            var currentGamepadStates = InputState.CurrentGamepadStates; // 4/30/2010 - Cache
            var lastGamepadStates = InputState.LastGamepadStates; // 4/30/2010 - Cache
            for (var i = 0; i < inputs; i++)
            {
                // 6/29/2012 - Cache
                GamePadState currentGamepadState = currentGamepadStates[i];
                GamePadState lastGamepadState = lastGamepadStates[i];
                GamePadTriggers gamePadTriggers = currentGamepadState.Triggers;
                GamePadTriggers padTriggers = lastGamepadState.Triggers;

                // 8/28/2008 - Test Turning on ShadowMap/Water for XBOX
                if (gamePadTriggers.Right <= 0.9f || (padTriggers.Right > 0.9f)) continue;

                // 11/14/2008: TODO - Debug Purposes
                switch (_useItem)
                {
                    case 0:
                        // Water
                        var water = (IWaterManager)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(IWaterManager));
                        water.IsVisible = !water.IsVisible;
                        break;
                    case 1:
                        // ShadowMap                       
                        if (TerrainShape.ShadowMapInterface != null)
                            TerrainShape.ShadowMapInterface.IsVisible = !TerrainShape.ShadowMapInterface.IsVisible;
                        else
                            TerrainShape.ShadowMapInterface = (IShadowMap)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(IShadowMap));
                        break;
                    case 2:
                        // StopWatchTimers                       
                        var timers = (StopWatchTimers)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(StopWatchTimers));
                        timers.IsVisible = !timers.IsVisible;

                        break;
                    case 3: // 1/20/2011 - Show Wireframe
                        TerrainShape.DrawMode = TerrainShape.DrawMode == DrawMode.Solid ? DrawMode.WireFrame : DrawMode.Solid;
                        break;
                } // End Switch
            }

            // 11/14/2008 - TODO: Debug Purposes
            // This simply circulates the '_useItem' value between 0-2, which is
            // checked in the method above.
            for (var i = 0; i < inputs; i++)
            {
                // 6/29/2012 - Cache
                GamePadState currentGamepadState = currentGamepadStates[i];
                GamePadState lastGamepadState = lastGamepadStates[i];
                GamePadTriggers gamePadTriggers = currentGamepadState.Triggers;
                GamePadTriggers padTriggers = lastGamepadState.Triggers;

                if (gamePadTriggers.Left <= 0.9f || (padTriggers.Left > 0.9f)) continue;

                if (_useItem < 4)
                    _useItem++;
                else
                    _useItem = 0;
            } // End Loop
#endif
            
        } 


        // 8/21/2008 - 
        // 1/15/2009: Updated to be a Static method, which should optimize memory.
        /// <summary>
        /// Checks Mouse/Gamepad input for <see cref="Player"/> instances.
        /// </summary>
        /// <param name="player">This instance of <see cref="Player"/></param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public static void PlayerInputCheck(Player player, GameTime gameTime)
        {
            // 4/28/2009
            if (InputState == null) return;
            
            // 6/15/2010 - Updated to retrieve the ROC collection.
            ReadOnlyCollection<SceneItemWithPick> itemsSelected;
            Player.GetItemsSelected(player, out itemsSelected);

            var playerNumber = player.PlayerNumber;

            try
            {
                // 1/2/2010 - Set Minimap ContainsCursor
                var miniMapContainsCursor = MiniMap != null && MiniMap.MiniMapContainsCursor;

                // 5/20/2009 - Set '_cursor' to normal, when true.
                // 12/8/2008 - Add Check to make sure '_cursor' not already inside MiniMap area.
                if (miniMapContainsCursor || IFDTileManager.CursorInSomeIFDTile)
                {
                    Cursor.CursorTextureToDisplay = CursorTextureEnum.Normal;
                    return;
                }
                
                // Check if any of the sceneItems have been Picked by mouse or is Hovered      
                Player.SceneItemsPickCheck(player, InputState, gameTime);

                // 2/23/2011 - Skip rest of method call if enemy player
                if (player.PlayerNumber != TemporalWars3DEngine.SThisPlayer)
                    return;

                // 2/23/2011 - Refactored code.
                // Check if Left Mouse Click for AreaSelect Picking!
                DoAreaSelectPicking(player);

                // 5/1/2009 - If some Items selected, then check if where _cursor is hovering on terrain, the
                //            location is valid for move orders!  If not, then display stop symbol.
                if (itemsSelected.Count > 0 && !IFDTileManager.CursorInSomeIFDTile)
                {
                    // If one SceneItemOwner selected, then make sure moveable SceneItemOwner.
                    if (itemsSelected.Count == 1)
                    {
                        // 6/29/2012 - Cache
                        var sceneItemWithPick = itemsSelected[0];

                        // 6/29/2012 - Check if null
                        if (sceneItemWithPick != null && sceneItemWithPick.ItemMoveable)
                        {
                            // Then check for blocked areas
                            CursorPositionBlockedCheck();
                        }
                        else
                            Cursor.CursorTextureToDisplay = CursorTextureEnum.Normal; // 5/6/2009
                    }
                    else
                    {
                        // check for blocked areas.
                        CursorPositionBlockedCheck();
                    }

                }
                else
                    Cursor.CursorTextureToDisplay = CursorTextureEnum.Normal; // 5/5/2009

                // Check if 'Attack Order' was just given.
                var attackOrderGiven = false;
                
                if (InputState.AttackOrderGiven)
                {
                    attackOrderGiven = Player.SceneItemsAttackCheck(player, InputState);
                }

                // 10/19/2009 - Check if issue a 'AttackMove' order, and 'attackOrder' for some item was not just given.
                Vector3 goalPosition;
                if (InputState.AttackMoveOrderGiven && !attackOrderGiven)
                {
                    // Get PickedRay Position in PathNodes Cordinates
                    TerrainPickingRoutines.GetCursorPosByPickedRay(PickRayScale.NoChange, out goalPosition); // was PickRayScale.DivideByAStarPathScale

                    // Calls Redirection method
                    Player.UnitsMoveOrder(playerNumber, ref goalPosition, true);

                } // End if RightClick with A key.

                // Check if issue a 'Move Order', and an 'attackOrder' for some item was not just given.
                if (InputState.MoveOrderGiven && !attackOrderGiven)
                {
                    // Get PickedRay Position in PathNodes Cordinates
                    TerrainPickingRoutines.GetCursorPosByPickedRay(PickRayScale.NoChange, out goalPosition); // was PickRayScale.DivideByAStarPathScale

                    // 12/1/2008 - Calls Redirection method
                    Player.UnitsMoveOrder(playerNumber, ref goalPosition, false);

                } // End If RightClick  
               
                // Check if 'Attack Ground Order' just given
                if (InputState.AttackGroundOrderGiven)
                {
                    // Get PickedRay Position in PathNodes Cordinates
                    TerrainPickingRoutines.GetCursorPosByPickedRay(PickRayScale.DivideByAStarPathScale, out goalPosition);
                    Player.UnitsAttackGroundOrder(player, ref goalPosition); // 6/14/2010 - Updated to passing in 'Player'.

                } // End If RightClick W/LeftControl Key

                // Check for 'SelectLocalUnits' order
                if (InputState.SelectLocalUnits)
                {
                    Player.SelectLocalUnits(player); // 6/14/2010 - Updated to passing in 'Player'.
                }

                // Check for 'SelectAllUnits' order
                if (InputState.SelectAllUnits)
                {
                    Player.SelectAllUnits(player); // 6/14/2010 - Updated to passing in 'Player'.
                }

                // Check for 'DeSelectAll' items order
                if (InputState.DeselectAllUnits)
                {
                    Player.DeSelectAll(player); // 6/14/2010 - Updated to passing in 'Player'.
                }

                // 6/1/2009 - Set DefenseAI Stance choosen by user for all items selected.
                if (InputState.SelectAggressiveStance)
                {
                    SetDefenseAIStance(itemsSelected, DefenseAIStance.Aggressive);
                }

                if (InputState.SelectGuardStance)
                {
                    SetDefenseAIStance(itemsSelected, DefenseAIStance.Guard);
                }

                if (InputState.SelectHoldFireStance)
                {
                    SetDefenseAIStance(itemsSelected, DefenseAIStance.HoldFire);
                }

                if (InputState.SelectHoldGroundStance)
                {
                    SetDefenseAIStance(itemsSelected, DefenseAIStance.HoldGround);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    string.Format("Method Error: 'HandleGameInput' classes PlayerInputCheck method exception {0}.",
                                  ex.Message));
            }
            
        }

        // 2/23/2011
        /// <summary>
        /// Does an AreaSelect picking routine, using the rubber-band.
        /// </summary>
        /// <param name="player">Instance of <see cref="Player"/>.</param>
        private static void DoAreaSelectPicking(Player player)
        {
            var cursorPosition = Cursor.Position; // 8/14/2009
            var inTmpX = (int)cursorPosition.X;
            var inTmpY = (int)cursorPosition.Y;

            // 6/16/2009: Updated to check the 'PickHoveredForSomeItem'.
            if (InputState.StartAreaSelect) //&& Player.PickHoveredForSomeBuildingItem == -1 && Player.PickHoveredForSomeVehicleItem == -1
                Player.AreaSelect_DrawRect(player, inTmpX, inTmpY);
            else // 9/28/2009 - Add 'Else'
            {
                Player.AreaSelect_SelectItems(player); // 6/14/2010 - Updated to pass in 'Player'.
                TerrainAreaSelect.AreaSelect = false;
            }
        }

        // 6/1/2009
        /// <summary>
        /// Itereates through the <see cref="Player._itemsSelected"/> collection, and sets their <see cref="DefenseAIStance"/> to the given value.
        /// </summary>
        /// <param name="itemsSelected">Collection of <see cref="SceneItemWithPick"/></param>
        /// <param name="defenseAIStance"><see cref="DefenseAIStance"/> Enum</param>
        private static void SetDefenseAIStance(IList<SceneItemWithPick> itemsSelected, DefenseAIStance defenseAIStance)
        {
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player thisPlayer;
            if (!TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out thisPlayer))
                return;

            // iterate ItemsSelected array
            var itemsSelectedCount = itemsSelected.Count;
            for (var i = 0; i < itemsSelectedCount; i++)
            {
                // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                SceneItemWithPick selected;
                if (!Player.GetItemsSelectedByIndex(thisPlayer, i, out selected))
                    break;

                if (selected == null) continue; // 4/30/2010

                // set new defense stance.
                selected.DefenseAIStance = defenseAIStance;

                // 6/2/2009 - Set the AIOrderIssued enum to 'None'.
                selected.AIOrderIssued = AIOrderType.None;

                // if MP game and Client, then send info to Host.
                if (thisPlayer.NetworkSession == null) continue;

                if (thisPlayer.NetworkSession.IsHost)
                {
                    // HOST SECTION

                    // 6/1/2009 - Tell client to stop any attacking for given sceneItem, if 'HoldFire' stance.
                    if (defenseAIStance == DefenseAIStance.HoldFire)
                    {
                        // 6/3/2009 - Updated to use the method now.
                        selected.SendCeaseAttackOrderToMPPlayer();
                    }

                    // 6/29/2009 - Send Host player's 'Stance' to Client.
                    // Get RTSComm Stance SceneItemOwner from PoolManager
                    RTSCommSceneItemStance sceneItemStance;
                    PoolManager.GetNode(out sceneItemStance);

                    sceneItemStance.Clear();
                    sceneItemStance.DefenseAIStance = selected.DefenseAIStance;
                    sceneItemStance.NetworkItemNumber = selected.NetworkItemNumber;
                    sceneItemStance.PlayerNumber = selected.PlayerNumber;
                    sceneItemStance.NetworkCommand = NetworkCommands.SceneItemStance;

                    // Send to Client
                    NetworkGameComponent.AddCommandsForClientG(sceneItemStance);


                }
                else // Client, so send Client player's 'Stance' to Host.
                {
                    // CLIENT SECTION

                    // Get RTSComm Stance SceneItemOwner from PoolManager
                    RTSCommSceneItemStance sceneItemStance;
                    PoolManager.GetNode(out sceneItemStance);

                    sceneItemStance.Clear();
                    sceneItemStance.DefenseAIStance = selected.DefenseAIStance;
                    sceneItemStance.NetworkItemNumber = selected.NetworkItemNumber;
                    sceneItemStance.PlayerNumber = selected.PlayerNumber;
                    sceneItemStance.NetworkCommand = NetworkCommands.SceneItemStance;

                    // Send to Host
                    NetworkGameComponent.AddCommandsForServerG(sceneItemStance);
                }
            }
        }

        // 5/1/2009
        /// <summary>
        /// Checks if the current <see cref="Cursor"/> location is <see cref="CursorTextureEnum.Blocked"/>, and updates 
        /// the <see cref="Cursor"/> class to display the BlockArea icon.
        /// </summary>
        private static void CursorPositionBlockedCheck()
        {
            Vector3 goalPosition;
            // Get PickedRay Position in PathNodes Cordinates
            TerrainPickingRoutines.GetCursorPosByPickedRay(PickRayScale.NoChange, out goalPosition);

            // Check AStarGraph to see if blocked.
            var aStarGraph = TemporalWars3DEngine.AStarGraph; // 4/30/2010 - Cache
            Cursor.CursorTextureToDisplay = aStarGraph != null
                                                ? (aStarGraph.IsNodeBlockedForCursor(
                                                    NodeScale.TerrainScale, (int) goalPosition.X,
                                                    (int) goalPosition.Z)
                                                       ? CursorTextureEnum.Blocked
                                                       : CursorTextureEnum.Normal)
                                                : CursorTextureEnum.Normal;
        }

        #region Special GroupSelection methods (Ex: Ctrl-1 to get a group)

        static readonly Dictionary<short, List<SceneItemWithPick>> SpecialSelectionGroups = new Dictionary<short, List<SceneItemWithPick>>();

        // 2/5/2009; // 6/15/2010 - Updated to pass in 'Player' instance param.
        /// <summary>
        /// Allows user to Group items together in 'Control-Groups', which are assinged using the 'LeftCtrl-#' method.  User
        /// is able to quickly select their group by simply selecting the designated number given (PC ONLY).
        /// </summary>
        /// <param name="player"><see cref="Player"/> instance</param>
        public static void SpecialGroupSelectionCheck(Player player)
        {
            // 4/28/2009
            var inputState = InputState; // 4/30/2010
            if (inputState == null) return;
           
            //
            // 5/13/2009 - Assign Group Check section
            //
            {
                if (inputState.AssignGroup1)
                    AddSelectionToSpecialGroups(player, 1);

                else if (inputState.AssignGroup2)
                    AddSelectionToSpecialGroups(player, 2);

                else if (inputState.AssignGroup3)
                    AddSelectionToSpecialGroups(player, 3);

                else if (inputState.AssignGroup4)
                    AddSelectionToSpecialGroups(player, 4);

                else if (inputState.AssignGroup5)
                    AddSelectionToSpecialGroups(player, 5);

                else if (inputState.AssignGroup6)
                    AddSelectionToSpecialGroups(player, 6);

                else if (inputState.AssignGroup7)
                    AddSelectionToSpecialGroups(player, 7);

                else if (inputState.AssignGroup8)
                    AddSelectionToSpecialGroups(player, 8);

                else if (inputState.AssignGroup9)
                    AddSelectionToSpecialGroups(player, 9);

                else if (inputState.AssignGroup10)
                    AddSelectionToSpecialGroups(player, 0);
            }

            //
            // 5/13/2009 - Select Group Check section
            //
            {
                if (inputState.SelectGroup1)
                    RetrieveSpecialGroupsSelection(player, 1);

                else if (inputState.SelectGroup2)
                    RetrieveSpecialGroupsSelection(player, 2);

                else if (inputState.SelectGroup3)
                    RetrieveSpecialGroupsSelection(player, 3);

                else if (inputState.SelectGroup4)
                    RetrieveSpecialGroupsSelection(player, 4);

                else if (inputState.SelectGroup5)
                    RetrieveSpecialGroupsSelection(player, 5);

                else if (inputState.SelectGroup6)
                    RetrieveSpecialGroupsSelection(player, 6);

                else if (inputState.SelectGroup7)
                    RetrieveSpecialGroupsSelection(player, 7);

                else if (inputState.SelectGroup8)
                    RetrieveSpecialGroupsSelection(player, 8);

                else if (inputState.SelectGroup9)
                    RetrieveSpecialGroupsSelection(player, 9);

                else if (inputState.SelectGroup10)
                    RetrieveSpecialGroupsSelection(player, 0);
            }            
            #endregion
        }

        // 2/5/2009
        /// <summary>
        /// Retrieves the items contain for the given 'Group' #, and reassigns back
        /// into the given <paramref name="player"/> <see cref="Player._itemsSelected"/> collection.
        /// </summary>
        /// <param name="player"><see cref="Player"/> instance</param>
        /// <param name="groupNumber">Group number to assign</param>
        private static void RetrieveSpecialGroupsSelection(Player player, short groupNumber)
        {
            if (!SpecialSelectionGroups.ContainsKey(groupNumber)) return;

            // 6/15/2010 - Updated to retrieve the ROC collection.
            ReadOnlyCollection<SceneItemWithPick> itemsSelected;
            Player.GetItemsSelected(player, out itemsSelected);

            if (itemsSelected == null) return;

            // 1st - Deselect current ItemsSelected and mark PickSelected off.
            var count = itemsSelected.Count; // 8/14/2009
            for (var i = 0; i < count; i++)
            {
                // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                SceneItemWithPick selected;
                if (!Player.GetItemsSelectedByIndex(player, i, out selected))
                    break;

                // 11/11/09 - check for Null
                if (selected == null) continue;

                selected.PickSelected = false;
            }
            
            Player.ClearItemsSelected(player); // 6/15/2010

            // Remove any items which might have been deleted.
            SpecialSelectionGroups[groupNumber].RemoveAll(IsDeleted);

            // then assign items to 'ItemsSelected' saved in dictionary.                                
            //itemsSelected.AddRange(SpecialSelectionGroups[groupNumber]);
            Player.AddItemsSelectedRange(player, SpecialSelectionGroups[groupNumber]); // 6/15/2010

            // Turn on Selection Flag
            count = itemsSelected.Count; // 8/14/2009
            for (var i = 0; i < count; i++)
            {
                // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                SceneItemWithPick selected;
                if (!Player.GetItemsSelectedByIndex(player, i, out selected))
                    break;

                // 11/11/09 - check for Null
                if (selected == null) continue;

                selected.PickSelected = true;
            }
        }

        // 2/5/2009
        /// <summary>
        /// Adds the given collection of given <paramref name="player"/> <see cref="Player._itemsSelected"/> to the internal Dictionary 
        /// called <see cref="SpecialSelectionGroups"/>.
        /// </summary>
        /// <param name="player"><see cref="Player"/> instance</param>
        /// <param name="groupNumber">Group number to assign</param>
        private static void AddSelectionToSpecialGroups(Player player, short groupNumber)
        {
            // 6/15/2010 - Updated to retrieve the ROC collection.
            ReadOnlyCollection<SceneItemWithPick> itemsSelected;
            Player.GetItemsSelected(player, out itemsSelected);

            if (itemsSelected == null) return;

            // 3/22/2009 - Remove any buildings from list!
            //itemsSelected.RemoveAll(RemoveBuildingItems);
            Player.RemoveAllItemsSelected(player, RemoveBuildingItems); // 6/15/2010

            // 11/11/09 - Updated to use TryGetValue, and not ContainsKey
            List<SceneItemWithPick> selectionGroup;
            if (SpecialSelectionGroups.TryGetValue(groupNumber, out selectionGroup))
            {
                // 11/11/09 - Sets ScreenTextItems for old selection 'Visible' to false.
                UpdateSelectionsScreenTextItems(selectionGroup, groupNumber, false); 

                // then clear old lists of items
                selectionGroup.Clear();

                // add new list into dictionary
                selectionGroup.AddRange(itemsSelected);

                // 11/11/09 - Sets ScreenTextItems for new selection 'Visible' to true.
                UpdateSelectionsScreenTextItems(selectionGroup, groupNumber, true); 

                // update changes back to dictionary
                SpecialSelectionGroups[groupNumber] = selectionGroup;
            }
            else
            {
                // add new entry into dictionary
                var specialGroup = new List<SceneItemWithPick>();
                specialGroup.AddRange(itemsSelected);

                // 11/11/09 - Sets ScreenTextItems for new selection 'Visible' to true.
                UpdateSelectionsScreenTextItems(specialGroup, groupNumber, true); 

                SpecialSelectionGroups.Add(groupNumber, specialGroup);
            }
        }

        // 11/11/2009
        /// <summary>
        /// Helper method, which updates collection of <paramref name="selectionGroup"/>, to have the 'Visible' flag
        /// of the <see cref="ScreenTextItem"/> set to <paramref name="isVisible"/>.
        /// </summary>
        /// <param name="selectionGroup">Special SelectionGroup array</param>
        /// <param name="groupNumber"></param>
        /// <param name="isVisible">True/False</param>
        private static void UpdateSelectionsScreenTextItems(IList<SceneItemWithPick> selectionGroup, short groupNumber, bool isVisible)
        {
            var count = selectionGroup.Count;
            for (var i = 0; i < count; i++)
            {
                // cache
                var selectedItem = selectionGroup[i];
                if (selectedItem == null) continue;

                // set Visible?
                selectedItem.ScreenTextSpecialGroupNumber.Visible = isVisible;

                selectedItem.SpecialGroupNumber = isVisible ? groupNumber : (short)-1; // 11/11/09

                selectionGroup[i] = selectedItem;
            }
        }

        // 3/22/2009 -
        /// <summary>
        /// Predicate used to determine if <see cref="SceneItemWithPick"/> item is a <see cref="BuildingScene"/> type.
        /// </summary>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        /// <returns>true/false</returns>
        private static bool RemoveBuildingItems(SceneItemWithPick item)
        {
            if (item != null)
            {
                if (!(item is BuildingScene))
                    return false;

                item.PickSelected = false;
            }
            return true;
        }

        /// <summary>
        /// Predicate used to check if <see cref="SceneItemWithPick"/> item has the internal 'Delete'
        /// flag set to true.
        /// </summary>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        /// <returns>true/false</returns>
        private static bool IsDeleted(SceneItemWithPick item)
        {
            return item.Delete;
        }

        #region DragMove Methods
        

        // 4/12/2010
        /// <summary>
        /// Checks if user has Left-Mouse button pressed down with Left-Alt pressed.
        /// </summary>
        /// <param name="dragMoveStarted">Sets if dragMove started.</param>
        public static bool DoDragMoveCheck(ref bool dragMoveStarted)
        {
            if (!InputState.LeftMouseButtonHeldDown) return false;

            // 2/8/2010 - Also make sure the 'Left-Alt' key is pressed too.
            if (!InputState.LeftAlt) return false;
           
            dragMoveStarted = true;

            return true;
        }

        /// <summary>
        /// Allows for some <see cref="SceneItem"/> to me Dragged around, when in EditMode.
        /// </summary>
        /// <param name="dragMoveStarted">Sets if dragMove started.</param>
        /// <param name="newPosition">(OUT) <see cref="Vector3"/> as new position</param>
        public static bool CheckForItemDragMove(ref bool dragMoveStarted, out Vector3 newPosition)
        {
            newPosition = Vector3.Zero;

            // 11/21/2009 - Check if DragMove started.
            if (!dragMoveStarted) return false;

            // Get Position of Mouse Cursor in World Space 
            Vector3 placeItemAt;
            TerrainPickingRoutines.GetCursorPosByPickedRay(PickRayScale.NoChange, out placeItemAt);

            // set item to new position
            newPosition = placeItemAt; // 4/12/2010

            // 4/3/2011
            TerrainQuadTree.UpdateSceneryCulledList = true;

            // check if LeftButton released, which ends the DragMove state.
            if (!InputState.LeftMouseButtonReleased) return true;

            dragMoveStarted = false;

            return false;
        }

        #endregion
    }
}
