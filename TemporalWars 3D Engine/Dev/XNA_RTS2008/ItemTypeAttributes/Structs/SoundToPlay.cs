#region File Description
//-----------------------------------------------------------------------------
// SoundToPlay.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using TWEngine.Audio.Enums;
using TWEngine.InstancedModels.Enums;

namespace TWEngine.ItemTypeAttributes.Structs
{
#pragma warning disable 1587
    ///<summary>
    /// The <see cref="SoundToPlay"/> structure stores the
    /// specific <see cref="Sounds"/> Enum to play for the given <see cref="ItemType"/>,
    /// as well as the <see cref="SoundBankGroup"/> Enum to use.
    ///</summary>
#pragma warning restore 1587
#if !XBOX360
    [Serializable]
#endif
    public struct SoundToPlay
    {
        ///<summary>
        /// Primary weapon fire sound.
        ///</summary>
        public Sounds SoundToPlayPrimaryFire;
        ///<summary>
        /// Secondary weapon fire sound.
        ///</summary>
        public Sounds SoundToPlaySecondaryFire;
    }
}