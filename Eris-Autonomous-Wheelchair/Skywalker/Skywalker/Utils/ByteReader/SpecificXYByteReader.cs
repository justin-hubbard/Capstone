using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skywalker.Utils.ByteReader {
    //MARVALMIND SPECIFIC
    //generates mock data for the Room UI Tool tests
	
	//However this can be useful for any situation where you want to
	//have specific X and Y's read out by MarvelMind
    //In order to use this byte reader you must make a list of Tuple<ushort, ushort>. The fist
    //Item in the Tuple is the x and the second is the corresponding y.
    //the constructor will consume this list and genearate a list of all the bytes needed
    //to make the required number of packets with the given number of x y pairs
    //Then the object can be given to MarvelMind and the system will read these packets
    class SpecificXYByteReader:IByteReader {

        private List<byte> _packet_data;
        int _byte_index;

        public SpecificXYByteReader(List<Tuple<ushort, ushort>> points) {

            _packet_data = new List<byte>();

            //create the data
            for(int i = 0; i < points.Count; i++) {
                //first we have to add the five header bytes
                _packet_data.Add(0xFF);
                _packet_data.Add(0x47);
                _packet_data.Add(0x01);
                _packet_data.Add(0x00);
                _packet_data.Add(0x10);

                //since the bytes for the rest of the packed don't matter for this
                //test we will just fill them up
                for(int j = 5; j < 9; j++) {
                    _packet_data.Add(0x1);
                }

                //now we need to put in the requested coordinate points
                //split the ushort into the two bytes
                byte upper_x = (byte)(points[i].Item1 >> 8);
                byte lower_x = (byte)(points[i].Item1 & 0xFF);
                byte upper_y = (byte)(points[i].Item2 >> 8);
                byte lower_y = (byte)(points[i].Item2 & 0xFF);

                //save the locations in the packet
                _packet_data.Add(lower_x); //byte 9
                _packet_data.Add(upper_x); //byte 10
                _packet_data.Add(lower_y); //byte 11
                _packet_data.Add(upper_y); //byte 12

                //fill out the rest of the packet with padding since its values
                //do not matter to us
                for(int j = 13; j < 23; j++) {

                    _packet_data.Add(0x1);
                }
            }//end for each tuple in points

            //at this point we have a list of bytes that contains all the information for the
            //packets that we want to test with

            _byte_index = 0;
        }//end constructor

        ~SpecificXYByteReader() {

        }

        virtual public byte getData() {
            //make sure we have not read over the size of the packets
            if(_byte_index < _packet_data.Count) {
                //get a byte from the list
                byte b = _packet_data[_byte_index];
                //advance list data
                _byte_index++;
                //give out the byte
                return b;
            } else { 
            
                return 0;
            }
        }

        //make sure we still have bytes to read
        public bool bytesToRead() {

            if(_byte_index < _packet_data.Count) { 
            
                return true;
            } else { 
            
                return false;
            }
        }
    }
}
