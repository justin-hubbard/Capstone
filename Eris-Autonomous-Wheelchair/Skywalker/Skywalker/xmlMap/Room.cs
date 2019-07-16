using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Skywalker.xmlMap
{
	[Serializable()]
	public class Room
	{

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlElement("TopLeftCorner_U")]
        public double TopLeftU { get; set; }

        [XmlElement("TopLeftCorner_V")]
        public double TopLeftV { get; set; }

        [XmlElement("TopRightCorner_U")]
        public double TopRightU { get; set; }

        [XmlElement("TopRightCorner_V")]
        public double TopRightV { get; set; }

        [XmlElement("BottomLeftCorner_U")]
        public double BottomLeftU { get; set; }

        [XmlElement("BottomLeftCorner_V")]
        public double BottomLeftV { get; set; }

        [XmlElement("BottomRightCorner_U")]
        public double BottomRightU { get; set; }

        [XmlElement("BottomRightCorner_V")]
        public double BottomRightV { get; set; }

        // num grid points in u direction 
        [XmlElement("Number_Width_GridPoints")]
        public int Number_Width_GridPoints { get; set; }

        // num grid points in v direction 
        [XmlElement("Number_Length_GridPoints")]
        public int Number_Length_GridPoints { get; set; }

        // root ips.x coordinate in u
        [XmlElement("Root_IPS_U")]
        public double Root_IPS_U { get; set; }

        // root ips.y coordinate in v
        [XmlElement("Root_IPS_V")]
        public double Root_IPS_V { get; set; }

        // width in cm of room
        [XmlElement("Width_CM")]
        public double Width_CM { get; set; }

        // length in cm of room
        [XmlElement("Length_CM")]
        public double Length_CM { get; set; }

        // horizontal element
        [XmlElement("U_Width")]
        public double Width_U { get; set; }

        // vertical element
        [XmlElement("V_Length")]
        public double Length_V { get; set; }

        // index in gridpoint list of root element
        [XmlElement("IndexRoot")]
        public int IndexOfRoot { get; set; }

        [XmlElement("Object")]
        public List<Room_Object> Objects { get; set; }

        [XmlElement("Region")]
        public List<Region> Regions { get; set; }

        [XmlElement("GridPoint")]
        public List<GridPoint> GridPoints { get; set; }
    }
}