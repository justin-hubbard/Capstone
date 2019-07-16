using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skywalker.Driver;
using Skywalker.ObjectDetection;

namespace Skywalker_TEST.Driver.Mocks
{
    /// <summary>
    /// Mock of the Obstacle Detector interface.
    /// </summary>
    public class ObstacleDetectorMock : IObstacleDetector
    {
        public event ObjectDetectedEvent ObjectDetected;

        public List<Obstacle> ObstacleData
        {
            get
            {
                return null;
            }
        }

        public void FireObjectDetection()
        {
            ObjectDetected(this, null);
        }

        public void Start() { }
        //public List<Obstacle> ObstacleData;
    }
}
