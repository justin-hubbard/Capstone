using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skywalker.Utils;

namespace Skywalker.Utils.LEntry {
    class StringLogEntry : LogEntry {

        private string _message;

        public StringLogEntry(string message) { 
        
            _message = message;
        }

        public string Message {

            get {return _message; }
        }
    }
}
