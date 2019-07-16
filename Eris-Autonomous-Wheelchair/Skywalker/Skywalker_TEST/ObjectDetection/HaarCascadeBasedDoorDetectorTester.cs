using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

using System.IO;
using System.Drawing;
using Skywalker.ObjectDetection;

namespace Skywalker_TEST.ObjectDetection
{
    [TestClass]
    public class HaarCascadeBasedDoorDetectorTester
    {
        string curDir = Environment.CurrentDirectory;


        [TestMethod]
        public void NoInputTest()
        {
            HaarCascadeBasedDoorDetector detector = new HaarCascadeBasedDoorDetector();
            detector.HaarCascadeDoorDection();
        }

        [TestMethod]
        public void LoadImageTest()
        {
            HaarCascadeBasedDoorDetector detector = new HaarCascadeBasedDoorDetector();
            string path = curDir + "\\sampleImageDirectory\\0001_0023_0052_0093_0093.jpg";
            detector.LoadFileImage(path);
            Assert.IsNotNull(detector.Getbitmap());
        }
        //test door detector with positive sample which contain a door in the image
        [TestMethod]
        public void HaarCascadePositiveTest()
        {
            double passRate;
            int Counter = 0;
            int Size;
            HaarCascadeBasedDoorDetector detector = new HaarCascadeBasedDoorDetector();
            string baseDir = "\\sampleImageDirectory";
            var fileNames = LoadFileName(baseDir + "\\cropped1.txt");
            Size = fileNames.Length;
            for (int i = 0; i < Size; i++)
            {
                detector.LoadFileImage(curDir + baseDir + "\\" + fileNames[i]);
                detector.HaarCascadeDoorDection();
                if (detector.HasDoor())
                    Counter++;
                //Assert.IsTrue(detector.HasDoor(),"Fail at " + i + ",name is " + fileNames[i]);
            }
            passRate = (double)Counter / fileNames.Length;
            Assert.IsTrue(passRate > 0.9, "Pass Rate is " + passRate + ".");
        }

        [TestMethod]
        public void HaarCascadeNegativeTest()
        {
            double passRate;
            int Counter = 0;
            int Size;
            HaarCascadeBasedDoorDetector detector = new HaarCascadeBasedDoorDetector();
            string baseDir = "\\negativeImageDirectory";
            var fileNames = LoadFileName(baseDir + "\\negtiveSampleFileNames.txt");
            Size = fileNames.Length;
            for (int i = 0; i < Size; i++)
            {
                detector.LoadFileImage(curDir + baseDir + "\\" + fileNames[i]);
                detector.HaarCascadeDoorDection();
                if (!detector.HasDoor())
                    Counter++;
                //Assert.IsTrue(!detector.HasDoor(),"Fail at " + i + ",name is " + fileNames[i]);
            }
            passRate = (double)Counter / fileNames.Length;
            Assert.IsTrue(passRate > 0.7, "Pass Rate is " + passRate + ".");
        }

        [TestMethod]
        public void HaarCascadeObjectLocationTest()
        {
            double passRate;
            int Counter = 0;
            int detectX, detectY, originalX, originalY;
            int Size;
            int error = 20;
            HaarCascadeBasedDoorDetector detector = new HaarCascadeBasedDoorDetector();
            string baseDir = "\\sampleImageDirectory";
            var fileNames = LoadFileName(baseDir + "\\cropped1.txt");
            var locationList = LoadLocation(baseDir + "\\cropped1.txt");
            Size = fileNames.Length;
            for (int i = 0; i < Size; i++)
            {
                detector.LoadFileImage(curDir + baseDir + "\\" + fileNames[i]);
                detector.HaarCascadeDoorDection();
                if (detector.HasDoor())
                {
                    detectX = detector.GetX();
                    detectY = detector.GetY();
                    originalX = locationList[i].Item1;
                    originalY = locationList[i].Item2;
                    if (detectX > originalX - error && detectX < originalX + error
                        && detectY > originalY - error && detectY < originalY + error)
                    {
                        Counter++;
                    }
                }
            }
            passRate = (double)Counter / fileNames.Length;
            Assert.IsTrue(passRate > 0.7, "Pass Rate is " + passRate + ".");
        }

        public string[] LoadFileName(string fileName)
        {
            string line;
            char[] delimiterChars = { ' ' };
            List<string> fileNameList = new List<string>();
            string infoFile = curDir + fileName;
            using (StreamReader sr = new StreamReader(infoFile))
            {
                while (sr.Peek() >= 0)
                {
                    line = sr.ReadLine();
                    string[] tokens = line.Split(delimiterChars);
                    fileNameList.Add(tokens[0]);
                }
            }
            return fileNameList.ToArray();
        }

        public List<Tuple<int, int>> LoadLocation(string fileName)
        {
            string line;
            char[] delimiterChars = { ' ' };
            List<Tuple<int, int>> locationList = new List<Tuple<int, int>>();
            string infoFile = curDir + fileName;
            using (StreamReader sr = new StreamReader(infoFile))
            {
                while (sr.Peek() >= 0)
                {
                    line = sr.ReadLine();
                    string[] tokens = line.Split(delimiterChars);
                    int x, y;
                    Int32.TryParse(tokens[2], out x);
                    Int32.TryParse(tokens[3], out y);
                    locationList.Add(new Tuple<int, int>(x, y));
                }
            }
            return locationList;
        }
    }
}
