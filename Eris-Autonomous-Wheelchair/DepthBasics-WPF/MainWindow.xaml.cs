//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.DepthBasics
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using Microsoft.Kinect;
    using Emgu;
    using Emgu.CV;
    using Emgu.Util;
    using Emgu.CV.Structure;
    using Emgu.CV.CvEnum;

    using System.Drawing.Imaging;
    using System.Drawing;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Map depth range to byte range
        /// </summary>
        private const int MapDepthToByte = 8000 / 256;
        
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Reader for depth frames
        /// </summary>
        private DepthFrameReader depthFrameReader = null;

        /// <summary>
        /// Description of the data contained in the depth frame
        /// </summary>
        private FrameDescription depthFrameDescription = null;
            
        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap depthBitmap = null;

        /// <summary>
        /// Intermediate storage for frame data converted to color
        /// </summary>
        private byte[] depthPixels = null;

        /// <summary>
        /// Current status text to display
        /// </summary>
        private string statusText = null;

        private enum ProcessType  {ProcessObstacle, ProcessContour};
        private ProcessType processType = ProcessType.ProcessObstacle;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            // get the kinectSensor object
            this.kinectSensor = KinectSensor.GetDefault();

            // open the reader for the depth frames
            this.depthFrameReader = this.kinectSensor.DepthFrameSource.OpenReader();

            // wire handler for frame arrival
            this.depthFrameReader.FrameArrived += this.Reader_FrameArrived;

            // get FrameDescription from DepthFrameSource
            this.depthFrameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;

            // allocate space to put the pixels being received and converted
            this.depthPixels = new byte[this.depthFrameDescription.Width * this.depthFrameDescription.Height];

            // create the bitmap to display
            this.depthBitmap = new WriteableBitmap(this.depthFrameDescription.Width, this.depthFrameDescription.Height, 96.0, 96.0, PixelFormats.Gray8, null);

            // set IsAvailableChanged event notifier
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            // open the sensor
            this.kinectSensor.Open();

            // set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.NoSensorStatusText;

            // use the window object as the view model in this simple example
            this.DataContext = this;

            // initialize the components (controls) of the window
            this.InitializeComponent();
        }

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        /*public ImageSource ImageSource
        {
            get
            {
                return this.depthBitmap;
            }
        }*/
       /* <Image Source="{Binding ImageSource}" Stretch="UniformToFill" />*/



        /// <summary>
        /// Gets or sets the current status text to display
        /// </summary>
        public string StatusText
        {
            get
            {
                return this.statusText;
            }

            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;

                    // notify any bound elements that the text has changed
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.depthFrameReader != null)
            {
                // DepthFrameReader is IDisposable
                this.depthFrameReader.Dispose();
                this.depthFrameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }

        /// <summary>
        /// Handles the user clicking on the screenshot button
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void ScreenshotButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.depthBitmap != null)
            {
                // create a png bitmap encoder which knows how to save a .png file
                BitmapEncoder encoder = new PngBitmapEncoder();

                // create frame from the writable bitmap and add to encoder
                encoder.Frames.Add(BitmapFrame.Create(this.depthBitmap));

                string time = System.DateTime.UtcNow.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

                string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                string path = Path.Combine(myPhotos, "KinectScreenshot-Depth-" + time + ".png");

                // write the new file to disk
                try
                {
                    // FileStream is IDisposable
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        encoder.Save(fs);
                    }

                    this.StatusText = string.Format(CultureInfo.CurrentCulture, Properties.Resources.SavedScreenshotStatusTextFormat, path);
                }
                catch (IOException)
                {
                    this.StatusText = string.Format(CultureInfo.CurrentCulture, Properties.Resources.FailedScreenshotStatusTextFormat, path);
                }
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.processType == ProcessType.ProcessContour)
            {
                this.processType = ProcessType.ProcessObstacle;
            }
            else
            {
                this.processType = ProcessType.ProcessContour;
            }
        }


        /// <summary>
        /// Handles the depth frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        /// public struct PolarCoordinates
        public struct PolarCoordinates
        {
            public float Rho;
            public float Theta;
        }
        private void Reader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            bool depthFrameProcessed = false;

           using (DepthFrame depthFrame = e.FrameReference.AcquireFrame())
            {
                if (depthFrame != null)
                {
                    // the fastest way to process the body index data is to directly access 
                    // the underlying buffer
                     using (Microsoft.Kinect.KinectBuffer depthBuffer = depthFrame.LockImageBuffer())
                    {
                        // verify data and write the color data to the display bitmap
                       /// if (((this.depthFrameDescription.Width * this.depthFrameDescription.Height) == (depthBuffer.Size / this.depthFrameDescription.BytesPerPixel)) &&
                           /// (this.depthFrameDescription.Width == this.depthBitmap.PixelWidth) && (this.depthFrameDescription.Height == this.depthBitmap.PixelHeight))
                        //{
                            // Note: In order to see the full range of depth (including the less reliable far field depth)
                            // we are setting maxDepth to the extreme potential depth threshold
                           // ushort maxDepth = ushort.MaxValue;
                            
                            // If you wish to filter by reliable depth distance, uncomment the following line:
                           // maxDepth = depthFrame.DepthMaxReliableDistance;

                        if (this.processType == ProcessType.ProcessObstacle)
                        {
                            this.ProcessDepthFrameData(depthFrame, depthBuffer.UnderlyingBuffer, depthBuffer.Size, /*depthFrame.DepthMinReliableDistance, maxDepth*/0, 50);
                        }
                        else
                        {
                            this.ProcessContourFrameData(depthFrame, depthBuffer.UnderlyingBuffer, depthBuffer.Size, 0, 50);
                        }
                            depthFrameProcessed = true;
                      //  }
                    }
                }
            }

            if (depthFrameProcessed)
            {
                //this.RenderDepthPixels();
            }
        }

        /// <summary>
        /// Sets the Camera Source to show obstacles.
        /// Directly accesses the underlying image buffer of the DepthFrame to 
        /// create a displayable bitmap.
        /// This function requires the /unsafe compiler option as we make use of direct
        /// access to the native memory pointed to by the depthFrameData pointer.
        /// </summary>
        /// <param name="depthFrameData">Pointer to the DepthFrame image data</param>
        /// <param name="depthFrameDataSize">Size of the DepthFrame image data</param>
        /// <param name="minDepth">The minimum reliable depth value for the frame</param>
        /// <param name="maxDepth">The maximum reliable depth value for the frame</param>
        private unsafe void ProcessDepthFrameData(DepthFrame depthFrame, IntPtr depthFrameData, uint depthFrameDataSize, ushort minDepth, ushort maxDepth)
        {
            // depth frame data is a 16 bit value
            ushort* frameData = (ushort*)depthFrameData;

            // convert depth to a visual representation
            /*for (int i = 0; i < (int)(depthFrameDataSize / this.depthFrameDescription.BytesPerPixel); ++i)
            {
                // Get the depth for this pixel
                ushort depth = frameData[i];

                // To convert to a byte, we're mapping the depth value to the byte range.
                // Values outside the reliable depth range are mapped to 0 (black).
                this.depthPixels[i] = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MapDepthToByte) : 0);
            }*/

            int depth;
            int gray;
            int loThreshold = 25;
            int hiThreshold = 1600;
            int bytesPerPixel = 4;
            byte[] rgb = new byte[3];
            byte[] enhPixelData = new byte[depthFrame.FrameDescription.Width * depthFrame.FrameDescription.Height * bytesPerPixel];

            for (int i = 0, j = 0; i < (int)(depthFrameDataSize / this.depthFrameDescription.BytesPerPixel); i++, j += bytesPerPixel)
            {
                depth = frameData[i]>>1;

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

            /*camera.Source = BitmapSource.Create(depthFrame.FrameDescription.Width, depthFrame.FrameDescription.Height,
 96, 96, PixelFormats.Bgr32, null,
 enhPixelData,
 depthFrame.FrameDescription.Width * bytesPerPixel);*/
           int width = depthFrame.FrameDescription.Width;
           int height = depthFrame.FrameDescription.Height;
           
            System.Windows.Media.PixelFormat format = PixelFormats.Bgr32;
            WriteableBitmap colorBitmap = new WriteableBitmap(width, height,
             96.0, 96.0, PixelFormats.Bgr32, null);                                                                

            colorBitmap.WritePixels(
            new Int32Rect(0, 0, colorBitmap.PixelWidth, colorBitmap.PixelHeight),
           enhPixelData,
           colorBitmap.PixelWidth * sizeof(int),
           0);


            Bitmap bmp2 = BitmapFromWriteableBitmap(colorBitmap);
            Image<Bgr, Byte> img = new Image<Bgr, Byte>(bmp2);
            Image<Gray, Byte> grey = new Image<Gray, Byte>(bmp2.Size);
          
           CvInvoke.cvInRangeS(img.Ptr, new MCvScalar(0.0, 0.0, 0.0), new MCvScalar(250.0, 250.0, 250.0), grey.Ptr);
           
         
           CvInvoke.cvErode(grey.Ptr, grey.Ptr, (IntPtr)null, 4);

            CvInvoke.cvDilate(grey.Ptr, grey.Ptr, (IntPtr)null, 3);
            Image<Gray, Byte> dst = new Image<Gray, Byte>(grey.Size);
            Image<Rgba, Byte> dst2 = new Image<Rgba, Byte>(grey.Size);

            CvInvoke.cvCanny(grey.Ptr, dst.Ptr, 50, 200, 3);
            CvInvoke.cvCvtColor(dst, dst2, Emgu.CV.CvEnum.COLOR_CONVERSION.CV_GRAY2BGR);
 
          //  byte a = grey.Data[200, 200, 0];


            using (MemStorage stor = new MemStorage())
            {
                IntPtr lines = CvInvoke.cvHoughLines2(dst.Ptr, stor.Ptr, Emgu.CV.CvEnum.HOUGH_TYPE.CV_HOUGH_STANDARD, 10, (10 * Math.PI) / 180, 50, 50, 10);

                int maxLines = 100;
                for (int i = 0; i < maxLines; i++)
                {
                    IntPtr line = CvInvoke.cvGetSeqElem(lines, i);
                   
                    if (line== IntPtr.Zero)
                    {
                        // No more lines
                        break;
                    }

                    MCvScalar color = new MCvScalar(255, 0, 0);

                    PolarCoordinates coords = (PolarCoordinates)System.Runtime.InteropServices.Marshal.PtrToStructure(line, typeof(PolarCoordinates));
                    float rho = coords.Rho, theta = coords.Theta;
                    System.Drawing.Point pt1 = new System.Drawing.Point();
                    System.Drawing.Point pt2 = new System.Drawing.Point();
                    double a = Math.Cos(theta), b = Math.Sin(theta);
                    double x0 = a * rho, y0 = b * rho;
                    pt1.X = (int)(x0*10 + 100000 * (-b));
                    pt1.Y = (int)(y0 * 10 + 100000 * (a));
                    pt2.X = (int)(x0 * 10 - 100000 * (-b));
                    pt2.Y = (int)(y0 * 10 - 100000 * (a));


                   //PolarCoordinates coords2= (PolarCoordinates)System.Runtime.InteropServices.Marshal.PtrToStructure(line2, typeof(PolarCoordinates));
                  // CvInvoke.cvLine(dst2, new System.Drawing.Point((int)coords1.Rho, (int)coords1.Theta), new System.Drawing.Point((int)coords2.Rho, (int)coords2.Theta), color, 3, LINE_TYPE.EIGHT_CONNECTED, 8);
                   CvInvoke.cvLine(dst2, pt1,pt2, color, 3, LINE_TYPE.CV_AA, 8);
                   //LineSegment2D line = new LineSegment2D( new System.Drawing.Point((int)coords1.Rho, (int)coords1.Theta), new System.Drawing.Point((int)coords2.Rho, (int)coords2.Theta));
                   //dst2.Draw(line, new Rgba(255, 0, 0,0), 5);
                  
                }
                
            
            }
            camera.Source = ToBitmapSource3(grey);

          
          //camera.Source = ToBitmapSource3(img);
           // double area = area_check(grey);
            //System.Diagnostics.Debug.WriteLine(a);
            //Console.WriteLine("    Obstruction Area: " +a);



        }

        /// <summary>
        /// Sets the Camera Source to a depth map showing contours.
        /// Directly accesses the underlying image buffer of the DepthFrame to 
        /// create a displayable bitmap.
        /// This function requires the /unsafe compiler option as we make use of direct
        /// access to the native memory pointed to by the depthFrameData pointer.
        /// </summary>
        /// <param name="depthFrameData">Pointer to the DepthFrame image data</param>
        /// <param name="depthFrameDataSize">Size of the DepthFrame image data</param>
        /// <param name="minDepth">The minimum reliable depth value for the frame</param>
        /// <param name="maxDepth">The maximum reliable depth value for the frame</param>
        private unsafe void ProcessContourFrameData(DepthFrame depthFrame, IntPtr depthFrameData, uint depthFrameDataSize, ushort minDepth, ushort maxDepth)
        {
            // depth frame data is a 16 bit value
            ushort* frameData = (ushort*)depthFrameData;

            // convert depth to a visual representation
            /*for (int i = 0; i < (int)(depthFrameDataSize / this.depthFrameDescription.BytesPerPixel); ++i)
            {
                // Get the depth for this pixel
                ushort depth = frameData[i];

                // To convert to a byte, we're mapping the depth value to the byte range.
                // Values outside the reliable depth range are mapped to 0 (black).
                this.depthPixels[i] = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MapDepthToByte) : 0);
            }*/

            int depth;
            int gray;
            int loThreshold = 25;
            int hiThreshold = 1600;
            int bytesPerPixel = 4;
            byte[] rgb = new byte[3];
            byte[] enhPixelData = new byte[depthFrame.FrameDescription.Width * depthFrame.FrameDescription.Height * bytesPerPixel];

            for (int i = 0, j = 0; i < (int)(depthFrameDataSize / this.depthFrameDescription.BytesPerPixel); i++, j += bytesPerPixel)
            {
                depth = frameData[i] >> 1;

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

            /*camera.Source = BitmapSource.Create(depthFrame.FrameDescription.Width, depthFrame.FrameDescription.Height,
 96, 96, PixelFormats.Bgr32, null,
 enhPixelData,
 depthFrame.FrameDescription.Width * bytesPerPixel);*/
            int width = depthFrame.FrameDescription.Width;
            int height = depthFrame.FrameDescription.Height;

            System.Windows.Media.PixelFormat format = PixelFormats.Bgr32;
            WriteableBitmap colorBitmap = new WriteableBitmap(width, height,
             96.0, 96.0, PixelFormats.Bgr32, null);

            colorBitmap.WritePixels(
            new Int32Rect(0, 0, colorBitmap.PixelWidth, colorBitmap.PixelHeight),
           enhPixelData,
           colorBitmap.PixelWidth * sizeof(int),
           0);


            Bitmap bmp2 = BitmapFromWriteableBitmap(colorBitmap);
            Image<Bgr, Byte> img = new Image<Bgr, Byte>(bmp2);
            Image<Gray, Byte> grey = new Image<Gray, Byte>(bmp2.Size);

            CvInvoke.cvInRangeS(img.Ptr, new MCvScalar(0.0, 0.0, 0.0), new MCvScalar(250.0, 250.0, 250.0), grey.Ptr);


            CvInvoke.cvErode(grey.Ptr, grey.Ptr, (IntPtr)null, 4);

            CvInvoke.cvDilate(grey.Ptr, grey.Ptr, (IntPtr)null, 3);
            Image<Gray, Byte> dst = new Image<Gray, Byte>(grey.Size);
            Image<Rgba, Byte> dst2 = new Image<Rgba, Byte>(grey.Size);

            CvInvoke.cvCanny(grey.Ptr, dst.Ptr, 50, 200, 3);
            CvInvoke.cvCvtColor(dst, dst2, Emgu.CV.CvEnum.COLOR_CONVERSION.CV_GRAY2BGR);

            //  byte a = grey.Data[200, 200, 0];

            using (MemStorage stor = new MemStorage())
            {
                IntPtr lines = CvInvoke.cvHoughLines2(dst.Ptr, stor.Ptr, Emgu.CV.CvEnum.HOUGH_TYPE.CV_HOUGH_STANDARD, 10, (10 * Math.PI) / 180, 50, 50, 10);

                int maxLines = 100;
                for (int i = 0; i < maxLines; i++)
                {
                    IntPtr line = CvInvoke.cvGetSeqElem(lines, i);

                    if (line == IntPtr.Zero)
                    {
                        // No more lines
                        break;
                    }

                    MCvScalar color = new MCvScalar(255, 0, 0);

                    PolarCoordinates coords = (PolarCoordinates)System.Runtime.InteropServices.Marshal.PtrToStructure(line, typeof(PolarCoordinates));
                    float rho = coords.Rho, theta = coords.Theta;
                    System.Drawing.Point pt1 = new System.Drawing.Point();
                    System.Drawing.Point pt2 = new System.Drawing.Point();
                    double a = Math.Cos(theta), b = Math.Sin(theta);
                    double x0 = a * rho, y0 = b * rho;
                    pt1.X = (int)(x0 * 10 + 100000 * (-b));
                    pt1.Y = (int)(y0 * 10 + 100000 * (a));
                    pt2.X = (int)(x0 * 10 - 100000 * (-b));
                    pt2.Y = (int)(y0 * 10 - 100000 * (a));
                   
                    //PolarCoordinates coords2= (PolarCoordinates)System.Runtime.InteropServices.Marshal.PtrToStructure(line2, typeof(PolarCoordinates));
                    // CvInvoke.cvLine(dst2, new System.Drawing.Point((int)coords1.Rho, (int)coords1.Theta), new System.Drawing.Point((int)coords2.Rho, (int)coords2.Theta), color, 3, LINE_TYPE.EIGHT_CONNECTED, 8);
                    CvInvoke.cvLine(dst2, pt1, pt2, color, 3, LINE_TYPE.CV_AA, 8);
                    //LineSegment2D line = new LineSegment2D( new System.Drawing.Point((int)coords1.Rho, (int)coords1.Theta), new System.Drawing.Point((int)coords2.Rho, (int)coords2.Theta));
                    //dst2.Draw(line, new Rgba(255, 0, 0,0), 5);

                }
            }
            camera.Source = ToBitmapSource3(dst);
        }
        /// <summary>
        /// Renders color pixels into the writeableBitmap.
        /// </summary>
        private void RenderDepthPixels()
        {
            this.depthBitmap.WritePixels(
                new Int32Rect(0, 0, this.depthBitmap.PixelWidth, this.depthBitmap.PixelHeight),
                this.depthPixels,
                this.depthBitmap.PixelWidth,
                0);



            Bitmap bmp2 = BitmapFromWriteableBitmap(depthBitmap);
            Image<Bgr, Byte> img = new Image<Bgr, Byte>(bmp2);
            Image<Gray, Byte> grey = new Image<Gray, Byte>(bmp2.Size);
           //CvInvoke.cvNot(img.Ptr, img.Ptr);
            
           //CvInvoke.cvInRangeS(img.Ptr, new MCvScalar(40.0, 40.0, 40.0), new MCvScalar(150.0, 150.0, 150.0), grey.Ptr);
           //CvInvoke.cvInRangeS(img.Ptr, new MCvScalar(0.0, 0.0, 0.0), new MCvScalar(2550.0, 2550.0, 2550.0), grey.Ptr);

           // CvInvoke.cvNot(grey.Ptr, grey.Ptr);
           // CvInvoke.cvErode(grey.Ptr, grey.Ptr, (IntPtr)null, 4);

             //CvInvoke.cvDilate(grey.Ptr, grey.Ptr, (IntPtr)null, 3);

             camera.Source = ToBitmapSource3(grey);
            // CvInvoke.cvSetImageROI(grey, new System.Drawing.Rectangle(0, 0, 640, 480));
            double area = area_check(grey);
            Console.WriteLine("    Obstruction Area: " + area.ToString());
            
            

        }
        public static double area_check(IImage bw)
        {

            double area;
            MCvMoments moments = new MCvMoments();
            CvInvoke.cvMoments(bw.Ptr, ref moments, 1);

            double moment10 = CvInvoke.cvGetSpatialMoment(ref moments, 1, 0);
            double moment01 = CvInvoke.cvGetSpatialMoment(ref moments, 0, 1);
            area = CvInvoke.cvGetCentralMoment(ref moments, 0, 0);
            int x = (int)(moment10 / area);
            int y = (int)(moment01 / area);
            return area;
        }

        public static System.Drawing.Bitmap BitmapFromWriteableBitmap(WriteableBitmap writeBmp)
        {
            System.Drawing.Bitmap bmp;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create((BitmapSource)writeBmp));
                enc.Save(outStream);
                bmp = new System.Drawing.Bitmap(outStream);
            }
            return bmp;
        }

        public static BitmapSource ToBitmapSource3(IImage image)
        {
            using (var stream = new MemoryStream())
            {
                image.Bitmap.Save(stream, ImageFormat.Png);

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = new MemoryStream(stream.ToArray());
                bitmap.EndInit();

                return bitmap;
            }
        }
        /// <summary>
        /// Handles the event which the sensor becomes unavailable (E.g. paused, closed, unplugged).
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            // on failure, set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.SensorNotAvailableStatusText;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
