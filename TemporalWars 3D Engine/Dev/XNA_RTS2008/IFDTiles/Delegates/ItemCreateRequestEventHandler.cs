#region File Description
//-----------------------------------------------------------------------------
// ItemCreateRequestEventHandler.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.SceneItems;

namespace ImageNexus.BenScharbach.TWEngine.IFDTiles.Delegates
{
    ///<summary>
    /// Delegate used by the <see cref="IFDTilePlacement"/> item, to request the creation of some <see cref="SceneItem"/>.
    ///</summary>
    public delegate void ItemCreateRequestEventHandler(object sender, ItemCreatedArgs e);
}