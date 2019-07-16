using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Skywalker.ObjectDetection
{
    public class DetectedDoor
    {
        public enum DetectionConfidence
        {
            LOW,
            MEDIUM,
            HIGH
        }

        public enum DetectMethod
        {
            COLOR,
            HAAR,
            DEPTH,
            INFRARED,
            COMPOSITE
        }

        // How confident we are that this is actually a door
        public DetectionConfidence Confidence
        {
            get;
            set;
        }

        // The angle of the detected door relative to the input image
        // in radians. 0 radians represents in front of the wheelchair
        // positive PI/4 means to the right and negative PI/4 means to
        // the left of the wheelchair
        public double RelativeAngle
        {
            get;
            private set;
        }

        // The distance of the door relative to the current wheelchair
        // position in centimeters
        public double RelativeDistance
        {
            get;
            private set;
        }

        public Rectangle BoundingBox
        {
            get;
            private set;
        }

        public Size OriginalImageSize
        {
            get;
            private set;
        }

        public DetectMethod Method
        {
            get;
            private set;
        }

        public DetectedDoor(Rectangle boundingBox, Size imageSize, DetectionConfidence confidence, double relativeAngle, double relativeDistance, DetectMethod detectionMethod)
        {
            this.BoundingBox = boundingBox;
            this.OriginalImageSize = imageSize;
            this.Confidence = confidence;
            this.RelativeAngle = relativeAngle;
            this.RelativeDistance = relativeDistance;
            this.Method = detectionMethod;
        }

        public override bool Equals(object obj)
        {
            if (!obj.GetType().Equals(this.GetType()))
            {
                return false;
            }

            DetectedDoor other = (DetectedDoor)obj;
            return this.BoundingBox == other.BoundingBox
                && this.Confidence == other.Confidence
                && this.RelativeAngle == other.RelativeAngle
                && this.RelativeDistance == other.RelativeDistance;
        }
    }
}
