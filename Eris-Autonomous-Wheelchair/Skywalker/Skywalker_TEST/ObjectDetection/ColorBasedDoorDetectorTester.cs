using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Drawing;

using Skywalker.ObjectDetection;

using Emgu.CV;
using Emgu.CV.Structure;

namespace Skywalker_TEST.ObjectDetection
{
    [TestClass]
    public class ColorBasedDoorDetectorTester
    {
        [TestMethod]
        public void MinAndMaxColorMatchInitialTest()
        {
            Hsv minColor = new Hsv(0, 150, 255);
            Hsv maxColor = new Hsv(33, 66, 99);

            ColorBasedDoorDetector doorDetector = new ColorBasedDoorDetector(minColor, maxColor);

            Assert.AreEqual<Hsv>(minColor, doorDetector.MinColor);
            Assert.AreEqual<Hsv>(maxColor, doorDetector.MaxColor);
        }

        [TestMethod]
        public void TestWellLitPositiveImageDetectsDoor()
        {
            ColorBasedDoorDetector doorDetector = CreateBasicDoorDetector();
            Image<Bgr, Byte> image = LoadImage<Bgr, byte>("door3.JPG");
            List<DetectedDoor> doors = doorDetector.GetDoors(image);

            List<DetectedDoor> expectedDoors = new List<DetectedDoor>();
            expectedDoors.Add(new DetectedDoor(new Rectangle(), DetectedDoor.DetectionConfidence.HIGH, 0.323, 5.433));

            Assert.AreEqual<List<DetectedDoor>>(doors, expectedDoors);
        }

        [TestMethod]
        public void TestNegativeImageDoesNotDetectDoor()
        {
            ColorBasedDoorDetector doorDetector = CreateBasicDoorDetector();
            Image<Bgr, Byte> image = new Image<Bgr, byte>(new Size(500, 500));
            List<DetectedDoor> doors = doorDetector.GetDoors(image);

            List<DetectedDoor> expectedDoors = new List<DetectedDoor>();

            Assert.AreEqual<List<DetectedDoor>>(doors, expectedDoors);
        }

        [TestMethod]
        public void TestFilterColorReturnsCorrectImage()
        {
            ColorBasedDoorDetector detector = CreateBasicDoorDetector();
            PrivateObject privateAccessor = new PrivateObject(detector);
            Image<Bgr, Byte> inputImage = LoadImage<Bgr, byte>("door3.JPG");
            Image<Gray, Byte> expectedImage = LoadImage<Gray, byte>("door3_filtered.JPG");

            Image<Gray, Byte> filtered = (Image<Gray, Byte>)privateAccessor.Invoke("FilterColor", inputImage);

            Assert.AreEqual<Image<Gray, Byte>>(expectedImage, filtered);
        }

        [TestMethod]
        public void TestSimplifyImageReturnsCorrectImage()
        {
            ColorBasedDoorDetector detector = CreateBasicDoorDetector();
            PrivateObject privateAccessor = new PrivateObject(detector);
            Image<Gray, Byte> inputImage = LoadImage<Gray, byte>("door3_filtered.JPG");
            Image<Gray, Byte> expectedImage = LoadImage<Gray, byte>("door3_simplified.JPG");

            Image<Gray, Byte> filtered = (Image<Gray, Byte>)privateAccessor.Invoke("SimplifyImage", inputImage, 10);

            Assert.AreEqual<Image<Gray, Byte>>(expectedImage, filtered);
        }

        [TestMethod]
        public void TestExtractDoorsReturnsCorrectDoors()
        {
            ColorBasedDoorDetector detector = CreateBasicDoorDetector();
            PrivateObject privateAccessor = new PrivateObject(detector);
            Image<Gray, Byte> inputImage = LoadImage<Gray, Byte>("box.JPG");

            List<DetectedDoor> doors = (List<DetectedDoor>)privateAccessor.Invoke("ExtractDoors", inputImage);
            List<DetectedDoor> expectedDoors = new List<DetectedDoor>();
            expectedDoors.Add(new DetectedDoor(new Rectangle(), DetectedDoor.DetectionConfidence.HIGH, 0, 10));

            Assert.AreEqual<List<DetectedDoor>>(doors, expectedDoors);
        }

        [TestMethod]
        public void TestDoorFromContourReturnsCorrectContour()
        {
            ColorBasedDoorDetector detector = CreateBasicDoorDetector();
            PrivateObject privateAccessor = new PrivateObject(detector);
            Size imageSize = new Size(100, 100);
            Rectangle boundingRect = new Rectangle(10, 10, 80, 80);
            Contour<Point> inputContour = new MockContour<Point>(boundingRect);

            List<DetectedDoor> doors = (List<DetectedDoor>)privateAccessor.Invoke("DoorFromContour", inputContour);
            List<DetectedDoor> expectedDoors = new List<DetectedDoor>();
            expectedDoors.Add(new DetectedDoor(new Rectangle(), DetectedDoor.DetectionConfidence.HIGH, 0, 10));

            Assert.AreEqual<List<DetectedDoor>>(doors, expectedDoors);
        }

        private Image<TColor, TDepth> LoadImage<TColor, TDepth>(string filename)
            where TColor : struct, Emgu.CV.IColor
            where TDepth : new()
        {
            return new Image<TColor, TDepth>("ObjectDetection\\Images\\" + filename);
        }

        private ColorBasedDoorDetector CreateBasicDoorDetector()
        {
            return new ColorBasedDoorDetector(new Hsv(10, 50, 76), new Hsv(179, 255, 255));
        }
    }
}
