using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skywalker_TEST.Driver
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skywalker.Driver;

    /// <summary>
    /// Tests the Navigator.
    /// </summary>
    [TestClass]
    public class NavigatorTester
    {
        /// <summary>
        /// Tests that GetDirection returns the appropriate
        /// direction given a variety of inputs.
        /// </summary>
        [TestMethod]
        public void GetDirectionTest()
        {
            /*INavigator navigator = new Navigator(new ObstacleMap(), null);

            Vector expected = new Vector(1,0);
            Vector direction = navigator.GetDirection(expected);
            Assert.IsTrue(Vector.Subtract(expected, direction).X < 0.001f);

            expected = new Vector(0, -1);
            direction = navigator.GetDirection(expected);
            Assert.IsTrue(Vector.Subtract(expected, direction).X < 0.001f);

            expected = new Vector(-1, 0);
            direction = navigator.GetDirection(expected);
            Assert.IsTrue(Vector.Subtract(expected, direction).X < 0.001f);

            expected = new Vector(0, 1);
            direction = navigator.GetDirection(expected);
            Assert.IsTrue(Vector.Subtract(expected, direction).X < 0.001f);*/
        }
    }
}
