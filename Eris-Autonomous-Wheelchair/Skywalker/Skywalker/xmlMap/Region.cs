using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Skywalker.xmlMap
{
    [Serializable]
    public class Region
    {

        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Origin_U")]
        public double Origin_U { get; set; }

        [XmlElement("Origin_V")]
        public double Origin_V { get; set; }

        [XmlElement("Width_U")]
        public double Width_U { get; set; }

        [XmlElement("Length_V")]
        public double Length_V { get; set; }
        
        [XmlIgnore]
        public int CanvasIndex { get; set; }

    }
}