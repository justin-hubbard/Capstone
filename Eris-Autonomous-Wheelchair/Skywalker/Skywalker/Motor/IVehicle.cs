using System.Windows;

namespace Skywalker.Motor
{
    /// <summary>
    /// The Vehicle interface.
    /// </summary>
    public interface IVehicle
    {
        /// <summary>
        /// Sets the left/right directional value. Range [-100,100]
        /// Expects a value between -100 and 100.
        /// Negative values indicate turning left.
        /// Positve values indicate turning right.
        /// </summary>
        /// <param name="x_percent">The value representing the speed and turn direction. Range [-100,100]</param>
        void SetX(int x_percent);

		void initializeOffset(int offset);
		void SetOffset(int offset);
		int XOffset {
			get;
		}
        /// <summary>
        /// Sets the forward/backward direction and speed. Range [-100, 100]
        /// Positive values represent moving forward.
        /// Negative values represent moving backward.
        /// </summary>
        /// <param name="y_percent">The value representing forward/backward movement. Range [-100,100]</param>
        void SetY(int y_percent);

        /// <summary>
        /// This method should be called to start the Vehicle.
        /// The Vehicle class should have communication initialized and begin communicating direction and speed
        /// when this function is called.
        /// </summary>
        void Start();

        /// <summary>
        /// This method should close communication channels (such as SerialPort) and stop sending direction and speed
        /// values to the Vehicle device.
        /// </summary>
        void End();

        /// <summary>
        /// Sets the vehicle's rotation.
        /// </summary>
        /// <param name="rotation">The clock-wise rotation for due north.</param>
        void SetRotation(Vector rotation);

        /// <summary>
        /// Sets the vehicle's speed.
        /// </summary>
        /// <param name="speed">The new speed.</param>
        /// <param name="delta">The desired acceleration to that speed.</param>
        void SetSpeed(float speed, float delta);

        /// <summary>
        /// Sets the vehicle's speed and direction.
        /// The direction starts at 0 degrees due north.
        /// Example directions:
        /// 0 - Forward, PI/4 - Turn Right, PI/2 - Reverse, 3PI/2 - Turn Left
        /// </summary>
        /// <param name="speed">The speed of the vehicle. Expected range: [0,100]</param>
        /// <param name="direction">Direction the vehicle should go in, in radians. Starts at 0 degrees due north.</param>
        void SetSpeedAndDirection(int speed, Vector direction);
    }
}
