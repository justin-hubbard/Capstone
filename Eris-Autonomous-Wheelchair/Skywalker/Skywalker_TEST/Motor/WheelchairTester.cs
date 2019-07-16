using System;
using System.Windows;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skywalker.Motor;

namespace Skywalker_TEST.Motor
{
    /// <summary>
    /// Summary description for WheelchairTester
    /// </summary>
    [TestClass]
    public class WheelchairTester
    {
        public WheelchairTester()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
            
        //}
        
        #endregion

        [TestMethod]
        [ExpectedException(typeof(UnauthorizedAccessException))]
        public void Start_PortInUse_ShouldThrowUnauthorizedEx()
        {
            // Setup
            MockSerialDevice serialDevice = new MockSerialDevice("COM5", 9600);
            Wheelchair chair = Wheelchair.Instance(serialDevice);

            chair.Start();
            chair.End();
            chair.Dispose();
            Assert.Fail("Unauthorized Access Exception was not thrown.");
        }

        [TestMethod]
        [ExpectedException(typeof(IOException))]
        public void Start_InvalidComPort_ShouldThrowIOException()
        {
            MockSerialDevice serialDevice = new MockSerialDevice("COM7", 9600);
            Wheelchair chair = Wheelchair.Instance(serialDevice);

            chair.Start();
            chair.End();
            chair.Dispose();
            Assert.Fail("Invalid COM Port. IO Exception was not thrown.");
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentException))]
        public void Start_InvalidPortName_ShouldThrowArgumentException()
        {
            MockSerialDevice serialDevice = new MockSerialDevice("Con3", 9600);
            Wheelchair chair = Wheelchair.Instance(serialDevice);

            chair.Start();
            chair.End();
            chair.Dispose();
            Assert.Fail("Invalid Port Name. Argument Exception was not thrown.");
        }

        [TestMethod]
        public void Start_InvalidBaudRate_ShouldThrowOutOfRange()
        {
            bool caught = false;
            try
            {
                MockSerialDevice serialDevice = new MockSerialDevice("COM3", 0);
                Wheelchair chair = Wheelchair.Instance(serialDevice);
            }
            catch (ArgumentOutOfRangeException e)
            {
                caught = true;
                Assert.AreEqual("BaudRate", e.ParamName);
            }
            if(!caught) {
                Assert.Fail("ArgumentOutOfRangeException was not thrown.");   
            }
        }

        [TestMethod]
        public void Start_Serial_With_No_Name()
        {
            try
            {
                MockSerialDevice sd = new MockSerialDevice();
            }
            catch (Exception e)
            {
                Assert.Fail("Should not throw exception");
            }
        }

        [TestMethod]
        public void SetXY_ValidVoltages()
        {
            // Setup
            MockSerialDevice serialDevice = new MockSerialDevice("COM3", 9600);
            Wheelchair chair = Wheelchair.Instance(serialDevice);
            bool correctValues = true;
            int[] inputValues =
            {
                0,
                -100,
                100
            };
            int[] expectedValues = {128, 66, 190};
            chair.Start();
            // act
            for (int i = 0; i < 3; i++)
            {
                chair.SetX(inputValues[i]);
                chair.SetY(inputValues[i]);
                Thread.Sleep(150);
                if (chair.VoltX != expectedValues[i])
                    correctValues = false;
                if (chair.VoltY != expectedValues[i])
                    correctValues = false;
            }
            //chair.End();
            chair.Dispose();
            Assert.IsTrue(correctValues, "Voltages are not correct.");
        }

        [TestMethod]
        public void SetXY_ForwardValue()
        {
            // Setup
            MockSerialDevice serialDevice = new MockSerialDevice("COM3", 9600);
            Wheelchair chair = Wheelchair.Instance(serialDevice);
            
            bool correctValues = true;
            int input = 100;
            int expectedOutput = 190;
            int actualOutput = -1;
            chair.Start();
            // act
            chair.SetX(0);
            chair.SetY(input);
            System.Threading.Thread.Sleep(150);
            actualOutput = chair.VoltY;
            chair.End();
            string message = "Expected: ";
            message += expectedOutput;
            message += ", Actual: ";
            message += actualOutput;
            chair.Dispose();
            Assert.AreEqual(expectedOutput, actualOutput, message);
        }

        [TestMethod]
        public void SetSpeedAndDirection_Inputs()
        {
            //int[] directions = {0, 45, 90, 180, 270};
            Vector[] directions = { new Vector(0, 0), new Vector(0, 1), new Vector(1, 0), new Vector(0, -1), new Vector(-1, 0) };
            int[] expected_x = {128, 172, 190, 128, 66};
            int[] expected_y = {190, 172, 128, 66, 128};
            string message = "";
            bool correctValues = true;
            MockSerialDevice serialDevice = new MockSerialDevice("COM3", 9600);
            Wheelchair chair = Wheelchair.Instance(serialDevice);
            chair.Start();
            for (int i = 0; i < 5; i++)
            {
                chair.SetSpeedAndDirection(100, directions[i]);
                Thread.Sleep(100);
                if (chair.VoltX != expected_x[i])
                {
                    correctValues = false;
                    message += "Direction[" + directions[i];
                    message += "] was incorrect (X Voltage). Expected:";
                    message += expected_x[i] + ", Actual:";
                    message += chair.VoltX;
                    break;
                }
                if (chair.VoltY != expected_y[i])
                {
                    correctValues = false;
                    message += "Direction[" + directions[i];
                    message += "] was incorrect (Y Voltage). Expected:";
                    message += expected_y[i] + ", Actual:";
                    message += chair.VoltY;
                    break;
                }
            }
            chair.Dispose();
            Assert.IsTrue(correctValues, message);
        }
    }
}
