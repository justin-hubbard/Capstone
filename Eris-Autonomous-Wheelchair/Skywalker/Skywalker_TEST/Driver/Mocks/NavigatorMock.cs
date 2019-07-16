using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Skywalker.Driver;
using Skywalker.Mapping;

namespace Skywalker_TEST.Driver.Mocks
{
    /// <summary>
    /// Mock of the Navigator interface.
    /// </summary>
    public class NavigatorMock : INavigator
    {
        public Vector DirectionToReturn = new Vector(0,0);

        public Vector GetCurrentDirectionInUV()
        {
            throw new NotImplementedException();
        }

        public int GetCurrentDriveType()
        {
            throw new NotImplementedException();
        }

        public mPoint GetCurrentPositionInUV()
        {
            throw new NotImplementedException();
        }

        public mPoint GetCurrentPositionInXY()
        {
            throw new NotImplementedException();
        }

        public string GetCurrentRoom()
        {
            throw new NotImplementedException();
        }

        public double GetDegreesFromCompass()
        {
            throw new NotImplementedException();
        }

        public Vector GetDirection(Vector input)
        {
            return DirectionToReturn;
        }

        public List<mPoint> GetPath()
        {
            throw new NotImplementedException();
        }

        public List<mPoint> GetRoute()
        {
            throw new NotImplementedException();
        }

        public void NavigateTo(mPoint destinationPointInUV)
        {
            throw new NotImplementedException();
        }

        public void NavigateTo(string destination)
        {
            throw new NotImplementedException();
        }

        public void SetRoute(mPoint nextPointUV)
        {
            throw new NotImplementedException();
        }

        public void ToggleUsermode()
        {
            throw new NotImplementedException();
        }
    }
}
