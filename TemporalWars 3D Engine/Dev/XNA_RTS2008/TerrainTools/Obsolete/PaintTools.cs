using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TWEngine.BeginGame.Enums;
using TWEngine.GameScreens;
using TWEngine.Terrain;
using TWEngine.Terrain.Enums;
using TWEngine.TerrainTools;
using TWEngine.TerrainTools.Enums;
using TWEngine.Utilities;
using ButtonState=Microsoft.Xna.Framework.Input.ButtonState;
using Color=System.Drawing.Color;
using Keys=System.Windows.Forms.Keys;
#if !Xbox360
#endif

namespace TWEngine
{
    internal partial class PaintTools : Form
    {
        // 7/22/2008 - Add ITerrainShape Interface
        //private const int TextureSize = 128;
        private readonly ITerrainShape _terrainShape;

        // 1/2/2010 - Ref for Minimap Interface
        private readonly IMinimap _miniMap;

        private bool _bEditing;
        public PaintTool CurrentTool = PaintTool.Select;
        private Vector3 _cursorPos;

        private Vector2 _lastMousePos;

        private Point _mousePoint;
        private MouseState _mouseState;
        //private System.Drawing.Point screenPoint = new System.Drawing.Point();
        private Rectangle _rectangle;

        // 11/20/2009 - PerlinNoise data
        private List<float> _noiseData;

        public PaintTools(Game game)
        {
            try // 6/22/2010
            {
                InitializeComponent();

                // 11/19/2009 - Need to turn of FOW, otherwise, blinking will occur.
                var fogOfWar = (IFogOfWar)game.Services.GetService(typeof(IFogOfWar));
                if (fogOfWar != null) fogOfWar.IsVisible = false;

                // 1/2/2010 - Get Minimap Ref
                _miniMap = (IMinimap)game.Services.GetService(typeof(IMinimap));

                // Get TerrainShape Interface
                _terrainShape = (ITerrainShape)game.Services.GetService(typeof(ITerrainShape));

                // 3/3/2009 - Set in EditMode for TerrainShape
                TerrainShape.TerrainIsIn = TerrainIsIn.EditMode;

                // 1/21/2009 - Set the Loaded MapName.
                txtMapName.Text = TerrainStorageRoutines.LoadMapName;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PaintTools constructor threw the exception " + ex.Message ?? "No Message");
#endif   
            }
        }

        private void ResetToolSelection(PaintTool tool)
        {
            try // 6/22/2010
            {
                if (tool != PaintTool.Select)
                    checkBox1.Checked = false;
                if (tool != PaintTool.Fill)
                    checkBox2.Checked = false;
                if (tool != PaintTool.Unfill)
                    checkBox3.Checked = false;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("ResetToolSelection method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        private void SelectTool(PaintTool tool)
        {
            try // 6/22/2010
            {
                ResetToolSelection(tool);

                switch (tool)
                {
                    case PaintTool.Select:
                        CurrentTool = PaintTool.Select;
                        break;
                    case PaintTool.Fill:
                        CurrentTool = PaintTool.Fill;
                        break;
                    case PaintTool.Unfill:
                        CurrentTool = PaintTool.Unfill;
                        break;
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("SelectTool method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        private void hScrollBarSize_Scroll(object sender, ScrollEventArgs e)
        {
            try // 6/22/2010
            {
                TerrainEditRoutines.PaintCursorSize = hScrollBarSize.Value;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("hScrollBarSize_Scroll method threw the exception " + ex.Message ?? "No Message");
#endif 
            }
        }

        private void hScrollBarIntensity_Scroll(object sender, ScrollEventArgs e)
        {
            try // 6/22/2010
            {
                TerrainEditRoutines.PaintCursorStrength = hScrollBarIntensity.Value;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("hScrollBarIntensity_Scroll method threw the exception " + ex.Message ?? "No Message");
#endif 
            }
        }

        // 6/22/2010
        private static void InitPaintToolSettings(PaintTools paintTools)
        {
            try // 6/22/2010
            {
                // Set AlphaMap Layer Percent Values
                var terrainShape = paintTools._terrainShape; // 6/22/2010
                paintTools.layer1Percent.Value = (decimal)terrainShape.AlphaMaps.AlphaLy1Percent * 100;
                paintTools.layer2Percent.Value = (decimal)terrainShape.AlphaMaps.AlphaLy2Percent * 100;
                paintTools.layer3Percent.Value = (decimal)terrainShape.AlphaMaps.AlphaLy3Percent * 100;
                paintTools.layer4Percent.Value = (decimal)terrainShape.AlphaMaps.AlphaLy4Percent * 100;

                // 5/8/2009 - Set Ambient/Specular Atttributes for Layer 1/2
                {
                    var ambientColorLayer1 = terrainShape.AmbientColorLayer1;
                    paintTools.ambientColorLayer1_R.Value = (decimal)ambientColorLayer1.X;
                    paintTools.ambientColorLayer1_G.Value = (decimal)ambientColorLayer1.Y;
                    paintTools.ambientColorLayer1_B.Value = (decimal)ambientColorLayer1.Z;
                    paintTools.ambientPowerLayer1.Value = (decimal)terrainShape.AmbientPowerLayer1;

                    var specularColorLayer1 = terrainShape.SpecularColorLayer1;
                    paintTools.specularColorLayer1_R.Value = (decimal)specularColorLayer1.X;
                    paintTools.specularColorLayer1_G.Value = (decimal)specularColorLayer1.Y;
                    paintTools.specularColorLayer1_B.Value = (decimal)specularColorLayer1.Z;
                    paintTools.specularPowerLayer1.Value = (decimal)terrainShape.SpecularPowerLayer1;

                    var ambientColorLayer2 = terrainShape.AmbientColorLayer2;
                    paintTools.ambientColorLayer2_R.Value = (decimal)ambientColorLayer2.X;
                    paintTools.ambientColorLayer2_G.Value = (decimal)ambientColorLayer2.Y;
                    paintTools.ambientColorLayer2_B.Value = (decimal)ambientColorLayer2.Z;
                    paintTools.ambientPowerLayer2.Value = (decimal)terrainShape.AmbientPowerLayer2;

                    var specularColorLayer2 = terrainShape.SpecularColorLayer2;
                    paintTools.specularColorLayer2_R.Value = (decimal)specularColorLayer2.X;
                    paintTools.specularColorLayer2_G.Value = (decimal)specularColorLayer2.Y;
                    paintTools.specularColorLayer2_B.Value = (decimal)specularColorLayer2.Z;
                    paintTools.specularPowerLayer2.Value = (decimal)terrainShape.SpecularPowerLayer2;
                }

                // 5/15/2009 - Set PerlinNoiseData Attributes
                {
                    // Group-1 (Texture Mix 1 to 2)
                    paintTools.nudRandomSeedValue_g1.Value = TerrainShape.PerlinNoiseDataTexture1To2MixLayer1.seed;
                    paintTools.nudNoiseSize_g1.Value = (decimal)TerrainShape.PerlinNoiseDataTexture1To2MixLayer1.noiseSize;
                    paintTools.nudPersistence_g1.Value = (decimal)TerrainShape.PerlinNoiseDataTexture1To2MixLayer1.persistence;
                    paintTools.nudOctaves_g1.Value = TerrainShape.PerlinNoiseDataTexture1To2MixLayer1.octaves;

                    // Group-2 (Texture Mix 2 to 3)
                    paintTools.nudRandomSeedValue_g2.Value = TerrainShape.PerlinNoiseDataTexture1To2MixLayer2.seed;
                    paintTools.nudNoiseSize_g2.Value = (decimal)TerrainShape.PerlinNoiseDataTexture1To2MixLayer2.noiseSize;
                    paintTools.nudPersistence_g2.Value = (decimal)TerrainShape.PerlinNoiseDataTexture1To2MixLayer2.persistence;
                    paintTools.nudOctaves_g2.Value = TerrainShape.PerlinNoiseDataTexture1To2MixLayer2.octaves;
                }

                paintTools.ResetToolSelection(PaintTool.Select);
                paintTools.hScrollBarSize.Value = TerrainEditRoutines.PaintCursorSize;
                paintTools.hScrollBarIntensity.Value = TerrainEditRoutines.PaintCursorStrength;
                TerrainEditRoutines.ShowPaintCursor = true;

                // Populate the TreeView Groups with current Terrain Textures.
                paintTools.PopulateTreeViewGroups();

                // Start Timer Tick
                paintTools.timer1.Start();
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("InitPaintToolSettings method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }


        private void PaintTools_Load(object sender, EventArgs e)
        {
            InitPaintToolSettings(this);
        }


        // 5/7/2008 - Load Group 1 & 2 Textures into their Containers
        // 7/23/2008 - Made correction in how Nodes are populated; rather than
        //             clear all nodes out, now the Nodes are Inserted at the
        //             exact Index. - This keeps from losing empty positions.
        /// <summary>
        /// Populates the TreeView Groups Textures Nodes with the textures
        /// in memory from Terrain Class.
        /// </summary>
        private void PopulateTreeViewGroups()
        {
            try // 6/22/2010
            {
                // Group 1
                if (_terrainShape.TextureGroupData1.Count > 0)
                {
                    //treeViewGroup1.Nodes.Clear();
                    for (int i = 0; i < _terrainShape.TextureGroupData1.Count; i++)
                    {
                        var treeNode = new TreeNode { ImageKey = "Item1", Text = "SceneItemOwner" };
                        treeNode.ImageKey = _terrainShape.TextureGroupData1[i].imageKey;
                        treeNode.SelectedImageKey = _terrainShape.TextureGroupData1[i].selectedImageKey;

                        // Add Texture to container
                        //treeViewGroup1.Nodes.Add("Item1", "SceneItemOwner", TerrainShape.QuadTerrain.textureGroupData1[i].imageKey, TerrainShape.QuadTerrain.textureGroupData1[i].selectedImageKey);
                        treeViewGroup1.Nodes.RemoveAt(i);
                        treeViewGroup1.Nodes.Insert(i, treeNode);
                    }
                }
                // Group 2
                if (_terrainShape.TextureGroupData2.Count > 0)
                {
                    //treeViewGroup2.Nodes.Clear();
                    for (int i = 0; i < _terrainShape.TextureGroupData2.Count; i++)
                    {
                        var treeNode = new TreeNode { ImageKey = "Item1", Text = "SceneItemOwner" };
                        treeNode.ImageKey = _terrainShape.TextureGroupData2[i + 4].imageKey;
                        treeNode.SelectedImageKey = _terrainShape.TextureGroupData2[i + 4].selectedImageKey;

                        // Add Texture to container
                        //treeViewGroup2.Nodes.Add("Item1", "SceneItemOwner", TerrainShape.QuadTerrain.textureGroupData2[i + 4].imageKey, TerrainShape.QuadTerrain.textureGroupData2[i + 4].selectedImageKey);
                        treeViewGroup2.Nodes.RemoveAt(i);
                        treeViewGroup2.Nodes.Insert(i, treeNode);
                    }
                }
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PopulateTreeViewGroups method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }


        /// <summary>
        /// Updates <see cref="PaintTool"/> Enum to Select.
        /// </summary>        
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                if (checkBox1.Checked)
                    SelectTool(PaintTool.Select);
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("checkBox1_CheckedChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Updates <see cref="PaintTool"/> Enum to Fill.
        /// </summary>        
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                if (checkBox2.Checked)
                    SelectTool(PaintTool.Fill);
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("checkBox2_CheckedChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Updates <see cref="PaintTool"/> Enum to UnFill.
        /// </summary>        
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                if (checkBox3.Checked)
                    SelectTool(PaintTool.Unfill);
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("checkBox3_CheckedChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // Check User Selection in TreeView, and update the proper paintTexture
        // & AlphaGroup used for texturing on the Terrain.
        private void tvTextures_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try // 6/22/2010
            {
                // Show Larger Image in ListView
                listView1.Items.Clear();
                listView1.Items.Add("Texture", tvTextures.SelectedNode.SelectedImageKey);
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("tvTextures_AfterSelect method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // Updates QuadTerrain's AlphaLy1Percent
        private void layer1Percent_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                var spinner1 = (NumericUpDown)sender;

                _terrainShape.AlphaMaps.AlphaLy1Percent = (float)spinner1.Value / 100;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("layer1Percent_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // Updates QuadTerrain's AlphaLy2Percent
        private void layer2Percent_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var spinner2 = (NumericUpDown)sender;

                _terrainShape.AlphaMaps.AlphaLy2Percent = (float)spinner2.Value / 100;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("layer2Percent_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // Updates QuadTerrain's AlphaLy3Percent
        private void layer3Percent_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var spinner3 = (NumericUpDown)sender;

                _terrainShape.AlphaMaps.AlphaLy3Percent = (float)spinner3.Value / 100;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("layer2Percent_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // Updates QuadTerrain's AlphaLy4Percent
        private void layer4Percent_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var spinner4 = (NumericUpDown)sender;

                _terrainShape.AlphaMaps.AlphaLy4Percent = (float)spinner4.Value / 100;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("layer4Percent_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // Rebuilds the AlphaMap for Layer 1 using the new AlphaLy Percents.
        private void btnRebuildAlphaMap_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                TerrainAlphaMaps.UpdateAlphaMap1Layers();

                // 11/20/2009 - Update MiniMap Landscape.               
                _miniMap.RenderLandscapeForMiniMap();
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnRebuildAlphaMap_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Get Mouse Position Input Data
                _mouseState = Mouse.GetState();

                // 5/18/2010 - Refactored code out into new STATIC method.
                DoTimerUpdate(this);
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("timer1_Tick method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 5/18/2010
        /// <summary>
        /// Helper method, which checks for the mouse input, and allows doing the Paint fills.
        /// </summary>
        /// <param name="paintTools">instance of this <see cref="PaintTools"/></param>
        private static void DoTimerUpdate(PaintTools paintTools)
        {
            try // 6/22/2010
            {
                // If left mouse button click, AND ShowCursor=True && PaintTools form is On.
                if (paintTools._mouseState.LeftButton == ButtonState.Pressed
                    && TerrainEditRoutines.ToolInUse == ToolType.PaintTool && !paintTools.IsMouseInControl(paintTools._bEditing))
                {
                    paintTools._bEditing = true;

                    // Only update if mouse has moved!
                    if (paintTools._lastMousePos.X != paintTools._mouseState.X && paintTools._lastMousePos.Y != paintTools._mouseState.Y)
                    {
                        // Save Current Mouse Position
                        paintTools._lastMousePos.X = paintTools._mouseState.X;
                        paintTools._lastMousePos.Y = paintTools._mouseState.Y;

                        TerrainPickingRoutines.GetCursorPosByPickedRay(PickRayScale.DivideByTerrainScale, out paintTools._cursorPos);

                        // 5/18/2010 - Cache
                        const int pathNodeStride = TemporalWars3DEngine._pathNodeStride;
                        var mapWidthToScale = TerrainData.MapWidthToScale;
                        var mapHeightToScale = TerrainData.MapHeightToScale;
                        var paintCursorSizeXScale = TerrainEditRoutines.PaintCursorSize * TerrainData.cScale;
                        var aStarGraph = TemporalWars3DEngine.AStarGraph;

                        switch (paintTools.CurrentTool)
                        {
                            case PaintTool.Fill:

                                // 3/4/2009 - Check if Painting Path finding Block Sections.
                                if (paintTools.cbPaintPathfindingBlocks.Checked)
                                {
                                    // 5/18/2010 - Return if null
                                    if (aStarGraph == null) return;

                                    // Get Cursor Pos with NoChange
                                    TerrainPickingRoutines.GetCursorPosByPickedRay(PickRayScale.NoChange, out paintTools._cursorPos);

                                    var paintCursorWidth =
                                        (int)(paintTools._cursorPos.X < 0
                                                   ? 0
                                                   : paintTools._cursorPos.X > mapWidthToScale
                                                         ?
                                                             mapWidthToScale
                                                         : paintTools._cursorPos.X +
                                                           paintCursorSizeXScale);
                                    var paintCursorHeight =
                                        (int)(paintTools._cursorPos.Z < 0
                                                   ? 0
                                                   : paintTools._cursorPos.Z > mapHeightToScale
                                                         ?
                                                             mapHeightToScale
                                                         : paintTools._cursorPos.Z +
                                                           paintCursorSizeXScale);
                                    paintCursorWidth -= (int)paintTools._cursorPos.X;
                                    paintCursorHeight -= (int)paintTools._cursorPos.Z;

                                    // Update all Nodes within paint size rectangle, to the cost of (-1), which means "Blocked".
                                    for (var loopY = 0; loopY < paintCursorHeight; loopY += pathNodeStride)
                                        for (var loopX = 0; loopX < paintCursorWidth; loopX += pathNodeStride)
                                        {
                                            aStarGraph.SetCostToPos(
                                                (int)paintTools._cursorPos.X + loopX,
                                                (int)paintTools._cursorPos.Z + loopY, -1, 1);
                                        }

                                    // Call update to show change in path node blocking.
                                    TerrainShape.PopulatePathNodesArray();
                                }
                                else
                                {
                                    // Update AlphaMap using PickedTriangle X/Y Coords                            
                                    TerrainAlphaMaps.UpdateAlphaMap_Fill((int)paintTools._cursorPos.X, (int)paintTools._cursorPos.Z);
                                }

                                break;
                            case PaintTool.Unfill:

                                // 3/4/2009 - Check if Painting Path finding Block Sections.
                                if (paintTools.cbPaintPathfindingBlocks.Checked)
                                {
                                    // 5/18/2010 - Return if null
                                    if (aStarGraph == null) return;

                                    TerrainPickingRoutines.GetCursorPosByPickedRay(PickRayScale.NoChange, out paintTools._cursorPos);

                                    var paintCursorWidth =
                                        (int)(paintTools._cursorPos.X < 0
                                                   ? 0
                                                   : paintTools._cursorPos.X > mapWidthToScale
                                                         ?
                                                             mapWidthToScale
                                                         : paintTools._cursorPos.X +
                                                           paintCursorSizeXScale);
                                    var paintCursorHeight =
                                        (int)(paintTools._cursorPos.Z < 0
                                                   ? 0
                                                   : paintTools._cursorPos.Z > mapHeightToScale
                                                         ?
                                                             mapHeightToScale
                                                         : paintTools._cursorPos.Z +
                                                           paintCursorSizeXScale);
                                    paintCursorWidth -= (int)paintTools._cursorPos.X;
                                    paintCursorHeight -= (int)paintTools._cursorPos.Z;

                                    // Update all Nodes within paint size rectangle, to the cost of (-1), which means "Blocked".
                                    for (var loopY = 0; loopY < paintCursorHeight; loopY += pathNodeStride)
                                        for (var loopX = 0; loopX < paintCursorWidth; loopX += pathNodeStride)
                                        {
                                            // 1/13/2010
                                            aStarGraph.RemoveCostAtPos((int)paintTools._cursorPos.X + loopX, (int)paintTools._cursorPos.Z + loopY, 1);
                                        }

                                    // Call update to show change in path node blocking.
                                    TerrainShape.PopulatePathNodesArray();
                                }
                                else
                                {
                                    // Update AlphaMap using PickedTriangle X/Y Coords                        
                                    TerrainAlphaMaps.UpdateAlphaMap_UnFill((int)paintTools._cursorPos.X, (int)paintTools._cursorPos.Z);
                                }
                                break;
                            default:
                                break;
                        } // End Switch

                        // 9/15/2008 - Set the AlphaMaps Texture
                        TerrainAlphaMaps.SetAlphaMapsTextureEffect();
                    } // End If LastMousePos                     
                } // End If Left Button Pressed


                // If in editing mode, and release mouse button, then we turn off Editing
                if (paintTools._bEditing && paintTools._mouseState.LeftButton == ButtonState.Released)
                {
                    paintTools._bEditing = false;

                    //quadTerrain.RebuildNormals();
                }
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("DoTimerUpdate method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }


        // 5/7/2008
        // Helper Function which checks if a Point given is within this Window Form's Client Rectangle.        
        // The 'PointToScreen' method is used to convert the Forms' MousePoint to Screen coordinates.  Finally,
        // this is compared using a rectangle, created with this Windows location, and the rectangle's 'Contain'
        // method is called.
        private bool IsMouseInControl(bool isEditing)
        {
            try // 6/22/2010
            {
                // Only checks the first call from Tick when Left-Click was just pressed.
                if (!isEditing)
                {
                    _mousePoint.X = MousePosition.X;
                    _mousePoint.Y = MousePosition.Y;

                    // set this Form's ClientRectangle            
                    _rectangle.X = Location.X + 5;
                    _rectangle.Y = Location.Y + 5;
                    _rectangle.Width = Width - 5;
                    _rectangle.Height = Height - 5;

                    bool isIn;
                    _rectangle.Contains(ref _mousePoint, out isIn);

                    if (isIn) Debug.WriteLine("Trigger as mouse in control!");

                    return isIn;
                }
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("IsMouseInControl method threw the exception " + ex.Message ?? "No Message");
#endif
            }
            return false;
        }

        // note: for btnLayer1.
        // 5/7/2008 - Add a texture to the listViewGroup1 Container
        private void btnGroup1_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Check if a node is selected in TreeView
                if (tvTextures.SelectedNode != null)
                {
                    // 1st - Find Checked Node's Index
                    int index = 0;
                    for (int i = 0; i < treeViewGroup1.Nodes.Count; i++)
                    {
                        if (treeViewGroup1.Nodes[i].Checked)
                        {
                            index = i;
                            break; // found our Index
                        }
                    }

                    // 2nd - load the texture into memory
                    TerrainShape.TerrainTextures[index] =
                        TemporalWars3DEngine.ContentTextures.Load<Texture2D>(tvTextures.SelectedNode.Tag.ToString());
                    // 7/30/2008 - Also load the BumpMap Texture into memory
                    try
                    {
                        TerrainShape.TerrainTextureNormals[index] =
                            TemporalWars3DEngine.ContentTextures.Load<Texture2D>(tvTextures.SelectedNode.Tag + "Normal");
                    }
                    catch (ContentLoadException)
                    {
                        MessageBox.Show("No BumpMap available for this texture");
                    }

                    // 3rd - remove old texture from container
                    treeViewGroup1.Nodes.RemoveAt(index);

                    // 4th - Add new Texture to container
                    treeViewGroup1.Nodes.Insert(index, "Item1", "SceneItemOwner", tvTextures.SelectedNode.ImageKey,
                                                tvTextures.SelectedNode.SelectedImageKey);

                    // 5th - Store the information into List Array
                    _terrainShape.TextureGroupData_AddRecord(index, tvTextures.SelectedNode.ImageKey,
                                                             tvTextures.SelectedNode.SelectedImageKey
                                                             , tvTextures.SelectedNode.Tag.ToString(), 1);


                    // 7/23/2008
                    // 6th - UpdateEffectsTextures on Terrain
                    TerrainShape.UpdateEffectDiffuseTextures();
                }
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnGroup1_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 5/7/2008 - Add a texture to the listViewGroup2 Container
        private void btnGroup2_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Check if a node is selected in TreeView
                if (tvTextures.SelectedNode != null)
                {
                    // 1st - Find Checked Node's Index
                    int index = 0;
                    for (int i = 0; i < treeViewGroup2.Nodes.Count; i++)
                    {
                        if (treeViewGroup2.Nodes[i].Checked)
                        {
                            index = i;
                            break; // found our Index
                        }
                    }

                    // 2nd - load the texture into memory
                    TerrainShape.TerrainTextures[index + 4] =
                        TemporalWars3DEngine.ContentTextures.Load<Texture2D>(tvTextures.SelectedNode.Tag.ToString());

                    // 3rd - remove old texture from container
                    treeViewGroup2.Nodes.RemoveAt(index);

                    // 4th - Add new Texture to container
                    treeViewGroup2.Nodes.Insert(index, "Item1", "SceneItemOwner", tvTextures.SelectedNode.ImageKey,
                                                tvTextures.SelectedNode.SelectedImageKey);

                    // 5th - Store the information into List Array
                    _terrainShape.TextureGroupData_AddRecord(index, tvTextures.SelectedNode.ImageKey,
                                                             tvTextures.SelectedNode.SelectedImageKey
                                                             , tvTextures.SelectedNode.Tag.ToString(), 2);


                    // 7/23/2008
                    // 6th - UpdateEffectsTextures on Terrain
                    TerrainShape.UpdateEffectDiffuseTextures();
                }
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnGroup2_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 5/7/2008 - Deletes the given selected Node from TreeViewGroup2
        private void treeViewGroup2_KeyDown(object sender, KeyEventArgs e)
        {
            try // 6/22/2010
            {
                if (e.KeyCode == Keys.Delete && treeViewGroup2.SelectedNode != null)
                {
                    // 1st - Remove from container
                    treeViewGroup1.Nodes.Remove(treeViewGroup2.SelectedNode);

                    // 2nd - Remove from Array
                    _terrainShape.TextureGroupData2.Remove(treeViewGroup2.SelectedNode.Index + 4);
                }
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("treeViewGroup2_KeyDown method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 5/7/2008
        // 1/22/2009: Updated to use the new 'SetPaintTextureToUse' method.
        // Check User Selection in TreeView, and update the proper paintTexture
        // & AlphaGroup used for texturing on the Terrain.
        private void treeViewGroup1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try // 6/22/2010
            {
                // Show Larger Image in ListView
                listView1.Items.Clear();
                listView1.Items.Add("Texture", treeViewGroup1.SelectedNode.SelectedImageKey);

                switch (treeViewGroup1.SelectedNode.Index)
                {
                    case 0:
                        TerrainAlphaMaps.SetPaintTextureToUse(1, LayerGroup.Layer1);
                        break;
                    case 1:
                        TerrainAlphaMaps.SetPaintTextureToUse(2, LayerGroup.Layer1);
                        break;
                    case 2:
                        TerrainAlphaMaps.SetPaintTextureToUse(3, LayerGroup.Layer1);
                        break;
                    case 3:
                        TerrainAlphaMaps.SetPaintTextureToUse(4, LayerGroup.Layer1);
                        break;
                }
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("treeViewGroup1_AfterSelect method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 5/7/2008
        // 1/22/2009: Updated to use the new 'SetPaintTextureToUse' method.
        // Check User Selection in TreeView, and update the proper paintTexture
        // & AlphaGroup used for texturing on the Terrain.
        private void treeViewGroup2_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try // 6/22/2010
            {
                // Show Larger Image in ListView
                listView1.Items.Clear();
                listView1.Items.Add("Texture", treeViewGroup2.SelectedNode.SelectedImageKey);

                switch (treeViewGroup2.SelectedNode.Index)
                {
                    case 0:
                        TerrainAlphaMaps.SetPaintTextureToUse(1, LayerGroup.Layer2);
                        break;
                    case 1:
                        TerrainAlphaMaps.SetPaintTextureToUse(2, LayerGroup.Layer2);
                        break;
                    case 2:
                        TerrainAlphaMaps.SetPaintTextureToUse(3, LayerGroup.Layer2);
                        break;
                    case 3:
                        TerrainAlphaMaps.SetPaintTextureToUse(4, LayerGroup.Layer2);
                        break;
                }
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("treeViewGroup2_AfterSelect method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 1/20/2009
        /// <summary>
        /// Combines the individual textures chosen, into one 'Texture Atlas' texture.
        /// </summary>
        private void btnCreateTextureVol1_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var storageTool = new Storage();

                // 11/3/2009 - Set depending on texture quality
                int textureSize;
                switch (TemporalWars3DEngine.TerrainTexturesQuality)
                {
                    case TerrainTextures.Low128X:
                        textureSize = 128;
                        break;
                    case TerrainTextures.Med256X:
                        textureSize = 256;
                        break;
                    case TerrainTextures.High512X:
                        textureSize = 512;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                // 2/1/2010: MapName NOT required anymore.
                // Check if name was given, since it is required.
                /*if (string.IsNullOrEmpty(txtMapName.Text))
                {
                    lblError.Text = "You must enter the Map-name:";
                    txtMapName.BackColor = Color.Yellow;
                    return;
                }*/
                txtMapName.BackColor = Color.White;
                lblError.Text = string.Empty;

                // 1/21/2009 - Volume Texture creation; a stack of Texture2D are put into one Texture3D volume!
                {

                    int elementCount = textureSize * textureSize;
                    const int numberOfTextures = 4;
                    var volumeTexture = new Texture3D(TemporalWars3DEngine.GameInstance.GraphicsDevice, textureSize, textureSize,
                                                      numberOfTextures, 1, TextureUsage.None, SurfaceFormat.Color);
                    var tmpData = new Microsoft.Xna.Framework.Graphics.Color[elementCount * numberOfTextures];

                    TerrainShape.TerrainTextures[0].GetData(0, null, tmpData, 0, elementCount);
                    TerrainShape.TerrainTextures[1].GetData(0, null, tmpData, elementCount * 1, elementCount);
                    TerrainShape.TerrainTextures[2].GetData(0, null, tmpData, elementCount * 2, elementCount);
                    TerrainShape.TerrainTextures[3].GetData(0, null, tmpData, elementCount * 3, elementCount);
                    volumeTexture.SetData(tmpData);

                    // Layer 1
                    // Set into the Array within TerrainShape class.
                    TerrainShape.TerrainTextureVolumes[0] = volumeTexture;
                    TerrainShape.TerrainTextureVolumeNames[0] = /*txtMapName.Text +*/ "VL1"; // 2/1/2010: MapName NOT required anymore.

                    // 11/20/2009 - Get mapType; MP or SP.
                    var mapType = string.IsNullOrEmpty(TerrainScreen.TerrainMapGameType) ? "SP" : TerrainScreen.TerrainMapGameType;

                    // 4/9/2010 - Set VL texture name and sub-directory path.
                    var volumeTexName = "VL1" + TemporalWars3DEngine.TerrainTexturesQuality + ".dds";
                    var subDirPath = TemporalWars3DEngine.ContentMapsLoc + @"\" + mapType + @"\" + txtMapName.Text + @"\";

                    // 4/9/2010: Updated to use 'ContentMapsLoc' global var.
                    // 11/3/2009: Updated to add the 'TextureQuality' name to the file name.
                    // Save new Texture Atlas out to disk for use later in game.
                    int errorCode;
                    if (!storageTool.StartTextureSaveOperation(volumeTexture, volumeTexName, subDirPath, ImageFileFormat.Dds, out errorCode))
                    {
                        // 4/9/2010 - Error occurred, so check which one.
                        if (errorCode == 1)
                        {
                            MessageBox.Show("Locked files detected for '" + volumeTexName + "' save.  Unlock files, and try again.",
                                            "Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        if (errorCode == 2)
                        {
                            MessageBox.Show("Directory location for '" + volumeTexName + "' save, not found.  Verify directory exist, and try again.",
                                            "Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        MessageBox.Show("Invalid Operation error for '" + volumeTexName + "' save.  Check for file locks, and try again.",
                                       "Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

               
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnCreateTextureVol1_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
            finally // 6/22/2010 - Fixed: Put in Finally block to guarantee code is run, even when error thrown.
            {
                // Update Effect Textures
                TerrainShape.UpdateEffectDiffuseTextures();

                // 7/30/2008 - Update Bump map Textures
                TerrainShape.UpdateEffectBumpMapTextures();

                // Update MiniMap Landscape.               
                if (_miniMap != null) _miniMap.RenderLandscapeForMiniMap();

            }
        }

        // 1/20/2009
        /// <summary>
        /// Combines the individual textures chosen, into one 'Texture Atlas' texture.
        /// </summary>
        private void btnCreateTextureVol2_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var storageTool = new Storage();

                // 11/3/2009 - Set depending on texture quality
                int textureSize;
                switch (TemporalWars3DEngine.TerrainTexturesQuality)
                {
                    case TerrainTextures.Low128X:
                        textureSize = 128;
                        break;
                    case TerrainTextures.Med256X:
                        textureSize = 256;
                        break;
                    case TerrainTextures.High512X:
                        textureSize = 512;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                // 2/1/2010: MapName NOT required anymore.
                // Check if name was given, since it is required.
                /*if (string.IsNullOrEmpty(txtMapName.Text))
                {
                    lblError.Text = "You must enter the Map-name:";
                    txtMapName.BackColor = Color.Yellow;
                    return;
                }*/
                txtMapName.BackColor = Color.White;
                lblError.Text = string.Empty;

                // 1/21/2009 - Volume Texture creation; a stack of Texture2D are put into one Texture3D volume!
                {
                    int elementCount = textureSize * textureSize;
                    const int numberOfTextures = 4;
                    var volumeTexture = new Texture3D(TemporalWars3DEngine.GameInstance.GraphicsDevice, textureSize, textureSize,
                                                      numberOfTextures, 1, TextureUsage.None, SurfaceFormat.Color);
                    var tmpData = new Microsoft.Xna.Framework.Graphics.Color[elementCount * numberOfTextures];

                    TerrainShape.TerrainTextures[4].GetData(0, null, tmpData, 0, elementCount);
                    TerrainShape.TerrainTextures[5].GetData(0, null, tmpData, elementCount * 1, elementCount);
                    TerrainShape.TerrainTextures[6].GetData(0, null, tmpData, elementCount * 2, elementCount);
                    TerrainShape.TerrainTextures[7].GetData(0, null, tmpData, elementCount * 3, elementCount);
                    volumeTexture.SetData(tmpData);

                    // Layer 2
                    // Set into the Array within TerrainShape class.
                    TerrainShape.TerrainTextureVolumes[1] = volumeTexture;
                    TerrainShape.TerrainTextureVolumeNames[1] = /*txtMapName.Text +*/ "VL2"; // 2/1/2010: MapName NOT required anymore.

                    // 11/20/2009 - Get mapType; MP or SP.
                    var mapType = string.IsNullOrEmpty(TerrainScreen.TerrainMapGameType) ? "SP" : TerrainScreen.TerrainMapGameType;

                    // 4/9/2010 - Set VL texture name and sub-directory path.
                    var volumeTexName = "VL2" + TemporalWars3DEngine.TerrainTexturesQuality + ".dds";
                    var subDirPath = TemporalWars3DEngine.ContentMapsLoc + @"\" + mapType + @"\" + txtMapName.Text + @"\";

                    // 4/9/2010: Updated to use 'ContentMapsLoc' global var.
                    // 11/3/2009: Updated to add the 'TextureQuality' name to the file name.
                    // Save new Texture Atlas out to disk for use later in game.
                    int errorCode;
                    if (!storageTool.StartTextureSaveOperation(volumeTexture, volumeTexName, subDirPath, ImageFileFormat.Dds, out errorCode))
                    {
                        // 4/9/2010 - Error occured, so check which one.
                        if (errorCode == 1)
                        {
                            MessageBox.Show("Locked files detected for '" + volumeTexName + "' save.  Unlock files, and try again.",
                                            "Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        if (errorCode == 2)
                        {
                            MessageBox.Show("Directory location for '" + volumeTexName + "' save, not found.  Verify directory exist, and try again.",
                                            "Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        MessageBox.Show("Invalid Operation error for '" + volumeTexName + "' save.  Check for file locks, and try again.",
                                       "Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnCreateTextureVol2_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
            finally // 6/22/2010 - Fixed: Put in Finally block to guarantee code is run, even when error thrown.
            {
                // Update Effect Textures
                TerrainShape.UpdateEffectDiffuseTextures();

                // 7/30/2008 - Update Bump map Textures
                TerrainShape.UpdateEffectBumpMapTextures();

                // Update MiniMap Landscape.               
                if (_miniMap != null) _miniMap.RenderLandscapeForMiniMap();
            }

        }

        // 1/22/2009
        /// <summary>
        /// Sets the new interpolated 'blend' amount for painting.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hScrollBarBlend_Scroll(object sender, ScrollEventArgs e)
        {
            try // 6/22/2010
            {
            	TerrainAlphaMaps.SetTextureBlendToUse(e.NewValue);
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("hScrollBarBlend_Scroll method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 1/22/2009
        private void btnClearLayer1_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
            	TerrainAlphaMaps.ClearGivenLayer(LayerGroup.Layer1);
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnClearLayer1_Click method threw the exception " + ex.Message ?? "No Message");
#endif	
            }
        }

        // 1/22/2009
        private void btnClearLayer2_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
            	TerrainAlphaMaps.ClearGivenLayer(LayerGroup.Layer2);
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnClearLayer2_Click method threw the exception " + ex.Message ?? "No Message");
#endif	
            }
        }

        // 1/22/2009
        /// <summary>
        /// Sets the current Layer-1 texture to On or Off.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInUseLayer1_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Turn on or off?
                if (TerrainAlphaMaps.InUseLayer1)
                {
                    // Was on, so turn off now.
                    TerrainAlphaMaps.InUseLayer1 = false;

                    // Update button text
                    btnInUseLayer1.Text = "Not In Use";

                    // Update button color
                    btnInUseLayer1.ForeColor = Color.Black;
                }
                else
                {
                    // Was off, so turn on now.
                    TerrainAlphaMaps.InUseLayer1 = true;

                    // Update button text
                    btnInUseLayer1.Text = "In Use";

                    // Update button color
                    btnInUseLayer1.ForeColor = Color.Green;
                }
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnInUseLayer1_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 1/22/2009
        /// <summary>
        /// Sets the current Layer-2 texture to On or Off.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInUseLayer2_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Turn on or off?
                if (TerrainAlphaMaps.InUseLayer2)
                {
                    // Was on, so turn off now.
                    TerrainAlphaMaps.InUseLayer2 = false;

                    // Update button text
                    btnInUseLayer2.Text = "Not In Use";

                    // Update button color
                    btnInUseLayer2.ForeColor = Color.Black;
                }
                else
                {
                    // Was off, so turn on now.
                    TerrainAlphaMaps.InUseLayer2 = true;

                    // Update button text
                    btnInUseLayer2.Text = "In Use";

                    // Update button color
                    btnInUseLayer2.ForeColor = Color.Green;
                }
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnInUseLayer2_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        #region Ambient/Specular Attributes methods

        private void ambientColorLayer1_R_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var ambientColorLayer1 = _terrainShape.AmbientColorLayer1;
                ambientColorLayer1.X = (float)ambientColorLayer1_R.Value;
                _terrainShape.AmbientColorLayer1 = ambientColorLayer1;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("ambientColorLayer1_R_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        private void ambientColorLayer1_G_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var ambientColorLayer1 = _terrainShape.AmbientColorLayer1;
                ambientColorLayer1.Y = (float)ambientColorLayer1_G.Value;
                _terrainShape.AmbientColorLayer1 = ambientColorLayer1;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("ambientColorLayer1_G_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        private void ambientColorLayer1_B_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var ambientColorLayer1 = _terrainShape.AmbientColorLayer1;
                ambientColorLayer1.Z = (float)ambientColorLayer1_B.Value;
                _terrainShape.AmbientColorLayer1 = ambientColorLayer1;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("ambientColorLayer1_B_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        private void ambientPowerLayer1_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
            	_terrainShape.AmbientPowerLayer1 = (float) ambientPowerLayer1.Value;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("ambientPowerLayer1_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        private void specularColorLayer1_R_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var specularColorLayer1 = _terrainShape.SpecularColorLayer1;
                specularColorLayer1.X = (float)specularColorLayer1_R.Value;
                _terrainShape.SpecularColorLayer1 = specularColorLayer1;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("specularColorLayer1_R_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        private void specularColorLayer1_G_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var specularColorLayer1 = _terrainShape.SpecularColorLayer1;
                specularColorLayer1.Y = (float)specularColorLayer1_G.Value;
                _terrainShape.SpecularColorLayer1 = specularColorLayer1;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("specularColorLayer1_G_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        private void specularColorLayer1_B_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var specularColorLayer1 = _terrainShape.SpecularColorLayer1;
                specularColorLayer1.Z = (float)specularColorLayer1_B.Value;
                _terrainShape.SpecularColorLayer1 = specularColorLayer1;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("specularColorLayer1_B_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        private void specularPowerLayer1_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
            	_terrainShape.SpecularPowerLayer1 = (float) specularPowerLayer1.Value;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("specularPowerLayer1_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        private void ambientColorLayer2_R_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var ambientColorLayer2 = _terrainShape.AmbientColorLayer2;
                ambientColorLayer2.X = (float)ambientColorLayer2_R.Value;
                _terrainShape.AmbientColorLayer2 = ambientColorLayer2;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("ambientColorLayer2_R_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        private void ambientColorLayer2_G_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var ambientColorLayer2 = _terrainShape.AmbientColorLayer2;
                ambientColorLayer2.Y = (float)ambientColorLayer2_G.Value;
                _terrainShape.AmbientColorLayer2 = ambientColorLayer2;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("ambientColorLayer2_G_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        private void ambientColorLayer2_B_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var ambientColorLayer2 = _terrainShape.AmbientColorLayer2;
                ambientColorLayer2.Z = (float)ambientColorLayer2_B.Value;
                _terrainShape.AmbientColorLayer2 = ambientColorLayer2;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("ambientColorLayer2_B_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        private void ambientPowerLayer2_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
            	_terrainShape.AmbientPowerLayer2 = (float) ambientPowerLayer2.Value;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("ambientPowerLayer2_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        private void specularColorLayer2_R_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var specularColorLayer2 = _terrainShape.SpecularColorLayer2;
                specularColorLayer2.X = (float)specularColorLayer2_R.Value;
                _terrainShape.SpecularColorLayer2 = specularColorLayer2;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("specularColorLayer2_R_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        private void specularColorLayer2_G_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var specularColorLayer2 = _terrainShape.SpecularColorLayer2;
                specularColorLayer2.Y = (float)specularColorLayer2_G.Value;
                _terrainShape.SpecularColorLayer2 = specularColorLayer2;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("specularColorLayer2_G_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        private void specularColorLayer2_B_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var specularColorLayer2 = _terrainShape.SpecularColorLayer2;
                specularColorLayer2.Z = (float)specularColorLayer2_B.Value;
                _terrainShape.SpecularColorLayer2 = specularColorLayer2;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("specularColorLayer2_B_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        private void specularPowerLayer2_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
            	_terrainShape.SpecularPowerLayer2 = (float) specularPowerLayer2.Value;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("specularPowerLayer2_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        #endregion

        #region Perlin Noise Texture methods

        private void nudRandomSeedValue_g1_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
            	TerrainShape.PerlinNoiseDataTexture1To2MixLayer1.seed = (int) nudRandomSeedValue_g1.Value;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nudRandomSeedValue_g1_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        private void nudNoiseSize_g1_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
            	TerrainShape.PerlinNoiseDataTexture1To2MixLayer1.noiseSize = (float) nudNoiseSize_g1.Value;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nudNoiseSize_g1_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        private void nudPersistence_g1_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
            	TerrainShape.PerlinNoiseDataTexture1To2MixLayer1.persistence = (float) nudPersistence_g1.Value;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nudPersistence_g1_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        private void nudOctaves_g1_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
            	TerrainShape.PerlinNoiseDataTexture1To2MixLayer1.octaves = (int) nudOctaves_g1.Value;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nudOctaves_g1_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        private void nudRandomSeedValue_g2_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
            	TerrainShape.PerlinNoiseDataTexture1To2MixLayer2.seed = (int) nudRandomSeedValue_g2.Value;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nudRandomSeedValue_g2_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        private void nudNoiseSize_g2_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
            	TerrainShape.PerlinNoiseDataTexture1To2MixLayer2.noiseSize = (float) nudNoiseSize_g2.Value;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nudNoiseSize_g2_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        private void nudPersistence_g2_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
            	TerrainShape.PerlinNoiseDataTexture1To2MixLayer2.persistence = (float) nudPersistence_g2.Value;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nudPersistence_g2_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        private void nudOctaves_g2_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
            	TerrainShape.PerlinNoiseDataTexture1To2MixLayer2.octaves = (int) nudOctaves_g2.Value;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nudOctaves_g2_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }


        // 5/15/2009- 
        /// <summary>
        /// Generates Perlin Noise used to alpha map between textures 1 and 2, for 
        /// Layer 1.
        /// </summary>
        private void btnNoiseGenerator_g1_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Get Perlin Noise Params
                var seed = (int)nudRandomSeedValue_g1.Value;
                var noiseSize = (float)nudNoiseSize_g1.Value;
                var persistence = (float)nudPersistence_g1.Value;
                var octaves = (int)nudOctaves_g1.Value;

                _noiseData = TerrainData.CreatePerlinNoiseMap(seed, noiseSize, persistence, octaves);

                // Populate Color 'Bits', used to store into texture map
                // Set Image into PictureBox on Form.
                pictureBox.BackgroundImage = TerrainData.CreateBitmapFromPerlinNoise(_noiseData);
                pictureBox.Refresh(); // 12/20/2009
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnNoiseGenerator_g1_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Generates Perlin Noise used to alpha map between textures 1 and 2, for 
        /// Layer 2.
        /// </summary>
        private void btnNoiseGenerator_g2_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Get Perlin Noise Params
                var seed = (int)nudRandomSeedValue_g2.Value;
                var noiseSize = (float)nudNoiseSize_g2.Value;
                var persistence = (float)nudPersistence_g2.Value;
                var octaves = (int)nudOctaves_g2.Value;

                _noiseData = TerrainData.CreatePerlinNoiseMap(seed, noiseSize, persistence, octaves);

                // Populate Color 'Bits', used to store into texture map
                // Set Image into PictureBox on Form.
                pictureBox.BackgroundImage = TerrainData.CreateBitmapFromPerlinNoise(_noiseData);
                pictureBox.Refresh();// 12/20/2009
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnNoiseGenerator_g2_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 11/20/2009
        /// <summary>
        /// Applies the given NoiseData to the AlphaMaps Layer-1.
        /// </summary>
        private void btnApplyNoise_g1_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Check if noise data is null or zero Count
                if (_noiseData == null || _noiseData.Count == 0)
                {
                    txtFloodErrorMessages.Text =
                        "A Perlin Noise MUST be generated first, before the automatic Flood generator can be applied.";
                    return;
                }

                // Apply Perlin Noise to Texture 1-2 channels (Layer-1), which is done by passing
                // the topTexture value; 2 in this case.
                TerrainAlphaMaps.ApplyPerlinNoise(LayerGroup.Layer1, 2, _noiseData);

                // Update MiniMap Landscape.               
                if (_miniMap != null) _miniMap.RenderLandscapeForMiniMap();
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnApplyNoise_g1_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 11/20/2009
        /// <summary>
        /// Applies the given NoiseData to the AlphaMaps Layer-2.
        /// </summary>
        private void btnApplyNoise_g2_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Check if noise data is null or zero Count
                if (_noiseData == null || _noiseData.Count == 0)
                {
                    txtFloodErrorMessages.Text =
                        "A Perlin Noise MUST be generated first, before the automatic Flood generator can be applied.";
                    return;
                }

                // Apply Perlin Noise to Texture 1-2 channels (Layer-2), which is done by passing
                // the topTexture value; 2 in this case.
                TerrainAlphaMaps.ApplyPerlinNoise(LayerGroup.Layer2, 2, _noiseData);

                // Update MiniMap Landscape.               
                if (_miniMap != null) _miniMap.RenderLandscapeForMiniMap();
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnApplyNoise_g2_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        #endregion


        // 12/7/2009
        /// <summary>
        /// Captures the close event, and cancels the close action; instead, the form is simply hidden
        /// from view by setting the 'Visible' flag to False.  This fixes the issue of the HEAP crash 
        /// when trying to re-instantiate a form, after it was already created in the 'TerrainEditRoutines' class.
        /// </summary>
        private void PaintTools_FormClosing(object sender, FormClosingEventArgs e)
        {
            try // 6/22/2010
            {
                e.Cancel = true;
                Visible = false;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PaintTools_FormClosing method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        
    }
}