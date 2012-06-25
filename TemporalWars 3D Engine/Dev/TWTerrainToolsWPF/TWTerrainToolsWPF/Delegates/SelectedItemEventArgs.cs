using System;

namespace TWTerrainToolsWPF.Delegates
{
    /// <summary>
    /// This <see cref="SelectedItemEventArgs"/> class is used to pass important
    /// attributes to the event handler.
    /// </summary>
    public class SelectedItemEventArgs : EventArgs
    {
        /// <summary>
        /// Image name of image to display.
        /// </summary>
        public string ImageName;
    }
}