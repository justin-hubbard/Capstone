using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Skywalker.UserInterface.Overlays
{
    public class OverlayRenderer
    {
        /*
         * The list of overlays to render
         * Overlays are rendered in the order in this list
         * The element Overlays[0] is rendered first on the bottom
         * and Overlays[n] is rendered last on top
         */
        public List<Overlay> Overlays
        {
            get;
            private set;
        }

        public Canvas OverlayCanvas
        {
            get;
            set;
        }

        public OverlayRenderer()
        {
            this.Overlays = new List<Overlay>();
        }

        public void RenderOverlays(double width, double height)
        {
            this.OverlayCanvas.Children.Clear();

            foreach (Overlay overlay in this.Overlays)
            {
                if (overlay.Enabled)
                {
                    overlay.RenderInCanvas(this.OverlayCanvas, width, height);
                }
            }
        }
    }
}
