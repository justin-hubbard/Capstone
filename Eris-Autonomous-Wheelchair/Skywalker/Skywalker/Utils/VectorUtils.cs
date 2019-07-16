using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Skywalker.Utils
{
    class VectorUtils
    {
        public static Vector ResizeVector(Vector v, double length)
        {
            if (v.Length != 0)
            {
                v.Normalize();
            }
            return v * length;
        }

        // Returns a unit vector pointing in the given direction
        // Angle is expected to be in radians
        public static Vector VectorFromAngle(double angle)
        {
            return new Vector(Math.Cos(angle), Math.Sin(angle));
        }

        // Returs a new vector which is equivalent the given vector rotated by the given angle
        public static Vector RotateVector(Vector vector, double angleInRadians)
        {
            double newVectorX = vector.X * Math.Cos(angleInRadians) - vector.Y * Math.Sin(angleInRadians);
            double newVectorY = vector.X * Math.Sin(angleInRadians) + vector.Y * Math.Cos(angleInRadians);

            return new Vector(newVectorX, newVectorY);
        }
    }
}
