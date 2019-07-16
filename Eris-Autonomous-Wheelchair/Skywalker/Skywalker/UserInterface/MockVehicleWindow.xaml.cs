using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Skywalker.Motor;

namespace Skywalker.UserInterface
{
    /// <summary>
    /// Interaction logic for MockVehicleWindow.xaml
    /// </summary>
    public partial class MockVehicleWindow : Window
    {
        private Vector currentSpeed;
        private Line arrow;

        public MockVehicleWindow(MockVehicle vehicle = null)
        {
            if (vehicle != null)
            {
                vehicle.OnXChanged += vehicle_OnXChanged;
                vehicle.OnYChanged += vehicle_OnYChanged;
            }

            InitializeComponent();

            Point origin = new Point(this.Canvas.Width / 2, this.Canvas.Height / 2);
            AddBackground(this.Canvas, origin);

            arrow = new Line();
            arrow.Stroke = Brushes.Red;
            this.Canvas.Children.Add(arrow);
        }

        void vehicle_OnYChanged(IVehicle sender, PositionChangedEventArgs args)
        {
            currentSpeed.Y = args.NewPosition;
            this.Dispatcher.Invoke(RenderDirection);
        }

        void vehicle_OnXChanged(IVehicle sender, PositionChangedEventArgs args)
        {
            currentSpeed.X = args.NewPosition;
            this.Dispatcher.Invoke(RenderDirection);
        }

        // HACK: For use debugging our direction in Navigator
        // using this UI to display current direction
        public void RenderDirection(Vector direction)
        {
            Canvas canvas = this.Canvas;
            Point origin = new Point(canvas.Width / 2, canvas.Height / 2);
            RenderDirection(canvas, direction, origin);
        }

        private void RenderDirection()
        {
            Canvas canvas = this.Canvas;
            Point origin = new Point(canvas.Width / 2, canvas.Height / 2);
            RenderDirection(canvas, this.currentSpeed, origin);
        }

        private void RenderDirection(Canvas canvas, Vector direction, Point origin)
        {
            RenderArrow(canvas, direction, origin);
        }

        private void RenderArrow(Canvas canvas, Vector direction, Point origin)
        {
            direction = ConvertVehicleDirectionToScreenDirection(direction);
            Vector arrowLength = ConvertDirectionToCanvasLength(canvas, direction);
            Point arrowTipPoint = origin + arrowLength;

            arrow.X1 = origin.X;
            arrow.Y1 = origin.Y;
            arrow.X2 = arrowTipPoint.X;
            arrow.Y2 = arrowTipPoint.Y;
        }

        // The wheelchair uses positive Y as forward but WPF uses positive y as downward
        // Flip the Y component
        private Vector ConvertVehicleDirectionToScreenDirection(Vector vehicleDirection)
        {
            return new Vector(vehicleDirection.X, -vehicleDirection.Y);
        }

        // The given direction vector has domain and range [-100, 100]
        // Return a vector which maps the given diretion vector to point values
        private Vector ConvertDirectionToCanvasLength(Canvas canvas, Vector direction)
        {
            double canvasSize = canvas.Width;
            return (direction / 100) * (canvasSize * 0.75 / 2);
        }

        private void AddBackground(Canvas canvas, Point origin)
        {
            double canvasSize = canvas.Width;

            double outerCircleDiameter = canvasSize * 0.75;
            Ellipse outerCircle = new Ellipse();
            outerCircle.Width = outerCircleDiameter;
            outerCircle.Height = outerCircleDiameter;
            outerCircle.Stroke = Brushes.Gray;

            double innerCircleDiameter = canvasSize * 0.375;
            Ellipse innerCircle = new Ellipse();
            innerCircle.Width = innerCircleDiameter;
            innerCircle.Height = innerCircleDiameter;
            innerCircle.Stroke = Brushes.Gray;

            Canvas.SetLeft(outerCircle, (canvasSize - outerCircleDiameter) / 2);
            Canvas.SetTop(outerCircle, canvasSize - (origin.Y + outerCircleDiameter / 2));

            Canvas.SetLeft(innerCircle, (canvasSize - innerCircleDiameter) / 2);
            Canvas.SetTop(innerCircle, canvasSize - (origin.Y + innerCircleDiameter / 2));

            canvas.Children.Add(outerCircle);
            canvas.Children.Add(innerCircle);
        }
    }
}
