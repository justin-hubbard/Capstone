using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skywalker.Utils.Logger.General;
//1 bottom right
//4 middle on camera
//5 front left

namespace Skywalker.Sensors.SensorArray {
    //This is a class that acts as a container for all of the SonarSensors
    //It will be incharge of receiveing and delegating packets to each individual sonar sensor
    public class SonarArray:SensorDevice {

        private List<SonarSensor> _sensors;

        public SonarArray(ILogger logger)
            : base(logger) {
            _sensors = new List<SonarSensor>();
            _sensors.Add(new SonarSensor(logger)); //0 - front left
            _sensors.Add(new SonarSensor(logger)); //1 - front right
            _sensors.Add(new SonarSensor(logger)); //2 - middle front left
            _sensors.Add(new SonarSensor(logger)); //3 - middle front right
            _sensors.Add(new SonarSensor(logger)); //4 - middle back left
            _sensors.Add(new SonarSensor(logger)); //5 - middle back right
            _sensors.Add(new SonarSensor(logger)); //6 - back left
            _sensors.Add(new SonarSensor(logger)); //7 - back right
        }

        //interact with the SonarSensors so we can get the updated
        public bool getUpdatedAt(int i) {

            return _sensors[i].Updated;
        }

        //Interact with sonar sensorSors so we can get distance
        public int getDistanceAt(int i) {
            
            return _sensors[i].Distance;
        }





        //I would not recomend using the get all methods. They will take time to process
        //and by the time you receive them the information may be stale. When posible grab the specific
        //information from the specific sensor you want. With the onboard PC this may not be a problem
        //but as a heads up

        //get all Updated values
        public List<bool> getAllUpdated() {

            List<bool> updated = new List<bool>();
            for(int i = 0; i < _sensors.Count; i++) {

                updated.Add(_sensors[i].Updated);
            }

            return updated;
        }

        //get all distances
        public List<int> getAllDistance() {

            List<int> distance = new List<int>();
            for(int i = 0; i < _sensors.Count; i++) {

                distance.Add(_sensors[i].Distance);
            }

            return distance;
        }

        public override void Process(byte[] packet) {

            int sensor_id = -1;
            //Process the Packet to figure out what Sonar sensor to delagate to
            //parse the sensor identifier
            sensor_id = (byte)packet[3];

            //check for valid id
            if(sensor_id < _sensors.Count) {//else ignore invalid id packet
                _sensors[sensor_id].Process(packet);
            }

        }
    }
}
