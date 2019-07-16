using System;
using System.ComponentModel;
using Microsoft.Kinect;

namespace Skywalker_Vision.Kinect {
    public class VideoStream : ImageStream, IImageStream {
        private static VideoStream instance;
        private static VFrame masterFrame;
        private volatile bool isWorking;

        /// <summary>
        /// Reader for color frames
        /// </summary>
        private ColorFrameReader colorFrameReader = null;

        protected VideoStream() {
            //Set up frame handlers
            // open the reader for the depth frames
            this.colorFrameReader = this.kinect.ColorFrameSource.OpenReader();
            isWorking = false;
            // wire handler for frame arrival
            this.colorFrameReader.FrameArrived += this.Reader_FrameArrived;

            // Open Kinect
            kinect.Open();
        }

        /// <summary>
        /// This is what gets called when a new Color Frame comes in
        /// </summary>
        private void Reader_FrameArrived(object sender, ColorFrameArrivedEventArgs e) {
            if (!isWorking)
            {
                isWorking = true;
                BackgroundWorker bw;
                //set up handlers
                bw = new BackgroundWorker();
                bw.DoWork += RenderFrame;
                bw.RunWorkerCompleted += Render_Complete;

                ProcessFrameEventArgs args = new ProcessFrameEventArgs();
                args.frame = e.FrameReference;
                bw.RunWorkerAsync(args);
            }
        }

        private void RenderFrame(object sender, DoWorkEventArgs e)
        {
            ProcessFrameEventArgs args = (ProcessFrameEventArgs) e.Argument;
            ColorFrameReference reference = args.frame;
            
            using (ColorFrame frame = reference.AcquireFrame()) {
                if (frame != null) {
                    //////Debug.WriteLine("Color frame arrived");
                    e.Result = new VFrame(frame);
                }
            }
        }

        private void Render_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            //finally set frame
            try {
                VFrame frame = (VFrame)e.Result;
                if (frame != null) {
                    masterFrame = frame;
                    ////Debug.WriteLine("Color masterframe set");
                }
            } catch (Exception err) {
                ////Debug.WriteLine("Set frame crashed: " + err.ToString());
            }
            isWorking = false;
        }

        /// <summary>
        /// Returns an instance of VideoStream
        /// </summary>
        public static VideoStream Instance {
            get {
                if (instance == null) {
                    instance = new VideoStream();
                }
                return instance;
            }
        }

        /// <summary>
        /// Returns an intstance of VFrame containing the latest frame
        /// </summary>
        /// <returns></returns>
        public override BaseFrame GetFrame() {
            return masterFrame;
        }

        public string GetName() {
            return "Video Stream";
        }
    }

    public class ProcessFrameEventArgs
    {
        public ColorFrameReference frame;
    }
}
