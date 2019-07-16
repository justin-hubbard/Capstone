using System;
using System.Windows.Controls;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Skywalker.ObjectDetection;
using Skywalker.UserInterface;

namespace Skywalker_TEST.UserInterface
{
    [TestClass]
    public class DoorsTabTester
    {
        [TestMethod]
        public void TestDrawDoorsAddsNoRectanglesForNoDoors()
        {
            Canvas canvas = new Canvas();
            List<DetectedDoor> doors = new List<DetectedDoor>();

            Doors doorsTab = new Doors(null);
            PrivateObject privateAccessor = new PrivateObject(doorsTab);
            privateAccessor.Invoke("DrawDoors", doors, canvas);

            // There were no doors given so there should be no children of the canvas
            Assert.AreEqual<int>(0, canvas.Children.Count);
        }

        [TestMethod]
        public void TestDrawDoorsAddsRectangleForOneDoor()
        {
            System.Drawing.Rectangle doorBoundingBox = new System.Drawing.Rectangle(20, 50, 300, 500);
            DetectedDoor detectedDoor = new DetectedDoor(doorBoundingBox,new System.Drawing.Size(0,0), DetectedDoor.DetectionConfidence.LOW, 0, 0,"");

            Canvas canvas = new Canvas();
            List<DetectedDoor> doors = new List<DetectedDoor>();
            doors.Add(detectedDoor);

            Doors doorsTab = new Doors(null);
            PrivateObject privateAccessor = new PrivateObject(doorsTab);
            privateAccessor.Invoke("DrawDoors", doors, canvas);

            // There was one door given so there should be one child in the canvas
            Assert.AreEqual<int>(1, canvas.Children.Count);

            // That one child rectangle should have the correct bounding box
            System.Windows.Shapes.Rectangle doorShape = (System.Windows.Shapes.Rectangle)canvas.Children[0];

            // Compare the two bounding boxes, they should be the same
            Assert.AreEqual<double>(doorBoundingBox.Width, doorShape.Width);
            Assert.AreEqual<double>(doorBoundingBox.Height, doorShape.Height);
            Assert.AreEqual<double>(doorBoundingBox.Left, Canvas.GetLeft(doorShape));
            Assert.AreEqual<double>(doorBoundingBox.Top, Canvas.GetTop(doorShape));
        }
    }
}
