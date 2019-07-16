using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SharpDX.XInput;

namespace Skywalker.Input {
    class XboxControllerInput : IInputDevice {
        private static XboxControllerInput instance;
        private Controller xController;
        private XboxControllerInput()
        {
            xController = new Controller(0);
        }


        /// <summary>
        /// Gets the singleton.
        /// </summary>
        public static XboxControllerInput Instance {
            get {
                if (instance == null) {
                    instance = new XboxControllerInput();
                }
                return instance;
            }
        }

        /// <summary>
        /// Gets the user's input direction.
        /// </summary>
        /// <returns>
        /// Angle of user's input, as a clockwise rotation from due straight, or NO_INPUT.
        /// </returns>
        public Vector InputDirection
        {
            get
            {
                Vector inputVector = new Vector(0, 0);
                bool connected = xController.IsConnected;
                if (connected)
                {
                    State controllerState = xController.GetState();

                    inputVector.Y =controllerState.Gamepad.LeftThumbY/short.MaxValue;
                    inputVector.X = controllerState.Gamepad.LeftThumbX/short.MaxValue;
                }
                else
                {
                    //Debug.WriteLine("No controller!");
                }
                if (inputVector.Length > 0.001f)
                {
                    inputVector.Normalize();
                }

                return inputVector;
            }
        }
    }
}
