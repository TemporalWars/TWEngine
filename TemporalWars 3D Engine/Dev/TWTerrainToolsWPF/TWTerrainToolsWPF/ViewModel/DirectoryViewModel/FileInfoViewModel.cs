using System.Windows.Media.Imaging;
using TWTerrainToolsWPF.DataModel;
using TWTerrainToolsWPF.DataModel.DirectoryDataModel;
using TWTerrainToolsWPF.Delegates;

namespace TWTerrainToolsWPF.ViewModel.DirectoryViewModel
{
    // 7/2/2010
    public class FileInfoViewModel : TreeViewItemViewModel
    {
        private readonly FileInfoItem _fileInfoItem;

        // 7/6/2010 - Ref to the AssetsImagePaths .
        private readonly AssetsImagePaths _assetImagePaths;

        // 7/4/2010 - Event
        public static event SelectedItemEventHandler UpdateImageBox;

        #region Properties

        // 7/7/2010
        /// <summary>
        /// Gets the current <see cref="FileInfoItem"/>.
        /// </summary>
        public FileInfoItem FileInfoItem
        {
            get { return _fileInfoItem; }
        }

        // 7/7/2010
        /// <summary>
        /// Gets the full TreeView's directory path, starting from root directory.
        /// </summary>
        /// <example>
        /// If actual directory is => "c:\Users\Ben\MyDocs\Game\GameContent\Textures\DesertTextures\texture1.png", and
        /// the TreeView is set to start at => "Textures" directory level, then this property would return the following
        /// result for the given example; "Textures\DesertTextures\texture1".
        /// </example>
        public string TreeViewDirectoryPath
        {
            get
            {
                return ((DirectoryInfoViewModel) Parent).TreeViewsDirectoryPath + @"\" + FileName;
            }
        }

        /// <summary>
        /// Gets the current file name.
        /// </summary>
        public string FileName
        {
            get { return FileInfoItem.FileName; }
        }

        /// <summary>
        /// Gets the current Icon image for this file.
        /// </summary>
        public BitmapSource FileIcon
        {
            get
            {
                BitmapSource bitmapSource;
                var keyName = FileInfoItem.FileName + KeyNameExtension; // 7/7/2010
                _assetImagePaths.GetAssetImage(keyName, out bitmapSource);
                return bitmapSource;
            }
        }

        // 7/7/2010
        /// <summary>
        /// Allows adding an extension to the end of the 'FileName' when retrieving
        /// the Image from the <see cref="AssetsImagePaths"/> class.
        /// </summary>
        /// <remarks>This extension is useful in situations where the user has come up
        /// with some type of naming convention to differentiate between the asset name and its
        /// icon image name; for example, the asset name could be 'Barrel01' and the icon image name
        /// would be 'Barrel01Pic'.  Therefore, in this example you would add 'Pic' as the extension.</remarks>
        public static string KeyNameExtension { get; set; }

        public override bool IsSelected
        {
            get
            {
                return base.IsSelected;
            }
            set
            {
                // is true
                if (value && UpdateImageBox != null)
                {
                    UpdateImageBox(this, new SelectedItemEventArgs { ImageName = FileInfoItem.FileName });
                }

                base.IsSelected = value;
            }
        }

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="directoryInfoItem">Instance of <see cref="FileInfoItem"/></param>
        /// <param name="parentDirectory">Instance of parent <see cref="DirectoryInfoViewModel"/></param>
        /// <param name="assetImagePaths">Reference to the Icon paths dictionary</param>
        public FileInfoViewModel(FileInfoItem directoryInfoItem, DirectoryInfoViewModel parentDirectory, AssetsImagePaths assetImagePaths)
            : base(parentDirectory, false)
        {
            _fileInfoItem = directoryInfoItem;
            _assetImagePaths = assetImagePaths; // 7/6/2010
        }

       
    }
}