namespace ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.DataModel.MainMenuDataModel
{
    // 8/20/2010
    public class MiniMapMenuItems
    {
        // Window DependencyObject
        private readonly MainMenuWindow _mainMenuWindow;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mainMenuWindow">Instance of <see cref="MainMenuWindow"/></param>
        public MiniMapMenuItems(MainMenuWindow mainMenuWindow)
        {
            _mainMenuWindow = mainMenuWindow;
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of UseMiniMap menu item.
        /// </summary>
        public bool UseMiniMapDp
        {
            get { return _mainMenuWindow.useMiniMapMI.IsChecked; }
            set { _mainMenuWindow.useMiniMapMI.IsChecked = value; }
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of ShowWrapper menu item.
        /// </summary>
        public bool ShowWrapperDp
        {
            get { return _mainMenuWindow.showWrapperMI.IsChecked; }
            set { _mainMenuWindow.showWrapperMI.IsChecked = value; }
        }
       
    }
    
}
