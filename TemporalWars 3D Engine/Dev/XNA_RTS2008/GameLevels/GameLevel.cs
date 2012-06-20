#region File Description
//-----------------------------------------------------------------------------
// GameLevel.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using TWEngine.Common;
using TWEngine.Common.Extensions;

namespace TWEngine.GameLevels
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.GameLevels"/> namespace contains the common classes
    /// used to create game levels and scripting commands.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    ///<summary>
    /// Abstract class, used in creating a <see cref="GameLevel"/> class.  A <see cref="GameLevel"/> contains
    /// a collection of <see cref="GameLevelPart"/>s, which make up the foundation
    /// of the <see cref="GameLevel"/>.  Once all <see cref="GameLevelPart"/> are completed, 
    /// the entire <see cref="GameLevel"/> is also complete.
    ///</summary>
    public abstract class GameLevel
    {
        // 10/2/2009 - Dictionary of GameLevelParts
        private readonly Dictionary<int, GameLevelPart> _gameLevelParts;

        // 1/15/2010 - Active GameLevelParts List
        private readonly List<GameLevelPart> _activeLevelParts;

        // 1/15/2010 - Flags when to check Active list for un-active parts.
        private bool _checkForUnActiveLevelParts;

        // 3/6/2011 - Tracks Accum GameTime when using frequency updates
        private double _accumGameTimeTotalMs;

        #region Properties

        // 1/15/2010 - 
        /// <summary>
        /// Set as 'True' to signal this Level is complete.
        /// </summary>
        public bool LevelComplete { get; set; }

        // 2/1/2010
        /// <summary>
        /// Requires TerrainMap name, which will load for this level.
        /// </summary>
        public string TerrainMapToLoad { get; set; }

        #endregion

        // Constructor
        /// <summary>
        /// Used to create a game level.  A Game level encapsulates many <see cref="GameLevelPart"/>s, which
        /// are required in order to build any game level.
        /// </summary>
        /// <param name="terrainMapToLoadName">Name of some TerrainMap to load with this game level.</param>
        /// <exception cref="ArgumentNullException">Thrown when given <paramref name="terrainMapToLoadName"/> is NULL.</exception>
        protected GameLevel(string terrainMapToLoadName)
        {
            // 2/1/2010 - Check if Null name given.
            if (string.IsNullOrEmpty(terrainMapToLoadName))
                throw new ArgumentNullException("terrainMapToLoadName", @"You MUST give a valid TerrainMap name to load for this level.");

            // 2/1/2010 - Save Map to load name.
            TerrainMapToLoad = terrainMapToLoadName;

            // Initialize the Dictionary
            _gameLevelParts = new Dictionary<int, GameLevelPart>();

            // 1/15/2010 - Init the Active List
           _activeLevelParts = new List<GameLevelPart>();
        }

        /// <summary>
        /// This is where actions or events which should occur at 
        /// the beginning of a <see cref="GameLevelPart"/> is placed.
        /// </summary>
        public abstract void LevelBegin();

        /// <summary>
        /// This is where the actual <see cref="GameLevelPart"/> updates occur.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance.</param>
        public virtual void LevelUpdate(GameTime gameTime)
        {
            // 1/15/2010 - Process all Active LevelParts
            for (var i = 0; i < _activeLevelParts.Count; i++)
            {
                // cache
                var gameLevelPart = _activeLevelParts[i];

                // process levelPart.
                ProcessGameLevelPart(gameLevelPart, gameTime);
            }

            // check if Active list needs to be refreshed.
            if (!_checkForUnActiveLevelParts) return;

            _activeLevelParts.RemoveAll(IsActive);
            _checkForUnActiveLevelParts = false; // reset
        }

        // 1/15/2010
        /// <summary>
        /// Predicate method, used to check if the <see cref="GameLevelPart.LevelPartIsActive"/> flag
        /// is False.
        /// </summary>
        private static bool IsActive(GameLevelPart levelPart)
        {
            return !levelPart.LevelPartIsActive;
        }

        // 1/15/2010
        /// <summary>
        /// Helper method, which process the given <see cref="GameLevelPart"/>.
        /// </summary>
        /// <param name="gameLevelPart"><see cref="GameLevelPart"/> to process.</param>
        /// <param name="gameTime"><see cref="GameTime"/> instance.</param>
        private void ProcessGameLevelPart(GameLevelPart gameLevelPart, GameTime gameTime)
        {
            if (gameLevelPart == null)
                return;

            // 3/6/2011 - Track GameTime Accum
            _accumGameTimeTotalMs += gameTime.ElapsedGameTime.Milliseconds;

            // 1/15/2011 - Updated to put 'FrequencyCounter' into GameLevelPart instance.
            // 1/15/2010 - Check Frequency
            if (gameLevelPart.LevelPartFrequency > 0)
            {
                gameLevelPart.FrequencyCounter++;
                if (gameLevelPart.FrequencyCounter <= gameLevelPart.LevelPartFrequency)
                    return;

                // reset counter
                gameLevelPart.FrequencyCounter = 0;
            }
            
            // Run current game level
            gameLevelPart.LevelPartUpdate(gameTime, _accumGameTimeTotalMs);

            // 3/6/2011 - Clear between calls.
            _accumGameTimeTotalMs = 0;

            // 1/15/2010 - Check if 'LevelPartComplete' set to True.
            if (!gameLevelPart.LevelPartComplete) return;

            //
            // LevelPart Complete, so call 'LevelPartEnd', and start next LevelPart, if any.
            //

            // trigger LevelEnd.
            gameLevelPart.LevelPartEnd();

            // 1/15/2010 - Set to be removed from Active list.
            gameLevelPart.LevelPartIsActive = false;
            _checkForUnActiveLevelParts = true; // tell Update method to do check.

            // Check for Linked LevelParts, in current LevelPart
            if (gameLevelPart.TriggerLevelParts.Count > 0)
            {
                // Set all Linked LevelParts to be Active.
                StartGameLevelPart(gameLevelPart.TriggerLevelParts);
            }
        }

        /// <summary>
        /// This is where actions or events which should occur at
        /// the end of a <see cref="GameLevelPart"/> is placed.
        /// </summary>
        public abstract void LevelEnd();

        // 10/2/2009
        /// <summary>
        /// Adds a new <see cref="GameLevelPart"/> to the internal <see cref="GameLevelPart"/>s Queue.
        /// </summary>
        /// <param name="key">Unique key for this <see cref="GameLevelPart"/></param>
        /// <param name="gameLevelPartToAdd">new Instance of <see cref="GameLevelPart"/> to Add</param>
        /// <exception cref="ArgumentNullException"><see cref="GameLevelPart"/> to add can not be NULL.</exception>
        /// <exception cref="ArgumentException">Thrown when given key already exist for another <see cref="GameLevelPart"/>.</exception>
        public void AddGameLevelPart(int key, GameLevelPart gameLevelPartToAdd)
        {
            // make sure not null
            if (gameLevelPartToAdd == null)
                throw new ArgumentNullException("gameLevelPartToAdd", @"GameLevelPart to add can not be NULL!");
           
            // make sure key is unique.
            if (_gameLevelParts.ContainsKey(key))
                throw new ArgumentException(@"Key given already exist for another GameLevelPart!", "key");

            // Add new LevelPart to the internal Dictionary
            _gameLevelParts.Add(key, gameLevelPartToAdd);
        }

        // 10/2/2009
        /// <summary>
        /// Starts up a <see cref="GameLevel"/>, by adding the given <see cref="GameLevelPart"/>(s)
        /// into the Active list.  
        /// </summary>
        /// <param name="key">Unique key(s) for <see cref="GameLevelPart"/> to set as Active.</param>
        /// <returns>True/False of success</returns>
        public bool StartGameLevelPart(params int[] key)
        {
            // make sure Dictionary not empty.
            if (_gameLevelParts.Count == 0)
                return false;

            // iterate params ints
            for (var i = 0; i < key.Length; i++)
            {
                // make sure given Key exist in Dictionary first
                var keyInstance = key[i];
                AddToActiveList(keyInstance);

            } // End List Pararms

            return true;
        }

        // 1/15/2010
        /// <summary>
        /// Helper method, called when another <see cref="GameLevelPart"/> ends, and it has 'Trigger' or linked
        /// <see cref="GameLevelPart"/> to make 'Active'. 
        /// </summary>
        /// <param name="key">List of Keys of <see cref="GameLevelPart"/> to make active</param>
        /// <returns>True/False of success</returns>
// ReSharper disable UnusedMethodReturnValue.Local
        private bool StartGameLevelPart(IList<int> key)
// ReSharper restore UnusedMethodReturnValue.Local
        {
            // make sure Dictionary not empty.
            if (_gameLevelParts.Count == 0)
                return false;

            // iterate given List, and add all to Active list.
            for (var i = 0; i < key.Count; i++)
            {
                // make sure given Key exist in Dictionary first
                var keyInstance = key[i];
                AddToActiveList(keyInstance);
            } // End List Pararms

            return true;
        }

        // 1/15/2010
        /// <summary>
        /// Helper method, used to specifically add the 'Key' of <see cref="GameLevelPart"/> 
        /// to the Active list.
        /// </summary>
        /// <param name="key">Key of <see cref="GameLevelPart"/> to add</param>
        /// <exception cref="ArgumentException">Thrown when given 'Key' does not exist in the internal dictionary.</exception>
        private void AddToActiveList(int key)
        {
            if (!_gameLevelParts.ContainsKey(key))
                throw new ArgumentException(
                    @"Key given does not exist in the internal Dictionary!  Make sure to add the GameLevelPart first.",
                    "key");

            // retrieve LevelPart from Dictionary.
            var currentGameLevelPart = _gameLevelParts[key];

            // call 'LevelPartBegin'.
            currentGameLevelPart.LevelPartBegin();
            
            // set to Active
            currentGameLevelPart.LevelPartIsActive = true;

            // 1/15/2010 - Add to Active list
            _activeLevelParts.Add(currentGameLevelPart);
        }

        // 1/21/2011
        ///<summary>
        /// Removes all <see cref="GameLevelPart"/> instances from the internal collection.
        ///</summary>
        public void ClearCurrentGameLevelParts()
        {
            // Check if dictionary is null
            if (_gameLevelParts == null || _gameLevelParts.Count == 0)
                return;

            // clear dictionary
            _gameLevelParts.Clear();
        }
    }
}
