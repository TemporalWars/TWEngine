namespace TWEngine
{
    /// <summary>
    /// Required reference to game engine.
    /// </summary>
    public interface IFOWEngineRef
    {

        /// <summary>
        /// Content Textures project folder location.
        /// </summary>
        string ContentTexturesLoc { get; } // 4/6/2010

        /// <summary>
        /// Stores the A* path node size, or number of nodes in
        /// the given graph; for example, 57 is 57x57.
        /// </summary>
        int PathNodeSize { get; set; }

        /// <summary>
        /// Reference to the <see cref="IFOWPlayer"/> instance.
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
        IFOWPlayer[] Players { get; }
    }
}