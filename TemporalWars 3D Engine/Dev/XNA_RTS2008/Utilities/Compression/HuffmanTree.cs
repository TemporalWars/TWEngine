#region File Description

//-----------------------------------------------------------------------------
// HuffmanTree.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

using System;

namespace TWEngine.Utilities.Compression
{
    internal class HuffmanTree
    {
        // Fields
        internal const int EndOfBlockCode = 0x100;
        internal const int MaxDistTreeElements = 0x20;
        internal const int MaxLiteralTreeElements = 0x120;
        internal const int NumberOfCodeLengthTreeElements = 0x13;
        private static readonly HuffmanTree _staticDistanceTree = new HuffmanTree(GetStaticDistanceTreeLength());
        private static readonly HuffmanTree _staticLiteralLengthTree = new HuffmanTree(GetStaticLiteralTreeLength());
        private readonly byte[] _codeLengthArray;
        private readonly int _tableBits;
        private readonly int _tableMask;
        private short[] _left;
        private short[] _right;
        private short[] _table;

        // Methods
        public HuffmanTree(byte[] codeLengths)
        {
            _codeLengthArray = codeLengths;
            if (_codeLengthArray.Length == 0x120)
            {
                _tableBits = 9;
            }
            else
            {
                _tableBits = 7;
            }
            _tableMask = ((1) << _tableBits) - 1;
            CreateTable();
        }

        public static HuffmanTree StaticDistanceTree
        {
            get { return _staticDistanceTree; }
        }

        public static HuffmanTree StaticLiteralLengthTree
        {
            get { return _staticLiteralLengthTree; }
        }

        private uint[] CalculateHuffmanCode()
        {
            var numArray = new uint[0x11];
            var codeLengthArray = _codeLengthArray;
            for (var i = 0; i < codeLengthArray.Length; i++)
            {
                int index = codeLengthArray[i];
                numArray[index]++;
            }
            numArray[0] = 0;
            var numArray2 = new uint[0x11];
            const uint num2 = 0;
            for (int j = 1; j <= 0x10; j++)
            {
                numArray2[j] = (num2 + numArray[j - 1]) << 1;
            }
            var numArray3 = new uint[0x120];
            for (var k = 0; k < _codeLengthArray.Length; k++)
            {
                int length = _codeLengthArray[k];
                if (length > 0)
                {
                    numArray3[k] = FastEncoderStatics.BitReverse(numArray2[length], length);
                    numArray2[length]++;
                }
            }
            return numArray3;
        }

        private void CreateTable()
        {
            var numArray = CalculateHuffmanCode();
            _table = new short[(1) << _tableBits];
            _left = new short[2*_codeLengthArray.Length];
            _right = new short[2*_codeLengthArray.Length];
            var length = (short) _codeLengthArray.Length;
            for (var i = 0; i < _codeLengthArray.Length; i++)
            {
                int num3 = _codeLengthArray[i];
                if (num3 > 0)
                {
                    var index = (int) numArray[i];
                    if (num3 <= _tableBits)
                    {
                        var num5 = (1) << num3;
                        if (index >= num5)
                        {
                            //throw new InvalidDataException();
                            throw new InvalidOperationException("InvalidHuffmanData");
                        }
                        var num6 = (1) << (_tableBits - num3);
                        for (var j = 0; j < num6; j++)
                        {
                            _table[index] = (short) i;
                            index += num5;
                        }
                    }
                    else
                    {
                        var num8 = num3 - _tableBits;
                        int num9 = (1) << _tableBits;
                        int num10 = index & (((1) << _tableBits) - 1);
                        short[] table = _table;
                        do
                        {
                            short num11 = table[num10];
                            if (num11 == 0)
                            {
                                table[num10] = (short) (-length);
                                num11 = (short) (-length);
                                length = (short) (length + 1);
                            }
                            if (num11 > 0)
                            {
                                throw new InvalidOperationException("InvalidHuffmanData");
                            }
                            if ((index & num9) == 0)
                            {
                                table = _left;
                            }
                            else
                            {
                                table = _right;
                            }
                            num10 = -num11;
                            num9 = num9 << 1;
                            num8--;
                        } while (num8 != 0);
                        table[num10] = (short) i;
                    }
                }
            }
        }

        public int GetNextSymbol(InputBuffer input)
        {
            uint num = input.TryLoad16Bits();
            if (input.AvailableBits == 0)
            {
                return -1;
            }
            int index = _table[(int) ((IntPtr) (num & _tableMask))];
            if (index < 0)
            {
                uint num3 = ((uint) 1) << _tableBits;
                do
                {
                    index = -index;
                    if ((num & num3) == 0)
                    {
                        index = _left[index];
                    }
                    else
                    {
                        index = _right[index];
                    }
                    num3 = num3 << 1;
                } while (index < 0);
            }
            int n = _codeLengthArray[index];
            if (n <= 0)
            {
                throw new InvalidOperationException("InvalidHuffmanData");
            }
            if (n > input.AvailableBits)
            {
                return -1;
            }
            input.SkipBits(n);
            return index;
        }

        private static byte[] GetStaticDistanceTreeLength()
        {
            var buffer = new byte[0x20];
            for (int i = 0; i < 0x20; i++)
            {
                buffer[i] = 5;
            }
            return buffer;
        }

        private static byte[] GetStaticLiteralTreeLength()
        {
            var buffer = new byte[0x120];
            for (int i = 0; i <= 0x8f; i++)
            {
                buffer[i] = 8;
            }
            for (int j = 0x90; j <= 0xff; j++)
            {
                buffer[j] = 9;
            }
            for (int k = 0x100; k <= 0x117; k++)
            {
                buffer[k] = 7;
            }
            for (int m = 280; m <= 0x11f; m++)
            {
                buffer[m] = 8;
            }
            return buffer;
        }

        // Properties
    }
}