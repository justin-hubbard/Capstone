using System;
using System.Collections.Generic;
using System.Windows;

namespace Skywalker.Mapping
{
    /// <summary>
    /// Interface for the Cartographer,
    /// which is responsible for maintaining
    /// the environment ment.
    /// </summary>
    public interface ICartographer
    {
        MapRoom GetStartingRoom();

        //takes your current location in XY and currentRoom and gets you your location in UV
        mPoint GetLocationInUV(mPoint pointXY, MapRoom currentRoom);

        Vector VectorTo(mPoint currentPositionUV, mPoint destinationUV);

        List<mPoint> PlanPath(mPoint currentLocationInUV, mPoint endLocation, IObstacleMap obstacleMap);
        List<mPoint> PlanRoute(mPoint currentLocationInUV, mPoint endLocation);

        //these will return null if they fail to find. 
        Tuple<mPoint, Tuple<double, double>> GetRoomCornerAndDimentions(mPoint PointInRoomInUV);
        Tuple<mPoint, Tuple<double, double>> GetAreaCornerAndDimentions(mPoint PointInRoomUV);

        //these will return empty lists if they fail. 
        List<string> GetListOfRoomNames();
        List<string> GetListOfPointOfInterestNames();
        List<string> GetListOfAreaNames();

        //save does not work will just return null
        bool Save(string dirname);
        bool Load(string dirname);

    }
}
