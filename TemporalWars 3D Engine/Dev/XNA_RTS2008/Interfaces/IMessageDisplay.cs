#region File Description
//-----------------------------------------------------------------------------
// IMessageDisplay.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.Interfaces
{
    /// <summary>
    /// Interface used to display notification messages when interesting events occur,
    /// for instance when gamers join or leave the network session. This interface
    /// is registered as a service, so any piece of code wanting to display a message
    /// can look it up from Game.Services, without needing to worry about how the
    /// message display is implemented. In this sample, the MessageDisplayComponent
    /// class implement this IMessageDisplay service.
    /// </summary>
    interface IMessageDisplay : IDrawable, IUpdateable
    {
        /// <summary>
        /// Shows a new notification message.
        /// </summary>
        /// <param name="message">message to display</param>
        /// <param name="parameters">replaces the format item in a specified string with the text equivalent of the value
        /// of a corresponding object instance in a specified array. A specified parameter supplies culture-specific 
        /// formatting information.</param>       
        void ShowMessage(string message, params object[] parameters);
    }
}