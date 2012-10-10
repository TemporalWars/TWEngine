using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWLate.RTS_FogOfWarInterfaces.FOW
{
    ///<summary>
    /// The <see cref="IFOWTerrainShape"/> class is a manager, which uses the other terrain classes to create and manage
    /// the <see cref="IFOWTerrainShape"/>.  For example, the drawing of the terrain is initialized in this class, but the actual drawing is
    /// done in the TerrainQuadTree class.  This class also loads the <see cref="IFOWSceneItem"/> into memory at the
    /// beginning of a level load.  
    ///</summary>
    /// <remarks>This class uses the TerrainAlphaMaps, TerrainPickingRoutines, and
    /// the TerrainEditRoutines classes.</remarks>
    public interface IFOWTerrainShape
    {
        /// <summary>
        /// Sets the <see cref="IFogOfWar"/> texture into <see cref="Effect"/>. 
        /// </summary>
        /// <remarks>Called from the <see cref="IFogOfWar"/> component.</remarks>
        /// <param name="isVisible">Sets the isVisible flag</param>
        void SetFogOfWarSettings(bool isVisible);

        /// <summary>
        /// Sets the <see cref="IFogOfWar"/> texture into <see cref="Effect"/>.  
        /// </summary>
        /// <remarks>Called from the <see cref="IFogOfWar"/> component.</remarks>
        /// <param name="fowTexture"><see cref="Texture2D"/> instance</param>
        void SetFogOfWarTextureEffect(Texture2D fowTexture);
    }
}