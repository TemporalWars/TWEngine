using ImageNexus.BenScharbach.TWTools.TWTerrainTools_Interfaces.Structs;

namespace ImageNexus.BenScharbach.TWTools.TWTerrainTools_Interfaces
{
    // 6/30/2010
    /// <summary>
    /// The <see cref="IHeightToolWindow"/> Interface defines the WCF contract to be used for clients.
    /// </summary>
    public interface IHeightToolWindow
    {
        /// <summary>
        /// Allows setting some error message for display to the windows form.
        /// </summary>
        /// <param name="errorMessage">Error message to display</param>
        void SetErrorMessage(string errorMessage);

        /// <summary>
        /// Converts a given System 'Bitmap' to a 'BitmapImage', and stores
        /// into the pictureBox.
        /// </summary>
        /// <param name="bitmapToSet"><see cref="System.Drawing.Bitmap"/> to set into pictureBox.</param>
        void SetPictureBoxImage(System.Drawing.Bitmap bitmapToSet);

        /// <summary>
        /// Retrieves the current Perlin-Noise attributes for pass-1.
        /// </summary>
        /// <param name="perlinNoisePass">(OUT) <see cref="PerlinNoisePass"/> structure</param>
        void GetPerlinNoiseAttributesForPass1(out PerlinNoisePass perlinNoisePass);

        /// <summary>
        /// Retrieves the current Perlin-Noise attributes for pass-2.
        /// </summary>
        /// <param name="perlinNoisePass">(OUT) <see cref="PerlinNoisePass"/> structure</param>
        void GetPerlinNoiseAttributesForPass2(out PerlinNoisePass perlinNoisePass);

        /// <summary>
        /// Checks if mouse cursor is within the visual window control.
        /// </summary>
        /// <returns>true/false of result</returns>
        bool IsMouseInControl();

        /// <summary>
        /// Shows the WPF form.
        /// </summary>
        void Show();

        /// <summary>
        /// Hides the WPF form.
        /// </summary>
        void Hide();

        /// <summary>
        /// Closes the WPF form.
        /// </summary>
        void Close();
    }
}