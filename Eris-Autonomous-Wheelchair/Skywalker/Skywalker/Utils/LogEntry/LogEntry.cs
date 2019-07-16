using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skywalker.Utils.LEntry
{
    //abstract base class for log entry
    public abstract class LogEntry
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = Name; }
        }

        //To use the LogEntry you will need to implement a logEntry of a specific type
        //eg a string long entry
    }
}
