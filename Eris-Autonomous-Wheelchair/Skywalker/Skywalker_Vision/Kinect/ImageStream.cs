using Microsoft.Kinect;

namespace Skywalker_Vision.Kinect
{
    /// <summary>
    /// Base Class for various frame types
    /// </summary>
    public abstract class ImageStream {
        protected FrameDescription frameDescriptor { get; set; }
        protected KinectSensor kinect { get; set; }

        public ImageStream() {
            //Get Kinect
            this.kinect = KinectSensor.GetDefault();
        }

        public virtual BaseFrame GetFrame() {
            return null;
        }

        ~ImageStream() {
            this.kinect.Close();
        }

    }
}