using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Skywalker.ObjectDetection;

namespace Skywalker.Driver
{
    // This is a navigation strategy whose purpose is to align the vehicle
    // directly in front of a door using a given door detector
    // It is assumed that the vehicle is already reasonably close to a door
    // therefore the closest door is the door that this strategy will 
    // try to orient in front of
    //
    // See INavigationStrategy for an explanation of navigation strategies
    public class DoorOrientationNavigationStrategy : INavigationStrategy
    {
        // The minimum and maximum distance we would like to be from the door to 
        // consider door navigation complete. Unit is centimeters
        private const double DOOR_MIN_TARGET_DISTANCE = 85;
        private const double DOOR_MAX_TARGET_DISTANCE = 95;

        // The minimum and maximum angle we would like to be relative to
        // the door to consider door navigation complete.6 Unit is radians
        private const double DOOR_MIN_TARGET_ANGLE = -Math.PI / 8;
        private const double DOOR_MAX_TARGET_ANGLE = Math.PI / 8;

        public event NavigationStrategyEndedEvent OnNavigationStrategyEnded;
        private DetectedDoor _targetDoor;
        private IDoorDetector _doorDetector;
        private SonarDetector _sonarDetector;

        //*****variable for door navigation with sonar*****
        private int step;
        //tracking whether the sonar sensor detected a door frame
        private List<bool> isSonarDetectedDoorList;
        //check if sonarsensor passed the door
        private List<bool> isSonarPassedList;

        private Vector sonarVector;
        private int sonarNum;
        private bool isForward;
        private int MIN_FRONT_SONAR_DISTANCE = 22;
        private int MIN_MID_SONAR_DISTANCE = 17;
        private int MIN_REAR_SONAR_DISTANCE = 10;
        private int RIGHT_OFFSET = 4;

        //acceptance of the difference between two sides of door frame when
        //passing a door using sonar
        private int SONAR_CALIBARATION_ERROR = 10; 

        public IDoorDetector DoorDetector
        {
            get
            {
                return _doorDetector;
            }

            private set
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

        public SonarDetector SonarDetector
        {
            get
            {
                return _sonarDetector;
            }
            private set
            {
                if(_sonarDetector != null)
                {
                    _sonarDetector.OnDistanceBigChangedDetected -= SonarDetector_OnDistanceChanged;
                    _sonarDetector.OnObstacleTooClosed -= SonarDetector_OnObstacleTooClosed;
                }

                _sonarDetector = value;

            }
        }

        //checking if rear sonar detected passed the door
        public void SonarDetector_OnDistanceChanged(object sender, SonarDistanceChangedEventArgs eventArgs)
        {
            for(int i = 0; i < eventArgs.Sonars.Count; i++)
            {
                SonarSensorInfo info = eventArgs.Sonars[i];
                List<int> currentDistances = this.SonarDetector.currentDistances;
                if (info.ID == i && isSonarDetectedDoorList[i] == false && info.ChangedDifference < 0 && currentDistances[i] < 40)
                {
                    if(i ==0 || i == 3)
                        isSonarDetectedDoorList[i] = true;
                    else if(isSonarPassedList[i-1] == true) //only changed this status when front sensor passed door
                    {
                        isSonarDetectedDoorList[i] = true;
                    }
                }
                if (info.ID == i && isSonarDetectedDoorList[i] == true && info.ChangedDifference > 0)
                {
                    isSonarDetectedDoorList[i] = false;
                    if(isSonarPassedList[i] == false && isForward == true) //passed door
                    {
                        isSonarPassedList[i] = true;
                    }
                    if(isSonarPassedList[i] == true && isForward == false) //back inside
                    {
                        isSonarPassedList[i] = false;
                    }
                }
            }
            if (step == 2)
            {
                if (IsNavigationSuceessfulPassedTheDoor())
                {
                    NotifyNavigationEnded(true);
                    //stop subscribe sonar detector
                    _sonarDetector.OnDistanceBigChangedDetected -= SonarDetector_OnDistanceChanged;
                    _sonarDetector.OnObstacleTooClosed -= SonarDetector_OnObstacleTooClosed;
                }
            }

        }

        private void UpdateSonarVector()
        {
                       
        }
        public void SonarDetector_OnObstacleTooClosed(object sender, SonarDistanceChangedEventArgs eventArgs)
        {
            //TODO: avoid to hit any obstacle 
            List<SonarSensorInfo> sonars = eventArgs.Sonars;
            List<int> currentDistances = this.SonarDetector.currentDistances;
            for (int i = 0; i < sonars.Count; i++)
            {
                if (sonars[i].ID == 0 || sonars[i].ID == 3) //front left sensor too close
                {
                    //check other sonar sensor
                    if (currentDistances[0] < MIN_FRONT_SONAR_DISTANCE && currentDistances[3] < MIN_FRONT_SONAR_DISTANCE + RIGHT_OFFSET) //cant pass
                    {
                        NotifyNavigationEnded(false);
                        //stop subscribe door detector
                        _doorDetector.OnDoorDetected -= DoorDetector_OnDoorDetected;
                    }
                    else if (currentDistances[0] > MIN_FRONT_SONAR_DISTANCE && currentDistances[3] >MIN_FRONT_SONAR_DISTANCE+RIGHT_OFFSET)
                    {
                        sonarVector = new Vector(0, 1); // enough space keep forward
                    }
                    else if ( currentDistances[0] < MIN_FRONT_SONAR_DISTANCE) //too left
                    {
                        sonarVector = new Vector(1, 0); //turn right
                    }
                    else if ( currentDistances[3] < MIN_FRONT_SONAR_DISTANCE + RIGHT_OFFSET)
                    {
                        sonarVector = new Vector(-1,0); //turn left
                    }
                    else
                        sonarVector = new Vector(0, 1); // enough space keep forward
                }

                if (sonars[i].ID == 1 || sonars[i].ID == 4) //front left sensor too close
                {
                    //check other sonar sensor
                    if (currentDistances[1] < MIN_MID_SONAR_DISTANCE && currentDistances[4] < MIN_MID_SONAR_DISTANCE +RIGHT_OFFSET) //cant pass
                    {
                        NotifyNavigationEnded(false);
                        //stop subscribe door detector
                        _doorDetector.OnDoorDetected -= DoorDetector_OnDoorDetected;
                    }
                    else if (currentDistances[1] > MIN_MID_SONAR_DISTANCE && currentDistances[4] > MIN_MID_SONAR_DISTANCE + RIGHT_OFFSET)
                    {
                        sonarVector = new Vector(0, 1); // enough space keep forward
                    }
                    else if (currentDistances[1] < MIN_MID_SONAR_DISTANCE) //too left
                    {
                        if(currentDistances[3] < MIN_FRONT_SONAR_DISTANCE)
                        {
                            sonarVector = new Vector(-1, 1);
                        }
                        else
                            sonarVector = new Vector(1, 0); //turn right
                    }
                    else if (currentDistances[4] < MIN_MID_SONAR_DISTANCE + RIGHT_OFFSET)
                    {
                        if(currentDistances[0] < MIN_FRONT_SONAR_DISTANCE)
                        {
                            sonarVector = new Vector(1, 1);
                        }
                        else
                            sonarVector = new Vector(-1, 0); //turn left
                    }
                    else
                        sonarVector = new Vector(0, 1); // enough space keep forward
                }
            }
        }

        public void DoorDetector_OnDoorDetected(object sender, DoorDetectedEventArgs eventArgs)
        {
            if (step == 1)
            {
                List<DetectedDoor> detectedDoors = eventArgs.DetectedDoors;
                _targetDoor = GetTargetDoor(detectedDoors);

                if (IsNavigationSuccessfulApproachedTheDoor(_targetDoor))
                {
                    //MessageBox.Show("Approached the door");
                    NotifyNavigationEnded(true);
                    //stop subscribe door detector
                    _doorDetector.OnDoorDetected -= DoorDetector_OnDoorDetected;
                }
            }
        }

        private bool IsNavigationSuceessfulPassedTheDoor()
        {
            if(isSonarPassedList[3] == true) //check rear sensor if it passed the door
            {
                MessageBox.Show("Passed the door");
                return true;
            }
            return false;
        }

        public DoorOrientationNavigationStrategy(IDoorDetector doorDetector, SonarDetector sonarDetector)
        {
            this.DoorDetector = doorDetector;
            this.SonarDetector = sonarDetector;
            this.step = 1;
            this.sonarVector = new Vector(0, 1);
            this.isForward = true; //default forward
            this.sonarNum = 5; //default 5 sensors
            this.isSonarDetectedDoorList = new List<bool>();
            this.isSonarPassedList = new List<bool>();
            for(int i=0; i < sonarNum; i++)
            {
                isSonarDetectedDoorList.Add(false);
                isSonarPassedList.Add(false);
            }
        }

        public Vector GetDirection()
        {
            if (step == 1)
            {
                DetectedDoor door = _targetDoor;
                if (door == null)
                {
                    return new Vector(0, 0);
                }

                // RelativeAngle is expressed such that 0 radians means the door is directly in front
                // of the vehicle whereas trigonometrically 0 radians represents the right direction.
                // We need to add PI / 2 to the angle in order for trig functions to work properly
                double doorAngle = door.RelativeAngle + Math.PI / 2;
                double xComponent = Math.Cos(doorAngle);
                double yComponent = Math.Sin(doorAngle);

                return new Vector(xComponent, yComponent);
                //if(door.RelativeAngle > 0.15)
                //{
                //    return new Vector(-1, 0);
                //}
                //else if(door.RelativeAngle < -0.15)
                //{
                //    return new Vector(1, 0);
                //}
                //else
                //{
                //    return new Vector(0, 1);
                //}
            }
            else if (step == 2)
            {
                //get direction from sonarVector
                //default value is (0,1) going forward
                return sonarVector;
            }
            else
                throw new Exception();
        }


        // Returns the door that we should select to navigate to (the target door)
        // The target door is chosen as the door within the given list of doors with the
        // minimum RelativeDistance
        private DetectedDoor GetTargetDoor(List<DetectedDoor> doors)
        {
            if (doors == null)
                return null;
            double minDistance = double.MaxValue;
            DetectedDoor closestDoor = null;

            foreach (DetectedDoor door in doors)
            {
                if (door.RelativeDistance < minDistance)
                {
                    closestDoor = door;
                    minDistance = door.RelativeDistance;
                }
            }

            return closestDoor;
        }

        // Return true iff we are properly lined up with the door
        private bool IsNavigationSuccessfulApproachedTheDoor(DetectedDoor targetDoor)
        {
            if (targetDoor == null)
            {
                return false;
            }

            double angle = targetDoor.RelativeAngle;
            double distance = targetDoor.RelativeDistance;
            bool res = angle >= DOOR_MIN_TARGET_ANGLE && angle <= DOOR_MAX_TARGET_ANGLE
                   && distance >= DOOR_MIN_TARGET_DISTANCE && distance <= DOOR_MAX_TARGET_DISTANCE;
            return res;
        }

        private void NotifyNavigationEnded(bool isSuccessful)
        {
            if (OnNavigationStrategyEnded != null)
            {
                if (step ==1)
                {
                    step++;
                    //navigate based on door detection finished
                    //now using sonar to make sure we can safely go throught the door
                    //could go back to step 1 if it's neccesary
                    if(_sonarDetector != null)
                    {
                        _sonarDetector.OnDistanceBigChangedDetected += SonarDetector_OnDistanceChanged;
                        _sonarDetector.OnObstacleTooClosed += SonarDetector_OnObstacleTooClosed;
                    }

                    if(!isSuccessful) //fail to approaching door change to manaul mode
                    {
                        NavigationStrategyEndedEventArgs args = new NavigationStrategyEndedEventArgs(isSuccessful, step);
                    }
                }
                else if (step == 2)
                {
                    NavigationStrategyEndedEventArgs args = new NavigationStrategyEndedEventArgs(isSuccessful, step);
                    OnNavigationStrategyEnded(this, args);
                }
            }
        }
    }
}
