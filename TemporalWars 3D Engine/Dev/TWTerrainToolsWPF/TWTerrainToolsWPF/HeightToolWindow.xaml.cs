using System;
using System.Windows;
using System.Windows.Controls;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.Enums;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.Extentions;
using ImageNexus.BenScharbach.TWTools.TWTerrainTools_Interfaces;
using ImageNexus.BenScharbach.TWTools.TWTerrainTools_Interfaces.Structs;

namespace ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF
{
    /// <summary>
    /// Interaction logic for HeightToolWindow.xaml
    /// </summary>
    public partial class HeightToolWindow : Window, IHeightToolWindow
    {
       
        public HeightTool CurrentTool = HeightTool.Select;

        #region Events

        // 3/30/2011
        /// <summary>
        /// Occurs when WPF 'Normals' button is pressed.
        /// </summary>
        public event EventHandler RegenerateNormals;

        // 3/30/2011
        /// <summary>
        /// Occurs when WPF form has just started the close cycle.
        /// </summary>
        public event EventHandler FormStartClose;

        /// <summary>
        /// Occurs when WPF form has just closed.
        /// </summary>
        public event EventHandler FormClosed;

        /// <summary>
        /// Occurs when 'ApplyHeightMap' button is pressed.
        /// </summary>
        public event EventHandler ApplyHeightMap;

        /// <summary>
        /// Occurs when 'CreateMap1024x1024' button is pressed.
        /// </summary>
        public event EventHandler CreateMap1024;

        /// <summary>
        /// Occurs when 'GeneratePerlinNoise' button is pressed.
        /// </summary>
        public event EventHandler GeneratePerlinNoise;

        /// <summary>
        /// Occurs when the GroundCursorSize is updated.
        /// </summary>
        public event EventHandler GroundCursorSizeUpdated;

        /// <summary>
        /// Occurs when the GroundCursorStength is updated.
        /// </summary>
        public event EventHandler GroundCursorStengthUpdated;

        /// <summary>
        /// Occurs when the ConstantFeet is updated.
        /// </summary>
        public event EventHandler ConstantFeetUpdated;

        /// <summary>
        /// Occurs when the UseConstantFeet is updated.
        /// </summary>
        public event EventHandler UseConstantFeetUpdated;

        #endregion

        #region Properties

        // 3/30/2011
        /// <summary>
        /// Get or set to start the window close cycle.
        /// </summary>
        public bool StartCloseCycle { get; set; }

        /// <summary>
        /// Get or set ground cursor size.
        /// </summary>
        public int GroundCursorSize { get; set; }

        /// <summary>
        /// Get or set ground cursor strength.
        /// </summary>
        public int GroundCursorStrength { get; set; }

        /// <summary>
        /// Constant Feet value to use.
        /// </summary>
        public bool UseConstantFeet { get; set; }

        /// <summary>
        /// Gets or sets the constant feet to add.
        /// </summary>
        public float ConstantFeetToAdd { get; set; }

        /// <summary>
        /// Get or set to show height cursor.
        /// </summary>
        public bool ShowHeightCursor { get; set; }

        /// <summary>
        /// Returns if Pass-1 checkBox is enabled.
        /// </summary>
        public bool IsPass1Enabled { get; private set; }

        /// <summary>
        /// Returns if Pass-2 checkBox is enabled.
        /// </summary>
        public bool IsPass2Enabled { get; private set; }

        #endregion
        
        /// <summary>
        /// constructor
        /// </summary>
        public HeightToolWindow()
        {
            try // 6/22/2010
            {
                InitializeComponent();

                // Default to true.
                StartCloseCycle = true;

                // 6/30/2010 - Create new instance of DispatcherTimer.
                //_dispatcherTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(1)};
                //_dispatcherTimer.Tick += DispatcherTimerTick;

            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("HeightTools method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 6/30/2010
        /// <summary>
        /// Allows setting some error message for display to the windows form.
        /// </summary>
        /// <param name="errorMessage">Error message to display</param>
        public void SetErrorMessage(string errorMessage)
        {
            txtFloodErrorMessages.Text = errorMessage;
        }

        // 6/30/2010
        /// <summary>
        /// Converts a given System 'Bitmap' to a 'BitmapImage', and stores
        /// into the pictureBox.
        /// </summary>
        /// <param name="bitmapToSet"><see cref="System.Drawing.Bitmap"/> to set into pictureBox.</param>
        public void SetPictureBoxImage(System.Drawing.Bitmap bitmapToSet)
        {
            // Now set into picture box.
            pictureBox.Source = bitmapToSet.ToBitmapImage();

        }

        // 6/30/2010
        /// <summary>
        /// Retrieves the current Perlin-Noise attributes for pass-1.
        /// </summary>
        /// <param name="perlinNoisePass">(OUT) <see cref="PerlinNoisePass"/> structure</param>
        public void GetPerlinNoiseAttributesForPass1(out PerlinNoisePass perlinNoisePass)
        {
            // populate pass-1 attributes.
            perlinNoisePass = new PerlinNoisePass
                                  {
                                      RandomSeed = (int) nudRandomSeedValue_p1.Value,
                                      PerlinNoiseSize = (float) nudNoiseSize_p1.Value,
                                      PerlinPersistence = (float) nudPersistence_p1.Value,
                                      PerlinOctaves = (int) nudOctaves_p1.Value
                                  };
        }

        // 6/30/2010
        /// <summary>
        /// Retrieves the current Perlin-Noise attributes for pass-2.
        /// </summary>
        /// <param name="perlinNoisePass">(OUT) <see cref="PerlinNoisePass"/> structure</param>
        public void GetPerlinNoiseAttributesForPass2(out PerlinNoisePass perlinNoisePass)
        {
            // populate pass-2 attributes.
            perlinNoisePass = new PerlinNoisePass
                                  {
                                      RandomSeed = (int) nudRandomSeedValue_p2.Value,
                                      PerlinNoiseSize = (float) nudNoiseSize_p2.Value,
                                      PerlinPersistence = (float) nudPersistence_p2.Value,
                                      PerlinOctaves = (int) nudOctaves_p2.Value
                                  };
        }

        // 6/30/2010
        /// <summary>
        /// Checks if mouse cursor is within the visual window control.
        /// </summary>
        /// <returns></returns>
        public bool IsMouseInControl()
        {
            try
            {
                // Use new WPF property for check.
                return IsMouseCaptureWithin;
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("IsMouseInControl method threw the exception " + ex.Message ?? "No Message");
#endif  
            }

            return false;
        }
       

        private void SelectTool(HeightTool tool)
        {
            try // 6/22/2010
            {
                ResetToolSelection(tool);

                switch (tool)
                {
                    case HeightTool.Select:
                        CurrentTool = HeightTool.Select;
                        break;
                    case HeightTool.Raise:
                        CurrentTool = HeightTool.Raise;
                        break;
                    case HeightTool.Lower:
                        CurrentTool = HeightTool.Lower;
                        break;
                    case HeightTool.Smooth:
                        CurrentTool = HeightTool.Smooth;
                        break;
                    case HeightTool.Flatten:
                        CurrentTool = HeightTool.Flatten;
                        break;
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("SelectTool method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        private void ResetToolSelection(HeightTool tool)
        {
            try // 6/22/2010
            {
                if (tool != HeightTool.Select)
                    btnSelect.IsChecked = false;
                if (tool != HeightTool.Raise)
                    btnRise.IsChecked = false;
                if (tool != HeightTool.Lower)
                    btnLower.IsChecked = false;
                if (tool != HeightTool.Smooth)
                    btnSmooth.IsChecked = false;
                if (tool != HeightTool.Flatten)
                    btnFlatten.IsChecked = false;
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("ResetToolSelection method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/7/2009
        /// <summary>
        /// Captures the close event, and cancels the close action; instead, the form is simply hidden
        /// from view by setting the 'Visible' flag to False.  This fixes the issue of the HEAP crash 
        /// when trying to re instantiate a form, after it was already created in the 'TerrainEditRoutines' class.
        /// </summary>
        private void HeightTools_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try // 6/22/2010
            {
                // 3/30/2011 - Check if start of close cycle
                if (StartCloseCycle)
                {
                    // 1/11/2011 - Fixed to close properly.
                    e.Cancel = true;
                    //Visibility = Visibility.Hidden;

                    if (FormStartClose != null)
                        FormStartClose(this, EventArgs.Empty);
                }
               
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("HeightTools_FormClosing method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 3/30/2011
        private void HeightTools_Closed(object sender, EventArgs e)
        {
            if (FormClosed != null)
                FormClosed(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when windows form loads.
        /// </summary>
        private void HeightTools_Loaded(object sender, RoutedEventArgs e)
        {
            try // 6/22/2010
            {
                ResetToolSelection(HeightTool.Select);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("HeightTools_Loaded method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Occurs when user scrolls bar.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void hScrollBar1_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                GroundCursorSize = (int) hScrollBar1.Value; // store value

                // trigger event
                if (GroundCursorSizeUpdated != null)
                    GroundCursorSizeUpdated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("hScrollBar1_Scroll method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Occurs when user scrolls bar.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void hScrollBar2_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                GroundCursorStrength = (int) hScrollBar2.Value; // store value

                // trigger event
                if (GroundCursorStengthUpdated != null)
                    GroundCursorStengthUpdated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("hScrollBar2_Scroll method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }
       
        /// <summary>
        /// Occurs when button in toggled state.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnSelect_Checked(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                SelectTool(HeightTool.Select);
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("btnSelect_Checked method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Occurs when button in toggled state.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnRise_Checked(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                SelectTool(HeightTool.Raise);
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("btnRise_Checked method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Occurs when button in toggled state.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnLower_Checked(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                SelectTool(HeightTool.Lower);
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("btnLower_Checked method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Occurs when button in toggled state.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnSmooth_Checked(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
               SelectTool(HeightTool.Smooth);
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("btnSmooth_Checked method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Occurs when button in toggled state.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnFlatten_Checked(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                SelectTool(HeightTool.Flatten);
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("btnFlatten_Checked method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Occurs when value is changed.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void trackBar1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                var tbInstance = (Slider)sender;
                // Set text box the value from scroll bar
                txtConstantFeet.Text = tbInstance.Value.ToString();
                // Set value for Height Field
                ConstantFeetToAdd = (float) tbInstance.Value;

                // 7/1/2010 - trigger event
                if (ConstantFeetUpdated != null)
                    ConstantFeetUpdated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("trackBar1_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Occurs when clicked.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void cbConstantFeet_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                var cbInstance = (CheckBox)sender;

                if (cbInstance.IsChecked != null) UseConstantFeet = (bool)cbInstance.IsChecked;

                // 7/1/2010 - trigger event
                if (UseConstantFeetUpdated != null)
                    UseConstantFeetUpdated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("cbConstantFeet_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Triggers the event for this button.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnApplyHeightMap_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // Trigger event
                if (ApplyHeightMap != null)
                    ApplyHeightMap(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("btnApplyHeightMap_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Triggers the event for this button.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnCreateMap1024x1024_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // Trigger event
                if (CreateMap1024 != null)
                    CreateMap1024(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("btnCreateMap1024x1024_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Triggers the event for this button.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnGeneratePerlinNoise_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // Trigger event
                if (GeneratePerlinNoise != null)
                    GeneratePerlinNoise(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("btnGeneratePerlinNoise_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 6/30/2010
        /// <summary>
        /// Occurs when checked box clicked.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void cbPass1Enable_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                IsPass1Enabled = (cbPass1Enable.IsChecked == null) ? false : cbPass1Enable.IsChecked.Value;
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("cbPass1Enable_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 6/30/2010
        /// <summary>
        /// Occurs when checked box clicked.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void cbPass2Enable_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                IsPass2Enabled = (cbPass2Enable.IsChecked == null) ? false : cbPass2Enable.IsChecked.Value;
            }
            catch (Exception ex)
            {
#if  DEBUG
                Debug.WriteLine("cbPass2Enable_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 3/30/2011
        /// <summary>
        /// Triggers a rebuild of the normal map on the terrain.
        /// </summary>
        private void btnRebuildNormals_Click(object sender, RoutedEventArgs e)
        {
            if (RegenerateNormals != null)
                RegenerateNormals(this, EventArgs.Empty);
        }

        
    }
}
