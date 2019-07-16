using System.Windows.Media.Imaging;

namespace Skywalker_Vision.Kinect {
    interface IVFrame {
        /// <summary>
        /// Returns a WriteableBitmap that can be displayed on the screen.
        /// </summary>
        /// <returns>A writeable bitmap.</returns>
        WriteableBitmap GetBitmap();

        /// <summary>
        /// Gets the number of bytes per pixel in the frame
        /// </summary>
        /// <returns>Int of how many bytes per pixel</returns>
        int GetBytesPerPixel();

        /// <summary>
        /// Returns the Diagonal Field of view of the frame in degrees
        /// </summary>
        /// <returns>Float of Diagonal FOV in degrees</returns>
        float GetDiagonalFov();

        /// <summary>
        /// Returns the height of the frame, in pixels
        /// </summary>
        int GetHeight();

        /// <summary>
        /// Returns the Horizontal field of view, in degrees
        /// </summary>
        /// <returns>Float of Horizontal FOV in degrees</returns>
        float GetHorizontalFov();

        /// <summary>
        /// Returns the length in pixels of the current frame
        /// </summary>
        /// <returns>Integer of how many pixels there are</returns>
        int GetLengthInPixels();

        /// <summary>
        /// Returns the vertical field of view of the frame
        /// </summary>
        /// <returns>Float of Vertical FOV in degrees</returns>
        float GetVerticalFov();

        /// <summary>
        /// Returns the width, in pixels, of the frame
        /// </summary>
        /// <returns>Width of frame in pixels</returns>
        int GetWidth();    
    }
}
