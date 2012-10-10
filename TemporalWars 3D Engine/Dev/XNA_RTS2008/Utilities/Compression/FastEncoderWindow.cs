#region File Description

//-----------------------------------------------------------------------------
// FastEncoderWindow.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

using System;
using System.Diagnostics;
using ImageNexus.BenScharbach.TWEngine.Utilities.Compression.Enums;

namespace ImageNexus.BenScharbach.TWEngine.Utilities.Compression
{
    internal class FastEncoderWindow
    {
        // Fields
        private const int FastEncoderHashMask = 0x7ff;
        private const int FastEncoderHashShift = 4;
        private const int FastEncoderHashtableSize = 0x800;
        private const int FastEncoderMatch3DistThreshold = 0x4000;
        private const int FastEncoderWindowMask = 0x1fff;
        private const int FastEncoderWindowSize = 0x2000;
        private const int GoodLength = 4;
        private const int LazyMatchThreshold = 6;
        internal const int MaxMatch = 0x102;
        internal const int MinMatch = 3;
        private const int NiceLength = 0x20;
        private const int SearchDepth = 0x20;
        private int _bufEnd;
        private int _bufPos;
        private ushort[] _lookup;
        private ushort[] _prev;
        private byte[] _window;

        // Methods
        public FastEncoderWindow()
        {
            ResetWindow();
        }

        public int BytesAvailable
        {
            get { return (_bufEnd - _bufPos); }
        }

        public int FreeWindowSpace
        {
            get { return (0x4000 - _bufEnd); }
        }

        public DeflateInput UnprocessedInput
        {
            get
            {
                var input = new DeflateInput {Buffer = _window, StartIndex = _bufPos, Count = _bufEnd - _bufPos};
                return input;
            }
        }

        public void CopyBytes(byte[] inputBuffer, int startIndex, int count)
        {
            Array.Copy(inputBuffer, startIndex, _window, _bufEnd, count);
            _bufEnd += count;
        }

        private int FindMatch(int search, out int matchPos, int searchDepth, int niceLength)
        {
            int num = 0;
            int num2 = 0;
            int num3 = _bufPos - 0x2000;
            byte num4 = _window[_bufPos];
            while (search > num3)
            {
                if (_window[search + num] == num4)
                {
                    int num5 = 0;
                    while (num5 < 0x102)
                    {
                        if (_window[_bufPos + num5] != _window[search + num5])
                        {
                            break;
                        }
                        num5++;
                    }
                    if (num5 > num)
                    {
                        num = num5;
                        num2 = search;
                        if (num5 > 0x20)
                        {
                            break;
                        }
                        num4 = _window[_bufPos + num5];
                    }
                }
                if (--searchDepth == 0)
                {
                    break;
                }
                search = _prev[search & 0x1fff];
            }
            matchPos = (_bufPos - num2) - 1;
            if ((num == 3) && (matchPos >= 0x4000))
            {
                return 0;
            }
            return num;
        }

        //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void FlushWindow()
        {
            ResetWindow();
        }

        internal bool GetNextSymbolOrMatch(Match match)
        {
            int num2;
            uint hash = HashValue(0, _window[_bufPos]);
            hash = HashValue(hash, _window[_bufPos + 1]);
            int matchPos = 0;
            if ((_bufEnd - _bufPos) <= 3)
            {
                num2 = 0;
            }
            else
            {
                var search = (int) InsertString(ref hash);
                if (search != 0)
                {
                    num2 = FindMatch(search, out matchPos, 0x20, 0x20);
                    if ((_bufPos + num2) > _bufEnd)
                    {
                        num2 = _bufEnd - _bufPos;
                    }
                }
                else
                {
                    num2 = 0;
                }
            }
            if (num2 < 3)
            {
                match.State = MatchState.HasSymbol;
                match.Symbol = _window[_bufPos];
                _bufPos++;
            }
            else
            {
                _bufPos++;
                if (num2 <= 6)
                {
                    int num5;
                    int num6 = 0;
                    var num7 = (int) InsertString(ref hash);
                    if (num7 != 0)
                    {
                        num5 = FindMatch(num7, out num6, (num2 < 4) ? 0x20 : 8, 0x20);
                        if ((_bufPos + num5) > _bufEnd)
                        {
                            num5 = _bufEnd - _bufPos;
                        }
                    }
                    else
                    {
                        num5 = 0;
                    }
                    if (num5 > num2)
                    {
                        match.State = MatchState.HasSymbolAndMatch;
                        match.Symbol = _window[_bufPos - 1];
                        match.Position = num6;
                        match.Length = num5;
                        _bufPos++;
                        num2 = num5;
                        InsertStrings(ref hash, num2);
                    }
                    else
                    {
                        match.State = MatchState.HasMatch;
                        match.Position = matchPos;
                        match.Length = num2;
                        num2--;
                        _bufPos++;
                        InsertStrings(ref hash, num2);
                    }
                }
                else
                {
                    match.State = MatchState.HasMatch;
                    match.Position = matchPos;
                    match.Length = num2;
                    InsertStrings(ref hash, num2);
                }
            }
            if (_bufPos == 0x4000)
            {
                MoveWindows();
            }
            return true;
        }

        private uint HashValue(uint hash, byte b)
        {
            return ((hash << 4) ^ b);
        }

        private uint InsertString(ref uint hash)
        {
            hash = HashValue(hash, _window[_bufPos + 2]);
            uint num = _lookup[hash & 0x7ff];
            _lookup[hash & 0x7ff] = (ushort) _bufPos;
            _prev[_bufPos & 0x1fff] = (ushort) num;
            return num;
        }

        private void InsertStrings(ref uint hash, int matchLen)
        {
            if ((_bufEnd - _bufPos) <= matchLen)
            {
                _bufPos += matchLen - 1;
            }
            else
            {
                while (--matchLen > 0)
                {
                    InsertString(ref hash);
                    _bufPos++;
                }
            }
        }

        public void MoveWindows()
        {
            int num;
            Array.Copy(_window, _bufPos - 0x2000, _window, 0, 0x2000);
            for (num = 0; num < 0x800; num++)
            {
                int num2 = _lookup[num] - 0x2000;
                if (num2 <= 0)
                {
                    _lookup[num] = 0;
                }
                else
                {
                    _lookup[num] = (ushort) num2;
                }
            }
            for (num = 0; num < 0x2000; num++)
            {
                long num3 = _prev[num] - 0x2000L;
                if (num3 <= 0L)
                {
                    _prev[num] = 0;
                }
                else
                {
                    _prev[num] = (ushort) num3;
                }
            }
            _bufPos = 0x2000;
            _bufEnd = _bufPos;
        }

        private uint RecalculateHash(int position)
        {
            return (uint) ((((_window[position] << 8) ^ (_window[position + 1] << 4)) ^ _window[position + 2]) & 0x7ff);
        }

        private void ResetWindow()
        {
            _window = new byte[0x4106];
            _prev = new ushort[0x2102];
            _lookup = new ushort[0x800];
            _bufPos = 0x2000;
            _bufEnd = _bufPos;
        }

        [Conditional("DEBUG")]
        private void VerifyHashes()
        {
            for (int i = 0; i < 0x800; i++)
            {
                ushort num3;
                for (ushort j = _lookup[i]; (j != 0) && ((_bufPos - j) < 0x2000); j = num3)
                {
                    num3 = _prev[j & 0x1fff];
                    if ((_bufPos - num3) >= 0x2000)
                    {
                        break;
                    }
                }
            }
        }

        // Properties
    }
}