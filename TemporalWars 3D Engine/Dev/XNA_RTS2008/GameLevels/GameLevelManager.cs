#region File Description
//-----------------------------------------------------------------------------
// GameLevelManager.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TWEngine.GameLevels.ChangeRequests;
using TWEngine.GameScreens;
using TWEngine.GameScreens.Generic;
using TWEngine.Networking.Structs;
using TWEngine.ScreenManagerC;


namespace TWEngine.GameLevels
{
    /// <summary>
    /// The <see cref="GameLevelManager"/> is responsible for updating
    /// the current <see cref="GameLevel"/>, until is complete.  Once complete,
    /// the manager will then start the next <see cref="GameLevel"/>, and repeat
    /// the cycle.
    /// </summary>
    public class GameLevelManager : GameComponent
    {
        // 1/21/2011 - Updated from Queue to List, so gameLevels are no longer removed.
        // 10/2/2009 - List of GameLevels.
        private static List<GameLevel> _gameLevels;

        // 10/2/2009 - Set to current gameLevel.
        private static GameLevel _currentGameLevel;

        // 1/21/2011 - Current GameLevel position within List.
        private static int _currentGameLevelIndex = -1;
        // 1/22/2011 - Safes ref to the current TerrainScreen instance.
        private static TerrainScreen _terrainScreen;
        // 1/22/2011 - Default GameInfo struct.
        private static GamerInfo _gamerInfo;
        // 1/22/2011 - Ref to current GameTime.
        private static GameTime _gameTime;

        #region Properties

        // 10/2/2009 - Flag set during SinglePlayer games; used as check to know to call the GameLevelManager.
        /// <summary>
        /// In order to run GameLevels, this flag must be set to true; this is to tell
        /// the class to call <see cref="GameLevelManager"/>, which starts the first <see cref="GameLevel"/>.
        /// </summary>
        public static bool UseGameLevelManager { get; set; }

        // 1/22/2011
        /// <summary>
        /// The <see cref="GamerInfo"/> structure, stores the current gamers
        /// Player side selection, location, and color.
        /// </summary>
        public static GamerInfo GamerInfo
        {
            get { return _gamerInfo; }
            set { _gamerInfo = value; }
        }

        #endregion

        ///<summary>
        /// Constructor for the <see cref="GameLevelManager"/>, which
        /// initializes the internal Queue for <see cref="GameLevel"/>s.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance.</param>
        public GameLevelManager(Game game)
            : base(game)
        {
            // Initialize the GameLevels List.
            _gameLevels = new List<GameLevel>();

            // 1/22/2011 - Init default GamerInfo Struct
            GamerInfo = new GamerInfo
                             {
                                 ColorName
                                     =
                                     "FireBrick Red",
                                 PlayerColor
                                     =
                                     Color.
                                     Firebrick,
                                 PlayerLocation
                                     = 1,
                                 PlayerSide
                                     = 1
                             };

            // 1/22/2011 - Subscribe to the TerrainScreen.Unloading event.
            TerrainScreen.UnLoading += TerrainScreen_UnLoading;

            
           
        }

        // 1/22/2011
        /// <summary>
        /// EventHandler which clears out all <see cref="GameLevel"/> instances.
        /// </summary>
// ReSharper disable InconsistentNaming
        private static void TerrainScreen_UnLoading(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            // 1/21/2011 - Unload any GameLevelParts for the current GameLevel (Scripting Purposes)
            if (UseGameLevelManager)
                ClearCurrentGameLevelParts();

            if (_gameLevels != null)
                _gameLevels.Clear();
            
        }

        // 1/21/2011
        ///<summary>
        /// Sets the current game-level to run; for example, 
        /// 'Game Level-1' would be the value 1.  
        ///</summary>
        ///<param name="gameLevelIndex">Game-level index to run.</param>
        ///<exception cref="ArgumentOutOfRangeException">Thrown when the value given is less than 1.</exception>
        public static void SetCurrentGameLevelToRun(int gameLevelIndex)
        {
            // check if valid gameLevelIndex
            if (gameLevelIndex < 1)
                throw new ArgumentOutOfRangeException("gameLevelIndex", String.Format("Value given MUST be 1 or greater."));

            // Set index to 0 base.
            _currentGameLevelIndex = gameLevelIndex - 1;

        }


        // 2/1/2010
        /// <summary>
        /// Dequeues a new GameLevel from the internal Queue, and
        /// returns the 'TerrainMapToLoad' name.
        /// </summary>
        /// <returns>TerrainMap name to load for this level.</returns>
        /// <exception cref="InvalidOperationException">Thrown when either the internal Queue is null or empty.</exception>
        /// <exception cref="ArgumentException">Thrown when the call to <see cref="SetCurrentGameLevelToRun"/> was 
        /// given a game level value which does not exist.</exception>
        public virtual string GetTerrainMapToLoad()
        {
            // make sure gameLevels Queue not null
            if (_gameLevels == null)
                throw new InvalidOperationException("GameLevels Queue is null!");

            // make sure Queue not empty.
            if (_gameLevels.Count == 0)
                throw new InvalidOperationException("GameLevels Queue is empty!  You must have at least one GameLevel added to the queue.");

            // 1/21/2011 - Verify some game level index was set.
            if (_currentGameLevelIndex == -1)
                throw new InvalidOperationException("You MUST call the 'SetCurrentGameLevelToRun' before calling this method.");

            // 1/22/2011 - Verify index is valid
            if (_currentGameLevelIndex > _gameLevels.Count - 1)
                throw new ArgumentException("Current game level set to run does not exist.");

            // 1/21/2011 - Updated to retrieve using index value.
            // dequeues next gameLevel off top of Queue.
            _currentGameLevel = _gameLevels[_currentGameLevelIndex];

            // 2/1/2010 - Retrieve map name to load, from current game level.
            return _currentGameLevel.TerrainMapToLoad;
                
        }

        /// <summary>
        /// Updates the current <see cref="GameLevel"/>, by calling the <see cref="GameLevel.LevelUpdate"/> method.
        /// Once a level is completed, the <see cref="GameLevel.LevelEnd"/> method is called, and the <see cref="StartGameLevel"/>
        /// method is called to queue up the next <see cref="GameLevel"/>, if any.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public override void Update(GameTime gameTime)
        {
            // 5/29/2012 - Enter Pause check here.
            if (TemporalWars3DEngine.GamePaused)
                return;

            base.Update(gameTime);

            // 1/22/2011 - Save Gametime ref.
            _gameTime = gameTime;

            // 1/19/2011 - Check if Game trial is over.
            if (!TemporalWars3DEngine.IsPurchasedGame && TemporalWars3DEngine.IsGameTrialOver)
                return;

            // Run current game level
            if (_currentGameLevel == null)
                return;

            // 5/17/2012 - Check for any ScriptinAction ChangeRequests to complete
            ScriptingActionChangeRequestManager.DoChangeRequestUpdates(gameTime);

            // Update current Level.
            _currentGameLevel.LevelUpdate(gameTime);

            // 1/15/2010 - Check if 'LevelComplete' set to True.
            if (!_currentGameLevel.LevelComplete) return;

            // 1/21/2011 - Set index to '-1', which implies no more levels.
            // Note: Additional levels will be set from the 'LevelEnd' using 'SetCurrentGameLevelToRun' call.
            _currentGameLevelIndex = -1;

            //
            // Level Complete, so call 'LevelEnd', and start next level, if any.
            //

            // trigger LevelEnd.
            _currentGameLevel.LevelEnd();
           

            // Retrieve ScreenManager service.
            var screenManager =
                (ScreenManager)
                TemporalWars3DEngine.GameInstance.Services.GetService(typeof(ScreenManager));


            // 1/21/2011 - Check to load another game level or return to main menu.
            if (_currentGameLevelIndex != -1)
            {
                // 1/22/2011 - Remove current instance of TerrainScreen.
                screenManager.RemoveScreen(_terrainScreen);

                // 1/22/2011 - Load Game using given map name, and GamerInfo data.
                LoadTerrainScreen();
            }
            else
            {
                // Reload back the default MainMenu screens.
                LoadingScreen.Load(screenManager, false, ScreenManager.MainMenuScreens.ToArray());
                // Reset flag for current game level.
                _currentGameLevel.LevelComplete = false;
                // Reset to load level-1
                SetCurrentGameLevelToRun(1);
            }

            // NOTE: Removed, since called in TerrainScreen initialize method.
            // start next game level, if any.
            //if (!StartGameLevel())
                //_currentGameLevel = null;
        }

        // 1/22/2011
        /// <summary>
        /// Starts the process of loading the <see cref="TerrainScreen"/>.
        /// </summary>
        public static void LoadTerrainScreen()
        {
            // Retrieve ScreenManager service.
            var screenManager =
                (ScreenManager)
                TemporalWars3DEngine.GameInstance.Services.GetService(typeof(ScreenManager));

            // Load Game using given map name, and GamerInfo data.
            _terrainScreen = new TerrainScreen("", GamerInfo);
            LoadingScreen.Load(screenManager, true, _terrainScreen);
        }

        // 10/2/2009
        /// <summary>
        /// Adds a new <see cref="GameLevel"/> to the internal Queue.
        /// </summary>
        /// <param name="gameLevelToAdd"><see cref="GameLevel"/> to add to internal Queue.</param>
        /// <exception cref="InvalidOperationException">Thrown when internal Queue is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="gameLevelToAdd"/> is null.</exception>
        public static void AddGameLevel(GameLevel gameLevelToAdd)
        {
            // make sure Queue not null
            if (_gameLevels == null)
                throw new InvalidOperationException("Internal Queue 'GameLevels' is NULL!");

            // make sure not null
            if (gameLevelToAdd == null)
                throw new ArgumentNullException("gameLevelToAdd", @"GameLevel to add cannot be NULL!");

            // Add new level
            _gameLevels.Add(gameLevelToAdd);
        }

        // 10/2/2009
        /// <summary>
        /// Starts up a <see cref="GameLevel"/>, by calling the <see cref="GameLevel.LevelBegin"/> on the current
        /// game level.
        /// </summary>
        /// <returns>True/False of success</returns>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="GetTerrainMapToLoad"/> was not called first.</exception>
        public static bool StartGameLevel()
        {
            // 2/1/2010 - Check if '_currentGameLevel' is null.
            if (_currentGameLevel == null)
                throw new InvalidOperationException("There is no Current game level to load; you MUST call the 'GetTerrainMapToLoad' method first!");

            // 1/22/2011 - Verify camera is reset and unlocked.
            ScriptingActions.Camera_LockOrUnlock(false);
            ScriptingActions.Camera_SetDefaultCameraBoundArea();
            ScriptingActions.Camera_ResetAngle(_gameTime, ScriptingActions.CameraAngle.Both);

            // call 'LevelBegin'.
            _currentGameLevel.LevelBegin();

            return true;
        }

        // 1/21/2011
        ///<summary>
        /// Sets the current game level as complete.
        ///</summary>
        ///<exception cref="InvalidOperationException">Thrown when there is no current game level to set <see cref="GameLevel.LevelComplete"/>.</exception>
        public static void SetCurrentGameLevelComplete()
        {
            // 2/1/2010 - Check if '_currentGameLevel' is null.
            if (_currentGameLevel == null)
                throw new InvalidOperationException("There is no Current game level to update.");

            // set as complete.
            _currentGameLevel.LevelComplete = true;

        }

        // 1/21/2011
        ///<summary>
        /// Removes all <see cref="GameLevelPart"/> instances from the current
        /// game level.
        ///</summary>
        ///<exception cref="InvalidOperationException">Thrown when there is no current game level to clear <see cref="GameLevelPart"/> instances.</exception>
        public static void ClearCurrentGameLevelParts()
        {
            // 2/1/2010 - Check if '_currentGameLevel' is null.
            if (_currentGameLevel == null)
                throw new InvalidOperationException("There is no Current game level to clear parts for.");

            // Clear all GameLevelParts
            _currentGameLevel.ClearCurrentGameLevelParts();
        }

        // 6/6/2012
        /// <summary>
        /// Used to unload resources during level loads.  Called from TerrainScreen class.
        /// </summary>
        public static void UnloadContent()
        {
            // Unload content for ScriptingACtionRequestMAnager
            ScriptingActionChangeRequestManager.UnloadContent();
            // Unload content for ScriptingActions
            ScriptingActions.UnloadContent();
            // Unload content for ScriptingCondition
            ScriptingConditions.UnloadContent();
        }
       
    }
}