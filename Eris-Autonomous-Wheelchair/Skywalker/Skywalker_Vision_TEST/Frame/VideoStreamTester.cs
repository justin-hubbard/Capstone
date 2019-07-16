using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Skywalker_Vision.Kinect;

namespace Skywalker_TEST
{
    using System;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Tools;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    [TestClass]
    public class VideoStreamTester
    {
        [TestMethod]
        public void GetVideoBitmapTest()
        {
            string[] files = Directory.GetFiles("C:\\KStudioRepo", "*.xef");
            using (KStudioClient client = KStudio.CreateClient()) {
                client.ConnectToService();
                using (KStudioPlayback playback = client.CreatePlayback(files[0])) {
                    playback.Start();
                    VideoStream stream = VideoStream.Instance;
                    Assert.IsNotNull(stream);

                    while (playback.State == KStudioPlaybackState.Playing) {
                        System.Threading.Thread.Sleep(500);
                        VFrame frame = (VFrame) stream.GetFrame();
                        if (frame == null) continue;
                        WriteableBitmap bmp = frame.GetBitmap();
                        Assert.IsNotNull(bmp);
                    }

                    Assert.AreNotEqual(playback.State, KStudioPlaybackState.Error);
                }
            }
        }
    }

    [TestClass]
    public class VideoFrame {
        [TestMethod]
        public void VFrameTest_LT_1Sec() {
            string[] files = Directory.GetFiles("C:\\KStudioRepo", "*.xef");
            Array.Sort(files);
            using (KStudioClient client = KStudio.CreateClient()) {
                client.ConnectToService();
                var broken = false;
                using (KStudioPlayback playback = client.CreatePlayback(files[0])) {
                    //Console.WriteLine("Playing: {0}", files[0]);
                    playback.Start();
                    Stopwatch time = new Stopwatch();

                    VideoStream stream = VideoStream.Instance;

                    while (playback.State == KStudioPlaybackState.Playing) {
                        time.Start();
                        BaseFrame frame = stream.GetFrame();
                        if (time.ElapsedMilliseconds > (1 * 1000)) {
                            if (frame == null) {
                                broken = true;
                                System.DateTime now = DateTime.Now;
                                //System.Diagnostics.////Debug.WriteLine("Failed at: {0}:{1}:{2}:{3}", now.Hour, now.Minute, now.Second, now.Millisecond);
                            }
                        }
                        Thread.Sleep(150);
                    }

                }

                client.DisconnectFromService();
                Assert.IsFalse(broken, "The frame should not be null after 1 second");
            }
        }
    }

    ///
    /// TODO: Add test to make sure VFrame and DFrame do not throw exceptiosn
    /// 
}
