namespace TWEngine
{
    /// <summary>
    /// Required reference to game engine.
    /// </summary>
    public interface IStatusBarEngineRef
    {
        ///<summary>
        /// Provides Content Misc project folder location.
        ///</summary>
        string ContentMiscLoc { get; } // 4/6/2010

        /// <summary>
        /// Provide reference to <see cref="IStatusBarPlayer"/> collection.
        /// </summary>
        IStatusBarPlayer[] Players { get; }
    }
}