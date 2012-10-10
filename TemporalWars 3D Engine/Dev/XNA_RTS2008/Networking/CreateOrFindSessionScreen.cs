#region File Description
//-----------------------------------------------------------------------------
// CreateOrFindSessionScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.GameScreens.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.GamerServices;

namespace ImageNexus.BenScharbach.TWEngine.Networking
{
    /// <summary>
    /// This <see cref="MenuScreen"/> lets the user choose whether to create a new
    /// network session, or search for an existing session to join.
    /// </summary>
    class CreateOrFindSessionScreen : MenuScreen
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

        readonly NetworkSessionType _sessionType;

        #endregion

        #region Initialization

        // 4/6/2010: Updated to use 'ContentTexturesLoc' global var.
        /// <summary>
        /// Constructor for <see cref="CreateOrFindSessionScreen"/>, which fills in the menu contents.
        /// </summary>
        /// <param name="sessionType"><see cref="NetworkSessionType"/> Enum</param>
        public CreateOrFindSessionScreen(NetworkSessionType sessionType)
            : base(GetMenuTitle(sessionType), null, new Vector2(600, 300), TemporalWars3DEngine.ContentTexturesLoc + @"\Textures\roundedMenu")
        {
            _sessionType = sessionType;

            // Create our menu entries.
            var createSessionMenuEntry = new MenuEntry(Resources.CreateSession, _menuEntryBackgroundRectangle, _menuEntryTextMargin);
            var findSessionsMenuEntry = new MenuEntry(Resources.FindSessions, _menuEntryBackgroundRectangle, _menuEntryTextMargin);
            var backMenuEntry = new MenuEntry(Resources.Back, _menuEntryBackgroundRectangle, _menuEntryTextMargin);

            // Hook up menu event handlers.
            createSessionMenuEntry.Selected += CreateSessionMenuEntrySelected;
            findSessionsMenuEntry.Selected += FindSessionsMenuEntrySelected;
            backMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            AddMenuEntry(createSessionMenuEntry);
            AddMenuEntry(findSessionsMenuEntry);
            AddMenuEntry(backMenuEntry);           

            // 4/8/2009 - Set IsSelected.
            IsSelected = true;
        }


        /// <summary>
        /// Helper chooses an appropriate menu title for the specified session type.
        /// </summary>
        /// <param name="sessionType"><see cref="NetworkSessionType"/> Enum</param>
        static string GetMenuTitle(NetworkSessionType sessionType)
        {
            switch (sessionType)
            {
                case NetworkSessionType.PlayerMatch:
                    return Resources.PlayerMatch;

                case NetworkSessionType.SystemLink:
                    return Resources.SystemLink;

                default:
                    throw new NotSupportedException();
            }
        }


        #endregion

        #region Event Handlers


        /// <summary>
        /// <see cref="EventHandler"/> for when the create session <see cref="MenuEntry"/> is selected.
        /// </summary>
        void CreateSessionMenuEntrySelected(object sender, EventArgs e)
        {
            try
            {
                // Begin an asynchronous create network session operation.
                var asyncResult = NetworkSession.BeginCreate(_sessionType,
                                                             NetworkSessionComponent.MaxLocalGamers,
                                                             NetworkSessionComponent.MaxGamers,
                                                             null, null);

                // Activate the network busy screen, which will display
                // an animation until this operation has completed.
                var busyScreen = new NetworkBusyScreen(asyncResult);

                busyScreen.OperationCompleted += CreateSessionOperationCompleted;

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
        /// <see cref="EventHandler"/> for when the asynchronous create network session
        /// operation has completed.
        /// </summary>
        void CreateSessionOperationCompleted(object sender,
                                             OperationCompletedEventArgs e)
        {
            try
            {
                // End the asynchronous create network session operation.
                var networkSession = NetworkSession.EndCreate(e.AsyncResult);

                // Create a component that will manage the session we just created.
                NetworkSessionComponent.Create(ScreenManager, networkSession);

                // 2/21/2009 - Store the NetworkSession into the NetworkGameComponent now!
                
                NetworkGameComponent.NetworkSession = networkSession;

                // Go to the lobby screen.
                ScreenManager.AddScreen(new Lobby2Screen(networkSession), false); // Was LobbyScreen
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
        /// <see cref="EventHandler"/> for when the find sessions <see cref="MenuEntry"/> is selected.
        /// </summary>
        void FindSessionsMenuEntrySelected(object sender, EventArgs e)
        {
            try
            {
                // Begin an asynchronous find network sessions operation.
                var asyncResult = NetworkSession.BeginFind(_sessionType,
                                                           NetworkSessionComponent.MaxLocalGamers,
                                                           null, null, null);

                // Activate the network busy screen, which will display
                // an animation until this operation has completed.
                var busyScreen = new NetworkBusyScreen(asyncResult);

                busyScreen.OperationCompleted += FindSessionsOperationCompleted;

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
        /// <see cref="EventHandler"/> for when the asynchronous find network sessions
        /// operation has completed.
        /// </summary>
        void FindSessionsOperationCompleted(object sender,
                                            OperationCompletedEventArgs e)
        {
            try
            {
                // End the asynchronous find network sessions operation.
                var availableSessions =
                    NetworkSession.EndFind(e.AsyncResult);

                if (availableSessions.Count == 0)
                {
                    // If we didn't find any sessions, display an error.
                    availableSessions.Dispose();

                    ScreenManager.AddScreen(
                            new MessageBoxScreen(Resources.NoSessionsFound, false), false);
                }
                else
                {
                    // If we did find some sessions, proceed to the JoinSessionScreen.
                    ScreenManager.AddScreen(new JoinSessionScreen(availableSessions), false);
                }
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


        #endregion
    }
}
