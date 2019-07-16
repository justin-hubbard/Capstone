using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skywalker.ObjectDetection;
using Skywalker.Driver;

namespace Skywalker_TEST.Driver
{
    /// <summary>
    /// Tests the Obstacle Map Class
    /// by constructing maps with a variety
    /// of obstacles, and ensuring that the 
    /// correct safe paths are found around them.
    /// </summary>
    [TestClass]
    public class ObstacleMapTester
    {
        /// <summary>
        /// Testing target
        /// </summary>
        ObstacleMap obstacleMap;

        /// <summary>
        /// Tests that the obstacle map will avoid an obstacle
        /// placed directly ahead of it by turning slightly.
        /// </summary>
        [TestMethod]
        public void AvoidDirectObstacleTest()
        {
            obstacleMap = new ObstacleMap();
            Obstacle obstacle = new Obstacle(new Vector(-500,1000), new Vector(500, 1000));

            obstacleMap.AddObstacle(obstacle);
            Vector result = obstacleMap.GetRouteSuggestion(new Vector(0, 1)).Item1;

            Assert.IsTrue(result.Y > 0.79f && result.Y < 0.81f);
            Assert.IsTrue(result.X > -0.59f && result.X < -0.57f);
            Assert.IsTrue(result.Length > 0.999 && result.Length < 1.001);
        }

        /// <summary>
        /// Tests that the obstacle map will avoid two obstacles
        /// with a space in between them, continuing directly forward
        /// through the gap.
        /// </summary>
        [TestMethod]
        public void BetweenObstaclesTest_WideSpace()
        {
            obstacleMap = new ObstacleMap();
            Obstacle obstacleLeft = new Obstacle(new Vector(-800, 500), new Vector(-400, 500));
            Obstacle obstacleRight = new Obstacle(new Vector(400, 1000), new Vector(800, 1000));

            obstacleMap.AddObstacle(obstacleLeft);
            obstacleMap.AddObstacle(obstacleRight);
            Vector result = obstacleMap.GetRouteSuggestion(new Vector(0, 1)).Item1;

            Assert.IsTrue(result.Y > 0.995f && result.Y < 1.005f);
        }

        /// <summary>
        /// Tests that the obstacle map will avoid two obstacles
        /// with a NARROW space in between them, acknowledging that
        /// 
        /// </summary>
        [TestMethod]
        public void BetweenObstaclesTest_NarrowSpace()
        {
            obstacleMap = new ObstacleMap();
            Obstacle obstacleLeft = new Obstacle(new Vector(-1000, 500), new Vector(-200, 500));
            Obstacle obstacleRight = new Obstacle(new Vector(200, 500), new Vector(1000, 500));

            obstacleMap.AddObstacle(obstacleLeft);
            obstacleMap.AddObstacle(obstacleRight);
            Vector result = obstacleMap.GetRouteSuggestion(new Vector(0, 1)).Item1;

            Assert.IsTrue(result.X < -0.998f && result.X > -1.005f);
        }

        /// <summary>
        /// Tests that the map will avoid an obstacle placed inside of its own boundary
        /// In practice, this should never happen, but we have
        /// to ensure that the map will not crash.
        /// </summary>
        [TestMethod]
        public void InsideRangeObstacleTest()
        {
            obstacleMap = new ObstacleMap();
            Obstacle obstacle = new Obstacle(new Vector(-100, 100), new Vector(100, 100));

            obstacleMap.AddObstacle(obstacle);
            Vector result = obstacleMap.GetRouteSuggestion(new Vector(0, 1)).Item1;

            Assert.IsTrue(result.X < -0.998f && result.X > -1.005f);
        }
    }
}
