using System;

namespace Skywalker.Mapping
{
    public class MapPOI
    {

        public string Name;
        public mPoint Location;

        //****************** Constructors ******************

        public MapPOI(string name, mPoint location)
        {
            Name = name;
            Location = location;
        }

        //************************************* Overridden Methods *************************************
        public override string ToString()
        {
            return "(Name: " + Name + ", (U,V): " + Location.ToString() + " )";
        }

    }
}
