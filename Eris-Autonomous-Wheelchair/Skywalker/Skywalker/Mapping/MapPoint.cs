using System;
using Skywalker.xmlMap;

namespace Skywalker.Mapping
{
    public class mPoint
    {

        public enum mPointType { Door, EndOfRoute, None };
        public mPointType PointType = mPointType.None;

        public double X;
        public double Y;

        private MapRoom RoomIn = null;

        //****************** Constructors ******************

        public mPoint()
        {
            X = 0;
            Y = 0;
        }

        public mPoint(double x, double y, MapRoom nRoom = null)
        {
            X = Math.Round(x, 4);
            Y = Math.Round(y, 4);

            RoomIn = nRoom;
        }

        public mPoint(Tuple<double, double> ipsPoint)
        {
            X = Math.Round(ipsPoint.Item1, 4);
            Y = Math.Round(ipsPoint.Item2, 4);
        }

        //copy constructor
        public mPoint(mPoint nPoint)
        {
            PointType = nPoint.PointType;
            X = nPoint.X;
            Y = nPoint.Y;
            RoomIn = nPoint.RoomIn;
        }

        //************************************* Get and Set Methods *************************************

        //gets the strait line distance between two mPoints
        public double GetDistanceToPoint(mPoint otherPoint)
        {
            return Math.Sqrt(Math.Pow(X - otherPoint.X, 2) + Math.Pow(Y - otherPoint.Y, 2));
        }

        public mPoint GetUVFromXY(MapRoom currentRoom)
        {
            double x = ConvertXtoU(X, currentRoom);
            double y = ConvertYtoV(Y, currentRoom);

            return new mPoint(x, y);
        }

        public MapRoom GetRoom()
        {
            return RoomIn;
        }

        public void SetRoom(MapRoom nRoom)
        {
            RoomIn = nRoom;
        }

        //************************************* Test Methods *************************************

        public bool IsDoor()
        {
            return (PointType == mPointType.Door);
        }

        public bool IsEndOfRoute()
        {
            return (PointType == mPointType.EndOfRoute);
        }

        //************************************* Overridden Methods *************************************
        public static bool operator ==(mPoint PointA, mPoint PointB)
        {

            if (object.ReferenceEquals(PointA, null))
            {
                return object.ReferenceEquals(PointB, null);
            }

            return PointA.Equals(PointB);
        }

        public static bool operator !=(mPoint PointA, mPoint PointB)
        {
            return !(PointA == PointB);
        }

        public override bool Equals(object obj)
        {
            //if we have been passed a null object
            if (obj == null)
            {
                return false;
            }

            //if we cant cast the object to an mPoint
            mPoint nPoint = obj as mPoint;

            if ((System.Object)nPoint == null)
            {
                return false;
            }

            return X == nPoint.X && Y == nPoint.Y;
        }

        public override int GetHashCode()
        {
            return ((int)X ^ (int)Y);
        }

        public override string ToString()
        {
            return "(" + X + ", " + Y + ")";
        }

        //************************************* Internal Methods *************************************

        private double ConvertXtoU(double x, MapRoom currentRoom)
        {
            double u;

            // handles conversion when root node is in topRight or bottomRight
            if (currentRoom.RootNodeUV.X == currentRoom.TopLeftCornerUV.X + currentRoom.WidthU)
            {
                u = currentRoom.RootNodeUV.X - (x * (currentRoom.WidthU / currentRoom.WidthX));
                return u;
            }

            // handles conversion when root node is in topLeft or bottomLeft
            u = x * (currentRoom.WidthU / currentRoom.WidthX) + currentRoom.RootNodeUV.X;

            return u;
        }

        private double ConvertYtoV(double y, MapRoom currentRoom)
        {
            double v;

            // handles conversion when root node is in bottomLeft or bottomRight
            if (currentRoom.RootNodeUV.Y == currentRoom.TopLeftCornerUV.Y + currentRoom.LengthV)
            {
                v = currentRoom.RootNodeUV.Y - (y * (currentRoom.LengthV / currentRoom.LengthY));
                return v;
            }

            // handles conversion when root node is in topLeft or topRight
            v = y * (currentRoom.LengthV / currentRoom.LengthY) + currentRoom.RootNodeUV.Y;

            return v;
        }

        private double ConvertUtoX(double u, MapRoom currentRoom)
        {
            double x;
            x = (u - currentRoom.RootNodeUV.X) * (currentRoom.WidthX / currentRoom.WidthU);
            return Math.Abs(x);
        }

        private double ConvertVtoY(double v, MapRoom currentRoom)
        {
            double y;
            y = (v - currentRoom.RootNodeUV.Y) * (currentRoom.LengthY / currentRoom.LengthV);
            return Math.Abs(y);
        }

    }

    public class mGridPoint : mPoint
    {
        //TODO : SHOULD BE PRIVATE
        public bool PreferredPath = false;
        public bool Passible = true;
        public string Name = "";


        GridPoint XMLGridPoint;

        //****************** Constructors ******************

        public mGridPoint(double x, double y, MapRoom room)
        {
            SetRoom(room);
            X = Math.Round(x, 4);
            Y = Math.Round(y, 4);
        }

        //copy constructor
        public mGridPoint(GridPoint gPoint)
        {
            X = gPoint.U;
            Y = gPoint.V;
            Name = gPoint.Name;
            PreferredPath = gPoint.Preferred_Path;
            Passible = !gPoint.Contains_Obstacle;
            XMLGridPoint = gPoint;
        }

        //************************************* Test Methods *************************************

        public bool IsPreferredPath()
        {
            return PreferredPath;
        }

        public bool IsPassible()
        {
            return Passible;
        }

        //this accounts for if both points are preferred path points.
        public double GetWeightedDistance(mGridPoint endGPoint)
        {
            //if both this point and the endGPoint are prefered paths weight the distance appropriatly
            if (PreferredPath && endGPoint.IsPreferredPath())
            {
                return GetDistanceToPoint(endGPoint) * .75;
            }

            return GetDistanceToPoint(endGPoint);
        }

        //************************************* Overridden Methods *************************************

        public override string ToString()
        {
            return "(" + Name +"Location: " + base.ToString() + " PreferredPath: " + PreferredPath + "Passible: " + Passible + " )";
        }

    }
}

