using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skywalker.Utils.ByteReader;
using Skywalker.Utils.Logger.General;

namespace Skywalker.Sensors.IPS {
    //This class represents a positioning system. It is ment to be used in a consumer producer
    //situation. The producer is the positiong system that is derived from this class. It interacts
    //with the hardware for determining position and then it will produce the X and Y, and flag as updated
    //the object that needs the position will consume the X and Y and then set flag to not updated.
    public class PositioningSystem {

        private short[] _position;

        private bool _x_updated; //help keep track for outside system to see if we have a new position
        private bool _y_updated;
        protected IByteReader _b_reader;
        volatile protected ILogger _logger;

        public PositioningSystem(IByteReader b_reader, ILogger logger) {
            _b_reader = b_reader;
            _logger = logger;
            _position = new short[2];
            _x_updated = false;
            _y_updated = false;
            _position[0] = -1;
            _position[1] = -1;
        }

        public int X {
            get {
                lock(_position) {
                    //XUpdated = false;
                    return _position[0];
                }
            }
        }

        protected void setX(short value) {
            lock(_position) {

                if(0 > value) { 
                    value = Math.Abs(value);
                }

                if(value != _position[0]) {
                    _position[0] = value;
                    _x_updated = true;
                }
            }
        }

        public int Y {

            get {
                lock(_position) {
                    //YUpdated = false;
                    return _position[1];
                }
            }
        }

        protected void setY(short value) {

            if(0 > value) {
                value = Math.Abs(value);
            }

            lock(_position) {
                if(value != _position[1]) {
                    _position[1] = value;
                    _y_updated = true;
                }
            }
        }

        public short[] Position {

            get { return _position; }
        }

        public bool XUpdated {

            get { return _x_updated; }
            set { _x_updated = value; }
        }

        public bool YUpdated {

            get { return _y_updated; }
            set { _y_updated = value; }
        }

    }
}
