using System;
using System.Windows.Controls;
using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.ViewModel.DirectoryViewModel;

namespace ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.Delegates
{
    // 7/8/2010
    /// <summary>
    /// Custom eventArgs for DragDrop event and delegate.
    /// </summary>
    public class DragDropEventArgs : EventArgs
    {
        /// <summary>
        /// Texture's file name.
        /// </summary>
        public string FileName;

        /// <summary>
        /// Stores the <see cref="FileInfoViewModel.TreeViewDirectoryPath"/>.
        /// </summary>
        public string TreeViewDirectoryPath;

        /// <summary>
        /// Index of <see cref="ListViewItem"/> which was just updated.
        /// </summary>
        public int Index;
    }
}