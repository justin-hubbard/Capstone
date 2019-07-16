using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skywalker.Utils.ByteReader {
    //MarvelMind Specific
    //yet another way to generate mock data for testing
    //but this time, its *RANDOM*!!!!!!!!
        //well within a range

    //basically you just pass in the range you want and let it run

    class RandomByteReader : IByteReader {

        private byte[] _packet;
        private int _packet_index;
        private int _x_max;
        private int _x_min;
        private int _y_max;
        private int _y_min;
        Random rand;

        public RandomByteReader(int max, int min) { 
        
            _x_max = _y_max = max;
            _x_min = _y_min = min;
            _packet = new byte[23];
            _packet_index = 0;
            rand = new Random();


            //initalize packet, so when we generate a new one all we do is
            //create the new x and y's
            initalizePacket();

            //generate the first packet
            generatePacket();
        }

        public RandomByteReader(int x_max, int x_min, int y_max, int y_min) {
            _x_max = x_max;
            _x_min = x_min;
            _y_max = y_max;
            _y_min = y_min;
            _packet = new byte[23];
            _packet_index = 0;
            rand = new Random();


            //initalize packet, so when we generate a new one all we do is
            //create the new x and y's
            initalizePacket();

            //generate the first packet
            generatePacket();
        }
        
        ~RandomByteReader() { 
        
            _packet = null;
            rand = null;
        }

        //initalize packet to have full 23 bytes
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
        
            ushort x = (ushort)rand.Next(_x_min, _x_max);
            ushort y = (ushort)rand.Next(_y_min, _y_max);

            byte upper_x = (byte)(x >> 8);
            byte lower_x = (byte)(x & 0xFF);
            byte upper_y = (byte)(y >> 8);
            byte lower_y = (byte)(y & 0xFF);

            _packet[9] = lower_x;
            _packet[10] = upper_x;
            _packet[11] = lower_y;
            _packet[12] = upper_y;
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
