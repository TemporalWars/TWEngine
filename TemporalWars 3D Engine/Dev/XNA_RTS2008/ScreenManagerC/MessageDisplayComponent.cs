#region File Description
//-----------------------------------------------------------------------------
// MessageDisplayComponent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Globalization;

namespace ImageNexus.BenScharbach.TWEngine.ScreenManagerC
{
    /// <summary>
    /// The <see cref="MessageDisplayComponent"/> implements the <see cref="IMessageDisplay"/> interface. 
    /// This is used to show <see cref="NotificationMessage"/> when interesting events occur, for instance when
    /// gamers join or leave the network session
    /// </summary>
    public class MessageDisplayComponent : DrawableGameComponent, IMessageDisplay
    {
        #region Fields

        private static SpriteBatch _spriteBatch;
        private SpriteFont _font;

        // List of the currently visible notification _messages.
        private List<NotificationMessage> _messages = new List<NotificationMessage>();

        // Coordinates threadsafe access to the message list.
        private object _syncObject = new object();

        // Tweakable Settings control how long each message is visible.
        private static readonly TimeSpan FadeInTime = TimeSpan.FromSeconds(0.25);
        private static readonly TimeSpan ShowTime = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan FadeOutTime = TimeSpan.FromSeconds(0.5);

        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new <see cref="MessageDisplayComponent"/>.
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        public MessageDisplayComponent(Game game)
            : base(game)
        {
            // Register ourselves to implement the IMessageDisplay service.
            game.Services.AddService(typeof(IMessageDisplay), this);

            // 4/28/2010 - Set draw order to 251; otherwise message does not show up!
            DrawOrder = 251;
        }


        /// <summary>
        /// Load graphics content for the message display.
        /// </summary>
        protected sealed override void LoadContent()
        {
            //_spriteBatch = new SpriteBatch(GraphicsDevice);
            // 4/28/2010 - Updated to retrieve global 'SpriteBatch'.
            _spriteBatch = (SpriteBatch) Game.Services.GetService(typeof (SpriteBatch));

            // 4/6/2010: Updated to use 'ContentMiscLoc' global var.
            _font = Game.Content.Load<SpriteFont>(TemporalWars3DEngine.ContentMiscLoc + @"\Fonts\menufont");
        }

        // 9/9/2008
        /// <summary>
        /// Unload content and dispose of resources
        /// </summary>
        protected sealed override void UnloadContent()
        {
            // Remove IMessageDisplay Service
            Game.Services.RemoveService(typeof(IMessageDisplay));

            if (_messages != null)
                _messages.Clear();

            _syncObject = null;
            _messages = null;
            _spriteBatch = null;
            _font = null;
           
            base.UnloadContent();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the collection of <see cref="NotificationMessage"/> instances.
        /// </summary>    
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>   
        public override void Update(GameTime gameTime)
        {
            lock (_syncObject)
            {
                var index = 0;
                float targetPosition = 0;

                // Update each message in turn.
                while (index < _messages.Count)
                {
                    // 4/28/2010 - Refactored out code into static method.
                    UpdateMessages(ref index, ref targetPosition, gameTime, _messages);
                } // End While
            } // End Lock
        }

        // 4/28/2010
        /// <summary>
        /// Helper method, used to update a <see cref="NotificationMessage"/>.
        /// </summary>
        /// <param name="index">Index in collection</param>
        /// <param name="targetPosition">Location to place message</param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="notificationMessages">Collection of <see cref="NotificationMessage"/></param>
        private static void UpdateMessages(ref int index, ref float targetPosition, 
                                          GameTime gameTime, IList<NotificationMessage> notificationMessages)
        {
            var message = notificationMessages[index];

            // Gradually slide the message toward its desired Position.
            var positionDelta = targetPosition - message.Position;

            var velocity = (float)gameTime.ElapsedGameTime.TotalSeconds * 2;

            message.Position += positionDelta * Math.Min(velocity, 1);

            // Update the age of the message.
            message.Age += gameTime.ElapsedGameTime;

            if (message.Age < ShowTime + FadeOutTime)
            {
                // This message is still alive.
                index++;

                // Any subsequent _messages should be positioned below
                // this one, unless it has started to fade out.
                if (message.Age < ShowTime)
                    targetPosition++;
            }
            else
            {
                // This message is old, and should be removed.
                notificationMessages.RemoveAt(index);
            }
            
        }

        /// <summary>
        /// Draws the collection of <see cref="NotificationMessage"/> instances.
        /// </summary>  
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>     
        public sealed override void Draw(GameTime gameTime)
        {
#if DEBUG
            // 5/26/2010 - DEBUG
            StopWatchTimers.StartStopWatchInstance(StopWatchName.GameDrawLoop_Main_Messages);
#endif
            const float scale = 0.75f;

            lock (_syncObject)
            {
                // Early out if there are no _messages to display.
                var count = _messages.Count; // 4/28/2010 - Cache
                if (count == 0) return;

                _spriteBatch.Begin();

                // 8/22/2008 - Changed into For-Loop, rather than FOREACH.
                // Draw each message in turn.
                DrawMessages(count, scale, _messages, _font, GraphicsDevice);               

                _spriteBatch.End();
            }

#if DEBUG
            // 5/26/2010 - DEBUG
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.GameDrawLoop_Main_Messages);
#endif
        }

        // 4/28/2010
        /// <summary>
        /// Helper method, which draws the <see cref="NotificationMessage"/> in the collection.
        /// </summary>
        /// <param name="count">Message count in collection</param>
        /// <param name="scale">Scale to use for text</param>
        /// <param name="notificationMessages">Collection of <see cref="NotificationMessage"/></param>
        /// <param name="spriteFont">Font to use</param>
        /// <param name="graphicsDevice"><see cref="GraphicsDevice"/> instance</param>
        private static void DrawMessages(int count, float scale, IList<NotificationMessage> notificationMessages, 
                                         SpriteFont spriteFont, GraphicsDevice graphicsDevice)
        {
            var position = Vector2.Zero;
            position.X = graphicsDevice.Viewport.Width / 2.0f; // was graphicsDevice.Viewport.Width - 100; 4/28/2010 - Decided to put at center of screen!
            position.Y = 0;
           
            for (var i = 0; i < count; i++)
            {
                // Compute the alpha of this message.
                byte alpha = 255;

                if (notificationMessages[i].Age < FadeInTime)
                {
                    // Fading in.
                    alpha = (byte)(255 * notificationMessages[i].Age.TotalSeconds /
                                   FadeInTime.TotalSeconds);
                }
                else if (notificationMessages[i].Age > ShowTime)
                {
                    // Fading out.
                    var fadeOut = ShowTime + FadeOutTime - notificationMessages[i].Age;

                    alpha = (byte)(255 * fadeOut.TotalSeconds /
                                   FadeOutTime.TotalSeconds);
                }

                // Compute the message Position.
                position.Y = 80 + notificationMessages[i].Position * spriteFont.LineSpacing * scale;

                // Compute an Origin value to right align each message.
                var origin = spriteFont.MeasureString(notificationMessages[i].Text);
                origin.Y = 0;

                // Draw the message text, with a drop shadow.                    
                _spriteBatch.DrawString(spriteFont, notificationMessages[i].Text, position + Vector2.One,
                                        new Color(0, 0, 0, alpha), 0,
                                        origin, scale, SpriteEffects.None, 0);

                _spriteBatch.DrawString(spriteFont, notificationMessages[i].Text, position,
                                        new Color(255, 255, 255, alpha), 0,
                                        origin, scale, SpriteEffects.None, 0);

            } // End For-loop
        }

        #endregion

        #region Implement IMessageDisplay

        /// <summary>
        /// Shows a new notification message.
        /// </summary>
        /// <param name="message">message to display</param>
        /// <param name="parameters">replaces the format item in a specified string with the text equivalent of the value
        /// of a corresponding object instance in a specified array. A specified parameter supplies culture-specific 
        /// formatting information.</param>       
        public void ShowMessage(string message, params object[] parameters)
        {
            // 5/3/2009: FXCop - Update to include the 'CultureInfo.CurrentCulture'.
            var formattedMessage = string.Format(CultureInfo.CurrentCulture, message, parameters);

            lock (_syncObject)
            {
                float startPosition = _messages.Count;

                _messages.Add(new NotificationMessage(ref formattedMessage, startPosition));
            }
        }


        #endregion

        #region Nested Types

        
        /// <summary>
        /// Helper class stores the Position and text of a single notification message.
        /// </summary>
        class NotificationMessage
        {
            public readonly string Text;
            public float Position;
            public TimeSpan Age;


            public NotificationMessage(ref string text, float position)
            {
                Text = text;
                Position = position;
                Age = TimeSpan.Zero;
            }
        }


        #endregion
    }
}
