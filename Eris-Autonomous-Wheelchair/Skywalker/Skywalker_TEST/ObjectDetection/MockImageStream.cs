using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Skywalker_Vision.Kinect;

namespace Skywalker_TEST.ObjectDetection
{
    public class MockImageStream : IImageStream
    {
        public BaseFrame Frame
        {
            get;
            private set;
        }

        public MockImageStream(BaseFrame mockFrame)
        {
            this.Frame = mockFrame;
        }

        public BaseFrame GetFrame()
        {
            return this.Frame;
        }

        public string GetName()
        {
            throw new NotImplementedException();
        }
    }
}
