using System;
using System.Collections.Generic;

//Note: at some point this should be redone with each Node inhariting from a base node class with distance 
//and location. And a single Priority Queue. This was done this way to save time. 

namespace Skywalker.Mapping
{

    //this has a built in table so it will not add nodes that have been visited to the queue
    //this is implamented using has a List Sturcture 
    //Note: implmenting this function with a heap would be a bit faster.
    public class PriorityQueue
    {

        List<SearchNode> Queue;

        public PriorityQueue()
        {
            Queue = new List<SearchNode>();
        }

        public void Add(SearchNode newNode)
        {

            if (isEmpty())
            {
                Queue.Add(newNode);
                return;
            }

            //look through each node present in the queue and make sure it does not already exist in the que
            foreach (SearchNode currentNode in Queue)
            {
                //if we have found a point that is already on the list
                if (currentNode.locationPoint == newNode.locationPoint)
                {
                    //check the distance. if the new nodes distance is less then the 
                    //old nodes distance replace the oldNode
                    if (currentNode.Distance >= newNode.Distance)
                    {
                        //replace paths that are the same lenth but have more points visited.
                        if (currentNode.ListOfPointsInUV.Count > newNode.ListOfPointsInUV.Count)
                        {
                            //remove the old node and replace it
                            Queue.Remove(currentNode);
                            Queue.Add(newNode);
                        }

                        //we have found what we are looking for
                        return;
                    }
                }
            }

            Queue.Add(newNode);

        }
        //returns if ture if the queue is empty
        public bool isEmpty()
        {
            if (Queue.Count == 0)
            {
                return true;
            }

            return false;
        }
        //returns null if the queue is empty
        //will return the node with the smallest distance if two are tied it will 
        //return the one it comes accross first
        public SearchNode Pop()
        {
            SearchNode smallestDistanceNode = null;

            foreach (SearchNode cNode in Queue)
            {
                //first itteration
                if (smallestDistanceNode == null)
                {
                    smallestDistanceNode = cNode;
                }
                //if the smallest node is larger then the current node replace it
                else if (smallestDistanceNode.Distance >= cNode.Distance)
                {
                    smallestDistanceNode = cNode;
                }
            }

            Queue.Remove(smallestDistanceNode);

            return smallestDistanceNode;
        }

        public void printPriorityQueue()
        {
            Console.Out.WriteLine("PriorityQue:");
            foreach (SearchNode node in Queue)
            {
                Console.Out.Write("\t");
                node.printSearchNode();
            }
        }
    }

    public class PathPQueue
    {
        List<PathSearchNode> Queue = new List<PathSearchNode>();

        public void Add(PathSearchNode newNode)
        {
            if (IsEmpty())
            {
                Queue.Add(newNode);
                return;
            }
            else
            {
                foreach (PathSearchNode cNode in Queue)
                {
                    //if we have found a point already on the list update that point with the new distance
                    //if it is less then old distance
                    if (cNode.CurrentGridPoint == newNode.CurrentGridPoint)
                    {
                        if (cNode.Distance >= newNode.Distance)
                        {
                            //replace paths that are the same lenth but have more points visited.
                            if (cNode.ListOfGridPoints.Count > newNode.ListOfGridPoints.Count)
                            {
                                Queue.Remove(cNode);
                                Queue.Add(newNode);
                            }

                            return;
                        }
                    }

                }
                Queue.Add(newNode);
            }


        }

        public bool IsEmpty()
        {
            return (Queue.Count == 0) ? true : false;
        }

        public PathSearchNode Pop()
        {
            PathSearchNode smallestDistanceNode = null;

            foreach (PathSearchNode cNode in Queue)
            {
                if (smallestDistanceNode == null)
                {
                    smallestDistanceNode = cNode;
                }
                else if (smallestDistanceNode.Distance >= cNode.Distance)
                {
                    smallestDistanceNode = cNode;
                }
            }

            Queue.Remove(smallestDistanceNode);

            return smallestDistanceNode;
        }

        public override string ToString()
        {
            string str = "";

            str += "PathPriorityQueue: ";

            foreach (PathSearchNode cNode in Queue)
            {
                str += cNode.ToString();
            }

            return str;
        }
    }

    public class PathSearchNode
    {
        public List<mGridPoint> ListOfGridPoints = new List<mGridPoint>();
        public mGridPoint CurrentGridPoint = null;
        public double Distance = 0;

        public PathSearchNode(mGridPoint currentPoint, List<mGridPoint> listOfGPoints, double distance)
        {
            CurrentGridPoint = currentPoint;
            AddPointsToListOfPointsUV(listOfGPoints);
            Distance = distance;
        }

        private void AddPointsToListOfPointsUV(List<mGridPoint> listOfGPoints)
        {
            foreach (mGridPoint gPoint in listOfGPoints)
            {
                ListOfGridPoints.Add(gPoint);
            }
        }

        public void Add(mGridPoint newGPoint)
        {
            ListOfGridPoints.Add(newGPoint);
        }

        public List<mPoint> GetListOfMPoints()
        {
            List<mPoint> ListOfPoints = new List<mPoint>();

            foreach (mGridPoint cPoint in ListOfGridPoints)
            {
                ListOfPoints.Add(cPoint);
            }

            return ListOfPoints;
        }

        public override string ToString()
        {
            string str = "{";

            str += "P: " + CurrentGridPoint.ToString() + " ";

            str += "Points = [";
            foreach (mGridPoint gPoint in ListOfGridPoints)
            {
                str += gPoint.ToString() + ", ";
            }
            str += "] ";

            str += "D: " + Distance + "} ";

            return str;

        }
    }

    public class SearchNode
    {
        public MapRoom Room = null;
        public MapEdge Edge = null;
        public mPoint locationPoint = null;
        public List<mPoint> ListOfPointsInUV = new List<mPoint>();
        public double Distance = 0;

        public SearchNode(MapRoom currentRoom, mPoint currentPoint, MapEdge currentMapEdge, List<mPoint> listOfPoints, double distance)
        {
            Room = currentRoom;
            locationPoint = currentPoint;
            Edge = currentMapEdge;
            AddPointsToListOfPointInUV(listOfPoints);
            Distance = distance;

        }
        public void AddPointsToListOfPointInUV(List<mPoint> listOfPoints)
        {
            foreach (mPoint currentPoint in listOfPoints)
            {
                ListOfPointsInUV.Add(currentPoint);
            }
        }
        public void addPoint(mPoint newPoint)
        {
            ListOfPointsInUV.Add(newPoint);
        }
        public void printSearchNode()
        {

            Console.Out.Write("\tSearchNode:");
            if (Room != null)
            {
                Console.Out.Write(" Room: " + Room.Name);
            }
            Console.Out.Write(" Point: " + locationPoint.ToString());
            if (Edge != null)
            {
                Console.Out.Write(Edge.ToString());
            }

            Console.Out.Write(" ListOfPoints: (");

            foreach (mPoint point in ListOfPointsInUV)
            {
                Console.Out.Write(point.ToString() + ", ");
            }

            Console.Out.WriteLine(" Distance: " + Math.Round(Distance, 2) + ")");
        }

    }
}
