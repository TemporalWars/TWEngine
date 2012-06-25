using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Xna.Framework.Graphics;

namespace TWTerrainToolsWPF.DataModel
{
    // 7/7/2010
    public class AssetsImagePaths
    {
        // 7/4/2010 - Dictionary of asset icon picture paths.
        private readonly Dictionary<string, string> _assetImagePathsDictionary = new Dictionary<string, string>();

        // 2/4/2011 - Fix: Capture duplicate keyName errors; Add DirectoryFilterBy filter param.
        /// <summary>
        /// Populates the dictionary with Icon image paths, where
        /// the name is filtered by the given predicate function.
        /// </summary>
        /// <param name="directoryForIconPics"><see cref="DirectoryInfo"/> instance where Icons are located</param>
        /// <param name="filesFilterBy">Lambda function as filterBy for base children files</param>
        /// <param name="directoryFilterBy">Lambda function as filterBy for directories.</param>
        public void BuildImageList(DirectoryInfo directoryForIconPics, Func<FileInfo, bool> filesFilterBy, Func<DirectoryInfo, bool> directoryFilterBy)
        {
            try // 6/22/2010
            {
                // Add Parent Directory to collection too
                var directories = new List<DirectoryInfo> {directoryForIconPics};

                // 2/4/2011 - Get Directories using given filter.
                var subDirectories = directoryForIconPics.GetDirectories().Where(directoryFilterBy);
               
                // Add SubDirectories to directories
                directories.AddRange(subDirectories);
               
                // 2/4/2011 - Iterate directories
                foreach (var directoryInfo in directories)
                {
                    // Get all files from directory
                    var files = directoryInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly).Where(filesFilterBy);

                    // iterate the 'Files' list, and add each Bitmap to the ImageList
                    foreach (var file in files)
                    {
                        // Get filename without extension, to use as key for collection
                        var keyName = Path.GetFileNameWithoutExtension(file.FullName);

                        // 2/4/2011 - Capture Null keys
                        if (keyName == null) continue;

                        // 2/4/2011 - Fix: Capture duplicate keyName errors.
                        // 2/4/2011 - Skip duplicate key names.
                        if (_assetImagePathsDictionary.ContainsKey(keyName.ToLower())) continue;
                       
                        // store into dictionary; 7/7/2010:Updated to store 'keyName' all in lowercase.
                        _assetImagePathsDictionary.Add(keyName.ToLower(), file.FullName);

                    } // End Forfiles

                } // End ForEach Directories

                
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("BuildImageList method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }


        // 7/7/2010
        /// <summary>
        /// Tries to retrieve a given <paramref name="keyName"/>, and returns the
        /// asset's <see cref="BitmapSource"/> image for viewing.  Also, if image is
        /// a '.dds' format, will automatically convert to a <see cref="BitmapSource"/>.
        /// </summary>
        /// <param name="keyName">Key name used in dictionary</param>
        /// <param name="bitmapSource">(OUT) <see cref="BitmapSource"/> instance</param>
        /// <returns>true/false of result</returns>
        public bool GetAssetImage(string keyName, out BitmapSource bitmapSource)
        {
            string iconPath;
            bitmapSource = null;
            // 7/7/2010: Updated to put 'keyName' as lowercase.
            if (_assetImagePathsDictionary.TryGetValue(keyName.ToLower(), out iconPath))
            {
                // 7/6/2010 - Get extension from iconPath.
                var extension = Path.GetExtension(iconPath);
                
                // 3/28/2011 - DDS format NOT supported.
                if (extension == ".dds")
                {
                    MessageBox.Show(string.Format("DDS format NOT supported - Replace image {0}",keyName));
                    //bitmapSource = ConvertToBitmapSource(iconPath);)
                    return false;
                }

                bitmapSource = new BitmapImage(new Uri(iconPath));
                return true;
            }
            
            return false;
        }

        // 7/6/2010
        /// <summary>
        /// Converts a given '.dds' format to a BitmapSource.
        /// </summary>
        /// <param name="filePath">Full path location of '.dds' texture to load</param>
        /// <returns><see cref="BitmapSource"/> instance</returns>
        public static BitmapSource ConvertToBitmapSource(string filePath)
        {
            return null;

            // XNA 4.0 Updates
            // Create the graphics device
            //using (var graphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, DeviceType.NullReference, IntPtr.Zero, new PresentationParameters()))
            /*using (var graphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, pp))
            {              

                // XNA 4.0 Updates - FromFile removed in Texture2D; now use FromStream.
                using (Stream stream = File.OpenWrite(filePath))
                {
                    // Load the texture
                    //using (var texture = Texture2D.FromFile(graphicsDevice, filePath, textureCreationParameters))
                    using (var texture = Texture2D.FromStream(graphicsDevice, stream))
                    {
                        // XNA 4.0 Updates - 'Color' struct namespace changed.
                        // Get the pixel data
                        //var pixelColors = new Microsoft.Xna.Framework.Graphics.Color[texture.Width*texture.Height];
                        var pixelColors = new Microsoft.Xna.Framework.Color[texture.Width*texture.Height];
                        texture.GetData(pixelColors);

                        // Copy the pixel colors into a byte array
                        const int bytesPerPixel = 3;
                        var stride = texture.Width*bytesPerPixel;

                        var pixelData = new byte[pixelColors.Length*bytesPerPixel];
                        for (var i = 0; i < pixelColors.Length; i++)
                        {
                            pixelData[i*bytesPerPixel + 0] = pixelColors[i].R;
                            pixelData[i*bytesPerPixel + 1] = pixelColors[i].G;
                            pixelData[i*bytesPerPixel + 2] = pixelColors[i].B;
                        }
                       
                        // Create a bitmap source
                        return BitmapSource.Create(texture.Width, texture.Height, 96, 96, PixelFormats.Rgb24, null,
                                                   pixelData, stride);

                        
                    } // End Using Texture
                } // End Using Stream
            }*/
        }
    }
}
