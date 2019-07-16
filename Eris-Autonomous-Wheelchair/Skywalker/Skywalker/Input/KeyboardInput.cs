using System;
using System.Windows;
using System.Windows.Input;
using Skywalker.Resources;

namespace Skywalker.Input
{
    /// <summary>
    /// Input device representing a keyboard.
    /// </summary>
    public class KeyboardInput : IInputDevice
    {
        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static KeyboardInput instance;
        
        /// <summary>
        /// Prevents a default instance of the <see cref="KeyboardInput"/> class from being created. 
        /// Initializes a new instance of the <see cref="KeyboardInput"/> class.
        /// </summary>
        private KeyboardInput()
        {
        }

        /// <summary>
        /// Gets the singleton.
        /// </summary>
        public static KeyboardInput Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new KeyboardInput();
                }
                return instance;
            }
        }

        /// <summary>
        /// Gets the user's input direction.
        /// If no input, returns NO_INPUT.
        /// </summary>
        /// <returns>
        /// Angle of user's input, as a clockwise rotation from due straight, or NO_INPUT.
        /// </returns>
        public Vector InputDirection
        {
            get
            {
                Vector inputVector = new Vector(0,0);
                if (Keyboard.IsKeyDown(Key.W))
                {
                    inputVector.Y = 1;
                }
                else if (Keyboard.IsKeyDown(Key.S))
                {
                    inputVector.Y = -1;
                }
                if (Keyboard.IsKeyDown(Key.A))
                {
                    inputVector.X = -1;
                }
                else if (Keyboard.IsKeyDown(Key.D))
                {
                    inputVector.X = 1;
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
