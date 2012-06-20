#region File Description
//-----------------------------------------------------------------------------
// SoundRequestItem.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework.Audio;
using TWEngine.Audio.Enums;
using TWEngine.SceneItems;

namespace TWEngine.Audio.Structs
{
    ///<summary>
    /// The <see cref="SoundRequestItem"/> structure is used to queue a specific sound request.
    ///</summary>
    public struct SoundRequestItem
    {
        // 6/10/2012 - Updated to Guid
        ///<summary>
        /// The <see cref="SceneItem"/>'s uniqueKey which requested the sound.
        ///</summary>
        public Guid UniqueKey;
        ///<summary>
        /// The <see cref="Sounds"/> Enum to play.
        ///</summary>
        public Sounds Sound;
        ///<summary>
        /// An <see cref="AudioListener"/> instance; used for 3D sound positioning.
        ///</summary>
        public AudioListener Listener;
        ///<summary>
        /// An <see cref="AudioEmitter"/> instance; used for 3D sound positioning.
        ///</summary>
        public AudioEmitter Emitter;
        ///<summary>
        /// Is sound a reusable cue?
        ///</summary>
        public bool ReusableCue;

    }
}