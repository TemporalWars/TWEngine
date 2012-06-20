#region File Description
//-----------------------------------------------------------------------------
// MenuScreen.cs
//
// XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TWEngine.ScreenManagerC;
using TWEngine.ScreenManagerC.Enums;

#if XBOX360
using TWEngine.Common.Extensions;
#endif

#endregion

namespace TWEngine.GameScreens.Generic
{
    /// <summary>
    /// Base class for <see cref="MenuScreen"/> that contain a menu of options. The user can
    /// move up and down to select an entry, or cancel to back out of the screen.
    /// </summary>
    public abstract class MenuScreen : GameScreen, IDisposable
    {
        #region Fields

        // 2/17/2009 - Add Background texture for menuScreen, which will default
        //             to be a simple transparent dark blue background.
        private Texture2D _menuBackground;
        private Rectangle _menuBackgroundRectSize = new Rectangle(0, 0, 600, 300); // 2/17/2009
        private readonly Vector2 _menuBackgroundPosition; // 2/17/2009
        private Color _titleColor = Color.Navy; // 2/20/2009
        private Color _titleColorSelected = Color.DarkRed;// 4/8/2009
        private Vector4 _menuTitleMargin = new Vector4(5, 0, 5, 15); // 6/17/2012 - As Top, Right, Bottom, Left.

        private readonly List<MenuEntry> _menuEntries = new List<MenuEntry>();

        private float _titleScale = 1.25f; // 4/8/2009  

        // 2/20/2009 - Position of MenuScreen; is set via inherited classes through constructor.
        private readonly Vector2 _initMenuPosition = new Vector2(100, 150);

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
        /// Gets or sets the margin to place around the Menu's Title.
        /// </summary>
        /// <remarks>
        /// Set as rotation of clock; Top, Right, Bottom, Left is X, Y, Z, and W respectively.
        /// </remarks>
        public Vector4 MenuTitleMargin
        {
            get { return _menuTitleMargin; }
            set { _menuTitleMargin = value; }
        }

        // 6/17/2012
        /// <summary>
        /// Gets or sets to center the Menu's title.
        /// </summary>
        public bool CenterMenuTitle { get; set; }

        /// <summary>
        /// Gets the list of <see cref="MenuEntry"/>, so internal classes can read the menu contents.
        /// </summary>
        internal ReadOnlyCollection<MenuEntry> MenuEntries { get; private set; }

        // 2/20/2009
        /// <summary>
        /// Returns the index of <see cref="MenuEntry"/> selection.
        /// </summary>
        internal int SelectedEntry { get; private set; }       

        // 4/8/2009
        /// <summary>
        /// Is MenuPane Selected?
        /// </summary>
        internal bool IsSelected { get; set; }

        /// <summary>
        /// The title text scale to use.
        /// </summary>
        protected float TitleScale
        {
            get { return _titleScale; }
            set { _titleScale = value; }
        }

        // 6/15/2009
        ///<summary>
        /// The <see cref="MenuEntry"/> main Title.
        ///</summary>
        public string MenuTitle { get; set; }

        // 6/17/2012
        /// <summary>
        /// Gets or sets the color for the menu titles in a non-selected state.
        /// </summary>
        /// <remarks>
        /// Defaults to Color.Navy.
        /// </remarks>
        public Color TitleColor
        {
            get { return _titleColor; }
            set { _titleColor = value; }
        }

        // 6/17/2012
        /// <summary>
        /// Gets or sets the color for the menu titles in a selected state.
        /// </summary>
        /// <remarks>
        /// Defaults to Color.DarkRed
        /// </remarks>
        public Color TitleColorSelected
        {
            get { return _titleColorSelected; }
            set { _titleColorSelected = value; }
        }

        #endregion

        #region Initialization

        // 6/16/2012 - Refactored out common code from public contructors, and made private.
        /// <summary>
        /// Private constructor, called by the two other public constructors.
        /// </summary>
        /// <param name="menuTitle"><see cref="MenuScreen"/> Title</param>
        /// <param name="menuPosition">Position to place <see cref="MenuScreen"/>; if left NULL, then will be automatically centered.</param>
        /// <param name="menuSize"><see cref="MenuScreen"/> Background size.</param>
        private MenuScreen(string menuTitle, Vector2? menuPosition, Vector2 menuSize)
        {
            // 9/1/2009 - Cache
            var screenScale = TemporalWars3DEngine.ScreenScale;
            var gameInstance = TemporalWars3DEngine.GameInstance;

            // 6/29/2009 - Set ReadOnlyCollection wrapper
            MenuEntries = new ReadOnlyCollection<MenuEntry>(_menuEntries);
            MenuTitle = menuTitle;

            // 4/8/2009 - Size the MenuSize with given ScreenScale
            menuSize.X *= screenScale;
            menuSize.Y *= screenScale;

            // 2/20/2009 - Store MenuSize given
            _menuBackgroundRectSize.Width = (int)menuSize.X;
            _menuBackgroundRectSize.Height = (int)menuSize.Y;

            // 2/20/2009 - Sets Position given; is Null, then set to be in middle of screen.

            if (menuPosition != null)
            {
                // 4/8/2009 - Size the menuPosition with given ScreenScale
                var tmpMenuPosition = menuPosition.Value;
                tmpMenuPosition.X *= screenScale;
                tmpMenuPosition.Y *= screenScale;
                menuPosition = tmpMenuPosition;

                // Set Background Position
                _menuBackgroundPosition = menuPosition.Value;

            }
            else
            {

                //
                // Calculate middle of screen, using size of menu screen.
                //

                var screenWidth = gameInstance.GraphicsDevice.PresentationParameters.BackBufferWidth;
                var screenHeight = gameInstance.GraphicsDevice.PresentationParameters.BackBufferHeight;

                // Set Background Position
                _menuBackgroundPosition.X = (screenWidth / 2) - ((int)menuSize.X / 2);
                _menuBackgroundPosition.Y = (screenHeight / 2) - ((int)menuSize.Y / 2);

            }

            // Set MenuEntry Position
            _initMenuPosition.X = _menuBackgroundPosition.X;
            _initMenuPosition.Y = _menuBackgroundPosition.Y;

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        // 2/20/2009: Updated to include 2nd/3rd parameters, 'menuPosition' & 'menuSize'.
        // 2/21/2009: Updated to include 4th parameter, 'backgroundTexture'.
        /// <summary>
        /// <see cref="MenuScreen"/> Abstract class contructor, which creates the internal <see cref="ReadOnlyCollection{MenuEntry}"/>,
        /// sets misc attributes like screen scale and size, and sets the <see cref="GameScreen.TransitionOnTime"/> and <see cref="GameScreen.TransitionOffTime"/>
        /// properties to 0.5 seconds.
        /// </summary>
        /// <param name="menuTitle"><see cref="MenuScreen"/> Title</param>
        /// <param name="menuPosition">Position to place <see cref="MenuScreen"/>; if left NULL, then will be automatically centered.</param>
        /// <param name="menuSize"><see cref="MenuScreen"/> Background size.</param>
        /// <param name="backgroundTexture">Content location to load background texture from.</param>
        protected MenuScreen(string menuTitle, Vector2? menuPosition, Vector2 menuSize, string backgroundTexture) 
            : this(menuTitle, menuPosition, menuSize)
        {
            var gameInstance = TemporalWars3DEngine.GameInstance;

            // 5/3/2009: FXCOP - Use String.IsNullOrEmpty.
            // 2/21/2009 - If string empty, then create generic background; otherwise, load
            //             given background texture
            if (string.IsNullOrEmpty(backgroundTexture))
            {
                // 9/22/2010 - XNA 4.0 Updates
                // 2/17/2009 - create background texture            
                //_menuBackground = new Texture2D(gameInstance.GraphicsDevice, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
                _menuBackground = new Texture2D(gameInstance.GraphicsDevice, 1, 1, true, SurfaceFormat.Color);
                _menuBackground.SetData(new[] { new Color(0, 0, 0, 125) });
            }
            else
            {
                _menuBackground = gameInstance.Content.Load<Texture2D>(backgroundTexture);
            }
           
        }

        // 6/15/2012
        /// <summary>
        /// <see cref="MenuScreen"/> Abstract class contructor, which creates the internal <see cref="ReadOnlyCollection{MenuEntry}"/>,
        /// sets misc attributes like screen scale and size, and sets the <see cref="GameScreen.TransitionOnTime"/> and <see cref="GameScreen.TransitionOffTime"/>
        /// properties to 0.5 seconds.
        /// </summary>
        /// <param name="menuTitle"><see cref="MenuScreen"/> Title</param>
        /// <param name="menuPosition">Position to place <see cref="MenuScreen"/>; if left NULL, then will be automatically centered.</param>
        /// <param name="menuSize"><see cref="MenuScreen"/> Background size.</param>
        /// <param name="backgroundTexture">Instance of <see cref="Texture2D"/> for background.</param>
        protected MenuScreen(string menuTitle, Vector2? menuPosition, Vector2 menuSize, Texture2D backgroundTexture)
            : this(menuTitle, menuPosition, menuSize)
        {
            var gameInstance = TemporalWars3DEngine.GameInstance;

            // 5/3/2009: FXCOP - Use String.IsNullOrEmpty.
            // 2/21/2009 - If string empty, then create generic background; otherwise, load
            //             given background texture
            if (backgroundTexture == null)
            {
                // 9/22/2010 - XNA 4.0 Updates
                // 2/17/2009 - create background texture            
                //_menuBackground = new Texture2D(gameInstance.GraphicsDevice, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
                _menuBackground = new Texture2D(gameInstance.GraphicsDevice, 1, 1, true, SurfaceFormat.Color);
                _menuBackground.SetData(new[] { new Color(0, 0, 0, 125) });
            }
            else
            {
                _menuBackground = backgroundTexture;
            }
        }

        // 6/17/2012
        /// <summary>
        /// <see cref="MenuScreen"/> Abstract class contructor, which creates the internal <see cref="ReadOnlyCollection{MenuEntry}"/>,
        /// sets misc attributes like screen scale and size, and sets the <see cref="GameScreen.TransitionOnTime"/> and <see cref="GameScreen.TransitionOffTime"/>
        /// properties to 0.5 seconds.
        /// </summary>
        /// <param name="menuTitle"><see cref="MenuScreen"/> Title</param>
        /// <param name="menuPosition">Position to place <see cref="MenuScreen"/>; if left NULL, then will be automatically centered.</param>
        /// <param name="menuSize"><see cref="MenuScreen"/> Background size.</param>
        /// <param name="backgroundColor"><see cref="Color"/> to use for menu background</param>
        protected MenuScreen(string menuTitle, Vector2? menuPosition, Vector2 menuSize, Color backgroundColor)
            : this(menuTitle, menuPosition, menuSize)
        {
            var gameInstance = TemporalWars3DEngine.GameInstance;

            // 9/22/2010 - XNA 4.0 Updates
            // 2/17/2009 - create background texture            
            //_menuBackground = new Texture2D(gameInstance.GraphicsDevice, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            _menuBackground = new Texture2D(gameInstance.GraphicsDevice, 1, 1, true, SurfaceFormat.Color);
            _menuBackground.SetData(new[] { backgroundColor });
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Responds to user input, changing the selected entry and accepting
        /// or cancelling the menu.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="input"><see cref="InputState"/> instance</param>
        public override void DoHandleInput(GameTime gameTime, InputState input)
        {
            // 6/29/2009
            if (_menuEntries == null || _menuEntries.Count == 0)
                return;

            // 4/29/2010 - Refactored out code to new STATIC method.
            DoHandleInput(this, input);
        }

        // 4/29/2010
        /// <summary>
        /// Helper method, which handle the check for 'MenuUp', 'MenuDown', 'MenuSelect'
        /// and 'MenuCancel'.
        /// </summary>
        /// <param name="menuScreen">This instance of <see cref="MenuScreen"/></param>
        /// <param name="input"><see cref="InputState"/> instance</param>
        private static void DoHandleInput(MenuScreen menuScreen, InputState input)
        {
            // Move to the previous menu entry?
            var menuEntries = menuScreen._menuEntries; // 4/29/2010 - Cache
            if (input.MenuUp)
            {
                // 6/15/2009 - Stop at a 'IsSelectable' entry.
                var isSelectable = false;
                while (!isSelectable)
                {
                    // 2/20/2009
                    menuScreen.SelectedEntry--;

                    if (menuScreen.SelectedEntry < 0)
                        menuScreen.SelectedEntry = menuEntries.Count - 1;

                    // 6/15/2009 - Check if 'IsSelectable' entry.
                    if (menuEntries[menuScreen.SelectedEntry].IsSelectable)
                        isSelectable = true;
                }
            }

            // Move to the next menu entry?
            if (input.MenuDown)
            {
                // 6/15/2009 - Stop at a 'IsSelectable' entry.
                var isSelectable = false;
                while (!isSelectable)
                {
                    // 2/20/2009
                    menuScreen.SelectedEntry++;

                    if (menuScreen.SelectedEntry >= menuEntries.Count)
                        menuScreen.SelectedEntry = 0;

                    // 6/15/2009 - Check if 'IsSelectable' entry.
                    if (menuEntries[menuScreen.SelectedEntry].IsSelectable)
                        isSelectable = true;
                }
            }

            // Accept or cancel the menu?
            if (input.MenuSelect)
            {
                menuScreen.OnSelectEntry(menuScreen.SelectedEntry);
            }
            else if (input.MenuCancel)
            {
                menuScreen.OnCancel();
            }
        }


        /// <summary>
        /// Handler for when the user has chosen a <see cref="MenuEntry"/>, which
        /// triggers the <see cref="MenuEntry.OnSelectEntry"/> event.
        /// </summary>
        /// <param name="entryIndex">Index to the menu entries collection.</param>
        protected virtual void OnSelectEntry(int entryIndex)
        {
            _menuEntries[SelectedEntry].OnSelectEntry();
        }


        /// <summary>
        /// Handler for when the user has cancelled the <see cref="MenuScreen"/>, which
        /// calls the <see cref="GameScreen.ExitScreen"/> method.
        /// </summary>
        protected virtual void OnCancel()
        {
            ExitScreen();
        }


        /// <summary>
        /// Helper overload makes it easy to use <see cref="MenuScreen.OnCancel()"/> as a <see cref="MenuEntry"/> 
        /// <see cref="EventHandler"/>.
        /// </summary>
        /// <param name="e">An <see cref=" EventArgs"/> that contains no event data.</param>
        /// <param name="sender">The source of the event.</param>
        protected void OnCancel(object sender, EventArgs e)
        {
            OnCancel();
        }


        #endregion

        #region Update and Draw

        /// <summary>
        /// Updates the <see cref="MenuScreen"/>, by iterating the internal collection of <see cref="MenuEntry"/> and
        /// calling the 'Update' method for each.
        /// </summary>  
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="otherScreenHasFocus">Another <see cref="GameScreen"/> has focus?</param>
        /// <param name="coveredByOtherScreen">Is covered by another <see cref="GameScreen"/>?</param>      
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // 4/29/2010 - Refactored out collection code to new STATIC method.
            UpdateMenuEntries(this, gameTime);
        }


        // 4/29/2010
        /// <summary>
        /// Helper method, which iterates the internal collection of <see cref="MenuEntry"/>, calling
        /// the Update method for each.
        /// </summary>
        /// <param name="menuScreen">This instance of <see cref="MenuScreen"/></param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private static void UpdateMenuEntries(MenuScreen menuScreen, GameTime gameTime)
        {
            // Update each nested MenuEntry object.
            var menuEntries = menuScreen._menuEntries; // 4/29/2010 - Cache
            var count = menuEntries.Count; // 8/14/2009
            for (var i = 0; i < count; i++)
            {
                var isSelectedL = menuScreen.IsActive && (i == menuScreen.SelectedEntry);

                // 4/29/2010 - Cache
                var menuEntry = menuEntries[i];
                if (menuEntry == null) continue;

                menuEntry.Update(menuScreen, isSelectedL, gameTime);
            }
        }


        /// <summary>
        /// Draws the <see cref="MenuScreen"/>, by drawing each <see cref="MenuEntry"/> contain within.
        /// </summary> 
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>            
        public override void Draw2D(GameTime gameTime)
        {
            DrawMenuScreen(this, gameTime);
        }

        // 9/1/2009
        /// <summary>
        /// Helper method, which iterates the internal collection of <see cref="MenuEntry"/> and
        /// calls the Draw method for each.
        /// </summary>
        /// <param name="screen"><see cref="MenuScreen"/> instance</param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private static void DrawMenuScreen(MenuScreen screen, GameTime gameTime)
        {
            var spriteBatch = ScreenManager.SpriteBatch; // 4/29/2010

            try // 1/1/2010
            {
                var screenScale = TemporalWars3DEngine.ScreenScale; // 6/17/2012
                var font = screen.Font ?? ScreenManager.Font; // 6/17/2012 - Updated with instance check for 'Font'
                var position = screen._initMenuPosition;

                // Make the menu slide into place during transitions, using a
                // power curve to make things look more interesting (this makes
                // the movement slow down as it nears the end).
                var transitionOffset = (float) Math.Pow(screen.TransitionPosition, 2);

                if (screen.ScreenState == ScreenState.TransitionOn)
                    position.X -= transitionOffset*256;
                else
                    position.X += transitionOffset*512;

                // 6/17/2012 - Add check to allow spacing between title and menu entries
                position.Y += screen.MenuTitleMargin.Z; // Z = Bottom

                // XNA 4.0 Updates
                // 2/17/2009: Updated to use the 'AlphaBlend' mode.
                //spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None);
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

                // Draw each menu entry in turn.
                var menuEntries = screen._menuEntries; // 4/29/2010
                var count = menuEntries.Count; // 8/14/2009
                for (var i = 0; i < count; i++)
                {
                    var menuEntry = menuEntries[i];

                    var isSelectedL = screen.IsActive && (i == screen.SelectedEntry);

                    menuEntry.Draw(screen, ref position, isSelectedL, gameTime);

                    position.Y += menuEntry.GetHeight(screen);
                }

                // 6/17/2012
                Vector2 titlePosition;
                Vector2 titleOrigin;
                var offsetPosition = Vector2.Zero;

                // 4/8/2009: Updated to use Vector2.Divide Ref version, to optimize on XBOX!
                // 2/20/2009 - Title screen location
                // Draw the menu title.  (533, 80)
                Vector2 tmpMeasureString = font.MeasureString(screen.MenuTitle);
                Vector2.Divide(ref tmpMeasureString, 2, out titleOrigin); // 4/8/2009
               
                var menuBackgroundPosition = screen._menuBackgroundPosition; // 6/17/2012 - cache
                if (screen.CenterMenuTitle)
                {
                    // take center of rectangle width
                    var rectangleWidthHalf = screen._menuBackgroundRectSize.Width/2;
                    offsetPosition.X = titleOrigin.X;

                    titlePosition = new Vector2
                    {
                        X = menuBackgroundPosition.X + rectangleWidthHalf, // No margin on X required.
                        Y = menuBackgroundPosition.Y + titleOrigin.Y + screen.MenuTitleMargin.X
                    };
                }
                else
                {
                    titlePosition = new Vector2
                    {
                        X = menuBackgroundPosition.X + screen.MenuTitleMargin.W,
                        Y = menuBackgroundPosition.Y + screen.MenuTitleMargin.X
                    };
                }

                screen._titleColor.A = screen.TransitionAlpha;
                titlePosition.Y -= transitionOffset*100;

                // 2/17/2009 - Draw MenuScreen Background           
                screen._menuBackgroundRectSize.X = (int) menuBackgroundPosition.X;
                screen._menuBackgroundRectSize.Y = (int) menuBackgroundPosition.Y;
                spriteBatch.Draw(screen._menuBackground, screen._menuBackgroundRectSize, null,
                                                      Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.55f); // 4/28/2010 - Updated from 1

                // Draw Title of MenuScreen
                spriteBatch.DrawString(font, screen.MenuTitle, titlePosition,
                                                            screen.IsSelected
                                                                ? screen.TitleColorSelected
                                                                : screen.TitleColor, 0,
                                                            offsetPosition,
                                                            screen._titleScale*screenScale,
                                                            SpriteEffects.None, 0.5f); // 4/28/2010 - Updated from 0

                spriteBatch.End();
                
            }
            // 1/5/2010 - Captures the SpriteBatch InvalidOpExp.
            catch (InvalidOperationException)
            {
                Debug.WriteLine("(DrawMenuScreen) threw the 'InvalidOpExp' error, in the DrawMenuScreen class.");

                // Dispose of SpriteBatch
                spriteBatch.Dispose();
            }
        }

        #endregion

        // 2/20/2009
        ///<summary>
        /// Adds a <see cref="MenuEntry"/> to the internal collection.
        ///</summary>
        ///<param name="menuEntry"><see cref="MenuEntry"/> to add</param>
        ///<returns>true/false of result</returns>
        public bool AddMenuEntry(MenuEntry menuEntry)
        {
            try
            {
                // Add entry to the menu.           
                _menuEntries.Add(menuEntry);
                return true;
            }
            catch
            {
                Debug.WriteLine("AddMenuEntry method, in MenuScreen class, failed to add the given menu entry.");
                return false;
            }
        }

        // 1/12/2011
        ///<summary>
        /// Clears internal collection of all <see cref="MenuEntry"/>.
        ///</summary>
        public void ClearMenuEntries()
        {
            _menuEntries.Clear();
        }

        // 9/1/2009
        /// <summary>
        /// Allows setting the default <see cref="MenuEntry"/> to start from.
        /// </summary>
        /// <remarks>The <see cref="SelectedEntry"/> property is set to the given <paramref name="startingPosition"/> value.</remarks>
        /// <param name="startingPosition"><see cref="MenuEntry"/> position to start from.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startingPosition"/> is not between 9 and <see cref="MenuEntries"/> total count.</exception>
        public void SetStartingMenuEntryPosition(int startingPosition)
        {
            if (startingPosition < 0 || startingPosition >= MenuEntries.Count)
                throw new ArgumentOutOfRangeException("startingPosition",
                                                      @"The starting position must be between 0 and menuEntries Count.");

            SelectedEntry = startingPosition;
        }

        // 4/8/2009
        /// <summary>
        /// Pulsates an output value, using the <paramref name="gameTime"/> and Sin.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="selectionFade">Selection fade value</param>
        /// <returns>scale float value</returns>
        public static float PulsateValue(GameTime gameTime, float selectionFade)
        {
            // Pulsate the size of the selected menu entry.
            var time = gameTime.TotalGameTime.TotalSeconds;

            var pulsate = (float)Math.Sin(time * 6) + 1;

            var scale = 1 + pulsate * 0.05f * selectionFade;
            return scale;
        }

        #region Find MenuEntry

        // 6/15/2009 - Find MenuEntry by name.
        ///<summary>
        /// Locates a <see cref="MenuEntry"/> by the given <paramref name="name"/>.
        ///</summary>
        ///<param name="name">Name of <see cref="MenuEntry"/> to locate</param>
        ///<returns>Index of <see cref="MenuEntry"/> in collection</returns>
        public int FindMenuEntryIndexByName(string name)
        {
            // Set name for predicate search.
            _menuEntryNameForSearch = name;

            // search using the List 'FindIndex' method.
            return _menuEntries.FindIndex(FindMenuEntryByNamePredicate);
        }

        private string _menuEntryNameForSearch;

        // 6/15/2009 - 
        /// <summary>
        /// Predicate delegate method for the FindIndex method.
        /// </summary>
        /// <param name="menuEntry"><see cref="MenuEntry"/> instance</param>
        /// <returns>true/false if found name</returns>
        private bool FindMenuEntryByNamePredicate(MenuEntry menuEntry)
        {
            if (string.IsNullOrEmpty(_menuEntryNameForSearch))
                return false;

            return menuEntry.Text == _menuEntryNameForSearch;
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

            if (_menuBackground != null)
                _menuBackground.Dispose();

            _menuBackground = null;
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
