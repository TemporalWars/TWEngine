#region File Description
//-----------------------------------------------------------------------------
// NetworkErrorScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.GameScreens.Generic;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net;

namespace ImageNexus.BenScharbach.TWEngine.Networking
{
    /// <summary>
    /// Specialized <see cref="MessageBoxScreen"/> subclass, used to display network error messages.
    /// </summary>
    internal sealed class NetworkErrorScreen : MessageBoxScreen
    {

        /// <summary>
        /// Constructs a <see cref="NetworkErrorScreen"/> box from the specified exception.
        /// </summary>
        /// <param name="exception"><see cref="Exception"/> instance</param>
        public NetworkErrorScreen(Exception exception)
            : base(GetErrorMessage(exception), false)
        { }


        /// <summary>
        /// Converts a network <see cref="Exception"/> into a user friendly error message.
        /// </summary>
        /// <param name="exception"><see cref="Exception"/> instance</param>
        static string GetErrorMessage(Exception exception)
        {
            /*Console.WriteLine(string.Format("Network operation threw {0}: {1}",
                                          exception, exception.Message));*/

            // Is this a GamerPrivilegeException?
            if (exception is GamerPrivilegeException)
            {
                return Resources.ErrorGamerPrivilege;
            }

            // Is it a NetworkSessionJoinException?
            var joinException = exception as
                                NetworkSessionJoinException;

            if (joinException != null)
            {
                switch (joinException.JoinError)
                {
                    case NetworkSessionJoinError.SessionFull:
                        return Resources.ErrorSessionFull;

                    case NetworkSessionJoinError.SessionNotFound:
                        return Resources.ErrorSessionNotFound;

                    case NetworkSessionJoinError.SessionNotJoinable:
                        return Resources.ErrorSessionNotJoinable;
                }
            }

            // Otherwise just a generic error message.
            return Resources.ErrorNetwork;
        }


      
    }
}
