using System.Diagnostics;
using System.Threading;

namespace Skywalker_TEST
{
    using System;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Kinect.Tools;
    using Skywalker_Vision.Kinect;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    [TestClass]
    public class DepthStreamTester
    {
        [TestMethod]
        public void GetDepthBitmapTest()
        {
            string[] files = Directory.GetFiles("C:\\KStudioRepo", "*.xef");
            using (KStudioClient client = KStudio.CreateClient()) {
                client.ConnectToService();
                using (KStudioPlayback playback = client.CreatePlayback(files[0])) {
                    playback.Start();
                    DepthStream stream = DepthStream.Instance;
                    Assert.IsNotNull(stream);

                    while (playback.State == KStudioPlaybackState.Playing) {
                        System.Threading.Thread.Sleep(500);
                        DFrame frame = (DFrame) stream.GetFrame();
                        if (frame == null) continue;
                        WriteableBitmap bmp = frame.GetBitmap();
                        Assert.IsNotNull(bmp);
                        //System.Diagnostics.////Debug.WriteLine("Bitmap : " + bmp.ToString());
                    }

                    Assert.AreNotEqual(playback.State, KStudioPlaybackState.Error);
                }
            }
        }
        [TestMethod]
        public void GetDepthArrayTest() {
            string[] files = Directory.GetFiles("C:\\KStudioRepo", "*.xef");
            using (KStudioClient client = KStudio.CreateClient()) {
                client.ConnectToService();
                using (KStudioPlayback playback = client.CreatePlayback(files[0])) {
                    playback.Start();
                    DepthStream stream = DepthStream.Instance; 
                    Assert.IsNotNull(stream);
                    
                    while (playback.State == KStudioPlaybackState.Playing)
                    {
                        System.Threading.Thread.Sleep(500);
                        DFrame frame = (DFrame) stream.GetFrame();
                        if (frame != null)
                        {
                            //ushort[] testArr = frame.GetDepthArray();
                            //System.Diagnostics.////Debug.WriteLine("Test Arr length : " + testArr.Length);
                            //Assert.IsNotNull(testArr);
                        }
                        
                    }

                    Assert.AreNotEqual(playback.State, KStudioPlaybackState.Error);
                }
                client.DisconnectFromService();
            }
        }
    }
    [TestClass]
    public class DepthFrame {
        [TestMethod]
        public void DFrameTest_LT_1Sec() {
            string[] files = Directory.GetFiles("C:\\KStudioRepo", "*.xef");

            using (KStudioClient client = KStudio.CreateClient()) {
                client.ConnectToService();
                var broken = false;
                using (KStudioPlayback playback = client.CreatePlayback(files[0])) {
                    playback.Start();
                    Stopwatch time = new Stopwatch();

                    DepthStream stream = DepthStream.Instance;

                    while (playback.State == KStudioPlaybackState.Playing) {
                        time.Start();
                        BaseFrame frame = stream.GetFrame();
                        if (time.ElapsedMilliseconds > (1 * 1000)) {
                            //Console.WriteLine("Elapsed time: {0}, frame: {1}", time.ElapsedMilliseconds, frame);
                            if (frame == null) {
                                broken = true;
                                System.DateTime now = DateTime.Now;
                                //System.Diagnostics.////Debug.WriteLine("Failed at: {0}:{1}:{2}:{3}", now.Hour, now.Minute, now.Second, now.Millisecond);
                            }
                        }
                        Thread.Sleep(100);
                    }

                }

                client.DisconnectFromService();
                Assert.IsFalse(broken, "The frame should not be null after 1 second");
            }
        }
    }
}
