#region Imports

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Content;

#endregion Imports

namespace XnaContentZipper
{
    /// <summary>
    /// Reads a zip file created with the program and creates a content manager to work with it. You should use the .cs
    /// file generated from the ZipArchiveCreator tool to ensure you have the correct resource name.
    /// </summary>
    public class ZippedContent : ContentManager, IEnumerable<KeyValuePair<string, long>>
    {
        #region Helper classes

       

        #endregion Helper classes

        #region Private variables

        private const int intSize = 4;
        private FileStream zipFile;
        private BinaryReader reader;
        private long indexOffset;
        private int count;

        #endregion Private variables

        #region Private methods

        private byte[] ReadBytes(string assetName)
        {
            int length;
            Stream stream = GetStream(assetName, out length);
            if (stream == null)
            {
                throw new IOException("Cannot get bytes for asset name " + assetName);
            }
            byte[] bytes = new byte[length];
            stream.Read(bytes, 0, length);
            return bytes;
        }

        private Stream GetStream(string assetName, out int length)
        {
            length = -1;
            int count;
            string key;
            ushort hash = GetHashCode(assetName);
            zipFile.Position = indexOffset + (hash * intSize);
            long offset = reader.ReadInt32();
            if (offset == int.MinValue)
            {
                return null;
            }
            zipFile.Position = indexOffset + offset;
            count = (int)reader.ReadByte();
            for (int i = 0; i < count; i++)
            {
                key = reader.ReadString();
                offset = reader.ReadInt64();
                if (key.Equals(assetName, StringComparison.OrdinalIgnoreCase))
                {
                    zipFile.Position = offset;
                    length = reader.ReadInt32();
                    return new DeflateStream(zipFile, CompressionMode.Decompress, true);
                }
            }
            return null;
        }

        private static void GetXactNameAndComment(string text, int index, out string name, out string comment)
        {
            name = null;
            comment = null;
            text = text.Substring(index).TrimStart();
            using (StringReader reader = new StringReader(text))
            {
                string line;
                int pos;
                while ((line = reader.ReadLine()) != null && (line = line.Trim()).Length > 0)
                {
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
            int hashCode = 5381;
            char c;
            text = text.Normalize().Trim();
            for (int i = 0; i < text.Length; i++)
            {
                c = text[i];
                if (char.IsLower(c))
                {
                    c = char.ToUpperInvariant(c);
                }
                hashCode = ((hashCode << 5) + hashCode) + c;
            }
            return (ushort)(hashCode & 0x0000FFFF);
        }

        #endregion Private methods

        #region Protected methods

        /// <summary>
        /// Gets a zip stream that will decompress an asset
        /// </summary>
        /// <param name="assetName">Asset name</param>
        /// <returns>Stream</returns>
        protected override Stream OpenStream(string assetName)
        {
            int length;
            Stream stream = GetStream(assetName, out length);
            if (stream == null)
            {
                stream = base.OpenStream(assetName);
            }
            return stream;
        }

        /// <summary>
        /// Disposes and closes the zip archive
        /// </summary>
        /// <param name="disposing">Disposing</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (zipFile != null)
            {
                zipFile.Close();
                zipFile = null;
            }
        }

        #endregion Protected methods

        #region Public methods

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="zipFileName">Path to zip file name</param>
        /// <param name="serviceProvider">Service provider</param>
        public ZippedContent(string zipFileName, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            zipFile = File.OpenRead(zipFileName);
            reader = new BinaryReader(zipFile, Encoding.UTF8);
            zipFile.Position = zipFile.Length - (intSize + intSize);
            indexOffset = reader.ReadInt64();
            zipFile.Position = indexOffset + (ushort.MaxValue * intSize) + intSize;
            count = reader.ReadInt32();
        }

        /// <summary>
        /// Loads a resource by using the id from a constants.cs file
        /// </summary>
        /// <typeparam name="T">Type of resource to load</typeparam>
        /// <param name="assetName">Id of the resource to load (should be a number)</param>
        /// <returns>Resource</returns>
        public override T Load<T>(string assetName)
        {
            return base.Load<T>(assetName);
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
            return Encoding.UTF8.GetString(LoadBytes(assetName));
        }

        /// <summary>
        /// Enumerates all resource names and their offsets into the file
        /// </summary>
        /// <returns>Enumerator</returns>
        public IEnumerator<KeyValuePair<string, long>> GetEnumerator()
        {
            zipFile.Position = indexOffset + (ushort.MaxValue * intSize);
            int count = reader.ReadInt32();
            int i = 0;
            int subCount;
            while (i < count)
            {
                subCount = (int)reader.ReadByte();
                for (int j = 0; j < subCount; j++)
                {
                    yield return new KeyValuePair<string, long>(reader.ReadString(), reader.ReadInt64());
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

       

        #endregion Public methods
    }
}
