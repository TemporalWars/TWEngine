#region File Description
//-----------------------------------------------------------------------------
// Storage.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.Utilities.Enums;
using ImageNexus.BenScharbach.TWEngine.Utilities.Structs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
using System.Threading;

namespace ImageNexus.BenScharbach.TWEngine.Utilities
{
    ///<summary>
    /// The <see cref="Storage"/> class is used to load and save game data, like <see cref="SaveTerrainData"/> or <see cref="SelectablesDataProperties"/>.
    ///</summary>
    public class Storage
    {
        private readonly string _containerPath = string.Empty;
        private StorageContainer _container;
        private IAsyncResult _result;
        private bool _gameSaveRequested;

#if !XBOX360
       
        // 2/1/2010 - Updated to use VisualStudio path.
        /// <summary>
        /// Starts a bits collection Save operation, to save an array of floats.  Currently used to save 
        /// the HeightMaps data.
        /// </summary>
        /// <param name="data">Collection of floats</param>
        /// <param name="fileName">File name to save data as</param>
        /// <param name="subDirPath">(Optional) Sub directory location to save in.</param>
        /// <param name="errorCode">Returns an ErrorCode number; 1 = UnauthorizedAccess/Lock files, 2 = Directory not found, 3 = Unknown.</param>
        /// <returns>True/False of success.</returns>
        public bool StartBitsSaveOperation(float[] data, string fileName, string subDirPath, out int errorCode)
        {
            // 4/9/2010
            errorCode = 0;

            try
            {
                // 2/1/2010 - Set environmental variables
                var visualStudioDir = TemporalWars3DEngine.VisualStudioProjectLocation;
               
                // Check if 'SubDirPath' Exist, and create if not.
                var subPath = visualStudioDir;
                if (!string.IsNullOrEmpty(subDirPath))
                {
                    subPath = Path.Combine(visualStudioDir, subDirPath);

                    if (!Directory.Exists(subPath))
                        Directory.CreateDirectory(subPath);
                }             

                // Create FilePath
                var filePath = Path.Combine(subPath, fileName);

                // Delete file if an old version exist
                if (File.Exists(filePath))
                    File.Delete(filePath);

                using (var binWriter = new BinaryWriter(File.Open(filePath, FileMode.Create)))
                {
                    binWriter.Write(data.Length);

                    foreach (var t in data)
                    {
                        binWriter.Write(t);
                    }
                }

                return true;

            }
            // 4/9/2010 - Capture error which occurs with locked files.
            catch (UnauthorizedAccessException)
            {
                errorCode = 1;
                return false;
            }
            // 4/9/2010 - Capture unknown directory error.
            catch (DirectoryNotFoundException)
            {
                errorCode = 2;
                return false;
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Method Error: Storage classes 'StartBitsSaveOperation' threw an error.");
                errorCode = 3;
                return false;
            }

        }

        // 2/1/2010 - Updated to use VisualStudio path.
        /// <summary>
        /// Starts a bits collection Save operation, to save an array of Color.  Currently used to save 
        /// the HeightMaps data.
        /// </summary>
        /// <param name="data">Collection of floats</param>
        /// <param name="fileName">File name to save data as</param>
        /// <param name="subDirPath">(Optional) Sub directory location to save in.</param>
        /// <param name="errorCode">Returns an ErrorCode number; 1 = UnauthorizedAccess/Lock files, 2 = Directory not found, 3 = Unknown.</param>
        /// <returns>True/False of success.</returns>
        public bool StartBitsSaveOperation(Color[] data, string fileName, string subDirPath, out int errorCode)
        {
            // 4/9/2010
            errorCode = 0;

            try
            {
                // 2/1/2010 - Set environmental variables
                var visualStudioDir = TemporalWars3DEngine.VisualStudioProjectLocation;

                // Check if 'SubDirPath' Exist, and create if not.
                var subPath = visualStudioDir;
                if (!string.IsNullOrEmpty(subDirPath))
                {
                    subPath = Path.Combine(visualStudioDir, subDirPath);

                    if (!Directory.Exists(subPath))
                        Directory.CreateDirectory(subPath);
                }

                // Create FilePath
                var filePath = Path.Combine(subPath, fileName);

                // Delete file if an old version exist
                if (File.Exists(filePath))
                    File.Delete(filePath);

                using (var binWriter = new BinaryWriter(File.Open(filePath, FileMode.Create)))
                {
                    binWriter.Write(data.Length);

                    foreach (var t in data)
                    {
                        binWriter.Write(t.A);
                        binWriter.Write(t.B);
                        binWriter.Write(t.G);
                        binWriter.Write(t.R);
                    }
                }

                return true;

            }
            // 4/9/2010 - Capture error which occurs with locked files.
            catch (UnauthorizedAccessException)
            {
                errorCode = 1;
                return false;
            }
            // 4/9/2010 - Capture unknown directory error.
            catch (DirectoryNotFoundException)
            {
                errorCode = 2;
                return false;
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Method Error: Storage classes 'StartBitsSaveOperation' threw an error.");
                errorCode = 3;
                return false;
            }

        }
#endif

        // 
        ///<summary>
        /// Starts a bits collection Load operation, to load an array of floats.
        ///</summary>
        ///<param name="data">Float[] collection to store data in</param>
        ///<param name="fileName">File name to load</param>
        ///<param name="subDirPath">(Optional) Sub directory within 'ContentMaps' to load file from.</param>
        ///<param name="location"><see cref="StorageLocation"/> Enum storage location</param>
        ///<returns>True/False of success</returns>
        public bool StartBitsLoadOperation(ref float[] data, string fileName, string subDirPath, StorageLocation location)
        {
            try
            {
                // 8/1/2008 - Updated to check Location Type.
                switch (location)
                {
                    case StorageLocation.TitleStorage:
                        //_containerPath = StorageContainer.TitleLocation;
                        break;
                    case StorageLocation.UserStorage:
                        //_containerPath = _container.Path;
                        break;
                    default:
                        break;
                }

                // 5/1/2009: Updated to String.IsNullOrEmpty per FXCop.
                // 8/12/2008 - Combine SubDirPath to ContainerPath, if necessary.
                var subPath = _containerPath;
                if (!string.IsNullOrEmpty(subDirPath))
                {
                    subPath = Path.Combine(_containerPath, subDirPath);  // TODO: XNA 4.0
                }

                // Create FilePath
                var filePath = Path.Combine(subPath, fileName);

                if (File.Exists(filePath))
                {
                    using (var binReader = new BinaryReader(File.Open(filePath, FileMode.Open, FileAccess.Read)))
                    {
                        // If the file is not empty
                        if (binReader.PeekChar() != -1)
                        {
                            var arrayLength = binReader.ReadInt32();

                            for (var i = 0; i < arrayLength; i++)
                            {
                                data[i] = binReader.ReadSingle();
                            }

                        }
                    }


                    return true;
                }   

                return false;
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Method Error: Storage classes 'StartBitsLoadOperation' threw an error.");
                return false;
            }

        }

        ///<summary>
        /// Starts a bits collection Load operation, to load an array of Color.
        ///</summary>
        ///<param name="data">Color[] collection to store data in</param>
        ///<param name="fileName">File name to load</param>
        ///<param name="subDirPath">(Optional) Sub directory within 'ContentMaps' to load file from.</param>
        ///<returns>True/False of success</returns>
        public bool StartBitsLoadOperation(string fileName, string subDirPath, out Color[] data)
        {
            try
            {
                data = default(Color[]);

                // 5/1/2009: Updated to String.IsNullOrEmpty per FXCop.
                // 8/12/2008 - Combine SubDirPath to ContainerPath, if necessary.
                var subPath = _containerPath;
                if (!string.IsNullOrEmpty(subDirPath))
                {
                    subPath = Path.Combine(_containerPath, subDirPath);  // TODO: XNA 4.0
                }

                // Create FilePath
                var filePath = Path.Combine(subPath, fileName);

                if (File.Exists(filePath))
                {
                    using (var binReader = new BinaryReader(File.Open(filePath, FileMode.Open, FileAccess.Read)))
                    {
                        // If the file is not empty
                        if (binReader.PeekChar() != -1)
                        {
                            var arrayLength = binReader.ReadInt32();

                            data = new Color[arrayLength];

                            for (var i = 0; i < arrayLength; i++)
                            {
                                data[i].A = binReader.ReadByte();
                                data[i].B = binReader.ReadByte();
                                data[i].G = binReader.ReadByte();
                                data[i].R = binReader.ReadByte();
                            }

                        }
                    }


                    return true;
                }

                return false;
            }
            catch
            {
                data = default(Color[]);
                System.Diagnostics.Debug.WriteLine("Method Error: Storage classes 'StartBitsLoadOperation' threw an error.");
                return false;
            }

        }

#if !XBOX360
        // XNA 4.0 Updates; textures can ONLY be saved in png and jpg now!
        // 8/20/2008 - Updated to save the Texture directly, rather than the Color Bits.  
        // 1/21/2009 - Overload version for Texture3D.
        /// <summary>
        /// Saves the given <see cref="Texture"/>, using the given <see cref="ImageFileFormat"/> Enum.
        /// </summary>
        /// <param name="texture">Texture to save</param>
        /// <param name="fileName">File name to save texture as</param>
        /// <param name="subDirPath">(Optional) Sub directory location to save in.</param>
        /// <param name="imageFileFormat"><see cref="ImageFileFormat"/> Enum to save as; for example 'jpg'.</param>
        /// <param name="errorCode">Returns an ErrorCode number; 1 = UnauthorizedAccess/Lock files, 2 = Directory not found, 3 = Unknown.</param>
        /// <returns>True/False of success.</returns>
        public bool StartTextureSaveOperation<TType>(TType texture, string fileName, string subDirPath, ImageFileFormat imageFileFormat, out int errorCode) where TType : Texture2D
        {
            // 4/9/2010
            errorCode = 0;

            try
            {
                // 2/1/2010 - Set environmental variables
                var visualStudioDir = TemporalWars3DEngine.VisualStudioProjectLocation;
               
                // Check if 'SubDirPath' Exist, and create if not.
                var subPath = visualStudioDir;
                if (!string.IsNullOrEmpty(subDirPath))
                {
                    subPath = Path.Combine(visualStudioDir, subDirPath);

                    if (!Directory.Exists(subPath))
                        Directory.CreateDirectory(subPath);
                }

                // Create FilePath
                var filePath = Path.Combine(subPath, fileName);

                // XNA 4.0 Updates - Update save routine.
                //texture.Save(filePath, imageFileFormat);
                switch (imageFileFormat)
                {
                    // 9/23/2010 - TODO: How save Texture3d?
                    case ImageFileFormat.Png:
                        using (Stream stream = File.OpenWrite(filePath))
                        {
                            texture.SaveAsPng(stream, texture.Width, texture.Height);

                        }
                        break;
                    case ImageFileFormat.Jpeg:
                        using (Stream stream = File.OpenWrite(filePath))
                        {
                            texture.SaveAsJpeg(stream, texture.Width, texture.Height);
                        }
                        break;
                    default:
                        break;
                }
                return true;

            }
            // 4/9/2010 - Capture error which occurs with locked files.
            catch (UnauthorizedAccessException)
            {
                errorCode = 1;
                return false;
            }
            // 4/9/2010 - Capture unknown directory error.
            catch (DirectoryNotFoundException)
            {
                errorCode = 2;
                return false;
            }
            catch (Exception)
            {
                errorCode = 3;
                System.Diagnostics.Debug.WriteLine("(StartTextureSaveOperation) threw the 'InvalidOpExp' error.");
                return false;
            }

        }
#endif
       

#if !XBOX360
        // 4/7/2010 - Updated to now return an ErrorCode, via the Out param, to allow caller to decide to Throw an error,
        //            or simply show some error message on the tool forms.
        // 2/1/2010 - Updated to save directly to the 'ContentMaps' folder.
        /// <summary>
        /// Saves the current serializable data structure, to the visual studio's
        /// 'ContentMaps' folder location.
        /// </summary>
        /// <typeparam name="TStorageStruct">Generic Serializable data structure Type</typeparam>
        /// <param name="data">Serializable data structure</param>
        /// <param name="fileName">File name</param>
        /// <param name="subDirPath">(Optional) Sub directory within 'ContentMaps' to save file to.</param>
        /// <param name="errorCode">Returns an ErrorCode number; 1 = UnauthorizedAccess/Lock files, 2 = Directory not found, 3 = Unknown.</param>
        /// <returns>True/False of success.</returns>
        public bool StartSaveOperation<TStorageStruct>(TStorageStruct data, string fileName, string subDirPath, out int errorCode)
        {
            // 4/7/2010 - Set errorCode to zero.
            errorCode = 0;

            try
            {
                DoSaveStructData(data, fileName, subDirPath);
                return true;
            }
            // 4/7/2010 - Capture error which occurs with locked files.
            catch (UnauthorizedAccessException)
            {
                errorCode = 1;
                return false;
            }
            // 4/7/2010 - Capture unknown directory error.
            catch (DirectoryNotFoundException)
            {
                errorCode = 2;
                return false;
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("(StartSaveOperation) threw the 'InvalidOpExp' error.");
                errorCode = 3;
                return false;
            }
            
        }

        // 10/29/2008 - Specifically for use with the ScenaryItemData.
        /// <summary>
        /// Saves the current serializable data structure, to the visual studio's
        /// 'ContentMaps' folder location.
        /// </summary>
        /// <param name="data">
        /// <see cref="SaveTerrainScenaryData"/> serializable data structure
        /// </param>
        /// <param name="fileName">File name</param>
        /// <param name="subDirPath">
        /// (Optional) Sub directory within 'ContentMaps' to save file to.
        /// </param>
        /// <param name="errorCode">Returns an ErrorCode number; 1 = UnauthorizedAccess/Lock files, 2 = Directory not found, 3 = Unknown.</param>
        /// <returns>True/False of success.</returns>
        public bool StartSave_ScenaryItemOperation(SaveTerrainScenaryData data, string fileName, string subDirPath, out int errorCode)
        {
            // 4/8/2010 - Set errorCode to zero.
            errorCode = 0;

            try
            {
                DoSaveScenaryItemData(data, fileName, subDirPath);
                return true;
            }
            // 4/8/2010 - Capture error which occurs with locked files.
            catch (UnauthorizedAccessException)
            {
                errorCode = 1;
                return false;
            }
            // 4/8/2010 - Capture unknown directory error.
            catch (DirectoryNotFoundException)
            {
                errorCode = 2;
                return false;
            }
            catch
            {
                errorCode = 3;
                System.Diagnostics.Debug.WriteLine("(StartSave_ScenaryItemOperation) threw the 'InvalidOpExp' error.");
                return false;
            }

        }
#endif

        // 3/21/2009: Updated to only call GUIDE for UserStorage location.
        // Starts by first getting Storage Device
        ///<summary>
        /// Starts the given Load operation, by getting the GUIDE for the proper user-storage
        /// location.
        ///</summary>
        ///<param name="data">(T) Data struct to hold loaded data</param>
        ///<param name="fileName">File name to load</param>
        ///<param name="subDirPath">Sub directory location of file, relative to project location.</param>
        ///<param name="location"><see cref="StorageLocation"/> Enum storage location</param>
        ///<typeparam name="TStorageStruct">Generic Struct type</typeparam>
        ///<returns>True/False of result</returns>
        public bool StartLoadOperation<TStorageStruct>(out TStorageStruct data, string fileName, 
                                                    string subDirPath, StorageLocation location)
        {
            try
            {
                data = default(TStorageStruct);

                // 3/21/2009 - Only call GUIDE if UserLocation
                switch (location)
                {
                    case StorageLocation.TitleStorage:
                        /*
                                                data = default(TStorageStruct);
                        */
                        return DoLoadStructData(null, fileName, subDirPath, location, out data);
                    case StorageLocation.UserStorage:
                        // 8/19/2008 - The 'GamerServiceComponent' was removed, since it does not come
                        //             with the XNA 2.0 Runtime Files when installed on another computer.
                        //             However, learned you still can call the 'Guide' methods below without
                        //             the 'GamerServiceComponent' being added, except for the 'IsVisible' call, 
                        //             which was removed and not needed.

                        if ((!Guide.IsVisible) && (_gameSaveRequested == false))
                        //if ((GameSaveRequested == false))
                        {
                            _gameSaveRequested = true;

                            // XNA 4.0 Updates
                            // Note: http://blogs.msdn.com/b/nicgrave/archive/2010/07/23/storage-in-xna-game-studio-4-0.aspx
                            //_result = Guide.BeginShowStorageDeviceSelector(null, null);
                            _result = StorageDevice.BeginShowSelector(null, null);

                        }

                        // If a Load is pending, Load as soon as the
                        // storage Device is chosen
                        if ((_gameSaveRequested))
                        {
                            // 9.6.2008 - Wait until StorageRoutines is ready for XBOX
                            while (!_result.IsCompleted)
                            {
                                Thread.Sleep(5);
                                TemporalWars3DEngine.GameInstance.Tick();
                            }

                            // 9XNA 4.0 Updates
                            // Note: http://blogs.msdn.com/b/nicgrave/archive/2010/07/23/storage-in-xna-game-studio-4-0.aspx
                            //var device = Guide.EndShowStorageDeviceSelector(_result);
                            var device = StorageDevice.EndShowSelector(_result);

                            if (device.IsConnected)
                            {
                                return DoLoadStructData(device, fileName, subDirPath, location, out data);
                            }
                            // Reset the request flag
                            _gameSaveRequested = false;
                        }

                        return false;
                }

                return false; 
               
            }
            catch
            {
                data = default(TStorageStruct);
                System.Diagnostics.Debug.WriteLine("Method Error: Storage classes 'StartLoadOperation' threw an error.");
                return false;
            }

        }

#if !XBOX360
        // Using Given Storage Stuct, save the data as an XML Stream.
        /// <summary>
        /// Saves the current serializable data structure, to the visual studio's
        /// 'ContentMaps' folder location.
        /// </summary>
        /// <typeparam name="TStorageStruct">Generic Serializable data structure Type</typeparam>
        /// <param name="data">Serializable data structure</param>
        /// <param name="fileName">File Name</param>
        /// <param name="subDirPath">(Optional) Sub directory within 'ContentMaps' to save file to.</param>
        private static void DoSaveStructData<TStorageStruct>(TStorageStruct data, string fileName, string subDirPath)
        {
            // 4/8/2010 - NOTE: Try-Catch not necessary here, since caller already has one.
            
            // 2/1/2010 - Set environmental variables
            var visualStudioDir = TemporalWars3DEngine.VisualStudioProjectLocation;
           
            // Check if 'SubDirPath' Exist, and create if not.
            var subPath = visualStudioDir;
            if (!string.IsNullOrEmpty(subDirPath))
            {
                subPath = Path.Combine(visualStudioDir, subDirPath);

                if (!Directory.Exists(subPath))
                    Directory.CreateDirectory(subPath);
            }

            // Get the Full Path for the save game file.
            var filePath = Path.Combine(subPath, fileName);

            // Delete file if an old version exist
            if (File.Exists(filePath))
                File.Delete(filePath);

            // Open the file, creating it if necessary.
            using (var stream = File.Open(filePath, FileMode.CreateNew))
            {
                // Convert the object to XML data and put it in the stream.
                var serializer = new XmlSerializer(typeof(TStorageStruct));
                serializer.Serialize(stream, data);
            }
            
        }
     

        // 10/29/2008
        // To speed up loading of data on XBOX, this method saves the ScenaryItem Data manually using the
        // BinaryWriter class, rather than the XMLSerializer.
        /// <summary>
        /// Saves the current serializable data structure, to the visual studio's
        /// 'ContentMaps' folder location.
        /// </summary>
        /// <param name="data"><see cref="SaveTerrainScenaryData"/> serializable data structure</param>
        /// <param name="fileName">File name</param>
        /// <param name="subDirPath">(Optional) Sub directory within 'ContentMaps' to save file to.</param>
        private static void DoSaveScenaryItemData(SaveTerrainScenaryData data, string fileName, string subDirPath)
        {
            // 4/8/2010 - NOTE: Try-Catch not necessary here, since caller already has one.

            // 2/1/2010 - Set environmental variables
            var visualStudioDir = TemporalWars3DEngine.VisualStudioProjectLocation;
           
            // Check if 'SubDirPath' Exist, and create if not.
            var subPath = visualStudioDir;
            if (!string.IsNullOrEmpty(subDirPath))
            {
                subPath = Path.Combine(visualStudioDir, subDirPath);

                if (!Directory.Exists(subPath))
                    Directory.CreateDirectory(subPath);
            }

            // Get the Full Path for the save game file.
            var filePath = Path.Combine(subPath, fileName);

            // Delete file if an old version exist
            if (File.Exists(filePath))
                File.Delete(filePath);

            //**********************
            // NOTE: (5/31/2012) Anytime a change is made to the saved data structure, the version
            //                   attribute must be bumped up, and a new ScenaryDataReader is required.
            //                   The version number is a constant, which is set in the TerrainStorageRoutines classes
            //                   'SaveTerrainScenaryItems' method call.
            //**********************

            var count = data.itemTypes.Count; // 5/20/2010
            using (var binWriter = new BinaryWriter(File.Open(filePath, FileMode.CreateNew)))               
            {
                // Write Count of List of ItemTypes
                binWriter.Write(count);
                for (var i = 0; i < count; i++)
                {
                    // Write ItemType
                    binWriter.Write((int)data.itemTypes[i]);
                    // Write Quat Rotation
                    var scenaryDataProperties = data.itemProperties[i]; // 5/20/2010 - Cache
                    binWriter.Write(scenaryDataProperties.rotation.W);
                    binWriter.Write(scenaryDataProperties.rotation.X);
                    binWriter.Write(scenaryDataProperties.rotation.Y);
                    binWriter.Write(scenaryDataProperties.rotation.Z);
                    // Write Position 
                    binWriter.Write(scenaryDataProperties.position.X);
                    binWriter.Write(scenaryDataProperties.position.Y);
                    binWriter.Write(scenaryDataProperties.position.Z);
                    // 5/31/2012 - Write Scale
                    binWriter.Write(scenaryDataProperties.scale.X);
                    binWriter.Write(scenaryDataProperties.scale.Y);
                    binWriter.Write(scenaryDataProperties.scale.Z);
                    // Write PathBlockSize
                    binWriter.Write(scenaryDataProperties.pathBlockSize);
                    // Write IsPathBlocked
                    binWriter.Write(scenaryDataProperties.isPathBlocked);
                    // Write Name - 10/6/2009
                    binWriter.Write(scenaryDataProperties.name);
                }

            }

        }
#endif


        /// <summary>
        /// Using given Stuct, this method loads the data from an XML stream.
        /// </summary>
        /// <typeparam name="TStorageStruct">Storage struct to use</typeparam>
        /// <param name="device"><see cref="StorageDevice"/> instance</param>
        /// <param name="fileName">File name</param>
        /// <param name="subDirPath">(Optional) Sub directory within 'ContentMaps' to save file to</param>
        /// <param name="location"><see cref="StorageLocation"/> Enum</param>
        /// <param name="data">(OUT) storage struct</param>
        /// <returns>true/false of result</returns>
        private bool DoLoadStructData<TStorageStruct>(StorageDevice device, string fileName, string subDirPath, StorageLocation location, out TStorageStruct data)
        {
            data = default(TStorageStruct);

            try
            {
                // Save container Path incase outside class needs location
                // 8/1/2008 - Updated to check Location Type.
                switch (location)
                {
                    case StorageLocation.TitleStorage:
                        // 10/1/2010 - StorageContainer is now specifically for UserData, while
                        //             the new TitleContainer is used to access data in the Title location!
                        //_containerPath = StorageContainer.TitleLocation;

                        // 5/1/2009: Updated to String.IsNullOrEmpty per FXCop.
                        // 8/12/2008 - Combine SubDirPath to ContainerPath, if necessary.
                        /*var subPath = _containerPath;
                        if (!string.IsNullOrEmpty(subDirPath))
                        {
                            subPath = Path.Combine(_containerPath, subDirPath);
                        }*/

                        // Get the path of the save game.
                        var filename = Path.Combine(subDirPath, fileName);


                        if (File.Exists(filename))
                        {
                            // Open the file, creating it if necessary.
                            /*using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read))
                            {
                                // Convert the object to XML data and put it in the stream.
                                var serializer = new XmlSerializer(typeof(TStorageStruct));
                                data = (TStorageStruct)serializer.Deserialize(stream);

                            }*;*/

                            // 10/1/2010
                            return DoOpenFile(filename, out data);
                        }

                        return false;

                        break;
                    case StorageLocation.UserStorage:
                        //_containerPath = _container.Path;

                        // 9/23/2010 - XNA 4.0 Updates
                        // Open a storage container.
                        //_container = device.OpenContainer("XNARTSSaveGames");
                        var asynResult = device.BeginOpenContainer("XNARTSSaveGames", null, null);
                        device.EndOpenContainer(asynResult);

                        // Dispose the container, to commit changes.
                        //_container.Dispose();

                        break;
                    default:
                        break;
                }

                return false;

            }
            catch
            {
                data = default(TStorageStruct);
                System.Diagnostics.Debug.WriteLine("Method Error: Storage classes 'DoLoadStructData' threw an error.");
                return false;
            }

        }

        // XNA 4.0 new method
        /// <summary>
        /// This method opens a file using System.IO classes and the
        /// TitleLocation property.  
        /// </summary>
        private static bool DoOpenFile<TStorageStruct>(string fileName, out TStorageStruct data)
        {
            try
            {
                var stream = TitleContainer.OpenStream(fileName);
                //var sreader = new StreamReader(stream);
                // use StreamReader.ReadLine or other methods to read the file data

                // Convert the object to XML data and put it in the stream.
                var serializer = new XmlSerializer(typeof(TStorageStruct));
                data = (TStorageStruct)serializer.Deserialize(stream);

                //Console.WriteLine("File Size: " + stream.Length);
                stream.Close();

                return true;
            }
            catch (FileNotFoundException)
            {
                // this will be thrown by OpenStream if gamedata.txt
                // doesn't exist in the title storage location
                data = default(TStorageStruct);
                return false;
            }
        }

        // 3/20/2011
        ///<summary>
        /// Saves the given <see cref="Texture2D"/>.
        ///</summary>
        ///<param name="textureToSave">Instance of <see cref="Texture2D"/>.</param>
        ///<param name="imageFileFormat"><see cref="ImageFileFormat"/> to save texture as.</param>
        ///<param name="savePathName">Absolute file path to save texture to.</param>
        ///<returns>True/False of result.</returns>
        public static bool SaveTexture(Texture2D textureToSave, ImageFileFormat imageFileFormat, string savePathName)
        {
            // Check if Texture2D null
            if (textureToSave == null) throw new ArgumentNullException("textureToSave");

            // Check if given name is null or empty
            if (string.IsNullOrEmpty(savePathName)) throw new ArgumentNullException("savePathName");

            try
            {
                switch (imageFileFormat)
                {
                    case ImageFileFormat.Png:
                        using (Stream stream = File.OpenWrite(String.Format("{0}.png", savePathName)))
                        {
                            textureToSave.SaveAsPng(stream, textureToSave.Width, textureToSave.Height);
                        }
                        break;
                    case ImageFileFormat.Jpeg:
                        using (Stream stream = File.OpenWrite(String.Format("{0}.jpg", savePathName)))
                        {
                            textureToSave.SaveAsJpeg(stream, textureToSave.Width, textureToSave.Height);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("imageFileFormat");
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        // 3/21/2009: Updated: Realized the Loading of TitleStorage data, does 
        //            not need to access the GUIDE at all!  Therefore, I have removed
        //            all code which was calling on the GUIDE.       
        // 8/12/2008
        /// <summary>
        /// Retrieves a collection of Map names and returns to caller.
        /// </summary>
        /// <param name="subDirPath">(Optional) Sub directory within 'ContentMaps' to save file to</param>        
        /// <returns>Collection of Map names</returns>
        public string[] GetSavedMapNames(string subDirPath)
        {
            try
            {
                var mapNames = new string[1];

                // XNA 4.0 Updates
                //_containerPath = StorageContainer.TitleLocation;

                // 5/1/2009: Updated to String.IsNullOrEmpty per FXCop.
                var subPath = _containerPath;
                if (!string.IsNullOrEmpty(subDirPath))
                {
                    subPath = Path.Combine(_containerPath, subDirPath); // TODO : XNA 4: How get _ContainerPath?
                }

                // Iterate through all Folders contain inside; each Folder name is the MapName.

                if (Directory.Exists(subPath))
                {
                    // 4/8/2009 - List
                    var finalMapNames = new List<string>();

                    mapNames = Directory.GetDirectories(subPath);

                    // Turns out in order to just get the last Directory names,
                    // I can use the Path.GetFileName, since this method assumes
                    // the last 'name' after a "\", must be the FileName; however,
                    // in this case, it is the Directory name, which is want I want!! - Ben
                    var length = mapNames.Length; // 5/20/2010 - Cache
                    for (var i = 0; i < length; i++)
                    {
                        // 4/8/2009 - Exclude the "_MapPreviews" folder.
                        if (Path.GetFileName(mapNames[i]) != "_MapPreviews")
                            finalMapNames.Add(Path.GetFileName(mapNames[i]));
                        //mapNames[i] = Path.GetFileName(mapNames[i]);
                    }

                    // 4/8/2009
                    // Clear mapNames, and resize to FinalArray
                    Array.Clear(mapNames, 0, length);
                    Array.Resize(ref mapNames, finalMapNames.Count);

                    // 4/8/2009
                    // Copy Final MapNames over
                    finalMapNames.CopyTo(mapNames);

                }


                return mapNames;
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Method Error: Storage classes 'GetSavedMapNames' threw an error.");
                return null;
            }

        }
    }
}
