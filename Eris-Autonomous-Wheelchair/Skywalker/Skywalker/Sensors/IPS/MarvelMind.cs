using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Ports;
using Skywalker.Utils.ByteReader;
using Skywalker.Utils.Logger.General;
using Skywalker.Utils.LEntry;

/*USAGE:
 * this class will interface with the hardware for 1 Marvelmind hedgehog
 * example of declaring:
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
 *              MarvelMind ips = new MarvelMind(byteReader, logger);
 *          }
 * 
 * 
 *          This will create a MavelMind object called ips. It will be reading bytes from serial from the port COM3
 *          The Null logger is just a logger that will do nothing. For a complete list of all loggers and ByteReaders
 *          look under Skywalker.Utils
 *          
 *          There are two ways you can get the infrmation out of the MarvelMind class, you can access its logger. Since the logger
 *          must be declared outside the scope of the MarvelMind you will be able to call it from that scope.
 *          
 *          However, the best way to get realtime updates of the position is from  the properties built into the PositioningSystem class
 *          Example:
 *          
 *          if(true == ips.XUpdated || true == ips.YUdated){
 *                  int x = ips.X;
 *                  int y = ips.Y;
 *          }
 *          
 *          This will return the most recent X or Y value. If you need both you can also alternatively use:
 *          ips.Position. Which will give you an int[] where x is element 0 and y is element 1
 */

namespace Skywalker.Sensors.IPS {
    public class MarvelMind:PositioningSystem {

        private bool _connected;//signals the Port is opened successfully
        private bool _running;
        private int _packet_size;
        private Thread _get_data_thread;

        //it takes a IByteReader declared somewhere else
        public MarvelMind(IByteReader b_reader, ILogger logger)
            : base(b_reader, logger) {

            _packet_size = 23;//23 byte packets. Given to us by MarvelMind documentation

            _running = true;
            _connected = true;

            _get_data_thread = new Thread(UpdatePositionLoop);
            _get_data_thread.Start();
        }

        ~MarvelMind() {
            _running = false;
        }

        public bool BytesToRead {
			//ideally we will use the byte reader, but there was some
			//hardware problems. So we sorta cheated a little
            //get { return _b_reader.bytesToRead(); }
            get { return true; }
        }

        public bool Connected {

            get { return _connected; }
        }

        public bool Running {

            get { return _running; }
        }

        //Finds the x,y position based on the serial input from the MarvelMind.
        //Runs in its own thread to be constantly updating
        //This code is a translation from the Arduino sketch given to us MarvelMind Robotics
        private void UpdatePositionLoop() {

            //header for packet, to make sure we are receiving things correctly
            //these values are all from the MarvelMind Serial Packet information
            byte[] headgehog_packet_header = { 0xff, 0x47, 0x01, 0x00, 0x10 };

            byte[] packet = new byte[30];//stores the actual packet with some extra room for saftey

            int total_reviced = 0;
            bool packet_recived = false;
            int packet_index = 0;
            int header_index = 0;

            while(_running) {

                byte b = _b_reader.getData();

                //read byte and store into byte_buf
                total_reviced++;

                //check to see if we are looking for the header still
                if(total_reviced <= 5) {
                    //if we have not read in the full header yet and the input value is not the value expected
                    //for the current header position
                    if(b != headgehog_packet_header[header_index]) {
                        //if we read a byte that does not belong in the header we restart   
                        header_index = 0;
                        packet_index = 0;
                        total_reviced = 0;
                        //continue; //loop back and try to make sure we start at a correct packet
                    } else {//we have a valid byte for the header, add it to the packet

                        packet[packet_index] = b;
                        header_index++;
                        packet_index++;
                    }
                } else {
                    //we have received the full header
                    packet[packet_index] = b;
                    packet_index++;
                }

                //check to see if we have received everything
                if(total_reviced == _packet_size) {//total size is 23

                    //if we got everything, let us know we received the packed, and reset offsets
                    packet_recived = true;
                    packet_index = 0;
                    header_index = 0;
                    total_reviced = 0;
                }

                if(true == packet_recived) {

                        //update the x and y
                        //get the bytes at positions 9 and 10 and set them to X
                        setX((short)(packet[9] + (packet[10] << 8)));
                        //get the bytes at positions 9 and 10 and set them to Y
                        setY((short)(packet[11] + (packet[12] << 8)));
                        IntLogEntry ie1 = new IntLogEntry(X);
                        IntLogEntry ie2 = new IntLogEntry(Y);
                        TwoIntLogEntry tie = new TwoIntLogEntry(ie1, ie2);
                        _logger.write(tie);
                        //This makes stuff work. DO NOT REMOVE UNLESS YOU KNOW WHAT YOU ARE DOING
						//....since we don't. But honestly with out it, this system doesn't seem
						//to work properly. Probably because it is reading too fast for info to be
						//processed by the other parts of the system
                        System.Threading.Thread.Sleep(10);
                    //reset packet received since just produced the packet and are waiting for the next one
                    packet_recived = false;
                }

                if(false == BytesToRead) {

                    _running = false;
                }

            } //while(_running)
        }//end updatePosition()

    }
}
