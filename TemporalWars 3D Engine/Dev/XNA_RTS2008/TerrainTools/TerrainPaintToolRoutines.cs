#region File Description
//-----------------------------------------------------------------------------
// TerrainPaintToolRoutines.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.BeginGame.Enums;
using ImageNexus.BenScharbach.TWEngine.Interfaces;
using ImageNexus.BenScharbach.TWEngine.Terrain;
using ImageNexus.BenScharbach.TWEngine.Terrain.Enums;
using ImageNexus.BenScharbach.TWEngine.Utilities;
using ImageNexus.BenScharbach.TWLate.RTS_FogOfWarInterfaces.FOW;
using ImageNexus.BenScharbach.TWLate.RTS_MinimapInterfaces.Minimap;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.Delegates;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.Enums;
using ImageNexus.BenScharbach.TWTools.TWTerrainTools_Interfaces.Structs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TWEngine.TerrainTools;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;

namespace ImageNexus.BenScharbach.TWEngine.TerrainTools
{
    // 7/1/2010
    /// <summary>
    /// The <see cref="TerrainPaintToolRoutines"/> class contains all necessary methods and event handlers to
    /// connect to the WPF ItemTools form.
    /// </summary>
    public class TerrainPaintToolRoutines : IDisposable
    {
        internal static PaintToolWindow PaintToolWindowI { get; private set; }

        // 1/2/2010 - Ref for Minimap Interface
        private static IMinimap _miniMap;
        private static ITerrainShape _terrainShape;

        private static bool _bEditing;
        private static Vector3 _cursorPos;

        private static Vector2 _lastMousePos;

        private Point _mousePoint;
        private static MouseState _mouseState;

        // 11/20/2009 - PerlinNoise data
        private static List<float> _noiseData;

        // 7/8/2010
        private static string _txtMapName;

        // 3/30/2011 - ManualResetEvent
        private static ManualResetEvent _manualResetEventForClosing;

#if WithLicense
        // 5/10/2012 - License
        private static readonly LicenseHelper LicenseInstance;
#endif

         /// <summary>
        /// constructor
        /// </summary>
        static TerrainPaintToolRoutines()
        {
#if WithLicense
            // 5/10/2012 Check for Valid License.
            LicenseInstance = new LicenseHelper();
#endif
        }

        /// <summary>
        /// constructor
        /// </summary>
        public TerrainPaintToolRoutines()
        {
            // 1/21/2009 - Set the Loaded MapName.
            _txtMapName = TerrainStorageRoutines.LoadMapName;

            // 11/19/2009 - Need to turn of FOW, otherwise, blinking will occur.
            var fogOfWar = (IFogOfWar)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(IFogOfWar));
            if (fogOfWar != null) fogOfWar.IsVisible = false;

            // 1/2/2010 - Get Minimap Ref
            _miniMap = (IMinimap)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(IMinimap));

            // Get TerrainShape Interface
            _terrainShape = (ITerrainShape)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(ITerrainShape));

            // 3/3/2009 - Set in EditMode for TerrainShape
            TerrainShape.TerrainIsIn = TerrainIsIn.EditMode;

            // 3/30/2011
            _manualResetEventForClosing = new ManualResetEvent(false);

            // 1/11/2011
            CreatePaintToolWindow();
        }

        // 1/11/2011
        private static void CreatePaintToolWindow()
        {
            PaintToolWindowI = new PaintToolWindow();

            // 7/9/2010 - Init attributes.
            InitPaintToolSettings();

            // 7/6/2010
            // Connect Events to event handlers.
            ConnectEventHandlers();

            // Populate Treeview
            StartPopulateTreeContent();

            // 7/7/2010
            // Populate listView controls with current textures.
            PopulateListViewGroups();
        }

        // 6/22/2010
        private static void InitPaintToolSettings()
        {
            try // 6/22/2010
            {
                // Set AlphaMap Layer Percent Values
                var terrainShape = _terrainShape; // 6/22/2010

                PaintToolWindowI.Layer1Percent = terrainShape.AlphaMaps.AlphaLy1Percent * 100;
                PaintToolWindowI.Layer2Percent = terrainShape.AlphaMaps.AlphaLy2Percent * 100;
                PaintToolWindowI.Layer3Percent = terrainShape.AlphaMaps.AlphaLy3Percent * 100;
                PaintToolWindowI.Layer4Percent = terrainShape.AlphaMaps.AlphaLy4Percent * 100;

                // 5/8/2009 - Set Ambient/Specular Attributes for Layer 1/2
                {
                    PaintToolWindowI.AmbientColorLayer1 = terrainShape.AmbientColorLayer1;
                    PaintToolWindowI.AmbientPowerLayer1 = terrainShape.AmbientPowerLayer1;

                    PaintToolWindowI.SpecularColorLayer1 = terrainShape.SpecularColorLayer1;
                    PaintToolWindowI.SpecularPowerLayer1 = terrainShape.SpecularPowerLayer1;

                    PaintToolWindowI.AmbientColorLayer2 = terrainShape.AmbientColorLayer2;
                    PaintToolWindowI.AmbientPowerLayer2 = terrainShape.AmbientPowerLayer2;

                    PaintToolWindowI.SpecularColorLayer2 = terrainShape.SpecularColorLayer2;
                    PaintToolWindowI.SpecularPowerLayer2 = terrainShape.SpecularPowerLayer2;
                }

                // 5/15/2009 - Set PerlinNoiseData Attributes
                {
                    // Group-1 (Texture Mix 1 to 2)
                    PerlinNoisePass perlinNoisePassG1;
                    perlinNoisePassG1.RandomSeed = TerrainShape.PerlinNoiseDataTexture1To2MixLayer1.seed;
                    perlinNoisePassG1.PerlinNoiseSize = TerrainShape.PerlinNoiseDataTexture1To2MixLayer1.noiseSize;
                    perlinNoisePassG1.PerlinPersistence = TerrainShape.PerlinNoiseDataTexture1To2MixLayer1.persistence;
                    perlinNoisePassG1.PerlinOctaves = TerrainShape.PerlinNoiseDataTexture1To2MixLayer1.octaves;
                    PaintToolWindowI.SetPerlinNoiseAttributesGroup1(ref perlinNoisePassG1);

                    // Group-2 (Texture Mix 2 to 3)
                    PerlinNoisePass perlinNoisePassG2;
                    perlinNoisePassG2.RandomSeed = TerrainShape.PerlinNoiseDataTexture1To2MixLayer2.seed;
                    perlinNoisePassG2.PerlinNoiseSize = TerrainShape.PerlinNoiseDataTexture1To2MixLayer2.noiseSize;
                    perlinNoisePassG2.PerlinPersistence = TerrainShape.PerlinNoiseDataTexture1To2MixLayer2.persistence;
                    perlinNoisePassG2.PerlinOctaves = TerrainShape.PerlinNoiseDataTexture1To2MixLayer2.octaves;
                    PaintToolWindowI.SetPerlinNoiseAttributesGroup2(ref perlinNoisePassG2);
                }

                PaintToolWindowI.ResetToolSelection(PaintTool.Select);
                PaintToolWindowI.PaintCursorSize = TerrainEditRoutines.PaintCursorSize;
                PaintToolWindowI.PaintCursorStrength = TerrainEditRoutines.PaintCursorStrength;
                TerrainEditRoutines.ShowPaintCursor = true;

               
            }
            catch (Exception ex)
            {
                Debug.WriteLine("InitPaintToolSettings method threw the exception " + ex.Message ?? "No Message");
            }
        }

        /// <summary>
        /// Connects all required event handlers to WPF form.
        /// </summary>
        private static void ConnectEventHandlers()
        {
            PaintToolWindowI.PaintCursorSizeUpdated += PaintToolWindowI_PaintCursorSizeUpdated;
            PaintToolWindowI.PaintCursorStrengthUpdated += PaintToolWindowI_PaintCursorStrengthUpdated;
            PaintToolWindowI.Layer1PercentUpdated += PaintToolWindowI_Layer1PercentUpdated;
            PaintToolWindowI.Layer2PercentUpdated += PaintToolWindowI_Layer2PercentUpdated;
            PaintToolWindowI.Layer3PercentUpdated += PaintToolWindowI_Layer3PercentUpdated;
            PaintToolWindowI.Layer4PercentUpdated += PaintToolWindowI_Layer4PercentUpdated;
            PaintToolWindowI.RebuildAlphaMap += PaintToolWindowI_RebuildAlphaMap;
            PaintToolWindowI.DragDropLayer1 += PaintToolWindowI_DragDropLayer1;
            PaintToolWindowI.DragDropLayer2 += PaintToolWindowI_DragDropLayer2;
            PaintToolWindowI.SelectedItemChangedLv1 += PaintToolWindowI_SelectedItemChangedLv1;
            PaintToolWindowI.SelectedItemChangedLv2 += PaintToolWindowI_SelectedItemChangedLv2;
            PaintToolWindowI.CreateVolume1 += PaintToolWindowI_CreateVolume1;
            PaintToolWindowI.CreateVolume2 += PaintToolWindowI_CreateVolume2;
            PaintToolWindowI.BlendToUseUpdated += PaintToolWindowI_BlendToUseUpdated;
            PaintToolWindowI.ClearLayer1 += PaintToolWindowI_ClearLayer1;
            PaintToolWindowI.ClearLayer2 += PaintToolWindowI_ClearLayer2;
            PaintToolWindowI.InUseLayer1 += PaintToolWindowI_InUseLayer1;
            PaintToolWindowI.InUseLayer2 += PaintToolWindowI_InUseLayer2;

            // Ambient/Specular event handlers
            PaintToolWindowI.AmbientColorLayer1Updated += PaintToolWindowI_AmbientColorLayer1Updated;
            PaintToolWindowI.AmbientColorLayer2Updated += PaintToolWindowI_AmbientColorLayer2Updated;
            PaintToolWindowI.AmbientPowerLayer1Updated += PaintToolWindowI_AmbientPowerLayer1Updated;
            PaintToolWindowI.AmbientPowerLayer2Updated += PaintToolWindowI_AmbientPowerLayer2Updated;
            PaintToolWindowI.SpecularColorLayer1Updated += PaintToolWindowI_SpecularColorLayer1Updated;
            PaintToolWindowI.SpecularColorLayer2Updated += PaintToolWindowI_SpecularColorLayer2Updated;
            PaintToolWindowI.SpecularPowerLayer1Updated += PaintToolWindowI_SpecularPowerLayer1Updated;
            PaintToolWindowI.SpecularPowerLayer2Updated += PaintToolWindowI_SpecularPowerLayer2Updated;

            // Perlin-Noise event handlers
            PaintToolWindowI.PerlinNoiseGroup1Updated += PaintToolWindowI_PerlinNoiseGroup1Updated;
            PaintToolWindowI.PerlinNoiseGroup2Updated += PaintToolWindowI_PerlinNoiseGroup2Updated;
            PaintToolWindowI.NoiseGeneratorGroup1 += PaintToolWindowI_NoiseGeneratorGroup1;
            PaintToolWindowI.NoiseGeneratorGroup2 += PaintToolWindowI_NoiseGeneratorGroup2;
            PaintToolWindowI.ApplyNoiseGroup1 += PaintToolWindowI_ApplyNoiseGroup1;
            PaintToolWindowI.ApplyNoiseGroup2 += PaintToolWindowI_ApplyNoiseGroup2;

            // 1/11/2011 - FormClosed
            PaintToolWindowI.FormClosed += PaintToolWindowI_FormClosed;
            // 3/30/2011 - FormStartClose
            PaintToolWindowI.FormStartClose += PaintToolWindowI_FormStartClose;

        }

        // 3/30/2011
        /// <summary>
        /// Occurs when the WPF form is starting the close cycle.
        /// </summary>
        static void PaintToolWindowI_FormStartClose(object sender, EventArgs e)
        {
            // Set ToolType window to start close cycle
            TerrainWPFTools.ToolTypeToClose = ToolType.PaintTool;

            // Set State of StartcloseCycle to false
            PaintToolWindowI.StartCloseCycle = false;
        }

        // 1/11/2011
        /// <summary>
        /// Updates proper settings when some WPF form closes.
        /// </summary>
        static void PaintToolWindowI_FormClosed(object sender, EventArgs e)
        {
            // 3/30/2011 - Signal close complete
            _manualResetEventForClosing.Set();
        }

        // 7/9/2010
        /// <summary>
        /// Applies the given NoiseData to the AlphaMaps Layer-2.
        /// </summary>
        static void PaintToolWindowI_ApplyNoiseGroup2(object sender, EventArgs e)
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
                // Check if noise data is null or zero Count
                if (_noiseData == null || _noiseData.Count == 0)
                {
                    PaintToolWindowI.SetErrorMessage("A Perlin Noise MUST be generated first, before the automatic Flood generator can be applied.");
                    return;
                }

                // Apply Perlin Noise to Texture 1-2 channels (Layer-2), which is done by passing
                // the topTexture value; 2 in this case.
                TerrainAlphaMaps.ApplyPerlinNoise(LayerGroup.Layer2, 2, _noiseData);

                // Update MiniMap Landscape.               
                if (_miniMap != null) _miniMap.RenderLandscapeForMiniMap();
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PaintToolWindowI_ApplyNoiseGroup2 method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/9/2010
        /// <summary>
        /// Applies the given NoiseData to the AlphaMaps Layer-1.
        /// </summary>
        static void PaintToolWindowI_ApplyNoiseGroup1(object sender, EventArgs e)
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

                // Check if noise data is null or zero Count
                if (_noiseData == null || _noiseData.Count == 0)
                {
                    PaintToolWindowI.SetErrorMessage("A Perlin Noise MUST be generated first, before the automatic Flood generator can be applied.");
                    return;
                }

                // Apply Perlin Noise to Texture 1-2 channels (Layer-1), which is done by passing
                // the topTexture value; 2 in this case.
                TerrainAlphaMaps.ApplyPerlinNoise(LayerGroup.Layer1, 2, _noiseData);

                // Update MiniMap Landscape.               
                if (_miniMap != null) _miniMap.RenderLandscapeForMiniMap();
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PaintToolWindowI_ApplyNoiseGroup1 method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/9/2010
        /// <summary>
        /// Generates Perlin Noise used to alpha map between textures 1 and 2, for 
        /// Layer 2.
        /// </summary>
        static void PaintToolWindowI_NoiseGeneratorGroup2(object sender, EventArgs e)
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

                // retrieve Perlin-Noise attributes
                PerlinNoisePass perlinNoisePass;
                PaintToolWindowI.GetPerlinNoiseAttributesGroup2(out perlinNoisePass);

                // Get Perlin Noise Params
                var seed = perlinNoisePass.RandomSeed;
                var noiseSize = perlinNoisePass.PerlinNoiseSize;
                var persistence = perlinNoisePass.PerlinPersistence;
                var octaves = perlinNoisePass.PerlinOctaves;

                _noiseData = TerrainData.CreatePerlinNoiseMap(seed, noiseSize, persistence, octaves);

                // Populate Color 'Bits', used to store into texture map
                // Set Image into PictureBox on Form.
                var perlinNoiseBitmap = TerrainData.CreateBitmapFromPerlinNoise(_noiseData);
                PaintToolWindowI.SetPictureBoxImage(perlinNoiseBitmap);

            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PaintToolWindowI_NoiseGeneratorGroup2 method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/9/2010
        /// <summary>
        /// Generates Perlin Noise used to alpha map between textures 1 and 2, for 
        /// Layer 1.
        /// </summary>
        static void PaintToolWindowI_NoiseGeneratorGroup1(object sender, EventArgs e)
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

                // retrieve Perlin-Noise attributes
                PerlinNoisePass perlinNoisePass;
                PaintToolWindowI.GetPerlinNoiseAttributesGroup1(out perlinNoisePass);

                // Get Perlin Noise Params
                var seed = perlinNoisePass.RandomSeed;
                var noiseSize = perlinNoisePass.PerlinNoiseSize;
                var persistence = perlinNoisePass.PerlinPersistence;
                var octaves = perlinNoisePass.PerlinOctaves;

                _noiseData = TerrainData.CreatePerlinNoiseMap(seed, noiseSize, persistence, octaves);

                // Populate Color 'Bits', used to store into texture map
                // Set Image into PictureBox on Form.
                var perlinNoiseBitmap = TerrainData.CreateBitmapFromPerlinNoise(_noiseData);
                PaintToolWindowI.SetPictureBoxImage(perlinNoiseBitmap);
               
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PaintToolWindowI_NoiseGeneratorGroup1 method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/9/2010
        /// <summary>
        /// Updates the Perlin-Noise values for group-2.
        /// </summary>
        static void PaintToolWindowI_PerlinNoiseGroup2Updated(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // retrieve Perlin-Noise attributes
                PerlinNoisePass perlinNoisePass;
                PaintToolWindowI.GetPerlinNoiseAttributesGroup2(out perlinNoisePass);

                // update values
                TerrainShape.PerlinNoiseDataTexture1To2MixLayer2.seed = perlinNoisePass.RandomSeed;
                TerrainShape.PerlinNoiseDataTexture1To2MixLayer2.persistence = perlinNoisePass.PerlinPersistence;
                TerrainShape.PerlinNoiseDataTexture1To2MixLayer2.octaves = perlinNoisePass.PerlinOctaves;
                TerrainShape.PerlinNoiseDataTexture1To2MixLayer2.noiseSize = perlinNoisePass.PerlinNoiseSize;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PaintToolWindowI_PerlinNoiseGroup2Updated method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/9/2010
        /// <summary>
        /// Updates the Perlin-Noise values for group-1.
        /// </summary>
        static void PaintToolWindowI_PerlinNoiseGroup1Updated(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // retrieve Perlin-Noise attributes
                PerlinNoisePass perlinNoisePass;
                PaintToolWindowI.GetPerlinNoiseAttributesGroup1(out perlinNoisePass);

                // update values
                TerrainShape.PerlinNoiseDataTexture1To2MixLayer1.seed = perlinNoisePass.RandomSeed;
                TerrainShape.PerlinNoiseDataTexture1To2MixLayer1.persistence = perlinNoisePass.PerlinPersistence;
                TerrainShape.PerlinNoiseDataTexture1To2MixLayer1.octaves = perlinNoisePass.PerlinOctaves;
                TerrainShape.PerlinNoiseDataTexture1To2MixLayer1.noiseSize = perlinNoisePass.PerlinNoiseSize;

            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PaintToolWindowI_PerlinNoiseGroup1Updated method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/9/2010
        /// <summary>
        /// Updates the current SpecularPowerLayer-2 value.
        /// </summary>
        static void PaintToolWindowI_SpecularPowerLayer2Updated(object sender, EventArgs e)
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

                _terrainShape.SpecularPowerLayer2 = PaintToolWindowI.SpecularPowerLayer2;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PaintToolWindowI_SpecularPowerLayer1Updated method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/9/2010
        /// <summary>
        /// Updates the current SpecularPowerLayer-1 value.
        /// </summary>
        static void PaintToolWindowI_SpecularPowerLayer1Updated(object sender, EventArgs e)
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

                _terrainShape.SpecularPowerLayer1 = PaintToolWindowI.SpecularPowerLayer1;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PaintToolWindowI_SpecularPowerLayer1Updated method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/9/2010
        /// <summary>
        /// Updates the current SpecularColorLayer-2 <see cref="Vector3"/> value.
        /// </summary>
        static void PaintToolWindowI_SpecularColorLayer2Updated(object sender, EventArgs e)
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

                _terrainShape.SpecularColorLayer2 = PaintToolWindowI.SpecularColorLayer2;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PaintToolWindowI_SpecularColorLayer1Updated method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/9/2010
        /// <summary>
        /// Updates the current SpecularColorLayer-1 <see cref="Vector3"/> value.
        /// </summary>
        static void PaintToolWindowI_SpecularColorLayer1Updated(object sender, EventArgs e)
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
                _terrainShape.SpecularColorLayer1 = PaintToolWindowI.SpecularColorLayer1;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PaintToolWindowI_SpecularColorLayer1Updated method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/9/2010
        /// <summary>
        /// Updates the current AmbientPowerLayer-2 value.
        /// </summary>
        static void PaintToolWindowI_AmbientPowerLayer2Updated(object sender, EventArgs e)
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

                _terrainShape.AmbientPowerLayer2 = PaintToolWindowI.AmbientPowerLayer2;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PaintToolWindowI_AmbientPowerLayer2Updated method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/9/2010
        /// <summary>
        /// Updates the current AmbientPowerLayer-1 value.
        /// </summary>
        static void PaintToolWindowI_AmbientPowerLayer1Updated(object sender, EventArgs e)
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

                _terrainShape.AmbientPowerLayer1 = PaintToolWindowI.AmbientPowerLayer1;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PaintToolWindowI_AmbientPowerLayer1Updated method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/9/2010
        /// <summary>
        /// Updates the current AmbientColorLayer-2 <see cref="Vector3"/> value.
        /// </summary>
        static void PaintToolWindowI_AmbientColorLayer2Updated(object sender, EventArgs e)
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

                _terrainShape.AmbientColorLayer2 = PaintToolWindowI.AmbientColorLayer2;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PaintToolWindowI_AmbientColorLayer2Updated method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/9/2010
        /// <summary>
        /// Updates the current AmbientColorLayer-1 <see cref="Vector3"/> value.
        /// </summary>
        static void PaintToolWindowI_AmbientColorLayer1Updated(object sender, EventArgs e)
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

                _terrainShape.AmbientColorLayer1 = PaintToolWindowI.AmbientColorLayer1;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PaintToolWindowI_AmbientColorLayer1Updated method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/9/2010
        /// <summary>
        /// Sets the current Layer-2 texture to On or Off.
        /// </summary>
        static void PaintToolWindowI_InUseLayer2(object sender, IsToggledEventArgs e)
        {
            try // 6/22/2010
            {
                TerrainAlphaMaps.InUseLayer2 = e.IsToggled;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PaintToolWindowI_InUseLayer2 method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/9/2010
        /// <summary>
        /// Sets the current Layer-1 texture to On or Off.
        /// </summary>
        static void PaintToolWindowI_InUseLayer1(object sender, IsToggledEventArgs e)
        {
            try // 6/22/2010
            {
                TerrainAlphaMaps.InUseLayer1 = e.IsToggled;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PaintToolWindowI_InUseLayer1 method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/8/2010
        /// <summary>
        /// Clears the texture layer-2 for <see cref="TerrainAlphaMaps"/>.
        /// </summary>
        static void PaintToolWindowI_ClearLayer2(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                TerrainAlphaMaps.ClearGivenLayer(LayerGroup.Layer2);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PaintToolWindowI_ClearLayer2 method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/8/2010
        /// <summary>
        /// Clears the texture layer-1 for <see cref="TerrainAlphaMaps"/>.
        /// </summary>
        static void PaintToolWindowI_ClearLayer1(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                TerrainAlphaMaps.ClearGivenLayer(LayerGroup.Layer1);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PaintToolWindowI_ClearLayer1 method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/8/2010
        /// <summary>
        /// Sets the new interpolated 'blend' amount for painting.
        /// </summary>
        static void PaintToolWindowI_BlendToUseUpdated(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // get value
                var blendToUse = PaintToolWindowI.BlendToUse;
                TerrainAlphaMaps.SetTextureBlendToUse(blendToUse);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PaintToolWindowI_BlendToUseUpdated method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/8/2010
        /// <summary>
        /// Combines the individual textures chosen, into one 'Texture Atlas' texture.
        /// </summary>
        static void PaintToolWindowI_CreateVolume2(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var storageTool = new Storage();
                ZippedContent zippedContent;
                int textureSize;
                var terrainTextures = TerrainShape.TerrainTextures; // cache

                 // 2/26/2011 - Iterate the enum
                foreach (var terrainTexture in Enum.GetNames(typeof(TerrainTextures)))
                {
                    // Check Texture Quality setting
                    switch (terrainTexture)
                    {
                        case "Tex128X":
                            // 7/17/2009; 11/2/2009: Updated to use the 'ZippedContent' version.
                            zippedContent = new ZippedContent(@"1ContentZipped\ContentTextures_low_x86.xzb",
                                                              TemporalWars3DEngine.GameInstance.Services);
                            textureSize = 128;
                            break;
                        case "Tex256X":
                            // 7/17/2009; 11/2/2009: Updated to use the 'ZippedContent' version.
                            zippedContent = new ZippedContent(@"1ContentZipped\ContentTextures_med_x86.xzb",
                                                              TemporalWars3DEngine.GameInstance.Services);
                            textureSize = 256;
                            break;
                        /*case "High512X":
                            // 7/17/2009; 11/2/2009: Updated to use the 'ZippedContent' version.
                            zippedContent = new ZippedContent(@"1ContentZipped\ContentTextures_high_x86.xzb",
                                                              TemporalWars3DEngine.GameInstance.Services);
                            textureSize = 512;*/
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    // Group 2
                    var count = _terrainShape.TextureGroupData2.Count; // cache
                    if (count > 0)
                    {
                        for (var i = 0; i < count; i++)
                        {
                            var textureImagePath = _terrainShape.TextureGroupData2[i + 4].textureImagePath;
                            var textureName = Path.GetFileNameWithoutExtension(textureImagePath);

                            // load the texture into memory
                            terrainTextures[i + 4] = zippedContent.Load<Texture2D>(textureName);
                        }
                    }

                    //txtMapName.BackColor = Color.White;
                    PaintToolWindowI.SetErrorMessage(string.Empty);

                    // 1/21/2009 - Volume Texture creation; a stack of Texture2D are put into one Texture3D volume!
                    {
                        var elementCount = textureSize*textureSize;
                        const int numberOfTextures = 4;

                        // XNA 4.0 Updates
                        //var volumeTexture = new Texture3D(TemporalWars3DEngine.GameInstance.GraphicsDevice, textureSize, textureSize, numberOfTextures, 1, TextureUsage.None, SurfaceFormat.Color);
                        var volumeTexture = new Texture3D(TemporalWars3DEngine.GameInstance.GraphicsDevice, textureSize, textureSize, numberOfTextures, true, SurfaceFormat.Color);

                        // XNA 4.0 Updates - Color namespace changed.
                        //var tmpData = new Microsoft.Xna.Framework.Graphics.Color[elementCount * numberOfTextures]
                        var tmpData = new Microsoft.Xna.Framework.Color[elementCount * numberOfTextures];

                        terrainTextures[4].GetData(0, null, tmpData, 0, elementCount);
                        terrainTextures[5].GetData(0, null, tmpData, elementCount*1, elementCount);
                        terrainTextures[6].GetData(0, null, tmpData, elementCount*2, elementCount);
                        terrainTextures[7].GetData(0, null, tmpData, elementCount*3, elementCount);
                        volumeTexture.SetData(tmpData);

                        // Layer 2
                        // Set into the Array within TerrainShape class.
                        TerrainShape.TerrainTextureVolumes[1] = volumeTexture;
                        TerrainShape.TerrainTextureVolumeNames[1] = /*txtMapName.Text +*/ "VL2";
                        // 2/1/2010: MapName NOT required anymore.

                        #region Obsolete Texture3D save routines
                        // 11/20/2009 - Get mapType; MP or SP.
                        /*var mapType = string.IsNullOrEmpty(TerrainScreen.TerrainMapGameType)
                                          ? "SP"
                                          : TerrainScreen.TerrainMapGameType;

                        // 2/26/2011 - Updated to use iterate texture name.
                        // 4/9/2010 - Set VL texture name and sub-directory path.
                        //var volumeTexName = "VL2" + TemporalWars3DEngine.TerrainTexturesQuality + ".dds";
                        var volumeTexName = "VL2" + terrainTexture + ".dds";
                        var subDirPath = TemporalWars3DEngine.ContentMapsLoc + @"\" + mapType + @"\" + _txtMapName +
                                         @"\";

                        // XNA 4.0 Updates - DDS not supported anymore; now use JPG
                        // 4/9/2010: Updated to use 'ContentMapsLoc' global var.
                        // 11/3/2009: Updated to add the 'TextureQuality' name to the file name.
                        // Save new Texture Atlas out to disk for use later in game.
                        int errorCode;
                        if (
                            !storageTool.StartTextureSaveOperation(volumeTexture, volumeTexName, subDirPath,
                                                                   ImageFileFormat.Jpeg, out errorCode))
                        {
                            // 4/9/2010 - Error occurred, so check which one.
                            if (errorCode == 1)
                            {
                                MessageBox.Show(
                                    @"Locked files detected for '" + volumeTexName +
                                    @"' save.  Unlock files, and try again.",
                                    @"Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            if (errorCode == 2)
                            {
                                MessageBox.Show(
                                    @"Directory location for '" + volumeTexName +
                                    @"' save, not found.  Verify directory exist, and try again.",
                                    @"Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            MessageBox.Show(
                                @"Invalid Operation error for '" + volumeTexName +
                                @"' save.  Check for file locks, and try again.",
                                @"Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }*/
                        #endregion
                    }
                } // End ForEach


            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PaintToolWindowI_CreateVolume2 method threw the exception " + ex.Message ?? "No Message");
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

        // 7/8/2010
        /// <summary>
        /// Combines the individual textures chosen, into one 'Texture Atlas' texture.
        /// </summary>
        static void PaintToolWindowI_CreateVolume1(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var storageTool = new Storage();
                ZippedContent zippedContent;
                int textureSize;
                var terrainTextures = TerrainShape.TerrainTextures; // cache

                //Debugger.Break();

                // 2/26/2011 - Iterate the enum
                foreach (var terrainTexture in Enum.GetNames(typeof(TerrainTextures)))
                {
                    // Check Texture Quality setting
                    switch (terrainTexture)
                    {
                        case "Tex128X":
                            // 7/17/2009; 11/2/2009: Updated to use the 'ZippedContent' version.
                            zippedContent = new ZippedContent(@"1ContentZipped\ContentTextures_low_x86.xzb", TemporalWars3DEngine.GameInstance.Services);
                            textureSize = 128;
                            break;
                        case "Tex256X":
                            // 7/17/2009; 11/2/2009: Updated to use the 'ZippedContent' version.
                            zippedContent = new ZippedContent(@"1ContentZipped\ContentTextures_med_x86.xzb", TemporalWars3DEngine.GameInstance.Services);
                            textureSize = 256;
                            break;
                        /*case "High512X":
                            // 7/17/2009; 11/2/2009: Updated to use the 'ZippedContent' version.
                            zippedContent = new ZippedContent(@"1ContentZipped\ContentTextures_high_x86.xzb", TemporalWars3DEngine.GameInstance.Services);
                            textureSize = 512;
                            break;*/
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    // Group 1
                    var count = _terrainShape.TextureGroupData1.Count; // cache
                    if (count > 0)
                    {
                        for (var i = 0; i < count; i++)
                        {
                            var textureImagePath = _terrainShape.TextureGroupData1[i].textureImagePath;
                            var textureName = Path.GetFileNameWithoutExtension(textureImagePath);

                            // load the texture into memory
                            terrainTextures[i] = zippedContent.Load<Texture2D>(textureName);
                        }
                    }

                    //txtMapName.BackColor = Color.White;
                    PaintToolWindowI.SetErrorMessage(string.Empty);

                    // 1/21/2009 - Volume Texture creation; a stack of Texture2D are put into one Texture3D volume!
                    {

                        var elementCount = textureSize*textureSize;
                        const int numberOfTextures = 4;

                        // XNA 4.0 Updates - 
                        //var volumeTexture = new Texture3D(TemporalWars3DEngine.GameInstance.GraphicsDevice, textureSize, textureSize, numberOfTextures, 1, TextureUsage.None, SurfaceFormat.Color);
                        var volumeTexture = new Texture3D(TemporalWars3DEngine.GameInstance.GraphicsDevice, textureSize,
                                                          textureSize, numberOfTextures, true, SurfaceFormat.Color);

                        // XNA 4.0 Updates - Color namespace changed.
                        //var tmpData = new Microsoft.Xna.Framework.Graphics.Color[elementCount * numberOfTextures];
                        var tmpData = new Microsoft.Xna.Framework.Color[elementCount*numberOfTextures];

                        terrainTextures[0].GetData(0, null, tmpData, 0, elementCount);
                        terrainTextures[1].GetData(0, null, tmpData, elementCount*1, elementCount);
                        terrainTextures[2].GetData(0, null, tmpData, elementCount*2, elementCount);
                        terrainTextures[3].GetData(0, null, tmpData, elementCount*3, elementCount);
                        volumeTexture.SetData(tmpData);

                        // Layer 1
                        // Set into the Array within TerrainShape class.
                        TerrainShape.TerrainTextureVolumes[0] = volumeTexture;
                        TerrainShape.TerrainTextureVolumeNames[0] = /*txtMapName.Text +*/ "VL1";
                            // 2/1/2010: MapName NOT required anymore.

                    }

                } // End ForEach
                
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PaintToolWindowI_CreateVolume1 method threw the exception " + ex.Message ?? "No Message");
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

        // 7/8/2010
        /// <summary>
        /// Update the proper paint texture's 'AlphaGroup' used for texturing on the terrain.
        /// </summary>
        static void PaintToolWindowI_SelectedItemChangedLv2(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // get index of selected item
                string textureName;
                var index = PaintToolWindowI.GetSelectedItemInLayerGroup2(out textureName);
                if (index == -1) return;

                switch (index)
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
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PaintToolWindowI_SelectedItemChangedLv2 method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/8/2010
        /// <summary>
        /// Update the proper paint texture's 'AlphaGroup' used for texturing on the terrain.
        /// </summary>
        static void PaintToolWindowI_SelectedItemChangedLv1(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // get index of selected item
                string textureName;
                var index = PaintToolWindowI.GetSelectedItemInLayerGroup1(out textureName);
                if (index == -1) return;

                switch (index)
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
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PaintToolWindowI_SelectedItemChangedLv1 method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/8/2010
        static void PaintToolWindowI_DragDropLayer2(object sender, DragDropEventArgs e)
        {
            try // 6/22/2010
            {
                // retrieve path from event args.
                var treeViewDirectoryPath = e.TreeViewDirectoryPath;

                // retrieve index from event args.
                var index = e.Index;

                // 7/7/2010
                if (index == -1)
                {
                    PaintToolWindowI.SetErrorMessage("A item MUST be selected first from list view control.");
                    return;
                }

                // 2nd - load the texture into memory
                TerrainShape.TerrainTextures[index + 4] =
                    TemporalWars3DEngine.ContentGroundTextures.Load<Texture2D>(treeViewDirectoryPath);

                // 3rd - Store the information into List Array
                _terrainShape.TextureGroupData_AddRecord(index, "", "", treeViewDirectoryPath, 2);

                // 7/23/2008
                // 4th - UpdateEffectsTextures on Terrain
                TerrainShape.UpdateEffectDiffuseTextures();
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PaintToolWindowI_DragDropLayer2 method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/8/2010
        static void PaintToolWindowI_DragDropLayer1(object sender, DragDropEventArgs e)
        {
            try // 6/22/2010
            {
                // retrieve path from event args.
                var treeViewDirectoryPath = e.TreeViewDirectoryPath;

                // retrieve index from event args.
                var index = e.Index;

                // 7/7/2010
                if (index == -1)
                {
                    PaintToolWindowI.SetErrorMessage("A item MUST be selected first from list view control.");
                    return;
                }

                // 2nd - load the texture into memory
                TerrainShape.TerrainTextures[index] =
                    TemporalWars3DEngine.ContentGroundTextures.Load<Texture2D>(treeViewDirectoryPath);
                // 7/30/2008 - Also load the BumpMap Texture into memory
                try
                {
                    TerrainShape.TerrainTextureNormals[index] =
                        TemporalWars3DEngine.ContentGroundTextures.Load<Texture2D>(treeViewDirectoryPath + "Normal");
                }
                catch (ContentLoadException)
                {
                    MessageBox.Show(@"No BumpMap available for this texture");
                }

                // 3rd - Store the information into List Array
                _terrainShape.TextureGroupData_AddRecord(index, "", "", treeViewDirectoryPath, 1);

                // 7/23/2008
                // 4th - UpdateEffectsTextures on Terrain
                TerrainShape.UpdateEffectDiffuseTextures();
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PaintToolWindowI_DragDropLayer1 method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/6/2010
        /// <summary>
        /// Rebuilds the AlphaMap for Layer 1 using the new AlphaLy Percents.
        /// </summary>
        static void PaintToolWindowI_RebuildAlphaMap(object sender, EventArgs e)
        {
            TerrainAlphaMaps.UpdateAlphaMap1Layers();

            // 11/20/2009 - Update MiniMap Landscape.               
            _miniMap.RenderLandscapeForMiniMap();
        }

        // 7/6/2010
        /// <summary>
        /// Updates QuadTerrain's AlphaLy4Percent
        /// </summary>
        static void PaintToolWindowI_Layer4PercentUpdated(object sender, EventArgs e)
        {
            _terrainShape.AlphaMaps.AlphaLy4Percent = PaintToolWindowI.Layer4Percent;
        }

        // 7/6/2010
        /// <summary>
        /// Updates QuadTerrain's AlphaLy3Percent
        /// </summary>
        static void PaintToolWindowI_Layer3PercentUpdated(object sender, EventArgs e)
        {
            _terrainShape.AlphaMaps.AlphaLy3Percent = PaintToolWindowI.Layer3Percent;
        }

        // 7/6/2010
        /// <summary>
        /// Updates QuadTerrain's AlphaLy2Percent
        /// </summary>
        static void PaintToolWindowI_Layer2PercentUpdated(object sender, EventArgs e)
        {
            _terrainShape.AlphaMaps.AlphaLy2Percent = PaintToolWindowI.Layer2Percent;
        }

        // 7/6/2010
        /// <summary>
        /// Updates QuadTerrain's AlphaLy1Percent
        /// </summary>
        static void PaintToolWindowI_Layer1PercentUpdated(object sender, EventArgs e)
        {
            _terrainShape.AlphaMaps.AlphaLy1Percent = PaintToolWindowI.Layer1Percent;
        }

        // 7/6/2010
        /// <summary>
        /// Updates the Paint cursors strength.
        /// </summary>
        static void PaintToolWindowI_PaintCursorStrengthUpdated(object sender, EventArgs e)
        {
            TerrainEditRoutines.PaintCursorStrength = PaintToolWindowI.PaintCursorStrength;
            
        }

        // 7/6/2010
        /// <summary>
        /// Updates the Paint size strength.
        /// </summary>
        static void PaintToolWindowI_PaintCursorSizeUpdated(object sender, EventArgs e)
        {
            TerrainEditRoutines.PaintCursorSize = PaintToolWindowI.PaintCursorSize;
        }

        #region EventHandlers
        

        #endregion

        // 7/1/2010
        /// <summary>
        /// Updates the current WPF tool.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            DoPaintToolUpdate();
        }

        // 3/30/2011
        public void CloseForm()
        {
            PaintToolWindowI.Close();

            // Wait for WPF Closed event to trigger before allowing exit of method call.
            _manualResetEventForClosing.WaitOne();
        }

        // 5/7/2008
        // Helper Function which checks if a Point given is within this Window Form's Client Rectangle.        
        // The 'PointToScreen' method is used to convert the Forms' MousePoint to Screen coordinates.  Finally,
        // this is compared using a rectangle, created with this Windows location, and the rectangle's 'Contain'
        // method is called.
        private static bool IsMouseInControl()
        {
            var isIn = false;
            try // 6/20/2010
            {
                if (PaintToolWindowI != null) isIn = PaintToolWindowI.IsMouseOver;
            }
            catch (Exception err)
            {
#if DEBUG
                Debug.WriteLine("TerrainPaintToolRoutines classes IsMouseInControl method threw an exception; " + err.Message ?? "No Message.");
#endif
            }

            return isIn;

        }

        // 7/9/2010
        /// <summary>
        /// Helper method, which checks for the mouse input, and allows doing the Paint fills.
        /// </summary>
        private static void DoPaintToolUpdate()
        {
            try // 6/22/2010
            {
                // Get Mouse Position Input Data
                _mouseState = Mouse.GetState();

                // If left mouse button click, AND ShowCursor=True && PaintTools form is On.
                if (_mouseState.LeftButton == ButtonState.Pressed
                    && TerrainEditRoutines.ToolInUse == ToolType.PaintTool && !IsMouseInControl())
                {
                    _bEditing = true;

                    // Only update if mouse has moved!
                    if (_lastMousePos.X != _mouseState.X && _lastMousePos.Y != _mouseState.Y)
                    {
                        // Save Current Mouse Position
                        _lastMousePos.X = _mouseState.X;
                        _lastMousePos.Y = _mouseState.Y;

                        TerrainPickingRoutines.GetCursorPosByPickedRay(PickRayScale.DivideByTerrainScale, out _cursorPos);

                        // 5/18/2010 - Cache
                        const int pathNodeStride = TemporalWars3DEngine._pathNodeStride;
                        var mapWidthToScale = TerrainData.MapWidthToScale;
                        var mapHeightToScale = TerrainData.MapHeightToScale;
                        var paintCursorSizeXScale = TerrainEditRoutines.PaintCursorSize * TerrainData.cScale;
                        var aStarGraph = TemporalWars3DEngine.AStarGraph;

                        switch (PaintToolWindowI.CurrentTool)
                        {
                            case PaintTool.Fill:

                                // 3/4/2009 - Check if Painting Path finding Block Sections.
                                if (PaintToolWindowI.PaintPathfindingBlocks)
                                {
                                    // 5/18/2010 - Return if null
                                    if (aStarGraph == null) return;

                                    // Get Cursor Pos with NoChange
                                    TerrainPickingRoutines.GetCursorPosByPickedRay(PickRayScale.NoChange, out _cursorPos);

                                    var paintCursorWidth =
                                        (int)(_cursorPos.X < 0
                                                   ? 0
                                                   : _cursorPos.X > mapWidthToScale
                                                         ?
                                                             mapWidthToScale
                                                         : _cursorPos.X +
                                                           paintCursorSizeXScale);
                                    var paintCursorHeight =
                                        (int)(_cursorPos.Z < 0
                                                   ? 0
                                                   : _cursorPos.Z > mapHeightToScale
                                                         ?
                                                             mapHeightToScale
                                                         : _cursorPos.Z +
                                                           paintCursorSizeXScale);
                                    paintCursorWidth -= (int)_cursorPos.X;
                                    paintCursorHeight -= (int)_cursorPos.Z;

                                    // Update all Nodes within paint size rectangle, to the cost of (-1), which means "Blocked".
                                    for (var loopY = 0; loopY < paintCursorHeight; loopY += pathNodeStride)
                                        for (var loopX = 0; loopX < paintCursorWidth; loopX += pathNodeStride)
                                        {
                                            aStarGraph.SetCostToPos(
                                                (int)_cursorPos.X + loopX,
                                                (int)_cursorPos.Z + loopY, -1, 1);
                                        }

                                    // Call update to show change in path node blocking.
                                    TerrainShape.PopulatePathNodesArray();
                                }
                                else
                                {
                                    // Update AlphaMap using PickedTriangle X/Y Coords                            
                                    TerrainAlphaMaps.UpdateAlphaMap_Fill((int)_cursorPos.X, (int)_cursorPos.Z);
                                }

                                break;
                            case PaintTool.Unfill:

                                // 3/4/2009 - Check if Painting Path finding Block Sections.
                                if (PaintToolWindowI.PaintPathfindingBlocks)
                                {
                                    // 5/18/2010 - Return if null
                                    if (aStarGraph == null) return;

                                    TerrainPickingRoutines.GetCursorPosByPickedRay(PickRayScale.NoChange, out _cursorPos);

                                    var paintCursorWidth =
                                        (int)(_cursorPos.X < 0
                                                   ? 0
                                                   : _cursorPos.X > mapWidthToScale
                                                         ?
                                                             mapWidthToScale
                                                         : _cursorPos.X +
                                                           paintCursorSizeXScale);
                                    var paintCursorHeight =
                                        (int)(_cursorPos.Z < 0
                                                   ? 0
                                                   : _cursorPos.Z > mapHeightToScale
                                                         ?
                                                             mapHeightToScale
                                                         : _cursorPos.Z +
                                                           paintCursorSizeXScale);
                                    paintCursorWidth -= (int)_cursorPos.X;
                                    paintCursorHeight -= (int)_cursorPos.Z;

                                    // Update all Nodes within paint size rectangle, to the cost of (-1), which means "Blocked".
                                    for (var loopY = 0; loopY < paintCursorHeight; loopY += pathNodeStride)
                                        for (var loopX = 0; loopX < paintCursorWidth; loopX += pathNodeStride)
                                        {
                                            // 1/13/2010
                                            aStarGraph.RemoveCostAtPos((int)_cursorPos.X + loopX, (int)_cursorPos.Z + loopY, 1);
                                        }

                                    // Call update to show change in path node blocking.
                                    TerrainShape.PopulatePathNodesArray();
                                }
                                else
                                {
                                    // Update AlphaMap using PickedTriangle X/Y Coords                        
                                    TerrainAlphaMaps.UpdateAlphaMap_UnFill((int)_cursorPos.X, (int)_cursorPos.Z);
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
                if (_bEditing && _mouseState.LeftButton == ButtonState.Released)
                {
                    _bEditing = false;
                }

            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("DoPaintToolUpdate method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Builds the an array of <see cref="DirectoryInfo"/> locations, to give to the
        /// WPF form's TreeView.
        /// </summary>
        static void StartPopulateTreeContent()
        {
            // 1/8/2010 - Updated to use new 'PaintToolsTextureDirPath' property.
            // set path to Icon asset images.
            //var visualStudioDir = @"D:\Users\Ben\Documents\Visual Studio 2008";
            //var texturesDirPath = Path.GetDirectoryName(visualStudioDir + @"\\Projects\\XNA_RTS2008\\Dev\\ContentForResources\\ContentTextures\\high512x\\Terrain\\");
            var directoryForTextures = new DirectoryInfo(TemporalWars3DEngine.PaintToolsTexturesDirPath);

            // create root
            var rootLocationsForTree = new DirectoryInfo[1];
            rootLocationsForTree[0] = directoryForTextures;

            // 1/9/2011 - Updated to pass new directory filter.
            // Set into WPF form
            PaintToolWindowI.CreateDataContextForTree(rootLocationsForTree,
                directoryForTextures, s => !Path.GetFileNameWithoutExtension(s.Name).EndsWith("Normal"),
                dir => (!dir.Name.Equals("obj") && !dir.Name.Equals("bin") && !dir.Name.Equals(".Thumbnails")));
        }

        // 7/23/2008 - Made correction in how Nodes are populated; rather than
        //             clear all nodes out, now the Nodes are Inserted at the
        //             exact Index. - This keeps from losing empty positions.
        /// <summary>
        /// Populates the ListView controls, with the textures
        /// in memory from Terrain Class.
        /// </summary>
        private static void PopulateListViewGroups()
        {
            try // 6/22/2010
            {
                // Group 1
                if (_terrainShape.TextureGroupData1.Count > 0)
                {
                    //treeViewGroup1.Nodes.Clear();
                    for (var i = 0; i < _terrainShape.TextureGroupData1.Count; i++)
                    {
                        var textureImagePath = _terrainShape.TextureGroupData1[i].textureImagePath;
                        var textureName = Path.GetFileNameWithoutExtension(textureImagePath);
                        PaintToolWindowI.RemoveItemInLayerGroup1(i);
                        PaintToolWindowI.AddItemToLayerGroup1(textureName, i);
                       
                    }
                }
                
                // Group 2
                if (_terrainShape.TextureGroupData2.Count > 0)
                {
                    //treeViewGroup2.Nodes.Clear();
                    for (var i = 0; i < _terrainShape.TextureGroupData2.Count; i++)
                    {
                        var textureImagePath = _terrainShape.TextureGroupData2[i + 4].textureImagePath;
                        var textureName = Path.GetFileNameWithoutExtension(textureImagePath);
                        PaintToolWindowI.RemoveItemInLayerGroup2(i);
                        PaintToolWindowI.AddItemToLayerGroup2(textureName, i);
                        
                    }
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PopulateTreeViewGroups method threw the exception " + ex.Message ?? "No Message");
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
            // dispose of WPF window
            if (PaintToolWindowI != null)
            {
                PaintToolWindowI.Close();
                PaintToolWindowI = null;
            }
        }
    }
}
