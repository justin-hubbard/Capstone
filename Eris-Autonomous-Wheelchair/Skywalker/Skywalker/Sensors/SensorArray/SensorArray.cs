using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Skywalker.Utils.ByteReader;
using Skywalker.Utils.LEntry;
using Skywalker.Utils.Logger.General;

using System.IO;

/*
 * This class will be the interface between the sensors on the sensor slave arduino and the rest of the code
 * an example of declaring:
 * 
 *          try
 *          {
 *              IByteReader byteReader= new SerialByteReader("COM3");
 *          }
 *          catch(Exception e)
 *          {
 *              MessageBox.Show(e.Message);
 *          }
 *          
 *          if (byteReader != null)
 *          {
 *              ILogger logger = new NullLogger();
 *              SensorArray sa = new SensorArray(byteReader, logger);
 *          }
 *          
 *          This will create a SensorArray object called sa. It will be reading bytes from serial from the port COM3
 *          The Null logger is just a logger that will do nothing. For a complete list of all loggers and ByteReaders
 *          look under Skywalker.Utils
 *          
 *          There are two ways you can get the infrmation out of the SensorArray class, you can access its logger. Since the logger
 *          must be declared outside the scope of the SensorArray you will be able to call it from that scope. At the time of this writing
 *          (Feb 22 2016) there were no loggers or logmessages implemented speficially for the SensorArray. So logging is wanted, you will
 *          have to implement it. The chosen logger will be passed down to each SensorDevice in the array. It would be upto the individual
 *          Sensor to implement the logging logic with the LogEntry. For more information look at Skywalker.Utils.LogEntery and
 *          Skywalker.Utils.Logger
 *          
 *          At the time of this writing (Feb 22 2016) there were two types of sensors in the SensorArray. They are the IMU sensor for a compass
 *          and the sonar sensor for distance. To access this data, you can use a logger if one has been implemented.
 *          
 *          Alternatively you can access the values directly.
 *          Examples:
 *              
 *              for the IMU sensor
 *              if(true == sa.CompassDevice.XUpdated) {
 *
 *                  float[] a = sa.CompassDevice.Orientation; 
 *                  Console.WriteLine("IMU:");
 *                  Console.WriteLine(sa.CompassDevice.X.ToString());
 *                  Console.WriteLine(sa.CompassDevice.Y.ToString());
 *                  Console.WriteLine(sa.CompassDevice.Z.ToString());
 *                  Console.WriteLine("\n");
 *              }
 *              
 *              Alternatively for the IMU you can use sa.CompassDevice.Orientation which returns a float[]
 *              where X is the element 0, Y is 1 and Z is 2.
 *              
 *              An example for the distance sensor
 *               if(true == sa.SonarArrayDevice.getUpdatedAt(i)) {
 *                  Console.WriteLine("Sonar sensor " + i.ToString() +":");
 *                  Console.WriteLine(sa.SonarArrayDevice.getDistanceAt(i).ToString());
 *                  Console.WriteLine("\n");
 *              }
 *              
 *              In this example, i is an index into the list of sonar sensors. At the time of this writing there was support
 *              for 8 sensors labled 0 - 8. Alternativly you can use: sa.SonarArrayDevice.getAllDistance(). However, this will
 *              involve copying the whole array. Since this could be a computationally expensive operation, it is possible this
 *              will leave some of the distance sensors without the newest infromation. If possible it would be better if you were
 *              able to write your code to only need data from one sensor at a time.
 * 
 */


namespace Skywalker.Sensors.SensorArray {

    //This will contain code to interact with the sensor slave arduino
    public class SensorArray {

        private Thread _get_data_thread; //thread to spin off and read from the ByteReader
        protected IByteReader _b_reader;
        volatile protected ILogger _logger;
        private List<SensorDevice> _devices;
        private bool _running;
        private byte[] _packet;

        public SensorArray(IByteReader b_reader, ILogger logger) {

            _b_reader = b_reader;
            _logger = logger;

            //set up devices
            _devices = new List<SensorDevice>();//need to get devices into this list
            //here is where you will declare all of the sensors:
            _devices.Add(new Compass(logger));
            _devices.Add(new SonarArray(logger));

            _packet = new byte[255];
            _running = true;
            _get_data_thread = new Thread(ReadLoop);
            _get_data_thread.Start();
        }

        //a property per sensor is necessary to get the information out
        //unless using the logger

        
        public Compass CompassDevice {
            get { return (Compass)_devices[0]; }
        }

        public SonarArray SonarArrayDevice {
            get { return (SonarArray)_devices[1]; }
        }

        //so you can read and interact with sensor
        public SensorDevice GetSensorAt(int i) {
            return _devices[i];
        }

        //Thread spun up in constructor will be running this loop
        //This loop will read until Ir recieves
        private void ReadLoop() {
            int packet_type = -1;
            int expected_size = 255;
            bool packet_start = false;
            int total_received = 0;

            while(_running) {

                byte b = _b_reader.getData();//read one byte from the bytereader
                total_received++;

                //see if we are at the start of a packet (seeing the control byte and have not started a packet)
                if((byte)0x0A == b && false == packet_start) {
                    total_received = 1;
                    packet_start = true;
                } else if(true == packet_start && 2 == total_received) {//we need to get the expected size
                    expected_size = (int)b;
                } else if(true == packet_start && 3 == total_received) {//we need to get the Sensor Descriptor
                    packet_type = (int)b;
                }

                if(false == packet_start && total_received > 0) { //if we have not started a packet, reset the total_received
                                                                    //this way we can get all of the info for a new packet
                    total_received = 1;
                }

                //regardless of byte contents store byte in packet
                _packet[total_received - 1] = b;

                //size does not count the control byte or the packet size
                if(expected_size == (total_received - 2)) {

                    //check for valid packet
                    if(packet_type < _devices.Count() && packet_type >= 0) {//else invalid packet, reset
                        _devices[packet_type].Process(_packet);
                        //TODO: add logging using the ILogger here
                    }

                    //reset for next packet
                    Array.Clear(_packet, 0, total_received);
                    packet_type = -1;
                    expected_size = 255;
                    packet_start = false;
                    total_received = 0;

                }
            } //end while(running)

        } // end of ReadLoop function
        

    } // end of class SensorArray
} // end of namespace
