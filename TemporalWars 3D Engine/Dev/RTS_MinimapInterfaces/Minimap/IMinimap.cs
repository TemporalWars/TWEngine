using System;
using ImageNexus.BenScharbach.TWEngine.TemporalWarInterfaces.Interfaces;
using ImageNexus.BenScharbach.TWLate.RTS_FogOfWarInterfaces.FOW;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWLate.RTS_MinimapInterfaces.Minimap
{
    // 8/14/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine"/> namespace contains the classes
    /// which make up the entire <see cref="IMinimap"/>.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    /// <summary>
    /// The <see cref="IMinimap"/> is designed to show unit movement, structures
    /// placed in the game world and to take direct mouse-click orders (Windows Platform). 
    /// The <see cref="IMinimap"/> XNA component will also communicate with the <see cref="IFogOfWar"/> XNA component,
    /// directly showing the influences of the <see cref="IFogOfWar"/> in real time.
    /// </summary>
   public interface IMinimap : IDisposable, ICommonInitilization
    {
        /// <summary>
        /// <see cref="IMinimap"/> width
        /// </summary>
// ReSharper disable InconsistentNaming
        int MMWidth { get; set; }

// ReSharper restore InconsistentNaming
        /// <summary> 
        /// <see cref="IMinimap"/> height
        /// </summary>
// ReSharper disable InconsistentNaming
        int MMHeight { get; set; }

// ReSharper restore InconsistentNaming
        /// <summary>
        /// Set to tell <see cref="IMinimap"/> to update for unit positions.
        /// </summary>
        bool DoUpdateMiniMap { set; }

        /// <summary>
        /// Set to render landscape for <see cref="IMinimap"/> 
        /// </summary>
        bool DoInitMiniMap { set; }

        /// <summary>
        /// Tracks if <see cref="IMinimap"/> contains the cursor. 
        /// </summary>
        bool MiniMapContainsCursor { get; set; }

        /// <summary>
        /// <see cref="MiniMapDestination"/> destination <see cref="Rectangle"/>.
        /// </summary>
        Microsoft.Xna.Framework.Rectangle MiniMapDestination { get; set; }

        /// <summary>
        /// Controls <see cref="IMinimap"/> display.
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// Call to set the interface references for the <see cref="IMinimapTerrainShape"/>,
        /// <see cref="IMinimapTerrainData"/>, <see cref="ICamera"/> and <see cref="IMinimapInterfaceDisplay"/>.
        /// </summary>
        void InitSettings();

        /// <summary>
        /// Saves the <see cref="IMinimap"/> texture using the given name; use the console
        /// to call this method.
        /// </summary>
        /// <remarks>The name given will be saved with '_MMP' and the .BMP extension.</remarks>
        /// <param name="name">The name you want to save the <see cref="IMinimap"/> texture as.</param>
        void SaveMiniMapTexture(string name);

        /// <summary>
        /// Controls the displaying of the <see cref="Texture2D"/> wrapper
        /// around the <see cref="IMinimap"/>.
        /// </summary>
        bool ShowTextureWrapper { set; get; }

        // 2/25/2011
        ///<summary>
        /// Updates the default wrapper texture to the given <paramref name="wrapperTexture"/>.
        ///</summary>
        ///<param name="wrapperTexture">Instance of <see cref="Texture2D"/>.</param>
        void UpdateWrapperTexture(Texture2D wrapperTexture);

       // 2/26/2011
        /// <summary>
        /// Uses the built-in default texture called 'IFDTileMinimapWrap'.
        /// </summary>
        void UseDefaultWrapperTexture();

        /// <summary>
        /// Renders the landscape, using orthogonal projection, onto
        /// the _landScapeRt <see cref="RenderTarget"/>.
        /// </summary>     
        void RenderLandscapeForMiniMap();

        /// <summary>
        /// <see cref="EventHandler"/> to force the <see cref="IMinimap"/> internal <see cref="IFogOfWarShapeItem"/> positions to update. 
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void UpdateMiniMapPosition_EventHandler(object sender, EventArgs e);
    }
}
