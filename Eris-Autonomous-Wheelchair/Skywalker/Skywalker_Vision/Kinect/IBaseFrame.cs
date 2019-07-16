using System.Drawing;
using System.Windows.Media.Imaging;

namespace Skywalker_Vision.Kinect {
    interface IBaseFrame {
        WriteableBitmap GetBitmap();
        Bitmap GetBMP();
        //BitmapSource GetBitmapSource();
    }
}
