#region File Description
//-----------------------------------------------------------------------------
// GameScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.GameScreens.Generic;
using ImageNexus.BenScharbach.TWEngine.HandleGameInput;
using ImageNexus.BenScharbach.TWEngine.ScreenManagerC.Enums;
using ImageNexus.BenScharbach.TWEngine.Terrain;
using ImageNexus.BenScharbach.TWEngine.Terrain.Structs;
using ImageNexus.BenScharbach.TWEngine.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ImageNexus.BenScharbach.TWEngine.ScreenManagerC
{

    /// <summary>
    /// A <see cref="GameScreen"/> is a single layer that has update and draw logic, and which
    /// can be combined with other layers to build up a complex menu system.
    /// For instance the main menu, the options menu, the "are you sure you
    /// want to quit" message box, and the main game itself are all implemented
    /// as screens.
    /// </summary>
    public abstract class GameScreen
    {   
    
        // 7/18/2009 - Content Resources for Map Previews
        /// <summary>
        /// Resources for <see cref="ContentMapPreviews"/>.
        /// </summary>
        protected static Microsoft.Xna.Framework.Content.ResourceContentManager ContentMapPreviews;


        #region Properties

        // 4/23/2011
        ///<summary>
        /// Sets DrawOrder.
        ///</summary>
        public int UseDrawOrder { get; internal set; }

        /// <summary>
        /// Normally when one <see cref="GameScreen"/> is brought up over the top of another,
        /// the first screen will transition off to make room for the new
        /// one. This property indicates whether the screen is only a small
        /// popup, in which case screens underneath it do not need to bother
        /// transitioning off.
        /// </summary>
        public bool IsPopup { get; internal set; }

        /// <summary>
        /// Indicates how long the <see cref="GameScreen"/> takes to
        /// transition on when it is activated.
        /// </summary>
        public TimeSpan TransitionOnTime
        {
            get { return _transitionOnTime; }
            protected set { _transitionOnTime = value; }
        }

        TimeSpan _transitionOnTime = TimeSpan.Zero;


        /// <summary>
        /// Indicates how long the <see cref="GameScreen"/> takes to
        /// transition off when it is deactivated.
        /// </summary>
        public TimeSpan TransitionOffTime
        {
            get { return _transitionOffTime; }
            protected set { _transitionOffTime = value; }
        }

        TimeSpan _transitionOffTime = TimeSpan.Zero;


        /// <summary>
        /// Gets the current position of the <see cref="GameScreen"/> transition, ranging
        /// from zero (fully active, no transition) to one (transitioned
        /// fully off to nothing).
        /// </summary>
        public float TransitionPosition
        {
            get { return _transitionPosition; }
            protected set { _transitionPosition = value; }
        }

        float _transitionPosition = 1;


        /// <summary>
        /// Gets the current alpha of the <see cref="GameScreen"/> transition, ranging
        /// from 255 (fully active, no transition) to 0 (transitioned
        /// fully off to nothing).
        /// </summary>
        public byte TransitionAlpha
        {
            get { return (byte)(255 - TransitionPosition * 255); }
        }


        /// <summary>
        /// Gets the current <see cref="GameScreen"/> transition state.
        /// </summary>
        public ScreenState ScreenState
        {
            get { return _screenState; }
            protected set { _screenState = value; }
        }

        ScreenState _screenState = ScreenState.TransitionOn;


        /// <summary>
        /// There are two possible reasons why a <see cref="GameScreen"/> might be transitioning
        /// off. It could be temporarily going away to make room for another
        /// screen that is on top of it, or it could be going away for good.
        /// This property indicates whether the screen is exiting for real:
        /// if set, the screen will automatically remove itself as soon as the
        /// transition finishes.
        /// </summary>
        public bool IsExiting { get; protected internal set; }

        /// <summary>
        /// Checks whether this screen is active and can respond to user input.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return !_otherScreenHasFocus &&
                       (_screenState == ScreenState.TransitionOn ||
                        _screenState == ScreenState.Active);
            }
        }

        bool _otherScreenHasFocus;


        /// <summary>
        /// Gets the <see cref="ScreenManager"/> that this <see cref="GameScreen"/> belongs to.
        /// </summary>
        public ScreenManager ScreenManager { get; internal set; }

        #endregion

        #region Initialization

        // 2/7/2011
        /// <summary>
        /// Used to initialize content or game logic.
        /// </summary>
        public virtual void Initialize()
        {
            // 4/23/2011 - Set Custom DrawOrder for this screen.
            if (UseDrawOrder != 0)
                ScreenManager.DrawOrder = UseDrawOrder;
        }
                
        // 6/17/2012 - Updated with parameter 'ContentManager'.
        /// <summary>
        /// Load graphics content for the <see cref="GameScreen"/>.
        /// </summary>
        /// <param name="contentManager"> </param>
        public virtual void LoadContent(ContentManager contentManager) { }


        /// <summary>
        /// Unload content for the <see cref="GameScreen"/>.
        /// </summary>
        public virtual void UnloadContent()
        {
            // 4/23/2011 - Reset Default DrawOrder when exiting this screen.
            if (ScreenManager != null)
                ScreenManager.ResetToDefaultDrawOrder();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows the <see cref="GameScreen"/> to run logic, such as updating the transition position.
        /// Unlike <see cref="HandleInput"/>, this method is called regardless of whether the screen
        /// is active, hidden, or in the middle of a transition.
        /// </summary>    
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="otherScreenHasFocus">Another <see cref="GameScreen"/> has focus?</param>
        /// <param name="coveredByOtherScreen">Is covered by another <see cref="GameScreen"/>?</param>     
        public virtual void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                      bool coveredByOtherScreen)
        {
            _otherScreenHasFocus = otherScreenHasFocus;
            bool result;
            if (IsExiting)
            {
                // If the screen is going away to die, it should transition off.
                _screenState = ScreenState.TransitionOff;
                UpdateTransition(this, ref gameTime, ref _transitionOffTime, 1, out result);
                if (!result)
                {
                    // When the transition finishes, remove the screen.
                    ScreenManager.RemoveScreen(this);
                }
            }
            else if (coveredByOtherScreen)
            {
                // If the screen is covered by another, it should transition off.
                UpdateTransition(this, ref gameTime, ref _transitionOffTime, 1, out result);
                _screenState = result ? ScreenState.TransitionOff : ScreenState.Hidden;
            }
            else
            {
                // Otherwise the screen should transition on and become active.
                UpdateTransition(this, ref gameTime, ref _transitionOnTime, -1, out result);
                _screenState = result ? ScreenState.TransitionOn : ScreenState.Active;
            }
        }

       
        /// <summary>
        /// Helper for updating the screen transition Position.
        /// </summary>        
        static void UpdateTransition(GameScreen screen, ref GameTime gameTime, ref TimeSpan time, int direction, out bool result)
        {
            // How much should we move by?
            float transitionDelta;

            // 11/19/2009 - Updated to use the Equals method.
            if (time.Equals(TimeSpan.Zero))
                transitionDelta = 1;
            else
                transitionDelta = (float)(gameTime.ElapsedGameTime.TotalMilliseconds /
                                          time.TotalMilliseconds);

            // Update the transition Position.
            screen._transitionPosition += transitionDelta * direction;

            // Did we reach the end of the transition?
            if ((screen._transitionPosition <= 0) || (screen._transitionPosition >= 1))
            {
                screen._transitionPosition = MathHelper.Clamp(screen._transitionPosition, 0, 1);
                //return false;
                result = false;
                return;
            }
            
            // Otherwise we are still busy transitioning.
            //return true;
            result = true;
            return;
        }


        /// <summary>
        /// Allows the <see cref="GameScreen"/> to handle user input. Unlike Update, this method
        /// is only called when the screen is active, and not when some other
        /// screen has taken the focus.
        /// </summary>
        /// <param name="gameTime">Instance of game.</param>
        /// <param name="input">Instance of <see cref="InputState"/>.</param>
        public virtual void DoHandleInput(GameTime gameTime, InputState input) { }


        /// <summary>
        /// This is called to draw the 3D World.
        /// </summary>
        /// <param name="gameTime">Instance of game.</param>
        public virtual void Draw3D(GameTime gameTime) { }

        /// <summary>
        /// This is called to draw the 3D scenery items.
        /// </summary>
        /// <param name="gameTime">Instance of game.</param>
        public virtual void Draw3DSceneryItems(GameTime gameTime) { }

        // 3/12/2009
        /// <summary>
        /// This is called to draw the 3D Selectable Items.
        /// </summary>
        /// <param name="gameTime">Instance of game.</param>
        public virtual void Draw3DSelectables(GameTime gameTime) { }

        // 2/18/2009
        /// <summary>
        /// This is called to draw the 3D World, for only items
        /// which use the 2-pass AlphaMap drawing method.
        /// </summary>
        /// <param name="gameTime">Instance of game.</param>
        public virtual void Draw3DAlpha(GameTime gameTime) { }

        // 2/19/2009
        /// <summary>
        /// This is called to draw the 3D World, for only items
        /// which have Illumination maps.
        /// </summary>
        /// <param name="gameTime">Instance of game.</param>
        public virtual void Draw3DIllumination(GameTime gameTime) { }

        // 2/18/2009
        /// <summary>
        /// This is called to draw the 2D info text and hud
        /// </summary>
        /// <param name="gameTime">Instance of game.</param>
        public virtual void Draw2D(GameTime gameTime) { }

        /// <summary>
        /// Populates a MenuPaneItem with a MenuEntry for each
        /// MapName found within the 'ContentMaps' folder.
        /// </summary>
        /// <param name="menuPaneItem">Instance of <see cref="MenuPaneItem"/>.</param>
        /// <param name="mapType">Map type of SP or MP (single or multi-player).</param>
        /// <param name="mapNames">(OUT) collection of map names.</param>
        protected virtual void PopulateMapListView(ref MenuPaneItem menuPaneItem, string mapType, out string[] mapNames)
        {
            var storageTool = new Storage();

            // 4/6/2010 - Updated to use 'ContentMapsLoc' global var.
            mapNames = storageTool.GetSavedMapNames(TemporalWars3DEngine.ContentMapsLoc + @"\" + mapType + @"\"); // 11/17/09 - Add 'MapType'.

            var length = mapNames.Length; // 4/28/2010 - Cache
            if (length < 1) return;

            // Populate Listview with MapNames,if any.
            for (var i = 0; i < length; i++)
            {
                // Create MenuEntry for given MapName
                var menuEntry = new MenuEntry(mapNames[i], new Rectangle(0, 60, 350, 40), new Vector2(15, 0));
                // Connect Event Handler for 'Selected'
                menuEntry.Selected += MenuEntrySelected;
                // Add MenuEntry into MenuPaneItem class.
                menuPaneItem.AddMenuEntry(menuEntry);
            }

        }

        // 9/1/2009
        /// <summary>
        /// <see cref="MenuEntry"/> <see cref="EventHandler"/>, for the PopulateMapListview <see cref="MenuEntry.Selected"/> event.
        /// </summary>
        /// <param name="e">An EventArgs that contains no event data.</param>
        /// <param name="sender">The source of the event.</param>
        protected virtual void MenuEntrySelected(object sender, EventArgs e)
        {
            return;
        }


        #endregion

        #region Public Methods


        /// <summary>
        /// Tells the <see cref="GameScreen"/> to go away. Unlike ScreenManager.RemoveScreen, which
        /// instantly kills the screen, this method respects the transition timings
        /// and will give the screen a chance to gradually transition off.
        /// </summary>
        public void ExitScreen()
        {
            if (TransitionOffTime == TimeSpan.Zero)
            {
                // If the screen has a zero transition Time, remove it immediately.
                ScreenManager.RemoveScreen(this);
            }
            else
            {
                // Otherwise flag that it should transition off and then exit.
                IsExiting = true;
            }
        }

        // 4/7/2009
        /// <summary>
        /// Returns the <paramref name="colorName"/> for a given color.
        /// </summary>
        /// <param name="color"><see cref="Color"/> to get name for</param>
        /// <param name="colorName">Name of given color</param>
        public static void GetColorName(ref Color color, out string colorName)
        {
            colorName = string.Empty;

            // Get Color name
            if (color == Color.Firebrick)
                colorName = "FireBrick Red";
            else if (color == Color.ForestGreen)
                colorName = "Forest Green";
            else if (color == Color.MidnightBlue)
                colorName = "Midnight Blue";
            else if (color == Color.Gold)
                colorName = "Gold";
            else if (color == Color.OrangeRed)
                colorName = "OrangeRed";
            else if (color == Color.DarkOrchid)
                colorName = "DarkOrchid";
            
        }

        // 4/9/2009
        /// <summary>
        /// Loads and sets the current maps 'Marker' positions.
        /// </summary>
        /// <param name="mapName">Name of map.</param>
        /// <param name="mapType">Map type of SP or MP (single or multi-player).</param>
        /// <param name="mapPreviewEntry">Instance of <see cref="MenuEntry"/>.</param>
        protected static void SetMapMarkerPositions(string mapName, string mapType, MenuEntry mapPreviewEntry)
        {
            MapMarkerPositions mapMarkerPositions;
            var storageTool = new Storage();
            if (TerrainShape.LoadMapMarkerPositionsData(storageTool, mapName, mapType, out mapMarkerPositions))
            {
                // Mark positions on preview map

                const int scale = 10;
                var screenScale = TemporalWars3DEngine.ScreenScale;
                var scaleFactor = 0.77f * screenScale;

                { // Marker Position 1
                    // 1st - get relative values
                    var relativeLoc1 = mapMarkerPositions.markerLoc1;
                    relativeLoc1.X /= mapMarkerPositions.mapSize * scale; //
                    relativeLoc1.Z /= mapMarkerPositions.mapSize * scale; //
                    relativeLoc1.X *= 225; // 225 is size of preview texture
                    relativeLoc1.Z *= 225; // 225 is size of preview texture

                    // use a scale of 0.77, which is calculated as -> 175/225, where 225 is the std size of the mapPreview textures.
                    Vector3.Multiply(ref relativeLoc1, scaleFactor, out relativeLoc1);

                    // 2nd - set MenuEntry to new location.
                    mapPreviewEntry.DrawMarker1 = true;
                    mapPreviewEntry.DrawMarker1Position.X = relativeLoc1.X;
                    mapPreviewEntry.DrawMarker1Position.Y = relativeLoc1.Z;
                }

                { // Marker Position 2
                    // 1st - get relative values
                    var relativeLoc2 = mapMarkerPositions.markerLoc2;
                    relativeLoc2.X /= mapMarkerPositions.mapSize * scale; //
                    relativeLoc2.Z /= mapMarkerPositions.mapSize * scale; // 
                    relativeLoc2.X *= 225; // 225 is size of preview texture
                    relativeLoc2.Z *= 225; // 225 is size of preview texture

                    // use a scale of 0.77, which is calculated as -> 175/225, where 225 is the std size of the mapPreview textures.
                    Vector3.Multiply(ref relativeLoc2, scaleFactor, out relativeLoc2);

                    // 2nd - set MenuEntry to new location.
                    mapPreviewEntry.DrawMarker2 = true;
                    mapPreviewEntry.DrawMarker2Position.X = relativeLoc2.X;
                    mapPreviewEntry.DrawMarker2Position.Y = relativeLoc2.Z;
                }

            }
            else // No Markers, so turn off
            {
                mapPreviewEntry.DrawMarker1 = false;
                mapPreviewEntry.DrawMarker2 = false;
            }
        }


        #endregion
    }
}
