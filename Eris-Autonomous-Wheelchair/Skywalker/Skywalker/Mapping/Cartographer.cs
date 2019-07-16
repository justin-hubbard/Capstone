using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Windows;
using Skywalker.xmlMap;

namespace Skywalker.Mapping
{

    public class Cartographer : ICartographer
    {
        private Map BuildingMap;
        private mPoint StartingPositionInXY;
        private MapRoom StartingRoom;

        //****************** Constructors ******************

        /// <summary>
        /// Public constructor 
        /// </summary>
        public Cartographer()
        {
            BuildingMap = new Map();
        }
        
        /// <summary>
        /// Public constructor
        /// </summary>
        public Cartographer(string dirname, mPoint startingPositionInXY)
        {
            //changed       
            Load(dirname);

            //wait for an iteration of the IPS system that way
            //we can get the starting location
            StartingPositionInXY = startingPositionInXY;

        }

        //****************** Methods for Interface ******************

        // called by Navigator: Route Planning ******************

        //takes a point in uv and returns a vector to the next location on the path. 
        public Vector VectorTo(mPoint currentPositionUV, mPoint destinationUV)
        {
            return BuildingMap.GetDirectionTo(currentPositionUV, destinationUV);
        }

        //takes a point in uv and uses this to get a series of points in UV to the desired Location
        public List<mPoint> PlanRoute(mPoint currentPositionUV, mPoint endLocationUV)
        {
            return BuildingMap.SearchRoute(currentPositionUV, endLocationUV);
        }

        //takes two points one for the currentLocation and one for the desired endLocation. It calls the room
        //search algorithm to find the best path to a point in a room. 
        public List<mPoint> PlanPath(mPoint currentLocationInUV, mPoint endLocation, IObstacleMap obstacleMap)
        {
            return BuildingMap.SearchPath(currentLocationInUV, endLocation, obstacleMap);
        }

        public double GetDegreeOffset()
        {
            return 0.0;
        }

        // called by Navigator: Location Finding ******************

        //returns the starting room from the Map
        public MapRoom GetStartingRoom()
        {
            return StartingRoom;
        }

        //takes the current room and point in XY and the current room then returns the point in UV
        public mPoint GetLocationInUV(mPoint pointXY, MapRoom currentRoom)
        {
            return pointXY.GetUVFromXY(currentRoom);
        }

        //returns all of the points of interest on the map. 
        public List<string> GetListOfPointOfInterestNames()
        {
            return BuildingMap.GetPointsOfInterestNames();
        }

        //returns the a list of all the names of the rooms from map
        public List<string> GetListOfRoomNames()
        {
            return BuildingMap.GetRoomNames();
        }

        //returns a list of all the names for all areas on the map. 
        public List<string> GetListOfAreaNames()
        {
            return BuildingMap.GetAreaNames();
        }

        //gets Room topLeftPoint and dimentions
        public Tuple<mPoint, Tuple<double, double>> GetRoomCornerAndDimentions(mPoint PointInRoomInUV)
        {
            MapRoom tmpRoom = BuildingMap.GetRoomFromPointInUV(PointInRoomInUV);

            if (tmpRoom != null)
            {
                return tmpRoom.GetRoomLocationAndDimentions();
            }

            return null;
        }

        //gets the room containing the point and then gets the area from that room. It will return null
        //if the point is not contained in an area. 
        public Tuple<mPoint, Tuple<double, double>> GetAreaCornerAndDimentions(mPoint PointInRoomUV)
        {
            MapRoom room = BuildingMap.GetRoomFromPointInUV(PointInRoomUV);
            return room.GetAreaCornerAndDimentions(PointInRoomUV);
        }

        //saves the current map
        public bool Save(string dirname)
        {
            return false;
        }

        public bool Load(string dirname)
        {

            BuildingMap = new Map(XMLFileToFloorPlan(dirname));
            StartingRoom = BuildingMap.GetStartingRoom();

            if (BuildingMap != null)
            {
                return true;
            }

            return false;
        }

        //****************** Methods for Internal Use ******************
        private static FloorPlan XMLFileToFloorPlan(string filePath)
        {
            XmlSerializer mydeserializer = new XmlSerializer(typeof(FloorPlan));
            //TextReader reader = new StreamReader(@"FloorPlanTest.xml");
            TextReader reader = new StreamReader(filePath);
            Object obj = mydeserializer.Deserialize(reader);
            FloorPlan xmlData = (FloorPlan)obj;
            reader.Close();
            return xmlData;
        }

        //************************************* Overriden Methods *************************************
        public override string ToString()
        {
            return BuildingMap.ToString();
        }

        //cotverts the map to the new system.
        //		private Map ConvertToMap(FloorPlan floorPlan)
        //		{

        //			Map newMap = new Map (floorPlan.Name, floorPlan.ImageName);

        //			//convert the Rooms into the new Rooms and add them to the listOfRooms
        //			foreach (Room currentRoom in floorPlan.Rooms) 
        //			{
        //				MapRoom tmpRoom = new MapRoom ();

        //				//should be changed later
        //				tmpRoom.Name = currentRoom.Name;
        //				tmpRoom.RootNodeInUV = new mPoint (currentRoom.Root_IPS_U, currentRoom.Root_IPS_V);
        //				tmpRoom.TopLeftCorner = new MapPoint (new mPoint(currentRoom.TopLeftU, currentRoom.TopLeftV), null);
        //				tmpRoom.TopRightCorner = new MapPoint (new mPoint(currentRoom.TopRightU, currentRoom.TopRightV), null);
        //				tmpRoom.BottomLeftCorner = new MapPoint (new mPoint(currentRoom.BottomLeftU, currentRoom.BottomLeftV), null);
        //				tmpRoom.BottomRightCorner = new MapPoint (new mPoint(currentRoom.BottomRightU, currentRoom.BottomRightV), null);

        //				//dimentions 
        //				tmpRoom.WidthU = currentRoom.Width_U;
        //				tmpRoom.LengthV = currentRoom.Length_V;
        //				tmpRoom.WidthX = currentRoom.Width_CM;
        //				tmpRoom.LengthY = currentRoom.Length_CM;
        ////change later
        //				//tmpRoom.ListOfGridPoints = currentRoom.GridPoints; 

        //				newMap.addRoom (tmpRoom);

        //                StartingRoom = tmpRoom;

        //			}

        //			//while we still have connections to go through
        //			while (floorPlan.Connections.Count != 0) 
        //			{
        //				Connection leftConnection = floorPlan.Connections[0];
        //				floorPlan.Connections.RemoveAt(0);

        //				int index = 0; 

        //				while (floorPlan.Connections [index] != null) 
        //				{
        //					Connection rightConnection = floorPlan.Connections [index];

        //					//check if they are the right rooms
        //					if (leftConnection.MyRoom_Name == rightConnection.OtherRoom_Name &&
        //						leftConnection.OtherRoom_Name == rightConnection.MyRoom_Name) 
        //					{
        //						//check if they are the correct points
        //						if( (leftConnection.MyRoom_U == rightConnection.OtherRoom_U) && (leftConnection.MyRoom_V == rightConnection.OtherRoom_V) &&
        //							(rightConnection.MyRoom_U == leftConnection.OtherRoom_U) && (rightConnection.MyRoom_V == leftConnection.OtherRoom_V))
        //						{
        //							MapPoint pointInA = new MapPoint(leftConnection.MyRoom_U, leftConnection.MyRoom_V,
        //								leftConnection.MyRoom_IPS_X, leftConnection.MyRoom_IPS_Y);

        //							MapPoint pointInB = new MapPoint(rightConnection.MyRoom_U, rightConnection.MyRoom_V,
        //								rightConnection.MyRoom_IPS_X, rightConnection.MyRoom_IPS_Y);

        //							newMap.addEdge (pointInA, pointInB);

        //							floorPlan.Connections.RemoveAt(index);
        //							break;
        //						}
        //					}

        //					index++;
        //				}
        //			}

        //			return newMap;

        //		}

    }
}
