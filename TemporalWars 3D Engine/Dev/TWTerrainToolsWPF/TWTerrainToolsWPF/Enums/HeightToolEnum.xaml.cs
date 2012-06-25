namespace TWTerrainToolsWPF.Enums
{
    ///<summary>
    /// The <see cref="HeightTool"/> Enum identifies what action to use
    /// when deforming the Terrain.
    ///</summary>
    public enum HeightTool
    {
        ///<summary>
        /// Tool is in select mode.
        ///</summary>
        Select,
        ///<summary>
        /// Set to 'Raise' the Terrain vertices.
        ///</summary>
        Raise,
        ///<summary>
        /// Set to 'Lower' the Terrain vertices.
        ///</summary>
        Lower,
        ///<summary>
        /// Set to 'Smooth' the Terrain vertices.
        ///</summary>
        Smooth,
        ///<summary>
        /// Set to 'Flatten' the Terrain vertices.
        ///</summary>
        Flatten
    }
}