using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skywalker.Utils.LEntry;

namespace Skywalker.Utils.Logger.General {
    //Logger to strings
    //It will store them in a list that can be accessed by the Values property
    class StringListLogger : ILogger {

        private List<string> _list;

        //nothing to construct
        public StringListLogger() { 
        
            _list = new List<string>();
        }

        //incase you need to reset the list
        ~StringListLogger() {
        
            _list = null;
        }

        public List<string> Values {

            get {return _list; }
        }

        public void write(LogEntry log_entry) { 
        
            StringLogEntry slog_entry = (StringLogEntry)log_entry;
            _list.Add(slog_entry.Message);
        }
    }
}
