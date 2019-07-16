using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skywalker.ObjectDetection;

namespace Skywalker.Mapping
{
    public interface IObstacleMap
    {
        void AddObstaclePoints(List<mPoint> points, MapRoom room);
        bool IsPointObstructed(mPoint point, MapRoom room);
    }
}
