#region File Description

//-----------------------------------------------------------------------------
// DeflateInput.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

using System.Runtime.InteropServices;

namespace TWEngine.Utilities.Compression
{
    internal class DeflateInput
    {
        // Fields
        private int _count;
        private int _startIndex;

        // Methods

        // Properties
        internal byte[] Buffer { get; set; }

        internal int Count
        {
            get { return _count; }
            set { _count = value; }
        }

        internal int StartIndex
        {
            get { return _startIndex; }
            set { _startIndex = value; }
        }

        internal void ConsumeBytes(int n)
        {
            _startIndex += n;
            _count -= n;
        }

        internal InputState DumpState()
        {
            InputState state;
            state.count = _count;
            state.startIndex = _startIndex;
            return state;
        }

        internal void RestoreState(InputState state)
        {
            _count = state.count;
            _startIndex = state.startIndex;
        }

        // Nested Types

        #region Nested type: InputState

        [StructLayout(LayoutKind.Sequential)]
        internal struct InputState
        {
            internal int count;
            internal int startIndex;
        }

        #endregion
    }
}