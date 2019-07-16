using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;

using Skywalker.ObjectDetection;
using Skywalker.Driver;
using System.Threading;
using System.Windows;

namespace Skywalker_TEST.ObjectDetection
{
    [TestClass]
    public class DoorNavigationIntegrationTester
    {
        [TestMethod]
        public void TestDoorNavigatorNavigatesTowardsDoor()
        {
            Bitmap image = new Bitmap("ObjectDetection\\Images\\door3.JPG");
            MockFrame frame = new MockFrame(image);
            MockImageStream stream = new MockImageStream(frame);

            ColorBasedDoorDetector doorDetector = new ColorBasedDoorDetector();
            DoorOrientationNavigationStrategy navigator = new DoorOrientationNavigationStrategy(doorDetector);
            doorDetector.RunAsync(stream);

            // Sleep for 1 second to ensure that the door detector has time to recognize the door
            Thread.Sleep(1000);

            Vector navigatorDirection = navigator.GetDirection();
            Vector expectedDirection = new Vector(0.23776, 0.7623);
            Assert.AreEqual<Vector>(expectedDirection, navigatorDirection);

            doorDetector.CancelAsync();
        }
    }
}
