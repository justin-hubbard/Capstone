using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skywalker.Motor
{
    public class MockSerialDevice : ISerialDevice
    {
        private byte[] message;
        private byte[] response;
        private float xpercent;
        private float ypercent;
        private int zeroValue = 128;
        private int scaleValue = 62;
        public int BaudRate { get; set; }
        public bool IsOpen { get; private set; }
        public string PortName { get; set; }
        public int ReadTimeout { get; set; }
        public int WriteTimeout { get; set; }

        public MockSerialDevice(string portName, int baudRate) : this()
        {
            this.PortName = portName;
            this.BaudRate = baudRate;
            IsOpen = false;
        }


        public void Close()
        {
            IsOpen = false;
        }

        public void Open()
        {
            if (this.BaudRate < 1)
            {
                throw new ArgumentOutOfRangeException("BaudRate");
            }
            if (PortName.StartsWith("COM", false, null) && PortName.Length == 4)
            {
                if (PortName.EndsWith("1") || PortName.EndsWith("2") || PortName.EndsWith("3") || PortName.EndsWith("4"))
                {
                    IsOpen = true;
                }
                else if (PortName.EndsWith("5"))
                {
                    throw new UnauthorizedAccessException("COM5 already in use.");
                }
                else
                {
                    throw new IOException("Invalid COM port.");
                }
            }
            else
            {
                throw new ArgumentException("ERROR: Invalid Port Name. PortName must begin with COM followed by a number.");
            }

        }

        public int Read(byte[] buffer, int offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                buffer[offset + i] = message[i % 3];
            }
            calc_message();
            return count;
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                response[i % 3] = buffer[offset + i];
            }
        }

        public MockSerialDevice()
        {
            message = new byte[3];
            response = new byte[3];
            message[0] = (byte)250;
        }

        private void calc_message()
        {
            int x = response[1];
            int y = response[2];
            xpercent = input_to_percent(x);
            ypercent = input_to_percent(y);
            message[1] = (byte)calc_out(xpercent);
            message[2] = (byte)calc_out(ypercent);
        }

        private float input_to_percent(int b)
        {
            float val = b;
            val = val - 100;
            val = val / 100;
            return val;
        }

        private int calc_out(float percentage)
        {
            float voltage = zeroValue + (scaleValue * percentage);
            return (int)voltage;
        }

        public void Dispose() { }
    }
}
