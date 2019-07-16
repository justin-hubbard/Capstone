using System.Windows;

namespace Skywalker.Odometry
{
    /// <summary>
    /// Interface for the Odometer,
    /// which is responsible for tracking
    /// distance moved.
    /// </summary>
    public interface IOdometer
    {
        /// <summary>
        /// Retrieves the linear displacement, in meters
        /// from our starting position. This vector draws a
        /// line from our starting point to where we are now.
        /// </summary>
        /// <returns>Vector representing displacement</returns>
        Vector GetTotalDisplacement();

        /// <summary>
        /// Retrieves the angular displacement, in radians
        /// from our initial orientation. This vector points
        /// from the facing of the vehicle.
        /// </summary>
        /// <returns>Vector pointing from our facing</returns>
        Vector GetTotalRotation();

        /// <summary>
        /// Resets the odometer, setting our starting point
        /// to our current position.
        /// </summary>
        void ResetOdometer(int x = 0, int y = 0);

        Vector Move(int Δx, int Δy);
        void SetPosition(int x, int y);
    };
}
