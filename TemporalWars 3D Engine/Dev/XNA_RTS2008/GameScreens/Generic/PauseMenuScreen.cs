#region File Description
//-----------------------------------------------------------------------------
// PauseMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.Networking;
using ImageNexus.BenScharbach.TWEngine.ScreenManagerC;
using ImageNexus.BenScharbach.TWScripting.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Net;

namespace ImageNexus.BenScharbach.TWEngine.GameScreens.Generic
{
    /// <summary>
    /// The <see cref="PauseMenuScreen"/> comes up over the top of the game,
    /// giving the player options to resume or quit.
    /// </summary>
    public class PauseMenuScreen : MenuScreen
    {
        #region Fields

        // 2/17/2009 - Add MenuEntry Rectangle for background.
        // Use the X/Y Position, which will be relative to the MenuScreen Position.
        private readonly Rectangle _menuEntryBackgroundRectangle = new Rectangle(0, 60, 600, 40);
        // 2/20/2009 - Add MenuEntry TextMargin
        private readonly Vector2 _menuEntryTextMargin = new Vector2(10, 0);

        // 1/19/2011 - Ref to 'Resume' menuEntry.
        private MenuEntry _resumeGameMenuEntry;
        private MenuEntry _quitGameMenuEntry;

        #endregion

        #region Properties

        // 6/17/2012
        /// <summary>
        /// Gets the instance of the "Resume" <see cref="MenuEntry"/>.
        /// </summary>
        public MenuEntry ResumeGameMenuEntry
        {
            get { return _resumeGameMenuEntry; }
        }

        // 6/17/2012
        /// <summary>
        /// Gets the instance of the "Quit" <see cref="MenuEntry"/>
        /// </summary>
        public MenuEntry QuitGameMenuEntry
        {
            get { return _quitGameMenuEntry; }
        }

        #endregion

        #region Initialization

        // 4/6/2010: Updated to use 'ContentTexturesLoc' global var.
        /// <summary>
        /// Constructor for the <see cref="PauseMenuScreen"/>
        /// </summary>
        /// <param name="networkSession"><see cref="NetworkSession"/> instance</param>
        public PauseMenuScreen(NetworkSession networkSession)
            : base(Resources.Paused, null, new Vector2(600, 300), TemporalWars3DEngine.ContentTexturesLoc + @"\Textures\roundedMenu")
        {
            ConstructorCommonInit(networkSession);
        }

        // 6/17/2012 - Overload with Texture2D
        /// <summary>
        /// Constructor for the <see cref="PauseMenuScreen"/>
        /// </summary>
        /// <param name="networkSession"><see cref="NetworkSession"/> instance</param>
        /// <param name="backgroundImage"><see cref="Texture2D"/> to use as background image.</param>
        public PauseMenuScreen(NetworkSession networkSession, Texture2D backgroundImage)
            : base(Resources.Paused, null, new Vector2(600, 300), backgroundImage)
        {
            ConstructorCommonInit(networkSession);
        }

        // 6/17/2012 - Overload with Color
        /// <summary>
        /// Constructor for the <see cref="PauseMenuScreen"/>
        /// </summary>
        /// <param name="networkSession"><see cref="NetworkSession"/> instance</param>
        /// <param name="backgroundColor"><see cref="Color"/> to use as background color.</param>
        public PauseMenuScreen(NetworkSession networkSession, Color backgroundColor)
            : base(Resources.Paused, null, new Vector2(600, 300), backgroundColor)
        {
            ConstructorCommonInit(networkSession);
        }

        // 6/17/2012
        /// <summary>
        /// Called by the constructors.
        /// </summary>
        private void ConstructorCommonInit(NetworkSession networkSession)
        {
            // Flag that there is no need for the game to transition
            // off when the pause menu is on top of it.
            IsPopup = true;

            // Set PauseState is started.
            TemporalWars3DEngine.GamePaused = true;

            // 4/23/2011 - Set temp drawOrder for this screen.
            UseDrawOrder = 400;

            // Add the Resume Game menu entry.
            _resumeGameMenuEntry = new MenuEntry(Resources.ResumeGame, _menuEntryBackgroundRectangle, _menuEntryTextMargin);
            ResumeGameMenuEntry.Selected += OnCancel;
            AddMenuEntry(ResumeGameMenuEntry);

            if (networkSession == null)
            {
                // If this is a single player game, add the Quit menu entry.
                _quitGameMenuEntry = new MenuEntry(Resources.QuitGame, _menuEntryBackgroundRectangle, _menuEntryTextMargin);
                QuitGameMenuEntry.Selected += QuitGameMenuEntrySelected;
                AddMenuEntry(QuitGameMenuEntry);
            }
            else
            {
                // ***
                // 8/18/2009: Removed the menuItem below, since not needed in the RTS game!
                // ***
                // If we are hosting a network game, add the Return to Lobby menu entry.
                /*if (networkSession.IsHost)
                {
                    var lobbyMenuEntry = new MenuEntry(Resources.ReturnToLobby, _menuEntryBackgroundRectangle, _menuEntryTextMargin);
                    lobbyMenuEntry.Selected += ReturnToLobbyMenuEntrySelected;                   
                    AddMenuEntry(lobbyMenuEntry);
                }*/

                // Add the End/Leave Session menu entry.
                var leaveEntryText = networkSession.IsHost
                                         ? Resources.EndSession
                                         : Resources.LeaveSession;

                var leaveSessionMenuEntry = new MenuEntry(leaveEntryText, _menuEntryBackgroundRectangle, _menuEntryTextMargin);
                leaveSessionMenuEntry.Selected += LeaveSessionMenuEntrySelected;
                AddMenuEntry(leaveSessionMenuEntry);
            }

            // 1/19/2011 - If trial mode over, then set to start on 2nd menu item.
            if (TemporalWars3DEngine.IsGameTrialOver && !TemporalWars3DEngine.IsPurchasedGame)
                SetStartingMenuEntryPosition(1);
        }

        #endregion

        #region Handle Input


        /// <summary>
        /// <see cref="EventHandler"/> for when the Quit game <see cref="MenuEntry"/> is selected.
        /// </summary>
        void QuitGameMenuEntrySelected(object sender, EventArgs e)
        {
            // Set PauseState to false
            TemporalWars3DEngine.GamePaused = false;

            var confirmQuitMessageBox =
                                    new MessageBoxScreen(Resources.ConfirmQuitGame);

            confirmQuitMessageBox.Accepted += ConfirmQuitMessageBoxAccepted;

            ScreenManager.AddScreen(confirmQuitMessageBox, false);
        }


        /// <summary>
        /// <see cref="EventHandler"/> for when the user selects ok on the "are you sure
        /// you want to quit" message box. This uses the <see cref="LoadingScreen"/> to
        /// transition from the game back to the <see cref="MainMenuScreen"/>.
        /// </summary>
        void ConfirmQuitMessageBoxAccepted(object sender, EventArgs e)
        {
            // 6/18/2012 - Get the GameLevelManager service
            var gameLevelManager = (IGameLevelManager)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(IGameLevelManager));

            // 1/21/2011 - Reset GameLevelManager to start at level-1.
            if (gameLevelManager != null && gameLevelManager.UseGameLevelManager)
                gameLevelManager.SetCurrentGameLevelToRun(1);

            // 1/12/2011 - Reload back the default MainMenu screens.
            //LoadingScreen.Load(ScreenManager, false, new BackgroundScreen(), new MainMenuScreen());
            LoadingScreen.Load(ScreenManager, false, ScreenManager.MainMenuScreens.ToArray());

        }

        /// <summary>
        /// <see cref="EventHandler"/>  for when the End/Leave Session <see cref="MenuEntry"/> is selected.
        /// </summary>
        void LeaveSessionMenuEntrySelected(object sender, EventArgs e)
        {
            NetworkSessionComponent.LeaveSession(ScreenManager);
        }

        #endregion

        #region Draw


        /// <summary>
        /// Draws the <see cref="PauseMenuScreen"/>. This darkens down the gameplay screen
        /// that is underneath us, and then chains to the base <see cref="MenuScreen.Draw2D"/> .
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public sealed override void Draw2D(GameTime gameTime)
        {
            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

            base.Draw2D(gameTime);
        }


        #endregion

        // 1/19/2011
        #region Update

        /// <summary>
        /// Updates the <see cref="MenuScreen"/>, by iterating the internal collection of <see cref="MenuEntry"/> and
        /// calling the 'Update' method for each.
        /// </summary>  
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="otherScreenHasFocus">Another <see cref="GameScreen"/> has focus?</param>
        /// <param name="coveredByOtherScreen">Is covered by another <see cref="GameScreen"/>?</param>      
        public sealed override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // 1/19/2011 - Check if GameTrialOver.
            ResumeGameMenuEntry.IsSelectable = !TemporalWars3DEngine.IsGameTrialOver ||
                                                TemporalWars3DEngine.IsPurchasedGame;
        }

        #endregion

        // 5/29/2012
        /// <summary>
        /// Handler for when the user has cancelled the <see cref="MenuScreen"/>, which
        /// calls the <see cref="GameScreen.ExitScreen"/> method.
        /// </summary>
        protected sealed override void OnCancel()
        {
            // Set PauseState is over.
            TemporalWars3DEngine.GamePaused = false;

            base.OnCancel();
        }
    }
}
