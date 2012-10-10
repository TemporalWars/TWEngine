using System.IO;

namespace ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.DataModel.DirectoryDataModel
{
    // 7/2/2010
    public class FileInfoItem
    {
        // 7/7/2010 - Store FileInfo instance.
        private readonly FileInfo _fileInfo;

        #region Properties

        // 7/7/2010
        /// <summary>
        /// Gets the current <see cref="FileInfo"/>
        /// </summary>
        public FileInfo FileInfo
        {
            get { return _fileInfo; }
        }

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="fileInfo">Instance of <see cref="FileInfo"/></param>
        public FileInfoItem(FileInfo fileInfo)
        {
            // store instance
            _fileInfo = fileInfo;
        }

        /// <summary>
        /// Gets the current file name.
        /// </summary>
        public string FileName
        {
            get
            {
                // Remove Extension from name
                var fileName = Path.GetFileNameWithoutExtension(FileInfo.FullName);
                return fileName;
            }
        }

        /// <summary>
        /// Gets the full path name for given file.
        /// </summary>
        public string FullPathName { get { return FileInfo.FullName; } }

        
    }
}