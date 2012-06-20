#region File Description
//-----------------------------------------------------------------------------
// MainMenuTool.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using TWEngine.GameScreens;
using TWEngine.PostProcessEffects.BloomEffect.Enums;
using TWEngine.Shadows;
using TWEngine.Shadows.Enums;
using TWEngine.Terrain;
using TWEngine.Terrain.Enums;
using TWEngine.Water.Enums;
using System.Diagnostics;


namespace TWEngine.TerrainTools
{
    ///<summary>
    /// The <see cref="MainMenuTool"/> class is used to select other game engine editors and tools, and
    /// to set attribute changes quickly; for example, turning on or off the ability to use shadows or water.
    ///</summary>
    public partial class MainMenuTool : Form
    {
        // 12/14/2009 - Water Atts form.
        private WaterAtts _waterAttsForm;

        ///<summary>
        /// Constructor
        ///</summary>
        public MainMenuTool()
        {
            try // 6/22/2010
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuTool constructor threw the exception " + ex.Message ?? "No Message");
#endif
            }

        }

        // 12/7/2009
        /// <summary>
        /// Captures the close event, and cancels the close action; instead, the form is simply hidden
        /// from view by setting the 'Visible' flag to False.  This fixes the issue of the HEAP crash 
        /// when trying to reinstantiate a form, after it was already created in the 'TerrainEditRoutines' class.
        /// </summary>
        private void MainMenuTool_FormClosing(object sender, FormClosingEventArgs e)
        {
            try // 6/22/2010
            {
                e.Cancel = true;
                Visible = false;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuTool_FormClosing method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        #region ShadowMenu

        // 12/7/2009
        /// <summary>
        /// When SubMenu opens up, this will update the 'UseShadows' flag to current setting.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void shadowsToolStripMenuItem1_DropDownOpened(object sender, EventArgs e)

        {
            try // 6/22/2010
            {
                useShadows.Checked = TemporalWars3DEngine.EngineGameConsole.ShadowMap.IsVisible;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("shadowsToolStripMenuItem1_DropDownOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Use Shadows.
        /// </summary>
        private void useShadows_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Update the ShadowMap Visible flag.
                TemporalWars3DEngine.EngineGameConsole.ShadowMap.IsVisible = useShadows.Checked;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("useShadows_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Turn Shadows Debugging on.
        /// </summary>
        private void showShadowMap_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Update the DebugValues flag.
                TemporalWars3DEngine.EngineGameConsole.ShadowMap.DebugValues = showShadowMap.Checked;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("showShadowMap_Click method threw the exception " + ex.Message ?? "No Message");
#endif  
            }
            
        }

        /// <summary>
        /// Set to Low shadow quality.
        /// </summary>
        private void low1024x_Click(object sender, EventArgs e)
        {
            if (!low1024x.Checked) return;

            try // 6/22/2010
            {
                // Set to Low setting.
                ShadowMap.ShadowQuality = ShadowQuality.Low;
                ShadowMap.InitializeRenderTargets();
                medium2048x.Checked = false;
                high4096x.Checked = false;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("low1024x_Click method threw the exception " + ex.Message ?? "No Message");
#endif  
            }
        }

        /// <summary>
        /// Set to Medium shadow quality.
        /// </summary>
        private void medium2048x_Click(object sender, EventArgs e)
        {
            if (!medium2048x.Checked) return;

            try // 6/22/2010
            {
                // Set to Low setting.
                ShadowMap.ShadowQuality = ShadowQuality.Medium;
                ShadowMap.InitializeRenderTargets();
                low1024x.Checked = false;
                high4096x.Checked = false;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("medium2048x_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Set to High shadow quality.
        /// </summary>
        private void high4096x_Click(object sender, EventArgs e)
        {
            if (!high4096x.Checked) return;

            try // 6/22/2010
            {
                // Set to Low setting.
                ShadowMap.ShadowQuality = ShadowQuality.High;
                ShadowMap.InitializeRenderTargets();
                medium2048x.Checked = false;
                low1024x.Checked = false;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("high4096x_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }
       
        /// <summary>
        /// This is clicked right before user sees the options to choose; therefore,
        /// the current ShadowQuality setting is updated.
        /// </summary>
        private void shadowQualityToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Set the current ShadowQuality setting.
                switch (ShadowMap.ShadowQuality)
                {
                    case ShadowQuality.Low:
                        low1024x.Checked = true;
                        medium2048x.Checked = false;
                        high4096x.Checked = false;
                        break;
                    case ShadowQuality.Medium:
                        low1024x.Checked = false;
                        medium2048x.Checked = true;
                        high4096x.Checked = false;
                        break;
                    case ShadowQuality.High:
                        low1024x.Checked = false;
                        medium2048x.Checked = false;
                        high4096x.Checked = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("shadowQualityToolStripMenuItem_DropDownOpened method threw the exception " + ex.Message ?? "No Message");
#endif 
            }
        }

        // 12/7/2009
        /// <summary>
        /// Rebuilds the STATIC ShadowMap.
        /// </summary>
        private void rebuildStaticMap_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                ShadowMap.DoPreShadowMapTextures = true;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("rebuildStaticMap_Click method threw the exception " + ex.Message ?? "No Message");
#endif  
            }
        }

        // 12/12/2009
        /// <summary>
        /// When clicked, will set the proper ShadowType state for the menu option.
        /// </summary>
        private void shadowTypeToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                SetShadowType(ShadowMap.UseShadowType);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("shadowTypeToolStripMenuItem_DropDownOpened method threw the exception " + ex.Message ?? "No Message");
#endif   
            }
        }


        // 12/12/2009
        /// <summary>
        /// Set the ShadowType to use to be 'Simple'.
        /// </summary>
        private void simpleShadowType_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                SetShadowType(ShadowMap.ShadowType.Simple);
                ShadowMap.UseShadowType = ShadowMap.ShadowType.Simple;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("simpleShadowType_Click method threw the exception " + ex.Message ?? "No Message");
#endif 
            }
        }

        // 12/12/2009
        /// <summary>
        /// Set the ShadowType to use to be 'PCF#1'.
        /// </summary>
        private void PCFShadowType_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                SetShadowType(ShadowMap.ShadowType.PercentageCloseFilter_1);
                ShadowMap.UseShadowType = ShadowMap.ShadowType.PercentageCloseFilter_1;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PCFShadowType_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 6/12/2010
        /// <summary>
        /// Set the ShadowType to use to be 'PCF#2'.
        /// </summary>
        private void PCF2ShadowType_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                SetShadowType(ShadowMap.ShadowType.PercentageCloseFilter_2);
                ShadowMap.UseShadowType = ShadowMap.ShadowType.PercentageCloseFilter_2;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PCF2ShadowType_Click method threw the exception " + ex.Message ?? "No Message");
#endif 
            }
        }

        // 12/12/2009
        /// <summary>
        /// Set the ShadowType to use to be 'Variance'.
        /// </summary>
        private void varianceShadowType_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                SetShadowType(ShadowMap.ShadowType.Variance);
                ShadowMap.UseShadowType = ShadowMap.ShadowType.Variance;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("varianceShadowType_Click method threw the exception " + ex.Message ?? "No Message");
#endif 
            }
        }

        // 12/12/2009; 6/12/2010 - Updated with PCF#2.
        /// <summary>
        ///  Helper method, which sets the proper ShadowType.
        /// </summary>
        private void SetShadowType(ShadowMap.ShadowType shadowType)
        {
            try // 6/22/2010
            {
                switch (shadowType)
                {
                    case ShadowMap.ShadowType.Simple:
                        simpleShadowType.Checked = true;
                        PCF1ShadowType.Checked = false;
                        PCF2ShadowType.Checked = false;
                        varianceShadowType.Checked = false;
                        break;
                    case ShadowMap.ShadowType.PercentageCloseFilter_1:
                        simpleShadowType.Checked = false;
                        PCF1ShadowType.Checked = true;
                        PCF2ShadowType.Checked = false;
                        varianceShadowType.Checked = false;
                        break;
                    case ShadowMap.ShadowType.PercentageCloseFilter_2:
                        simpleShadowType.Checked = false;
                        PCF1ShadowType.Checked = false;
                        PCF2ShadowType.Checked = true;
                        varianceShadowType.Checked = false;
                        break;
                    case ShadowMap.ShadowType.Variance:
                        simpleShadowType.Checked = false;
                        PCF1ShadowType.Checked = false;
                        PCF2ShadowType.Checked = false;
                        varianceShadowType.Checked = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("SetShadowType method threw the exception " + ex.Message ?? "No Message");
#endif  
            }
        }

        // 12/12/2009
        /// <summary>
        /// Allows user to set the ShadowDarkness level.
        /// </summary>
        private void shadowDarknessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Displays the ShadowMap Darkness Group
                gbShadowDarkness.Visible = true;

                // Set current Darkness value into the scroll bar
                tbShadowDarkness.Value = (int)(ShadowMap.ShadowMapDarkness * 100);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("shadowDarknessToolStripMenuItem_Click method threw the exception " + ex.Message ?? "No Message");
#endif 
            }
        }

        // 12/12/2009
        /// <summary>
        /// Eventhandler for when the user moves the scroll bar.
        /// </summary>
        private void tbShadowDarkness_Scroll(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // Update into ShadowMap component.
                ShadowMap.ShadowMapDarkness = MathHelper.Clamp(tbShadowDarkness.Value / 100.0f, 0, 1.0f);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("tbShadowDarkness_Scroll method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/12/2009
        /// <summary>
        /// Hides the ShadowMapDarkness group section.
        /// </summary>
        private void btnDone_Click(object sender, EventArgs e)
        {
            try // 6/22/2010            
            {
                gbShadowDarkness.Visible = false;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnDone_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        #endregion

        #region FOWMenu

        /// <summary>
        /// When clicked, will set the proper FOW 'IsVisible' state for the menu option.
        /// </summary>
        private void fogOfWarToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // 3/23/2010 - Check if FOW is null.
                if (TemporalWars3DEngine.EngineGameConsole.FOW != null)
                    useFogOfWar.Checked = TemporalWars3DEngine.EngineGameConsole.FOW.IsVisible;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("fogOfWarToolStripMenuItem_DropDownOpened method threw the exception " + ex.Message ?? "No Message");
#endif 
            }
        }

        /// <summary>
        /// Set the Fog-Of-War Visible.
        /// </summary>
        private void useFogOfWar_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // 3/23/2010 - Check if FOW is null.
                if (TemporalWars3DEngine.EngineGameConsole.FOW != null)
                    TemporalWars3DEngine.EngineGameConsole.FOW.IsVisible = useFogOfWar.Checked;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("useFogOfWar_Click method threw the exception " + ex.Message ?? "No Message");
#endif  
            }
        }

        #endregion

        #region BloomMenu

        /// <summary>
        /// When Bloom's SubMenu opens, this will update the 'UseBloom' flag.
        /// </summary>
        private void bloomToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                useBloom.Checked = TemporalWars3DEngine.EngineGameConsole.ScreenManager.UseBloom;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("bloomToolStripMenuItem_DropDownOpened method threw the exception " + ex.Message ?? "No Message");
#endif  
            }
        }

        /// <summary>
        /// When clicked, sets the UseBloom.
        /// </summary>
        private void useBloom_Click(object sender, EventArgs e)
        {
            try
            {
                TemporalWars3DEngine.EngineGameConsole.ScreenManager.UseBloom = useBloom.Checked;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("useBloom_Click method threw the exception " + ex.Message ?? "No Message");
#endif 
            }
        }

        /// <summary>
        /// When BloomSetting's SubMenu opens, this will set the proper 'current' bloom setting flag.
        /// </summary>
        private void bloomSettingToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var bloomSetting = (int)TemporalWars3DEngine.EngineGameConsole.ScreenManager.BloomSetting;

                defaultBloom.Checked = (0 == bloomSetting);
                softBloom.Checked = (1 == bloomSetting);
                deSaturatedBloom.Checked = (2 == bloomSetting);
                saturatedBloom.Checked = (3 == bloomSetting);
                blurryBloom.Checked = (4 == bloomSetting);
                subtleBloom.Checked = (5 == bloomSetting);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("bloomSettingToolStripMenuItem_DropDownOpened method threw the exception " + ex.Message ?? "No Message");
#endif 
            }
            
        }

        /// <summary>
        /// Sets the Bloom to use the 'Default' setting.
        /// </summary>
        private void defaultBloom_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                TemporalWars3DEngine.EngineGameConsole.ScreenManager.BloomSetting = BloomType.Default;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("defaultBloom_Click method threw the exception " + ex.Message ?? "No Message");
#endif 
            }
        }

        /// <summary>
        /// Sets the Bloom to use the 'Default' setting.
        /// </summary>
        private void softBloom_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                TemporalWars3DEngine.EngineGameConsole.ScreenManager.BloomSetting = BloomType.Soft;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("softBloom_Click method threw the exception " + ex.Message ?? "No Message");
#endif 
            }
        }

        /// <summary>
        /// Sets the Bloom to use the 'DeSaturated' setting.
        /// </summary>
        private void deSaturatedBloom_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                TemporalWars3DEngine.EngineGameConsole.ScreenManager.BloomSetting = BloomType.DeSaturated;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("deSaturatedBloom_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Sets the Bloom to use the 'Bloom' setting.
        /// </summary>
        private void saturatedBloom_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                TemporalWars3DEngine.EngineGameConsole.ScreenManager.BloomSetting = BloomType.Saturated;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("saturatedBloom_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Sets the Bloom to use the 'Blurry' setting.
        /// </summary>
        private void blurryBloom_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                TemporalWars3DEngine.EngineGameConsole.ScreenManager.BloomSetting = BloomType.Blurry;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("blurryBloom_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Sets the Bloom to use the 'Subtle' setting.
        /// </summary>
        private void subtleBloom_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                TemporalWars3DEngine.EngineGameConsole.ScreenManager.BloomSetting = BloomType.Subtle;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("subtleBloom_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        #endregion

        #region GlowMenu

        /// <summary>
        ///  When Glow's SubMenu opens, this will update the 'UseGlow' flag.
        /// </summary>
        private void glowToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                useGlow.Checked = TemporalWars3DEngine.EngineGameConsole.ScreenManager.UseGlow;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("glowToolStripMenuItem_DropDownOpening method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Sets the UseGlow flag.
        /// </summary>
        private void useGlow_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                TemporalWars3DEngine.EngineGameConsole.ScreenManager.UseGlow = useGlow.Checked;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("useGlow_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

#endregion

        #region SkyBoxMenu

        /// <summary>
        ///  When Skyboxes SubMenu opens, this will update the 'UseSkyBox' flag.
        /// </summary>
        private void skyBoxToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                useSkyBox.Checked = TemporalWars3DEngine.EngineGameConsole.ScreenManager.UseSkyBox;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("skyBoxToolStripMenuItem_DropDownOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Sets the UseSkyBox flag.
        /// </summary>
        private void useSkyBox_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                TemporalWars3DEngine.EngineGameConsole.ScreenManager.UseSkyBox = useSkyBox.Checked;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("useSkyBox_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        #endregion

        #region CoreToolsMenu

        // 12/7/2009
        /// <summary>
        /// Activates the HeightTool form.
        /// </summary>
        private void btnHeightTool_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                TerrainEditRoutines.ActivateTool(ToolType.HeightTool);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnHeightTool_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/7/2009
        /// <summary>
        /// Activates the PaintTool form.
        /// </summary>
        private void btnPaintTool_Click(object sender, EventArgs e)
        {
            try // /6/22/2010
            {
                TerrainEditRoutines.ActivateTool(ToolType.PaintTool);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnPaintTool_Click method threw the exception " + ex.Message ?? "No Message");
#endif 
            }
        }

        // 12/7/2009
        /// <summary>
        /// Activates the ItemTool form.
        /// </summary>
        private void btnItemsTool_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                TerrainEditRoutines.ActivateTool(ToolType.ItemTool);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnItemsTool_Click method threw the exception " + ex.Message ?? "No Message");
#endif  
            }
        }

        // 12/7/2009
        /// <summary>
        /// Activates the PropertiesTool form.
        /// </summary>
        private void btnPropertiesTool_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                TerrainEditRoutines.ActivateTool(ToolType.PropertiesTool);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnPropertiesTool_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        #endregion

        #region MiniMapMenu

        // 12/7/2009
        /// <summary>
        /// When clicked, will set the proper MiniMap 'IsVisible' state for the menu option,
        /// and the 'Show Wrapper' menu option.
        /// </summary>
        private void miniMapToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            // 1/2/2010
            if (TemporalWars3DEngine.EngineGameConsole.MiniMap == null) return;

            try // 6/22/2010
            {
                useMiniMap.Checked = TemporalWars3DEngine.EngineGameConsole.MiniMap.IsVisible;
                showWrapper.Checked = TemporalWars3DEngine.EngineGameConsole.MiniMap.ShowTextureWrapper;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("miniMapToolStripMenuItem_DropDownOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Set the Minimap Visible.
        /// </summary>
        private void useMiniMap_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                if (TemporalWars3DEngine.EngineGameConsole.MiniMap != null)
                    TemporalWars3DEngine.EngineGameConsole.MiniMap.IsVisible = useMiniMap.Checked;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("useMiniMap_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Allows turning On/Off the Minimap's texture wrapper.
        /// </summary>
        private void showWrapper_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                if (TemporalWars3DEngine.EngineGameConsole.MiniMap != null)
                    TemporalWars3DEngine.EngineGameConsole.MiniMap.ShowTextureWrapper = showWrapper.Checked;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("showWrapper_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/7/2009
        /// <summary>
        /// Creates a MiniMap Textures, using the given terrain map, and
        /// saves to the hard-drive.
        /// </summary>
        private void createMMTexture_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var result = MessageBox.Show(@"This will create a new MiniMap Texture for the given Terrain.  Do you want to continue?",
                                @"Create MM Texture", MessageBoxButtons.YesNo);

                if (result != DialogResult.Yes) return;

                // 12/7/2009 - Get Map Name.
                var name = TerrainScreen.TerrainMapToLoad;
                var miniMap = (IMinimap)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(IMinimap));
                if (miniMap != null) miniMap.SaveMiniMapTexture(name);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("createMMTexture_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        #endregion

        #region IFDMenu

        // 12/8/2009
        /// <summary>
        /// When clicked, will set the proper IFD 'IsVisible' state for the menu option.
        /// </summary>
        private void iFDToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                useIFD.Checked = TemporalWars3DEngine.EngineGameConsole.IFD.IsVisible;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("iFDToolStripMenuItem_DropDownOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Set the IFD Visible.
        /// </summary>
        private void useIFD_Click(object sender, EventArgs e)
        {
            try
            {
                TemporalWars3DEngine.EngineGameConsole.IFD.IsVisible = useIFD.Checked;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("useIFD_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        #endregion

        #region NormalMapMenu

        // 12/9/2009
        /// <summary>
        /// When clicked, will set the proper NormalMap 'IsVisible' state for the menu option.
        /// </summary>
        private void normalMapToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            try
            {
                useNormalMap.Checked = TerrainShape.EnableNormalMap;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("normalMapToolStripMenuItem_DropDownOpened method threw the exception " + ex.Message ?? "No Message");
#endif  
            }
        }

        /// <summary>
        /// Set the NormalMap Visible.
        /// </summary>
        private void useNormalMap_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                TerrainShape.EnableNormalMap = useNormalMap.Checked;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("useNormalMap_Click method threw the exception " + ex.Message ?? "No Message");
#endif   
            }
        }

        #endregion

        #region LightingTypeMenu

        // 12/12/2009
        /// <summary>
        /// When clicked, will set the proper LightingType state for the menu option.
        /// </summary>
        private void lightingTypeToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                SetLightingType(TerrainShape.LightingType);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("lightingTypeToolStripMenuItem_DropDownOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/12/2009
        /// <summary>
        /// Set the Plastic LightingType.
        /// </summary>
        private void plasticLightingType_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                TerrainShape.LightingType = TerrainLightingType.Plastic;
                SetLightingType(TerrainLightingType.Plastic);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("plasticLightingType_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/12/2009
        /// <summary>
        /// Set the Metal LightingType.
        /// </summary>
        private void metalLightingType_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                TerrainShape.LightingType = TerrainLightingType.Metal;
                SetLightingType(TerrainLightingType.Metal);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("metalLightingType_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/12/2009
        /// <summary>
        /// Set the Blinn LightingType.
        /// </summary>
        private void blinnLightingType_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                TerrainShape.LightingType = TerrainLightingType.Blinn;
                SetLightingType(TerrainLightingType.Blinn);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("blinnLightingType_Click method threw the exception " + ex.Message ?? "No Message");
#endif 
            }
        }

        // 12/12/2009
        /// <summary>
        /// Set the Glossy LightingType.
        /// </summary>
        private void glossyLightingType_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                TerrainShape.LightingType = TerrainLightingType.Glossy;
                SetLightingType(TerrainLightingType.Glossy);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("glossyLightingType_Click method threw the exception " + ex.Message ?? "No Message");
#endif 
            }
        }

        // 12/12/009
        /// <summary>
        /// Helper method, used to set the proper LightingType.
        /// </summary>
        private void SetLightingType(TerrainLightingType lightingType)
        {
            try // 6/22/2010
            {
                switch (lightingType)
                {
                    case TerrainLightingType.Plastic:
                        plasticLightingType.Checked = true;
                        metalLightingType.Checked = false;
                        blinnLightingType.Checked = false;
                        glossyLightingType.Checked = false;
                        break;
                    case TerrainLightingType.Metal:
                        plasticLightingType.Checked = false;
                        metalLightingType.Checked = true;
                        blinnLightingType.Checked = false;
                        glossyLightingType.Checked = false;
                        break;
                    case TerrainLightingType.Blinn:
                        plasticLightingType.Checked = false;
                        metalLightingType.Checked = false;
                        blinnLightingType.Checked = true;
                        glossyLightingType.Checked = false;
                        break;
                    case TerrainLightingType.Glossy:
                        plasticLightingType.Checked = false;
                        metalLightingType.Checked = false;
                        blinnLightingType.Checked = false;
                        glossyLightingType.Checked = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("SetLightingType method threw the exception " + ex.Message ?? "No Message");
#endif  
            }
        }

        #endregion

        #region WaterMenu

        // 12/14/2009
        /// <summary>
        /// When clicked, will set the proper Water state for the menu options.
        /// </summary>
        private void toolStripMenuItem1_DropDownOpened(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                useWater.Checked = TemporalWars3DEngine.EngineGameConsole.Water.IsVisible;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("toolStripMenuItem1_DropDownOpened method threw the exception " + ex.Message ?? "No Message");
#endif 
            }
        }

        // 6/1/2010
        /// <summary>
        /// Allows setting the 'IsVisible' flag in the WaterManager.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void isVisible_Click(object sender, EventArgs e)
        {
            try
            {
                TemporalWars3DEngine.EngineGameConsole.Water.IsVisible = isVisible.Checked;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("isVisible_Click method threw the exception " + ex.Message ?? "No Message");
#endif  
            }
        }

        // 6/1/2010
        /// <summary>
        /// Updates the <see cref="WaterType"/> Enum to 'None'.
        /// </summary>
        private void waterType_None_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // set other 2 menu options to off.
                waterType_Lake.Checked = false;
                waterType_Ocean.Checked = false;

                // update WaterManager.
                TemporalWars3DEngine.EngineGameConsole.Water.WaterTypeToUse = WaterType.None;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("waterType_None_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 6/1/2010
        /// <summary>
        /// Updates the <see cref="WaterType"/> Enum to 'Lake'.
        /// </summary>
        private void waterType_Lake_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // set other 2 menu options to off.
                waterType_None.Checked = false;
                waterType_Ocean.Checked = false;

                // update WaterManager.
                TemporalWars3DEngine.EngineGameConsole.Water.WaterTypeToUse = WaterType.Lake;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("waterType_Lake_Click method threw the exception " + ex.Message ?? "No Message");
#endif 
            }
        }

        // 6/1/2010
        /// <summary>
        /// Updates the <see cref="WaterType"/> Enum to 'Ocean'.
        /// </summary>
        private void waterType_Ocean_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // set other 2 menu options to off.
                waterType_Lake.Checked = false;
                waterType_None.Checked = false;

                // update WaterManager.
                TemporalWars3DEngine.EngineGameConsole.Water.WaterTypeToUse = WaterType.Ocean;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("waterType_Ocean_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/14/2009
        /// <summary>
        /// Turns on the GameViewPort display, to show one of the 3 waterMap textures
        /// for debug purposes.
        /// </summary>
        private void showWaterMap_Click(object sender, EventArgs e)
        {
            try
            {
                TemporalWars3DEngine.EngineGameConsole.GVP.Visible = showWaterMap.Checked;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("showWaterMap_Click method threw the exception " + ex.Message ?? "No Message");
#endif 
            }
        }

        // 12/14/2009
        /// <summary>
        /// When clicked, will check mark the proper subMenu option.
        /// </summary>
        private void waterMapTypeToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            try
            {
                SetWaterMapType(TemporalWars3DEngine.EngineGameConsole.Water.ShowTexture);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("waterMapTypeToolStripMenuItem_DropDownOpened method threw the exception " + ex.Message ?? "No Message");
#endif  
            }
        }

        // 12/14/2009
        /// <summary>
        /// Sets the WaterMap type to view in the GameViewPort.
        /// </summary>
        private void reflectionMap_Click(object sender, EventArgs e)
        {
            try // /6/22/2010
            {
                TemporalWars3DEngine.EngineGameConsole.Water.ShowTexture = ViewPortTexture.Reflection;
                SetWaterMapType(ViewPortTexture.Reflection);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("reflectionMap_Click method threw the exception " + ex.Message ?? "No Message");
#endif 
            }
        }

        // 12/14/2009
        /// <summary>
        /// Sets the WaterMap type to view in the GameViewPort.
        /// </summary>
        private void refractionMap_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                TemporalWars3DEngine.EngineGameConsole.Water.ShowTexture = ViewPortTexture.Refraction;
                SetWaterMapType(ViewPortTexture.Refraction);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("refractionMap_Click method threw the exception " + ex.Message ?? "No Message");
#endif 
            }
        }

        // 12/14/2009
        /// <summary>
        /// Sets the WaterMap type to view in the GameViewPort.
        /// </summary>
        private void bumpMap_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                TemporalWars3DEngine.EngineGameConsole.Water.ShowTexture = ViewPortTexture.Bump;
                SetWaterMapType(ViewPortTexture.Bump);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("bumpMap_Click method threw the exception " + ex.Message ?? "No Message");
#endif 
            }
        }


        // 12/14/2009
        /// <summary>
        /// Helper method, which updates the WaterMap texture to view.
        /// </summary>
        /// <param name="waterMapType"></param>
        private void SetWaterMapType(ViewPortTexture waterMapType)
        {
            try // 6/22/2010
            {
                switch (waterMapType)
                {
                    case ViewPortTexture.Refraction:
                        refractionMap.Checked = true;
                        reflectionMap.Checked = false;
                        bumpMap.Checked = false;
                        break;
                    case ViewPortTexture.Reflection:
                        refractionMap.Checked = false;
                        reflectionMap.Checked = true;
                        bumpMap.Checked = false;
                        break;
                    case ViewPortTexture.Bump:
                        refractionMap.Checked = false;
                        reflectionMap.Checked = false;
                        bumpMap.Checked = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("SetWaterMapType method threw the exception " + ex.Message ?? "No Message");
#endif  
            }
        }

        // 12/14/2009
        /// <summary>
        /// Opens up the WaterAtts form.
        /// </summary>
        private void editWaterAttributes_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                if (_waterAttsForm == null) _waterAttsForm = new WaterAtts();
                _waterAttsForm.Visible = true;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("editWaterAttributes_Click method threw the exception " + ex.Message ?? "No Message");
#endif 
            }
            
        }

        #endregion

       
       
// ReSharper restore InconsistentNaming

    }
}
