using Microsoft.Xna.Framework;

namespace TWEngine
{
    /// <summary>
    /// The <see cref="IMinimapSceneItem"/> is the base class, which provides the primary funtions
    /// for any <see cref="IMinimapSceneItem"/>.  This includes updating the transforms for position data, 
    /// updating attributes, like current health, etc.
    /// This class inherts from a collection of <see cref="IMinimapSceneItem"/>, allowing a single item to have
    /// multiple child <see cref="IMinimapSceneItem"/>.
    /// </summary>
    public interface IMinimapSceneItem
    {   
        /// <summary>
        /// Returns the <see cref="IMinimapShapeItem"/> instance.
        /// </summary>
        IMinimapShapeItem ShapeItem { get; }
        /// <summary>
        /// Returns the <see cref="IMinimapShapeItem"/> instance.
        /// </summary>
        Vector3 Position { get; set; }
        /// <summary>
        /// Is this <see cref="IMinimapShapeItem"/> alive?
        /// </summary>
        bool IsAlive { get; } // 4/21/2010
    }
}