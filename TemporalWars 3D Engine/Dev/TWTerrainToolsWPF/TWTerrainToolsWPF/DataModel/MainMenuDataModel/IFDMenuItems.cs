namespace ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.DataModel.MainMenuDataModel
{
    // 8/20/2010
    public class IFDMenuItems
    {
        // Window DependencyObject
        private readonly MainMenuWindow _mainMenuWindow;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mainMenuWindow">Instance of <see cref="MainMenuWindow"/></param>
        public IFDMenuItems(MainMenuWindow mainMenuWindow)
        {
            _mainMenuWindow = mainMenuWindow;
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of UseIFD menu item.
        /// </summary>
        public bool UseIFDDp
        {
            get { return _mainMenuWindow.useIfdMI.IsChecked; }
            set { _mainMenuWindow.useIfdMI.IsChecked = value; }
        }

       
    }
    
}
