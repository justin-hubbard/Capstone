using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using Skywalker_Vision.Kinect;

namespace Skywalker_TEST.ObjectDetection
{
    public class MockFrame : BaseFrame
    {
        public MockFrame(Bitmap bmp)
        {
            this.Bitmap = bmp;
        }
    }
}
