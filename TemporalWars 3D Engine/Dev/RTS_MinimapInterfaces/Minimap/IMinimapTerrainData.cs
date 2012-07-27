namespace TWEngine
{
    ///<summary>
    /// The <see cref="IMinimapTerrainData"/> class is used to hold the important meta-data for the Terrain class; for example,
    /// the HeightData collection, VertexMultitextured_Stream1 collection, and the terrain Normals collection
    /// to name a few.
    ///</summary>
    public interface IMinimapTerrainData
    {
        /// <summary>
        /// Height of heightmap, multiplied by scale value.
        /// </summary>
        int MapWidthToScale { get; }
        /// <summary>
        /// Width of heightmap, multiplied by scale value.
        /// </summary>
        int MapHeightToScale { get; }
    }
}