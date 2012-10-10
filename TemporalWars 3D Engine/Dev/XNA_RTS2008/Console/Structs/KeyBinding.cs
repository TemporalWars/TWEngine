#region File Description
//-----------------------------------------------------------------------------
// KeyBinding.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using Microsoft.Xna.Framework.Input;

namespace ImageNexus.BenScharbach.TWEngine.Console.Structs
{
    /// <summary>
    /// Characters and commands bound to specific key strokes.
    /// </summary>
    struct KeyBinding
    {
        public Keys		Key;
        public char		UnmodifiedChar;
        public char		ShiftChar;
        public char		AltChar;
        public char		ShiftAltChar;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="key">Enter the <see cref="Keys"/>.</param>
        /// <param name="unmodifiedChar">Character as is.</param>
        /// <param name="shiftChar">Character with Shift key pressed.</param>
        /// <param name="altChar">Character with Alt key pressed.</param>
        /// <param name="shiftAltChar">Character with Shift and Alt keys pressed.</param>
        public KeyBinding(Keys key, char unmodifiedChar, char shiftChar, char altChar, char shiftAltChar)
        {
            Key = key;
            UnmodifiedChar = unmodifiedChar;
            ShiftChar = shiftChar;
            AltChar = altChar;
            ShiftAltChar = shiftAltChar;
        }
    }
}