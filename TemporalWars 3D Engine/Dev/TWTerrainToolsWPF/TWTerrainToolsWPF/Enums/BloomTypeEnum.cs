using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TWTerrainToolsWPF.Enums
{
    ///<summary>
    /// The <see cref="BloomType"/> Enum allows setting the type
    /// of Bloom effect.
    ///</summary>
    public enum BloomType
    {
        ///<summary>
        /// Standard Bloom effect.
        ///</summary>
        Default = 0,
        ///<summary>
        /// Soft Bloom effect.
        ///</summary>
        Soft = 1,
        ///<summary>
        /// Desaturated Bloom effect.
        ///</summary>
        DeSaturated = 2,
        ///<summary>
        /// Saturated Bloom effect.
        ///</summary>
        Saturated = 3,
        ///<summary>
        /// Blurry Bloom effect.
        ///</summary>
        Blurry = 4,
        ///<summary>
        /// Subtle Bloom effect.
        ///</summary>
        Subtle = 5
    }
}
