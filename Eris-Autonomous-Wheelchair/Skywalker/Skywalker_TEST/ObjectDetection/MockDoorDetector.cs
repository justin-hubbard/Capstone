using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Skywalker.ObjectDetection;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Skywalker_TEST.ObjectDetection
{
    public class MockDoorDetector : IDoorDetector
    {
        public event DoorDetectedEvent OnDoorDetected;

        public List<DetectedDoor> DetectedDoors
        {
            get;
            private set;
        }

        public MockDoorDetector(List<DetectedDoor> doorsToDetect)
        {
            this.DetectedDoors = doorsToDetect;
        }

        public List<DetectedDoor> GetDoors(Image<Bgr, Byte> inputImage)
        {
            return this.DetectedDoors;
        }

        public void RunAsync(Skywalker_Vision.Kinect.IImageStream stream) {}
        public void CancelAsync() { }

        public List<DetectedDoor> GetDoors(object data)
        {
            throw new NotImplementedException();
        }

        public bool IsRunning { get; private set; }
    }
}
