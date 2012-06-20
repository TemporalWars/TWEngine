#region File Description
//-----------------------------------------------------------------------------
// SceneItemPlacedEventHandler.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using TWEngine.SceneItems;

namespace TWEngine.IFDTiles.Delegates
{
    // 10/19/2008 - Event Handler Delegate    
    ///<summary>
    /// <see cref="EventHandler"/> delegate for the <see cref="SceneItem"/> placed event.
    ///</summary>
    public delegate void SceneItemPlacedEventHandler(object sender, EventArgs e);
}