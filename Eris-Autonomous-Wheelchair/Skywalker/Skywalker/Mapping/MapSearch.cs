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
		public List<mPoint> Search(mPoint startPointInUV, mPoint endPointInUV)
		{

			//get the start and end rooms and make sure they exist
			MapRoom startRoom = getRoomFromPointInUV (startPointInUV);
			if (startRoom == null) 
			{
				return null;
			}
			MapRoom endRoom = getRoomFromPointInUV (endPointInUV);
			if (endRoom == null) 
			{
				return null;
			}

			if ((startPointInUV.X == endPointInUV.X) && (startPointInUV.Y == endPointInUV.Y)) 
			{
				List<mPoint> listOfPoints = new List<mPoint> ();
				listOfPoints.Add (endPointInUV);

				return listOfPoints;
			}

			//if we are searching in room
			if (startRoom == endRoom) 
			{
				//get the list of points and return them
				List<mPoint> listOfPoints = new List<mPoint> ();
				listOfPoints.Add (startPointInUV);
				listOfPoints.Add (endPointInUV);

				return listOfPoints;
			}
		

			PriorityQue queue = new PriorityQue ();

			//hashset to keep track of the visited points 
			HashSet<mPoint> TableOfVisitedPoints = new HashSet<mPoint>();

			//add the starting point to the que
			List<mPoint> startingListOfPoints = new List<mPoint> ();
			startingListOfPoints.Add (startPointInUV);
			queue.Add(new SearchNode(startRoom, startPointInUV, null, startingListOfPoints, 0));

			//queue.printPriorityQueue ();
			int index = 0;
	
			while (!queue.isEmpty ()) 
			{

				index++;

				//get the node with the shortest path
				SearchNode currentNode = queue.Pop ();

				//mark the point we have visited off.
				TableOfVisitedPoints.Add (currentNode.locationPoint);

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
					double newDistance = currentNode.Distance + currentNode.locationPoint.getDistanceToPoint (endPointInUV);

					//the searchNode has the same room as the currentNode, the newpoint will be the endpoint, there is no edge
					//the list of points will be updated with the endPoint and the distance will be updated
					SearchNode newNode = new SearchNode (currentNode.Room,
						                     			 endPointInUV,
						                     			 null, 
														 currentNode.ListOfPointsInUV,
						                     			 newDistance);

					newNode.addPoint (endPointInUV);

					//does not check table because if we have hit the endpoint already we are done
					queue.Add (newNode);
					
				}

				//if there is an edge to traverse 
				if (currentNode.Edge != null) 
				{
					mPoint nextPoint = currentNode.Edge.getPointInOpositRoom (currentNode.Room).PointInUV;
					MapRoom newRoom = currentNode.Edge.getOpositRoom (currentNode.Room);
					double newDistance = currentNode.Distance + currentNode.Edge.getDistanceUV ();

					//moves the room the the next room, moves the point to the next room's point 
					//keeps the edge the same and adds the distance of the edge to the node distance
					SearchNode newNode = new SearchNode (newRoom,
														 nextPoint,
													   	 currentNode.Edge, 
														 currentNode.ListOfPointsInUV,
														 newDistance);

					newNode.addPoint (nextPoint);

					//make sure we have not been to that point before
					if (!TableOfVisitedPoints.Contains (newNode.locationPoint)) 
					{
						queue.Add (newNode);
					}
				}

				//search through the room and get all of the edges 
				foreach (MapEdge currentEdge in currentNode.Room.ListOfEdges) 
				{
					mPoint pointInRoom = currentEdge.getPointInRoom (currentNode.Room).PointInUV;

					//if the point has not been visited
					if(!TableOfVisitedPoints.Contains(pointInRoom))	
					{
								
						double newDistance = currentNode.locationPoint.getDistanceToPoint (pointInRoom);

						SearchNode newNode = new SearchNode (currentNode.Room,
							                 			     pointInRoom,
							              			         currentEdge,
															 currentNode.ListOfPointsInUV,
							              			         newDistance);

						newNode.addPoint (pointInRoom);

						if (!TableOfVisitedPoints.Contains (newNode.locationPoint)) 
						{
							queue.Add (newNode);
						}								
					}
				}
			}
				
			return null;
		}

		//convert the given location
		public Vector getDirectionTo(mPoint startPointInUV, mPoint destPointInUV, MapRoom CurrentRoom)
		{	
            return new Vector(destPointInUV.X - startPointInUV.X, destPointInUV.Y - startPointInUV.Y);
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
				ListOfPointsInUV.Add (currentPoint);
			}
		}
		public void addPoint(mPoint newPoint)
		{
			ListOfPointsInUV.Add(newPoint);
		}
		public void printSearchNode()
		{

			Console.Out.Write ("\tSearchNode:");
			if (Room != null) {
				Console.Out.Write (" Room: " + Room.Name);
			}
			Console.Out.Write (" Point: " + locationPoint.ToString());
			if (Edge != null) {
				Console.Out.Write (Edge.ToString());
			}

			Console.Out.Write (" ListOfPoints: (");

			foreach (mPoint point in ListOfPointsInUV) 
			{
				Console.Out.Write (point.ToString () + ", ");
			}

			Console.Out.WriteLine (" Distance: " + Math.Round( Distance,2) + ")");
		}

	}
	//this has a built in table so it will not add nodes that have been visited to the queue
	//this is implamented using has a List Sturcture 
	//Note: implmenting this function with a heap would be a bit faster.
	public class PriorityQue
	{
		
		List<SearchNode> Queue;

		public PriorityQue()
		{
			Queue = new List<SearchNode> ();
		}

		public void Add(SearchNode newNode)
		{

			if (isEmpty ()) 
			{
				Queue.Add (newNode);
			}
			
			//look through each node present in the queue and make sure it does not already exit in the que
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
							Queue.Remove (currentNode);
							Queue.Add (newNode);
						}

						//we have found what we are looking for
						return;
					} 
				} 
			}

			Queue.Add (newNode);

		}
		//returns if ture if the queue is empty
		public bool isEmpty()
		{
			if(Queue.Count == 0)
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
				else if(smallestDistanceNode.Distance >= cNode.Distance)
				{
					smallestDistanceNode = cNode;
				}
			}	

			Queue.Remove (smallestDistanceNode);

			return smallestDistanceNode;
		}

		public void printPriorityQueue()
		{
			Console.Out.WriteLine ("PriorityQue:");
			foreach (SearchNode node in Queue) 
			{
				Console.Out.Write("\t");
				node.printSearchNode ();
			}
		}
	}
}

