using FastBoot;
using SAPTeam.AndroCtrl.Adb;
using WOADeviceManager.Helpers;
using WOADeviceManager.Managers;

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

        public DeviceProduct Product
        {
            get; set;
        }

        public DeviceStateEnum State { get; set; } = DeviceStateEnum.DISCONNECTED;

        public OEMUnlockStateEnum OEMUnlockState
        {
            get; set;
        }

        public bool IsADBCompatible => State is DeviceStateEnum.ANDROID_ADB_ENABLED or DeviceStateEnum.TWRP or DeviceStateEnum.RECOVERY;
        public bool IsFastBootCompatible => State is DeviceStateEnum.FASTBOOTD or DeviceStateEnum.BOOTLOADER;

        public string BatteryLevel => IsADBCompatible
                    ? ADBProcedures.GetDeviceBatteryLevel()
                    : IsFastBootCompatible ? FastbootProcedures.GetDeviceBatteryLevel() : null;

        public string DeviceStateLocalized => State switch
        {
            DeviceStateEnum.ANDROID => "Android",
            DeviceStateEnum.ANDROID_ADB_ENABLED => "Android (ADB Connected)",
            DeviceStateEnum.WINDOWS => "Windows",
            DeviceStateEnum.BOOTLOADER => "Bootloader",
            DeviceStateEnum.FASTBOOTD => "Fastboot",
            DeviceStateEnum.RECOVERY => "Recovery",
            DeviceStateEnum.TWRP => "TWRP",
            DeviceStateEnum.UFP => "UFP",
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

        public bool TWRPConnected => State == DeviceStateEnum.TWRP;

        public bool FastBootDConnected => State == DeviceStateEnum.FASTBOOTD;

        public bool RecoveryConnected => State == DeviceStateEnum.RECOVERY;

        public bool BootloaderConnected => State == DeviceStateEnum.BOOTLOADER;

        public bool WindowsConnected => State == DeviceStateEnum.WINDOWS;

        public FastBootTransport FastBootTransport
        {
            get; internal set;
        }
    }
}
