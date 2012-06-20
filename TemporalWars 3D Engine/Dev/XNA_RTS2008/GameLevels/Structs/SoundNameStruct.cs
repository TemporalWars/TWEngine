#region File Description
//-----------------------------------------------------------------------------
// SoundNameStruct.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using TWEngine.Audio.Enums;

namespace TWEngine.GameLevels.Structs
{
    // 6/11/2012
    /// <summary>
    /// Stores a single <see cref="Sounds"/> request, with the 'SoundName' and 'Guid' association.
    /// </summary>
    public struct SoundNameStruct
    {
        /// <summary>
        /// Name to associate with sound.
        /// </summary>
        public string SoundName { get; private set; }
        /// <summary>
        /// <see cref="Sounds"/> unique <see cref="Guid"/>, return my AudioManager.
        /// </summary>
        public Guid UniqueKey { get; private set; }
        /// <summary>
        /// <see cref="Sounds"/> to play.
        /// </summary>
        public Sounds Sound { get; private set; }

        /// <summary>
        /// Initializes a new soundName request.
        /// </summary>
        /// <param name="soundName">Sounds unique name.</param>
        /// <param name="sound"><see cref="Sounds"/> to play.</param>
        public SoundNameStruct(string soundName, Sounds sound):this()
        {
            SoundName = soundName;
            Sound = sound;
            UniqueKey = Guid.NewGuid();
        }
    }
}