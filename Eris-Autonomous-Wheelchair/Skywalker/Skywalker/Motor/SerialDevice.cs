using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace Skywalker.Motor
{
    class SerialDevice : IDisposable, ISerialDevice
    {
        private SerialPort port;
        //private static SerialDevice instance;
        public const int DefaultBaudRate = 9600;

        public SerialDevice(string portName, int baudRate)
        {
            port = new SerialPort(portName, baudRate);
        }

        public SerialDevice()
        {
            var portName = SerialPort.GetPortNames()[0];
            port = new SerialPort(portName, SerialDevice.DefaultBaudRate);
        }
        /*
        public static SerialDevice Instance()
        {
            return instance ?? (instance = new SerialDevice());
        }

        public static SerialDevice Instance(string portName, int baudRate)
        {
            return instance ?? (instance = new SerialDevice(portName, baudRate));
        }
        */
        public void Dispose()
        {
            port.Dispose();
        }



        public event EventHandler Disposed
        {
            add { port.Disposed += value; }
            remove { port.Disposed -= value; }
        }

        public void Close()
        {
            port.Close();
        }

        public void DiscardInBuffer()
        {
            port.DiscardInBuffer();
        }

        public void DiscardOutBuffer()
        {
            port.DiscardOutBuffer();
        }

        public void Open()
        {
            port.Open();
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return port.Read(buffer, offset, count);
        }

        public int ReadChar()
        {
            return port.ReadChar();
        }

        public int Read(char[] buffer, int offset, int count)
        {
            return port.Read(buffer, offset, count);
        }

        public int ReadByte()
        {
            return port.ReadByte();
        }

        public string ReadExisting()
        {
            return port.ReadExisting();
        }

        public string ReadLine()
        {
            return port.ReadLine();
        }

        public string ReadTo(string value)
        {
            return port.ReadTo(value);
        }

        public void Write(string text)
        {
            port.Write(text);
        }

        public void Write(char[] buffer, int offset, int count)
        {
            port.Write(buffer, offset, count);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            port.Write(buffer, offset, count);
        }

        public void WriteLine(string text)
        {
            port.WriteLine(text);
        }

        public Stream BaseStream
        {
            get { return port.BaseStream; }
        }

        public int BaudRate
        {
            get { return port.BaudRate; }
            set { port.BaudRate = value; }
        }

        public bool BreakState
        {
            get { return port.BreakState; }
            set { port.BreakState = value; }
        }

        public int BytesToWrite
        {
            get { return port.BytesToWrite; }
        }

        public int BytesToRead
        {
            get { return port.BytesToRead; }
        }

        public bool CDHolding
        {
            get { return port.CDHolding; }
        }

        public bool CtsHolding
        {
            get { return port.CtsHolding; }
        }

        public int DataBits
        {
            get { return port.DataBits; }
            set { port.DataBits = value; }
        }

        public bool DiscardNull
        {
            get { return port.DiscardNull; }
            set { port.DiscardNull = value; }
        }

        public bool DsrHolding
        {
            get { return port.DsrHolding; }
        }

        public bool DtrEnable
        {
            get { return port.DtrEnable; }
            set { port.DtrEnable = value; }
        }

        public Encoding Encoding
        {
            get { return port.Encoding; }
            set { port.Encoding = value; }
        }

        public Handshake Handshake
        {
            get { return port.Handshake; }
            set { port.Handshake = value; }
        }

        public bool IsOpen
        {
            get { return port.IsOpen; }
        }

        public string NewLine
        {
            get { return port.NewLine; }
            set { port.NewLine = value; }
        }

        public Parity Parity
        {
            get { return port.Parity; }
            set { port.Parity = value; }
        }

        public byte ParityReplace
        {
            get { return port.ParityReplace; }
            set { port.ParityReplace = value; }
        }

        public string PortName
        {
            get { return port.PortName; }
            set { port.PortName = value; }
        }

        public int ReadBufferSize
        {
            get { return port.ReadBufferSize; }
            set { port.ReadBufferSize = value; }
        }

        public int ReadTimeout
        {
            get { return port.ReadTimeout; }
            set { port.ReadTimeout = value; }
        }

        public int ReceivedBytesThreshold
        {
            get { return port.ReceivedBytesThreshold; }
            set { port.ReceivedBytesThreshold = value; }
        }

        public bool RtsEnable
        {
            get { return port.RtsEnable; }
            set { port.RtsEnable = value; }
        }

        public StopBits StopBits
        {
            get { return port.StopBits; }
            set { port.StopBits = value; }
        }

        public int WriteBufferSize
        {
            get { return port.WriteBufferSize; }
            set { port.WriteBufferSize = value; }
        }

        public int WriteTimeout
        {
            get { return port.WriteTimeout; }
            set { port.WriteTimeout = value; }
        }

        public event SerialErrorReceivedEventHandler ErrorReceived
        {
            add { port.ErrorReceived += value; }
            remove { port.ErrorReceived -= value; }
        }

        public event SerialPinChangedEventHandler PinChanged
        {
            add { port.PinChanged += value; }
            remove { port.PinChanged -= value; }
        }

        public event SerialDataReceivedEventHandler DataReceived
        {
            add { port.DataReceived += value; }
            remove { port.DataReceived -= value; }
        }
    }
}
