using TWEngine.SceneItems;

namespace TWEngine.TerrainTools.Enums
{
    ///<summary>
    /// The <see cref="ItemsTool"/> Enum is used when 
    /// placing <see cref="SceneItem"/> on the <see cref="Terrain"/>.
    ///</summary>
    public enum ItemsTool
    {
        ///<summary>
        /// Set to 'Select' some <see cref="SceneItem"/>.
        ///</summary>
        Select,
        ///<summary>
        /// Set to do the Perlin-Noise 'Fill' operation; used to
        /// batch place multiply <see cref="SceneItem"/>.
        ///</summary>
        Fill,
        ///<summary>
        /// Set to do the Perlin-Noise 'UnFill' operation; used to
        /// batch remove multiply <see cref="SceneItem"/>.
        ///</summary>
        Unfill,
    }
}