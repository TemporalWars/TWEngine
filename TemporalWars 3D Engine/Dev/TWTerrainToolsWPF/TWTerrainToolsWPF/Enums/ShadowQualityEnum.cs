using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TWTerrainToolsWPF.Enums
{
    // NOTE: (8/17/2010): List Enum list MUST match the 'ShadowMap' classes Enum list.
    /// <summary>
    /// The <see cref="ShadowQuality"/> Enum controls the level
    /// of detail used for shadows; specifically, the size of the
    /// RenderTarget used when creating the shadow maps.
    /// </summary>
    public enum ShadowQuality
    {
        ///<summary>
        /// Low = 1024x for RenderTarget
        ///</summary>
        Low,
        ///<summary>
        /// Med = 2048x for RenderTarget
        ///</summary>
        Medium,
        ///<summary>
        /// High = 4096x for RenderTarget
        ///</summary>
        High
    }
}
