#region File Description
//-----------------------------------------------------------------------------
// LoopSongData.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.Audio.Enums;

namespace ImageNexus.BenScharbach.TWEngine.Audio.Structs
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.Audio.Structs"/> namespace contains the structures
    /// used by the <see cref="TWEngine.Audio"/> namespace.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    
    ///<summary>
    /// The <see cref="LoopSongData"/> is used to store the sound which will
    /// continually play, until told to stop.
    ///</summary>
    public struct LoopSongData
    {
        ///<summary>
        /// The <see cref="SoundBankGroup"/> to use.
        ///</summary>
        public SoundBankGroup SoundBank;
        ///<summary>
        /// The <see cref="Sounds"/> to play.
        ///</summary>
        public Sounds Sound;
        ///<summary>
        /// Delay value to store back into the <see cref="DelayBetweenPlays"/>.
        ///</summary>
        public int ResetDelayBetweenPlaysValue;
        ///<summary>
        /// <see cref="TimeSpan"/> as delay to be used between playing sound.
        ///</summary>
        public TimeSpan DelayBetweenPlays;
    }
}