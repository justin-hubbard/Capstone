using System.Collections.Generic;
using System.Threading;
using System.Windows.Media.Imaging;
using Skywalker.Driver;
using Skywalker.Input;
using Skywalker.Mapping;
using Skywalker_Vision.Kinect;


namespace Skywalker.UserInterface {
    public static class Data {
        //private getter/setter and public getter
        internal static List<IImageStream> _streams { get; set; }
        public static List<IImageStream> streams { get { return _streams; } }

        internal static IImageStream _BackgroundStream { get; set; }
        public static IImageStream BackgroundStream { get { return _BackgroundStream; } }

        //Delay before we move from gaze input
        internal static int _GazeDelay { get; set; }
        public static int GazeDelay { get { return _GazeDelay; } }

        public static bool Driving { get; set; }

        public static Thread FrameUpdateThread;

        public static IGrid Grid { get; set; }
        /// <summary>
        /// The mouse smoothing object.
        /// </summary>
        internal static SmoothMouse _SmoothMouse { get; set; }
        public static SmoothMouse SmoothMouse { get { return _SmoothMouse; } }

        /// <summary>
        /// Last bitmap so if we are missing a frame, we can redraw with this
        /// </summary>
        public static WriteableBitmap LastBitmap { get; set; }

        /// <summary>
        /// Instance of the cartographer/map
        /// </summary>
        public static ICartographer Cartographer { get; set; }

        /// <summary>
        /// Instance of the navigator
        /// </summary>
        public static INavigator Navigator { get; set; }

        //when merged with master, use the Skywalker.Resources file
        //--------------------------------------------------------
        /// <summary>
        /// Indicates whether or not the EyeTribe
        /// device is connected to the machine,
        /// allowing for EyeTribe control.
        /// If an EyeTribe is not connected, 
        /// controls will be by mouse instead.
        /// </summary>
        public static bool EyeTribeConnected { get; set; }
        public static bool XboxConnected { get; set; }
    }
}
