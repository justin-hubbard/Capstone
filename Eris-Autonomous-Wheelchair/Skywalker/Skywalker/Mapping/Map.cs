using System;
using System.Collections.Generic;
using Skywalker.xmlMap;

namespace Skywalker.Mapping
{

    //NOTE: Important there is more to Map Look in MapSearch.
    public partial class Map
    {

        private Map MapInstance;

        public Map Instance
        {
            get
            {
                if (MapInstance == null)
                {
                    MapInstance = new Map();
                }
                return MapInstance;
            }
        }

        public string Name
        {
            get;
            set;
        }

        public string ImageName
        {
            get;
            set;
        }
        //TODO: CHANGE TO PRIVATE
        public List<MapRoom> RoomsInMap = new List<MapRoom>();
        //private List<MapRoom> RoomsInMap = new List<MapRoom> ();
        private List<MapEdge> EdgesInMap = new List<MapEdge>();
        private List<MapPOI> POIInMap = new List<MapPOI>();

        private FloorPlan MapFloorPlan;

        //****************** Constructors ******************

        public Map()
        {
            MapInstance = new Map("Map", "ImageName");
        }

        public Map(string name, string imageName)
        {
            Name = name;
            ImageName = imageName;

        }

        public Map(FloorPlan fPlan)
        {
            Name = fPlan.Name;
            ImageName = fPlan.ImageName;

            MapFloorPlan = fPlan;

            foreach (Room nRoom in fPlan.Rooms)
            {
                RoomsInMap.Add(new MapRoom(nRoom));
            }

            foreach (Connection nConnection in fPlan.Connections)
            {
                AddEdge(nConnection);
            }


        }

        //****************** Methods for External Use ******************

        //returns the first room in the list or rooms as the starting room.
        public MapRoom GetStartingRoom()
        {
            return RoomsInMap[0];
        }

        //retunrs a MapRoom structure from a given point in UV if there is no associated room we return null
        public MapRoom GetRoomFromPointInUV(mPoint pointInUV)
        {
            foreach (MapRoom currentRoom in RoomsInMap)
            {
                if (currentRoom.ContainsPointInUV(pointInUV))
                {
                    return currentRoom;
                }
            }

            return null;
        }

        //returns a list of names from all the RoomsInMap.
        public List<string> GetRoomNames()
        {
            List<string> listOfRoomNames = new List<string>();

            foreach (MapRoom cRoom in RoomsInMap)
            {
                listOfRoomNames.Add(cRoom.Name);
            }

            return listOfRoomNames;
        }

        //returns a list of all points of interest.
        public List<string> GetPointsOfInterestNames()
        {
            List<string> listOfPointsOfInterest = new List<string>();

            foreach (MapPOI poi in POIInMap)
            {
                listOfPointsOfInterest.Add(poi.Name);
            }

            return listOfPointsOfInterest;
        }

        //returns a list of the names of all areas by going to each room and getting all of the 
        //area names from there.
        public List<string> GetAreaNames()
        {
            List<string> listOfAreaNames = new List<string>();

            foreach (MapRoom cRoom in RoomsInMap)
            {
                listOfAreaNames.AddRange(cRoom.GetListOfAreaNames());
            }

            return listOfAreaNames;
        }

        //****************** Methods for Loading ******************

        //adds edges to the map
        private void AddEdge(Connection nConnection)
        {
            mPoint uvPointA = new mPoint(nConnection.Room_U, nConnection.Room_V);
            mPoint uvPointB = new mPoint(nConnection.Other_Room_U, nConnection.Other_Room_V);

            //get the two rooms associated with the given points in uv
            MapRoom RoomA = GetRoomFromPointInUV(uvPointA);
            MapRoom RoomB = GetRoomFromPointInUV(uvPointB);

            //create a new edge add it to the Associated Rooms then add it to the map.
            MapEdge tmpEdge = new MapEdge(RoomA, uvPointA, RoomB, uvPointB, nConnection);

            RoomA.AddEdge(tmpEdge);
            RoomB.AddEdge(tmpEdge);

            EdgesInMap.Add(tmpEdge);
        }

        //****************** Overriden Methods ******************

        public override string ToString()
        {
            string str = "";

            str += "++++++++++ Map: " + Name + "++++++++++\n";
            str += "Imagename: " + ImageName + "\n";

            foreach (MapRoom cRoom in RoomsInMap)
            {
                str += cRoom.ToString() + "\n";
            }


            return str;
        }
    }
}

