using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.IO;

namespace Skywalker.ObjectDetection
{
    public class HaarCascadeBasedDoorDetector
    {
        private CascadeClassifier mHaar;
        private Bitmap mBitmap;
        private bool hasDoor;
        private Rectangle[] mRectangle;
        private int mX, mY, mRecIndex;

        private const double DISTANCE_SCALE_COEFFICIENT_FACTOR = 43.861;
        private const double DISTANCE_SCALE_EXPONENT_FACTOR = 1.9314;

        public event DoorDetectedEvent OnDoorDetected;

        public HaarCascadeBasedDoorDetector()
        {
            string xmlFile = Environment.CurrentDirectory;
            xmlFile += "\\cascade.xml";
            this.mHaar = new CascadeClassifier(xmlFile);
            this.mBitmap = null;
            this.hasDoor = false;
            this.mRectangle = null;
            this.mX = this.mY = -1;
            this.mRecIndex = -1;
        }

        public void HaarCascadeDoorDection()
        {
            if (mBitmap == null)
            {
                this.hasDoor = false;
                return;
            }
            var grayFrame = new Image<Gray, Byte>(mBitmap);
            //get door position in the frame
            mRectangle = mHaar.DetectMultiScale(grayFrame, 1.4, 5, new Size(50, 50), new Size(800, 800));
            if (mRectangle.Length > 0)
            {
                hasDoor = true;
                int maxSize = 0;

                //store the info of the largest object in the frame
                for (int i = 0; i < mRectangle.Length; i++)
                {
                    if (mRectangle[i].Size.Height > maxSize) //Height and width are same 
                    {
                        mRecIndex = i;
                        mX = mRectangle[i].Location.X;
                        mY = mRectangle[i].Location.Y;
                    }
                }
            }
            else
            {
                hasDoor = false;
            }
            ExtractDoors(grayFrame);
        }

        private List<DetectedDoor> ExtractDoors(Image<Gray, Byte> img)
        {
            List<DetectedDoor> doors = new List<DetectedDoor>();
            for (int i=0; i < mRectangle.Length;i++ )
            {
                doors.Add(DoorFromRec(mRectangle[i], img.Size));
            }
            return doors;
        }

        private DetectedDoor DoorFromRec(Rectangle rec, Size imageSize)
        {
            Rectangle boundingBox = rec;
            int centerX = (boundingBox.Left + boundingBox.Right) / 2;
            int centerY = (boundingBox.Bottom + boundingBox.Top) / 2;

            // Get the angle relative to the bottom center of the image
            int centerXOffset = centerX - (imageSize.Width / 2);
            double angle = -Math.Atan2(-centerY, centerXOffset) - 2 * Math.PI / 4;
            double distance = GetEstimatedDistance(rec, imageSize);
            DetectedDoor.DetectionConfidence confidence = GetConfidence(boundingBox, imageSize);

            return new DetectedDoor(boundingBox,imageSize, confidence, angle, distance, DetectedDoor.DetectMethod.HAAR);
        }

        private double GetEstimatedDistance(Rectangle rec, Size imageSize)
        {
            double recArea = rec.Width * rec.Height;
            double imageArea = imageSize.Width * imageSize.Height;

            // Create a range (0, 1] representing how far away the contour is
            // 1 means infinitely far away, 0 means a near-zero proximity
            // Scale that range by DISTANCE_SCALE_FACTOR to return the actual
            // distance
            double areaPercentage = 1 - recArea / imageArea;
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

        public void LoadImage(Image image)
        {
            mBitmap = new Bitmap(image);
        }

        public void LoadFileImage(string fileName)
        {
            using (Stream BitmapStream = System.IO.File.Open(fileName, System.IO.FileMode.Open))
            {
                Image img = Image.FromStream(BitmapStream);
                LoadImage(img);
            }
        }

    }
}
