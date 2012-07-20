using Microsoft.Xna.Framework;

namespace TWEngine
{
    /// <summary>
    /// The <see cref="IFOWSceneItem"/> is the base class, which provides the primary funtions
    /// for any <see cref="IFOWSceneItem"/>.  This includes updating the transforms for position data, 
    /// updating attributes, like current health, etc.
    /// This class inherts from a collection of <see cref="IFOWSceneItem"/>, allowing a single item to have
    /// multiple children of <see cref="IFOWSceneItem"/>.
    /// </summary>
    public interface IFOWSceneItem
    {
        /// <summary>
        /// Returns the <see cref="IFogOfWarShapeItem"/> instance.
        /// </summary>
        IFogOfWarShapeItem ShapeItem { get; }

        /// <summary>
        /// The position for this <see cref="IFOWSceneItem"/>
        /// </summary>
        Vector3 Position { get; set; }

        /// <summary>
        /// The <see cref="IFOWPlayer"/> number this <see cref="IFOWSceneItem"/> belongs to.
        /// </summary>
        byte PlayerNumber { get; }

        ///<summary>
        /// Is current position value the final position for this <see cref="IFOWSceneItem"/>?
        ///</summary>
        /// <remarks>Generally only applies the static items which do not move, like a building scene item.</remarks>
        bool ItemPlacedInFinalPosition { get; set; }

        ///<summary>
        /// The <see cref="GameTime"/> this item was placed at.
        ///</summary>
        /// <remarks>This is used to determine <see cref="IFogOfWar"/> visibility for enemy players.</remarks>
        double TimePlacedAt { get; set; }
        
        ///<summary>
        /// Set to force an update to World <see cref="Matrix"/>.
        ///</summary>
        bool UpdateWorldMatrix { get; set; }

        // 6/10/2010
        /// <summary>
        /// Stores the unique item number for this <see cref="IFOWSceneItem"/>. 
        /// </summary>
        /// <remarks>
        /// It should be set to either the 'SceneItemNumber' or 'NetworkItemNumber', depending if 
        /// SP or MP game type.
        /// </remarks>
        int UniqueItemNumber { get; }
    }
}