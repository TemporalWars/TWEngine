using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;

namespace TWTerrainToolsWPF.DataModel.MainMenuDataModel
{
    // 8/20/2010
    public class NormalMapMenuItems
    {
        // Window DependencyObject
        private readonly MainMenuWindow _mainMenuWindow;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mainMenuWindow">Instance of <see cref="MainMenuWindow"/></param>
        public NormalMapMenuItems(MainMenuWindow mainMenuWindow)
        {
            _mainMenuWindow = mainMenuWindow;
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of UseNormalMap menu item.
        /// </summary>
        public bool UseNormalMapDp
        {
            get { return _mainMenuWindow.useNormalMapMI.IsChecked; }
            set { _mainMenuWindow.useNormalMapMI.IsChecked = value; }
        }

       
    }
    
}
