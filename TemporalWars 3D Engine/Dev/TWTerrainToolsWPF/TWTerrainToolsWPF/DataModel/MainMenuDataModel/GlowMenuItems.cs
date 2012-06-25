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
    public class GlowMenuItems
    {
        // Window DependencyObject
        private readonly MainMenuWindow _mainMenuWindow;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mainMenuWindow">Instance of <see cref="MainMenuWindow"/></param>
        public GlowMenuItems(MainMenuWindow mainMenuWindow)
        {
            _mainMenuWindow = mainMenuWindow;
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of UseGlow menu item.
        /// </summary>
        public bool UseGlowDp
        {
            get { return _mainMenuWindow.useGlowMI.IsChecked; }
            set { _mainMenuWindow.useGlowMI.IsChecked = value; }
        }

       
    }
    
}
