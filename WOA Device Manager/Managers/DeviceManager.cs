using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace WOADeviceManager.Managers
{
    public class DeviceManager
    {
        private DeviceWatcher watcher;

        private static DeviceManager _instance;
        public static DeviceManager Instance
        {
            get
            {
                if (_instance == null) _instance = new DeviceManager();
                return _instance;
            }
        }

        public static Device device;
        public static Device Device
        {
            get
            {
                return device;
            }
            private set { device = value; }
        }

        public delegate void DeviceFoundEventHandler(DeviceWatcher sender, Device device);
        public event DeviceFoundEventHandler DeviceFoundEvent;

        public DeviceManager() 
        {
            watcher = DeviceInformation.CreateWatcher();
            watcher.Added += DeviceAdded;
            watcher.Removed += DeviceRemoved;
            watcher.Start();
        }

        private void DeviceAdded(DeviceWatcher sender, DeviceInformation args)
        {
            // TODO: Replace with ID or whatever needed to make it unique
            // TODO: If we're going to support multiple devices, needs a list of compatible identifiers
            if (args.Name == "Surface Duo ADB" && args.IsEnabled) 
            {
                Device device = new Device()
                {
                    DeviceId = args.Id,
                    DeviceName = "Surface Duo",
                    DeviceState = Device.DeviceStateType.ANDROID_ADB_ENABLED,
                    DeviceInformation = args
                };
                DeviceFoundEvent?.Invoke(sender, device);
            }
        }

        private void DeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
        }
    }
}
