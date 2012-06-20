#region File Description
//-----------------------------------------------------------------------------
// LoadingScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Threading;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Net;
using TWEngine.Interfaces;
using TWEngine.ScreenManagerC;
using System.Text;
using TWEngine.ScreenManagerC.Enums;

namespace TWEngine.GameScreens.Generic
{
    /// <summary>
    /// The <see cref="LoadingScreen"/> coordinates transitions between the menu system and the
    /// game itself. Normally one <see cref="GameScreen"/> will transition off at the same time as
    /// the next <see cref="GameScreen"/> is transitioning on, but for larger transitions that can
    /// take a longer time to load their data, we want the menu system to be entirely
    /// gone before we start loading the game. This is done as follows:
    /// 
    /// - Tell all the existing <see cref="GameScreen"/> to transition off.
    /// - Activate a loading <see cref="GameScreen"/>, which will transition on at the same Time.
    /// - The loading <see cref="GameScreen"/> watches the state of the previous <see cref="GameScreen"/>.
    /// - When it sees they have finished transitioning off, it activates the real
    ///   next <see cref="GameScreen"/>, which may take a long Time to load its data. The loading
    ///   <see cref="GameScreen"/> will be the only thing displayed while this load is taking place.
    /// </summary>
    public class LoadingScreen : GameScreen
    {
        #region Fields

        private readonly bool _loadingIsSlow;
        private bool _otherScreensAreGone;

        private static GameScreen[] _screensToLoad;

         // 8/7/2009
// ReSharper disable InconsistentNaming
        private static readonly StringBuilder _stringBuilder = new StringBuilder(25);
// ReSharper restore InconsistentNaming

        private static Thread _backgroundThread;
        private static EventWaitHandle _backgroundThreadExit;

        private GraphicsDevice _graphicsDevice;
        private NetworkSession _networkSession;
        private readonly IMessageDisplay _messageDisplay;

        // 9/6/2008: Moved from Draw command.
        private static SpriteBatch _spriteBatch;
        private static SpriteFont _font;
        private static Viewport _viewport;

        /// <summary>
        /// Activates the <see cref="LoadingScreen"/>.
        /// </summary>
        private static GameScreen[] _screens = new GameScreen[1];

        // 9/15/2008: Allows changing of the Message shown on Loading screen
        public static string LoadingMessage;

        private static GameTime _loadStartTime;
        private static TimeSpan _loadAnimationTimer;

        #endregion

        #region Properties
        // 4/29/2010
        /// <summary>
        /// Set or Get the current <see cref="Texture2D"/> background image.
        /// </summary>
        public static Texture2D BackgroundTexture { get; set; }

        #endregion

        #region Initialization


        /// <summary>
        /// The constructor is private: loading <see cref="GameScreen"/> should
        /// be activated via the static Load method instead.
        /// </summary>
        private LoadingScreen(ScreenManager screenManager, bool loadingIsSlow,
                              GameScreen[] screensToLoad)
        {
            _loadingIsSlow = loadingIsSlow;
            _screensToLoad = screensToLoad;            

            TransitionOnTime = TimeSpan.FromSeconds(0.5);

            // If this is going to be a slow load operation, create a background
            // thread that will update the network session and draw the load screen
            // animation while the load is taking place.
            if (!loadingIsSlow) return;

            _backgroundThread = new Thread(BackgroundWorkerThread);
            _backgroundThreadExit = new ManualResetEvent(false);

            _graphicsDevice = screenManager.GraphicsDevice;

            // Look up some services that will be used by the background thread.
            IServiceProvider services = screenManager.Game.Services;

            _networkSession = (NetworkSession)services.GetService(
                                                  typeof(NetworkSession));

            _messageDisplay = (IMessageDisplay)services.GetService(
                                                   typeof(IMessageDisplay));

            // 4/29/2010 - Set SpriteBatch
            _spriteBatch = ScreenManager.SpriteBatch;

            // 6/16/2012 - Skip when NOT null.
            // 11/13/2009 - Load Background texture
            if (BackgroundTexture == null)
                BackgroundTexture = TemporalWars3DEngine.ContentResourceManager.Load<Texture2D>("MenuBackgroundSpaceEclipse");
        }

       
        /// <summary>
        /// This method is used to load some <see cref="GameScreen"/>(s).  When
        /// the <paramref name="loadingIsSlow"/> is true, the loading screen will
        /// draw the background and display a message to the user, which is all 
        /// done in a <see cref="Thread"/> environment.
        /// </summary>
        /// <param name="screenManager"><see cref="ScreenManager"/> instance</param>
        /// <param name="loadingIsSlow">Is going to take a while to load?</param>
        /// <param name="screensToLoad"><see cref="GameScreen"/>(s) to load</param>
        public static void Load(ScreenManager screenManager, bool loadingIsSlow,
                                params GameScreen[] screensToLoad)
        {
            // 9/6/2008: Change to a ForLoop, rather than ForEach; also updated
            //           the Array to be Static Class Variable, and update size
            //           using the Array.Resize() method.
            // Tell all the current _screens to transition off.
            Array.Resize(ref _screens, ScreenManager.GetScreens().Length);

            _screens = ScreenManager.GetScreens();
            var length = _screens.Length; // 8/18/2009
            for (var i = 0; i < length; i++)
            {
                // 4/29/2010 - Cache
                var gameScreen = _screens[i];
                if (gameScreen == null) continue;

                gameScreen.ExitScreen();
            }

            // Create and activate the loading screen.
            var loadingScreen = new LoadingScreen(screenManager,
                                                            loadingIsSlow,
                                                            screensToLoad);

            screenManager.AddScreen(loadingScreen, false);
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the <see cref="LoadingScreen"/> by updating the background thread,
        /// check if current <see cref="GameScreen"/>(s) are loaded, if when true, unload
        /// the current <see cref="LoadingScreen"/>.
        /// </summary>
        public sealed override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // If all the previous _screens have finished transitioning
            // off, it is Time to actually perform the load.
            if (!_otherScreensAreGone) return;

            // Start up the background thread, which will update the network
            // session and draw the animation while we are loading.

            // 4/29/2010 - Refactored core code to new STATIC method.
            UpdateLoadingScreen(gameTime, ScreenManager, this);

            // Once the load has finished, we use ResetElapsedTime to tell
            // the  game timing mechanism that we have just finished a very
            // long frame, and that it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();
        }

        // 4/29/2010
        /// <summary>
        /// Helper method, which iterates the internal collection <see cref="_screensToLoad"/>,
        /// and updates the background loading <see cref="Thread"/>.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="screenManager"><see cref="ScreenManager"/> instance</param>
        /// <param name="loadingScreen"><see cref="GameScreen"/> as loadingScreen instance</param>
        private static void UpdateLoadingScreen(GameTime gameTime, ScreenManager screenManager, GameScreen loadingScreen)
        {
            if (_backgroundThread != null)
            {
                _loadStartTime = gameTime;
                _backgroundThread.Start();
            }

            // Perform the load operation.
            screenManager.RemoveScreen(loadingScreen);

            // 9/6/2008: Updated to ForLoop, rather than ForEach.
            var count = _screensToLoad.Length; // 11/19/2009
            for (var i = 0; i < count; i++)
            {
                // 11/19/2009 - Cache
                var screenToLoad = _screensToLoad[i];

                if (screenToLoad == null) continue;

                screenManager.AddScreen(screenToLoad, false);
            }               

            // Signal the background thread to exit, then wait for it to do so.
            if (_backgroundThread == null) return;

            _backgroundThreadExit.Set();
            _backgroundThread.Join();
            _backgroundThread = null; // 4/29/2010
        }

       

        // 9/6/2008: Updated to optimize memory.
        /// <summary>
        /// Draws the <see cref="LoadingScreen"/>, when the <see cref="_loadingIsSlow"/> is true.
        /// </summary>            
        public sealed override void Draw2D(GameTime gameTime)
        {
            
            // If we are the only active screen, that means all the previous _screens
            // must have finished transitioning off. We check for this in the Draw
            // method, rather than in Update, because it isn't enough just for the
            // _screens to be gone: in order for the transition to look good we must
            // have actually drawn a frame without them before we perform the load.
            if ((ScreenState == ScreenState.Active) &&
                (ScreenManager.GetScreens().Length == 1))
            {
                _otherScreensAreGone = true;
            }

            // The gameplay screen takes a while to load, so we display a loading
            // message while that is going on, but the menus load very quickly, and
            // it would look silly if we flashed this up for just a fraction of a
            // second while returning from the game to the menus. This parameter
            // tells us how long the loading is going to take, so we know whether
            // to bother drawing the message.
            if (!_loadingIsSlow) return;

            // 4/29/2010 - Refactored core code to new STATIC method.
            DrawLoadingScreen(gameTime, ScreenManager, TransitionAlpha);
        }

        // 4/29/2010
        /// <summary>
        /// Helper method, using the <see cref="SpriteBatch"/> from <see cref="ScreenManager"/>, draws
        /// the loading screen background and update messages.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="screenManager"><see cref="ScreenManager"/> instance</param>
        /// <param name="transitionAlpha">Transition alpha byte value</param>
        private static void DrawLoadingScreen(GameTime gameTime, ScreenManager screenManager, byte transitionAlpha)
        {
            var spriteBatch = _spriteBatch; // 4/29/2010
            _font = ScreenManager.Font;

            // 8/7/2009: Updated to use StringBuilder to eliminate String garbage!
            // 9/15/2008 - Check if I should display default message
            var stringBuilder = _stringBuilder; // 4/29/2010
            stringBuilder.Length = 0;
            stringBuilder.Append(LoadingMessage ?? Resources.Loading);

            // Center the text in the _viewport.
            _viewport = screenManager.GraphicsDevice.Viewport;
            var viewportSize = new Vector2 {X = _viewport.Width, Y = _viewport.Height};

            var textSize = _font.MeasureString(stringBuilder);
            var textPosition = (viewportSize - textSize)/2;

            var color = new Color(255, 255, 255, transitionAlpha);

            // Animate the number of dots after our "Loading..." message.
            _loadAnimationTimer += gameTime.ElapsedGameTime;

            var dotCount = (int) (_loadAnimationTimer.TotalSeconds*5)%10;

            // 8/7/2009 - Updated to use StringBuilder.
            for (var i = 0; i < dotCount; i++)
                stringBuilder.Append(".");

            // 11/13/2009
            var viewport = screenManager.GraphicsDevice.Viewport;
            var fullscreen = new Rectangle {X = 0, Y = 0, Width = viewport.Width, Height = viewport.Height};

            try // 1/5/2010
            {
                // XNA 4.0 Updates
                // Draw the text.
                //spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None);
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

                // 11/13/2009 - Draw Background texture
                spriteBatch.Draw(BackgroundTexture, fullscreen, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.1f); // 1 is back.

                spriteBatch.DrawString(_font, stringBuilder, textPosition, color);

                spriteBatch.End();
            }
            // 1/5/2010 - Captures the SpriteBatch InvalidOpExp.
            catch (InvalidOperationException)
            {
                Debug.WriteLine("(Draw2D) threw the 'InvalidOpExp' error, in the LoadingScreen class.");

                // Dispose of SpriteBatch
                spriteBatch.Dispose();
            }
        }

        #endregion

        #region Background Thread


        /// <summary>
        /// Worker thread draws the loading animation and updates the network
        /// session while the load is taking place.
        /// </summary>       
        void BackgroundWorkerThread()
        {
            var lastTime = Stopwatch.GetTimestamp();

            // EventWaitHandle.WaitOne will return true if the exit signal has
            // been triggered, or false if the timeout has expired. We use the
            // timeout to update at regular intervals, then break out of the
            // loop when we are signalled to exit.
            while (!_backgroundThreadExit.WaitOne(1000 / 30)) //, false))
            {
                var gameTime = GetGameTime(lastTime);

                DrawLoadAnimation(gameTime);

                UpdateNetworkSession();
            }
        }


        /// <summary>
        /// Works out how long it has been since the last background thread update.
        /// </summary>         
        static GameTime GetGameTime(long lastTime)
        {
            var currentTime = Stopwatch.GetTimestamp();
            var elapsedTicks = currentTime - lastTime;

            var elapsedTime = TimeSpan.FromTicks(elapsedTicks *
                                                      TimeSpan.TicksPerSecond /
                                                      Stopwatch.Frequency);

            // XNA 4.0 Updates - TotalRealTime obsolete.
            /*return new GameTime(_loadStartTime.TotalRealTime + elapsedTime, elapsedTime,
                                _loadStartTime.TotalGameTime + elapsedTime, elapsedTime);*/
            return new GameTime(_loadStartTime.TotalGameTime + elapsedTime, elapsedTime);
        }


        /// <summary>
        /// Calls directly into our Draw method from the background worker thread,
        /// so as to update the load animation in parallel with the actual loading.
        /// </summary>
        void DrawLoadAnimation(GameTime gameTime)
        {
            if ((_graphicsDevice == null) || _graphicsDevice.IsDisposed)
                return;

            try
            {
                _graphicsDevice.Clear(Color.Black);

                // Draw the loading screen.
                Draw2D(gameTime);

                // If we have a message display component, we want to display
                // that over the top of the loading screen, too.
                if (_messageDisplay != null)
                {
                    _messageDisplay.Update(gameTime);
                    _messageDisplay.Draw(gameTime);
                }

                _graphicsDevice.Present();
            }
            catch
            {
                // If anything went wrong (for instance the graphics Device was lost
                // or reset) we don't have any good way to recover while running on a
                // background thread. Setting the Device to null will stop us from
                // rendering, so the main game can deal with the problem later on.
                _graphicsDevice = null;
            }
        }


        /// <summary>
        /// Updates the network session from the background worker thread, to avoid
        /// disconnecting due to network timeouts even if loading takes a long Time.
        /// </summary>
        void UpdateNetworkSession()
        {
            if ((_networkSession == null) ||
                (_networkSession.SessionState == NetworkSessionState.Ended))
                return;
            
            try
            {
                _networkSession.Update();
            }
            catch
            {
                // If anything went wrong, we don't have a good way to report that
                // error while running on a background thread. Setting the session to
                // null will stop us from updating it, so the main game can deal with
                // the problem later on.
                _networkSession = null;
            }
        }
        #endregion
    }
}
