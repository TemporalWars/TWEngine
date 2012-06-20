#region File Description

//-----------------------------------------------------------------------------
// SinglePlayerScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TWEngine.GameScreens;
using TWEngine.GameScreens.Generic;
using TWEngine.Networking.Structs;
using TWEngine.ScreenManagerC;


namespace TWEngine.Networking
{
    /// <summary>
    /// The <see cref="SinglePlayerScreen"/> class, is used to draw the campaign maps for single player games.   
    /// </summary>
    internal sealed class SinglePlayerScreen : GameScreen
    {
        #region Fields

        // 2/20/2009 - MenuPanes class

        // 4/11/2009 - Save Ref to MenuEnty for MapPreviews
        private static MenuEntry _mapPreviewEntry;
        // 4/11/2009 - Save Ref to MenuEntries for Location-1/2.
        private readonly MenuEntry _mapLocation1Entry;
        private readonly MenuEntry _mapLocation2Entry;
        private readonly MenuPanes _menuPanes;

        // 4/8/2009 - Saves the SinglePlayer's 'Side' & 'Color' choices.
        private GamerInfo _gamerInfo;

        // 4/6/2010 - Updated to use 'ContentMapsLoc' global var.
        // 11/17/2009 - The path for the MapPreview folder for PC.
        private static readonly string MapsPreviewLoadPath = TemporalWars3DEngine.ContentMapsLoc + @"\SP\_MapPreviews\{0}_MMP";


        #endregion

        #region Initialization

        /// <summary>
        /// Constructs a new <see cref="SinglePlayerScreen"/> instance, by creating all required
        /// <see cref="MenuPaneItem"/> and <see cref="MenuEntry"/> collections.
        /// </summary>
        public SinglePlayerScreen()

        {
#if XBOX360
    // 7/18/2009 - Create instance of ResourceContentManager, used to get the MapPreviews
            ContentMapPreviews = new ResourceContentManager(TemporalWars3DEngine.GameInstance.Services, Resources.ResourceManager);
#endif

            // Instantiate MenuPanes
            _menuPanes = new MenuPanes();

            // 2/21/2009 - Show Maps list
            var menuPaneItem1 = new MenuPaneItem("Maps", new Vector2(100, 150), new Vector2(350, 300));
            var menuPaneItem2 = new MenuPaneItem("Color", new Vector2(850, 150), new Vector2(300, 325));
            var menuPaneItem3 = new MenuPaneItem("Side", new Vector2(850, 500), new Vector2(300, 150));
            var menuPaneItem4 = new MenuPaneItem("Map Preview", new Vector2(500, 150), new Vector2(300, 300));

            // Add 5 Colors 
            var menuEntry1 = new MenuEntry("FireBrick Red", new Rectangle(0, 60, 300, 40), new Vector2(15, 0));
            var menuEntry2 = new MenuEntry("Forest Green", new Rectangle(0, 60, 300, 40), new Vector2(15, 0));
            var menuEntry3 = new MenuEntry("Midnight Blue", new Rectangle(0, 60, 300, 40), new Vector2(15, 0));
            var menuEntry4 = new MenuEntry("Gold", new Rectangle(0, 60, 300, 40), new Vector2(15, 0));
            var menuEntry5 = new MenuEntry("OrangeRed", new Rectangle(0, 60, 300, 40), new Vector2(15, 0));
            var menuEntry6 = new MenuEntry("DarkOrchid", new Rectangle(0, 60, 300, 40), new Vector2(15, 0));

            //
            // Add a couple of menuPaneItems.
            //
            // MenuPane-1
            menuPaneItem1.ScreenManager =
                (ScreenManager) TemporalWars3DEngine.GameInstance.Services.GetService(typeof (ScreenManager));
            menuPaneItem1.SelectIndexChanged += MenuPaneItem1SelectIndexChanged;
            menuPaneItem1.IsSelected = true; // default to 1st menu.

            // Populate the MenuPaneItem-1 menu with MenuEntries for all MapNames.
            PopulateMapListView(ref menuPaneItem1);

            // MenuPane-2 (colors)
            menuPaneItem2.ScreenManager =
                (ScreenManager) TemporalWars3DEngine.GameInstance.Services.GetService(typeof (ScreenManager));
            menuPaneItem2.SelectIndexChanged += MenuPaneItem2SelectIndexChanged;


            // Add new MenuEntries to menuPane.
            menuPaneItem2.AddMenuEntry(menuEntry1);
            menuPaneItem2.AddMenuEntry(menuEntry2);
            menuPaneItem2.AddMenuEntry(menuEntry3);
            menuPaneItem2.AddMenuEntry(menuEntry4);
            menuPaneItem2.AddMenuEntry(menuEntry5);
            menuPaneItem2.AddMenuEntry(menuEntry6);

            // MenuPane-3 (Sides)
            menuPaneItem3.ScreenManager =
                (ScreenManager) TemporalWars3DEngine.GameInstance.Services.GetService(typeof (ScreenManager));
            menuPaneItem3.SelectIndexChanged += MenuPaneItem3SelectIndexChanged;

            // Add 2 side entires
            menuEntry1 = new MenuEntry("Side 1", new Rectangle(0, 60, 300, 40), new Vector2(15, 0));
            menuEntry2 = new MenuEntry("Side 2", new Rectangle(0, 60, 300, 40), new Vector2(15, 0));
            // Add new MenuEntries to menuPane.
            menuPaneItem3.AddMenuEntry(menuEntry1);
            menuPaneItem3.AddMenuEntry(menuEntry2);

            // MenuPane-4 (Map Preview)
            menuPaneItem4.ScreenManager =
                (ScreenManager) TemporalWars3DEngine.GameInstance.Services.GetService(typeof (ScreenManager));
            menuPaneItem4.SelectIndexChanged += MenuPaneItem4SelectIndexChanged;

            // Add Background menu entry
            _mapPreviewEntry = new MenuEntry("", new Rectangle(10, 60, 175, 175), new Vector2(15, 0));
            try
            {
#if XBOX360
                _mapPreviewEntry.MenuEntryBackgroundCustom = ContentMapPreviews.Load<Texture2D>(menuPaneItem1.MenuEntries[0].Text + "_MMP");
                // 7/18/2009
#else
                _mapPreviewEntry.MenuEntryBackgroundCustom =
                    TemporalWars3DEngine.GameInstance.Content.Load<Texture2D>(
                        String.Format(MapsPreviewLoadPath, menuPaneItem1.MenuEntries[0].Text));
#endif
            }
            catch
            {
                _mapPreviewEntry.MenuEntryBackgroundCustom = null;
            }

            _mapPreviewEntry.IsSelectable = false; // shows custom background instead, when set to False.
            _mapPreviewEntry.IsNeverSelectable = true; // 9/1/2009 - Keeps this entry from every being set to TRUE!
            _mapLocation1Entry = new MenuEntry("Loc 1", new Rectangle(200, 60, 100, 40), new Vector2(15, 0));
            _mapLocation2Entry = new MenuEntry("Loc 2", new Rectangle(200, 60, 100, 40), new Vector2(15, 0));


            // Add new MenuEntries to menuPane.
            menuPaneItem4.AddMenuEntry(_mapPreviewEntry);
            menuPaneItem4.AddMenuEntry(_mapLocation1Entry);
            menuPaneItem4.AddMenuEntry(_mapLocation2Entry);
           
            // 9/1/2009 - Skip '_MapPreview' MenuEntry.
            menuPaneItem4.SetStartingMenuEntryPosition(1); 


            // Add MenuPaneItems to MenuPane
            _menuPanes.MenuPaneItems.Add(menuPaneItem1);
            _menuPanes.MenuPaneItems.Add(menuPaneItem4);
            _menuPanes.MenuPaneItems.Add(menuPaneItem2);
            _menuPanes.MenuPaneItems.Add(menuPaneItem3);


            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            // 4/8/2009 - Set Default values for GamerInfo
            _gamerInfo.PlayerSide = 1;
            _gamerInfo.PlayerColor = Color.Firebrick;
            GetColorName(ref _gamerInfo.PlayerColor, out _gamerInfo.ColorName);
        }

        #region EventHandlers

        /// <summary>
        /// <see cref="EventHandler"/> for <see cref="MenuPaneItem"/> 'selectIndexChanged', which sets the map to play with.
        /// </summary>
        private static void MenuPaneItem1SelectIndexChanged(object sender, EventArgs e)
        {
            var menuPaneItem = (MenuPaneItem) sender;
            if (menuPaneItem == null) return; // 4/20/2010

            // Set MapName to selected SceneItemOwner
            var mapName = menuPaneItem.MenuEntries[menuPaneItem.SelectedEntry].Text;

            // 4/8/2009 - Set the Map Preview Image
            try
            {
#if XBOX360
                _mapPreviewEntry.MenuEntryBackgroundCustom = ContentMapPreviews.Load<Texture2D>(mapName + "_MMP");
                // 7/18/2009
#else

                _mapPreviewEntry.MenuEntryBackgroundCustom =
                    TemporalWars3DEngine.GameInstance.Content.Load<Texture2D>(
                        String.Format(MapsPreviewLoadPath, mapName));
#endif
            }
            catch (ContentLoadException)
            {
                _mapPreviewEntry.MenuEntryBackgroundCustom = null;
            }

            // 4/8/2009 - Sets the MapMarkerPositions for current map
            SetMapMarkerPositions(mapName, "SP", _mapPreviewEntry);
        }

        // 4/7/2009
        /// <summary>
        /// <see cref="EventHandler"/> for <see cref="MenuPaneItem"/> 'selectIndexChanged', which will set
        /// the Color the player choose into their gamer 'Tag'.
        /// </summary>
        private void MenuPaneItem2SelectIndexChanged(object sender, EventArgs e)
        {
            var menuPaneItem = (MenuPaneItem) sender;
            if (menuPaneItem == null) return; // 4/20/2010

            // 4/20/2010 - Cache
            var menuEntry = menuPaneItem.MenuEntries[menuPaneItem.SelectedEntry];
            if (menuEntry == null) return;

            switch (menuEntry.Text)
            {
                case "FireBrick Red":
                    _gamerInfo.PlayerColor = Color.Firebrick;
                    break;
                case "Forest Green":
                    _gamerInfo.PlayerColor = Color.ForestGreen;
                    break;
                case "Midnight Blue":
                    _gamerInfo.PlayerColor = Color.MidnightBlue;
                    break;
                case "Gold":
                    _gamerInfo.PlayerColor = Color.Gold;
                    break;
                case "OrangeRed":
                    _gamerInfo.PlayerColor = Color.OrangeRed;
                    break;
                case "DarkOrchid":
                    _gamerInfo.PlayerColor = Color.DarkOrchid;
                    break;
                default:
                    break;
            }

            // set color name
            GetColorName(ref _gamerInfo.PlayerColor, out _gamerInfo.ColorName);
        }

        // 4/7/2009
        /// <summary>
        /// <see cref="EventHandler"/> for <see cref="MenuPaneItem"/> 'selectIndexChanged', which will set
        /// the Side the player choose into their gamer 'Tag'.
        /// </summary>
        private void MenuPaneItem3SelectIndexChanged(object sender, EventArgs e)
        {
            var menuPaneItem = (MenuPaneItem) sender;
            if (menuPaneItem == null) return; // 4/20/2010

            // 4/20/2010 - Cache
            var menuEntry = menuPaneItem.MenuEntries[menuPaneItem.SelectedEntry];
            if (menuEntry == null) return;

            switch (menuEntry.Text)
            {
                case "Side 1":
                    _gamerInfo.PlayerSide = 1;
                    break;
                case "Side 2":
                    _gamerInfo.PlayerSide = 2;
                    break;
            }
        }

        // 4/8/2009
        /// <summary>
        /// <see cref="EventHandler"/> for <see cref="MenuPaneItem"/>  'selectIndexChanged', which will set
        /// the Location the player choose into their gamer 'Tag'.
        /// </summary>
        private void MenuPaneItem4SelectIndexChanged(object sender, EventArgs e)
        {
            var menuPaneItem = (MenuPaneItem) sender;
            if (menuPaneItem == null) return; // 4/20/2010

            // 4/20/2010 - Cache
            var menuEntry = menuPaneItem.MenuEntries[menuPaneItem.SelectedEntry];
            if (menuEntry == null) return;

            switch (menuEntry.Text)
            {
                case "Loc 1":
                    _gamerInfo.PlayerLocation = 1;
                    break;
                case "Loc 2":
                    _gamerInfo.PlayerLocation = 2;
                    break;
                default:
                    break;
            }
        }
      

        #endregion

        /// <summary>
        /// Populates a <see cref="MenuPaneItem"/> with a <see cref="MenuEntry"/> for each
        /// map name found within the 'ContentMaps' folder.
        /// </summary>
        /// <param name="menuPaneItem"><see cref="MenuPaneItem"/> instance</param>
        private void PopulateMapListView(ref MenuPaneItem menuPaneItem)
        {
            string[] mapNames;
            PopulateMapListView(ref menuPaneItem, "SP", out mapNames);
           
        }
        

        /// <summary>
        /// <see cref="EventHandler"/> for <see cref="MenuPaneItem"/> 'eventEntry' selected.
        /// </summary>
        protected override void MenuEntrySelected(object sender, EventArgs e)
        {
            var menuEntry = (MenuEntry)sender;
            if (menuEntry == null) return; // 4/20/2010

            // Load Game using given map name, and GamerInfo data.
            LoadingScreen.Load(ScreenManager, true, new TerrainScreen(menuEntry.Text, _gamerInfo));
        }
       

        #endregion

        #region Update

        /// <summary>
        /// Updates the <see cref="SinglePlayerScreen"/>.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="otherScreenHasFocus">If other screen has focus?</param>
        /// <param name="coveredByOtherScreen">If covered by other screen?</param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                    bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Call MenuPanes
            if (_menuPanes != null) _menuPanes.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }


        /// <summary>
        /// Handles user input for all the local gamers in the session. Unlike most
        /// screens, which use the <see cref="InputState"/> class to combine input data from all
        /// gamepads, the lobby needs to individually mark specific players as ready,
        /// so it loops over all the local gamers and reads their inputs individually.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="inputState"><see cref="InputState"/> instance</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="inputState"/> is null.</exception>
        public override void DoHandleInput(GameTime gameTime, InputState inputState)
        {
            // 4/20/2010 - Check if null
            if (inputState == null)
                throw new ArgumentNullException("inputState",@"The parameter 'inputState' cannot be null!");

            // 3/23/2009 - Allow user to exit screen.
            if (inputState.MenuCancel)
            {
                OnCancel();
            }

            // Call MenuPanes
            if (_menuPanes != null) _menuPanes.DoHandleInput(gameTime, inputState);
        }


        /// <summary>
        /// Handler for when the user has cancelled the menu.
        /// </summary>
        private void OnCancel()
        {
            ExitScreen();
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draws the <see cref="SinglePlayerScreen"/>.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public override void Draw2D(GameTime gameTime)
        {
            base.Draw2D(gameTime);

            // Call MenuPanes
            if (_menuPanes != null) _menuPanes.Draw2D(gameTime);
        }

        #endregion
    }
}