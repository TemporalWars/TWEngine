namespace TWEngine
{
    /// <summary>
    /// Represents the current state of each <see cref="IFOWPlayer"/> in the game
    /// </summary>
    public interface IFOWPlayer
    {
        //List<IFOWSceneItem> SelectableItems { get; } 2/2/2010 - Discovered this creates TONS of garbage on heap!!! :(

        /// <summary>
        /// True when this <see cref="IFOWPlayer"/> sighted an enemy <see cref="IFOWPlayer"/> units or buildings.
        /// </summary>
        bool PlayerSightedEnemyPlayer { get; set; }
    }
}