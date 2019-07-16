using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Skywalker.Utils.ByteReader {
    class NullByteReader : IByteReader {

        private bool _bytes_to_read;

        //this will be a byte reader that returns 0's
        //mostly for some sort of testing or saftey
        public NullByteReader() { 
        
            _bytes_to_read = true;
        }

        ~NullByteReader() { }

        public byte getData() { 
        
            return 0;
        }

        public void setByteToRead(bool btr) { 
        
            _bytes_to_read = btr;
        }

        public bool bytesToRead() { 
            return _bytes_to_read;
        }
    
    }
}
