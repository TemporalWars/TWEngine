#region File Description

//-----------------------------------------------------------------------------
// Lobby2Screen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

using System;
using System.Diagnostics;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.BeginGame.Enums;
using ImageNexus.BenScharbach.TWEngine.GameScreens.Generic;
using ImageNexus.BenScharbach.TWEngine.HandleGameInput;
using ImageNexus.BenScharbach.TWEngine.MemoryPool;
using ImageNexus.BenScharbach.TWEngine.Networking.Structs;
using ImageNexus.BenScharbach.TWEngine.Players;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWEngine.ScreenManagerC;
using ImageNexus.BenScharbach.TWEngine.rtsCommands;
using ImageNexus.BenScharbach.TWEngine.rtsCommands.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Net;

namespace ImageNexus.BenScharbach.TWEngine.Networking
{
    /// <summary>
    /// The <see cref="Lobby2Screen"/> provides a place for gamers to congregate before starting
    /// the actual gameplay. It displays a list of all the gamers in the session,
    /// and indicates which ones are currently talking. Each gamer can press a button
    /// to mark themselves as ready: gameplay will begin after everyone has done this.
    /// </summary>
    internal class Lobby2Screen : LobbyScreen
    {
        #region Fields

        // 9/1/2009
        private static Game _gameInstance;

        private static string _oldMapName = string.Empty;
       
        // 2/20/2009 - MenuPanes class
        private static MenuEntry _mapPreviewEntry;
        private static MenuPanes _menuPanes;
        // 2/21/2009 - MapEntry for just client
        internal static MenuEntry MenuEntryMapName;

        // 4/8/2009 - Save Ref to MenuEnty for MapPreviews
        // 4/8/2009 - Save Ref to MenuEntries for Location-1/2.
        private readonly MenuEntry _mapLocation1Entry;
        private readonly MenuEntry _mapLocation2Entry;
        private readonly MenuEntry _mapLocation3Entry;

        // 4/6/2010 - Updated to reference new ContentMaps location.
        // 11/17/2009 - The path for the MapPreview folder for PC.
        private static readonly string MapsPreviewLoadPath = TemporalWars3DEngine.ContentMapsLoc + @"\MP\_MapPreviews\{0}_MMP";

        #endregion

        #region Initialization
        
        /// <summary>
        /// Constructs a new <see cref="Lobby2Screen"/>.
        /// </summary>
        /// <param name="networkSession"><see cref="NetworkSession"/> instance</param>
        public Lobby2Screen(NetworkSession networkSession)
            : base(networkSession)
        {
            // 9/1/2009
            _gameInstance = TemporalWars3DEngine.GameInstance;
            

#if XBOX360
            // 7/18/2009 - Create instance of ResourceContentManager, used to get the MapPreviews.
            ContentMapPreviews = new ResourceContentManager(_gameInstance.Services, Resources.ResourceManager);
#endif

            // Instantiate MenuPanes
            _menuPanes = new MenuPanes();

            // 4/8/2009 - Set AspectRatioAdj, depending on Screen Resolution
            if (TemporalWars3DEngine.ScreenResolution == ScreenResolution.Type1024X768 ||
                TemporalWars3DEngine.ScreenResolution == ScreenResolution.Type1280X1024)
                AspectRatioAdj = 50;

            // 4/8/2009 - Captures the GameJoined event, which then send the Host's choices, like 'Color', 'Side', & map choices.
            networkSession.GamerJoined += NetworkSessionGamerJoined;

            var menuPaneItem2 = new MenuPaneItem("Color", new Vector2(850, 150 + AspectRatioAdj), new Vector2(300, 325));
            var menuPaneItem3 = new MenuPaneItem("Side", new Vector2(850, 500 + AspectRatioAdj), new Vector2(300, 150));
            var menuPaneItem4 = new MenuPaneItem("Players", new Vector2(100, 500 + AspectRatioAdj),
                                                 new Vector2(700, 150));
            var menuPaneItem5 = new MenuPaneItem("Map Preview", new Vector2(500, 150 + AspectRatioAdj),
                                                 new Vector2(300, 300));
            MenuPaneItem menuPaneItem1;

            // Add 5 Colors
            var menuEntry0 = new MenuEntry("-------------", new Rectangle(0, 60, 300, 40), new Vector2(15, 0));
            var menuEntry1 = new MenuEntry("FireBrick Red", new Rectangle(0, 60, 300, 40), new Vector2(15, 0));
            var menuEntry2 = new MenuEntry("Forest Green", new Rectangle(0, 60, 300, 40), new Vector2(15, 0));
            var menuEntry3 = new MenuEntry("Midnight Blue", new Rectangle(0, 60, 300, 40), new Vector2(15, 0));
            var menuEntry4 = new MenuEntry("Gold", new Rectangle(0, 60, 300, 40), new Vector2(15, 0));
            var menuEntry5 = new MenuEntry("OrangeRed", new Rectangle(0, 60, 300, 40), new Vector2(15, 0));
            var menuEntry6 = new MenuEntry("DarkOrchid", new Rectangle(0, 60, 300, 40), new Vector2(15, 0));

            // 2/21/2009 - If Host, then create Maps list; otherwise, only show MapName choosen by host.
            if (networkSession.IsHost)
            {
                //
                // Add a couple of menuPaneItems.
                //
                // MenuPane-1
                menuPaneItem1 = new MenuPaneItem("Maps", new Vector2(100, 150 + AspectRatioAdj), new Vector2(350, 300))
                                    {
                                        ScreenManager =(ScreenManager)_gameInstance.Services.GetService(typeof (ScreenManager))
                                    };
                menuPaneItem1.SelectIndexChanged += MenuPaneItem1SelectIndexChanged;
                menuPaneItem1.IsSelected = true; // default to 1st menu.

                // Populate the MenuPaneItem-1 menu with MenuEntries for all MapNames.
                PopulateMapListView(ref menuPaneItem1);
            }
            else
            {
                //
                // Add a couple of menuPaneItems.
                //
                // MenuPane-1
                menuPaneItem1 = new MenuPaneItem("Map name", new Vector2(100, 150 + AspectRatioAdj),
                                                 new Vector2(350, 300))
                                    {
                                        ScreenManager =(ScreenManager)_gameInstance.Services.GetService(typeof (ScreenManager)),
                                        IsSelected = true
                                    };

                // Add 1 menu entry to show MapName choose by Host
                MenuEntryMapName = new MenuEntry("Waiting Host Choice...", new Rectangle(0, 60, 350, 40),
                                                 new Vector2(15, 0));

                // Add new MenuEntries to menuPane.
                menuPaneItem1.AddMenuEntry(MenuEntryMapName);
            }

            // MenuPane-2 (colors)
            menuPaneItem2.ScreenManager =
                (ScreenManager) _gameInstance.Services.GetService(typeof (ScreenManager));
            menuPaneItem2.SelectIndexChanged += MenuPaneItem3SelectIndexChanged;

            // Add new MenuEntries to menuPane.
            menuPaneItem2.AddMenuEntry(menuEntry0);
            menuPaneItem2.AddMenuEntry(menuEntry1);
            menuPaneItem2.AddMenuEntry(menuEntry2);
            menuPaneItem2.AddMenuEntry(menuEntry3);
            menuPaneItem2.AddMenuEntry(menuEntry4);
            menuPaneItem2.AddMenuEntry(menuEntry5);
            menuPaneItem2.AddMenuEntry(menuEntry6);

            // MenuPane-3 (sides)
            menuPaneItem3.ScreenManager =
                (ScreenManager) _gameInstance.Services.GetService(typeof (ScreenManager));
            menuPaneItem3.SelectIndexChanged += MenuPaneItem2SelectIndexChanged;

            // Add 2 menu entries
            menuEntry1 = new MenuEntry("Side 1", new Rectangle(0, 60, 300, 40), new Vector2(15, 0));
            menuEntry2 = new MenuEntry("Side 2", new Rectangle(0, 60, 300, 40), new Vector2(15, 0));

            // Add new MenuEntries to menuPane.
            menuPaneItem3.AddMenuEntry(menuEntry1);
            menuPaneItem3.AddMenuEntry(menuEntry2);

            // MenuPane-4 (players)
            menuPaneItem4.ScreenManager =
                (ScreenManager) _gameInstance.Services.GetService(typeof (ScreenManager));

            // MenuPane-5 (Map Preview)
            menuPaneItem5.ScreenManager =
                (ScreenManager) _gameInstance.Services.GetService(typeof (ScreenManager));
            menuPaneItem5.SelectIndexChanged += MenuPaneItem5SelectIndexChanged;

            // Add Background menu entry
            _mapPreviewEntry = new MenuEntry("", new Rectangle(10, 60, 175, 175), new Vector2(15, 0));
            if (networkSession.IsHost)
            {
#if XBOX360
                _mapPreviewEntry.MenuEntryBackgroundCustom = ContentMapPreviews.Load<Texture2D>(menuPaneItem1.MenuEntries[0].Text + "_MMP");
                // 7/18/2009
#else

                _mapPreviewEntry.MenuEntryBackgroundCustom =
                    _gameInstance.Content.Load<Texture2D>(String.Format(MapsPreviewLoadPath, menuPaneItem1.MenuEntries[0].Text));
#endif
            }
            else
            {
#if XBOX360
                _mapPreviewEntry.MenuEntryBackgroundCustom = ContentMapPreviews.Load<Texture2D>(LobbyScreenData.mapName + "_MMP");
                // 7/18/2009
#else

                _mapPreviewEntry.MenuEntryBackgroundCustom =
                    _gameInstance.Content.Load<Texture2D>(String.Format(MapsPreviewLoadPath, LobbyScreenData.mapName));
#endif
            }
            _mapPreviewEntry.IsSelectable = false; // shows custom background instead, when set to False.
            _mapPreviewEntry.IsNeverSelectable = true; // 9/1/2009 - Keeps this entry from every being set to TRUE!
            _mapLocation1Entry = new MenuEntry("-----", new Rectangle(200, 60, 100, 40), new Vector2(15, 0));
            _mapLocation2Entry = new MenuEntry("Loc 1", new Rectangle(200, 60, 100, 40), new Vector2(15, 0));
            _mapLocation3Entry = new MenuEntry("Loc 2", new Rectangle(200, 60, 100, 40), new Vector2(15, 0));


            // Add new MenuEntries to menuPane.
            menuPaneItem5.AddMenuEntry(_mapPreviewEntry);
            menuPaneItem5.AddMenuEntry(_mapLocation1Entry);
            menuPaneItem5.AddMenuEntry(_mapLocation2Entry);
            menuPaneItem5.AddMenuEntry(_mapLocation3Entry);
            // 9/1/2009 - Skip '_MapPreview' MenuEntry.
            menuPaneItem5.SetStartingMenuEntryPosition(1); 

            // Add MenuPaneItems to MenuPane, in order desired.
            _menuPanes.MenuPaneItems.Add(menuPaneItem1);
            _menuPanes.MenuPaneItems.Add(menuPaneItem5);
            _menuPanes.MenuPaneItems.Add(menuPaneItem2);
            _menuPanes.MenuPaneItems.Add(menuPaneItem3);
            _menuPanes.MenuPaneItems.Add(menuPaneItem4);


            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        #region EventHandlers

        // 4/8/2009
        /// <summary>
        /// <see cref="EventHandler"/> for when a gamer just joined, which will send out the current
        /// host selections, like 'Side', 'Color', & Mapname.
        /// </summary>
        private void NetworkSessionGamerJoined(object sender, GamerJoinedEventArgs e)
        {
            // if host, and gamer joined is not host, then send info.
            if (!NetworkSession.IsHost || e.Gamer.IsHost) return;


            var gamerInfo = (GamerInfo) NetworkSession.LocalGamers[0].Tag;

            SendRTSCommLobbyData(this, ref gamerInfo);
        }

        /// <summary>
        /// <see cref="EventHandler"/> for <see cref="MenuPaneItem"/> 'selectIndexChanged', which sets the map to play with.
        /// </summary>
        private void MenuPaneItem1SelectIndexChanged(object sender, EventArgs e)
        {
            var menuPaneItem = (MenuPaneItem) sender;

            // Set MapName to selected SceneItemOwner
            LobbyScreenData.mapName = menuPaneItem.MenuEntries[menuPaneItem.SelectedEntry].Text;

            // 4/8/2009 - Set the Map Preview Image
            try
            {
#if XBOX360
                _mapPreviewEntry.MenuEntryBackgroundCustom = ContentMapPreviews.Load<Texture2D>(LobbyScreenData.mapName + "_MMP");
                // 7/18/2009
#else
                _mapPreviewEntry.MenuEntryBackgroundCustom =
                    _gameInstance.Content.Load<Texture2D>(String.Format(MapsPreviewLoadPath, LobbyScreenData.mapName));
#endif
            }
            catch (ContentLoadException)
            {
                _mapPreviewEntry.MenuEntryBackgroundCustom = null;
            }

            // 4/8/2009 - Sets the MapMarkerPositions for current map
            SetMapMarkerPositions(LobbyScreenData.mapName, "MP", _mapPreviewEntry);

            // 9/1/2009  - set 'IsReady' to False for all GAMERS, since change occured.
            SetLocalGamersToNotReady();
            
           
            var gamerInfo = (GamerInfo) NetworkSession.LocalGamers[0].Tag;
            SendRTSCommLobbyData(this, ref gamerInfo);
            
        }

        // 4/7/2009
        /// <summary>
        /// <see cref="EventHandler"/> for <see cref="MenuPaneItem"/> 'selectIndexChanged', which will set
        /// the side the <see cref="Player"/> choose into their gamer 'Tag'.
        /// </summary>
        private void MenuPaneItem2SelectIndexChanged(object sender, EventArgs e)
        {
            var menuPaneItem = (MenuPaneItem) sender;
           
            if (NetworkSession.LocalGamers[0].Tag == null) return;

            // set side into gamer's tag
            var gamerInfo = (GamerInfo) NetworkSession.LocalGamers[0].Tag;

            // 9/1/2009  - set 'IsReady' to False for all GAMERS, since change occured.
            SetLocalGamersToNotReady();

            switch (menuPaneItem.MenuEntries[menuPaneItem.SelectedEntry].Text)
            {
                case "Side 1":
                    gamerInfo.PlayerSide = 1;
                    break;
                case "Side 2":
                    gamerInfo.PlayerSide = 2;
                    break;
            }

            NetworkSession.LocalGamers[0].Tag = gamerInfo;
            
            SendRTSCommLobbyData(this, ref gamerInfo);
            
        }

        // 4/7/2009
        /// <summary>
        /// <see cref="EventHandler"/> for <see cref="MenuPaneItem"/> 'selectIndexChanged', which will set
        /// the color the <see cref="Player"/> choose into their gamer 'Tag'.
        /// </summary>
        private void MenuPaneItem3SelectIndexChanged(object sender, EventArgs e)
        {
            var menuPaneItem = (MenuPaneItem) sender;

            // Update old selected SceneItemOwner to 'White' color override.
            //menuPaneItem.MenuEntries[menuPaneItem.OldSelectedEntry].MenuEntryBackgroundColorOverride = Color.White;

            // Set new selected SceneItemOwner to 'Yellow' color override.
            //menuPaneItem.MenuEntries[menuPaneItem.SelectedEntry].MenuEntryBackgroundColorOverride = Color.Yellow;

            // set side into gamer's tag
            if (NetworkSession.LocalGamers[0].Tag == null) return;

            var gamerInfo = (GamerInfo) NetworkSession.LocalGamers[0].Tag;

            // 9/1/2009  - set 'IsReady' to False for all GAMERS, since change occured.
            SetLocalGamersToNotReady();

            switch (menuPaneItem.MenuEntries[menuPaneItem.SelectedEntry].Text)
            {
                case "-------------":
                    gamerInfo.PlayerColor = Color.Black; // 9/1/2009 - Not usable color.
                    break;
                case "FireBrick Red":
                    gamerInfo.PlayerColor = Color.Firebrick;
                    break;
                case "Forest Green":
                    gamerInfo.PlayerColor = Color.ForestGreen;
                    break;
                case "Midnight Blue":
                    gamerInfo.PlayerColor = Color.MidnightBlue;
                    break;
                case "Gold":
                    gamerInfo.PlayerColor = Color.Gold;
                    break;
                case "OrangeRed":
                    gamerInfo.PlayerColor = Color.OrangeRed;
                    break;
                case "DarkOrchid":
                    gamerInfo.PlayerColor = Color.DarkOrchid;
                    break;
                default:
                    break;
            }

            // set color name
            GetColorName(ref gamerInfo.PlayerColor, out gamerInfo.ColorName);

            NetworkSession.LocalGamers[0].Tag = gamerInfo;
           
            SendRTSCommLobbyData(this, ref gamerInfo);
            
        }

        // 4/8/2009
        /// <summary>
        /// <see cref="EventHandler"/> for <see cref="MenuPaneItem"/> 'selectIndexChanged', which will set
        /// the location the <see cref="Player"/> choose into their gamer 'Tag'.
        /// </summary>
        private void MenuPaneItem5SelectIndexChanged(object sender, EventArgs e)
        {
            var menuPaneItem = (MenuPaneItem) sender;

            // 11/19/2009 - cache
            var localGamer = NetworkSession.LocalGamers[0];
            if (localGamer == null) return;

            // set side into gamer's tag
            if (localGamer.Tag == null) return;

            var gamerInfo = (GamerInfo) localGamer.Tag;

            // 9/1/2009  - set 'IsReady' to False for all GAMERS, since change occured.
            SetLocalGamersToNotReady();

            switch (menuPaneItem.MenuEntries[menuPaneItem.SelectedEntry].Text)
            {
                case "-----":
                    gamerInfo.PlayerLocation = 0; // 9/1/2009 - Not usable location.
                    break;
                case "Loc 1":
                    gamerInfo.PlayerLocation = 1;
                    break;
                case "Loc 2":
                    gamerInfo.PlayerLocation = 2;
                    break;
            }

            localGamer.Tag = gamerInfo;
            
            SendRTSCommLobbyData(this, ref gamerInfo);
            
        }

        #endregion

        #endregion

        #region Update

        private int _sendChangesCounter;

        /// <summary>
        /// Updates the <see cref="Lobby2Screen"/>.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="otherScreenHasFocus">When more than one screen is shown, does other screen currently have focus?</param>
        /// <param name="coveredByOtherScreen">Is this screen covered by another screen?</param>
        public override sealed void Update(GameTime gameTime, bool otherScreenHasFocus,
                                           bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // Call MenuPanes
            _menuPanes.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // 5/4/2009 - Send changes every 60 ticks
            _sendChangesCounter++;
            if (_sendChangesCounter < 60) return;

            _sendChangesCounter = 0;

            // 3/1/2010 - Catch 'ArgOutOfRange'.
            try
            {
                // 3/1/2010 - Lock
                GamerInfo gamerInfo;
                lock (NetworkSession.LocalGamers)
                {
                    gamerInfo = (GamerInfo) NetworkSession.LocalGamers[0].Tag;
                }

                SendRTSCommLobbyData(this, ref gamerInfo);
            }
            catch (ArgumentOutOfRangeException)
            {
                Debug.WriteLine("Update method, for 'Lobby2Screen', threw ArgumentOutOfRangeException.");
            }
        }


        /// <summary>
        /// Handles user input for all the local gamers in the session. Unlike most
        /// screens, which use the <see cref="InputState"/> class to combine input data from all
        /// gamepads, the lobby needs to individually mark specific players as ready,
        /// so it loops over all the local gamers and reads their inputs individually.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="input"><see cref="InputState"/> instance</param>
        public override sealed void DoHandleInput(GameTime gameTime, InputState input)
        {
            base.DoHandleInput(gameTime, input);

            // Call MenuPanes
            _menuPanes.DoHandleInput(gameTime, input);
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draws the <see cref="MenuPanes"/> for this <see cref="Lobby2Screen"/> instance.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public override sealed void Draw2D(GameTime gameTime)
        {
            base.Draw2D(gameTime);

            // Call MenuPanes
            _menuPanes.Draw2D(gameTime);
        }

        #endregion

        // 4/9/2009
        /// <summary>
        /// Sets the <see cref="GamerInfo"/> values for the current <see cref="NetworkGamer"/>, which is 
        /// determine by the NetworkID.  The data is received from the <see cref="NetworkGameComponent"/> class.
        /// </summary>
        /// <param name="lobbyData"><see cref="RTSCommLobbyData"/> instance</param>
        /// <param name="networkSession"><see cref="NetworkSession"/> instance</param>
        public static void Network_SetGamerInfo(RTSCommLobbyData lobbyData, NetworkSession networkSession)
        {
            // Set MapName, if client.
            if (lobbyData.MapName != "Empty" && !networkSession.IsHost)
            {
                // Set directly into LobbyScreen
                LobbyScreenData.mapName = lobbyData.MapName;
                MenuEntryMapName.Text = lobbyData.MapName;

                // 4/8/2009 - Set the Map Preview Image
                try
                {
#if XBOX360
                    _mapPreviewEntry.MenuEntryBackgroundCustom = ContentMapPreviews.Load<Texture2D>(lobbyData.MapName + "_MMP");
               
#else

                    _mapPreviewEntry.MenuEntryBackgroundCustom =
                        _gameInstance.Content.Load<Texture2D>(String.Format(MapsPreviewLoadPath, lobbyData.MapName));
#endif
                }
                catch (ContentLoadException)
                {
                    _mapPreviewEntry.MenuEntryBackgroundCustom = null;
                }

                // Set current MapMarkerPositions                
                if (_oldMapName != lobbyData.MapName)
                {
                    SetMapMarkerPositions(lobbyData.MapName, "MP", _mapPreviewEntry);
                    _oldMapName = lobbyData.MapName;
                }
            }

            // Get Gamer using the Gamer-ID.
            var networkGamer = networkSession.FindGamerById(lobbyData.GamerID);

            // Set 'PlayerSide' & 'PlayerColor' directly into gamer's tag.
            if (networkGamer != null)
                if (networkGamer.Tag != null)
                {
                    // 5/31/2009
                    GamerInfo gamerInfo;
                    if (networkGamer.Tag is GamerInfo)
                    {
                        gamerInfo = (GamerInfo)networkGamer.Tag;

                        gamerInfo.PlayerSide = lobbyData.PlayerSide;
                        gamerInfo.PlayerLocation = lobbyData.PlayerLocation;
                        gamerInfo.PlayerColor = lobbyData.PlayerColor;
                        GetColorName(ref lobbyData.PlayerColor, out gamerInfo.ColorName);

                        networkGamer.Tag = gamerInfo;

                        // 6/15/2009 - Deselect other players color from available menu entries.
                        {
                            const string title = "Color";
                            var colorName = gamerInfo.ColorName;

                            // 9/1/2009 - Updates the Selectable flag of the given MenuEntry.
                            UpdateMenuEntrySelectableFlag(title, colorName);
                        }

                        // 9/1/2009 - Deselect other players location from availble menu entries.
                        {
                            const string title = "Map Preview";
                            var locationName = GetLocationNameForGivenIndex(gamerInfo.PlayerLocation);
                           
                            // 9/1/2009 - Updates the Selectable flag of the given MenuEntry.
                            UpdateMenuEntrySelectableFlag(title, locationName);
                        }
                    }
                }
        }

        // 9/1/2009
        /// <summary>
        /// Updates the <see cref="MenuEntry"/> 'IsSelectable' flag, to True or False.  This is used
        /// to lock out a choice in MP games; for example, the color 'Red' would be de-activated
        /// when the other player has choosen this color!
        /// </summary>
        /// <param name="menuTitle">Menu's Title, which the <see cref="MenuEntry"/> belongs to.</param>
        /// <param name="menuEntryName"><see cref="MenuEntry"/> title</param>
        private static void UpdateMenuEntrySelectableFlag(string menuTitle, string menuEntryName)
        {
            var menuPanesIndex = _menuPanes.FindMenuPaneIndexByTitle(menuTitle);
            var menuEntryIndex = _menuPanes.MenuPaneItems[menuPanesIndex].FindMenuEntryIndexByName(menuEntryName);
                        
            // Get MenuEntryIndex
            SetAllMenuEntriesToTrue(menuPanesIndex); // Set All MenuEntries Back to True
            SetMenuEntrySelectableFlag(menuPanesIndex, menuEntryIndex, false);
        }

        // 6/15/2009
        /// <summary>
        /// Sets all <see cref="MenuEntry"/> items, for a given <see cref="MenuPaneItem"/> to TRUE.
        /// </summary>
        /// <param name="menuPaneIndex">Index of <see cref="MenuPaneItem"/> in collection</param>
        private static void SetAllMenuEntriesToTrue(int menuPaneIndex)
        {
            if (menuPaneIndex == -1)
                return;

            // 9/1/2009 - Cache
            var menuPaneItem = _menuPanes.MenuPaneItems[menuPaneIndex];

            if (menuPaneItem == null) return;

            // iterate through all internal menuEntries, and set 'IsSelectable' to TRUE.
            var count = menuPaneItem.MenuEntries.Count; // 9/1/2009
            for (var i = 0; i < count; i++)
            {
                // 4/20/2010 - Cache
                var menuEntry = menuPaneItem.MenuEntries[i];
                if (menuEntry == null) continue;

                // 9/1/2009 - Skip any menuEntries which have the flag 'IsNeverSelectable' set.
                if (menuEntry.IsNeverSelectable)
                    continue;

                menuEntry.IsSelectable = true;
            }
        }

        // 6/15/2009
        /// <summary>
        /// Sets some <see cref="MenuPaneItem"/> <see cref="MenuEntry"/> <see cref="SceneItem"/> owner's 'IsSelectable' flag to the given value.
        /// </summary>
        /// <param name="menuPaneIndex">Index to <see cref="MenuPaneItem"/></param>
        /// <param name="menuEntryIndex">Index to <see cref="MenuEntry"/></param>
        /// <param name="isSelectable">Value to set</param>
        private static void SetMenuEntrySelectableFlag(int menuPaneIndex, int menuEntryIndex, bool isSelectable)
        {
            if (menuPaneIndex == -1 || menuEntryIndex == -1)
                return;

            // access the MenuEntry from the MenuPanes and MenuEntries arrays, to update the given 'IsSelectable' flag.
            if (_menuPanes.MenuPaneItems[menuPaneIndex].MenuEntries[menuEntryIndex] != null)
            {
                _menuPanes.MenuPaneItems[menuPaneIndex].MenuEntries[menuEntryIndex].IsSelectable = isSelectable;
            }
        }

        // 6/15/2009 - 
// ReSharper disable UnusedMember.Local
        /// <summary>
        /// Get Location name for given location index.
        /// </summary>
        /// <param name="locationIndex">location index</param>
        /// <returns>location name</returns>
        private static string GetLocationNameForGivenIndex(int locationIndex)
// ReSharper restore UnusedMember.Local
        {
            switch (locationIndex)
            {
                case 1:
                    return "Loc 1";

                case 2:
                    return "Loc 2";
            }

            return null;
        }


        // 4/8/2009
        /// <summary>
        /// Helper function, which simply creates a new <see cref="RTSCommLobbyData"/> command, and populates
        /// with the data from the <see cref="GamerInfo"/>.  This is then added to the proper network Queue,
        /// depending if host or client.
        /// </summary>
        /// <param name="screen"><see cref="LobbyScreen"/> instance</param>
        /// <param name="gamerInfo"><see cref="GamerInfo"/> Struct to send.</param>
        private static void SendRTSCommLobbyData(LobbyScreen screen, ref GamerInfo gamerInfo)
        {
            // 9/1/2009 - Only send when 2nd player is available.
            if (NetworkSession.RemoteGamers.Count <= 0) return;

            // 6/29/2009 - Create LobbyData command.
            RTSCommLobbyData lobbyData;
            PoolManager.GetNode(out lobbyData);

            lobbyData.Clear();
            lobbyData.NetworkCommand = NetworkCommands.LobbyData;
            lobbyData.GamerID = NetworkSession.LocalGamers[0].Id;
            lobbyData.MapName = LobbyScreenData.mapName;
            lobbyData.PlayerSide = gamerInfo.PlayerSide;
            lobbyData.PlayerColor = gamerInfo.PlayerColor;
            lobbyData.PlayerLocation = gamerInfo.PlayerLocation; // 4/8/2009

            // 9/1/2009 - Send data.
            SendLobbyData(screen, lobbyData);
        }

        // 9/1/2009
        /// <summary>
        /// Populates a <see cref="MenuPaneItem"/> with a <see cref="MenuEntry"/> for each
        /// MapName found within the 'ContentMaps' folder.
        /// </summary>
        /// <param name="menuPaneItem"><see cref="MenuPaneItem"/> instance</param>
        private void PopulateMapListView(ref MenuPaneItem menuPaneItem)
        {
            string[] mapNames;
            PopulateMapListView(ref menuPaneItem, "MP", out mapNames);
        }

        /// <summary>
        /// Populates a <see cref="MenuPaneItem"/> with a <see cref="MenuEntry"/> for each
        /// MapName found within the 'ContentMaps' folder.
        /// </summary>
        /// <param name="menuPaneItem"><see cref="MenuPaneItem"/> instance</param>
        /// <param name="mapType">Is SP or MP type map.</param>
        /// <param name="mapNames">(OUT) Collection of map names</param>
        protected override void PopulateMapListView(ref MenuPaneItem menuPaneItem, string mapType, out string[] mapNames)
        {
            base.PopulateMapListView(ref menuPaneItem, mapType, out mapNames);
            
            // 6/15/2009 - Set 1st entry of list as default entry.
            LobbyScreenData.mapName = mapNames[0];
        }

      
    }
}