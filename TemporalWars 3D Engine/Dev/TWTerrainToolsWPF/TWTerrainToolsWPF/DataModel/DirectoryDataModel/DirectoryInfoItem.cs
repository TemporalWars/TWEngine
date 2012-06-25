using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using TWTerrainToolsWPF.Extentions;
using TWTerrainToolsWPF.Properties;

namespace TWTerrainToolsWPF.DataModel.DirectoryDataModel
{
    // 7/2/2010
    /// <summary>
    /// The <see cref="DirectoryInfoItem"/> class holds a single instance of the <see cref="DirectoryInfo"/>.
    /// </summary>
    public class DirectoryInfoItem
    {
        internal DirectoryInfo DirectoryInfoI;

        // 7/4/2010 - Folder Icons
        private static bool _folderIconsLoaded;
        private static BitmapImage _folderIconOpen;
        private static BitmapImage _folderIconClosed;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="directoryInfo">Instance of <see cref="DirectoryInfo"/></param>
        public DirectoryInfoItem(DirectoryInfo directoryInfo)
        {
            DirectoryInfoI = directoryInfo;
            DirectoryName = directoryInfo.Name;

            // 7/4/2010 - Load Folder Icons
            if (!_folderIconsLoaded)
            {
                var folderOpen = new Bitmap(Resources.folderIconOpen);
                _folderIconOpen = folderOpen.ToBitmapImage();
                var folderClosed = new Bitmap(Resources.folderIconClosed);
                _folderIconClosed = folderClosed.ToBitmapImage();

                _folderIconsLoaded = true;
            }

        }

        // 1/9/2011 - Converted from Property to Method, which allows passing in the FilterBy predicate.
        /// <summary>
        /// Returns an array of <see cref="DirectoryInfo"/>.
        /// </summary>
        /// <param name="directoryFilterBy">Lambda function as filterBy for directories.</param>
        public DirectoryInfo[] GetDirectories(Func<DirectoryInfo, bool> directoryFilterBy)
        {
            return DirectoryInfoI.GetDirectories("*", SearchOption.TopDirectoryOnly).Where(directoryFilterBy).ToArray();
        }

        /// <summary>
        /// Gets the current directory name.
        /// </summary>
        public string DirectoryName { get; private set; }

        /// <summary>
        /// Gets the folder icon Open image.
        /// </summary>
        public BitmapImage FolderIconOpen
        {
            get { return _folderIconOpen; }
        }
        /// <summary>
        /// Gets the folder icon Closed image.
        /// </summary>
        public BitmapImage FolderIconClosed
        {
            get { return _folderIconClosed; }
        }
    }
}