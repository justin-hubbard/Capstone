using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skywalker.Mapping
{
    public partial class Map
    {
        private int BufferDistance = 45;

        //returns a list of mPoints in UV that we can use to navigate a room. 
        //if they are the same point it will just return that point
        //note: this does include the start point. 

        //if this fails it will return null
        public List<mPoint> SearchPath(mPoint startPointUV, mPoint endPointUV, IObstacleMap obstacleMap)
        {
            List<mPoint> returnList = new List<mPoint>();

            MapRoom room1 = GetRoomFromPointInUV(startPointUV);
            if (room1 == null)
            {
                return null;
            }

            MapRoom room2 = GetRoomFromPointInUV(endPointUV);
            if (room2 == null)
            {
                return null;
            }

            //The names of each room are unique this is enforced when the map is created
            if (room1.Name != room2.Name)
            {
                return null;
            }

            if (startPointUV == endPointUV)
            {
                returnList.Add(startPointUV);

                return returnList;
            }

            mGridPoint startingGridPoint = room1.GetClosestGridPoint(startPointUV);
            if (startingGridPoint == null)
            {
                return null;
            }

            mGridPoint endingGridPoint = room1.GetClosestGridPoint(endPointUV);
            if (startingGridPoint == null)
            {
                return null;
            }

            PathPQueue queue = new PathPQueue();

            //hashset to keep track of the visited points 
            HashSet<mPoint> tableOfVisitedPoints = new HashSet<mPoint>();

            Console.Out.WriteLine(room1.ToString());

            //set up the queue and cast the starting point in UV to a grid point so we can store it in the 
            //PathSearchNode.
            List<mGridPoint> startingListOfGPoints = new List<mGridPoint>();
            startingListOfGPoints.Add(new mGridPoint(startPointUV.X, startPointUV.Y, room1));
            startingListOfGPoints.Add(startingGridPoint);

            queue.Add(new PathSearchNode(startingGridPoint, startingListOfGPoints, 0));

            while (!queue.IsEmpty())
            {
                PathSearchNode currentNode = queue.Pop();

                tableOfVisitedPoints.Add(currentNode.CurrentGridPoint);

                if (currentNode.CurrentGridPoint == endingGridPoint)
                {
                    //add the last point to the list
                    returnList = currentNode.GetListOfMPoints();
                    returnList.Add(endPointUV);

                    printPathSearch(currentNode, queue, tableOfVisitedPoints, room1, startingGridPoint, endingGridPoint, currentNode.CurrentGridPoint, new List<mGridPoint>());

                    return returnList;
                }

                List<mGridPoint> adjacentGridPoints = room1.GetAdjacentGridPoints(currentNode.CurrentGridPoint);
                
                foreach (mGridPoint adjGPoint in adjacentGridPoints)
                {
                    //if we have not visited this point and it is passible
                    bool isPointObstructed = false; // obstacleMap.IsPointObstructed(adjGPoint, room1);
                    if (!tableOfVisitedPoints.Contains(adjGPoint) && adjGPoint.IsPassible() && !isPointObstructed)
                    {
                        double newDistance = room1.GetWeightedDistance(currentNode.CurrentGridPoint, adjGPoint, BufferDistance);

                        //get weighted distance returns -1 if it crosses an object.
                        if (newDistance > 0)
                        {
                            //add the point to the queue make a deep copy
                            List<mGridPoint> tmpGridPointList = new List<mGridPoint>(currentNode.ListOfGridPoints);
                            tmpGridPointList.Add(adjGPoint);

                            queue.Add(new PathSearchNode(adjGPoint, tmpGridPointList, currentNode.Distance + newDistance));
                        }
                    }
                }
            }

            return null;
        }

        //Testing Method
        //very helpful for debugging what.
        public void printPathSearch(PathSearchNode currentPath, PathPQueue queue, HashSet<mPoint> tableOfVisitedPoints, MapRoom cRoom, mGridPoint start, mGridPoint end, mGridPoint current,
                                    List<mGridPoint> adjacentGridPoints)
        {
            string Row = "";

            Console.Out.WriteLine("******************************");
            //Console.Out.WriteLine("CurrentPath: " + currentPath);
            //Console.Out.WriteLine("---------------------------------");
            //Console.Out.WriteLine("Queue: " + queue);

            //print top row
            foreach (mGridPoint gpoint in cRoom.ListOfGridPointRows[0])
            {
                Row += Math.Round(gpoint.X, 1) + "\t";
            }

            Console.Out.WriteLine(" \t" + Row);

            Row = "";

            //print each row ad mark if we have visited a place
            foreach (List<mGridPoint> gridRows in cRoom.ListOfGridPointRows)
            {
                Row = Math.Round(gridRows[0].Y, 1) + "\t";

                foreach (mGridPoint gpoint in gridRows)
                {
                    if (adjacentGridPoints.Contains(gpoint))
                    {
                        Row += "a";
                    }
                    if (currentPath.ListOfGridPoints.Contains(gpoint))
                    {
                        Row += "p";
                    }
                    if (!gpoint.IsPassible())
                    {
                        Row += "[";
                    }
                    if (gpoint == current)
                    {
                        Row += "  C  ";
                    }
                    else if (gpoint == start)
                    {
                        Row += "  S  ";
                    }
                    else if (gpoint == end)
                    {
                        Row += "  E  ";
                    }
                    else if (tableOfVisitedPoints.Contains(gpoint))
                    {
                        Row += "  X  ";
                    }
                    else
                    {
                        Row += "  .  ";
                    }

                    if (!gpoint.IsPassible())
                    {
                        Row += "]";
                    }

                    Row += "\t";
                }

                Console.Out.WriteLine(Row + "\t");
            }
        }
    }

}
