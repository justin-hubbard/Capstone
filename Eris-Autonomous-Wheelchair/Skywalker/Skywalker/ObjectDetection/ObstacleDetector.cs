using Skywalker.Resources;

namespace Skywalker.ObjectDetection
{
    using System.Threading;
    using Skywalker_Vision.Kinect;

    using System;
    using System.Windows.Media.Imaging;
    using System.Drawing.Imaging;
    using System.IO;

    using Emgu.CV;
    using Emgu.CV.Structure;
    using System.Windows;
    using System.Drawing;
    using System.Collections.Generic;

    /// <summary>
    /// Detects objects in the path of the vehicle.
    /// </summary>
    public class ObstacleDetector : IObstacleDetector
    {
        /// <summary>
        /// The vision, through which objects are detected.
        /// </summary>
        //  private IDepthStream vision;
        private IImageStream vision;
        /// <summary>
        /// The watcher thread.
        /// </summary>
        private Thread watchThread;
        private int corX = 0;
        private int corY = 0;
        private volatile List<Obstacle> _obstacleData;
        private CustomStream CStream;

        public List<Obstacle> ObstacleData
        {
            get
            {
                return _obstacleData ?? new List<Obstacle>();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObstacleDetector"/> class.
        /// </summary>
        /// <param name="vision">
        /// The IDepthSteram vision stream
        /// </param>
        /// <param name="autoStart">
        /// Whether or not the detector should start automatically
        /// </param>

        public ObstacleDetector(IImageStream vision, bool autoStart = true)
        {
            this.vision = vision;
            this.watchThread = new Thread(this.Watch);
            CStream = CustomStream.GetEdgeViewInstance();

            if (autoStart)
            {
                Start();
            }
        }

        ~ObstacleDetector()
        {
            this.watchThread.Abort();
        }

        public void Start()
        {
            this.watchThread.Start();
        }

        /// <summary>
        /// Event raised when an object is detected.
        /// </summary>
        public event ObstaclesDetectedEvent ObstaclesDetected;

        /// <summary>
        /// Watches for obstacles.
        /// </summary>
        public void Watch()
        {
            // TODO : Obstacle detection
            // Check whether the thread has previously been named 
            // to avoid a possible InvalidOperationException.
            this.watchThread.Name = "ObstructionThread";

            while (true)
            {
                DFrame myFrame = (DFrame)vision.GetFrame();

                Detect(myFrame);
                Thread.Sleep(200);
            }
        }

        private class Edge 
        {
            private int minX;
            private int maxX;
            private int minY;
            private int maxY;
            public int distance;

            public Edge(int minX, int maxX, int minY, int maxY, int distance)
            {
                this.minX = minX;
                this.maxX = maxX;
                this.minY = minY;
                this.maxY = maxY;
                this.distance = distance;
            }

            public int getMinX()
            {
                return minX;
            }

            public int getMaxX()
            {
                return maxX;
            }

            public int getMinY()
            {
                return minY;
            }

            public int getMaxY()
            {
                return maxY;
            }
        }

        public void Detect(DFrame myFrame)
        {
            if (myFrame != null)
            {
                Bitmap bmp2 = myFrame.GetBMP();
                Image<Bgr, Byte> img;
                Image<Gray, Byte> grey;

                try
                {
                    img = new Image<Bgr, Byte>(bmp2);
                    grey = img.Convert<Gray,Byte>();
                    CvInvoke.cvInRangeS(img.Ptr, new MCvScalar(0.0, 0.0, 0.0), new MCvScalar(250.0, 250.0, 250.0), grey.Ptr);
                    CvInvoke.cvErode(grey.Ptr, grey.Ptr, (IntPtr)null, 4);
                }
                catch (Exception)
                {
                    return;
                }

                double area = area_check(grey);
                List<Edge> myEdge = new List<Edge>();
                List<int> minDepthList = new List<int>();
                List<int> counterList = new List<int>();

                int minX = 512;
                int maxX = 0;

                int minY = 424;
                int maxY = 0;

                using (MemStorage storage = new MemStorage())
                {
                    CStream.SetFrame(grey.Copy());
                    for (Contour<System.Drawing.Point> contours = grey.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_EXTERNAL, storage); contours != null; contours = contours.HNext)
                    {
                        Contour<System.Drawing.Point> currentContour = contours.ApproxPoly(contours.Perimeter * 0.015, storage);
                        System.Drawing.Point[] pts = currentContour.ToArray();

                        foreach (System.Drawing.Point p in pts)
                        {
                            if (p.X < minX)
                            {
                                minX = p.X;
                            } 
                            else if (p.X > maxX)
                            {
                                maxX = p.X;
                            }

                            if (p.Y < minY)
                            {
                                minY = p.Y;
                            } 
                            else if (p.Y > maxY)
                            {
                                maxY = p.Y;
                            }

                        }

                        if (maxX > minX && maxY > minY)
                        {
                            myEdge.Add(new Edge(minX, maxX, minY, maxY, 0));
                        }

                        minX = 512;
                        maxX = 0;

                        minY = 424;
                        maxY = 0;
                    }

                    int minDepth = 999999;
                    int counter = 0;
                    ushort[] depthdata = myFrame.get_DepthData();

                    foreach (var edge in myEdge)
                    {
                        for (int i = edge.getMinY(); i < edge.getMaxY(); i += 10)
                        {
                            for (int j = edge.getMinX(); j < edge.getMaxX(); j += 10)
                            {
                                int tmp = depthdata[(j) + (i * 512)];
                                if (tmp < minDepth && tmp != 0)
                                {
                                    minDepth = tmp;
                                }
                            }
                        }

                        if (minDepth != 999999)
                        {
                            minDepthList.Add(minDepth);
                        } 
                        else
                        {
                            counterList.Add(counter);
                        }

                        minDepth = 999999;
                        counter++;
                    }

                    foreach (var index in counterList)
                    {
                        for (int i = myEdge.Count - 1; i > -1; i--)
                        {
                            if (i == index)
                            {
                                myEdge.RemoveAt(i);
                            }
                        }
                    }
                    counterList.Clear();

                    int kk = 0;
                    foreach (var ele in minDepthList)
                    {
                        myEdge[kk].distance = ele;
                        kk++;
                    }

                    List<Obstacle> obstacles = new List<Obstacle>();
                    foreach (Edge edge in myEdge)
                    {
                        Obstacle obstacle = EdgeToObstacle(edge);

                        float frameWidth = grey.Width;
                        float frameHeight = grey.Height;

                        float leftPercentage = edge.getMinX() / frameWidth;
                        float topPercentage = edge.getMinY() / frameHeight;

                        float widthPercentage = (edge.getMaxX() - edge.getMinX()) / frameWidth;
                        float heightPercentage = (edge.getMaxY() - edge.getMinY()) / frameHeight;

                        System.Drawing.Size frameSize = new System.Drawing.Size((int)frameWidth, (int)frameHeight);
                        ObstacleFrameInfo frameInfo = new ObstacleFrameInfo(leftPercentage, topPercentage, widthPercentage, heightPercentage, frameSize);
                        obstacle.FrameInfo = frameInfo;
                        obstacles.Add(obstacle);
                    }
                    _obstacleData = obstacles;

                    NotifyObstaclesDetectedEvent(obstacles);
                }
            }
        }

        /// <summary>
        /// Takes an Obstacle object whose bounds
        /// are measured in relative screen position
        /// (e.g. leftEdge and rightEdge are pixel positions)
        /// and converts them to absolute positions in the world,
        /// measured in distance from our position.
        /// </summary>
        /// <param name="obstacle"></param>
        private Obstacle EdgeToObstacle(Edge edge)
        {
            double leftAngle = (edge.getMinX() * (Config.Instance.DEPTH_VIEW_ANGLE / Config.Instance.DEPTH_PIXEL_WIDTH))
                - Config.Instance.DEPTH_VIEW_ANGLE / 2;
            double rightAngle = (edge.getMaxX() * (Config.Instance.DEPTH_VIEW_ANGLE / Config.Instance.DEPTH_PIXEL_WIDTH))
                - Config.Instance.DEPTH_VIEW_ANGLE / 2;

            leftAngle *= -1;
            rightAngle *= -1;

            Vector leftPoint = new Vector(Math.Sin(rightAngle) * edge.distance, Math.Cos(rightAngle) * edge.distance);
            Vector rightPoint = new Vector(Math.Sin(leftAngle) * edge.distance, Math.Cos(leftAngle) * edge.distance);

            return new Obstacle(leftPoint, rightPoint);
        }

        private double area_check(IImage bw)
        {
            double area;
            MCvMoments moments = new MCvMoments();
            CvInvoke.cvMoments(bw.Ptr, ref moments, 1);

            double moment10 = CvInvoke.cvGetSpatialMoment(ref moments, 1, 0);
            double moment01 = CvInvoke.cvGetSpatialMoment(ref moments, 0, 1);
            area = CvInvoke.cvGetCentralMoment(ref moments, 0, 0);
            corX = (int)(moment10 / area);
            corY = (int)(moment01 / area);
            return area;
        }

        private void NotifyObstaclesDetectedEvent(List<Obstacle> obstacles)
        {
            if (this.ObstaclesDetected != null)
            {
                ObstaclesDetectedArgs args = new ObstaclesDetectedArgs(obstacles);
                this.ObstaclesDetected(this, args);
            }
        }
    }
}
