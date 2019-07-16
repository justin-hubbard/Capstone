using System.Windows;
using Skywalker.Input;
using Skywalker.Motor;
using Skywalker.ObjectDetection;
using System.Threading;
using Skywalker.Resources;
using Skywalker.UserInterface;
using Skywalker.Mapping;

namespace Skywalker.Driver
{
    using System;

    /// <summary>
    /// The AI Driver
    /// Acts as a mediator between the various input systems.
    /// </summary>
    public class Driver
    {
        // Whether the driver should continue running
        private volatile bool Running;

        /// <summary>
        /// The user input device.
        /// </summary>
        private volatile IInputDevice UserInput;

        /// <summary>
        /// The vehicle to drive.
        /// </summary>
        private readonly IVehicle Vehicle;

        private readonly INavigator Navigator;

        /// <summary>
        /// The thread that handles driving the vehicle.
        /// </summary>
        private Thread DriveThread;

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="input">The user input device.</param>
        /// <param name="vehicle">The vehicle.</param>
        /// <param name="navigator">The navigator.</param>
        public Driver(IInputDevice input, IVehicle vehicle, INavigator navigator)
        {
            UserInput = input;
            this.Vehicle = vehicle;
            this.Navigator = navigator;

            Input.Input.DeviceChanged += Input_DeviceChanged;
        }

        private void Input_DeviceChanged(object sender, EventArgs e)
        {
            DeviceChangedEventArgs args = (DeviceChangedEventArgs) e;
            string device = args.DeviceName;

            if (device == "Keyboard")
            {
                UserInput = KeyboardInput.Instance;
            }
            else if (device == "Controller")
            {
                UserInput = XboxControllerInput.Instance;
            }
            else if(device == "EyeTribe")
            {
                UserInput = VisualInputGridPresenter.Instance;
            }
        }

        /// <summary>
        /// Starts the driver.
        /// </summary>
        public void Start()
        {
            this.Running = true;

            DriveThread = new Thread(Drive);
            DriveThread.SetApartmentState(ApartmentState.STA);
            DriveThread.Name = "Drive Thread";
            DriveThread.Start();
        }

        /// <summary>
        /// Stops the driver.
        /// </summary>
        public void Stop()
        {
            this.Running = false;
        }

        // A delegate type for hooking up change notifications.
        public delegate void UserInputChanged(object sender, EventArgs e);

        // Controls the direction the wheelchair moves
        private void Drive() //this function calls a function in navigator that looks for offset
        {
            // Check whether the thread has previously been named 
            // to avoid a possible InvalidOperationException. 

			int offset = Navigator.GetCalibrationResult(); //initial setup
			Vehicle.SetOffset(offset);

            while (this.Running)
            {
                Vector direction = Navigator.GetDirection(this.UserInput.InputDirection);

				if (Navigator.GetCalibrationResult() != offset) { //if the offset has changed
					offset = Navigator.GetCalibrationResult();
					Vehicle.SetOffset(offset); //sets the offset in the wheelchair class, SetOffset does stuff
				}
                
				// The length should be in the range [0, 1]. Convert it to [0, 100] as the wheelchair expects
                int speed = (int)(direction.Length * 100);

                // Clamp the speed to [0, 100]
                speed = Math.Min(speed, 100);
                speed = Math.Max(speed, 0);

                if (direction.Length != 0)
                {
                    direction.Normalize();
                }

                this.Vehicle.SetSpeedAndDirection(speed, direction);
                
                Thread.Sleep(50);
            }

            this.Vehicle.SetSpeedAndDirection(0, new Vector(0, 0));

        }
    }
}
