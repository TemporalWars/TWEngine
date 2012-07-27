namespace TWEngine
{
    /// <summary>
    /// Required reference to game engine.
    /// </summary>
    public interface IMinimapEngineRef
    {
        /// <summary>
        /// Content Misc project folder location.
        /// </summary>
        string ContentMiscLoc { get; } // 4/6/2010
        /// <summary>
        /// Stores the A* path node size, or number of nodes in
        /// the given graph; for example, 57 is 57x57.
        /// </summary>
        int PathNodeSize { get; set; }
        /// <summary>
        /// Get or set the SPlayers collection index value, used
        /// to retrieve the current 'Players'.
        /// </summary>
        int ThisPlayer { get; }
        /// <summary>
        /// Stores the A* Graph's path node stride, or distance between
        /// a tile node.
        /// </summary>
        int PathNodeStride { get; }
        /// <summary>
        /// Max allowed players in game engine.
        /// </summary>
        int MaxAllowablePlayers { get; }
        /// <summary>
        /// Collection of <see cref="IFOWPlayer"/> items.
        /// </summary>
        IMinimapPlayer[] Players { get; }
    }
}