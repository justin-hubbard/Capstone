using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skywalker.Motor
{
    public interface ISerialDevice : IDisposable
    {
        int BaudRate { get; set; }

        bool IsOpen { get; }

        string PortName { get; set; }

        int ReadTimeout { get; set; }

        int WriteTimeout { get; set; }

        void Close();

        void Open();

        int Read(byte[] buffer, int offset, int count);
        void Write(byte[] buffer, int offset, int count);
    }

}
