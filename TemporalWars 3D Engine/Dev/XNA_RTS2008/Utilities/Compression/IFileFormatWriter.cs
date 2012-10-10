#region File Description
//-----------------------------------------------------------------------------
// IFileFormatWriter.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace ImageNexus.BenScharbach.TWEngine.Utilities.Compression
{
    internal interface IFileFormatWriter
    {
        // Methods
        byte[] GetFooter();
        byte[] GetHeader();
        void UpdateWithBytesRead(byte[] buffer, int offset, int bytesToCopy);
    }
}
