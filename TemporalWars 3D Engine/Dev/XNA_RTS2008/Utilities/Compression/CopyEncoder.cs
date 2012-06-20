#region File Description

//-----------------------------------------------------------------------------
// CopyEncoder.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

using System;

namespace TWEngine.Utilities.Compression
{
    internal class CopyEncoder
    {
        // Fields
        private const int MaxUncompressedBlockSize = 0x10000;
        private const int PaddingSize = 5;

        // Methods
        public void GetBlock(DeflateInput input, OutputBuffer output, bool isFinal)
        {
            var count = 0;
            if (input != null)
            {
                count = Math.Min(input.Count, (output.FreeBytes - 5) - output.BitsInBuffer);
                if (count > 0xfffb)
                {
                    count = 0xfffb;
                }
            }
            if (isFinal)
            {
                output.WriteBits(3, 1);
            }
            else
            {
                output.WriteBits(3, 0);
            }
            output.FlushBits();
            WriteLenNLen((ushort) count, output);
            if ((input != null) && (count > 0))
            {
                output.WriteBytes(input.Buffer, input.StartIndex, count);
                input.ConsumeBytes(count);
            }
        }

        private void WriteLenNLen(ushort len, OutputBuffer output)
        {
            output.WriteUInt16(len);
            var num = (ushort) (~len);
            output.WriteUInt16(num);
        }
    }
}