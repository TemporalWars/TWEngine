using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TWEngine.ScreenManagerC;

namespace TWEngine.GameScreens.Generic
{
    /// <summary>
    /// A popup <see cref="MessageBoxScreen"/>, used to display "are you sure?"
    /// confirmation messages.
    /// </summary>
    public class MessageBoxScreen : GameScreen
    {
        #region Fields

        // 4/28/2010 - SpriteBatch local instance, used for nesting.
        private static SpriteBatch _spriteBatch;

        // 6/18/2012 - StringBuilder
        private readonly StringBuilder _messageSb;
        private Color _textColor = Color.White;

        #endregion

        #region Events

        // 6/18/2012
        /// <summary>
        /// Occurs when the Initialize method is triggerd.
        /// </summary>
        public static event EventHandler Initialized;

        /// <summary>
        /// Occurs when the user selects the 'Accept'.
        /// </summary>
        public event EventHandler<EventArgs> Accepted;

        /// <summary>
        /// Occurs when the user selects the 'Cancel'.
        /// </summary>
        public event EventHandler<EventArgs> Cancelled;

        #endregion

        #region Properties

        // 6/18/2012
        /// <summary>
        /// Gets or sets the <see cref="SpriteFont"/> to use.
        /// </summary>
        /// <remarks>
        /// If this is left NULL, then the <see cref="ScreenManager"/> default font will be used.
        /// </remarks>
        public SpriteFont Font { get; set; }

        // 6/18/2012
        /// <summary>
        /// Set or Get the current <see cref="Texture2D"/> background image.
        /// </summary>
        public Texture2D BackgroundTexture { get; set; }

        // 6/18/2012
        /// <summary>
        /// Gets or sets the color for the message text.
        /// </summary>
        /// <remarks>
        /// Defaults to Color.Navy.
        /// </remarks>
        public Color TextColor
        {
            get { return _textColor; }
            set { _textColor = value; }
        }

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor automatically includes the standard "A=ok, B=cancel"
        /// usage text prompt.
        /// </summary>
        /// <param name="message">Message to use</param>
        public MessageBoxScreen(string message)
            : this(message, true)
        { }


        /// <summary>
        /// Constructor lets the caller specify whether to include the standard
        /// "A=ok, B=cancel" usage text prompt.  Also, the <see cref="GameScreen.TransitionOnTime"/> and
        /// <see cref="GameScreen.TransitionOffTime"/> are set to 0.2 seconds.
        /// </summary>
        /// <param name="message">Message to use</param>
        /// <param name="includeUsageText">Include usage text?</param>
        public MessageBoxScreen(string message, bool includeUsageText)
        {
            // 6/18/2012 - use a stringbuilder
            _messageSb = includeUsageText ? new StringBuilder(message + Resources.MessageBoxUsage) : new StringBuilder(message);

            IsPopup = true;

            TransitionOnTime = TimeSpan.FromSeconds(0.2);
            TransitionOffTime = TimeSpan.FromSeconds(0.2);
        }

        // 6/18/2012 - Overload with Texture2D
        /// <summary>
        /// Constructor lets the caller specify whether to include the standard
        /// "A=ok, B=cancel" usage text prompt.  Also, the <see cref="GameScreen.TransitionOnTime"/> and
        /// <see cref="GameScreen.TransitionOffTime"/> are set to 0.2 seconds.
        /// </summary>
        /// <param name="message">Message to use</param>
        /// <param name="includeUsageText">Include usage text?</param>
        /// <param name="backgroundTexture"> </param>
        public MessageBoxScreen(string message, bool includeUsageText, Texture2D backgroundTexture)
            : this(message, includeUsageText)
        {
            var gameInstance = TemporalWars3DEngine.GameInstance;

            if (backgroundTexture == null)
            {
                BackgroundTexture = new Texture2D(gameInstance.GraphicsDevice, 1, 1, true, SurfaceFormat.Color);
                BackgroundTexture.SetData(new[] { new Color(0, 0, 0, 125) });
            }
            else
            {
                BackgroundTexture = backgroundTexture;
            }
        }

        // 6/18/2012 - Overload with Color
        /// <summary>
        /// Constructor lets the caller specify whether to include the standard
        /// "A=ok, B=cancel" usage text prompt.  Also, the <see cref="GameScreen.TransitionOnTime"/> and
        /// <see cref="GameScreen.TransitionOffTime"/> are set to 0.2 seconds.
        /// </summary>
        /// <param name="message">Message to use</param>
        /// <param name="includeUsageText">Include usage text?</param>
        /// <param name="backgroundColor"><see cref="Color"/> to use for menu background</param>
        public MessageBoxScreen(string message, bool includeUsageText, Color backgroundColor) 
            : this(message, includeUsageText)
        {
            SetBackgroundColor(backgroundColor);
        }

        // 6/18/2012
        /// <summary>
        /// Used to initialize content or game logic.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            // trigger the event
            OnInitialized(this, EventArgs.Empty);
        }

        /// <summary>
        /// Loads graphics content for this screen. This uses the shared <see cref="ContentManager"/>
        /// provided by the Game class, so the content will remain loaded forever.
        /// Whenever a subsequent <see cref="MessageBoxScreen"/> tries to load this same content,
        /// it will just get back another reference to the already loaded data.
        /// </summary>
        /// <param name="contentManager"> </param>
        public override void LoadContent(ContentManager contentManager)
        {
            // 6/17/2012 - Check if null
            if (contentManager == null) return;

            // 4/28/2010 - Create local copy of SpriteBatch, used for nesting.
            _spriteBatch = new SpriteBatch(ScreenManager.Game.GraphicsDevice);

            // 6/18/2012 - Updated to check if texture already set, before loading default.
            // 4/6/2010: Updated to use 'ContentTexturesLoc' global var.
            if (BackgroundTexture == null)
                BackgroundTexture = contentManager.Load<Texture2D>(TemporalWars3DEngine.ContentTexturesLoc + @"\Textures\gradient");
        }

        #endregion

        // 6/18/2012
        /// <summary>
        /// Sets the background to the given <paramref name="backgroundColor"/>.
        /// </summary>
        public void SetBackgroundColor(Color backgroundColor)
        {
            var gameInstance = TemporalWars3DEngine.GameInstance;

            BackgroundTexture = new Texture2D(gameInstance.GraphicsDevice, 1, 1, true, SurfaceFormat.Color);
            BackgroundTexture.SetData(new[] { backgroundColor });
        }

        #region Handle Input

        /// <summary>
        /// Responds to user input, accepting or cancelling the _message box.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="input"><see cref="InputState"/> instance</param>
        public sealed override void DoHandleInput(GameTime gameTime, InputState input)
        {
            if (input.MenuSelect)
            {
                // Raise the accepted event, then exit the _message box.
                if (Accepted != null)
                    Accepted(this, EventArgs.Empty);
            
                ExitScreen();
            }
            else if (input.MenuCancel)
            {
                // Raise the cancelled event, then exit the _message box.
                if (Cancelled != null)
                    Cancelled(this, EventArgs.Empty);

                ExitScreen();
            }
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draws the <see cref="MessageBoxScreen"/>, by calling the internal <see cref="DrawMessageBoxScreen"/>
        /// method.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public sealed override void Draw2D(GameTime gameTime)
        {
            // 4/29/2010 - Refactored draw code to new STATIC method.
            DrawMessageBoxScreen(this);
        }

        // 4/29/2010
        /// <summary>
        /// Helper method, which draws the <see cref="MessageBoxScreen"/>
        /// </summary>
        /// <param name="messageBoxScreen">This instance of <see cref="MessageBoxScreen"/></param>
        private static void DrawMessageBoxScreen(MessageBoxScreen messageBoxScreen)
        {
            var spriteBatch = _spriteBatch; // 4/28/2010 - Use local copy.
            var screenManager = messageBoxScreen.ScreenManager; // 6/18/2012
            var font = messageBoxScreen.Font ?? ScreenManager.Font; // 6/18/2012 - Updated with instance check for 'Font'
            var background = messageBoxScreen.BackgroundTexture; // 6/18/2012

            // Darken down any other screens that were drawn beneath the popup.
            screenManager.FadeBackBufferToBlack(messageBoxScreen.TransitionAlpha * 2 / 3);

            // Center the _message text in the viewport.
            var viewport = screenManager.GraphicsDevice.Viewport;
            var viewportSize = new Vector2(viewport.Width, viewport.Height);
            var textSize = font.MeasureString(messageBoxScreen._messageSb);
            var textPosition = (viewportSize - textSize) / 2;

            // The background includes a border somewhat larger than the text itself.
            const int hPad = 32;
            const int vPad = 16;

            var backgroundRectangle = new Rectangle((int)textPosition.X - hPad,
                                                    (int)textPosition.Y - vPad,
                                                    (int)textSize.X + hPad * 2,
                                                    (int)textSize.Y + vPad * 2);

            // Fade the popup alpha during transitions.
            var color = new Color(255, 255, 255, messageBoxScreen.TransitionAlpha);

            // 6/18/2012 - fade textColor
            messageBoxScreen._textColor.A = messageBoxScreen.TransitionAlpha;

            // XNA 4.0 Updates
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

            // Draw the background rectangle.
            spriteBatch.Draw(background, backgroundRectangle, null, color, 0, Vector2.Zero, SpriteEffects.None, 0.05f);

            // Draw the _message box text.
            spriteBatch.DrawString(font, messageBoxScreen._messageSb, textPosition, messageBoxScreen._textColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0); // 4/28/2010

            spriteBatch.End();
        }

        #endregion

        // 6/18/2012
        /// <summary>
        /// Occurs when the Initialized is triggered.
        /// </summary>
        private static void OnInitialized(MessageBoxScreen messageBoxScreen, EventArgs e)
        {
            EventHandler handler = Initialized;
            if (handler != null) handler(messageBoxScreen, e);
        }
    }
}
