#region File Description
//-----------------------------------------------------------------------------
// IIFDTileManager.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.IFDTiles;
using ImageNexus.BenScharbach.TWEngine.Players;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWLate.RTS_MinimapInterfaces.Minimap;
using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWEngine.Interfaces
{
    /// <summary>
    /// The <see cref="IIFDTileManager"/> interface manages all <see cref="IFDTile"/> instances, by
    /// adding them to the internal collections, drawing and updating each game cycle, and updating
    /// <see cref="SceneItem"/> placement into the game world.
    /// </summary>
// ReSharper disable InconsistentNaming
    public interface IIFDTileManager : IMinimapInterfaceDisplay
// ReSharper restore InconsistentNaming
    {
        ///<summary>
        /// Set to turn On/Off rendering of all <see cref="IFDTile"/> instances.
        ///</summary>
        bool IsVisible { get; set; }

        ///<summary>
        /// Set to turn On/Off rendering of all <see cref="IFDTile"/> instances.
        ///</summary>
        bool V // ReSharper restore UnusedMember.Global
        { get; set; }

        /// <summary>
        /// Returns reference to instance of <see cref="GraphicsDevice"/>
        /// </summary>
        GraphicsDevice GraphicsDevice { get; }

        /// <summary>
        /// Creates the default set of <see cref="IFDTile"/> to be shown on screen.
        /// </summary>
        /// <param name="playerSide"><see cref="Player"/> side</param>
        void CreateIFDTiles(int playerSide);
    }
}