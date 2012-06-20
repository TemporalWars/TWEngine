#region File Description
//-----------------------------------------------------------------------------
// FogOfWarItem.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;

namespace TWEngine.Common
{
    ///<summary>
    /// Fog-of-war Item, inherting from the 'IFogOfWarShapeItem' 
    /// interface, which extends a SceneItem with the FOW ability.
    ///</summary>
    public struct FogOfWarItem : IFogOfWarShapeItem
    {

        private Rectangle _fogOfWarDestination;

        #region Properties
       
        /// <summary>
        /// Fog-Of-War Dest Rectangle
        /// </summary>
        public Rectangle FogOfWarDestination
        {
            get
            {
                return _fogOfWarDestination;
            }
            set
            {
                _fogOfWarDestination = value;
            }
        }

        /// <summary>
        /// isFOWVisible flag.
        /// </summary>
        public bool IsFOWVisible { get; set; }

        /// <summary>
        /// Use Fog-Of-War for specific SceneItemOwner?
        /// </summary>
        public bool UseFogOfWar { get; set; }      
        

        ///<summary>
        /// Fog-of-war rectangle width.
        ///</summary>
        public int FogOfWarWidth
        {
            get { return _fogOfWarDestination.Width; }
            set { _fogOfWarDestination.Width = value; }
        }

        /// <summary>
        /// Fog-of-war rectangle height.
        /// </summary>
        public int FogOfWarHeight
        {
            get { return _fogOfWarDestination.Height; }
            set { _fogOfWarDestination.Height = value; }
        }

        #endregion
       
    }
}