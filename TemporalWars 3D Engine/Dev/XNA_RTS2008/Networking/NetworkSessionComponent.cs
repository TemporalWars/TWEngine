#region File Description
//-----------------------------------------------------------------------------
// NetworkSessionComponent.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net;
using TWEngine.GameScreens.Generic;
using TWEngine.Interfaces;
using TWEngine.ScreenManagerC;


namespace TWEngine.Networking
{
    /// <summary>
    /// Component in charge of owning and updating the current <see cref="NetworkSession"/> object.
    /// This is responsible for calling NetworkSession.Update at regular intervals,
    /// and also exposes the <see cref="NetworkSession"/> as a game service which can easily be
    /// looked up by any other code that needs to access it.
    /// </summary>
    class NetworkSessionComponent : GameComponent
    {
        #region Fields

        // 9/8/2008: Updated to 2 MaxGamers, and 1 Max LocalPlayer.
        /// <summary>
        /// Max gamers allowed.
        /// </summary>
        public const int MaxGamers = 2;
        /// <summary>
        /// Max local gamers allowed.
        /// </summary>
        public const int MaxLocalGamers = 1;

        readonly ScreenManager _screenManager;
        NetworkSession _networkSession;
        IMessageDisplay _messageDisplay;

        bool _notifyWhenPlayersJoinOrLeave;

        string _sessionEndMessage;

        #endregion

        #region Initialization


        /// <summary>
        /// The constructor is private: external callers should use the Create method.
        /// </summary>
        NetworkSessionComponent(ScreenManager screenManager,
                                NetworkSession networkSession)
            : base(screenManager.Game)
        {
            _screenManager = screenManager;
            _networkSession = networkSession;

            // Hook up our session event handlers.
            networkSession.GamerJoined += GamerJoined;
            networkSession.GamerLeft += GamerLeft;
            networkSession.SessionEnded += NetworkSessionEnded;
        }


        /// <summary>
        /// Creates a new <see cref="NetworkSessionComponent"/> instance.
        /// </summary>
        /// <param name="screenManager"><see cref="ScreenManager"/> instance</param>
        /// <param name="networkSession"><see cref="NetworkSession"/> instance</param>
        public static void Create(ScreenManager screenManager,
                                  NetworkSession networkSession)
        {
            var game = screenManager.Game;

            // Register this network session as a service.
            game.Services.AddService(typeof(NetworkSession), networkSession);

            // Create a NetworkSessionComponent, and add it to the Game.
            game.Components.Add(new NetworkSessionComponent(screenManager, networkSession));
        }


        /// <summary>
        /// Initializes the component, by retrieving the <see cref="IMessageDisplay"/> service.
        /// </summary>
        public sealed override void Initialize()
        {
            base.Initialize();

            // Look up the IMessageDisplay service, which will
            // be used to report gamer join/leave notifications.
            _messageDisplay = (IMessageDisplay)Game.Services.GetService(typeof(IMessageDisplay));

            if (_messageDisplay != null)
                _notifyWhenPlayersJoinOrLeave = true;
        }


        #endregion

        #region Update


        /// <summary>
        /// Updates the <see cref="NetworkSession"/>, by directly calling
        /// the <see cref="NetworkSession.Update"/> method.
        /// </summary>
        public sealed override void Update(GameTime gameTime)
        {
            // 4/20/2010 - Cache
            var networkSession = _networkSession;
            if (networkSession == null) return;

            try
            {
                networkSession.Update();

                // Has the session ended?
                if (networkSession.SessionState == NetworkSessionState.Ended)
                {
                    LeaveSession(this);
                }
            }
            catch (NetworkException exception)
            {
                // Handle any errors from the network session update.
                /*Console.WriteLine(string.Format("NetworkSession.Update threw {0}: {1}",
                                              exception, exception.Message));*/

                _sessionEndMessage = Resources.ErrorNetwork;

                LeaveSession(this);
            }
        }


        #endregion

        #region Event Handlers


        /// <summary>
        /// <see cref="EventHandler"/> called when a gamer joins the session;
        /// displays a notification message.
        /// </summary>
        void GamerJoined(object sender, GamerJoinedEventArgs e)
        {
            if (_notifyWhenPlayersJoinOrLeave)
            {
                _messageDisplay.ShowMessage(Resources.MessageGamerJoined,
                                           e.Gamer.Gamertag);
            }
        }


        /// <summary>
        /// <see cref="EventHandler"/> called when a gamer leaves the session;
        /// displays a notification message.
        /// </summary>
        void GamerLeft(object sender, GamerLeftEventArgs e)
        {
            if (_notifyWhenPlayersJoinOrLeave)
            {
                _messageDisplay.ShowMessage(Resources.MessageGamerLeft,
                                           e.Gamer.Gamertag);
            }
        }


        /// <summary>
        /// <see cref="EventHandler"/> called when the <see cref="NetworkSession"/> ends.
        /// Stores the end reason, so this can later be displayed to the user.
        /// </summary>
        void NetworkSessionEnded(object sender, NetworkSessionEndedEventArgs e)
        {
            switch (e.EndReason)
            {
                case NetworkSessionEndReason.ClientSignedOut:
                    _sessionEndMessage = null;
                    break;

                case NetworkSessionEndReason.HostEndedSession:
                    _sessionEndMessage = Resources.ErrorHostEndedSession;
                    break;

                case NetworkSessionEndReason.RemovedByHost:
                    _sessionEndMessage = Resources.ErrorRemovedByHost;
                    break;

                default:
                    _sessionEndMessage = Resources.ErrorDisconnected;
                    break;
            }

            _notifyWhenPlayersJoinOrLeave = false;
        }


        #endregion

        #region Methods

        // 10/2/2008: Updated to use ForLoop, rather than ForEach.
        /// <summary>
        /// Public method called when the user wants to leave the <see cref="NetworkSession"/>.
        /// Displays a confirmation <see cref="MessageBoxScreen"/>, then disposes the session, removes
        /// the <see cref="NetworkSessionComponent"/>, and returns them to the main menu screen.
        /// </summary>
        /// <param name="screenManager"><see cref="ScreenManager"/> instance</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="screenManager"/> is null.</exception>
        public static void LeaveSession(ScreenManager screenManager)
        {
            // 4/20/2010 - Check if null
            if (screenManager == null)
                throw new ArgumentNullException("screenManager", @"Given 'ScreenManager' parameter cannot be null!");

            // 4/20/2010 - Cache
            var gameComponentCollection = screenManager.Game.Components;
            if (gameComponentCollection == null) return;

            // Search through Game.Components to find the NetworkSessionComponent.
            var count = gameComponentCollection.Count; // 11/19/2009
            for (var i = 0; i < count; i++)
            {
                var component = gameComponentCollection[i];
                if (component == null) continue; // 4/20/2010

                var self = component as NetworkSessionComponent;
                if (self == null) continue;

                // Display a message box to confirm the user really wants to leave.
                var message = self._networkSession.IsHost ? Resources.ConfirmEndSession : Resources.ConfirmLeaveSession;

                var confirmMessageBox = new MessageBoxScreen(message);

                // Hook the messge box ok event to actually leave the session.
                confirmMessageBox.Accepted += delegate
                                                  {
                                                      LeaveSession(self);
                                                  };

                screenManager.AddScreen(confirmMessageBox, false);

                break;
            }
        }


        /// <summary>
        /// Internal method for leaving the <see cref="NetworkSession"/>. This disposes the 
        /// session, removes the <see cref="NetworkSessionComponent"/>, and returns the user
        /// to the main menu screen.
        /// </summary>
        /// <param name="networkSessionComponent"><see cref="NetworkSessionComponent"/> instance</param>
        static void LeaveSession(NetworkSessionComponent networkSessionComponent)
        {
            // Remove the NetworkSessionComponent.
            networkSessionComponent.Game.Components.Remove(networkSessionComponent);

            // Remove the NetworkSession service.
            networkSessionComponent.Game.Services.RemoveService(typeof(NetworkSession));

            // Dispose the NetworkSession.
            networkSessionComponent._networkSession.Dispose();
            networkSessionComponent._networkSession = null;

            // If we have a _sessionEndMessage string explaining why the session has
            // ended (maybe this was a network disconnect, or perhaps the host kicked
            // us out?) create a message box to display this reason to the user.
            var messageBox = !string.IsNullOrEmpty(networkSessionComponent._sessionEndMessage) ? new MessageBoxScreen(networkSessionComponent._sessionEndMessage, false) : null;

            // At this point we normally want to return the user all the way to the
            // main menu screen. But what if they just joined a session? In that case
            // they went through this flow of screens:
            //
            //  - MainMenuScreen
            //  - CreateOrFindSessionsScreen
            //  - JoinSessionScreen (if joining, skipped if creating a new session)
            //  - LobbyScreeen
            //
            // If we have these previous screens on the history stack, and the user
            // backs out of the LobbyScreen, the right thing is just to pop off the
            // LobbyScreen and JoinSessionScreen, returning them to the
            // CreateOrFindSessionsScreen (we cannot just back up to the
            // JoinSessionScreen, because it contains search results that will no
            // longer be valid). But if the user is in gameplay, or has been in
            // gameplay and then returned to the lobby, the screen stack will have
            // been emptied.
            //
            // To do the right thing in both cases, we scan through the screen history
            // stack looking for a CreateOrFindSessionScreen. If we find one, we pop
            // any subsequent screens so as to return back to it, while if we don't
            // find it, we just reset everything and go back to the main menu.

            var screens = ScreenManager.GetScreens();
            if (screens == null) return; // 4/20/2010

            // Look for the CreateOrFindSessionsScreen.
            var length = screens.Length; // 8/18/2009
            for (var i = 0; i < length; i++)
            {
                // 11/19/2009 - cache
                var createOrFindScreen = (screens[i] as CreateOrFindSessionScreen);
                if (createOrFindScreen == null) continue;

                // If we found one, pop everything since then to return back to it.
                for (var j = i + 1; j < length; j++)
                {
                    // 4/20/2010 - Cache
                    var gameScreen = screens[j];
                    if (gameScreen == null) continue;

                    gameScreen.ExitScreen();
                }

                // Display the why-did-the-session-end message box.
                if (messageBox != null)
                    networkSessionComponent._screenManager.AddScreen(messageBox, false);

                return;
            }

            // If we didn't find a CreateOrFindSessionsScreen, reset everything and
            // go back to the main menu. The why-did-the-session-end message box
            // will be displayed after the loading screen has completed.
            LoadingScreen.Load(networkSessionComponent._screenManager, false, new BackgroundScreen(),
                                                     new MainMenuScreen((string) null),
                                                     messageBox);
        }


        #endregion
    }
}
