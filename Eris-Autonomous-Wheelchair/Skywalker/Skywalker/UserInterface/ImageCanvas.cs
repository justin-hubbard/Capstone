using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Skywalker.UserInterface
{
    class ImageCanvas : Canvas
    {
        public ImageSource CanvasImageSource
        {
            get { return (ImageSource)GetValue(CanvasImageSourceProperty); }
            set { SetValue(CanvasImageSourceProperty, value); }
        }

        public static readonly DependencyProperty CanvasImageSourceProperty =
            DependencyProperty.Register("CanvasImageSource", typeof(ImageSource),
            typeof(ImageCanvas), new FrameworkPropertyMetadata(default(ImageSource)));

        protected override void OnRender(System.Windows.Media.DrawingContext dc)
        {
            dc.DrawImage(CanvasImageSource, new Rect(this.RenderSize));
            base.OnRender(dc);
        }

    }
}
