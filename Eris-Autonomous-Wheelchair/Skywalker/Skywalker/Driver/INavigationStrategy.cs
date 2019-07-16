using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Skywalker.Driver
{
    // A navigation strategy is a strategy that an INavigator should use in order to reach its target destination
    // Each strategy should have a specific task, for example "navigating around an obstacle" or "navigating through a door"

    public delegate void NavigationStrategyEndedEvent(object sender, NavigationStrategyEndedEventArgs eventArgs);

    public interface INavigationStrategy
    {
        // This event is fired when the strategy has completed running (successfully or not)
        event NavigationStrategyEndedEvent OnNavigationStrategyEnded;

        // Returns a normalized vector representing the direction the vehicle should drive
        Vector GetDirection();
    }
}
