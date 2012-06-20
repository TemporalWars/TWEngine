#region File Description
//-----------------------------------------------------------------------------
// GameLevelPart.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TWEngine.SceneItems;

namespace TWEngine.GameLevels
{
    ///<summary>
    /// Abstract class, used in creating a <see cref="GameLevelPart"/> class.  The
    /// <see cref="GameLevelPart"/> item, is where all the level logic is stored.  <see cref="GameLevel"/>s
    /// will usually have multiple <see cref="GameLevelPart"/>s running at any one time, in parallel.  Furthermore,
    /// as the <see cref="GameLevel"/> progresses, additional <see cref="GameLevelPart"/>s will be spun into
    /// action, until the entire level is complete.
    ///</summary>
    /// <remarks><see cref="GameLevelPart"/>s can be as simple as monitoring a 'TriggerArea', until some <see cref="SceneItem"/> 
    /// enters the area, and trigger some <see cref="ScriptingActions"/>.  Or it can be more complex, allowing camera 
    /// movement, <see cref="SceneItem"/> movement, particle effect generation, and the creation of additional
    /// <see cref="GameLevelPart"/>s.</remarks>
    public abstract class GameLevelPart
    {
        // 1/15/2010 - 
        /// <summary>
        /// Set to signal this LevelPart is complete.
        /// </summary>
        public bool LevelPartComplete { get; set; }

        // 1/15/2010
        /// <summary>
        /// Controls the frequency of calls to the LevelPartUpdate method.
        /// You can set this to values from 0-60, where the number is how
        /// often the update will occur; use 0 for every frame.
        /// </summary>
        public int LevelPartFrequency { get; set; }

        // 1/15/2010; 1/15/2011 - Moved from GameLevel down to GameLevelPart.
        ///<summary>
        /// Used to track the Frequency count.
        ///</summary>
        public int FrequencyCounter { get; set; }

        // 1/15/2010
        /// <summary>
        /// Set to signal removal from Active list.
        /// </summary>
        public bool LevelPartIsActive { get; set; }

        // 1/15/2010
        /// <summary>
        /// Set additional LevelParts to trigger to start, when
        /// this LevelPart ends.
        /// </summary>
        internal List<int> TriggerLevelParts { get; set; }


        // Constructor
        /// <summary>
        /// Constructor for the <see cref="GameLevelPart"/>, which
        /// simply creates the internal List of <see cref="int"/>s, which
        /// represent additional <see cref="GameLevelPart"/> to use or start.
        /// </summary>
        protected GameLevelPart()
        {
            // Init the internal TriggerLevelParts array.
            TriggerLevelParts = new List<int>();
        }

        /// <summary>
        /// Allows the GameLevel to initialize data/services at inception.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// This is where actions or events which should occur at 
        /// the beginning of a <see cref="GameLevelPart"/> is placed.
        /// </summary>
        public abstract void LevelPartBegin();

        // 3/6/2011 - Updated with new param - accumGameTime
        /// <summary>
        /// This is where the actual <see cref="GameLevelPart"/> updates occur.  
        /// </summary>
        /// <remarks>Will continue to be called, until the <see cref="LevelPartComplete"/> is set to True.</remarks>
        /// <param name="gameTime">Instance of game time.</param>
        /// <param name="accumGameTimeTotalMs">Total accumlation of gameTime.</param>
        public abstract void LevelPartUpdate(GameTime gameTime, double accumGameTimeTotalMs);

        /// <summary>
        /// This is where actions or events which should occur at
        /// the end of a <see cref="GameLevelPart"/> is placed.
        /// </summary>
        public abstract void LevelPartEnd();


        // 1/15/2010
        /// <summary>
        /// Use to add additional <see cref="GameLevelPart"/> which are set 'Active',
        /// when this <see cref="GameLevelPart"/> ends.
        /// </summary>
        /// <param name="key">Add the key(s) of linked <see cref="GameLevelPart"/>s.</param>
        public void AddLinkedLevelParts(params int[] key)
        {
            // iterate keys params, and add to internal List
            for (var i = 0; i < key.Length; i++)
            {
               TriggerLevelParts.Add(key[i]);
            }
            
        }

    }
}
