#region File Description
//-----------------------------------------------------------------------------
// PoolMemoryEventHandler.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

namespace ImageNexus.BenScharbach.TWTools.MemoryPoolComponent.Delegates
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="MemoryPoolComponent.Delegates"/> namespace contains the classes
    /// which make up the entire <see cref="MemoryPoolComponent.Delegates"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    ///<summary>
    /// Delegate <see cref="PoolMemoryEventHandler"/>, used for the <see cref="Pool{TDefault}"/> classes
    /// events <see cref="Pool{TDefault}.PoolItemGetCalled"/> and  <see cref="Pool{TDefault}.PoolItemReturnCalled"/>.
    ///</summary>
    public delegate void PoolMemoryEventHandler(object sender, PoolEventArgs e);
}