#region File Description

//-----------------------------------------------------------------------------
// Inflater.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.Utilities.Compression.Enums;

namespace ImageNexus.BenScharbach.TWEngine.Utilities.Compression
{
    internal class Inflater
    {
        // Fields
        private static readonly byte[] CodeOrder = new byte[]
                                                       {
                                                           0x10, 0x11, 0x12, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2,
                                                           14, 1, 15
                                                       };

        private static readonly int[] DistanceBasePosition = new[]
                                                                 {
                                                                     1, 2, 3, 4, 5, 7, 9, 13, 0x11, 0x19, 0x21, 0x31,
                                                                     0x41, 0x61, 0x81, 0xc1,
                                                                     0x101, 0x181, 0x201, 0x301, 0x401, 0x601, 0x801,
                                                                     0xc01, 0x1001, 0x1801, 0x2001, 0x3001, 0x4001,
                                                                     0x6001, 0, 0
                                                                 };

        private static readonly byte[] ExtraLengthBits = new byte[]
                                                             {
                                                                 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2,
                                                                 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0
                                                             };

        private static readonly int[] LengthBase = new[]
                                                       {
                                                           3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15, 0x11, 0x13, 0x17, 0x1b,
                                                           0x1f,
                                                           0x23, 0x2b, 0x33, 0x3b, 0x43, 0x53, 0x63, 0x73, 0x83, 0xa3,
                                                           0xc3, 0xe3, 0x102
                                                       };

        private static readonly byte[] StaticDistanceTreeTable = new byte[]
                                                                     {
                                                                         0, 0x10, 8, 0x18, 4, 20, 12, 0x1c, 2, 0x12, 10,
                                                                         0x1a, 6, 0x16, 14, 30,
                                                                         1, 0x11, 9, 0x19, 5, 0x15, 13, 0x1d, 3, 0x13,
                                                                         11, 0x1b, 7, 0x17, 15, 0x1f
                                                                     };

        private readonly byte[] _blockLengthBuffer = new byte[4];
        private readonly byte[] _codeLengthTreeCodeLength = new byte[0x13];
        private readonly byte[] _codeList = new byte[320];
        private readonly InputBuffer _input = new InputBuffer();
        private readonly OutputWindow _output = new OutputWindow();

        private int _bfinal;
        private int _blockLength;
        private BlockType _blockType;
        private int _codeArraySize;
        private int _codeLengthCodeCount;
        private HuffmanTree _codeLengthTree;

        private int _distanceCode;
        private int _distanceCodeCount;
        private HuffmanTree _distanceTree;
        private int _extraBits;

        private IFileFormatReader _formatReader;
        private bool _hasFormatReader;
        private int _length;

        private int _lengthCode;
        private int _literalLengthCodeCount;
        private HuffmanTree _literalLengthTree;
        private int _loopCounter;
        private InflaterState _state;

        // Methods
        public Inflater()
        {
            Reset();
        }

        public int AvailableOutput
        {
            get { return _output.AvailableBytes; }
        }

        private bool Decode()
        {
            var flag = false;
            var flag2 = false;
            if (Finished())
            {
                return true;
            }
            if (_hasFormatReader)
            {
                if (_state == InflaterState.ReadingHeader)
                {
                    if (!_formatReader.ReadHeader(_input))
                    {
                        return false;
                    }
                    _state = InflaterState.ReadingBFinal;
                }
                else if ((_state == InflaterState.StartReadingFooter) || (_state == InflaterState.ReadingFooter))
                {
                    if (!_formatReader.ReadFooter(_input))
                    {
                        return false;
                    }
                    _state = InflaterState.VerifyingFooter;
                    return true;
                }
            }
            if (_state == InflaterState.ReadingBFinal)
            {
                if (!_input.EnsureBitsAvailable(1))
                {
                    return false;
                }
                _bfinal = _input.GetBits(1);
                _state = InflaterState.ReadingBType;
            }
            if (_state == InflaterState.ReadingBType)
            {
                if (!_input.EnsureBitsAvailable(2))
                {
                    _state = InflaterState.ReadingBType;
                    return false;
                }
                _blockType = (BlockType) _input.GetBits(2);
                if (_blockType != BlockType.Dynamic)
                {
                    if (_blockType != BlockType.Static)
                    {
                        if (_blockType != BlockType.Uncompressed)
                        {
                            //throw new InvalidDataException(SR.GetString("UnknownBlockType"));
                            throw new InvalidOperationException("UnknownBlockType");
                        }
                        _state = InflaterState.UncompressedAligning;
                    }
                    else
                    {
                        _literalLengthTree = HuffmanTree.StaticLiteralLengthTree;
                        _distanceTree = HuffmanTree.StaticDistanceTree;
                        _state = InflaterState.DecodeTop;
                    }
                }
                else
                {
                    _state = InflaterState.ReadingNumLitCodes;
                }
            }
            if (_blockType == BlockType.Dynamic)
            {
                if (_state < InflaterState.DecodeTop)
                {
                    flag2 = DecodeDynamicBlockHeader();
                }
                else
                {
                    flag2 = DecodeBlock(out flag);
                }
            }
            else if (_blockType == BlockType.Static)
            {
                flag2 = DecodeBlock(out flag);
            }
            else
            {
                if (_blockType != BlockType.Uncompressed)
                {
                    throw new InvalidOperationException("UnknownBlockType");
                }
                flag2 = DecodeUncompressedBlock(out flag);
            }
            if (flag && (_bfinal != 0))
            {
                if (_hasFormatReader)
                {
                    _state = InflaterState.StartReadingFooter;
                    return flag2;
                }
                _state = InflaterState.Done;
            }
            return flag2;
        }

        private bool DecodeBlock(out bool end_of_block_code_seen)
        {
            end_of_block_code_seen = false;
            int freeBytes = _output.FreeBytes;
            while (freeBytes > 0x102)
            {
                int nextSymbol;
                int num4;
                switch (_state)
                {
                    case InflaterState.DecodeTop:
                        nextSymbol = _literalLengthTree.GetNextSymbol(_input);
                        if (nextSymbol >= 0)
                        {
                            break;
                        }
                        return false;

                    case InflaterState.HaveInitialLength:
                        goto Label_00E4;

                    case InflaterState.HaveFullLength:
                        goto Label_0151;

                    case InflaterState.HaveDistCode:
                        goto Label_01B3;

                    default:
                        throw new InvalidOperationException("UnknownState");
                }
                if (nextSymbol < 0x100)
                {
                    _output.Write((byte) nextSymbol);
                    freeBytes--;
                    continue;
                }
                if (nextSymbol == 0x100)
                {
                    end_of_block_code_seen = true;
                    _state = InflaterState.ReadingBFinal;
                    return true;
                }
                nextSymbol -= 0x101;
                if (nextSymbol < 8)
                {
                    nextSymbol += 3;
                    _extraBits = 0;
                }
                else if (nextSymbol == 0x1c)
                {
                    nextSymbol = 0x102;
                    _extraBits = 0;
                }
                else
                {
                    if ((nextSymbol < 0) || (nextSymbol >= ExtraLengthBits.Length))
                    {
                        throw new InvalidOperationException("GenericInvalidData");
                    }
                    _extraBits = ExtraLengthBits[nextSymbol];
                }
                _length = nextSymbol;
                Label_00E4:
                if (_extraBits > 0)
                {
                    _state = InflaterState.HaveInitialLength;
                    int bits = _input.GetBits(_extraBits);
                    if (bits < 0)
                    {
                        return false;
                    }
                    if ((_length < 0) || (_length >= LengthBase.Length))
                    {
                        throw new InvalidOperationException("GenericInvalidData");
                    }
                    _length = LengthBase[_length] + bits;
                }
                _state = InflaterState.HaveFullLength;
                Label_0151:
                if (_blockType == BlockType.Dynamic)
                {
                    _distanceCode = _distanceTree.GetNextSymbol(_input);
                }
                else
                {
                    _distanceCode = _input.GetBits(5);
                    if (_distanceCode >= 0)
                    {
                        _distanceCode = StaticDistanceTreeTable[_distanceCode];
                    }
                }
                if (_distanceCode < 0)
                {
                    return false;
                }
                _state = InflaterState.HaveDistCode;
                Label_01B3:
                if (_distanceCode > 3)
                {
                    _extraBits = (_distanceCode - 2) >> 1;
                    int num5 = _input.GetBits(_extraBits);
                    if (num5 < 0)
                    {
                        return false;
                    }
                    num4 = DistanceBasePosition[_distanceCode] + num5;
                }
                else
                {
                    num4 = _distanceCode + 1;
                }
                _output.WriteLengthDistance(_length, num4);
                freeBytes -= _length;
                _state = InflaterState.DecodeTop;
            }
            return true;
        }

        private bool DecodeDynamicBlockHeader()
        {
            switch (_state)
            {
                case InflaterState.ReadingNumLitCodes:
                    _literalLengthCodeCount = _input.GetBits(5);
                    if (_literalLengthCodeCount >= 0)
                    {
                        _literalLengthCodeCount += 0x101;
                        _state = InflaterState.ReadingNumDistCodes;
                        break;
                    }
                    return false;

                case InflaterState.ReadingNumDistCodes:
                    break;

                case InflaterState.ReadingNumCodeLengthCodes:
                    goto Label_0096;

                case InflaterState.ReadingCodeLengthCodes:
                    goto Label_0107;

                case InflaterState.ReadingTreeCodesBefore:
                case InflaterState.ReadingTreeCodesAfter:
                    goto Label_0315;

                default:
                    throw new InvalidOperationException("GenericInvalidData");
            }
            _distanceCodeCount = _input.GetBits(5);
            if (_distanceCodeCount < 0)
            {
                return false;
            }
            _distanceCodeCount++;
            _state = InflaterState.ReadingNumCodeLengthCodes;
            Label_0096:
            _codeLengthCodeCount = _input.GetBits(4);
            if (_codeLengthCodeCount < 0)
            {
                return false;
            }
            _codeLengthCodeCount += 4;
            _loopCounter = 0;
            _state = InflaterState.ReadingCodeLengthCodes;
            Label_0107:
            while (_loopCounter < _codeLengthCodeCount)
            {
                int bits = _input.GetBits(3);
                if (bits < 0)
                {
                    return false;
                }
                _codeLengthTreeCodeLength[CodeOrder[_loopCounter]] = (byte) bits;
                _loopCounter++;
            }
            for (int i = _codeLengthCodeCount; i < CodeOrder.Length; i++)
            {
                _codeLengthTreeCodeLength[CodeOrder[i]] = 0;
            }
            _codeLengthTree = new HuffmanTree(_codeLengthTreeCodeLength);
            _codeArraySize = _literalLengthCodeCount + _distanceCodeCount;
            _loopCounter = 0;
            _state = InflaterState.ReadingTreeCodesBefore;
            Label_0315:
            while (_loopCounter < _codeArraySize)
            {
                if ((_state == InflaterState.ReadingTreeCodesBefore) &&
                    ((_lengthCode = _codeLengthTree.GetNextSymbol(_input)) < 0))
                {
                    return false;
                }
                if (_lengthCode <= 15)
                {
                    _codeList[_loopCounter++] = (byte) _lengthCode;
                }
                else
                {
                    int num3;
                    if (!_input.EnsureBitsAvailable(7))
                    {
                        _state = InflaterState.ReadingTreeCodesAfter;
                        return false;
                    }
                    if (_lengthCode == 0x10)
                    {
                        if (_loopCounter == 0)
                        {
                            throw new InvalidOperationException();
                        }
                        byte num4 = _codeList[_loopCounter - 1];
                        num3 = _input.GetBits(2) + 3;
                        if ((_loopCounter + num3) > _codeArraySize)
                        {
                            throw new InvalidOperationException();
                        }
                        for (int j = 0; j < num3; j++)
                        {
                            _codeList[_loopCounter++] = num4;
                        }
                    }
                    else if (_lengthCode == 0x11)
                    {
                        num3 = _input.GetBits(3) + 3;
                        if ((_loopCounter + num3) > _codeArraySize)
                        {
                            throw new InvalidOperationException();
                        }
                        for (int k = 0; k < num3; k++)
                        {
                            _codeList[_loopCounter++] = 0;
                        }
                    }
                    else
                    {
                        num3 = _input.GetBits(7) + 11;
                        if ((_loopCounter + num3) > _codeArraySize)
                        {
                            throw new InvalidOperationException();
                        }
                        for (int m = 0; m < num3; m++)
                        {
                            _codeList[_loopCounter++] = 0;
                        }
                    }
                }
                _state = InflaterState.ReadingTreeCodesBefore;
            }
            var destinationArray = new byte[0x120];
            var buffer2 = new byte[0x20];
            Array.Copy(_codeList, destinationArray, _literalLengthCodeCount);
            Array.Copy(_codeList, _literalLengthCodeCount, buffer2, 0, _distanceCodeCount);
            if (destinationArray[0x100] == 0)
            {
                throw new InvalidOperationException();
            }
            _literalLengthTree = new HuffmanTree(destinationArray);
            _distanceTree = new HuffmanTree(buffer2);
            _state = InflaterState.DecodeTop;
            return true;
        }

        private bool DecodeUncompressedBlock(out bool end_of_block)
        {
            end_of_block = false;
            while (true)
            {
                switch (_state)
                {
                    case InflaterState.UncompressedAligning:
                        _input.SkipToByteBoundary();
                        _state = InflaterState.UncompressedByte1;
                        break;

                    case InflaterState.UncompressedByte1:
                    case InflaterState.UncompressedByte2:
                    case InflaterState.UncompressedByte3:
                    case InflaterState.UncompressedByte4:
                        break;

                    case InflaterState.DecodingUncompressed:
                        {
                            int num3 = _output.CopyFrom(_input, _blockLength);
                            _blockLength -= num3;
                            if (_blockLength != 0)
                            {
                                return (_output.FreeBytes == 0);
                            }
                            _state = InflaterState.ReadingBFinal;
                            end_of_block = true;
                            return true;
                        }
                    default:
                        throw new InvalidOperationException("UnknownState");
                }
                int bits = _input.GetBits(8);
                if (bits < 0)
                {
                    return false;
                }
                _blockLengthBuffer[((int) _state) - 0x10] = (byte) bits;
                if (_state == InflaterState.UncompressedByte4)
                {
                    _blockLength = _blockLengthBuffer[0] + (_blockLengthBuffer[1]*0x100);
                    int num2 = _blockLengthBuffer[2] + (_blockLengthBuffer[3]*0x100);
                    if (((ushort) _blockLength) != ((ushort) ~num2))
                    {
                        throw new InvalidOperationException("InvalidBlockLength");
                    }
                }
                _state += 1;
            }
        }

        public bool Finished()
        {
            if (_state != InflaterState.Done)
            {
                return (_state == InflaterState.VerifyingFooter);
            }
            return true;
        }

        public int Inflate(byte[] bytes, int offset, int length)
        {
            int num = 0;
            do
            {
                int bytesToCopy = _output.CopyTo(bytes, offset, length);
                if (bytesToCopy > 0)
                {
                    if (_hasFormatReader)
                    {
                        _formatReader.UpdateWithBytesRead(bytes, offset, bytesToCopy);
                    }
                    offset += bytesToCopy;
                    num += bytesToCopy;
                    length -= bytesToCopy;
                }
            } while (((length != 0) && !Finished()) && Decode());
            if ((_state == InflaterState.VerifyingFooter) && (_output.AvailableBytes == 0))
            {
                _formatReader.Validate();
            }
            return num;
        }

        public bool NeedsInput()
        {
            return _input.NeedsInput();
        }

        private void Reset()
        {
            if (_hasFormatReader)
            {
                _state = InflaterState.ReadingHeader;
            }
            else
            {
                _state = InflaterState.ReadingBFinal;
            }
        }

        internal void SetFileFormatReader(IFileFormatReader reader)
        {
            _formatReader = reader;
            _hasFormatReader = true;
            Reset();
        }

        public void SetInput(byte[] inputBytes, int offset, int length)
        {
            _input.SetInput(inputBytes, offset, length);
        }

        // Properties
    }
}