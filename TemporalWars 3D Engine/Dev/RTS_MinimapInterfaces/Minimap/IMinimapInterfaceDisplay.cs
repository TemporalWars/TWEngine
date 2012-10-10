using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWLate.RTS_MinimapInterfaces.Minimap
{
    /// <summary>
    /// The <see cref="IMinimapInterfaceDisplay"/> class manages all IFDTile instances, by
    /// adding them to the internal collections, drawing and updating each game cycle, and updating
    /// SceneItem placement into the game world.
    /// </summary>
    public interface IMinimapInterfaceDisplay
    {
        /// <summary>
        /// Adds a new <see cref="IMinimapInterfaceDisplay"/> tile to the 'Display-Always' collection. This is a tile
        /// which will always be displayed.
        /// </summary>        
        /// <param name="tileTextureName"><see cref="IMinimapInterfaceDisplay"/> to add</param>
        /// <param name="tileLocation"><see cref="Rectangle"/> as tile location </param>
        /// <returns>Returns the tile's unique instance key.</returns>
        int AddInterFaceDisplayTileOverlay(string tileTextureName, Rectangle tileLocation);

        // 2/25/2011
        /// <summary>
        /// Adds a new <see cref="IMinimapInterfaceDisplay"/> tile to the 'Display-Always' collection. This is a tile
        /// which will always be displayed.
        /// </summary>        
        /// <param name="tileTexture"><see cref="IMinimapInterfaceDisplay"/> texture to add</param>
        /// <param name="tileLocation"><see cref="Rectangle"/> as tile location </param>  
        /// <returns>Returns the tile's unique instance key.</returns>
        int AddInterFaceDisplayTileOverlay(Texture2D tileTexture, Rectangle tileLocation);

        /// <summary>
        /// Explicit implementation for the interface <see cref="IMinimapInterfaceDisplay"/>.
        /// </summary>
        /// <param name="tileInstanceKey">The tile's unique instance key.</param>
        void RemoveInterFaceDisplayTile(int tileInstanceKey);

        
    }
}