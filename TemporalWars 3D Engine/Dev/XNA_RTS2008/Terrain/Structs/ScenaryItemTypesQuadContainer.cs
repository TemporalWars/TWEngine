using System.Collections.Generic;
using ImageNexus.BenScharbach.TWEngine.InstancedModels;

namespace ImageNexus.BenScharbach.TWEngine.Terrain.Structs
{
    // 10/15/2012
    /// <summary>
    /// Container which holds the collection of ScenaryItem instances 
    /// which should be drawn for the given <see cref="TerrainQuadTree"/>.
    /// </summary>
    /// <remarks>
    /// This container was created to hold the simple array copy of the dictionary collection.  This eliminates 
    /// the need to copy to a simple array for every iteration in the <see cref="InstancedItem.CreateSceneryInstancesCulledList"/>.
    /// </remarks>
    public struct ScenaryItemTypesQuadContainer
    {
        public Dictionary<int, int> ItemKeys;
        public int[] ItemKeysArray;
    }
}