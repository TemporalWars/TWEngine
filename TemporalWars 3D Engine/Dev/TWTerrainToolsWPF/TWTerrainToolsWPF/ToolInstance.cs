using TWTerrainToolsWPF.Interfaces;

namespace TWTerrainToolsWPF
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
