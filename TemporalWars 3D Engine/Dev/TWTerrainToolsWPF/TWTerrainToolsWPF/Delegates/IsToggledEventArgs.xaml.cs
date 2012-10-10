using System;

namespace ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.Delegates
{
    /// <summary>
    /// Custom eventArgs for IsToggled event and delegate
    /// </summary>
    public class IsToggledEventArgs : EventArgs
    {
        // 7/9/2010
        /// <summary>
        /// Is button in toggled state.
        /// </summary>
        public bool IsToggled;
    }
}