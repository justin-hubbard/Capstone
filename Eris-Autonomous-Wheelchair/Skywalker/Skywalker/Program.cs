using System;
using System.IO;
using System.IO.Ports;
using Skywalker.Driver;
using Skywalker.Localization;
using Skywalker.Input;
using Skywalker.Mapping;
using Skywalker.Resources;
using Skywalker_Vision.Kinect;
using Skywalker.Motor;
using Skywalker.ObjectDetection;
using System.Windows;
using Microsoft.Win32;
using Skywalker.UserInterface;
using Skywalker.UserInterface.Overlays;
using Skywalker.Odometry;
using Skywalker.Sensors.IPS;
using Skywalker.Utils.ByteReader;
using Skywalker.Utils.Logger.General;
using Skywalker.Utils.Logger.IPS;
using Skywalker.Sensors.SensorArray;


namespace Skywalker
{
    /// <summary>
    /// Main program class
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Program Main function,
        /// EXECUTION STARTS HERE.
        /// </summary>
        [STAThread]
        static void Main()
        {

            //change the test file xml here and make sure it is in bin.
            string startingXMLFile = "ETRL_03_07.xml";
            //string startingXMLFile = "FloorPlanTest.xml";
            //string startingOpenFile = "Sloan46_FINAL.xml";
            double compassToMapOffset = 72.0;

            // List of all the objects we'll be initializing.
            // Default to null, as some of them will only
            // be initialized conditionally e.g.
            // vehicle is only initialized if
            // there is a vehicle connected.
            SerialPort sp = null;
            IVehicle vehicle = null;
            IImageStream depthStream = null;
            IImageStream videoStream = null;
            IObstacleDetector obstacleDetector = null;
            ICartographer cartographer = null;
            IOdometer odometer = null;
            IInputDevice input = KeyboardInput.Instance;
            ObstacleMap obstacleMap = null;
            INavigator navigator = null;
            MainWindow mainWindow = null;
            Driver.Driver driver = null;
            VisualInputGridPresenter uiInputPresenter = null;
            ObstacleGridPresenter uiGridPresenter = null;
            PositioningSystem ips = null;
            OverlayRenderer overlayRenderer = null;
            SensorArray sensorArray = null;
            Pose pose = null;
            IDoorDetector doorDetector = null;
            ObstacleLocalizer obstacleLocalizer = null;
            SonarDetector sonarDetector = null;

            Config.Initialize();

            Devices.Initialize();
            //string wheelchair_com_port = Devices.IsWheelchairConnected();
            string wheelchair_com_port = "COM3";//Devices.FindComPort("Arduino Uno");
            bool WheelChairConnected = false;
            if (wheelchair_com_port != "")
            {
                WheelChairConnected = true;
            }
            //bool WheelChairConnected = true;
            bool EyeTribeConnected = true;// Devices.IsEyeTribeConnected();
            bool ControllerConnected = true;//Devices.IsControllerConnected();
            bool KinectConnected = true; //Devices.IsKinectConnected();

            //Console.WriteLine("Kinect Connected: {0}", KinectConnected.ToString());
            //Console.WriteLine("Eyetribe Connected: {0}", EyeTribeConnected.ToString());
            //Console.WriteLine("Wheelchair Connected: {0}", WheelChairConnected.ToString());
            //Console.WriteLine("Controller Connected: {0}", ControllerConnected.ToString());

            // Initialize vehicle and serial connection if
            // there is a vehicle connected.

            if (WheelChairConnected)
            {
                //sp = new SerialPort(wheelchair_com_port, 9600);
                vehicle = Wheelchair.Instance(wheelchair_com_port);
                vehicle.initializeOffset(Properties.Settings.Default.CalibrationOffset);
            }
            else
            {
                vehicle = new MockVehicle();

                MockVehicleWindow mockDisplay = new MockVehicleWindow((MockVehicle)vehicle);
                mockDisplay.Show();
            }

            //initalize IPS here

            IByteReader sensorReader = null;
            try
            {
                //sensorReader = new SerialByteReader("Arduino Mega");
                sensorReader = new SerialByteReader("COM4");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            if (sensorReader != null)
            {
                ILogger logger = new NullLogger();
                sensorArray = new SensorArray(sensorReader, logger);
                sonarDetector = new SonarDetector();
            }
            else
            {
                sensorReader = new NullByteReader();

                ILogger logger = new NullLogger();
                sensorArray = new SensorArray(sensorReader, logger);
            }

            IByteReader byteReader = null;
            try
            {
                //byteReader = new SerialByteReader(Devices.FindComPort("STMicroelectronics"));
                byteReader = new SerialByteReader("COM3");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            if (byteReader != null)
            {
                ILogger logger = new NullLogger();
                ips = new MarvelMind(byteReader, logger);
            }
            else
            {
                //Setup Mock IPS

                //IByteReader mockByteReader = new FileByteReader(@"C:\Users\Dana\Documents\Visual Studio 2013\Projects\UITests\UITests\Mock_IPS_Data.txt");
                //IByteReader mockByteReader = new XLineFixedYByteReader(800, 100, 200);
                IByteReader mockByteReader = new RandomByteReader(300, 299);
                ILogger mockLogger = new NullLogger();
                ips = new MarvelMind(mockByteReader, mockLogger);
            }

            //wait for an iteration of the IPS system that way
            //we can get the starting location
            while (false == ips.XUpdated && false == ips.YUpdated)
            {
                continue;
            }

            //Tuple<double, double> t = new Tuple<double, double>((double)ips.X, (double)ips.Y);
            Tuple<double, double> t = new Tuple<double, double>((double)ips.Y, (double)ips.X);
            mPoint startingLocationInXY = new mPoint(t);

            // UI, input, object detection, navigation, and driver are initialized always,
            // as they are not directly dependent on external hardware.
            obstacleMap = new ObstacleMap();
            //cartographer = new Cartographer(Directory.GetCurrentDirectory() + @"\FloorPlanTest.xml", ips);

            // might need to be changed later. If the location is not in the starting room. add staring room.
            //cartographer = new Cartographer(Directory.GetCurrentDirectory() + @"\FloorPlanTest.xml", startingLocationInXY);

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "XML | *.xml";
            ofd.Title = "Open Map";
            if (ofd.ShowDialog() == true)
            {
                startingXMLFile = ofd.FileName;
                Console.WriteLine(startingXMLFile);
            }

            cartographer = new Cartographer(startingXMLFile, startingLocationInXY);
            pose = new Pose(ips, cartographer.GetStartingRoom(), sensorArray.CompassDevice, compassToMapOffset);

            overlayRenderer = new OverlayRenderer();

            if (KinectConnected)
            {
                depthStream = DepthStream.Instance;
                videoStream = VideoStream.Instance;

                // Initialize depthstream if kinect is connected.
                obstacleDetector = new ObstacleDetector(depthStream, false);
                obstacleDetector.Start();

                obstacleLocalizer = new ObstacleLocalizer(pose);

                ObstaclesOverlay obstaclesOverlay = new ObstaclesOverlay(obstacleDetector);
                overlayRenderer.Overlays.Add(obstaclesOverlay);

                doorDetector = new DepthBasedDoorDetector();
                doorDetector.RunAsync(depthStream);

                try
                {
                    sonarDetector.RunAsync(sensorArray);
                }
                catch (Exception e)
                {

                }
            }

            // Obstacle detector and driver are only run
            // if both the vehicle and kinect are connected.
            if (vehicle != null && KinectConnected)
            {
                Vector startPoint = new Vector(7 / 0.75f, 43 / 0.75f);
                Vector startRotation = new Vector(-7, -43);
                startRotation.Normalize();
                odometer = new Odometer();

                navigator = new Navigator(obstacleMap, obstacleDetector, obstacleLocalizer, cartographer, doorDetector, pose, sonarDetector, sensorArray);
                driver = new Driver.Driver(input, vehicle, navigator);
                driver.Start();
            }

            mainWindow = new MainWindow(pose, obstacleMap, cartographer, navigator, doorDetector, overlayRenderer, EyeTribeConnected, ControllerConnected, sensorArray);
            uiInputPresenter = new VisualInputGridPresenter(mainWindow.visualInputGrid);

            //This starts the program.
            Application app = new Application();
            app.ShutdownMode = ShutdownMode.OnMainWindowClose;
            app.MainWindow = mainWindow;
            app.MainWindow.Show();
            app.Run();
        }
    }

}
