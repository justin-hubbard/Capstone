using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skywalker.Utils.LEntry;
using System.IO;

namespace Skywalker.Utils.Logger.General {
    //writes string Log messages to a file
    class FileLogger:ILogger {

        private StreamWriter _file;

        //pass in file name you want to save to
        public FileLogger(string file_name) {

            _file = new StreamWriter(file_name);
        }

        ~FileLogger() {

            if(_file != null) {
                _file.Close();
            }
        }

        public void Close() {

            if(_file != null) {
                _file.Close();
            }
        }

        //write the entry to the file
        //each entry will be a line
        //so the file will be in the format:
        //log message 0
        //log message 1
        //log message 2
        //log message 3
        //...
        public void write(LogEntry log_entry) { 
        
            StringLogEntry slog_entry = (StringLogEntry)log_entry;
            _file.WriteLine(slog_entry.Message);
        }
    }
}
