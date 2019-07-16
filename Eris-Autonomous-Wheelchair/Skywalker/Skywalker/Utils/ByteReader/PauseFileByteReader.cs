using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skywalker.Utils;
using System.Threading;

namespace Skywalker.Utils.ByteReader {
    //MARVALMIND SPECIFIC (well, not entierly, but ment to test MarvelMind)
    //this is a bytereader ment to test the marvalmind system and how it handles pauses
    //reads in a file and will return data after a delay
    class PauseFileByteReader : FileByteReader {


        Random rnd;
        public PauseFileByteReader(string filename) : base(filename){ 
        
            rnd = new Random();
        }

        override public byte getData() { 
        
            int  millisec = rnd.Next() % 200;

            Thread.Sleep(200);
            return base.getData();
        }
    }
}
