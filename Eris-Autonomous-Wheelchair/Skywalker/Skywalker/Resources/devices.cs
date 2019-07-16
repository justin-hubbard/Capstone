using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Windows.Controls;
using Microsoft.Win32;
using SharpDX.XInput;

namespace Skywalker.Resources {
    public class Devices
    {
        static private List<string> connectedDevicesList = new List<string>();
        private static RegistryKey USB;

        public static void Initialize()
        {
            USB = GetUSBRegistryKey();
            CheckActiveUSB();
        }

        public static void CheckActiveUSB()
        {
            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_PnPEntity")) {
                collection = searcher.Get();
            }

            foreach (var device in collection)
            {
                var id = (string) device.GetPropertyValue("DeviceID");
                if (id.StartsWith("USB"))
                {
                    var name = GetFriendlyName(id);
                    if (name != null)
                    {
                        //Console.WriteLine("Device: {0} -> {1}", id, name);  
                        connectedDevicesList.Add(name);
                    }
                }
                
            }

        }

        private static string GetFriendlyName(string deviceID)
        {
            string[] idComponents = deviceID.Split('\\');
            string folder = idComponents[1];
            string subFolder = idComponents[2];
            string friendly = null;
            RegistryKey folderKey = USB.OpenSubKey(folder);
            RegistryKey subFolderKey = null;

            if (folderKey != null)
            {
                subFolderKey = folderKey.OpenSubKey(subFolder);
                if (subFolderKey != null)
                {
                    friendly = (string)subFolderKey.GetValue("FriendlyName");
                }
            }

            return friendly;

        }

        private static RegistryKey GetUSBRegistryKey()
        {
            RegistryKey key = Registry.LocalMachine;

            RegistryKey sys = key.OpenSubKey("SYSTEM");
            RegistryKey set = sys.OpenSubKey("CurrentControlSet");
            RegistryKey enumRegistryKey = set.OpenSubKey("Enum");
            RegistryKey usbKey = enumRegistryKey.OpenSubKey("USB");

            //string[] usbStrings = usbKey.GetSubKeyNames();

            //GetDeviceNames(usbKey, usbStrings);
            return usbKey;
        }

        public static bool IsControllerConnected()
        {
            bool connected = false;
            try
            {
                Controller controller = new Controller(0);
                connected = controller.IsConnected;
            }
            catch (Exception e)
            {
                connected = false;
            }
            return connected;
        }

        public static bool IsKinectConnected()
        {
            return Skywalker_Vision.Kinect.Utils.IsKinectConnected();
        }

        public static string IsWheelchairConnected()
        {
            bool connected =  connectedDevicesList.Any(device => device.ToLower().Contains("arduino"));
            string port = "";
            if (connected) {

                for (int i = 0; i < connectedDevicesList.Count; i++) {
                
                    if(connectedDevicesList[i].Contains("Arduino Uno")){
                    
                        port = connectedDevicesList[i];
                        break;
                    }
                }
            }

            if (port != "")
            {
                //parse the port name
                char[] delim = { '(', ')' };
                string[] pieces = port.Split(delim);
                port = pieces[1];
            }

            return port;

        }
        
        public static string FindComPort(string device_name){
            
            bool connected = connectedDevicesList.Any(device => device.ToLower().Contains(device_name.ToLower()));
            string port = "";
            if(connected){
                for(int i = 0; i < connectedDevicesList.Count; i++){
                    if(connectedDevicesList[i].Contains(device_name)){
                        port = connectedDevicesList[i];
                        break;
                    }
                }
            }
            if(port != ""){
                //parse the port name
                char[] delim = {'(', ')'};
                string[] pieces = port.Split(delim);
                port = pieces[1];
            }
            
            return port;
        }

        public static bool IsEyeTribeConnected()
        {
            return connectedDevicesList.Any(device => device.ToLower().Contains("eyetribe"));
        }
        
    }
}
