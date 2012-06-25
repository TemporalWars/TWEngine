using System.Windows;
using TWTerrainToolsWPF.Extentions;

namespace TWTerrainToolsWPF
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        /// <summary>
        /// Constuctor
        /// </summary>
        //[CatchAllExceptionHandling(false, null, null)]
        public About(bool validLicense, string userName, string serialNumber)
        {
            InitializeComponent();

            // Set License Edition
            txtEditionType.Text = !validLicense ? "Evaluation Edition" : "Standard Edition";

            // Set Image.
            ceoPic.Source = Properties.Resources.BenScharbachPic.ToBitmapImage();

            // Set License UserName
            txtLicenseUser.Text = userName;
            // Set License Serial Number
            txtLicenseSerial.Text = serialNumber;

        }


    }
}
