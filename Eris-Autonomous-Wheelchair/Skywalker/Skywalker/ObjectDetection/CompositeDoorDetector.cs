using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;

using Emgu.CV;
using Emgu.CV.Structure;

using Skywalker_Vision.Kinect;

namespace Skywalker.ObjectDetection
{
    public class CompositeDoorDetector : IDoorDetector
    {
        public event DoorDetectedEvent OnDoorDetected;

        private Thread processingThread;
        private IImageStream imageStream;

        // If an image stream is provided to the door detector, UPDATE_INTERVAL represents how often
        // that stream will be used to detect doors. The unit is milliseconds
        private const int UPDATE_INTERVAL = 1000;

        public List<IDoorDetector> DoorDetectors
        {
            get;
            private set;
        }

        public bool IsRunning
        {
            get;
            private set;
        }

        public CompositeDoorDetector(List<IDoorDetector> doorDetectors)
        {
            this.DoorDetectors = doorDetectors;
        }

        public List<DetectedDoor> GetDoors(object imageData)
        {
            Image<Bgr,Byte> inputImage = imageData as Image<Bgr,byte>;
            if (inputImage == null)
                return null;

            List<DetectedDoor> detectedDoors = new List<DetectedDoor>();
            List<Thread> threads = new List<Thread>();

            foreach (IDoorDetector doorDetector in this.DoorDetectors)
            {
                Thread thread = new Thread((object obj) =>
                {
                    List<DetectedDoor> doors = doorDetector.GetDoors(inputImage);
                    lock (detectedDoors)
                    {
                        detectedDoors.AddRange(doors);
                    }
                });

                thread.Start();
                threads.Add(thread);
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            return CollectDoors(detectedDoors);

        }

        public void RunAsync(IImageStream imageStream)
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("This ColorBasedDoorDetector is already running");
            }

            IsRunning = true;
            processingThread = new Thread(ProcessingThread_DoWork);
            processingThread.Start();
        }

        public void CancelAsync()
        {
            if (!IsRunning)
            {
                throw new InvalidOperationException("This ColorBasedDoorDetector is not running");
            }

            processingThread.Abort();
            processingThread = null;
            imageStream = null;
            IsRunning = false;
        }

        private void ProcessingThread_DoWork()
        {
            while (true)
            {
                Bitmap bitmap = imageStream.GetFrame().GetBMP();
                Image<Bgr, Byte> image = new Image<Bgr, byte>(bitmap);

                List<DetectedDoor> doors = GetDoors(image);
                DoorDetectedEventArgs args = new DoorDetectedEventArgs(doors,DetectedDoor.DetectMethod.COMPOSITE);
                if (OnDoorDetected != null)
                {
                    OnDoorDetected(this, args);
                }

                Thread.Sleep(UPDATE_INTERVAL);
            }
        }

        private List<DetectedDoor> CollectDoors(List<DetectedDoor> doors)
        {
            List<DetectedDoor> doorsToRemove = new List<DetectedDoor>();

            for (int i = 0; i < doors.Count(); ++i)
            {
                DetectedDoor door1 = doors[i];

                for (int j = i + 1; j < doors.Count(); ++j)
                {
                    DetectedDoor door2 = doors[j];

                    if (door1.BoundingBox.Contains(door2.BoundingBox))
                    {
                        door1.Confidence = DetectedDoor.DetectionConfidence.HIGH;
                        doorsToRemove.Add(door2);
                    }
                    else if (door2.BoundingBox.Contains(door1.BoundingBox))
                    {
                        door2.Confidence = DetectedDoor.DetectionConfidence.HIGH;
                        doorsToRemove.Add(door1);
                    }
                }
            }
            
            foreach (DetectedDoor door in doorsToRemove)
            {
                doors.Remove(door);
            }

            return doors;
        }
    }
}
