using SAPTeam.AndroCtrl.Adb;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using Windows.Devices.Enumeration;
using WOADeviceManager.Helpers;

namespace WOADeviceManager.Managers
{
    public class DeviceManager
    {
        private DeviceWatcher watcher;
        private static DeviceMonitor adbMonitor;

        private static DeviceManager _instance;
        public static DeviceManager Instance
        {
            get
            {
                if (_instance == null) _instance = new DeviceManager();
                return _instance;
            }
        }

        private static Device device;
        public static Device Device
        {
            get
            {
                if (device == null) device = new Device();
                return device;
            }
            private set { device = value; }
        }

        public delegate void DeviceFoundEventHandler(object sender, Device device);
        public delegate void DeviceConnectedEventHandler(object sender, Device device);
        public delegate void DeviceDisconnectedEventHandler(object sender, Device device);
        public static event DeviceFoundEventHandler DeviceFoundEvent;
        public static event DeviceConnectedEventHandler DeviceConnectedEvent;
        public static event DeviceDisconnectedEventHandler DeviceDisconnectedEvent;

        private DeviceManager()
        {
            if (device == null) device = new Device();

            adbMonitor = new DeviceMonitor(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)));
            adbMonitor.DeviceConnected += OnADBDeviceConnected;
            adbMonitor.DeviceDisconnected += OnADBDeviceDisconnected;
            adbMonitor.Start();

            watcher = DeviceInformation.CreateWatcher();
            watcher.Added += DeviceAdded;
            watcher.Removed += DeviceRemoved;
            watcher.Updated += Watcher_Updated;
            watcher.Start();
        }

        void OnADBDeviceConnected(object sender, DeviceDataEventArgs e)
        {
            Debug.WriteLine($"The device {e.Device.Serial} has connected to this PC");
            var connectedDevices = ADBManager.Client.GetDevices();

            foreach (var connectedDevice in connectedDevices)
            {
                if (e.Device.Serial.Equals(connectedDevice.Serial))
                {
                    device.SerialNumber = connectedDevice.Serial;
                    device.Data = connectedDevice;
                }
            }
        }

        void OnADBDeviceDisconnected(object sender, DeviceDataEventArgs e)
        {
            Debug.WriteLine($"The device {e.Device.Serial} has disconnected from this PC");
        }

        public static void ManuallyCheckForADBDevices()
        {
            try
            {
                var connectedDevices = ADBManager.Client.GetDevices();
                var firstDevice = connectedDevices.FirstOrDefault();
                if (firstDevice != null)
                {
                    Device.SerialNumber = firstDevice.Serial;
                    Device.Data = firstDevice;
                }
            } catch { }
        }

        private void Watcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            args.Properties.TryGetValue("System.Devices.InterfaceEnabled", out object? IsInterfaceEnabledObjectValue);
            bool IsInterfaceEnabled = (bool?)IsInterfaceEnabledObjectValue ?? false;

            if (args.Id.Equals(device.ADBID) && IsInterfaceEnabled == true)
            {
                Thread.Sleep(1000); //ADB doesn't get enough time to connect to the device, needs a better way to wait -> maybe run "adb devices" each 0.5s until the device is detected
                device.LastInformationUpdate = args;
                device.Name = ADBProcedures.GetDeviceProductModel();
                device.State = Device.DeviceStateEnum.ANDROID_ADB_ENABLED;
                DeviceConnectedEvent?.Invoke(sender, device);
            }
            else if (args.Id.Equals(device.FastbootID) && IsInterfaceEnabled == true)
            {
                Thread.Sleep(1000); //Fastboot doesn't get enough time to connect to the device, needs a better way to wait -> maybe run "fastboot devices" each 0.5s until the device is detected
                device.LastInformationUpdate = args;
                device.State = Device.DeviceStateEnum.BOOTLOADER;
                device.Name = FastbootProcedures.GetProduct(device.SerialNumber);
                DeviceConnectedEvent?.Invoke(sender, device);
            }
            else if (args.Id.Equals(device.TWRPID) && IsInterfaceEnabled == true)
            {
                Thread.Sleep(1000); //Fastboot doesn't get enough time to connect to the device, needs a better way to wait -> maybe run "adb devices" each 0.5s until the device is detected
                device.LastInformationUpdate = args;
                device.State = Device.DeviceStateEnum.TWRP;
                device.Name = ADBProcedures.GetDeviceProductModel();
                DeviceConnectedEvent?.Invoke(sender, device);
            }   
            else if ((args.Id.Equals(device.ADBID) && device.State == Device.DeviceStateEnum.ANDROID_ADB_ENABLED && IsInterfaceEnabled == false)
                || (args.Id.Equals(device.TWRPID) && device.State == Device.DeviceStateEnum.TWRP && IsInterfaceEnabled == false)
                || (args.Id.Equals(device.FastbootID) && device.State == Device.DeviceStateEnum.BOOTLOADER && IsInterfaceEnabled == false))
            {
                device.LastInformationUpdate = args;
                device.State = Device.DeviceStateEnum.DISCONNECTED;
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
                    Thread.Sleep(1000); // TODO: ADB doesn't get enough time to connect to the device, needs a better way to wait -> maybe run "adb devices" each 0.5s until the device is detected
                    device.State = Device.DeviceStateEnum.ANDROID_ADB_ENABLED;
                    device.Name = ADBProcedures.GetDeviceProductModel();
                    DeviceConnectedEvent?.Invoke(null, device);
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
                    Thread.Sleep(1000); // TODO: ADB doesn't get enough time to connect to the device, needs a better way to wait -> maybe run "adb devices" each 0.5s until the device is detected
                    device.State = Device.DeviceStateEnum.BOOTLOADER;
                    device.Name = FastbootProcedures.GetProduct(device.SerialNumber);
                    DeviceConnectedEvent?.Invoke(null, device);
                }
            }
            if (args.Name == "MTP")
            {
                device.TWRPID = args.Id;
                device.Information = args;
                string pattern = @"#(\d+)#";
                Match match = Regex.Match(device.TWRPID, pattern);
                device.SerialNumber = match.Groups[1].Value;
                if (args.IsEnabled)
                {
                    Thread.Sleep(1000); // TODO: ADB doesn't get enough time to connect to the device, needs a better way to wait -> maybe run "adb devices" each 0.5s until the device is detected
                    device.State = Device.DeviceStateEnum.TWRP;
                    // device.Name = ADBProcedures.GetDeviceProductModel(device.SerialNumber).GetAwaiter().GetResult(); // TODO: Find a way to detect what device is connected, if we want to make this work with other models too
                    device.Name = "Surface Duo";
                    DeviceConnectedEvent?.Invoke(null, device);
                }
            }
        }

        private void DeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
        }
    }
}
