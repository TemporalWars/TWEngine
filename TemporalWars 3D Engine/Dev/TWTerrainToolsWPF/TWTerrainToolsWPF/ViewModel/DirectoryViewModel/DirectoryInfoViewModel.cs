using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;
using TWTerrainToolsWPF.DataModel;
using TWTerrainToolsWPF.DataModel.DirectoryDataModel;

namespace TWTerrainToolsWPF.ViewModel.DirectoryViewModel
{
    // 7/2/2010
    public class DirectoryInfoViewModel : TreeViewItemViewModel
    {

        readonly DirectoryInfoItem _directoryInfoItem;

        // 7/6/2010 - Filter used for base children selection.
        private readonly Func<FileInfo, bool> _filesFilterBy; // 1/9/2011 - Renamed from _filterBy

        // 1/11/2011 - Filter used for directory selection.
        private readonly Func<DirectoryInfo, bool> _directoryFilterBy; 

        // 7/6/2010 - Ref to the AssetsImagePaths .
        private readonly AssetsImagePaths _assetImagePaths;

        // 7/7/2010 - Store TreeView's Directory path.
        internal string TreeViewsDirectoryPath;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="directoryInfoItem">Instance of <see cref="DirectoryInfoItem"/></param>
        /// <param name="parentDirectory">Instance of parent <see cref="DirectoryInfoViewModel"/></param>
        /// <param name="filesFilterBy">Lambda function as filterBy for base children files</param>
        /// <param name="directoryFilterBy"></param>
        /// <param name="assetImagePaths">Reference to the Icon paths dictionary</param>
        public DirectoryInfoViewModel(DirectoryInfoItem directoryInfoItem, DirectoryInfoViewModel parentDirectory, 
            Func<FileInfo, bool> filesFilterBy, Func<DirectoryInfo, bool> directoryFilterBy, AssetsImagePaths assetImagePaths)
            : base(parentDirectory, true)
        {
            _directoryInfoItem = directoryInfoItem;

            // 7/6/2010 - Set FilterBy lambda func.
            _filesFilterBy = filesFilterBy;

            // 1/9/2011 - Set FilterBy lambda func.
            _directoryFilterBy = directoryFilterBy;

            // 7/6/2010 - Set reference to Icons path dictionary
            _assetImagePaths = assetImagePaths;

            // 7/7/2010 - Set TreeView's directory path; this is extended when children added.
            TreeViewsDirectoryPath = (parentDirectory != null)
                                          ? parentDirectory.TreeViewsDirectoryPath + directoryInfoItem.DirectoryName +
                                            @"\"
                                          : directoryInfoItem.DirectoryName + @"\";
        }

        /// <summary>
        /// Gets the current directory name.
        /// </summary>
        public string DirectoryName
        {
            get { return _directoryInfoItem.DirectoryName; }
        }

        // 7/4/2010
        /// <summary>
        /// Gets the current directory image.
        /// </summary>
        public BitmapImage DirectoryImage
        {
            get
            {
                return IsExpanded ? _directoryInfoItem.FolderIconOpen : _directoryInfoItem.FolderIconClosed;
            }
        }

        /// <summary>
        /// Invoked when the child items need to be loaded on demand.
        /// Subclasses can override this to populate the Children collection.
        /// </summary>
        //[OnWorkerThread]
        protected override void LoadChildren()
        {
            // Create BackgroundWorker thread.
            using (var worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true })
            {
                worker.DoWork += new DoWorkEventHandler(delegate(object sender, DoWorkEventArgs e)
                {
                    using (var workerSent = sender as BackgroundWorker)
                    {
                        // 1/9/2010 - Updated to pass _directoryFilterBy.
                        // check if existing subDirectories exist
                        if (_directoryInfoItem.GetDirectories(_directoryFilterBy).Length > 0)
                        {
                            foreach (var directoryInfo in _directoryInfoItem.GetDirectories(_directoryFilterBy))
                            {
                                var directoryInfoItem = new DirectoryInfoItem(directoryInfo);

                                // 2/4/2011 - Refactored code to new method.
                                //Children.Add(new DirectoryInfoViewModel(directoryInfoItem, this, _filesFilterBy, _directoryFilterBy, _assetImagePaths));
                                AddChildrenRecordHelper(new DirectoryInfoViewModel(directoryInfoItem, this, _filesFilterBy, _directoryFilterBy, _assetImagePaths));
                            }
                        }
                        // else, check if files exist
                        else if (_directoryInfoItem.DirectoryInfoI.GetFiles().Length > 0)
                        {
                            foreach (var fi in _directoryInfoItem.DirectoryInfoI.GetFiles())
                            {
                                // Only add files with the '.x' or '.fbx' extensions.
                                //_filterBy = fi => ((fi.Extension != ".X" && fi.Extension != ".FBX") && fi.Extension != ".x") && fi.Extension != ".fbx";
                                if (_filesFilterBy != null && !_filesFilterBy(fi)) continue;

                                var fileInfoItem = new FileInfoItem(fi);

                                var fileInfoViewModel = new FileInfoViewModel(fileInfoItem, this, _assetImagePaths);

                                //Children.Add(fileInfoViewModel);
                                AddChildrenRecordHelper(fileInfoViewModel);
                            }
                        }

                        // Returns results.
                        //e.Result = methodInterceptionArgs.ReturnValue;

                    }
                });

                // Set the ProgressChanged delegate callback
                worker.ProgressChanged += delegate(object s, ProgressChangedEventArgs args)
                {
                    // update progressbar
                    //_progressBar.Value = (double)args.UserState;
                };

                // Set the WorkCompleted delegate callback
                worker.RunWorkerCompleted += delegate(object sender1, RunWorkerCompletedEventArgs e1)
                {
                    // set bar to 100%>
                    //_progressBar.Value = _progressBar.Maximum;
                    /*if (RunWorkCompleted != null)
                        RunWorkCompleted(sender1, e1);*/

                };

                // Start work.
                worker.RunWorkerAsync();

            } // End Using Worker
            
            
        }

        // 2/4/2011
        private void AddChildrenRecordHelper(TreeViewItemViewModel treeViewItemViewModel)
        {
            // Get Window IOnGui Interface
            var onGuiInterface = ToolInstance.OnGuiThread;  

            // Call OnGuiThread Method call
            if (onGuiInterface != null)
                onGuiInterface.OnGuiThreadMethodCall(delegate
                                                         {
                                                             Children.Add(treeViewItemViewModel);
                                                         }, treeViewItemViewModel);

            
        }

       
    }
}