using System.Windows;

namespace Skywalker.Input
{
    /// <summary>
    /// Interface for input devices.
    /// </summary>
    public interface IInputDevice
    {
        /// <summary>
        /// The direction that the user requests.
        /// </summary>
        /// <returns>
        /// True if forward is pressed, false otherwise.
        /// </returns>
        Vector InputDirection { get; }
    }
}
