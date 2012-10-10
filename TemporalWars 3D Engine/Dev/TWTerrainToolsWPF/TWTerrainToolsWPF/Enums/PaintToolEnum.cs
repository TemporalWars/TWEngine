namespace ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.Enums
{
    ///<summary>
    /// The <see cref="PaintTool"/> Enum is used to paint or erase textures on the Terrain.
    ///</summary>
    public enum PaintTool
    {
        ///<summary>
        /// Set to select a paint texture.
        ///</summary>
        Select,
        ///<summary>
        /// Set to 'Fill' (paint) an area of the Terrain with
        /// the chosen paint texture.
        ///</summary>
        Fill,
        ///<summary>
        /// Set to 'UnFill' (erase) an area of the Terrain texture.
        ///</summary>
        Unfill,
    }
}
