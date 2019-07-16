using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using Skywalker.Driver;
using Skywalker_TEST.Driver.Mocks;

namespace Skywalker_TEST.Driver
{
    using Skywalker.Resources;

    /// <summary>
    /// Tests the Driver class.
    /// Since the driver is a mediator,
    /// it is tested by mocking its composing objects
    /// and simulating their behaviors, to ensure that
    /// it coordinates them accurately.
    /// </summary>
    [TestClass]
    public class DriverTester
    {
        /// <summary>
        /// Tests that the driver will stop the vehicle
        /// upon detection of an object from the
        /// Obstacle Detector.
        /// </summary>
        [TestMethod]
        public void StopOnObjectDetectionTest()
        {
            var navigatorMock = new NavigatorMock();
            var inputMock = new InputMock();
            var obstacleDetectorMock = new ObstacleDetectorMock();
            var vehicleMock = new VehicleMock();

            navigatorMock.DirectionToReturn = new Vector(0,0);
            var driver = new Skywalker.Driver.Driver(inputMock, vehicleMock, navigatorMock);
            driver.Start();
            Thread.Sleep(100);

            Assert.IsTrue((int)vehicleMock.SpeedSetTo == 100);

            obstacleDetectorMock.FireObjectDetection();
            Thread.Sleep(100);

            Assert.IsTrue((int)vehicleMock.SpeedSetTo == 0);
        }

        [TestMethod]
        public void ReverseAfterStoppingTest()
        {
            var navigatorMock = new NavigatorMock();
            var inputMock = new InputMock();
            var obstacleDetectorMock = new ObstacleDetectorMock();
            var vehicleMock = new VehicleMock();

            navigatorMock.DirectionToReturn = new Vector(1, 0);
            var driver = new Skywalker.Driver.Driver(inputMock, vehicleMock, navigatorMock);
            driver.Start();
            Thread.Sleep(100);

            Assert.IsTrue((int)vehicleMock.SpeedSetTo == 100);

            obstacleDetectorMock.FireObjectDetection();
            Thread.Sleep(100);

            Assert.IsTrue((int)vehicleMock.SpeedSetTo == 0);

            navigatorMock.DirectionToReturn = new Vector(0, 0);
            Thread.Sleep(100);
            navigatorMock.DirectionToReturn = new Vector(0, -1);
            Thread.Sleep(100);
            Assert.IsTrue((int)vehicleMock.SpeedSetTo == 100);
            Assert.IsTrue(vehicleMock.RotationSetTo.Y > 1 - 0.001 &&
                          vehicleMock.RotationSetTo.Y < 1 + 0.001);
        }

        [TestMethod]
        public void TurningTest()
        {
            var navigatorMock = new NavigatorMock();
            var inputMock = new InputMock();
            var obstacleDetectorMock = new ObstacleDetectorMock();
            var vehicleMock = new VehicleMock();

            navigatorMock.DirectionToReturn = new Vector(1, 0);
            var driver = new Skywalker.Driver.Driver(inputMock, vehicleMock, navigatorMock);
            driver.Start();
            Thread.Sleep(100);

            Assert.IsTrue(vehicleMock.RotationSetTo.X > 1 - 0.001 &&
                          vehicleMock.RotationSetTo.X < 1 + 0.001);

            navigatorMock.DirectionToReturn = new Vector(-1, 0); ;
            Thread.Sleep(100);

            Assert.IsTrue(vehicleMock.RotationSetTo.X > -1 - 0.001 &&
                          vehicleMock.RotationSetTo.X < -1 + 0.001);
        }

        [TestMethod]
        public void TurnAndStopTest()
        {
            var navigatorMock = new NavigatorMock();
            var inputMock = new InputMock();
            var obstacleDetectorMock = new ObstacleDetectorMock();
            var vehicleMock = new VehicleMock();

            navigatorMock.DirectionToReturn = new Vector(1, 0); ;
            var driver = new Skywalker.Driver.Driver(inputMock, vehicleMock, navigatorMock);
            driver.Start();
            Thread.Sleep(100);

            Assert.IsTrue(vehicleMock.RotationSetTo.X > 1 - 0.001 &&
                          vehicleMock.RotationSetTo.X < 1 + 0.001);

            navigatorMock.DirectionToReturn = new Vector(0, 0); ;
            Thread.Sleep(100);

            Assert.IsTrue(vehicleMock.RotationSetTo.X > 0 - 0.001 &&
                          vehicleMock.RotationSetTo.X < 0 + 0.001);
        }
    }
}
