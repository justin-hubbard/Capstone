using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Skywalker.ObjectDetection;
using Skywalker.Mapping;
using Skywalker.Odometry;
using Skywalker.Localization;
using Skywalker.Resources;
using Skywalker.Sensors.IPS;
using Skywalker.Utils;
using Skywalker.Utils.Logger.IPS;
using Skywalker.Sensors.SensorArray;

using System.Collections.Concurrent;
using System.IO;

namespace Skywalker.Driver
{
    /// <summary>
    /// The navigator for the driver.
    /// Decides where the driver should go.
    /// </summary>
    public class Navigator : INavigator
    {
        //****************** Threads ******************
        private Thread FollowRouteThread;

        //****************** Vairables ******************

        //classes in project ******************
        private IObstacleMap ObstacleMap;
        private IObstacleDetector ObstacleDetector;
        private ObstacleLocalizer ObstacleLocalizer;
        private IDoorDetector DoorDetector;
        private SonarDetector SonarDetector;
        private ICartographer Cartographer;
        private INavigationStrategy DoorOrientationNavigator;
        private Pose CurrentPose;

        //sensors/input devices ******************
        private MarvelMind mmIPS;
        private SensorArray ChairSensorArray;

        //state ******************

        //speed
        private double Speed = .75;

        //this will hold who has control of where we are going. 
        public enum DriveTypeStatus {UserDriven, UnRestrictedUserDriven, RoutDriven, DoorDriven, ProgramDriven, ObjectDetectedDriven};
        private DriveTypeStatus CurrentDriveType = DriveTypeStatus.UnRestrictedUserDriven;

        //route ******************
        private mPoint LastPoint = null;
        private List<mPoint> Route = new List<mPoint>();
        private List<mPoint> Path = new List<mPoint>();

        //tolerences ******************
        private double DistanceToBeAtPoint = 55;
        private double DistanceToBeAtDoor = 100;

        //****************** Consturtors ******************
        public Navigator(IObstacleMap obstacleMap, IObstacleDetector obstacleDetector, ObstacleLocalizer obstacleLocalizer,
                         ICartographer cartographer, IDoorDetector doorDetector, Pose pose, SonarDetector sonarDetector,
                         SensorArray sensorArray)
        {
            this.ObstacleDetector = obstacleDetector;
            this.ObstacleMap = obstacleMap;
            this.ObstacleLocalizer = obstacleLocalizer;
            this.DoorDetector = doorDetector;
            this.CurrentPose = pose;
            this.Cartographer = cartographer;
            this.SonarDetector = sonarDetector;
            this.ChairSensorArray = sensorArray;

            pose.CurrentRoom = Cartographer.GetStartingRoom();
            LastPoint = pose.CurrentPositionInUV;

            FollowRouteThread = new Thread(UpdateRoute);
            FollowRouteThread.Name = "FollowRouteThread";
            FollowRouteThread.Start();

            this.ObstacleDetector.ObstaclesDetected += ObstacleDetector_ObstaclesDetected; // ?
        }

        //****************** Methods for Threads ******************

        /// <summary>
        /// Updates the current route, poppin the next destination off the
        /// stack if we've arrived at it. If we dont have a route to follow 
        /// it will just update the current position. 
        /// </summary>
        /// ToDo: might need to update to allow the chair to recover from 
        /// door navigation correctly
        private void UpdateRoute()
        {
            while (true)
            {
                //update the position and grab it for use.
                mPoint mostCurrentPosition = CurrentPose.CurrentPositionInUV;

                lock (Route)
                {

                    //if we have a path to follow. 
                    if (Path.Count > 0)
                    {
                        double pathDestinationDif = Path[0].GetDistanceToPoint(mostCurrentPosition);

                        //if the point are checking is a door then use the DistanceToBeAtDoor else use DistanceToBeAtPoint
                        if (Path[0].IsDoor() && pathDestinationDif < DistanceToBeAtDoor)
                        {
                            Path.RemoveAt(0);
                        }
                        else if (pathDestinationDif < DistanceToBeAtPoint)
                        {
                            Path.RemoveAt(0);
                        }
                    }

                    //Check if I'm at my next destination and if so, pop it off the stack.
                    //if we have a rout we need to follow
                    if (Route.Count > 0)
                    {
                        //get the distance from the current location to the next location
                        double distanceToDestination = Route[0].GetDistanceToPoint(mostCurrentPosition);

                        //if our destination is a door and we are there
                        if (Route[0].IsDoor() && distanceToDestination < DistanceToBeAtDoor)
                        {
                            //swap to door driven
                            InitializeDoorNavigation();
                            CurrentDriveType = DriveTypeStatus.DoorDriven;

                            Route.RemoveAt(0);
                        }
                        // if we are at the destination
                        else if (distanceToDestination < DistanceToBeAtPoint)
                        {
                            Route.RemoveAt(0);
                        }
                    }

                    //if we have a route but not a path setup the new path and we are still in routeDriven mode. 
                    //Note: the RoutDriven check makes sure we are not trying to get through a door and setting up a path for it
                    if ((Route.Count > 0 && Path.Count == 0) && CurrentDriveType == DriveTypeStatus.RoutDriven)
                    {
                        List<mPoint> newPath = Cartographer.PlanPath(mostCurrentPosition, Route[0], this.ObstacleMap);

                        //if we can get to the new locating set up the path to do so else set us back to UserDriven
                        if (newPath != null)
                        {
                            Path = newPath;
                        }
                        else
                        {
                            SetToUserDriven();
                        }
                    }
                }

                Thread.Sleep(500);
            }
        }


        //sets the Route and path to empty and sets the CurrentDriveType to userDriven.
        private void SetToUserDriven()
        {
            lock (Route)
            {
                Route = new List<mPoint>();
                Path = new List<mPoint>();
                CurrentDriveType = DriveTypeStatus.UnRestrictedUserDriven;
            }
        }

        //****************** Methods for Interface ******************

        // called by UI ******************

        public void ToggleUsermode()
        {
            //locks on route to prevent a switch in CurrentDriveType while we are updating route
            lock (Route)
            {
                if (CurrentDriveType == DriveTypeStatus.UserDriven)
                {
                    CurrentDriveType = DriveTypeStatus.UnRestrictedUserDriven;
                }
                else if (CurrentDriveType == DriveTypeStatus.UnRestrictedUserDriven)
                {
                    CurrentDriveType = DriveTypeStatus.UserDriven;
                }
            }
        }

        public void ToggleDoorNavigationMode()
        {
            if (CurrentDriveType != DriveTypeStatus.DoorDriven)
            {
                InitializeDoorNavigation();
                CurrentDriveType = DriveTypeStatus.DoorDriven;
            }
            else
            {
                CurrentDriveType = DriveTypeStatus.UnRestrictedUserDriven;
                StopDoorNavigation();
            }
        }

        // Returns a copy of the current route in the form of a list of mPoints.
        // if the route is empty it will return null
        public List<mPoint> GetRoute()
        {
            lock (Route)
            {
                if (Route.Count > 0)
                {
                    return new List<mPoint>(Route);
                }
                else
                {
                    return new List<mPoint>();
                }
            }
        }

        //returns a copy of the current path
        public List<mPoint> GetPath()
        {
            lock (Route)
            {
                if (Path.Count > 0)
                {
                    return new List<mPoint>(Path);
                }
                else
                {
                    return new List<mPoint>();
                }
            }
        }

        public void SetRoute(mPoint nextPointUV)
        {
            lock (Route)
            {
                Route = new List<mPoint>();
                Route.Add(CurrentPose.CurrentPositionInUV);
                Route.Add(nextPointUV);
                CurrentDriveType = DriveTypeStatus.RoutDriven;
            }
        }


        //Returns the current drive type for navigator. Used to color the output to the user
        // 0 for UserDriven
        // 1 for UnRestrictedUserDriven
        // 2 for RoutDriven
        // 3 for DoorDriven.
        // 4 for motor calibration
        // 5 for ObjectDetectedDriven
        // -1 for error.
        public int GetCurrentDriveType()
        {
			switch (CurrentDriveType) {
				case DriveTypeStatus.UserDriven:
					return 0;
				case DriveTypeStatus.UnRestrictedUserDriven:
					return 1;
				case DriveTypeStatus.RoutDriven:
					return 2;
				case DriveTypeStatus.DoorDriven:
					return 3;
				case DriveTypeStatus.ProgramDriven:
					return 4;
				case DriveTypeStatus.ObjectDetectedDriven: // Added Drive type status for object detected
					return 5;
			}
			return -1;
        }
        public void setCurrentDriveType(int newStatus)
        {
            switch (newStatus)
            {
                case 0:
                    CurrentDriveType = DriveTypeStatus.UserDriven;
                    break;
                case 1:
                    CurrentDriveType = DriveTypeStatus.UnRestrictedUserDriven;
                    break;
                case 2:
                    CurrentDriveType = DriveTypeStatus.RoutDriven;
                    break;
                case 3:
                    CurrentDriveType = DriveTypeStatus.DoorDriven;
                    break;
		        case 4:
			        CurrentDriveType = DriveTypeStatus.ProgramDriven;
			        break;
                case 5:
                    CurrentDriveType = DriveTypeStatus.ObjectDetectedDriven; // ADDED for when an object is detected
                    break;

            }
        }

        //returns the current room
        public string GetCurrentRoom()
        {
            return CurrentPose.CurrentRoom.Name;
        }

        //set the desired speed
        public void SetSpeed(double newSpeed)
        {
            Speed = newSpeed;
        }

        //get the desired speed 
        public double GetSpeed()
        {
            return Speed;
        }

        /// <summary>
        /// Signals the navigator to find a route to the given name destination.
        /// sets the driveType to driven by route unless the rout was null which 
        /// means there was an error in cartographer and you cant get to that point
        /// </summary>
        /// <param name="destination">Name of location to route to.</param>
        public void NavigateTo(mPoint destinationPointInUV)
        {
            lock (Route)
            {
                mPoint currentPosition = CurrentPose.CurrentPositionInUV;

                List<mPoint> newRoute = Cartographer.PlanRoute(currentPosition, destinationPointInUV);

                //if we have a valid route set the route and switch to route driven else set to user driven
                if (newRoute != null)
                {
                    Route = newRoute;
                    Path = new List<mPoint>();
                    CurrentDriveType = DriveTypeStatus.RoutDriven;
                }
                else
                {
                    SetToUserDriven();
                }

            }
        }
		public int[] MotorCalibrationRun() {
			//FileStream fs = new FileStream("C:\Code\Senior Design\Eris-Autonomous-Wheelchair\Logs", FileMode.);
			CurrentDriveType = DriveTypeStatus.ProgramDriven;
			Thread.Sleep(1500);
			double originalDegrees = CurrentPose.CurrentDirection;
//			U.Text = originalDegrees.ToString();
			Thread.Sleep(5000); //time is how long wheelchair will move that way

			double endDegrees = CurrentPose.CurrentDirection;
			SetToUserDriven();
			calibrationResult = (int) (endDegrees-originalDegrees);

			int[] toRet = { (int)originalDegrees, (int)endDegrees, calibrationResult };
			return toRet;
			//return endDegrees; //mostly debugging
		}

		private int calibrationResult;
		public int GetCalibrationResult() {
			return calibrationResult;
		}

        // called by Driver ******************

        //used by driver. This will set the starting vector which will be a vector 
        // calculated in UV. It will then take the current compass direction and 
        // get the difference between it and the start vector giving us the change 
        // needed in degrees to calculate the actual direction using the compass.
        //public void setStartVector(Vector startVector)
        //{
        //    //since we have not set the off set for the current compass direction in UV
        //    //this will return the unchanged vector from the compass.
        //    Vector compassDirection = CurrentDirectionInUV;

        //    //calculates the difference between the startVector and the compassDirection
        //    CompassToUVDifInRadians = DifInRaidans(startVector, compassDirection);

        //    Calibrated = true;
        //}

        /// <summary>
        /// Gets the angle of our direction vector.
        /// Imagine a point five feet in front of the wheelchair
        /// at all times - this represents the destination.
        /// This destination will be used to track our immediate 
        /// direction of movement.
        /// It is stored as a float, representing an angle of 
        /// clockwise rotation from straight ahead of the vehicle.
        /// 
        /// TODO : This function should also take some information
        /// about the obstacles, such as an occupancy grid. Right now,
        /// it just follows whatever the user inputs.
        /// </summary>
        //it will keepgoing ignoring user input if there is a route
        //this should be changed later to makesure we give the user 
        //controle when they want it. For not hit the kill switch.
        public Vector GetDirection(Vector userInputVector)
        {

            //The vector to attempt to move in.
            Vector desiredDirection;

            ////for door navigation testing
            //CurrentDriveType = DriveTypeStatus.DoorDriven;

            //check the searchType
            if (CurrentDriveType == DriveTypeStatus.RoutDriven)
            {
                //If we have a route to follow, attempt to follow it.
                desiredDirection = NavigateFromMap();
            }
            else if (CurrentDriveType == DriveTypeStatus.DoorDriven)
            {
                desiredDirection = NavigateFromDoorDetection();
            }
            else if (CurrentDriveType == DriveTypeStatus.UnRestrictedUserDriven)
            {
                desiredDirection = userInputVector;
			} else if (CurrentDriveType == DriveTypeStatus.ProgramDriven) {

				Vector inputVector = new Vector(0, 0);
				inputVector.Y = 1;
				inputVector.Normalize();
				desiredDirection = inputVector;
				Speed = 1;
				//	desiredDirection = //STRAIGHT? LOL wat
			}
            else if (CurrentDriveType == DriveTypeStatus.ObjectDetectedDriven) // ADDED
            {
                // DUMMY CODE
                desiredDirection = new Vector(0, 0); // sets the desired direction...
            }
            //Else follow user input.
            else
            {
                desiredDirection = NavigateUserMode(userInputVector);
            }

            if (CurrentDriveType == DriveTypeStatus.DoorDriven)
            {
                //desiredDirection = new Vector(-(desiredDirection.X), desiredDirection.Y);
                Speed = 0.4;
                if (desiredDirection.X < 0 && desiredDirection.Y != 0)
                {
                    desiredDirection = new Vector(desiredDirection.X * 5, desiredDirection.Y);
                }
                else if (desiredDirection.X < 0 && desiredDirection.Y == 0)
                {
                    desiredDirection = new Vector(desiredDirection.X, desiredDirection.Y);
                    Speed = 0.5;
                }
                else if (desiredDirection.X > 0 && desiredDirection.Y == 0)
                {
                    desiredDirection = new Vector(desiredDirection.X * 0.5, desiredDirection.Y);
                }
                else if (desiredDirection.X > 0 && desiredDirection.Y != 0)
                {
                    desiredDirection = new Vector(desiredDirection.X * 0.8, desiredDirection.Y);
                }
                Speed *= 1.4;
            }
            else if (CurrentDriveType == DriveTypeStatus.RoutDriven)
            {
                Speed = 0.5;
            }
            else
            {
                Speed = 1;
            }
            return VectorUtils.ResizeVector(desiredDirection, Speed);
        }


        //****************** Internal Methods ******************

        //looks at the first element of the route and passes it to Cartographer. This will return a vector to get us to that location
        //From there we look at the difference between that vector and our direction vector and calculate the difference in degrees
        //we use this to adjust the desired direction vector to account for our faceing. From there we normalize it and return the vector.
        private Vector NavigateFromMap()
        {
            lock (Route)
            {

                mPoint currentPosition = CurrentPose.CurrentPositionInUV;
                Vector currentDirection = CurrentPose.CurrentDirectionVector;

                //if we dont have a route to follow or if the path is empty stop
                if (Route.Count == 0 || Path.Count == 0)
                {
                    return new Vector(0, 0);
                }

                Vector desiredDirection = Cartographer.VectorTo(currentPosition, Path[0]);

                //get the dif in radians between the current direction vector and the desired direction
                double difAngleInRadians = DifInRaidans(desiredDirection, currentDirection);

                // Correct the target angle by 90 degrees
                // Unsure why so far, issue #18 is tracking this
                //difAngleInRadians -= Math.PI;

                // Convert difAngleInRadians to a unit vector
                return VectorUtils.VectorFromAngle(difAngleInRadians);
            }
        }

        private Vector NavigateFromDoorDetection()
        {
            //return new Vector(0, 0);
            return DoorOrientationNavigator.GetDirection();
        }

        //navigate in usermode is safe navigation. It will not let the user drive into a wall or obstructions known by the map.
        private Vector NavigateUserMode(Vector userInputVector)
        {
            return new Vector(0, 0);
        }

        private void InitializeDoorNavigation()
        {
            DoorOrientationNavigator = new DoorOrientationNavigationStrategy(DoorDetector, SonarDetector);
            DoorOrientationNavigator.OnNavigationStrategyEnded += DoorOrientationNavigator_OnNavigationStrategyEnded;
        }

        private void StopDoorNavigation()
        {
            //DoorOrientationNavigator = null;
            DoorOrientationNavigator.OnNavigationStrategyEnded -= DoorOrientationNavigator_OnNavigationStrategyEnded;
        }

        void DoorOrientationNavigator_OnNavigationStrategyEnded(object sender, NavigationStrategyEndedEventArgs eventArgs)
        {
            // If the door navigation wasn't able to successfully finish
            if (!eventArgs.IsComplete)
            {
                // If we encounter an error, return to user control
                CurrentDriveType = DriveTypeStatus.UserDriven;
                return;
            }

            DoorOrientationNavigator.OnNavigationStrategyEnded -= DoorOrientationNavigator_OnNavigationStrategyEnded;
            //DoorOrientationNavigator = null;
            CurrentDriveType = DriveTypeStatus.RoutDriven;
        }

        public double DifInRaidans(Vector startVector, Vector compassDirection)
        {
            double value = Vector.AngleBetween(startVector, compassDirection);
            return AngleUtils.DegreesToRadians(value);
        }

        // CHANGED TODO:
        // This function 
        void ObstacleDetector_ObstaclesDetected(object sender, ObstaclesDetectedArgs e)
        {
            int i = 0;

            setCurrentDriveType(1); // set to user driven

            foreach (Obstacle obstacle in e.Obstacles) // loop through the list for the detected obstacles event
            {
                if (obstacle.rightPoint.X > 0 || obstacle.leftPoint.X > 0) // if the right/left X point is +
                {
                    int right, left, diff;
                    right = (int)obstacle.rightPoint.X; // right X point of the obstacle
                    left = (int)obstacle.leftPoint.X; // left X point of the obstacle
                    diff = right - left; // difference of X values 

                    //List<mPoint> points = this.ObstacleLocalizer.LocalizeObstacle(obstacle); // OLD
                    //this.ObstacleMap.AddObstaclePoints(points, CurrentPose.CurrentRoom); // OLD

                    /*Console.WriteLine("OBSTACLE DETECTED: {0}", i); // Write the number of objects detected
                    Console.WriteLine("Right: {0}, Left: {1} \nDiff: {2}", right, left, diff);
                    Console.WriteLine("Right: ({0},{1}) \nLeft: ({2},{3})",
                        obstacle.rightPoint.X, obstacle.rightPoint.Y, obstacle.leftPoint.X, obstacle.leftPoint.Y);*/

                    double depth = (obstacle.rightPoint.Y + obstacle.leftPoint.Y) / 2; // set average of Y points for depth 
                    //Console.WriteLine("Depth: {0}\n", depth);

                    //if ((obstacle.rightPoint.X - obstacle.leftPoint.X > 350) && obstacle.rightPoint.Y < 600)
                    /*if (depth < 600.0)
                        setCurrentDriveType(5); // stop the motor...
                    else
                        setCurrentDriveType(1); // set to user driven
                        */
                    /*
                    if (i > 1) // possbly change to  == 1
                    {
                        //setCurrentDriveType(4); // stop the motor when an object is detected
                        Console.WriteLine("BRAKED!"); 
                        Thread.Sleep(1000); // Possibly delete or change. // Lets the thread sleep...
                        setCurrentDriveType(1); // Set the drive type back to user driven

                        break;
                    }
                    */
                }

            } // end of for each loop for obstacle events
        } // end of ObstacleDetector_ObstaclesDetected function 

        public int getDistanceFromSonar(int id)
        {
            return ChairSensorArray.SonarArrayDevice.getDistanceAt(id);
        }
    }
}
