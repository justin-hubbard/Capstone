using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skywalker.Motor;

namespace Skywalker_TEST.Driver.Mocks
{
    /// <summary>
    /// Mock of the Vehicle interface.
    /// </summary>
    public class VehicleMock : IVehicle
    {
        public float SpeedSetTo;

        public Vector RotationSetTo;

        public void SetRotation(Vector rotation)
        {
            RotationSetTo = rotation;
        }

        public void SetSpeed(float speed, float delta)
        {
            SpeedSetTo = speed;
        }

        public void SetX(int x_percent)
        {
            // Set X 
        }

        public void SetY(int y_percent)
        {
            // Set Y
        }

        public void Start()
        {
        }

        public void End()
        {
        }

        public void SetSpeedAndDirection(int speed, Vector direction)
        {
            this.SpeedSetTo = speed;
            this.RotationSetTo = direction;
        }
    }
}