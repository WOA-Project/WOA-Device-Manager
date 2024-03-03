using FastBoot;
using SAPTeam.AndroCtrl.Adb;
using System.Collections.Generic;
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
        private readonly DeviceWatcher watcher;
        private static DeviceMonitor adbMonitor;

        private static DeviceManager _instance;
        public static DeviceManager Instance
        {
            get
            {
                _instance ??= new DeviceManager();
                return _instance;
            }
        }

        private static Device device;
        public static Device Device
        {
            get
            {
                device ??= new Device();
                return device;
            }
            private set => device = value;
        }

        public delegate void DeviceFoundEventHandler(object sender, Device device);
        public delegate void DeviceConnectedEventHandler(object sender, Device device);
        public delegate void DeviceDisconnectedEventHandler(object sender, Device device);
        public static event DeviceFoundEventHandler DeviceFoundEvent;
        public static event DeviceConnectedEventHandler DeviceConnectedEvent;
        public static event DeviceDisconnectedEventHandler DeviceDisconnectedEvent;

        private DeviceManager()
        {
            device ??= new Device();

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

        private void OnADBDeviceConnected(object sender, DeviceDataEventArgs e)
        {
            Debug.WriteLine($"The device {e.Device.Serial} has connected to this PC");
            System.Collections.Generic.List<DeviceData> connectedDevices = ADBManager.Client.GetDevices();

            foreach (DeviceData connectedDevice in connectedDevices)
            {
                if (e.Device.Serial.Equals(connectedDevice.Serial))
                {
                    device.SerialNumber = connectedDevice.Serial;
                    device.AndroidDebugBridgeTransport = connectedDevice;
                }
            }
        }

        private void OnADBDeviceDisconnected(object sender, DeviceDataEventArgs e)
        {
            Debug.WriteLine($"The device {e.Device.Serial} has disconnected from this PC");
            Device.AndroidDebugBridgeTransport = null;
        }

        public static void ManuallyCheckForADBDevices()
        {
            try
            {
                System.Collections.Generic.List<DeviceData> connectedDevices = ADBManager.Client.GetDevices();
                DeviceData firstDevice = connectedDevices.FirstOrDefault();
                if (firstDevice != null)
                {
                    Device.SerialNumber = firstDevice.Serial;
                    Device.AndroidDebugBridgeTransport = firstDevice;
                }
            }
            catch { }
        }

        private void Watcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            _ = args.Properties.TryGetValue("System.Devices.InterfaceEnabled", out object? IsInterfaceEnabledObjectValue);
            bool IsInterfaceEnabled = (bool?)IsInterfaceEnabledObjectValue ?? false;

            if (args.Id.Equals(Device.ADBID) && IsInterfaceEnabled)
            {
                Thread.Sleep(1000);
                device.LastInformationUpdate = args;

                switch (ADBProcedures.GetDeviceProductDevice())
                {
                    case "duo":
                        {
                            device.Name = "Surface Duo";
                            string productName = ADBProcedures.GetDeviceProductName();
                            switch (productName)
                            {
                                case "duo":
                                    {
                                        device.Variant = "GEN";
                                        break;
                                    }
                                case "duo-att":
                                    {
                                        device.Variant = "ATT";
                                        break;
                                    }
                                case "duo-eu":
                                    {
                                        device.Variant = "EEA";
                                        break;
                                    }
                                default:
                                    {
                                        device.Variant = productName;
                                        break;
                                    }
                            }
                            device.Product = Device.DeviceProduct.Epsilon;
                            device.State = Device.DeviceStateEnum.ANDROID_ADB_ENABLED;

                            DeviceConnectedEvent?.Invoke(null, device);
                            break;
                        }
                    case "duo2":
                        {
                            device.Name = "Surface Duo 2";
                            device.Variant = "";
                            device.Product = Device.DeviceProduct.Zeta;
                            device.State = Device.DeviceStateEnum.ANDROID_ADB_ENABLED;

                            DeviceConnectedEvent?.Invoke(sender, device);
                            break;
                        }
                }
            }
            else if (args.Id.Equals(Device.BootloaderID) && IsInterfaceEnabled)
            {
                Thread.Sleep(1000);
                device.LastInformationUpdate = args;

                if (Device.FastBootTransport != null)
                {
                    Device.FastBootTransport.Dispose();
                    Device.FastBootTransport.Close();
                }

                device.FastBootTransport = new FastBoot.FastBootTransport(device.BootloaderID);

                switch (FastbootProcedures.GetProduct())
                {
                    case "surfaceduo":
                    case "duo":
                        {
                            device.Name = "Surface Duo";
                            device.Variant = "";
                            device.Product = Device.DeviceProduct.Epsilon;
                            break;
                        }
                    case "surfaceduo2":
                    case "duo2":
                        {
                            device.Name = "Surface Duo 2";
                            device.Variant = "";
                            device.Product = Device.DeviceProduct.Zeta;
                            break;
                        }
                }

                device.FastBootTransport.GetVariable("is-userspace", out string isUserSpaceString);
                if (isUserSpaceString == "yes")
                {
                    device.State = Device.DeviceStateEnum.FASTBOOTD;
                }
                else
                {
                    device.State = Device.DeviceStateEnum.BOOTLOADER;
                }

                DeviceConnectedEvent?.Invoke(sender, device);
            }
            else if (args.Id.Equals(Device.TWRPID) && IsInterfaceEnabled && args.Id.StartsWith(@"\\?\USB#VID_05C6&PID_9039&MI_00"))
            {
                Thread.Sleep(1000);
                device.LastInformationUpdate = args;

                device.Name = "Surface Duo";
                device.Variant = "";
                device.Product = Device.DeviceProduct.Epsilon;
                device.State = Device.DeviceStateEnum.TWRP;

                DeviceConnectedEvent?.Invoke(sender, device);
            }
            else if (args.Id.Equals(Device.TWRPID) && IsInterfaceEnabled && args.Id.StartsWith(@"\\?\USB#VID_18D1&PID_D001"))
            {
                Thread.Sleep(1000);
                device.LastInformationUpdate = args;

                device.Name = "Surface Duo 2";
                device.Variant = "";
                device.Product = Device.DeviceProduct.Zeta;
                device.State = Device.DeviceStateEnum.TWRP;

                DeviceConnectedEvent?.Invoke(sender, device);
            }
            else if ((args.Id.Equals(device.ADBID) && device.State == Device.DeviceStateEnum.ANDROID_ADB_ENABLED && IsInterfaceEnabled == false)
                || (args.Id.Equals(device.TWRPID) && device.State == Device.DeviceStateEnum.TWRP && IsInterfaceEnabled == false)
                || (args.Id.Equals(device.BootloaderID) && device.State == Device.DeviceStateEnum.BOOTLOADER && IsInterfaceEnabled == false))
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

            // Adb Enabled Android example:
            //
            // Args.ID: \\?\USB#VID_045E&PID_0C26#0F0012E214600A#{dee824ef-729b-4a0e-9c14-b7117d33a817}
            //
            // SurfaceDuoUsb.inf list:
            //
            // %Fastboot%              = Fastboot_Install, USB\VID_045E&PID_0C2F
            // %Adb%                   = Adb_Install, USB\VID_045E&PID_0C26
            // %AdbSideload%           = AdbSideload_Install, USB\VID_045E&PID_0C30
            // %AdbComposite%          = AdbComposite_Install, USB\VID_045E&PID_0C26&MI_01
            // %AdbCompositeTether%    = AdbCompositeTether_Install, USB\VID_045E&PID_0C28&MI_02
            // %AdbCompositeFT%        = AdbCompositeFT_Install, USB\VID_045E&PID_0C2A&MI_01
            // %AdbCompositePTP%       = AdbCompositePTP_Install, USB\VID_045E&PID_0C2C&MI_01
            // %AdbCompositeMIDI%      = AdbCompositeMIDI_Install, USB\VID_045E&PID_0C2E&MI_02
            //
            // Fastboot           = "Surface Duo Fastboot"
            // Adb                = "Surface Duo ADB"
            // AdbSideload        = "Surface Duo ADB Sideload"
            // AdbComposite       = "Surface Duo Composite ADB"
            // AdbCompositeTether = "Surface Duo Composite ADB Tether"
            // AdbCompositeFT     = "Surface Duo Composite ADB File Transfer"
            // AdbCompositePTP    = "Surface Duo Composite ADB PTP"
            // AdbCompositeMIDI   = "Surface Duo Composite ADB MIDI"

            bool IsInterfaceEnabled = args.IsEnabled;

            if (args.Name == "Surface Duo ADB" && args.Id.StartsWith(@"\\?\USB#VID_045E&PID_0C26"))
            {
                device.ADBID = args.Id;
                device.Information = args;
                device.SerialNumber = GetSerialNumberFromUSBID(args.Id);

                if (IsInterfaceEnabled)
                {
                    Thread.Sleep(1000);

                    switch (ADBProcedures.GetDeviceProductDevice())
                    {
                        case "duo":
                            {
                                device.Name = "Surface Duo";
                                string productName = ADBProcedures.GetDeviceProductName();
                                switch (productName)
                                {
                                    case "duo":
                                        {
                                            device.Variant = "GEN";
                                            break;
                                        }
                                    case "duo-att":
                                        {
                                            device.Variant = "ATT";
                                            break;
                                        }
                                    case "duo-eu":
                                        {
                                            device.Variant = "EEA";
                                            break;
                                        }
                                    default:
                                        {
                                            device.Variant = productName;
                                            break;
                                        }
                                }
                                device.Product = Device.DeviceProduct.Epsilon;
                                device.State = Device.DeviceStateEnum.ANDROID_ADB_ENABLED;

                                DeviceConnectedEvent?.Invoke(null, device);
                                break;
                            }
                        case "duo2":
                            {
                                device.Name = "Surface Duo 2";
                                device.Variant = "";
                                device.Product = Device.DeviceProduct.Zeta;
                                device.State = Device.DeviceStateEnum.ANDROID_ADB_ENABLED;

                                DeviceConnectedEvent?.Invoke(null, device);
                                break;
                            }
                    }
                }
            }
            else if (args.Name == "Surface Duo Fastboot" && args.Id.StartsWith(@"\\?\USB#VID_045E&PID_0C2F"))
            {
                device.BootloaderID = args.Id;
                device.Information = args;
                device.SerialNumber = GetSerialNumberFromUSBID(args.Id);

                if (IsInterfaceEnabled)
                {
                    Thread.Sleep(1000);

                    if (Device.FastBootTransport != null)
                    {
                        Device.FastBootTransport.Dispose();
                        Device.FastBootTransport.Close();
                    }

                    device.FastBootTransport = new FastBootTransport(device.BootloaderID);

                    switch (FastbootProcedures.GetProduct())
                    {
                        case "surfaceduo":
                        case "duo":
                            {
                                device.Name = "Surface Duo";
                                device.Variant = "";
                                device.Product = Device.DeviceProduct.Epsilon;
                                break;
                            }
                        case "surfaceduo2":
                        case "duo2":
                            {
                                device.Name = "Surface Duo 2";
                                device.Variant = "";
                                device.Product = Device.DeviceProduct.Zeta;
                                break;
                            }
                    }

                    device.FastBootTransport.GetVariable("is-userspace", out string isUserSpaceString);
                    if (isUserSpaceString == "yes")
                    {
                        device.State = Device.DeviceStateEnum.FASTBOOTD;
                    }
                    else
                    {
                        device.State = Device.DeviceStateEnum.BOOTLOADER;
                    }

                    DeviceConnectedEvent?.Invoke(null, device);
                }
            }
            else if (args.Name == "MTP" && args.Id.StartsWith(@"\\?\USB#VID_05C6&PID_9039&MI_00"))
            {
                device.TWRPID = args.Id;
                device.Information = args;
                device.SerialNumber = GetSerialNumberFromUSBID(args.Id);

                if (IsInterfaceEnabled)
                {
                    Thread.Sleep(1000);

                    device.Name = "Surface Duo";
                    device.Variant = "";
                    device.Product = Device.DeviceProduct.Epsilon;
                    device.State = Device.DeviceStateEnum.TWRP;

                    DeviceConnectedEvent?.Invoke(null, device);
                }
            }
            else if (args.Name == "ASUS_I006D" && args.Id.StartsWith(@"\\?\USB#VID_18D1&PID_D001"))
            {
                device.TWRPID = args.Id;
                device.Information = args;
                device.SerialNumber = GetSerialNumberFromUSBID(args.Id);

                if (IsInterfaceEnabled)
                {
                    Thread.Sleep(1000);

                    device.Name = "Surface Duo 2";
                    device.Variant = "";
                    device.Product = Device.DeviceProduct.Zeta;
                    device.State = Device.DeviceStateEnum.TWRP;

                    DeviceConnectedEvent?.Invoke(null, device);
                }
            }
        }

        private static string GetSerialNumberFromUSBID(string USB)
        {
            string pattern = @"#(\d+)#";
            Match match = Regex.Match(USB, pattern);
            return match.Groups[1].Value;
        }

        private void DeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            if ((args.Id.Equals(device.ADBID) && device.State == Device.DeviceStateEnum.ANDROID_ADB_ENABLED)
                || (args.Id.Equals(device.TWRPID) && device.State == Device.DeviceStateEnum.TWRP)
                || (args.Id.Equals(device.BootloaderID) && device.State == Device.DeviceStateEnum.BOOTLOADER)
                || (args.Id.Equals(device.BootloaderID) && device.State == Device.DeviceStateEnum.FASTBOOTD))
            {
                if (Device.FastBootTransport != null)
                {
                    Device.FastBootTransport.Dispose();
                    Device.FastBootTransport.Close();
                }

                device.LastInformationUpdate = args;
                device.State = Device.DeviceStateEnum.DISCONNECTED;

                DeviceDisconnectedEvent?.Invoke(sender, device);
            }
        }
    }
}
