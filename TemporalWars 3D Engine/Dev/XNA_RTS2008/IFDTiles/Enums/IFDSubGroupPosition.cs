#region File Description
//-----------------------------------------------------------------------------
// IFDSubGroupPosition.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

namespace ImageNexus.BenScharbach.TWEngine.IFDTiles.Enums
{
    ///<summary>
    /// <see cref="IFDSubGroupPosition"/> Enum, used in the <see cref="IFDTileSubGroupControl"/> class, to
    /// specifiy the location for each sub-group of some <see cref="IFDGroupControlType"/> tile, created by 
    /// the user during game play.
    ///</summary>
    /// <remarks>Currently, only 5 positions specified in engine.</remarks>
    public enum IFDSubGroupPosition
    {
        ///<summary>
        /// Position 1;
        /// Original Tile placement start point, 
        /// with NO adjustment to X or Y.
        ///</summary>
        Pos1 = 1,
        ///<summary>
        /// Position 2;
        /// Original Tile placement start point,
        /// with +30 on X channel.
        ///</summary>
        Pos2 = 2,
        ///<summary>
        /// Position 3;
        /// Original Tile placement start point,
        /// with +60 on X channel.
        ///</summary>
        Pos3 = 3,
        ///<summary>
        /// Position 4;
        /// Original Tile placement start point,
        /// with +90 on X channel.
        ///</summary>
        Pos4 = 4,
        ///<summary>
        /// Position 5;
        /// Original Tile placement start point,
        /// with +120 on X channel.
        ///</summary>
        Pos5 = 5
    }
}
