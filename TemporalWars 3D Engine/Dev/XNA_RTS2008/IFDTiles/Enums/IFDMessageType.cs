#region File Description
//-----------------------------------------------------------------------------
// IFDMessageType.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace TWEngine.IFDTiles.Enums
{
    ///<summary>
    /// <see cref="IFDTileMessage"/> Enum for type of message
    /// to draw to the screen. 
    ///</summary>
    public enum IFDMessageType
    {
        ///<summary>
        /// Draws the simple 1 line message type to screen
        ///</summary>
        SimpleOneLine,
        ///<summary>
        /// Draws the multi-message timed type to screen. (Scripting Purposes)
        ///</summary>
        MultiTimedMessages, // 1/15/2010
        ///<summary>
        /// Draws the blocked message decription type to screen.
        ///</summary>
        /// <remarks>Primarely used to display unit attibutes when user clicks on the <see cref="InterfaceDisplay"/> tiles.</remarks>
        ItemTagDescription
    }
}
