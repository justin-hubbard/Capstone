using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skywalker.Utils.LEntry;

namespace Skywalker.Utils.Logger.General
{
    //basic interface for loggers
    public interface ILogger
    {
        //logger must be able to write a log entry
        //loggers can be implemented to write to many different sources
        //log entries can be implemented to store information in many different formats
        void write(LogEntry log_entry);
    }
}
