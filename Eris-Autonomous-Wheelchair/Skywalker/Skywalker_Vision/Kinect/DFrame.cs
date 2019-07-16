using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using System.Windows.Media;

namespace Skywalker_Vision.Kinect
{
    public class DFrame : BaseFrame, IDFrame, IBaseFrame
    {
        int width;
        int height;
        private ushort minReliableDistance { get; set; }
        private ushort maxReliableDistance { get; set; }
        private ushort[] pixels;

        //private FrameDescription depthFrameDescription = null;

        internal DFrame(DepthFrame depthFrame)
        {
            this.frameDescriptor = depthFrame.FrameDescription;

            this.width = depthFrame.FrameDescription.Width;
            this.height = depthFrame.FrameDescription.Height;
            this.pixels = new ushort[width * height];
            // this.depthFrameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;

            this.WriteableBitmap = new WriteableBitmap(
                    this.width, this.height,
                    96.0, 96.0,
                    PixelFormats.Bgr32,
                    null);

            this.WriteableBitmapForDoorNavigation = new WriteableBitmap(this.width, this.height, 96.0, 96.0, PixelFormats.Gray8, null);

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
            byte[] pixelData;//for door navigation
            ushort* frameData = (ushort*)depthFrameData;
            int depth;
            int gray;
            int loThreshold = 250;
            int hiThreshold = 1000;
            int bytesPerPixel = 4;
            int width = depthFrame.FrameDescription.Width;
            int height = depthFrame.FrameDescription.Height;
            int minDepth = depthFrame.DepthMinReliableDistance;
            //int maxDepth = depthFrame.DepthMaxReliableDistance;
            ushort maxDepth = ushort.MaxValue;
            //using for obstacle detection
            enhPixelData = new byte[depthFrame.FrameDescription.Width * depthFrame.FrameDescription.Height * bytesPerPixel];
            //using for door detection
            pixelData = new byte[depthFrame.FrameDescription.Width * depthFrame.FrameDescription.Height];

            for (int i = 0, j = 0; i < (int)(depthFrameDataSize / this.frameDescriptor.BytesPerPixel); i++,
                j += bytesPerPixel)
            {

                pixels[i] = frameData[i];
                depth = frameData[i] >> 1;

                int pixelX = i % width;
                int newX = width - pixelX;
                ushort depthValue = frameData[i];
                int MapDepthToByte = 8000 / 256;
                pixelX = i - pixelX + newX - 1;
                //pixels[i] = depthValue;
                //if (depthValue >= minDepth && depthValue <= maxDepth)
                //    pixelData[pixelX] = (byte)(depthValue / MapDepthToByte);
                //else if (depthValue < minDepth)
                //    pixelData[pixelX] = (byte)0;
                //else
                //    pixelData[pixelX] = (byte)255;

                //pixelData[pixelX] = (byte)(depthValue / MapDepthToByte);
                pixelData[pixelX] = (byte)(depthValue >= minDepth && depthValue <= maxDepth ? (depthValue / MapDepthToByte) : 0);


                j = pixelX * bytesPerPixel;
                if (depth < loThreshold || depth > hiThreshold)
                {
                    gray = 0xFF;
                    //gray = 0;
                }
                else
                {
                    gray = (255 * depth / 0xFFF);
                }

                enhPixelData[j] = (byte)gray;
                enhPixelData[j + 1] = (byte)gray;
                enhPixelData[j + 2] = (byte)gray;
            }

            this.WriteableBitmap.WritePixels(
                new Int32Rect(0, 0, this.WriteableBitmap.PixelWidth, this.WriteableBitmap.PixelHeight),
                enhPixelData,
                this.WriteableBitmap.PixelWidth * sizeof(int), 0);
            this.WriteableBitmapForDoorNavigation.WritePixels(
               new Int32Rect(0, 0, this.WriteableBitmapForDoorNavigation.PixelWidth, this.WriteableBitmapForDoorNavigation.PixelHeight),
               pixelData,
               this.WriteableBitmapForDoorNavigation.PixelWidth,
               0);
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