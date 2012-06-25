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
    public class LightingTypeMenuItems
    {
        // Window DependencyObject
        private readonly MainMenuWindow _mainMenuWindow;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mainMenuWindow">Instance of <see cref="MainMenuWindow"/></param>
        public LightingTypeMenuItems(MainMenuWindow mainMenuWindow)
        {
            _mainMenuWindow = mainMenuWindow;
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of Plastic menu item.
        /// </summary>
        public bool UsePlasticLightingDp
        {
            get { return _mainMenuWindow.plasticLightingTypeMI.IsChecked; }
            set { _mainMenuWindow.plasticLightingTypeMI.IsChecked = value; }
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of Metal menu item.
        /// </summary>
        public bool UseMetalLightingDp
        {
            get { return _mainMenuWindow.metalLightingTypeMI.IsChecked; }
            set { _mainMenuWindow.metalLightingTypeMI.IsChecked = value; }
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of Blinn menu item.
        /// </summary>
        public bool UseBlinnLightingDp
        {
            get { return _mainMenuWindow.blinnLightingTypeMI.IsChecked; }
            set { _mainMenuWindow.blinnLightingTypeMI.IsChecked = value; }
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of Glossy menu item.
        /// </summary>
        public bool UseGlossyLightingDp
        {
            get { return _mainMenuWindow.glossyLightingTypeMI.IsChecked; }
            set { _mainMenuWindow.glossyLightingTypeMI.IsChecked = value; }
        }
       
    }
    
}
