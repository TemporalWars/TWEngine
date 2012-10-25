#region File Description
//-----------------------------------------------------------------------------
// ScreenManager.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Diagnostics;
using System.Collections.Generic;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.BeginGame.Enums;
using ImageNexus.BenScharbach.TWEngine.Common;
using ImageNexus.BenScharbach.TWEngine.Common.Enums;
using ImageNexus.BenScharbach.TWEngine.GameScreens;
using ImageNexus.BenScharbach.TWEngine.GameScreens.Generic;
using ImageNexus.BenScharbach.TWEngine.HandleGameInput;
using ImageNexus.BenScharbach.TWEngine.PostProcessEffects.BloomEffect;
using ImageNexus.BenScharbach.TWEngine.PostProcessEffects.BloomEffect.Enums;
using ImageNexus.BenScharbach.TWEngine.PostProcessEffects.GBlurEffect;
using ImageNexus.BenScharbach.TWEngine.PostProcessEffects.GlowEffect;
using ImageNexus.BenScharbach.TWEngine.ScreenManagerC.Enums;
using ImageNexus.BenScharbach.TWEngine.SkyDomes;
using ImageNexus.BenScharbach.TWTools.PerfTimersComponent.Counters;
using ImageNexus.BenScharbach.TWTools.PerfTimersComponent.Timers;
using ImageNexus.BenScharbach.TWTools.PerfTimersComponent.Timers.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net;

namespace ImageNexus.BenScharbach.TWEngine.ScreenManagerC
{
    /// <summary>
    /// The <see cref="ScreenManager"/> is a component which manages one or more <see cref="GameScreen"/>
    /// instances. It maintains a stack of Screens, calls their Update and Draw
    /// methods at the appropriate times, and automatically routes _input to the
    /// topmost active screen.
    /// </summary>
    public class ScreenManager : DrawableGameComponent
    {
        #region Fields  

        // 6/17/2012 -
        /// <summary>
        /// Set to capture the Add 'PauseMenuScreen' call, allowing to override with an overloaded
        /// version of the 'PuaseMenuScreen'.
        /// </summary>
        public static event EventHandler AddPauseMenuScreen;

        // 4/20/2010
        private static GraphicsDevice _graphicsDevice;
               
        private static readonly List<GameScreen> Screens = new List<GameScreen>();
        private static readonly List<GameScreen> ScreensToUpdate = new List<GameScreen>();

        // 1/12/2010 - Screens added to this list are reloaded in 'PauseMenuScreen'.
        /// <summary>
        /// This collection is used to reload screens when exiting some game level.  Ideally
        /// this should be used to add the default MainMenu screen.
        /// </summary>
        public static readonly List<GameScreen> MainMenuScreens = new List<GameScreen>();

        //private readonly InputState _input = new InputState(); // 10/19/2012 - GameComponent now.
        private Texture2D _blankTexture;

        // 2/18/2009
        private static RenderTarget2D _colorRt;   // render target for main color buffer   
        private static RenderTarget2D _colorRtEffects;   // render target for PostProcess effects. 

        // 7/18/2009
        private static RenderTarget2D _shadowOutputRt; // render target for ShadowMap output from terrain.
#pragma warning disable 169
        private static Texture2D _shadowGBlurOutputTexture; // ShadowMap texture for GBlur use.
#pragma warning restore 169

        private static Glow _glowEffect;  // 6/19/2009
        private static RenderTarget2D _glowRt;   // render target for glow effect 
        private static Texture2D _textureBackground;  // the background texture used on menus

        private static Bloom _bloomEffect;
// ReSharper disable UnaccessedField.Local
        private static GBlur _gBlurPass;
// ReSharper restore UnaccessedField.Local

        // 4/28/2010 - DeferredRenderingStyle class instance.
        private static DeferredRenderingStyle _deferredRendering;

        // 2/17/2010
        private static RenderingType _renderingType;
        private bool _isInitialized;

        // 2/17/2010 - Create DoDrawProcess signature requirement
        private delegate void DoDrawProcess(GameTime gameTime, bool useBloom, bool useGlow, bool useSkyBox);
        // 2/17/2010 - Holds the current 'DoDrawProcess' method type to use for the current game.
        private static DoDrawProcess _doDrawProcess;

        // XNA 4.0 Updates - RenderState replaced with 4 new states; BlendState, RasterizerState, DepthStencilState, or SampleState.
        private static DepthStencilState _depthStencilState1;
        private static DepthStencilState _depthStencilState2;
        private static DepthStencilState _depthStencilStateForBackground;
        private static BlendState _blendState1;
        private static BlendState _blendStateForBackground;

#if DEBUG
        // 10/20/2012 - Stores the PerformanceCounter's UniqueId.
        private Guid _counterId;
#endif

        #endregion

        #region Properties

        /// <summary>
        /// The <see cref="RenderingType"/> Enum to use within 
        /// the GameEngine (Ex: Normal, NormalPP, or Deferred)
        /// </summary>
        public static RenderingType RenderingType
        {
            get { return _renderingType; }
            set
            {
                _renderingType = value;

                // Set the proper Draw delegate.
                switch (value)
                {
                    case RenderingType.DeferredRendering:
                        // 4/28/2010 - Cache
                        _doDrawProcess = DeferredRenderingStyle.DrawWithDeferredRendering;
                        break;
                    case RenderingType.NormalRendering:
                        _doDrawProcess = DrawWithNormalRendering;
                        break;
                    case RenderingType.NormalRenderingWithPostProcessEffects:
                        _doDrawProcess = DrawWithNormalRenderingAndPostProcessEffects; // 
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("value");
                }
            }
        }

        // 11/5/2009; 1/10/2011 - Fixed to set to effect.
        /// <summary>
        /// Controls the use of the <see cref="Bloom"/> PostProcess effect.
        /// </summary>
        public bool UseBloom
        {
            get { return Bloom.UseBloom; }
            set { Bloom.UseBloom = value; }
        }

        // 12/7/2009
        /// <summary>
        /// The <see cref="BloomType"/> Enum, allows settings the type of <see cref="Bloom"/> PostProcess
        /// effect to use;
        /// 0 = default, 1 = soft, 2 = desaturated, 3 = saturated, 4 = blurry, 5 = subtle.
        /// </summary>
        public BloomType BloomSetting
        {
            get { return Bloom.BloomTypeSetting; }
            set{ Bloom.BloomTypeSetting = value; }
        }
       
        // 12/7/2009
        /// <summary>
        /// Controls the use of the <see cref="Glow"/> PostProcess effect.
        /// </summary>
        /// <remarks>
        /// Requires setting the RenderingType to be <see cref="ImageNexus.BenScharbach.TWEngine.BeginGame.Enums.RenderingType.NormalRenderingWithPostProcessEffects"/>.
        /// </remarks>
        public bool UseGlow { get; set; }

        // 12/7/2009
        /// <summary>
        /// Controls the use of the <see cref="SkyDome"/>.
        /// </summary>
        public bool UseSkyBox { get; set; }
       
        /// <summary>
        /// A default <see cref="SpriteBatch"/> shared by all the <see cref="GameScreen"/>. This saves
        /// each screen having to bother creating their own local instance.
        /// </summary>
        public static SpriteBatch SpriteBatch { get; internal set; }


        /// <summary>
        /// A default <see cref="SpriteFont"/> shared by all the <see cref="GameScreen"/>. This saves
        /// each screen having to bother loading their own local copy.
        /// </summary>
        public static SpriteFont Font { get; private set; }


        /// <summary>
        /// If true, the <see cref="ScreenManager"/> prints out a list of all the Screens
        /// each time it is updated. This can be useful for making sure
        /// everything is being added and removed at the right times.
        /// </summary>
        public bool TraceEnabled { get; set; }

        // 4/23/2011; 10/19/2012
        /// <summary>
        /// Gets a reference to the <see cref="InputState"/> component.
        /// </summary>
        public InputState Input
        {
            get
            {
                if (Game == null || Game.Services == null) return null;
                var input = (InputState)Game.Services.GetService(typeof(InputState));

                return input;
            }
        }

        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new <see cref="ScreenManager"/> component.
        /// </summary>
        /// <param name="game">Instance of <see cref="Game"/>.</param>
        public ScreenManager(Game game)
            : base(game)
        {
            // 8/31/2008 - Try adding the GamerServicesDispatcher directly, rather than use the GamerServicesComponent
            //             Helper Class, since the Helper Class will always crash on a computer that does not have 
            //             XNA GSE 2.0 installed; however, using this approach, I am able to proactively catch the
            //             error, and set the 'IsXnaLiveReady' to false.
            try
            {
#if XBOX360
                // 3/21/2009 - Add for XBOX.
                Game.Components.Add(new GamerServicesComponent(Game));
                TemporalWars3DEngine.IsXnaLiveReady = true; 
#else
                GamerServicesDispatcher.WindowHandle = Game.Window.Handle;
                GamerServicesDispatcher.Initialize(Game.Services);
                TemporalWars3DEngine.IsXnaLiveReady = true;

#endif
            }
            catch
            {
                TemporalWars3DEngine.IsXnaLiveReady = false;
            }

            // 8/28/2008 - Set Draw Order
            DrawOrder = 100; // was 100

            // 12/7/2009 - Turn on the Glow PP effect.
            //UseGlow = true;

            // 4/20/2010 - Store refernec to graphics device.
            _graphicsDevice = game.GraphicsDevice;

            // XNA 4.0 Updates
            _depthStencilState1 = new DepthStencilState { DepthBufferEnable = false, DepthBufferWriteEnable = false };
            _depthStencilState2 = new DepthStencilState { DepthBufferEnable = true, DepthBufferWriteEnable = true };
            _depthStencilStateForBackground = new DepthStencilState { DepthBufferEnable = false, DepthBufferWriteEnable = false };

            _blendState1 = new BlendState
            {
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
                ColorBlendFunction = BlendFunction.Add
            };
            _blendStateForBackground = new BlendState
            {
                ColorSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
                AlphaSourceBlend = Blend.Zero,
                AlphaDestinationBlend = Blend.Zero,
                AlphaBlendFunction = BlendFunction.Add
            };

#if DEBUG
            // Create PerformanceCounter for debug purposes
            //_counterId = PerformanceCounters.CreateCounter();
            //_counterId = PerformanceCounters.CreateCounter(3000);
            _counterId = PerformanceCounters.CreateCounter(1500, "ScreenManager Update()");
#endif
            
        }       


        /// <summary>
        /// Initializes the <see cref="ScreenManager"/> component, by loading any required
        /// services, like the <see cref="SpriteBatch"/> for example.
        /// </summary>
        public sealed override void Initialize()
        {
            // 4/28/2010
            SpriteBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));

            _isInitialized = true;
            base.Initialize();
            
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected sealed override void LoadContent()
        {
            // Load content belonging to the screen manager.
            var content = Game.Content;
            
            // 4/6/2010: Updated to use 'ContentTexturesLoc' global var / Updated to use 'ContentMiscLoc' global var.
            // 11/12/200 - Updated 'MenuFont' to be TimesNewRoman.
            // 7/18/2009 - Updated 'MenuFont' to be Arial-21, rather than Comic-23.
            Font = content.Load<SpriteFont>(TemporalWars3DEngine.ContentMiscLoc + @"\Fonts\menufont");
            _blankTexture = content.Load<Texture2D>(TemporalWars3DEngine.ContentTexturesLoc + @"\Textures\blank"); 

            // 2/18/2009
            var graphicsDevice = Game.GraphicsDevice; // 11/25/2009
            var width = graphicsDevice.Viewport.Width;
            var height = graphicsDevice.Viewport.Height;

            // 4/6/2010: UPdated to use 'ContentTexturesLoc' global var / Updated to use 'ContentMiscLoc' global var.
            _textureBackground = content.Load<Texture2D>(TemporalWars3DEngine.ContentTexturesLoc + @"\Textures\intro_bg");
            BlurManager.SetBlurManager(graphicsDevice, content.Load<Effect>(TemporalWars3DEngine.ContentMiscLoc + @"\Shaders\Blur"), width, height); // 5/26/2009 - 512,512

            // XNA 4.0 Updates - Updates RenderTarget2D signature.
            /*_colorRt = new RenderTarget2D(graphicsDevice, width, height, 1, SurfaceFormat.Color,
                    graphicsDevice.PresentationParameters.MultiSampleType, graphicsDevice.PresentationParameters.MultiSampleQuality);*/
            _colorRt = new RenderTarget2D(graphicsDevice, width, height, true, SurfaceFormat.Color,
                                          DepthFormat.Depth24Stencil8);

            // XNA 4.0 Updates - Updates RenderTarget2D signature.
            // 7/18/2009 - ShadowMap Output from Terrain.
            /*_shadowOutputRt = new RenderTarget2D(graphicsDevice, width, height, 1, SurfaceFormat.Color,
                   graphicsDevice.PresentationParameters.MultiSampleType, graphicsDevice.PresentationParameters.MultiSampleQuality);*/
            _shadowOutputRt = new RenderTarget2D(graphicsDevice, width, height, true, SurfaceFormat.Color,
                                                 DepthFormat.Depth24Stencil8);

            // 7/18/2009 - Add GaussianBlur PostProcess for Shadows            
            _gBlurPass = new GBlur(graphicsDevice, Game.Content);

           
            // 3/13/2009 - Create Deferred Render targets & Load Deferred Effects
            if (RenderingType == RenderingType.DeferredRendering)
            {
                // 4/28/2010 - Create new instance of DeferredRenderingStyle class.
                _deferredRendering = new DeferredRenderingStyle(Game, Screens, content, width, height);
            }

            if (RenderingType == RenderingType.NormalRenderingWithPostProcessEffects ||
                RenderingType == RenderingType.NormalRendering) // 10/27/2009
            {
                _glowEffect = new Glow(graphicsDevice, content);
               
                _bloomEffect = new Bloom(GraphicsDevice, content) {DoGaussianBlurPasses = false};

                // XNA 4.0 Updates - Updates RenderTarget2D signature.
                // 1/25/2010 - Updated RT size from 512x512 to 256x256.
                /*_glowRt = new RenderTarget2D(graphicsDevice, 256, 256, 1, SurfaceFormat.Color,
                    graphicsDevice.PresentationParameters.MultiSampleType, graphicsDevice.PresentationParameters.MultiSampleQuality);*/
                _glowRt = new RenderTarget2D(graphicsDevice, 256, 256, true, SurfaceFormat.Color,
                                             DepthFormat.Depth24Stencil8);


                // XNA 4.0 Updates - Updates RenderTarget2D signature.
                /*_colorRtEffects = new RenderTarget2D(graphicsDevice, width, height, 1, SurfaceFormat.Color,
                   graphicsDevice.PresentationParameters.MultiSampleType, graphicsDevice.PresentationParameters.MultiSampleQuality);*/
                _colorRtEffects = new RenderTarget2D(graphicsDevice, width, height, true, SurfaceFormat.Color,
                                                     DepthFormat.Depth24Stencil8);

               
            }

            // Tell each of the Screens to load their content.
            // 8/25/2008: Updated to For-Loop, rather than ForEach.
            var count = Screens.Count; // 4/20/2010
            for (var i = 0; i < count; i++)
            {
                // 4/28/2010 - Cache
                var gameScreen = Screens[i];
                if (gameScreen == null) continue;

                // 6/17/2012 - Updated to pass 'Content'
                gameScreen.LoadContent(content);
            }
        }

       

       

        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        protected sealed override void UnloadContent()
        {
            // Tell each of the Screens to unload their content.
            // 8/25/2008: Updated to For-Loop, rather than ForEach.
            var count = Screens.Count; // 4/20/2010
            for (var i = 0; i < count; i++)
            {
                // 4/28/2010 - Cache
                var gameScreen = Screens[i];
                if (gameScreen == null) continue;

                gameScreen.UnloadContent();
            }

            // 5/26/2009
            BlurManager.Dispose();
            
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Iterates the internal <see cref="GameScreen"/> collection.
        /// </summary>  
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>  
        public sealed override void Update(GameTime gameTime)
        {
#if DEBUG
            // 10/20/2012 - TESTING new PerformanceCounter
            PerformanceCounters.DoCount(_counterId);
#endif

            // Read the keyboard and gamepad.
            var inputState = Input; // 4/20/2010 - Cache
            //inputState.Update(gameTime); // 10/19/2012 - GameComponent now.

            // 4/28/2010 - Refactored out main code into new STATIC method.
            UpdateGameScreens(inputState, gameTime, !Game.IsActive);

            // 2/18/2009
            // accumulate elapsed Time for background animation
            _backgroundTime += (float) gameTime.ElapsedGameTime.TotalSeconds;

            // 3/13/2009
            // Set Deferred Light Positions and Targets
            if (RenderingType == RenderingType.DeferredRendering && _deferredRendering != null)
                _deferredRendering.HandleInputForDebugging(inputState); // 4/28/2010

            // Print debug trace?
            if (TraceEnabled)
                TraceScreens();

            // 8/31/2008 - Add Pumping of the GamerDispatchService Manually for Networking
            try
            {
                var isXnaLiveReady = TemporalWars3DEngine.IsXnaLiveReady; // 4/20/2010
                if (isXnaLiveReady)
                    GamerServicesDispatcher.Update();
            }
            catch
            {
                Debug.WriteLine("Method error in Update method for GamerServicesDispatcher.Update() call.");
            }
        }

        // 4/28/2010
        /// <summary>
        /// Helper method, which iterates the current <see cref="GameScreen"/> collection, calling
        /// the 'Update' method on each.
        /// </summary>
        /// <param name="inputState"><see cref="InputState"/> instance</param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="otherScreenHasFocus">other screen has focus?</param>
        private static void UpdateGameScreens(InputState inputState, GameTime gameTime, bool otherScreenHasFocus)
        {
            var screensToUpdate = ScreensToUpdate; 
            if (screensToUpdate == null) return;

            // Make a copy of the master screen list, to avoid confusion if
            // the process of updating one screen adds or removes others.
            screensToUpdate.Clear();

            // 4/20/2010 - Cache
            var screens = Screens; 
            if (screens == null) return;

            // 8/22/2008 - Changed to For-Loop, rather than FOREACH
            var count = screens.Count; // 4/20/2010
            for (var i = 0; i < count; i++)
                screensToUpdate.Add(screens[i]);
            
            var coveredByOtherScreen = false;

            // Loop as long as there are Screens waiting to be updated.
            while (screensToUpdate.Count > 0)
            {
                // Pop the topmost screen off the waiting list.
                var index = screensToUpdate.Count - 1; // 4/28/2010
                var screen = screensToUpdate[index];
                if (screen == null) continue; // 4/20/2010

                screensToUpdate.RemoveAt(index);

                // Update the screen.
                screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

                if (screen.ScreenState != ScreenState.TransitionOn && screen.ScreenState != ScreenState.Active)
                    continue;

                // If this is the first active screen we came across,
                // give it a chance to handle _input.
                if (!otherScreenHasFocus)
                {
                    screen.DoHandleInput(gameTime, inputState);

                    otherScreenHasFocus = true;
                }

                // If this is an active non-popup, inform any subsequent
                // Screens that they are covered by it.
                if (!screen.IsPopup)
                    coveredByOtherScreen = true;
            } // End While Loop
        }


        /// <summary>
        /// Prints a list of all the <see cref="GameScreens"/>, for debugging.
        /// </summary>
        static void TraceScreens()
        {
            var screenNames = new List<string>();

            // 10/2/2008: Updated to use ForLoop, rather than ForEach.
            var count = Screens.Count; // 4/20/2010
            for (var i = 0; i < count; i++)
            {
                screenNames.Add(Screens[i].GetType().Name);
            }
           
            //Console.WriteLine(String.Join(", ", screenNames.ToArray()));
        }

       
        /// <summary>
        /// Tells each <see cref="GameScreen"/> to draw itself.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public sealed override void Draw(GameTime gameTime)
        {
#if DEBUG
            // 3/28/2009 - TODO: Debug purposes
            StopWatchTimers.StartStopWatchInstance(StopWatchName.GameDrawLoop_Main_ScreenDraw);//"ScreenManagerDraw"
#endif
           
            // 2/17/2010 - Call Delegate Draw method.
            if (_doDrawProcess != null)
                _doDrawProcess(gameTime, UseBloom, UseGlow, UseSkyBox);
            

            // 4/28/2010 - Do SpriteBatch Draw call.
            Draw2D(gameTime);

            // 6/26/2009 - Update GameViewPort Texture, for debug purposes.            
            //_gameViewPortItem.Texture = _shadowGBlurOutputTexture;
            //GameViewPort.UpdateGameViewPortItem(ref _gameViewPortItem);

#if DEBUG
            // 3/28/2009 - TODO: Debug purposes
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.GameDrawLoop_Main_ScreenDraw);//"ScreenManagerDraw"
#endif

        }

        // 4/28/2010
        /// <summary>
        /// Helper method, which specifically draws the 2D <see cref="SpriteBatch"/>
        /// screens.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private static void Draw2D(GameTime gameTime)
        {
            // 4/20/2010 - Cache
            var gameScreens = Screens;
            if (gameScreens == null) return;

            // Draw 2d text items last
            var count = gameScreens.Count;
            for (var i = 0; i < count; i++)
            {
                // cache
                var screen = gameScreens[i];
                if (screen == null) continue;

                if (screen.ScreenState == ScreenState.Hidden)
                    continue;

                screen.Draw2D(gameTime);
            }
            
        }

        #region Normal Rendering

        // 7/20/2009
// ReSharper disable UnusedMember.Local
        private static void DrawWithNormalRendering(GameTime gameTime, bool useBloom, bool useGlow, bool useSkyBox)
// ReSharper restore UnusedMember.Local
        {
            var graphicsDevice = _graphicsDevice; // 4/20/2010

            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            
            // The following CLEAR command is a MUST; otherwise, the renderTarget color will be Purple!
            //graphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

            // 10/27/2009 - Draw SkyBox
            if (useSkyBox) TerrainScreen.DrawSkyBox();

            // 4/20/2010 - Cache
            var gameScreens = Screens;
            if (gameScreens == null) return;

            // Draw Terrain.
            var count = gameScreens.Count;
            for (var i = 0; i < count; i++)
            {
                // 8/13/2009 - Cache
                var screen = gameScreens[i];
                if (screen == null) continue;

                if (screen.ScreenState == ScreenState.Hidden)
                    continue;

                screen.Draw3D(gameTime);
                screen.Draw3DSceneryItems(gameTime); // 8/1/2009
                screen.Draw3DSelectables(gameTime);
                screen.Draw3DAlpha(gameTime); // 3/19/2009 - Draw AlphaItems Now, like Trees for example.
            }

            // Water need to be drawn here to see it!     
            //WaterManager.RenderWater(gameTime);

        }
       
        // 12/7/2009: Updated to include 'UseGlow', & 'UseSkyBox' params.
        // 11/5/2009: Updated to include the Bloom effect.
        // 7/20/2009; 10/27/2009: Updated to include the Glow effect!
        private static void DrawWithNormalRenderingAndPostProcessEffects(GameTime gameTime, bool useBloom, bool useGlow, bool useSkyBox)
        {
            // 4/20/2010 - Cache
            var graphicsDevice = _graphicsDevice;
            
            // 11/25/2009 - Create the RT with the Glow items.
            // NOTE: This call MUST proceed the 'Terrain' draw call; otherwise, the DepthBuffer info will be lost, and
            //       the units will be seen through the 'Terrain'!
            //StopWatchTimers.StartStopWatchInstance(StopWatchName.GameDrawLoop_Main_ScreenDrawGlow);
            //if (useGlow) CreateGlowRT(graphicsDevice, gameTime);
            //StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.GameDrawLoop_Main_ScreenDrawGlow);

            // The following CLEAR command is a MUST; otherwise, the renderTarget color will be Purple!
            graphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0); //  

            // 5/16/2012 - Draw SkyBox
            TerrainScreen.UseSkyBox = useSkyBox;

            // 4/20/2010 - Cache
            var gameScreens = Screens;
            if (gameScreens == null) return;

            // Draw Terrain.
            var count = gameScreens.Count; // 8/13/2009
            for (var i = 0; i < count; i++)
            {
                // 8/13/2009 - Cache
                var screen = gameScreens[i];
                if (screen == null) continue;

                if (screen.ScreenState == ScreenState.Hidden)
                    continue;

                screen.Draw3D(gameTime);

            }

            // Water need to be drawn here to see it!     
            //WaterManager.RenderWater(gameTime);

            // 7/18/2009 - GBlur ShadowOutput Textures
            //StopWatchTimers.StartStopWatchInstance(StopWatchName.GameDrawLoop_Main_ScreenDrawGBlur);
            //_gBlurPass.PostProcess(_shadowOutputRt.GetTexture(), out _shadowGBlurOutputTexture);
            //StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.GameDrawLoop_Main_ScreenDrawGBlur);

            //
            // IMPORTANT: Actual point where stuff gets drawn back to the screen!
            //

            // 11/25/2009: Updated to use new 'DepthBuffer' version.
            // Draw the combine texture to screen
            //DrawRenderTargetTexture(graphicsDevice, _colorRt, 1.0f, false, _blendState1); // 9/22/2010 - XNA 4.0 Updates - Pass in blendState.

            // Draw the GBlur Shadow texture with additive blending
            //DrawRenderTargetTexture(graphicsDevice, _shadowGBlurOutputTexture, 1.0f, true, Blend.DestinationColor, Blend.One, BlendFunction.Add);

            // Draw Remaining items
            /*for (var i = 0; i < count; i++)
            {
                // 8/13/2009 - Cache
                var screen = gameScreens[i];
                if (screen == null) continue;

                if (screen.ScreenState == ScreenState.Hidden)
                    continue;

                screen.Draw3DSelectables(gameTime);
                screen.Draw3DSceneryItems(gameTime); // 8/1/2009
                screen.Draw3DAlpha(gameTime); // 3/19/2009 - Draw AlphaItems Now, like Trees for example.
               
            }*/

            // 10/27/2009 - Draw the glow texture with additive blending - _glowBlurRT
            //StopWatchTimers.StartStopWatchInstance(StopWatchName.GameDrawLoop_Main_ScreenDrawGlow);
            //if (useGlow) DrawRenderTargetTexture(graphicsDevice, _glowRt, 2.5f, true, todo); // Note: ??


            //StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.GameDrawLoop_Main_ScreenDrawGlow);
            
        }

        #endregion

     
        
        #region Normal Rendering with Blur-Glow  
     
        // 10/27/2009
        /// <summary>
        /// Draws only the items in the Draw3DIllumination method, and then 
        /// creates the does the post-process 'BlurGlow' effect, which is written
        /// to the _glowRT renderTarget.
        /// </summary>
        /// <param name="graphicsDevice"><see cref="GraphicsDevice"/> instance</param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private static void CreateGlowRT(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            { // Draw ONLY Glow Items to a seperate render target.               

                // XNA 4.0 Updates - Index no longer required on SetRenderTarget.
                // set the color render target
                //graphicsDevice.SetRenderTarget(0, _colorRtEffects);
                graphicsDevice.SetRenderTarget(_colorRtEffects);

                // The following CLEAR command is a MUST; otherwise, the renderTarget color will be Purple!
                graphicsDevice.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);

                var count = Screens.Count; // 11/24/09
                for (var i = 0; i < count; i++)
                {
                    // 11/24/2009 - Cache
                    var screen = Screens[i];
                    if (screen == null) continue;

                    if (screen.ScreenState == ScreenState.Hidden)
                        continue;

                    screen.Draw3DIllumination(gameTime);

                }

                // XNA 4.0 Updates - Index no longer required on SetRenderTarget.
                // resolve the color render targets
                //graphicsDevice.SetRenderTarget(0, null);
                graphicsDevice.SetRenderTarget(null);              
                                
            }


            // XNA 4.0 Updates - GetTexture obsolete; RenderTarget inherit from Texture.
            // Blur the glow render target
            //_glowEffect.PostProcess(_colorRtEffects.GetTexture(), _glowRt);
            _glowEffect.PostProcess(_colorRtEffects, _glowRt);
            
            
        }

        // 9/22/2010 - XNA 4.0 Updates - Removed the 3 params 'SourceBlend', 'DestBlend', and 'BlendFunction', and instead now pass
        // in the new 'BlendState'.
        // 10/27/2009 - 
        /// <summary>
        /// Draws render target as fullscreen texture with given intensity and blend mode
        /// </summary>
        /// <param name="graphicsDevice"><see cref="GraphicsDevice"/> instance</param>
        /// <param name="renderTarget"><see cref="RenderTarget2D"/> instance</param>
        /// <param name="intensity">Intensity setting</param>
        /// <param name="additiveBlend">Use additive blend?</param>
        /// <param name="blendState">Sets type of <see cref="BlendState"/> to use.</param>
        internal static void DrawRenderTargetTexture(GraphicsDevice graphicsDevice, RenderTarget2D renderTarget, 
            float intensity, bool additiveBlend, BlendState blendState)
        {
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice");
            }

            // 9/22/2010 - XNA 4.0 Updates - RenderState replaced with 4 new states; BlendState, RasterizerState, DepthStencilState, or SampleState.
            // set up render state and blend mode
            //graphicsDevice.RenderState.DepthBufferWriteEnable = false;
            //graphicsDevice.RenderState.DepthBufferEnable = false;
            graphicsDevice.DepthStencilState = _depthStencilState1;


            if (additiveBlend)
            {
                // XNA 4.0 Updates - AlphaBlendEnable is implied now.
                //graphicsDevice.RenderState.AlphaBlendEnable = true;  

                // XNA 4.0 Updates - RenderState replaced with 4 new states; BlendState, RasterizerState, DepthStencilState, or SampleState.
                /*graphicsDevice.RenderState.SourceBlend = blendState; // was Blend.One
                graphicsDevice.RenderState.DestinationBlend = destBlend; // was Blend.One
                graphicsDevice.RenderState.BlendFunction = blendFunction; // 10/27/2009*/
                graphicsDevice.BlendState = blendState;
            }

            // XNA 4.0 Updates - GetTexture obsolete; RenderTarget inherit from Texture.
            // draw render target as fullscreen texture
            //BlurManager.RenderScreenQuad(graphicsDevice, BlurTechnique.ColorTexture, renderTarget.GetTexture(), new Vector4(intensity));
            BlurManager.RenderScreenQuad(graphicsDevice, BlurTechnique.ColorTexture, renderTarget, new Vector4(intensity));

            // XNA 4.0 Updates - RenderState replaced with 4 new states; BlendState, RasterizerState, DepthStencilState, or SampleState.
            // restore render state and blend mode
            //graphicsDevice.RenderState.DepthBufferWriteEnable = true;
            //graphicsDevice.RenderState.DepthBufferEnable = true;
            graphicsDevice.DepthStencilState = _depthStencilState2;

            // XNA 4.0 Updates - AlphaBlendEnable is implied now.
            //graphicsDevice.RenderState.AlphaBlendEnable = false;
        }
      
        #endregion

       
        float _backgroundTime;  // Time for background animation used on menus
        
        ///<summary>
        /// Draws the background animated image
        ///</summary>
        ///<param name="graphicsDevice"><see cref="GraphicsDevice"/> instance</param>
        public void DrawBackground(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null)
                graphicsDevice = _graphicsDevice;
            
            
            const float animationTime = 3.0f;
            const float animationLength = 0.4f;
            const int numberLayers = 2;
            const float layerDistance = 1.0f / numberLayers;

            // normalized Time
            var normalizedTime = ((_backgroundTime / animationTime) % 1.0f);

            // XNA 4.0 Updates - AlphaBlendEnable & SeparateAlphaBlendEnabled are implied now.
            //graphicsDevice.RenderState.AlphaBlendEnable = true;
            //graphicsDevice.RenderState.SeparateAlphaBlendEnabled = true;

            // XNA 4.0 Updates - RenderState replaced with 4 new states; BlendState, RasterizerState, DepthStencilState, or SampleState.
            // set render states
            /*graphicsDevice.RenderState.DepthBufferEnable = false;
            graphicsDevice.RenderState.DepthBufferWriteEnable = false;
            graphicsDevice.RenderState.SourceBlend = Blend.One;
            graphicsDevice.RenderState.DestinationBlend = Blend.One;
            graphicsDevice.RenderState.AlphaBlendOperation = BlendFunction.Add;
            graphicsDevice.RenderState.AlphaDestinationBlend = Blend.Zero;
            graphicsDevice.RenderState.AlphaSourceBlend = Blend.Zero;*/
            graphicsDevice.DepthStencilState = _depthStencilStateForBackground;
            graphicsDevice.BlendState = _blendStateForBackground;

            // render all background layers
            for (var i = 0; i < numberLayers; i++)
            {
                float scale;
                if (normalizedTime > 0.5f)
                    scale = 2 - normalizedTime * 2;
                else
                    scale = normalizedTime * 2;

                var color = new Vector4(scale, scale, scale, 1);

                scale = 1 + normalizedTime * animationLength;

                BlurManager.RenderScreenQuad(graphicsDevice, BlurTechnique.ColorTexture, _textureBackground, color, scale);

                normalizedTime = (normalizedTime + layerDistance) % 1.0f;
            }

            // XNA 4.0 Updates
            // restore render states
            //graphicsDevice.RenderState.DepthBufferEnable = true;
            //graphicsDevice.RenderState.DepthBufferWriteEnable = true;
            graphicsDevice.DepthStencilState = _depthStencilState2;

            // XNA 4.0 Updates - AlphaBlendEnable & SeparateAlphaBlendEnabled are implied now.
            //graphicsDevice.RenderState.AlphaBlendEnable = false;
            //graphicsDevice.RenderState.SeparateAlphaBlendEnabled = false;
        }


        #endregion

        #region Public Methods

        // 6/29/2012
        /// <summary>
        /// Displays the proper 'Pause' screen.
        /// </summary>
        /// <param name="networkSession">Instance of <see cref="NetworkSession"/></param>
        /// <returns></returns>
        public bool DisplayPauseScreen(NetworkSession networkSession)
        {
            // 6/17/2012 - Check if callback should be used.
            if (!OnAddPauseMenuScreen(EventArgs.Empty))
            {
                // If they pressed pause, bring up the pause menu screen.
                AddScreen(new PauseMenuScreen(networkSession), false);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds a new <see cref="GameScreen"/> to the <see cref="ScreenManager"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when internal 'Screens' collection is null.</exception>
        /// <param name="screen"><see cref="GameScreen"/> instance</param>
        /// <param name="isMainMenuScreen">Set to true to add to the internal <see cref="MainMenuScreens"/> collection.</param>
        public void AddScreen(GameScreen screen, bool isMainMenuScreen)
        {
            if (Screens == null)
                throw new InvalidOperationException("The internal 'Screens' collection is null, which is not allowed for this operation.");

            // Load content belonging to the screen manager.
            var content = Game.Content;

            screen.ScreenManager = this;
            screen.IsExiting = false;

            // If we have a graphics Device, tell the screen to load content.
            if (_isInitialized)
            {
                // 6/17/2012 - Updated to pass 'Content'.
                screen.LoadContent(content);
            }
            
            Screens.Add(screen);

            // 2/7/2011 - Trigger Initialize method.
            screen.Initialize();

            // 1/12/2011
            if (isMainMenuScreen)
                MainMenuScreens.Add(screen);
        }


        /// <summary>
        /// Removes a <see cref="GameScreen"/> from the <see cref="ScreenManager"/>. You should normally
        /// use GameScreen.ExitScreen instead of calling this directly, so
        /// the screen can gradually transition off rather than just being
        /// instantly removed.
        /// </summary>
        /// <param name="screen"><see cref="GameScreen"/> instance</param>
        public void RemoveScreen(GameScreen screen)
        {
            if (Screens == null)
                throw new InvalidOperationException("The internal 'Screens' collection is null, which is not allowed for this operation.");

            // If we have a graphics Device, tell the screen to unload content.
            if (_isInitialized)
            {
                screen.UnloadContent();
            }
            
            Screens.Remove(screen);
            ScreensToUpdate.Remove(screen);
        }


        /// <summary>
        /// Expose an array holding all the <see cref="GameScreen"/>. We return a copy rather
        /// than the real master list, because <see cref="GameScreen"/> should only ever be added
        /// or removed using the <see cref="AddScreen"/> and <see cref="RemoveScreen"/> methods.
        /// </summary>
        /// <returns>Array of <see cref="GameScreen"/>.</returns>
        public static GameScreen[] GetScreens()
        {
            return Screens.ToArray();
        }


        /// <summary>
        /// Helper draws a translucent black fullscreen sprite, used for fading
        /// <see cref="GameScreen"/> in and out, and for darkening the background behind popups.
        /// </summary>  
        /// <param name="alpha">Alpha value to use</param>      
        public void FadeBackBufferToBlack(int alpha)
        {
            try // 3/6/2011
            {
                var viewport = GraphicsDevice.Viewport;

                SpriteBatch.Begin();

                // 8/22/2008
                var tmpRectangle = Rectangle.Empty;
                tmpRectangle.X = 0; tmpRectangle.Y = 0;
                tmpRectangle.Width = viewport.Width; tmpRectangle.Height = viewport.Height;

                SpriteBatch.Draw(_blankTexture, tmpRectangle, new Color(0, 0, 0, (byte)alpha));

                SpriteBatch.End();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(string.Format("FadeBackBufferToBlack threw the exception {0}", ex.Message));
            }
            
        }

        // 4/23/2011
        ///<summary>
        /// Resets the <see cref="ScreenManager"/> to the default DrawOrder.
        ///</summary>
        public void ResetToDefaultDrawOrder()
        {
            DrawOrder = 100;
        }

        // 6/17/2012
        /// <summary>
        /// Trigger the EventHandler for the AddPauseMenuScreen.
        /// </summary>
        internal static bool OnAddPauseMenuScreen(EventArgs e)
        {
            EventHandler handler = AddPauseMenuScreen;
            if (handler != null)
            {
                handler(null, e);
                return true;
            }

            return false;
        }
   

        // XNA 4.0 Updates; 'DepthStencilBuffer' is gone!
        // 2/18/2009
        /*/// <summary>
        /// Creates <see cref="DepthStencilBuffer"/> using <see cref="RenderTarget2D"/> <paramref name="target"/> parameter.
        /// </summary>
        /// <param name="target"><see cref="RenderTarget2D"/> to use</param>
        /// <returns><see cref="DepthStencilBuffer"/> instance</returns>
        internal static DepthStencilBuffer CreateDepthStencil(RenderTarget2D target)
        {
            return new DepthStencilBuffer(target.GraphicsDevice, target.Width,
                target.Height, target.GraphicsDevice.DepthStencilBuffer.Format,
                target.MultiSampleType, target.MultiSampleQuality);
        }*/

        // XNA 4.0 Updates; 'DepthStencilBuffer' is gone!
        // 2/18/2009
        /*/// <summary>
        /// Creates <see cref="DepthStencilBuffer"/> using <see cref="RenderTarget2D"/> <paramref name="target"/> parameter.
        /// </summary>
        /// <param name="target"><see cref="RenderTarget2D"/> to use</param>
        /// <param name="depth"><see cref="DepthFormat"/> Enum to use</param>
        /// <returns><see cref="DepthStencilBuffer"/> instance</returns>
        internal static DepthStencilBuffer CreateDepthStencil(RenderTarget2D target, DepthFormat depth)
        {
            if (GraphicsAdapter.DefaultAdapter.CheckDepthStencilMatch(DeviceType.Hardware,
               GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Format, target.Format,
                depth))
            {
                return new DepthStencilBuffer(target.GraphicsDevice, target.Width,
                    target.Height, depth, target.MultiSampleType, target.MultiSampleQuality);
            }
            return CreateDepthStencil(target);
        }*/

        // XNA 4.0 Updates; 'DepthStencilBuffer' is gone!
        // 1/26/2010
        /*/// <summary>
        /// Creates <see cref="DepthStencilBuffer"/> using <see cref="RenderTarget2D"/> <paramref name="target"/> parameter.
        /// </summary>
        /// <param name="target"><see cref="RenderTarget2D"/> to use</param>
        /// <param name="depth"><see cref="DepthFormat"/> Enum to use</param>
        /// <param name="width">Desired width</param>
        /// <param name="height">Desired height</param>
        /// <returns><see cref="DepthStencilBuffer"/> instance</returns>
        internal static DepthStencilBuffer CreateDepthStencil(RenderTarget2D target, DepthFormat depth, 
                                                              int width, int height)
        {
            if (GraphicsAdapter.DefaultAdapter.CheckDepthStencilMatch(DeviceType.Hardware,
               GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Format, target.Format,
                depth))
            {
                return new DepthStencilBuffer(target.GraphicsDevice, width,
                    height, depth, target.MultiSampleType, target.MultiSampleQuality);
            }
            return CreateDepthStencil(target);
        }*/

       
       
        #endregion

      
    }
}
