#region File Description
//-----------------------------------------------------------------------------
// IGameConsole.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
#if !XBOX360
using ImageNexus.BenScharbach.TWEngine.Console.Enums;

#endif

namespace ImageNexus.BenScharbach.TWEngine.Interfaces
{
    /// <summary>
    /// Game console class creates an instance of the Python console internally,
    /// allowing reference to the interal classes via the console scripting window.
    /// </summary>
    public interface IGameConsole
    {
#if !XBOX360
        /// <summary>
        /// Returns the State of the Python Console
        /// </summary>
        ConsoleState ConsoleState { get; }
#endif
        ///<summary>
        ///Disposes of older Interfaced References.
        ///</summary>
        void DisposeInterfaceReferences();
    }
}