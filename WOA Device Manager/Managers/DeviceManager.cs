using AndroidDebugBridge;
using FastBoot;
using MadWizard.WinUSBNet;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using Windows.Devices.Enumeration;

namespace WOADeviceManager.Managers
{
    public class DeviceManager
    {
        private readonly DeviceWatcher watcher;

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

            watcher = DeviceInformation.CreateWatcher();
            watcher.Added += DeviceAdded;
            watcher.Removed += DeviceRemoved;
            watcher.Updated += Watcher_Updated;
            watcher.Start();
        }

        private async void Watcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            _ = args.Properties.TryGetValue("System.Devices.InterfaceEnabled", out object? IsInterfaceEnabledObjectValue);
            bool IsInterfaceEnabled = (bool?)IsInterfaceEnabledObjectValue ?? false;

            if (!IsInterfaceEnabled && args.Id == Device.ID)
            {
                Debug.WriteLine("Device Disconnected!");
                Debug.WriteLine($"Device path: {Device.ID}");
                Debug.WriteLine($"Name: {Device.Name}");
                Debug.WriteLine($"Variant: {Device.Variant}");
                Debug.WriteLine($"Product: {Device.Product}");
                Debug.WriteLine($"State: {Device.DeviceStateLocalized}");

                Device.State = Device.DeviceStateEnum.DISCONNECTED;
                Device.ID = null;
                Device.Name = null;
                Device.Variant = null;
                // TODO: Device.Product = Device.Product;

                DeviceDisconnectedEvent?.Invoke(this, device);
                return;
            }
            else if (!IsInterfaceEnabled)
            {
                return;
            }

            DeviceInformation deviceInformation = null;
            DeviceInformationCollection allDevices = await DeviceInformation.FindAllAsync();
            foreach (DeviceInformation _deviceInformation in allDevices)
            {
                if (_deviceInformation.Id == args.Id)
                {
                    deviceInformation = _deviceInformation;
                }
            }

            string ID = args.Id;
            string Name = deviceInformation != null ? deviceInformation.Name : "N/A";

            HandleDevice(ID, Name);
        }

        private void DeviceAdded(DeviceWatcher sender, DeviceInformation args)
        {
            bool IsInterfaceEnabled = args.IsEnabled;
            if (!IsInterfaceEnabled)
            {
                return;
            }

            string ID = args.Id;
            string Name = args.Name;

            HandleDevice(ID, Name);
        }

        private void HandleDevice(string ID, string Name)
        {
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

            if (ID.Contains("USBSTOR#Disk&Ven_Linux&Prod_File-Stor_Gadget&Rev_0414#"))
            {
                Device.State = Device.DeviceStateEnum.TWRP_MASS_STORAGE;
                // No ID, to be filled later
                Device.Product = Device.DeviceProduct.Epsilon;
                Device.Name = "Surface Duo";
                Device.Variant = "N/A";

                Debug.WriteLine("New Device Found!");
                Debug.WriteLine($"Device path: {Device.ID}");
                Debug.WriteLine($"Name: {Device.Name}");
                Debug.WriteLine($"Variant: {Device.Variant}");
                Debug.WriteLine($"Product: {Device.Product}");
                Debug.WriteLine($"State: {Device.DeviceStateLocalized}");

                DeviceConnectedEvent?.Invoke(this, device);
                return;
            }
            else if (ID.Contains("USBSTOR#Disk&Ven_Linux&Prod_File-Stor_Gadget&Rev_0504#"))
            {
                Device.State = Device.DeviceStateEnum.TWRP_MASS_STORAGE;
                // No ID, to be filled later
                Device.Product = Device.DeviceProduct.Zeta;
                Device.Name = "Surface Duo 2";
                Device.Variant = "N/A";

                Debug.WriteLine("New Device Found!");
                Debug.WriteLine($"Device path: {Device.ID}");
                Debug.WriteLine($"Name: {Device.Name}");
                Debug.WriteLine($"Variant: {Device.Variant}");
                Debug.WriteLine($"Product: {Device.Product}");
                Debug.WriteLine($"State: {Device.DeviceStateLocalized}");

                DeviceConnectedEvent?.Invoke(this, device);
                return;
            }
            else if (ID.Contains("VID_045E&PID_0C2A&MI_00"))
            {
                Device.State = Device.DeviceStateEnum.WINDOWS;
                Device.ID = ID;
                Device.Name = Name;
                Device.Variant = "N/A";
                Device.Product = Name.Contains("Duo 2") ? Device.DeviceProduct.Zeta : Device.DeviceProduct.Epsilon;

                Debug.WriteLine("New Device Found!");
                Debug.WriteLine($"Device path: {Device.ID}");
                Debug.WriteLine($"Name: {Device.Name}");
                Debug.WriteLine($"Variant: {Device.Variant}");
                Debug.WriteLine($"Product: {Device.Product}");
                Debug.WriteLine($"State: {Device.DeviceStateLocalized}");

                DeviceConnectedEvent?.Invoke(this, device);
            }
            // Normal:
            // Surface Duo Fastboot
            else if (ID.Contains("USB#VID_045E&PID_0C2F#"))
            {
                try
                {
                    FastBootTransport fastBootTransport = new(ID);

                    bool result = fastBootTransport.GetVariable("product", out string productGetVar);
                    string ProductName = !result ? null : productGetVar;
                    result = fastBootTransport.GetVariable("is-userspace", out productGetVar);
                    string IsUserSpace = !result ? null : productGetVar;

                    switch (ProductName)
                    {
                        case "surfaceduo":
                        case "duo":
                            {
                                if (IsUserSpace == "yes")
                                {
                                    Device.State = Device.DeviceStateEnum.FASTBOOTD;
                                }
                                else
                                {
                                    Device.State = Device.DeviceStateEnum.BOOTLOADER;
                                }
                                Device.ID = ID;
                                Device.Name = "Surface Duo";
                                Device.Variant = "N/A";
                                Device.Product = Device.DeviceProduct.Epsilon;

                                if (Device.FastBootTransport != null && Device.FastBootTransport != fastBootTransport)
                                {
                                    Device.FastBootTransport.Dispose();
                                    Device.FastBootTransport = fastBootTransport;
                                }
                                else if (Device.FastBootTransport == null)
                                {
                                    Device.FastBootTransport = fastBootTransport;
                                }

                                Debug.WriteLine("New Device Found!");
                                Debug.WriteLine($"Device path: {Device.ID}");
                                Debug.WriteLine($"Name: {Device.Name}");
                                Debug.WriteLine($"Variant: {Device.Variant}");
                                Debug.WriteLine($"Product: {Device.Product}");
                                Debug.WriteLine($"State: {Device.DeviceStateLocalized}");

                                DeviceConnectedEvent?.Invoke(this, device);
                                return;
                            }
                        case "surfaceduo2":
                        case "duo2":
                            {
                                if (IsUserSpace == "yes")
                                {
                                    Device.State = Device.DeviceStateEnum.FASTBOOTD;
                                }
                                else
                                {
                                    Device.State = Device.DeviceStateEnum.BOOTLOADER;
                                }
                                Device.ID = ID;
                                Device.Name = "Surface Duo 2";
                                Device.Variant = "N/A";
                                Device.Product = Device.DeviceProduct.Zeta;

                                if (Device.FastBootTransport != null && Device.FastBootTransport != fastBootTransport)
                                {
                                    Device.FastBootTransport.Dispose();
                                    Device.FastBootTransport = fastBootTransport;
                                }
                                else if (Device.FastBootTransport == null)
                                {
                                    Device.FastBootTransport = fastBootTransport;
                                }

                                Debug.WriteLine("New Device Found!");
                                Debug.WriteLine($"Device path: {Device.ID}");
                                Debug.WriteLine($"Name: {Device.Name}");
                                Debug.WriteLine($"Variant: {Device.Variant}");
                                Debug.WriteLine($"Product: {Device.Product}");
                                Debug.WriteLine($"State: {Device.DeviceStateLocalized}");

                                DeviceConnectedEvent?.Invoke(this, device);
                                return;
                            }
                    }

                    fastBootTransport.Dispose();
                }
                catch { }
            }
            // Normal:
            // Surface Duo ADB
            // Surface Duo ADB Sideload
            // Surface Duo Composite ADB
            // Surface Duo Composite ADB Tether
            // Surface Duo Composite ADB File Transfer
            // Surface Duo Composite ADB PTP
            // Surface Duo Composite ADB MIDI
            //
            // Custom:
            // Surface Duo TWRP
            // Surface Duo 2 TWRP
            else if ((ID.Contains("USB#VID_045E&PID_0C26#") ||
             ID.Contains("USB#VID_045E&PID_0C30#") ||
             ID.Contains("USB#VID_045E&PID_0C26&MI_01#") ||
             ID.Contains("USB#VID_045E&PID_0C28&MI_02#") ||
             ID.Contains("USB#VID_045E&PID_0C2A&MI_01#") ||
             ID.Contains("USB#VID_045E&PID_0C2C&MI_01#") ||
             ID.Contains("USB#VID_045E&PID_0C2E&MI_02#") ||
             ID.Contains("USB#VID_05C6&PID_9039") ||
             ID.Contains("USB#VID_18D1&PID_D001")) && ID.Contains("{dee824ef-729b-4a0e-9c14-b7117d33a817}"))
            {
                Thread.Sleep(1000);
                try
                {
                    AndroidDebugBridgeTransport androidDebugBridgeTransport;
                    if (ID == Device.ID && Device.AndroidDebugBridgeTransport != null)
                    {
                        androidDebugBridgeTransport = Device.AndroidDebugBridgeTransport;
                    }
                    else
                    {
                        androidDebugBridgeTransport = new(ID);
                        androidDebugBridgeTransport.Connect();
                        androidDebugBridgeTransport.WaitTilConnected();
                        if (androidDebugBridgeTransport.PhoneConnectionEnvironment == "")
                        {
                            return;
                        }
                    }

                    if (ID.Contains("USB#VID_05C6&PID_9039"))
                    {
                        if (Device.State != Device.DeviceStateEnum.TWRP_MASS_STORAGE)
                        {
                            Device.State = Device.DeviceStateEnum.TWRP;
                        }
                        Device.ID = ID;
                        Device.Name = "Surface Duo";
                        Device.Variant = "N/A";
                        Device.Product = Device.DeviceProduct.Epsilon;

                        if (Device.AndroidDebugBridgeTransport != null && Device.AndroidDebugBridgeTransport != androidDebugBridgeTransport)
                        {
                            Device.AndroidDebugBridgeTransport.Dispose();
                            Device.AndroidDebugBridgeTransport = androidDebugBridgeTransport;
                        }
                        else if (Device.AndroidDebugBridgeTransport == null)
                        {
                            Device.AndroidDebugBridgeTransport = androidDebugBridgeTransport;
                        }

                        Debug.WriteLine("New Device Found!");
                        Debug.WriteLine($"Device path: {Device.ID}");
                        Debug.WriteLine($"Name: {Device.Name}");
                        Debug.WriteLine($"Variant: {Device.Variant}");
                        Debug.WriteLine($"Product: {Device.Product}");
                        Debug.WriteLine($"State: {Device.DeviceStateLocalized}");

                        DeviceConnectedEvent?.Invoke(this, device);
                        return;
                    }
                    else if (ID.Contains("USB#VID_18D1&PID_D001"))
                    {
                        if (Device.State != Device.DeviceStateEnum.TWRP_MASS_STORAGE)
                        {
                            Device.State = Device.DeviceStateEnum.TWRP;
                        }
                        Device.ID = ID;
                        Device.Name = "Surface Duo 2";
                        Device.Variant = "N/A";
                        Device.Product = Device.DeviceProduct.Zeta;

                        if (Device.AndroidDebugBridgeTransport != null && Device.AndroidDebugBridgeTransport != androidDebugBridgeTransport)
                        {
                            Device.AndroidDebugBridgeTransport.Dispose();
                            Device.AndroidDebugBridgeTransport = androidDebugBridgeTransport;
                        }
                        else if (Device.AndroidDebugBridgeTransport == null)
                        {
                            Device.AndroidDebugBridgeTransport = androidDebugBridgeTransport;
                        }

                        Debug.WriteLine("New Device Found!");
                        Debug.WriteLine($"Device path: {Device.ID}");
                        Debug.WriteLine($"Name: {Device.Name}");
                        Debug.WriteLine($"Variant: {Device.Variant}");
                        Debug.WriteLine($"Product: {Device.Product}");
                        Debug.WriteLine($"State: {Device.DeviceStateLocalized}");

                        DeviceConnectedEvent?.Invoke(this, device);
                        return;
                    }
                    else
                    {
                        string ProductDevice = "duo";
                        if (androidDebugBridgeTransport.PhoneConnectionVariables.ContainsKey("ro.product.device"))
                        {
                            ProductDevice = androidDebugBridgeTransport.PhoneConnectionVariables["ro.product.device"];
                        }

                        switch (ProductDevice)
                        {
                            case "duo":
                                {
                                    if (androidDebugBridgeTransport.PhoneConnectionEnvironment == "recovery")
                                    {
                                        Device.State = Device.DeviceStateEnum.RECOVERY;
                                    }
                                    else if (androidDebugBridgeTransport.PhoneConnectionEnvironment == "sideload")
                                    {
                                        Device.State = Device.DeviceStateEnum.SIDELOAD;
                                    }
                                    else if (androidDebugBridgeTransport.PhoneConnectionEnvironment == "device")
                                    {
                                        Device.State = Device.DeviceStateEnum.ANDROID_ADB_ENABLED;
                                    }
                                    else
                                    {
                                        Device.State = Device.DeviceStateEnum.ANDROID_ADB_ENABLED;
                                    }
                                    Device.ID = ID;
                                    Device.Name = "Surface Duo";

                                    string ProductName = "N/A";
                                    if (androidDebugBridgeTransport.PhoneConnectionVariables.ContainsKey("ro.product.name"))
                                    {
                                        ProductName = androidDebugBridgeTransport.PhoneConnectionVariables["ro.product.name"];
                                    }

                                    switch (ProductName)
                                    {
                                        case "duo":
                                            {
                                                Device.Variant = "GEN";
                                                break;
                                            }
                                        case "duo-att":
                                            {
                                                Device.Variant = "ATT";
                                                break;
                                            }
                                        case "duo-eu":
                                            {
                                                Device.Variant = "EEA";
                                                break;
                                            }
                                        default:
                                            {
                                                Device.Variant = ProductName;
                                                break;
                                            }
                                    }

                                    Device.Product = Device.DeviceProduct.Epsilon;

                                    if (Device.AndroidDebugBridgeTransport != null && Device.AndroidDebugBridgeTransport != androidDebugBridgeTransport)
                                    {
                                        Device.AndroidDebugBridgeTransport.Dispose();
                                        Device.AndroidDebugBridgeTransport = androidDebugBridgeTransport;
                                    }
                                    else if (Device.AndroidDebugBridgeTransport == null)
                                    {
                                        Device.AndroidDebugBridgeTransport = androidDebugBridgeTransport;
                                    }

                                    Debug.WriteLine("New Device Found!");
                                    Debug.WriteLine($"Device path: {Device.ID}");
                                    Debug.WriteLine($"Name: {Device.Name}");
                                    Debug.WriteLine($"Variant: {Device.Variant}");
                                    Debug.WriteLine($"Product: {Device.Product}");
                                    Debug.WriteLine($"State: {Device.DeviceStateLocalized}");

                                    DeviceConnectedEvent?.Invoke(this, device);
                                    return;
                                }
                            case "duo2":
                                {
                                    if (androidDebugBridgeTransport.PhoneConnectionEnvironment == "recovery")
                                    {
                                        Device.State = Device.DeviceStateEnum.RECOVERY;
                                    }
                                    else if (androidDebugBridgeTransport.PhoneConnectionEnvironment == "sideload")
                                    {
                                        Device.State = Device.DeviceStateEnum.SIDELOAD;
                                    }
                                    else
                                    {
                                        Device.State = Device.DeviceStateEnum.ANDROID_ADB_ENABLED;
                                    }

                                    Device.ID = ID;
                                    Device.Name = "Surface Duo 2";
                                    Device.Variant = "N/A";
                                    Device.Product = Device.DeviceProduct.Zeta;

                                    if (Device.AndroidDebugBridgeTransport != null && Device.AndroidDebugBridgeTransport != androidDebugBridgeTransport)
                                    {
                                        Device.AndroidDebugBridgeTransport.Dispose();
                                        Device.AndroidDebugBridgeTransport = androidDebugBridgeTransport;
                                    }
                                    else if (Device.AndroidDebugBridgeTransport == null)
                                    {
                                        Device.AndroidDebugBridgeTransport = androidDebugBridgeTransport;
                                    }

                                    Debug.WriteLine("New Device Found!");
                                    Debug.WriteLine($"Device path: {Device.ID}");
                                    Debug.WriteLine($"Name: {Device.Name}");
                                    Debug.WriteLine($"Variant: {Device.Variant}");
                                    Debug.WriteLine($"Product: {Device.Product}");
                                    Debug.WriteLine($"State: {Device.DeviceStateLocalized}");

                                    DeviceConnectedEvent?.Invoke(this, device);
                                    return;
                                }
                        }
                    }
                }
                catch { }
            }
            else if (ID.Contains("USB#VID_045E&PID_0C29"))
            {
                Device.State = Device.DeviceStateEnum.ANDROID;
                Device.ID = ID;
                Device.Name = Name;
                Device.Variant = "N/A";
                Device.Product = Name.Contains("Duo 2") ? Device.DeviceProduct.Zeta : Device.DeviceProduct.Epsilon;

                Debug.WriteLine("New Device Found!");
                Debug.WriteLine($"Device path: {Device.ID}");
                Debug.WriteLine($"Name: {Device.Name}");
                Debug.WriteLine($"Variant: {Device.Variant}");
                Debug.WriteLine($"Product: {Device.Product}");
                Debug.WriteLine($"State: {Device.DeviceStateLocalized}");

                DeviceConnectedEvent?.Invoke(this, device);
            }
        }

        private void DeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            if (Device.ID != args.Id)
            {
                return;
            }

            Debug.WriteLine("Device Disconnected!");
            Debug.WriteLine($"Device path: {Device.ID}");
            Debug.WriteLine($"Name: {Device.Name}");
            Debug.WriteLine($"Variant: {Device.Variant}");
            Debug.WriteLine($"Product: {Device.Product}");
            Debug.WriteLine($"State: {Device.DeviceStateLocalized}");

            Device.State = Device.DeviceStateEnum.DISCONNECTED;
            Device.ID = null;
            Device.Name = null;
            Device.Variant = null;
            // TODO: Device.Product = Device.Product;

            if (Device.FastBootTransport != null)
            {
                Device.FastBootTransport.Dispose();
                Device.FastBootTransport = null;
            }

            if (Device.AndroidDebugBridgeTransport != null)
            {
                Device.AndroidDebugBridgeTransport.Dispose();
            }

            DeviceDisconnectedEvent?.Invoke(this, device);
        }
    }
}
