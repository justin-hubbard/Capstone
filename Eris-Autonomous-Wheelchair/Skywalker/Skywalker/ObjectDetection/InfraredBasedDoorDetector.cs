using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;

using Emgu;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Skywalker_Vision.Kinect;
namespace Skywalker.ObjectDetection
{
    class InfraredBasedDoorDetector:IDoorDetector
    {
        public event DoorDetectedEvent OnDoorDetected;
        private IImageStream imageStream;
        private Thread processingThread;
        // If an image stream is provided to the door detector, UPDATE_INTERVAL represents how often
        // that stream will be used to detect doors. The unit is milliseconds
        private const int UPDATE_INTERVAL = 500;

        public bool IsRunning
        {
            get;
            private set;
        }

        public InfraredBasedDoorDetector()
        {
            IsRunning = false;
        }

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
                throw new InvalidOperationException("This InfraredBasedDoorDetector is already running");
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
                throw new InvalidOperationException("This Infrared BasedDoorDetector is not running");
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
                IFrame iFrame= (IFrame)frame;
                if (iFrame != null)
                {
                    List<DetectedDoor> doors = GetDoors(iFrame);
                    DoorDetectedEventArgs args = new DoorDetectedEventArgs(doors, DetectedDoor.DetectMethod.INFRARED);
                    if (OnDoorDetected != null)
                    {
                        OnDoorDetected(this, args);
                    }
                }
                Thread.Sleep(UPDATE_INTERVAL);
            }
        }

        public List<DetectedDoor> GetDoors(object data)
        {
            IFrame infraredFrame = data as IFrame;
            if (infraredFrame == null)
                return null;
            List<DetectedDoor> doors = new List<DetectedDoor>();
            //Image<Bgr, Byte> smoothed = img.SmoothGaussian(15);
            //Image<Gray, Byte> threshold = FilterColor(smoothed);
            //threshold = SimplifyImage(threshold, 100);

            // new method

            //new method end
            using (MemStorage stor = new MemStorage())
            {
                Bitmap bmp = infraredFrame.GetBMP();
                //Bitmap bmp2 = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), bmp.PixelFormat);
                //Image<Bgr, Byte> img = new Image<Bgr, Byte>(bmp2);
                Image<Gray, Byte> grey;
                try
                {
                    grey = new Image<Gray, Byte>(bmp);
                }
                catch(Exception)
                {
                    return null;
                }

                //CvInvoke.cvInRangeS(img.Ptr, new MCvScalar(0.0, 0.0, 0.0), new MCvScalar(250.0, 250.0, 250.0), grey.Ptr);

                //new method
                CvInvoke.cvErode(grey.Ptr, grey.Ptr, (IntPtr)null, 1);
                CvInvoke.cvDilate(grey.Ptr, grey.Ptr, (IntPtr)null, 2);
                Image<Gray, Byte> cannyEdges = grey.Canny(180, 120);
                LineSegment2D[] lines = cannyEdges.HoughLinesBinary(
                    1, //Distance resolution in pixel-related units
                    Math.PI / 45.0, //Angle resolution measured in radians.
                    20, //threshold
                    5, //min Line width
                    10 //gap between lines
                    )[0]; //Get the lines from the first channel

                LineSegment2D[] container = null;

                container = FilterAndMergeVerticalLines(lines, 1);
                container = FilterShortLines(container, 100);
                container = FindVerticalDoorFrameLines(container, 500, grey);
                
                if (container != null)
                {
                    int boxX = container[0].P1.X;
                    int boxY = container[0].P1.Y;
                    int boxWidth = container[1].P1.X - boxX;
                    int boxHeight = container[1].P2.Y - boxY;
                    Rectangle newBox = new Rectangle(boxX, boxY, boxWidth, boxHeight);
                    int centerX = (newBox.Left + newBox.Right) / 2;
                    int centerY = (newBox.Bottom + newBox.Top) / 2;
                    DetectedDoor.DetectionConfidence confidence = GetConfidence(newBox, grey.Size);
                    int centerXOffset = centerX - (grey.Size.Width / 2);
                    double angle = -Math.Atan2(-centerY, centerXOffset) - Math.PI / 2;
                    DetectedDoor newDoor = new DetectedDoor(newBox, grey.Size, confidence, angle, 0,DetectedDoor.DetectMethod.INFRARED);
                    doors.Add(newDoor);
                }
            }                    
            return doors;
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


        private LineSegment2D[] FilterShortLines(LineSegment2D[] lines, double minLength)
        {
            return Array.FindAll(lines, line => line.Length >= minLength);
        }

        private LineSegment2D[] FindVerticalDoorFrameLines(LineSegment2D[] lines, int maxDistanceDifference, Image<Gray, byte> imageData)
        {
            Array.Sort(lines, (l1, l2) => l1.P1.X.CompareTo(l2.P1.X)); //sort vertical line
            int leftIndex = 0;
            int rightIndex = lines.Length - 1;
            if (leftIndex < 0 || rightIndex < 1)
                return null;
            LineSegment2D ll = lines[leftIndex];
            LineSegment2D rl = lines[rightIndex];

            double leftAverage, rightAverage, leftSum, rightSum;
            leftSum = rightSum = 0;
            while (true)
            {
                if (ll.P1.X < 11 && leftIndex + 1 < rightIndex)
                {
                    leftIndex++;
                    ll = lines[leftIndex];
                }
                else if (ll.P1.X >= 11)
                {

                }
                else
                    return null;
                if (rl.P1.X > 500 && rightIndex - 1 > leftIndex)
                {
                    rightIndex--;
                    rl = lines[rightIndex];
                }
                else if (rl.P1.X <= 500)
                {
                    break;
                }
                else
                    return null;
            }

            while (true)
            {
                //check distance of left side of left frame and right side of right frame
                for (int i = 0; i < ll.Length; i++)
                {
                    for (int j = 0; j < 11; j++)
                    {
                        try
                        {
                            leftSum += imageData.Data[ll.P1.Y + i, ll.P1.X - j, 0];
                        }
                        catch (Exception e)
                        {
                            return null;
                        }
                    }
                }
                leftAverage = leftSum / ll.Length;

                for (int i = 0; i < rl.Length; i++)
                {
                    for (int j = 0; j < 11; j++)
                    {
                        rightSum += imageData.Data[rl.P1.Y + i, rl.P1.X + j, 0];
                    }
                }
                rightAverage = rightSum / rl.Length;

                //get insdie pix value
                double leftInsideSum, leftInsideAverage, rightInsideSum, rightInsideAverage;
                leftInsideSum = rightInsideSum = 0;
                for (int i = 0; i < ll.Length; i++)
                {
                    for (int j = 0; j < 11; j++)
                    {
                        leftInsideSum += imageData.Data[ll.P1.Y + i, ll.P1.X + j, 0];
                    }
                }
                leftInsideAverage = leftInsideSum / ll.Length;

                for (int i = 0; i < rl.Length; i++)
                {
                    for (int j = 0; j < 11; j++)
                    {
                        rightInsideSum += imageData.Data[rl.P1.Y + i, rl.P1.X - j, 0];
                    }
                }
                rightInsideAverage = rightInsideSum / rl.Length;

                if (Math.Abs(leftAverage - rightAverage) > maxDistanceDifference
                    || Math.Abs(leftAverage - leftInsideAverage) < 200
                    || Math.Abs(rightAverage - rightInsideAverage) < 200)//larger than maximum distance difference or the frame still can be pushed
                {
                    if (Math.Abs(ll.P1.X - rl.P1.X) < 21 || leftIndex + 1 >= rightIndex) //too close to find aother door frame
                        return null;


                    if (Math.Abs(leftAverage - leftInsideAverage) < Math.Abs(rightAverage - rightInsideAverage))
                    {
                        leftIndex++;
                        ll = lines[leftIndex];
                    }
                    else
                    {
                        rightIndex--;
                        rl = lines[rightIndex];
                    }

                    continue;
                }
                else
                {
                    List<LineSegment2D> res = new List<LineSegment2D>();
                    res.Add(ll);
                    res.Add(rl);
                    return res.ToArray();
                }
            }
        }


        private LineSegment2D[] FilterAndMergeVerticalLines(LineSegment2D[] lines, int maxCenterXDistance)
        {
            LineSegment2D[] vertical = FilterLinesWithinAngle(lines, 45, 135);

            // Sort 'vertical' based on the center X point of each line
            Array.Sort(vertical, (l1, l2) => GetCenterPoint(l1).X.CompareTo(GetCenterPoint(l2).X));

            // Merge lines which are close together
            // Close together is determined 
            List<LineSegment2D> merged = new List<LineSegment2D>();

            int minY = int.MaxValue;
            int maxY = int.MinValue;
            int accumulatedCenterX = 0;
            List<double> angles = new List<double>();
            int accumulatedCount = 1;
            LineSegment2D? previousLine = null;
            foreach (LineSegment2D line in vertical)
            {
                if (previousLine != null)
                {
                    System.Drawing.Point currentCenter = GetCenterPoint(line);
                    System.Drawing.Point previousCenter = GetCenterPoint(previousLine.Value);

                    if (Math.Abs(previousCenter.X - currentCenter.X) <= maxCenterXDistance)
                    {
                        maxY = Math.Max(Math.Max(line.P1.Y, line.P2.Y), maxY);
                        minY = Math.Min(Math.Min(line.P1.Y, line.P2.Y), minY);

                        accumulatedCenterX += currentCenter.X;
                        angles.Add(GetLineAngle(line));
                        accumulatedCount++;
                    }
                    else
                    {
                        int mergedCenterX = accumulatedCenterX / accumulatedCount;
                        double averageAngle = GetAverageAngle(angles);
                        System.Drawing.Point lowerPoint = new System.Drawing.Point(mergedCenterX, minY);
                        System.Drawing.Point upperPoint = new System.Drawing.Point(mergedCenterX, maxY);

                        LineSegment2D mergedLine = new LineSegment2D(lowerPoint, upperPoint);
                        //mergedLine = RotateLineAboutCenter(mergedLine, averageAngle);
                        merged.Add(mergedLine);

                        accumulatedCenterX = GetCenterPoint(line).X;
                        accumulatedCount = 1;
                        angles.Clear();
                        angles.Add(GetLineAngle(line));

                        previousLine = null;
                    }
                }

                previousLine = line;
            }

            int lastMergedCenterX = accumulatedCenterX / accumulatedCount;
            System.Drawing.Point lastLowerPoint = new System.Drawing.Point(lastMergedCenterX, minY);
            System.Drawing.Point lastUpperPoint = new System.Drawing.Point(lastMergedCenterX, maxY);
            LineSegment2D lastMergedLine = new LineSegment2D(lastLowerPoint, lastUpperPoint);
            merged.Add(lastMergedLine);

            return merged.ToArray();
        }

        private LineSegment2D[] FilterLinesWithinAngle(LineSegment2D[] lines, double minAngle, double maxAngle)
        {
            List<LineSegment2D> filtered = new List<LineSegment2D>();

            foreach (LineSegment2D line in lines)
            {
                double angle = Math.Abs(GetLineAngle(line)) * 180 / Math.PI;
                if (minAngle <= angle && angle <= maxAngle)
                {
                    filtered.Add(line);
                }
            }

            return filtered.ToArray();
        }

        // Returns the angle of the line in radians
        // Return value will be in range [-pi, pi]
        private double GetLineAngle(LineSegment2D line)
        {
            double dy = line.P2.Y - line.P1.Y;
            double dx = line.P2.X - line.P1.X;

            double angle = Math.Atan2(dy, dx);
            if (angle < 0)
            {
                return angle + Math.PI;
            }

            return angle;
        }

        private System.Drawing.Point GetCenterPoint(LineSegment2D line)
        {
            int centerX = (line.P1.X + line.P2.X) / 2;
            int centerY = (line.P1.Y + line.P2.Y) / 2;

            return new System.Drawing.Point(centerX, centerY);
        }

        private LineSegment2D RotateLineAboutCenter(LineSegment2D line, double angle)
        {
            System.Drawing.Point center = GetCenterPoint(line);
            double radians = angle;// * 180 / Math.PI;

            int newP1X = (int)Math.Round(((line.P1.X - center.X) * Math.Cos(radians) + (line.P1.Y - center.Y) * Math.Sin(radians)) + center.X);
            int newP1Y = (int)Math.Round((-(line.P1.X - center.X) * Math.Sin(radians) + (line.P1.Y - center.Y) * Math.Cos(radians)) + center.Y);
            int newP2X = (int)Math.Round(((line.P2.X - center.X) * Math.Cos(radians) + (line.P2.Y - center.Y) * Math.Sin(radians)) + center.X);
            int newP2Y = (int)Math.Round((-(line.P2.X - center.X) * Math.Sin(radians) + (line.P2.Y - center.Y) * Math.Cos(radians)) + center.Y);

            System.Drawing.Point newP1 = new System.Drawing.Point(newP1X, newP1Y);
            System.Drawing.Point newP2 = new System.Drawing.Point(newP2X, newP2Y);
            return new LineSegment2D(newP1, newP2);
        }

        private double GetAverageAngle(List<double> angles)
        {
            double x = 0;
            double y = 0;

            foreach (double angle in angles)
            {
                x += Math.Cos(angle);
                y += Math.Sin(angle);
            }

            return Math.Atan2(y, x);
        }
    }
}
