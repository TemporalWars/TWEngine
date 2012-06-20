#region File Description
//-----------------------------------------------------------------------------
// GamerInfo.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TWEngine.Players;

namespace TWEngine.Networking.Structs
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.Networking.Structs"/> namespace contains the structures
    /// which make up the entire <see cref="Networking.Structs"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }


    // 4/8/2009: Add playerLoc.
    // 4/7/2009 - 
    ///<summary>
    /// The <see cref="GamerInfo"/> structure, stores the current gamers
    /// <see cref="Player"/> side selection, location, and color.
    ///</summary>
    public struct GamerInfo
    {
        ///<summary>
        /// Player's side; example: 1 or 2.
        ///</summary>
        private int _playerSide; // ex: 1 or 2.
        ///<summary>
        /// Player's location; example: 1 or 2.
        ///</summary>
        private int _playerLocation; // ex: Loc-1 or 2.
        ///<summary>
        /// Player's color, like red, blue, yellow, etc.
        ///</summary>
        public Color PlayerColor; // ex: red, blue, yellow, etc.
        ///<summary>
        /// Color string name, like 'Red'.
        ///</summary>
        public string ColorName;

        ///<summary>
        /// Player's side; example: 1 or 2.
        ///</summary>
        public int PlayerSide
        {
            get { return _playerSide; }
            set
            {
                _playerSide = value;
                PlayerSideString = value.ToString();
            }
        }

        ///<summary>
        /// Player's location; example: 1 or 2.
        ///</summary>
        public int PlayerLocation
        {
            get { return _playerLocation; }
            set
            {
                _playerLocation = value;
                PlayerLocationString = value.ToString();
            }
        }

        // 6/16/2010 - To reduce GC using strings, so number not 
        //             converted every draw cycle.
        ///<summary>
        /// Player's side in string format.
        ///</summary>
        public string PlayerSideString { get; private set; }

        // 6/16/2010 - To reduce GC using strings, so number not 
        //             converted every draw cycle.
        ///<summary>
        /// Player's location in string format.
        ///</summary>
        public string PlayerLocationString { get; private set; }
    }
}
