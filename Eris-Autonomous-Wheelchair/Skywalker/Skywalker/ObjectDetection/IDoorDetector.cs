using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using Emgu.CV;
using Emgu.CV.Structure;

using Skywalker_Vision.Kinect;

namespace Skywalker.ObjectDetection
{
    public delegate void DoorDetectedEvent(object sender, DoorDetectedEventArgs eventArgs);
    
    public interface IDoorDetector
    {
        event DoorDetectedEvent OnDoorDetected;

        // Whether or not there is a current processing operation
        // running on an image stream
        bool IsRunning { get; }

        // Cancels the operation created by RunAsync.
        // Precondition: IsRunning is true
        // Postcondition: IsRunning is false
        void CancelAsync();

        // Begin asynchronously processing frames from the
        // given image stream. The door detector should only be running with one stream
        // at a time. If it is currently running, IsRunning will be set to true.
        // Call CancelAsync() to cancel this process.
        // When the door detector is running, it will fire OnDoorDetected events
        // Precondition: IsRunning is false
        // Postcondition: IsRunning is true
        void RunAsync(IImageStream imageStream);

        // Detect the doors in the given image and return them as a list of DetectedDoors
        List<DetectedDoor> GetDoors(object data);

    }
}
