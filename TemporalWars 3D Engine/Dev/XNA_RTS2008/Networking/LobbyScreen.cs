#region File Description
//-----------------------------------------------------------------------------
// LobbyScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion


using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Net;
using TWEngine.GameScreens;
using TWEngine.GameScreens.Generic;
using TWEngine.MemoryPool;
using TWEngine.Networking.Structs;
using TWEngine.rtsCommands;
using TWEngine.rtsCommands.Enums;
using TWEngine.ScreenManagerC;
using TWEngine.ScreenManagerC.Enums;


namespace TWEngine.Networking
{

    /// <summary>
    /// The <see cref="LobbyScreen"/> provides a place for gamers to congregate before starting
    /// the actual gameplay. It displays a list of all the gamers in the session,
    /// and indicates which ones are currently talking. Each gamer can press a button
    /// to mark themselves as ready: gameplay will begin after everyone has done this.
    /// </summary>
    class LobbyScreen : GameScreen
    {
        #region Fields        

        /// <summary>
        /// The <see cref="LobbyScreenData"/> structure
        /// </summary>
        protected static LobbyScreenData LobbyScreenData;

        /// <summary>
        /// <see cref="NetworkSession"/> instance.
        /// </summary>
        protected static NetworkSession NetworkSession;

        private Texture2D _isReadyTexture;
        private Texture2D _hasVoiceTexture;
        private Texture2D _isTalkingTexture;
        private Texture2D _voiceMutedTexture;

        // 4/8/2009 - Adj of Y value, when 4/3 vs widescreen resolutions.
        protected int AspectRatioAdj;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new lobby screen.
        /// </summary>
        /// <param name="networkSession"><see cref="NetworkSession"/> instance</param>
        public LobbyScreen(NetworkSession networkSession)
        {
            NetworkSession = networkSession;

            LobbyScreenData.mapName = "GreenValley"; // default

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }


        /// <summary>
        /// Loads graphics content used by the lobby screen, using the
        /// <see cref="TemporalWars3DEngine.ContentResourceManager"/>.
        /// </summary>
        /// <param name="contentManager"> </param>
        public override void LoadContent(ContentManager contentManager)
        {
            // 9/1/2009 - Load from Resources instance.
            var contentResourceManager = TemporalWars3DEngine.ContentResourceManager;
            _isReadyTexture = contentResourceManager.Load<Texture2D>("chat_ready");
            _hasVoiceTexture = contentResourceManager.Load<Texture2D>("chat_able");
            _isTalkingTexture = contentResourceManager.Load<Texture2D>("chat_talking");
            _voiceMutedTexture = contentResourceManager.Load<Texture2D>("chat_mute");
        }


        #endregion

        #region Update

        /// <summary>
        /// Updates the lobby screen.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="otherScreenHasFocus">If other screen has focus?</param>
        /// <param name="coveredByOtherScreen">If this screen covered by other screen?</param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (IsExiting) return;

            // 9/1/2009
            UpdateLobbyScreen(this);
        }

        /// <summary>
        /// Updates the <see cref="LobbyScreen"/>.
        /// </summary>
        /// <param name="gameScreen"><see cref="GameScreen"/> instance</param>
        private static void UpdateLobbyScreen(GameScreen gameScreen)
        {
            // 9/1/2009 - Cache
            var networkSession = NetworkSession;
            if (networkSession == null) return;

            var count = networkSession.AllGamers.Count; // 11/19/2009

            if (networkSession.SessionState == NetworkSessionState.Playing)
            {
                // 4/8/2009 - Get GamerInfo from TAG object, and save into player's.    
                var gamerInfoHost = new GamerInfo();
                var gamerInfoClient = new GamerInfo();
                for (var i = 0; i < count; i++)
                {
                    // 11/19/2009 - cache
                    var gamer = networkSession.AllGamers[i];
                    if (gamer == null) continue;

                    if (gamer.IsHost)
                    {
                        // save into host player
                        gamerInfoHost = (GamerInfo) gamer.Tag;
                    }
                    else
                    {
                        // save into client player
                        gamerInfoClient = (GamerInfo) gamer.Tag;
                    }
                }

                // Check if we should leave the lobby and begin gameplay.
                LoadingScreen.Load(gameScreen.ScreenManager, true,
                                   new TerrainScreen(networkSession, LobbyScreenData.mapName, gamerInfoHost, gamerInfoClient));
            }
            else if (networkSession.IsHost && networkSession.IsEveryoneReady)
            {
                // 12/9/2008 - We won't let the game start if there is only one person!
                if (count >= 2)
                {
                    // The host checks whether everyone has marked themselves
                    // as ready, and starts the game in response.
                    networkSession.StartGame();
                }
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
        public override void DoHandleInput(GameTime gameTime, InputState input)
        {
            // 4/20/2010 - Cache
            var networkSession = NetworkSession;
            if (networkSession == null) return;

            // 4/20/2010 - Cache
            var localNetworkGamers = networkSession.LocalGamers;
            if (localNetworkGamers == null) return;

            // 9/2/2008: Update to ForLoop, rather than ForEach
            var count = localNetworkGamers.Count; // 11/19/2009o
            for (var i = 0; i < count; i++)
            {
                // 11/19/2009 - Cache
                var localGamer = localNetworkGamers[i];
                if (localGamer == null || localGamer.SignedInGamer == null) continue;

                var playerIndex = localGamer.SignedInGamer.PlayerIndex;

                // 9/1/2009: Updated to use the new 'IsReady' property of InputState!
                //if (input.IsNewButtonPress(Buttons.X, playerIndex) || input.IsNewKeyPress(Keys.X, playerIndex))
                if (input.IsReady)
                {
                    HandleMenuSelect(localGamer);
                }
                else if (input.IsMenuCancel(playerIndex))
                {
                    HandleMenuCancel(localGamer);
                }
            }
        }


        /// <summary>
        /// Handle MenuSelect inputs by marking ourselves as ready.
        /// </summary>
        /// <param name="networkGamer"><see cref="NetworkGamer"/> instance</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="networkGamer"/> is null.</exception>
        void HandleMenuSelect(NetworkGamer networkGamer)
        {
            // 4/20/2010 - Check if param null.
            if (networkGamer == null)
                throw new ArgumentNullException("networkGamer", @"The parameter 'NetworkGamer' cannot be null!");

            // 9/1/2009 - Before setting 'IsReady', make sure proper atts are choosen.
            if (!IsUserReallyReady(networkGamer))
                return;

            if (!networkGamer.IsReady)
            {
                networkGamer.IsReady = true;
            }
            else if (networkGamer.IsHost)
            {
                // NOTE: (11/19/2009); this affects the 'UpdateLobbyScreen', which is where
                //                     the call is made to start off the game!
                // The host has an option to force starting the game, even if not
                // everyone has marked themselves ready. If they press select twice
                // in a row, the first Time marks the host ready, then the second
                // Time we ask if they want to force start.
                var messageBox = new MessageBoxScreen(Resources.ConfirmForceStartGame);

                messageBox.Accepted += ConfirmStartGameMessageBoxAccepted;

                if (ScreenManager != null) ScreenManager.AddScreen(messageBox, false);
            }
        }

        // 9/1/2009
        /// <summary>
        /// Checks if the given <see cref="Gamer"/> is 'Really' ready, by making
        /// sure they have picked a team 'Color' and some 'PlayerSide'.
        /// </summary>
        /// <param name="gamer"><see cref="Gamer"/> to check</param>
        /// <returns>True/False of result</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="gamer"/> is null.</exception>
        private static bool IsUserReallyReady(Gamer gamer)
        {
            // 4/20/2010 - Check if param null.
            if (gamer == null)
                throw new ArgumentNullException("gamer", @"The 'Gamer' parameter cannot be null!");

            var gamerInfo = (GamerInfo)gamer.Tag;

            // make sure color choosen
            if (gamerInfo.ColorName == "-------------" || gamerInfo.PlayerColor == Color.Black)
            {
                _errorMessage = "You need to choose a team color.";
                return false;
            }

            // make sure some player location choosen
            if (gamerInfo.PlayerLocation == 0)
            {
                _errorMessage = "You need to choose a player location.";
                return false;
            }

            _errorMessage = string.Empty;
            return true;
        }


        // 9/1/2009
        /// <summary>
        /// Sets the 'IsReady' property for all local gamers, to False.  This overload
        /// version is called from the <see cref="NetworkGameComponent"/> class.
        /// </summary>
        /// <param name="networkSession"><see cref="NetworkSession"/> instance</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="networkSession"/> is null.</exception>
        public static void SetLocalGamersToNotReady(NetworkSession networkSession)
        {
            // 4/20/2010 - Check if null
            if (networkSession == null)
                throw new ArgumentNullException("networkSession", @"The parameter 'NetworkSession' cannot be null!");

            // 4/20/2010 - Cache
            var localNetworkGamers = networkSession.LocalGamers;
            if (localNetworkGamers == null) return;

            // iterate all Local gamers, and set 'IsReady' to FALSE.
            var count = localNetworkGamers.Count;
            for (var i = 0; i < count; i++)
            {
                // 11/19/2009 - cache
                var localGamer = localNetworkGamers[i];
                if (localGamer == null) continue;

                localGamer.IsReady = false;
            }
        }

        // 9/1/2009
        /// <summary>
        /// Sets the 'IsReady' property for all local gamers, to False.
        /// </summary>
        protected void SetLocalGamersToNotReady()
        {
            // set 'IsReady' to FALSE.
            SetLocalGamersToNotReady(NetworkSession);

            // send 'NotReady' to all other players, so their
            // 'IsReady' property will be changed to 'False'.
            RTSCommLobbyData lobbyData;
            PoolManager.GetNode(out lobbyData);

            // 4/20/2010 - Check if null.
            if (lobbyData == null) return;

            lobbyData.Clear();
            lobbyData.NetworkCommand = NetworkCommands.LobbyData_UserNotReady;

            SendLobbyData(this, lobbyData);
        }

        // 9/1/2009
        /// <summary>
        /// Sends the given <see cref="RTSCommLobbyData"/> command, to the other player.
        /// </summary>
        /// <param name="lobbyScreen"><see cref="LobbyScreen"/> instance</param>
        /// <param name="lobbyData"><see cref="RTSCommLobbyData"/> instance</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="lobbyScreen"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="lobbyData"/> is null.</exception>
        protected static void SendLobbyData(LobbyScreen lobbyScreen, RTSCommLobbyData lobbyData)
        {
            // 4/20/2010 - Check if params are null.
            if (lobbyScreen == null)
                throw new ArgumentNullException("lobbyScreen", @"The parameter 'lobbyScreen' cannot be null!");

            if (lobbyData == null)
                throw new ArgumentNullException("lobbyData", @"The parameter 'lobbyData' cannot be null!");

            // 4/20/2010 - Check if null
            if (NetworkSession != null)
                if (NetworkSession.IsHost)
                {
                    // Send update to client                       
                    NetworkGameComponent.AddCommandsForClientG(lobbyData);
                    NetworkGameComponent.SendPacketThisFrame = true;
                }
                else
                {
                    // Send update to host                      
                    NetworkGameComponent.AddCommandsForServerG(lobbyData);
                    NetworkGameComponent.SendPacketThisFrame = true;
                }// End If Host or Client
        }

        /// <summary>
        /// <see cref="EventHandler"/> for when the host selects ok on the "are you sure
        /// you want to start even though not everyone is ready" message box.
        /// </summary>
        static void ConfirmStartGameMessageBoxAccepted(object sender, EventArgs e)
        {
            // 4/20/2010 - Cache
            var networkSession = NetworkSession;
            if (networkSession == null) return;

            if (networkSession.SessionState != NetworkSessionState.Lobby) return;

            // 11/19/2009 - Set TerrainScreen to be in 'SandBoxMode'.
            TerrainScreen.SandBoxMode = true;

            networkSession.StartGame();
        }


        /// <summary>
        /// Handle MenuCancel inputs by clearing our ready Status, or if it is
        /// already clear, prompting if the user wants to leave the session.
        /// </summary>
        /// <param name="gamer"><see cref="LocalNetworkGamer"/> instance</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="gamer"/> is null.</exception>
// ReSharper disable SuggestBaseTypeForParameter
        void HandleMenuCancel(LocalNetworkGamer gamer)
// ReSharper restore SuggestBaseTypeForParameter
        {
            // 4/20/2010 - Check if param null
            if (gamer == null)
                throw new ArgumentNullException("gamer", @"The parameter 'gamer' cannot be null!");

            if (gamer.IsReady)
            {
                gamer.IsReady = false;
            }
            else
            {
                NetworkSessionComponent.LeaveSession(ScreenManager);
            }
        }


        #endregion

        #region Draw
        
        private Vector2 _titlePosition = new Vector2(533, 80);
        private Color _titleColor = new Color(192, 192, 192, 0);

        /// <summary>
        /// The <see cref="Draw2D"/> method draws the lobby screen, by calling the internal
        /// method <see cref="DrawLobbyScreen"/>.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public override void Draw2D(GameTime gameTime)
        {
            // 9/1/2009
            DrawLobbyScreen(this);
        }

        /// <summary>
        /// Draws the lobby screen.
        /// </summary>
        /// <param name="lobbyScreen"><see cref="LobbyScreen"/> instance</param>
        private static void DrawLobbyScreen(LobbyScreen lobbyScreen)
        {
            // 9/1/2009 - Cache
            var networkSession = NetworkSession;
            if (networkSession == null) return; // 4/20/2010

            var position = new Vector2 {X = 75, Y = 550 + lobbyScreen.AspectRatioAdj};

            // Make the lobby slide into place during transitions.
            var transitionOffset = (float)Math.Pow(lobbyScreen.TransitionPosition, 2);

            if (lobbyScreen.ScreenState == ScreenState.TransitionOn)
                position.X -= transitionOffset * 256;
            else
                position.X += transitionOffset * 512;

            // 4/20/2010 - Cache
            var screenManager = lobbyScreen.ScreenManager;
            if (screenManager == null) return;

            // Start SpriteBatch draw.
            ScreenManager.SpriteBatch.Begin();

            // Draw all the gamers in the session.
            var gamerCount = 0;

            // 4/20/2010 - Cache
            var gamerCollection = networkSession.AllGamers;
            if (gamerCollection == null) return;

            // 9/2/2008: Updated to ForLoop, rather than ForEach.
            var count = gamerCollection.Count;
            for (var i = 0; i < count; i++)
            {
                // 11/19/2009 - cache
                var networkGamer = gamerCollection[i];
                if (networkGamer == null) continue;

                DrawGamer(lobbyScreen, networkGamer, ref position);

                // Advance to the next screen Position, wrapping into two
                // columns if there are more than 8 gamers in the session.
                if (++gamerCount == 8)
                {
                    position.X += 433;
                    position.Y = 150;
                }
                else
                    position.Y += ScreenManager.Font.LineSpacing;
            }      
    
            // 9/1/2009 - Draw ErrorMessage, if any; for example, not having the proper Color choosen!
            DrawErrorMessage(lobbyScreen);
                
            // Draw the screen title.
            var title = Resources.Lobby;

            var tmpMeasureString = ScreenManager.Font.MeasureString(title);
            // 4/7/2009 - Updated to use Ref version to optimize on XBOX!
            Vector2 titleOrigin;
            Vector2.Divide(ref tmpMeasureString, 2, out titleOrigin);

            lobbyScreen._titlePosition.Y -= transitionOffset * 100;
            lobbyScreen._titleColor.R = lobbyScreen._titleColor.G = lobbyScreen._titleColor.B = 192;
            lobbyScreen._titleColor.A = lobbyScreen.TransitionAlpha; // 4/7/2009
            ScreenManager.SpriteBatch.DrawString(ScreenManager.Font, title, lobbyScreen._titlePosition, lobbyScreen._titleColor, 0,
                                                 titleOrigin, 1.25f, SpriteEffects.None, 0.1f);

            ScreenManager.SpriteBatch.End();
        }

        private static string _errorMessage = string.Empty;

        // 9/1/2009
        /// <summary>
        /// Draws the current 'Error' message to the screen; the error
        /// message can be something like 'Choose proper Team Color', for example.
        /// </summary>
        /// <param name="lobbyScreen"><see cref="LobbyScreen"/> instance</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="lobbyScreen"/> is null.</exception>
        private static void DrawErrorMessage(LobbyScreen lobbyScreen)
        {
            // 4/20/2010 - Check if param null.
            if (lobbyScreen == null)
                throw new ArgumentNullException("lobbyScreen", @"The parameter 'lobbyScreen' cannot be null!");

            if (string.IsNullOrEmpty(_errorMessage))
                return;

            var position = new Vector2 { X = 75, Y = 650 + lobbyScreen.AspectRatioAdj };

            ScreenManager.SpriteBatch.DrawString(ScreenManager.Font, _errorMessage, position, Color.Red, 0,
                                                 Vector2.Zero, 0.80f*TemporalWars3DEngine.ScreenScale,
                                                 SpriteEffects.None, 0);
        }

        // 4/7/2009
        private Vector2 _iconWidth = new Vector2(34, 0);
        private Vector2 _iconOffset = new Vector2(0, 12);
        private Color _fadeColor = Color.White;
        private Color _fadeColorAlpha = Color.White;

        // 6/16/2010 - Updated to use StringBuilder, to reduce GC on Xbox!
        private static readonly StringBuilder LobbyText = new StringBuilder(150);

        /// <summary>
        /// Helper draws the gamer-tag and status icons for a single <see cref="NetworkGamer"/>.
        /// </summary>
        /// <param name="lobbyScreen"><see cref="LobbyScreen"/> instance</param>
        /// <param name="networkGamer"><see cref="NetworkGamer"/> instance</param>
        /// <param name="inPosition"><see cref="Vector2"/> as position</param>
        private static void DrawGamer(LobbyScreen lobbyScreen, NetworkGamer networkGamer, ref Vector2 inPosition)
        {
            // 4/20/2010 - Check if params null
            if (lobbyScreen == null)
                throw new ArgumentNullException("lobbyScreen", @"The parameter 'lobbyScreen' cannot be null!");
            if (networkGamer == null)
                throw new ArgumentNullException("networkGamer",@"The parameter 'networkGamer' cannot be null!");

            // 9/1/2009 - Cache
            var scale = TemporalWars3DEngine.ScreenScale;

            // 7/25/2009 - Save copy of Position, since it will be changed.
            var position = inPosition;

            // 4/8/2009 - Apply Scale
            Vector2.Multiply(ref lobbyScreen._iconOffset, scale, out lobbyScreen._iconOffset);
            Vector2.Multiply(ref position, scale, out position);

            Vector2 iconPosition;
            Vector2.Add(ref position, ref lobbyScreen._iconOffset, out iconPosition);

            // 4/20/20201 - Cache
            var screenManager = lobbyScreen.ScreenManager;
            if (screenManager == null) return;

            // Draw the "is ready" icon.
            if (networkGamer.IsReady)
            {
                // 4/7/2009 - Updated to use new FadeAlphaDuringTransition with ref/out format.
                lobbyScreen._fadeColor = Color.Lime;
                FadeAlphaDuringTransition(lobbyScreen.TransitionAlpha, ref lobbyScreen._fadeColor, out lobbyScreen._fadeColorAlpha);
                ScreenManager.SpriteBatch.Draw(lobbyScreen._isReadyTexture, iconPosition, lobbyScreen._fadeColorAlpha);
            }

            iconPosition += lobbyScreen._iconWidth;

            // Draw the "is muted", "is talking", or "has voice" icon.
            if (networkGamer.IsMutedByLocalUser)
            {
                // 4/7/2009 - Updated to use new FadeAlphaDuringTransition with ref/out format.
                lobbyScreen._fadeColor = Color.Red;
                FadeAlphaDuringTransition(lobbyScreen.TransitionAlpha, ref lobbyScreen._fadeColor, out lobbyScreen._fadeColorAlpha);
                ScreenManager.SpriteBatch.Draw(lobbyScreen._voiceMutedTexture, iconPosition, lobbyScreen._fadeColorAlpha);
            }
            else if (networkGamer.IsTalking)
            {
                // 4/7/2009 - Updated to use new FadeAlphaDuringTransition with ref/out format.
                lobbyScreen._fadeColor = Color.Yellow;
                FadeAlphaDuringTransition(lobbyScreen.TransitionAlpha, ref lobbyScreen._fadeColor, out lobbyScreen._fadeColorAlpha);
                ScreenManager.SpriteBatch.Draw(lobbyScreen._isTalkingTexture, iconPosition, lobbyScreen._fadeColorAlpha);
            }
            else if (networkGamer.HasVoice)
            {
                // 4/7/2009 - Updated to use new FadeAlphaDuringTransition with ref/out format.
                lobbyScreen._fadeColor = Color.White;
                FadeAlphaDuringTransition(lobbyScreen.TransitionAlpha, ref lobbyScreen._fadeColor, out lobbyScreen._fadeColorAlpha);
                ScreenManager.SpriteBatch.Draw(lobbyScreen._hasVoiceTexture, iconPosition, lobbyScreen._fadeColorAlpha);
            }

            // 4/7/2009 - Check if Gamer 'Tag' is null; if so, then let's add a new Struct 'GamerInfo'.
            if (networkGamer.Tag == null)
            {
                // Set to Color.Black, which is not a pickable Team Color, to force user to pick proper color.
                var gamerInfo = new GamerInfo {PlayerColor = Color.Black, PlayerSide = 1};
                GetColorName(ref gamerInfo.PlayerColor, out gamerInfo.ColorName);               

                networkGamer.Tag = gamerInfo;
            }

            // 4/18/2009: Updated to use the String.Concat to optimize.
            // 4/7/2009: Updated to also draw the Side/Color for current player.
            // Draw the gamertag, normally in white, but yellow for local players.
            var gamerInfo2Display = (GamerInfo)networkGamer.Tag;

            // 6/15/2010 - Updated to use StringBuilder, to reduce GC on Xbox!
            LobbyText.Length = 0;// Required step, in order to start the appending at the beg!
            LobbyText.Append(networkGamer.Gamertag);
            LobbyText.Append(" Side: ");
            LobbyText.Append(gamerInfo2Display.PlayerSideString); // 6/16/2010 - Updated to use new string version.
            LobbyText.Append(", Loc: ");
            LobbyText.Append(gamerInfo2Display.PlayerLocationString); // 6/16/2010 - Updated to use new string version.
            LobbyText.Append(", Color: ");
            LobbyText.Append(gamerInfo2Display.ColorName);

            // Append extra data if Host.
            if (networkGamer.IsHost)
            {
                LobbyText.Append(Resources.HostSuffix);

                /*text =
                    String.Concat(
                        String.Concat(networkGamer.Gamertag, " Side: ", gamerInfo2Display.PlayerSide.ToString(),
                                      ", Loc: ", gamerInfo2Display.PlayerLocation.ToString(), ", Color: ",
                                      gamerInfo2Display.ColorName), Resources.HostSuffix);*/
            }
            

            var color = (networkGamer.IsLocal) ? Color.Goldenrod : Color.White;

            // 4/8/2009 - Optimize for XBOX.
            Vector2 dblIconWidth, finalPosition;
            Vector2.Multiply(ref lobbyScreen._iconWidth, 2, out dblIconWidth);
            Vector2.Add(ref position, ref dblIconWidth, out finalPosition);

            // 4/7/2009 - Updated to use new FadeAlphaDuringTransition with ref/out format.            
            FadeAlphaDuringTransition(lobbyScreen.TransitionAlpha, ref color, out lobbyScreen._fadeColorAlpha);
            ScreenManager.SpriteBatch.DrawString(ScreenManager.Font, LobbyText, finalPosition, lobbyScreen._fadeColorAlpha, 0, Vector2.Zero, 0.80f * scale, SpriteEffects.None, 0);
        }

        // 4/7/2009 - Updated to pass 1st param by ref, and 2nd as an out, to optimize on XBOX!
        /// <summary>
        /// Helper modifies a color to fade its alpha value during screen transitions.
        /// </summary>
        static void FadeAlphaDuringTransition(byte alpha, ref Color color, out Color alphaColor)
        {
            //return new Color(color.R, color.G, color.B, TransitionAlpha);
            alphaColor = Color.White;

            alphaColor.R = color.R;
            alphaColor.G = color.G;
            alphaColor.B = color.B;
            alphaColor.A = alpha;
        }        

        #endregion
    }
}
