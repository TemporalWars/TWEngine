#region File Description
//-----------------------------------------------------------------------------
// ItemMoveState.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.SceneItems.Structs
{
    // 12/16/2008 - Add SmoothHeading
    // 9/8/2008 - Interpolation State Struct for Network Games    
    ///<summary>
    /// The <see cref="ItemMoveState"/> structure holds the
    /// interpolation state data for network games.
    ///</summary>
    [Obsolete]
    public struct ItemMoveState
    {
        ///<summary>
        /// Current position
        ///</summary>
        public Vector3 Position;
        ///<summary>
        /// Current Velocity
        ///</summary>
        public Vector3 Velocity;
        ///<summary>
        /// Current smooth heading
        ///</summary>
        public Vector3 SmoothHeading; // 12/16/2008 
        ///<summary>
        /// Current facing direction
        ///</summary>
        public float FacingDirection; // 12/22/2008

    }
}
