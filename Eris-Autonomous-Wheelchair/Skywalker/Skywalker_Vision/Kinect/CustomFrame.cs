using System.Windows.Media.Imaging;

namespace Skywalker_Vision.Kinect {
    class CustomFrame : BaseFrame, IBaseFrame {
        public void SetBitmap(WriteableBitmap nBitmap)
        {
            this.WriteableBitmap = nBitmap;
            this.ProcessBitmap(); // creates bitmap, and bitmap source
        }
    }
}
