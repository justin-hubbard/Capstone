using System;

namespace Skywalker.Input {
    // A delegate type for hooking up change notifications.
    public delegate void UserInputChanged(object sender, EventArgs e);

    static class Input {
        public static event EventHandler DeviceChanged;
        public static event EventHandler<DeviceChangedEventArgs> DeviceChangedArgs;

        public static void RequestChange(object sender, string device)
        {
            if (DeviceChanged != null)
            {
                DeviceChangedEventArgs args = new DeviceChangedEventArgs(device);
                DeviceChanged(sender, args);   
            }
        }
    }
}
