#region File Description
//-----------------------------------------------------------------------------
// PoolEventArgs.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;

namespace ImageNexus.BenScharbach.TWTools.MemoryPoolComponent
{
    // 3/23/2009 - 
    ///<summary>
    /// Custom <see cref="PoolEventArgs"/> for the Get/Return events
    ///</summary>
    public class PoolEventArgs : EventArgs
    {
        ///<summary>
        /// <see cref="PoolNode"/> collection index.
        ///</summary>
        public int NodeIndex;
    }
}