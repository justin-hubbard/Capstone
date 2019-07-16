using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Skywalker.Utils.ByteReader {
    //This class will read from a file and give out
    //the file byte by byte.
    //return a byte from getByte if available.
    //if we are at the end of the file
    //bytes to read will be false
    class FileByteReader:IByteReader {

        private FileStream _fs;

        public FileByteReader(string filename) {

            _fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
        }

        ~FileByteReader() {

            _fs.Close();
        }

        virtual public byte getData() {

            byte b = 0;
            if(bytesToRead()) {

                b = Convert.ToByte(_fs.ReadByte());
            }

            return b;
        }

        public bool bytesToRead() {
            if(_fs.Position == _fs.Length) {

                return false;
            } else {

                return true;
            }
        }
    }
}
