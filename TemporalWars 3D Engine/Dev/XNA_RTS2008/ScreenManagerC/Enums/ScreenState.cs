#region File Description
//-----------------------------------------------------------------------------
// ScreenState.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace TWEngine.ScreenManagerC.Enums
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.ScreenManagerC.Enums"/> namespace contains the enumerations
    /// which make up the entire <see cref="ScreenManagerC.Enums"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
    
    /// <summary>
    /// The <see cref="ScreenState"/> Enum describes the screen transition state.
    /// </summary>
    public enum ScreenState
    {
        ///<summary>
        /// The <see cref="GameScreen"/> is starting.
        ///</summary>
        TransitionOn,
        ///<summary>
        /// The <see cref="GameScreen"/> is active.
        ///</summary>
        Active,
        ///<summary>
        /// The <see cref="GameScreen"/> is closing.
        ///</summary>
        TransitionOff,
        ///<summary>
        /// The <see cref="GameScreen"/> is hidden by some other screen.
        ///</summary>
        Hidden,
    }
}
