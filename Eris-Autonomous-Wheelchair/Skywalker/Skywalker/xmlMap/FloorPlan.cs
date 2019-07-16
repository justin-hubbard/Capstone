using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Skywalker.xmlMap
{
	[XmlRoot("FloorPlan")]
	public class FloorPlan
	{
		[XmlElement("Name")]
		public string Name
		{
			get;
			set;
		}

		[XmlElement("ImageName")]
		public string ImageName
		{
			get;
			set;
		}

		[XmlElement("Room")]
		public List<Room> Rooms
		{
			get;
			set;
		}

		[XmlElement("Connection")]
		public List<Connection> Connections
		{
			get;
			set;
		}

	}
}
