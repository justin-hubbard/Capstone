using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skywalker.Utils.Logger.General;
using Skywalker.Utils.LEntry;

namespace Skywalker.Utils.Logger.IPS {

    //A logger that will display X and Y's from the IPS system to the Console
    //needs to have a TwoIntLogEntry given to it

    class ConsoleLogger : ILogger{

        //nothing needed for constructor
        public ConsoleLogger(){
        
        }

        //Nothing to Destruct
        ~ConsoleLogger() { 
        
        }

        //Called by position system to store x and y coordinates
        //does need the TwoIntLogEntry
        public void write(LogEntry log_entry) {

            TwoIntLogEntry ti_entry = (TwoIntLogEntry)log_entry;
            Tuple<int, int> msg = ti_entry.Message;
            Console.WriteLine("X: " + msg.Item1.ToString() + " Y: " + msg.Item2.ToString());

        }
    }
}
