using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;

using Skywalker.ObjectDetection;
using Skywalker.Resources;
using System.Windows;

namespace Skywalker.UserInterface.Overlays
{
    public class ObstaclesOverlay : Overlay
    {
        private List<Obstacle> _detectedObstacles;

        //public delegate void ObjectSizeDetectedEvent(object sender, ObjectSizeDetectedArgs e);

        public ObstaclesOverlay(IObstacleDetector obstacleDetector) : base()
        {
            this.Name = "Obstacles";

            _detectedObstacles = null;
            obstacleDetector.ObstaclesDetected += obstacleDetector_ObstaclesDetected;
        }

        public override void RenderInCanvas(Canvas canvas, double width, double height)
        {
            if (_detectedObstacles == null)
            {
                return;
            }

            foreach (Obstacle obstacle in _detectedObstacles)
            {
                Rectangle rect = new Rectangle();
                rect.Fill = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0)); // Red, 50% opacity

                ObstacleFrameInfo frameInfo = obstacle.FrameInfo;

                double frameWidthDifference = canvas.ActualWidth - frameInfo.FrameSize.Width;
                double frameHeightDifference = canvas.ActualHeight - frameInfo.FrameSize.Height;

                // Since the depth frame is flipped along the vertical axis compared to the RGB stream, the left
                // margin is actually the right margind
                double leftMarginPercentage = 1 - (frameInfo.LeftMarginPercentage + frameInfo.WidthPercentage);

                double leftMarginInPixels = leftMarginPercentage * (canvas.ActualWidth - frameWidthDifference) + (frameWidthDifference / 2);
                double topMarginInPixels = frameInfo.TopMarginPercentage * (canvas.ActualHeight - frameHeightDifference) + (frameHeightDifference / 2);

                rect.Width = (canvas.ActualWidth - frameWidthDifference) * frameInfo.WidthPercentage;
                rect.Height = (canvas.ActualHeight - frameHeightDifference) * frameInfo.HeightPercentage;

                Canvas.SetLeft(rect, leftMarginInPixels);
                Canvas.SetTop(rect, topMarginInPixels);

                int maxSize = 1;

                // ADDED
                if (rect.Width > maxSize && rect.Height > maxSize) // if the AREA of the rectangle (Width * Height)
                {
                    // send event
                    canvas.Children.Add(rect); // orignal functions add the rectangle to the canvas
                }

                //Console.WriteLine("width = {0}", rect.Width);
                //Console.WriteLine("height = {0}", rect.Height);
                //Console.WriteLine();
            }

            //Console.WriteLine("------------------------------");
        }

        void obstacleDetector_ObstaclesDetected(object sender, ObstaclesDetectedArgs e)
        {
            _detectedObstacles = e.Obstacles;
        }

        // Returns a positioned Rectangle which can be added to the canvas
        private Rectangle RectangleFromObstacle(Obstacle obstacle, Canvas canvas, double width, double height)
        {
            Rectangle rect = new Rectangle();

            // Convert to screen coordinates from world coordinates.
            // Perform the reverse operation on ObstacleDetector's EdgeToObstacle method

            Vector normalizedLeft = obstacle.leftPoint;
            normalizedLeft.Normalize();

            Vector normalizedRight = obstacle.rightPoint;
            normalizedRight.Normalize();

            double rightAngle = Math.Asin(normalizedLeft.X);
            double leftAngle = Math.Asin(normalizedRight.X);

            rightAngle *= -1;
            leftAngle *= -1;

            // Find the min/max pixel X values relative to the depth frame
            double minFrameX = (leftAngle + Config.Instance.DEPTH_VIEW_ANGLE / 2) * (Config.Instance.DEPTH_PIXEL_WIDTH / Config.Instance.DEPTH_VIEW_ANGLE);
            double maxFrameX = (rightAngle + Config.Instance.DEPTH_VIEW_ANGLE / 2) * (Config.Instance.DEPTH_PIXEL_WIDTH / Config.Instance.DEPTH_VIEW_ANGLE);

            // Convert min/maxFrameX to canvas coordinates
            // leftCanvasMargin is the difference between the left of the canvas and the left of the rendered image
            double leftCanvasMargin = (canvas.ActualWidth - width) / 2;
            double minCanvasX = (minFrameX / Config.Instance.DEPTH_PIXEL_WIDTH * width) + leftCanvasMargin;
            double maxCanvasX = (maxFrameX / Config.Instance.DEPTH_PIXEL_WIDTH * width) + leftCanvasMargin;

            Canvas.SetLeft(rect, minCanvasX);
            rect.Width = maxCanvasX - minCanvasX;

            rect.Height = 200;

            return rect;
        }
    }
}
