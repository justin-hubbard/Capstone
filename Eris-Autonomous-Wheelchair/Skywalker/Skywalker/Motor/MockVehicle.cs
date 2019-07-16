using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;

namespace Skywalker.Motor
{
    public class PositionChangedEventArgs : EventArgs
    {
        private int newPosition;

        public int NewPosition { get { return newPosition; } }

        public PositionChangedEventArgs(int newPosition)
        {
            this.newPosition = newPosition;
        }
    }

    public delegate void PositionChangedHandler(IVehicle sender, PositionChangedEventArgs args);

    public class MockVehicle : IVehicle
    {
        public event PositionChangedHandler OnXChanged;
        public event PositionChangedHandler OnYChanged;
		public void initializeOffset(int offset) { }
		public int XOffset {
			get { return 0; }
		}
        public MockVehicle()
        {
            
        }

        /// <summary>
        /// Sets the left/right directional value. Range [-100,100]
        /// Expects a value between -100 and 100.
        /// Negative values indicate turning left.
        /// Positve values indicate turning right.
        /// </summary>
        /// <param name="x_percent">The value representing the speed and turn direction. Range [-100,100]</param>
        public void SetX(int x_percent)
			{
            if (OnXChanged != null)
            {
                OnXChanged(this, new PositionChangedEventArgs(x_percent));
            }
        }

        /// <summary>
        /// Sets the forward/backward direction and speed. Range [-100, 100]
        /// Positive values represent moving forward.
        /// Negative values represent moving backward.
        /// </summary>
        /// <param name="y_percent">The value representing forward/backward movement. Range [-100,100]</param>
        public void SetY(int y_percent)
        {
            if (OnYChanged != null)
            {
                OnYChanged(this, new PositionChangedEventArgs(y_percent));
            }
        }

        public void Start()
        {
        }

        public void End()
        {
        }

        /// <summary>
        /// Sets the vehicle's rotation.
        /// </summary>
        /// <param name="rotation">The clock-wise rotation for due north.</param>
        public void SetRotation(Vector rotation)
        {
            this.SetX((int)(rotation.X * 100f));
        }

        /// <summary>
        /// Sets the vehicle's speed.
        /// </summary>
        /// <param name="speed">The new speed.</param>
        /// <param name="delta">The desired acceleration to that speed.</param>
        public void SetSpeed(float speed, float delta)
        {
            // Matching the behavior of the Vehicle class
            SetY((int)speed /10);
        }

        /// <summary>
        /// Sets the vehicle's speed and direction.
        /// The direction starts at 0 degrees due north.
        /// Example directions:
        /// 0 - Forward, PI/4 - Turn Right, PI/2 - Reverse, 3PI/2 - Turn Left
        /// </summary>
        /// <param name="speed">The speed of the vehicle. Expected range: [0,100]</param>
        /// <param name="direction">Direction the vehicle should go in, in radians. Starts at 0 degrees due north.</param>
        public void SetSpeedAndDirection(int speed, Vector direction)
        {
            // Matching the behavior of the Vehicle class
            Vector targetPosition = direction * speed;
            int x = Convert.ToInt32(Math.Round(targetPosition.X));
            int y = Convert.ToInt32(Math.Round(targetPosition.Y));

            SetX(x);
            SetY(y);
        }

		public void SetOffset(int offset) {}
    }
}