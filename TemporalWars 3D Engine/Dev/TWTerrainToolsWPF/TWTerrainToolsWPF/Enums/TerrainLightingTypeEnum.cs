namespace ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.Enums
{
    /// <summary>
    /// The <see cref="TerrainLightingType"/> Enum Controls the material LightingType to use during rendering, 
    /// specifically for the Terrain.
    /// </summary>
    public enum TerrainLightingType
    {
        ///<summary>
        /// Applies the plastic material lighting type.
        ///</summary>
        Plastic = 0,
        ///<summary>
        /// Applies the metal material lighting type.
        ///</summary>
        Metal = 1,
        ///<summary>
        /// Applies the blinn material lighting type.
        ///</summary>
        Blinn = 2,
        ///<summary>
        /// Applies the glossy material lighting type.
        ///</summary>
        Glossy = 3
    }
}
