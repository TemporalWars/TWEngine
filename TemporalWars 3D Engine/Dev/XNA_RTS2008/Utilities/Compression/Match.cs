#region File Description
//-----------------------------------------------------------------------------
// Match.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using TWEngine.Utilities.Compression.Enums;

namespace TWEngine.Utilities.Compression
{
    internal class Match
    {
        // Fields
        private int len;
        private int pos;
        private MatchState state;
        private byte symbol;

        // Properties
        internal int Length
        {
            get
            {
                return this.len;
            }
            set
            {
                this.len = value;
            }
        }

        internal int Position
        {
            get
            {
                return this.pos;
            }
            set
            {
                this.pos = value;
            }
        }

        internal MatchState State
        {
            get
            {
                return this.state;
            }
            set
            {
                this.state = value;
            }
        }

        internal byte Symbol
        {
            get
            {
                return this.symbol;
            }
            set
            {
                this.symbol = value;
            }
        }
    }


}
