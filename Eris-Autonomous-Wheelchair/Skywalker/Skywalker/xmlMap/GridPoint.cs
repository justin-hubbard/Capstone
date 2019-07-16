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
	public class GridPoint
	{
        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("U")]
        public double U { get; set; }

        [XmlElement("V")]
        public double V { get; set; }

        [XmlElement("IPS_X")]
        public double IPS_X { get; set; }

        [XmlElement("IPS_Y")]
        public double IPS_Y { get; set; }

        [XmlIgnore]
        public double Radius { get; set; }

        [XmlElement("DrawingOrigin_U")]
        public double DrawingOrigin_U { get; set; }

        [XmlElement("DrawingOrigin_V")]
        public double DrawingOrigin_V { get; set; }

        [XmlElement("Preferred")]
        public bool Preferred_Path { get; set; }

        [XmlElement("Contains_Obstacle")]
        public bool Contains_Obstacle { get; set; }

        [XmlElement("Associated_Region")]
        public string Region { get; set; }

        [XmlIgnore]
        public int CanvasIndex { get; set; }
    }
}