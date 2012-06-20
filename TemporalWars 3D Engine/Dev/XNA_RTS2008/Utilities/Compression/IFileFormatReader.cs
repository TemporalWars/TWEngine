#region File Description
//-----------------------------------------------------------------------------
// IFileFormatReader.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace TWEngine.Utilities.Compression
{
    internal interface IFileFormatReader
    {
        // Methods
        bool ReadFooter(InputBuffer input);
        bool ReadHeader(InputBuffer input);
        void UpdateWithBytesRead(byte[] buffer, int offset, int bytesToCopy);
        void Validate();
    }

}
