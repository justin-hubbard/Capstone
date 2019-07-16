using Math = System.Math;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Ports;

namespace Skywalker.Motor
{
    /// <summary>
    /// This class handles the serial communication with the Arduino.
    /// 
    /// TODO : Get SetSpeed and SetRotation to set the appropriate X,Y values.
    /// X,Y values are currently set using SetX and SetY.
    /// </summary>
    public class Wheelchair : IVehicle, IDisposable
    {
        /// <summary>
        /// The X voltage from the controller.
        /// </summary>
        private int x_volt;
		
        /// <summary>
        /// The Y voltage from the controller.
        /// </summary>
        private int y_volt;

        private ISerialDevice serialDevice;
        private Thread commThread;
        /// <summary>
        /// The value of X to be sent
        /// </summary>
        private byte xval;
        private byte yval;

		private int xOffset; //offset used to straighten chair
        /// <summary>
        /// The message to be sent over serial.
        /// </summary>
        private byte[] message;
        /// <summary>
        /// The response sent from serial device.
        /// </summary>
        private byte[] response;
        /// <summary>
        /// Serial Port Name.
        /// </summary>
        public string portName { get; set; }
        public int baudRate { get; set; }
        /// <summary>
        /// Returns the current x-voltage.
        /// </summary>
        public int VoltX { get { return x_volt; } }
        /// <summary>
        /// Returns the current y-voltage.
        /// </summary>
        public int VoltY { get { return y_volt; } }

        /// <summary>
        /// Singleton instance.
        /// </summary>
        private static Wheelchair instance;

        /// <summary>
        /// Initialize the XY values to 
        /// </summary>
        /// <param name="portName"></param>
        /// <param name="baudRate"></param>
        private Wheelchair(string portName = "COM7", int baudRate = 9600)
        {
			xOffset = 0;

            this.portName = portName;
            this.baudRate = baudRate;
            this.commThread = new Thread(ThreadCommunication);
            xval = (byte)100;
            yval = (byte)100;
            message = new byte[8];
            response = new byte[8];
            message[0] = (byte)250;
            this.serialDevice = new SerialDevice(portName, baudRate);
            this.Start();

            // Check whether the thread has previously been named 
            // to avoid a possible InvalidOperationException. 
            if (System.Threading.Thread.CurrentThread.Name == null) {
                System.Threading.Thread.CurrentThread.Name = "WheelchairThread";
            }
        }

        private Wheelchair(string port)
        {
			xOffset = 0;

            this.serialDevice = new SerialDevice(port, 9600);
            this.commThread = new Thread(ThreadCommunication);
            xval = (byte)100;
            yval = (byte)100;
            message = new byte[8];
            response = new byte[8];
            message[0] = (byte)250;
            this.Start();
        }

        private Wheelchair(ISerialDevice serialDevice, string portName, int baudRate)
        {
			xOffset = 0;

            this.serialDevice = serialDevice;
            if (portName != null)
            {
                this.serialDevice.PortName = portName;
            }
            if (baudRate > 0)
            {
                this.serialDevice.BaudRate = baudRate;
            }
            this.commThread = new Thread(ThreadCommunication);
            xval = (byte)100;
            yval = (byte)100;
            message = new byte[8];
            response = new byte[8];
            message[0] = (byte)250;
            this.Start();
        }


        public static Wheelchair Instance()
        {
            if (instance == null)
            {
                instance =  new Wheelchair();
            }
            return instance;
        }

        /// <summary>
        /// Gets the singleton instance of
        /// the Wheelchair.
        /// </summary>
        /// <param name="portName"></param>
        /// <param name="baudRate"></param>
        /// <returns>Singleton instance of the wheelchair.</returns>
        public static Wheelchair Instance(string portName = "COM7", int baudRate = 9600)
        {
            if (instance == null)
            {
                instance = new Wheelchair(portName, baudRate);
            }

            return instance;
        }

        public static Wheelchair Instance(ISerialDevice serialDevice)
        {
            if (instance == null)
            {
                instance = new Wheelchair(serialDevice, null, 0);
            }
            return instance;
        }

        public static Wheelchair Instance(ISerialDevice serialDevice, string portName, int baudRate)
        {
            if (instance == null)
            {
                instance = new Wheelchair(serialDevice, portName, baudRate);
            }
            return instance;
        }
        /// <summary>
        /// Opens the serial port and starts the thread that communicates with the serial device.
        /// </summary>
        public void Start()
        {
            try
            {
                if (!serialDevice.IsOpen)
                {
                    serialDevice.Open();
                    if (serialDevice.IsOpen)
                    {
                        if (commThread == null)
                            commThread = new Thread(ThreadCommunication);
                        commThread.Start();
                    }
                }
            }
            catch (Exception)
            {
                // TODO: Decide how to handle SerialPort errors.
                //throw;
            }
        }
        /// <summary>
        /// Closes the serial port and stops the serial communication thread.
        /// </summary>
        public void End()
        {
            if (this.commThread != null)
            {
                commThread.Abort();
                if (serialDevice != null && serialDevice.IsOpen)
                {
                    serialDevice.Close();
                }
                commThread = null;
            }
            /*if (serialDevice != null && serialDevice.IsOpen)
            {
                if (this.commThread != null)
                {
                    commThread.Abort();
                    if (commThread.ThreadState == ThreadState.Aborted)
                    {
                        serialDevice.Close();
                    }
                    commThread = null;
                }
                
            }
             */
        }

		public int XOffset {
			get {return xOffset;}
		}

		public void initializeOffset(int offset) {//DO NOT USE THIS FUNCTION OUTSIDE OF LOADING A SAVED OFFSET
			xOffset = offset;
		}
		public void SetOffset(int offset) {
			if (offset > 5) {
				if (offset > 15)
					xOffset += 2;
				else
					xOffset += 1;
			} else if (offset < -5) {
				if (offset < -15)
					xOffset -= 2;
				else 
					xOffset -= 1;
			} else {

			}
			Properties.Settings.Default.CalibrationOffset = xOffset;
			Properties.Settings.Default.Save();
		}
		/*
		public int XOffset {
			get { return xOffset; }
			set { if () 
				xOffset = value; }
		}*/
        /// <summary>
        /// The left/right value sent to the wheelchair. Expects a value between -100 and 100. Zero is neutral.
        /// </summary>
        /// <param name="x">Positive values indicate turning right. Negative values indicate left turn.</param>
        public void SetX(int x)
        {
            if (x >= -100 && x <= 100)
            {
                // NOTE: This is janky because we are correcting the wheelchair's inability to go straight
				//if (x != 0) { //if its not 0
					x -= xOffset;
				//}
				if (x - xOffset > 100) {
					x = 100;
				}
				if (x - xOffset < -100) {
					x = -100;
				}
				//maybe add something to prevent <-100 or >100
                x += 100;
                this.xval = (byte)x;
            }
        }

		public void SetXZero() {
			int x = 100;
			this.xval = (byte)x;
		}
        /// <summary>
        /// The forward/reverse value sent to the wheelchair. Expects a value between -100 and 100. Zero is neutral (no movement).
        /// </summary>
        /// <param name="y">Positive values are forward movement. Negative values are reverse.</param>
        public void SetY(int y)
        {
            if (y >= -100 && y <= 100)
            {
                y += 100;
                this.yval = (byte)y;
            }
        }

        public void SetXY(int x, int y)
        {
			if (x == 0 && y == 0) {
				SetXZero();
			} else {
				SetX(x);
			}
            SetY(y);
        }
        /// <summary>
        /// Continuously sends XY values and receives XY voltages.
        /// </summary>
        private void ThreadCommunication()
        {
            while (true)
            {
                message[1] = xval;
                message[2] = yval;
                message[3] = (byte)'z';
                serialDevice.Write(message, 0, 4);
                Thread.Sleep(25);
                serialDevice.Read(response, 0, 1);
                if (response[0] == 250)
                {
                    //x_volt = response[1];
                    //y_volt = response[2];
                    //Console.WriteLine("Arduino works");
                }
                else
                {
                    // Something went wrong...
                }

            }
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
            this.SetY((int)speed);
        }

        /// <summary>
        /// Sets the wheelchair's speed and direction.
        /// The direction starts at 0 degrees due north.
        /// Example directions:
        /// 0 - Forward, PI/4 - Turn Right, PI/2 - Reverse, 3PI/2 - Turn Left
        /// </summary>
        /// <param name="speed">The speed of the vehicle. Expected range: [0,100]</param>
        /// <param name="direction">Direction the vehicle should go in. Starts at 0 radians due north.</param>
        public void SetSpeedAndDirection(int speed, Vector direction)
        {
            double x_val = speed * direction.X;
            double y_val = speed * direction.Y;
            int x = 0;
            int y = 0;
            try
            {
                x = Convert.ToInt32(Math.Round(x_val));
                y = Convert.ToInt32(Math.Round(y_val));
                this.SetXY(x, y);
            }
            catch (Exception e)
            {
                //Console.WriteLine("Unable to convert Vector");
            }
            
        }

        public void Dispose()
        {
            if (this.serialDevice != null && this.serialDevice.IsOpen)
            {
                this.End();
            }
            instance = null;
        }
    }


}