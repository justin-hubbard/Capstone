using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skywalker.Utils.LEntry {
    //A log entry for an integer number
    //pretty straight forward, construct it by passing in and int
    //the message is that int
    class IntLogEntry : LogEntry {
    
        private int _message;

        public IntLogEntry(int i) { 
        
            _message = i;
        }

        public int Message {

            get {return _message; }
        }
    }
}
