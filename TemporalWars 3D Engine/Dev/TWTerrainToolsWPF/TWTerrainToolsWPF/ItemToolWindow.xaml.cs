using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.DataModel;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.DataModel.DirectoryDataModel;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.Extentions;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.Interfaces;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.ViewModel.DirectoryViewModel;
using ImageNexus.BenScharbach.TWTools.TWTerrainTools_Interfaces.Structs;

namespace ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF
{
    /// <summary>
    /// Interaction logic for ItemToolWindow.xaml
    /// </summary>
    public partial class ItemToolWindow : Window, IOnGuiThread
    {
        // 7/7/2010 - Instance of AssetsImagePaths class.
        private readonly AssetsImagePaths _assetsImagePaths = new AssetsImagePaths();

        #region Events

        // 3/30/2011
        /// <summary>
        /// Occurs when WPF form has just started the close cycle.
        /// </summary>
        public event EventHandler FormStartClose;

        // 8/18/2010
        /// <summary>
        /// Occurs when WPF form has just closed.
        /// </summary>
        public event EventHandler FormClosed;

        /// <summary>
        /// Occurs when 'GeneratePerlinNoise' button is pressed.
        /// </summary>
        public event EventHandler GeneratePerlinNoise;

        /// <summary>
        /// Occurs when 'GenerateFloodList' button is pressed.
        /// </summary>
        public event EventHandler GenerateFloodList;

        /// <summary>
        /// Occurs when 'UndoGenerateFloodList' button is pressed.
        /// </summary>
        public event EventHandler UndoGenerateFloodList;

        /// <summary>
        /// Occurs when 'DoGenerateFloodList' button is pressed.
        /// </summary>
        public event EventHandler DoGenerateFloodList;

        /// <summary>
        /// Occurs when TreeView selected item changes.
        /// </summary>
        public event EventHandler SelectedItemChanged;

        // 1/19/2011
        /// <summary>
        /// Occurs when 'PlayerNumber' item changes.
        /// </summary>
        public event EventHandler PlayerNumberChanged;

        #endregion

        #region Properties

        // 3/30/2011
        /// <summary>
        /// Get or set to start the window close cycle.
        /// </summary>
        public bool StartCloseCycle { get; set; }

        /// <summary>
        /// Gives access to the internal TreeView control.
        /// </summary>
        public TreeView TreeViewItems
        {
            get
            {
                return tvItems;
            }
        }

        // 7/4/2010
        /// <summary>
        /// Gets or set the total flood count value.
        /// </summary>
        public int FloodListCount
        {
            get
            {
                return Convert.ToInt32(txtTotalCountInFloodList.Text);
            }
            set
            {
                txtTotalCountInFloodList.Text = value.ToString();
            }
        }

        // 7/4/2010
        /// <summary>
        /// Gets the current assets instance number.
        /// </summary>
        public int InstanceNumber
        {
            set
            {
                tbInstanceNumber.Text = value.ToString();
            }
            get
            {
                return string.IsNullOrEmpty(tbInstanceNumber.Text) ? -1 : Convert.ToInt32(tbInstanceNumber.Text);
            }
        }

        // 7/4/2010
        /// <summary>
        /// Gets the currently selected asset's fileName.
        /// </summary>
        public string CurrentSelectedFileName { get; private set; }

        // 1/19/2011
        /// <summary>
        /// Gets the current PlayerNumber.
        /// </summary>
        public int PlayerNumber { get; private set; }
       

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public ItemToolWindow()
        {
            InitializeComponent();

            // 5/8/2011 - Default to true.
            StartCloseCycle = true;

            // 7/4/2010 - Set EventHandler
            FileInfoViewModel.UpdateImageBox += FileInfoViewModel_UpdateImageBox;

            // 7/7/2010 - Set the KeyExtension name.
            FileInfoViewModel.KeyNameExtension = "Pic";

            // 2/4/2011 - Store reference to this.
            ToolInstance.OnGuiThread = this;
            
            // 2/4/2011 - Start Dispatcher Thread on Form.
            Dispatcher.BeginInvoke(new Action(delegate() {}), DispatcherPriority.Normal);

            #region TESTING

            // 7/2/2010 - Init TreeView with new directory data
            /*var visualStudioDir = @"D:\Users\Ben\Documents\Visual Studio 2008";
            var modelsDirPath = Path.GetDirectoryName(visualStudioDir + @"\\Projects\\XNA_RTS2008\\Dev\\ContentForResources\\ContentAlleyPack\\");
            var directoryForModels = new DirectoryInfo(modelsDirPath);

            // 7/4/2010 - BuildImage
            var imagesDirPath =
                   Path.GetDirectoryName(visualStudioDir + @"\\Projects\\XNA_RTS2008\\Dev\\ItemToolPics\\");
            var directoryForItemToolPics = new DirectoryInfo(imagesDirPath);
            _assetsImagePaths.BuildImageList(directoryForItemToolPics,
                                             s => Path.GetFileNameWithoutExtension(s.Name).EndsWith("Pic"));

            var directoryInfoItem = new DirectoryInfoItem[1];
            directoryInfoItem[0] = new DirectoryInfoItem(directoryForModels);

            var directoryVieWModel = new DirectoryViewModel(directoryInfoItem,
                                                            fi =>
                                                            ((fi.Extension == ".X" || fi.Extension == ".FBX") ||
                                                             fi.Extension == ".x") || fi.Extension == ".fbx",
                                                            _assetsImagePaths);
            DataContext = directoryVieWModel;*/

            #endregion
        }

        // 2/4/2011
        //[ShowWaitCursor]
        public void OnGuiThreadMethodCall<T>(Action<T> action, T paramItem) where T : class
        {
            // The Work to perform on another thread
            ThreadStart start = delegate()
            {
                // ...

                // This will work as its using the dispatcher
                var op = Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    action,
                    paramItem);

                DispatcherOperationStatus status = op.Status;
                while (status != DispatcherOperationStatus.Completed)
                {
                    status = op.Wait(TimeSpan.FromMilliseconds(1000));
                    if (status == DispatcherOperationStatus.Aborted)
                    {
                        // Alert Someone
                    }
                }
            };

            // Create the thread and kick it started!
            new Thread(start).Start();

        }

        /// <summary>
        /// Occurs when user clicks on some file in the TreeView.
        /// </summary>
        void FileInfoViewModel_UpdateImageBox(object sender, EventArgs e)
        {
            // retrieve sender
            var fileInfoVm = (FileInfoViewModel) sender;

            // set currently selected item, for use to outside callers.
            CurrentSelectedFileName = fileInfoVm.FileName;

            // create keyName for dictionary
            var keyName = fileInfoVm.FileName + FileInfoViewModel.KeyNameExtension;

            // 7/7/2010 - try retrieve from AssetsImagePaths class
            BitmapSource bitmapSource;
            if (_assetsImagePaths.GetAssetImage(keyName, out bitmapSource))
                imageBox.Source = bitmapSource;
            
        }

        // 7/4/2010
        /// <summary>
        /// Allows setting the initial directories to display in the TreeView.
        /// </summary>
        /// <remarks>Since the TreeView is designed to 'Lazy-Load', only the initial root folders are required to be set at start.</remarks>
        /// <param name="directoryInfos">Array of root content directories</param>
        /// <param name="directoryForIconPics"><see cref="DirectoryInfo"/> instance where Icons are located</param>
        /// <param name="filesFilterBy">Lambda function as filterBy for base children files</param>
        /// <param name="directoryFilterBy">Lambda function as filterBy for directories.</param>
        public void CreateDataContextForTree(DirectoryInfo[] directoryInfos, DirectoryInfo directoryForIconPics, 
                                            Func<FileInfo, bool> filesFilterBy, Func<DirectoryInfo, bool> directoryFilterBy)
        {
            // create array of items for ViewModel.
            var count = directoryInfos.Length;
            var directoryInfoItem = new DirectoryInfoItem[count];

            for (var i = 0; i < count; i++)
            {
                var directoryForModels = directoryInfos[i];
                directoryInfoItem[i] = new DirectoryInfoItem(directoryForModels); 
            }

            // 2/4/2011 - Add 'DirectoryFilterBy' param.
            // populate dictionary with Icon image paths.
            _assetsImagePaths.BuildImageList(directoryForIconPics, s => Path.GetFileNameWithoutExtension(s.Name).EndsWith("Pic"), directoryFilterBy);

            // set as DataContext of TreeView
            var directoryVieWModel = new DirectoryViewModel(directoryInfoItem, filesFilterBy, directoryFilterBy, _assetsImagePaths);
            DataContext = directoryVieWModel;
        }

       

        // 7/2/2010
        /// <summary>
        /// Allows setting some error message for display to the windows form.
        /// </summary>
        /// <param name="errorMessage">Error message to display</param>
        public void SetErrorMessage(string errorMessage)
        {
            txtFloodErrorMessages.Text = errorMessage;
        }

        // 7/2/2010
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

        // 7/2/2010
        /// <summary>
        /// Retrieves the current Perlin-Noise attributes.
        /// </summary>
        /// <param name="perlinNoisePass">(OUT) <see cref="PerlinNoisePass"/> structure</param>
        public void GetPerlinNoiseAttributes(out PerlinNoisePass perlinNoisePass)
        {
            // populate pass-1 attributes.
            perlinNoisePass = new PerlinNoisePass
                                  {
                                      RandomSeed = (int) nudRandomSeedValue.Value,
                                      PerlinNoiseSize = (float) nudNoiseSize.Value,
                                      PerlinPersistence = (float) nudPersistence.Value,
                                      PerlinOctaves = (int) nudOctaves.Value
                                  };
        }

        // 7/2/2010
        /// <summary>
        /// Retrieves the current Flood-Constraints attributes.
        /// </summary>
        /// <param name="floodConstraints">(OUT) <see cref="FloodConstraints"/> structure</param>
        public void GetFloodConstraintAttributes(out FloodConstraints floodConstraints)
        {
            // populate attributes
            floodConstraints = new FloodConstraints
                                   {
                                       HeightMin = (int) nupHeightMin.Value,
                                       HeightMax = (int) nupHeightMax.Value,
                                       NoiseGreater_Lv3 = (float)nupNoiseGreater_Lv3.Value,
                                       NoiseGreater_Lv2 = (float)nupNoiseGreater_Lv2.Value,
                                       NoiseGreater_Lv1 = (float)nupNoiseGreater_Lv1.Value,
                                       Density_Lv3 = (int)nupDensity_Lv3.Value,
                                       Density_Lv2 = (int)nupDensity_Lv2.Value,
                                       Density_lv1 = (int)nupDensity_Lv1.Value,
                                       Spacing = (int)nupSpacing.Value,
                                       DensitySpacing = (int)nupDensitySpacing.Value
                                   };
        }


       

        // 7/2/2010
        /// <summary>
        /// Occurs when generate button pressed.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnGeneratePerlinNoise_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (GeneratePerlinNoise != null)
                    GeneratePerlinNoise(this, EventArgs.Empty);
            }
            catch (Exception err)
            {
#if DEBUG
                Debug.WriteLine("btnGeneratePerlinNoise_Click method threw an exception; " + err.Message ??
                                "No Message.");
#endif
            }
        }

        // 7/2/2010
        /// <summary>
        /// Occurs when generate flood button pressed.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnGenerateFloodList_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (GenerateFloodList != null)
                    GenerateFloodList(this, EventArgs.Empty);
            }
            catch (Exception err)
            {
#if DEBUG
                Debug.WriteLine("btnGenerateFloodList_Click method threw an exception; " + err.Message ??
                                "No Message.");
#endif
            }
        }

        // 7/2/2010
        /// <summary>
        /// Occurs when Undo generate flood button pressed.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnUndoLastFlood_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (UndoGenerateFloodList != null)
                    UndoGenerateFloodList(this, EventArgs.Empty);
            }
            catch (Exception err)
            {
#if DEBUG
                Debug.WriteLine("btnUndoLastFlood_Click method threw an exception; " + err.Message ??
                                "No Message.");
#endif
            }
        }

        // 7/2/2010
        /// <summary>
        /// Occurs when Do generate flood button pressed.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnPerformFlood_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (DoGenerateFloodList != null)
                    DoGenerateFloodList(this, EventArgs.Empty);
            }
            catch (Exception err)
            {
#if DEBUG
                Debug.WriteLine("btnPerformFlood_Click method threw an exception; " + err.Message ??
                                "No Message.");
#endif
            }
        }

        // 7/2/2010 - Was 'AfterSelect' in old Form version.
        /// <summary>
        /// Occurs when a new selectedItem is picked.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void tvItems_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // trigger event
                if (SelectedItemChanged != null)
                    SelectedItemChanged(this, EventArgs.Empty);
            }
            catch (Exception err)
            {
#if DEBUG
                Debug.WriteLine("tvItems_SelectedItemChanged method threw an exception; " + err.Message ??
                                "No Message.");
#endif
            }
        }

        // 8/18/2010
        /// <summary>
        /// Captures the close event, and cancels the close action; instead, the form is simply hidden
        /// from view by setting the 'Visible' flag to False.  This fixes the issue of the HEAP crash 
        /// when trying to re instantiate a form, after it was already created in the 'TerrainEditRoutines' class.
        /// </summary>
        private void ItemTools_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try 
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
                Debug.WriteLine("ItemTools_Closing method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 3/30/2011
        private void Window_Closed(object sender, EventArgs e)
        {
            if (FormClosed != null)
                FormClosed(this, EventArgs.Empty);
        }

        // 1/19/2011
        /// <summary>
        /// Occurs when the PlayerNumnber Numeric UpDown control values is updated.
        /// </summary>
        private void nudPlayerNumber_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                // Set PlayerNumber
                PlayerNumber = (int) nudPlayerNumber.Value;

                // trigger event
                if (PlayerNumberChanged != null)
                    PlayerNumberChanged(this, EventArgs.Empty);
            }
            catch (Exception err)
            {
#if DEBUG
                Debug.WriteLine("nudPlayerNumber_ValueChanged method threw an exception; " + err.Message ??
                                "No Message.");
#endif
            }
        }

       


    }
}
