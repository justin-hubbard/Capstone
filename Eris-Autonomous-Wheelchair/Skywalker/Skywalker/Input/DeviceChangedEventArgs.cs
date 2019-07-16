using System;

namespace Skywalker.Input {
    public class DeviceChangedEventArgs : EventArgs
    {
        private readonly string _deviceName;

        public DeviceChangedEventArgs(string deviceName)
        {
            _deviceName = deviceName;
        }

        public string DeviceName
        {
            get { return _deviceName; }
        }
    }
}
