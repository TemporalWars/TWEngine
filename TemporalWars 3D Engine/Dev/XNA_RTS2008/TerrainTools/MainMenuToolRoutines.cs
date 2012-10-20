#region File Description
//-----------------------------------------------------------------------------
// MainMenuToolRoutines.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.GameScreens;
using ImageNexus.BenScharbach.TWEngine.PostProcessEffects.BloomEffect.Enums;
using ImageNexus.BenScharbach.TWEngine.Shadows;
using ImageNexus.BenScharbach.TWEngine.Shadows.Enums;
using ImageNexus.BenScharbach.TWEngine.Terrain;
using ImageNexus.BenScharbach.TWEngine.Terrain.Enums;
using ImageNexus.BenScharbach.TWEngine.Utilities;
using ImageNexus.BenScharbach.TWEngine.Water.Enums;
using ImageNexus.BenScharbach.TWLate.RTS_FogOfWarInterfaces.FOW;
using ImageNexus.BenScharbach.TWLate.RTS_MinimapInterfaces.Minimap;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.Delegates;
using Microsoft.Xna.Framework;
using TWEngine.TerrainTools;

namespace ImageNexus.BenScharbach.TWEngine.TerrainTools
{
    // 8/17/2010
    /// <summary>
    /// The <see cref="MainMenuToolRoutines"/> class contains all necessary methods and event handlers to
    /// connect to the WPF MainMenuTool form.
    /// </summary>
    internal class MainMenuToolRoutines : IDisposable
    {
        internal static MainMenuWindow MainMenuWindowI { get; private set; }

        // Water Atts form.
        private static WaterAtts _waterAttsForm;

        // 3/30/2011 - ManualResetEvent
        private static ManualResetEvent _manualResetEventForClosing;

#if WithLicense
        // 5/10/2012 - License
        private static readonly LicenseHelper LicenseInstance;
#endif

        /// <summary>
        /// Static Constructor
        /// </summary>
        static MainMenuToolRoutines()
        {
#if WithLicense
            // 5/10/2012 Check for Valid License.
            LicenseInstance = new LicenseHelper();
#endif
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MainMenuToolRoutines()
        {
            // 11/19/2009 - Need to turn of FOW, otherwise, blinking will occur.
            var fogOfWar = (IFogOfWar)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(IFogOfWar));
            if (fogOfWar != null) fogOfWar.IsVisible = false;

            // 3/30/2011
            _manualResetEventForClosing = new ManualResetEvent(false);

            // 1/11/2011
            CreateMainMenuWindow();
        }

        // 1/11/2011
        private static void CreateMainMenuWindow()
        {
            MainMenuWindowI = new MainMenuWindow();

            // 8/17/2010
            // Connect Events to event handlers.
            ConnectEventHandlers();
        }

        // 8/17/2010
        /// <summary>
        /// Connects all required event handlers to WPF form.
        /// </summary>
        private static void ConnectEventHandlers()
        {
            // Bloom Menu items
            MainMenuWindowI.UseBloom += MainMenuWindowI_UseBloom;
            MainMenuWindowI.BloomTypeUpdated += MainMenuWindowI_BloomTypeUpdated;
            MainMenuWindowI.BloomSettingsSubMenuOpened += MainMenuWindowI_BloomSettingsSubMenuOpened;
            MainMenuWindowI.BloomSubMenuOpened += MainMenuWindowI_BloomSubMenuOpened;
           
            // FOW Menu Item
            MainMenuWindowI.UseFogOfWar += MainMenuWindowI_UseFogOfWar;
            MainMenuWindowI.FogOfWarSubMenuOpened += MainMenuWindowI_FogOfWarSubMenuOpened;
            // Glow Menu Item
            MainMenuWindowI.UseGlow += MainMenuWindowI_UseGlow;
            MainMenuWindowI.GlowSubMenuOpened += MainMenuWindowI_GlowSubMenuOpened;
            // IFD Menu Item
            MainMenuWindowI.UseIFD += MainMenuWindowI_UseIFD;
            MainMenuWindowI.IFDSubMenuOpened += MainMenuWindowI_IFDSubMenuOpened;
            // LightingType Menu Items
            MainMenuWindowI.LightingTypeUpdated += MainMenuWindowI_LightingTypeUpdated;
            MainMenuWindowI.LightingTypeSubMenuOpened += MainMenuWindowI_LightingTypeSubMenuOpened;
            // MiniMap Menu Items
            MainMenuWindowI.UseMiniMap += MainMenuWindowI_UseMiniMap;
            MainMenuWindowI.ShowMiniMapWrapper += MainMenuWindowI_ShowMiniMapWrapper;
            MainMenuWindowI.CreateMiniMapTexture += MainMenuWindowI_CreateMiniMapTexture;
            MainMenuWindowI.MiniMapSubMenuOpened += MainMenuWindowI_MiniMapSubMenuOpened;
            // NormmalMap Menu Item
            MainMenuWindowI.UseNormalMap += MainMenuWindowI_UseNormalMap;
            MainMenuWindowI.NormalMapSubMenuOpened += MainMenuWindowI_NormalMapSubMenuOpened;
            // Shadows Menu Items
            MainMenuWindowI.UseShadows += MainMenuWindowI_UseShadows;
            MainMenuWindowI.ShowShadowMap += MainMenuWindowI_ShowShadowMap;
            MainMenuWindowI.ShadowTypeUpdated += MainMenuWindowI_ShadowTypeUpdated;
            MainMenuWindowI.ShadowQualityUpdated += MainMenuWindowI_ShadowQualityUpdated;
            MainMenuWindowI.RebuildStaticShadowMap += MainMenuWindowI_RebuildStaticShadowMap;
            MainMenuWindowI.ShadowDarknessPanel += MainMenuWindowI_ShadowDarknessPanel;
            MainMenuWindowI.ShadowMapDarknessUpdated += MainMenuWindowI_ShadowMapDarknessUpdated;
            MainMenuWindowI.ShadowMapSubMenuOpened += MainMenuWindowI_ShadowMapSubMenuOpened;
            MainMenuWindowI.ShadowMapTypeSubMenuOpened += MainMenuWindowI_ShadowMapTypeSubMenuOpened;
            MainMenuWindowI.ShadowMapQualitySubMenuOpened += MainMenuWindowI_ShadowMapQualitySubMenuOpened;
            // Skybox Menu Item
            MainMenuWindowI.UseSkyBox += MainMenuWindowI_UseSkyBox;
            MainMenuWindowI.SkyBoxSubMenuOpened += MainMenuWindowI_SkyBoxSubMenuOpened;
            // Water Menu Items
            MainMenuWindowI.IsWaterVisible += MainMenuWindowI_IsWaterVisible;
            MainMenuWindowI.WaterTypeUpdated += MainMenuWindowI_WaterTypeUpdated;
            MainMenuWindowI.ShowWaterMap += MainMenuWindowI_ShowWaterMap;
            MainMenuWindowI.WaterMapTypeUpdated += MainMenuWindowI_WaterMapTypeUpdated;
            MainMenuWindowI.EditWaterAtts += MainMenuWindowI_EditWaterAtts;
            MainMenuWindowI.WaterSubMenuOpened += MainMenuWindowI_WaterSubMenuOpened;
            MainMenuWindowI.WaterTypeSubMenuOpened += MainMenuWindowI_WaterTypeSubMenuOpened;
            MainMenuWindowI.WaterMapSubMenuOpened += MainMenuWindowI_WaterMapSubMenuOpened;
            // Perlin Menu Items (1/10/2011)
            MainMenuWindowI.UsePerlinClouds += MainMenuWindowI_UsePerlinClouds;
            MainMenuWindowI.PerlinCloudsSubMenuOpened += MainMenuWindowI_PerlinCloudsSubMenuOpened;
            // DebugShader Menu Items (5/28/2012)
            MainMenuWindowI.UseDebugShader += MainMenuWindowI_UseDebugShader;
            MainMenuWindowI.DebugShaderSubMenuOpened += MainMenuWindowI_DebugShaderSubMenuOpened;
            MainMenuWindowI.UseDrawCollisionsScenaryItems += MainMenuWindowI_UseDrawCollisionsScenaryItems;
            MainMenuWindowI.UseDrawCollisionsPlayableItems += MainMenuWindowI_UseDrawCollisionsPlayableItems;
            // Core Menu Items
            MainMenuWindowI.StartHeightTool += MainMenuWindowI_StartHeightTool;
            MainMenuWindowI.StartItemsTool += MainMenuWindowI_StartItemsTool;
            MainMenuWindowI.StartPaintTool += MainMenuWindowI_StartPaintTool;
            MainMenuWindowI.StartPropertiesTool += MainMenuWindowI_StartPropertiesTool;
            // 1/11/2011 - FormClosed
            MainMenuWindowI.FormClosed += MainMenuWindowI_FormClosed;
            // 3/30/2011 - FormStartClose
            MainMenuWindowI.FormStartClose += MainMenuWindowI_FormStartClose;

        }

        // 3/30/2011
        /// <summary>
        /// Occurs when the WPF form is starting the close cycle.
        /// </summary>
        static void MainMenuWindowI_FormStartClose(object sender, EventArgs e)
        {
            // Set ToolType window to start close cycle
            TerrainWPFTools.ToolTypeToClose = ToolType.MainMenuTool;

            // Set State of StartcloseCycle to false
            MainMenuWindowI.StartCloseCycle = false;
        }

        // 1/11/2011
        /// <summary>
        /// Updates proper settings when some WPF form closes.
        /// </summary>
        static void MainMenuWindowI_FormClosed(object sender, EventArgs e)
        {
            // 3/30/2011 - Signal close complete
            _manualResetEventForClosing.Set();
        }
       
        
        #region CoreMenu EventHandlers

        // 8/18/2010
        /// <summary>
        /// Activates the PropertiesTool form.
        /// </summary>
        static void MainMenuWindowI_StartPropertiesTool(object sender, EventArgs e)
        {
            try 
            {
                // 1/10/2011 - ActivateTool now called in TerrainWPFTools to avoid cross-thread errors from WPF.
                //TerrainEditRoutines.ActivateTool(ToolType.PropertiesTool);
                TerrainWPFTools.ActivatePropertiesTool = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MainMenuWindowI_StartPropertiesTool method threw the exception " + ex.Message ?? "No Message");
            }
        }

        // 8/18/2010
        /// <summary>
        /// Activates the PaintTool form.
        /// </summary>
        static void MainMenuWindowI_StartPaintTool(object sender, EventArgs e)
        {
            try 
            {
                // 1/10/2011 - ActivateTool now called in TerrainWPFTools to avoid cross-thread errors from WPF.
                //TerrainEditRoutines.ActivateTool(ToolType.PaintTool);
                TerrainWPFTools.ActivatePaintTool = true;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_StartPaintTool method threw the exception " + ex.Message ?? "No Message");
#endif
            };
        }

        // 8/18/2010
        /// <summary>
        /// Activates the ItemTool form.
        /// </summary>
        static void MainMenuWindowI_StartItemsTool(object sender, EventArgs e)
        {
            try 
            {
                // 1/10/2011 - ActivateTool now called in TerrainWPFTools to avoid cross-thread errors from WPF.
                //TerrainEditRoutines.ActivateTool(ToolType.ItemTool);
                TerrainWPFTools.ActivateItemTool = true;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_StartItemsTool method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/18/2010
        /// <summary>
        /// Activates the HeightTool form.
        /// </summary>
        static void MainMenuWindowI_StartHeightTool(object sender, EventArgs e)
        {
            try 
            {
                // 1/10/2011 - ActivateTool now called in TerrainWPFTools to avoid cross-thread errors from WPF.
                //TerrainEditRoutines.ActivateTool(ToolType.HeightTool);
                TerrainWPFTools.ActivateHeightTool = true;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_StartHeightTool method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        #endregion

        #region Water EventHandlers

        // 8/20/2010
        /// <summary>
        /// When clicked, will check mark the proper subMenu option.
        /// </summary>
        static void MainMenuWindowI_WaterMapSubMenuOpened(object sender, EventArgs e)
        {
            try
            {
                SetWaterMapType(TemporalWars3DEngine.EngineGameConsole.Water.ShowTexture);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_WaterMapSubMenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Helper method, which updates the WaterMap texture to view.
        /// </summary>
        /// <param name="waterMapType"></param>
        private static void SetWaterMapType(ViewPortTexture waterMapType)
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

                switch (waterMapType)
                {
                    case ViewPortTexture.Refraction:
                        MainMenuWindowI.WaterMenuItems.WaterMapRefractionDp = true;
                        MainMenuWindowI.WaterMenuItems.WaterMapReflectionDp = false;
                        MainMenuWindowI.WaterMenuItems.WaterMapBumpDp = false;
                        break;
                    case ViewPortTexture.Reflection:
                        MainMenuWindowI.WaterMenuItems.WaterMapRefractionDp = false;
                        MainMenuWindowI.WaterMenuItems.WaterMapReflectionDp = true;
                        MainMenuWindowI.WaterMenuItems.WaterMapBumpDp = false;
                        break;
                    case ViewPortTexture.Bump:
                        MainMenuWindowI.WaterMenuItems.WaterMapRefractionDp = false;
                        MainMenuWindowI.WaterMenuItems.WaterMapReflectionDp = false;
                        MainMenuWindowI.WaterMenuItems.WaterMapBumpDp = true;
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

        // 8/20/2010
        /// <summary>
        /// Updates the proper WaterType in use in the SubMenu.
        /// </summary>
        static void MainMenuWindowI_WaterTypeSubMenuOpened(object sender, EventArgs e)
        {
            try
            {
#if WithLicense
                // 5/10/2012 - Return if trial.
                if (LicenseInstance.IsTrial)
                {
                    MessageBox.Show("Valid ONLY in FULL PAID Version!", "License Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
#endif

                switch (TemporalWars3DEngine.EngineGameConsole.Water.WaterTypeToUse)
                {
                    case WaterType.None:
                        MainMenuWindowI.WaterMenuItems.WaterTypeNoneDp = true;
                        MainMenuWindowI.WaterMenuItems.WaterTypeLakeDp = false;
                        MainMenuWindowI.WaterMenuItems.WaterTypeOceanDp = false;
                        break;
                    case WaterType.Lake:
                        MainMenuWindowI.WaterMenuItems.WaterTypeNoneDp = false;
                        MainMenuWindowI.WaterMenuItems.WaterTypeLakeDp = true;
                        MainMenuWindowI.WaterMenuItems.WaterTypeOceanDp = false;
                        break;
                    case WaterType.Ocean:
                        MainMenuWindowI.WaterMenuItems.WaterTypeNoneDp = false;
                        MainMenuWindowI.WaterMenuItems.WaterTypeLakeDp = false;
                        MainMenuWindowI.WaterMenuItems.WaterTypeOceanDp = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_WaterTypeSubMenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/20/2010
        /// <summary>
        /// When clicked, will set the proper Water state for the menu options.
        /// </summary>
        static void MainMenuWindowI_WaterSubMenuOpened(object sender, EventArgs e)
        {
            try
            {
                MainMenuWindowI.WaterMenuItems.IsVisibleDp = TemporalWars3DEngine.EngineGameConsole.Water.IsVisible;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_WaterSubMenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        /// Opens up the WaterAtts form.
        /// </summary>
        static void MainMenuWindowI_EditWaterAtts(object sender, EventArgs e)
        {
            try 
            {
#if WithLicense
                // 5/10/2012 - Return if trial.
                if (LicenseInstance.IsTrial)
                {
                    MessageBox.Show("Valid ONLY in FULL PAID Version!", "License Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
#endif

                if (_waterAttsForm == null) _waterAttsForm = new WaterAtts();
                _waterAttsForm.Visible = true;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_EditWaterAtts method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        /// Sets the WaterMap type to view in the GameViewPort.
        /// </summary>
        static void MainMenuWindowI_WaterMapTypeUpdated(object sender, EventArgs e)
        {
            try 
            {
                TemporalWars3DEngine.EngineGameConsole.Water.ShowTexture = (ViewPortTexture) MainMenuWindowI.WaterMapType;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_WaterMapTypeUpdated method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        /// Turns on the GameViewPort display, to show one of the 3 waterMap textures
        /// for debug purposes.
        /// </summary>
        static void MainMenuWindowI_ShowWaterMap(object sender, IsToggledEventArgs e)
        {
            try
            {
                TemporalWars3DEngine.EngineGameConsole.GVP.Visible = e.IsToggled;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_ShowWaterMap method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        /// Updates the <see cref="WaterType"/> Enum.
        /// </summary>
        static void MainMenuWindowI_WaterTypeUpdated(object sender, EventArgs e)
        {
            try 
            {
                // update WaterManager.
                TemporalWars3DEngine.EngineGameConsole.Water.WaterTypeToUse = (WaterType) MainMenuWindowI.WaterType;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_WaterTypeUpdated method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        /// Allows setting the 'IsVisible' flag in the WaterManager.
        /// </summary>
        static void MainMenuWindowI_IsWaterVisible(object sender, IsToggledEventArgs e)
        {
            try
            {
                TemporalWars3DEngine.EngineGameConsole.Water.IsVisible = e.IsToggled;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_IsWaterVisible method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        #endregion

        #region SkyBox EventHandlers

        // 8/20/2010
        /// <summary>
        ///  When Skyboxes SubMenu opens, this will update the 'UseSkyBox' flag.
        /// </summary>
        static void MainMenuWindowI_SkyBoxSubMenuOpened(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                MainMenuWindowI.SkyBoxMenuItems.UseSkyBoxDp = TemporalWars3DEngine.EngineGameConsole.ScreenManager.UseSkyBox;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_SkyBoxSubMenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        /// Sets the UseSkyBox flag.
        /// </summary>
        static void MainMenuWindowI_UseSkyBox(object sender, IsToggledEventArgs e)
        {
            try 
            {
                TemporalWars3DEngine.EngineGameConsole.ScreenManager.UseSkyBox = e.IsToggled;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_UseSkyBox method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        #endregion

        #region Shadows EventHandlers

        // 8/20/2010
        /// <summary>
        /// This is clicked right before user sees the options to choose; therefore,
        /// the current ShadowQuality setting is updated.
        /// </summary>
        static void MainMenuWindowI_ShadowMapQualitySubMenuOpened(object sender, EventArgs e)
        {
            try
            {
                // Set the current ShadowQuality setting.
                switch (ShadowMap.ShadowQuality)
                {
                    case ShadowQuality.Low:
                        MainMenuWindowI.ShadowsMenuItems.LowShadowQualityTypeDp = true;
                        MainMenuWindowI.ShadowsMenuItems.MediumShadowQualityTypeDp = false;
                        MainMenuWindowI.ShadowsMenuItems.HighShadowQualityTypeDp = false;
                        break;
                    case ShadowQuality.Medium:
                        MainMenuWindowI.ShadowsMenuItems.LowShadowQualityTypeDp = false;
                        MainMenuWindowI.ShadowsMenuItems.MediumShadowQualityTypeDp = true;
                        MainMenuWindowI.ShadowsMenuItems.HighShadowQualityTypeDp = false;
                        break;
                    case ShadowQuality.High:
                        MainMenuWindowI.ShadowsMenuItems.LowShadowQualityTypeDp = false;
                        MainMenuWindowI.ShadowsMenuItems.MediumShadowQualityTypeDp = false;
                        MainMenuWindowI.ShadowsMenuItems.HighShadowQualityTypeDp = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_ShadowMapQualitySubMenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/20/2010
        /// <summary>
        /// When clicked, will set the proper ShadowType state for the menu option.
        /// </summary>
        static void MainMenuWindowI_ShadowMapTypeSubMenuOpened(object sender, EventArgs e)
        {
            try
            {
                SetShadowType(ShadowMap.UseShadowType);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_ShadowMapTypeSubMenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        ///  Helper method, which sets the proper ShadowType.
        /// </summary>
        private static void SetShadowType(ShadowMap.ShadowType shadowType)
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

                switch (shadowType)
                {
                    case ShadowMap.ShadowType.Simple:
                        MainMenuWindowI.ShadowsMenuItems.SimpleShadowTypeDp = true;
                        MainMenuWindowI.ShadowsMenuItems.PCF1TypeDp = false;
                        MainMenuWindowI.ShadowsMenuItems.PCF2TypeDp = false;
                        MainMenuWindowI.ShadowsMenuItems.VarianceShadowTypeDp = false;
                        break;
                    case ShadowMap.ShadowType.PercentageCloseFilter_1:
                        MainMenuWindowI.ShadowsMenuItems.SimpleShadowTypeDp = false;
                        MainMenuWindowI.ShadowsMenuItems.PCF1TypeDp = true;
                        MainMenuWindowI.ShadowsMenuItems.PCF2TypeDp = false;
                        MainMenuWindowI.ShadowsMenuItems.VarianceShadowTypeDp = false;
                        break;
                    case ShadowMap.ShadowType.PercentageCloseFilter_2:
                        MainMenuWindowI.ShadowsMenuItems.SimpleShadowTypeDp = false;
                        MainMenuWindowI.ShadowsMenuItems.PCF1TypeDp = false;
                        MainMenuWindowI.ShadowsMenuItems.PCF2TypeDp = true;
                        MainMenuWindowI.ShadowsMenuItems.VarianceShadowTypeDp = false;
                        break;
                    case ShadowMap.ShadowType.Variance:
                        MainMenuWindowI.ShadowsMenuItems.SimpleShadowTypeDp = false;
                        MainMenuWindowI.ShadowsMenuItems.PCF1TypeDp = false;
                        MainMenuWindowI.ShadowsMenuItems.PCF2TypeDp = false;
                        MainMenuWindowI.ShadowsMenuItems.VarianceShadowTypeDp = true;
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

        // 8/20/2010
        /// <summary>
        /// When SubMenu opens up, this will update the 'UseShadows' flag to current setting.
        /// </summary>
        static void MainMenuWindowI_ShadowMapSubMenuOpened(object sender, EventArgs e)
        {
            try
            {
                MainMenuWindowI.ShadowsMenuItems.UseShadowsDp = TemporalWars3DEngine.EngineGameConsole.ShadowMap.IsVisible;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_ShadowMapSubMenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        /// Eventhandler for when the user moves the scroll bar.
        /// </summary>
        static void MainMenuWindowI_ShadowMapDarknessUpdated(object sender, ShadowMapDarknessEventArgs e)
        {
            try 
            {
#if WithLicense
                // 5/10/2012 - Return if trial.
                if (LicenseInstance.IsTrial)
                {
                    MessageBox.Show("Valid ONLY in FULL PAID Version!", "License Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
#endif
                // Update into ShadowMap component.
                ShadowMap.ShadowMapDarkness = MathHelper.Clamp((float)e.ShadowMapDarknessValue / 100.0f, 0, 1.0f);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_ShadowMapDarknessUpdated method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        /// Allows user to set the ShadowDarkness level.
        /// </summary>
        static void MainMenuWindowI_ShadowDarknessPanel(object sender, EventArgs e)
        {
            try 
            {
                // Set current Darkness value into the scroll bar
                MainMenuWindowI.ShadowDarkness = (int)(ShadowMap.ShadowMapDarkness * 100);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_ShadowDarknessPanel method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        /// Rebuilds the STATIC ShadowMap.
        /// </summary>
        static void MainMenuWindowI_RebuildStaticShadowMap(object sender, EventArgs e)
        {
            try 
            {
                ShadowMap.DoPreShadowMapTextures = true;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_RebuildStaticShadowMap method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        /// Sets shadow quality.
        /// </summary>
        static void MainMenuWindowI_ShadowQualityUpdated(object sender, EventArgs e)
        {
            try 
            {
                // Set to Low setting.
                ShadowMap.ShadowQuality = (ShadowQuality) MainMenuWindowI.ShadowQuality;
                ShadowMap.InitializeRenderTargets();
                
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_ShadowQualityUpdated method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        /// Sets the ShadowType.
        /// </summary>
        static void MainMenuWindowI_ShadowTypeUpdated(object sender, EventArgs e)
        {
            try 
            {
                ShadowMap.UseShadowType = (ShadowMap.ShadowType)MainMenuWindowI.ShadowType;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_ShadowTypeUpdated method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        /// Turn Shadows Debugging on.
        /// </summary>
        static void MainMenuWindowI_ShowShadowMap(object sender, IsToggledEventArgs e)
        {
            try 
            {
                // Update the DebugValues flag.
                TemporalWars3DEngine.EngineGameConsole.ShadowMap.DebugValues = e.IsToggled;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_ShowShadowMap method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        /// Use Shadows.
        /// </summary>
        static void MainMenuWindowI_UseShadows(object sender, IsToggledEventArgs e)
        {
            try 
            {
                // Update the ShadowMap Visible flag.
                TemporalWars3DEngine.EngineGameConsole.ShadowMap.IsVisible = e.IsToggled;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_UseShadows method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        #endregion

        #region NormalMap EventHandlers

        // 8/20/2010
        /// <summary>
        /// When clicked, will set the proper NormalMap 'IsVisible' state for the menu option.
        /// </summary>
        static void MainMenuWindowI_NormalMapSubMenuOpened(object sender, EventArgs e)
        {
            try
            {
                MainMenuWindowI.NormalMapMenuItems.UseNormalMapDp = TerrainShape.EnableNormalMap;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_NormalMapSubMenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        /// Set the NormalMap Visible.
        /// </summary>
        static void MainMenuWindowI_UseNormalMap(object sender, IsToggledEventArgs e)
        {
            try 
            {
                TerrainShape.EnableNormalMap = e.IsToggled;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_UseNormalMap method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        #endregion

        #region MiniMap EventHandlers

        // 8/20/2010
        /// <summary>
        /// When clicked, will set the proper MiniMap 'IsVisible' state for the menu option,
        /// and the 'Show Wrapper' menu option.
        /// </summary>
        static void MainMenuWindowI_MiniMapSubMenuOpened(object sender, EventArgs e)
        {
            // 1/2/2010
            if (TemporalWars3DEngine.EngineGameConsole.MiniMap == null) return;

            try // 6/22/2010
            {
                MainMenuWindowI.MiniMapMenuItems.UseMiniMapDp = TemporalWars3DEngine.EngineGameConsole.MiniMap.IsVisible;
                MainMenuWindowI.MiniMapMenuItems.ShowWrapperDp = TemporalWars3DEngine.EngineGameConsole.MiniMap.ShowTextureWrapper;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("miniMapToolStripMenuItem_DropDownOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        /// Creates a MiniMap Textures, using the given terrain map, and
        /// saves to the hard-drive.
        /// </summary>
        static void MainMenuWindowI_CreateMiniMapTexture(object sender, EventArgs e)
        {
            try 
            {
#if WithLicense
                // 5/10/2012 - Return if trial.
                if (LicenseInstance.IsTrial)
                {
                    MessageBox.Show("Valid ONLY in FULL PAID Version!", "License Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
#endif
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
                Debug.WriteLine("MainMenuWindowI_CreateMiniMapTexture method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        /// Allows turning On/Off the Minimap's texture wrapper.
        /// </summary>
        static void MainMenuWindowI_ShowMiniMapWrapper(object sender, IsToggledEventArgs e)
        {
            try 
            {
#if WithLicense
                // 5/10/2012 - Return if trial.
                if (LicenseInstance.IsTrial)
                {
                    MessageBox.Show("Valid ONLY in FULL PAID Version!", "License Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
#endif
                if (TemporalWars3DEngine.EngineGameConsole.MiniMap != null)
                    TemporalWars3DEngine.EngineGameConsole.MiniMap.ShowTextureWrapper = e.IsToggled;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_ShowMiniMapWrapper method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        /// Set the Minimap Visible.
        /// </summary>
        static void MainMenuWindowI_UseMiniMap(object sender, IsToggledEventArgs e)
        {
            try 
            {
#if WithLicense
                // 5/10/2012 - Return if trial.
                if (LicenseInstance.IsTrial)
                {
                    MessageBox.Show("Valid ONLY in FULL PAID Version!", "License Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
#endif

                if (TemporalWars3DEngine.EngineGameConsole.MiniMap != null)
                    TemporalWars3DEngine.EngineGameConsole.MiniMap.IsVisible = e.IsToggled;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_UseMiniMap method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        #endregion

        #region LightingType EventHandlers

        // 8/20/2010
        /// <summary>
        /// When clicked, will set the proper LightingType state for the menu option.
        /// </summary>
        static void MainMenuWindowI_LightingTypeSubMenuOpened(object sender, EventArgs e)
        {
            try
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

        /// <summary>
        /// Helper method, used to set the proper LightingType.
        /// </summary>
        private static void SetLightingType(TerrainLightingType lightingType)
        {
            try
            {
                switch (lightingType)
                {
                    case TerrainLightingType.Plastic:
                        MainMenuWindowI.LightingTypeMenuItems.UsePlasticLightingDp = true;
                        MainMenuWindowI.LightingTypeMenuItems.UseMetalLightingDp = false;
                        MainMenuWindowI.LightingTypeMenuItems.UseBlinnLightingDp = false;
                        MainMenuWindowI.LightingTypeMenuItems.UseGlossyLightingDp = false;
                        break;
                    case TerrainLightingType.Metal:
                        MainMenuWindowI.LightingTypeMenuItems.UsePlasticLightingDp = false;
                        MainMenuWindowI.LightingTypeMenuItems.UseMetalLightingDp = true;
                        MainMenuWindowI.LightingTypeMenuItems.UseBlinnLightingDp = false;
                        MainMenuWindowI.LightingTypeMenuItems.UseGlossyLightingDp = false;
                        break;
                    case TerrainLightingType.Blinn:
                        MainMenuWindowI.LightingTypeMenuItems.UsePlasticLightingDp = false;
                        MainMenuWindowI.LightingTypeMenuItems.UseMetalLightingDp = false;
                        MainMenuWindowI.LightingTypeMenuItems.UseBlinnLightingDp = true;
                        MainMenuWindowI.LightingTypeMenuItems.UseGlossyLightingDp = false;
                        break;
                    case TerrainLightingType.Glossy:
                        MainMenuWindowI.LightingTypeMenuItems.UsePlasticLightingDp = false;
                        MainMenuWindowI.LightingTypeMenuItems.UseMetalLightingDp = false;
                        MainMenuWindowI.LightingTypeMenuItems.UseBlinnLightingDp = false;
                        MainMenuWindowI.LightingTypeMenuItems.UseGlossyLightingDp = true;
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

        // 8/17/2010
        static void MainMenuWindowI_LightingTypeUpdated(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                TerrainShape.LightingType = (TerrainLightingType) MainMenuWindowI.LightingType;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_LightingTypeUpdated method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        #endregion

        #region IFD EventHandlers

        // 8/20/2010
        /// <summary>
        /// When clicked, will set the proper IFD 'IsVisible' state for the menu option.
        /// </summary>
        // ReSharper disable InconsistentNaming
        static void MainMenuWindowI_IFDSubMenuOpened(object sender, EventArgs e)
        // ReSharper restore InconsistentNaming
        {
            try
            {
                MainMenuWindowI.IFDMenuItems.UseIFDDp = TemporalWars3DEngine.EngineGameConsole.IFD.IsVisible;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_IFDSubMenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        static void MainMenuWindowI_UseIFD(object sender, IsToggledEventArgs e)
        {
            try
            {
#if WithLicense
                // 5/10/2012 - Return if trial.
                if (LicenseInstance.IsTrial)
                {
                    MessageBox.Show("Valid ONLY in FULL PAID Version!", "License Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
#endif

                TemporalWars3DEngine.EngineGameConsole.IFD.IsVisible = e.IsToggled;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_UseIFD method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        #endregion

        #region Glow EventHandlers

        // 8/20/2010
        /// <summary>
        ///  When Glow's SubMenu opens, this will update the 'UseGlow' flag.
        /// </summary>
        static void MainMenuWindowI_GlowSubMenuOpened(object sender, EventArgs e)
        {
            try
            {
                MainMenuWindowI.GlowMenuItems.UseGlowDp = TemporalWars3DEngine.EngineGameConsole.ScreenManager.UseGlow;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_GlowSubMenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        static void MainMenuWindowI_UseGlow(object sender, IsToggledEventArgs e)
        {
            try 
            {
#if WithLicense
                // 5/10/2012 - Return if trial.
                if (LicenseInstance.IsTrial)
                {
                    MessageBox.Show("Valid ONLY in FULL PAID Version!", "License Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
#endif
                TemporalWars3DEngine.EngineGameConsole.ScreenManager.UseGlow = e.IsToggled;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_UseGlow method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        #endregion

        #region FOW EventHandlers

        // 8/20/2010
        /// <summary>
        /// When clicked, will set the proper FOW 'IsVisible' state for the menu option.
        /// </summary>
        static void MainMenuWindowI_FogOfWarSubMenuOpened(object sender, EventArgs e)
        {
            try
            {
                // 3/23/2010 - Check if FOW is null.
                if (TemporalWars3DEngine.EngineGameConsole.FOW != null)
                    MainMenuWindowI.FogOfWarMenuItems.UseFOWDp = TemporalWars3DEngine.EngineGameConsole.FOW.IsVisible;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_FogOfWarSubMenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        static void MainMenuWindowI_UseFogOfWar(object sender, IsToggledEventArgs e)
        {
            try 
            {
                // 3/23/2010 - Check if FOW is null.
                if (TemporalWars3DEngine.EngineGameConsole.FOW != null)
                    TemporalWars3DEngine.EngineGameConsole.FOW.IsVisible = e.IsToggled;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_UseFogOfWar method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        #endregion

        #region Bloom EventHandlers

        // 8/20/2010
        /// <summary>
        /// When Bloom's SubMenu opens, this will update the 'UseBloom' flag.
        /// </summary>
        static void MainMenuWindowI_BloomSubMenuOpened(object sender, EventArgs e)
        {
            try
            {
                // 8/20/2010
                // Update Menu Items values.
                MainMenuWindowI.BloomMenuItems.UseBloomDp = TemporalWars3DEngine.EngineGameConsole.ScreenManager.UseBloom;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_BloomSubMenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/20/2010
        /// <summary>
        /// When BloomSetting's SubMenu opens, this will set the proper 'current' bloom setting flag.
        /// </summary>
        static void MainMenuWindowI_BloomSettingsSubMenuOpened(object sender, EventArgs e)
        {
            try
            {
                var bloomSetting = (int)TemporalWars3DEngine.EngineGameConsole.ScreenManager.BloomSetting;

                MainMenuWindowI.BloomMenuItems.DefaultBloomDp = (0 == bloomSetting);
                MainMenuWindowI.BloomMenuItems.SoftBloomDp = (1 == bloomSetting);
                MainMenuWindowI.BloomMenuItems.DeSatBloomDp = (2 == bloomSetting);
                MainMenuWindowI.BloomMenuItems.SatBloomDp = (3 == bloomSetting);
                MainMenuWindowI.BloomMenuItems.BlurryBloomDp = (4 == bloomSetting);
                MainMenuWindowI.BloomMenuItems.SubtleBloomDp = (5 == bloomSetting);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_BloomSettingsSubMenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        static void MainMenuWindowI_BloomTypeUpdated(object sender, EventArgs e)
        {
            // Updates to proper bloom type choosen.
            try
            {
#if WithLicense
                // 5/10/2012 - Return if trial.
                if (LicenseInstance.IsTrial)
                {
                    MessageBox.Show("Valid ONLY in FULL PAID Version!", "License Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
#endif
                TemporalWars3DEngine.EngineGameConsole.ScreenManager.BloomSetting = (BloomType) MainMenuWindowI.BloomTypeToUse;
                
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_BloomTypeUpdated method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        /// When clicked, sets the UseBloom.
        /// </summary>
// ReSharper disable InconsistentNaming
        static void MainMenuWindowI_UseBloom(object sender, IsToggledEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                TemporalWars3DEngine.EngineGameConsole.ScreenManager.UseBloom = e.IsToggled;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_UseBloom method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        #endregion

        #region PerlinClouds EventHandlers

        // 1/10/2011
        /// <summary>
        /// When clicked, sets the UsePerlinClouds.
        /// </summary>
        static void MainMenuWindowI_UsePerlinClouds(object sender, IsToggledEventArgs e)
        {
            try
            {
#if WithLicense
                // 5/10/2012 - Return if trial.
                if (LicenseInstance.IsTrial)
                {
                    MessageBox.Show("Valid ONLY in FULL PAID Version!", "License Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
#endif
                TerrainPerlinClouds.EnableClouds = e.IsToggled;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_UsePerlinClouds method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 1/10/2011
        /// <summary>
        /// When PerlinClouds's SubMenu opens, this will update the 'UsePerlinCloud' flag.
        /// </summary>
        static void MainMenuWindowI_PerlinCloudsSubMenuOpened(object sender, EventArgs e)
        {
            try
            {
                MainMenuWindowI.PerlinCloudsMenuItems.UsePerlinCloudsDp = TerrainPerlinClouds.EnableClouds;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_PerlinCloudsSubMenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        #endregion

        #region DebugShader EventHandlers

        // 5/28/2012
        /// <summary>
        /// When DebugShader's SubMenu opens, this will update the 'UseDebugShader' flag.
        /// </summary>
        static void MainMenuWindowI_UseDebugShader(object sender, IsToggledEventArgs e )
        {
            try
            {
#if WithLicense
                // 5/10/2012 - Return if trial.
                if (LicenseInstance.IsTrial)
                {
                    MessageBox.Show("Valid ONLY in FULL PAID Version!", "License Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
#endif
                DebugShapeRenderer.IsVisible = e.IsToggled;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_UseDebugShader method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 5/28/2012
        /// <summary>
        /// When clicked, sets the UseDebugShader.
        /// </summary>
        static void MainMenuWindowI_DebugShaderSubMenuOpened(object sender, EventArgs e)
        {
            try
            {
                MainMenuWindowI.DebugShaderMenuItems.UseDebugShaderDp = DebugShapeRenderer.IsVisible;
                MainMenuWindowI.DebugShaderMenuItems.UseDrawCollisionsPlayableItemsDp = DebugShapeRenderer.DrawCollisionSpheresForPlayableItems;
                MainMenuWindowI.DebugShaderMenuItems.UseDrawCollisionsScenaryItemsDp = DebugShapeRenderer.DrawCollisionSpheresForScenaryItems;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_DebugShaderSubMenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 5/28/2012
        /// <summary>
        /// When clicked, sets the UseDrawCollisionsPlayableItems.
        /// </summary>
        static void MainMenuWindowI_UseDrawCollisionsPlayableItems(object sender, IsToggledEventArgs e)
        {
            try
            {
#if WithLicense
                // 5/10/2012 - Return if trial.
                if (LicenseInstance.IsTrial)
                {
                    MessageBox.Show("Valid ONLY in FULL PAID Version!", "License Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
#endif
                DebugShapeRenderer.DrawCollisionSpheresForPlayableItems = e.IsToggled;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_UseDrawCollisionsPlayableItems method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 5/28/2012
        /// <summary>
        /// When clicked, sets the UseDrawCollisionsScenaryItems.
        /// </summary>
        static void MainMenuWindowI_UseDrawCollisionsScenaryItems(object sender, IsToggledEventArgs e)
        {
            try
            {
#if WithLicense
                // 5/10/2012 - Return if trial.
                if (LicenseInstance.IsTrial)
                {
                    MessageBox.Show("Valid ONLY in FULL PAID Version!", "License Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
#endif
                DebugShapeRenderer.DrawCollisionSpheresForScenaryItems = e.IsToggled;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("MainMenuWindowI_UseDrawCollisionsScenaryItems method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        #endregion

        /// <summary>
        /// Updates the current WPF tool.
        /// </summary>
        public void Update(GameTime gameTime)
        {
           // Empty;
        }

        // 3/30/2011
        public void CloseForm()
        {
            MainMenuWindowI.Close();

            // Wait for WPF Closed event to trigger before allowing exit of method call.
            _manualResetEventForClosing.WaitOne();
        }
        
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            // dispose of WPF form
            if (MainMenuWindowI != null)
            {
                MainMenuWindowI.Close();
                MainMenuWindowI = null;
            }
        }
    }
}
