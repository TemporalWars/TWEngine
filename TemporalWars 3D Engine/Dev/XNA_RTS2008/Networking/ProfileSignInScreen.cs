#region File Description
//-----------------------------------------------------------------------------
// ProfileSignInScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.GameScreens.Generic;
using ImageNexus.BenScharbach.TWEngine.ScreenManagerC;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.GamerServices;

namespace ImageNexus.BenScharbach.TWEngine.Networking
{
    /// <summary>
    /// In order to play a networked game, you must have a player profile signed in.
    /// If you want to play on Live, that has to be a Live profile. Rather than just
    /// failing with an error message, it is nice if we can automatically bring up the
    /// <see cref="Guide"/> screen when we detect that no suitable profiles are currently signed in,
    /// so the user can easily correct the problem. This screen checks the sign in
    /// state, and brings up the <see cref="Guide"/> user interface if there is a problem with it.
    /// It then raises an event as soon as a valid profile has been signed in.
    /// 
    /// There are two scenarios for how this can work. If no good profile is signed in:
    /// 
    ///     - <see cref="MainMenuScreen"/>  activates the <see cref="ProfileSignInScreen"/>
    ///     - <see cref="ProfileSignInScreen"/> activates the Guide user interface
    ///     - User signs in a profile
    ///     - <see cref="ProfileSignInScreen"/> raises the ProfileSignedIn event
    ///     - This advances to the <see cref="CreateOrFindSessionScreen"/>
    /// 
    /// Alternatively, there might already be a valid profile signed in. In this case:
    /// 
    ///     - <see cref="MainMenuScreen"/>  activates the <see cref="ProfileSignInScreen"/>
    ///     - <see cref="ProfileSignInScreen"/> notices everything is already good
    ///     - <see cref="ProfileSignInScreen"/> raises the ProfileSignedIn event
    ///     - This advances to the <see cref="CreateOrFindSessionScreen"/>
    /// 
    /// In this second case, the <see cref="ProfileSignInScreen"/> is only active for a single
    /// Update, so the user just sees a transition directly from the <see cref="MainMenuScreen"/> 
    /// to the <see cref="CreateOrFindSessionScreen"/>.
    /// </summary>
    class ProfileSignInScreen : GameScreen
    {
        #region Fields

        readonly NetworkSessionType _sessionType;
        bool _haveShownGuide;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a profile has signed in.
        /// </summary>
        public event EventHandler<EventArgs> ProfileSignedIn;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new profile sign in screen.
        /// </summary>
        /// <param name="sessionType"><see cref="NetworkSessionType"/> Enum</param>
        public ProfileSignInScreen(NetworkSessionType sessionType)
        {
            _sessionType = sessionType;

            IsPopup = true;
        }


        #endregion

        #region Update

        /// <summary>
        /// Updates the profile sign in screen.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="otherScreenHasFocus">If other screen has focus.</param>
        /// <param name="coveredByOtherScreen">If covered by other screen.</param>
        public sealed override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (ValidProfileSignedIn())
            {
                // As soon as we detect a suitable profile is signed in,
                // we raise the profile signed in event, then go away.
                if (ProfileSignedIn != null)
                    ProfileSignedIn(this, EventArgs.Empty);

                ExitScreen();
            }
            else if (IsActive && !Guide.IsVisible)
            {
                if (!_haveShownGuide)
                {
                    // No suitable profile is signed in, and we haven't already shown
                    // the Guide. Let's show it now, so they can sign in a profile.
                    var onlineOnly = (_sessionType == NetworkSessionType.PlayerMatch);

                    Guide.ShowSignIn(NetworkSessionComponent.MaxLocalGamers,
                                     onlineOnly);

                    _haveShownGuide = true;
                }
                else
                {
                    // Hmm. No suitable profile is signed in, but we already showed
                    // the Guide, and the Guide isn't still visible. There is only
                    // one thing that can explain this: they must have cancelled the
                    // Guide without signing in a profile. We'd better just exit,
                    // which will leave us on the same menu as before.
                    ExitScreen();
                }
            }
        }


        /// <summary>
        /// Helper checks whether a valid player profile is signed in.
        /// </summary>
        bool ValidProfileSignedIn()
        {
            // 4/20/2010 - Cache
            var signedInGamerCollection = Gamer.SignedInGamers;
            if (signedInGamerCollection == null) return false;
            
            // If there are no profiles signed in, that is never good.
            var count = signedInGamerCollection.Count;
            if (count == 0) return false;

            // If we want to play in a Live session, also make sure the profiles are
            // signed in to Live, and that they have the privilege for online gameplay.
            if (_sessionType == NetworkSessionType.PlayerMatch)
            {
                for (var i = 0; i < count; i++)
                {
                    if (i >= NetworkSessionComponent.MaxLocalGamers)
                        break;

                    var gamer = signedInGamerCollection[i];
                    if (gamer == null) continue; // 4/20/2010

                    if (!gamer.IsSignedInToLive)
                        return false;

                    if (!gamer.Privileges.AllowOnlineSessions)
                        return false;
                }
            }

            // Okeydokey, this looks good.
            return true;
        }


        #endregion
    }
}
