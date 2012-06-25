using System.Windows.Media.Imaging;
using TWTerrainToolsWPF.DataModel;
using TWTerrainToolsWPF.DataModel.PaintToolDataModel;
using TWTerrainToolsWPF.Delegates;

namespace TWTerrainToolsWPF.ViewModel.PaintToolViewModel
{
    // 7/7/2010
    public class TextureItemViewModel : ListViewItemViewModel
    {
        private readonly TextureItem _textureItem;

        // 7/6/2010 - Ref to the AssetsImagePaths.
        private readonly AssetsImagePaths _assetImagePaths;

        #region Properties

        // 7/7/2010
        /// <summary>
        /// Returns an instance of the <see cref="TextureItem"/>
        /// </summary>
        public TextureItem TextureItem
        {
            get { return _textureItem; }
        }
        

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="textureItem">Instance of <see cref="TextureItem"/></param>
        /// <param name="assetImagePaths">Reference to the Icon paths dictionary</param>
        public TextureItemViewModel(TextureItem textureItem, AssetsImagePaths assetImagePaths)
        {
            _textureItem = textureItem;

            // 7/6/2010 - Set reference to Icons path dictionary
            _assetImagePaths = assetImagePaths;
        }

        /// <summary>
        /// Gets the current texture name.
        /// </summary>
        public string TextureName
        {
            get { return TextureItem.TextureName; }
        }

        /// <summary>
        /// Gets the current image for this texture.
        /// </summary>
        public BitmapSource TextureImage
        {
            get
            {
                BitmapSource bitmapSource;
                var keyName = TextureItem.TextureName;
                _assetImagePaths.GetAssetImage(keyName, out bitmapSource);
                return bitmapSource;
            }
        }

       
    }
}