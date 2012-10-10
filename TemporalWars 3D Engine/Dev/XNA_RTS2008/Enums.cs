
namespace ImageNexus.BenScharbach.TWEngine
{
    /// <summary>
    /// This enum if for the state transitions.
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// Default value - means no state is set
        /// </summary>
        None,

        /// <summary>
        /// Nothing visible, game has just been run and nothing is initialized
        /// </summary>
        Started,

        /// <summary>
        /// Logo Screen is being displayed
        /// </summary>
        LogoSplash,

        /// <summary>
        /// RTS Terrain Screen is being displayed
        /// </summary>
        RTSTerrain,

        /// <summary>
        /// Currently playing a version of the Evolved game
        /// </summary>
        PlayEvolved,

        /// <summary>
        /// Currently playing a version of the Evolved game
        /// </summary>
        PlayRetro,

        /// <summary>
        /// Choosing the ship
        /// </summary>
        ShipSelection,

        /// <summary>
        /// UpgradingWeapons
        /// </summary>
        ShipUpgrade,

        /// <summary>
        /// In the victory screen
        /// </summary>
        Victory,
    }
}