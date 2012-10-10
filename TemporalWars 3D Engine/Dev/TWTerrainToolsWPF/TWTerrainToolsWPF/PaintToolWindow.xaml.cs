using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.DataModel;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.DataModel.DirectoryDataModel;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.DataModel.PaintToolDataModel;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.Delegates;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.Enums;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.Extentions;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.Interfaces;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.ViewModel.DirectoryViewModel;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.ViewModel.PaintToolViewModel;
using ImageNexus.BenScharbach.TWTools.TWTerrainTools_Interfaces.Structs;
using Microsoft.Xna.Framework;
using System.Windows.Threading;
using Point = System.Windows.Point;

namespace ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF
{
    /// <summary>
    /// Interaction logic for PaintToolWindow.xaml
    /// </summary>
    public partial class PaintToolWindow : Window, IOnGuiThread
    {
        public PaintTool CurrentTool = PaintTool.Select;

        // 7/7/2010 - Instance of AssetsImagePaths class.
        private readonly AssetsImagePaths _assetsImagePaths;

        // 7/7/2010 - Instance of TexturesViewModel class for listView (group-1) control.
        private readonly TexturesViewModel _texturesViewModelGroup1;
        // 7/7/2010 - Instance of TexturesViewModel class for listView (group-2) control.
        private readonly TexturesViewModel _texturesViewModelGroup2;
        
        // 7/8/2010 - DragNDrop start point.
        private Point _startPoint;

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
        /// Occurs when scroll bar updates <see cref="PaintCursorSize"/>.
        /// </summary>
        public event EventHandler PaintCursorSizeUpdated;

        /// <summary>
        /// Occurs when scroll bar updates <see cref="PaintCursorStrength"/>.
        /// </summary>
        public event EventHandler PaintCursorStrengthUpdated;

        /// <summary>
        /// Occurs when Layer-1 % updated.
        /// </summary>
        public event EventHandler Layer1PercentUpdated;

        /// <summary>
        /// Occurs when Layer-2 % updated.
        /// </summary>
        public event EventHandler Layer2PercentUpdated;

        /// <summary>
        /// Occurs when Layer-3 % updated.
        /// </summary>
        public event EventHandler Layer3PercentUpdated;

        /// <summary>
        /// Occurs when Layer-4 % updated.
        /// </summary>
        public event EventHandler Layer4PercentUpdated;

        /// <summary>
        /// Occurs when 'RebuildAlphaMap' button pressed.
        /// </summary>
        public event EventHandler RebuildAlphaMap;

        /// <summary>
        /// Occurs when DragDrop operation occurs for ListView layer-1.
        /// </summary>
        public event DragDropDelegate DragDropLayer1;

        /// <summary>
        /// Occurs when DragDrop operation occurs for ListView layer-2.
        /// </summary>
        public event DragDropDelegate DragDropLayer2;

        /// <summary>
        /// Occurs when 'CreateVol' button pressed for layer-1.
        /// </summary>
        public event EventHandler CreateVolume1;

        /// <summary>
        /// Occurs when 'CreateVol' button pressed for layer-2.
        /// </summary>
        public event EventHandler CreateVolume2;

        /// <summary>
        /// Occurs when <see cref="BlendToUse"/> is updated.
        /// </summary>
        public event EventHandler BlendToUseUpdated;

        /// <summary>
        ///  Occurs when ClearLayer-1 button pressed.
        /// </summary>
        public event EventHandler ClearLayer1;

        /// <summary>
        ///  Occurs when ClearLayer-2 button pressed.
        /// </summary>
        public event EventHandler ClearLayer2;

        /// <summary>
        ///  Occurs when InUse Layer-1 button pressed.
        /// </summary>
        public event IsToggledDelegate InUseLayer1;

        /// <summary>
        ///  Occurs when InUse Layer-2 button pressed.
        /// </summary>
        public event IsToggledDelegate InUseLayer2;

        /// <summary>
        /// Occurs when the <see cref="AmbientColorLayer1"/> is updated.
        /// </summary>
        public event EventHandler AmbientColorLayer1Updated;

        /// <summary>
        /// Occurs when the <see cref="AmbientColorLayer2"/> is updated.
        /// </summary>
        public event EventHandler AmbientColorLayer2Updated;

        /// <summary>
        /// Occurs when the <see cref="AmbientPowerLayer1"/> is updated.
        /// </summary>
        public event EventHandler AmbientPowerLayer1Updated;

        /// <summary>
        /// Occurs when the <see cref="AmbientPowerLayer2"/> is updated.
        /// </summary>
        public event EventHandler AmbientPowerLayer2Updated;

        /// <summary>
        /// Occurs when the <see cref="SpecularColorLayer1"/> is updated.
        /// </summary>
        public event EventHandler SpecularColorLayer1Updated;

        /// <summary>
        /// Occurs when the <see cref="SpecularColorLayer2"/> is updated.
        /// </summary>
        public event EventHandler SpecularColorLayer2Updated;

        /// <summary>
        /// Occurs when the <see cref="SpecularPowerLayer1"/> is updated.
        /// </summary>
        public event EventHandler SpecularPowerLayer1Updated;

        /// <summary>
        /// Occurs when the <see cref="SpecularPowerLayer2"/> is updated.
        /// </summary>
        public event EventHandler SpecularPowerLayer2Updated;

        /// <summary>
        /// Occurs when any of the Numeric-Up-Down controls are updated for Perlin-Noise group-1.
        /// </summary>
        public event EventHandler PerlinNoiseGroup1Updated;

        /// <summary>
        /// Occurs when any of the Numeric-Up-Down controls are updated for Perlin-Noise group-1.
        /// </summary>
        public event EventHandler PerlinNoiseGroup2Updated;

        /// <summary>
        /// Occurs when 'NoiseGenerator' group-1 button pressed.
        /// </summary>
        public event EventHandler NoiseGeneratorGroup1;

        /// <summary>
        /// Occurs when 'NoiseGenerator' group-2 button pressed.
        /// </summary>
        public event EventHandler NoiseGeneratorGroup2;

        /// <summary>
        /// Occurs when 'ApplyNoise' group-1 button pressed.
        /// </summary>
        public event EventHandler ApplyNoiseGroup1;

        /// <summary>
        /// Occurs when 'ApplyNoise' group-2 button pressed.
        /// </summary>
        public event EventHandler ApplyNoiseGroup2;

        /// <summary>
        /// Occurs when TreeView for textures selected item changes.
        /// </summary>
        public event EventHandler SelectedItemChangedTv1;

        /// <summary>
        /// Occurs when ListView (group-1) selected item changes.
        /// </summary>
        public event EventHandler SelectedItemChangedLv1;

        /// <summary>
        /// Occurs when ListView (group-2) selected item changes.
        /// </summary>
        public event EventHandler SelectedItemChangedLv2;
       
       

        #endregion

        #region Properties

        // 3/30/2011
        /// <summary>
        /// Get or set to start the window close cycle.
        /// </summary>
        public bool StartCloseCycle { get; set; }

        /// <summary>
        /// Gets or sets the current paint cursor size.
        /// </summary>
        public int PaintCursorSize { get; set; }

        /// <summary>
        /// Gets or sets the current paint cursor strength.
        /// </summary>
        public int PaintCursorStrength { get; set; }

        /// <summary>
        /// Gets the current Layer-1 %.
        /// </summary>
        public float Layer1Percent { get; set; }

        /// <summary>
        /// Gets the current Layer-2 %.
        /// </summary>
        public float Layer2Percent { get; set; }

        /// <summary>
        /// Gets the current Layer-3 %.
        /// </summary>
        public float Layer3Percent { get; set; }

        /// <summary>
        /// Gets the current Layer-4 %.
        /// </summary>
        public float Layer4Percent { get; set; }

        /// <summary>
        /// Gets the current interpolated 'blend' amount to use for painting.
        /// </summary>
        public float BlendToUse { get; private set; }

        /// <summary>
        /// Gets or sets the ambient color for layer-1.
        /// </summary>
        public Vector3 AmbientColorLayer1 { get; set; }

        /// <summary>
        /// Gets or sets the ambient color for layer-2.
        /// </summary>
        public Vector3 AmbientColorLayer2 { get; set; }

        /// <summary>
        /// Gets or sets the ambient power for layer-1.
        /// </summary>
        public float AmbientPowerLayer1 { get; set; }

        /// <summary>
        /// Gets or sets the ambient power for layer-2.
        /// </summary>
        public float AmbientPowerLayer2 { get; set; }

        /// <summary>
        /// Gets or sets the specular color for layer-1.
        /// </summary>
        public Vector3 SpecularColorLayer1 { get; set; }

        /// <summary>
        /// Gets or sets the specular color for layer-2.
        /// </summary>
        public Vector3 SpecularColorLayer2 { get; set; }

        /// <summary>
        /// Gets or sets the specular power for layer-1.
        /// </summary>
        public float SpecularPowerLayer1 { get; set; }

        /// <summary>
        /// Gets or sets the specular power for layer-2.
        /// </summary>
        public float SpecularPowerLayer2 { get; set; }

        /// <summary>
        /// Gets the currently selected TreeView <see cref="FileInfoViewModel"/> item.
        /// </summary>
        public FileInfoViewModel CurrentSelectedTreeViewItem { get; private set; }

        /// <summary>
        /// Gets the currently selected asset's fileName for TreeView textures.
        /// </summary>
        public string CurrentSelectedFileName
        {
            get { return CurrentSelectedTreeViewItem.FileName; }
        }

        // 7/9/2010
        /// <summary>
        /// Gets the value in the CheckBox for painting pathfinding blocks.
        /// </summary>
        public bool PaintPathfindingBlocks
        {
            get
            {
                return (cbPaintPathfindingBlocks.IsChecked == null) ? false : cbPaintPathfindingBlocks.IsChecked.Value;
            }
        }

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public PaintToolWindow()
        {
            InitializeComponent();

            // 5/8/2011 - Default to true.
            StartCloseCycle = true;
            
            // 2/5/2011
            _assetsImagePaths = new AssetsImagePaths();
            
            // 7/7/2010 - Set EventHandler
            FileInfoViewModel.UpdateImageBox += FileInfoViewModel_UpdateImageBox;

            // 7/7/2010 - Create instance of listView control ViewModels
            // Group-1
            _texturesViewModelGroup1 = new TexturesViewModel(_assetsImagePaths);
            _texturesViewModelGroup1.SelectedItemEvent += TextureItemViewModel_UpdateImageBox; // 7/8/2010
            _texturesViewModelGroup1.SelectedItemEvent += _texturesViewModelGroup1_SelectedItemEvent; // 7/8/2010
            listViewGroup1.DataContext = _texturesViewModelGroup1; // Set ViewModel source on Listview.

            // Group-2
            _texturesViewModelGroup2 = new TexturesViewModel(_assetsImagePaths);
            _texturesViewModelGroup2.SelectedItemEvent += TextureItemViewModel_UpdateImageBox; // 7/8/2010
            _texturesViewModelGroup2.SelectedItemEvent += _texturesViewModelGroup2_SelectedItemEvent;
            listViewGroup2.DataContext = _texturesViewModelGroup2; // Set ViewModel source on Listview.

            // 2/4/2011 - Store reference to this.
            ToolInstance.OnGuiThread = this;

            // 2/4/2011 - Start Dispatcher Thread on Form.
            Dispatcher.BeginInvoke(new Action(delegate() { }), DispatcherPriority.Normal);

            #region TESTING TreeView

            // 7/6/2010 - Init TreeView with new directory data
            /*var visualStudioDir = @"D:\Users\Ben\Documents\Visual Studio 2008";
            var texturesDirPath = Path.GetDirectoryName(visualStudioDir + @"\\Projects\\XNA_RTS2008\\Dev\\ContentForResources\\ContentTextures\\high512x\\Terrain\\");
            var directoryForTextures = new DirectoryInfo(texturesDirPath);

            // 7/4/2010 - BuildImage
            //var imagesDirPath = Path.GetDirectoryName(visualStudioDir + @"\\Projects\\XNA_RTS2008\\Dev\\ItemToolPics\\");
            var directoryForPics = new DirectoryInfo(texturesDirPath);
            _assetsImagePaths.BuildImageList(directoryForPics, s => !Path.GetFileNameWithoutExtension(s.Name).EndsWith("Normal"));

            var directoryInfoItem = new DirectoryInfoItem[1];
            directoryInfoItem[0] = new DirectoryInfoItem(directoryForTextures);

            var directoryVieWModel = new DirectoryViewModel(directoryInfoItem, s => !Path.GetFileNameWithoutExtension(s.Name).EndsWith("Normal"), _assetsImagePaths);
            DataContext = directoryVieWModel;*/

            #endregion

            #region TESTING ListView

            // add item to list
            //AddItemToLayerGroup1("Grass01", 0);
            //AddItemToLayerGroup1("Grass01", 1);
            //AddItemToLayerGroup1("Grass01", 2);
            //AddItemToLayerGroup1("Grass01", 3);

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

        // 7/8/2010
        /// <summary>
        /// Occurs when some item is selected within the ListView (Group-2) control.
        /// </summary>
// ReSharper disable InconsistentNaming
        void _texturesViewModelGroup2_SelectedItemEvent(object sender, SelectedItemEventArgs e)
// ReSharper restore InconsistentNaming
        {
            // trigger event
            if (SelectedItemChangedLv2 != null)
                SelectedItemChangedLv2(this, EventArgs.Empty);
        }

        // 7/8/2010
        /// <summary>
        /// Occurs when some item is selected within the ListView (Group-1) control.
        /// </summary>
// ReSharper disable InconsistentNaming
        void _texturesViewModelGroup1_SelectedItemEvent(object sender, SelectedItemEventArgs e)
// ReSharper restore InconsistentNaming
        {
            // trigger event
            if (SelectedItemChangedLv1 != null)
                SelectedItemChangedLv1(this, EventArgs.Empty);
        }
        

        // 7/7/2010
        /// <summary>
        /// Occurs when some item is selected within the ListView controls.
        /// </summary>
        void TextureItemViewModel_UpdateImageBox(object sender, SelectedItemEventArgs e)
        {
            // create keyName for dictionary
            var keyName = e.ImageName;

            // set imageBox control with texture.
            SetImageBox(keyName);
        }

        /// <summary>
        /// Occurs when user clicks on some file in the TreeView.
        /// </summary>
        void FileInfoViewModel_UpdateImageBox(object sender, SelectedItemEventArgs e)
        {
            // retrieve sender
            var fileInfoVm = (FileInfoViewModel)sender;

            // set currently selected item, for use to outside callers.
            CurrentSelectedTreeViewItem = fileInfoVm;

            // create keyName for dictionary
            var keyName = e.ImageName;

            // set imageBox control with texture.
            SetImageBox(keyName);
        }

        // 7/7/2010
        /// <summary>
        /// Retrieves the proper TextureImage from internal <see cref="AssetsImagePaths"/>,
        ///  using the given <paramref name="keyName"/>, and sets into the forms control's 'ImageBox'.
        /// </summary>
        /// <param name="keyName">Texture name to retrieve</param>
        public void SetImageBox(string keyName)
        {
            // 7/7/2010 - try retrieve from AssetsImagePaths class
            BitmapSource bitmapSource;
            if (_assetsImagePaths.GetAssetImage(keyName, out bitmapSource))
                imageBox.Source = bitmapSource;
        }

        // 7/5/2010
        /// <summary>
        /// Allows setting some error message for display to the windows form.
        /// </summary>
        /// <param name="errorMessage">Error message to display</param>
        public void SetErrorMessage(string errorMessage)
        {
            txtFloodErrorMessages.Text = errorMessage;
        }

        // 7/5/2010
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

        // 7/6/2010
        /// <summary>
        /// Retrieves the current Perlin-Noise attributes for group-1.
        /// </summary>
        /// <param name="perlinNoisePass">(OUT) <see cref="PerlinNoisePass"/> structure</param>
        public void GetPerlinNoiseAttributesGroup1(out PerlinNoisePass perlinNoisePass)
        {
            // populate pass-1 attributes.
            perlinNoisePass = new PerlinNoisePass
            {
                RandomSeed = (int)nudRandomSeedValue_g1.Value,
                PerlinNoiseSize = (float)nudNoiseSize_g1.Value,
                PerlinPersistence = (float)nudPersistence_g1.Value,
                PerlinOctaves = (int)nudOctaves_g1.Value
            };
        }

        // 7/9/2010
        /// <summary>
        /// Sets the current Perlin-Noise attributes for group-1
        /// </summary>
        /// <param name="perlinNoisePass"><see cref="PerlinNoisePass"/> structure with new attributes</param>
        public void SetPerlinNoiseAttributesGroup1(ref PerlinNoisePass perlinNoisePass)
        {
            nudRandomSeedValue_g1.Value = perlinNoisePass.RandomSeed;
            nudNoiseSize_g1.Value = (decimal)perlinNoisePass.PerlinNoiseSize;
            nudPersistence_g1.Value = (decimal) perlinNoisePass.PerlinPersistence;
            nudOctaves_g1.Value = perlinNoisePass.PerlinOctaves;
        }

        // 7/6/2010
        /// <summary>
        /// Retrieves the current Perlin-Noise attributes for group-2.
        /// </summary>
        /// <param name="perlinNoisePass">(OUT) <see cref="PerlinNoisePass"/> structure</param>
        public void GetPerlinNoiseAttributesGroup2(out PerlinNoisePass perlinNoisePass)
        {
            // populate pass-1 attributes.
            perlinNoisePass = new PerlinNoisePass
            {
                RandomSeed = (int)nudRandomSeedValue_g2.Value,
                PerlinNoiseSize = (float)nudNoiseSize_g2.Value,
                PerlinPersistence = (float)nudPersistence_g2.Value,
                PerlinOctaves = (int)nudOctaves_g2.Value
            };
        }

        // 7/9/2010
        /// <summary>
        /// Sets the current Perlin-Noise attributes for group-2
        /// </summary>
        /// <param name="perlinNoisePass"><see cref="PerlinNoisePass"/> structure with new attributes</param>
        public void SetPerlinNoiseAttributesGroup2(ref PerlinNoisePass perlinNoisePass)
        {
            nudRandomSeedValue_g2.Value = perlinNoisePass.RandomSeed;
            nudNoiseSize_g2.Value = (decimal)perlinNoisePass.PerlinNoiseSize;
            nudPersistence_g2.Value = (decimal)perlinNoisePass.PerlinPersistence;
            nudOctaves_g2.Value = perlinNoisePass.PerlinOctaves;
        }

        // 7/6/2010; 1/9/2011 - Add new directoryFilterBy param.
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
            _assetsImagePaths.BuildImageList(directoryForIconPics, filesFilterBy, directoryFilterBy);

            // set as DataContext of TreeView
            var directoryVieWModel = new DirectoryViewModel(directoryInfoItem, filesFilterBy, directoryFilterBy, _assetsImagePaths);
            DataContext = directoryVieWModel;
        }

        // 7/7/2010
        /// <summary>
        /// Creates a <see cref="TextureItem"/> and adds to ListView control for Layer-1.
        /// </summary>
        /// <param name="textureName">Name of texture</param>
        /// <param name="insertAt">Index location to insert this record</param>
        public void AddItemToLayerGroup1(string textureName, int insertAt)
        {
            // create new TextureItem
            var textureItem = new TextureItem(textureName);

            // Add to TextureView list control
            _texturesViewModelGroup1.AddTextureItem(textureItem, insertAt);

            // 7/8/2010 - Refresh ListView
            listViewGroup1.Items.Refresh();
        }

        // 7/7/2010
        /// <summary>
        /// Creates a <see cref="TextureItem"/> and adds to ListView control for Layer-2.
        /// </summary>
        /// <param name="textureName">Name of texture</param>
        /// <param name="insertAt">Index location to insert this record</param>
        public void AddItemToLayerGroup2(string textureName, int insertAt)
        {
            // create new TextureItem
            var textureItem = new TextureItem(textureName);

            // Add to TextureView list control
            _texturesViewModelGroup2.AddTextureItem(textureItem, insertAt);

            // 7/8/2010 - Refresh ListView
            listViewGroup2.Items.Refresh();
        }

        // 7/7/2010
        /// <summary>
        /// Removes a <see cref="TextureItem"/> from ListView control for Layer-1, at
        /// given index <paramref name="removeAt"/>.
        /// </summary>
        /// <param name="removeAt">Index location to remove record from</param>
        public void RemoveItemInLayerGroup1(int removeAt)
        {
            // Remove record
            _texturesViewModelGroup1.RemoveTextureItem(removeAt);

            // 7/8/2010 -  Refresh ListView
            listViewGroup1.Items.Refresh();
        }

        // 7/7/2010
        /// <summary>
        /// Removes a <see cref="TextureItem"/> from ListView control for Layer-2, at
        /// given index <paramref name="removeAt"/>.
        /// </summary>
        /// <param name="removeAt">Index location to remove record from</param>
        public void RemoveItemInLayerGroup2(int removeAt)
        {
            // Remove record
            _texturesViewModelGroup2.RemoveTextureItem(removeAt);

            // 7/8/2010 - Refresh ListView
            listViewGroup2.Items.Refresh();
        }

        // 7/7/2010
        /// <summary>
        /// Returns the 'IsSelected' <see cref="TextureItem"/> in ListView control for Layer-1.
        /// </summary>
        /// <param name="textureName">(OUT) Texture name of selected item.</param>
        /// <returns>Index location where item found within collection; -1 if no result.</returns>
        public int GetSelectedItemInLayerGroup1(out string textureName)
        {
            TextureItem textureItem;
            var indexFoundAt = _texturesViewModelGroup1.GetSelectedItem(out textureItem);

            textureName = (textureItem != null) ? textureItem.TextureName : string.Empty;
            return indexFoundAt;
        }

        // 7/7/2010
        /// <summary>
        /// Returns the 'IsSelected' <see cref="TextureItem"/> in ListView control for Layer-2.
        /// </summary>
        /// <param name="textureName">(OUT) Texture name of selected item.</param>
        /// <returns>Index location where item found within collection; -1 if no result.</returns>
        public int GetSelectedItemInLayerGroup2(out string textureName)
        {
            TextureItem textureItem;
            var indexFoundAt = _texturesViewModelGroup2.GetSelectedItem(out textureItem);

            textureName = (textureItem != null) ? textureItem.TextureName : string.Empty;
            return indexFoundAt;
        }

        // 7/5/2010
        /// <summary>
        /// Event handler, which updates the <see cref="PaintCursorSize"/>
        /// </summary>
// ReSharper disable InconsistentNaming
        private void hScrollBarSize_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                //update value
                PaintCursorSize = (int) hScrollBarSize.Value;

                // trigger event
                if (PaintCursorSizeUpdated != null)
                    PaintCursorSizeUpdated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("hScrollBarSize_Scroll method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/5/2010
        /// <summary>
        /// Event handler, which updates the <see cref="PaintCursorStrength"/>
        /// </summary>
// ReSharper disable InconsistentNaming
        private void hScrollBarIntensity_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                //update value
                PaintCursorStrength = (int)hScrollBarSize.Value;

                // trigger event
                if (PaintCursorStrengthUpdated != null)
                    PaintCursorStrengthUpdated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("hScrollBarIntensity_Scroll method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Resets the PaintTool buttons to the default mode.
        /// </summary>
        /// <param name="tool"><see cref="PaintTool"/> Enum</param>
        public void ResetToolSelection(PaintTool tool)
        {
            try // 6/22/2010
            {
                if (tool != PaintTool.Select)
                    btnPaintOff.IsChecked = false;
                if (tool != PaintTool.Fill)
                    btnPaint.IsChecked = false;
                if (tool != PaintTool.Unfill)
                    btnErase.IsChecked = false;
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("ResetToolSelection method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Sets as current <see cref="PaintTool"/> to use.
        /// </summary>
        /// <param name="tool"></param>
        private void SelectTool(PaintTool tool)
        {
            try // 6/22/2010
            {
                ResetToolSelection(tool);

                switch (tool)
                {
                    case PaintTool.Select:
                        CurrentTool = PaintTool.Select;
                        break;
                    case PaintTool.Fill:
                        CurrentTool = PaintTool.Fill;
                        break;
                    case PaintTool.Unfill:
                        CurrentTool = PaintTool.Unfill;
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

        // 7/5/2010
        /// <summary>
        /// Occurs when paint-off button pressed.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnPaintOff_Checked(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                SelectTool(PaintTool.Select);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnPaintOff_Checked method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/5/2010
        /// <summary>
        /// Occurs when paint button pressed.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnPaint_Checked(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
               SelectTool(PaintTool.Fill);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnPaint_Checked method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }


        // 7/5/2010
        /// <summary>
        /// Occurs when erase button pressed.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnErase_Checked(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                SelectTool(PaintTool.Unfill);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnErase_Checked method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/5/2010
        /// <summary>
        /// Occurs when Layer-1 % value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void layer1Percent_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try
            {
                // Set new percent value
                var spinner1 = (UserNumUpDownControl)sender;
                Layer1Percent = (float)spinner1.Value / 100;

                // trigger event
                if (Layer1PercentUpdated != null)
                    Layer1PercentUpdated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("layer1Percent_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/5/2010
        /// <summary>
        /// Occurs when Layer-2 % value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void layer2Percent_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // Set new percent value
                var spinner2 = (UserNumUpDownControl)sender;
                Layer2Percent = (float)spinner2.Value / 100;

                // trigger event
                if (Layer2PercentUpdated != null)
                    Layer2PercentUpdated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("layer2Percent_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/5/2010
        /// <summary>
        /// Occurs when Layer-3 % value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void layer3Percent_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // Set new percent value
                var spinner3 = (UserNumUpDownControl)sender;
                Layer3Percent = (float)spinner3.Value / 100;

                // trigger event
                if (Layer3PercentUpdated != null)
                    Layer3PercentUpdated(this, EventArgs.Empty);

            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("layer3Percent_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/5/2010
        /// <summary>
        /// Occurs when Layer-4 % value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void layer4Percent_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // Set new percent value
                var spinner4 = (UserNumUpDownControl)sender;
                Layer4Percent = (float)spinner4.Value / 100;

                // trigger event
                if (Layer4PercentUpdated != null)
                    Layer4PercentUpdated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("layer4Percent_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/5/2010
        /// <summary>
        /// Occurs when RebuildAlphaMap button pressed.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnRebuildAlphaMap_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // trigger event
                if (RebuildAlphaMap != null)
                    RebuildAlphaMap(this, EventArgs.Empty);

            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnRebuildAlphaMap_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/5/2010
        /// <summary>
        /// Occurs when CreateVol-1 button pressed.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnCreateTextureVol1_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // trigger event
                if (CreateVolume1 != null)
                    CreateVolume1(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnCreateTextureVol1_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/5/2010
        /// <summary>
        /// Occurs when CreateVol-2 button pressed.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnCreateTextureVol2_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // trigger event
                if (CreateVolume2 != null)
                    CreateVolume2(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnCreateTextureVol2_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/5/2010
        /// <summary>
        /// Occurs when ScrollBar 'Blend' is updated.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void hScrollBarBlend_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // update blend value
                BlendToUse = (float) e.NewValue;

                // trigger event
                if (BlendToUseUpdated != null)
                    BlendToUseUpdated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("hScrollBarBlend_Scroll method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/5/2010
        /// <summary>
        /// Occurs when ClearLayer-1 button is pressed.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnClearLayer1_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // trigger event
                if (ClearLayer1 != null)
                    ClearLayer1(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnClearLayer1_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/5/2010
        /// <summary>
        /// Occurs when ClearLayer-2 button is pressed.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnClearLayer2_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // trigger event
                if (ClearLayer2 != null)
                    ClearLayer2(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnClearLayer2_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/5/2010
        /// <summary>
        /// Occurs when UseLayer-1 button is pressed.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnInUseLayer1_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // check button state
                var isChecked = btnInUseLayer1.IsChecked;
                if (isChecked != null)
                {
                    if (isChecked.Value)
                    {
                        // update button text
                        btnInUseLayer1.Content = "In Use";

                        // update button color
                        btnInUseLayer1.Foreground = new SolidColorBrush(Colors.DarkGreen);

                        // update font weight
                        btnInUseLayer1.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        // update button text
                        btnInUseLayer1.Content = "Not in Use";

                        // update button color
                        btnInUseLayer1.Foreground = new SolidColorBrush(Colors.DarkRed);

                        // update font weight
                        btnInUseLayer1.FontWeight = FontWeights.Normal;
                    }
                }

                // trigger event
                if (InUseLayer1 != null)
                    InUseLayer1(this, new IsToggledEventArgs { IsToggled = (isChecked == null) ? false : isChecked.Value });
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnInUseLayer1_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/5/2010
        /// <summary>
        /// Occurs when UseLayer-2 button is pressed.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnInUseLayer2_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // check button state
                var isChecked = btnInUseLayer2.IsChecked;
                if (isChecked != null)
                {
                    if (isChecked.Value)
                    {
                        // update button text
                        btnInUseLayer2.Content = "In Use";

                        // update button color
                        btnInUseLayer2.Foreground = new SolidColorBrush(Colors.DarkGreen);

                        // update font weight
                        btnInUseLayer2.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        // update button text
                        btnInUseLayer2.Content = "Not in Use";

                        // update button color
                        btnInUseLayer2.Foreground = new SolidColorBrush(Colors.DarkRed);

                        // update font weight
                        btnInUseLayer2.FontWeight = FontWeights.Normal;
                    }
                }

                // trigger event
                if (InUseLayer2 != null)
                    InUseLayer2(this, new IsToggledEventArgs { IsToggled = (isChecked == null) ? false : isChecked.Value });
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnInUseLayer2_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/5/2010
        /// <summary>
        /// Occurs when Numeric-Up-Down control value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void ambientColorLayer1_R_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // Update value
                var ambientColorLayer1 = AmbientColorLayer1;
                ambientColorLayer1.X = (float)ambientColorLayer1_R.Value;
                AmbientColorLayer1 = ambientColorLayer1;

                // trigger event
                if (AmbientColorLayer1Updated != null)
                    AmbientColorLayer1Updated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("ambientColorLayer1_R_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/5/2010
        /// <summary>
        ///  Occurs when Numeric-Up-Down control value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void ambientColorLayer1_G_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // Update value
                var ambientColorLayer1 = AmbientColorLayer1;
                ambientColorLayer1.Y = (float)ambientColorLayer1_G.Value;
                AmbientColorLayer1 = ambientColorLayer1;

                // trigger event
                if (AmbientColorLayer1Updated != null)
                    AmbientColorLayer1Updated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("ambientColorLayer1_G_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/5/2010
        /// <summary>
        /// Occurs when Numeric-Up-Down control value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void ambientColorLayer1_B_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // Update value
                var ambientColorLayer1 = AmbientColorLayer1;
                ambientColorLayer1.Z = (float)ambientColorLayer1_B.Value;
                AmbientColorLayer1 = ambientColorLayer1;

                // trigger event
                if (AmbientColorLayer1Updated != null)
                    AmbientColorLayer1Updated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("ambientColorLayer1_B_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/5/2010
        /// <summary>
        /// Occurs when Numeric-Up-Down control value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void ambientPowerLayer1_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // Update value
                AmbientPowerLayer1 = (float)ambientPowerLayer1.Value;

                // trigger event
                if (AmbientPowerLayer1Updated != null)
                    AmbientPowerLayer1Updated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("ambientPowerLayer1_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/6/2010
        /// <summary>
        /// Occurs when Numeric-Up-Down control value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void specularColorLayer1_R_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // Update value
                var specularColorLayer1 = SpecularColorLayer1;
                specularColorLayer1.X = (float)specularColorLayer1_R.Value;
                SpecularColorLayer1 = specularColorLayer1;

                // trigger event
                if (SpecularColorLayer1Updated != null)
                    SpecularColorLayer1Updated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("specularColorLayer1_R_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/6/2010
        /// <summary>
        /// Occurs when Numeric-Up-Down control value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void specularColorLayer1_G_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // Update value
                var specularColorLayer1 = SpecularColorLayer1;
                specularColorLayer1.Y = (float)specularColorLayer1_G.Value;
                SpecularColorLayer1 = specularColorLayer1;

                // trigger event
                if (SpecularColorLayer1Updated != null)
                    SpecularColorLayer1Updated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("specularColorLayer1_G_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/6/2010
        /// <summary>
        /// Occurs when Numeric-Up-Down control value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void specularColorLayer1_B_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // Update value
                var specularColorLayer1 = SpecularColorLayer1;
                specularColorLayer1.Z = (float)specularColorLayer1_B.Value;
                SpecularColorLayer1 = specularColorLayer1;

                // trigger event
                if (SpecularColorLayer1Updated != null)
                    SpecularColorLayer1Updated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("specularColorLayer1_B_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/6/2010
        /// <summary>
        /// Occurs when Numeric-Up-Down control value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void specularPowerLayer1_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // Update value
                SpecularPowerLayer1 = (float)specularPowerLayer1.Value;

                // trigger event
                if (SpecularPowerLayer1Updated != null)
                    SpecularPowerLayer1Updated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("specularPowerLayer1_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/6/2010
        /// <summary>
        /// Occurs when Numeric-Up-Down control value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void ambientColorLayer2_R_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // update value
                var ambientColorLayer2 = AmbientColorLayer2;
                ambientColorLayer2.X = (float)ambientColorLayer2_R.Value;
                AmbientColorLayer2 = ambientColorLayer2;

                // trigger event
                if (AmbientColorLayer2Updated != null)
                    AmbientColorLayer2Updated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("ambientColorLayer2_R_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/6/2010
        /// <summary>
        /// Occurs when Numeric-Up-Down control value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void ambientColorLayer2_G_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // update value
                var ambientColorLayer2 = AmbientColorLayer2;
                ambientColorLayer2.Y = (float)ambientColorLayer2_G.Value;
                AmbientColorLayer2 = ambientColorLayer2;

                // trigger event
                if (AmbientColorLayer2Updated != null)
                    AmbientColorLayer2Updated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("ambientColorLayer2_G_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/6/2010
        /// <summary>
        /// Occurs when Numeric-Up-Down control value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void ambientColorLayer2_B_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // update value
                var ambientColorLayer2 = AmbientColorLayer2;
                ambientColorLayer2.Z = (float)ambientColorLayer2_B.Value;
                AmbientColorLayer2 = ambientColorLayer2;

                // trigger event
                if (AmbientColorLayer2Updated != null)
                    AmbientColorLayer2Updated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("ambientColorLayer2_B_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/6/2010
        /// <summary>
        /// Occurs when Numeric-Up-Down control value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void ambientPowerLayer2_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // update value
                AmbientPowerLayer2 = (float)ambientPowerLayer2.Value;

                // trigger event
                if (AmbientPowerLayer2Updated != null)
                    AmbientPowerLayer2Updated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("ambientPowerLayer2_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/6/2010
        /// <summary>
        /// Occurs when Numeric-Up-Down control value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void specularColorLayer2_R_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // update value
                var specularColorLayer2 = SpecularColorLayer2;
                specularColorLayer2.X = (float)specularColorLayer2_R.Value;
                SpecularColorLayer2 = specularColorLayer2;

                // trigger event
                if (SpecularColorLayer2Updated != null)
                    SpecularColorLayer2Updated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("specularColorLayer2_R_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/6/2010
        /// <summary>
        /// Occurs when Numeric-Up-Down control value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void specularColorLayer2_G_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // update value
                var specularColorLayer2 = SpecularColorLayer2;
                specularColorLayer2.Y = (float)specularColorLayer2_G.Value;
                SpecularColorLayer2 = specularColorLayer2;

                // trigger event
                if (SpecularColorLayer2Updated != null)
                    SpecularColorLayer2Updated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("specularColorLayer2_G_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/6/2010
        /// <summary>
        /// Occurs when Numeric-Up-Down control value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void specularColorLayer2_B_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // update value
                var specularColorLayer2 = SpecularColorLayer2;
                specularColorLayer2.Z = (float)specularColorLayer2_B.Value;
                SpecularColorLayer2 = specularColorLayer2;

                // trigger event
                if (SpecularColorLayer2Updated != null)
                    SpecularColorLayer2Updated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("specularColorLayer2_B_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/6/2010
        /// <summary>
        /// Occurs when Numeric-Up-Down control value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void specularPowerLayer2_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // update value
                SpecularPowerLayer2 = (float)specularPowerLayer2.Value;

                // trigger event
                if (SpecularPowerLayer2Updated != null)
                    SpecularPowerLayer2Updated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("specularPowerLayer2_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/6/2010
        /// <summary>
        /// Occurs when Numeric-Up-Down control value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void nudRandomSeedValue_g1_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // trigger event
                if (PerlinNoiseGroup1Updated != null)
                    PerlinNoiseGroup1Updated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nudRandomSeedValue_g1_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/6/2010
        /// <summary>
        /// Occurs when Numeric-Up-Down control value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void nudNoiseSize_g1_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // trigger event
                if (PerlinNoiseGroup1Updated != null)
                    PerlinNoiseGroup1Updated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nudNoiseSize_g1_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/6/2010
        /// <summary>
        /// Occurs when Numeric-Up-Down control value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void nudPersistence_g1_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // trigger event
                if (PerlinNoiseGroup1Updated != null)
                    PerlinNoiseGroup1Updated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nudPersistence_g1_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/6/2010
        /// <summary>
        /// Occurs when Numeric-Up-Down control value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void nudOctaves_g1_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // trigger event
                if (PerlinNoiseGroup1Updated != null)
                    PerlinNoiseGroup1Updated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nudOctaves_g1_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/6/2010
        /// <summary>
        /// Occurs when Numeric-Up-Down control value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void nudRandomSeedValue_g2_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // trigger event
                if (PerlinNoiseGroup2Updated != null)
                    PerlinNoiseGroup2Updated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nudRandomSeedValue_g2_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/6/2010
        /// <summary>
        /// Occurs when Numeric-Up-Down control value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void nudNoiseSize_g2_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // trigger event
                if (PerlinNoiseGroup2Updated != null)
                    PerlinNoiseGroup2Updated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nudNoiseSize_g2_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/6/2010
        /// <summary>
        /// Occurs when Numeric-Up-Down control value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void nudPersistence_g2_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // trigger event
                if (PerlinNoiseGroup2Updated != null)
                    PerlinNoiseGroup2Updated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nudPersistence_g2_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/6/2010
        /// <summary>
        /// Occurs when Numeric-Up-Down control value changes.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void nudOctaves_g2_ValueChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // trigger event
                if (PerlinNoiseGroup2Updated != null)
                    PerlinNoiseGroup2Updated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nudOctaves_g2_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/6/2010
        /// <summary>
        /// Occurs when the NoiseGenerator button for group-1 is pressed.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnNoiseGenerator_g1_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // trigger event
                if (NoiseGeneratorGroup1 != null)
                    NoiseGeneratorGroup1(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnNoiseGenerator_g1_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/6/2010
        /// <summary>
        /// Occurs when the NoiseGenerator button for group-2 is pressed.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnNoiseGenerator_g2_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // trigger event
                if (NoiseGeneratorGroup2 != null)
                    NoiseGeneratorGroup2(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnNoiseGenerator_g2_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/6/2010
        /// <summary>
        /// Occurs when the ApplyNoise button for group-1 is pressed.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnApplyNoise_g1_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // trigger event
                if (ApplyNoiseGroup1 != null)
                    ApplyNoiseGroup1(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnApplyNoise_g1_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/6/2010
        /// <summary>
        /// Occurs when the ApplyNoise button for group-1 is pressed.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnApplyNoise_g2_Click(object sender, RoutedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // trigger event
                if (ApplyNoiseGroup2 != null)
                    ApplyNoiseGroup2(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnApplyNoise_g2_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 7/7/2010
        /// <summary>
        /// Occurs when user selects item in Treeview for textures.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void tvTextures_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
// ReSharper restore InconsistentNaming
        {
            try // 6/22/2010
            {
                // trigger event
                if (SelectedItemChangedTv1 != null)
                    SelectedItemChangedTv1(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("tvTextures_SelectedItemChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        #region DragNDrop functionality

        // 7/8/2010
        /// <summary>
        /// Stores the starting mouse position for a Drag-N-Drop operation.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void tvTextures_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
// ReSharper restore InconsistentNaming
        {
            // Store the mouse position
            _startPoint = e.GetPosition(null);
        }

        // 7/8/2010
        /// <summary>
        /// Starts DragDrop operation, retrieving the current <see cref="TreeViewItem"/> mouse cursor is
        /// currently selecting in <see cref="TreeView"/>.  <see cref="TreeViewItem"/> internal 'Header' property
        /// is checked to see if the proper <see cref="FileInfoViewModel"/> item has been picked; if not, the
        /// current DragDrop operation stops.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void tvTextures_PreviewMouseMove(object sender, MouseEventArgs e)
// ReSharper restore InconsistentNaming
        {
            // Get the current mouse position
            var mousePos = e.GetPosition(null);
            var diff = _startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                // Get the dragged TreeViewItem
                var treeView = sender as TreeView;
                if (treeView == null) return;
                var treeViewItem = FindAnchestor<TreeViewItem>((DependencyObject)e.OriginalSource);
                if (treeViewItem == null) return;

                // check if Header exist which should have datacontext type!
                if (!treeViewItem.HasHeader) return;
                var header = treeViewItem.Header;
                // check if Header is 'FileInfoViewModel' type.
                var fileInfoViewModelItem = header as FileInfoViewModel;
                if (fileInfoViewModelItem == null) return;

                // Initialize the drag & drop operation
                var dragData = new DataObject("FileInfoFormat", fileInfoViewModelItem);
                DragDrop.DoDragDrop(treeViewItem, dragData, DragDropEffects.Copy);
            } 
        }

        static UIElement GetDragElementFromHitTest(UIElement dragSourceContainer, MouseEventArgs args)
        {
            var hr = VisualTreeHelper.HitTest(dragSourceContainer, args.GetPosition(dragSourceContainer));
            return hr.VisualHit as UIElement;
        }
         
        /// <summary>
        /// Helper to search up the VisualTree
        /// </summary>
        /// <typeparam name="T">Control item to search for; for example, a <see cref="ListViewItem"/>.</typeparam>
        /// <param name="current">Container object to search, which can be cast to <see cref="DependencyObject"/></param>
        /// <returns>Instance of <typeparamref name="T"/></returns>
        private static T FindAnchestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        // 7/8/2010
        /// <summary>
        /// Occurs when mouse cursor enters drop control during a DragDrop operation; then the
        /// data type is checked to verify proper item is being dragged into the control.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void listViewGroup1_DragEnter(object sender, DragEventArgs e)
// ReSharper restore InconsistentNaming
        {
            if (!e.Data.GetDataPresent("FileInfoViewModel") || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }

        }

        // 7/8/2010
        /// <summary>
        /// Completes the DragDrop operation, by checking if the item to be dropped is the
        /// correct type.  Then the <see cref="ListViewItem"/> is retrieved for the position the
        /// mouse cursor is currently at.  Finally, the index of this item is retrieved and used to
        /// update the <see cref="ListView"/> control with the drop item.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void listViewGroup1_Drop(object sender, DragEventArgs e)
// ReSharper restore InconsistentNaming
        {
            if (!e.Data.GetDataPresent("FileInfoFormat")) return;

            var fileInfoViewModelItem = e.Data.GetData("FileInfoFormat") as FileInfoViewModel;
            if (fileInfoViewModelItem == null) return;

            // get ListViewItem which mouse cursor is over
            var listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
            if (listViewItem == null) return;
            
            // get index of selected item
            var indexAt = listViewGroup1.ItemContainerGenerator.IndexFromContainer(listViewItem);

            // return if -1 index given.
            if (indexAt == -1) return;

            // update ListView with new drop item
            RemoveItemInLayerGroup1(indexAt);
            AddItemToLayerGroup1(fileInfoViewModelItem.FileName, indexAt);

            // trigger event
            if (DragDropLayer1 != null)
                DragDropLayer1(this,
                               new DragDropEventArgs
                                   {
                                       FileName = fileInfoViewModelItem.FileName,
                                       TreeViewDirectoryPath = fileInfoViewModelItem.TreeViewDirectoryPath,
                                       Index = indexAt
                                   });
        }

        // 7/8/2010
        /// <summary>
        /// Occurs when mouse cursor enters drop control during a DragDrop operation; then the
        /// data type is checked to verify proper item is being dragged into the control.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void listViewGroup2_DragEnter(object sender, DragEventArgs e)
// ReSharper restore InconsistentNaming
        {
            if (!e.Data.GetDataPresent("FileInfoViewModel") || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        // 7/8/2010
        /// <summary>
        /// Completes the DragDrop operation, by checking if the item to be dropped is the
        /// correct type.  Then the <see cref="ListViewItem"/> is retrieved for the position the
        /// mouse cursor is currently at.  Finally, the index of this item is retrieved and used to
        /// update the <see cref="ListView"/> control with the drop item.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void listViewGroup2_Drop(object sender, DragEventArgs e)
// ReSharper restore InconsistentNaming
        {
            if (!e.Data.GetDataPresent("FileInfoFormat")) return;

            var fileInfoViewModelItem = e.Data.GetData("FileInfoFormat") as FileInfoViewModel;
            if (fileInfoViewModelItem == null) return;

            // get ListViewItem which mouse cursor is over
            var listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
            if (listViewItem == null) return;

            // get index of selected item
            var indexAt = listViewGroup2.ItemContainerGenerator.IndexFromContainer(listViewItem);

            // return if -1 index given.
            if (indexAt == -1) return;

            // update ListView with new drop item
            RemoveItemInLayerGroup2(indexAt);
            AddItemToLayerGroup2(fileInfoViewModelItem.FileName, indexAt);

            // trigger event
            if (DragDropLayer2 != null)
                DragDropLayer2(this,
                               new DragDropEventArgs
                               {
                                   FileName = fileInfoViewModelItem.FileName,
                                   TreeViewDirectoryPath = fileInfoViewModelItem.TreeViewDirectoryPath,
                                   Index = indexAt
                               });
        }

        #endregion

        // 8/18/2010
        /// <summary>
        /// Captures the close event, and cancels the close action; instead, the form is simply hidden
        /// from view by setting the 'Visible' flag to False.  This fixes the issue of the HEAP crash 
        /// when trying to re instantiate a form, after it was already created in the 'TerrainEditRoutines' class.
        /// </summary>
        private void PaintTool_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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
                Debug.WriteLine("PaintTool_Closing method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 3/30/2011
        private void Window_Closed(object sender, EventArgs e)
        {
            if (FormClosed != null)
                FormClosed(this, EventArgs.Empty);
        }
        
    }
}
