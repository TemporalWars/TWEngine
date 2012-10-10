namespace ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.DataModel.MainMenuDataModel
{
    // 8/20/2010
    public class ShadowsMenuItems
    {
        // Window DependencyObject
        private readonly MainMenuWindow _mainMenuWindow;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mainMenuWindow">Instance of <see cref="MainMenuWindow"/></param>
        public ShadowsMenuItems(MainMenuWindow mainMenuWindow)
        {
            _mainMenuWindow = mainMenuWindow;
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of UseShadow menu item.
        /// </summary>
        public bool UseShadowsDp
        {
            get { return _mainMenuWindow.useShadowsMI.IsChecked; }
            set { _mainMenuWindow.useShadowsMI.IsChecked = value; }
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of ShowShadowMap menu item.
        /// </summary>
        public bool ShowShadowMapDp
        {
            get { return _mainMenuWindow.showShadowMapMI.IsChecked; }
            set { _mainMenuWindow.showShadowMapMI.IsChecked = value; }
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of SimpleShadowType menu item.
        /// </summary>
        public bool SimpleShadowTypeDp
        {
            get { return _mainMenuWindow.simpleShadowTypeMI.IsChecked; }
            set { _mainMenuWindow.simpleShadowTypeMI.IsChecked = value; }
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of PercentageCloseFilter_1 menu item.
        /// </summary>
        public bool PCF1TypeDp
        {
            get { return _mainMenuWindow.pcf1ShadowTypeMI.IsChecked; }
            set { _mainMenuWindow.pcf1ShadowTypeMI.IsChecked = value; }
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of PercentageCloseFilter_2 menu item.
        /// </summary>
        public bool PCF2TypeDp
        {
            get { return _mainMenuWindow.pcf2ShadowTypeMI.IsChecked; }
            set { _mainMenuWindow.pcf2ShadowTypeMI.IsChecked = value; }
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of VarianceShadowType menu item.
        /// </summary>
        public bool VarianceShadowTypeDp
        {
            get { return _mainMenuWindow.varianceShadowTypeMI.IsChecked; }
            set { _mainMenuWindow.varianceShadowTypeMI.IsChecked = value; }
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of Low-1024x menu item.
        /// </summary>
        public bool LowShadowQualityTypeDp
        {
            get { return _mainMenuWindow.low1024xMI.IsChecked; }
            set { _mainMenuWindow.low1024xMI.IsChecked = value; }
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of Med-2048x menu item.
        /// </summary>
        public bool MediumShadowQualityTypeDp
        {
            get { return _mainMenuWindow.medium2048xMI.IsChecked; }
            set { _mainMenuWindow.medium2048xMI.IsChecked = value; }
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of High-4096x menu item.
        /// </summary>
        public bool HighShadowQualityTypeDp
        {
            get { return _mainMenuWindow.high4096xMI.IsChecked; }
            set { _mainMenuWindow.high4096xMI.IsChecked = value; }
        }

       
    }
    
}
