using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skywalker.ObjectDetection
{
    public class SonarDistanceChangedEventArgs:EventArgs
    {
        public List<SonarSensorInfo> Sonars
        {
            get;
            private set;
        }

        public SonarDistanceChangedEventArgs(List<SonarSensorInfo> sonars)
        {
            this.Sonars = sonars;
        }
    }
}
