using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using Emgu.CV;

namespace Skywalker_Vision.Kinect {
    public class CustomStream : ImageStream, IImageStream {
        private static int EDGEVIEWINSTANCE = 0;
        private static int HOUGHLINEINSTANCE = 1;
        private static int STREAMS = 2;
        private string name;
        private volatile bool isWorking = false;

        private CustomFrame masterFrame;
        private static CustomStream[] _instances = new CustomStream[STREAMS];

        internal CustomStream()
        {
            isWorking = false;
            name = "Custom Stream";
            masterFrame = null;
        }

        public BaseFrame GetFrame() {
            return masterFrame;
        }

        /// <summary>
        /// Returns the stream name
        /// </summary>
        /// <returns>stream name</returns>
        public string GetName()
        {
            return name;
        }

        /// <summary>
        /// Sets string name
        /// </summary>
        /// <param name="newName">The new name</param>
        public void SetName(string newName)
        {
            this.name = newName;
        }

        public static CustomStream GetInstance(int i) {
            if (i >= 0 && _instances.GetLength(0) > i) {
                if (_instances[i] == null) {
                    _instances[i] = new CustomStream();
                }
                return _instances[i];
            }
            return null;
        }

        public static CustomStream GetEdgeViewInstance() {
            CustomStream instance = GetInstance(EDGEVIEWINSTANCE);
            instance.SetName("Edge View");
            return instance;
        }

        public static CustomStream GetHoughLineInstance() {
            CustomStream instance = GetInstance(HOUGHLINEINSTANCE);
            instance.SetName("Hough Line");
            return instance;
        }

        public void SetFrame(WriteableBitmap nBitmap) {
            if (!isWorking)
            {
                isWorking = true;
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += RenderFrame;
                bw.RunWorkerCompleted += Render_Complete;

                ProcessCustomFrameEventArgs args = new ProcessCustomFrameEventArgs();
                args.img = null;
                args.writeablebmp = nBitmap;

                bw.RunWorkerAsync(args);
            }
        }

        public void SetFrame(IImage nImage) {
            if (!isWorking)
            {
                isWorking = true;
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += RenderFrame;
                bw.RunWorkerCompleted += Render_Complete;

                ProcessCustomFrameEventArgs args = new ProcessCustomFrameEventArgs();
                args.img = nImage;
                args.writeablebmp = null;

                bw.RunWorkerAsync(args);
            }
        }

        private void RenderFrame(object sender, DoWorkEventArgs e)
        {
            ProcessCustomFrameEventArgs args = (ProcessCustomFrameEventArgs) e.Argument;

            CustomFrame nFrame = new CustomFrame();
            WriteableBitmap src = null;

            if (args.img != null)
            {
                //if we have an IImage, convert it
                src = new WriteableBitmap(Utils.IImageToBitmapSource(args.img));
                args.img.Dispose();
            }
            else if (args.writeablebmp != null)
            {
                //otherwise we can just use the writeablebitmap
                src = args.writeablebmp;
            }

            //set CustomFrame bitmap
            if (src != null)
            {
                src.Freeze();
                nFrame.SetBitmap(src);
            }
            else
            {
                //easier to avoid setting
                nFrame = null;
            }

            e.Result = nFrame;
        }

        private void Render_Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                CustomFrame frame = (CustomFrame) e.Result;
                if (frame != null)
                {
                    masterFrame = frame;
                }
            }
            catch (Exception err)
            {
                ////Debug.WriteLine("Custom Stream failed to set master frame: " + err.ToString());
            }
            isWorking = false;
        }
    }

    public class ProcessCustomFrameEventArgs
    {
        public WriteableBitmap writeablebmp;
        public IImage img;
    }
}
