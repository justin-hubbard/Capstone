using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skywalker.Sensors.SensorArray;

using System.Threading;

using System.IO;


namespace Skywalker.ObjectDetection
{
    public delegate void SonarDistanceChangedEvent(object sender, SonarDistanceChangedEventArgs eventArgs);

    public class SonarDetector
    {
        public event SonarDistanceChangedEvent OnDistanceBigChangedDetected;
        public event SonarDistanceChangedEvent OnObstacleTooClosed;

        private SensorArray sensorArray;
        private Thread processingThread;

        private List<int> historicalDistances;

        //private List<LimitedQueue<int>> currentList;
        //private List<LimitedQueue<int>> historicalList;

        private int sensorNum;
        private List<bool> isSensorDetectedObstacle;

        // UPDATE_INTERVAL represents how often checking new value of distance detetcted by sonar
        // sensors. The unit is milliseconds
        private const int UPDATE_INTERVAL = 100; //temp value

        // The Max value of difference of distance between historical value an monitoring value
        // Otherwise, we raise an event
        private const int MAX_DIFFERENCE = 20; //temp value

        private const int MIN_DISTANCE = 25;

        private const int Current_SIZE = 5; //temp value
        private const int Historical_SIZE = 5; //temp value

        public SonarDetector()
        {
            sensorNum = 5; 
            currentDistances = new List<int>();
            historicalDistances = new List<int>();
            isSensorDetectedObstacle = new List<bool>();
            //currentList = new List<LimitedQueue<int>>();
            //historicalList = new List<LimitedQueue<int>>();
            for (int i = 0; i < sensorNum; i++ )
            {
                currentDistances.Add(-1);
                historicalDistances.Add(-1);
                isSensorDetectedObstacle.Add(false);
                //currentList.Add(new LimitedQueue<int>(Current_SIZE));
                //historicalList.Add(new LimitedQueue<int>(Current_SIZE));
            }
            updateDistanceData();
            updateHistoricalData();
            IsRunning = false;
        }

        public List<int> currentDistances
        {
            get;
            private set;
        }


        public bool IsRunning
        {
            get;
            private set;
        }

        // Begin asynchronously processing sonar distance array
        // If it is currently running, IsRunning will be set to true.
        // When the sonar detector is running, it will fire OnDistanceChanged events and OnObstacleTooClosed events
        // Call CancelAsync() to cancel this process.
        // Precondition: IsRunning is false
        // Postcondition: IsRunning is true
        public void RunAsync(SensorArray sensorArray)
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("This SonarDetector is already running");
            }

            IsRunning = true;
            this.sensorArray = sensorArray;
            updateDistanceData();
            updateHistoricalData();
            processingThread = new Thread(ProcessingThread_DoWork);
            processingThread.Start();
        }

        // Cancels the operation created by RunAsync.
        // Precondition: IsRunning is true
        // Postcondition: IsRunning is false
        public void CancelAsync()
        {
            if (!IsRunning)
            {
                throw new InvalidOperationException("This SonarDetector is not running");
            }

            processingThread.Abort();
            processingThread = null;
            sensorArray = null;
            IsRunning = false;
        }

        private void ProcessingThread_DoWork()
        {
            while (true)
            {
                //get current distances
                if(sensorArray != null)
                {
                    updateDistanceData();
                }

                if (currentDistances != null && historicalDistances != null)
                {
                    MonitorSonarSensor();
                    updateHistoricalData();
                }
                Thread.Sleep(UPDATE_INTERVAL);

            }
        }


        //update current distance data 
        void updateDistanceData()
        {
            for (int i = 0; i < sensorNum; i++)
            {
                try
                {
                    int distance = sensorArray.SonarArrayDevice.getDistanceAt(i);
                    if (distance < 6)
                    {
                        distance = historicalDistances[i];
                    }

                    currentDistances[i] = distance;

                    //Console.WriteLine("----------------------------------------");
                    //Console.WriteLine("*** Output current distances ***");
                    //Console.WriteLine("Sonar sensor " + i.ToString() + ":");
                    //Console.WriteLine(distance.ToString());
                    //Console.WriteLine("----------------------------------------");
                }
                catch (Exception e)
                {

                }

            }
            //check if any sonar too closed to the door
            for (int i = 0; i < sensorNum; i++)
            {
                Console.WriteLine("----------------------------------------");
                Console.WriteLine("*** Output current distances ***");
                Console.WriteLine("Sonar sensor " + i.ToString() + ":");
                Console.WriteLine(currentDistances[i].ToString());
                Console.WriteLine("----------------------------------------");

                if ((currentDistances[i] <= MIN_DISTANCE)
                    || (currentDistances[i] > MIN_DISTANCE && isSensorDetectedObstacle[i] == true))
                {

                    if(currentDistances[i] <=MIN_DISTANCE)
                        isSensorDetectedObstacle[i] = true;
                    else
                        isSensorDetectedObstacle[i] = false;

                    List<SonarSensorInfo> sensors = new List<SonarSensorInfo>();
                    sensors.Add(new SonarSensorInfo(i, currentDistances[i], 0));
                    SonarDistanceChangedEventArgs args = new SonarDistanceChangedEventArgs(sensors);
                    if (OnObstacleTooClosed != null)
                    {
                        OnObstacleTooClosed(this, args);
                    }
                }
            }
        }

        //update historical distance date
        void updateHistoricalData()
        {
            for(int i = 0; i < sensorNum; i++)
            {
                historicalDistances[i] = currentDistances[i];
            }
        }

        //check if any sonar sensor sensed a big change of distance
        void MonitorSonarSensor()
        {
            List<SonarSensorInfo> sensors = GetBigDistanceChangedSensor();
            if (sensors.Count > 0)
            {
                SonarDistanceChangedEventArgs args = new SonarDistanceChangedEventArgs(sensors);
                if (OnDistanceBigChangedDetected != null)
                {
                    OnDistanceBigChangedDetected(this, args);
                }
            }
        }
        
        //Return a list of Tuple, which contains sensor id and differnces
        private List<SonarSensorInfo> GetBigDistanceChangedSensor()
        {
            List<SonarSensorInfo> res = new List<SonarSensorInfo>();
            for(int i=0; i < historicalDistances.Count; i++)
            {
                int difference = currentDistances[i] - historicalDistances[i];
                if(difference > MAX_DIFFERENCE || difference < - MAX_DIFFERENCE)
                {
                    SonarSensorInfo sensor = new SonarSensorInfo(i, currentDistances[i], difference);
                    res.Add(sensor);
                    //if(i == 0)
                    //{
                    //    int test = i;
                    //}
                }
            }
            return res;
        }
    }
    public class LimitedQueue<T> : Queue<T>
    {
        private int limit = -1;

        public int Limit
        {
            get { return limit; }
            set { limit = value; }
        }

        public LimitedQueue(int limit)
            : base(limit)
        {
            this.Limit = limit;
        }

        public new void Enqueue(T item)
        {
            if (this.Count >= this.Limit)
            {
                this.Dequeue();
            }
            base.Enqueue(item);
        }
    }
}
