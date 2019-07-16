using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skywalker.Utils.Logger.General;
using Skywalker.Utils.LEntry;

namespace Skywalker.Utils.Logger.IPS {
    class DoubleDoubleLogger : ILogger {

               private Object _lock;
        private Tuple<double, double> _cord;
        
        public DoubleDoubleLogger(){
        
            _lock = new Object();
            _cord = new Tuple<double, double>(-1, -1);
        }
        
        //nothing to destruct
        ~DoubleDoubleLogger() {
        
        }
        
        public void write(LogEntry log_entry){
        
            lock(_lock){
                TwoIntLogEntry ti_entry = (TwoIntLogEntry)log_entry;
                Tuple<int, int> msg = ti_entry.Message;
                _cord = new Tuple<double, double>((double)msg.Item1, (double)msg.Item2);
            }
        }
        
        public Tuple<double, double> Cord{
        
            get{
              lock(_lock){
                  return _cord;
              }
            }
        }
    }
}
