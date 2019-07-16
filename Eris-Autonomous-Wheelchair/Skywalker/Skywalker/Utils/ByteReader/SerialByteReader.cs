using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

namespace Skywalker.Utils.ByteReader {
    //This class will be used to represent a serial port
    //it connects to the serial port and will be called to get data
    //it returns a single byte each time, if the byte is available
    class SerialByteReader : IByteReader{

        SerialPort _port;

        public SerialByteReader(string com = "COM1", int baud = 9600) {

            _port = new SerialPort();
            _port.BaudRate = baud;
            _port.PortName = com;

            int attempts = 20; //max attempts before saying we failed

            while(!_port.IsOpen) {

                try {
                    _port.Open();
                } catch {
                    attempts--;
                    if(attempts < 0) {

                        throw new Exception("unable to open serial port " + com);
                    }
                }

                continue;
            }

        }

        ~SerialByteReader() {

            _port.Close();
        }

        //This will block on waiting for bytes to read. DO NOT use this on a main thread that cannot be blocked
        public byte getData() {
            byte[] byte_buf = new byte[1];//declare buf to read bytes into, we read one byte at
            //a time, not the most efficient, but that is how MarvelMind
            //wrote their code, so for now that is how we do it.

            while(_port.BytesToRead <= 0) { //wait until we have bytes to read
                Thread.Sleep(200);
                continue;
            }

            _port.Read(byte_buf, 0, 1);
            //Console.WriteLine(byte_buf[0]);
            return byte_buf[0];
        }

        public bool bytesToRead() {

            if(0 < _port.BytesToRead) {

                return true;
            } else {
                return false;
            }
        }
    }
}
