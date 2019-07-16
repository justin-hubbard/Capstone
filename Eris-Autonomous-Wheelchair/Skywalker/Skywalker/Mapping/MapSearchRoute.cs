using System;
using System.Windows;
using System.Collections.Generic;

namespace Skywalker.Mapping
{
    public partial class Map
    {
        //this will return a list of PointsInUV. from the given point to the end point.
        //note: does include the startPoint

        //if the start and end point is in the same room it will return a list containing the two points

        //fail cases: return null
        //	we cant get the the given point from the start point
        //  the start or end points are not in a room
        public List<mPoint> SearchRoute(mPoint startPointInUV, mPoint endPointInUV)
        {

            //get the start and end rooms and make sure they exist
            MapRoom startRoom = GetRoomFromPointInUV(startPointInUV);
            if (startRoom == null)
            {
                return null;
            }

            MapRoom endRoom = GetRoomFromPointInUV(endPointInUV);
            if (endRoom == null)
            {
                return null;
            }

            if ((startPointInUV.X == endPointInUV.X) && (startPointInUV.Y == endPointInUV.Y))
            {
                List<mPoint> listOfPoints = new List<mPoint>();
                listOfPoints.Add(endPointInUV);

                return listOfPoints;
            }

            //if we are searching in room
            if (startRoom == endRoom)
            {
                //get the list of points and return them
                List<mPoint> listOfPoints = new List<mPoint>();
                listOfPoints.Add(startPointInUV);
                listOfPoints.Add(endPointInUV);

                return listOfPoints;
            }

            PriorityQueue queue = new PriorityQueue();

            //hashset to keep track of the visited points 
            HashSet<mPoint> TableOfVisitedPoints = new HashSet<mPoint>();

            //add the starting point to the que
            List<mPoint> startingListOfPoints = new List<mPoint>();
            startingListOfPoints.Add(startPointInUV);
            queue.Add(new SearchNode(startRoom, startPointInUV, null, startingListOfPoints, 0));

            //queue.printPriorityQueue ();
            int index = 0;

            while (!queue.isEmpty())
            {

                index++;

                //get the node with the shortest path
                SearchNode currentNode = queue.Pop();

                //mark the point we have visited off.
                TableOfVisitedPoints.Add(currentNode.locationPoint);

                //if we have found the shortest path to the endpoint
                if (currentNode.locationPoint == endPointInUV)
                {
                    return currentNode.ListOfPointsInUV;
                }

                //if we have found a path to the last room. here we want to make sure we get 
                //the distance from the currentpoint to the end point
                if (currentNode.Room == endRoom)
                {

                    //claculate the new distance from the total path distance + the distance between the last point and the endpoint
                    double newDistance = currentNode.Distance + currentNode.locationPoint.GetDistanceToPoint(endPointInUV);

                    //the searchNode has the same room as the currentNode, the newpoint will be the endpoint, there is no edge
                    //the list of points will be updated with the endPoint and the distance will be updated
                    SearchNode newNode = new SearchNode(currentNode.Room,
                                                          endPointInUV,
                                                          null,
                                                         currentNode.ListOfPointsInUV,
                                                          newDistance);

                    newNode.addPoint(endPointInUV);

                    //does not check table because if we have hit the endpoint already we are done
                    queue.Add(newNode);

                }

                //if there is an edge to traverse 
                if (currentNode.Edge != null)
                {
                    mPoint nextPoint = currentNode.Edge.GetPointUVInOpositRoom(currentNode.Room);
                    MapRoom newRoom = currentNode.Edge.GetOpositRoom(currentNode.Room);
                    double newDistance = currentNode.Distance + currentNode.Edge.GetDistanceUV();

                    //moves the room the the next room, moves the point to the next room's point 
                    //keeps the edge the same and adds the distance of the edge to the node distance
                    SearchNode newNode = new SearchNode(newRoom,
                                                         nextPoint,
                                                            currentNode.Edge,
                                                         currentNode.ListOfPointsInUV,
                                                         newDistance);

                    newNode.addPoint(nextPoint);

                    //make sure we have not been to that point before
                    if (!TableOfVisitedPoints.Contains(newNode.locationPoint))
                    {
                        queue.Add(newNode);
                    }
                }

                //search through the room and get all of the edges 
                foreach (MapEdge currentEdge in currentNode.Room.ListOfEdges)
                {
                    mPoint pointInRoom = currentEdge.GetPointUVInCurrentRoom(currentNode.Room);

                    //if the point has not been visited
                    if (!TableOfVisitedPoints.Contains(pointInRoom))
                    {

                        double newDistance = currentNode.locationPoint.GetDistanceToPoint(pointInRoom);

                        SearchNode newNode = new SearchNode(currentNode.Room,
                                                              pointInRoom,
                                                               currentEdge,
                                                             currentNode.ListOfPointsInUV,
                                                               newDistance);

                        newNode.addPoint(pointInRoom);

                        if (!TableOfVisitedPoints.Contains(newNode.locationPoint))
                        {
                            queue.Add(newNode);
                        }
                    }
                }
            }

            return null;
        }

        //convert the given location in UV and a destination point in UV 
        public Vector GetDirectionTo(mPoint currentLocationInUV, mPoint endPointInUV)
        {
            //vector is = (pointInUV.x - pointInUV.x, pointInUV.y - pointInUV.y)	
            return new Vector(currentLocationInUV.X - endPointInUV.X, currentLocationInUV.Y - endPointInUV.Y);
        }
    }

}

