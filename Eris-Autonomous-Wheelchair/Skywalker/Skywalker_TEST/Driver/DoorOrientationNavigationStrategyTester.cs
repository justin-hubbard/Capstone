using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Skywalker.Driver;
using Skywalker.ObjectDetection;
using Skywalker_TEST.ObjectDetection;

namespace Skywalker_TEST.Driver
{
    [TestClass]
    public class DoorOrientationNavigationStrategyTester
    {
        [TestMethod]
        public void TestDoorDetectorIsSavedInProperty()
        {
            MockDoorDetector doorDetector = new MockDoorDetector(null);
            DoorOrientationNavigationStrategy doorOrientor = new DoorOrientationNavigationStrategy(doorDetector);

            Assert.ReferenceEquals(doorDetector, doorOrientor.DoorDetector);
        }

        [TestMethod]
        public void TestNearestTargetDoorIsSelected()
        {
            DetectedDoor door1 = new DetectedDoor(new System.Drawing.Rectangle(), DetectedDoor.DetectionConfidence.HIGH, 0, 50);
            DetectedDoor door2 = new DetectedDoor(new System.Drawing.Rectangle(), DetectedDoor.DetectionConfidence.HIGH, 0, 25);
            List<DetectedDoor> doors = new List<DetectedDoor>();
            doors.Add(door1);
            doors.Add(door2);

            DoorOrientationNavigationStrategy strategy = new DoorOrientationNavigationStrategy(new MockDoorDetector(null));
            PrivateObject privateAccessor = new PrivateObject(strategy);

            DetectedDoor targetDoor = (DetectedDoor)privateAccessor.Invoke("GetTargetDoor", doors);

            // door2 has the smallest estimated distance therefore it should be selected as the target door
            Assert.AreEqual<DetectedDoor>(door2, targetDoor);
        }
    }
}
