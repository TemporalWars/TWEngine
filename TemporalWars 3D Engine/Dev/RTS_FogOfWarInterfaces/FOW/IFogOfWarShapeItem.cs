using Microsoft.Xna.Framework;

namespace TWEngine
{
    /// <summary>
    /// The <see cref="IFogOfWarShapeItem"/> Interface allows a given Shape to 
    /// become <see cref="IFogOfWar"/> capable.
    /// </summary>
    public interface IFogOfWarShapeItem
    {
        ///<summary>
        /// The <see cref="Rectangle"/> structure as <see cref="IFogOfWarShapeItem"/> destination.
        ///</summary>
        Rectangle FogOfWarDestination { get; set; }

        /// <summary>
        /// The <see cref="IFogOfWarShapeItem"/> visibility height.
        /// </summary>
        int FogOfWarHeight { get; set; }

        /// <summary>
        /// Use the <see cref="IFogOfWar"/> with the <see cref="IFogOfWarShapeItem"/>.
        /// </summary>
        bool UseFogOfWar { get; set; }

        /// <summary>
        /// The <see cref="IFogOfWarShapeItem"/> visibility width.
        /// </summary>
        int FogOfWarWidth { get; set; }

        /// <summary>
        /// Is this <see cref="IFogOfWarShapeItem"/> in visible location?
        /// (Can be seen by enemy player)
        /// </summary>
        bool IsFOWVisible { get; set; }
    }
}
