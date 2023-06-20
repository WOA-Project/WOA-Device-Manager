using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using WOADeviceManager.Helpers;

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

        public delegate void DeviceConnectedEventHandler(DeviceWatcher sender, Device device);
        public delegate void DeviceDisconnectedEventHandler(DeviceWatcher sender, Device device);
        public event DeviceConnectedEventHandler DeviceConnectedEvent;
        public event DeviceDisconnectedEventHandler DeviceDisconnectedEvent;

        public DeviceManager()
        {
            device = new Device();

            watcher = DeviceInformation.CreateWatcher();
            watcher.Added += DeviceAdded;
            watcher.Removed += DeviceRemoved;
            watcher.Updated += Watcher_Updated;
            watcher.Start();
        }

        private void Watcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            if (args.Id.Equals(device.ADBID) && (bool)args.Properties["System.Devices.InterfaceEnabled"] == true)
            {
                Thread.Sleep(1000); //ADB doesn't get enough time to connect to the device, needs a better way to wait -> maybe run "adb devices" each 0.5s until the device is detected
                device.LastInformationUpdate = args;
                device.State = Device.DeviceState.ANDROID_ADB_ENABLED;
                device.Name = ADBProcedures.GetDeviceProductModel(device.SerialNumber).GetAwaiter().GetResult();
                DeviceConnectedEvent?.Invoke(sender, device);
            }
            else if (args.Id.Equals(device.FastbootID) && (bool)args.Properties["System.Devices.InterfaceEnabled"] == true)
            {
                Thread.Sleep(1000); //Fastboot doesn't get enough time to connect to the device, needs a better way to wait -> maybe run "fastbot devices" each 0.5s until the device is detected
                device.LastInformationUpdate = args;
                device.State = Device.DeviceState.FASTBOOT;
                device.Name = FastbootProcedures.GetProduct(device.SerialNumber);
                DeviceConnectedEvent?.Invoke(sender, device);
            }
            else if ((args.Id.Equals(device.ADBID) && device.State == Device.DeviceState.ANDROID_ADB_ENABLED && (bool)args.Properties["System.Devices.InterfaceEnabled"] == false) 
                || (args.Id.Equals(device.FastbootID) && device.State == Device.DeviceState.FASTBOOT && (bool)args.Properties["System.Devices.InterfaceEnabled"] == false))
            {
                device.LastInformationUpdate = args;
                device.State = Device.DeviceState.DISCONNECTED;
                DeviceDisconnectedEvent?.Invoke(sender, device);
            }
        }

        private void DeviceAdded(DeviceWatcher sender, DeviceInformation args)
        {
            // TODO: Replace with ID or whatever needed to make it unique
            // TODO: If we're going to support multiple devices, needs a list of compatible names
            if (args.Name == "Surface Duo ADB")
            {
                device.ADBID = args.Id;
                device.Information = args;
                string pattern = @"#(\d+)#";
                Match match = Regex.Match(device.ADBID, pattern);
                device.SerialNumber = match.Groups[1].Value;
                if (args.IsEnabled)
                {
                    Thread.Sleep(1000); //ADB doesn't get enough time to connect to the device, needs a better way to wait -> maybe run "adb devices" each 0.5s until the device is detected
                    device.State = Device.DeviceState.ANDROID_ADB_ENABLED;
                    device.Name = ADBProcedures.GetDeviceProductModel(device.SerialNumber).GetAwaiter().GetResult();
                    DeviceConnectedEvent?.Invoke(sender, device);
                }
            }
            if (args.Name == "Surface Duo Fastboot")
            {
                device.FastbootID = args.Id;
                device.Information = args;
                string pattern = @"#(\d+)#";
                Match match = Regex.Match(device.FastbootID, pattern);
                device.SerialNumber = match.Groups[1].Value;
                if (args.IsEnabled)
                {
                    Thread.Sleep(1000); //ADB doesn't get enough time to connect to the device, needs a better way to wait -> maybe run "adb devices" each 0.5s until the device is detected
                    device.State = Device.DeviceState.FASTBOOT;
                    device.Name = FastbootProcedures.GetProduct(device.SerialNumber);
                    DeviceConnectedEvent?.Invoke(sender, device);
                }
            }
        }

        private void DeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
        }
    }
}
