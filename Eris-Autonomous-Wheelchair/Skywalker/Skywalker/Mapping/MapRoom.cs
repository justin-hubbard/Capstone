using System;
using System.Collections.Generic;
using Skywalker.xmlMap;

namespace Skywalker.Mapping
{
    public class MapRoom
    {

        public string Name { get; set; }

        public mPoint RootNodeUV;

        public mPoint TopLeftCornerUV;

        //mesured in UV
        public double WidthU { get; set; }
        public double LengthV { get; set; }

        //mesured in cm
        public double WidthX { get; set; }
        public double LengthY { get; set; }

        public int GridPointsInU;
        public int GridPointsInV;

        public List<List<mGridPoint>> ListOfGridPointRows = new List<List<mGridPoint>>();
        public List<MapEdge> ListOfEdges = new List<MapEdge>();
        public List<MapObject> ListOfObjects = new List<MapObject>();
        public List<MapArea> ListOfAreas = new List<MapArea>();

        public Room XMLRoom;

        //****************** Constructors ******************

        public MapRoom()
        {

        }

        public MapRoom(Room nRoom)
        {
            Name = nRoom.Name;

            RootNodeUV = new mPoint(nRoom.Root_IPS_U, nRoom.Root_IPS_V);

            TopLeftCornerUV = new mPoint(nRoom.TopLeftU, nRoom.TopLeftV);

            WidthU = nRoom.Width_U;
            LengthV = nRoom.Length_V;

            WidthX = nRoom.Width_CM;
            LengthY = nRoom.Length_CM;

            //the -1 excludes the nodes
            GridPointsInU = nRoom.Number_Width_GridPoints - 1;
            GridPointsInV = nRoom.Number_Length_GridPoints - 1;

            //populate the rows
            for (int i = 0; i < GridPointsInV; i++)
            {
                ListOfGridPointRows.Add(new List<mGridPoint>());
            }

            List<GridPoint> xmlGridPoints = new List<GridPoint>();

            //ignore the first 4 nodes wich are the root nodes
            //all point are given in y
            for (int i = 4; i < nRoom.GridPoints.Count; i++)
            {
                xmlGridPoints.Add(nRoom.GridPoints[i]);
            }

            //x
            for (int i = 0; i < GridPointsInU; i++)
            {
            //x
                for (int j = 0; j < GridPointsInV; j++)
                {
                    ListOfGridPointRows[j].Add(new mGridPoint(xmlGridPoints[(i * GridPointsInV) + j]));
                }
            }

            Console.Out.WriteLine(ToString());




            foreach (Room_Object nObject in nRoom.Objects)
            {
                ListOfObjects.Add(new MapObject(nObject));
            }

            foreach (Region nRegion in nRoom.Regions)
            {
                ListOfAreas.Add(new MapArea(nRegion));
            }

            XMLRoom = nRoom;
        }

        //************************************* Test Methods *************************************

        //returns true if the point is within the area
        public bool ContainsPointInUV(mPoint pointUV)
        {

            //checks to make sure it is withing the area assumes it is a rectangle. 
            if (pointUV.X >= TopLeftCornerUV.X && pointUV.X <= TopLeftCornerUV.X + WidthU &&
               pointUV.Y <= TopLeftCornerUV.Y + LengthV && pointUV.Y >= TopLeftCornerUV.Y)
            {
                return true;
            }

            return false;
        }
        
        //************************************* Get and Set Methods *************************************

        //returns the width in U
        public double GetWidth()
        {
            return WidthU;
        }

        //returns the legnth in V
        public double GetLegnth()
        {
            return LengthV;
        }

        //retuns a copy of the list of MapObjects contained in the room
        public List<MapObject> GetListOfObjects()
        {
            return new List<MapObject>(ListOfObjects);
        }

        //returns a copy of the list of MapAreas contained in the room
        public List<MapArea> GetListOfAreas()
        {
            return new List<MapArea>(ListOfAreas);
        }

        //gets a list of area names
        public List<string> GetListOfAreaNames()
        {
            List<string> strList = new List<string>();

            foreach (MapArea cArea in ListOfAreas)
            {
                strList.Add(cArea.GetName());
            }

            return strList;
        }

        //returns the location of the gridpoint in the list of lists (0, 0) is top left
        //TODO: You can make this constant time by calculating the location
        private Tuple<int, int> GetGridPointLocation(mPoint mPointUV)
        {
            int y = 0;
            int x = 0;

            foreach (List<mGridPoint> rowList in ListOfGridPointRows)
            {
                foreach (mGridPoint gPoint in rowList)
                {
                    if (gPoint == mPointUV)
                    {
                        return new Tuple<int, int>(x, y);
                    }
                    x++;
                }
                x = 0;
                y++;
            }

            return null;
        }

        private mGridPoint GetGridPointAtLocation(int x, int y)
        {
            //make sure the points are in the grid
            if (x >= 0 && x < GridPointsInU &&
               y >= 0 && y < GridPointsInV)
            {
List<mGridPoint> rowList = ListOfGridPointRows[y];
                return rowList[x];
            }

            return null;
        }

        //returns empty list if there are no adjacent points
        public List<mGridPoint> GetAdjacentGridPoints(mPoint mPointUV)
        {
            List<mGridPoint> AdjacentGPoints = new List<mGridPoint>();

            Tuple<int, int> location = GetGridPointLocation(mPointUV);

            if (location != null)
            {
                mGridPoint nGPoint;

                //get top
                nGPoint = GetGridPointAtLocation(location.Item1 + 0, location.Item2 + 1);
                if (nGPoint != null)
                {
                    AdjacentGPoints.Add(nGPoint);
                }

                //get top rigth
                nGPoint = GetGridPointAtLocation(location.Item1 + 1, location.Item2 + 1);
                if (nGPoint != null)
                {
                    AdjacentGPoints.Add(nGPoint);
                }

                //get right
                nGPoint = GetGridPointAtLocation(location.Item1 + 1, location.Item2 + 0);
                if (nGPoint != null)
                {
                    AdjacentGPoints.Add(nGPoint);
                }

                //get bottom right
                nGPoint = GetGridPointAtLocation(location.Item1 + 1, location.Item2 - 1);
                if (nGPoint != null)
                {
                    AdjacentGPoints.Add(nGPoint);
                }

                //get bottom 
                nGPoint = GetGridPointAtLocation(location.Item1 + 0, location.Item2 - 1);
                if (nGPoint != null)
                {
                    AdjacentGPoints.Add(nGPoint);
                }

                //get bottom left
                nGPoint = GetGridPointAtLocation(location.Item1 - 1, location.Item2 - 1);
                if (nGPoint != null)
                {
                    AdjacentGPoints.Add(nGPoint);
                }

                //get left
                nGPoint = GetGridPointAtLocation(location.Item1 - 1, location.Item2 + 0);
                if (nGPoint != null)
                {
                    AdjacentGPoints.Add(nGPoint);
                }

                //get top left
                nGPoint = GetGridPointAtLocation(location.Item1 - 1, location.Item2 + 1);
                if (nGPoint != null)
                {
                    AdjacentGPoints.Add(nGPoint);
                }

            }

            return AdjacentGPoints;
        }

        //goes through the grid points and finds the point that is the closest returns null if failed
        //TODO: Make sure the point is accessable. Can be made faster by again claculating the location
        public mGridPoint GetClosestGridPoint(mPoint pointUV)
        {
            mGridPoint closestPoint = null;
            double currentShortestDistance = 0;

            foreach (List<mGridPoint> cRow in ListOfGridPointRows)
            {
                foreach (mGridPoint cGridPoint in cRow)
                {
                    //TODO: add a check for accesability
                    if (closestPoint == null)
                    {
                        closestPoint = cGridPoint;
                        currentShortestDistance = pointUV.GetDistanceToPoint(cGridPoint);
                    }
                    else
                    {
                        double tmpDistance = pointUV.GetDistanceToPoint(cGridPoint);

                        //TODO: add a check for accesability
                        if (tmpDistance < currentShortestDistance)
                        {
                            closestPoint = cGridPoint;
                            currentShortestDistance = tmpDistance;
                        }
                    }
                }

            }

            return closestPoint;
        }

        //gets the weighted distance between two grid points. accounts for the weight of the area it
        //traverses and if it is a prefeard path or not. Checks if the path intesects objects as well
        //will return -1 if that is the case. 
        //NOTE: Will return -1 if the point is blocked. 
        public double GetWeightedDistance(mGridPoint startPoint, mGridPoint endPoint, int bufferDistance = 0)
        {
            double calculatedDistance = -1;
            double heaviestWeight = -1;

            foreach (MapObject cObj in ListOfObjects)
            {
                //if we cant cross the object and the line crosses it return -1
                if (!cObj.IsPassible() && cObj.DoesLineCross(startPoint, endPoint, bufferDistance))
                {
                    return -1;
                }
            }

            //get the weight
            foreach (MapArea cArea in ListOfAreas)
            {
                if (cArea.DoesLineCross(startPoint, endPoint))
                {

                    if (heaviestWeight < cArea.GetAreaWeight())
                    {
                        heaviestWeight = cArea.GetAreaWeight();
                    }

                }
            }

            //if they are both prefered paths it will apply the change in weight
            calculatedDistance = startPoint.GetWeightedDistance(endPoint);

            //if we have not crossed any areas.
            if (heaviestWeight != -1)
            {
                calculatedDistance *= heaviestWeight;
            }

            return calculatedDistance;
        }

        //returns a copy of the top left corner and the width and length.
        public Tuple<mPoint, Tuple<double, double>> GetRoomLocationAndDimentions()
        {
            return new Tuple<mPoint, Tuple<double, double>>(new mPoint(TopLeftCornerUV), new Tuple<double, double>(WidthU, LengthV));
        }

        //gets Room topLeftPoint and dimentions of an area from a point in UV. returns null if 
        //failed to find.
        public Tuple<mPoint, Tuple<double, double>> GetAreaCornerAndDimentions(mPoint pointUV)
        {
            foreach (MapArea cArea in ListOfAreas)
            {
                if (cArea.ContainsPoint(pointUV))
                {
                    return cArea.GetCornerAndDimentions();
                }
            }

            return null;

        }

        public void AddEdge(MapEdge nEdge)
        {
            ListOfEdges.Add(nEdge);
        }

        //public void AddObject(MapObject nObject)
        //{
        //    foreach (List<mGridPoint> gridRows in ListOfGridPointRows)
        //    {
        //        foreach (mGridPoint gpoint in gridRows)
        //        {
        //            if (nObject.DoesContain(gpoint))
        //            {
        //                gpoint.Passible = false;
        //            }
        //        }
        //    }

        //    ListOfObjects.Add(nObject);
        //}

        //************************************* Overriden Methods *************************************

        public override string ToString()
        {
            string str = "";
            str += "++++++++++ Room: " + Name + " ++++++++++\n";
            str += "Root IPS (U,V): " + RootNodeUV.ToString() + "\n";
            str += "Location (U,V): " + TopLeftCornerUV.ToString() + "\n";
            str += "Dimentions(U,V): " + WidthU + ", " + LengthV + "\n";
            str += "GridPoint Number: " + ListOfGridPointRows.Count + "\n";

            str += "Edges: \n";
            foreach (MapEdge cEdge in ListOfEdges)
            {
                str += cEdge.ToString() + "\n";
            }

            str += "Objects: \n";
            foreach (MapObject cObject in ListOfObjects)
            {
                str += cObject.ToString() + "\n";
            }

            str += "Areas: \n";
            foreach (MapArea cArea in ListOfAreas)
            {
                str += cArea.ToString() + "\n";
            }

            str += "Grid Map: \n \t";

            foreach (mGridPoint gpoint in ListOfGridPointRows[0])
            {
                str += Math.Round(gpoint.X, 1) + "\t";
            }

            str += "\n";

            foreach (List<mGridPoint> gridRows in ListOfGridPointRows)
            {
                str += Math.Round(gridRows[0].Y, 1) + "\t";

                foreach (mGridPoint gpoint in gridRows)
                {

                    if (!gpoint.IsPassible())
                    {
                        str += "  X  ";
                    }
                    else if (gpoint.IsPreferredPath())
                    {
                        str += "  P  ";
                    }
                    else
                    {
                        str += "  .  ";
                    }

                    str += "\t";
                }
                str += "\n";

            }

            foreach (List<mGridPoint> gridRows in ListOfGridPointRows)
            {
                str += Math.Round(gridRows[0].Y, 1) + "\t";

                foreach (mGridPoint gpoint in gridRows)
                {

                    str += gpoint.Name;
                }
                str += "\n";

            }

            return str;
        }

    }
}

