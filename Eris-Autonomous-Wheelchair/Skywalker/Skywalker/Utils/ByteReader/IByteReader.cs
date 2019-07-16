using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skywalker.Utils.ByteReader {
    public interface IByteReader {
        //This interface is meant to take a source and return a byte as it is requested by an outside source

        //implement this specific to where the source of information is coming from
        byte getData();

        //if we have bytes to read. IE information is available
        bool bytesToRead();
    }
}
