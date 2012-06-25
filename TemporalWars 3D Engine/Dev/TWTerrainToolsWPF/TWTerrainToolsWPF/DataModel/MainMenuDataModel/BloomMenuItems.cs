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
    public class BloomMenuItems
    {
        // Window DependencyObject
        private readonly MainMenuWindow _mainMenuWindow;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mainMenuWindow">Instance of <see cref="MainMenuWindow"/></param>
        public BloomMenuItems(MainMenuWindow mainMenuWindow)
        {
            _mainMenuWindow = mainMenuWindow;
        }

        // 8/19/2010
        /*public static DependencyProperty UseBloomDpProperty =
                    DependencyProperty.Register("UseBloomDp", typeof(bool), typeof(MainMenuWindow));

        // 8/19/2010
        public static DependencyProperty DefaultBloomDpProperty =
                    DependencyProperty.Register("DefaultBloomDp", typeof(bool), typeof(MainMenuWindow));

        // 8/19/2010
        public static DependencyProperty SoftBloomDpProperty =
                    DependencyProperty.Register("SoftBloomDp", typeof(bool), typeof(MainMenuWindow));

        // 8/19/2010
        public static DependencyProperty DeSatBloomDpProperty =
                    DependencyProperty.Register("DeSatBloomDp", typeof(bool), typeof(MainMenuWindow));

        // 8/19/2010
        public static DependencyProperty SatBloomDpProperty =
                    DependencyProperty.Register("SatBloomDp", typeof(bool), typeof(MainMenuWindow));

        // 8/19/2010
        public static DependencyProperty BlurryBloomDpProperty =
                    DependencyProperty.Register("BlurryBloomDp", typeof(bool), typeof(MainMenuWindow));

        // 8/19/2010
        public static DependencyProperty SubtleBloomDpProperty =
                    DependencyProperty.Register("SubtleBloomDp", typeof(bool), typeof(MainMenuWindow));*/


        /// <summary>
        /// Gets or Sets the IsCheckable property of UseBloom menu item.
        /// </summary>
        public bool UseBloomDp
        {
            get { return _mainMenuWindow.useBloomMI.IsChecked; }
            set { _mainMenuWindow.useBloomMI.IsChecked = value; }
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of DefaultBloom menu item.
        /// </summary>
        public bool DefaultBloomDp
        {
            get { return _mainMenuWindow.defaultBloomMI.IsChecked; }
            set { _mainMenuWindow.defaultBloomMI.IsChecked = value; }
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of SoftBloomDp menu item.
        /// </summary>
        public bool SoftBloomDp
        {
            get { return _mainMenuWindow.softBloomMI.IsChecked; }
            set { _mainMenuWindow.softBloomMI.IsChecked = value; }
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of DeSatBloomDp menu item.
        /// </summary>
        public bool DeSatBloomDp
        {
            get { return _mainMenuWindow.deSaturatedBloomMI.IsChecked; }
            set { _mainMenuWindow.deSaturatedBloomMI.IsChecked = value; }
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of SatBloomDp menu item.
        /// </summary>
        public bool SatBloomDp
        {
            get { return _mainMenuWindow.saturatedBloomMI.IsChecked; }
            set { _mainMenuWindow.saturatedBloomMI.IsChecked = value; }
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of BlurryBloomDp menu item.
        /// </summary>
        public bool BlurryBloomDp
        {
            get { return _mainMenuWindow.blurryBloomMI.IsChecked; }
            set { _mainMenuWindow.blurryBloomMI.IsChecked = value; }
        }

        /// <summary>
        /// Gets or Sets the IsCheckable property of SubtleBloomDp menu item.
        /// </summary>
        public bool SubtleBloomDp
        {
            get { return _mainMenuWindow.subtleBloomMI.IsChecked; }
            set { _mainMenuWindow.subtleBloomMI.IsChecked = value; }
        }
    }
    
}
