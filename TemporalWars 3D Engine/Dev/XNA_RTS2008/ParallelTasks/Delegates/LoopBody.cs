#region File Description
//-----------------------------------------------------------------------------
// LoopBody.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System.Collections.Generic;

namespace ImageNexus.BenScharbach.TWEngine.ParallelTasks.Delegates
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.ParallelTasks.Delegates"/> namespace contains the classes
    /// which make up the entire <see cref="TWEngine.TWEngine.ParallelTasks.Delegatesnent.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
    
    
    /// <summary>
    /// Core method for the <see cref="LoopBody{T}"/> of the 'For-Loop'.  Inheriting classes
    /// MUST override and provide the core <see cref="LoopBody{T}"/> to the 'For-Loop' logic.
    /// </summary>
    /// <typeparam name="T">Generic type contained in collection.</typeparam>
    /// <param name="array">Collection of <typeparamref name="T"/>.</param>
    /// <param name="index">Index of the current iteration of the collection.</param>
    public delegate void LoopBody<T>(IList<T> array, int index);
}