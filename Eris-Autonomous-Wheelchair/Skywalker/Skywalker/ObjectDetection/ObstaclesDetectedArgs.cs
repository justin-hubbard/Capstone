using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skywalker.ObjectDetection
{
    /// <summary>
    /// Arguments for the Obstacles Detected event.
    /// Describes the objects that were detected.
    /// </summary>
    public class ObstaclesDetectedArgs : EventArgs
    {
        public List<Obstacle> Obstacles
        {
            get;
            private set;
        }

        public ObstaclesDetectedArgs(List<Obstacle> obstacles)
        {
            this.Obstacles = obstacles;
        }
    }
}
