namespace ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.DataModel.MainMenuDataModel
{
    // 8/20/2010
    public class WaterMenuItems
    {
        // Window DependencyObject
        private readonly MainMenuWindow _mainMenuWindow;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mainMenuWindow">Instance of <see cref="MainMenuWindow"/></param>
        public WaterMenuItems(MainMenuWindow mainMenuWindow)
        {
            _mainMenuWindow = mainMenuWindow;
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of Visible menu item.
        /// </summary>
        public bool IsVisibleDp
        {
            get { return _mainMenuWindow.isVisibleMI.IsChecked; }
            set { _mainMenuWindow.isVisibleMI.IsChecked = value; }
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of WaterType None menu item.
        /// </summary>
        public bool WaterTypeNoneDp
        {
            get { return _mainMenuWindow.waterTypeNoneMI.IsChecked; }
            set { _mainMenuWindow.waterTypeNoneMI.IsChecked = value; }
        }


        /// <summary>
        /// Gets or Sets the IsCheckable property of WaterType Lake menu item.
        /// </summary>
        public bool WaterTypeLakeDp
        {
            get { return _mainMenuWindow.waterTypeLakeMI.IsChecked; }
            set { _mainMenuWindow.waterTypeLakeMI.IsChecked = value; }
        }


        /// <summary>
        /// Gets or Sets the IsCheckable property of WaterType Ocean menu item.
        /// </summary>
        public bool WaterTypeOceanDp
        {
            get { return _mainMenuWindow.waterTypeOceanMI.IsChecked; }
            set { _mainMenuWindow.waterTypeOceanMI.IsChecked = value; }
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of WaterMap Reflection menu item.
        /// </summary>
        public bool WaterMapReflectionDp
        {
            get { return _mainMenuWindow.reflectionMapMI.IsChecked; }
            set { _mainMenuWindow.reflectionMapMI.IsChecked = value; }
        }


        /// <summary>
        /// Gets or Sets the IsCheckable property of WaterMap Refraction menu item.
        /// </summary>
        public bool WaterMapRefractionDp
        {
            get { return _mainMenuWindow.refractionMapMI.IsChecked; }
            set { _mainMenuWindow.refractionMapMI.IsChecked = value; }
        }


        /// <summary>
        /// Gets or Sets the IsCheckable property of WaterMap Bump menu item.
        /// </summary>
        public bool WaterMapBumpDp
        {
            get { return _mainMenuWindow.bumpMapMI.IsChecked; }
            set { _mainMenuWindow.bumpMapMI.IsChecked = value; }
        }
       
    }
    
}
