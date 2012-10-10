#region File Description
//-----------------------------------------------------------------------------
// LobbyScreenData.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace ImageNexus.BenScharbach.TWEngine.Networking.Structs
{
    // 2/21/2009
    ///<summary>
    /// The <see cref="LobbyScreenData"/> structure stores user choices
    /// to be sent to other network lobby users.  Currently, only the
    /// <see name="mapName"/> is stored.
    ///</summary>
    public struct LobbyScreenData
    {
        ///<summary>
        /// What map is going to be played on.
        ///</summary>
// ReSharper disable InconsistentNaming
        public string mapName;
// ReSharper restore InconsistentNaming

    }
}
