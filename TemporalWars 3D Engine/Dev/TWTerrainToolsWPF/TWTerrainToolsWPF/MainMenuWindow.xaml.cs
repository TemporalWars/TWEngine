using System;
using System.Windows;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.DataModel.MainMenuDataModel;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.Delegates;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.Enums;

namespace ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF
{
    /// <summary>
    /// Interaction logic for MainMenuWindow.xaml
    /// </summary>
    public partial class MainMenuWindow : Window
    {
        // 8/19/2010 - Instance of BloomMenuItem class.
        private readonly BloomMenuItems _bloomMenuItems;
        // 8/20/2010 - Instance of FOWMenuItem class.
        private readonly FogOfWarMenuItems _fogOfWarMenuItems;
        // 8/20/2010 - Instance of GlowMenuItem class.
        private readonly GlowMenuItems _glowMenuItems;
        // 8/20/2010 - Instance of IFDMenuItem class.
        private readonly IFDMenuItems _ifdMenuItems;
        // 8/20/2010 - Instance of LightingTypeMenuItems class.
        private readonly LightingTypeMenuItems _lightingTypeMenuItems;
        // 8/20/2010 - Instance of MiniMapMenuItems class.
        private readonly MiniMapMenuItems _miniMapMenuItems;
        // 8/20/2010 - Instance of NormalMapMenuItems class.
        private readonly NormalMapMenuItems _normalMapMenuItems;
        // 8/20/2010 - Instance of ShadowsMenuItems class.
        private readonly ShadowsMenuItems _shadowsMenuItems;
        // 8/20/2010 - Instance of SkyBoxMenuItems class.
        private readonly SkyBoxMenuItems _skyBoxMenuItems;
        // 8/20/2010 - Instance of SkyBoxMenuItems class.
        private readonly WaterMenuItems _waterMenuItems;
        // 1/10/2011 - Instance of PerlinCloudsMenuItems class.
        private readonly PerlinCloudsMenuItems _perlinCloudsMenuItems;
        // 5/28/2012 - Instance of DebugShaderMenuItems class
        private readonly DebugShaderMenuItems _debugShaderMenuItems;

        #region Properties


        // 3/30/2011
        /// <summary>
        /// Get or set to start the window close cycle.
        /// </summary>
        public bool StartCloseCycle { get; set; }

        /// <summary>
        /// Gets or sets the current <see cref="BloomType"/> to use.
        /// </summary>
        public BloomType BloomTypeToUse { get; set; }

        // 8/17/2010
        /// <summary>
        /// Gets or Sets the current <see cref="TerrainLightingType"/> to use.
        /// </summary>
        public TerrainLightingType LightingType { get; set; }

        // 8/17/2010
        /// <summary>
        /// Gets or Sets the current <see cref="ShadowType"/> to use.
        /// </summary>
        public ShadowType ShadowType { get; set; }

        // 8/17/2010
        /// <summary>
        /// Gets or Sets the current <see cref="ShadowQuality"/> to use.
        /// </summary>
        public ShadowQuality ShadowQuality { get; set; }

        // 8/17/2010
        /// <summary>
        /// Gets or Sets the current ShadowDarkness level.
        /// </summary>
        public int ShadowDarkness
        {
            set { tbShadowDarkness.Value = value; }
            get { return (int)tbShadowDarkness.Value; }
        }

        // 8/17/2010
        /// <summary>
        /// Gets or Sets the current <see cref="WaterType"/> to use.
        /// </summary>
        public WaterType WaterType { get; set; }

        // 8/17/2010
        /// <summary>
        /// Gets or Sets the current <see cref="WaterMapType"/> to use.
        /// </summary>
        public ViewPortTexture WaterMapType { get; set; }

        // 8/19/2010
        /// <summary>
        /// Gets the current <see cref="BloomMenuItems"/> used to update
        /// the MenuItem's IsChecked properties.
        /// </summary>
        public BloomMenuItems BloomMenuItems
        {
            get { return _bloomMenuItems; }
        }

        // 8/20/2010
        /// <summary>
        /// Gets the current <see cref="FogOfWarMenuItems"/> used to update
        /// the MenuItem's IsChecked properties.
        /// </summary>
        public FogOfWarMenuItems FogOfWarMenuItems
        {
            get { return _fogOfWarMenuItems; }
        }

        // 8/20/2010
        /// <summary>
        /// Gets the current <see cref="GlowMenuItems"/> used to update
        /// the MenuItem's IsChecked properties.
        /// </summary>
        public GlowMenuItems GlowMenuItems
        {
            get { return _glowMenuItems; }
        }

        // 8/20/2010
        /// <summary>
        /// Gets the current <see cref="IFDMenuItems"/> used to update
        /// the MenuItem's IsChecked properties.
        /// </summary>
        public IFDMenuItems IFDMenuItems
        {
            get { return _ifdMenuItems; }
        }

        // 8/20/2010
        /// <summary>
        /// Gets the current <see cref="LightingTypeMenuItems"/> used to update
        /// the MenuItem's IsChecked properties.
        /// </summary>
        public LightingTypeMenuItems LightingTypeMenuItems
        {
            get { return _lightingTypeMenuItems; }
        }

        // 8/20/2010
        /// <summary>
        /// Gets the current <see cref="MiniMapMenuItems"/> used to update
        /// the MenuItem's IsChecked properties.
        /// </summary>
        public MiniMapMenuItems MiniMapMenuItems
        {
            get { return _miniMapMenuItems; }
        }

        // 8/20/2010
        /// <summary>
        /// Gets the current <see cref="NormalMapMenuItems"/> used to update
        /// the MenuItem's IsChecked properties.
        /// </summary>
        public NormalMapMenuItems NormalMapMenuItems
        {
            get { return _normalMapMenuItems; }
        }

        // 8/20/2010
        /// <summary>
        /// Gets the current <see cref="ShadowsMenuItems"/> used to update
        /// the MenuItem's IsChecked properties.
        /// </summary>
        public ShadowsMenuItems ShadowsMenuItems
        {
            get { return _shadowsMenuItems; }
        }

        // 8/20/2010
        /// <summary>
        /// Gets the current <see cref="SkyBoxMenuItems"/> used to update
        /// the MenuItem's IsChecked properties.
        /// </summary>
        public SkyBoxMenuItems SkyBoxMenuItems
        {
            get { return _skyBoxMenuItems; }
        }

        // 8/20/2010
        /// <summary>
        /// Gets the current <see cref="WaterMenuItems"/> used to update
        /// the MenuItem's IsChecked properties.
        /// </summary>
        public WaterMenuItems WaterMenuItems
        {
            get { return _waterMenuItems; }
        }

        // 1/10/2011
        /// <summary>
        /// Gets the current <see cref="PerlinCloudsMenuItems"/> used to update
        /// the MenuItem's IsChecked properties.
        /// </summary>
        public PerlinCloudsMenuItems PerlinCloudsMenuItems
        {
            get { return _perlinCloudsMenuItems; }
        }

        // 5/28/2012
        /// <summary>
        /// Gets the current <see cref="DebugShaderMenuItems"/> used to update
        /// the MenuItem's IsChecked properties.
        /// </summary>
        public DebugShaderMenuItems DebugShaderMenuItems
        {
            get { return _debugShaderMenuItems; }
        }

        #endregion

        #region Events

        // 3/30/2011
        /// <summary>
        /// Occurs when WPF form has just started the close cycle.
        /// </summary>
        public event EventHandler FormStartClose;

        /// <summary>
        /// Occurs when the 'UseBloom' menu item is updated.
        /// </summary>
        public event IsToggledDelegate UseBloom;

        /// <summary>
        /// Occurs when the 'UseMiniMap' menu item is updated.
        /// </summary>
        public event IsToggledDelegate UseMiniMap;

        /// <summary>
        /// Occurs when the <see cref="BloomTypeToUse"/> Enum is updated.
        /// </summary>
        public event EventHandler BloomTypeUpdated;

        /// <summary>
        /// Occurs when the 'UseFogOfWar' menu item is updated.
        /// </summary>
        public event IsToggledDelegate UseFogOfWar;

        /// <summary>
        /// Occurs when the 'UseGlow' menu item is updated.
        /// </summary>
        public event IsToggledDelegate UseGlow;

        /// <summary>
        /// Occurs when the 'UseIFD' menu item is updated.
        /// </summary>
        public event IsToggledDelegate UseIFD;

        /// <summary>
        /// Occurs when some 'LightingType' menu item is clicked.
        /// </summary>
        public event EventHandler LightingTypeUpdated;

        /// <summary>
        /// Occurs when the 'ShowWrapper' menu item is clicked.
        /// </summary>
        public event IsToggledDelegate ShowMiniMapWrapper;

        /// <summary>
        /// Occurs when the 'CreateMMTexture' menu item is clicked.
        /// </summary>
        public event EventHandler CreateMiniMapTexture;

        /// <summary>
        /// Occurs when the 'UseNormalMap' menu item is clicked.
        /// </summary>
        public event IsToggledDelegate UseNormalMap;

        /// <summary>
        /// Occurs when the 'UseShadows' menu item is clicked.
        /// </summary>
        public event IsToggledDelegate UseShadows;

        /// <summary>
        /// Occurs when the 'ShowShadowMap' menu item is clicked.
        /// </summary>
        public event IsToggledDelegate ShowShadowMap;

        // 8/17/2010
        /// <summary>
        /// Occurs when some 'ShadowType' menu item is clicked.
        /// </summary>
        public event EventHandler ShadowTypeUpdated;

        // 8/17/2010
        /// <summary>
        /// Occurs when some 'ShadowQuality' menu item is clicked.
        /// </summary>
        public event EventHandler ShadowQualityUpdated;

        /// <summary>
        /// Occurs when the 'RebuildStaticSM' menu item is clicked.
        /// </summary>
        public event EventHandler RebuildStaticShadowMap;

        /// <summary>
        /// Occurs when the 'ShadowShadowDarkness' menu item is clicked.
        /// </summary>
        public event EventHandler ShadowDarknessPanel;

        // 8/17/2010
        /// <summary>
        /// Occurs when the 'UseSkyBox' menu item is clicked.
        /// </summary>
        public event IsToggledDelegate UseSkyBox;

        // 8/17/2010
        /// <summary>
        /// Occurs when the 'HeightTool' menu item is clicked.
        /// </summary>
        public event EventHandler StartHeightTool;

        // 8/17/2010
        /// <summary>
        /// Occurs when the 'PaintTool' menu item is clicked.
        /// </summary>
        public event EventHandler StartPaintTool;

        // 8/17/2010
        /// <summary>
        /// Occurs when the 'ItemTool' menu item is clicked.
        /// </summary>
        public event EventHandler StartItemsTool;

        // 8/17/2010
        /// <summary>
        /// Occurs when the 'PropertiesTool' menu item is clicked.
        /// </summary>
        public event EventHandler StartPropertiesTool;

        // 8/17/2010
        /// <summary>
        /// Occurs when the ShadowMapDarkness slider value is updated.
        /// </summary>
        public event ShadowMapDarknessDelegate ShadowMapDarknessUpdated;

        // 8/17/2010
        /// <summary>
        /// Occurs when the water's 'Visible' menu item is clicked.
        /// </summary>
        public event IsToggledDelegate IsWaterVisible;

        // 8/17/2010
        /// <summary>
        /// Occurs when some 'WaterType' menu item is clicked.
        /// </summary>
        public event EventHandler WaterTypeUpdated;

        // 8/17/2010
        /// <summary>
        /// Occurs when the 'Show WaterMap' menu item is clicked.
        /// </summary>
        public event IsToggledDelegate ShowWaterMap;

        // 8/17/2010
        /// <summary>
        /// Occurs when some 'WaterMap Type' menu item is clicked.
        /// </summary>
        public event EventHandler WaterMapTypeUpdated;

        // 8/17/2010
        /// <summary>
        /// Occurs when the 'Edit Water Attributes' menu item is clicked.
        /// </summary>
        public event EventHandler EditWaterAtts;

        // 8/20/2010
        /// <summary>
        /// Occurs when the 'BloomSettings' SubMenu is opened.
        /// </summary>
        public event EventHandler BloomSettingsSubMenuOpened;

        // 8/20/2010
        /// <summary>
        /// Occurs when the 'Bloom' SubMenu is opened.
        /// </summary>
        public event EventHandler BloomSubMenuOpened;

        // 8/20/2010
        /// <summary>
        /// Occurs when the 'FOW' SubMenu is opened.
        /// </summary>
        public event EventHandler FogOfWarSubMenuOpened;

        // 8/20/2010
        /// <summary>
        /// Occurs when the 'Glow' SubMenu is opened.
        /// </summary>
        public event EventHandler GlowSubMenuOpened;

        // 8/20/2010
        /// <summary>
        /// Occurs when the 'IFD' SubMenu is opened.
        /// </summary>
        public event EventHandler IFDSubMenuOpened;

        // 8/20/2010
        /// <summary>
        /// Occurs when the 'LightingType' SubMenu is opened.
        /// </summary>
        public event EventHandler LightingTypeSubMenuOpened;

        // 8/20/2010
        /// <summary>
        /// Occurs when the 'MiniMap' SubMenu is opened.
        /// </summary>
        public event EventHandler MiniMapSubMenuOpened;

        // 8/20/2010
        /// <summary>
        /// Occurs when the 'NormalMap' SubMenu is opened.
        /// </summary>
        public event EventHandler NormalMapSubMenuOpened;

        // 8/20/2010
        /// <summary>
        /// Occurs when the 'ShadowMap' SubMenu is opened.
        /// </summary>
        public event EventHandler ShadowMapSubMenuOpened;

        // 8/20/2010
        /// <summary>
        /// Occurs when the 'ShadowMapType' SubMenu is opened.
        /// </summary>
        public event EventHandler ShadowMapTypeSubMenuOpened;

        // 8/20/2010
        /// <summary>
        /// Occurs when the 'ShadowMapQuality' SubMenu is opened.
        /// </summary>
        public event EventHandler ShadowMapQualitySubMenuOpened;

        // 8/20/2010
        /// <summary>
        /// Occurs when the 'SkyBox' SubMenu is opened.
        /// </summary>
        public event EventHandler SkyBoxSubMenuOpened;

        // 8/20/2010
        /// <summary>
        /// Occurs when the 'Water' SubMenu is opened.
        /// </summary>
        public event EventHandler WaterSubMenuOpened;

        // 8/20/2010
        /// <summary>
        /// Occurs when the 'WaterType' SubMenu is opened.
        /// </summary>
        public event EventHandler WaterTypeSubMenuOpened;

        // 8/20/2010
        /// <summary>
        /// Occurs when the 'WaterMap' SubMenu is opened.
        /// </summary>
        public event EventHandler WaterMapSubMenuOpened;

        // 1/10/2011
        /// <summary>
        /// Occurs when the 'PerlinCloud' SubMenu is opened.
        /// </summary>
        public event EventHandler PerlinCloudsSubMenuOpened;

        // 1/10/2011
        /// <summary>
        /// Occurs when the 'UsePerlinClouds' menu item is clicked.
        /// </summary>
        public event IsToggledDelegate UsePerlinClouds;

        // 5/28/2012
        /// <summary>
        /// Occurs when the 'UseDebugShader' SubMenu is opened.
        /// </summary>
        public event EventHandler DebugShaderSubMenuOpened;

        // 5/28/2012
        /// <summary>
        /// Occurs when the 'UseDebugShader' menu item is clicked.
        /// </summary>
        public event IsToggledDelegate UseDebugShader;

        // 5/28/2012
        /// <summary>
        /// Occurs when the 'UseDrawCollisionsScenaryItems' menu item is clicked.
        /// </summary>
        public event IsToggledDelegate UseDrawCollisionsScenaryItems;

        // 5/28/2012
        /// <summary>
        /// Occurs when the 'UseDrawCollisionsPlayableItems' menu item is clicked.
        /// </summary>
        public event IsToggledDelegate UseDrawCollisionsPlayableItems;

        // 1/11/2011
        /// <summary>
        /// Occurs when WPF form has just closed.
        /// </summary>
        public event EventHandler FormClosed;


        #endregion

        public MainMenuWindow()
        {
            InitializeComponent();

            // 8/19/2010 - Create instances of the MenuItem dataBind classes.
            _bloomMenuItems = new BloomMenuItems(this);
            _fogOfWarMenuItems = new FogOfWarMenuItems(this);
            _glowMenuItems = new GlowMenuItems(this);
            _ifdMenuItems = new IFDMenuItems(this);
            _lightingTypeMenuItems = new LightingTypeMenuItems(this);
            _miniMapMenuItems = new MiniMapMenuItems(this);
            _normalMapMenuItems = new NormalMapMenuItems(this);
            _shadowsMenuItems = new ShadowsMenuItems(this);
            _skyBoxMenuItems = new SkyBoxMenuItems(this);
            _waterMenuItems = new WaterMenuItems(this);
            _perlinCloudsMenuItems = new PerlinCloudsMenuItems(this);
            _debugShaderMenuItems = new DebugShaderMenuItems(this); // 5/28/2012

        }

        // 7/10/2010
        /// <summary>
        /// When clicked, sets the UseBloom.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void useBloomMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // trigger event
                if (UseBloom != null)
                {
                    UseBloom(this, new IsToggledEventArgs{ IsToggled = useBloomMI.IsChecked });
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("useBloomMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Sets the Bloom to use the 'Default' setting.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void defaultBloomMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // update value
                BloomTypeToUse = BloomType.Default;

                // trigger event
                if (BloomTypeUpdated != null)
                    BloomTypeUpdated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("defaultBloomMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Sets the Bloom to use the 'Default' setting.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void softBloomMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // update value
                BloomTypeToUse = BloomType.Soft;

                // trigger event
                if (BloomTypeUpdated != null)
                    BloomTypeUpdated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("softBloomMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Sets the Bloom to use the 'Bloom' setting.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void saturatedBloomMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // update value
                BloomTypeToUse = BloomType.Saturated;

                // trigger event
                if (BloomTypeUpdated != null)
                    BloomTypeUpdated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("saturatedBloomMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Sets the Bloom to use the 'Blurry' setting.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void blurryBloomMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // update value
                BloomTypeToUse = BloomType.Blurry;

                // trigger event
                if (BloomTypeUpdated != null)
                    BloomTypeUpdated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("blurryBloomMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Sets the Bloom to use the 'Subtle' setting.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void subtleBloomMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // update value
                BloomTypeToUse = BloomType.Subtle;

                // trigger event
                if (BloomTypeUpdated != null)
                    BloomTypeUpdated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("subtleBloom_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Sets the Bloom to use the 'DeSaturated' setting.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void deSaturatedBloomMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // update value
                BloomTypeToUse = BloomType.DeSaturated;

                // trigger event
                if (BloomTypeUpdated != null)
                    BloomTypeUpdated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("deSaturatedBloomMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Set the Fog-Of-War Visible.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void useFogOfWarMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // trigger event
                if (UseFogOfWar != null)
                {
                    UseFogOfWar(this, new IsToggledEventArgs { IsToggled = useFogOfWarMI.IsChecked });
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("useFogOfWarMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Sets the UseGlow flag.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void useGlowMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // trigger event
                if (UseGlow != null)
                {
                    UseGlow(this, new IsToggledEventArgs { IsToggled = useGlowMI.IsChecked });
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("useGlowMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Set the IFD Visible.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void useIfdMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (UseIFD != null)
                {
                    UseIFD(this, new IsToggledEventArgs { IsToggled = useIfdMI.IsChecked });
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("useIfdMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Set the Plastic LightingType.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void plasticLightingTypeMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // 8/17/2010
                LightingType = TerrainLightingType.Plastic;
                SetLightingType(TerrainLightingType.Plastic);

                // trigger event
                if (LightingTypeUpdated != null)
                    LightingTypeUpdated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("plasticLightingTypeMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Set the Metal LightingType.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void metalLightingTypeMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // 8/17/2010
                LightingType = TerrainLightingType.Metal;
                SetLightingType(TerrainLightingType.Metal);

                // trigger event
                if (LightingTypeUpdated != null)
                    LightingTypeUpdated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("metalLightingTypeMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Set the Blinn LightingType.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void blinnLightingTypeMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // 8/17/2010
                LightingType = TerrainLightingType.Blinn; 
                SetLightingType(TerrainLightingType.Blinn);

                // trigger event
                if (LightingTypeUpdated != null)
                    LightingTypeUpdated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("blinnLightingTypeMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Set the Glossy LightingType.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void glossyLightingTypeMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // 8/17/2010
                LightingType = TerrainLightingType.Glossy;
                SetLightingType(TerrainLightingType.Glossy);

                // trigger event
                if (LightingTypeUpdated != null)
                    LightingTypeUpdated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("glossyLightingTypeMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
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
                        plasticLightingTypeMI.IsChecked = true;
                        metalLightingTypeMI.IsChecked = false;
                        blinnLightingTypeMI.IsChecked = false;
                        glossyLightingTypeMI.IsChecked = false;
                        break;
                    case TerrainLightingType.Metal:
                        plasticLightingTypeMI.IsChecked = false;
                        metalLightingTypeMI.IsChecked = true;
                        blinnLightingTypeMI.IsChecked = false;
                        glossyLightingTypeMI.IsChecked = false;
                        break;
                    case TerrainLightingType.Blinn:
                        plasticLightingTypeMI.IsChecked = false;
                        metalLightingTypeMI.IsChecked = false;
                        blinnLightingTypeMI.IsChecked = true;
                        glossyLightingTypeMI.IsChecked = false;
                        break;
                    case TerrainLightingType.Glossy:
                        plasticLightingTypeMI.IsChecked = false;
                        metalLightingTypeMI.IsChecked = false;
                        blinnLightingTypeMI.IsChecked = false;
                        glossyLightingTypeMI.IsChecked = true;
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

        /// <summary>
        /// When clicked, sets the UseMiniMap.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void useMiniMapMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // trigger event
                if (UseMiniMap != null)
                {
                    UseMiniMap(this, new IsToggledEventArgs { IsToggled = useMiniMapMI.IsChecked });
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("useMiniMapMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/26/2010
        /// <summary>
        /// When clicked, shows the wrapper around MiniMap.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void showWrapperMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 7/26/2010
            {
                // trigger event
                if (ShowMiniMapWrapper != null)
                {
                    ShowMiniMapWrapper(this, new IsToggledEventArgs { IsToggled = showWrapperMI.IsChecked });
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("showWrapperMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/26/2010
        /// <summary>
        /// When clicked, creates the MiniMap texture representation of the terrain.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void createMMTextureMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 7/26/2010
            {
                // trigger event
                if (CreateMiniMapTexture != null)
                {
                    CreateMiniMapTexture(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("createMMTextureMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/26/2010
        /// <summary>
        /// When clicked, turns on/off the use of the normalMap on the terrain.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void useNormalMapMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 7/26/2010
            {
                // trigger event
                if (UseNormalMap != null)
                {
                    UseNormalMap(this, new IsToggledEventArgs { IsToggled = useNormalMapMI.IsChecked });
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("useNormalMapMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/26/2010
        /// <summary>
        /// When clicked, turns on/off the use of the Shadows on the terrain.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void useShadowsMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 7/26/2010
            {
                // trigger event
                if (UseShadows != null)
                {
                    UseShadows(this, new IsToggledEventArgs { IsToggled = useShadowsMI.IsChecked });
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("useShadowsMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/26/2010
        /// <summary>
        /// When clicked, turns on/off showing the shadowMap.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void showShadowMapMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 7/26/2010
            {
                // trigger event
                if (ShowShadowMap != null)
                {
                    ShowShadowMap(this, new IsToggledEventArgs { IsToggled = showShadowMapMI.IsChecked });
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("showShadowMapMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/26/2010
        /// <summary>
        /// When clicked, turns on/off the 'SimpleShadowType'.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void simpleShadowTypeMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 7/26/2010
            {
                // 8/17/2010
                ShadowType = ShadowType.Simple;
                SetShadowType(ShadowType);

                // trigger event
                if (ShadowTypeUpdated != null)
                {
                    ShadowTypeUpdated(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("simpleShadowTypeMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/26/2010
        /// <summary>
        /// When clicked, turns on/off the 'PCFShadowType-1'.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void pcf1ShadowTypeMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 7/26/2010
            {
                // 8/17/2010
                ShadowType = ShadowType.PercentageCloseFilter_1;
                SetShadowType(ShadowType);

                // trigger event
                if (ShadowTypeUpdated != null)
                {
                    ShadowTypeUpdated(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("pcf1ShadowTypeMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/26/2010
        /// <summary>
        /// When clicked, turns on/off the 'PCFShadowType-2'.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void pcf2ShadowTypeMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 7/26/2010
            {
                // 8/17/2010
                ShadowType = ShadowType.PercentageCloseFilter_2;
                SetShadowType(ShadowType);

                // trigger event
                if (ShadowTypeUpdated != null)
                {
                    ShadowTypeUpdated(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("pcf2ShadowTypeMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/26/2010
        /// <summary>
        /// When clicked, turns on/off the 'VarianceShadowType'
        /// </summary>
// ReSharper disable InconsistentNaming
        private void varianceShadowTypeMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 7/26/2010
            {
                // 8/17/2010
                ShadowType = ShadowType.Variance;
                SetShadowType(ShadowType);

                // trigger event
                if (ShadowTypeUpdated != null)
                {
                    ShadowTypeUpdated(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("varianceShadowTypeMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        // 12/12/2009; 6/12/2010 - Updated with PCF#2.
        /// <summary>
        ///  Helper method, which sets the proper ShadowType.
        /// </summary>
        private void SetShadowType(ShadowType shadowType)
        {
            try 
            {
                switch (shadowType)
                {
                    case ShadowType.Simple:
                        simpleShadowTypeMI.IsChecked = true;
                        pcf1ShadowTypeMI.IsChecked = false;
                        pcf2ShadowTypeMI.IsChecked = false;
                        varianceShadowTypeMI.IsChecked = false;
                        break;
                    case ShadowType.PercentageCloseFilter_1:
                        simpleShadowTypeMI.IsChecked = false;
                        pcf1ShadowTypeMI.IsChecked = true;
                        pcf2ShadowTypeMI.IsChecked = false;
                        varianceShadowTypeMI.IsChecked = false;
                        break;
                    case ShadowType.PercentageCloseFilter_2:
                        simpleShadowTypeMI.IsChecked = false;
                        pcf1ShadowTypeMI.IsChecked = false;
                        pcf2ShadowTypeMI.IsChecked = true;
                        varianceShadowTypeMI.IsChecked = false;
                        break;
                    case ShadowType.Variance:
                        simpleShadowTypeMI.IsChecked = false;
                        pcf1ShadowTypeMI.IsChecked = false;
                        pcf2ShadowTypeMI.IsChecked = false;
                        varianceShadowTypeMI.IsChecked = true;
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

        // 7/26/2010
        /// <summary>
        /// When clicked, turns on/off the Low1024x.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void low1024xMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 7/26/2010
            {
                // 8/17/2010
                ShadowQuality = ShadowQuality.Low;
                SetShadowQuality(ShadowQuality);

                // trigger event
                if (ShadowQualityUpdated != null)
                {
                    ShadowQualityUpdated(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("low1024xMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/26/2010
        /// <summary>
        /// When clicked, turns on/off the Medium2048x.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void medium2048xMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 7/26/2010
            {
                // 8/17/2010
                ShadowQuality = ShadowQuality.Medium;
                SetShadowQuality(ShadowQuality);

                // trigger event
                if (ShadowQualityUpdated != null)
                {
                    ShadowQualityUpdated(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("medium2048xMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/26/2010
        /// <summary>
        /// When clicked, turns on/off the High4096x.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void high4096xMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 7/26/2010
            {
                // 8/17/2010
                ShadowQuality = ShadowQuality.High;
                SetShadowQuality(ShadowQuality);

                // trigger event
                if (ShadowQualityUpdated != null)
                {
                    ShadowQualityUpdated(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("high4096xMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        ///  Helper method, which sets the proper ShadowQuality.
        /// </summary>
        private void SetShadowQuality(ShadowQuality shadowQuality)
        {
            try
            {
                switch (shadowQuality)
                {
                    case ShadowQuality.Low:
                        low1024xMI.IsChecked = true;
                        medium2048xMI.IsChecked = false;
                        high4096xMI.IsChecked = false;
                        break;
                    case ShadowQuality.Medium:
                        low1024xMI.IsChecked = false;
                        medium2048xMI.IsChecked = true;
                        high4096xMI.IsChecked = false;
                        break;
                    case ShadowQuality.High:
                        low1024xMI.IsChecked = false;
                        medium2048xMI.IsChecked = false;
                        high4096xMI.IsChecked = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("SetShadowQuality method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/26/2010
        /// <summary>
        /// When clicked, builds the static shadow map.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void rebuildStaticMapMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 7/26/2010
            {
                // trigger event
                if (RebuildStaticShadowMap != null)
                {
                    RebuildStaticShadowMap(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("rebuildStaticMapMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/26/2010
        /// <summary>
        /// When clicked, shows the shadow darkness panel.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void shadowDarknessMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 7/26/2010
            {
                // 8/17/2010
                // Displays the ShadowMap Darkness Group
                gbShadowDarkness.Visibility = Visibility.Visible;

                // trigger event
                if (ShadowDarknessPanel != null)
                {
                    ShadowDarknessPanel(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("shadowDarknessMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        /// When clicked, shows the SkyBox.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void useSkyBoxMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try 
            {
                // trigger event
                if (UseSkyBox != null)
                {
                    UseSkyBox(this, new IsToggledEventArgs { IsToggled = useSkyBoxMI.IsChecked });
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("useSkyBoxMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        /// When clicked, starts the Height-Tool form.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnHeightTool_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (StartHeightTool != null)
                {
                    StartHeightTool(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnHeightTool_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        /// When clicked, starts the Paint-Tool form.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnPaintTool_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (StartPaintTool != null)
                {
                    StartPaintTool(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnPaintTool_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        /// When clicked, starts the Items-Tool form.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnItemsTool_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (StartItemsTool != null)
                {
                    StartItemsTool(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnItemsTool_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        /// When clicked, Properties-Tool form.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnPropertiesTool_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (StartPropertiesTool != null)
                {
                    StartPropertiesTool(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnPropertiesTool_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        /// When clicked, sets the water type to 'None'.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void waterTypeNoneMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // 8/17/2010
                WaterType = WaterType.None;
                SetWaterType(WaterType);

                // trigger event
                if (WaterTypeUpdated != null)
                {
                    WaterTypeUpdated(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("waterTypeNoneMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        ///  When clicked, sets the water type to 'Lake'.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void waterTypeLakeMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // 8/17/2010
                WaterType = WaterType.Lake;
                SetWaterType(WaterType);

                // trigger event
                if (WaterTypeUpdated != null)
                {
                    WaterTypeUpdated(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("waterTypeLakeMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        ///  When clicked, sets the water type to 'Ocean'.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void waterTypeOceanMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // 8/17/2010
                WaterType = WaterType.Ocean;
                SetWaterType(WaterType);

                // trigger event
                if (WaterTypeUpdated != null)
                {
                    WaterTypeUpdated(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("waterTypeOceanMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        ///  Helper method, which sets the proper WaterType.
        /// </summary>
        private void SetWaterType(WaterType waterType)
        {
            try
            {
                switch (waterType)
                {
                    case WaterType.None:
                        waterTypeNoneMI.IsChecked = true;
                        waterTypeLakeMI.IsChecked = false;
                        waterTypeOceanMI.IsChecked = false;
                        break;
                    case WaterType.Lake:
                        waterTypeNoneMI.IsChecked = false;
                        waterTypeLakeMI.IsChecked = true;
                        waterTypeOceanMI.IsChecked = false;
                        break;
                    case WaterType.Ocean:
                        waterTypeNoneMI.IsChecked = false;
                        waterTypeLakeMI.IsChecked = false;
                        waterTypeOceanMI.IsChecked = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("SetWaterType method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        ///  Occurs when the value changes on the slider.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void tbShadowDarkness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (ShadowMapDarknessUpdated != null)
                {
                    ShadowMapDarknessUpdated(this, new ShadowMapDarknessEventArgs() { ShadowMapDarknessValue = tbShadowDarkness.Value });
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("tbShadowDarkness_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        ///  Occurs when the 'Done' button is pressed.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnDone_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010            
            {
                gbShadowDarkness.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnDone_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        ///  Occurs when the 'Visible' button is pressed for Water.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void isVisibleMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (IsWaterVisible != null)
                {
                    IsWaterVisible(this, new IsToggledEventArgs { IsToggled = waterMI.IsChecked });
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("isVisibleMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        ///  Occurs when the 'Show WaterMap' button is pressed.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void showWaterMapMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (ShowWaterMap != null)
                {
                    ShowWaterMap(this, new IsToggledEventArgs { IsToggled = showWaterMapMI.IsChecked });
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("showWaterMapMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        ///  Occurs when Water's 'ReflectionMap' button is pressed.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void reflectionMapMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // 8/17/2010
                WaterMapType = ViewPortTexture.Reflection;
                SetWaterMapType(WaterMapType);

                // trigger event
                if (WaterMapTypeUpdated != null)
                {
                    WaterMapTypeUpdated(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("reflectionMapMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        ///   Occurs when Water's 'RefractionMap' button is pressed.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void refractionMapMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // 8/17/2010
                WaterMapType = ViewPortTexture.Refraction;
                SetWaterMapType(WaterMapType);

                // trigger event
                if (WaterMapTypeUpdated != null)
                {
                    WaterMapTypeUpdated(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("refractionMapMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
        /// <summary>
        ///  Occurs when Water's 'BupmMap' button is pressed.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void bumpMapMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // 8/17/2010
                WaterMapType = ViewPortTexture.Bump;
                SetWaterMapType(WaterMapType);

                // trigger event
                if (WaterMapTypeUpdated != null)
                {
                    WaterMapTypeUpdated(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("bumpMapMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/17/2010
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
                        reflectionMapMI.IsChecked = true;
                        refractionMapMI.IsChecked = false;
                        bumpMapMI.IsChecked = false;
                        break;
                    case ViewPortTexture.Reflection:
                        reflectionMapMI.IsChecked = false;
                        refractionMapMI.IsChecked = true;
                        bumpMapMI.IsChecked = false;
                        break;
                    case ViewPortTexture.Bump:
                        reflectionMapMI.IsChecked = false;
                        refractionMapMI.IsChecked = false;
                        bumpMapMI.IsChecked = true;
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

        // 8/17/2010
        /// <summary>
        ///  Occurs when Water's 'Attributes' button is pressed.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void editWaterAttributesMI_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (EditWaterAtts != null)
                {
                    EditWaterAtts(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("editWaterAttributesMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }
        
        // 8/20/2010
        /// <summary>
        /// Occurs when the subMenu is opened.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void bloomSettingMI_SubmenuOpened(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (BloomSettingsSubMenuOpened != null)
                {
                    BloomSettingsSubMenuOpened(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("bloomSettingMI_SubmenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/20/2010
        /// <summary>
        /// Occurs when the subMenu is opened.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void BloomMI_SubmenuOpened(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (BloomSubMenuOpened != null)
                {
                    BloomSubMenuOpened(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("BloomMI_SubmenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/20/2010
        /// <summary>
        /// Occurs when the subMenu is opened.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void fogOfWarMI_SubmenuOpened(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (FogOfWarSubMenuOpened != null)
                {
                    FogOfWarSubMenuOpened(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("fogOfWarMI_SubmenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/20/2010
        /// <summary>
        /// Occurs when the subMenu is opened.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void glowMI_SubmenuOpened(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (GlowSubMenuOpened != null)
                {
                    GlowSubMenuOpened(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("glowMI_SubmenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/20/2010
        /// <summary>
        /// Occurs when the subMenu is opened.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void ifdMI_SubmenuOpened(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (IFDSubMenuOpened != null)
                {
                    IFDSubMenuOpened(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("ifdMI_SubmenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/20/2010
        /// <summary>
        /// Occurs when the subMenu is opened.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void lightingTypeMI_SubmenuOpened(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (LightingTypeSubMenuOpened != null)
                {
                    LightingTypeSubMenuOpened(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("lightingTypeMI_SubmenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/20/2010
        /// <summary>
        /// Occurs when the subMenu is opened.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void miniMapMI_SubmenuOpened(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (MiniMapSubMenuOpened != null)
                {
                    MiniMapSubMenuOpened(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("miniMapMI_SubmenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/20/2010
        /// <summary>
        /// Occurs when the subMenu is opened.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void normalMapMI_SubmenuOpened(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (NormalMapSubMenuOpened != null)
                {
                    NormalMapSubMenuOpened(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("normalMapMI_SubmenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/20/2010
        /// <summary>
        /// Occurs when the subMenu is opened.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void shadowsMI_SubmenuOpened(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (ShadowMapSubMenuOpened != null)
                {
                    ShadowMapSubMenuOpened(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("shadowsMI_SubmenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/20/2010
        /// <summary>
        /// Occurs when the subMenu is opened.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void shadowTypeMI_SubmenuOpened(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (ShadowMapTypeSubMenuOpened != null)
                {
                    ShadowMapTypeSubMenuOpened(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("shadowTypeMI_SubmenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/20/2010
        /// <summary>
        /// Occurs when the subMenu is opened.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void shadowQualityMI_SubmenuOpened(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (ShadowMapQualitySubMenuOpened != null)
                {
                    ShadowMapQualitySubMenuOpened(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("shadowQualityMI_SubmenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/20/2010
        /// <summary>
        /// Occurs when the subMenu is opened.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void skyBoxMI_SubmenuOpened(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (SkyBoxSubMenuOpened != null)
                {
                    SkyBoxSubMenuOpened(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("skyBoxMI_SubmenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/20/2010
        /// <summary>
        /// Occurs when the subMenu is opened.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void waterMI_SubmenuOpened(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (WaterSubMenuOpened != null)
                {
                    WaterSubMenuOpened(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("waterMI_SubmenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/20/2010
        /// <summary>
        /// Occurs when the subMenu is opened.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void useWaterMI_SubmenuOpened(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (WaterTypeSubMenuOpened != null)
                {
                    WaterTypeSubMenuOpened(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("useWaterMI_SubmenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/20/2010
        /// <summary>
        /// Occurs when the subMenu is opened.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void waterMapTypeMI_SubmenuOpened(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (WaterMapSubMenuOpened != null)
                {
                    WaterMapSubMenuOpened(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("waterMapTypeMI_SubmenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 1/10/2011
        /// <summary>
        /// Occurs when the subMenu is opened.
        /// </summary>
        private void PerlinCloudsMI_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            try
            {
                // trigger event
                if (PerlinCloudsSubMenuOpened != null)
                {
                    PerlinCloudsSubMenuOpened(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PerlinCloudsMI_SubmenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 1/10/2011
        /// <summary>
        /// When clicked, sets the UsePerlinClouds.
        /// </summary>
        private void usePerlinCloudsMI_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                // trigger event
                if (UsePerlinClouds != null)
                {
                    UsePerlinClouds(this, new IsToggledEventArgs { IsToggled = usePerlinCloudsMI.IsChecked });
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("usePerlinCloudsMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 5/28/2012
        /// <summary>
        /// Occurs when the subMenu is opened.
        /// </summary>
        private void DebugShaderMI_SubmenuOpened(object sender, RoutedEventArgs e)
        {
             try
            {
                // trigger event
                if (DebugShaderSubMenuOpened != null)
                {
                    DebugShaderSubMenuOpened(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("DebugShaderMI_SubmenuOpened method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 5/28/2012
        /// <summary>
        /// When clicked, sets the Debug-Shader.
        /// </summary>
        private void useDebugShaderMI_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // trigger event
                if (UseDebugShader != null)
                {
                    UseDebugShader(this, new IsToggledEventArgs { IsToggled = useDebugShaderMI.IsChecked });
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("useDebugShaderMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 5/28/2012
        /// <summary>
        /// When clicked, sets to draw the 'Collision-Spheres' for the scenary items.
        /// </summary>
        private void DrawCollisionForScenaryItemsMI_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // trigger event
                if (UseDrawCollisionsScenaryItems != null)
                {
                    UseDrawCollisionsScenaryItems(this, new IsToggledEventArgs { IsToggled = DrawCollisionForScenaryItemsMI.IsChecked });
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("DrawCollisionForScenaryItemsMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 5/28/2012
        /// <summary>
        /// When clicked, sets to draw the 'Collision-Spheres' for the scenary items.
        /// </summary>
        private void DrawCollisionForPlayableItemsMI_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // trigger event
                if (UseDrawCollisionsPlayableItems != null)
                {
                    UseDrawCollisionsPlayableItems(this, new IsToggledEventArgs { IsToggled = DrawCollisionForPlayableItemsMI.IsChecked });
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("DrawCollisionForPlayableItemsMI_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 1/11/2011
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                 // 3/30/2011 - Check if start of close cycle
                if (StartCloseCycle)
                {
                    // 1/11/2011 - Fixed to close properly.
                    e.Cancel = true;
                    //Visibility = Visibility.Hidden;

                    if (FormStartClose != null)
                        FormStartClose(this, EventArgs.Empty);

                }
               

            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("MainMenuWindow_Closing method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 3/30/2011
        private void Window_Closed(object sender, EventArgs e)
        {
            if (FormClosed != null)
                FormClosed(this, EventArgs.Empty);
        }
    }
}
