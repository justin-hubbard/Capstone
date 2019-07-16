//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace Microsoft.Samples.Kinect.ColorBasics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Runtime.InteropServices;
    using System.Runtime.CompilerServices;
    using Emgu.CV.CvEnum;
    using System.IO;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {


        class Line
        {
            public System.Drawing.Point p1;
            public System.Drawing.Point p2;
            public double slop;

        };
        
        
        // <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Reader for color frames
        /// </summary>
        private ColorFrameReader colorFrameReader = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap colorBitmap = null;
        

        /// <summary>
        /// Current status text to display
        /// </summary>
        private string statusText = null;
        int frameCount ;
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        List<Line> myLines = new List<Line>();
        public MainWindow()
        {
            frameCount = 0;
            // get the kinectSensor object
            this.kinectSensor = KinectSensor.GetDefault();

            // open the reader for the color frames
            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();

            // wire handler for frame arrival
            this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;

            // create the colorFrameDescription from the ColorFrameSource using Bgra format
            FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

            // create the bitmap to display
            this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
            
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
      /*  public ImageSource ImageSource
        {
            get
           {
               //return this.colorBitmap;
               return (ImageSource)this.displayframe;
           }
        }*/

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
            if (this.colorFrameReader != null)
            {
                // ColorFrameReder is IDisposable
                this.colorFrameReader.Dispose();
                this.colorFrameReader = null;
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
            if (this.colorBitmap != null)
            {
                // create a png bitmap encoder which knows how to save a .png file
                BitmapEncoder encoder = new PngBitmapEncoder();

                // create frame from the writable bitmap and add to encoder
                encoder.Frames.Add(BitmapFrame.Create(this.colorBitmap));

                string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

                string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                string path = Path.Combine(myPhotos, "KinectScreenshot-Color-" + time + ".png");

                // write the new file to disk
                try
                {
                    // FileStream is IDisposable
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        encoder.Save(fs);
                    }

                    this.StatusText = string.Format(Properties.Resources.SavedScreenshotStatusTextFormat, path);
                }
                catch (IOException)
                {
                    this.StatusText = string.Format(Properties.Resources.FailedScreenshotStatusTextFormat, path);
                }
            }
        }

        /// <summary>
        /// Handles the color frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
       
        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // ColorFrame is IDisposable
            
            frameCount++ ;
            if (frameCount % 6 == 0)
            {
                using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
                {
                    if (colorFrame != null)
                    {
                        FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                        BitmapSource aa = ToBitmap(colorFrame);
                        Bitmap bmp = BitmapFromSource(aa);
                        Image<Bgr, Byte> img = new Image<Bgr, Byte>(bmp);


                   /*    Image<Gray, Byte> img2 = img.Convert<Gray, Byte>();
                        Image<Gray, Byte> dst = new Image<Gray, Byte>(img2.Size);
                        Image<Rgb, Byte> dst2 = new Image<Rgb, Byte>(img2.Size);


                        CvInvoke.cvCanny(img2.Ptr, dst.Ptr, 50, 200, 3);

                        Bgr color1 = new Bgr(100, 40, 243);

                        using (MemStorage stor = new MemStorage())
                        {

                            MCvScalar color = new MCvScalar(0, 255, 0);
                            IntPtr lines = CvInvoke.cvHoughLines2(dst.Ptr, stor.Ptr, Emgu.CV.CvEnum.HOUGH_TYPE.CV_HOUGH_PROBABILISTIC, 2, (1 * Math.PI / 2) / 180, 60, 80, 4);
                            Seq<LineSegment2D> segments = new Seq<LineSegment2D>(lines, stor);
                            LineSegment2D[] ar = segments.ToArray();
                            foreach (LineSegment2D line in ar)
                            {
                                // if(line.Length>30)
                                double Angle = Math.Atan2(line.P2.Y - line.P1.Y, line.P2.X - line.P1.X) * 180.0 / Math.PI;

                                // if((Angle <60.0 && Angle>0.0) || (Angle >100.0 && Angle<180.0))

                                Angle *= -1;
                                System.Diagnostics.Debug.WriteLine(Angle);
                                if ((Angle < 60 && Angle > 20) || (Angle > -60 && Angle < -20))
                                    // if ((Angle > -60 && Angle < 0))

                                    CvInvoke.cvLine(img, new System.Drawing.Point(line.P1.X, line.P1.Y), new System.Drawing.Point(line.P2.X, line.P2.Y), color, 3, LINE_TYPE.EIGHT_CONNECTED, 0);


                            }

                        }*/

                        Image<Gray, Byte> img2 = img.Convert<Gray, Byte>();
                        Image<Gray, Byte> dst = new Image<Gray, Byte>(img2.Size);
                        Image<Rgb, Byte> dst2 = new Image<Rgb, Byte>(img2.Size);


                        CvInvoke.cvErode(img2.Ptr, img2.Ptr, (IntPtr)null, 2);

                        //CvInvoke.cvDilate(img2.Ptr, img2.Ptr, (IntPtr)null, 1);

                        CvInvoke.cvCanny(img2.Ptr, dst.Ptr, 50, 200, 3);




                        using (MemStorage stor = new MemStorage())
                        {

                            MCvScalar color = new MCvScalar(0, 255, 0);
                            IntPtr lines = CvInvoke.cvHoughLines2(dst.Ptr, stor.Ptr, Emgu.CV.CvEnum.HOUGH_TYPE.CV_HOUGH_PROBABILISTIC, 2, (1 * Math.PI / 2) / 180, 60, 80, 4);
                            Seq<LineSegment2D> segments = new Seq<LineSegment2D>(lines, stor);
                            LineSegment2D[] ar = segments.ToArray();
                            foreach (LineSegment2D line in ar)
                            {
                                // if(line.Length>30)
                                double Angle = Math.Atan2(line.P2.Y - line.P1.Y, line.P2.X - line.P1.X) * 180.0 / Math.PI;

                                // if((Angle <60.0 && Angle>0.0) || (Angle >100.0 && Angle<180.0))

                                Angle *= -1;
                                // System.Diagnostics.Debug.WriteLine(Angle);
                                if (((Angle < 60 && Angle > 20) || (Angle > -60 && Angle < -20)))
                                {
                                    if (line.P1.Y > 700 && line.P2.Y > 700)
                                    {

                                        CvInvoke.cvLine(img, new System.Drawing.Point(line.P1.X, line.P1.Y), new System.Drawing.Point(line.P2.X, line.P2.Y), color, 3, LINE_TYPE.EIGHT_CONNECTED, 0);
                                        Line ins = new Line();
                                        ins.p1.X = line.P1.X;
                                        ins.p1.Y = line.P1.Y;

                                        ins.p2.X = line.P2.X;
                                        ins.p2.Y = line.P2.Y;

                                        ins.slop = Angle;
                                        myLines.Add(ins);
                                    }
                                }

                            }

                        }
                        List<Contour<System.Drawing.Point>> candidates = new List<Contour<System.Drawing.Point>>();
                        System.Diagnostics.Debug.WriteLine(img.Height);

                        using (MemStorage storage = new MemStorage())
                        {
                            System.Diagnostics.Debug.WriteLine("next frame\n");
                            for (Contour<System.Drawing.Point> contours = dst.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_EXTERNAL, storage); contours != null; contours = contours.HNext)
                            {
                                Bgr color = new Bgr(100, 40, 243);
                                Contour<System.Drawing.Point> currentContour = contours.ApproxPoly(contours.Perimeter * 0.000015, storage);



                                System.Drawing.Point[] pts = currentContour.ToArray();

                                if (pts.Any(a => a.Y < 200) && pts.Any(a => a.Y > 800))
                                {

                                    foreach (var line in myLines)
                                    {
                                        double con = line.p2.Y - line.slop * line.p2.X;

                                        if (pts.Any(a => (line.slop * a.X - a.Y + con > -2) && (line.slop * a.X - a.Y + con < 2)))
                                        {
                                            System.Diagnostics.Debug.WriteLine("door find\n");
                                            foreach (System.Drawing.Point p in pts)
                                            {
                                               // System.Diagnostics.Debug.WriteLine("X: " + p.X + " Y: " + p.Y);
                                              
                                                img[p.Y, p.X] = color;
                                                img[p.Y + 1, p.X + 1] = color;
                                                img[p.Y - 1, p.X - 1] = color;

                                            }

                                            break;
                                        }
                                    }
                                }
                                //System.Diagnostics.Debug.WriteLine("next contour\n");
                            }
                        }







                        camera.Source = ToBitmapSource3(dst);

                      

                        camera.Source = ToBitmapSource3(img);

                    }
                }
            }
        }

        private BitmapSource ToBitmap(ColorFrame frame)
        {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;
            System.Windows.Media.PixelFormat format = PixelFormats.Bgr32;

            byte[] pixels = new byte[width * height * ((PixelFormats.Bgr32.BitsPerPixel + 7) / 8)];

            if (frame.RawColorImageFormat == ColorImageFormat.Bgra)
            {
                frame.CopyRawFrameDataToArray(pixels);
            }
            else
            {
                frame.CopyConvertedFrameDataToArray(pixels, ColorImageFormat.Bgra);
            }

            int stride = width * format.BitsPerPixel / 8;
            BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);
            ScaleTransform scale = new ScaleTransform((1920.0 / bitmap.PixelWidth), (1080.0 / bitmap.PixelHeight));
            TransformedBitmap tbBitmap = new TransformedBitmap(bitmap, scale);
          
        
            return tbBitmap;
        }

        public static Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
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
    }
}
