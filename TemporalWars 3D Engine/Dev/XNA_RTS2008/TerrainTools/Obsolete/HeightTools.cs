using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TWEngine.Terrain;
using TWEngine.Terrain.Enums;
using TWEngine.TerrainTools.Enums;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using System.Diagnostics;

namespace TWEngine.TerrainTools
{
    internal partial class HeightTools : Form
    {
        // 7/22/2008 - Add ITerrainShape Interface
        private readonly ITerrainShape _terrainShape;

        private bool _bEditing;
        private bool _bHeightSet;
        private readonly bool _bUseConstantFeet;
        public HeightTool CurrentTool = HeightTool.Select;

        private Vector2 _lastMousePos;

        private Point _mousePoint;
        private MouseState _mouseState;
        //private System.Drawing.Point screenPoint = new System.Drawing.Point();
        private Rectangle _rectangle;

        // 11/20/2009 - PerlinNoise data
        private List<float> _noiseData;

        // default constructor
        public HeightTools(Game game) : this (game, true)
        {
            
        }

        public HeightTools(Game game, bool bUseConstantFeet)
        {
            try // 6/22/2010
            {
                // Init Form Components           
                InitializeComponent();

                // 11/19/2009 - Need to turn of FOW, otherwise, blinking will occur.
                var fogOfWar = (IFogOfWar)game.Services.GetService(typeof(IFogOfWar));
                if (fogOfWar != null) fogOfWar.IsVisible = false;

                // Get TerrainShape Interface
                _terrainShape = (ITerrainShape)game.Services.GetService(typeof(ITerrainShape));

                // 3/3/2009 - Set in EditMode for TerrainShape
                TerrainShape.TerrainIsIn = TerrainIsIn.EditMode;

                _bUseConstantFeet = bUseConstantFeet;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("HeightTools method threw the exception " + ex.Message ?? "No Message");
#endif
            }

        }


        private void HeightTools_Load(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                ResetToolSelection(HeightTool.Select);
                hScrollBar1.Value = TerrainEditRoutines.GroundCursorSize;
                hScrollBar2.Value = TerrainEditRoutines.GroundCursorStrength;
                TerrainEditRoutines.ShowHeightCursor = true;

                // Start Timer Tick
                //timer1.Start();
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("HeightTools_Load method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        private void SelectTool(HeightTool tool)
        {
            try // 6/22/2010
            {
                ResetToolSelection(tool);

                switch (tool)
                {
                    case HeightTool.Select:
                        CurrentTool = HeightTool.Select;
                        break;
                    case HeightTool.Raise:
                        CurrentTool = HeightTool.Raise;
                        break;
                    case HeightTool.Lower:
                        CurrentTool = HeightTool.Lower;
                        break;
                    case HeightTool.Smooth:
                        CurrentTool = HeightTool.Smooth;
                        break;
                    case HeightTool.Flatten:
                        CurrentTool = HeightTool.Flatten;
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

        private void ResetToolSelection(HeightTool tool)
        {
            try // 6/22/2010
            {
                if (tool != HeightTool.Select)
                    checkBox1.Checked = false;
                if (tool != HeightTool.Raise)
                    checkBox2.Checked = false;
                if (tool != HeightTool.Lower)
                    checkBox3.Checked = false;
                if (tool != HeightTool.Smooth)
                    checkBox4.Checked = false;
                if (tool != HeightTool.Flatten)
                    checkBox5.Checked = false;
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("ResetToolSelection method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

// ReSharper disable InconsistentNaming
        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            try // 6/22/2010
            {
                TerrainEditRoutines.GroundCursorSize = hScrollBar1.Value;
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("hScrollBar1_Scroll method threw the exception " + ex.Message ?? "No Message");
#endif  
            }
        }

        private void hScrollBar2_Scroll(object sender, ScrollEventArgs e)
        {
            try // 6/22/2010
            {
                TerrainEditRoutines.GroundCursorStrength = hScrollBar2.Value;
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("hScrollBar2_Scroll method threw the exception " + ex.Message ?? "No Message");
#endif     
            }
        }


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                if (checkBox1.Checked)
                    SelectTool(HeightTool.Select);
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("checkBox1_CheckedChanged method threw the exception " + ex.Message ?? "No Message");
#endif  
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                if (checkBox2.Checked)
                    SelectTool(HeightTool.Raise);
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("checkBox2_CheckedChanged method threw the exception " + ex.Message ?? "No Message");
#endif  
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                if (checkBox3.Checked)
                    SelectTool(HeightTool.Lower);
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("checkBox3_CheckedChanged method threw the exception " + ex.Message ?? "No Message");
#endif  
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                if (checkBox4.Checked)
                    SelectTool(HeightTool.Smooth);
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("checkBox4_CheckedChanged method threw the exception " + ex.Message ?? "No Message");
#endif  
            }
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                if (checkBox5.Checked)
                    SelectTool(HeightTool.Flatten);
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("checkBox5_CheckedChanged method threw the exception " + ex.Message ?? "No Message");
#endif  
            }
        }


        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var tbInstance = (TrackBar)sender;
                // Set text box the value from scroll bar
                txtConstantFeet.Text = tbInstance.Value.ToString();
                // Set value into Height Field
                TerrainEditRoutines.ConstantFeetToAdd = tbInstance.Value;
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("trackBar1_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif   
            }
        }

        private void cbConstantFeet_CheckedChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var cbInstance = (CheckBox)sender;

                TerrainEditRoutines.UseConstantFeet = cbInstance.Checked;
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("cbConstantFeet_CheckedChanged method threw the exception " + ex.Message ?? "No Message");
#endif 
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Get Mouse Position Input Data
                _mouseState = Mouse.GetState();

                // 5/18/2010 - Refactored core code to new STATIC method.
                //DoTimerUpdate(this);
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("timer1_Tick method threw the exception " + ex.Message ?? "No Message");
#endif 
            }
        }
// ReSharper restore InconsistentNaming
        // 5/18/2010
        /// <summary>
        /// Helper method, which checks for the <see cref="HeightTool"/> Enum setting, and calls the 
        /// appropriate <see cref="TerrainEditRoutines"/> method.
        /// </summary>
        /*private static void DoTimerUpdate(HeightTools heightTools)
        {
            try // 6/22/2010
            {
                // If left mouse button click, AND ShowCursor=True && HeightTools form is On.
                if (heightTools._mouseState.LeftButton == ButtonState.Pressed
                    && TerrainEditRoutines.ToolInUse == ToolType.HeightTool && !heightTools.IsMouseInControl(heightTools._bEditing))
                {
                    heightTools._bEditing = true;

                    // Only update if mouse has moved!
                    if (heightTools._lastMousePos.X != heightTools._mouseState.X && heightTools._lastMousePos.Y != heightTools._mouseState.Y)
                    {
                        // Save Current Mouse Position
                        heightTools._lastMousePos.X = heightTools._mouseState.X;
                        heightTools._lastMousePos.Y = heightTools._mouseState.Y;

                        switch (heightTools.CurrentTool)
                        {
                            case HeightTool.Raise:
                                if (!heightTools._bHeightSet && heightTools._bUseConstantFeet)
                                {
                                    TerrainEditRoutines.ConstantFeetValue =
                                        TerrainPickingRoutines.PickedTriangle.Triangle[0].Position.Y +
                                        TerrainEditRoutines.ConstantFeetToAdd;
                                    heightTools._bHeightSet = true;
                                }
                                TerrainEditRoutines.QuadRaiseHeight();
                                break;
                            case HeightTool.Lower:
                                TerrainEditRoutines.QuadLowerHeight();
                                break;
                            case HeightTool.Smooth:
                                TerrainEditRoutines.QuadSmooth();
                                break;
                            case HeightTool.Flatten:
                                if (!heightTools._bHeightSet)
                                {
                                    TerrainEditRoutines.FlattenHeight =
                                        TerrainPickingRoutines.PickedTriangle.Triangle[0].Position.Y;
                                    heightTools._bHeightSet = true;
                                }
                                TerrainEditRoutines.QuadFlattenVertices();
                                break;
                            case HeightTool.Select:
                            default:
                                break;
                        } // End Switch
                    } // End If LastMousePos              
                } // End If Left Button Pressed
                else
                {
                    heightTools._bEditing = false;

                    if (heightTools._bHeightSet)
                        heightTools._bHeightSet = false;
                }

                // 1/8/2009
                // If right mouse button click, AND ShowCursor=True && HeightTools form is On.
                if (heightTools._mouseState.RightButton == ButtonState.Pressed
                    && TerrainEditRoutines.ToolInUse == ToolType.HeightTool && !heightTools.IsMouseInControl(heightTools._bEditing))
                {
                    // Force a Tessellation on current Quad
                    if (!TerrainEditRoutines.TessellateToLowerLODLocked)
                        TerrainEditRoutines.TessellateToLowerLOD();
                }
                else
                {
                    // 4/25/2008 - This removed Lock which was preventing multiple calls to Tessellation.
                    TerrainEditRoutines.TessellateToLowerLODLocked = false;
                }
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("DoTimerUpdate method threw the exception " + ex.Message ?? "No Message");
#endif 
            }
        }*/

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

                    return isIn;
                }
                
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("IsMouseInControl method threw the exception " + ex.Message ?? "No Message");
#endif   
            }

            return false;
        }

        // 3/3/2009
        /// <summary>
        /// Applies the already generated PerlinNoise HeightMap, to the
        /// TerrainData's HeightData array.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnApplyHeightMap_Click(object sender, EventArgs e)

        {
            try // 6/22/2010
            {
                // Check if noise data is null or zero Count
                if (_noiseData == null || _noiseData.Count == 0)
                {
                    txtFloodErrorMessages.Text =
                        @"A Perlin Noise MUST be generated first, before the automatic Flood generator can be applied.";
                    return;
                }

                // Apply the NoiseData
                TerrainData.HeightData = _noiseData.ToArray();

                // Update VBs
                TerrainData.SetupTerrainVertexBuffer();
                TerrainData.SetupVertexDataAndVertexLookup();
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("btnApplyHeightMap_Click method threw the exception " + ex.Message ?? "No Message");
#endif    
            }
        }

        // 6/30/2009
        private void btnCreateMap1024x1024_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                TerrainShape.CreateNewEmptyTerrain(512, 512); // 1024,1024
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("btnCreateMap1024x1024_Click method threw the exception " + ex.Message ?? "No Message");
#endif    
            }
        }

        // 11/20/2009
        /// <summary>
        /// Generates the Perlin Noise, and displays in the picture box.
        /// </summary>
        private void btnGeneratePerlinNoise_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Read values in from Group controls

                // Pass-1 values
                var randomSeedP1 = (int)nudRandomSeedValue_p1.Value;
                var perlinNoiseSizeP1 = (float)nudNoiseSize_p1.Value;
                var perlinPersistenceP1 = (float)nudPersistence_p1.Value;
                var perlinOctavesP1 = (int)nudOctaves_p1.Value;

                // Pass-2 values
                var randomSeedP2 = (int)nudRandomSeedValue_p2.Value;
                var perlinNoiseSizeP2 = (float)nudNoiseSize_p2.Value;
                var perlinPersistenceP2 = (float)nudPersistence_p2.Value;
                var perlinOctavesP2 = (int)nudOctaves_p2.Value;

                // Generate Random Map           
                if (cbPass1Enable.Checked && !cbPass2Enable.Checked)
                {
                    _noiseData = TerrainData.CreatePerlinNoiseMap(randomSeedP1, perlinNoiseSizeP1,
                                                                  perlinPersistenceP1, perlinOctavesP1);
                }
                else if (cbPass2Enable.Checked && cbPass2Enable.Checked)
                {
                    var noiseData1 = TerrainData.CreatePerlinNoiseMap(randomSeedP1, perlinNoiseSizeP1, perlinPersistenceP1,
                                                                      perlinOctavesP1);
                    var noiseData2 = TerrainData.CreatePerlinNoiseMap(randomSeedP2, perlinNoiseSizeP2, perlinPersistenceP2,
                                                                      perlinOctavesP2);

                    // combine 2 heightmaps together
                    _noiseData = TerrainData.CombineHeightMaps(noiseData1, noiseData2);

                }

                // 11/20/2009
                if (_noiseData == null)
                {
                    txtFloodErrorMessages.Text =
                        @"At least ONE PerlinNoise 'Enabled' checkbox MUST be selected.";
                    return;
                }

                // Populate Color 'Bits', used to store into texture map
                // Set Image into PictureBox on Form.
                pictureBox.BackgroundImage = TerrainData.CreateBitmapFromPerlinNoise(_noiseData);
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("btnGeneratePerlinNoise_Click method threw the exception " + ex.Message ?? "No Message");
#endif  
            }
        }
// ReSharper restore InconsistentNaming
        // 12/7/2009
        /// <summary>
        /// Captures the close event, and cancels the close action; instead, the form is simply hidden
        /// from view by setting the 'Visible' flag to False.  This fixes the issue of the HEAP crash 
        /// when trying to reinstantiate a form, after it was already created in the 'TerrainEditRoutines' class.
        /// </summary>
        private void HeightTools_FormClosing(object sender, FormClosingEventArgs e)
        {
            try // 6/22/2010
            {
                e.Cancel = true;
                Visible = false;
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("HeightTools_FormClosing method threw the exception " + ex.Message ?? "No Message");
#endif  
            }
        }
    }
}