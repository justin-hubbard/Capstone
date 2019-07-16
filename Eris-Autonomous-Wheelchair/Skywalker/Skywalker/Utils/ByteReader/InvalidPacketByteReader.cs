using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skywalker.Utils;

namespace Skywalker.Utils.ByteReader {
    //MARVALMIND SPECIFIC
    //This is a bytereader for testing, it creates invalid header packest to make sure
    //the MarvelMind system will not read them in as valid packets
    class InvalidPacketByteReader : IByteReader {

        //stores number of bytes left
        private int _num_bytes;
        //current number of bytes sent
        private int _num_sent;
        Random rnd;
        private byte [] _packet;

        //these are a collection of different incorrect headers which will be randomly chosen from
        //for each packet
        byte [,] headers = {
                       { 0xff, 0x47, 0x01, 0x00, 0x11 },
                       { 0xff, 0x47, 0x01, 0x00, 0x7 },
                       { 0xff, 0x47, 0x01, 0x5, 0x10 },
                       { 0xff, 0xaa, 0x01, 0x00, 0x10 },
                       { 0xff, 0x47, 0x51, 0x00, 0x10 },
                       { 0xa3, 0x47, 0x01, 0x00, 0x10 },
                       { 0xaa, 0xbb, 0x11, 0x23, 0x81 }
                       };

        //takes the number of packets you want to test
        public InvalidPacketByteReader(int n){ 
       
            rnd = new Random();
            _num_bytes = n * 23;
            _num_sent = 0;
        }

        private void fill_header(int header) {

            for(int i = 0; i < 5; i++) { 
            
                _packet[i] = headers[header, i];
            }
        }

        public byte getData() {

            //if num_sent % 23 == 0, then we need a new packet
            if((_num_sent % 23) == 0){
                _packet = new byte [23];
                //we need to fill the packet buffer
                int header = rnd.Next() % 6;
                fill_header(header);
                //now we generated the reast of the header
                for(int i = 5; i < 23; i++){
                
                    byte [] by = new byte[1];
                    if(255 == by[0]) {
                        //make sure we can't randomly have an accidentally correct packet
                        by[0] = 0;
                    }
                    rnd.NextBytes(by);
                    _packet[i] = by[0];
                }
            }

            byte b = _packet[_num_sent % 23];
            _num_sent++;
            _num_bytes--;
            return b;
        }

        public bool bytesToRead() {

            if(_num_bytes <= 0) { 
            
                return false;
            }

            return true;
        }
    }
}
