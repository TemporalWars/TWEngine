#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Net;
using TWEngine.BeginGame.Enums;
using TWEngine.Common;
using TWEngine.Interfaces;
using TWEngine.Networking;
using Microsoft.Xna.Framework;
using TWEngine.ScreenManagerC;

namespace TWEngine.GameScreens.Generic
{
    /// <summary>
    /// The <see cref="MainMenuScreen"/> is the first thing displayed when the game starts up.
    /// </summary>
    public class MainMenuScreen : MenuScreen
    {    
        // 2/17/2009 - Add MenuEntry Rectangle for background.
        // Use the X/Y Position, which will be relative to the MenuScreen Position.
        protected readonly Rectangle MenuEntryBackgroundRectangle = new Rectangle(0, 60, 600, 40);
        // 2/20/2009 - Add MenuEntry TextMargin
        protected readonly Vector2 MenuEntryTextMargin = new Vector2(15, 0);

        #region Initialization

        // 4/6/2010: Updated to use 'ContentTexturesLoc' global var.
        /// <summary>
        /// Constructor fills in the <see cref="MainMenuScreen"/> contents.
        /// </summary>
        /// <param name="backgroundTexture"> </param>
        public MainMenuScreen(string backgroundTexture)
            : base(Resources.MainMenu, null, new Vector2(600, 300), backgroundTexture ?? TemporalWars3DEngine.ContentTexturesLoc + @"\Textures\roundedMenu")
        {
            ConstructorCommonInit();
        }

        // 6/17/2012 - Overload with Texture2D
        /// <summary>
        /// Constructor fills in the <see cref="MainMenuScreen"/> contents.
        /// </summary>
        public MainMenuScreen(Texture2D backgroundImage)
            : base(Resources.MainMenu, null, new Vector2(600, 300), backgroundImage)
        {
            ConstructorCommonInit();
        }

        // 6/17/2012 - Overload with Color
        /// <summary>
        /// Constructor fills in the <see cref="MainMenuScreen"/> contents.
        /// </summary>
        public MainMenuScreen(Color backgroundColor)
            : base(Resources.MainMenu, null, new Vector2(600, 300), backgroundColor)
        {
            ConstructorCommonInit();
        }

        // 6/16/2012
        /// <summary>
        /// Called by the constructors.
        /// </summary>
        private void ConstructorCommonInit()
        {
            // 10/28/2009 - Set what Rendering Style the MainMenu should use!
            ScreenManager.RenderingType = RenderingType.NormalRendering;

            // 4/21/2011 - Retrieve Cursor interface and hide cursor.
            var cursor = (Cursor) TemporalWars3DEngine.GameInstance.Services.GetService(typeof (ICursor));
            cursor.Visible = false;

            // Create our menu entries.
            var singlePlayerMenuEntry = new MenuEntry(Resources.SinglePlayer, MenuEntryBackgroundRectangle, MenuEntryTextMargin);
            var liveMenuEntry = new MenuEntry(Resources.PlayerMatch, MenuEntryBackgroundRectangle, MenuEntryTextMargin);
            var systemLinkMenuEntry = new MenuEntry(Resources.SystemLink, MenuEntryBackgroundRectangle, MenuEntryTextMargin);
            var exitMenuEntry = new MenuEntry(Resources.Exit, MenuEntryBackgroundRectangle, MenuEntryTextMargin);


            // Hook up menu event handlers.
            singlePlayerMenuEntry.Selected += SinglePlayerMenuEntrySelected;

            // 8/31/2008 - Is XNA Live Framework available? 
            if (TemporalWars3DEngine.IsXnaLiveReady)
            {
                // Hook XNA Live Events
                liveMenuEntry.Selected += LiveMenuEntrySelected;
                systemLinkMenuEntry.Selected += SystemLinkMenuEntrySelected;
            }
            else
            {
                // Hook 'Not Available' Message
                liveMenuEntry.Selected += XnaLiveFrameworkUnavailable;
                systemLinkMenuEntry.Selected += XnaLiveFrameworkUnavailable;
            }

            exitMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            AddMenuEntry(singlePlayerMenuEntry);
            AddMenuEntry(liveMenuEntry);
            AddMenuEntry(systemLinkMenuEntry);
            AddMenuEntry(exitMenuEntry);

            // 4/8/2009 - Set IsSelected.
            IsSelected = true;
        }

        #endregion

        #region Handle Input

        /// <summary>
        /// <see cref="EventHandler"/> for when the <see cref="SinglePlayerScreen"/> menu entry is selected.
        /// </summary>
        void SinglePlayerMenuEntrySelected(object sender, EventArgs e)
        {

            // 2/22/2009 - Load the 'SinglePlayerScreen.
            ScreenManager.AddScreen(new SinglePlayerScreen(), false);
            
#if !XBOX360
            // 8/18/2008 - Show Load Map Tool  
            //LoadMapsTool loadMapsTool = new LoadMapsTool(this);
            //loadMapsTool.Visible = true;            
#else                    
            //LoadingScreen.Load(ScreenManager, true, new TerrainScreen(null, null));
#endif
        }

        /// <summary>
        /// <see cref="EventHandler"/> for when the Live menu entry is selected.
        /// </summary>
        void LiveMenuEntrySelected(object sender, EventArgs e)
        {
            CreateOrFindSession(NetworkSessionType.PlayerMatch);
        }      


        /// <summary>
        /// <see cref="EventHandler"/> for when the System Link menu entry is selected.
        /// </summary>
        void SystemLinkMenuEntrySelected(object sender, EventArgs e)
        {           
            CreateOrFindSession(NetworkSessionType.SystemLink);
        }

        // 8/19/2008 
        /// <summary>
        /// <see cref="EventHandler"/> for when the XNA Framework is not on the target computer,
        /// then this event handler is attached to the 'Live' & 'System Link' menu options.
        /// </summary>       
        void XnaLiveFrameworkUnavailable(object sender, EventArgs e)
        {
            var missingXnaLiveMessageBox = new MessageBoxScreen(Resources.ErrorXNALiveNotAvailable) { IsPopup = true };

            ScreenManager.AddScreen(missingXnaLiveMessageBox, false);
        }


        /// <summary>
        /// Helper method shared by the Live and System Link menu event handlers.
        /// </summary>
        void CreateOrFindSession(NetworkSessionType sessionType)
        {
            // First, we need to make sure a suitable gamer profile is signed in.
            var profileSignIn = new ProfileSignInScreen(sessionType);

            // Hook up an event so once the ProfileSignInScreen is happy,
            // it will activate the CreateOrFindSessionScreen.
            profileSignIn.ProfileSignedIn += delegate
            {
                ScreenManager.AddScreen(new CreateOrFindSessionScreen(sessionType), false);
            };

            // Activate the ProfileSignInScreen.
            ScreenManager.AddScreen(profileSignIn, false);
        }


        /// <summary>
        /// When the user cancels the main menu, ask if they want to exit the sample.
        /// </summary>
        protected sealed override void OnCancel()
        {
            var confirmExitMessageBox =
                                    new MessageBoxScreen(Resources.ConfirmExitSample);

            confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;

            ScreenManager.AddScreen(confirmExitMessageBox, false);
        }


        /// <summary>
        /// <see cref="EventHandler"/> for when the user selects ok on the "are you sure
        /// you want to exit" message box.
        /// </summary>
        void ConfirmExitMessageBoxAccepted(object sender, EventArgs e)
        {            

            ScreenManager.Game.Exit();
        }        


        #endregion
    }
}
