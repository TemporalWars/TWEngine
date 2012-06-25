using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;

namespace TWTerrainToolsWPF.DataModel.MainMenuDataModel
{
    // 5/28/2012
    public class DebugShaderMenuItems
    {
        // Window DependencyObject
        private readonly MainMenuWindow _mainMenuWindow;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mainMenuWindow">Instance of <see cref="MainMenuWindow"/></param>
        public DebugShaderMenuItems(MainMenuWindow mainMenuWindow)
        {
            _mainMenuWindow = mainMenuWindow;
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of UseDebugShader menu item.
        /// </summary>
        public bool UseDebugShaderDp
        {
            get { return _mainMenuWindow.useDebugShaderMI.IsChecked; }
            set { _mainMenuWindow.useDebugShaderMI.IsChecked = value; }
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of UseDrawCollisionsScenaryItems menu item.
        /// </summary>
        public bool UseDrawCollisionsScenaryItemsDp
        {
            get { return _mainMenuWindow.DrawCollisionForScenaryItemsMI.IsChecked; }
            set { _mainMenuWindow.DrawCollisionForScenaryItemsMI.IsChecked = value; }
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of UseDrawCollisionsPlayableItems menu item.
        /// </summary>
        public bool UseDrawCollisionsPlayableItemsDp
        {
            get { return _mainMenuWindow.DrawCollisionForPlayableItemsMI.IsChecked; }
            set { _mainMenuWindow.DrawCollisionForPlayableItemsMI.IsChecked = value; }
        }
    }
    
}
