#region File Description
//-----------------------------------------------------------------------------
// SaveMapsTool.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Windows.Forms;
using ImageNexus.BenScharbach.TWEngine.GameScreens;
using ImageNexus.BenScharbach.TWEngine.Utilities;
using System.Diagnostics;

namespace TWEngine.TerrainTools
{
    ///<summary>
    /// The <see cref="SaveMapsTool"/> class is used to save the game maps.
    ///</summary>
    public partial class SaveMapsTool : Form
    {
        // 8/12/2008 - Add ITerrainShape Interface
        private readonly TerrainScreen _terrainScreen;

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="terrain"><see cref="TerrainScreen"/> instance</param>
        public SaveMapsTool(TerrainScreen terrain)
        {
            try // 6/22/2010
            {
                // Set TerrainScreen Reference
                _terrainScreen = terrain;

                InitializeComponent();

                // 11/20/2009 - Set SP/MP choice.
                cmbMapType.SelectedIndex = (String.IsNullOrEmpty(TerrainScreen.TerrainMapGameType))
                                               ? 0 : (TerrainScreen.TerrainMapGameType == "SP") ? 0 : 1;

                // 11/20/2009 - Set MapName loaded.
                txtBoxMapSaveName.Text = TerrainScreen.TerrainMapToLoad;

                // 10/7/2009 - Update the CheckBox
                switch (TerrainScreen.SaveSelectableItemsWithMap)
                {
                    case true:
                        chkSaveSelectableItems.CheckState = CheckState.Checked;
                        break;
                    case false:
                        chkSaveSelectableItems.CheckState = CheckState.Unchecked;
                        break;
                }

                // 8/12/2008 - Populate Load ListView with Map Names
                PopulateMapListView();
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("SaveMapsTool method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/12/2008
        /// <summary>
        /// Populates the ListView Map Control with Map Names.
        /// </summary>
        private void PopulateMapListView()
        {
            try // 6/22/2010
            {
                var storageTool = new Storage();
                var mapNames = storageTool.GetSavedMapNames(@"GameData\Maps\");

                // Populate List view with MapNames,if any.
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
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("PopulateMapListView method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 8/12/2008
        /// <summary>
        /// Saves a Terrain Map, using the given name from the
        /// text box.
        /// </summary>
        /// <param name="sender">Event Sender</param>
        /// <param name="e">Event Param</param>
// ReSharper disable InconsistentNaming
        private void btnSaveMap_Click(object sender, EventArgs e)

        {
            try // 6/22/2010
            {
                var mapName = txtBoxMapSaveName.Text;
                var mapType = cmbMapType.SelectedIndex == 0 ? "SP" : "MP";

                if (!string.IsNullOrEmpty(mapName))
                    _terrainScreen.ITerrainShape.SaveTerrainData(mapName, mapType);
                else
                    MessageBox.Show(@"Saving requires you give a Map name.");
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnSaveMap_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 10/7/2009
        private void chkSaveSelectableItems_CheckedChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                // save user value to the 'TerrainScreen' class.
                switch (chkSaveSelectableItems.CheckState)
                {
                    case CheckState.Checked:
                        TerrainScreen.SaveSelectableItemsWithMap = true;
                        break;
                    case CheckState.Unchecked:
                        TerrainScreen.SaveSelectableItemsWithMap = false;
                        break;
                }
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("chkSaveSelectableItems_CheckedChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

// ReSharper restore InconsistentNaming
    }
}