using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skywalker.Utils.Logger.General;
using Skywalker.Utils.LEntry;

namespace Skywalker.Utils.Logger.IPS {

    //To avoid memory issues of keeping an ever growing list, this logger can be used to
    //keep track of only the most recent x and y from the IPS system
    class IntIntLogger : ILogger{
    
        private Object _lock;
        private Tuple<int, int> _cord;
        
        public IntIntLogger(){
        
            _lock = new Object();
            _cord = new Tuple<int, int>(-1, -1);
        }
        
        //nothing to destruct
        ~IntIntLogger(){
        
        }
        
        public void write(LogEntry log_entry){
        
            lock(_lock){
                TwoIntLogEntry ti_entry = (TwoIntLogEntry)log_entry;
                Tuple<int, int> msg = ti_entry.Message;
                _cord = new Tuple<int, int>(msg.Item1, msg.Item2);
            }
        }
        
        public Tuple<int, int> Cord{
        
            get{
              lock(_lock){
                  return _cord;
              }
            }
        }
        
    }
}
