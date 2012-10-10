namespace ImageNexus.BenScharbach.TWLate.RTS_FogOfWarInterfaces.FOW
{
    ///<summary>
    /// The <see cref="IFOWTerrainData"/> class is used to hold the important meta-data for the <see cref="IFOWTerrainShape"/> class; for example,
    /// the HeightData collection, VertexMultitextured_Stream1 collection, and the terrain Normals collection to name a few.
    ///</summary>
    public interface IFOWTerrainData
    {

        /// <summary>
        /// The spacing between the individual triangles when creating the <see cref="IFOWTerrainShape"/> mesh.
        /// </summary>
        int Scale { get; }

        /// <summary>
        /// Checks if given X/Y cordinates are on the heightmap.
        /// </summary>
        /// <param name="xPos">X value</param>
        /// <param name="yPos">Y value</param>
        /// <returns>True/False as result</returns>
        bool IsOnHeightmap(float xPos, float yPos);
        
        /// <summary>
        /// Width of heightmap multiplied by scale value.
        /// </summary>
        int MapWidthToScale { get; }

        /// <summary>
        ///  Height of heightmap multiplied by scale value.
        /// </summary>
        int MapHeightToScale { get; }
    }
}