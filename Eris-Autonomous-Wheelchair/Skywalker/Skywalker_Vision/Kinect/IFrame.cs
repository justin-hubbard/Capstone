using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using System.Windows.Media;

//Infraredframe
namespace Skywalker_Vision.Kinect
{
    public class IFrame : BaseFrame, IDFrame, IBaseFrame
    {
        private int width;
        private int height;

        private ushort[] pixels;
        /// <summary>
        /// Maximum value (as a float) that can be returned by the InfraredFrame
        /// </summary>
        private const float InfraredSourceValueMaximum = (float)ushort.MaxValue;

        /// <summary>
        /// The value by which the infrared source data will be scaled
        /// </summary>
        private const float InfraredSourceScale = 0.75f;

        /// <summary>
        /// Smallest value to display when the infrared data is normalized
        /// </summary>
        private const float InfraredOutputValueMinimum = 0.01f;

        /// <summary>
        /// Largest value to display when the infrared data is normalized
        /// </summary>
        private const float InfraredOutputValueMaximum = 1.0f;

        internal IFrame(InfraredFrame InfraredFrame) {
            this.frameDescriptor = InfraredFrame.FrameDescription;

            this.width = InfraredFrame.FrameDescription.Width;
            this.height = InfraredFrame.FrameDescription.Height;
            this.pixels = new ushort[width*height];
            // this.depthFrameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;

            this.WriteableBitmap = new WriteableBitmap(
                    this.width, this.height,
                    96.0, 96.0,
                    PixelFormats.Gray8,
                    null);

            using (KinectBuffer depthBuffer = InfraredFrame.LockImageBuffer())
            {
                // ushort maxDepth = ushort.MaxValue;
                this.ProcessDepthFrameData(InfraredFrame, depthBuffer.UnderlyingBuffer, depthBuffer.Size);
            }

            this.ProcessBitmap(); // creates bitmap, and bitmap source
        }

        private unsafe void ProcessDepthFrameData(InfraredFrame infraredFrame, System.IntPtr infraredFrameData, 
            uint infraredFrameDataSize) {
            ushort* frameData = (ushort*)infraredFrameData;
            this.WriteableBitmap = new WriteableBitmap(infraredFrame.FrameDescription.Width, infraredFrame.FrameDescription.Height, 96.0, 96.0, PixelFormats.Gray32Float, null);
            
            // lock the target bitmap
            this.WriteableBitmap.Lock();

            // get the pointer to the bitmap's back buffer
            float* backBuffer = (float*)this.WriteableBitmap.BackBuffer;

            // process the infrared data
            for (int i = 0; i < (int)(infraredFrameDataSize / infraredFrame.FrameDescription.BytesPerPixel); ++i)
            {
                // since we are displaying the image as a normalized grey scale image, we need to convert from
                // the ushort data (as provided by the InfraredFrame) to a value from [InfraredOutputValueMinimum, InfraredOutputValueMaximum]
                int pixelX = i % width;
                int newX = width - pixelX;
                pixelX = i - pixelX + newX - 1;
                backBuffer[pixelX] = System.Math.Min(InfraredOutputValueMaximum, (((float)frameData[i] / InfraredSourceValueMaximum * InfraredSourceScale) * (1.0f - InfraredOutputValueMinimum)) + InfraredOutputValueMinimum);
            }

            // mark the entire bitmap as needing to be drawn
            this.WriteableBitmap.AddDirtyRect(new Int32Rect(0, 0, this.WriteableBitmap.PixelWidth, this.WriteableBitmap.PixelHeight));

            // unlock the bitmap
            this.WriteableBitmap.Unlock();
        }

    }
}
