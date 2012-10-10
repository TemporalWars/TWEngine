#region File Description
//-----------------------------------------------------------------------------
// TileState.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.Players;
using ImageNexus.BenScharbach.TWEngine.SceneItems;

namespace ImageNexus.BenScharbach.TWEngine.IFDTiles.Enums
{
    // 9/28/2008 - State of Tile
    ///<summary>
    /// <see cref="TileState"/> Enum, used to represent the state of some <see cref="InterfaceDisplay"/> tile.
    ///</summary>
    public enum TileState
    {
        ///<summary>
        /// Default empty state
        ///</summary>
        None,
        ///<summary>
        /// Disabled state
        ///</summary>
        Disabled, // 3/25/2009
        ///<summary>
        /// User is hovering mouse over tile state
        ///</summary>
        Hovered,
        ///<summary>
        /// Tile is in countdown state
        ///</summary>
        Countdown,
        ///<summary>
        /// Tile has been paused during a countdown state
        ///</summary>
        Paused,
        ///<summary>
        /// Tile has finished countdown, and is now in the ready state
        ///</summary>
        Ready,
        ///<summary>
        /// User has selected the tile
        ///</summary>
        Selected,
        ///<summary>
        /// Tile is in Queued state, waiting for another tile to finish its countdown state
        ///</summary>
        Queued,
        ///<summary>
        /// Tile is waiting for some <see cref="SceneItem"/> instance to give clearance to proceed.
        /// For example, this is typical used with building units, and waiting for the building to
        /// open some type of door to let the unit out.
        ///</summary>
        WaitingForClearance,
        ///<summary>
        /// Tile is used with the <see cref="Player"/> instance <see cref="Player.Cash"/> credits, and
        /// this value is below some threshold value, setting it to this state.
        ///</summary>
        InsufficientFunds

    }
}
