#region File Description
//-----------------------------------------------------------------------------
// AvailableSessionMenuEntry.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.GameScreens.Generic;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;

namespace ImageNexus.BenScharbach.TWEngine.Networking
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.Networking"/> namespace contains the classes
    /// which make up the entire <see cref="TWEngine.Networking"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
    
    
    /// <summary>
    /// Helper class customizes the standard <see cref="MenuEntry"/> class
    /// for displaying <see cref="AvailableSessionMenuEntry"/> objects.
    /// </summary>
    class AvailableSessionMenuEntry : MenuEntry
    {
        #region Fields       
        bool _gotQualityOfService;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the available network session corresponding to this <see cref="MenuEntry"/>.
        /// </summary>
        public AvailableNetworkSession AvailableSession { get; private set; }


        #endregion

        #region Initialization


        /// <summary>
        /// Constructs a <see cref="MenuEntry"/> describing an available network session.
        /// </summary>
        /// <param name="availableSession"><see cref="AvailableNetworkSession"/> instance</param>
        public AvailableSessionMenuEntry(AvailableNetworkSession availableSession)
            : base(GetMenuItemText(availableSession), new Rectangle(0, 60, 600, 40), new Vector2(10, 0))
        {
            AvailableSession = availableSession;
        }


        /// <summary>
        /// Formats session information to create the menu text string.
        /// </summary>
        static string GetMenuItemText(AvailableNetworkSession session)
        {
            var totalSlots = session.CurrentGamerCount +
                             session.OpenPublicGamerSlots;

// ReSharper disable RedundantToStringCall
            return string.Format("{0} ({1}/{2})", session.HostGamertag.ToString(),
                                                  session.CurrentGamerCount.ToString(),
                                                  totalSlots.ToString()); // 8/25/2009: Updated to use ToString, otherwise boxing occurs.
// ReSharper restore RedundantToStringCall
        }


        #endregion

        #region Update

        /// <summary>
        /// Updates the menu <see cref="SceneItem"/> owner text, adding information about the network
        /// quality of service as soon as that becomes available.
        /// </summary>
        /// <param name="screen"><see cref="MenuScreen"/> instance</param>
        /// <param name="isSelected">Is item selected?</param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public sealed override void Update(MenuScreen screen, bool isSelected,
                                                       GameTime gameTime)
        {
            base.Update(screen, isSelected, gameTime);

            // Quality of service data can take some Time to query, so it will not
            // be filled in straight away when NetworkSession.Find returns. We want
            // to display the list of available sessions straight away, and then
            // fill in the quality of service data whenever that becomes available,
            // so we keep checking until this data shows up.
            if (!screen.IsActive || _gotQualityOfService) return;

            var qualityOfService = AvailableSession.QualityOfService;

            if (!qualityOfService.IsAvailable) return;

            var pingTime = qualityOfService.AverageRoundtripTime;

// ReSharper disable RedundantToStringCall
            Text += string.Format(" - {0} ms", pingTime.TotalMilliseconds.ToString()); // 8/25/2009: Updated to use ToString, otherwise boxing occurs.
// ReSharper restore RedundantToStringCall

            _gotQualityOfService = true;
        }


        #endregion
    }
}
