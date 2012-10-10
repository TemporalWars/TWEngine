#region File Description
//-----------------------------------------------------------------------------
// ScenaryItemDataAudio.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using ImageNexus.BenScharbach.TWEngine.Audio;
using ImageNexus.BenScharbach.TWEngine.Audio.Enums;
using Microsoft.Xna.Framework.Audio;

namespace ImageNexus.BenScharbach.TWEngine.SceneItems.Structs
{
    // 6/10/2012
    ///<summary>
    /// The <see cref="ScenaryItemDataAudio"/> is used to store the audio attributes.
    ///</summary>
    internal struct ScenaryItemDataAudio
    {
        // stores the sounds to play.
        private readonly Queue<Sounds> _soundsToPlay;

        #region Properties

        /// <summary>
        /// Holds the picked index location for the <see cref="ScenaryItemData"/> structure.
        /// </summary>
        public int InstancedItemPickedIndex { get; private set; }

        // 6/10/2012
        /// <summary>
        /// <see cref="AudioEmitter"/> instance
        /// </summary>
        public AudioEmitter AudioEmitterI { get; private set; }

        // 6/10/2012
        /// <summary>
        /// <see cref="AudioListener"/> instance
        /// </summary>
        public AudioListener AudioListenerI { get; private set; }

        #endregion

        /// <summary>
        /// Initializes the internal
        /// </summary>
        /// <param name="index"></param>
        public ScenaryItemDataAudio(int index): this()
        {
            InstancedItemPickedIndex = index;
            AudioEmitterI = new AudioEmitter();
            AudioListenerI = new AudioListener();
            _soundsToPlay = new Queue<Sounds>();
        }

        /// <summary>
        /// Adds a <see cref="Sounds"/> to play.
        /// </summary>
        /// <param name="soundToPlay"></param>
        public void AddSoundToPlay(Sounds soundToPlay)
        {
            if (_soundsToPlay == null)
                return;

            _soundsToPlay.Enqueue(soundToPlay);
        }

        /// <summary>
        /// Checks the internal Queue for requested <see cref="Sounds"/> to play.
        /// </summary>
        /// <param name="uniqueKey"></param>
        public void CheckToPlaySound(Guid uniqueKey)
        {
            if (_soundsToPlay == null)
                return;

            if (_soundsToPlay.Count == 0)
                return;

            Sounds soundToPlay = _soundsToPlay.Dequeue();

            // Play3D sound
            AudioManager.Play3D(uniqueKey, soundToPlay, AudioListenerI, AudioEmitterI, false);
            // Now apply new emitter atts to cue.
            AudioManager.UpdateCues3DEmitters(uniqueKey, soundToPlay, AudioListenerI, AudioEmitterI);
        }
    }
}