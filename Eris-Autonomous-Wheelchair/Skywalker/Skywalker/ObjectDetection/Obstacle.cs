using System.Windows;

namespace Skywalker.ObjectDetection
{
    public class Obstacle
    {
        public Vector leftPoint
        {
            get;
            private set;
        }

        public Vector rightPoint
        {
            get;
            private set;
        }

        public ObstacleFrameInfo FrameInfo
        {
            get;
            set;
        }

        public Obstacle(Vector leftPoint, Vector rightPoint)
        {
            this.leftPoint = leftPoint;
            this.rightPoint = rightPoint;
        }
    }
}
