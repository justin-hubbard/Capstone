using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Skywalker.ObjectDetection;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Skywalker_TEST.ObjectDetection
{
    [TestClass]
    public class CompositeDoorDetectorTester
    {
        [TestMethod]
        public void TestCollectDoorsCollectsContainedDoors()
        {
            Point center = new Point(50, 50);
            Rectangle outerRect = new Rectangle(center, new Size(30, 30));
            Rectangle innerRect = new Rectangle(center, new Size(10, 10));

            DetectedDoor outerDoor = new DetectedDoor(outerRect, DetectedDoor.DetectionConfidence.LOW, 0, 0);
            DetectedDoor innerDoor = new DetectedDoor(innerRect, DetectedDoor.DetectionConfidence.LOW, 0, 0);
            
            List<DetectedDoor> inputDoors = new List<DetectedDoor>();
            inputDoors.Add(outerDoor);
            inputDoors.Add(innerDoor);

            DetectedDoor expectedDoor = new DetectedDoor(outerRect, DetectedDoor.DetectionConfidence.HIGH, 0, 0);
            List<DetectedDoor> expectedDoors = new List<DetectedDoor>();
            expectedDoors.Add(expectedDoor);

            CompositeDoorDetector doorDetector = new CompositeDoorDetector(null);
            PrivateObject privateAccessor = new PrivateObject(doorDetector);

            List<DetectedDoor> collectedDoors = (List<DetectedDoor>)privateAccessor.Invoke("CollectDoors", inputDoors);
            CollectionAssert.AreEqual(collectedDoors, expectedDoors);
        }

        [TestMethod]
        public void TestCollectDoorsDoesntCollectSeparateDoors()
        {
            Rectangle rect1 = new Rectangle(new Point(0, 0), new Size(10, 10));
            Rectangle rect2 = new Rectangle(new Point(100, 100), new Size(10, 10));

            DetectedDoor door1 = new DetectedDoor(rect1, DetectedDoor.DetectionConfidence.LOW, 0, 0);
            DetectedDoor door2 = new DetectedDoor(rect2, DetectedDoor.DetectionConfidence.LOW, 0, 0);

            List<DetectedDoor> inputDoors = new List<DetectedDoor>();
            inputDoors.Add(door1);
            inputDoors.Add(door2);

            CompositeDoorDetector doorDetector = new CompositeDoorDetector(null);
            PrivateObject privateAccessor = new PrivateObject(doorDetector);

            List<DetectedDoor> collectedDoors = (List<DetectedDoor>)privateAccessor.Invoke("CollectDoors", inputDoors);
            CollectionAssert.AreEqual(collectedDoors, inputDoors);
        }

        [TestMethod]
        public void TestOverallFunctionalityOfCompositeDoorDetectorWithOverlap()
        {
            DetectedDoor door1 = new DetectedDoor(new Rectangle(0, 0, 100, 100), DetectedDoor.DetectionConfidence.LOW, 0, 0);
            DetectedDoor door2 = new DetectedDoor(new Rectangle(10, 10, 80, 80), DetectedDoor.DetectionConfidence.LOW, 0, 0);

            List<DetectedDoor> mock1Doors = new List<DetectedDoor>();
            mock1Doors.Add(door1);

            List<DetectedDoor> mock2Doors = new List<DetectedDoor>();
            mock2Doors.Add(door2);

            MockDoorDetector mock1 = new MockDoorDetector(mock1Doors);
            MockDoorDetector mock2 = new MockDoorDetector(mock2Doors);
            List<IDoorDetector> detectors = new List<IDoorDetector>();
            detectors.Add(mock1);
            detectors.Add(mock2);

            DetectedDoor expectedDoor = new DetectedDoor(new Rectangle(0, 0, 100, 100), DetectedDoor.DetectionConfidence.HIGH, 0, 0);
            List<DetectedDoor> expectedDoors = new List<DetectedDoor>();
            expectedDoors.Add(expectedDoor);

            CompositeDoorDetector composite = new CompositeDoorDetector(detectors);
            Image<Bgr, Byte> image = new Image<Bgr, byte>(new Size(300, 300));
            List<DetectedDoor> actualDoors = composite.GetDoors(image);

            CollectionAssert.AreEqual(expectedDoors, actualDoors);
        }
    }
}
