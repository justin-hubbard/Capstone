using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skywalker.Driver
{
    public class NavigationStrategyEndedEventArgs : EventArgs
    {
        // Whether or not the strategy was able to successfully complete its task
        public bool IsComplete
        {
            get;
            private set;
        }

        public int Step
        {
            get;
            private set;
        }

        public NavigationStrategyEndedEventArgs(bool isComplete, int step)
        {
            this.IsComplete = isComplete;
            this.Step = step;
        }
    }
}
