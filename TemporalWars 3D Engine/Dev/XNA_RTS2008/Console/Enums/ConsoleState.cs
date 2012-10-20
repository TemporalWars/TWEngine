#region File Description
//-----------------------------------------------------------------------------
// ConsoleState.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace ImageNexus.BenScharbach.TWEngine.Console.Enums
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.Console.Enums"/> namespace contains the common enumerations
    /// which make up the entire <see cref="TWEngine.Console"/>.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    // 10/16/2012: Updated enum to inherit from short value.
    ///<summary>
    /// Enumeration of the ConsoleState; for example, 'Open' 
    /// or 'Closed'.
    ///</summary>
    public enum ConsoleState : short
    {
        ///<summary>
        /// Console is closed.
        ///</summary>
        Closed,
        ///<summary>
        /// Console is in process of closing.
        ///</summary>
        Closing,
        ///<summary>
        /// Console is open
        ///</summary>
        Open,
        ///<summary>
        /// Console is in process of opening.
        ///</summary>
        Opening
    }
}