using System;
using System.IO;
using System.Linq;
using System.Xaml;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Media.Imaging;
using Skywalker.ObjectDetection;
using Skywalker_Vision.Kinect;


namespace Skywalker_Vision_TEST.Obstruction
{
    /*[TestClass()]
    public class Obstacle {
        private bool expectObstacle = false;
        private bool objectDetected = false;
        [TestMethod()]
        public void BasicObstacle() {
            bool passing = true;
            //Does analysis then make sure the yes/no decision is correct
            string[] bmps = Directory.GetFiles("C:\\KSTudioRepo", "*_Obstacle.png");
            TestDStream testd = TestDStream.Instance;
            bool result = false;

            //crappy sort
            Array.Sort(bmps);

            ObstacleDetector detector = new ObstacleDetector(testd, false);
            detector.ObjectDetected += ObjectEventListener;

            for (int i = 0; i < bmps.Length; i++)
            {
                objectDetected = false;
                expectObstacle = false;
                System.Diagnostics.////Debug.WriteLine("Testing Frame: " + bmps[i]);
                Stream pngSource = new FileStream(bmps[i], FileMode.Open, FileAccess.Read, FileShare.Read);
                PngBitmapDecoder decoded = new PngBitmapDecoder(pngSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                WriteableBitmap bmp = new WriteableBitmap(decoded.Frames[0]);
                testd.SetBitmap(bmp);
                /* Create Object object */
                /* Pass instance of testd */
                /* Check if obstacle detected */
                /* use filename to see if it should pass or not */

                //Checks to see if it is good

                /*if (bmps[i].IndexOf("True") > 0) {
                    expectObstacle = true;
                } else {
                    expectObstacle = false;
                }
                System.Diagnostics.////Debug.WriteLine("    Expecting Obstacle: " + expectObstacle.ToString());

                detector.Detect((DFrame)testd.GetFrame());

                Thread.Sleep(100);
                System.Diagnostics.////Debug.WriteLine("    Obstacle Detected: " + objectDetected.ToString());

                if (expectObstacle != objectDetected) {
                    passing = false;
                }
                
                Thread.Sleep(100);

            }
            Assert.IsTrue(passing);
        }

        private void ObjectEventListener(object sender, ObstacleDetectedArgs e) {
            objectDetected = true;
        }
    }*/
}
