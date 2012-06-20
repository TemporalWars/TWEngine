#region File Description

//-----------------------------------------------------------------------------
// FastEncoder.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

using System;
using TWEngine.Utilities.Compression.Enums;

namespace TWEngine.Utilities.Compression
{
    internal class FastEncoder
    {
        // Fields
        private readonly Match _currentMatch = new Match();
        private readonly FastEncoderWindow _inputWindow = new FastEncoderWindow();
        private double _lastCompressionRatio;

        internal int BytesInHistory
        {
            get { return _inputWindow.BytesAvailable; }
        }

        internal double LastCompressionRatio
        {
            get { return _lastCompressionRatio; }
        }

        internal DeflateInput UnprocessedInput
        {
            get { return _inputWindow.UnprocessedInput; }
        }

        // Methods
        internal void FlushInput()
        {
            _inputWindow.FlushWindow();
        }

        internal void GetBlock(DeflateInput input, OutputBuffer output, int maxBytesToCopy)
        {
            WriteDeflatePreamble(output);
            GetCompressedOutput(input, output, maxBytesToCopy);
            WriteEndOfBlock(output);
        }

        internal void GetBlockFooter(OutputBuffer output)
        {
            WriteEndOfBlock(output);
        }

        internal void GetBlockHeader(OutputBuffer output)
        {
            WriteDeflatePreamble(output);
        }

        internal void GetCompressedData(DeflateInput input, OutputBuffer output)
        {
            GetCompressedOutput(input, output, -1);
        }

        private void GetCompressedOutput(OutputBuffer output)
        {
            while ((_inputWindow.BytesAvailable > 0) && SafeToWriteTo(output))
            {
                _inputWindow.GetNextSymbolOrMatch(_currentMatch);
                if (_currentMatch.State == MatchState.HasSymbol)
                {
                    WriteChar(_currentMatch.Symbol, output);
                }
                else
                {
                    if (_currentMatch.State == MatchState.HasMatch)
                    {
                        WriteMatch(_currentMatch.Length, _currentMatch.Position, output);
                        continue;
                    }
                    WriteChar(_currentMatch.Symbol, output);
                    WriteMatch(_currentMatch.Length, _currentMatch.Position, output);
                }
            }
        }

        private void GetCompressedOutput(DeflateInput input, OutputBuffer output, int maxBytesToCopy)
        {
            int bytesWritten = output.BytesWritten;
            int num2 = 0;
            int num3 = BytesInHistory + input.Count;
            do
            {
                int num4 = (input.Count < _inputWindow.FreeWindowSpace) ? input.Count : _inputWindow.FreeWindowSpace;
                if (maxBytesToCopy >= 1)
                {
                    num4 = Math.Min(num4, maxBytesToCopy - num2);
                }
                if (num4 > 0)
                {
                    _inputWindow.CopyBytes(input.Buffer, input.StartIndex, num4);
                    input.ConsumeBytes(num4);
                    num2 += num4;
                }
                GetCompressedOutput(output);
            } while ((SafeToWriteTo(output) && InputAvailable(input)) &&
                     ((maxBytesToCopy < 1) || (num2 < maxBytesToCopy)));
            int num6 = output.BytesWritten - bytesWritten;
            int num7 = BytesInHistory + input.Count;
            int num8 = num3 - num7;
            if (num6 != 0)
            {
                _lastCompressionRatio = (num6)/((double) num8);
            }
        }

        private bool InputAvailable(DeflateInput input)
        {
            if (input.Count <= 0)
            {
                return (BytesInHistory > 0);
            }
            return true;
        }

        private bool SafeToWriteTo(OutputBuffer output)
        {
            return (output.FreeBytes > 0x10);
        }

        internal static void WriteChar(byte b, OutputBuffer output)
        {
            uint num = FastEncoderStatics.FastEncoderLiteralCodeInfo[b];
            output.WriteBits(((int) num) & 0x1f, num >> 5);
        }

        internal static void WriteDeflatePreamble(OutputBuffer output)
        {
            output.WriteBytes(FastEncoderStatics.FastEncoderTreeStructureData, 0,
                              FastEncoderStatics.FastEncoderTreeStructureData.Length);
            output.WriteBits(9, 0x22);
        }

        private void WriteEndOfBlock(OutputBuffer output)
        {
            uint num = FastEncoderStatics.FastEncoderLiteralCodeInfo[0x100];
            int n = ((int) num) & 0x1f;
            output.WriteBits(n, num >> 5);
        }

        internal static void WriteMatch(int matchLen, int matchPos, OutputBuffer output)
        {
            uint num = FastEncoderStatics.FastEncoderLiteralCodeInfo[0xfe + matchLen];
            int n = ((int) num) & 0x1f;
            if (n <= 0x10)
            {
                output.WriteBits(n, num >> 5);
            }
            else
            {
                output.WriteBits(0x10, (num >> 5) & 0xffff);
                output.WriteBits(n - 0x10, num >> 0x15);
            }
            num = FastEncoderStatics.FastEncoderDistanceCodeInfo[FastEncoderStatics.GetSlot(matchPos)];
            output.WriteBits(((int) num) & 15, num >> 8);
            int num3 = ((int) (num >> 4)) & 15;
            if (num3 != 0)
            {
                output.WriteBits(num3, ((uint) matchPos) & FastEncoderStatics.BitMask[num3]);
            }
        }

        // Properties
    }
}