#region File Description
//-----------------------------------------------------------------------------
// TerrainWaypoints.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.GameCamera;
using ImageNexus.BenScharbach.TWEngine.HandleGameInput;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWEngine.Terrain.Enums;
using ImageNexus.BenScharbach.TWEngine.Terrain.Structs;
using ImageNexus.BenScharbach.TWEngine.TerrainTools;
using ImageNexus.BenScharbach.TWEngine.Utilities;
using ImageNexus.BenScharbach.TWEngine.Utilities.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if !XBOX360
using System.Windows.Forms;
using TWEngine.TerrainTools;
#endif


namespace ImageNexus.BenScharbach.TWEngine.Terrain
{
    /// <summary>
    /// The <see cref="TerrainWaypoints"/> class is used to create waypoints, which are simply <see cref="Vector3"/> locations
    /// within the game world.  These are used for various scripting conditions, like unit movement, spawning new units, <see cref="Camera"/> path
    /// movement, etc.
    /// </summary>
    public sealed class TerrainWaypoints : DrawableGameComponent
    {
        // Increment for each new Waypoint requested.
        private static int _waypointsCounter;

        // Used to draw the square boxes to show waypoints.
        private static BasicEffect _lineEffect;
        private static VertexDeclaration _lineVertexDeclaration;
        private static Rectangle _rectangleArea;
        private static readonly Color VisualWaypointColor = Color.BlueViolet;

        // XNA 4.0 Updates - Replaces 'RenderState' settings.
        private static RasterizerState _rasterizerState;
        private static DepthStencilState _depthStencilState;

        // 10/16/2009 - Used to draw Lines to show the waypoint paths.
        // NOTE: Only ONE waypoint path can be shown at a time!
        private static VertexPositionColor[] _visualPathPointList;
        private static int[] _visualPathStripIndices;

        // 4/12/2010 - Stores the current Selected waypoint key
        private static int _selectedWaypointKey = -1;

        // 4/12/2010 - Stores the DragMove vars.
        private static int _dragMoveWaypointKey = -1;
        private static bool _dragMoveStarted;
      

        #region Properties

        // 12/17/2009
        /// <summary>
        /// Allows defining new <see cref="WaypointStruct"/>
        /// </summary>
        public static bool DoDefineWaypoint { get; set; }

        // 10/13/2009 - 
        /// <summary>
        /// Dictionary storing the <see cref="WaypointStruct"/>, using the index as key.
        /// </summary>
        public static Dictionary<int, WaypointStruct> Waypoints { get; private set; }

        // 10/15/2009
        /// <summary>
        /// Dictionary storing a <see cref="LinkedList{T}"/> of <see cref="WaypointStruct"/> 
        /// indexes, which define paths for scripts.  
        /// </summary>
        /// <remarks> Waypoints must be created first, before a path can be created.</remarks>
        public static Dictionary<string, LinkedList<int>> WaypointPaths { get; private set; }

        #endregion

        ///<summary>
        /// Constructor for the <see cref="TerrainWaypoints"/>, which initializes the
        /// two internal dictionaries, <see cref="Waypoints"/> and <see cref="WaypointPaths"/>, and creates
        /// the <see cref="BasicEffect"/> for the lines.
        ///</summary>
        ///<param name="game">Instance of Game</param>
        public TerrainWaypoints(Game game)
            : base(game)
        {
            // 10/13/2009 - Init the STATIC Waypoints Dictionary.
            if (Waypoints == null)
                Waypoints = new Dictionary<int, WaypointStruct>();

            // 10/15/2009 - Init the STATIC WaypointPaths Dictionary.
            if (WaypointPaths == null)
                WaypointPaths = new Dictionary<string, LinkedList<int>>();

            // XNA 4.0 Updates - Remove 2nd param.
            // 10/14/2009 - Line effect is used for rendering area rectangle
            //_lineEffect = new BasicEffect(game.GraphicsDevice, null) { VertexColorEnabled = true };
            _lineEffect = new BasicEffect(game.GraphicsDevice) { VertexColorEnabled = true };

            // XNA 4.0 Updates - VertexDeclaration not needed here.
            //_lineVertexDeclaration = new VertexDeclaration(game.GraphicsDevice, VertexPositionColor.VertexElements);

            // XNA 4.0 Updates - Replaces 'RenderState' settings.
            _rasterizerState = new RasterizerState { CullMode = CullMode.None, FillMode = FillMode.Solid };
            _depthStencilState = new DepthStencilState { DepthBufferEnable = false };

            // Set 'DoDefineArea' to false, for off.  This is set via the 'PropertiesTool' form 'Waypoints' tab.
            DoDefineWaypoint = false;

            // Draworder
            DrawOrder = 125;
        }

        /// <summary>
        /// When the PropertiesTool window is open, this will check for waypoint picking, and
        /// creates new waypoints at given cursor location when user left-clicks mouse.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance.</param>
        public override void Update(GameTime gameTime)
        {

#if !XBOX360
            // cache
            var propertiesTools = TerrainEditRoutines.PropertiesTools;

            // make sure not NULL
            if (propertiesTools == null)
                return;

            // When propertiesTool form is open, checks for picking of visual Waypoints.
            DoPropertiesToolCheck();
            
            // 10/13/2009: Waypoint picking can only be done when 'DefineWaypoints' Checked,
            //             AND Mouse can not be in control!
            if (!DoDefineWaypoint || propertiesTools.IsMouseInControl()) return;

            // Get WorldSpace position for cursor.
            Vector3 placeItemAt;
            TerrainPickingRoutines.GetCursorPosByPickedRay(PickRayScale.NoChange, out placeItemAt);

            // Show cordinates of cursor location.
            propertiesTools.SetWaypointLocationText(ref placeItemAt);

            // Check for Left-Click, and add to list when TRUE.
            if (HandleInput.InputState.LeftMouseButton)
            {
                // Add Waypoint to AStarItem Dictionary.
                var index = AddWaypoint(ref placeItemAt);

                // Add location to ListView for Waypoints
                propertiesTools.AddWaypointIndexToListview(index);

            }

#endif

            base.Update(gameTime);
        }

        /// <summary>
        /// Draws the waypoints into the game world, ONLY when the
        /// PropertiesTool window is open.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public override void Draw(GameTime gameTime)
        {
#if DEBUG
            // 5/26/2010 - DEBUG
            StopWatchTimers.StartStopWatchInstance(StopWatchName.GameDrawLoop_Main_Waypoints);
#endif

#if !XBOX360
            // Only draw when PropertiesTool form is open.
            if (TerrainEditRoutines.PropertiesTools == null
                || TerrainEditRoutines.PropertiesTools.Visible == false)
                return;

            // Draw the Waypoints
            DrawWaypoints(GraphicsDevice);

            // Draw the WaypointPath LineStrip, if possible.
            DrawWaypointPathLineStrip(GraphicsDevice);

#endif

            base.Draw(gameTime);

#if DEBUG
            // 5/26/2010 - DEBUG
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.GameDrawLoop_Main_Waypoints);
#endif
        }

        // 10/14/2009
        /// <summary>
        /// Draws all instances of waypoints contain in the dictionary, into
        /// the game world.
        /// </summary>
        /// <param name="graphicsDevice"><see cref="GraphicsDevice"/> instance</param>
        private static void DrawWaypoints(GraphicsDevice graphicsDevice)
        {
            // XNA 4.0 Updates - RenderState obsolete.
            /*graphicsDevice.RenderState.FillMode = FillMode.Solid;
            graphicsDevice.RenderState.CullMode = CullMode.None;
            graphicsDevice.RenderState.DepthBufferEnable = false;*/
            graphicsDevice.RasterizerState = _rasterizerState;
            graphicsDevice.DepthStencilState = _depthStencilState;

            _lineEffect.View = Camera.View;
            _lineEffect.Projection = Camera.Projection;

            // XNA 4.0 updates - Begin() and End() obsolete.
            //_lineEffect.Begin();
            _lineEffect.CurrentTechnique.Passes[0].Apply();

            // XNA 4.0 Updates - VertexDeclaration is obsolete
            // Draw the triangle.
            //graphicsDevice.VertexDeclaration = _lineVertexDeclaration;

            // Iterate Dictionary to draw each VisualRectangles.
            foreach (var waypoint in Waypoints)
            {
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, waypoint.Value.VisualRectangleArea, 0, 2);
            }

            // XNA 4.0 updates - Begin() and End() obsolete.
            //_lineEffect.CurrentTechnique.Passes[0].End();
            //_lineEffect.End();
           
        }

        /// <summary>
        /// Creates the rectangle used to draw the waypoint rectangle in the game world.
        /// </summary>
        /// <param name="waypointLocation"><see cref="Vector3"/> location</param>
        /// <param name="colorToUse"><see cref="Color"/> to use</param>
        /// <param name="visualRectangle">Collection of <see cref="VertexPositionColor"/></param>
        private static void CreateVisualWaypoint(Vector3 waypointLocation, Color colorToUse, out VertexPositionColor[] visualRectangle)
        {
            // Create initial rectangle area, using waypoint location.
            const int recWidth = 40;
            const int recHeight = 40;
            _rectangleArea.X = (int)waypointLocation.X - 20; _rectangleArea.Y = (int)waypointLocation.Z - 20;
            _rectangleArea.Width = recWidth; _rectangleArea.Height = recHeight;

            // Create the visual triangle points, to draw rectangle.
            CreateVisualRectangle(ref _rectangleArea, colorToUse, out visualRectangle, waypointLocation.Y);
        }

        /// <summary>
        /// Creates the <see cref="visualRectangle"/>, used to draw the waypoint rectangles in the game world.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="visualRectangle"/> length is less than 6.</exception>
        /// <param name="rectangle"><see cref="rectangle"/> struct</param>
        /// <param name="colorToUse"><see cref="Color"/> to use</param>
        /// <param name="visualRectangle">Collection of <see cref="VertexPositionColor"/></param>
        /// <param name="useHeight">Height value to use</param>
        private static void CreateVisualRectangle(ref Rectangle rectangle, Color colorToUse, out VertexPositionColor[] visualRectangle, float useHeight)
        {
            // make sure array exist
            visualRectangle = new VertexPositionColor[6];

            // make sure proper size array
            if (visualRectangle.Length < 6)
                throw new ArgumentOutOfRangeException("visualRectangle", @"Array must be a size of 6.");

            // Set Positions
            visualRectangle[0].Position = new Vector3(rectangle.X, useHeight, rectangle.Y);
            visualRectangle[1].Position = new Vector3(rectangle.Right, useHeight, rectangle.Bottom);
            visualRectangle[2].Position = new Vector3(rectangle.X, useHeight, rectangle.Bottom);

            visualRectangle[3].Position = new Vector3(rectangle.X, useHeight, rectangle.Y);
            visualRectangle[4].Position = new Vector3(rectangle.Right, useHeight, rectangle.Top);
            visualRectangle[5].Position = new Vector3(rectangle.Right, useHeight, rectangle.Bottom);

            // Set Color
            for (var i = 0; i < 6; i++)
                visualRectangle[i].Color = colorToUse;

        }

        // 10/16/2009
        /// <summary>
        /// Draws the current WaypointPath choosen, via PropertiesTool form 
        /// comboBox 'cbWaypointPathNames', as a line strip between each waypoint, 
        /// showing the connections.
        /// </summary>
        /// <param name="graphicsDevice"><see cref="GraphicsDevice"/> instance</param>
        private static void DrawWaypointPathLineStrip(GraphicsDevice graphicsDevice)
        {
            // 4/7/2010: Fix: Updated the check for 'PointList.Length', to be at least 2,
            //                which is required for a single line, otherwise crash will occur.
            // Make sure something can be drawn.
            if (_visualPathPointList == null || _visualPathStripIndices == null
                || _visualPathPointList.Length <= 1 || _visualPathStripIndices.Length <= 1)
                return;

            // XNA 4.0 Updates - RenderState obsolete.
            /*graphicsDevice.RenderState.FillMode = FillMode.Solid;
            graphicsDevice.RenderState.CullMode = CullMode.None;
            graphicsDevice.RenderState.DepthBufferEnable = false;*/
            graphicsDevice.RasterizerState = _rasterizerState;
            graphicsDevice.DepthStencilState = _depthStencilState;

            _lineEffect.View = Camera.View;
            _lineEffect.Projection = Camera.Projection;

            // XNA 4.0 updates - Begin() and End() obsolete.
            //_lineEffect.Begin();
            _lineEffect.CurrentTechnique.Passes[0].Apply();

            // XNA 4.0 Updates - VertexDeclaration obsolete.
            // Draw the LineStrip.
            //graphicsDevice.VertexDeclaration = _lineVertexDeclaration;

            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineStrip, _visualPathPointList, 0, // vertex buffer offset to add to each element of the index buffer
                                                                          _visualPathPointList.Length, // number of vertices to draw
                                                                          _visualPathStripIndices,
                                                                          0, // first index element to read
                                                                          _visualPathPointList.Length - 1 // number of primitives to draw
                );

            // XNA 4.0 updates - Begin() and End() obsolete.
            //_lineEffect.CurrentTechnique.Passes[0].End();
            //_lineEffect.End();

        }

        // 10/16/2009
        /// <summary>
        /// Creates the visual WaypointPath line strip, which shows the connections between 
        /// the waypoints using red lines.  This should be called from the 'PropertiesTool' 
        /// form, via the ComboBox 'cbWaypointPathNames' selection for choosing a given <paramref name="waypointPathName"/>.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when <paramref name="waypointPathName"/> was not found, or is not valid.</exception>
        /// <param name="waypointPathName">WaypointPath Name</param>
        public static void CreateVisualPathLineStripForPathName(string waypointPathName)
        {
            // try get 'WaypointName' from Dictionary
            LinkedList<int> linkedList;
            if (WaypointPaths.TryGetValue(waypointPathName, out linkedList))
            {
                // Init arrays, if necessary.
                var linkedListCount = linkedList.Count; // Cache
                if (_visualPathPointList == null) _visualPathPointList = new VertexPositionColor[linkedListCount];
                if (_visualPathStripIndices == null) _visualPathStripIndices = new int[linkedListCount];

                // Resize arrays.
                if (_visualPathPointList.Length != linkedListCount) Array.Resize(ref _visualPathPointList, linkedListCount);
                if (_visualPathStripIndices.Length != linkedListCount) Array.Resize(ref _visualPathStripIndices, linkedListCount);

                // Populate the waypoint locations & Strip List indices
                var index = 0;
                foreach (var waypoint in linkedList)
                {
                    // Retrieve waypointStruct for given waypoint index.
                    var waypointItem = Waypoints[waypoint];

                    // Create Point in list
                    _visualPathPointList[index].Position = waypointItem.Location;
                    _visualPathPointList[index].Color = Color.Red;

                    // Update StripList Indices
                    _visualPathStripIndices[index] = index;

                    // Update index
                    index++;
                } // End ForEach

                return;
            }

            throw new ArgumentException(@"Waypoint PathName given does not exist!", "waypointPathName");
        }

        // 4/7/2010
        /// <summary>
        /// Searches all internal <see cref="WaypointPaths"/> for the given <paramref name="waypointIndex"/>, and
        /// removes when found.
        /// </summary>
        /// <param name="waypointIndex">Waypoint index to remove from all paths</param>
        public static void DeleteWaypointFromAllWaypointPaths(int waypointIndex)
        {
            // iterate through all name paths.
            foreach (var waypointPath in WaypointPaths)
            {
                // have LinkedList search & remove given index
                while (waypointPath.Value.Remove(waypointIndex))
                {
                    // While loops until ALL waypoint indexes are removed,
                    // which is known when 'FALSE' is returned.
                }

            } // End ForEach WaypointPaths

        }

        // 6/24/2012
        /// <summary>
        /// Creates a new waypointPath with the given <paramref name="waypointPathName"/>. (Scripting Purposes)
        /// </summary>
        /// <param name="waypointPathName">Waypoint path's name.</param>
        /// <param name="waypoints">params of waypoint indexes.</param>
        /// <remarks>
        /// All waypoint index values given MUST be valid waypoints; otherwise, an exception will be thrown.
        /// </remarks>
        public static void CreateWaypointPath(string waypointPathName, params int[] waypoints)
        {
            // verify name is not null and unique
            ValidateWaypointsPathName(waypointPathName);

            // create new entry into dictionary
            CreateEmptyWaypointPathInDictionary(waypointPathName);
            var linkedList = WaypointPaths[waypointPathName];

            // iterate given array
            var length = waypoints.Length;
            for (var index = 0; index < length; index++)
            {
                var waypointIndex = waypoints[index];

                // validate waypoint exist
                DoesWaypointExist(waypointIndex);

                // Add item to the LinkedList
                linkedList.AddLast(waypointIndex);
            }

            // Store updated LinkList back to Dictionary
            WaypointPaths[waypointPathName] = linkedList;
        }

        // 6/24/2012
        /// <summary>
        /// Validates the given <paramref name="waypointPathName"/> is unique.
        /// </summary>
        /// <param name="waypointPathName">New waypointPath name to validate.</param>
        public static void ValidateWaypointsPathName(string waypointPathName)
        {
            // if Null string, return.
            if (string.IsNullOrEmpty(waypointPathName))
                throw new ArgumentNullException(waypointPathName);

            // check if 'WaypointPathName' exist in the WaypointPaths Dictionary
            if (WaypointPaths.ContainsKey(waypointPathName))
            {
                throw new InvalidOperationException("Name given must be unique!");
            }
        }

        // 6/24/2012
        /// <summary>
        /// Checks if the given waypoint exist and throws exception if it does not.
        /// </summary>
        /// <param name="waypointIndex">Index key for waypoint.</param>
        public static void DoesWaypointExist(int waypointIndex)
        {
            // check if index exist.
            if (!Waypoints.ContainsKey(waypointIndex))
            {
                throw new ArgumentOutOfRangeException("waypointIndex", "Waypoint index given does not exist.");
            }
        }

        // 6/24/2012
        /// <summary>
        /// Creates the required LinkedList{int} in the internal <see cref="WaypointPaths"/> dictionary, with
        /// the given <paramref name="waypointPathName"/>.
        /// </summary>
        /// <param name="waypointPathName">Waypoint path's name.</param>
        public static void CreateEmptyWaypointPathInDictionary(string waypointPathName)
        {
            // verify name is not null and unique
            ValidateWaypointsPathName(waypointPathName);

            // Store new 'WaypointPathName' into WaypointPaths Dictionary
            var linkedList = new LinkedList<int>();
            WaypointPaths.Add(waypointPathName, linkedList);
        }

#if !XBOX360

        // 4/7/2010
        /// <summary>
        /// Adds the given 'SelectedItems' to the internal WaypointsPath linkedList, and
        /// updates the lines strip visual path to match.
        /// </summary>
        /// <param name="propertiesTools"><see cref="PropertiesTools"/> instance</param>
        /// <param name="waypointPathName">WaypointPath name to update</param>
        /// <param name="selectedItems">New items to add to list</param>
        /// <param name="lvWaypointsPaths"><see cref="PropertiesTools"/> Form's visual <see cref="ListView.SelectedListViewItemCollection"/>, 
        /// which displays the waypoints for user.</param>
        public static void UpdateVisualPath_AddSelectedItems(PropertiesTools propertiesTools, string waypointPathName,
                                                      ListView.SelectedListViewItemCollection selectedItems, ListView lvWaypointsPaths)
        {
            var linkedList = WaypointPaths[waypointPathName];

            // iterate selectedItems, and copy into WaypointsPaths
            var count = selectedItems.Count; // 5/19/2010 - Cache
            for (var i = 0; i < count; i++)
            {
                // 4/7/2010 - Retrieve ListViewItem
                var selectedItem = selectedItems[i];
                if (selectedItem == null) continue; // 5/19/2010

                // get waypoint index
                var waypointIndex = GetWaypointIndex(selectedItem);

                // Add item to the LinkedList
                linkedList.AddLast(waypointIndex);

                // add item to list
                lvWaypointsPaths.Items.Add("Waypoint " + waypointIndex);
            }

            // Store updated LinkList back to Dictionary
            WaypointPaths[waypointPathName] = linkedList;

            // 10/16/2009 - Update the Visual Path StripList
            CreateVisualPathLineStripForPathName(waypointPathName);
        }

        // 4/7/2010
        /// <summary>
        /// Deletes all selectedItems, in the <see cref="ListView"/> of WaypointsPaths, from the internal 
        /// <see cref="WaypointPaths"/> <see cref="LinkedList{T}"/>, and updates the lines strip visual path to match.
        /// </summary>
        /// <param name="propertiesTools"><see cref="PropertiesTools"/> instance</param>
        /// <param name="waypointPathName">WaypointPath name to update</param>
        /// <param name="lvWaypointsPaths"><see cref="PropertiesTools"/> Form's visual <see cref="ListView"/>, which displays the waypoints for user.</param>
        public static void UpdateVisualPath_DeleteSelectedItems(PropertiesTools propertiesTools, string waypointPathName, ListView lvWaypointsPaths)
        {
            var count = lvWaypointsPaths.SelectedItems.Count;

            // iterate the SelectedItems array, and delete all from the list.
            for (var i = 0; i < count; i++)
            {
                // get item from list
                var waypointItem = lvWaypointsPaths.SelectedItems[i];
                if (waypointItem == null) continue; // 5/19/2010

                // get waypointIndex
                var waypointIndex = GetWaypointIndex(waypointItem);

                // remove from ListView
                lvWaypointsPaths.Items.Remove(waypointItem);

                // remove from LinkedList in Dictionary
                RemoveWaypointFromWaypointPath(waypointPathName, waypointIndex);

            } // End For Loop

            // 4/7/2010 - Update the Visual Path StripList
            CreateVisualPathLineStripForPathName(waypointPathName);
        }

        // 4/7/2010
        /// <summary>
        /// Helper method, which extracts the index from the text of the waypoint in the <see cref="ListView"/>, for
        /// the given <paramref name="selectedItem"/> position.
        /// </summary>
        /// <param name="selectedItem"><see cref="ListViewItem"/> instance</param>
        /// <returns>Returns the waypoint index as integer</returns>
        private static int GetWaypointIndex(ListViewItem selectedItem)
        {
            return Convert.ToInt32(selectedItem.Text.TrimStart("Waypoint ".ToCharArray()));
        }
       
        // 10/14/2009
        /// <summary>
        /// When <see cref="PropertiesTools"/> window is open, checks if cursor is
        /// within an existing waypoint rectangle.
        /// </summary>
        private static void DoPropertiesToolCheck()
        {
            // make sure not in the control itself; otherwise skip check
            var propertiesTools = TerrainEditRoutines.PropertiesTools; // 11/21/2009
            if (propertiesTools == null || propertiesTools.IsMouseInControl())
                return;

            // 6/2/2012 - Check if Index tab 3, for "Waypoints"
            if (!propertiesTools.IsTabIndexActive(3))
                return;

            // 4/12/2010 - Check for Waypoint dragMove.
            Vector3 newPosition;
            if (_dragMoveWaypointKey != -1 && HandleInput.CheckForItemDragMove(ref _dragMoveStarted, out newPosition))
                UpdateWaypoint(_dragMoveWaypointKey, ref newPosition);
            else
                _dragMoveWaypointKey = -1;
           
            // 4/12/2010 - Check for a waypoint selection
            CheckForWaypointSelection();

            // 4/12/2010 - Check for Start of DragMove operation.
            if (HandleInput.DoDragMoveCheck(ref _dragMoveStarted))
                _dragMoveWaypointKey = _selectedWaypointKey;
            
        }

        // 4/12/2010
        /// <summary>
        /// Checks for a current waypoint selection, which is caused by the
        /// user selecting with a left-click on screen.
        /// </summary>
        private static void CheckForWaypointSelection()
        {
            Vector3 cursorPosition;
            TerrainPickingRoutines.GetCursorPosByPickedRay(PickRayScale.NoChange, out cursorPosition);

            // 4/12/2010 - Reset key; 4/13/2010 - Updated to ONLY clear when 'DragMoveStarted' is false.
            if (!_dragMoveStarted) _selectedWaypointKey = -1;

            // do check to see which Waypoints the cursor is in.
            // ** Normally, would not use the ForEach construct, since this causes garbage due to enumeration; however
            //    since this is only used during editing and not during critical game play, it will be allowed - Ben.
            foreach (var waypoint in Waypoints)
            {
                // Create Intersection Rectangle, using CursorPosition
                var intersecRect = new Rectangle((int)cursorPosition.X, (int)cursorPosition.Z, 15, 15);
                
                // If contains cursor or intersects with cursor rectangle, then make Red, else Orange.
                if (waypoint.Value.RectangleArea.Contains((int)cursorPosition.X, (int)cursorPosition.Z) ||
                    waypoint.Value.RectangleArea.Intersects(intersecRect))
                {
                    // Update color
                    for (var i = 0; i < waypoint.Value.VisualRectangleArea.Length; i++)
                        waypoint.Value.VisualRectangleArea[i].Color = Color.Red;

                    // Check if user pressed left mouse button, to select.
                    if (HandleInput.InputState.LeftMouseButton)
                    {
                        // then show selection in ListView of Areas tab in 'PropetiesTool' form.
                        TerrainEditRoutines.PropertiesTools.SelectWaypointsInListView(waypoint.Key);

                        // 4/12/2010 - Store selected key
                        _selectedWaypointKey = waypoint.Key;
                    }
                }
                else
                {
                    // Update color
                    for (var i = 0; i < waypoint.Value.VisualRectangleArea.Length; i++)
                        waypoint.Value.VisualRectangleArea[i].Color = VisualWaypointColor;
                }
            } // End ForEach
        }

#endif

        // 10/13/2009
        /// <summary>
        /// Add a new waypoint to the internal dictionary. (Scripting Purposes)
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when given waypoint already exist in dictionary.</exception>
        /// <param name="waypointLocation"><see cref="Vector3"/> location</param>
        /// <returns>Waypoint number</returns>
        public static int AddWaypoint(ref Vector3 waypointLocation)
        {
            // Increment the counter
            _waypointsCounter++;

            // Add new Dictionary Waypoint entry
            if (!Waypoints.ContainsKey(_waypointsCounter))
            {
                // Create Visual Waypoint
                VertexPositionColor[] visualRectangle;
                CreateVisualWaypoint(waypointLocation, VisualWaypointColor, out visualRectangle);

                // Create Waypoint Struct
                var waypoint = new WaypointStruct
                                   {
                                       Location = waypointLocation,
                                       RectangleArea = _rectangleArea, // which was updated in 'CreateVisualWaypoint'
                                       VisualRectangleArea = visualRectangle,
                                   };

                // Add new record
                Waypoints.Add(_waypointsCounter, waypoint);

                // return index of new waypoint.
                return _waypointsCounter;
            }

            throw new InvalidOperationException("Scripting Waypoints Dictionary already contains this entry!");
        }

        // 4/12/2010
        ///<summary>
        /// Updates an existing waypoint's location, and updates the 'VisualRectangle' based on
        /// the new location.
        ///</summary>
        /// <exception cref="KeyNotFoundException">Thrown when the given <paramref name="waypointKey"/> is not valid.</exception>
        ///<param name="waypointKey">Waypoint number (Key in Dictionary)</param>
        ///<param name="waypointLocation">New waypoint location</param>
        public static void UpdateWaypoint(int waypointKey, ref Vector3 waypointLocation)
        {
            // Retrieve existing waypoint
            WaypointStruct waypointStruct;
            if (Waypoints.TryGetValue(waypointKey, out waypointStruct))
            {
                // Create Visual Waypoint
                VertexPositionColor[] visualRectangle;
                CreateVisualWaypoint(waypointLocation, VisualWaypointColor, out visualRectangle);

                // Update existing waypoint struct
                waypointStruct.Location = waypointLocation;
                waypointStruct.RectangleArea = _rectangleArea; // which was updated in 'CreateVisualWaypoint'
                waypointStruct.VisualRectangleArea = visualRectangle;

                // Store back updated struct
                Waypoints[waypointKey] = waypointStruct;
                return;
            }

            throw new KeyNotFoundException(@"UpdateWaypoint method failed, because waypoint Key given is not valid."); 
        }
      

        // 10/13/2009
        /// <summary>
        /// Searches for the given waypoint in the internal dictionary, and
        /// returns the <see cref="Vector3"/> location via the (OUT) param.
        /// </summary>
        /// <param name="waypointIndex">Waypoint index</param>
        /// <param name="waypointLocation">(OUT) <see cref="Vector3"/> waypoint location</param>
        /// <returns>True/False of result</returns>
        public static bool GetExistingWaypoint(int waypointIndex, out Vector3 waypointLocation)
        {
            // Search for given waypointIndex in Dictionary
            WaypointStruct waypoint;
            if (Waypoints.TryGetValue(waypointIndex, out waypoint))
            {
                waypointLocation = waypoint.Location;

                // 10/20/2009 - get correct height for given location
                waypointLocation.Y = TerrainData.GetTerrainHeight(waypointLocation.X, waypointLocation.Z);

                return true;
            }

            waypointLocation = Vector3.Zero;
            return false;
        }

        // 10/15/2009
        /// <summary>
        /// Reorders the given <see cref="WaypointPaths"/> <see cref="LinkedList{T}"/>, by moving <paramref name="waypointIndexToMove"/> node 
        /// to be in front of the <paramref name="waypointIndexToTarget"/> node specified.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when <paramref name="waypointPathName"/> was not found, or is not valid.</exception>
        /// <param name="waypointPathName"><see cref="WaypointPaths"/> name to affect</param>
        /// <param name="waypointIndexToMove">Waypoint within <see cref="LinkedList{T}"/> to move</param>
        /// <param name="waypointIndexToTarget">Waypoint 'target' node to move the 'source' node before</param>
        public static void ReorderPathWaypointToBeBefore(string waypointPathName, int waypointIndexToMove, int waypointIndexToTarget)
        {
            // try get 'WaypointName' from Dictionary
            LinkedList<int> linkedList;
            if (WaypointPaths.TryGetValue(waypointPathName, out linkedList))
            {
                // 1st - get LinkedNode to 'Move'.
                var linkedNodeToMove = linkedList.Find(waypointIndexToMove);

                // 2nd - get LinkedNode to 'Target' move to (Before)
                var linkedNodeToTarget = linkedList.Find(waypointIndexToTarget);

                // Check if either nodes are NULL
                if (linkedNodeToMove == null || linkedNodeToTarget == null)
                    return;

                // 3rd - Do Reorder in LinkedList
                linkedList.Remove(linkedNodeToMove);
                linkedList.AddBefore(linkedNodeToTarget, linkedNodeToMove);

                return;
            }

            throw new ArgumentException(@"Waypoint PathName given does not exist!", "waypointPathName");
        }

        // 10/15/2009
        /// <summary>
        /// Reorders the given <see cref="WaypointPaths"/> <see cref="LinkedList{T}"/>, by moving <paramref name="waypointIndexToMove"/> node 
        /// to be after the <paramref name="waypointIndexToTarget"/> node specified.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when <paramref name="waypointPathName"/> was not found, or is not valid.</exception>
        /// <param name="waypointPathName"><see cref="WaypointPaths"/> name to affect</param>
        /// <param name="waypointIndexToMove">Waypoint within <see cref="LinkedList{T}"/> to move</param>
        /// <param name="waypointIndexToTarget">Waypoint 'target' node to move the 'source' node after</param>
        public static void ReorderPathWaypointToBeAfter(string waypointPathName, int waypointIndexToMove, int waypointIndexToTarget)
        {
            // try get 'WaypointName' from Dictionary
            LinkedList<int> linkedList;
            if (WaypointPaths.TryGetValue(waypointPathName, out linkedList))
            {
                // 1st - get LinkedNode to 'Move'.
                var linkedNodeToMove = linkedList.Find(waypointIndexToMove);

                // 2nd - get LinkedNode to 'Target' move to (After)
                var linkedNodeToTarget = linkedList.Find(waypointIndexToTarget);

                // Check if either nodes are NULL
                if (linkedNodeToMove == null || linkedNodeToTarget == null)
                    return;

                // 3rd - Do Reorder in LinkedList
                linkedList.Remove(linkedNodeToMove);
                linkedList.AddAfter(linkedNodeToTarget, linkedNodeToMove);

                return;
            }

            throw new ArgumentException(@"Waypoint PathName given does not exist!", "waypointPathName");
        }

        // 10/15/2009
        /// <summary>
        /// Removes a waypoint index from the given <see cref="WaypointPaths"/> <see cref="LinkedList{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when <paramref name="waypointPathName"/> was not found, or is not valid.</exception>
        /// <param name="waypointPathName"><see cref="WaypointPaths"/> name to affect</param>
        /// <param name="waypointIndexToRemove">Waypoint to remove</param>
        public static void RemoveWaypointFromWaypointPath(string waypointPathName, int waypointIndexToRemove)
        {
            // try get 'WaypointName' from Dictionary
            LinkedList<int> linkedList;
            if (WaypointPaths.TryGetValue(waypointPathName, out linkedList))
            {
                // Remove item from LinkedList
                linkedList.Remove(waypointIndexToRemove);

                return;
            }

            throw new ArgumentException(@"Waypoint PathName given does not exist!", "waypointPathName");
        }
       
#if !XBOX360
        // 10/14/2009; 10/16/2009: Updated to save 'WaypointPaths'.
        /// <summary>
        /// Saves the internal <see cref="Waypoints"/> as a collection of <see cref="WaypointsSaveStruct"/>, and
        /// the <see cref="WaypointPaths"/> as a collection of <see cref="WaypointPathsSaveStruct"/>. 
        /// </summary>
        /// <remarks>This should be called from the 'TerrainStorageRoutine' class.</remarks>
        /// <exception cref="InvalidOperationException">Thrown when save operation fails.</exception>
        /// <param name="storageTool"><see cref="Storage"/> instance</param>
        /// <param name="mapName">Map name</param>
        /// <param name="mapType">Map type; either SP or MP.</param>
        public static void SaveWaypoints(Storage storageTool, string mapName, string mapType)
        {
            // iterate Waypoints dictionary and transfer to list for saving
            var tmpWaypoints = Waypoints.Select(waypoint => waypoint.Value.Location).ToList(); // 5/19/2010 - Convert to Linq

            // iterate WaypointPaths dictionary and transfer for list for saving
            var tmpWaypointPaths = new List<WaypointPathsSaveStruct>();
// ReSharper disable LoopCanBeConvertedToQuery
            foreach (var path in WaypointPaths)
// ReSharper restore LoopCanBeConvertedToQuery
            {
                // Store LinkedList into regular List.
                var tmpWaypointLinkedList = path.Value.ToList(); // 5/19/2010 - Convert to Linq

                // store the Name & LinkedList
                var waypointPathSaveItem = new WaypointPathsSaveStruct { PathName = path.Key, PathConnections = tmpWaypointLinkedList };

                // add to List
                tmpWaypointPaths.Add(waypointPathSaveItem);
            }

            // create final WaypointSaveStruct.
            var waypointSaveStruct = new WaypointsSaveStruct {Waypoints = tmpWaypoints, WaypointPaths = tmpWaypointPaths};

            // 4/7/2010: Updated to use 'ContentMapsLoc' global var.
            int errorCode;
            if (storageTool.StartSaveOperation(waypointSaveStruct, "tdWaypoints.twd",
                                               String.Format(@"{0}\{1}\{2}\", TemporalWars3DEngine.ContentMapsLoc,
                                                             mapType, mapName), out errorCode)) return;
            // 4/7/2010 - Error occured, so check which one.
            if (errorCode == 1)
            {
                MessageBox.Show(@"Locked files detected for 'Waypoint' (tdWaypoints.twd) save.  Unlock files, and try again.",
                                @"Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;                
            }

            if (errorCode == 2)
            {
                MessageBox.Show(@"Directory location for 'Waypoint' (tdWaypoints.twd) save, not found.  Verify directory exist, and try again.",
                                @"Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // if errorCode 3, unknown, then just Throw exception.
            throw new InvalidOperationException("The Save Struct Waypoints Operation Failed.");
        }
#endif

        // 10/14/2009; // 10/16/2009: Updated to Load 'WaypointsPaths'.
        /// <summary>
        /// Loads the internal <see cref="Waypoints"/> and <see cref="WaypointPaths"/>, as collections
        /// </summary>
        /// <remarks>This should be called from the 'TerrainStorageRoutine' class.</remarks>
        /// <param name="storageTool"><see cref="Storage"/> instance</param>
        /// <param name="mapName">Map name</param>
        /// <param name="mapType">Map type; either SP or MP.</param>
        public static void LoadWaypoints(Storage storageTool, string mapName, string mapType)
        {
            // make sure Waypoints dictionary is not null
            if (Waypoints == null)
                Waypoints = new Dictionary<int, WaypointStruct>();

            // 10/16/2009 - make sure WaypointsPaths dictionary is not null
            if (WaypointPaths == null)
                WaypointPaths = new Dictionary<string, LinkedList<int>>();

            // 4/7/2010: Updated to use 'ContentMapsLoc' global var.
            // Load WaypointsSaveStruct Struct data
            WaypointsSaveStruct tmpWaypointSaveStruct;
            if (!storageTool.StartLoadOperation(out tmpWaypointSaveStruct, "tdWaypoints.twd", String.Format(@"{0}\{1}\{2}\", TemporalWars3DEngine.ContentMapsLoc, mapType, mapName),
                                                StorageLocation.TitleStorage))
            {
#if DEBUG
                Debug.WriteLine("LoadWaypoints method, of TerrainWaypoints, failed to load 'tdWaypoints.twd' file.");
#endif
                return; // Waypoints are not required, so just return if failed.
            }

            // 1st - repopulate the Waypoints dictionary with given list
            Waypoints.Clear();
            _waypointsCounter = 0;
            var waypointsCount = tmpWaypointSaveStruct.Waypoints.Count; // cache
            for (var i = 0; i < waypointsCount; i++)
            {
                // Retrieve location
                var waypointLocation = tmpWaypointSaveStruct.Waypoints[i];

                // Create Visual Waypoint
                VertexPositionColor[] visualRectangle;
                CreateVisualWaypoint(waypointLocation, VisualWaypointColor, out visualRectangle);

                // Create Waypoint Struct
                var waypoint = new WaypointStruct
                {
                    Location = waypointLocation,
                    RectangleArea = _rectangleArea,
                    VisualRectangleArea = visualRectangle,
                };

                // Increment the counter
                _waypointsCounter++;

                // Add new record
                Waypoints.Add(_waypointsCounter, waypoint);

            } // End For Loop

            // 10/16/2009 - 2nd - repopulate the WaypointPaths dictionary
            WaypointPaths.Clear(); // 10/27/2009
            var waypointPathsStructCount = tmpWaypointSaveStruct.WaypointPaths.Count; // cache
            for (var i = 0; i < waypointPathsStructCount; i++)
            {
                // cache item
                var waypointPathItem = tmpWaypointSaveStruct.WaypointPaths[i];

                // Recreate the LinkedList from List of ints.
                var linkedList = new LinkedList<int>();
                var count = waypointPathItem.PathConnections.Count;
                for (var j = 0; j < count; j++)
                {
                    linkedList.AddLast(waypointPathItem.PathConnections[j]);
                }

                // create entry in WaypointPaths Dictionary
                WaypointPaths.Add(waypointPathItem.PathName, linkedList);

            } // End For Loop
        }

        // 10/14/2009
        /// <summary>
        /// Given <paramref name="sceneItem"/> will turn to face the given <paramref name="waypointIndex"/> position. 
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="waypointIndex"/> was not found, or is not valid.</exception>
        /// <param name="sceneItem"><see cref="SceneItemWithPick"/> to affect</param>
        /// <param name="waypointIndex">Waypoint index; for example 3 is for Waypoint#3.</param>
        public static void SceneItemBeginFacingWaypoint(SceneItemWithPick sceneItem, int waypointIndex)
        {
            // Try get waypointIndex from Dictionary
            WaypointStruct waypointItem;
            if (Waypoints.TryGetValue(waypointIndex, out waypointItem))
            {
                // Tell sceneItem to Face given position.
                sceneItem.FaceWaypointPosition(ref waypointItem.Location);

                return;
            }

            throw new InvalidOperationException("The given Waypoint Index is not valid!");
        }

        // 1/8/2010
        /// <summary>
        /// Dispose of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this final dispose?</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;

            // Dispose
            if (_lineEffect != null)
                _lineEffect.Dispose();
            if (_lineVertexDeclaration != null)
                _lineVertexDeclaration.Dispose();

            // Clear Arrays
            if (_visualPathPointList != null) 
                Array.Clear(_visualPathPointList, 0, _visualPathPointList.Length);
            if (_visualPathStripIndices != null)
                Array.Clear(_visualPathStripIndices, 0, _visualPathStripIndices.Length);

            // Clear Dictionaries
            if (Waypoints != null) Waypoints.Clear();
            if (WaypointPaths != null) WaypointPaths.Clear();
        }
    }
}