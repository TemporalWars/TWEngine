namespace ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.DataModel.PaintToolDataModel
{
    // 7/2/2010
    /// <summary>
    /// The <see cref="TextureItem"/> class holds a single instance for a given texture.
    /// </summary>
    public class TextureItem
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="textureName">Name of texture</param>
        public TextureItem(string textureName)
        {
            TextureName = textureName;

        }

        /// <summary>
        /// Gets the current directory name.
        /// </summary>
        public string TextureName { get; private set; }
       
    }
}