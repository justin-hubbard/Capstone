using System;
using Skywalker.xmlMap;

namespace Skywalker.Mapping
{

    public class MapEdge
    {
        public MapRoom RoomA { get; set; }
        public MapRoom RoomB { get; set; }

        public mPoint PointInA { get; set; }
        public mPoint PointInB { get; set; }

        public double DistanceUV;

        private Connection EdgeConnection;

        //****************** Constructors ******************

        public MapEdge(MapRoom firstRoom, mPoint pointInFirstRoom, MapRoom secondRoom,
                        mPoint pointInsecondRoom, Connection nConnection)
        {
            RoomA = firstRoom;
            RoomB = secondRoom;

            PointInA = pointInFirstRoom;
            PointInA.PointType = mPoint.mPointType.Door;
            PointInB = pointInsecondRoom;
            PointInB.PointType = mPoint.mPointType.Door;

            DistanceUV = nConnection.UVDistance;

            EdgeConnection = nConnection;

        }

        //************************************* Get Methods *************************************

        //gets the distance between the two points in each room.
        public double GetDistanceUV()
        {
            return DistanceUV;
        }

        public MapRoom GetOpositRoom(MapRoom currentRoom)
        {
            if (RoomA == currentRoom)
            {
                return RoomB;
            }
            else
            {
                return RoomA;
            }
        }

        public mPoint GetPointUVInOpositRoom(MapRoom currentRoom)
        {
            if (RoomA == currentRoom)
            {
                return PointInB;
            }
            else
            {
                return PointInA;
            }
        }

        public mPoint GetPointUVInCurrentRoom(MapRoom currentRoom)
        {
            if (RoomA == currentRoom)
            {
                return PointInA;
            }
            else
            {
                return PointInB;
            }
        }

        //************************************* Overridden Methods *************************************

        override public string ToString()
        {
            return "Edge: (" + RoomA.Name + ":" + PointInA.ToString() + "," + RoomB.Name + ":" + PointInB.ToString() + ")";
        }

    }
}

