#region File Description
//-----------------------------------------------------------------------------
// NetworkBusyScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion


using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TWEngine.ScreenManagerC;

namespace TWEngine.Networking
{
    /// <summary>
    /// When an asynchronous network operation (for instance searching for or joining a
    /// session) is in progress, we want to display some sort of busy indicator to let
    /// the user know the game hasn't just locked up. We also want to make sure they
    /// can't pick some other menu option before the current operation has finished.
    /// This screen takes care of both requirements in a single stroke. It monitors
    /// the <see cref="IAsyncResult"/> returned by an asynchronous network call, displaying a busy
    /// indicator for as long as the call is still in progress. When it notices the
    /// <see cref="IAsyncResult"/> has completed, it raises an event to let the game know it should
    /// proceed to the next step, after which the busy screen automatically goes away.
    /// Because this screen is on top of all others for as long as the asynchronous
    /// operation is in progress, it automatically takes over all user input,
    /// preventing any other menu entries being selected until the operation completes.
    /// </summary>
    class NetworkBusyScreen : GameScreen
    {
        #region Fields

        IAsyncResult _asyncResult;
        Texture2D _gradientTexture;
        Texture2D _networkBusyIconTexture;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the operation is considered completed.
        /// </summary>
        public event EventHandler<OperationCompletedEventArgs> OperationCompleted;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a network busy screen for the specified asynchronous operation.
        /// </summary>
        /// <param name="asyncResult"><see cref="IAsyncResult"/> interface reference</param>
        public NetworkBusyScreen(IAsyncResult asyncResult)
        {
            _asyncResult = asyncResult;

            IsPopup = true;

            TransitionOnTime = TimeSpan.FromSeconds(0.1);
            TransitionOffTime = TimeSpan.FromSeconds(0.2);
        }


        /// <summary>
        /// Loads graphics content for this screen. This uses the shared <see cref="ContentManager"/>
        /// provided by the <see cref="Game"/> class, so the content will remain loaded throughout game life-span.
        /// Whenever a subsequent <see cref="NetworkBusyScreen"/> tries to load this same content,
        /// it will just get back another reference to the already loaded data.
        /// </summary>
        /// <param name="contentManager"> </param>
        public sealed override void LoadContent(ContentManager contentManager)
        {
            // 6/17/2012 - Check if null
            if (contentManager == null) return;

            // 4/20/2010 - Updated to use global 'ContentTexturesLoc'.
            _gradientTexture = contentManager.Load<Texture2D>(TemporalWars3DEngine.ContentTexturesLoc + @"\Textures\gradient"); // was @"ContentTextures\Textures\gradient"
            
            _networkBusyIconTexture = TemporalWars3DEngine.ContentResourceManager.Load<Texture2D>("NetworkBusyIcon");
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the <see cref="NetworkBusyScreen"/>, by checking if the asynchronous operation completed.
        /// </summary>
        public sealed override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Has our asynchronous operation completed?
            if ((_asyncResult == null) || !_asyncResult.IsCompleted) return;

            // If so, raise the OperationCompleted event.
            if (OperationCompleted != null)
            {
                OperationCompleted(this,
                                   new OperationCompletedEventArgs(_asyncResult));
            }

            ExitScreen();

            _asyncResult = null;
        }

        // 4/28/2010: Updated the method to be 'Draw2D', and not 'Draw3D'; otherwise gets called out of order, which puts
        //            the 'Busy' icon behind all the screens!
        /// <summary>
        /// Draws the <see cref="NetworkBusyScreen"/> icon, spinning some sprite texture in circles.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public sealed override void Draw2D(GameTime gameTime)
        {
            var spriteBatch = ScreenManager.SpriteBatch;
            var font = ScreenManager.Font;

            var message = Resources.NetworkBusy;

            const int hPad = 32;
            const int vPad = 16;

            // Center the message text in the viewport.
            var viewport = ScreenManager.GraphicsDevice.Viewport;
            var viewportSize = new Vector2(viewport.Width, viewport.Height);
            var textSize = font.MeasureString(message);

            // Add enough room to spin the sprite 'Wait' icon.
            var spriteSize = new Vector2(_networkBusyIconTexture.Width);

            textSize.X = Math.Max(textSize.X, spriteSize.X);
            textSize.Y += spriteSize.Y + vPad;

            var textPosition = (viewportSize - textSize) / 2;

            // The background includes a border somewhat larger than the text itself.
            var backgroundRectangle = new Rectangle((int)textPosition.X - hPad,
                                                          (int)textPosition.Y - vPad,
                                                          (int)textSize.X + hPad * 2,
                                                          (int)textSize.Y + vPad * 2);

            // Fade the popup alpha during transitions.
            var color = new Color(255, 255, 255, TransitionAlpha);

            spriteBatch.Begin();

            // Draw the background rectangle.
            spriteBatch.Draw(_gradientTexture, backgroundRectangle, null, color, 0, Vector2.Zero, SpriteEffects.None, 0.1f); // 4/28/2010

            // Draw the message box text.
            spriteBatch.DrawString(font, message, textPosition, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0.1f); // 4/28/2010 

            // Draw the spinning 'Sprite' progress indicator.
            var spriteRotation = (float)gameTime.TotalGameTime.TotalSeconds * 3;

            var spritePosition = new Vector2(textPosition.X + textSize.X/2,
                                             textPosition.Y + textSize.Y -
                                             spriteSize.Y/2);

            // 4/28/2010 - Updated to use the Divide overload.
            Vector2 spriteSizeInHalf; 
            Vector2.Divide(ref spriteSize, 2, out spriteSizeInHalf);

            spriteBatch.Draw(_networkBusyIconTexture, spritePosition, null, color, spriteRotation,
                             spriteSizeInHalf, 1, SpriteEffects.None, 0.1f);

            spriteBatch.End();
        }


        #endregion
    }
}
