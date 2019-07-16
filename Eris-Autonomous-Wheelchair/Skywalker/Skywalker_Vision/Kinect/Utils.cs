using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Microsoft.Kinect;

namespace Skywalker_Vision.Kinect {
    public static class Utils {
        public static bool IsKinectConnected() {
            bool connected = false;
            try {
                KinectSensor sensor = KinectSensor.GetDefault();
                sensor.Open();
                Thread.Sleep(500);
                connected = sensor.IsAvailable;
                sensor.Close();
            } catch (Exception e) {
                connected = false;
            }
            if (connected) {
                //Console.WriteLine("Kinect Connected");
            }
            return connected;
        }

        public static BitmapSource IImageToBitmapSource(IImage img) {
            using (System.Drawing.Bitmap src = img.Bitmap) {
                IntPtr ptr = src.GetHbitmap();
                BitmapSource srcBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                //delete ptr?

                return srcBitmap;
            }
        }

        public static Bitmap WriteableBitmapToBitmap(WriteableBitmap wbmp) {
            Bitmap bmp;
            using (MemoryStream outStream = new MemoryStream()) {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create((BitmapSource)wbmp));
                enc.Save(outStream);
                bmp = new System.Drawing.Bitmap(outStream);
            }
            return bmp;
        }

        public static BitmapSource BitmapToBitmapSource(Bitmap bmp)
        {
            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap
            (
                bmp.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );
            return bitmapSource;
        }
    }
}
