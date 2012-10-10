using ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.Interfaces;

namespace ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF
{
    // 2/4/2011
    internal class ToolInstance
    {
        // 2/4/2011
        /// <summary>
        /// Stores references to <see cref="IOnGuiThread"/>.
        /// </summary>
        internal static IOnGuiThread OnGuiThread { get; set; }
    }
}
