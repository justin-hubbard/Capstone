using System;
using System.ComponentModel;
using Microsoft.Kinect;

namespace Skywalker_Vision.Kinect {
    public class DepthStream : ImageStream, IImageStream {
        private static DepthStream instance;
        private static DFrame masterFrame;
        private DepthFrameReader depthFrameReader = null;
        private volatile bool isWorking = false;

        protected DepthStream() {
            //Set up frame handlers
            // open the reader for the depth frames
            this.depthFrameReader = this.kinect.DepthFrameSource.OpenReader();
            isWorking = false;
            // wire handler for frame arrival
            this.depthFrameReader.FrameArrived += this.Reader_FrameArrived;

            // get FrameDescription from DepthFrameSource
            this.frameDescriptor = this.kinect.DepthFrameSource.FrameDescription;

            // Open Kinect
            kinect.Open();
        }

        private void Render_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            //finally set frame
            try {
                DFrame frame = (DFrame)e.Result;
                if (frame != null) {
                    masterFrame = frame;
                    //////Debug.WriteLine("Depth masterframe set");
                }
            } catch (Exception err) {
                ////Debug.WriteLine("Set frame crashed: " + err.ToString());
            }
            isWorking = false;
        }

        private void RenderFrame(object sender, DoWorkEventArgs e)
        {
            ProcessDepthFrameEventArgs args = (ProcessDepthFrameEventArgs)e.Argument;
            DepthFrameReference reference = args.frame;
            
            using (DepthFrame frame = reference.AcquireFrame())
            {
                if (frame != null)
                {
                    //////Debug.WriteLine("Depth frame arrived");
                    try
                    {
                        e.Result = new DFrame(frame);
                    }
                    catch(Exception)
                    {

                    }
                }
            }
        }

        /// <summary>
        /// This is what gets called when a new Depth Frame comes in
        /// </summary>
        private void Reader_FrameArrived(object sender, DepthFrameArrivedEventArgs e) {
            if (!isWorking)
            {
                isWorking = true;
                BackgroundWorker bw;

                ProcessDepthFrameEventArgs args = new ProcessDepthFrameEventArgs();

                args.frame = e.FrameReference;

                //set up background worker
                bw = new BackgroundWorker();
                bw.DoWork += RenderFrame;
                bw.RunWorkerCompleted += Render_Complete;
                bw.RunWorkerAsync(args);
            }
            else
            {
                ////Debug.WriteLine("Depth fram already working.");
            }
        }


        /// <summary>
        /// Returns a DepthStream instance
        /// </summary>
        public static DepthStream Instance {
            get {
                if (instance == null) {
                    instance = new DepthStream();
                }
                return instance;
            }
        }

        /// <summary>
        /// Returns an intstance of DFrame containing the latest frame
        /// </summary>
        /// <returns></returns>
        public override BaseFrame GetFrame() {
            return masterFrame;
        }

        public string GetName() {
            return "Depth Stream";
        }
    }

    public class ProcessDepthFrameEventArgs {
        public DepthFrameReference frame;
    }
}
