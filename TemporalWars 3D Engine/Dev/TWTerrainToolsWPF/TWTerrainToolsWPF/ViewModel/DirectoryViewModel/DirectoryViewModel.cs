using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.DataModel;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.DataModel.DirectoryDataModel;

namespace ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.ViewModel.DirectoryViewModel
{
    /// <summary>
    /// The ViewModel for the LoadOnDemand demo.  This simply
    /// exposes a read-only collection of regions.
    /// </summary>
    public class DirectoryViewModel
    {
        readonly ReadOnlyCollection<DirectoryInfoViewModel> _directories;

        // 1/9/2011 - Add Func<DirectoryInfo, bool> param.
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="directoryInfoItems">Array of <see cref="DirectoryInfoItem"/></param>
        /// <param name="filesFilterBy">Lambda function as filterBy for base children files.</param>
        /// <param name="directoryFilterBy">Lambda function as filterBy for directories.</param>
        /// <param name="assetImagePaths">Reference to the Icon paths dictionary</param>
        public DirectoryViewModel(DirectoryInfoItem[] directoryInfoItems, Func<FileInfo, bool> filesFilterBy, 
                                Func<DirectoryInfo, bool> directoryFilterBy, AssetsImagePaths assetImagePaths)
        {
            _directories = new ReadOnlyCollection<DirectoryInfoViewModel>(
                (from directoryInfo in directoryInfoItems
                 select new DirectoryInfoViewModel(directoryInfo, null, filesFilterBy, directoryFilterBy, assetImagePaths))
                .ToList());
        }

        /// <summary>
        /// Returns list of ROC <see cref="DirectoryInfoViewModel"/> collection.
        /// </summary>
        public ReadOnlyCollection<DirectoryInfoViewModel> Directories
        {
            get { return _directories; }
        }
    }
}