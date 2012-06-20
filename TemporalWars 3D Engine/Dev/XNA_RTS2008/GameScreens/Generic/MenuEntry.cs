#region File Description
//-----------------------------------------------------------------------------
// MenuEntry.cs
//
// XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TWEngine.Networking;
using TWEngine.ScreenManagerC;

namespace TWEngine.GameScreens.Generic
{
    /// <summary>
    /// The <see cref="MenuEntry"/> represents a single entry in a <see cref="MenuScreen"/>. By default this
    /// just draws the entry text string, but it can be customized to display menu
    /// entries in different ways. This also provides an event that will be raised
    /// when the menu entry is selected.
    /// </summary>
    public class MenuEntry : IDisposable
    {
        #region Fields
        private Texture2D _menuEntryBackground;       
        private Texture2D _menuEntryBackgroundSelected; // 4/7/2009
        // Use the X/Y Position, which will be relative to the MenuScreen Position.
        private Rectangle _menuEntryBackgroundRectSize = new Rectangle(0, 20, 225, 40); // 2/17/2009
        private Vector2 _menuEntryBackgroundPosition = Vector2Zero;
        private Vector2 _menuEntryTextMargin = Vector2Zero;
        private Vector4 _menuEntryMargin = new Vector4(1); // 6/17/2012 - Add MenuEntry margin

        // 2/20/2009 - _menuEntryBackground Color Override
        private Color _menuEntryBackgroundColorOverride = Color.White;     
  
        // 4/7/2009 - MenuEntry Text Color for Selected/NotSelected
        private Color _selectedTextColor = Color.DarkBlue;
        private Color _unselectedTextColor = Color.WhiteSmoke;

        // 4/8/2009 - Is MenuEntry Selectable?  
        private bool _isSelectable = true;       
        // 4/8/2009 - Show MenuEntry Background?
        private bool _displayBackground = true;       

        // 4/8/200 9- Draw Markers on Custom Background?
        internal bool DrawMarker1;
        internal bool DrawMarker2;
        internal Vector2 DrawMarker1Position;
        internal Vector2 DrawMarker2Position;

        /// <summary>
        /// Tracks a fading selection effect on the entry.
        /// </summary>
        /// <remarks>
        /// The entries transition out of the selection effect when they are deselected.
        /// </remarks>
        float _selectionFade;

        private static readonly Vector2 Vector2Zero = Vector2.Zero;
       

        #endregion

        #region Properties

        // 6/17/2012
        /// <summary>
        /// Gets or sets the <see cref="SpriteFont"/> to use.
        /// </summary>
        /// <remarks>
        /// If this is left NULL, then the <see cref="ScreenManager"/> default font will be used.
        /// </remarks>
        public SpriteFont Font { get; set; }

        // 6/17/2012
        /// <summary>
        /// Gets or sets the margin to place around the MenuEntry.
        /// </summary>
        /// <remarks>
        /// Set as rotation of clock; Top, Right, Bottom, Left is X, Y, Z, and W respectively.
        /// </remarks>
        public Vector4 MenuEntryMargin
        {
            get { return _menuEntryMargin; }
            set { _menuEntryMargin = value; }
        }

        // 6/17/2012
        /// <summary>
        /// Gets or sets to center the MenuEntry's text.
        /// </summary>
        public bool CenterMenuText { get; set; }

        /// <summary>
        /// Gets or sets the text of this menu entry.
        /// </summary>
        public string Text { get; set; }

        ///<summary>
        /// Gets or sets the background color.
        ///</summary>
        public Color MenuEntryBackgroundColorOverride
        {
            get { return _menuEntryBackgroundColorOverride; }
            set { _menuEntryBackgroundColorOverride = value; }
        }

        ///<summary>
        /// Gets or sets the color to use for selected text.
        ///</summary>
        public Color SelectedTextColor
        {
            get { return _selectedTextColor; }
            set { _selectedTextColor = value; }
        }

        ///<summary>
        /// Gets or sets the color to use for unselected text.
        ///</summary>
        public Color UnselectedTextColor
        {
            get { return _unselectedTextColor; }
            set { _unselectedTextColor = value; }
        }

        // 4/8/2009
        /// <summary>
        ///  Background <see cref="Texture2D"/> for <see cref="MenuEntry"/>, which will default
        ///  to be a simple transparent dark blue background.
        /// </summary>
        public Texture2D MenuEntryBackgroundCustom { get; set; }

        // 4/8/2009
        ///<summary>
        /// Is this <see cref="MenuEntry"/> selectable?
        ///</summary>
        public bool IsSelectable
        {
            get { return _isSelectable; }
            set { _isSelectable = value; }
        }

        // 9/1/2009 - Is Never Selectable?  
        ///<summary>
        /// Is NEVER allowed to be set to <see cref="IsSelectable"/>.
        ///</summary>
        /// <remarks>
        /// This is needed, because the <see cref="Lobby2Screen"/> classes 'SetAllMenuEntriesToTrue'
        /// method, will set ALL entries to FALSE; however, with this new flag, menuEntries which should never
        /// be considered will be skipped!
        /// </remarks>
        public bool IsNeverSelectable { get; set; }


        // 4/8/2009
        ///<summary>
        /// Should display background?
        ///</summary>
        public bool DisplayBackground
        {
            get { return _displayBackground; }
            set { _displayBackground = value; }
        }

        #endregion

        #region Events


        /// <summary>
        /// Occurs when the <see cref="MenuEntry"/> is selected.
        /// </summary>
        public event EventHandler<EventArgs> Selected;


        /// <summary>
        /// Triggers the event <see cref="Selected"/>.
        /// </summary>
        protected internal virtual void OnSelectEntry()
        {
            if (Selected != null)
                Selected(this, EventArgs.Empty);
        }


        #endregion

        #region Initialization

        // 11/12/2009 - Updated by removing the Alpha blue background.
        // 2/17/2009 - Add new 2nd parameter of 'Rectangle' for _menuEntryBackground.
        // 2/20/2009 - Add new 3rd parameter of 'Vector2' for _menuEntryTextMargin.
        /// <summary>
        /// Constructs a new <see cref="MenuEntry"/> with the specified <paramref name="text"/>.
        /// </summary>
        /// <param name="text">Text to display</param>
        /// <param name="menuEntryBackgroundRect"><see cref="Rectangle"/> providing location and size</param>
        /// <param name="menuEntryTextMargin"><see cref="Vector2"/> with text margin</param>
        public MenuEntry(string text, Rectangle menuEntryBackgroundRect, Vector2 menuEntryTextMargin)
        {
            Text = text;

            // XNA 4.0 Updates
            // 2/17/2009 - create background texture            
            //_menuEntryBackground = new Texture2D(TemporalWars3DEngine.GameInstance.GraphicsDevice, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            _menuEntryBackground = new Texture2D(TemporalWars3DEngine.GameInstance.GraphicsDevice, 1, 1, true, SurfaceFormat.Color);

            _menuEntryBackground.SetData(new[] { new Color(0, 0, 60, 0) }); // 11/12/09 Alpha was 125

            // XNA 4.0 Updates
            // 4/7/2009 - create background texture for selected items.
            //_menuEntryBackgroundSelected = new Texture2D(TemporalWars3DEngine.GameInstance.GraphicsDevice, 1, 1, 1,TextureUsage.None, SurfaceFormat.Color);
            _menuEntryBackgroundSelected = new Texture2D(TemporalWars3DEngine.GameInstance.GraphicsDevice, 1, 1, true, SurfaceFormat.Color);

            _menuEntryBackgroundSelected.SetData(new[] { new Color(0, 0, 120, 0) }); // 11/12/09 Alpha was 150

            // save rectangle
            _menuEntryBackgroundRectSize = menuEntryBackgroundRect;
            // 2/20/2009 
            _menuEntryTextMargin = menuEntryTextMargin;
        }     


        #endregion

        #region Update and Draw

        /// <summary>
        /// Updates the <see cref="MenuEntry"/>.
        /// </summary>    
        /// <param name="screen"><see cref="MenuScreen"/> parent instance</param>
        /// <param name="isSelected">Is selected?</param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>    
        public virtual void Update(MenuScreen screen, bool isSelected, GameTime gameTime)
        {
            // When the menu selection changes, entries gradually fade between
            // their selected and deselected appearance, rather than instantly
            // popping to the new state.
            var fadeSpeed = (float)gameTime.ElapsedGameTime.TotalSeconds * 4;

            _selectionFade = isSelected ? Math.Min(_selectionFade + fadeSpeed, 1) : Math.Max(_selectionFade - fadeSpeed, 0);
        }


        // 9/9/2008 - Updated to optimize memory.
        /// <summary>
        /// Draws the <see cref="MenuEntry"/>. This can be overridden to customize the appearance.
        /// </summary>   
        /// <param name="screen"><see cref="MenuScreen"/> parent instance</param>
        /// <param name="position"><see cref="Vector3"/> position</param>
        /// <param name="isSelected">Is selected?</param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>      
        public virtual void Draw(MenuScreen screen, ref Vector2 position, bool isSelected, GameTime gameTime)
        {
            // 4/7/2009: Updated to use the 'selected'/'unselected' variables.
            // Draw the selected entry in yellow, otherwise white.
            DrawMenuEntry(this, screen, ref position, isSelected, gameTime);
        }

        // 9/1/2009
        /// <summary>
        /// Draws the selected menu entry screen.
        /// </summary>
        /// <param name="menuEntry"><see cref="MenuEntry"/> instance to draw</param>
        /// <param name="screen"><see cref="GameScreen"/> instance</param>
        /// <param name="position"><see cref="Vector2"/> position</param>
        /// <param name="isSelected">Is selected?</param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private static void DrawMenuEntry(MenuEntry menuEntry, GameScreen screen, ref Vector2 position, bool isSelected, GameTime gameTime)
        {
            // 9/1/2009 - Cache
            var spriteBatch = ScreenManager.SpriteBatch; // 4/29/2010
            var screenScale = TemporalWars3DEngine.ScreenScale;
            var font = menuEntry.Font ?? ScreenManager.Font; // 6/17/2012 - Updated with instance check for 'Font'
            var fontLineSpacingHalfed = (font.LineSpacing / 2); // 11/13/09
            var menuEntryBackgroundRectSize = menuEntry._menuEntryBackgroundRectSize; // 6/17/2012

            // 6/17/2012 - Adjust size for width margins
            var menuEntryBackgroundRectSizeAdjustedForMargin = menuEntryBackgroundRectSize;
            menuEntryBackgroundRectSizeAdjustedForMargin.X = (int) (menuEntryBackgroundRectSize.X +
                                                                    menuEntry._menuEntryMargin.W);
            menuEntryBackgroundRectSizeAdjustedForMargin.Y = (int) (menuEntryBackgroundRectSize.Y +
                                                                    menuEntry._menuEntryMargin.X);
            menuEntryBackgroundRectSizeAdjustedForMargin.Width = (int)(menuEntryBackgroundRectSize.Width -
                                                                         (menuEntry._menuEntryMargin.Y +
                                                                          menuEntry._menuEntryMargin.W));
            menuEntryBackgroundRectSizeAdjustedForMargin.Height = (int)(menuEntryBackgroundRectSize.Height -
                                                                         (menuEntry._menuEntryMargin.X +
                                                                          menuEntry._menuEntryMargin.Z));

            // Draw text, centered on the middle of each line.
            var offsetPosition = new Vector2 { X = 0, Y = fontLineSpacingHalfed };
            var color = isSelected ? menuEntry._selectedTextColor : menuEntry._unselectedTextColor;
            var scale = MenuScreen.PulsateValue(gameTime, menuEntry._selectionFade); // 4/29/2010 - Updated to new STATIC version.

            // 2/17/2009: Updated to set value directly, since this is now allowed in XNA 3.0.
            // Modify the alpha to fade text out during transitions.
            //color = new Color(color.R, color.G, color.B, screen.TransitionAlpha);
            color.A = screen.TransitionAlpha;

            // 2/17/2009 - Draw MenuEntry Background
            menuEntry._menuEntryBackgroundPosition.X = position.X + (menuEntryBackgroundRectSize.X + menuEntry._menuEntryMargin.W) * screenScale; // 4/8/2009 - Add 'ScreenScale'.
            menuEntry._menuEntryBackgroundPosition.Y = position.Y + (menuEntryBackgroundRectSize.Y + menuEntry._menuEntryMargin.X) * screenScale; // 4/8/2009 - Add 'ScreenScale'.

            // 4/8/2009 - Display background?
            if (menuEntry._displayBackground)
                if (menuEntry._isSelectable && !menuEntry.IsNeverSelectable)
                {
                    var entryBackgroundRectSizeAdjustedForMarginScaled = menuEntryBackgroundRectSizeAdjustedForMargin;
                    entryBackgroundRectSizeAdjustedForMarginScaled.Width = (int)(menuEntryBackgroundRectSizeAdjustedForMargin.Width * screenScale);
                    spriteBatch.Draw(isSelected ? menuEntry._menuEntryBackgroundSelected : menuEntry._menuEntryBackground,
                                                         menuEntry._menuEntryBackgroundPosition, entryBackgroundRectSizeAdjustedForMarginScaled, menuEntry._menuEntryBackgroundColorOverride
                                                          , 0, Vector2Zero, screenScale, SpriteEffects.None, 0.4f); // Last Parameter, 1 = back.
                }
                else
                {
                    // use a scale of 0.78, which is calculated as -> 175/225, where 225 is the std size of the mapPreview textures.
                    spriteBatch.Draw(menuEntry.MenuEntryBackgroundCustom ?? menuEntry._menuEntryBackground, menuEntry._menuEntryBackgroundPosition, null, Color.White, 0, 
                                                          Vector2Zero, 0.78f * screenScale, SpriteEffects.None, 0.4f);

                    // 4/8/2009 - Draw Map-Marker-Positions on map?
                    if (menuEntry.DrawMarker1)
                    {
                        // 9/1/2009 - Optimized to use the Vector2.Add method, which is faster on XBOX!
                        Vector2 position1;
                        Vector2.Add(ref menuEntry._menuEntryBackgroundPosition, ref menuEntry.DrawMarker1Position, out position1);

                        spriteBatch.DrawString(font, "1", position1, Color.Magenta, 0, offsetPosition, 0.78f * screenScale, SpriteEffects.None, 0.4f);
                    }
                    if (menuEntry.DrawMarker2)
                    {
                        // 9/1/2009 - Optimized to use the Vector2.Add method, which is faster on XBOX!
                        Vector2 position1;
                        Vector2.Add(ref menuEntry._menuEntryBackgroundPosition, ref menuEntry.DrawMarker2Position, out position1);

                        spriteBatch.DrawString(font, "2", position1, Color.Magenta, 0, offsetPosition, 0.78f * screenScale, SpriteEffects.None, 0.4f);
                    }
                }

            // 2/23/2009 - Make sure not Null, or crash on XBOX will occur!
            if (menuEntry.Text == null) menuEntry.Text = "empty";

            // 6/17/2012
            Vector2 textPosition;
            Vector2 textOrigin;

            // 4/8/2009: Updated to use Vector2.Divide Ref version, to optimize on XBOX!
            // 2/20/2009 - Title screen location
            // Draw the menu title.  (533, 80)
            Vector2 tmpMeasureString = font.MeasureString(menuEntry.Text);
            Vector2.Divide(ref tmpMeasureString, 2, out textOrigin); // 4/8/2009

            var menuBackgroundPosition = menuEntryBackgroundRectSizeAdjustedForMargin.Location; // 6/17/2012 - cache
            if (menuEntry.CenterMenuText)
            {
                // take center of rectangle width
                var rectangleWidthHalf = menuEntryBackgroundRectSizeAdjustedForMargin.Width / 2;
                offsetPosition.X = textOrigin.X;

                textPosition = new Vector2
                {
                    X = position.X + (menuBackgroundPosition.X + rectangleWidthHalf) * screenScale, // No margin on X required.
                    Y = position.Y + (menuBackgroundPosition.Y + textOrigin.Y + menuEntry._menuEntryTextMargin.Y) * screenScale
                };
            }
            else
            {
                textPosition = new Vector2
                {
                    X = position.X + (menuBackgroundPosition.X + menuEntry._menuEntryTextMargin.X) * screenScale,
                    Y = position.Y + (menuBackgroundPosition.Y + menuEntry._menuEntryTextMargin.Y) * screenScale
                };
            }

            // Draw the Text
            spriteBatch.DrawString(font, menuEntry.Text, textPosition, color, 0, offsetPosition, scale * screenScale, SpriteEffects.None, 0.4f);

            // 6/17/2012 - Add bottom margin before return Position ref.
            position.Y += menuEntry._menuEntryMargin.Z * screenScale;
        }

        // 6/17/2012
        /// <summary>
        /// Queries how much space this <see cref="MenuEntry"/> Text requires.
        /// </summary>
        /// <param name="screen"><see cref="MenuScreen"/> instance</param>
        /// <returns>Returns the height of the Text value.</returns>
        public virtual int GetTextHeight(MenuScreen screen)
        {
            // 6/17/2012 - Updated with instance check for 'Font'
            var font = Font ?? ScreenManager.Font;
            var lineSpacing = font.LineSpacing;
            var screenScale = TemporalWars3DEngine.ScreenScale;  // 4/8/2009 - Add 'ScreenScale'.
            var finalScale = lineSpacing * screenScale;

            return (int)finalScale;
        }

        // 6/17/2012 - Adjusted to return the height of the MenuEntry
        /// <summary>
        /// Queries how much space this <see cref="MenuEntry"/> requires.
        /// </summary>
        /// <param name="screen"><see cref="MenuScreen"/> instance</param>
        /// <returns>Returns the height value.</returns>
        public virtual int GetHeight(MenuScreen screen)
        {
            var screenScale = TemporalWars3DEngine.ScreenScale;  // 4/8/2009 - Add 'ScreenScale'.
            var finalHeight = _menuEntryBackgroundRectSize.Height * screenScale;

            return (int)finalHeight;
        }

        #endregion

        #region Dispose

        // 5/3/2009
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this final dispose?</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                if (MenuEntryBackgroundCustom != null)
                    MenuEntryBackgroundCustom.Dispose();

                if (_menuEntryBackground != null)
                    _menuEntryBackground.Dispose();
              
                if (_menuEntryBackgroundSelected != null)
                    _menuEntryBackgroundSelected.Dispose();


                // Null REfs
                MenuEntryBackgroundCustom = null;
                _menuEntryBackground = null;
                _menuEntryBackgroundSelected = null;
               
            }
            // free native resources
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        #endregion
    }
}
