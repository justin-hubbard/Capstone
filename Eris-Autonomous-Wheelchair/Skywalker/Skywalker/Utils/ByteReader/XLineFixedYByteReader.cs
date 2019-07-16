using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skywalker.Utils.ByteReader {
    class XLineFixedYByteReader : IByteReader {
        //MarvelMind Specific
        //yet another way to generate mock data for testing
        //but this time, it just goes back and forth along
        //a the x axis and the y will be constant
        //It will move in increments of 10
        //the x goes back and forth within a range

        private byte[] _packet;
        private int _packet_index;
        private int _x_max;
        private int _x_min;
        private int _y;
        private List<int> _x_values;
        private int _x_index;
        private bool _going_up;


        //basically you just pass in the range you want and let it run
        public XLineFixedYByteReader(int max, int min, int y) { 
        
            _x_max = max;
            _x_min = min;
            _y = y;
            _packet_index = 0;
            _going_up = false;
            _x_values = new List<int>();

            //create our list of x coordinates to move between
            for(int i = min; i <= max; i += 10){
            
                _x_values.Add(i);
            }

            _x_index = 0;
            _going_up = true;
            _packet = new byte[23];
            initalizePacket();
            generatePacket();
        }

        //initialize packet to have full 23 bytes
        //generatePacket will put the actual x and y
        //into the packet. So when we generate a new
        //packet all we have to do is make the 4 bytes
        //for the x and y coordinates
        private void initalizePacket() {

            //setup the header - see MarvelMind Documentation for header info
            _packet[0] = 0xFF;
            _packet[1] = 0x47;
            _packet[2] = 0x01;
            _packet[3] = 0x00;
            _packet[4] = 0x10;

            //fill in the rest of the packet with stuff
            //when a packet is generated we will overwrite
            //the positions for x and y
            for(int i = 5; i < 23; i++) {

                _packet[i] = 0;
            }
        }

        //what puts the actual data into the packet
        private void generatePacket() {

            ushort x = getNextXInt();
            ushort y = (ushort)_y;

            byte upper_x = (byte)(x >> 8);
            byte lower_x = (byte)(x & 0xFF);
            byte upper_y = (byte)(y >> 8);
            byte lower_y = (byte)(y & 0xFF);

            _packet[9] = lower_x;
            _packet[10] = upper_x;
            _packet[11] = lower_y;
            _packet[12] = upper_y;
        }

        private ushort getNextXInt(){
        
            ushort x = (ushort)_x_values[_x_index];
            if(true == _going_up) {
                _x_index++;
                if(_x_index == _x_values.Count) { 
                
                    //we need to flip direction
                    _x_index--;
                    _going_up = false;
                }
            } else { 
                _x_index--;
                if(_x_index == 0) { 
                
                    //we need to flip
                    _x_index++;
                    _going_up = true;
                }
            }//end else

            return x;
        }

        virtual public byte getData() {

            byte b = _packet[_packet_index];

            _packet_index++;

            //if we have sent a full packet
            if(23 == _packet_index) {

                //well if we have sent a full packet
                //then we get a new packet and we
                //reset the index to read from beginnning
                //of new packet
                generatePacket();
                _packet_index = 0;
            }

            return b;
        }

        public bool bytesToRead() {

            if(_packet == null) {

                return false;
            } else {

                return true;
            }
        }
    }
}
