#region File Description
//-----------------------------------------------------------------------------
// OperationCompletedEventArgs.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;

namespace ImageNexus.BenScharbach.TWEngine.Networking
{
    /// <summary>
    /// Custom EventArgs class used by the <see cref="NetworkBusyScreen.OperationCompleted"/> event.
    /// </summary>
    class OperationCompletedEventArgs : EventArgs
    {
        #region Properties


        /// <summary>
        /// Gets or sets the <see cref="IAsyncResult"/> associated with
        /// the network operation that has just completed.
        /// </summary>
        public IAsyncResult AsyncResult { get; set; }

        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a new event arguments class.
        /// </summary>
        /// <param name="asyncResult"><see cref="IAsyncResult"/> interface instance</param>
        public OperationCompletedEventArgs(IAsyncResult asyncResult)
        {
            AsyncResult = asyncResult;
        }


        #endregion
    }
}
