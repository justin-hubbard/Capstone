using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using System.Windows.Media;

namespace Skywalker_Vision.Kinect
{
    public class NDFrame : BaseFrame, IDFrame, IBaseFrame
    {
        int width;
        int height;
        private ushort minReliableDistance { get; set; }
        private ushort maxReliableDistance { get; set; }
        private ushort[] pixels;

        //private FrameDescription depthFrameDescription = null;

        internal NDFrame(DepthFrame depthFrame)
        {
            this.frameDescriptor = depthFrame.FrameDescription;

            this.width = depthFrame.FrameDescription.Width;
            this.height = depthFrame.FrameDescription.Height;
            this.pixels = new ushort[width * height];
            // this.depthFrameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;

            this.WriteableBitmap = new WriteableBitmap(
                    this.width, this.height,
                    96.0, 96.0,
                    PixelFormats.Gray8,
                    null);

            using (KinectBuffer depthBuffer = depthFrame.LockImageBuffer())
            {
                // ushort maxDepth = ushort.MaxValue;
                this.ProcessDepthFrameData(depthFrame, depthBuffer.UnderlyingBuffer, depthBuffer.Size);
            }

            this.ProcessBitmap(); // creates bitmap, and bitmap source
        }

        private unsafe void ProcessDepthFrameData(DepthFrame depthFrame, System.IntPtr depthFrameData,
            uint depthFrameDataSize)
        {

            byte[] enhPixelData;
            ushort* frameData = (ushort*)depthFrameData;

            enhPixelData = new byte[depthFrame.FrameDescription.Width * depthFrame.FrameDescription.Height];

            ushort maxDepth = ushort.MaxValue;
            ushort minDepth = depthFrame.DepthMinReliableDistance;
            int MapDepthToByte = 8000 / 256;

            for (int i = 0; i < (int)(depthFrameDataSize / depthFrame.FrameDescription.BytesPerPixel); ++i)
            {
                ushort depthValue = frameData[i];
                //int pixelY = i / height;
                int pixelX = i % width;
                int newX = width - pixelX;
                pixelX = i - pixelX + newX - 1;
                enhPixelData[pixelX] = (byte)(depthValue >= minDepth && depthValue <= maxDepth ? (depthValue / MapDepthToByte) : 0);
            }
            this.WriteableBitmap.WritePixels(
                new Int32Rect(0, 0, this.WriteableBitmap.PixelWidth, this.WriteableBitmap.PixelHeight),
                enhPixelData,
                this.WriteableBitmap.PixelWidth, 0);
        }

        public ushort[] get_DepthData()
        {
            return pixels;
        }

        /// <summary>
        /// Returns the Minimum Reliable Distance reported by the Kinect
        /// </summary>
        public ushort MinReliableDistance
        {
            get
            {
                return minReliableDistance;
            }
        }

        /// <summary>
        /// Returns the Maximum Reliable Distance as reported by the Kinect
        /// </summary>
        public ushort MaxReliableDistance
        {
            get
            {
                return maxReliableDistance;
            }
        }

        public int frameWidth()
        {
            int x = this.width;
            return x;
        }

        public int frameHeight()
        {
            int y = this.height;
            return y;
        }
    }
}
