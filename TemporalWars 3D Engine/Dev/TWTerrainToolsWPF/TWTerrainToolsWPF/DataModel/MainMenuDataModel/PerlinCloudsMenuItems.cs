using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;

namespace TWTerrainToolsWPF.DataModel.MainMenuDataModel
{
    // 1/10/2011
    public class PerlinCloudsMenuItems
    {
        // Window DependencyObject
        private readonly MainMenuWindow _mainMenuWindow;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mainMenuWindow">Instance of <see cref="MainMenuWindow"/></param>
        public PerlinCloudsMenuItems(MainMenuWindow mainMenuWindow)
        {
            _mainMenuWindow = mainMenuWindow;
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of UsePerlinClouds menu item.
        /// </summary>
        public bool UsePerlinCloudsDp
        {
            get { return _mainMenuWindow.usePerlinCloudsMI.IsChecked; }
            set { _mainMenuWindow.usePerlinCloudsMI.IsChecked = value; }
        }

       
    }
    
}
