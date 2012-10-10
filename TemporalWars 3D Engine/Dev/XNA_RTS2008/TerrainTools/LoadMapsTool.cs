#region File Description
//-----------------------------------------------------------------------------
// LoadMapsTool.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Windows.Forms;
using ImageNexus.BenScharbach.TWEngine.Utilities;
using System.Diagnostics;

namespace TWEngine.TerrainTools
{
    ///<summary>
    /// The <see cref="LoadMapsTool"/> class is used to load the content maps, and return a
    /// collection of content map names.
    ///</summary>
    public partial class LoadMapsTool : Form
    {

        ///<summary>
        /// Constructor
        ///</summary>
        public LoadMapsTool()
        {
            // Set MainMenu Screen Ref
            InitializeComponent();

            // 8/12/2008 - Populate Load ListView with Map Names
            PopulateMapListView();
        }

        // 8/12/2008
        /// <summary>
        /// Populates the ListView map control with map names.
        /// </summary>
        private void PopulateMapListView()
        {
            try // 6/22/2010
            {
                var storageTool = new Storage();
                var mapNames = storageTool.GetSavedMapNames(@"ContentMaps\");

                // Populate Listview with MapNames,if any.
                var length = mapNames.Length; // 5/28/2010
                if (length >= 1)
                {
                    lvLoadMaps.Clear();
                    for (var i = 0; i < length; i++)
                    {
                        lvLoadMaps.Items.Add(mapNames[i]);
                    }
                }

                // Set ListView to use View.List method.
                lvLoadMaps.View = View.List;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PopulateMapListView method threw the exception " + ex.Message ?? "No Message");
#endif
            }

        }

        // 8/12/2008 - Updates the Textbox 'LoadMapName', with selected SceneItemOwner.
// ReSharper disable InconsistentNaming
        private void lvLoadMaps_SelectedIndexChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                txtBoxMapLoadName.Text = lvLoadMaps.FocusedItem.Text;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("lvLoadMaps_SelectedIndexChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }    
    }
}