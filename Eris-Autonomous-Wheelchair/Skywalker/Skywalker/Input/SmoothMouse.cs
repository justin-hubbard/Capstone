using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skywalker.Input
{
    public class SmoothMouse
    {
        private List<double> xPoints;
        private List<double> yPoints;
        private int numOfPoints;
        private double currentX;
        private double currentY;
        private double sdX;
        private double sdY;

        public double X
        {
            get { return currentX; }
        }

        public double Y
        {
            get { return currentY; }
        }

        public int IntX
        {
            get { return Convert.ToInt32(currentX);}
        }

        public int IntY
        {
            get { return Convert.ToInt32(currentY); }
        }
        public SmoothMouse(int numberOfPoints)
        {
            this.numOfPoints = numberOfPoints;
            Point p = new Point();
            xPoints = new List<double>();
            yPoints = new List<double>();
            xPoints.Add(1);
            yPoints.Add(1);
            this.currentX = 1;
            this.currentY = 1;
            sdX = 0;
            sdY = 0;
        }

        public void Add(double x, double y)
        {
            xPoints.Add(x);
            yPoints.Add(y);
            while (xPoints.Count > numOfPoints)
            {
                xPoints.RemoveAt(0);
                yPoints.RemoveAt(0);
            }
            double avgX = xPoints.Average();
            double avgY = yPoints.Average();
            double localsdx = xPoints.StandardDeviation();
            double localsdy = yPoints.StandardDeviation(); 
            //Console.WriteLine("X:{0}, Y:{1}, SDX:{2}, SDY:{3}, LocalX:{4}, LocalY:{5}", currentX, currentY, sdX, sdY, localsdx, localsdy);
            //localsdx = Math.Min(localsdx, sdX);
            //localsdy = Math.Min(localsdy, sdY);
            sdX = (localsdx < 25.0) ? localsdx : 25.0;
            sdY = (localsdy < 25.0) ? localsdy : 25.0;
            if (avgX < (currentX - sdX) || avgX > (currentX + sdX))
            {
                currentX = avgX;
                //sdX = (localsdx < 25.0) ? localsdx : 25.0;
            }
            if (avgY < (currentY - sdY) || avgY > (currentY + sdY))
            {
                currentY = avgY;
                //sdY = (localsdy < 25.0) ? localsdy : 25.0;
            }
        }

        private void Update_Coordinates()
        {
            
        }
    }
    public static class Extend
    {
        public static double StandardDeviation(this IEnumerable<double> values)
        {
            double avg = values.Average();
            return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
        }

        public static int StandardDeviation(this IEnumerable<int> values)
        {
            double avg = values.Average();
            return Convert.ToInt32(Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2))));
        }
    }
}
