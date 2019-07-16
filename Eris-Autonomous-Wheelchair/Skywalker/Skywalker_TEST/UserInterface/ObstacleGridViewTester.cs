using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skywalker.UserInterface;

namespace Skywalker_TEST.UserInterface
{
    [TestClass]
    public class ObstacleGridViewTester
    {
        [TestMethod]
        public void GridAppearanceTest()
        {
            GridMock testData = new GridMock();
            ObstacleGridView view = new ObstacleGridView();
            ObstacleGridPresenter presenter = new ObstacleGridPresenter(testData, view);

            Window testWindow = new Window();
            testWindow.Content = view;
            Exception test;
            try
            {
                testWindow.ShowDialog();
            }
            catch (Exception e)
            {
                test = e;
            }
        }
    }
}
