﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;

namespace TWTerrainToolsWPF.DataModel.MainMenuDataModel
{
    // 8/20/2010
    public class SkyBoxMenuItems
    {
        // Window DependencyObject
        private readonly MainMenuWindow _mainMenuWindow;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mainMenuWindow">Instance of <see cref="MainMenuWindow"/></param>
        public SkyBoxMenuItems(MainMenuWindow mainMenuWindow)
        {
            _mainMenuWindow = mainMenuWindow;
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of UseSkyBox menu item.
        /// </summary>
        public bool UseSkyBoxDp
        {
            get { return _mainMenuWindow.useSkyBoxMI.IsChecked; }
            set { _mainMenuWindow.useSkyBoxMI.IsChecked = value; }
        }
       
    }
    
}