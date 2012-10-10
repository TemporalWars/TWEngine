#region File Description
//-----------------------------------------------------------------------------
// CameraBoundItemStruct.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;

namespace ImageNexus.BenScharbach.TWEngine.ParallelTasks.Structs
{
    // 2/28/2011
    ///<summary>
    /// Camera bound structure used in the <see cref="CameraBoundThreadManager{TAParam}"/>.
    ///</summary>
    public struct CameraBoundItemStruct<TAParam>
    {
        ///<summary>
        /// Delegate <see cref="Action{TAParam}"/> method to run in thread.
        ///</summary>
        public Action<TAParam> ActionMethod;

        ///<summary>
        /// Generic <typeparam name="TAParam" /> to pass into method.
        ///</summary>
        public TAParam MethodParam;
    }
}