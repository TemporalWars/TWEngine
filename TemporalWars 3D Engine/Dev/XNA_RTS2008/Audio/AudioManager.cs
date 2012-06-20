#region File Description
//-----------------------------------------------------------------------------
// AudioManager.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Audio;
using System.Threading;
using Microsoft.Xna.Framework;
using ParallelTasksComponent;
using PerfTimersComponent.Timers;
using PerfTimersComponent.Timers.Enums;
using TWEngine.Audio.Enums;
using TWEngine.Audio.Structs;
using TWEngine.SceneItems;

namespace TWEngine.Audio
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.Audio"/> namespace contains the classes
    /// which make up the audio component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
    
    /// <summary>
    /// The <see cref="AudioManager"/> class is responsible for storing sound requests made from within
    /// the game engine, and playing them.  It is design to eliminate potential GC (Garbage Collection) issues, by
    /// storing audio <see cref="Cue"/> into an internal dictionary, rather than disposing of the <see cref="Cue"/> each
    /// sound request.  Also tracks looping sounds, and performs the maintance for pumping the internal XNA audio engine.
    /// </summary>
    /// <remarks>
    /// This class inherits from the <see cref="ThreadProcessor{T}"/> abstract class.
    /// </remarks>
    public sealed class AudioManager : ThreadProcessor<SoundRequestItem>
    {
        private static volatile GameTime _gameTime;
        private static readonly Stopwatch TimerToSleep = new Stopwatch();
        private static readonly TimeSpan TimerSleepMax = new TimeSpan(0, 0, 0, 0, 20);

        private static AudioEngine _engine;
        private static WaveBank _explosionsWaveBank;
        private static WaveBank _sciFiWeaponsWaveBank;
        private static WaveBank _ambientWaveBank;
        private static WaveBank _ambientMusicWaveBank; // 2/7/2011
        private static WaveBank _interfaceWaveBank; // 5/6/2009

// ReSharper disable UnaccessedField.Local
        private static WaveBank _mechanicalWaveBank; // 5/6/2009
// ReSharper restore UnaccessedField.Local
        private static SoundBank _explosionsSoundBank;
        private static SoundBank _sciFiWeaponsSoundBank;
        private static SoundBank _ambientSoundBank;
        private static SoundBank _ambientMusicSoundBank; // 2/7/2011
        private static SoundBank _interfaceSoundBank; // 5/6/2009
        private static SoundBank _mechanicalSoundBank; // 5/6/2009

        // 5/2/2009 - Store Cue's into Dictionary, to avoid re-instantiating new Cues!       
        //            Key = Int, which will be the 'SceneItemNumber'.
        // 6/10/2012  CHANGED: Key = GUID, which will be the 'UniqueKey'.
        // 5/13/2009 - Store 2nd Dictionary with 1st, to store each 'Sound' type for the given 'SceneItemNumber'.
        private static readonly Dictionary<Guid, Dictionary<int, Cue>> Cues = new Dictionary<Guid, Dictionary<int, Cue>>(100);

        // 6/10/2012 - Store cue names to the SoundEnum Int as key.
        private static readonly Dictionary<int, string> CueNamesLookup = new Dictionary<int, string>(100); 

        // 4/5/2009 - Did _engine init?
        private static volatile bool _soundEngineInitialized; // 2/7/2011 - Add volatile

        // 3/27/2009 - Category; used to change volume
#pragma warning disable 169
        private static AudioCategory _defaultCategory;
#pragma warning restore 169

        // 6/10/2012 - Struct to pair Sound names with number range.
        private struct SoundToNumber
        {
            public int SoundBankNumberRange;
            public string CueName;
        }

        // 6/10/2012
        private static readonly SoundToNumber[] CueNames = new SoundToNumber[]
                                                           {
                                                               #region AmibentMusic

                                                               new SoundToNumber {CueName = "ComeAndPlay", SoundBankNumberRange = 0},
                                                               new SoundToNumber {CueName = "KickItBack", SoundBankNumberRange = 1},
                                                               new SoundToNumber {CueName = "LetterOfIntent", SoundBankNumberRange = 2},
                                                               new SoundToNumber {CueName = "Mystique", SoundBankNumberRange = 3},
                                                               new SoundToNumber {CueName = "RiverFlow", SoundBankNumberRange = 4},
                                                               #endregion

                                                               #region Ambient

                                                               new SoundToNumber {CueName = "Birds_Creepy", SoundBankNumberRange = 100},
                                                               new SoundToNumber {CueName = "Birds_Crows", SoundBankNumberRange = 101},
                                                               new SoundToNumber {CueName = "Birds_Desert", SoundBankNumberRange = 102},
                                                               new SoundToNumber {CueName = "Birds_Falcons", SoundBankNumberRange = 103},
                                                               new SoundToNumber {CueName = "Birds_Mountain", SoundBankNumberRange = 104},
                                                               new SoundToNumber {CueName = "Birds_Owls", SoundBankNumberRange = 105},
                                                               new SoundToNumber {CueName = "Birds_Roosters", SoundBankNumberRange = 106},
                                                               new SoundToNumber {CueName = "Birds_Seagulls", SoundBankNumberRange = 107},
                                                               new SoundToNumber {CueName = "Birds_Typical", SoundBankNumberRange = 108},
                                                               new SoundToNumber {CueName = "Crick_Group", SoundBankNumberRange = 109},
                                                               new SoundToNumber {CueName = "Crick_Single", SoundBankNumberRange = 110},
                                                               new SoundToNumber {CueName = "Critters_Desert", SoundBankNumberRange = 111},
                                                               new SoundToNumber {CueName = "Rain1", SoundBankNumberRange = 112},
                                                               new SoundToNumber {CueName = "Rain2", SoundBankNumberRange = 113},
                                                               new SoundToNumber {CueName = "Rain3", SoundBankNumberRange = 114},
                                                               new SoundToNumber {CueName = "Rain4", SoundBankNumberRange = 115},
                                                               new SoundToNumber {CueName = "WF_Selected", SoundBankNumberRange = 116},
                                                               new SoundToNumber {CueName = "Wind_ColdGroup", SoundBankNumberRange = 117},
                                                               new SoundToNumber {CueName = "Wind_GrassGroup", SoundBankNumberRange = 118},
                                                               new SoundToNumber {CueName = "Wind_MtnGroup", SoundBankNumberRange = 119},
                                                               
                                                               #endregion
                                                               
                                                               #region Explosion

                                                               new SoundToNumber {CueName = "Exp_BomberGroup", SoundBankNumberRange = 200},
                                                               new SoundToNumber {CueName = "Exp_C4Group", SoundBankNumberRange = 201},
                                                               new SoundToNumber {CueName = "Exp_Harsh_Big1", SoundBankNumberRange = 202},
                                                               new SoundToNumber {CueName = "Exp_Harsh_Big2", SoundBankNumberRange = 203},
                                                               new SoundToNumber {CueName = "Exp_Harsh_Big3", SoundBankNumberRange = 204},
                                                               new SoundToNumber {CueName = "Exp_Harsh_Big4", SoundBankNumberRange = 205},
                                                               new SoundToNumber {CueName = "Exp_Harsh_Big5", SoundBankNumberRange = 206},
                                                               new SoundToNumber {CueName = "Exp_Harsh_Big6", SoundBankNumberRange = 207},
                                                               new SoundToNumber {CueName = "Exp_Harsh_Small1", SoundBankNumberRange = 208},
                                                               new SoundToNumber {CueName = "Exp_Harsh_Small2", SoundBankNumberRange = 209},
                                                               new SoundToNumber {CueName = "Exp_Harsh_Small3", SoundBankNumberRange = 210},
                                                               new SoundToNumber {CueName = "Exp_Harsh_Small4", SoundBankNumberRange = 211},
                                                               new SoundToNumber {CueName = "Exp_Harsh_Small5", SoundBankNumberRange = 212},
                                                               new SoundToNumber {CueName = "Exp_Medium1", SoundBankNumberRange = 213},
                                                               new SoundToNumber {CueName = "Exp_Medium2", SoundBankNumberRange = 214},
                                                               new SoundToNumber {CueName = "Exp_Medium3", SoundBankNumberRange = 215},
                                                               new SoundToNumber {CueName = "Exp_Medium4", SoundBankNumberRange = 216},
                                                               new SoundToNumber {CueName = "Exp_Medium5", SoundBankNumberRange = 217},
                                                               new SoundToNumber {CueName = "Exp_Medium6", SoundBankNumberRange = 218},
                                                               new SoundToNumber {CueName = "Exp_Medium7", SoundBankNumberRange = 219},
                                                               new SoundToNumber {CueName = "Exp_Medium8", SoundBankNumberRange = 220},
                                                               new SoundToNumber {CueName = "Exp_Medium9", SoundBankNumberRange = 221},
                                                               new SoundToNumber {CueName = "Exp_Medium10", SoundBankNumberRange = 222},
                                                               new SoundToNumber {CueName = "Exp_Medium11", SoundBankNumberRange = 223},
                                                               new SoundToNumber {CueName = "Exp_Medium12", SoundBankNumberRange = 224},
                                                               new SoundToNumber {CueName = "Exp_Medium13", SoundBankNumberRange = 225},
                                                               new SoundToNumber {CueName = "Exp_MediumGroup", SoundBankNumberRange = 226},
                                                               new SoundToNumber {CueName = "Exp_RocketGroup", SoundBankNumberRange = 227},
                                                               new SoundToNumber {CueName = "Exp_Smooth1", SoundBankNumberRange = 228},
                                                               new SoundToNumber {CueName = "Exp_Smooth2", SoundBankNumberRange = 229},
                                                                new SoundToNumber {CueName = "Exp_Smooth3", SoundBankNumberRange = 230},
                                                               new SoundToNumber {CueName = "Exp_Smooth4", SoundBankNumberRange = 231},
                                                               new SoundToNumber {CueName = "Exp_Smooth5", SoundBankNumberRange = 232},
                                                               new SoundToNumber {CueName = "Exp_Smooth6", SoundBankNumberRange = 233},
                                                               new SoundToNumber {CueName = "Exp_SmoothGroup", SoundBankNumberRange = 234},
                                                               
                                                               #endregion

                                                               #region Interface

                                                               new SoundToNumber {CueName = "Cash_Down", SoundBankNumberRange = 301},
                                                               new SoundToNumber {CueName = "Cash_Up", SoundBankNumberRange = 302},
                                                               new SoundToNumber {CueName = "Menu_Click", SoundBankNumberRange = 303},

                                                               #endregion

                                                               #region Mechanical

                                                               new SoundToNumber {CueName = "ChopperIdleLoop1", SoundBankNumberRange = 401},
                                                               new SoundToNumber {CueName = "ChopperIdleLoop2", SoundBankNumberRange = 402},
                                                               new SoundToNumber {CueName = "ChopperIdleLoop3", SoundBankNumberRange = 403},
                                                               new SoundToNumber {CueName = "TankMove", SoundBankNumberRange = 404},

                                                               #endregion          

                                                               #region SciFiWeapons

                                                               new SoundToNumber {CueName = "Cannon1", SoundBankNumberRange = 500},
                                                               new SoundToNumber {CueName = "Cannon2", SoundBankNumberRange = 501},
                                                               new SoundToNumber {CueName = "Cannon3", SoundBankNumberRange = 502},
                                                               new SoundToNumber {CueName = "ElectroGun1", SoundBankNumberRange = 503},
                                                               new SoundToNumber {CueName = "ElectroGun2", SoundBankNumberRange = 504},
                                                               new SoundToNumber {CueName = "ElectroGun3", SoundBankNumberRange = 505},
                                                               new SoundToNumber {CueName = "ElectroGun4", SoundBankNumberRange = 506},
                                                               new SoundToNumber {CueName = "ElectroGun5", SoundBankNumberRange = 507},
                                                               new SoundToNumber {CueName = "ElectroGun6", SoundBankNumberRange = 508},
                                                               new SoundToNumber {CueName = "GuardGun_Group", SoundBankNumberRange = 509},
                                                               new SoundToNumber {CueName = "GunshotWReload_Group", SoundBankNumberRange = 510},
                                                               new SoundToNumber {CueName = "Laser1", SoundBankNumberRange = 511},
                                                               new SoundToNumber {CueName = "Laser2", SoundBankNumberRange = 512},
                                                               new SoundToNumber {CueName = "Laser3", SoundBankNumberRange = 513},
                                                               new SoundToNumber {CueName = "Laser4", SoundBankNumberRange = 514},
                                                               new SoundToNumber {CueName = "Laser5", SoundBankNumberRange = 515},
                                                               new SoundToNumber {CueName = "Laser6", SoundBankNumberRange = 516},
                                                               new SoundToNumber {CueName = "LaserMissle_Group", SoundBankNumberRange = 517},
                                                               new SoundToNumber {CueName = "MachineGunA_Group", SoundBankNumberRange = 518},
                                                               new SoundToNumber {CueName = "MachineGunB_Group", SoundBankNumberRange = 519},
                                                               new SoundToNumber {CueName = "MachineGunC", SoundBankNumberRange = 520},
                                                               new SoundToNumber {CueName = "PowerDown1", SoundBankNumberRange = 521},
                                                               new SoundToNumber {CueName = "PowerDown2", SoundBankNumberRange = 522},
                                                               new SoundToNumber {CueName = "PowerDown3", SoundBankNumberRange = 523},
                                                               new SoundToNumber {CueName = "PowerDown4", SoundBankNumberRange = 524},
                                                               new SoundToNumber {CueName = "PowerUp1", SoundBankNumberRange = 525},
                                                               new SoundToNumber {CueName = "PowerUp2", SoundBankNumberRange = 526},
                                                               new SoundToNumber {CueName = "PowerUp3", SoundBankNumberRange = 527},
                                                               new SoundToNumber {CueName = "PowerUp4", SoundBankNumberRange = 528},
                                                               new SoundToNumber {CueName = "PulseGun1", SoundBankNumberRange = 529},
                                                               new SoundToNumber {CueName = "PulseGun2", SoundBankNumberRange = 530},
                                                               new SoundToNumber {CueName = "PulseGun3", SoundBankNumberRange = 531},
                                                               new SoundToNumber {CueName = "PulseGun4", SoundBankNumberRange = 532},
                                                               new SoundToNumber {CueName = "PulseGun4a", SoundBankNumberRange = 533},
                                                               new SoundToNumber {CueName = "PulseGun5", SoundBankNumberRange = 534},
                                                               new SoundToNumber {CueName = "RocketFire_Group", SoundBankNumberRange = 535},
                                                               new SoundToNumber {CueName = "RocketMissle_Group", SoundBankNumberRange = 536},
                                                               new SoundToNumber {CueName = "SuperGun1", SoundBankNumberRange = 537},
                                                               new SoundToNumber {CueName = "SuperGun2", SoundBankNumberRange = 538},
                                                               new SoundToNumber {CueName = "WeaponReload", SoundBankNumberRange = 539},
                                                               
                                                               #endregion
                                                           };
       

        #region Events

        // 2/7/2011
        public static event EventHandler AudioManagerReady;

        #endregion

        // 2/17/2010: Updated to use the new 'AutoBlocking' type.
        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        public AudioManager(Game game)
            : base(game, "SoundManager Thread", 5, ThreadMethodTypeEnum.AutoBlocking)
        {
            // Init Loop Sound List.
            _loopInfinteSoundList = new List<LoopSongData>(5);

            // 6/10/2012 - Build Sound names to Sound range numbers.
            BuildCueNamesToSoundEnumLookup();
        }

        // 6/10/2012
        /// <summary>
        /// Builds the CueNames lookup dictionary with the 'Sound' enum number ranger to name lookup.
        /// </summary>
        private void BuildCueNamesToSoundEnumLookup()
        {
            var length = CueNames.Length;
            for (var i = 0; i < length; i++)
            {
                var soundToNumber = CueNames[i];

                // add to dictionary 
                CueNamesLookup.Add(soundToNumber.SoundBankNumberRange, soundToNumber.CueName);
            }
        }

        // 6/10/2012 - Removed parameter 'soundBank'
        /// <summary>
        /// Plays a <see cref="Sounds"/> from some given <see cref="SoundBankGroup"/>.
        /// </summary>
        /// <param name="uniqueKey">The <see cref="SceneItem"/>'s uniqueKey.</param>
        /// <param name="sound"><see cref="Sounds"/> to play</param>
        public static void Play(Guid uniqueKey, Sounds sound)
        {
            try // 3/27/2011
            {
                // 4/5/2009 - SoundEngine init?
                if (!_soundEngineInitialized) return;

                // 8/13/2009 - Cache
                var cues = Cues;
                var soundNumberKey = (int) sound;

                // 5/2/2009 - Check if SceneItemNumber is in Dictionary  
                Dictionary<int, Cue> sounds;
                if (cues.TryGetValue(uniqueKey, out sounds))
                {
                    // 5/13/2009 - Now retrieve Sound cue, from 2nd Dictionary
                    if (!sounds.ContainsKey(soundNumberKey))
                    {
                        Cue newCue;
                        if (GetCueFromSoundBank(sound, out newCue))
                        {
                            // Add cue to Dictionary
                            sounds.Add(soundNumberKey, newCue);
                        }
                    }
                    else
                    {
                        Cue cue;
                        if (GetCueFromSoundBank(sound, out cue))
                        {
                            // Update new instance of cue to Dictionary
                            sounds[soundNumberKey] = cue;
                        }

                    }

                }
                else // No SceneItemNumber, so add to Dictionary.
                {

                    // 1st - Init a new Sounds Dictionary
                    sounds = new Dictionary<int, Cue>(10);

                    // Get cue from SoundBank, and add to Dictionary.
                    Cue cue;
                    if (GetCueFromSoundBank(sound, out cue))
                    {
                        sounds.Add(soundNumberKey, cue);
                    }

                    // 2nd - Add new Sound Dictionary to primary Dictionary
                    cues.Add(uniqueKey, sounds);

                }

                // 8/13/2009
                var cue1 = cues[uniqueKey][soundNumberKey];
                try
                {
                    cue1.Play();
                }
                catch (InstancePlayLimitException)
                {

                    if (cue1.IsPlaying)
                        cue1.Stop(AudioStopOptions.AsAuthored);

                    Debug.WriteLine("Method Error: Play Method threw InstancePlayLimitExpception.");

                    Toggle(uniqueKey, sound);
                }

            }
            catch (Exception err)
            {
                if (err.InnerException != null)
                    Debug.WriteLine("Method Error: Play: {0}", err.InnerException.Message);
                else
                    Debug.WriteLine("Method Error: Play.");
            }

        }

        // 8/15/2009; // 6/10/2012 - Removed parameter 'soundBank'
        /// <summary>
        /// Adds a 3D <see cref="Sounds"/> request to the internal list, to be played in the next batch.
        /// </summary>
        /// <param name="uniqueKey">The <see cref="SceneItem"/>'s uniqueKey.</param>
        /// <param name="sound"><see cref="Sounds"/> to play</param>
        /// <param name="listener"><see cref="AudioListener"/> instance</param>
        /// <param name="emitter"><see cref="AudioEmitter"/> instance</param>
        /// <param name="reusableCue">is reusable <see cref="Cue"/>?</param>
        public static void Play3D(Guid uniqueKey, Sounds sound, AudioListener listener, AudioEmitter emitter, bool reusableCue)
        {
            try // 3/27/2011
            {
                // Create new SoundItem request
                var soundRequestItem = new SoundRequestItem
                                           {
                                               UniqueKey = uniqueKey,
                                               //SoundBank = soundBank,
                                               Sound = sound,
                                               Listener = listener,
                                               Emitter = emitter,
                                               ReusableCue = reusableCue

                                           };

                // Add Request to list
                //ItemRequests.Add(soundRequestItem);
                LocklessQueue.Enqueue(soundRequestItem);

                // 2/17/2010 - Need to manually wake-up the thread
                WakeUpThread();
            }
            catch (Exception err)
            {
                if (err.InnerException != null)
                    Debug.WriteLine("Method Error: Play3D: {0}", err.InnerException.Message);
                else
                    Debug.WriteLine("Method Error: Play3D.");
            }
        }

        /// <summary>
        /// Plays a 3D Sound, with a given <see cref="AudioListener"/> and <see cref="AudioEmitter"/>.
        /// </summary>
        /// <param name="soundRequestItem"><see cref="SoundRequestItem"/> struct</param>
        private static void Play3D(ref SoundRequestItem soundRequestItem)
        {
            try // 3/27/2011
            {
                // 4/5/2009 - SoundEngine init?
                if (!_soundEngineInitialized)
                    return;

                // 8/13/2009 - Cache
                var cues = Cues;
                var soundNumberKey = (int)soundRequestItem.Sound;


                // 5/2/2009 - Check if SceneItemNumber is in Dictionary  
                Dictionary<int, Cue> sounds;
                if (cues.TryGetValue(soundRequestItem.UniqueKey, out sounds))
                {
                    // 5/13/2009 - Now retrieve Sound cue, from 2nd Dictionary
                    if (!sounds.ContainsKey(soundNumberKey))
                    {
                        Cue newCue;
                        if (GetCueFromSoundBank(soundRequestItem.Sound, out newCue))
                        {
                            // Add cue to Dictionary
                            sounds.Add(soundNumberKey, newCue);
                        }
                    }
                    else if (soundRequestItem.ReusableCue)
                    {
                        // then no need to get a new instance of cue!

                        // re-start playback, if paused.
                        var cue = cues[soundRequestItem.UniqueKey][soundNumberKey];
                        if (cue.IsPaused)
                        {
                            cue.Apply3D(soundRequestItem.Listener, soundRequestItem.Emitter);
                            cue.Resume();
                        }

                        return;
                    }
                    else
                    {
                        Cue cue;
                        if (GetCueFromSoundBank(soundRequestItem.Sound, out cue))
                        {
                            // Update new instance of cue to Dictionary
                            sounds[soundNumberKey] = cue;
                        }

                    }

                }
                else // No SceneItemNumber, so add to Dictionary.
                {

                    // 1st - Init a new Sounds Dictionary
                    sounds = new Dictionary<int, Cue>(10);

                    // Get cue from SoundBank, and add to Dictionary.
                    Cue cue;
                    if (GetCueFromSoundBank(soundRequestItem.Sound, out cue))
                    {
                        sounds.Add(soundNumberKey, cue);
                    }

                    // 2nd - Add new Sound Dictionary to primary Dictionary
                    cues.Add(soundRequestItem.UniqueKey, sounds);

                }

                // 8/13/2009 - Cache
                var cue1 = cues[soundRequestItem.UniqueKey][soundNumberKey];

                // 8/16/2009 : TEST
                //float numCueInstances = cue1.GetVariable("NumCueInstances");
                //if (numCueInstances > 5)
                //System.Diagnostics.Debugger.Break();

                try
                {
                    cue1.Apply3D(soundRequestItem.Listener, soundRequestItem.Emitter);
                    cue1.Play();
                }
                catch (InstancePlayLimitException)
                {

                    if (cue1.IsPlaying)
                        cue1.Stop(AudioStopOptions.AsAuthored);

                    Debug.WriteLine("Method Error: Play3D Method threw InstancePlayLimitExpception.");

                    Toggle(soundRequestItem.UniqueKey, soundRequestItem.Sound);
                }
                
            }
            catch (Exception err)
            {
                if (err.InnerException != null)
                    Debug.WriteLine("Method Error: Play3D: {0}", err.InnerException.Message);
                else
                    Debug.WriteLine("Method Error: Play3D.");
            }
           
        }

        // 6/10/2012 - Removed parameter 'soundBank'
        /// <summary>
        /// Plays a <see cref="Sounds"/> from some given <see cref="SoundBankGroup"/>.
        /// </summary>
        /// <param name="sound"><see cref="Sounds"/> to play</param>
        /// <returns>Nothing!  This cue will play through to completion and then free itself.</returns>
        public static void PlayCue(Sounds sound)
        {
            // 4/5/2009 - SoundEngine init?
            if (!_soundEngineInitialized) return;
            
            // 1/26/2009 - Add to capture any strange errors by _engine about Que Names?
            try
            {
                // 6/10/2012 - Get 'SoundBankGroup'
                var soundBank = GetSoundBankEnumForSoundEnum(sound);

                // 6/10/2012 - Get 'Cue' name
                var cueName = CueNamesLookup[(int) sound];

                // 1/8/2009
                switch (soundBank)
                {
                    // 2/7/2011
                    case SoundBankGroup.AmbientMusic:
                        if (_ambientMusicSoundBank != null)
                            _ambientMusicSoundBank.PlayCue(cueName);
                        break;
                    case SoundBankGroup.Ambient:
                        if (_ambientSoundBank != null)
                            _ambientSoundBank.PlayCue(cueName);
                        break;
                    case SoundBankGroup.Explosions:
                        if (_explosionsSoundBank != null)
                            _explosionsSoundBank.PlayCue(cueName);
                        break;
                    case SoundBankGroup.Interface:
                        if (_interfaceSoundBank != null)
                            _interfaceSoundBank.PlayCue(cueName);
                        break;
                    case SoundBankGroup.Mechanical:
                        if (_mechanicalSoundBank != null)
                            _mechanicalSoundBank.PlayCue(cueName);
                        break;
                    case SoundBankGroup.SciFiWeapons:
                        if (_sciFiWeaponsSoundBank != null)
                            _sciFiWeaponsSoundBank.PlayCue(cueName);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                if (err.InnerException != null)
                    Debug.WriteLine("Method Error: PlayCue: {0}", err.InnerException.Message);
                else
                    Debug.WriteLine("Method Error: PlayCue.");
            }
            
        }

        // 1/7/2009; // 6/10/2012 - Removed parameter 'soundBank'
        /// <summary>
        /// Plays a 3D Sound, with a given <see cref="AudioListener"/> and <see cref="AudioEmitter"/>.
        /// </summary>
        /// <param name="sound"><see cref="Sounds"/> to play</param>
        /// <param name="listener"><see cref="AudioListener"/> instance</param>
        /// <param name="emitter"><see cref="AudioEmitter"/> instance</param>
        /// <returns>Nothing!  This cue will play through to completion and then free itself.</returns>
        public static void PlayCue3D(Sounds sound, AudioListener listener, AudioEmitter emitter)
        {
            // 4/5/2009 - SoundEngine init?
            if (!_soundEngineInitialized) return;

            try // 3/27/2011
            {
                // 6/10/2012 - Get 'SoundBankGroup'
                var soundBank = GetSoundBankEnumForSoundEnum(sound);

                // 6/10/2012 - Get 'Cue' name
                var cueName = CueNamesLookup[(int)sound];

                // 1/8/2009
                switch (soundBank)
                {
                    // 2/7/2011
                    case SoundBankGroup.AmbientMusic:
                        if (_ambientMusicSoundBank != null)
                            _ambientMusicSoundBank.PlayCue(cueName, listener, emitter);
                        break;
                    case SoundBankGroup.Ambient:
                        if (_ambientSoundBank != null)
                            _ambientSoundBank.PlayCue(cueName, listener, emitter);
                        break;
                    case SoundBankGroup.Explosions:
                        if (_explosionsSoundBank != null)
                            _explosionsSoundBank.PlayCue(cueName, listener, emitter);
                        break;
                    case SoundBankGroup.Interface:
                        if (_interfaceSoundBank != null)
                            _interfaceSoundBank.PlayCue(cueName, listener, emitter);
                        break;
                    case SoundBankGroup.Mechanical:
                        if (_mechanicalSoundBank != null)
                            _mechanicalSoundBank.PlayCue(cueName, listener, emitter);
                        break;
                    case SoundBankGroup.SciFiWeapons:
                        if (_sciFiWeaponsSoundBank != null)
                            _sciFiWeaponsSoundBank.PlayCue(cueName, listener, emitter);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception err)
            {
                if (err.InnerException != null)
                    Debug.WriteLine("Method Error: PlayCue3D: {0}", err.InnerException.Message);
                else
                    Debug.WriteLine("Method Error: PlayCue3D.");
            }
            

        }

        // 5/6/2009
        /// <summary>
        /// Updates a given <see cref="Cue"/>, with new 3D <see cref="AudioListener"/> and <see cref="AudioEmitter"/> instances.
        /// </summary>
        /// <param name="uniqueKey">The <see cref="SceneItem"/>'s uniqueKey.</param>
        /// <param name="sound"><see cref="Sounds"/> to play</param>
        /// <param name="listener"><see cref="AudioListener"/> instance</param>
        /// <param name="emitter"><see cref="AudioEmitter"/> instance</param>
        public static void UpdateCues3DEmitters(Guid uniqueKey, Sounds sound, AudioListener listener, AudioEmitter emitter)
        {
            try
            {
                // check if in dictionary
                Dictionary<int, Cue> sounds;
                if (Cues.TryGetValue(uniqueKey, out sounds))
                {
                    // 6/10/2012
                    var soundNumberKey = (int) sound;

                    // TODO: The emitter has invalid positions of 'NaN', which causes InvalidOperationException from XNA Sound.
                    // apply new Settings
                    if (sounds.ContainsKey(soundNumberKey))
                        Cues[uniqueKey][soundNumberKey].Apply3D(listener, emitter);
                }
            }
            catch (Exception err)
            {
                if (err.InnerException != null)
                    Debug.WriteLine("Method Error: UpdateCues3DEmitters: {0}", err.InnerException.Message);
                else
                    Debug.WriteLine("Method Error: UpdateCues3DEmitters.");
            }
        }

        // 5/2/2009; // 6/10/2012 - Removed parameter 'soundBank'
        /// <summary>
        /// Helper function, which will return a <see cref="Cue"/>, from the specified <see cref="SoundBankGroup"/>.
        /// </summary>
        /// <param name="sound"><see cref="Sounds"/> to retrieve</param>
        /// <param name="soundBankCue">(OUT) <see cref="Cue"/> instance</param>
        /// <returns>true/false of result</returns>
        public static bool GetCueFromSoundBank(Sounds sound, out Cue soundBankCue)
        {
            try
            {
                soundBankCue = null;

                // 6/10/2012 - Get 'SoundBankGroup'
                var soundBank = GetSoundBankEnumForSoundEnum(sound);
                // 6/10/2012 - Get 'Cue' name
                var cueName = CueNamesLookup[(int)sound];

                switch (soundBank)
                {
                    // 2/7/2011
                    case SoundBankGroup.AmbientMusic:
                        if (_ambientMusicSoundBank != null)
                            soundBankCue = _ambientMusicSoundBank.GetCue(cueName);
                        break;
                    case SoundBankGroup.Ambient:
                        if (_ambientSoundBank != null)
                            soundBankCue = _ambientSoundBank.GetCue(cueName);
                        break;
                    case SoundBankGroup.Explosions:
                        if (_explosionsSoundBank != null)
                            soundBankCue = _explosionsSoundBank.GetCue(cueName);
                        break;
                    case SoundBankGroup.Interface:
                        if (_interfaceSoundBank != null)
                            soundBankCue = _interfaceSoundBank.GetCue(cueName);
                        break;
                    case SoundBankGroup.Mechanical:
                        if (_mechanicalSoundBank != null)
                            soundBankCue = _mechanicalSoundBank.GetCue(cueName);
                        break;
                    case SoundBankGroup.SciFiWeapons:
                        if (_sciFiWeaponsSoundBank != null)
                            soundBankCue = _sciFiWeaponsSoundBank.GetCue(cueName);
                        break;
                }

                return true;
            }
            catch (Exception err)
            {
                soundBankCue = null;

                if (err.InnerException != null)
                    Debug.WriteLine("Method Error: GetCueFromSoundBank: {0}", err.InnerException.Message);
                else
                    Debug.WriteLine("Method Error: GetCueFromSoundBank.");

                return false;
            }            

        }

        // 5/2/2009; // 6/10/2012 - Removed parameter 'soundBank'
        /// <summary>
        /// Restarts a <see cref="Cue"/>, and plays it.
        /// </summary>
        /// <param name="uniqueKey">The <see cref="SceneItem"/>'s uniqueKey.</param>
        /// <param name="sound"><see cref="Sounds"/> to play</param>
        private static void Toggle(Guid uniqueKey, Sounds sound)
        {
            try // 3/27/2011
            {
                // 5/13/2009
                Dictionary<int, Cue> sounds;
                if (Cues.TryGetValue(uniqueKey, out sounds))
                {
                    // 6/10/2012
                    var soundNumberKey = (int)sound;

                    Cue cue;
                    if (sounds.TryGetValue(soundNumberKey, out cue))
                    {
                        if (cue.IsPaused)
                        {
                            cue.Resume();
                        }
                        else if (cue.IsPlaying)
                        {
                            cue.Pause();
                        }
                        else // played but stopped
                        {
                            // need to re-get cue if stopped
                            Play(uniqueKey, sound);
                        }
                    }

                }
                else // never played, need to re-get cue
                    Play(uniqueKey, sound);
            }
            catch (Exception err)
            {
                if (err.InnerException != null)
                    Debug.WriteLine("Method Error: Toggle: {0}", err.InnerException.Message);
                else
                    Debug.WriteLine("Method Error: Toggle.");
                
            }            
            
        }

        // 5/5/2009
        /// <summary>
        /// Pauses a given <see cref="Cue"/>.
        /// </summary>
        /// <param name="uniqueKey">The <see cref="SceneItem"/>'s uniqueKey.</param>
        /// <param name="sound"><see cref="Sounds"/> to pause</param>
        public static void Pause(Guid uniqueKey, Sounds sound)
        {
            try
            {
                // 5/13/2009
                Dictionary<int, Cue> sounds;
                if (!Cues.TryGetValue(uniqueKey, out sounds)) return;

                // 6/10/2012
                var soundNumberKey = (int)sound;

                Cue cue;
                if (!sounds.TryGetValue(soundNumberKey, out cue)) return;

                if (cue.IsPlaying)
                    cue.Pause();
            }
            catch (Exception err)
            {
                if (err.InnerException != null)
                    Debug.WriteLine("Method Error: Pause: {0}", err.InnerException.Message);
                else
                    Debug.WriteLine("Method Error: Pause.");

            }       
            
        }

        // 5/5/2009
        /// <summary>
        /// Stops playing a given <see cref="Cue"/>.
        /// </summary>
        /// <param name="uniqueKey">The <see cref="SceneItem"/>'s uniqueKey.</param>
        /// <param name="sound"><see cref="Sounds"/> to stop</param>
        public static void Stop(Guid uniqueKey, Sounds sound)
        {
            try // 3/27/2011
            {
                // 5/13/2009
                Dictionary<int, Cue> sounds;
                if (!Cues.TryGetValue(uniqueKey, out sounds)) return;

                // 6/10/2012
                var soundNumberKey = (int)sound;

                Cue cue;
                if (!sounds.TryGetValue(soundNumberKey, out cue)) return;

                if (cue.IsPlaying)
                    cue.Stop(AudioStopOptions.Immediate);
            }
            catch (Exception err)
            {
                if (err.InnerException != null)
                    Debug.WriteLine("Method Error: Stop: {0}", err.InnerException.Message);
                else
                    Debug.WriteLine("Method Error: Stop.");
            }       
           
        }

        // 5/11/2009
        /// <summary>
        /// Removes a particular <see cref="Cue"/> from the internal Dictionary.
        /// </summary>
        /// <param name="uniqueKey">The <see cref="SceneItem"/>'s uniqueKey.</param>
        /// <param name="sound"><see cref="Sounds"/> to remove</param>
        public static void Remove(Guid uniqueKey, Sounds sound)
        {
            try
            {
                // 5/13/2009
                Dictionary<int, Cue> sounds;
                if (!Cues.TryGetValue(uniqueKey, out sounds)) return;

                // 6/10/2012
                var soundNumberKey = (int)sound;

                Cue cue;
                if (!sounds.TryGetValue(soundNumberKey, out cue)) return;

                if (cue.IsPlaying)
                    cue.Stop(AudioStopOptions.Immediate);

                sounds.Remove((int)sound);
            }
            catch (Exception err)
            {
                if (err.InnerException != null)
                    Debug.WriteLine("Method Error: Remove: {0}", err.InnerException.Message);
                else
                    Debug.WriteLine("Method Error: Remove.");
            }       
            
        }

        // 5/5/2009
        /// <summary>
        /// Stops playing all <see cref="Cue"/> instances.
        /// </summary>
        public static void StopAll()
        {
            try // 3/27/2011
            {
                // 5/13/2009
                // iterate though all entries in dictionary
                foreach (var sounds in Cues.Values)
                {
                    foreach (var cue in sounds)
                    {
                        cue.Value.Stop(AudioStopOptions.Immediate);
                    }
                }

                // Clear Loop sounds List
                _loopInfinteSoundList.Clear();
            }
            catch (Exception err)
            {
                if (err.InnerException != null)
                    Debug.WriteLine("Method Error: StopAll: {0}", err.InnerException.Message);
                else
                    Debug.WriteLine("Method Error: StopAll.");
            }      
           
        }

        // 5/5/2009
        /// <summary>
        /// Resumes a given <see cref="Cue"/>.
        /// </summary>
        /// <param name="uniqueKey">The <see cref="SceneItem"/>'s uniqueKey.</param>
        /// <param name="sound"><see cref="Sounds"/> to play</param>
// ReSharper disable UnusedMember.Global
        public static void Resume(Guid uniqueKey, Sounds sound)
// ReSharper restore UnusedMember.Global
        {
            try // 3/27/2011
            {
                // 5/13/2009
                Dictionary<int, Cue> sounds;
                if (!Cues.TryGetValue(uniqueKey, out sounds)) return;

                // 6/10/2012
                var soundNumberKey = (int)sound;

                Cue cue;
                if (!sounds.TryGetValue(soundNumberKey, out cue)) return;

                if (cue.IsPlaying)
                    cue.Resume();
            }
            catch (Exception err)
            {
                if (err.InnerException != null)
                    Debug.WriteLine("Method Error: Resume: {0}", err.InnerException.Message);
                else
                    Debug.WriteLine("Method Error: Resume.");
            }      
            
        }

        #region Play Cues (Looped) with delay

        // 5/5/2009

        static List<LoopSongData> _loopInfinteSoundList;

        // 5/5/2009; // 6/10/2012 - Removed parameter 'soundBank'
        /// <summary>
        /// Tells <see cref="AudioManager"/> to play the given <see cref="Sounds"/> for an infinite loop, with the given
        /// <paramref name="delayBetweenPlays"/> before playing the next cycle of the <see cref="Sounds"/>.
        /// </summary>
        /// <param name="sound"><see cref="Sounds"/> to play</param>
        /// <param name="delayBetweenPlays">delay in seconds</param>
        public static void PlayCueLoopedWithConstantDelay(Sounds sound, int delayBetweenPlays)
        {
            if (_loopInfinteSoundList == null) return;

            // 6/10/2012 - Get 'SoundBankGroup'
            var soundBank = GetSoundBankEnumForSoundEnum(sound);

            try // 3/27/2011
            {
                // create LoopSongData Struct, and populate with info given.
                var loopSongData = new LoopSongData
                {
                    Sound = sound,
                    SoundBank = soundBank,
                    ResetDelayBetweenPlaysValue = delayBetweenPlays,
                    DelayBetweenPlays = TimeSpan.FromSeconds(delayBetweenPlays)
                };

                // add to internal list
                _loopInfinteSoundList.Add(loopSongData);
            }
            catch (Exception err)
            {
                if (err.InnerException != null)
                    Debug.WriteLine("Method Error: PlayCueLoopedWithConstantDelay: {0}", err.InnerException.Message);
                else
                    Debug.WriteLine("Method Error: PlayCueLoopedWithConstantDelay.");
            }    
            
        }
        

        // 5/5/2009
        /// <summary>
        /// Iterates the internal <see cref="_loopInfinteSoundList"/>, and plays any sounds where the Time
        /// delay has expired.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private static void UpdatePlayCueLoopedWithConstantDelay(GameTime gameTime)
        {
            if (_loopInfinteSoundList == null) return;

            try // 3/27/2011
            {
                // 8/13/2009  - Cache
                var count = _loopInfinteSoundList.Count;
                for (var i = 0; i < count; i++)
                {
                    // get current node
                    var loopSongData = _loopInfinteSoundList[i];

                    // reduce Time
                    loopSongData.DelayBetweenPlays -= gameTime.ElapsedGameTime;

                    // if Time expired, then play song and reset timer
                    if (loopSongData.DelayBetweenPlays <= TimeSpanZero)
                    {
                        // reset Time value
                        loopSongData.DelayBetweenPlays = TimeSpan.FromSeconds(loopSongData.ResetDelayBetweenPlaysValue);

                        // play given Sound
                        PlayCue(loopSongData.Sound);
                    }

                    // store value back into list
                    _loopInfinteSoundList[i] = loopSongData;

                } // end For Loop
            }
            catch (Exception err)
            {
                if (err.InnerException != null)
                    Debug.WriteLine("Method Error: UpdatePlayCueLoopedWithConstantDelay: {0}", err.InnerException.Message);
                else
                    Debug.WriteLine("Method Error: UpdatePlayCueLoopedWithConstantDelay.");
            }    
           
        }

        #endregion
          
             

        /// <summary>
        /// Pumps the AudioEngine to help it clean itself up
        /// </summary>
        /// <param name="inGameTime">Instance of <see cref="GameTime"/>.</param>
        public override void Update(GameTime inGameTime)
        {
            // 7/22/2009 - store GameTime, to be used by thread.
            _gameTime = inGameTime;
            
            base.Update(inGameTime);
        }

        /// <summary>
        /// Stops a previously playing cue
        /// </summary>
        /// <param name="cue">The cue to stop that you got returned from Play(Sound)</param>
        public static void Stop(Cue cue)
        {
            try // 3/27/2011
            {
                cue.Stop(AudioStopOptions.Immediate);
            }
            catch (Exception err)
            {
                if (err.InnerException != null)
                    Debug.WriteLine("Method Error: Stop: {0}", err.InnerException.Message);
                else
                    Debug.WriteLine("Method Error: Stop.");
            }    
           
        }

        // 4/21/2009 - Thread members
        private static Thread _loadSoundEngineThread;

        /// <summary>
        /// Starts up the Sound code
        /// </summary>
        public override void Initialize()
        {
            try // 3/27/2011
            {
                // 4/21/2009 - Start Thread
                _loadSoundEngineThread = new Thread(InitalizeSoundEngineThreadMethod)
                {
                    Name = "Init Sound-Engine Thread",
                    IsBackground = true
                };
                _loadSoundEngineThread.Start();
            }
            catch (Exception err)
            {
                if (err.InnerException != null)
                    Debug.WriteLine("Method Error: Initialize: {0}", err.InnerException.Message);
                else
                    Debug.WriteLine("Method Error: Initialize.");
            } 
           
        }

        // 4/21/2009
        private static void InitalizeSoundEngineThreadMethod()
        {
            // Set XBOX-360 CPU Core for thread            
#if XBOX360
            Thread.CurrentThread.SetProcessorAffinity(3);            
#endif

            //
            // BEN: This statement below creates the 2509 instances of Boxing.
            //
            // Init Sound Engine.
            try
            {
                // 4/6/2010 - Set to use 'ContentAudioLoc' global var.
                var audioContentPath = TemporalWars3DEngine.ContentAudioLoc;

                _engine = new AudioEngine(audioContentPath + @"\XNA2008_ben.xgs");
                _soundEngineInitialized = true;

                // Load Explosions Sounds
                _explosionsWaveBank = new WaveBank(_engine, audioContentPath + @"\ExplosionsWaveBank.xwb");
                _explosionsSoundBank = new SoundBank(_engine, audioContentPath + @"\ExplosionsSoundBank.xsb");
                // Load SciFiWeapons Sounds
                _sciFiWeaponsWaveBank = new WaveBank(_engine, audioContentPath + @"\SciFiWeaponsWaveBank.xwb");
                _sciFiWeaponsSoundBank = new SoundBank(_engine, audioContentPath + @"\SciFiWeaponsSoundBank.xsb");
                // Load Ambient Sounds
                _ambientWaveBank = new WaveBank(_engine, audioContentPath + @"\AmbientWaveBank.xwb");
                _ambientSoundBank = new SoundBank(_engine, audioContentPath + @"\AmbientSoundBank.xsb");
                // 2/7/2011 - Load Ambient Music Sounds
                _ambientMusicWaveBank = new WaveBank(_engine, audioContentPath + @"\AmbientMusicWaveBank.xwb");
                _ambientMusicSoundBank = new SoundBank(_engine, audioContentPath + @"\AmbientMusicSoundBank.xsb");
                // 5/6/2009 - Load Interface Sounds
                _interfaceWaveBank = new WaveBank(_engine, audioContentPath + @"\InterfaceWaveBank.xwb");
                _interfaceSoundBank = new SoundBank(_engine, audioContentPath + @"\InterfaceSoundBank.xsb");
                // 5/6/2009 - Load Mechanical Sounds
                _mechanicalWaveBank = new WaveBank(_engine, audioContentPath + @"\MechanicalWaveBank.xwb");
                _mechanicalSoundBank = new SoundBank(_engine, audioContentPath + @"\MechanicalSoundBank.xsb");


                // 3/27/2009 - Get the category.
                //_defaultCategory = _engine.GetCategory("default");
                //_defaultCategory.SetVolume(0.5f);  

                // 2/7/2011
                if (AudioManagerReady != null)
                    AudioManagerReady(null, EventArgs.Empty);

            }
            catch (InvalidOperationException)
            {
                // Thrown when AudioEngine can't be created.
                _soundEngineInitialized = false;
            }
            catch (Exception err)
            {
                if (err.InnerException != null)
                    Debug.WriteLine("Method Error: InitalizeSoundEngineThreadMethod: {0}", err.InnerException.Message);
                else
                    Debug.WriteLine("Method Error: InitalizeSoundEngineThreadMethod.");
            }

            try // 3/27/2011
            {
                Thread.CurrentThread.Abort();
            }
            catch (ThreadAbortException)
            {
                // Valid Thread ABort - Do Nothing.
            }

        }

        // 8/21/2009
        /// <summary>
        /// Main SoundManager Method for Thread.
        /// </summary>
        protected override void ProcessorThreadDelegateMethod()
        {
            try
            {
                // 5/29/2012 - Skip if game paused.
                if (TemporalWars3DEngine.GamePaused)
                    return;

                // Start StopWatch timer
                TimerToSleep.Reset();
                TimerToSleep.Start();

                // Process current batch of _soundRequests
                SoundRequestItem soundRequestItem;
                while (LocklessQueue.TryDequeue(out soundRequestItem))
                {
                    // Play the sound
                    Play3D(ref soundRequestItem);

                    // Sleep every few ms.
                    if (TimerToSleep.Elapsed.TotalMilliseconds < TimerSleepMax.Milliseconds) continue;

                    Thread.Sleep(1);
                    TimerToSleep.Reset();
                    TimerToSleep.Start();
                }
                //soundRequestItems.Clear();

#if DEBUG
                StopWatchTimers.StartStopWatchInstance(StopWatchName.SoundManagerUpdate);//"SoundManager_Update"
#endif

                // SoundEngine init?
                if (_soundEngineInitialized)
                {
                    // Update the List of Sound to play with constant delay.
                    UpdatePlayCueLoopedWithConstantDelay(_gameTime);

                    _engine.Update();
                }
#if DEBUG
                StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.SoundManagerUpdate);//"SoundManager_Update"
#endif
            }
            catch (Exception err)
            {
                if (err.InnerException != null)
                    Debug.WriteLine("Method Error: ProcessorThreadDelegateMethod: {0}", err.InnerException.Message);
                else
                    Debug.WriteLine("Method Error: ProcessorThreadDelegateMethod.");
            } 
           
        }

        // 6/10/2012
        /// <summary>
        /// Gets the proper <see cref="SoundBankGroup"/> for the given <paramref name="sound"/>.
        /// </summary>
        private static SoundBankGroup GetSoundBankEnumForSoundEnum(Sounds sound)
        {
            var soundInt = (int) sound;

            //
            // return proper soundBankGroup depending on sound number range
            //

            if (soundInt >= 0 && soundInt < 100)
            {
                return SoundBankGroup.AmbientMusic;
            }

            if (soundInt >= 100 && soundInt < 200)
            {
                return SoundBankGroup.Ambient;
            }

            if (soundInt >= 200 && soundInt < 300)
            {
                return SoundBankGroup.Explosions;
            }

            if (soundInt >= 300 && soundInt < 400)
            {
                return SoundBankGroup.Interface;
            }

            if (soundInt >= 400 && soundInt < 500)
            {
                return SoundBankGroup.Mechanical;
            }

            if (soundInt >= 500 && soundInt < 600)
            {
                return SoundBankGroup.SciFiWeapons;
            }

            throw new ArgumentOutOfRangeException("sound", "Enumeration sound given is not the proper range!");
        }

        #region Dispose

        /// <summary>
        /// Releases the unmanaged resources used by the GameComponent and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            // dispose of resources
            if (_explosionsSoundBank != null)
                _explosionsSoundBank.Dispose();

            if (_explosionsWaveBank != null)
                _explosionsWaveBank.Dispose();

            if (_sciFiWeaponsSoundBank != null)
                _sciFiWeaponsSoundBank.Dispose();

            if (_sciFiWeaponsWaveBank != null)
                _sciFiWeaponsWaveBank.Dispose();

            if (_ambientSoundBank != null)
                _ambientSoundBank.Dispose();

            if (_ambientWaveBank != null)
                _ambientWaveBank.Dispose();

            // 2/7/2011
            if (_ambientMusicWaveBank != null)
                _ambientMusicWaveBank.Dispose();

            if (_interfaceSoundBank != null)
                _interfaceSoundBank.Dispose();

            if (_interfaceWaveBank != null)
                _interfaceWaveBank.Dispose();

            if (_engine != null)
                _engine.Dispose();

            // null refs
            _explosionsSoundBank = null;
            _explosionsWaveBank = null;
            _sciFiWeaponsSoundBank = null;
            _sciFiWeaponsWaveBank = null;
            _ambientSoundBank = null;
            _ambientWaveBank = null;
            _ambientMusicWaveBank = null; // 2/7/2011
            _interfaceSoundBank = null;
            _interfaceWaveBank = null;

            base.Dispose(disposing);
        }

        #endregion
    }
}