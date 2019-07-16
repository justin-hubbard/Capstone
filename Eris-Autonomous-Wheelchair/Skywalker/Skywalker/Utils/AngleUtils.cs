using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skywalker.Utils
{
    class AngleUtils
    {
        public static double DegreesToRadians(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        public static double RadiansToDegrees(double radians)
        {
            return radians * (180 / Math.PI);
        }
    }
}
