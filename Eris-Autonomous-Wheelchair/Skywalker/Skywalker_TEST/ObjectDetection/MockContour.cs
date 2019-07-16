using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Emgu.CV;
using Emgu.Util;

namespace Skywalker_TEST.ObjectDetection
{
    class MockContour<T> : Contour<T> where T : struct
    {
        private Rectangle bb;

        public override Rectangle BoundingRectangle
        {
            get
            {
                return bb;
            }
        }

        public MockContour(Rectangle boundingRect) : base(new MemStorage())
        {
            this.bb = boundingRect;
        }
    }
}
