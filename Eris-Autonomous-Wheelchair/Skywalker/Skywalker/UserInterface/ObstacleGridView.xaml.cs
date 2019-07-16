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
using System.Drawing;


using Skywalker_Vision.Kinect;
using Skywalker.ObjectDetection;

using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;



namespace Skywalker.UserInterface
{
    /// <summary>
    /// Interaction logic for ObstacleGridView.xaml
    /// TODO
    /// 1. mess with thread rate value
    /// 2. implement the ignoring of the zeroReading and maximumWorkingDistance values
    /// 3. move sensors to side of the chair (and possibly back?)
    /// 4. slowly increase/decrease speeds of the motor when stopping.
    /// </summary>
    public partial class ObstacleGridView : UserControl
    {
        private int zeroReading = 0; // a threshold to ignore any 0 values read from the sensor (0 = outside of working range, no ping echo)
        private int maximumWorkingDistance = 200; // the working range is up to 400cm, 
        private int fowardSonarThreshold = 20; // 20cm threshold for our forward facing sonar
        private int sensorDistanceClose = 20; // 20cm for the side sensor "bumpers"
        private int backupSensorThreshold = 10; // Within 10cm, we back up

        // Pertinant variables for canvas math
        /*
        static double scale = 1.5;
        private double chairX1 = 367 / scale, chairX2 = 433 * scale, chairY1 = 339 / scale, chairY2 = 461 * scale;
        private double chairHeight = 122 * scale, chairWidth = 66 * scale, gradientLength = 250 * scale;*/

        static double scale = 1.5, translateX = 0.0, translateY = 200.0, height = 122, width = 66, maxPDep = 150, gradientLength = 200 * scale;
        static double chairHeight = height * scale, chairWidth = width * scale;
        private double chairX1 = 400 - (chairWidth / 2) + translateX, chairX2 = 400 + (chairWidth / 2) + translateX,
                        chairY1 = 400 - (chairHeight / 2) + translateY, chairY2 = 400 + (chairHeight / 2) + translateY;

        static int snsrsPerSide = 5;
        int totalSens = snsrsPerSide * 3;

        enum DriveType
        {
            User, ObjectDetectedDriven, Error //Override_user, Route, Door, Error
        };

        private Thread SonarUpdateThread;
        private DriveType drive_type;
        //private ObstacleLocalizer points;
        private const float FRAMERATE = 1 / 5f;
        //kinect depth/height frame info

        /// <summary>
        /// Pubic constructor
        /// </summary>
        public ObstacleGridView()
        {
            InitializeComponent();
            InitializeSonars();
        }

        public void InitializeSonars()
        {
            InitializeComponent();

            drive_type = new DriveType();

            SonarUpdateThread = new Thread(UpdateSensors);
            SonarUpdateThread.Start();
            SonarUpdateThread.Name = "Sonar Update Thread";
            //SonarUpdateThread.Priority = ThreadPriority.Lowest;

        }

        public void UpdateSensors()
        {
            int sensorTotal = 7;

            Point[] depthPoints = new Point[sensorTotal];
            Point[] chairSensorPoints = new Point[sensorTotal];

            

            while (true)
            {
                //int minDistance = 10000; // Reset the closest obstacle distance to a large value.
                int frontLeftDistance = Data.Navigator.getDistanceFromSonar(1); // front left sonar sensor distance
                int frontMiddleDistance = Data.Navigator.getDistanceFromSonar(2); // front middle sonar sensor distance
                int frontRightDistance = Data.Navigator.getDistanceFromSonar(4); // front right sonar sensor distance
                Data.Navigator.setCurrentDriveType(1); // initially set the drive type status to user driven
                

                // Sonar box logic
                sonar2.Dispatcher.Invoke(() => //SONAR 3 left front
                {
                    sonar2.Text = "Front Left Distance: " + frontLeftDistance + " cm"; // print to screen
                });
                sonar3.Dispatcher.Invoke(() => //SONAR 3 middle front
                {
                    sonar3.Text = "Front Middle Distance: " + frontMiddleDistance + " cm"; // print to screen
                });
                sonar5.Dispatcher.Invoke(() => // set textbox for SONAR 5 front right wheel
                {
                    sonar5.Text = "Front Right Distance: " + frontRightDistance + " cm"; // print to screen
                });

                nextMoveTextBox.Dispatcher.Invoke(() =>
                {
                    string nextMove = "Straight"; // Start with the assumption that we can go straight.
                    nextMoveTextBox.Text = nextMove; // tell user to go straight at first

                    // check if front sensors detect an obstacle closer than threshold.
                    if (frontLeftDistance < fowardSonarThreshold || frontMiddleDistance < fowardSonarThreshold || frontRightDistance < fowardSonarThreshold)
                    {
                        Data.Navigator.setCurrentDriveType(5); // set the drive type to stop the wheelchair
                        //nextMove = "Left or Right \n"; // change the nextMove
                        //nextMoveTextBox.Text = "Stopped on Object Detected. Object distance < threshold distance. \n Turn " + nextMove; // tell user to go left or right
                    }
                    else
                    {
                        nextMoveTextBox.Text = nextMove; // tell user to go straight
                        Data.Navigator.setCurrentDriveType(1); // set the drive type to user driven
                    }

                    if (frontMiddleDistance < sensorDistanceClose)
                    {
                        nextMove = "Left/Right"; // change the nextMove
                        nextMoveTextBox.Text = "There is an Object detected in front middle. \n Move: " + nextMove; // alert the user
                    }

                    // if Left side distance < side threshold   AND   Right side distance < sidethreshold
                    else if (frontLeftDistance < sensorDistanceClose && frontRightDistance < sensorDistanceClose)
                    {
                        nextMove = "Left/Right"; // change the nextMove ????
                        nextMoveTextBox.Text = "Both Sides are too close to an object."; // alert the user of the next move
                    }

                    // if just the left side sensor is less than threshold
                    else if (frontLeftDistance < sensorDistanceClose)
                    {
                        nextMove = "Right"; // change next move to right
                        nextMoveTextBox.Text = "Next Move: " + nextMove; // alert user
                    }
                    else if (frontRightDistance < sensorDistanceClose) // just the right side sensor is less than threshold
                    {
                        nextMove = "Left"; // change next move to left
                        nextMoveTextBox.Text = "Next Move: " + nextMove; // alert user
                    }

                    // LOGIC FOR TURNING

                    // Check close is the closest object in front of us?  
                    // Note how we subtract 4cm from the front center IR sensor (under the robot) and 5cm from the the tower IR sensor.  
                    // This is because these two sensors are offset by these distances back from the front of the robot.
                    //minDistance = Math.Min(Sensors.SonarFront(), Sensors.IRfrontCenter() - 4,
                    //Sensors.IRfrontTower() - 5, Sensors.IRfrontUpward(), Sensors.IRleft(), Sensors.IRright());

                    //if (minDistance < backupSensorThreshold) nextMove = "Backup"; // If the closest object is too close, then get ready to back up.

                    /*if (nextMove != "Straight") // If next move can't go forward
                    {
                        Data.Navigator.setCurrentDriveType(5); // stop the wheelchair for object detected

                        //if (turnDirection == "Backward") //
                        //{
                        //lastMove = "Backward"; // store the most previous movement
                        //Data.Navigator.SetSpeed = currentDriveSpeed / 2; // sudo code logic for decreasing the speed
                        // simulate backing up 10 cm or time interval
                        //}

                        //make sure there's no oscillation back and forth in a corner 
                        //if (lastMove == "Left" || lastMove == "Right")
                        //{
                        //    nextMove = lastMove;
                        //}
                        //if (nextMove == "Left")
                        //{
                        //    nextTurn = -1;
                        //}
                        //else if (nextMove == "Right")
                        //{
                        //    nextTurn = 1;
                        //}
                        //else if (nextMove == "LeftRight")
                        //{
                        //    nextTurn = turnChoice.Next(-1, 1); //Random left or right.
                        //}

                        // set drive speed
                        // Turn 45 degrees left or right.
                        //DriveMotors.pidDrive.RotationAngle = Math.Sign(nextTurn) * 45;
                        //DriveMotors.pidDrive.Rotate();

                    }
                    else // if no obstacles are detected in front
                    {
                        string lastMove = "Straight";
                        //DriveMotors.pidDrive.Speed = driveSpeed;
                        //DriveMotors.pidDrive.TravelAtSpeed();
                    }*/


                }); // end of next move text box

                // Display Box Logic
                //*********************************************
                //*********************************************
                //*********************************************
                //*********************************************
                //*********************************************
                //*********************************************

                Random rand = new Random();

                sensorCanvas.Dispatcher.Invoke(() =>
                {
                    SensorFrame(depthPoints, chairSensorPoints, 2);
                    //PlotLine(rand.Next(0,800), rand.Next(0, 800), rand.Next(0, 800), rand.Next(0, 800));
                });

                Thread.Sleep((int)(50 / FRAMERATE)); // let thread sleep 1/10 of a second

            } //end of while true loop

        } // end of UpdateSensors function


        public void SensorFrame(Point[] depthPts, Point[] chairPts, int debug)
        {
            // Clear frame
            sensorCanvas.Children.Clear();

            // Draw chair on screen
            Rectangle chair = new Rectangle();
            chair.StrokeThickness = 1;
            chair.Fill = new SolidColorBrush(Colors.Cyan);
            chair.Width = chairWidth;
            chair.Height = chairHeight;
            Canvas.SetTop(chair, chairY1);
            Canvas.SetLeft(chair, chairX1);
            sensorCanvas.Children.Add(chair);

            // These checks should only determine contents of depthPts and chairPts

            if (debug == 0) // Use sensor data
            {

            }
            else if (debug == 1) // Use static dummy data w/ five sensors (three in front, one per side
            {
                chairPts[0] = new Point(chairX1, 400); // Left middle side sensor
                chairPts[1] = new Point(chairX1, chairY1); // Left corner side sensor   ||
                chairPts[2] = new Point(chairX1, chairY1); // Left front sensor         ||
                chairPts[3] = new Point(400, chairY1); // Middle front sensor
                chairPts[4] = new Point(chairX2, chairY1); // Right front sensor        ||
                chairPts[5] = new Point(chairX2, chairY1); // Right corner side sensor  ||
                chairPts[6] = new Point(chairX2, 400); // Right side sensor

                depthPts[0] = new Point(chairX1 - (50 * scale), 400); // Left middle side depth
                depthPts[1] = new Point(chairX1 - (75 * scale), chairY1); // Left corner side depth    ||
                depthPts[2] = new Point(chairX1, chairY1 - (100 * scale)); // Left front depth         ||
                depthPts[3] = new Point(400, chairY1 - (125 * scale)); // Middle front depth
                depthPts[4] = new Point(chairX2, chairY1 - (150 * scale)); // Right front depth        ||
                depthPts[5] = new Point(chairX2 + (175 * scale), chairY1); // Right corner side depth  ||
                depthPts[6] = new Point(chairX2 + (200 * scale), 400); // Right side depth
            }
            else // Use generated data for representing possible sensor values
            {
                GenerateDepths(out depthPts, out chairPts);
            }
            


            // Loop that draws depth lines
            for (int i = 0; i < chairPts.Length; i++)
            {
                PlotLine(chairPts[i], depthPts[i]);
            }

            // Loop that connects depthPts
            for (int j = 0; j < depthPts.Length-1; j++)
            {
                PlotLine(depthPts[j],  depthPts[j+1]);
            }

            // Loop that draws heat squares
            // 250 = green = safe
            // 150 = yellow = warning
            // 60 = red = danger
            for (int k = 0; k < depthPts.Length-1; k++)
            {
                Polygon heat = new Polygon();


                LinearGradientBrush heatMap = new LinearGradientBrush(); // Colors.Red, Colors.Green, new Point(chairX1,chairY2), new Point(chairX1-gradientLength, chairY2));

                GradientStop r = new GradientStop();
                r.Color = Colors.Red;

                GradientStop y = new GradientStop();
                y.Color = Colors.Yellow;

                GradientStop g = new GradientStop();
                g.Color = Colors.Green;
                

                if (k < snsrsPerSide) // Left side
                {
                    heatMap.StartPoint = new Point(0, 0);
                    heatMap.EndPoint = new Point(1, 0);
                    r.Offset = 0.8;
                    y.Offset = 0.6;
                    g.Offset = 0.0;
                }
                else if (k > snsrsPerSide*2 - 1) // Right side
                {
                    heatMap.StartPoint = new Point(0, 0);
                    heatMap.EndPoint = new Point(1, 0);
                    r.Offset = 0.0;
                    y.Offset = 0.4;
                    g.Offset = 0.8;
                }
                else // Top side
                {
                    heatMap.StartPoint = new Point(0, 1);
                    heatMap.EndPoint = new Point(0, 0);
                    r.Offset = 0.3;
                    y.Offset = 0.6;
                    g.Offset = 0.7;
                }

                heat.Fill = heatMap;

                Point midpoint = MidPoint(depthPts[k], depthPts[k + 1]);

                if (midpoint.X < chairX1 && midpoint.Y < chairY1)
                {
                    heatMap.StartPoint = new Point(0, 0);
                    heatMap.EndPoint = new Point(1, 1);
                }
                else if (midpoint.X > chairX2 && midpoint.Y < chairY1)
                {
                    heatMap.StartPoint = new Point(1, 1);
                    heatMap.EndPoint = new Point(0, 0);
                }



                heatMap.GradientStops.Add(r);
                heatMap.GradientStops.Add(y);
                heatMap.GradientStops.Add(g);

                // Make polygon filled with gradient
                //heat.Fill = heatMap;
                // Need this for some reason
                heat.StrokeThickness = 1;

                // Add all four points of a polygon
                PointCollection pC = new PointCollection();
                pC.Add(depthPts[k]);
                pC.Add(chairPts[k]);
                pC.Add(chairPts[k+1]);
                pC.Add(depthPts[k+1]);
                heat.Points = pC;

                sensorCanvas.Children.Add(heat);
            }

            

        }

        // Pass in first point and end point of line
        public void PlotLine(Point start, Point end)
        {
            Line ln = new Line();
            ln.Stroke = System.Windows.Media.Brushes.Cyan;
            ln.X1 = start.X;
            ln.Y1 = start.Y;
            ln.X2 = end.X;
            ln.Y2 = end.Y;
            ln.StrokeThickness = 1;

            sensorCanvas.Children.Add(ln);

        }

        public void GenerateDepths(out Point[] depthPts, out Point[] chairPts)
        {

            Random rand = new Random();
            int depDif;

            chairPts = new Point[totalSens];
            depthPts = new Point[totalSens];

            // Calculate chair sensor points
            for (int k = 0; k < snsrsPerSide; k++)
            {
                depDif = rand.Next((int)(-30 * scale), (int)(30 * scale));

                // Left
                chairPts[k] = new Point(chairX1, chairY2 - (k*(chairHeight / (snsrsPerSide - 1))));
                depthPts[k] = new Point(chairX1 - ((maxPDep * scale) * (k+1) / (snsrsPerSide - 1)) + depDif , chairY2 - ((chairHeight * k) / (snsrsPerSide - 1)));


                // Top
                chairPts[k+5] = new Point(chairX1 + ((chairWidth * k) / (snsrsPerSide - 1)), chairY1);
                depthPts[k+5] = new Point(chairX1 + ((chairWidth * k) / (snsrsPerSide - 1)), chairY1 - (100 + (maxPDep * scale) * (k+1) / snsrsPerSide) + depDif);

                // Right
                chairPts[k+10] = new Point(chairX2, chairY1 + ((chairHeight * k) / (snsrsPerSide - 1)));
                depthPts[k+10] = new Point(chairX2 + ((maxPDep * scale) * (k+1) / snsrsPerSide) + depDif, chairY1 + ((chairHeight * k) / (snsrsPerSide - 1)));

            }


        }

        public Point MidPoint (Point str, Point end)
        {
            Point mid = new Point();

            mid.X = (str.X + end.X) / 2;
            mid.Y = (str.Y + end.Y) / 2;

            return mid;
        }
      

        /*public void UpdateCurrentCompass()
        {
            while (true)
            {
                compassTextBox.Dispatcher.Invoke(() =>
                {
                    //float degrees = Data.Navigator.GetDirection();
                    //compassTextBox.Text = "Direction:  " + degrees.ToString();

                });

                Thread.Sleep((int)(100 / FRAMERATE));
            }

        }*/

    }
}
