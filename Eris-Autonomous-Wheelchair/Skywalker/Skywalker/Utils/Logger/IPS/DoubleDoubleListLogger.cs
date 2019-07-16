using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skywalker.Utils.Logger.General;
using Skywalker.Utils.LEntry;

namespace Skywalker.Utils.Logger.IPS {
    //this is a logger that will create a list of Tuple<double, double>
    //The idea is the marvel mind will read through the bytes given to it and construct
    //a list of double double tuples, the first element of the tuple is the x coordinate and the
    //second element is the y coordinate read by the packet from MarvelMind.
    //This is meant for testing purposes

    //to use this logger just construct and pass into the MarvelMind object,
    //let it run:
    //MarvelMind mm = new MarvelMind(byte_reader, DoubleDoubleListLogger)
    //while(mm.Bytes_to_Read){
    // continue;
    //}
    //List<Tuple<double, double>> your_list = logger.Values
    //run your tests on the list

    class DoubleDoubleListLogger : ILogger{

        private List<Tuple<double, double>> _list;

        //nothing needed for constructor
        public DoubleDoubleListLogger(){
        
            _list = new List<Tuple<double,double>>();
        }

        //incase you need to reset the list easily
        ~DoubleDoubleListLogger() { 
        
            _list = null;
        }

        //get the values saved in the list
        public List<Tuple<double, double>> Values {

            get { return _list; }
        }

        //Called by position system to store x and y coordinates
        //does need the TwoIntLogEntry
        public void write(LogEntry log_entry) {

            TwoIntLogEntry ti_entry = (TwoIntLogEntry)log_entry;
            Tuple<int, int> msg = ti_entry.Message;
            Tuple<double, double> tmp = new Tuple<double,double>((double)msg.Item1, (double)msg.Item2);
            _list.Add(tmp);
        }
    }
}
