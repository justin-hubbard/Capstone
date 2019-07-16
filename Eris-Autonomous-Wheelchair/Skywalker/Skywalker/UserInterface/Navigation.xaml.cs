using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Timers;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Skywalker.Mapping;
using Skywalker.Input;
using Skywalker.Localization;
using Skywalker.Driver;

namespace Skywalker.UserInterface {
    /// <summary>
    /// Interaction logic for Navigation.xaml
    /// </summary>
    public partial class Navigation : UserControl 
    {
        enum DriveType
        {
            User, Override_user, Route, Door, Error
        };


        private Thread MapUpdateThread;
        private Pose CurrentPose;
        
        private const float FRAMERATE = 1/5f;
        
        // New variables from December 2015 
        private BitmapFrame bitmapFrame;
        private Canvas highlightedRooms;
        private Rectangle currentSelectedRoom;
        private Rectangle currentLocation;
        private Canvas recentLocations;
        private Canvas routeLocations;
        private Canvas pathLocations;
        private Canvas obstacleCanvas;
        
        private DriveType drive_type;


        public Navigation() 
        {
            InitializeComponent();
            Initialize();
        }

       
        public void Initialize()
        {
            currentSelectedRoom = new Rectangle();
            currentLocation = new Rectangle();

            InitializeComponent();

            drive_type = new DriveType();

            MapUpdateThread = new Thread(DrawMap);
            MapUpdateThread.Start();
            MapUpdateThread.Name = "Map Update Thread";
            MapUpdateThread.Priority = ThreadPriority.Lowest;

            UserDoorDetectionButton.Click += DoorNavigation_Click;

            //WheelChairInfo_UpdateCurrentRoomThread = new Thread(UpdateCurrentRoom);
            //WheelChairInfo_UpdateCurrentRoomThread.Name = "UpdateCurrentRoom";
            //WheelChairInfo_UpdateCurrentRoomThread.Priority = ThreadPriority.Lowest;
            //WheelChairInfo_UpdateCurrentRoomThread.Start();

            //WheelChairInfo_UpdateCurrentLocationThread = new Thread(UpdateCurrentLocation);
            //WheelChairInfo_UpdateCurrentLocationThread.Name = "UpdateCurrentLocation";
            //WheelChairInfo_UpdateCurrentLocationThread.Priority = ThreadPriority.Lowest;
            //WheelChairInfo_UpdateCurrentLocationThread.Start();

            //WheelChairInfo_UpdateCurrentCompassThread = new Thread(UpdateCurrentCompass);
            //WheelChairInfo_UpdateCurrentCompassThread.Name = "UpdateCurrentCompass";
            //WheelChairInfo_UpdateCurrentCompassThread.Priority = ThreadPriority.Lowest;
            //WheelChairInfo_UpdateCurrentCompassThread.Start();
        }

        

        public void DrawMap()
        {
            const DispatcherPriority priority = DispatcherPriority.Render;
           
            while (true)
            {                
                if (Data.Cartographer != null)
                {                 
                    MapImage.Dispatcher.Invoke(() =>
                    {                        
                        Tuple<double, double> tupleUV = new Tuple<double, double>(0,0);
                        mPoint myUV = new mPoint(tupleUV);
                        myUV = this.CurrentPose.CurrentPositionInUV;

                        if (myUV.X < MapImage.Width - 10 && myUV.Y < MapImage.Height - 10)
                        {


                            SolidColorBrush solidColorBrush1 = new SolidColorBrush();

                            SetMapDotColor(Data.Navigator.GetCurrentDriveType(), solidColorBrush1);

                            Ellipse dot = new Ellipse();

                            dot.Width = 10;
                            dot.Height = 10;
                            dot.Fill = solidColorBrush1;

                            Canvas.SetLeft(dot, myUV.X);
                            Canvas.SetTop(dot, myUV.Y);

                            recentLocations.Children.Add(dot);

                            SolidColorBrush solidColorBrush2 = new SolidColorBrush();

                            
                            
                        }
                        if (recentLocations.Children.Count > 5)
                            recentLocations.Children.RemoveAt(0);
                        if (obstacleCanvas.Children.Count > 3)
                            obstacleCanvas.Children.RemoveAt(0);
                        
                    }, priority);

                    roomLabel.Dispatcher.Invoke(() =>
                    {
                        roomLabel.Content = "Room Location:   " + this.CurrentPose.CurrentRoom.Name;
                    });

                    drivetypeLabel.Dispatcher.Invoke(() =>
                    {
                        if (drive_type == DriveType.User)
                            drivetypeLabel.Content = "Drive Type:  User Driven";
                        else if (drive_type == DriveType.Override_user)
                            drivetypeLabel.Content = "Drive Type:  User Override Driven";
                        else if (drive_type == DriveType.Route)
                            drivetypeLabel.Content = "Drive Type:  Route Driven";
                        else if (drive_type == DriveType.Door)
                            drivetypeLabel.Content = "Drive Type:  Door Detected Driven";
                        else
                            drivetypeLabel.Content = "Drive Type:  Error Detected";
                    });

                    ucoord.Dispatcher.Invoke(() =>
                    {
                        mPoint myUV = CurrentPose.CurrentPositionInUV;
                        mPoint myXY = CurrentPose.CurrentPositionInXY;
                        ucoord.Content = " Current Location U/X:  " + myUV.X + " / " + myXY.X;
                    });

                    vcoord.Dispatcher.Invoke(() =>
                    {
                        mPoint myUV = myUV = CurrentPose.CurrentPositionInUV;
                        mPoint myXY = CurrentPose.CurrentPositionInXY;
                        vcoord.Content = " Current Location V/Y:  " + myUV.Y + " / " + myXY.Y;
                    });
                    compassLabel.Dispatcher.Invoke(() =>
                    {
                        double degrees = CurrentPose.CurrentDirection;
                        compassLabel.Content = "Direction:  " + degrees.ToString();

                    });
                    SonarLabel.Dispatcher.Invoke(() =>
                        {
                            int distance1 = Data.Navigator.getDistanceFromSonar(0);
                            int distance2 = Data.Navigator.getDistanceFromSonar(1);
                            int distance3 = Data.Navigator.getDistanceFromSonar(2);
                            int distance4 = Data.Navigator.getDistanceFromSonar(3);
                            int distance5 = Data.Navigator.getDistanceFromSonar(4);
                            SonarLabel.Content = "S1:" + distance1 + ";S2:"+distance2 + ";S3:" + distance3 +";S4:" +distance4 + ";S5:" +distance5;
                        });

                }
              
                Thread.Sleep((int)(100/FRAMERATE));                
            }
        }

        public void SetPose(Pose pose)
        {
            this.CurrentPose = pose;
        }

        /*public void UpdateCurrentRoom()
        {
            while (true)
            {
                roomLabel.Dispatcher.Invoke(() =>
                {
                    roomLabel.Content = "Room Location:   " + Data.Cartographer.MyCurrentRoom().ToString();
                });
                Thread.Sleep((int)(100 / FRAMERATE));
            }
        }

        public void UpdateCurrentLocation()
        {
            while(true)
            {
                ucoord.Dispatcher.Invoke(() =>
                {
                    Tuple<double, double> tupleUV = new Tuple<double, double>(0, 0);
                    mPoint myUV = new mPoint(tupleUV);
                    myUV = Data.Cartographer.GetLocationInUV();
                    ucoord.Content = " Current Location U:  "  + myUV.X;
                });
                
                vcoord.Dispatcher.Invoke(() =>
                {
                    Tuple<double, double> tupleUV = new Tuple<double, double>(0, 0);
                    mPoint myUV = new mPoint(tupleUV);
                    myUV = Data.Cartographer.GetLocationInUV();
                    vcoord.Content = " Current Location V:  " + myUV.Y;
                });
                Thread.Sleep((int)(100 / FRAMERATE));
            }
        }

        public void UpdateCurrentCompass()
        {
            while (true)
            {
                compassLabel.Dispatcher.Invoke(() =>
                {
                    float degrees = Data.Navigator.getDegreesFromCompass();
                    compassLabel.Content = "Direction:  " + degrees.ToString();

                });

                Thread.Sleep((int)(100 / FRAMERATE));
            }

        }
        */
        
        private void SetMapDotColor(int driveType, SolidColorBrush solidColorBrush)
        {
            //user
            if (driveType == 0)
            {
                solidColorBrush.Color = System.Windows.Media.Colors.DarkBlue;
                drive_type = DriveType.User;
            }
            //override_user driven
            else if (driveType == 1)
            {
                solidColorBrush.Color = System.Windows.Media.Colors.LightBlue;
                drive_type = DriveType.Override_user;
            }
            //route
            else if (driveType == 2)
            {
                solidColorBrush.Color = System.Windows.Media.Colors.Red;
                drive_type = DriveType.Route;
            }
            //door
            else if (driveType == 3)
            {
                solidColorBrush.Color = System.Windows.Media.Colors.Green;
                drive_type = DriveType.Door;
            }
            //error
            else
            {
                solidColorBrush.Color = System.Windows.Media.Colors.Magenta;
                drive_type = DriveType.Error;
            }
        }

        private void MapImage_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {

        }

        private void MapImage_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            highlightedRooms.Children.Clear();
        }

        private void MapImage_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point click = e.GetPosition(MapImage);
            mPoint clickPoint = new mPoint(click.X, click.Y);
            Tuple<mPoint, Tuple<double, double>> clickRegion = Data.Cartographer.GetRoomCornerAndDimentions(clickPoint);

            pathLocations.Children.Clear();
            routeLocations.Children.Clear();
            Thread.Sleep(500);

            highlightedRooms.Children.Clear();

            if (clickRegion != null)
            {
                currentSelectedRoom.Width = clickRegion.Item2.Item1;
                currentSelectedRoom.Height = clickRegion.Item2.Item2;

                SolidColorBrush solidColorBrush = new SolidColorBrush();

                var r = (byte)0;
                var g = (byte)255;
                var b = (byte)0;
                var a = (byte)50;

                // solidColorBrush.Color = System.Windows.Media.Color.FromArgb(a, r, g, b);
                solidColorBrush.Color = System.Windows.Media.Colors.LightBlue;
                currentSelectedRoom.Fill = solidColorBrush;

                Canvas.SetLeft(currentSelectedRoom, clickRegion.Item1.X);
                Canvas.SetTop(currentSelectedRoom, clickRegion.Item1.Y);

                highlightedRooms.Children.Add(currentSelectedRoom);
            }
            Data.Navigator.NavigateTo(clickPoint);
            if (Data.Navigator.GetCurrentDriveType() == 2)
            {


                //SetMapDotColor(Data.Navigator.GetCurrentDriveType(), solidColorBrush);

                Thread.Sleep(500);
                List<mPoint> pathList = Data.Navigator.GetPath(); 
                List<mPoint> routeList = Data.Navigator.GetRoute();
                

                foreach (mPoint rl in routeList)
                {
                    Ellipse dot = new Ellipse();
                  
                    dot.Width = 10;
                    dot.Height = 10;
                    dot.Fill = Brushes.Magenta;
                    Canvas.SetLeft(dot, rl.X);
                    Canvas.SetTop(dot, rl.Y);
                    routeLocations.Children.Add(dot);
                }

                foreach (mPoint pl in pathList)
                {
                    Ellipse dot = new Ellipse();
                    
                    dot.Width = 10;
                    dot.Height = 10;
                    dot.Fill = Brushes.MediumPurple;

                    Canvas.SetLeft(dot, pl.X);
                    Canvas.SetTop(dot, pl.Y);
                    pathLocations.Children.Add(dot);
                }

            }
            
           
        }

        private void MapImage_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point click = e.GetPosition(MapImage);
            mPoint clickPoint = new mPoint(click.X, click.Y);
            Tuple<mPoint, Tuple<double, double>> clickRegion = Data.Cartographer.GetRoomCornerAndDimentions(clickPoint);

            highlightedRooms.Children.Clear();

            if (clickRegion != null)
            {
                currentSelectedRoom.Width = clickRegion.Item2.Item1;
                currentSelectedRoom.Height = clickRegion.Item2.Item2;
                
                SolidColorBrush solidColorBrush = new SolidColorBrush();
                
                var r = (byte)0;
                var g = (byte)255;
                var b = (byte)0;
                var a = (byte)50;

                solidColorBrush.Color = System.Windows.Media.Color.FromArgb(a, r, g, b);

                currentSelectedRoom.Fill = solidColorBrush;

                Canvas.SetLeft(currentSelectedRoom, clickRegion.Item1.X);
                Canvas.SetTop(currentSelectedRoom, clickRegion.Item1.Y);

                highlightedRooms.Children.Add(currentSelectedRoom);
            }
        }

        private void LoadMapImageToScreen(string filename)
        {
            ImageCanvas imageCanvas = new ImageCanvas();

            imageCanvas.CanvasImageSource = new BitmapImage(new Uri(filename));

            Image image = new Image
            {
                Source = imageCanvas.CanvasImageSource
            };

            int amount = 0;
            bitmapFrame = CreateResizedImage(image.Source, (int)image.Source.Width - amount, (int)image.Source.Height - amount, 5);

            while (bitmapFrame.Width < 550 && bitmapFrame.Height < 550)
            {
                bitmapFrame = CreateResizedImage(image.Source, (int)image.Source.Width + amount, (int)image.Source.Height + amount, 5);
                amount += 100;
            }

            image.Source = bitmapFrame;

            MapImage.Width = bitmapFrame.Width;
            MapImage.Height = bitmapFrame.Height;
            MapImage.VerticalAlignment = VerticalAlignment.Center;
            MapImage.HorizontalAlignment = HorizontalAlignment.Center;
            MapImage.Children.Add(imageCanvas);
            Console.WriteLine(String.Format("MapImage Width: {0} MapImage Height: {1}", MapImage.Width, MapImage.Height));
            imageCanvas.Children.Add(image);

            highlightedRooms = new Canvas();
            highlightedRooms.Width = MapImage.Width;
            highlightedRooms.Height = MapImage.Height;
            highlightedRooms.VerticalAlignment = VerticalAlignment.Center;
            highlightedRooms.HorizontalAlignment = HorizontalAlignment.Center;
            imageCanvas.Children.Add(highlightedRooms);
            Console.WriteLine(String.Format("highlightedRooms Width: {0} highlightedRooms Height: {1}", highlightedRooms.Width, highlightedRooms.Height));

            recentLocations = new Canvas();
            recentLocations.Width = MapImage.Width;
            recentLocations.Height = MapImage.Height;
            recentLocations.VerticalAlignment = VerticalAlignment.Center;
            recentLocations.HorizontalAlignment = HorizontalAlignment.Center;

            imageCanvas.Children.Add(recentLocations);
            Console.WriteLine(String.Format("recentLocations Width: {0} recentLocations Height: {1}", recentLocations.Width, recentLocations.Height));

            routeLocations = new Canvas();
            routeLocations.Width = MapImage.Width;
            routeLocations.Height = MapImage.Height;
            routeLocations.VerticalAlignment = VerticalAlignment.Center;
            routeLocations.HorizontalAlignment = HorizontalAlignment.Center;
            imageCanvas.Children.Add(routeLocations);

            pathLocations = new Canvas();
            pathLocations.Width = MapImage.Width;
            pathLocations.Height = MapImage.Height;
            pathLocations.VerticalAlignment = VerticalAlignment.Center;
            pathLocations.HorizontalAlignment = HorizontalAlignment.Center;
            imageCanvas.Children.Add(pathLocations);

            obstacleCanvas = new Canvas();
            obstacleCanvas.Width = MapImage.Width;
            obstacleCanvas.Height = MapImage.Height;
            obstacleCanvas.VerticalAlignment = VerticalAlignment.Center;
            obstacleCanvas.HorizontalAlignment = HorizontalAlignment.Center;
            imageCanvas.Children.Add(obstacleCanvas);

        }

       

        private static BitmapFrame CreateResizedImage(ImageSource source, int width, int height, int margin)
        {
            var rect = new Rect(margin, margin, width - margin * 2, height - margin * 2);

            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.HighQuality);
            group.Children.Add(new ImageDrawing(source, rect));

            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
                drawingContext.DrawDrawing(group);

            var resizedImage = new RenderTargetBitmap(
                width, height,         // Resized dimensions
                96, 96,                // Default DPI values
                PixelFormats.Default); // Default pixel format
            resizedImage.Render(drawingVisual);

            return BitmapFrame.Create(resizedImage);
        }

        private void MapImage_Loaded(object sender, RoutedEventArgs e)
        {
            string imageName = "Sloan46_2_Obstacle.png";
            string location = Directory.GetCurrentDirectory() + @"\TestData\TestImages\" + imageName;
            LoadMapImageToScreen(location);        
        }

        private void LetsGo_Click(object sender, RoutedEventArgs e)
        {
            string us = U.Text;
            string vs = V.Text;
            double u; double v;
            Double.TryParse(us, out u);
            Double.TryParse(vs, out v);
            mPoint clickPoint = new mPoint(u, v);
            //Data.Navigator.NavigateTo(clickPoint);    
            Data.Navigator.SetRoute(clickPoint);
        }

        private void DoorNavigation_Click(object sender,RoutedEventArgs e)
        {
            Data.Navigator.ToggleDoorNavigationMode();
        }
    }
}
