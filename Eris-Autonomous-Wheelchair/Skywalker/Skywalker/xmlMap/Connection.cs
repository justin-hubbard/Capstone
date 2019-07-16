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
	public class Connection
	{
        [XmlElement("MyRoom_Name")]
        public string My_Room_Name { get; set; }

        [XmlElement("MyRoom_U")]
        public double Room_U { get; set; }

        [XmlElement("MyRoom_V")]
        public double Room_V { get; set; }

        [XmlElement("MyRoom_IPS_X")]
        public double Room_IPS_X { get; set; }

        [XmlElement("MyRoom_IPS_Y")]
        public double Room_IPS_Y { get; set; }

        [XmlElement("OtherRoom_Name")]
        public string Other_Room_Name { get; set; }

        [XmlElement("OtherRoom_U")]
        public double Other_Room_U { get; set; }

        [XmlElement("OtherRoom_V")]
        public double Other_Room_V { get; set; }

        [XmlElement("OtherRoom_IPS_X")]
        public double Other_Room_IPS_X { get; set; }

        [XmlElement("OtherRoom_IPS_Y")]
        public double Other_Room_IPS_Y { get; set; }

        [XmlElement("Doorway_Distance_CM")]
        public double Distance_CM { get; set; }

        [XmlElement("Doorway_Distance_UV")]
        public double UVDistance { get; set; }
    }
}
