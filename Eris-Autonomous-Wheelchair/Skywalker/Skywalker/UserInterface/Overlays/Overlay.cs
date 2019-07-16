using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Skywalker.UserInterface.Overlays
{
    public abstract class Overlay
    {
        // The name of the overlay. This is what will be displayed when choosing which overlays to enable
        public String Name
        {
            get;
            protected set;
        }

        // Whether or not this overlay should actually render
        public bool Enabled
        { 
            get; 
            set; 
        }

        protected Overlay()
        {
            this.Enabled = true;
        }

        // Add any visual elements to the given canvas. The given width and height are the
        // width and height of the image the overlays are over
        // If the width and height don't match the actual width and height of the canvas (due to window resizing or
        // mismatched stream and window aspect ratios) then position elements as if the width and height are 
        // centered at the center of the canvas
        public abstract void RenderInCanvas(Canvas canvas, double width, double height);
    }
}
