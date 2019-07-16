using System.Drawing;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace Skywalker_Vision.Kinect {
    public abstract class BaseFrame {
        protected FrameDescription frameDescriptor { get; set; }
        protected WriteableBitmap WriteableBitmap { get; set; }
        protected WriteableBitmap WriteableBitmapForDoorNavigation { get; set; }
        protected Bitmap Bitmap { get; set; }
        protected Bitmap BitmapForDoorNavigation { get; set; }
        protected BitmapSource BitmapSource { get; set; }

        ~BaseFrame()
        {
            //this.WriteableBitmap.di
            //this.Bitmap.Dispose();
            //this.BitmapSource.di
        }

        // <summary>
        /// Gets the number of bytes per pixel in the frame
        /// </summary>
        /// <returns>Integer</returns>
        public int GetBytesPerPixel() { return (int)frameDescriptor.BytesPerPixel; }

        /// <summary>
        /// Returns the Diagonal Field of view of the frame in degrees
        /// </summary>
        public float GetDiagonalFov() { return frameDescriptor.DiagonalFieldOfView; }

        /// <summary>
        /// Returns the height of the frame, in pixels
        /// </summary>
        public int GetHeight() { return (int)frameDescriptor.Height; }

        /// <summary>
        /// Returns the Horizontal field of view, in degrees
        /// </summary>
        public float GetHorizontalFov() { return frameDescriptor.HorizontalFieldOfView; }

        /// <summary>
        /// Returns the length in pixels of the current frame
        /// </summary>
        public int GetLengthInPixels() { return (int)frameDescriptor.LengthInPixels; }

        /// <summary>
        /// Returns the vertical field of view of the frame
        /// </summary>
        public float GetVerticalFov() { return frameDescriptor.VerticalFieldOfView; }

        /// <summary>
        /// Returns the width, in pixels, of the frame
        /// </summary>
        public int GetWidth() { return (int)frameDescriptor.Width; }

        /// <summary>
        /// Returns a WriteableBitmap that can be displayed on the screen
        /// </summary>
        public WriteableBitmap GetBitmap() 
        {
            return this.WriteableBitmap;
        }

        public Bitmap GetBMP()
        {
            return this.Bitmap;
        }

        public WriteableBitmap GetWriteableBitmapForDoorNavi()
        {
            return this.WriteableBitmapForDoorNavigation;
        }

        public Bitmap GetDoorNaviBitmap()
        {
            return this.BitmapForDoorNavigation;
        }

        public void Dispose()
        {
            this.Bitmap.Dispose();
        }

        public void DisposeDoorNaviBitmap()
        {
            this.BitmapForDoorNavigation.Dispose();
        }

        protected void ProcessBitmap()
        {
            if (this.WriteableBitmap != null)
            {
                this.Bitmap = Utils.WriteableBitmapToBitmap(this.WriteableBitmap);
                this.WriteableBitmap.Freeze();
            }
            
            if (this.WriteableBitmapForDoorNavigation != null)
            {
                this.BitmapForDoorNavigation = Utils.WriteableBitmapToBitmap(this.WriteableBitmapForDoorNavigation);
                this.WriteableBitmapForDoorNavigation.Freeze();
            }
           
            /*this.BitmapSource = Utils.BitmapToBitmapSource(this.Bitmap);
            this.BitmapSource.Freeze();*/

        }
    }
}
    