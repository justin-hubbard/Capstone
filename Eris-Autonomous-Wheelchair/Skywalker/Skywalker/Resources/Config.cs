using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;

namespace Skywalker.Resources
{
    /// <summary>
    /// Contains the system configuration attributes.
    /// Implements a singleton pattern.
    /// </summary>
    public class Config
    {
        /// <summary>
        /// View angle of the depth camera
        /// </summary>
        public double DEPTH_VIEW_ANGLE;

        /// <summary>
        /// Number of horizontal pixels in the depth view.
        /// </summary>
        public int DEPTH_PIXEL_WIDTH;

        /// <summary>
        /// Private constructor, to prevent leaking instances.
        /// </summary>
        private Config() { }

        /// <summary>
        /// Initializes from the configuration file.
        /// </summary>
        private static void ReadConfig()
        {
            XmlSerializer serializer = new XmlSerializer(new Config().GetType());

            using (FileStream fileStream = new FileStream(Constants.CONFIG_FILE, FileMode.Open))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    Config config = serializer.Deserialize(streamReader) as Config;
                    _instance = config;
                }
            }
        }

        /// <summary>
        /// Private Singleton instance.
        /// </summary>
        private static Config _instance;

        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static Config Instance
        {
            get
            {
                if (_instance == null)
                {
                    ReadConfig();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Initializes the singleton.
        /// </summary>
        public static void Initialize()
        {
            if (_instance == null)
            {
                ReadConfig();
            }
        }
    }
}
