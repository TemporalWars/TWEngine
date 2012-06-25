using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace TWTerrainToolsWPF.Extentions
{
    // 7/4/2010
    ///<summary>
    /// The <see cref="WPFExtensions"/> is used to extend other pre-made classes with new
    /// abilities. 
    ///</summary>
    public static class WPFExtensions
    {
        // 7/4/2010
        /// <summary>
        /// Extends the <see cref="Bitmap"/> class by including this new method which
        /// converts an instance of the <see cref="Bitmap"/> to a new instance of the WPF 
        /// <see cref="BitmapImage"/> type.
        /// </summary>
        /// <param name="bitmap">this instance of <see cref="Bitmap"/></param>
        /// <returns>New instance of <see cref="BitmapImage"/></returns>
        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            //
            // Convert to proper BitmapImage format.
            //

            // 1st - convert into memory stream
            var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

            // 2nd - create new BitmapImage type from memory stream.
            var bImg = new BitmapImage();
            bImg.BeginInit();
            bImg.StreamSource = new MemoryStream(ms.ToArray());
            bImg.EndInit();

            return bImg;
        }
    }
}
