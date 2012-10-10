#region File Description
//-----------------------------------------------------------------------------
// AIManager.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.ParallelTasks;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using Microsoft.Xna.Framework;
using TWEngine;

namespace ImageNexus.BenScharbach.TWEngine.AI
{
    // 8/21/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.AI"/> namespace contains the classes
    /// which make up the entire <see cref="AIManager"/>.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }


    ///<summary>
    /// AI Manager class, manages the AI Defense components and the
    /// AI AStar components.  Both the defense and a-star components are 
    /// inherited from the AbstractParallelFor class, which threads the 
    /// For-Loop statements.
    ///</summary>
    public sealed class AIManager : GameComponent
    {
        // 2/16/2010 - AIDefense ParallelFor instance
        private static AIDefenseParallelFor _aiDefenseParallelFor;
        // 2/16/2010 - AIaStar ParallelFor instance
        private static AIaStarParallelFor _aiAStarParallelFor;

        #region Properties

        // 6/1/2009 - 
        /// <summary>
        /// Is Multi-player or Single-player game.
        /// </summary>
        public static bool IsNetworkGame { get; set; }

        #endregion

        // contructor
        ///<summary>
        /// Constructor for the AI-Manager class, which initializes
        /// the components for the AI-Defense and AI-AStar internal
        /// ParallelFor components.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        public AIManager(Game game) : base(game)
        {
#if DEBUG
            // 11/7/2008 - StopWatchTimers           
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.AIDefenseThread, false); // "AIDefense-Thread1"
            // 7/20/2009 - StopWatchTimers
            StopWatchTimers.CreateStopWatchInstance(StopWatchName.AStarItemThread, false); // "AStarItem-Thread1"
#endif
            
            // 2/16/2010 - Create instance of AIDefense ParallelFor
            _aiDefenseParallelFor = new AIDefenseParallelFor(TemporalWars3DEngine.UseDotNetThreadPool);
            // 2/16/2010 - Create instance of AIAStar ParallelFor
            _aiAStarParallelFor = new AIaStarParallelFor(TemporalWars3DEngine.UseDotNetThreadPool);

           
        }       

        // 2/16/2010
        /// <summary>
        /// Updates the AI Defense per <see cref="SceneItem"/>.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public override void Update(GameTime gameTime)
        {
            // 5/29/2012 - Skip if game paused.
            if (TemporalWars3DEngine.GamePaused)
                return;

#if DEBUG
            // 2/16/2010 - Parallize For-Loop for AIDefense
            StopWatchTimers.StartStopWatchInstance(StopWatchName.AIDefenseThread); // "AIDefense-Thread1"
#endif

            _aiDefenseParallelFor.ParallelFor(gameTime);

#if DEBUG
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.AIDefenseThread);  //  "AIDefense-Thread1"  
#endif

#if DEBUG
            // 2/16/2010 - Parallize For-Loop for AI-AStaritem
            StopWatchTimers.StartStopWatchInstance(StopWatchName.AStarItemThread); // "AIDefense-Thread1"
#endif

            _aiAStarParallelFor.ParallelFor();

#if DEBUG
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.AStarItemThread);  //  "AIDefense-Thread1"
#endif

            base.Update(gameTime);
        }
           

        /// <summary>
        /// Add a SceneItemWithPick reference to one of the internal List array.  
        /// </summary>
        /// <param name="itemToAdd">SceneItemWithPick instance to add</param>
        public static void AddDefenseAI(SceneItemWithPick itemToAdd)
        {
            // 6/1/2009 - For MP games, ONLY the Host player will have the DefenseAI added and running.
            //            However, DefenseScenes are the excpetion!
            var sThisPlayer = TemporalWars3DEngine.SThisPlayer;

            // 6/15/2010 - Updated to use new GetPlayer method.
            global::ImageNexus.BenScharbach.TWEngine.Players.Player player;
            if (!TemporalWars3DEngine.GetPlayer(sThisPlayer, out player))
                return;

            var networkSession = player.NetworkSession;
            if (networkSession != null && (!(itemToAdd is DefenseScene) && !networkSession.IsHost)) return;

            // 2/16/2010
            _aiDefenseParallelFor.AddSceneItem(itemToAdd);
        }

        /// <summary>
        /// Add a SceneItemWithPick reference to one of the internal List array.
        /// </summary>
        /// <param name="itemToAdd">SceneItemWithPick instance to add</param>
        public static void AddAStarItemAI(SceneItemWithPick itemToAdd)
        {
            // 2/16/2010
            _aiAStarParallelFor.AddSceneItem(itemToAdd);
        }

        // 7/17/2009
        /// <summary>
        /// Clears out ALL internal arrays for both the 'AIDefense' and 'AIAstarItem' arrays.
        /// Called when exiting a level.
        /// </summary>
        public static void ClearAIArrays()
        {
            // 2/16/2010
            if (_aiDefenseParallelFor != null)
                _aiDefenseParallelFor.ClearAIArrays();

            // 2/16/2010
            if (_aiAStarParallelFor != null)
                _aiAStarParallelFor.ClearAIArrays();
            
        }
        
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="isDisposing">Is this final dispose?</param>
        protected override void Dispose(bool isDisposing)
        {
            _aiDefenseParallelFor = null;
            _aiAStarParallelFor = null;

            base.Dispose(isDisposing);
        }

       
    }
}