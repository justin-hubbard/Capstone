using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skywalker.Sensors.IPS;
using Skywalker.Sensors.SensorArray;
using Skywalker.Mapping;
using Skywalker.ObjectDetection;
using Skywalker.Utils;
using System.Windows;

namespace Skywalker.Localization
{
    // Converts obstacles to absolute grid points occupied by the obstacle
    public class ObstacleLocalizer
    {
        private Pose CurrentPose;

        public ObstacleLocalizer(Pose pose)
        {
            this.CurrentPose = pose;
        }

        // Returns the grid points in XY occupied by the given obstacle
        public List<mPoint> LocalizeObstacle(Obstacle obstacle)
        {
            double currentDirection = AngleUtils.DegreesToRadians(CurrentPose.CurrentDirection);

            // Convert the left and right points from mm to cm
            Vector leftObstaclePoint = obstacle.leftPoint / 10;
            Vector rightObstaclePoint = obstacle.rightPoint / 10;

            // Find the offset of the left and right of the obstacle from the current position
            Vector leftOffsetVector = CurrentPose.CurrentDirectionVector + VectorUtils.RotateVector(leftObstaclePoint, currentDirection);
            Vector rightOffsetVector = CurrentPose.CurrentDirectionVector + VectorUtils.RotateVector(rightObstaclePoint, currentDirection);

            // Estimate the locations of the actual grid points
            mPoint leftMapPoint = new mPoint(CurrentPose.CurrentPositionInXY.X + leftOffsetVector.X, CurrentPose.CurrentPositionInXY.Y + leftOffsetVector.Y, CurrentPose.CurrentRoom);
            mPoint rightMapPoint = new mPoint(CurrentPose.CurrentPositionInXY.X + leftOffsetVector.X, CurrentPose.CurrentPositionInXY.Y + leftOffsetVector.Y, CurrentPose.CurrentRoom);

            // Find the actual grid points from the estimated grid points
            leftMapPoint = CurrentPose.CurrentRoom.GetClosestGridPoint(leftMapPoint.GetUVFromXY(CurrentPose.CurrentRoom));
            rightMapPoint = CurrentPose.CurrentRoom.GetClosestGridPoint(rightMapPoint.GetUVFromXY(CurrentPose.CurrentRoom));

            List<mPoint> points = new List<mPoint>();
            points.Add(leftMapPoint);
            points.Add(rightMapPoint);
            return points;
        }
    }
}
