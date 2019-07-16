using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skywalker.ObjectDetection
{
    public class SonarSensorInfo
    {
        public int ID
        {
            get;
            private set;
        }

        public int CurrentAverageDistance
        {
            get;
            private set;
        }

        public int ChangedDifference
        {
            get;
            private set;
        }

        public SonarSensorInfo (int ID, int Distance, int Differece)
        {
            this.ID = ID;
            this.CurrentAverageDistance = Distance;
            this.ChangedDifference = Differece;
        }
    }
}
