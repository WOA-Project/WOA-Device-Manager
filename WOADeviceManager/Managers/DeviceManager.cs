using AndroidDebugBridge;
using FastBoot;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnifiedFlashingPlatform;
using Windows.Devices.Enumeration;
using WOADeviceManager.Managers.Connectivity;

namespace WOADeviceManager.Managers
{
    public class DeviceManager
    {
        private const string OEMEP_MassStorage_LinuxGadget_USBID = "USBSTOR#Disk&Ven_Linux&Prod_File-Stor_Gadget&Rev_0414#";
        private const string OEMZE_MassStorage_LinuxGadget_USBID = "USBSTOR#Disk&Ven_Linux&Prod_File-Stor_Gadget&Rev_0504#";
        private const string WINDOWS_USBID = "VID_045E&PID_0C2A&MI_00";
        private const string UFP_USBID = "USB#VID_045E&PID_066B#";
        private const string FASTBOOT_USBID = "USB#VID_045E&PID_0C2F#";
        private const string ANDROID_USBID = "USB#VID_045E&PID_0C29";
        private const string OEMEP_TWRP_USBID = "USB#VID_05C6&PID_9039";
        private const string OEMZE_TWRP_USBID = "USB#VID_18D1&PID_D001";

        private const string OEMEP_PLATFORMID = "Microsoft Corporation.Surface.Surface Duo.1930";
        private const string OEMZE_MMWAVE_PLATFORMID = "Microsoft Corporation.Surface.Surface Duo 2.1995";
        private const string OEMZE_NR_PLATFORMID = "Microsoft Corporation.Surface.Surface Duo 2.1968";

        private const string ADB_USB_INTERFACEGUID = "{dee824ef-729b-4a0e-9c14-b7117d33a817}";

        private const string OEMEP_FRIENDLY_NAME = "Surface Duo";
        private const string OEMZE_FRIENDLY_NAME = "Surface Duo 2";

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

            // Disconnection
            if (!IsInterfaceEnabled)
            {
                if (args.Id == Device.ID)
                {
                    NotifyDeviceDeparture();

                    if (Device.AndroidDebugBridgeTransport != null)
                    {
                        Device.AndroidDebugBridgeTransport.OnConnectionEstablished -= AndroidDebugBridgeTransport_OnConnectionEstablished;
                        Device.AndroidDebugBridgeTransport.Dispose();
                        Device.AndroidDebugBridgeTransport = null;
                    }

                    if (Device.FastBootTransport != null)
                    {
                        Device.FastBootTransport.Dispose();
                        Device.FastBootTransport = null;
                    }

                    if (Device.UnifiedFlashingPlatformTransport != null)
                    {
                        Device.UnifiedFlashingPlatformTransport.Dispose();
                        Device.UnifiedFlashingPlatformTransport = null;
                    }

                    Device.State = DeviceState.DISCONNECTED;
                    Device.ID = null;
                    Device.Name = null;
                    Device.Variant = null;
                    // TODO: Device.Product = Device.Product;
                }
                else if (args.Id == Device.MassStorageID)
                {
                    NotifyDeviceDeparture();

                    switch (Device.State)
                    {
                        case DeviceState.TWRP_MASS_STORAGE_ADB_ENABLED:
                            {
                                Device.State = DeviceState.TWRP_ADB_ENABLED;
                                break;
                            }
                    }

                    Device.MassStorageID = null;
                    Device.MassStorage.Dispose();
                    Device.MassStorage = null;

                    NotifyDeviceArrival();
                }

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

        private void NotifyDeviceArrival()
        {
            Device.JustDisconnected = false;

            Debug.WriteLine("New Device Found!");
            Debug.WriteLine($"Device path: {Device.ID}");
            Debug.WriteLine($"Name: {Device.Name}");
            Debug.WriteLine($"Variant: {Device.Variant}");
            Debug.WriteLine($"Product: {Device.Product}");
            Debug.WriteLine($"State: {Device.DeviceStateLocalized}");

            DeviceConnectedEvent?.Invoke(this, device);
        }

        private void NotifyDeviceDeparture()
        {
            Device.JustDisconnected = true;

            Debug.WriteLine("Device Disconnected!");
            Debug.WriteLine($"Device path: {Device.ID}");
            Debug.WriteLine($"Name: {Device.Name}");
            Debug.WriteLine($"Variant: {Device.Variant}");
            Debug.WriteLine($"Product: {Device.Product}");
            Debug.WriteLine($"State: {Device.DeviceStateLocalized}");

            DeviceDisconnectedEvent?.Invoke(this, device);
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

            if (ID.Contains(OEMEP_MassStorage_LinuxGadget_USBID))
            {
                if (Device.State != DeviceState.DISCONNECTED)
                {
                    NotifyDeviceDeparture();
                }

                if (Device.State == DeviceState.TWRP_ADB_ENABLED)
                {
                    Device.State = DeviceState.TWRP_MASS_STORAGE_ADB_ENABLED;
                }
                else
                {
                    Device.State = DeviceState.OFFLINE_CHARGING;
                }

                // No ID, to be filled later
                Device.MassStorageID = ID;
                Device.Product = DeviceProduct.Epsilon;
                Device.Name = OEMEP_FRIENDLY_NAME;
                Device.Variant = "N/A";
                Device.MassStorage = new Helpers.MassStorage(ID);

                NotifyDeviceArrival();
                return;
            }
            else if (ID.Contains(OEMZE_MassStorage_LinuxGadget_USBID))
            {
                if (Device.State != DeviceState.DISCONNECTED)
                {
                    NotifyDeviceDeparture();
                }

                if (Device.State == DeviceState.TWRP_ADB_ENABLED)
                {
                    Device.State = DeviceState.TWRP_MASS_STORAGE_ADB_ENABLED;
                }
                else
                {
                    Device.State = DeviceState.OFFLINE_CHARGING;
                }

                // No ID, to be filled later
                Device.MassStorageID = ID;
                Device.Product = DeviceProduct.Zeta;
                Device.Name = OEMZE_FRIENDLY_NAME;
                Device.Variant = "N/A";
                Device.MassStorage = new Helpers.MassStorage(ID);

                NotifyDeviceArrival();
                return;
            }
            else if (ID.Contains(WINDOWS_USBID))
            {
                if (Device.State != DeviceState.DISCONNECTED)
                {
                    NotifyDeviceDeparture();
                }

                Device.State = DeviceState.WINDOWS;
                Device.ID = ID;
                Device.Name = Name;
                Device.Variant = "N/A";
                Device.Product = Name.Contains("Duo 2") ? DeviceProduct.Zeta : DeviceProduct.Epsilon;

                NotifyDeviceArrival();
            }
            else if (ID.Contains(UFP_USBID))
            {
                try
                {
                    UnifiedFlashingPlatformTransport unifiedFlashingPlatformTransport;
                    if (ID == Device.ID && Device.UnifiedFlashingPlatformTransport != null)
                    {
                        unifiedFlashingPlatformTransport = Device.UnifiedFlashingPlatformTransport;
                    }
                    else
                    {
                        unifiedFlashingPlatformTransport = new(ID);
                    }

                    // Microsoft Corporation.Surface.Surface Duo.1930
                    // Microsoft Corporation.Surface.Surface Duo 2.1995
                    // Microsoft Corporation.Surface.Surface Duo 2.1968
                    string PlatformID = unifiedFlashingPlatformTransport.ReadDevicePlatformID();

                    switch (PlatformID)
                    {
                        case OEMEP_PLATFORMID:
                            {
                                if (Device.State != DeviceState.DISCONNECTED)
                                {
                                    NotifyDeviceDeparture();
                                }

                                Device.State = DeviceState.UFP;
                                Device.ID = ID;
                                Device.Name = OEMEP_FRIENDLY_NAME;
                                Device.Variant = "N/A";
                                Device.Product = DeviceProduct.Epsilon;

                                if (Device.UnifiedFlashingPlatformTransport != null && Device.UnifiedFlashingPlatformTransport != unifiedFlashingPlatformTransport)
                                {
                                    Device.UnifiedFlashingPlatformTransport.Dispose();
                                    Device.UnifiedFlashingPlatformTransport = unifiedFlashingPlatformTransport;
                                }
                                else if (Device.UnifiedFlashingPlatformTransport == null)
                                {
                                    Device.UnifiedFlashingPlatformTransport = unifiedFlashingPlatformTransport;
                                }

                                NotifyDeviceArrival();
                                return;
                            }
                        case OEMZE_MMWAVE_PLATFORMID:
                        case OEMZE_NR_PLATFORMID:
                            {
                                if (Device.State != DeviceState.DISCONNECTED)
                                {
                                    NotifyDeviceDeparture();
                                }

                                Device.State = DeviceState.UFP;
                                Device.ID = ID;
                                Device.Name = OEMZE_FRIENDLY_NAME;
                                Device.Variant = "N/A";
                                Device.Product = DeviceProduct.Zeta;

                                if (Device.UnifiedFlashingPlatformTransport != null && Device.UnifiedFlashingPlatformTransport != unifiedFlashingPlatformTransport)
                                {
                                    Device.UnifiedFlashingPlatformTransport.Dispose();
                                    Device.UnifiedFlashingPlatformTransport = unifiedFlashingPlatformTransport;
                                }
                                else if (Device.UnifiedFlashingPlatformTransport == null)
                                {
                                    Device.UnifiedFlashingPlatformTransport = unifiedFlashingPlatformTransport;
                                }

                                NotifyDeviceArrival();
                                return;
                            }
                    }

                    unifiedFlashingPlatformTransport.Dispose();
                } catch { }
            }
            // Normal:
            // Surface Duo Fastboot
            else if (ID.Contains(FASTBOOT_USBID))
            {
                try
                {
                    FastBootTransport fastBootTransport = new(ID);

                    bool result = fastBootTransport.GetVariable("product", out string productGetVar);
                    string ProductName = !result ? null : productGetVar;
                    result = fastBootTransport.GetVariable("is-userspace", out productGetVar);
                    string IsUserSpace = !result ? null : productGetVar;

                    // Attempt to retrieve the device type in bootloader mode
                    string DeviceVariant = "N/A";
                    if (IsUserSpace != "yes")
                    {
                        (FastBootStatus getBootPropResponseStatus, string getBootPropResponse, byte[] _)[] getBootPropResponses = fastBootTransport.SendCommand("oem get-boot-prop");
                        if (getBootPropResponses.Length > 0 && getBootPropResponses.Last().getBootPropResponseStatus == FastBootStatus.OKAY)
                        {
                            (FastBootStatus _, DeviceVariant, byte[] _) = getBootPropResponses[0];
                            DeviceVariant = DeviceVariant.Split('=').Last();

                            switch (DeviceVariant)
                            {
                                case "gen":
                                    {
                                        DeviceVariant = "GEN";
                                        break;
                                    }
                                case "att":
                                    {
                                        DeviceVariant = "ATT";
                                        break;
                                    }
                                case "eea":
                                    {
                                        DeviceVariant = "EEA";
                                        break;
                                    }
                            }
                        }
                    }

                    switch (ProductName)
                    {
                        case "surfaceduo":
                        case "duo":
                            {
                                if (Device.State != DeviceState.DISCONNECTED)
                                {
                                    NotifyDeviceDeparture();
                                }

                                if (IsUserSpace == "yes")
                                {
                                    Device.State = DeviceState.FASTBOOTD;
                                }
                                else
                                {
                                    Device.State = DeviceState.BOOTLOADER;
                                }

                                Device.ID = ID;
                                Device.Name = OEMEP_FRIENDLY_NAME;
                                Device.Variant = DeviceVariant;
                                Device.Product = DeviceProduct.Epsilon;

                                if (Device.FastBootTransport != null && Device.FastBootTransport != fastBootTransport)
                                {
                                    Device.FastBootTransport.Dispose();
                                    Device.FastBootTransport = fastBootTransport;
                                }
                                else if (Device.FastBootTransport == null)
                                {
                                    Device.FastBootTransport = fastBootTransport;
                                }

                                NotifyDeviceArrival();
                                return;
                            }
                        case "surfaceduo2":
                        case "duo2":
                            {
                                if (Device.State != DeviceState.DISCONNECTED)
                                {
                                    NotifyDeviceDeparture();
                                }

                                if (IsUserSpace == "yes")
                                {
                                    Device.State = DeviceState.FASTBOOTD;
                                }
                                else
                                {
                                    Device.State = DeviceState.BOOTLOADER;
                                }

                                Device.ID = ID;
                                Device.Name = OEMZE_FRIENDLY_NAME;
                                Device.Variant = DeviceVariant;
                                Device.Product = DeviceProduct.Zeta;

                                if (Device.FastBootTransport != null && Device.FastBootTransport != fastBootTransport)
                                {
                                    Device.FastBootTransport.Dispose();
                                    Device.FastBootTransport = fastBootTransport;
                                }
                                else if (Device.FastBootTransport == null)
                                {
                                    Device.FastBootTransport = fastBootTransport;
                                }

                                NotifyDeviceArrival();
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
             ID.Contains(OEMEP_TWRP_USBID) ||
             ID.Contains(OEMZE_TWRP_USBID)) && ID.Contains(ADB_USB_INTERFACEGUID))
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
                    }

                    if (androidDebugBridgeTransport.IsConnected)
                    {
                        HandleADBEnabledDevice(androidDebugBridgeTransport);
                    }
                    else
                    {
                        HandleADBDisabledDevice(androidDebugBridgeTransport);

                        // Request a connection
                        androidDebugBridgeTransport.OnConnectionEstablished += AndroidDebugBridgeTransport_OnConnectionEstablished;
                        androidDebugBridgeTransport.Connect();
                    }
                }
                catch { }
            }
            else if (ID.Contains(ANDROID_USBID))
            {
                if (Device.State != DeviceState.DISCONNECTED)
                {
                    NotifyDeviceDeparture();
                }

                Device.State = DeviceState.ANDROID;
                Device.ID = ID;
                Device.Name = Name;
                Device.Variant = "N/A";
                Device.Product = Name.Contains("Duo 2") ? DeviceProduct.Zeta : DeviceProduct.Epsilon;

                NotifyDeviceArrival();
            }
        }

        private void HandleADBEnabledDevice(AndroidDebugBridgeTransport androidDebugBridgeTransport)
        {
            if (!androidDebugBridgeTransport.IsConnected)
            {
                return;
            }

            string ID = androidDebugBridgeTransport.DevicePath;

            if (ID.Contains(OEMEP_TWRP_USBID))
            {
                if (Device.MassStorageID != null)
                {
                    Device.State = DeviceState.TWRP_MASS_STORAGE_ADB_ENABLED;
                }
                else
                {
                    Device.State = DeviceState.TWRP_ADB_ENABLED;
                }

                Device.ID = ID;
                Device.Name = OEMEP_FRIENDLY_NAME;

                string DeviceVariant = Device.AndroidDebugBridgeTransport.GetVariableValue("ro.boot.product.hardware.sku");
                switch (DeviceVariant)
                {
                    case "gen":
                        {
                            DeviceVariant = "GEN";
                            break;
                        }
                    case "att":
                        {
                            DeviceVariant = "ATT";
                            break;
                        }
                    case "eea":
                        {
                            DeviceVariant = "EEA";
                            break;
                        }
                    default:
                        {
                            DeviceVariant = "N/A";
                            break;
                        }
                }

                Device.Variant = DeviceVariant;

                Device.Product = DeviceProduct.Epsilon;

                if (Device.AndroidDebugBridgeTransport != null && Device.AndroidDebugBridgeTransport != androidDebugBridgeTransport)
                {
                    Device.AndroidDebugBridgeTransport.Dispose();
                    Device.AndroidDebugBridgeTransport = androidDebugBridgeTransport;
                }
                else if (Device.AndroidDebugBridgeTransport == null)
                {
                    Device.AndroidDebugBridgeTransport = androidDebugBridgeTransport;
                }

                NotifyDeviceArrival();
                return;
            }
            else if (ID.Contains(OEMZE_TWRP_USBID))
            {
                if (Device.MassStorageID != null)
                {
                    Device.State = DeviceState.TWRP_MASS_STORAGE_ADB_ENABLED;
                }
                else
                {
                    Device.State = DeviceState.TWRP_ADB_ENABLED;
                }

                Device.ID = ID;
                Device.Name = OEMZE_FRIENDLY_NAME;
                Device.Variant = "N/A";
                Device.Product = DeviceProduct.Zeta;

                if (Device.AndroidDebugBridgeTransport != null && Device.AndroidDebugBridgeTransport != androidDebugBridgeTransport)
                {
                    Device.AndroidDebugBridgeTransport.Dispose();
                    Device.AndroidDebugBridgeTransport = androidDebugBridgeTransport;
                }
                else if (Device.AndroidDebugBridgeTransport == null)
                {
                    Device.AndroidDebugBridgeTransport = androidDebugBridgeTransport;
                }

                NotifyDeviceArrival();
                return;
            }
            else
            {
                string ProductDevice = "duo";
                if (androidDebugBridgeTransport.GetPhoneConnectionVariables().ContainsKey("ro.product.device"))
                {
                    ProductDevice = androidDebugBridgeTransport.GetPhoneConnectionVariables()["ro.product.device"];
                }

                switch (ProductDevice)
                {
                    case "duo":
                        {
                            if (androidDebugBridgeTransport.GetPhoneConnectionEnvironment() == "recovery")
                            {
                                Device.State = DeviceState.RECOVERY_ADB_ENABLED;
                            }
                            else if (androidDebugBridgeTransport.GetPhoneConnectionEnvironment() == "sideload")
                            {
                                Device.State = DeviceState.SIDELOAD_ADB_ENABLED;
                            }
                            else if (androidDebugBridgeTransport.GetPhoneConnectionEnvironment() == "device")
                            {
                                Device.State = DeviceState.ANDROID_ADB_ENABLED;
                            }
                            else
                            {
                                Device.State = DeviceState.ANDROID_ADB_ENABLED;
                            }
                            Device.ID = ID;
                            Device.Name = OEMEP_FRIENDLY_NAME;

                            string ProductName = "N/A";
                            if (androidDebugBridgeTransport.GetPhoneConnectionVariables().ContainsKey("ro.product.name"))
                            {
                                ProductName = androidDebugBridgeTransport.GetPhoneConnectionVariables()["ro.product.name"];
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

                            Device.Product = DeviceProduct.Epsilon;

                            if (Device.AndroidDebugBridgeTransport != null && Device.AndroidDebugBridgeTransport != androidDebugBridgeTransport)
                            {
                                Device.AndroidDebugBridgeTransport.Dispose();
                                Device.AndroidDebugBridgeTransport = androidDebugBridgeTransport;
                            }
                            else if (Device.AndroidDebugBridgeTransport == null)
                            {
                                Device.AndroidDebugBridgeTransport = androidDebugBridgeTransport;
                            }

                            NotifyDeviceArrival();
                            return;
                        }
                    case "duo2":
                        {
                            if (androidDebugBridgeTransport.GetPhoneConnectionEnvironment() == "recovery")
                            {
                                Device.State = DeviceState.RECOVERY_ADB_ENABLED;
                            }
                            else if (androidDebugBridgeTransport.GetPhoneConnectionEnvironment() == "sideload")
                            {
                                Device.State = DeviceState.SIDELOAD_ADB_ENABLED;
                            }
                            else
                            {
                                Device.State = DeviceState.ANDROID_ADB_ENABLED;
                            }

                            Device.ID = ID;
                            Device.Name = OEMZE_FRIENDLY_NAME;
                            Device.Variant = "N/A";
                            Device.Product = DeviceProduct.Zeta;

                            if (Device.AndroidDebugBridgeTransport != null && Device.AndroidDebugBridgeTransport != androidDebugBridgeTransport)
                            {
                                Device.AndroidDebugBridgeTransport.Dispose();
                                Device.AndroidDebugBridgeTransport = androidDebugBridgeTransport;
                            }
                            else if (Device.AndroidDebugBridgeTransport == null)
                            {
                                Device.AndroidDebugBridgeTransport = androidDebugBridgeTransport;
                            }

                            NotifyDeviceArrival();
                            return;
                        }
                }
            }
        }

        private void HandleADBDisabledDevice(AndroidDebugBridgeTransport androidDebugBridgeTransport)
        {
            if (androidDebugBridgeTransport.IsConnected)
            {
                return;
            }

            string ID = androidDebugBridgeTransport.DevicePath;

            if (ID.Contains(OEMEP_TWRP_USBID))
            {
                if (Device.State != DeviceState.DISCONNECTED)
                {
                    NotifyDeviceDeparture();
                }

                if (Device.MassStorageID != null)
                {
                    Device.State = DeviceState.TWRP_MASS_STORAGE_ADB_DISABLED;
                }
                else
                {
                    Device.State = DeviceState.TWRP_ADB_DISABLED;
                }

                Device.ID = ID;
                Device.Name = OEMEP_FRIENDLY_NAME;
                Device.Variant = "N/A";
                Device.Product = DeviceProduct.Epsilon;

                if (Device.AndroidDebugBridgeTransport != null && Device.AndroidDebugBridgeTransport != androidDebugBridgeTransport)
                {
                    Device.AndroidDebugBridgeTransport.Dispose();
                    Device.AndroidDebugBridgeTransport = androidDebugBridgeTransport;
                }
                else if (Device.AndroidDebugBridgeTransport == null)
                {
                    Device.AndroidDebugBridgeTransport = androidDebugBridgeTransport;
                }

                NotifyDeviceArrival();
                return;
            }
            else if (ID.Contains(OEMZE_TWRP_USBID))
            {
                if (Device.State != DeviceState.DISCONNECTED)
                {
                    NotifyDeviceDeparture();
                }

                if (Device.MassStorageID != null)
                {
                    Device.State = DeviceState.TWRP_MASS_STORAGE_ADB_DISABLED;
                }
                else
                {
                    Device.State = DeviceState.TWRP_ADB_DISABLED;
                }

                Device.ID = ID;
                Device.Name = OEMZE_FRIENDLY_NAME;
                Device.Variant = "N/A";
                Device.Product = DeviceProduct.Zeta;

                if (Device.AndroidDebugBridgeTransport != null && Device.AndroidDebugBridgeTransport != androidDebugBridgeTransport)
                {
                    Device.AndroidDebugBridgeTransport.Dispose();
                    Device.AndroidDebugBridgeTransport = androidDebugBridgeTransport;
                }
                else if (Device.AndroidDebugBridgeTransport == null)
                {
                    Device.AndroidDebugBridgeTransport = androidDebugBridgeTransport;
                }

                NotifyDeviceArrival();
                return;
            }
            else
            {
                string ProductDevice = "duo";

                switch (ProductDevice)
                {
                    case "duo":
                        {
                            if (Device.State != DeviceState.DISCONNECTED)
                            {
                                NotifyDeviceDeparture();
                            }

                            Device.State = DeviceState.ANDROID_ADB_DISABLED;
                            Device.ID = ID;
                            Device.Name = OEMEP_FRIENDLY_NAME;
                            Device.Variant = "N/A";
                            Device.Product = DeviceProduct.Epsilon;

                            if (Device.AndroidDebugBridgeTransport != null && Device.AndroidDebugBridgeTransport != androidDebugBridgeTransport)
                            {
                                Device.AndroidDebugBridgeTransport.Dispose();
                                Device.AndroidDebugBridgeTransport = androidDebugBridgeTransport;
                            }
                            else if (Device.AndroidDebugBridgeTransport == null)
                            {
                                Device.AndroidDebugBridgeTransport = androidDebugBridgeTransport;
                            }

                            NotifyDeviceArrival();
                            return;
                        }
                    case "duo2":
                        {
                            if (Device.State != DeviceState.DISCONNECTED)
                            {
                                NotifyDeviceDeparture();
                            }

                            Device.State = DeviceState.ANDROID_ADB_DISABLED;

                            Device.ID = ID;
                            Device.Name = OEMZE_FRIENDLY_NAME;
                            Device.Variant = "N/A";
                            Device.Product = DeviceProduct.Zeta;

                            if (Device.AndroidDebugBridgeTransport != null && Device.AndroidDebugBridgeTransport != androidDebugBridgeTransport)
                            {
                                Device.AndroidDebugBridgeTransport.Dispose();
                                Device.AndroidDebugBridgeTransport = androidDebugBridgeTransport;
                            }
                            else if (Device.AndroidDebugBridgeTransport == null)
                            {
                                Device.AndroidDebugBridgeTransport = androidDebugBridgeTransport;
                            }

                            NotifyDeviceArrival();
                            return;
                        }
                }
            }
        }

        private void AndroidDebugBridgeTransport_OnConnectionEstablished(object sender, EventArgs e)
        {
            AndroidDebugBridgeTransport androidDebugBridgeTransport = (AndroidDebugBridgeTransport)sender;
            androidDebugBridgeTransport.OnConnectionEstablished -= AndroidDebugBridgeTransport_OnConnectionEstablished;
            HandleADBEnabledDevice(androidDebugBridgeTransport);
        }

        private void DeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            if (Device.ID != args.Id)
            {
                return;
            }

            NotifyDeviceDeparture();

            Device.State = DeviceState.DISCONNECTED;
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
                Device.AndroidDebugBridgeTransport = null;
            }

            if (Device.UnifiedFlashingPlatformTransport != null)
            {
                Device.UnifiedFlashingPlatformTransport.Dispose();
                Device.UnifiedFlashingPlatformTransport = null;
            }
        }
    }
}
