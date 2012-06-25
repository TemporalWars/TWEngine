using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TWTerrainToolsWPF.Enums
{
    // NOTE: (8/17/2010): List Enum list MUST match the 'Water' classes Enum list.
    ///<summary>
    /// The <see cref="ViewPortTexture"/> Enum identifies RenderTarget texture to
    /// show on screen, for debug purposes.
    ///</summary>
    public enum ViewPortTexture
    {
        ///<summary>
        /// Displays refraction texture.
        ///</summary>
        Refraction,
        ///<summary>
        /// Displays reflection texture.
        ///</summary>
        Reflection,
        ///<summary>
        /// Displays the bump-map texture.
        ///</summary>
        Bump
    }
}
