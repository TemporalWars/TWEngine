#region File Description
//-----------------------------------------------------------------------------
// JoinSessionScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.GamerServices;
using TWEngine.GameScreens.Generic;


namespace TWEngine.Networking
{
    /// <summary>
    /// This <see cref="MenuScreen"/> displays a list of available network sessions,
    /// and lets the user choose which one to join.
    /// </summary>
    class JoinSessionScreen : MenuScreen
    {
        #region Fields

        // 2/17/2009 - Add MenuEntry Rectangle for background.
        // 
        /// <summary>
        /// Use the X/Y Position, which will be relative to the MenuScreen Position.
        /// </summary>
        private readonly Rectangle _menuEntryBackgroundRectangle = new Rectangle(0, 60, 600, 40);
        // 2/20/2009 -
        /// <summary>
        /// Add MenuEntry TextMargin
        /// </summary>
        private readonly Vector2 _menuEntryTextMargin = new Vector2(15, 0);

        private const int MaxSearchResults = 8;

        private readonly AvailableNetworkSessionCollection _availableSessions;

        #endregion

        #region Initialization

        // 4/6/2010: Updated to use 'ContentTexturesLoc' global var.
        /// <summary>
        /// Constructs a <see cref="MenuScreen"/> listing the available network sessions.
        /// </summary>
        /// <param name="availableSessions"><see cref="AvailableNetworkSessionCollection"/> instance</param>
        public JoinSessionScreen(AvailableNetworkSessionCollection availableSessions)
            : base(Resources.JoinSession, null, new Vector2(600, 500), TemporalWars3DEngine.ContentTexturesLoc + @"\Textures\roundedMenu")
        {
            _availableSessions = availableSessions;

            // 8/31/2008: Updated to ForLoop, rather than ForEach.
            var count = availableSessions.Count;
            for (var i = 0; i < count; i++)
            {
                // Create menu entries for each available session.
                MenuEntry menuEntry = new AvailableSessionMenuEntry(availableSessions[i]);
                menuEntry.Selected += AvailableSessionMenuEntrySelected;               
                AddMenuEntry(menuEntry);

                // Matchmaking can return up to 25 available sessions at a Time, but
                // we don't have room to fit that many on the screen. In a perfect
                // World we should make the menu scroll if there are too many, but it
                // is easier to just not bother displaying more than we have room for.
                if (MenuEntries.Count >= MaxSearchResults)
                    break;   
            }
           
            // Add the Back menu entry.
            var backMenuEntry = new MenuEntry(Resources.Back, _menuEntryBackgroundRectangle, _menuEntryTextMargin);
            backMenuEntry.Selected += BackMenuEntrySelected;            
            AddMenuEntry(backMenuEntry);
        }


        #endregion
           
        #region Event Handlers


        /// <summary>
        /// <see cref="EventHandler"/> for when an available session <see cref="MenuEntry"/> is selected.
        /// </summary>
        void AvailableSessionMenuEntrySelected(object sender, EventArgs e)
        {
            // Which menu entry was selected?
            var menuEntry = (AvailableSessionMenuEntry)sender;
            var availableSession = menuEntry.AvailableSession;

            try
            {
                // Begin an asynchronous join network session operation.
                var asyncResult = NetworkSession.BeginJoin(availableSession,
                                                           null, null);

                // Activate the network busy screen, which will display
                // an animation until this operation has completed.
                var busyScreen = new NetworkBusyScreen(asyncResult);

                busyScreen.OperationCompleted += JoinSessionOperationCompleted;

                ScreenManager.AddScreen(busyScreen, false);
            }
            catch (NetworkException exception)
            {
                ScreenManager.AddScreen(new NetworkErrorScreen(exception), false);
            }
            catch (GamerPrivilegeException exception)
            {
                ScreenManager.AddScreen(new NetworkErrorScreen(exception), false);
            }
        }


        /// <summary>
        /// <see cref="EventHandler"/> for when the asynchronous join network session
        /// operation has completed.
        /// </summary>
        void JoinSessionOperationCompleted(object sender, OperationCompletedEventArgs e)
        {
            try
            {
                // End the asynchronous join network session operation.
                var networkSession = NetworkSession.EndJoin(e.AsyncResult);

                // Create a component that will manage the session we just joined.
                NetworkSessionComponent.Create(ScreenManager, networkSession);

                // 2/21/2009 - Store the NetworkSession into the NetworkGameComponent now!
                
                NetworkGameComponent.NetworkSession = networkSession;

                // Go to the lobby screen.
                ScreenManager.AddScreen(new Lobby2Screen(networkSession), false); // Was LobbyScreen

                _availableSessions.Dispose();
            }
            catch (NetworkException exception)
            {
                ScreenManager.AddScreen(new NetworkErrorScreen(exception), false);
            }
            catch (GamerPrivilegeException exception)
            {
                ScreenManager.AddScreen(new NetworkErrorScreen(exception), false);
            }
        }


        /// <summary>
        /// <see cref="EventHandler"/> for when the Back <see cref="MenuEntry"/> is selected.
        /// </summary>
        void BackMenuEntrySelected(object sender, EventArgs e)
        {
            _availableSessions.Dispose();

            ExitScreen();
        }


        #endregion
    }
}
