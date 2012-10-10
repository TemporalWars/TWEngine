using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWLate.RTS_MinimapInterfaces.Minimap
{
    /// <summary>
    /// Represents the current state of each <see cref="IMinimapPlayer"/> in the game
    /// </summary>
    public interface IMinimapPlayer
    {
        // 6/18/2010 - Updated to include TRUE count of 'SelectableItems' pass back to caller.
        // 6/17/2010 - Converted Property 'SelectableItems' to method 'GetSelectableItems'.
        /// <summary>
        /// Helper method used to retrieve the <see cref="IMinimapSceneItem"/> collections type.
        /// </summary> 
        /// <param name="minimapSceneItems">Returns a collection of <see cref="IMinimapSceneItem"/>.</param> 
        /// <param name="actualCount">The actual count value within the collection.</param> 
        void GetSelectableItems(ref IMinimapSceneItem[] minimapSceneItems, out int actualCount);

        /// <summary>
        /// Collection of <see cref="Rectangle"/> for <see cref="IMinimap"/>.
        /// </summary>
        Rectangle[] SelectableMinimapRects { get; set; }
        /// <summary>
        /// The current <see cref="IMinimapPlayer"/> <see cref="Color"/>.
        /// </summary>
        Color PlayerColor { get; }
    }
}