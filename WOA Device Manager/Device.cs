using FastBoot;
using SAPTeam.AndroCtrl.Adb;
using Windows.Devices.Enumeration;
using WOADeviceManager.Helpers;

namespace WOADeviceManager
{
    public class Device
    {
        public enum DeviceStateEnum
        {
            ANDROID, ANDROID_ADB_ENABLED, BOOTLOADER, RECOVERY, FASTBOOTD, TWRP, UFP, WINDOWS, DISCONNECTED
        }

        public enum OEMUnlockStateEnum
        {
            UNLOCKED, LOCKED, UNKNOWN
        }

        public enum DeviceProduct
        {
            Epsilon,
            Zeta
        }

        public Device()
        {
        }

        public string ID
        {
            get; set;
        }
        public string Name
        {
            get; set;
        }
        public string Variant
        {
            get; set;
        }
        public string SerialNumber
        {
            get; set;
        }
        public DeviceProduct Product
        {
            get; set;
        }
        public DeviceStateEnum State { get; set; } = DeviceStateEnum.DISCONNECTED;
        public OEMUnlockStateEnum OEMUnlockState
        {
            get; set;
        }
        public DeviceInformation Information
        {
            get; set;
        }
        public DeviceInformationUpdate LastInformationUpdate
        {
            get; set;
        }

        public DeviceData AndroidDebugBridgeTransport
        {
            get; set;
        }

        public FastBootTransport FastBootTransport
        {
            get; set;
        }

        public string ADBID
        {
            get; set;
        }

        public string BootloaderID
        {
            get; set;
        }

        public string TWRPID
        {
            get; set;
        }

        public string BatteryLevel => State == DeviceStateEnum.ANDROID_ADB_ENABLED
                    ? ADBProcedures.GetDeviceBatteryLevel()
                    : State is DeviceStateEnum.FASTBOOTD or DeviceStateEnum.BOOTLOADER ? FastbootProcedures.GetDeviceBatteryLevel() : null;

        public string DeviceStateLocalized => State switch
        {
            DeviceStateEnum.ANDROID => "Android",
            DeviceStateEnum.ANDROID_ADB_ENABLED => "Android (ADB Connected)",
            DeviceStateEnum.WINDOWS => "Windows",
            DeviceStateEnum.BOOTLOADER => "Bootloader",
            DeviceStateEnum.FASTBOOTD => "Fastboot",
            DeviceStateEnum.TWRP => "TWRP",
            DeviceStateEnum.DISCONNECTED => "Disconnected",
            _ => null,
        };

        public bool IsConnected => State switch
        {
            DeviceStateEnum.DISCONNECTED => false,
            _ => true,
        };

        public bool IsDisconnected => !IsConnected;

        public bool ADBConnected => State == DeviceStateEnum.ANDROID_ADB_ENABLED;

        public bool ADBORTWRPConnected => State is DeviceStateEnum.ANDROID_ADB_ENABLED or DeviceStateEnum.TWRP;

        public bool TWRPConnected => State == DeviceStateEnum.TWRP;

        public bool FastbootConnected => State == DeviceStateEnum.FASTBOOTD;

        public bool BootloaderConnected => State == DeviceStateEnum.BOOTLOADER;
    }
}
