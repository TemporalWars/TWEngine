#region File Description
//-----------------------------------------------------------------------------
// ZippedContent.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Imports

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Content;
using TWEngine.Utilities.Compression;
using TWEngine.Utilities.Compression.Enums;

#endregion Imports

namespace TWEngine.Utilities
{
    /// <summary>
    /// Reads a zip file created with the program and creates a content manager to work with it. You should use the .cs
    /// file generated from the ZipArchiveCreator tool to ensure you have the correct resource name.
    /// </summary>
    public class ZippedContent : ContentManager, IEnumerable<KeyValuePair<string, long>>
    {
        private const int IntSize = 4;
        private FileStream _zipFile;
        private readonly BinaryReader _reader;
        private readonly long _indexOffset;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="zipFileName">Path to zip file name</param>
        /// <param name="serviceProvider">Service provider</param>
        public ZippedContent(string zipFileName, IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _zipFile = File.OpenRead(zipFileName);
            _reader = new BinaryReader(_zipFile, Encoding.UTF8);
            _zipFile.Position = _zipFile.Length - (IntSize + IntSize);
            _indexOffset = _reader.ReadInt64();
            _zipFile.Position = _indexOffset + (ushort.MaxValue * IntSize) + IntSize;
            _reader.ReadInt32();
        }
      

        #region Public methods

        /// <summary>
        /// Loads a resource by using the id from a constants.cs file
        /// </summary>
        /// <typeparam name="T">Type of resource to load</typeparam>
        /// <param name="assetName">Id of the resource to load (should be a number)</param>
        /// <returns>Resource</returns>
        public override T Load<T>(string assetName)
        {
            // 11/11/09 - Captures the InvalidOpExp, which is thrown when the same
            //            resource is being loaded simultaneously.
            try
            {
                return base.Load<T>(assetName);
            }
            catch (InvalidOperationException err)
            {
                Debug.WriteLine(string.Format("ZippedContent Load<t> method threw InvalidOpExp. {0}", err.Message));
                return default(T);
            }
            catch (NullReferenceException err)
            {
                Debug.WriteLine(string.Format("ZippedContent Load<t> method threw NullRefExp. {0}", err.Message));
                return default(T);
            }
            // XNA 4.0 Updates - DriverInternalErrorException and OutOfVideoMemoryException obsolete.
            /*catch (DriverInternalErrorException err) // 1/5/2010
            {
                Debug.WriteLine("(Zipped Content Load<T>) threw the 'DriverInternalError' error.");
                return default(T);
            }
            catch(OutOfVideoMemoryException) // 3/23/2010
            {
                Debug.WriteLine("(Zipped Content Load<T>) threw the 'OutOfVideoMemoryException' error.");
                return default(T);
            }*/
            
        }

        /// <summary>
        /// Loads raw bytes from the file by using the id from a constants.cs file
        /// </summary>
        /// <param name="assetName">Asset name</param>
        /// <returns>Bytes</returns>
        public byte[] LoadBytes(string assetName)
        {
            return ReadBytes(assetName);
        }

        /// <summary>
        /// Loads the bytes for a resource into a stream
        /// </summary>
        /// <param name="assetName">Asset name</param>
        /// <returns>Stream</returns>
        public Stream LoadStream(string assetName)
        {
            return new MemoryStream(LoadBytes(assetName));
        }

        /// <summary>
        /// Loads a string from the file (assumes the file was in utf-8) by using the id from a constants.cs file
        /// </summary>
        /// <param name="assetName">Asset name</param>
        /// <returns>String</returns>
        public string LoadString(string assetName)
        {
            var loadBytes = LoadBytes(assetName);

#if XBOX360
            return Encoding.UTF8.GetString(loadBytes, 0, loadBytes.Length);
#else
            return Encoding.UTF8.GetString(loadBytes);
#endif
            
        }

        /// <summary>
        /// Enumerates all resource names and their offsets into the file
        /// </summary>
        /// <returns>Enumerator</returns>
        public IEnumerator<KeyValuePair<string, long>> GetEnumerator()
        {
            _zipFile.Position = _indexOffset + (ushort.MaxValue * IntSize);
            var count = _reader.ReadInt32();
            const int i = 0;
            while (i < count)
            {
                var subCount = (int)_reader.ReadByte();
                for (var j = 0; j < subCount; j++)
                {
                    yield return new KeyValuePair<string, long>(_reader.ReadString(), _reader.ReadInt64());
                }
                count += subCount;
            }
        }

        /// <summary>
        /// Enumerates all resource names and their offsets into the file
        /// </summary>
        /// <returns>Enumerator</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion 

        #region Protected methods

        /// <summary>
        /// Gets a zip stream that will decompress an asset
        /// </summary>
        /// <param name="assetName">Asset name</param>
        /// <returns>Stream</returns>
        protected override Stream OpenStream(string assetName)
        {
            int length;
            var stream = GetStream(assetName, out length) ?? base.OpenStream(assetName);
            return stream;
        }

        /// <summary>
        /// Disposes and closes the zip archive
        /// </summary>
        /// <param name="disposing">Disposing</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_zipFile == null) return;

            _zipFile.Close();
            _zipFile = null;
        }

        #endregion Protected methods

        #region Private methods

        /// <summary>
        /// Reads the bytes for the given <paramref name="assetName"/>.
        /// </summary>
        private byte[] ReadBytes(string assetName)
        {
            int length;
            var stream = GetStream(assetName, out length);
            if (stream == null)
            {
                throw new IOException("Cannot get bytes for asset name " + assetName);
            }
            var bytes = new byte[length];
            stream.Read(bytes, 0, length);
            return bytes;
        }

        /// <summary>
        /// Gets a <see cref="Stream"/> for the given <paramref name="assetName"/>.
        /// </summary>
        private Stream GetStream(string assetName, out int length)
        {
            length = -1;

            // 11/1/2009
            // Make sure assetName has no Path format; for example '..\\'.
            assetName = Path.GetFileNameWithoutExtension(assetName);

            // DEBUG
            //if (assetName.Equals("sciFiGunShip01")) System.Diagnostics.Debugger.Break();

            var hash = GetHashCode(assetName);
            _zipFile.Position = _indexOffset + (hash * IntSize);
            long offset = _reader.ReadInt32();
            if (offset == int.MinValue)
            {
                return null;
            }
            _zipFile.Position = _indexOffset + offset;
            var count = (int)_reader.ReadByte();
            for (var i = 0; i < count; i++)
            {
                var key = _reader.ReadString();
                offset = _reader.ReadInt64();
                if (!key.Equals(assetName, StringComparison.OrdinalIgnoreCase)) continue;

                _zipFile.Position = offset;
                length = _reader.ReadInt32();
                return new DeflateStream(_zipFile, CompressionMode.Decompress, true);
            }
            return null;
        }

        /// <summary>
        /// Gets the name and comment.
        /// </summary>
        private static void GetXactNameAndComment(string text, int index, out string name, out string comment)
        {
            name = null;
            comment = null;
            text = text.Substring(index).TrimStart();
            using (var reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null && (line = line.Trim()).Length > 0)
                {
                    int pos;
                    if (line.StartsWith("Name = ", StringComparison.OrdinalIgnoreCase))
                    {
                        pos = line.IndexOf(" = ");
                        name = FieldEncode(line.Substring(pos + 3).TrimEnd(';'));
                    }
                    else if (line.StartsWith("Comment = ", StringComparison.OrdinalIgnoreCase))
                    {
                        pos = line.IndexOf(" = ");
                        comment = IntellisenseEncode(line.Substring(pos + 3).TrimEnd(';'));
                    }
                }
            }
        }

        /// <summary>
        /// Encodes the given string.
        /// </summary>
        private static string FieldEncode(string text)
        {
            return Regex.Replace(text, "[^a-zA-Z0-9_\\\\]", string.Empty);
        }

        /// <summary>
        /// Intellisense Encode.
        /// </summary>
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
#if XBOX360
            text = text.Trim();
#else
            text = text.Normalize().Trim();
#endif

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (char.IsLower(c))
                {
#if XBOX360
                    c = char.ToUpper(c);
#else
                    c = char.ToUpperInvariant(c);
#endif
                }
                hashCode = ((hashCode << 5) + hashCode) + c;
            }
            return (ushort)(hashCode & 0x0000FFFF);
        }

        #endregion Private methods
    }
}