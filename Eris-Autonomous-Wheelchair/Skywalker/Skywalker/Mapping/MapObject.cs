using System;
using Skywalker.xmlMap;

namespace Skywalker.Mapping
{

    abstract public class ObjectShape
    {
        protected mPoint Location = null;

        //************************************* Abstract Methods *************************************

        abstract public bool DoesLineCross(mPoint pt1, mPoint pt2, int buffer);
        abstract public bool DoesContain(mPoint point, int buffer);

        abstract public double GetWidth();
        abstract public double GetLegnth();

        //************************************* Get and Set Methods *************************************

        public mPoint GetLocation()
        {
            return new mPoint(Location.X, Location.Y);
        }

        //************************************* Test Methods *************************************

        //given two lines defined as two points it will return if the lines cross
        //http://stackoverflow.com/questions/5514366/how-to-know-if-a-line-intersects-a-rectangle
        protected static bool LineIntersectsLine(mPoint l1p1, mPoint l1p2, mPoint l2p1, mPoint l2p2)
        {
            double q = (l1p1.Y - l2p1.Y) * (l2p2.X - l2p1.X) - (l1p1.X - l2p1.X) * (l2p2.Y - l2p1.Y);
            double d = (l1p2.X - l1p1.X) * (l2p2.Y - l2p1.Y) - (l1p2.Y - l1p1.Y) * (l2p2.X - l2p1.X);

            if (d == 0)
            {
                return false;
            }

            double r = q / d;

            q = (l1p1.Y - l2p1.Y) * (l1p2.X - l1p1.X) - (l1p1.X - l2p1.X) * (l1p2.Y - l1p1.Y);

            double s = q / d;

            if (r < 0 || r > 1 || s < 0 || s > 1)
            {
                return false;
            }

            return true;
        }

    }

    public class RectangleObj : ObjectShape
    {
        private double Width = 0;                  //in x direction
        private double Length = 0;                 //in y direction

        //****************** Constructors ******************

        //givent the upperleft hand corrner and the width in x and length in y
        public RectangleObj(mPoint location, double width, double length)
        {
            Location = location;
            Width = width;
            Length = length;
        }

        //************************************* Test Methods *************************************

        //returns if a line segment given by two points crosses the rectangle
        //http://stackoverflow.com/questions/5514366/how-to-know-if-a-line-intersects-a-rectangle
        public override bool DoesLineCross(mPoint pt1, mPoint pt2, int buffer = 0)
        {
            //check top (top left, top right)
            return LineIntersectsLine(pt1, pt2, new mPoint(Location.X - buffer, Location.Y - buffer), new mPoint(Location.X + Width + buffer, Location.Y + buffer)) ||
            //check left (top right, bottom right)
                   LineIntersectsLine(pt1, pt2, new mPoint(Location.X + Width + buffer, Location.Y - buffer), new mPoint(Location.X + Width + buffer, Location.Y + Length + buffer)) ||
            //check bottom (bottom right, bottom left)
                    LineIntersectsLine(pt1, pt2, new mPoint(Location.X + Width + buffer, Location.Y + Length + buffer), new mPoint(Location.X - buffer, Location.Y + Length + buffer)) ||
            //check right (bottom left, top left)
                    LineIntersectsLine(pt1, pt2, new mPoint(Location.X - buffer, Location.Y + Length + buffer), new mPoint(Location.X - buffer, Location.Y - buffer)) ||
            //check contains
            //from &&
                  (DoesContain(pt1, buffer) || DoesContain(pt2, buffer));

            //return LineIntersectsLine(pt1, pt2, new mPoint(Location.X, Location.Y), new mPoint(Location.X + Width, Location.Y)) ||
            //       LineIntersectsLine(pt1, pt2, new mPoint(Location.X + Width, Location.Y), new mPoint(Location.X + Width, Location.Y + Length)) ||
            //       LineIntersectsLine(pt1, pt2, new mPoint(Location.X + Width, Location.Y + Length), new mPoint(Location.X, Location.Y + Length)) ||
            //       LineIntersectsLine(pt1, pt2, new mPoint(Location.X, Location.Y + Length), new mPoint(Location.X, Location.Y)) ||
            //       (DoesContain(pt1) && DoesContain(pt2));   
        }

        //************************************* Overriden From ObjectShape Methods *************************************

        //retunrs if the point given is inside of the rectangle
        public override bool DoesContain(mPoint point, int buffer = 0)
        {
            //checks to see if the point is in the rectangle by making sure it is bounded by
            //the points

            //check in x
            //return (point.X - buffer >= Location.X && point.X + Width + buffer <= Location.X) &&
            ////check in y
            //       (point.Y - buffer >= Location.Y && point.Y + Length + buffer <= Location.Y);

            //check in x
            return (point.X >= Location.X - buffer && point.X <= Location.X + Width + buffer) &&
                   (point.Y >= Location.Y - buffer && point.Y <= Location.Y + Length + buffer);

            //return (point.X >= Location.X && point.X <= Location.X + Width) &&
            //       (point.Y >= Location.Y && point.Y <= Location.Y + Length);
        }

        public override double GetWidth()
        {
            return Width;
        }

        public override double GetLegnth()
        {
            return Length;
        }

        //************************************* Overriden Methods *************************************

        public override string ToString()
        {
            return "(Shape: Rectangle, Location: " + Location.ToString() + ", Width: " + Width + ", Length: " + Length + ")";
        }
    }

    //these represent areas inside of a room. Can be used for open floor plans. Like if you
    //have a kitchen joined with your living room. 
    public class MapArea
    {
        private string Name = "";
        private ObjectShape ObjShape = null;
        private Region xmlRegion;

        private double AreaWeight = 1;

        //****************** Constructors ******************

        public MapArea(string name, mPoint location, double width, double length)
        {
            Name = name;
            ObjShape = new RectangleObj(location, width, length);
        }

        public MapArea(Region region)
        {
            Name = region.Name;
            ObjShape = new RectangleObj(new mPoint(region.Origin_U, region.Origin_V), region.Width_U, region.Length_V);
            xmlRegion = region;
        }

        //************************************* Get and Set Methods *************************************

        public string GetName()
        {
            return Name;
        }

        public mPoint GetLocation()
        {
            return ObjShape.GetLocation();
        }

        //this should be removed if we exted this to support more then rectangles
        public double GetWidth()
        {
            return ObjShape.GetWidth();
        }

        public double GetLegnth()
        {
            return ObjShape.GetLegnth();
        }

        public double GetAreaWeight()
        {
            return AreaWeight;
        }

        public bool DoesLineCross(mPoint startPt, mPoint endPt, int buffer = 0)
        {
            //check to see if the vector intersects the shape with the 
            //addition of the bufferZone
            return ObjShape.DoesLineCross(startPt, endPt, buffer);
        }

        public Tuple<mPoint, Tuple<double, double>> GetCornerAndDimentions()
        {
            return new Tuple<mPoint, Tuple<double, double>>(GetLocation(), new Tuple<double, double>(GetWidth(), GetLegnth()));
        }

        //************************************* Test Methods *************************************

        public bool ContainsPoint(mPoint point, int buffer = 0)
        {
            return ObjShape.DoesContain(point, buffer);
        }

        //****************** Overriden Methods ******************

        public override string ToString()
        {
            return "Area: Name: " + Name + " " + ObjShape.ToString() + ".";
        }
    }

    //these represent known obsticals. Like built in furnature or a table that never gets moved. 
    public class MapObject
    {
        private string Name = "";
        private bool Permanent = true;
        private bool Passible = false;
        private ObjectShape ObjShape = null;

        //****************** Constructors ******************

        public MapObject(string name, bool passibility, mPoint location, int width, int length)
        {
            Name = name;
            Passible = passibility;

            ObjShape = new RectangleObj(location, width, length);
        }

        public MapObject(Room_Object rObj)
        {

            Name = rObj.Name;
            Permanent = rObj.Permanent;
            Passible = false;

            ObjShape = new RectangleObj(new mPoint(rObj.Origin_U, rObj.Origin_V), rObj.Width_U, rObj.Length_V);

        }

        //************************************* Test Methods *************************************

        //runs a check to see if the given Vector is intersecting
        //this object
        public bool DoesLineCross(mPoint startPt, mPoint endPt, int buffer = 0)
        {
            //check to see if the vector intersects the shape with the 
            //addition of the bufferZone
            return ObjShape.DoesLineCross(startPt, endPt, buffer);
        }

        public bool DoesContain(mPoint point, int buffer = 0)
        {
            return ObjShape.DoesContain(point, buffer);
        }

        public bool IsPassible()
        {
            return Passible;
        }

        //************************************* Get and Set Methods *************************************

        public string GetName()
        {
            return Name;
        }

        //************************************* Overriden Methods *************************************

        public override string ToString()
        {
            string str = "";

            str += "RoomObj: ";
            str += "Name: " + Name + ", ";
            str += ObjShape.ToString() + " ";
            str += "Perm: " + Permanent + ", ";
            str += "Pass: " + Passible + ".";

            return str;

        }

    }
}
