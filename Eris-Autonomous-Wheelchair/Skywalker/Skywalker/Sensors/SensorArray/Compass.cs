using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skywalker.Utils.Logger.General;

//interacting with the IMU sensor to get degrees
//x is the yaw
//Y is the pitch
//z is the roll
namespace Skywalker.Sensors.SensorArray {
    public class Compass:SensorDevice {

        private float[] _orientation;//x,y,z orientation from compass
        private bool _orientation_updated;
        private bool _x_updated;
        private bool _y_updated;
        private bool _z_updated;

        public Compass(ILogger logger)
            : base(logger) {

            _orientation = new float[3];
            _orientation_updated = false;
        }

        //Yaw
        public float X {

            get {
                lock(_orientation) {
                    //_x_updated = false;
                    return _orientation[0];
                }
            }
        }

        //pitch
        public float Y {

            get {
                lock(_orientation) {
                    //_y_updated = false;
                    return _orientation[1];
                }
            }
        }

        //roll
        public float Z {
            get {
                //_z_updated = false;
                return _orientation[2];
            }
        }

        //gets an array of all the orientation data
        public float[] Orientation {

            get {
                lock(_orientation) {

                    //_orientation_updated = false;
                    return (float[])_orientation.Clone();
                }
            }
        }

        //check to see if all updated
        public bool Updated {

            get { return _orientation_updated; }
        }

        //yaw updated
        public bool XUpdated {
            get {return _x_updated; }
        }

        //pitch updated
        public bool YUpdated {
            get { return _y_updated; }
        }

        //roll updated
        public bool ZUpdated {
            get { return _z_updated; }
        }

        //internally used to set orientation safely
        private void setOrientation(float x, float y, float z) {

            lock(_orientation) {

                if(!(x == X && y == Y && z == Z)) {

                    _orientation[0] = x;
                    _orientation[1] = y;
                    _orientation[2] = z;
                    _orientation_updated = true;
                    _x_updated = _y_updated = _z_updated = true;
                }
            }
        }

        public override void Process(byte[] packet) {

            //read packet and update orientation as necessary.
            //Use the properties since they have syncronization already
 
            //Get X
            //3 is the most signifigant
            //4 is next most signifigant
            //5 is third most signifigant
            //6 is least signifigant
            byte[] byte_arr = new byte[4];
            byte_arr[3] = packet[3];
            byte_arr[2] = packet[4];
            byte_arr[1] = packet[5];
            byte_arr[0] = packet[6];
            float x = System.BitConverter.ToSingle(byte_arr, 0);
            //Get Y
            //7 is the most signifigant
            //8 is next most signifigant
            //9 is third most signifigant
            //10 is least signifigant
            byte_arr[3] = packet[7];
            byte_arr[2] = packet[8];
            byte_arr[1] = packet[9];
            byte_arr[0] = packet[10];
            float y = System.BitConverter.ToSingle(byte_arr, 0);
            //Get Z
            //11 is the most signifigant
            //12 is next most signifigant
            //13 is third most signifigant
            //14 is least signifigant
            byte_arr[3] = packet[11];
            byte_arr[2] = packet[12];
            byte_arr[1] = packet[13];
            byte_arr[0] = packet[14];
            float z = System.BitConverter.ToSingle(byte_arr, 0);

            setOrientation(x, y, z);
        }
    }
}
