#region File Description
//-----------------------------------------------------------------------------
// ItemStates.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

namespace ImageNexus.BenScharbach.TWEngine.SceneItems.Enums
{
    ///<summary>
    /// The <see cref="ItemStates"/> of some <see cref="SceneItem"/>; for example, item is in 'Resting' state
    /// or 'Moving' state.
    ///</summary>
    public enum ItemStates
    {
        ///<summary>
        /// <see cref="SceneItem"/> is sitting still in one spot.
        ///</summary>
        Resting,
        ///<summary>
        /// <see cref="SceneItem"/> is currently following some A* path.
        ///</summary>
        PathFindingAI, // 6/9/2009
        ///<summary>
        /// <see cref="SceneItem"/> is currently following some Temp goal, during
        /// an A-Star path.
        ///</summary>
        PathFindingTempGoal,
        ///<summary>
        /// <see cref="SceneItem"/> is actively waiting for the A* engine to 
        /// return a valid path to follow.
        ///</summary>
        PathFindingCalc,
        ///<summary>
        /// <see cref="SceneItem"/> retrieved and validated some A* path, and is
        /// ready to start the move operation.
        ///</summary>
        PathFindingReady,
        ///<summary>
        /// <see cref="SceneItem"/> is actively in an A* solution, following given nodes,
        /// and has stopped to wait for another unit to move or leave the next
        /// valid node within this units solution path.
        ///</summary>
        PausePathfinding,
        ///<summary>
        /// <see cref="SceneItem"/> is actively moving between one node to the next.
        ///</summary>
        PathFindingMoving,
        ///<summary>
        /// <see cref="SceneItem"/> is manually moving from one location to the next; not
        /// necessarily due to an A* solution.
        ///</summary>
        Moving,
        ///<summary>
        /// <see cref="SceneItem"/> is not a regular item, but a BotHelper <see cref="SceneItem"/>; most likely
        /// attached to some other parent or master <see cref="SceneItem"/>.
        ///</summary>
        BotHelper // 8/4/2009 - Used for Bots which always need to move around.
    }
}