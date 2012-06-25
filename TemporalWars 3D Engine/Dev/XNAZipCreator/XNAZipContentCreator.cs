using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace XNAZipCreator
{
    public partial class FrmXNAZipContentCreator : Form
    {
        private string _visualStudioDir;
        private string _projectDir; // location of the 'GameEngine' project
        private string _contentDir; // location of the 'ContentForResources' project

        public FrmXNAZipContentCreator()
        {
            InitializeComponent();

            // Set environmental variables
            _visualStudioDir = GetVisualStudioLocation();
            _projectDir = @"\\Projects\\TemporalWars 3D Engine\\Dev\\XNA_RTS2008\\";
            _contentDir = @"\\Projects\\TemporalWars 3D Engine\\Dev\\ContentForResources\\";

            // set on form
            txtVisualStudioEnv.Text = string.IsNullOrEmpty(GetVisualStudioLocation()) ? @"Enter VS path; Like C:\Users\Ben\Documents\Visual Studio 2008\" : _visualStudioDir;
            txtProjectDir.Text = _projectDir;

            // set the default values for the textboxes of 'PlayableItems' group.
            txtContentDir_p.Text = @"bin\$(Platform)\debug\ContentPlayableModels\*.xnb";
            txtContentOutput_p.Text = @"1ContentZipped\ContentPlayable_$(Platform).xzb";
            txtResourceFile_p.Text = @"ResourceId.cs";
            txtResourceClassName_p.Text = @"ResourceId";
            txtResourceNamespace_p.Text = @"Spacewar";

            // set the default values for the textboxes of 'ScenaryItems' group.
            txtContentDir_s.Text =
                @"bin\$(Platform)\debug\ContentAlleyPack\AlleyPack\*.xnb|bin\$(Platform)\debug\ContentDowntownDistrictPack\DowntownDistrictPack\*.xnb|" +
                @"bin\$(Platform)\debug\ContentRTSPack\*.xnb|bin\$(Platform)\debug\ContentSticksNTwiggPack\STPack\*.xnb|" +
                @"bin\$(Platform)\debug\ContentUrbanPack\Urban\*.xnb|bin\$(Platform)\debug\ContentWarehouseDistrictPack\WarehouseDistrictPack\*.xnb";
            txtContentOutput_s.Text = @"1ContentZipped\ContentScenary_$(Platform).xzb";
            txtResourceFile_s.Text = @"ResourceId.cs";
            txtResourceClassName_s.Text = @"ResourceId";
            txtResourceNamespace_s.Text = @"Spacewar";

            // set the default values for the textboxes of 'TerrainTextures' group.
            txtContentDir_t.Text = @"bin\$(Platform)\debug\ContentTextures\high512x\*.xnb";
            txtContentOutput_t.Text = @"1ContentZipped\ContentTextures_high_$(Platform).xzb";
            txtResourceFile_t.Text = @"ResourceId.cs";
            txtResourceClassName_t.Text = @"ResourceId";
            txtResourceNamespace_t.Text = @"Spacewar";
        }

        /// <summary>
        /// Creates a Zip content file for the PlayableItems.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnCreateZipFile_Click(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            // 11/1/2009
            string outputFileName;
            string constantsFileName;
            string constantNameSpace;
            string constantClassName;
            string[] directories;
            if (!GetDirectories(txtContentDir_p, txtContentOutput_p, txtResourceFile_p, txtResourceNamespace_p,
                           txtResourceClassName_p,
                           out outputFileName, out constantsFileName, out constantNameSpace, out constantClassName,
                           out directories))
                return;

            // 2/8/2010 - Disable button during ZipContent...
            btnCreateZipFile_p.Enabled = false;

            ZipContent(directories, outputFileName, constantsFileName, constantNameSpace, constantClassName, null, progressBar1);

            // 2/8/2010 - Enable button.
            btnCreateZipFile_p.Enabled = true;
        }

        /// <summary>
        /// Creates a Zip content file for the ScenaryItems.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnCreateZipFile_s_Click(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            // 11/1/2009
            string outputFileName;
            string constantsFileName;
            string constantNameSpace;
            string constantClassName;
            string[] directories;
            if (!GetDirectories(txtContentDir_s, txtContentOutput_s, txtResourceFile_s, txtResourceNamespace_s,
                           txtResourceClassName_s,
                           out outputFileName, out constantsFileName, out constantNameSpace, out constantClassName,
                           out directories))
                return;

            // 2/8/2010 - Disable button during ZipContent...
            btnCreateZipFile_s.Enabled = false;

            ZipContent(directories, outputFileName, constantsFileName, constantNameSpace, constantClassName, null, progressBar1);

            // 2/8/2010 - Enable button.
            btnCreateZipFile_s.Enabled = true;
        }

        // 11/3/2009
        /// <summary>
        /// Creates a Zip content file for the TerrainTextures.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnCreateZipFile_t_Click(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            // 11/3/2009
            string outputFileName;
            string constantsFileName;
            string constantNameSpace;
            string constantClassName;
            string[] directories;
            if (!GetDirectories(txtContentDir_t, txtContentOutput_t, txtResourceFile_t, txtResourceNamespace_t,
                           txtResourceClassName_t,
                           out outputFileName, out constantsFileName, out constantNameSpace, out constantClassName,
                           out directories))
                return;

            // 2/8/2010 - Disable button during ZipContent...
            btnCreateZipFile_t.Enabled = false;

            ZipContent(directories, outputFileName, constantsFileName, constantNameSpace, constantClassName, null, progressBar1);

            // 2/8/2010 - Enable button.
            btnCreateZipFile_t.Enabled = true;
        }

        // 2/8/2010
        /// <summary>
        /// Retrieves all directories of where the files are to create a ZipContent file.
        /// </summary>
        /// <param name="txtContentDirectory">ContentDir TextBox</param>
        /// <param name="txtContentOutput">ContentOutput TextBox</param>
        /// <param name="txtResourceFile">ResourceFile TextBox</param>
        /// <param name="txtResourceNamespace">Resource NameSpace TextBox</param>
        /// <param name="txtResourceClassName">Resource ClassName</param>
        /// <param name="outputFileName">(OUT) Zip file name</param>
        /// <param name="constantsFileName">(OUT) file name</param>
        /// <param name="constantNameSpace">(OUT) Resource namespace</param>
        /// <param name="constantClassName">(OUT) Resource class name</param>
        /// <param name="directories">(OUT) Array of Directories.</param>
        /// <returns>True/False of operation</returns>
        private bool GetDirectories(Control txtContentDirectory, Control txtContentOutput, Control txtResourceFile, Control txtResourceNamespace, Control txtResourceClassName, 
            out string outputFileName, out string constantsFileName, out string constantNameSpace, out string constantClassName, out string[] directories)
        {
            outputFileName = null;
            constantsFileName = null;
            constantNameSpace = null;
            constantClassName = null;
            directories = null;

            // 2/8/2010 - Check if any params are Null.
            if (string.IsNullOrEmpty(txtContentDirectory.Text))
            {
                LblMessages.Text = @"Enter a Content Directory...";
                errorProviderName.SetError(txtContentDirectory, "Enter a Content Directory...");
                return false;
            }
            errorProviderName.SetError(txtContentDirectory, ""); // Clear error instance.

            // 2/8/2010 - Check if any params are Null.
            if (string.IsNullOrEmpty(txtContentOutput.Text))
            {
                LblMessages.Text = @"Enter a Content Output...";
                errorProviderName.SetError(txtContentOutput, "Enter a Content Output...");
                return false;
            }
            errorProviderName.SetError(txtContentOutput, ""); // Clear error instance.

            // 2/8/2010 - Check if any params are Null.
            if (string.IsNullOrEmpty(txtResourceFile.Text))
            {
                LblMessages.Text = @"Enter a Resource FileName...";
                errorProviderName.SetError(txtResourceFile, "Enter a Resource FileName...");
                return false;
            }
            errorProviderName.SetError(txtResourceFile, ""); // Clear error instance.

            // 2/8/2010 - Check if any params are Null.
            if (string.IsNullOrEmpty(txtResourceNamespace.Text))
            {
                LblMessages.Text = @"Enter a Resource NameSpace...";
                errorProviderName.SetError(txtResourceNamespace, "Enter a Resource NameSpace...");
                return false;
            }
            errorProviderName.SetError(txtResourceNamespace, ""); // Clear error instance.

            // 2/8/2010 - Check if any params are Null.
            if (string.IsNullOrEmpty(txtResourceClassName.Text))
            {
                LblMessages.Text = @"Enter a Resource ClassName...";
                errorProviderName.SetError(txtResourceClassName, "Enter a Resource ClassName...");
                return false;
            }
            errorProviderName.SetError(txtResourceClassName, ""); // Clear error instance.

            // 2/8/2010 - Check if 'VS' is null.
            if (string.IsNullOrEmpty(_visualStudioDir))
            {
                LblMessages.Text = @"Enter the VisualStudio's directory path...";
                errorProviderName.SetError(txtVisualStudioEnv, "Entert he Visual Studio's directory path...");
                return false;
            }
            errorProviderName.SetError(txtVisualStudioEnv, ""); // Clear error instance.

            // 2/8/2010 - Check if 'ProjectDir' is null.
            if (string.IsNullOrEmpty(_projectDir))
            {
                LblMessages.Text = @"Enter the Project's directory path...";
                errorProviderName.SetError(txtProjectDir, "Enter the Project's directory path...");
                return false;
            }
            errorProviderName.SetError(txtProjectDir, ""); // Clear error instance.

            // 2/8/2010 - Check if 'ContentDir' is null.
            if (string.IsNullOrEmpty(_contentDir))
            {
                LblMessages.Text = @"Enter the Content's directory path...";
                errorProviderName.SetError(txtContentDirectory, "Enter the Content's directory path...");
                return false;
            }
            errorProviderName.SetError(txtContentDirectory, ""); // Clear error instance.

            var count = 0;
            directories = new string[txtContentDirectory.Text.Split('|').Length];
            foreach (var split in txtContentDirectory.Text.Split('|'))
            {
                // set proper 'Platform' choosen by user; either 'x86' or 'Xbox 360'.
                var updatedSplit = split.Replace("$(Platform)", cmbPlatform.Text);

                directories[count++] = _visualStudioDir + _contentDir + updatedSplit;
            }

            outputFileName = _visualStudioDir + _projectDir + txtContentOutput.Text.Replace("$(Platform)", cmbPlatform.Text);
            constantsFileName = _visualStudioDir + _projectDir + txtResourceFile.Text;
            constantNameSpace = txtResourceNamespace.Text;
            constantClassName = txtResourceClassName.Text;

            return true;
        }


        private struct ContentNode : IComparable<ContentNode>
        {
            public readonly string Name;
            public readonly long Offset;

            public ContentNode(string name, long offset)
            {
                Name = name;
                Offset = offset;
            }

            public int CompareTo(ContentNode node)
            {
                return StringComparer.OrdinalIgnoreCase.Compare(Name, node.Name);
            }
        }

        private static string FieldEncode(string text)
        {
            return Regex.Replace(text, "[^a-zA-Z0-9_\\\\]", string.Empty);
        }

        private static string IntellisenseEncode(string text)
        {
            return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
        }

        /// <summary>
        /// Gets a 16 bit hash code for text
        /// </summary>
        /// <param name="text">Text</param>
        /// <returns>Hash code</returns>
        private static ushort GetHashCode(string text)
        {
            var hashCode = 5381;
            text = text.Normalize().Trim();
            var length = text.Length;
            for (var i = 0; i < length; i++)
            {
                var c = text[i];
                if (char.IsLower(c))
                {
                    c = char.ToUpperInvariant(c);
                }
                hashCode = ((hashCode << 5) + hashCode) + c;
            }
            return (ushort)(hashCode & 0x0000FFFF);
        }

        /// <summary>
        /// Zips all content in a directory into a file (including sub directories)
        /// </summary>
        /// <param name="directoryMasks">Directoriy masks (i.e. c:\files\*.xnb)</param>
        /// <param name="outputFileName">Output zip file (pass this file name into the constructor)</param>
        /// <param name="constantsFileName">File name for constants.cs file - each resource may have a matching .description.txt file name
        ///   containing a description for the resource, these descriptions will be put in the .xml documentation for the constant.</param>
        /// <param name="constantsNamespace">Namespace for constants file</param>
        /// <param name="constantsClassName">Class name for constants file</param>
        /// <param name="xactProjectFileName">File name for an Xact project (can be null or empty for none)</param>
        /// <param name="progressBar"></param>
        /// <returns>Key value pair, key is the number of assets zipped, value is the size of the zip file</returns>
        public void ZipContent(string[] directoryMasks, string outputFileName, string constantsFileName, string constantsNamespace, string constantsClassName, string xactProjectFileName, ProgressBar progressBar)
        {
            // 2/8/2010 - Add Try-Catch error handling
            try
            {
                // 11/1/2009 - Set Progress bar
                LblMessages.Text = @"Zip Process Started...";
                progressBar.Maximum = directoryMasks.Length;
                progressBar.Value = 0;
                progressBar.Step = 1;

                const int intSize = 4;

                File.Delete(outputFileName);
                var count = 0;
                var b = new StringBuilder();
                List<ContentNode> subList;
                var hashArray = new List<ContentNode>[ushort.MaxValue];
                using (var zipStream = File.Create(outputFileName))
                {
                    var binWriter = new BinaryWriter(zipStream, Encoding.UTF8);
                    using (var writer = new StringWriter(b))
                    {
                        writer.WriteLine("// *** Auto-generated from XNAZipContentCreator form *** //");
                        writer.WriteLine();
                        writer.WriteLine("#region Imports");
                        writer.WriteLine();
                        writer.WriteLine("using System;");
                        writer.WriteLine();
                        writer.WriteLine("#endregion Imports");
                        writer.WriteLine();
                        writer.WriteLine("namespace {0}", constantsNamespace);
                        writer.WriteLine("{");
                        writer.WriteLine("\t/// <summary>");
                        writer.WriteLine("\t/// Constants for resources in file {0}", Path.GetFileName(outputFileName));
                        writer.WriteLine("\t/// </summary>");
                        writer.WriteLine("\tpublic class {0}", constantsClassName);
                        writer.WriteLine("\t{");
                        foreach (var directoryMask in directoryMasks)
                        {
                            var directory = Path.GetDirectoryName(directoryMask);
                            var extension = Path.GetFileName(directoryMask);
                            var info = new DirectoryInfo(directory);
                            var files = info.GetFiles(extension, SearchOption.AllDirectories);
                            foreach (var file in files)
                            {
                                // .description files and .zip files can never be added to the archive
                                if (file.FullName.IndexOf(".description", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    file.Extension.Equals(".zip", StringComparison.OrdinalIgnoreCase))
                                {
                                    continue;
                                }
                                var bytes = File.ReadAllBytes(file.FullName);
                                var subFileName = Path.GetFileNameWithoutExtension(file.Name).Replace(".", string.Empty);

                                // DEBUG
                                //if (subFileName.Equals("leaf002_NRM_0")) System.Diagnostics.Debugger.Break();

                                var variableName = FieldEncode(subFileName);
                                var descriptionFileName = file.FullName + ".description.txt";
                                writer.WriteLine("\t\t/// <summary>");
                                if (File.Exists(descriptionFileName))
                                {
                                    writer.Write("\t\t/// ");
                                    writer.WriteLine(IntellisenseEncode(File.ReadAllText(descriptionFileName)) + "<br/>");
                                }
                                writer.WriteLine("\t\t/// [Original file = &apos;{0}&apos;]",
                                                 file.FullName.Substring(info.FullName.Length));
                                writer.WriteLine("\t\t/// </summary>");
                                writer.WriteLine("\t\tpublic const string {0} = \"{1}\";", variableName, subFileName);
                                var hash = GetHashCode(subFileName);
                                if (hashArray[hash] == null)
                                {
                                    hashArray[hash] =
                                        new List<ContentNode>(new[] { new ContentNode(subFileName, zipStream.Length) });
                                }
                                else
                                {
                                    hashArray[hash].Add(new ContentNode(subFileName, zipStream.Length));
                                }
                                writer.WriteLine();
                                binWriter.Write(bytes.Length);
                                using (var stream = new DeflateStream(zipStream, CompressionMode.Compress, true))
                                {
                                    stream.Write(bytes, 0, bytes.Length);
                                }
                                count++;
                            }

                            // 11/1/2009 - Update ProgressBar
                            progressBar.PerformStep();
                            progressBar.Update();
                        }

                        writer.WriteLine("\t}");
                        writer.WriteLine("}");
                    }

                    var indexPos = zipStream.Length;
                    zipStream.SetLength(zipStream.Length + (ushort.MaxValue * intSize) + intSize);
                    zipStream.Position = zipStream.Length;
                    binWriter.Write(count);
                    var hashOffset = zipStream.Length;
                    var length = hashArray.Length; // 5/26/2010
                    for (var i = 0; i < length; i++)
                    {
                        zipStream.Position = indexPos + (i * intSize);
                        if ((subList = hashArray[i]) == null)
                        {
                            binWriter.Write(int.MinValue);
                            continue;
                        }
                        binWriter.Write((int)(zipStream.Length - indexPos));
                        if (subList.Count > byte.MaxValue)
                        {
                            throw new ApplicationException("Too many items in sub list");
                        }
                        subList.Sort();
                        zipStream.Position = hashOffset;
                        binWriter.Write((byte)subList.Count);
                        foreach (var node in subList)
                        {
                            binWriter.Write(node.Name);
                            binWriter.Write(node.Offset);
                        }
                        hashOffset = zipStream.Position;
                    }
                    zipStream.Position = zipStream.Length;
                    binWriter.Write(indexPos);
                    if (!File.Exists(constantsFileName) ||
                        b.ToString() != File.ReadAllText(constantsFileName, Encoding.UTF8))
                    {
                        File.WriteAllText(constantsFileName, b.ToString(), Encoding.UTF8);
                    }
                }

                // 11/1/2009 - Update message 'Done'.
                LblMessages.Text = @"Zip Operation Completed.";

                //return new KeyValuePair<int, long>(count, size);
            }
            catch (ApplicationException err)
            {
                LblMessages.Text = @"ZipContent error. (" + err.Message + @")";
            }
            catch (DirectoryNotFoundException err)
            {
                LblMessages.Text = @"Directory was not found.  Check your output and input directory paths. (" +
                                   err.Message + @")";
            }
            // 3/23/2010 - Capture error which occurs with locked files.
            catch (UnauthorizedAccessException err)
            {
                LblMessages.Text = @"Locked files detected.  Unlock the output files, and try again. (" +
                                  err.Message + @")";
            }

        }

        // 2/3/2010
        /// <summary>
        /// Updates to the internal _visualStudioDir variable.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void txtVisualStudioEnv_TextChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            // 2/8/2010 - Check if null
            if (string.IsNullOrEmpty(txtVisualStudioEnv.Text))
            {
                errorProviderName.SetError(txtVisualStudioEnv, "Name cannot be NULL!");
                return;
            }
            errorProviderName.SetError(txtVisualStudioEnv, ""); // Clear out error

            _visualStudioDir = txtVisualStudioEnv.Text;
        }

        // 2/8/2010
        /// <summary>
        /// Updates to the internal _projectDir variable.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void txtProjectDir_TextChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            // 2/8/2010 - Check if null
            if (string.IsNullOrEmpty(txtProjectDir.Text))
            {
                errorProviderName.SetError(txtProjectDir, "Name cannot be NULL!");
                return;
            }
            errorProviderName.SetError(txtProjectDir, ""); // Clear out error

            _projectDir = txtProjectDir.Text;
        }

        // 2/8/2010
        /// <summary>
        ///  Updates to the internal _contentDir variable.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void txtContentDir_TextChanged(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            // 2/8/2010 - Check if null
            if (string.IsNullOrEmpty(txtContentDir.Text))
            {
                errorProviderName.SetError(txtContentDir, "Name cannot be NULL!");
                return;
            }
            errorProviderName.SetError(txtContentDir, ""); // Clear out error


            _contentDir = txtContentDir.Text;
        }
        
        /// <summary>
        /// Using the Environment variable for Visual Studio, returns
        /// the current location for the project folders.
        /// </summary>
        /// <returns>Visual Studio's directory file path</returns>
        protected static string GetVisualStudioLocation()
        {
            try
            {
                // 1st - get path to Visual Studio DTE
                /*var dte = (EnvDTE.DTE)System.Runtime.InteropServices.Marshal.GetActiveObject("VisualStudio.DTE");

                // 2nd - get folder path to current 'Solution', which should be this project file.
                var folderPath = Path.GetDirectoryName(dte.Solution.FullName);*/

                return Environment.GetEnvironmentVariable("VisualStudioDir");
            }
            catch (Exception)
            {
                Debug.WriteLine("Unable to set VSProjLoc variable - ", "Warning");
                return string.Empty;
            }

        }

        // 1/18/2011
        /// <summary>
        /// Eventhandler which retrieves the VS directory from the Environment variable.
        /// </summary>
        private void btnGetVSDirectory_Click(object sender, EventArgs e)
        {
            _visualStudioDir = GetVisualStudioLocation();
            txtVisualStudioEnv.Text = _visualStudioDir;

        }

        private static string _dummyDirectoryLocationOutput;
        private const string DirectoriesSearchKey = @"ContentForResources";
        private static readonly List<string> ContentSearchLocations = new List<string>();

        // 4/1/2011
        private void btnCreateItemToolAssetNameList_Click(object sender, EventArgs e)
        {
            try
            {
                // Set Output Location
                _dummyDirectoryLocationOutput = _visualStudioDir + @"\\Projects\\TemporalWars 3D Engine\\Dev\\ItemToolAssetNameList\\";

                ContentSearchLocations.Add(_visualStudioDir + @"\\Projects\\TemporalWars 3D Engine\\Dev\\ContentForResources\\ContentAlleyPack\\");
                ContentSearchLocations.Add(_visualStudioDir + @"\\Projects\\TemporalWars 3D Engine\\Dev\\ContentForResources\\ContentDowntownDistrictPack\\");
                ContentSearchLocations.Add(_visualStudioDir + @"\\Projects\\TemporalWars 3D Engine\\Dev\\ContentForResources\\ContentRTSPack\\");
                ContentSearchLocations.Add(_visualStudioDir + @"\\Projects\\TemporalWars 3D Engine\\Dev\\ContentForResources\\ContentSticksNTwiggPack\\");
                ContentSearchLocations.Add(_visualStudioDir + @"\\Projects\\TemporalWars 3D Engine\\Dev\\ContentForResources\\ContentUrbanPack\\");
                ContentSearchLocations.Add(_visualStudioDir + @"\\Projects\\TemporalWars 3D Engine\\Dev\\ContentForResources\\ContentWarehouseDistrictPack\\");
                // Add also 'PlayableModels', used for scripting AI side.
                ContentSearchLocations.Add(_visualStudioDir + @"\\Projects\\TemporalWars 3D Engine\\Dev\\ContentForResources\\ContentPlayableModels\\");

                // Setup progressBar
                progressBar1.Minimum = 0;
                progressBar1.Maximum = ContentSearchLocations.Count;

                // Iterate ContentSearchLocations to create dummy asset name list.
                foreach (var searchLocation in ContentSearchLocations)
                {

                    var directoryInfo = new DirectoryInfo(searchLocation);
                    CreateAssetNameList(directoryInfo,
                                        fi =>
                                        ((fi.Extension == ".X" || fi.Extension == ".FBX") || fi.Extension == ".x") ||
                                        fi.Extension == ".fbx",
                                        dir => (!dir.Name.Equals("obj") && !dir.Name.Equals("bin") && !dir.Name.Equals(".Thumbnails")));

                    // 11/1/2009 - Update ProgressBar
                    progressBar1.PerformStep();
                    progressBar1.Update();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Exception thrown = {0}", ex.Message));
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
        
    }
}
