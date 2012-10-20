#region File Description
//-----------------------------------------------------------------------------
// ParticlesManager.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.Explosions;
using ImageNexus.BenScharbach.TWEngine.GameCamera;
using ImageNexus.BenScharbach.TWEngine.Particles.Enums;
using ImageNexus.BenScharbach.TWEngine.Particles.ParticleSystems;
using ImageNexus.BenScharbach.TWEngine.Particles.Structs;
using ImageNexus.BenScharbach.TWTools.Particles3DComponentLibrary;
using ImageNexus.BenScharbach.TWTools.PerfTimersComponent.Timers;
using ImageNexus.BenScharbach.TWTools.PerfTimersComponent.Timers.Enums;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.Particles
{
    // 1/16/2010

    /// <summary>
    /// The <see cref="ParticlesManager"/> updates and draws all <see cref="ParticleSystem"/> types in the game engine.
    /// </summary>
    public class ParticlesManager : GameComponent
    {
        // 1/16/2010: Updated to be List<> inside List<>, where the outer List is indexed by the Enum (int) value.
        // 7/7/2009 - List of Particles to display; particles added using the 'Enum' as index.
        private static List<List<ParticleSystemItem>> _particlesToDisplay = new List<List<ParticleSystemItem>>(20);

        private static ExplosionParticleSystem _explosionParticles;
        private static SmallExplosionSmokeParticleSystem _smallExplosionSmokeParticles;
        private static MediumExplosionSmokeParticleSystem _mediumExplosionSmokeParticles; // 4/4/2009
        private static LargeExplosionSmokeParticleSystem _largeExplosionSmokeParticles; // 4/5/2009
        private static ProjectileTrailParticleSystem _projectileTrailParticles;
        private static SmokePlumeParticleSystem _smokePlumeParticles;       
        private static FireParticleSystem _fireParticles;
        private static RainParticleSystem _rainParticles; // 4/25/2009
        private static SnowParticleSystem _snowParticles; // 3/1/2011

        // 10/27/2008
        private static BallParticleSystemWhite _whiteBallParticles;
        private static BallParticleSystemRed _redBallParticles;
        private static BallParticleSystemBlue _blueBallParticles;
        private static BallParticleSystemOrange _orangeBallParticles; // 3/24/2009
        private static DustPlumeParticleSystem _dustParticles;
        private static TorchParticleSystem _torchParticles; // 4/10/2009
        private static TorchParticleSystem _gunshipEngineParticles; // 11/28/2009
        private static FlashParticleSystem _flashParticles; // 4/10/2009

        // Rain Emitter
        private static ParticleFrequencyEmitter _rainParticleFreqEmitter;
        private static readonly Vector3 Vector3Zero = Vector3.Zero;

        // Snow Emitter
        private static ParticleFrequencyEmitter _snowParticleFreqEmitter;

        // 11/13/2008 - Black Smoke Emitter
        private static SmokePlumeParticleSystem _blackSmokePlumeParticles;
        // 1/16/2010 - Blue Smoke Emitter
        private static SmokePlumeParticleSystem _blueSmokePlumeParticles;

        // 3/24/2011 - XNA 4.0 Updates - Ref to Particles3D component.
        private static Particle3DSampleGame _particles3DComponent;

        /// <summary>
        /// Constructor, which creates all <see cref="ParticleSystem"/>, and add to internal collection for processing.
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        public ParticlesManager(Game game)
            : base(game)
        {

            // 7/7/2009 - StopWatchTimers            
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.ParticleSystemUpdate, false); // "ParticleSystemUpdate"
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.ParticleSystemDraw, false); // "ParticleSystemDraw""

             // 3/24/2011 - XNA 4.0 Updates -Retrieve Particles3D component service
            _particles3DComponent = (Particle3DSampleGame)game.Services.GetService(typeof(Particle3DSampleGame));

             // Create ParticleSystem instances.
            _smokePlumeParticles = new SmokePlumeParticleSystem(game, game.Content);
            _smallExplosionSmokeParticles = new SmallExplosionSmokeParticleSystem(game, game.Content);
            _mediumExplosionSmokeParticles = new MediumExplosionSmokeParticleSystem(game, game.Content); // 4/4/2009
            _largeExplosionSmokeParticles = new LargeExplosionSmokeParticleSystem(game, game.Content); // 4/5/2009
            _projectileTrailParticles = new ProjectileTrailParticleSystem(game, game.Content);
            _explosionParticles = new ExplosionParticleSystem(game, game.Content);
            _fireParticles = new FireParticleSystem(game, game.Content);
            _whiteBallParticles = new BallParticleSystemWhite(game, game.Content);
            _redBallParticles = new BallParticleSystemRed(game, game.Content); // 12/27/2008
            _blueBallParticles = new BallParticleSystemBlue(game, game.Content); // 12/27/2008
            _orangeBallParticles = new BallParticleSystemOrange(game, game.Content); // 3/24/2009
            _dustParticles = new DustPlumeParticleSystem(game, game.Content);
            _blackSmokePlumeParticles = new SmokePlumeParticleSystem(game, game.Content);
            _torchParticles = new TorchParticleSystem(game, game.Content); // 4/10/2009
            _gunshipEngineParticles = new TorchParticleSystem(game, game.Content); // 11/28/2009
            _flashParticles = new FlashParticleSystem(game, game.Content); // 4/10/2009
            _rainParticles = new RainParticleSystem(game, game.Content); // 4/25/2009
            _snowParticles = new SnowParticleSystem(game, game.Content); // 3/1/2011
            _blueSmokePlumeParticles = new SmokePlumeParticleSystem(game, game.Content); // 1/16/2010

            // 3/24/2011 - XNA 4.0 Update - Add to draw collection
            _particles3DComponent.AddParticleSystemToDraw(_smokePlumeParticles);
            _particles3DComponent.AddParticleSystemToDraw(_smallExplosionSmokeParticles);
            _particles3DComponent.AddParticleSystemToDraw(_mediumExplosionSmokeParticles);
            _particles3DComponent.AddParticleSystemToDraw(_largeExplosionSmokeParticles);
            _particles3DComponent.AddParticleSystemToDraw(_projectileTrailParticles);
            _particles3DComponent.AddParticleSystemToDraw(_explosionParticles);
            _particles3DComponent.AddParticleSystemToDraw(_fireParticles);
            _particles3DComponent.AddParticleSystemToDraw(_whiteBallParticles);
            _particles3DComponent.AddParticleSystemToDraw(_redBallParticles);
            _particles3DComponent.AddParticleSystemToDraw(_blueBallParticles);
            _particles3DComponent.AddParticleSystemToDraw(_orangeBallParticles);
            _particles3DComponent.AddParticleSystemToDraw(_dustParticles);
            _particles3DComponent.AddParticleSystemToDraw(_blackSmokePlumeParticles);
            _particles3DComponent.AddParticleSystemToDraw(_torchParticles);
            _particles3DComponent.AddParticleSystemToDraw(_gunshipEngineParticles);
            _particles3DComponent.AddParticleSystemToDraw(_flashParticles);
            _particles3DComponent.AddParticleSystemToDraw(_rainParticles);
            _particles3DComponent.AddParticleSystemToDraw(_snowParticles);
            _particles3DComponent.AddParticleSystemToDraw(_blueSmokePlumeParticles);


            // 3/24/2011 - XNA 4.0 Updates - Set Draw Order
            _smokePlumeParticles.DrawOrder = 500;
            _smallExplosionSmokeParticles.DrawOrder = 501;
            _mediumExplosionSmokeParticles.DrawOrder = 502;
            _largeExplosionSmokeParticles.DrawOrder = 503;
            _projectileTrailParticles.DrawOrder = 504;
            _explosionParticles.DrawOrder = 505;
            _fireParticles.DrawOrder = 506;
            _whiteBallParticles.DrawOrder = 507;
            _redBallParticles.DrawOrder = 508;
            _blueBallParticles.DrawOrder = 509;
            _orangeBallParticles.DrawOrder = 510;
            _dustParticles.DrawOrder = 511;
            _blackSmokePlumeParticles.DrawOrder = 512;
            _torchParticles.DrawOrder = 513;
            _gunshipEngineParticles.DrawOrder = 514;
            _flashParticles.DrawOrder = 515;
            _rainParticles.DrawOrder = 516;
            _snowParticles.DrawOrder = 517;
            _blueSmokePlumeParticles.DrawOrder = 518;

            // 1/16/2010 - Set Min/Max colors to make black smoke!
            _blackSmokePlumeParticles.Settings.MinColor = Color.Green; // Color.DarkGray
            _blackSmokePlumeParticles.Settings.MaxColor = Color.DarkGreen; // Color.Black
            _blackSmokePlumeParticles.Settings.UseFrequency = true;
           
            // 11/28/2009 - Set some specific settings for the 'GunShipEngine' particles
            _gunshipEngineParticles.Settings.Gravity = new Vector3(0, 15, 0);
            _gunshipEngineParticles.Settings.MaxParticles = 1000;
            _gunshipEngineParticles.Settings.MinVerticalVelocity = -10;
            _gunshipEngineParticles.Settings.MaxVerticalVelocity = -35;
            _gunshipEngineParticles.Settings.Duration = TimeSpan.FromSeconds(1.25f);
           
            // 7/7/2009 - Create empty list of 19 positions, for the 19 particle systems
            //            This allows using the 'Insert' method below.
            for (var i = 0; i < 19; i++)
            {
                _particlesToDisplay.Add(new List<ParticleSystemItem>());
            }
            
            // 7/7/2009
            // Add particles to List<> for drawing.  
            AddNewParticleSystemItem(ParticleSystemTypes.SmokePlumeParticleSystem, _smokePlumeParticles);
            AddNewParticleSystemItem(ParticleSystemTypes.SmallExplosionSmokeParticleSystem, _smallExplosionSmokeParticles);
            AddNewParticleSystemItem(ParticleSystemTypes.MediumExplosionSmokeParticleSystem, _mediumExplosionSmokeParticles);
            AddNewParticleSystemItem(ParticleSystemTypes.LargeExplosionSmokeParticleSystem, _largeExplosionSmokeParticles);
            AddNewParticleSystemItem(ParticleSystemTypes.ProjectileTrailParticleSystem, _projectileTrailParticles);
            AddNewParticleSystemItem(ParticleSystemTypes.ExplosionParticleSystem, _explosionParticles);
            AddNewParticleSystemItem(ParticleSystemTypes.FireParticleSystem, _fireParticles);
            AddNewParticleSystemItem(ParticleSystemTypes.BallParticleSystemWhite, _whiteBallParticles);
            AddNewParticleSystemItem(ParticleSystemTypes.BallParticleSystemRed, _redBallParticles);
            AddNewParticleSystemItem(ParticleSystemTypes.BallParticleSystemBlue, _blueBallParticles);
            AddNewParticleSystemItem(ParticleSystemTypes.BallParticleSystemOrange, _orangeBallParticles);
            AddNewParticleSystemItem(ParticleSystemTypes.DustPlumeParticleSystem, _dustParticles);
            AddNewParticleSystemItem(ParticleSystemTypes.BlackSmokePlumeParticleSystem, _blackSmokePlumeParticles);
            AddNewParticleSystemItem(ParticleSystemTypes.BlueSmokePlumeParticleSystem, _blueSmokePlumeParticles);
            AddNewParticleSystemItem(ParticleSystemTypes.TorchParticleSystem, _torchParticles);
            AddNewParticleSystemItem(ParticleSystemTypes.GunshipParticleSystem, _gunshipEngineParticles);
            AddNewParticleSystemItem(ParticleSystemTypes.FlashParticleSystem, _flashParticles);
            AddNewParticleSystemItem(ParticleSystemTypes.RainParticleSystem, _rainParticles);
            AddNewParticleSystemItem(ParticleSystemTypes.SnowParticleSystem, _snowParticles); // 3/1/2011
           
        }  
      

        // 3/24/2011 - XNA 4.0 Updates
        public override void Initialize()
        {
            base.Initialize();

            // Create any emitters where necessary.
            var count = _particlesToDisplay.Count; // 6/2/2012
            for (var i = 0; i < count; i++)
            {
                // Cache
                var particleType = _particlesToDisplay[i];
                if (particleType == null) continue;

                var particleTypeCount = particleType.Count; // 6/2/2012
                for (var j = 0; j < particleTypeCount; j++)
                {
                    // Cache
                    var particleSystemItem = particleType[j];
                    var particleSettings = particleSystemItem.ParticleSystem.Settings;

                    if (!particleSettings.UseFrequency) continue;

                    particleSystemItem.ParticleEmitters =
                        new ParticleFrequencyEmitter(particleSystemItem.ParticleSystem,
                                                     particleSettings.
                                                         ParticlesPerFrequency,
                                                     particleSettings.Frequency);
                    // Store back into array
                    particleType[j] = particleSystemItem;
                }
            }

            // 4/27/2009 - Add Rain ParticleFrequencyEmitter, using the 2nd overload to use the Random Emitter Range.
            _rainParticleFreqEmitter = new ParticleFrequencyEmitter(_rainParticles, _rainParticles.Settings.EmitPerSecond, TimeSpan.Zero, _rainParticles.Settings.EmitPosition, _rainParticles.Settings.EmitRange);

            // 3/1/2011 - Add Snow PartcileFrequencyEmitter
            _snowParticleFreqEmitter = new ParticleFrequencyEmitter(_snowParticles, _snowParticles.Settings.EmitPerSecond, TimeSpan.Zero, _snowParticles.Settings.EmitPosition, _snowParticles.Settings.EmitRange);

        }

        /// <summary>
        /// Iterates the internal <see cref="ParticleSystem"/> collection, by calling the particles update method, and 
        /// removing particles which have expired.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public sealed override void Update(GameTime gameTime)
        {
            // 5/29/2012 - Enter Pause check here.
            if (TemporalWars3DEngine.GamePaused)
                return;
#if DEBUG
            StopWatchTimers.StartStopWatchInstance(StopWatchName.ParticleSystemUpdate);//"ParticleSystemUpdate"
#endif

            // Get View/Projection
            var view = Camera.View;
            var projection = Camera.Projection;

            // 7/7/2009 - Update all Particles in List.
            var count = _particlesToDisplay.Count; // 11/19/2009
            for (var i = 0; i < count; i++)
            {
                // 11/19/2009 - cache
                var particleSystemList = _particlesToDisplay[i];
                if (particleSystemList == null) continue;

                // 1/16/2010 - Iterate internal List
                var count1 = particleSystemList.Count;
                for (var j = 0; j < count1; j++)
                {
                    // cache
                    var particleSystemItem = particleSystemList[j];
                    if (particleSystemItem.ParticleSystem == null) continue;

                    // 3/24/2011 - XNA 4.0 Updates - Set Camera View/Projection
                    particleSystemItem.ParticleSystem.SetCamera(view, projection);

                    // Update ParticleSystem
                    particleSystemItem.ParticleSystem.Update(gameTime);
                    
                    // Check if Emitter attached.
                    if (!particleSystemItem.ParticleSystem.Settings.UseFrequency) continue;

                    // Check if Emitter is Null.
                    if (particleSystemItem.ParticleEmitters == null) continue;

                    // Call Update on Emitter
                    particleSystemItem.ParticleEmitters.Update(gameTime);

                    // Check if AutoEmit true?
                    if (particleSystemItem.AutoEmit)
                        particleSystemItem.ParticleEmitters.AddParticles();

                } // End ParticleSystem List
                
            } // End ParticleToDisplay List

            // 1/16/2010 - Update Rain emitter
            _rainParticleFreqEmitter.Update(gameTime);

            // 3/1/2011 - Update Snow emitter
            _snowParticleFreqEmitter.Update(gameTime);

#if DEBUG
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.ParticleSystemUpdate);//"ParticleSystemUpdate"
#endif

            base.Update(gameTime);
        }     
       

        // 1/16/2010
        /// <summary>
        /// Helper method, which updates for the given particle system.
        /// </summary>
        /// <param name="particleSystemType">Particle System to update</param>
        /// <param name="position">Position vector</param>
        /// <param name="velocity">Velocity vector</param>
        public static void UpdateParticleSystem(ParticleSystemTypes particleSystemType, ref Vector3 position, ref Vector3 velocity)
        {
            // Retrieve given 'ParticleSystem' to update.
            var particleSystemList = _particlesToDisplay[(int)particleSystemType];

            // iterate internal List of instances of this ParticleSystemType
            var count = particleSystemList.Count;
            for (var i = 0; i < count; i++)
            {
                // cache
                var particleSystemItem = particleSystemList[i];
                if (particleSystemItem.ParticleSystem == null) continue;

                // Update ParticleSystem
                if (particleSystemItem.ParticleSystem.Settings.UseFrequency)
                {
                    // Update 'Emitter' system.
                    particleSystemItem.ParticleEmitters.AddParticles(ref position, ref velocity);
                    return;
                }

                // Otherwise, just create one new particle per frame.
                particleSystemItem.ParticleSystem.AddParticle(position, velocity);

            } // End ParticleSystemList
                
        }

        // 1/16/2010
        /// <summary>
        /// Helper method, which adds the given <see cref="ParticleSystem"/> to the internal List. 
        /// </summary>
        /// <param name="particleSystemType"><see cref="ParticleSystemTypes"/> Enum</param>
        /// <param name="particleSystem"><see cref="ParticleSystem"/> instance</param>
        private static void AddNewParticleSystemItem(ParticleSystemTypes particleSystemType, ParticleSystem particleSystem)
        {
            // Create new ParticleSystemItem
            var particleSystemItem = new ParticleSystemItem
                                         {
                                             // Get current Instance Index for given type.
                                             InstanceKey = _particlesToDisplay[(int) particleSystemType].Count,
                                             AutoEmit = false,
                                             ParticleSystem = particleSystem,
                                             ParticleEmitters = null // (particleSystem.Settings.UseFrequency) ? new ParticleFrequencyEmitter(particleSystem, particleSystem.Settings.ParticlesPerFrequency, particleSystem.Settings.Frequency) : null,
                                         };
           

            // Add to List at given Enum position
            _particlesToDisplay[(int) particleSystemType].Add(particleSystemItem);
        }

        // 1/16/2010
        /// <summary>
        /// Helper method, used by outside classes, to request a new <see cref="ParticleSystem"/> instance.  The <see cref="ParticleSystemTypes"/>
        /// requested will be instantiated, and added to the internal list. 
        /// </summary>
        /// <param name="particleSystemType"><see cref="ParticleSystemTypes"/> Enum</param>
        /// <param name="particleSystem">Instance of <see cref="ParticleSystem"/></param>
        /// <param name="autoEmit">Allow <see cref="ParticleSystem"/> to automatically emit new particles</param>
        /// <param name="emitPosition">Auto-Emit Position for particles</param>
        /// <param name="emitVelocity">Auto-Emit Velocity for particles</param>
        /// <typeparam name="T">The <see cref="ParticleSystem"/> base-type instance</typeparam>
        /// <returns>InstanceKey</returns>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="autoEmit"/> is set to true, but <see cref="ParticleSettings.UseFrequency"/> is not.</exception>
        public static int AddNewParticleSystemItem(ParticleSystemTypes particleSystemType, ParticleSystem particleSystem, 
            bool autoEmit, ref Vector3 emitPosition, ref Vector3 emitVelocity) 
        {
            // 3/26/2011 - Add to Particle3D Component
            _particles3DComponent.AddParticleSystemToDraw(particleSystem);
            particleSystem.Initialize();
            particleSystem.DrawOrder = 500;

            // If Auto-Emit, verify the 'UseFrequency' is set to TRUE.
            if (!particleSystem.Settings.UseFrequency && autoEmit)
                throw new InvalidOperationException("Auto-Emit particle system requires the 'UseFrequency' be set to TRUE.");

            // Create new ParticleSystemItem
            var particleSystemItem = new ParticleSystemItem
            {
                // Get current Instance Index for given type.
                InstanceKey = _particlesToDisplay[(int)particleSystemType].Count,
                AutoEmit = autoEmit,
                ParticleSystem = particleSystem,
                ParticleEmitters = (particleSystem.Settings.UseFrequency) ? new ParticleFrequencyEmitter(particleSystem, particleSystem.Settings.ParticlesPerFrequency, particleSystem.Settings.Frequency) : null,
            };

            // If Auto-Emit, set Required Auto-Emit vars
            if (autoEmit)
                particleSystemItem.ParticleEmitters.SetAutoEmitVectors(emitPosition, emitVelocity);

            // Add to List at given Enum position
            _particlesToDisplay[(int)particleSystemType].Add(particleSystemItem);

            // return InstanceKey
            return particleSystemItem.InstanceKey;

        }

        // 6/29/2012
        /// <summary>
        /// Updates ALL internal particles to the current Draw 'Visibility' state.
        /// </summary>
        /// <param name="drawParticles">Draw Particles?</param>
        public static void SetParticlesToDraw(bool drawParticles)
        {
            if (_particlesToDisplay == null)
                return;

            // iterate particlesSystems
            var count = _particlesToDisplay.Count;
            for (var i = 0; i < count; i++)
            {
                // Retrieve ParticleSystem List<>.
                var particleSystemList = _particlesToDisplay[i];
                if (particleSystemList == null) continue;

                // iterate internal list
                var count1 = particleSystemList.Count;
                for (var j = 0; j < count1; j++)
                {
                    // Update Draw Visibility
                    var particleSystem = particleSystemList[j];

                    if (particleSystem.ParticleSystem != null) 
                        particleSystem.ParticleSystem.Visible = drawParticles;
                }
            }
        }

        // 1/16/2010
        /// <summary>
        /// Retrieves the <see cref="ParticleSystem"/> instance, for the given <paramref name="particleSystemType"/>.
        /// </summary>
        /// <remarks>
        /// The <paramref name="instanceKey"/> is used when multiple instances of the same <see cref="ParticleSystem"/> exist; 
        /// for example, if 3 instances of the <see cref="ParticleSystem"/> 'Smoke' exist, and you want the 2nd instance, 
        /// then pass in 1, since zero-based. 
        /// </remarks>
        /// <param name="particleSystemType"><see cref="ParticleSystemTypes"/> Enum to retrieve</param>
        /// <param name="instanceKey">InstanceKey</param>
        /// <param name="particleSystem">(OUT) <see cref="ParticleSystemTypes"/> instance</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="instanceKey"/> is not a valid index position key.</exception>
        public static void GetParticleSystem(ParticleSystemTypes particleSystemType, int instanceKey, out ParticleSystem particleSystem)
        {
            particleSystem = null;
 
            // Retrieve ParticleSystem from List using Enum as index.
            var particleSystemList = _particlesToDisplay[(int) particleSystemType];
            if (particleSystemList == null) return;

            // Check if given 'instanceKey' is valid.
            if (instanceKey + 1 > particleSystemList.Count)
                throw new ArgumentOutOfRangeException("instanceKey", @"Given Key index position is not valid!");

            // Retrieve item
            var particleSystemItem = particleSystemList[instanceKey];
            if (particleSystemItem.ParticleSystem == null) return;

            // otherwise, set OUT param to ParticleSystem instance.
            particleSystem = particleSystemItem.ParticleSystem;
        }

        // 6/2/2012
        /// <summary>
        /// Retrieves the <see cref="ParticleSystemItem"/> structure, for the given <paramref name="particleSystemType"/>.
        ///  </summary>
        /// <remarks>
        /// The <paramref name="instanceKey"/> is used when multiple instances of the same <see cref="ParticleSystem"/> exist; 
        /// for example, if 3 instances of the <see cref="ParticleSystem"/> 'Smoke' exist, and you want the 2nd instance, 
        /// then pass in 1, since zero-based. 
        /// </remarks>
        /// <param name="particleSystemType"><see cref="ParticleSystemTypes"/> Enum to retrieve</param>
        /// <param name="instanceKey">InstanceKey</param>
        /// <param name="particleSystemItem">(OUT) <see cref="ParticleSystemTypes"/>'s structure <see cref="ParticleSystemItem"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="instanceKey"/> is not a valid index position key.</exception>
        public static void GetParticleSystemItem(ParticleSystemTypes particleSystemType, int instanceKey, out ParticleSystemItem particleSystemItem)
        {
            particleSystemItem = default(ParticleSystemItem);

            // Retrieve ParticleSystem from List using Enum as index.
            var particleSystemList = _particlesToDisplay[(int)particleSystemType];
            if (particleSystemList == null) return;

            // Check if given 'instanceKey' is valid.
            if (instanceKey + 1 > particleSystemList.Count)
                throw new ArgumentOutOfRangeException("instanceKey", @"Given Key index position is not valid!");

            // Retrieve item
            particleSystemItem = particleSystemList[instanceKey];
        }

        // 1/16/2010
        /// <summary>
        /// Stores the <see cref="ParticleSystem"/> instance, for the given <paramref name="particleSystemType"/>.
        /// </summary>
        /// <remarks>
        /// The <paramref name="instanceKey"/> is used when multiple instances of the same <see cref="ParticleSystem"/> exist; 
        /// for example, if 3 instances of the <see cref="ParticleSystem"/> 'Smoke' exist, and you want the 2nd instance, 
        /// then pass in 1, since zero-based. 
        /// </remarks>
        /// <param name="particleSystemType"><see cref="ParticleSystemTypes"/> Enum Group</param>
        /// <param name="instanceKey">InstanceKey</param>
        /// <param name="particleSystem"><see cref="ParticleSystem"/> instance to store.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="instanceKey"/> is not a valid index position key.</exception>
        public static void SetParticleSystem(ParticleSystemTypes particleSystemType, int instanceKey, ParticleSystem particleSystem)
        {
            // Retrieve ParticleSystem from List using Enum as index.
            var particleSystemList = _particlesToDisplay[(int)particleSystemType];
            if (particleSystemList == null) return;

            // Check if given 'instanceKey' is valid.
            if (instanceKey + 1 > particleSystemList.Count)
                throw new ArgumentOutOfRangeException("instanceKey", @"Given Key index position is not valid!");

            // Retrieve item
            var particleSystemItem = particleSystemList[instanceKey];
            if (particleSystemItem.ParticleSystem == null) return;

            // Update to new instance given.
            particleSystemItem.ParticleSystem = particleSystem;

            // Store change back into List
            particleSystemList[instanceKey] = particleSystemItem;
            // Store List change back into original Enum 
            _particlesToDisplay[(int) particleSystemType] = particleSystemList;
        }

        // 6/2/2012
        /// <summary>
        /// Stores the <see cref="ParticleSystemItem"/> structure, for the given <paramref name="particleSystemType"/>.
        /// </summary>
        /// <remarks>
        /// The <paramref name="instanceKey"/> is used when multiple instances of the same <see cref="ParticleSystem"/> exist; 
        /// for example, if 3 instances of the <see cref="ParticleSystem"/> 'Smoke' exist, and you want the 2nd instance, 
        /// then pass in 1, since zero-based. 
        /// </remarks>
        /// <param name="particleSystemType"><see cref="ParticleSystemTypes"/> Enum Group</param>
        /// <param name="instanceKey">InstanceKey</param>
        /// <param name="particleSystemItem"><see cref="ParticleSystemItem"/> structure to store.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="instanceKey"/> is not a valid index position key.</exception>
        public static void SetParticleSystemItem(ParticleSystemTypes particleSystemType, int instanceKey, ParticleSystemItem particleSystemItem)
        {
            // Retrieve ParticleSystem from List using Enum as index.
            var particleSystemList = _particlesToDisplay[(int)particleSystemType];
            if (particleSystemList == null) return;

            // Check if given 'instanceKey' is valid.
            if (instanceKey + 1 > particleSystemList.Count)
                throw new ArgumentOutOfRangeException("instanceKey", @"Given Key index position is not valid!");

            // Store change back into List
            particleSystemList[instanceKey] = particleSystemItem;
            // Store List change back into original Enum 
            _particlesToDisplay[(int)particleSystemType] = particleSystemList;
        }

        // 1/16/2010
        /// <summary>
        /// Removes the given <see cref="ParticleSystem"/> instance, from the internal list, to stop
        /// all updating and drawing of the given  <see cref="ParticleSystem"/>.
        /// </summary>
        /// <param name="particleSystemType"><see cref="ParticleSystemTypes"/> Enum</param>
        /// <param name="instanceKey">InstanceKey</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="instanceKey"/> is not a valid index position key.</exception>
        public static void RemoveParticleSystem(ParticleSystemTypes particleSystemType, int instanceKey)
        {
            // Retrieve ParticleSystem from List using Enum as index.
            var particleSystemList = _particlesToDisplay[(int)particleSystemType];
            if (particleSystemList == null) return;

            // Check if given 'instanceKey' is valid.
            if (instanceKey + 1 > particleSystemList.Count)
                throw new ArgumentOutOfRangeException("instanceKey", @"Given Key index position is not valid!");

            // Retrieve item
            var particleSystemItem = particleSystemList[instanceKey];
            if (particleSystemItem.ParticleSystem == null) return;

            // Set position to Null; can't actually remove it, since other instances could exist after this one.
            particleSystemItem.ParticleSystem = null;

            // Store change back into List
            particleSystemList[instanceKey] = particleSystemItem;
            // Store List change back into original Enum 
            _particlesToDisplay[(int)particleSystemType] = particleSystemList;

        }


        // Note: 1/16/2010: Since Rain uses a different Emitter, let's leave this call here.
        // 4/25/2009
        /// <summary>
        /// Updates the rain particles effect.
        /// </summary>
        public static void UpdateRain()
        {
            var tmpPosZero = Vector3Zero;
            var tmpVelZero = Vector3Zero;
            _rainParticleFreqEmitter.AddParticles(ref tmpPosZero, ref tmpVelZero);           

        }

        // 3/1/2011
        /// <summary>
        /// Updates the snow particles effect.
        /// </summary>
        public static void UpdateSnow()
        {
            var tmpPosZero = Vector3Zero;
            var tmpVelZero = Vector3Zero;
            _snowParticleFreqEmitter.AddParticles(ref tmpPosZero, ref tmpVelZero);
        }

        // 7/7/2009 - Dispose of objects
        /// <summary>
        /// Releases the unmanaged resources used by the GameComponent and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;

            // Iterate List and dispose of each particleSystem.
            if (_particlesToDisplay == null) return;

            var count = _particlesToDisplay.Count;
            for (var i = 0; i < count; i++)
            {
                // cache
                var particleSystemList = _particlesToDisplay[i];
                if (particleSystemList == null) continue;

                var count1 = particleSystemList.Count;
                for (var j = 0; j < count1; j++)
                {
                    // cache
                    var particleSystemItem = particleSystemList[j];

                    // Dispose of ParticleSystem
                    if (particleSystemItem.ParticleSystem != null) 
                        particleSystemItem.ParticleSystem.Dispose();

                    // Set null
                    particleSystemItem.ParticleEmitters = null;
                }
                particleSystemList.Clear();
            }
            _particlesToDisplay.Clear();
            _particlesToDisplay = null;
        }

        #region Particle Explosions

        ///<summary>
        /// Adds 25 particles to the small <see cref="ParticleSystem"/>, which draws
        /// a small smoke explosion.
        ///</summary>
        ///<param name="currentPosition">Current position to start explosion</param>
        ///<param name="currentVelocity">Current velocity for explosion smoke</param>
        public static void DoParticles_SmallExplosion(ref Vector3 currentPosition, ref Vector3 currentVelocity)
        {
            for (var i = 0; i < 25; i++)
                _explosionParticles.AddParticle(currentPosition, currentVelocity);

            for (var i = 0; i < 25; i++)
                _smallExplosionSmokeParticles.AddParticle(currentPosition, currentVelocity);
        }

        ///<summary>
        /// Adds 50 particles to the medium <see cref="ParticleSystem"/>, which draws
        /// a medium smoke explosion.
        ///</summary>
        ///<param name="currentPosition">Current position to start explosion</param>
        ///<param name="currentVelocity">Current velocity for explosion smoke</param>
        public static void DoParticles_MediumExplosion(ref Vector3 currentPosition, ref Vector3 currentVelocity)
        {

            for (var i = 0; i < 50; i++)
                _explosionParticles.AddParticle(currentPosition, currentVelocity);

            for (var i = 0; i < 50; i++)
                _mediumExplosionSmokeParticles.AddParticle(currentPosition, currentVelocity);
        }

        ///<summary>
        /// Adds 60 particles to the large <see cref="ParticleSystem"/>, which draws
        /// a large smoke explosion.
        ///</summary>
        ///<param name="currentPosition">Current position to start explosion</param>
        ///<param name="currentVelocity">Current velocity for explosion smoke</param>
        public static void DoParticles_LargeExplosion(ref Vector3 currentPosition, ref Vector3 currentVelocity)
        {
            for (var i = 0; i < 60; i++)
                _explosionParticles.AddParticle(currentPosition, currentVelocity);

            for (var i = 0; i < 60; i++)
                _largeExplosionSmokeParticles.AddParticle(currentPosition, currentVelocity);
        }

        #endregion
    }
}