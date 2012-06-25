using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;

namespace TWTerrainToolsWPF.DataModel.MainMenuDataModel
{
    // 8/19/2010
    public class FogOfWarMenuItems
    {
        // Window DependencyObject
        private readonly MainMenuWindow _mainMenuWindow;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mainMenuWindow">Instance of <see cref="MainMenuWindow"/></param>
        public FogOfWarMenuItems(MainMenuWindow mainMenuWindow)
        {
            _mainMenuWindow = mainMenuWindow;
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of UseFOW menu item.
        /// </summary>
        public bool UseFOWDp
        {
            get { return _mainMenuWindow.useFogOfWarMI.IsChecked; }
            set { _mainMenuWindow.useFogOfWarMI.IsChecked = value; }
        }

       
    }
    
}
