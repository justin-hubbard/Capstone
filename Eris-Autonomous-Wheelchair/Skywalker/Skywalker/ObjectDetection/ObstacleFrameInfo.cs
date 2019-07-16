using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Skywalker.ObjectDetection
{
    // Contains information about the visual location
    // of an obstacle within the frame used to detect it
    public class ObstacleFrameInfo
    {
        public float LeftMarginPercentage
        {
            get;
            private set;
        }

        public float TopMarginPercentage
        {
            get;
            private set;
        }

        public float WidthPercentage
        {
            get;
            private set;
        }

        public float HeightPercentage
        {
            get;
            private set;
        }

        public Size FrameSize
        {
            get;
            private set;
        }

        public ObstacleFrameInfo(float leftMarginPercentage, float topMarginPercentage, float widthPercentage, float heightPercentage, Size frameSize)
        {
            this.LeftMarginPercentage = leftMarginPercentage;
            this.TopMarginPercentage = topMarginPercentage;
            this.WidthPercentage = widthPercentage;
            this.HeightPercentage = heightPercentage;
            this.FrameSize = frameSize;
        }
    }
}
