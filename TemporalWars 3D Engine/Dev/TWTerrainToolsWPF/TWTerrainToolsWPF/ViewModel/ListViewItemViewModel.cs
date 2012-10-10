using System.ComponentModel;

namespace ImageNexus.BenScharbach.TWTools.TWTerrainToolsWPF.ViewModel
{
    /// <summary>
    /// Base class for all ViewModel classes displayed by TreeViewItems.  
    /// This acts as an adapter between a raw data object and a TreeViewItem.
    /// </summary>
    public class ListViewItemViewModel : INotifyPropertyChanged
    {
        bool _isSelected;

        #region Constructors

        /// <summary>
        /// constructor
        /// </summary>
        protected ListViewItemViewModel()
        {
            // empty
        }
        

        #endregion // Constructors

        #region Presentation Members
        
        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// </summary>
        public virtual bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }
        

        #endregion // Presentation Members

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // INotifyPropertyChanged Members
    }
}