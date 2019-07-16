using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skywalker.Utils.LEntry {
    //Log entry that stores two ints
    //pretty straightforward, just stores two intLogEntries and returns them together as a Tuple of two ints
    class TwoIntLogEntry : LogEntry {
     
        
        private IntLogEntry _log_e_1;
        private IntLogEntry _log_e_2;

        public TwoIntLogEntry(IntLogEntry le1, IntLogEntry le2) { 
        
            _log_e_1 = le1;
            _log_e_2 = le2;
        }

        public Tuple<int, int> Message {

            get {
            
                Tuple<int, int> t = new Tuple<int, int>(_log_e_1.Message, _log_e_2.Message);
                return t;
            }
        }
    
    }
}
