#region File Description
//-----------------------------------------------------------------------------
// BackgroundScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//
// Updated by Ben Scharbach
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TWEngine.ScreenManagerC;

namespace TWEngine.GameScreens.Generic
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.GameScreens.Generic"/> namespace contains the common classes
    /// which make up the entire <see cref="TWEngine.GameScreens"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    /// <summary>
    /// The <see cref="BackgroundScreen"/> sits behind all the other menu screens.
    /// It draws a background image that remains fixed in place regardless
    /// of whatever transitions the screens on top of it may be doing.
    /// </summary>
    public class BackgroundScreen : GameScreen , IDisposable
    {
        #region Fields

        // 1/6/2010 - Content Resources for some Assets.
        private ResourceContentManager _contentResourceManager;

        private static SpriteBatch _spriteBatch;      
        private static readonly Vector2 Vector2Zero = Vector2.Zero;
        private static readonly Color ColorWhite = Color.White;
        private static bool _drawBackgroundAnimated = true; // 6/16/2012

        #endregion

        #region Properties

        // 6/16/2012
        /// <summary>
        /// Gets or sets to do the animated default background drawing.
        /// </summary>
        /// <remarks>
        /// This defaults to TRUE.
        /// </remarks>
        public static bool DrawBackgroundAnimated
        {
            get { return _drawBackgroundAnimated; }
            set { _drawBackgroundAnimated = value; }
        }

        ///<summary>
        /// Main <see cref="Texture2D"/> background image to use.
        ///</summary>
        public static Texture2D BackgroundTexture { get; set; }

        ///<summary>
        /// Optional 2nd <see cref="Texture2D"/> background image to use.
        ///</summary>
        public static Texture2D BackgroundTexture2 { get; set; }

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor for <see cref="BackgroundScreen"/>, which sets the internal
        /// <see cref="GameScreen.TransitionOnTime"/> and <see cref="GameScreen.TransitionOffTime"/> to
        /// be 0.5f seconds.
        /// </summary>
        public BackgroundScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }


        /// <summary>
        /// Loads graphics content for this <see cref="BackgroundScreen"/>. The background texture is quite
        /// big, so we use our own local <see cref="ContentManager"/> to load it. This allows us to unload 
        /// before going from the menus into the game itself, wheras if we used the shared <see cref="ContentManager"/> 
        /// provided by the <see cref="Game"/> class, the content would remain loaded until the game was shut-down.
        /// </summary>
        /// <param name="contentManager"> </param>
        public sealed override void LoadContent(ContentManager contentManager)
        {
            // 6/16/2012 - Skips loading these textures when not doing animated background.
            if (!DrawBackgroundAnimated)
                return;

            // 1/6/2010 - Create ContentResourceManager
            if (_contentResourceManager == null)
            {
#if XBOX360
                _contentResourceManager = new ResourceContentManager(TemporalWars3DEngine.GameInstance.Services, Resource360.ResourceManager);
#else
                _contentResourceManager = new ResourceContentManager(TemporalWars3DEngine.GameInstance.Services, Resources.ResourceManager);
#endif
            }
            // 2/17/2009 - Updated the Menu Background texture            
            BackgroundTexture = _contentResourceManager.Load<Texture2D>("MenuBackgroundImage1a");
            BackgroundTexture2 = _contentResourceManager.Load<Texture2D>("MenuBackgroundImage1b");

            // Make sure init background has an opacity alpha channel data!
            /*{
                Color[] tmpData = new Color[_backgroundTexture.Height * _backgroundTexture.Width];
                _backgroundTexture.GetData<Color>(tmpData);

                for (int i = 0; i < tmpData.Length; i++)
                {
                    tmpData[i].A = 75;
                }

                _backgroundTexture.SetData<Color>(tmpData);

            }   */        

            _spriteBatch = ScreenManager.SpriteBatch;

        }


        /// <summary>
        /// Unloads graphics content for this screen.
        /// </summary>
        public sealed override void UnloadContent()
        {
            // 1/6/2010 - Unload Backgrounds in Content Manager.
            if (_contentResourceManager != null)
            {
                _contentResourceManager.Unload();
            }
            _contentResourceManager = null;
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Updates the <see cref="BackgroundScreen"/>. Unlike most <see cref="GameScreen"/>, this should not
        /// transition off even if it has been covered by another screen: it is
        /// supposed to be covered, after all! This overload forces the  <paramref name="coveredByOtherScreen"/> parameter
        /// to false in order to stop the base Update method wanting to transition off.
        /// </summary>
        /// <param name="coveredByOtherScreen">Another <see cref="GameScreen"/> has focus?</param>
        /// <param name="gameTime">Instance of <see cref="gameTime"/></param>
        /// <param name="otherScreenHasFocus">Is covered by another <see cref="GameScreen"/>?</param>
        public sealed override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

        }

        // 2/20/2009
        /// <summary>
        /// Draw the animated <see cref="BackgroundScreen"/> effect.
        /// </summary>
        /// <param name="gameTime">Instance of <see cref="gameTime"/>.</param>
        public sealed override void Draw3D(GameTime gameTime)
        {
            ScreenManager.DrawBackground(ScreenManager.GraphicsDevice);
        }
        
        /// <summary>
        /// Draws the <see cref="BackgroundScreen"/>.
        /// </summary>
        /// <param name="gameTime">Instance of <see cref="gameTime"/>.</param> 
        public sealed override void Draw2D(GameTime gameTime)
        {
            // 6/16/2012: Updated to check new animated flag.
            // 9/1/2009
            if (DrawBackgroundAnimated)
                DrawBackgroundScreenAnimated(this);
            else
                DrawBackgroundScreen(this);
        }

        // 9/1/2009; 6/16/2012: Updated to 'Animated' in name.
        // 9/6/2008: Updated to optimize memory.
        /// <summary>
        /// Draws the <see cref="BackgroundScreen"/> screen.
        /// </summary>
        /// <param name="screen"><see cref="BackgroundScreen"/> to draw</param>
        private static void DrawBackgroundScreenAnimated(BackgroundScreen screen)
        {
            // 4/29/2010 - Cache
            var spriteBatch = _spriteBatch;
            try // 1/5/2010
            {
                var viewport = screen.ScreenManager.GraphicsDevice.Viewport;
                var fullscreen = new Rectangle {X = 0, Y = 0, Width = viewport.Width, Height = viewport.Height};

                var fade = screen.TransitionAlpha;

                // XNA 4.0 Updates
                //spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None); // 1/5/2010 -was SaveStateMode.none
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend); // 1/5/2010 -was SaveStateMode.none

                var tmpColorOverride = ColorWhite;
                tmpColorOverride.R = tmpColorOverride.G = tmpColorOverride.B = fade;
                spriteBatch.Draw(BackgroundTexture, fullscreen, null, tmpColorOverride, 0, Vector2Zero,
                                         SpriteEffects.None, 1); // 1 is back.              

                spriteBatch.Draw(BackgroundTexture2, fullscreen, null, ColorWhite, 0, Vector2Zero,
                                         SpriteEffects.None, 0.9f); // 0 is front.


                spriteBatch.End();
            }
            // 4/28/2010 - Captures NullRefExp, and check if _spriteBatch is null.
            catch (NullReferenceException)
            {
                Debug.WriteLine("(DrawMenuScreen) threw the 'NullRefExp' error, in the DrawMenuScreen class.");

                if (_spriteBatch == null)
                {
                    _spriteBatch = (SpriteBatch) screen.ScreenManager.Game.Services.GetService(typeof (SpriteBatch));

                    Debug.WriteLine("(_spriteBatch) was null; however, got new reference from services.");
                }

            }
            // 1/5/2010 - Captures the SpriteBatch InvalidOpExp.
            catch (InvalidOperationException)
            {
                Debug.WriteLine("(DrawMenuScreen) threw the 'InvalidOpExp' error, in the DrawMenuScreen class.");

            }
        }

        // 6/16/2012
        /// <summary>
        /// Draws the <see cref="BackgroundScreen"/> screen.
        /// </summary>
        /// <param name="screen"><see cref="BackgroundScreen"/> to draw</param>
        private static void DrawBackgroundScreen(BackgroundScreen screen)
        {
            // 4/29/2010 - Cache
            var spriteBatch = _spriteBatch;
            try // 1/5/2010
            {
                var viewport = screen.ScreenManager.GraphicsDevice.Viewport;
                var fullscreen = new Rectangle { X = 0, Y = 0, Width = viewport.Width, Height = viewport.Height };

                // XNA 4.0 Updates
                // Draw the text.
                //spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None);
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

                // 11/13/2009 - Draw Background texture
                spriteBatch.Draw(BackgroundTexture, fullscreen, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.1f); // 1 is back.

                spriteBatch.End();
            }
            // 4/28/2010 - Captures NullRefExp, and check if _spriteBatch is null.
            catch (NullReferenceException)
            {
                Debug.WriteLine("(DrawBackgroundScreen) threw the 'NullRefExp' error, in the DrawMenuScreen class.");

                if (_spriteBatch == null)
                {
                    _spriteBatch = (SpriteBatch)screen.ScreenManager.Game.Services.GetService(typeof(SpriteBatch));

                    Debug.WriteLine("(DrawBackgroundScreen) was null; however, got new reference from services.");
                }

            }
            // 1/5/2010 - Captures the SpriteBatch InvalidOpExp.
            catch (InvalidOperationException)
            {
                Debug.WriteLine("(DrawBackgroundScreen) threw the 'InvalidOpExp' error, in the DrawMenuScreen class.");

            }
        }

        #endregion

        #region Dispose
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this final dispose?</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            // dispose managed resources
            if (BackgroundTexture != null)
                BackgroundTexture.Dispose();

            if (BackgroundTexture2 != null)
                BackgroundTexture2.Dispose();

            BackgroundTexture = null;
            BackgroundTexture2 = null;
            // free native resources
        }

        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
