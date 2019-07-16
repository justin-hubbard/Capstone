using System;
using System.Drawing;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Skywalker.ObjectDetection;

namespace Skywalker.Mapping
{
    // Tracks "temporary" obstacles, that is, obstacles detected by the wheelchair during runtime
    // Does not track permanent obstacles set in the map
    //
    // When an obstacle is detected, it is only tracked in the obstacle map for its TTL
    // This is to prevent, for example, someone who walked in front of the wheelchair from marking the whole area as impassible
    public class ObstacleMap : IObstacleMap
    {
        private const double OBSTACLE_TTL = 5; // number of seconds for an obstacle to be tracked

        private Dictionary<mPoint, int> PointMarkedCount; // the number of times each point has been marked as obstructed

        public ObstacleMap()
        {
            PointMarkedCount = new Dictionary<mPoint, int>();
        }

        // Adds the given obstacle to the given room
        // Obstacles remain in the room for OBSTACLE_TTL seconds
        // Expects the points to be in XY
        public void AddObstaclePoints(List<mPoint> points, MapRoom room)
        {
            foreach (mPoint point in points)
            {
                int numberMarked = 0;

                //Console.WriteLine("Adding point to obstacle map: XY({0}, {1}), UV({2}, {3})", point.X, point.Y, point.GetUVFromXY(room).X, point.GetUVFromXY(room).Y);

                lock (PointMarkedCount)
                {
                    if (PointMarkedCount.ContainsKey(point))
                    {
                        numberMarked = PointMarkedCount[point];
                    }

                    PointMarkedCount[point] = numberMarked + 1;
                }
            }
        }

        // Returns whether or not the given point is occupied by an obstacle
        // The point should be in XY
        public bool IsPointObstructed(mPoint point, MapRoom room)
        {
            lock (PointMarkedCount)
            {
                if (PointMarkedCount.ContainsKey(point))
                {
                    return PointMarkedCount[point] > 0;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
