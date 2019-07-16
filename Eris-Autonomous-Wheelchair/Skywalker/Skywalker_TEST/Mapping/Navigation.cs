using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skywalker.Mapping;

namespace Skywalker_TEST.Mapping {
    [TestClass]
    public class Navigation {
        private string cwd = Directory.GetCurrentDirectory();

        /*[TestMethod]
        public void ASTarShouldNotCrash() {
            Cartographer map = new Cartographer(cwd + @"\mapping");

            try
            {
                map.NavigateTo(9, 7);
            }
            catch (Exception e)
            {
                Assert.Fail("A* should not crash: {0}", e.ToString());
            }
        }

        [TestMethod]
        public void LastPointShouldBeFinal()
        {
            Cartographer map = new Cartographer(cwd + @"\mapping");
            int x = 9, y = 7;
            List<Direction> directions = null;
            try
            {
                directions = map.NavigateTo(x, y);
            }
            catch (Exception e)
            {
                Assert.Fail("A* should not crash");
            }

            Assert.IsNotNull(directions, "We should still get something back");    
            if (directions != null)
            {
                Assert.AreEqual(x, directions[directions.Count - 1].DestinationVector.X);
                Assert.AreEqual(y, directions[directions.Count - 1].DestinationVector.Y);
            }
        }

        [TestMethod]
        public void NavigationShouldAcceptString() {
            Cartographer map = new Cartographer(cwd + @"\mapping");
            int x = 15, y = 10;
            List<Direction> directions = null;
            try {
                directions = map.NavigateTo("Matt's Office");
            } catch (Exception e) {
                Assert.Fail("A* should not crash");
            }
        }*/
    }
}
