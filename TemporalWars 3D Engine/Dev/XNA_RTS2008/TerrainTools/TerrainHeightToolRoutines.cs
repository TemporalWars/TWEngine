#region File Description
//-----------------------------------------------------------------------------
// TerrainHeightToolRoutines.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.Terrain;
using ImageNexus.BenScharbach.TWEngine.Terrain.Enums;
using ImageNexus.BenScharbach.TWEngine.TerrainTools.Enums;
using ImageNexus.BenScharbach.TWLate.RTS_FogOfWarInterfaces.FOW;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF;
using ImageNexus.BenScharbach.TWTools.TWTerrainTools_Interfaces.Structs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TWEngine.TerrainTools;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;

namespace ImageNexus.BenScharbach.TWEngine.TerrainTools
{
    // 7/1/2010
    /// <summary>
    /// The <see cref="TerrainHeightToolRoutines"/> class contains all necessary methods and event handlers to
    /// connect to the WPF HeightTools form.
    /// </summary>
    public class TerrainHeightToolRoutines : IDisposable
    {
        internal static HeightToolWindow HeightToolWindowI { get; private set; }

        // 7/1/2010
        private static bool _bEditing;
        private static bool _bHeightSet;
        private static bool _bUseConstantFeet;
        private static List<float> _noiseData; // PerlinNoise data

        // 7/1/2010
        private static Vector2 _lastMousePos;
        private static MouseState _mouseState;

        // 3/30/2011 - ManualResetEvent
        private static ManualResetEvent _manualResetEventForClosing;

#if WithLicense
        // 5/10/2012 - License
        private static readonly LicenseHelper LicenseInstance;
#endif

        /// <summary>
        /// constructor
        /// </summary>
        static TerrainHeightToolRoutines()
        {
#if WithLicense
            // 5/10/2012 Check for Valid License.
            LicenseInstance = new LicenseHelper();
#endif
        }

        /// <summary>
        /// constructor
        /// </summary>
        public TerrainHeightToolRoutines()
        {
            // 11/19/2009 - Need to turn of FOW, otherwise, blinking will occur.
            var fogOfWar = (IFogOfWar)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(IFogOfWar));
            if (fogOfWar != null) fogOfWar.IsVisible = false;

            // 3/30/2011
            _manualResetEventForClosing = new ManualResetEvent(false);

            // 1/11/2011
            CreateHeightToolWindow();
        }

        // 1/11/2011
        private static void CreateHeightToolWindow()
        {
            HeightToolWindowI = new HeightToolWindow();

            // 7/1/2010
            // Connect Events to event handlers.
            ConnectEventHandlers();
        }

        /// <summary>
        /// Connects all required event handlers to WPF form.
        /// </summary>
        private static void ConnectEventHandlers()
        {
            //HeightToolWindowI.CreateMap1024 += TerrainToolsCreateMap1024;
            HeightToolWindowI.ApplyHeightMap += TerrainToolsApplyHeightMap;
            HeightToolWindowI.GeneratePerlinNoise += TerrainToolsGeneratePerlinNoise;
            HeightToolWindowI.GroundCursorSizeUpdated += TerrainToolsGroundCursorSizeUpdated;
            HeightToolWindowI.GroundCursorStengthUpdated += TerrainToolsGroundCursorStengthUpdated;
            HeightToolWindowI.ConstantFeetUpdated += TerrainToolsConstantFeetUpdated;
            HeightToolWindowI.UseConstantFeetUpdated += TerrainToolsUseConstantFeetUpdated;
            HeightToolWindowI.RegenerateNormals += HeightToolWindowI_RegenerateNormals; // 3/30/2011
            HeightToolWindowI.FormStartClose += HeightToolWindowIFormStartClose; // 3/30/2011
            HeightToolWindowI.FormClosed += TerrainToolsFormClosed;
        }

        // 3/30/2011
        /// <summary>
        /// Occurs when user clicks on the 'Rebuild Normals' button.
        /// </summary>
        static void HeightToolWindowI_RegenerateNormals(object sender, EventArgs e)
        {
            var rootQuadTree = TerrainShape.RootQuadTree;
            TerrainData.RebuildNormals(ref rootQuadTree);
        }

        /// <summary>
        /// Updates proper settings when some WPF form closes.
        /// </summary>
        private static void TerrainToolsFormClosed(object sender, EventArgs e)
        {
            // 3/30/2011 - Signal close complete
            _manualResetEventForClosing.Set();
        }

        // 3/30/2011
        /// <summary>
        /// Occurs when the WPF form is starting the close cycle.
        /// </summary>
        private static void HeightToolWindowIFormStartClose(object sender, EventArgs e)
        {
            // Set ToolType window to start close cycle
            TerrainWPFTools.ToolTypeToClose = ToolType.HeightTool;

            // Set State of StartcloseCycle to false
            HeightToolWindowI.StartCloseCycle = false;
        }

        /// <summary>
        /// Updates the <see cref="TerrainEditRoutines.ConstantFeetToAdd"/> value.
        /// </summary>
        private static void TerrainToolsUseConstantFeetUpdated(object sender, EventArgs e)
        {
            TerrainEditRoutines.ConstantFeetToAdd = HeightToolWindowI.ConstantFeetToAdd;
        }

        /// <summary>
        /// Updates the <see cref="TerrainEditRoutines.UseConstantFeet"/> value.
        /// </summary>
        private static void TerrainToolsConstantFeetUpdated(object sender, EventArgs e)
        {
            TerrainEditRoutines.UseConstantFeet = HeightToolWindowI.UseConstantFeet;
        }

        /// <summary>
        /// Updates the <see cref="TerrainEditRoutines.GroundCursorSize"/> value.
        /// </summary>
        private static void TerrainToolsGroundCursorStengthUpdated(object sender, EventArgs e)
        {
            TerrainEditRoutines.GroundCursorStrength = HeightToolWindowI.GroundCursorStrength;
        }

        /// <summary>
        /// Updates the <see cref="TerrainEditRoutines.GroundCursorStrength"/> value.
        /// </summary>
        private static void TerrainToolsGroundCursorSizeUpdated(object sender, EventArgs e)
        {
            TerrainEditRoutines.GroundCursorSize = HeightToolWindowI.GroundCursorSize;
        }

        /// <summary>
        /// Generates the Perlin Noise, and displays in the picture box.
        /// </summary>
        private static void TerrainToolsGeneratePerlinNoise(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
#if WithLicense
                // 5/10/2012 - Return if trial.
                if (LicenseInstance.IsTrial)
                {
                    MessageBox.Show("Valid ONLY in FULL PAID Version!", "License Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
#endif
                // Read values in from Group controls
                PerlinNoisePass perlinNoisePass;
                HeightToolWindowI.GetPerlinNoiseAttributesForPass1(out perlinNoisePass);

                // Pass-1 values
                var randomSeedP1 = perlinNoisePass.RandomSeed;
                var perlinNoiseSizeP1 = perlinNoisePass.PerlinNoiseSize;
                var perlinPersistenceP1 = perlinNoisePass.PerlinPersistence;
                var perlinOctavesP1 = perlinNoisePass.PerlinOctaves;

                HeightToolWindowI.GetPerlinNoiseAttributesForPass2(out perlinNoisePass);

                // Pass-2 values
                var randomSeedP2 = perlinNoisePass.RandomSeed;
                var perlinNoiseSizeP2 = perlinNoisePass.PerlinNoiseSize;
                var perlinPersistenceP2 = perlinNoisePass.PerlinPersistence;
                var perlinOctavesP2 = perlinNoisePass.PerlinOctaves;

                // Generate Random Map           
                if (HeightToolWindowI.IsPass1Enabled && !HeightToolWindowI.IsPass2Enabled)
                {
                    _noiseData = TerrainData.CreatePerlinNoiseMap(randomSeedP1, perlinNoiseSizeP1,
                                                                  perlinPersistenceP1, perlinOctavesP1);
                }
                else if (HeightToolWindowI.IsPass1Enabled && HeightToolWindowI.IsPass2Enabled)
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
                    HeightToolWindowI.SetErrorMessage(@"At least ONE PerlinNoise 'Enabled' check box MUST be selected.");
                    return;
                }

                // Populate Color 'Bits', used to store into texture map
                // Set Image into PictureBox on Form.
                HeightToolWindowI.SetPictureBoxImage(TerrainData.CreateBitmapFromPerlinNoise(_noiseData));

            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("TerrainToolsGeneratePerlinNoise method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Applies the already generated PerlinNoise HeightMap, to the
        /// TerrainData's HeightData array.
        /// </summary>
        private static void TerrainToolsApplyHeightMap(object sender, EventArgs e)
        {
            // Check if noise data is null or zero Count
            if (_noiseData == null || _noiseData.Count == 0)
            {
                HeightToolWindowI.SetErrorMessage(@"A Perlin Noise MUST be generated first, before the automatic Flood generator can be applied.");
                return;
            }

            // Apply the NoiseData
            TerrainData.HeightData = _noiseData.ToArray();

            // Update VBs
            TerrainData.SetupTerrainVertexBuffer();
            TerrainData.SetupVertexDataAndVertexLookup();
        }

        /// <summary>
        /// Creates a Terrain of given 1024x1024 size.
        /// </summary>
        private static void TerrainToolsCreateMap1024(object sender, EventArgs e)
        {
            TerrainShape.CreateNewEmptyTerrain(1024, 1024); // 1024,1024
        }

        // 7/1/2010
        /// <summary>
        /// Updates the current WPF tool.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            DoHeightToolUpdate();
        }

        // 3/30/2011
        public void CloseForm()
        {
            HeightToolWindowI.Close();

            // Wait for WPF Closed event to trigger before allowing exit of method call.
            _manualResetEventForClosing.WaitOne();
        }

        // 5/18/2010
        /// <summary>
        /// Helper method, which checks for the <see cref="HeightTool"/> Enum setting, and calls the 
        /// appropriate <see cref="TerrainEditRoutines"/> method.
        /// </summary>
        private static void DoHeightToolUpdate()
        {
            try // 6/22/2010
            {
                _mouseState = Mouse.GetState();

                // If left mouse button click, AND ShowCursor=True && HeightTools form is On.
                if (_mouseState.LeftButton == ButtonState.Pressed
                    && TerrainEditRoutines.ToolInUse == ToolType.HeightTool) // && !HeightToolWindowI.IsMouseInControl()
                {
                    _bEditing = true;

                    // Only update if mouse has moved!
                    if (_lastMousePos.X != _mouseState.X && _lastMousePos.Y != _mouseState.Y)
                    {
                        // Save Current Mouse Position
                        _lastMousePos.X = _mouseState.X;
                        _lastMousePos.Y = _mouseState.Y;

                        switch (HeightToolWindowI.CurrentTool)
                        {
                            case ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.Enums.HeightTool.Raise:
                                if (!_bHeightSet && _bUseConstantFeet)
                                {
                                    TerrainEditRoutines.ConstantFeetValue =
                                        TerrainPickingRoutines.PickedTriangle.Triangle[0].Position.Y +
                                        TerrainEditRoutines.ConstantFeetToAdd;
                                    _bHeightSet = true;
                                }
                                TerrainEditRoutines.QuadRaiseHeight();
                                break;
                            case ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.Enums.HeightTool.Lower:
                                TerrainEditRoutines.QuadLowerHeight();
                                break;
                            case ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.Enums.HeightTool.Smooth:
                                TerrainEditRoutines.QuadSmooth();
                                break;
                            case ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.Enums.HeightTool.Flatten:
                                if (!_bHeightSet)
                                {
                                    TerrainEditRoutines.FlattenHeight =
                                        TerrainPickingRoutines.PickedTriangle.Triangle[0].Position.Y;
                                    _bHeightSet = true;
                                }
                                TerrainEditRoutines.QuadFlattenVertices();
                                break;
                            default:
                                break;
                        } // End Switch
                    } // End If LastMousePos              
                } // End If Left Button Pressed
                else
                {
                    if (_bEditing)
                    {
                        // 3/30/2011 - Regenerate Normal Map
                        var rootQuadTree = TerrainShape.RootQuadTree;
                        TerrainData.RebuildNormals(ref rootQuadTree);
                    }

                    _bEditing = false;

                    if (_bHeightSet)
                        _bHeightSet = false;
                   
                }

                // 1/8/2009
                // If right mouse button click, AND ShowCursor=True && HeightTools form is On.
                if (_mouseState.RightButton == ButtonState.Pressed
                    && TerrainEditRoutines.ToolInUse == ToolType.HeightTool) // && !HeightToolWindowI.IsMouseInControl()
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
        }

        // 7/9/2010
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            // dispose of WPF form
            if (HeightToolWindowI != null)
            {
                HeightToolWindowI.Close();
                HeightToolWindowI = null;
            }
        }
    }
}
