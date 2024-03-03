using AndroidDebugBridge;
using FastBoot;
using UnifiedFlashingPlatform;
using WOADeviceManager.Helpers;

namespace WOADeviceManager
{
    public class Device
    {
        public enum DeviceStateEnum
        {
            ANDROID, ANDROID_ADB_ENABLED, BOOTLOADER, RECOVERY, SIDELOAD, FASTBOOTD, TWRP, TWRP_MASS_STORAGE, UFP, WINDOWS, DISCONNECTED
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

        public bool IsADBCompatible => State is DeviceStateEnum.ANDROID_ADB_ENABLED or DeviceStateEnum.TWRP or DeviceStateEnum.TWRP_MASS_STORAGE or DeviceStateEnum.RECOVERY or DeviceStateEnum.SIDELOAD;
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
            DeviceStateEnum.SIDELOAD => "Sideload",
            DeviceStateEnum.TWRP => "TWRP",
            DeviceStateEnum.TWRP_MASS_STORAGE => "TWRP (Mass Storage Connected)",
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

        public bool MassStorageConnected => State == DeviceStateEnum.TWRP_MASS_STORAGE;

        public bool FastBootDConnected => State == DeviceStateEnum.FASTBOOTD;

        public bool RecoveryConnected => State == DeviceStateEnum.RECOVERY;

        public bool SideloadConnected => State == DeviceStateEnum.SIDELOAD;

        public bool BootloaderConnected => State == DeviceStateEnum.BOOTLOADER;

        public bool WindowsConnected => State == DeviceStateEnum.WINDOWS;

        public bool UFPConnected => State == DeviceStateEnum.UFP;

        public FastBootTransport FastBootTransport
        {
            get; internal set;
        }

        public AndroidDebugBridgeTransport AndroidDebugBridgeTransport
        {
            get; internal set;
        }

        public UnifiedFlashingPlatformTransport UnifiedFlashingPlatformTransport
        {
            get; internal set;
        }
    }
}
