namespace ImageNexus.BenScharbach.TWEngine.TerrainTools.Enums
{
    // 10/16/2012: Updated enum to inherit from short value.
    ///<summary>
    /// The <see cref="PaintTool"/> Enum is used to paint or erase textures on the <see cref="Terrain"/>.
    ///</summary>
    public enum PaintTool : short
    {
        ///<summary>
        /// Set to select a paint texture.
        ///</summary>
        Select,
        ///<summary>
        /// Set to 'Fill' (paint) an area of the <see cref="Terrain"/> with
        /// the choosen paint texture.
        ///</summary>
        Fill,
        ///<summary>
        /// Set to 'UnFill' (erase) an area of the <see cref="Terrain"/> texture.
        ///</summary>
        Unfill,
    }
}