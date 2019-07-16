using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skywalker.Utils.Logger.General;
using Skywalker.Utils.LEntry;

namespace Skywalker.Utils.Logger.General
{
    //This is a logger that is entirly inteded to do nothing
    //This will be usefull if you wish to just use the built in
    //X and Y from the position system
    class NullLogger : ILogger
    {
        //nothing to construct
        public NullLogger(){
        
        }
        //nothing to destruct
        ~NullLogger() { 
        
        }

        //do nothing, hence the null logger
        public void write(LogEntry log_entry) {

            return;
        }
    }
}
