using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace Skywalker_Vision.Kinect {
    public class VFrame : BaseFrame, IVFrame, IBaseFrame {
        internal VFrame(ColorFrame colorFrame)
        {
            WriteableBitmap temp;
            // copy over FrameDescription Variables
            frameDescriptor = colorFrame.FrameDescription;

            // Initialize the bitmap
            temp = new WriteableBitmap(this.frameDescriptor.Width, this.frameDescriptor.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
            
            /* Consider better methods to prevent callers from waiting for the frame to be unlocked */
            temp.Lock();
            
            //Copy frame to temp variable
            colorFrame.CopyConvertedFrameDataToIntPtr(
                        temp.BackBuffer,
                        (uint)(this.frameDescriptor.Width * this.frameDescriptor.Height * 4),
                        ColorImageFormat.Bgra);

            temp.AddDirtyRect(new Int32Rect(0, 0, temp.PixelWidth, temp.PixelHeight));

            temp.Unlock();

            this.WriteableBitmap = temp.Flip(WriteableBitmapExtensions.FlipMode.Vertical);

            this.ProcessBitmap(); // creates bitmap, and bitmap source
        }
    }
}
