using System;

namespace TWEngine
{
    ///<summary>
    /// The <see cref="IMinimapTerrainShape"/> class is a manager, which uses the other terrain classes to create and manage
    /// the Terrain.  For example, the drawing of the terrain is initialized in this class, but the actual drawing is
    /// done in the TerrainQuadTree class.  Furthermore, the class also loads the <see cref="IMinimapSceneItem"/> into memory at the
    /// beginning of a level load, while communicating with the TerrainAlphaMaps, TerrainPickingRoutines, and
    /// the TerrainEditRoutines classes.
    ///</summary>
    public interface IMinimapTerrainShape
    {   ///<summary>
        /// Occurs when the <see cref="IMinimapTerrainShape"/> is first created.
        ///</summary>
        event EventHandler TerrainShapeCreated;
        /// <summary>
        /// Draws the <see cref="IMinimapTerrainShape"/> landscape, using a top-down-view, 
        /// for the <see cref="IMinimap"/> component.
        /// </summary>
        void DrawMiniMap();
    }
}