using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Skywalker.Driver;

namespace Skywalker.ObjectDetection
{
    /// <summary>
    /// An event that describes a detected object.
    /// </summary>
    /// <param name="sender">
    /// The sending object.
    /// </param>
    /// <param name="e">
    /// The event arguments.
    /// </param>
    public delegate void ObstaclesDetectedEvent(object sender, ObstaclesDetectedArgs e);

    /// <summary>
    /// Interface for the object detector.
    /// </summary>
    public interface IObstacleDetector
    {
        /// <summary>
        /// Event raised when an object is detected.
        /// </summary>
        event ObstaclesDetectedEvent ObstaclesDetected;

        /// <summary>
        /// Publishes data on the current obstacle state.
        /// </summary>
        List<Obstacle> ObstacleData { get; }

        /// <summary>
        /// Starts the obstacle detector
        /// </summary>
        void Start();
    }
}
