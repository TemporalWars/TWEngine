namespace ImageNexus.BenScharbach.TWEngine.TerrainTools.Enums
{
    ///<summary>
    /// The <see cref="HeightTool"/> Enum identifies what action to use
    /// when deforming the <see cref="Terrain"/>.
    ///</summary>
    public enum HeightTool
    {
        ///<summary>
        /// Tool is in select mode.
        ///</summary>
        Select,
        ///<summary>
        /// Set to 'Raise' the <see cref="Terrain"/> vertices.
        ///</summary>
        Raise,
        ///<summary>
        /// Set to 'Lower' the <see cref="Terrain"/> vertices.
        ///</summary>
        Lower,
        ///<summary>
        /// Set to 'Smooth' the <see cref="Terrain"/> vertices.
        ///</summary>
        Smooth,
        ///<summary>
        /// Set to 'Flatten' the <see cref="Terrain"/> vertices.
        ///</summary>
        Flatten
    }
}