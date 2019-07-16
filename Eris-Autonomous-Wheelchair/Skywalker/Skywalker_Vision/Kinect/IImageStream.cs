using System.Drawing;
using System.Windows.Media.Imaging;
using Emgu.CV;

namespace Skywalker_Vision.Kinect {
    public interface IImageStream {
        /// <summary>
        /// Gets the latest RGB frame
        /// </summary
        /// <returns>Latest RGB frame as a VFrame which inherits from BaseFrame</returns>
        BaseFrame GetFrame();

        /// <summary>
        /// Name of stream
        /// </summary>
        /// <returns>Stream name</returns>
        string GetName();
    }
}
