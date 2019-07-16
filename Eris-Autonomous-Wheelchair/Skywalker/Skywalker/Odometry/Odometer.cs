using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Emgu.CV.Structure;

namespace Skywalker.Odometry
{
    /// <summary>
    /// The odometer, responsible for tracking the distance
    /// that the vehicle has moved.
    /// </summary>
    public class Odometer : IOdometer
    {
        /// <summary>
        /// Whether a stop of the thread has been requested.
        /// </summary>
        private volatile bool Stopped = false;
        private volatile bool Started = false;

        /// <summary>
        /// Instance of Odometer
        /// </summary>
        private static Odometer instance;

        /// <summary>
        /// Returns a Odometer instance
        /// </summary>
        public static Odometer Instance {
            get {
                if (instance == null) {
                    instance = new Odometer();
                }
                return instance;
            }
        }

        /// <summary>
        /// current position of the chair
        /// </summary>
        private Vector position;

        internal Odometer(int x = 0, int y = 0)
        {
            position = new Vector(x,y);
        }

        public Vector Move(int Δx, int Δy)
        {
            position.X += Δx;
            position.Y += Δy;
            return position;
        }

        public void SetPosition(int x, int y)
        {
            position.X = x;
            position.Y = y;
        }

        /// Retrieves the total displacement from the
        /// vehicle's starting position, in meters.
        /// </summary>
        /// <returns>A vector that points from our starting point, to our current position.</returns>
        public Vector GetTotalDisplacement()
        {
            return new Vector(0, 0);
        }

        /// <summary>
        /// Retrieves the angular displacement, in radians
        /// from our initial orientation. This vector points
        /// from the facing of the vehicle.
        /// </summary>
        /// <returns>Vector pointing from our facing</returns>
        public Vector GetTotalRotation()
        {
            return new Vector(0, 0);
        }

        /// <summary>
        /// Resets the odometer, setting our starting point
        /// to our current position.
        /// </summary>
        public void ResetOdometer(int x = 0, int y = 0)
        {
            position.X = x;
            position.Y = y;
        }
    }
}
