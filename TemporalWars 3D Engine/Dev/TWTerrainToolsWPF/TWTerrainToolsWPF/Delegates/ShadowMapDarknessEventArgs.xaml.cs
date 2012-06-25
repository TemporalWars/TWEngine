using System;

namespace TWTerrainToolsWPF.Delegates
{
    /// <summary>
    /// Custom eventArgs for ShadowMapDarkness event and delegate
    /// </summary>
    public class ShadowMapDarknessEventArgs : EventArgs
    {
        // 8/17/2010
        /// <summary>
        /// New value for ShadowMapDarkness
        /// </summary>
        public double ShadowMapDarknessValue;
    }
}