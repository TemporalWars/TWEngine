using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TWTerrainToolsWPF.Enums
{
    // NOTE: (8/17/2010): List Enum list MUST match the 'ShadowMap' classes Enum list.
    ///<summary>
    ///The <see cref="ShadowType"/> Enum 
    ///</summary>
    public enum ShadowType
    {
        ///<summary>
        /// Draws the ShadowMap to its final target, using
        /// the 'Simple' shadowing technique.
        ///</summary>
        Simple = 0,
        ///<summary>
        /// Draws the ShadowMap to its final target, using
        /// the 'PCF' shadowing technique#1.
        ///</summary>
        // ReSharper disable InconsistentNaming
        PercentageCloseFilter_1 = 1,
        // ReSharper restore InconsistentNaming
        ///<summary>
        /// Draws the ShadowMap to its final target, using
        /// the 'PCF' shadowing technique#2.
        ///</summary>
        // ReSharper disable InconsistentNaming
        PercentageCloseFilter_2 = 2,
        // ReSharper restore InconsistentNaming
        ///<summary>
        /// Draws the ShadowMap to its final target, using
        /// the 'Variance' shadowing technique.
        ///</summary>
        Variance = 3
    }
}
