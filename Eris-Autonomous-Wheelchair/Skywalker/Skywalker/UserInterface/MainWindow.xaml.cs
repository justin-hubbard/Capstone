using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Skywalker.Driver;
using Skywalker.Input;
using Skywalker.Mapping;
using Skywalker.ObjectDetection;
using Skywalker.UserInterface.Overlays;
using Skywalker_Vision.Kinect;
using TETCSharpClient;
using TETCSharpClient.Data;
using Microsoft.Win32;
using Image = System.Drawing.Image;
using Skywalker.Localization;
using Skywalker.Sensors.SensorArray;

namespace Skywalker.UserInterface
{
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IGazeListener
    {
        WriteableBitmap backgroundSource;
        //private ImageSource backgroundSource;
        private static readonly int FRAMERATE = 2;

        private OverlayRenderer overlayRenderer;

        //bool on whether or not we are listening to eyetribe
        private bool EyeGazeListening = false;
        private bool EyesOnScreen = false;

        private Pose CurrentPose;

        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="cartographer">Instance of the cartographer</param>
        /// <param name="eyetribeConnected">Whether an EyeTribe is connected</param>
        /// <param name="controllerConnected">Whether an Xbox 360 controller is connected</param>
        public MainWindow(Pose pose, ObstacleMap obstacleMap, ICartographer cartographer = null, INavigator nav = null, IDoorDetector doorDetector = null, OverlayRenderer overlayRenderer = null, bool eyetribeConnected = false, bool controllerConnected = false, SensorArray senorArray = null)
        {
            //Only done here because it is passed to this function
            Data.EyeTribeConnected = eyetribeConnected;
            Data.XboxConnected = controllerConnected;
            Data.Cartographer = cartographer;
            Data.Navigator = nav; 

            InitializeComponent();

            this.doorsControl.DoorDetector = doorDetector;

            this.CurrentPose = pose;
            this.doorsControl.SensorInfo = senorArray;
            this.overlayRenderer = overlayRenderer;
            this.NavigationTab.SetPose(this.CurrentPose);

            ConfigureOverlays();

            Input.Input.DeviceChanged += Input_DeviceChanged;
        }

        //Add EyeGaze listeners if eyetibe is selected as input device
        void Input_DeviceChanged(object sender, EventArgs e) {
            DeviceChangedEventArgs args = (DeviceChangedEventArgs)e;
            string device = args.DeviceName;

            if (device == "EyeTribe")
            {
                //if eyetribe and not listening, turn on listening
                if (!EyeGazeListening)
                {
                    Data._SmoothMouse = new SmoothMouse(10);
                    GazeManager.Instance.Activate(GazeManager.ApiVersion.VERSION_1_0, GazeManager.ClientMode.Push);
                    GazeManager.Instance.AddGazeListener(this);
                    EyeGazeListening = true;
                    NavigationTabs.SelectedIndex = 0;
                }
            }
            else
            {
                //if not eyetribe and listening, turn off listening
                if (EyeGazeListening)
                {
                    GazeManager.Instance.Deactivate();
                    GazeManager.Instance.RemoveGazeListener(this);
                    EyeGazeListening = false;
                }
            }
        }

        /// <summary>
        /// Handles EyeGaze updates.
        /// On update, adjust the cursor position
        /// to the smoothed EyeTribe position.
        /// </summary>
        /// <param name="gazeData"></param>
        public void OnGazeUpdate(GazeData gazeData)
        {
            double gX = gazeData.SmoothedCoordinates.X;
            double gY = gazeData.SmoothedCoordinates.Y;
            Win32.POINT pt = new Win32.POINT();
            SmoothMouse smoothMouse = Data.SmoothMouse;
            if (gX > 0 && gY > 0)
            {
                EyesOnScreen = true;
                smoothMouse.Add(gX, gY);
                Win32.SetCursorPos(smoothMouse.IntX, smoothMouse.IntY);
                //Win32.SetCursorPos(Convert.ToInt32(gX), Convert.ToInt32(gY));
            }
            else if (EyesOnScreen)
            {
                EyesOnScreen = false;
                Win32.SetCursorPos(0, 0);
            }
        }

        /// <summary>
        /// Handles window resizing by resizing all child objects.
        /// </summary>
        /// <param name="sender">Sending Object</param>
        /// <param name="e">Size Change Event Arguments</param>
        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Console.WriteLine("Height: {0}, Width:{1}", this.Height, this.Width);
            visualInputGrid.UpdateBounds();
        }

        private void UpdateBackgroundFrame()
        {
            const DispatcherPriority priority = DispatcherPriority.Render;
            while (true)
            {
                BaseFrame frame = Data.BackgroundStream.GetFrame();
                WriteableBitmap frameBitmap = null;
                if (frame != null) {
                    frameBitmap = frame.GetBitmap();
                }

                if (frameBitmap == null)
                {
                    StreamImage.Dispatcher.Invoke(() =>
                    {
                        if (StreamImage.Source == null)
                        {
                            //otherwise send a blank frame
                            StreamImage.Source = new WriteableBitmap(800, 600, 96.0, 96.0, PixelFormats.Pbgra32, null);
                        }
                    }, priority);
                    //otherwise don't update frame
                }
                else
                {
                    StreamImage.Dispatcher.Invoke(() =>
                    {
                        StreamImage.Source = frameBitmap;
                    }, priority);

                    OverlayCanvas.Dispatcher.Invoke(() =>
                    {
                        overlayRenderer.RenderOverlays(StreamImage.ActualWidth, StreamImage.ActualHeight);
                    });
                }

                Thread.Sleep(1000/FRAMERATE);
            }
        }
        
        /// <summary>
        /// Initializes the UI when the main window is loaded.
        /// If an EyeTribe is connected, this is when we activate it.
        /// </summary>
        /// <param name="sender">Sending object</param>
        /// <param name="e">Routed Event Arguments</param>
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            visualInputGrid.Initialize();
            Data.FrameUpdateThread = new Thread(UpdateBackgroundFrame);
            Data.FrameUpdateThread.Name = "Stream Update Thread";
            Data.FrameUpdateThread.Start();
            //OpenFileDialog ofd = new OpenFileDialog();
            //ofd.Filter = "XML | *.xml";
            //if (ofd.ShowDialog() == true)
            //    Data.Cartographer.Load(ofd.FileName);
        }

        private void ConfigureOverlays()
        {
            this.overlayRenderer.OverlayCanvas = this.OverlayCanvas;

            foreach (Overlay overlay in this.overlayRenderer.Overlays)
            {
                CheckBox checkBox = new CheckBox();
                checkBox.IsChecked = overlay.Enabled;
                checkBox.FontSize = 12;
                checkBox.Name = overlay.Name;
                checkBox.Content = overlay.Name;
                
                checkBox.Checked += overlayCheckBox_Checked;
                checkBox.Unchecked += overlayCheckBox_Unchecked;

                this.OverlayList.Children.Add(checkBox);
            }
        }

        void overlayCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            OverlayCheckboxCheckChanged(sender as CheckBox, false);
        }

        void overlayCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            OverlayCheckboxCheckChanged(sender as CheckBox, true);
        }

        private void OverlayCheckboxCheckChanged(CheckBox sender, bool isChecked)
        {
            Overlay overlay = this.overlayRenderer.Overlays.Find(obj => obj.Name == sender.Name);
            overlay.Enabled = isChecked;
        }
    }
}
