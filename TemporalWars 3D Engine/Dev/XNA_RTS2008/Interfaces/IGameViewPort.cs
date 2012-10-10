#region File Description
//-----------------------------------------------------------------------------
// IGameViewPort.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.Viewports;

namespace ImageNexus.BenScharbach.TWEngine.Interfaces
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    interface IGameViewPort
    {
        /// <summary>
        /// Returns a reference to <see cref="GameViewPort"/> 
        /// </summary>
        GameViewPort GVP { get; }
        ///<summary>
        /// Set to turn On/Off rendering of all <see cref="GameViewPort"/> instances.
        ///</summary>
        bool IsVisible { get; set; }
        ///<summary>
        /// Set to turn On/Off rendering of all <see cref="GameViewPort"/> instances.
        ///</summary>
        bool V { get; set; }
    }
}