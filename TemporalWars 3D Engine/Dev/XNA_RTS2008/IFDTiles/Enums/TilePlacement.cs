#region File Description
//-----------------------------------------------------------------------------
// TilePlacement.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace TWEngine.IFDTiles.Enums
{
    // 9/23/2008 - Common Tile Placement Enum
    ///<summary>
    /// <see cref="TilePlacement"/> Enum, which specifies the default
    /// positions for each of the 12 locations a tile could be placed.
    ///</summary>
    public enum TilePlacement
    {
        ///<summary>
        /// Position 1;
        /// Original Tile placement start point, 
        /// with NO adjustment to X or Y.
        ///</summary>
        Pos1,
        ///<summary>
        /// Position 2;
        /// Original Tile placement start point, 
        /// with +75 on X channel.
        ///</summary>
        Pos2,
        ///<summary>
        /// Position 3;
        /// Original Tile placement start point, 
        /// with +150 on X channel.
        ///</summary>
        Pos3,
        ///<summary>
        /// Position 4;
        /// Original Tile placement start point, 
        /// with +0 on X channel, +75 on Y channel.
        ///</summary>
        Pos4,
        ///<summary>
        /// Position 5;
        /// Original Tile placement start point, 
        /// with +75 on X channel, +75 on Y channel.
        ///</summary>
        Pos5,
        ///<summary>
        /// Position 6;
        /// Original Tile placement start point, 
        /// with +150 on X channel, +75 on Y channel.
        ///</summary>
        Pos6,
        ///<summary>
        /// Position 7;
        /// Original Tile placement start point, 
        /// with +0 on X channel, +150 on Y channel.
        ///</summary>
        Pos7,
        ///<summary>
        /// Position 8;
        /// Original Tile placement start point, 
        /// with +75 on X channel, +150 on Y channel.
        ///</summary>
        Pos8,
        ///<summary>
        /// Position 9;
        /// Original Tile placement start point, 
        /// with +150 on X channel, +150 on Y channel.
        ///</summary>
        Pos9,
        ///<summary>
        /// Position 10;
        /// Original Tile placement start point, 
        /// with +0 on X channel, +225 on Y channel.
        ///</summary>
        Pos10,
        ///<summary>
        /// Position 11;
        /// Original Tile placement start point, 
        /// with +75 on X channel, +225 on Y channel.
        ///</summary>
        Pos11,
        ///<summary>
        /// Position 12;
        /// Original Tile placement start point, 
        /// with +150 on X channel, +225 on Y channel.
        ///</summary>
        Pos12
    }
}
