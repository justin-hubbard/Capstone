using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skywalker.Utils.Logger.General;

namespace Skywalker.Sensors.SensorArray {
    //Take Distance sonar distance sensor packet and processes it
    public class SonarSensor:SensorDevice {

        private int _distance;
        public object distance_lock;
        private bool _distance_updated;

        public SonarSensor(ILogger logger)
            : base(logger) {

            distance_lock = new Object();//generic object so we can lock on something. Ints can't be locked with
            _distance = 0;
        }

        public int Distance {
            get {
                lock(distance_lock) {
                    //_distance_updated = false;
                    return _distance;
                }
            }
        }

        public bool Updated {

            get { return _distance_updated; }
        }

        private void SetDistance(int value) {

            lock(distance_lock) {

                if(value != _distance) {
                    _distance = value;
                    _distance_updated = true;
                }
            }
        }

        public override void Process(byte[] packet) {

            //grab information from the packet and set equal to _distance
            //use properties to access the variables since the SetDistance have
            //syncornization support already written

            //4 is the most signifigant
            //5 is next most signifigant
            //6 is third most signifigant
            //7 is least signifigant
            int distance = (packet[4] << 16) + (packet[5] << 12) + (packet[6] << 8) + packet[7];
            //avoid locking _distance as much as possible
            SetDistance(distance);
        }
    }
}
