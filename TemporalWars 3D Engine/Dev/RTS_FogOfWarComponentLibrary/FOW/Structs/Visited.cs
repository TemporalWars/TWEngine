using Microsoft.Xna.Framework;

namespace TWEngine.FOW
{
    /// <summary>
    /// The <see cref="Visited"/> structure is used to store when a visiting <see cref="IFOWSceneItem"/> has
    /// occurred, storing the exact <see cref="GameTime"/> of the occurrence. 
    /// </summary>
    public struct Visited
    {
        ///<summary>
        /// Was <see cref="IFOWSceneItem"/> visited by some other <see cref="IFOWSceneItem"/>?
        ///</summary>
        public bool WasVisited;

        ///<summary>
        /// Exact <see cref="GameTime"/> of the occurrence.
        ///</summary>
        public double TimeVisitedAt;
    }
}