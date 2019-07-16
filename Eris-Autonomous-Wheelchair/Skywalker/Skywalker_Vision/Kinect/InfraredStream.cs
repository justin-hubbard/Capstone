using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.ComponentModel;
using Microsoft.Kinect;

namespace Skywalker_Vision.Kinect
{
        public class InfraredStream : ImageStream, IImageStream
        {
            private static InfraredStream instance;
            private static IFrame masterFrame;
            private InfraredFrameReader infraredFrameReader = null;
            private volatile bool isWorking = false;

            protected InfraredStream()
            {
                //Set up frame handlers
                // open the reader for the depth frames
                this.infraredFrameReader = this.kinect.InfraredFrameSource.OpenReader();
                isWorking = false;
                // wire handler for frame arrival
                this.infraredFrameReader.FrameArrived += this.Reader_FrameArrived;

                // get FrameDescription from DepthFrameSource
                this.frameDescriptor = this.kinect.DepthFrameSource.FrameDescription;

                // Open Kinect
                kinect.Open();
            }

            private void Render_Complete(object sender, RunWorkerCompletedEventArgs e)
            {
                //finally set frame
                try
                {
                    IFrame frame = (IFrame)e.Result;
                    if (frame != null)
                    {
                        masterFrame = frame;
                        //////Debug.WriteLine("Depth masterframe set");
                    }
                }
                catch (Exception err)
                {
                    ////Debug.WriteLine("Set frame crashed: " + err.ToString());
                }
                isWorking = false;
            }

            private void RenderFrame(object sender, DoWorkEventArgs e)
            {
                ProcessInfraredFrameEventArgs args = (ProcessInfraredFrameEventArgs)e.Argument;
                InfraredFrameReference reference = args.frame;

                using (InfraredFrame frame = reference.AcquireFrame())
                {
                    if (frame != null)
                    {
                        //////Debug.WriteLine("Depth frame arrived");
                        try
                        {
                            e.Result = new IFrame(frame);
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }

            /// <summary>
            /// This is what gets called when a new Depth Frame comes in
            /// </summary>
            private void Reader_FrameArrived(object sender, InfraredFrameArrivedEventArgs e)
            {
                if (!isWorking)
                {
                    isWorking = true;
                    BackgroundWorker bw;

                    ProcessInfraredFrameEventArgs args = new ProcessInfraredFrameEventArgs();

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
            public static InfraredStream Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new InfraredStream();
                    }
                    return instance;
                }
            }

            /// <summary>
            /// Returns an intstance of DFrame containing the latest frame
            /// </summary>
            /// <returns></returns>
            public override BaseFrame GetFrame()
            {
                return masterFrame;
            }

            public string GetName()
            {
                return "Infrared Stream";
            }
        }

        public class ProcessInfraredFrameEventArgs
        {
            public InfraredFrameReference frame;
        }
}
