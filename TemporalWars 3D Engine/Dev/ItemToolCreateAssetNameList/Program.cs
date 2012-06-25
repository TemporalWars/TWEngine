using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ItemToolCreateAssetNameList
{
    /// <summary>
    /// Console tool used to recreate the ContentForResources artwork structure, but as a dummy structure where the
    /// files are empty files.  This dummy structure is then used in the ItemTools WPF window.
    /// </summary>
    class Program
    {
        private static string _dummyDirectoryLocationOutput;

        private const string DirectoriesSearchKey = @"ContentForResources";

        private static readonly List<string> ContentSearchLocations = new List<string>();

        /// <summary>
        /// Entry point into console application.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //if (args != null && args[0] != null)

            // Environment.GetEnvironmentVariable("VisualStudioDir");
            var visualStudioDir = ""; 

            // Set Output Location
            _dummyDirectoryLocationOutput = visualStudioDir + @"\\Projects\\TemporalWars 3D Engine\\Dev\\ItemToolAssetNameList\\";

            ContentSearchLocations.Add(visualStudioDir + @"\\Projects\\TemporalWars 3D Engine\\Dev\\ContentForResources\\ContentAlleyPack\\");
            ContentSearchLocations.Add(visualStudioDir + @"\\Projects\\TemporalWars 3D Engine\\Dev\\ContentForResources\\ContentDowntownDistrictPack\\");
            ContentSearchLocations.Add(visualStudioDir + @"\\Projects\\TemporalWars 3D Engine\\Dev\\ContentForResources\\ContentRTSPack\\");
            ContentSearchLocations.Add(visualStudioDir + @"\\Projects\\TemporalWars 3D Engine\\Dev\\ContentForResources\\ContentSticksNTwiggPack\\");
            ContentSearchLocations.Add(visualStudioDir + @"\\Projects\\TemporalWars 3D Engine\\Dev\\ContentForResources\\ContentUrbanPack\\");
            ContentSearchLocations.Add(visualStudioDir + @"\\Projects\\TemporalWars 3D Engine\\Dev\\ContentForResources\\ContentWarehouseDistrictPack\\");
            // Add also 'PlayableModels', used for scripting AI side.
            ContentSearchLocations.Add(visualStudioDir + @"\\Projects\\TemporalWars 3D Engine\\Dev\\ContentForResources\\ContentPlayableModels\\");

            // Iterate ContentSearchLocations to create dummy asset name list.
            foreach (var searchLocation in ContentSearchLocations)
            {
                var directoryInfo = new DirectoryInfo(searchLocation);
                CreateAssetNameList(directoryInfo,
                                    fi =>
                                    ((fi.Extension == ".X" || fi.Extension == ".FBX") || fi.Extension == ".x") ||
                                    fi.Extension == ".fbx",
                                    dir => (!dir.Name.Equals("obj") && !dir.Name.Equals("bin") && !dir.Name.Equals(".Thumbnails")));
            }

        }

        // 1/9/2011
        /// <summary>
        /// Recursive helper method used to iterate the given directory, reading each
        /// directory and subdirectory, and then recreating each file to the <see cref="_dummyDirectoryLocationOutput"/>.
        /// </summary>
        /// <param name="directoryInfo">Instance of <see cref="DirectoryInfo"/>.</param>
        /// <param name="filesFilterBy">Lambda function used to filter the files collection.</param>
        /// <param name="directoryFilterBy">Lambda function used to filter the directories collection.</param>
        private static void CreateAssetNameList(DirectoryInfo directoryInfo, 
                                                Func<FileInfo, bool> filesFilterBy, 
                                                Func<DirectoryInfo, bool> directoryFilterBy)
        {
            try
            {
                // Parse out path before '/ContentForResources/'.
                var directories = directoryInfo.FullName.Split(Convert.ToChar(@"\"));

                // Iterate each directory name in the given path.
                var indexLocation = 0;
                foreach (var directory in directories)
                {
                    if (directory.Equals(DirectoriesSearchKey)) break;
                    indexLocation++;
                }

                // Iterate collection from indexLocation to rebuild relative path.
                var relativeDirectoryName = string.Empty;
                for (var i = indexLocation; i < directories.Length; i++)
                {
                    relativeDirectoryName += @"\" + directories[i];
                }

                // Create dummy directory to mirror original directory.
                Directory.CreateDirectory(_dummyDirectoryLocationOutput + relativeDirectoryName);

                // Check for addtional directories and do recursive call.
                var subDirectories = directoryInfo.GetDirectories("*", SearchOption.TopDirectoryOnly).Where(directoryFilterBy);
                foreach (var subDirectory in subDirectories)
                {
                    CreateAssetNameList(subDirectory, filesFilterBy, directoryFilterBy);
                }

                // Iterate all files within this directory
                foreach (var fi in directoryInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly).Where(filesFilterBy))
                {
                    Console.WriteLine(fi.Name);

                    // Create dummyfile.
                    using (var sw = File.CreateText(_dummyDirectoryLocationOutput + relativeDirectoryName + @"\" + fi.Name))
                    {
                        sw.WriteLine(".");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Exception thrown = {0}", ex.Message));
            }
           
        }
    }
}
