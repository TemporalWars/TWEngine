#region File Description
//-----------------------------------------------------------------------------
// Settings.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.IO;
using System.Xml.Serialization;
#endregion

namespace TWEngine
{
    /// <summary>
    /// The Setting class handles loading and saving of global application Settings.
    /// The normal .Net classes (System.Configuration) for doing this are not available on the CF (and therefore 360)
    /// </summary>
    public class Settings
    {
        #region General App Settings

        /// <summary>
        /// The path to look for all media in
        /// </summary>
        public string MediaPath = @"content\";

        /// <summary>
        /// The name of the window when running in windowed mode
        /// </summary>
        public string WindowTitle = "TemporalWars";         

        #endregion             
       
       
    }
}