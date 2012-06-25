using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TWTerrainToolsWPF.DataModel;
using TWTerrainToolsWPF.DataModel.PaintToolDataModel;
using TWTerrainToolsWPF.Delegates;

namespace TWTerrainToolsWPF.ViewModel.PaintToolViewModel
{
    /// <summary>
    /// The ViewModel for the <see cref="TexturesViewModel"/>.
    /// </summary>
    public class TexturesViewModel
    {
        // collections
        private readonly ObservableCollection<TextureItemViewModel> _textures = new ObservableCollection<TextureItemViewModel>();

        // assetsImagePath class
        private readonly AssetsImagePaths _assetsImagePaths;

        // 7/7/2010 - Event
        /// <summary>
        /// Occurs when the current item 'IsSelected' in the ListView control.
        /// </summary>
        public event SelectedItemEventHandler SelectedItemEvent;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assetsImagePaths">Reference to the Icon paths dictionary</param>
        public TexturesViewModel(AssetsImagePaths assetsImagePaths)
        {
            // set
            _assetsImagePaths = assetsImagePaths;
           
        }

        // 7/7/2010
        /// <summary>
        /// Gets the current 'IsSelected' <see cref="TextureItem"/>, 
        /// while returning the index item was found at; -1 if no results.
        /// </summary>
        /// <param name="textureItem">(OUT) <see cref="TextureItem"/> instance</param>
        /// <returns>Index of item in collection</returns>
        public int GetSelectedItem(out TextureItem textureItem)
        {
            textureItem = null;
            var indexFoundAt = -1;

            // search for 'IsSelected' item and return to caller
            var count = _textures.Count;
            for (var i = 0; i < count; i++)
            {
                // retrieve current item
                var item = _textures[i];
                if (item == null) continue;

                // check if correct item
                if (!item.IsSelected) continue;

                // found item
                textureItem = item.TextureItem;
                indexFoundAt = i;
            }

            return indexFoundAt;
        }

        // 7/7/2010
        /// <summary>
        /// Adds a new <see cref="TextureItemViewModel"/> to the internal ROC collection at the
        /// given index, using the given <see cref="TextureItem"/> instance.
        /// </summary>
        /// <param name="textureItem"><see cref="TextureItem"/> instance to add</param>
        /// <param name="insertAt">Index location to insert this record</param>
        public void AddTextureItem(TextureItem textureItem, int insertAt)
        {
            // check if null
            if (textureItem == null)
                throw new ArgumentNullException("textureItem");

            // create TextureItemViewModel item, and add to list
            var newTextureItemViewModel = new TextureItemViewModel(textureItem, _assetsImagePaths);

            // 7/8/2010 - Capture when PropertyChanges.
            newTextureItemViewModel.PropertyChanged += newTextureItemViewModel_PropertyChanged;
            
            // add to list
            _textures.Insert(insertAt, newTextureItemViewModel);

        }

        // 7/8/2010
        /// <summary>
        /// Occurs when propertyChanged event triggers, which is then checked for the specific 'IsSelected' property.  If property is the 
        /// 'IsSelected', then <see cref="SelectedItemEvent"/> is triggered.
        /// </summary>
// ReSharper disable InconsistentNaming
        void newTextureItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
// ReSharper restore InconsistentNaming
        {
            // Check if 'IsSelected' property updated.
            if (e.PropertyName != "IsSelected" || SelectedItemEvent == null) return;

            // cast sender
            var textureItemViewModel = sender as TextureItemViewModel;

            // trigger event
            SelectedItemEvent(this,
                              new SelectedItemEventArgs
                                  {
                                      ImageName =
                                          (textureItemViewModel == null) ? "" : textureItemViewModel.TextureName
                                  });
        }

        /// <summary>
        /// Removes a <see cref="TextureItem"/> from the internal ROC collection at the given index.
        /// </summary>
        /// <param name="removeAt">Index location to remove record</param>
        public void RemoveTextureItem(int removeAt)
        {
            try
            {
                // just return if index not within valid range.
                if (removeAt >= _textures.Count) return;

                _textures.RemoveAt(removeAt);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("RemoveTextureItem method threw exception " + ex.Message ?? "No Message");
#endif
            }
        }

        /// <summary>
        /// Returns list of <see cref="TextureItemViewModel"/> <see cref="ObservableCollection{T}"/>.
        /// </summary>
        public ObservableCollection<TextureItemViewModel> Textures
        {
            get { return _textures; }
        }
    }
}