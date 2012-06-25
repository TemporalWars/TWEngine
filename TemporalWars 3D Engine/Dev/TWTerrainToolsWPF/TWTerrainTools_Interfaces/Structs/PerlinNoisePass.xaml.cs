using System.Runtime.Serialization;

namespace TWTerrainTools_Interfaces.Structs
{
    // 6/30/2010
    /// <summary>
    /// Stores the current Perlin-Noise attributes
    /// settings, used to communicate to outside callers.
    /// </summary>
    public struct PerlinNoisePass
    {
        
        public int RandomSeed;

        
        public float PerlinNoiseSize;

        
        public float PerlinPersistence;

        
        public int PerlinOctaves;
    }
}