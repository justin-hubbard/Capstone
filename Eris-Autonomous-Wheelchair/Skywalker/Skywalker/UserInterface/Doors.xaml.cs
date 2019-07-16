using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Skywalker_Vision.Kinect;
using Skywalker.ObjectDetection;
using System.Threading;
using System.Windows.Threading;

using Skywalker.Sensors.SensorArray;

namespace Skywalker.UserInterface
{
    /// <summary>
    /// Interaction logic for Doors.xaml
    /// </summary>
    public partial class Doors : UserControl
    {
        private Thread DoorsBackgroundUpdateThread;
        private static readonly int FRAMERATE = 2;
        private IImageStream _imageStream;
        public IImageStream ImageStream
        {
            get
            {
                return _imageStream;
            }

            set
            {
                if (this.DoorDetector != null && this.DoorDetector.IsRunning)
                {
                    this.DoorDetector.CancelAsync();
                }

                _imageStream = value;

                if (_imageStream != null && this.DoorDetector != null)
                {
                    this.DoorDetector.RunAsync(_imageStream);
                }
            }
        }

        private IDoorDetector _doorDetector;
        public IDoorDetector DoorDetector
        {
            get
            {
                return _doorDetector;
            }

            set
            {
    
                if (_doorDetector != null)
                {
                   _doorDetector.OnDoorDetected -= DoorDetector_OnDoorDetected;
                }

                _doorDetector = value;

                if (_doorDetector != null)
                {
                    _doorDetector.OnDoorDetected += DoorDetector_OnDoorDetected;
                }
            }
        }

        private SensorArray _sensorArray;
        public SensorArray SensorInfo
        {
            get
            {
                return _sensorArray;
            }
            set
            {
                _sensorArray = value;
            }
        }

        public Doors()
        {
            InitializeComponent();
            this.DoorDetector = new DepthBasedDoorDetector();
            this.Run();
            ColorBasedButton.Checked += RadioButton_CheckedChanged; 
            DepthBasedButton.Checked += RadioButton_CheckedChanged; 
            InfraredBasedButton.Checked += RadioButton_CheckedChanged; 
            DoorsBackgroundUpdateThread = new Thread(UpdateBackground);
            DoorsBackgroundUpdateThread.Start();
            DoorsBackgroundUpdateThread.Name = "Door Detection Background Update Thread";
            DoorsBackgroundUpdateThread.Priority = ThreadPriority.Lowest;
        }

        private void RadioButton_CheckedChanged(object sender, RoutedEventArgs e)
        {
            // ... Get RadioButton reference.
            var button = sender as RadioButton;
            string detectionMethodStr = button.Content.ToString();
            if(detectionMethodStr == "ColorBased")
            {
                DoorDetector = new ColorBasedDoorDetector();
            }
            else if (detectionMethodStr == "DepthBased")
            {
                DoorDetector = new DepthBasedDoorDetector();
            }
            else if(detectionMethodStr == "InfraredBased")
            {
                DoorDetector = new InfraredBasedDoorDetector();
            }
            this.Run();
        }

        private void Run()
        {
            if (_doorDetector != null && _doorDetector.IsRunning)
            {
                _doorDetector.CancelAsync();
            }

            ColorBasedDoorDetector cDetector = _doorDetector as ColorBasedDoorDetector; 
            if(cDetector != null)
                _imageStream = VideoStream.Instance;
            else
            {
                DepthBasedDoorDetector dDetector = _doorDetector as DepthBasedDoorDetector;
                if (dDetector != null)
                    _imageStream = DepthStream.Instance;
                else
                {
                    InfraredBasedDoorDetector iDetector = _doorDetector as InfraredBasedDoorDetector;
                    if (iDetector != null)
                        _imageStream = InfraredStream.Instance;
                }
            }

            if (_imageStream != null && this.DoorDetector != null)
            {
                _doorDetector.RunAsync(_imageStream);
            }
        }
        void DoorDetector_OnDoorDetected(object sender, DoorDetectedEventArgs eventArgs)
        {
            this.Dispatcher.Invoke(() =>
            {
                List<DetectedDoor> doors = eventArgs.DetectedDoors;
                DetectedDoor.DetectMethod method = eventArgs.DetetMethod;
                this.OverlayCanvas1.Children.Clear();
                DrawDoors(doors, this.OverlayCanvas1);
            });
        }

        //Render the background
        private void UpdateBackground()
        {
            //const DispatcherPriority priority = DispatcherPriority.Render;
            while (true)
            {
                BaseFrame frame = null;
                WriteableBitmap frameBitmap = null;

                ColorBasedDoorDetector cDetector = _doorDetector as ColorBasedDoorDetector;
                if (cDetector != null)
                {
                    frame = VideoStream.Instance.GetFrame();
                    if(frame!=null)
                        frameBitmap = frame.GetBitmap();
                }
                else
                {
                    DepthBasedDoorDetector dDetector = _doorDetector as DepthBasedDoorDetector;
                    if (dDetector != null)
                    {
                        frame = DepthStream.Instance.GetFrame();
                        if(frame!=null)
                            frameBitmap = frame.GetWriteableBitmapForDoorNavi();
                    }
                    else
                    {
                        InfraredBasedDoorDetector iDetector = _doorDetector as InfraredBasedDoorDetector;
                        if (iDetector != null)
                        {
                            frame = InfraredStream.Instance.GetFrame();
                            if(frame!=null)
frameBitmap = frame.GetBitmap();
                        }
                    }
                }

                if (frameBitmap != null)
                {
                    RenderImage(frameBitmap, ImageView1);
                }
                Thread.Sleep(1000 / FRAMERATE);
            }
        }

        //Render each image view
        private void RenderImage(WriteableBitmap frameBitmap, Image imageView)
        {


            if (frameBitmap == null)
            {
                imageView.Dispatcher.Invoke(() =>
                {
                    if (imageView.Source == null)
                    {
                        //otherwise send a blank frame
                        imageView.Source = new WriteableBitmap(800, 600, 96.0, 96.0, PixelFormats.Pbgra32,
                        null);
                    }
                }, DispatcherPriority.Render);
                //otherwise, don't update frame
            }
            else
            {
                imageView.Dispatcher.Invoke(() =>
                {
                    imageView.Source = frameBitmap;
                }, DispatcherPriority.Render);
            }
        }

        // Render the given doors in the given canvas
        private void DrawDoors(List<DetectedDoor> doors, Canvas canvas)
        {
            if (doors == null)
                return;
            canvas.Height = ImageView1.Height;
            canvas.Width = ImageView1.Width;
            
            foreach (DetectedDoor door in doors)
            {
                double heightRatio = canvas.Height / door.OriginalImageSize.Height;
                double widthRatio = canvas.Width / door.OriginalImageSize.Width;
                // Convert from System.Drawing.Rectangle to System.Windows.Shapes.Rectangle
                System.Windows.Shapes.Rectangle outline = new System.Windows.Shapes.Rectangle();
                outline.Width = door.BoundingBox.Width*widthRatio;
                //outline.Height = door.BoundingBox.Height*heightRatio;
                outline.Height = door.BoundingBox.Height*heightRatio * 0.7;
                Canvas.SetTop(outline, door.BoundingBox.Top*heightRatio * 0.7);
                Canvas.SetLeft(outline, door.BoundingBox.Left*widthRatio);

                // Use a10 pt brush for the stroke
                outline.StrokeThickness = 10; 
                // Using different color for different detection method
                if (door.Method == DetectedDoor.DetectMethod.COLOR)
                {
                    outline.Stroke = new SolidColorBrush(Colors.Red);
                }
                else if (door.Method == DetectedDoor.DetectMethod.DEPTH)
                {
                    outline.Stroke = new SolidColorBrush(Colors.Blue);
                }
                else if (door.Method == DetectedDoor.DetectMethod.INFRARED)
                {
                    outline.Stroke = new SolidColorBrush(Colors.Green);
                }
                else
                    outline.Stroke = new SolidColorBrush(Colors.Yellow);

                canvas = OverlayCanvas1;
                ColorDetectionInfo.Content = "angle: " + Math.Round(door.RelativeAngle,2) + "; distance: " + door.RelativeDistance;
                canvas.Children.Add(outline);
            }
        }
    }
}
