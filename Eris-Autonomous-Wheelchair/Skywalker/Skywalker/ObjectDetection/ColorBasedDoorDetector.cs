using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;

using Emgu;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

using Skywalker_Vision.Kinect;

namespace Skywalker.ObjectDetection
{
    public class ColorBasedDoorDetector : IDoorDetector
    {
        // In order to be classified as a door, each contour must have 
        //  contour_area > image_area * MINIMUM_DOOR_AREA_FACTOR
        // where contour_area is the area of the contour and image_area
        // is the total area of the processed image frame
        private const double MINIMUM_DOOR_AREA_FACTOR = 0.01;

        // The equation true_distance = DISTANCE_SCALE_COEFFICIENT_FACTOR * e ^ (DISTANCE_SCALE_EXPONENT_FACTOR * x)
        // is used to determine the true distance of the door, where x is a number in the range (0, 1] representing the
        // percentage the area of the door is compared to the full area of the image
        // In the range, 0 represents infinitely far away and 1 represents a near-zero proximity
        private const double DISTANCE_SCALE_COEFFICIENT_FACTOR = 43.861;
        private const double DISTANCE_SCALE_EXPONENT_FACTOR = 1.9314;

        // If an image stream is provided to the door detector, UPDATE_INTERVAL represents how often
        // that stream will be used to detect doors. The unit is milliseconds
        private const int UPDATE_INTERVAL = 1000;

        public event DoorDetectedEvent OnDoorDetected;

        public Hsv MinColor
        {
            get;
            private set;
        }

        public Hsv MaxColor
        {
            get;
            private set;
        }

        public bool IsRunning
        {
            get;
            private set;
        }

        private IImageStream imageStream;
        private Thread processingThread;

        public ColorBasedDoorDetector(Hsv minColor, Hsv maxColor)
        {
            this.MinColor = minColor;
            this.MaxColor = maxColor;
        }

        // A ColorBasedDoorDetector calibrated for the doors in EME
        public ColorBasedDoorDetector()
            : this(new Hsv(0, 31, 48), new Hsv(32, 255, 178))
        { }

        // Begin asynchronously processing frames from the
        // given image stream. The door detector should only be running with one stream
        // at a time. If it is currently running, IsRunning will be set to true.
        // When the door detector is running, it will fire OnDoorDetected events
        // Call CancelAsync() to cancel this process.
        // Precondition: IsRunning is false
        // Postcondition: IsRunning is true
        public void RunAsync(IImageStream imageStream)
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("This ColorBasedDoorDetector is already running");
            }

            IsRunning = true;
            this.imageStream = imageStream;
            processingThread = new Thread(ProcessingThread_DoWork);
            processingThread.Start();
        }

        // Cancels the operation created by RunAsync.
        // Precondition: IsRunning is true
        // Postcondition: IsRunning is false
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
                BaseFrame frame = imageStream.GetFrame();
                try
                {
                    if (frame != null)
                    {
                        Bitmap bitmap = frame.GetBMP();
                        Image<Bgr, Byte> image = new Image<Bgr, byte>(bitmap);

                        List<DetectedDoor> doors = GetDoors(image);
                        DoorDetectedEventArgs args = new DoorDetectedEventArgs(doors, DetectedDoor.DetectMethod.COLOR);
                        if (OnDoorDetected != null)
                        {
                            OnDoorDetected(this, args);
                        }
                    }
                    Thread.Sleep(UPDATE_INTERVAL);
                }
                catch(Exception e)
                {
                    //
                }
            }
        }

        public List<DetectedDoor> GetDoors(object data)
        {
            Image<Bgr, Byte> img = data as Image<Bgr, Byte>;
            if (img == null)
                return null;

            // Copy the image to avoid threading issues
            img = img.Copy();
            Image<Bgr, Byte> smoothed = img.SmoothGaussian(15);
            Image<Gray, Byte> threshold = FilterColor(smoothed);
            threshold = SimplifyImage(threshold, 100);

            return ExtractDoors(threshold);
        }

        private Image<Gray, Byte> FilterColor(Image<Bgr, Byte> img)
        {
            return img.Convert<Hsv, Byte>().InRange(MinColor, MaxColor);
        }

        private Image<Gray, Byte> SimplifyImage(Image<Gray, Byte> img, int iterations)
        {
            return img.Erode(iterations).Dilate(iterations);
        }

        private List<DetectedDoor> ExtractDoors(Image<Gray, Byte> img)
        {
            List<DetectedDoor> doors = new List<DetectedDoor>();

            using (MemStorage stor = new MemStorage())
            {
                double minimum_contour_area = img.Width * img.Height * MINIMUM_DOOR_AREA_FACTOR;

                Contour<Point> contours = img.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_TC89_L1, RETR_TYPE.CV_RETR_EXTERNAL);
                for (; contours != null; contours = contours.HNext)
                {
                    Contour<Point> contour = contours.ApproxPoly(contours.Perimeter * 0.05, stor);

                    if (contour.Area < minimum_contour_area)
                    {
                        continue;
                    }
                    doors.Add(DoorFromContour(contour, img.Size));
                }
            }

            return doors;
        }

        private DetectedDoor DoorFromContour(Contour<Point> contour, Size imageSize)
        {
            Rectangle boundingBox = contour.BoundingRectangle;
            int centerX = (boundingBox.Left + boundingBox.Right) / 2;
            int centerY = (boundingBox.Bottom + boundingBox.Top) / 2;

            // Get the angle relative to the bottom center of the image
            int centerXOffset = centerX - (imageSize.Width / 2);
            double angle = -Math.Atan2(-centerY, centerXOffset) - Math.PI / 2;
            double distance = GetEstimatedDistance(contour, imageSize);
            DetectedDoor.DetectionConfidence confidence = GetConfidence(boundingBox, imageSize);

            return new DetectedDoor(boundingBox, imageSize, confidence, angle, distance, DetectedDoor.DetectMethod.COLOR);
        }

        private double GetEstimatedDistance(Contour<Point> contour, Size imageSize)
        {
            double contourArea = contour.BoundingRectangle.Width * contour.BoundingRectangle.Height;
            double imageArea = imageSize.Width * imageSize.Height;

            // Create a range (0, 1] representing how far away the contour is
            // 1 means infinitely far away, 0 means a near-zero proximity
            // Scale that range by DISTANCE_SCALE_FACTOR to return the actual
            // distance
            double areaPercentage = 1 - contourArea / imageArea;
            return DISTANCE_SCALE_COEFFICIENT_FACTOR * Math.Pow(Math.E, DISTANCE_SCALE_EXPONENT_FACTOR * areaPercentage);
        }

        private DetectedDoor.DetectionConfidence GetConfidence(Rectangle boundingBox, Size imageSize)
        {
            // Confidence is created based on a score from 0-9
            // 0-2 is LOW
            // 3-6 is MEDIUM
            // 7-9 is HIGH
            // The score is created by averaging the output of different tests
            int points = 0;

            int numberOfTests = 2;
            points += GetBoundingBoxPositionScore(boundingBox, imageSize);
            points += GetBoundingBoxAspectRatioScore(boundingBox);

            int score = points / numberOfTests;
            if (score >= 7)
            {
                return DetectedDoor.DetectionConfidence.HIGH;
            }
            else if (score >= 3)
            {
                return DetectedDoor.DetectionConfidence.MEDIUM;
            }
            else
            {
                return DetectedDoor.DetectionConfidence.LOW;
            }
        }

        // Returns a score from 0-10 based on the aspect ratio of the bounding box
        // Optimally a bounding box would have a 1:2 aspect ratio
        private int GetBoundingBoxAspectRatioScore(Rectangle boundingBox)
        {
            double aspectRatio = boundingBox.Height / boundingBox.Width;

            if (aspectRatio >= 1.5 && aspectRatio <= 2.5)
            {
                return 10;
            }
            else if (aspectRatio >= 1.0 && aspectRatio <= 3.0)
            {
                return 5;
            }
            else
            {
                return 0;
            }
        }

        // Returns a score from 0-10 based on the vertical position of the bounding box in the image
        // Optimally, the center point of the bounding box would be within the center 50% of the image
        private int GetBoundingBoxPositionScore(Rectangle boundingBox, Size imageSize)
        {
            int centerY = (boundingBox.Bottom + boundingBox.Top) / 2;
            int halfHeight = imageSize.Height / 2;

            // The vertical distance of centerY from the center point of the image
            int distance = Math.Abs(centerY - halfHeight);

            // Scale the distance to [0, 1] then multiply by 10 to get a score
            return 10 * distance / halfHeight;
        }
    }
}