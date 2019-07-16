using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skywalker.ObjectDetection
{
    public class DoorDetectedEventArgs : EventArgs
    {
        public List<DetectedDoor> DetectedDoors
        {
            get;
            private set;
        }

        public DoorDetectedEventArgs(List<DetectedDoor> detectedDoors, DetectedDoor.DetectMethod method)
        {
            this.DetectedDoors = detectedDoors;
            this.DetetMethod = method;
        }

        public DetectedDoor.DetectMethod DetetMethod
        {
            get;
            private set;
        }
    }
}
