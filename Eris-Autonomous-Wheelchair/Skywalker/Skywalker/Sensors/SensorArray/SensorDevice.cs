using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skywalker.Utils.Logger.General;

namespace Skywalker.Sensors.SensorArray {
    //Generic Sensor Device that can be plugged into the Sensor Array class
    public abstract class SensorDevice {

        protected ILogger _logger;

        public SensorDevice(ILogger logger) {
            _logger = logger;
        }

        public abstract void Process(byte[] packet);
    }
}
