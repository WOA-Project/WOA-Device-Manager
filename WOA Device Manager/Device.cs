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
            ANDROID_ADB_DISABLED,
            ANDROID_ADB_ENABLED,
            ANDROID,
            BOOTLOADER,
            FASTBOOTD,
            OFFLINE_CHARGING,
            RECOVERY_ADB_DISABLED,
            RECOVERY_ADB_ENABLED,
            SIDELOAD_ADB_DISABLED,
            SIDELOAD_ADB_ENABLED,
            TWRP_ADB_DISABLED,
            TWRP_ADB_ENABLED,
            TWRP_MASS_STORAGE_ADB_DISABLED,
            TWRP_MASS_STORAGE_ADB_ENABLED,
            UFP,
            WINDOWS,
            DISCONNECTED
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

        public string MassStorageID
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

        public bool IsADBCompatible => State is DeviceStateEnum.ANDROID_ADB_ENABLED or DeviceStateEnum.TWRP_ADB_ENABLED or DeviceStateEnum.TWRP_MASS_STORAGE_ADB_ENABLED or DeviceStateEnum.RECOVERY_ADB_ENABLED or DeviceStateEnum.SIDELOAD_ADB_ENABLED;
        public bool IsFastBootCompatible => State is DeviceStateEnum.FASTBOOTD or DeviceStateEnum.BOOTLOADER;

        public string BatteryLevel => IsADBCompatible
                    ? ADBProcedures.GetDeviceBatteryLevel()
                    : IsFastBootCompatible ? FastbootProcedures.GetDeviceBatteryLevel() : null;

        public string DeviceStateLocalized => State switch
        {
            DeviceStateEnum.ANDROID_ADB_DISABLED => "Android (ADB Disconnected)",
            DeviceStateEnum.ANDROID_ADB_ENABLED => "Android (ADB Connected)",
            DeviceStateEnum.ANDROID => "Android",
            DeviceStateEnum.BOOTLOADER => "Bootloader",
            DeviceStateEnum.FASTBOOTD => "Fastboot",
            DeviceStateEnum.OFFLINE_CHARGING => "Offline Charging",
            DeviceStateEnum.RECOVERY_ADB_DISABLED => "Recovery (ADB Disconnected)",
            DeviceStateEnum.RECOVERY_ADB_ENABLED => "Recovery (ADB Connected)",
            DeviceStateEnum.SIDELOAD_ADB_DISABLED => "Sideload (ADB Disconnected)",
            DeviceStateEnum.SIDELOAD_ADB_ENABLED => "Sideload (ADB Connected)",
            DeviceStateEnum.TWRP_ADB_DISABLED => "TWRP (ADB Disconnected)",
            DeviceStateEnum.TWRP_ADB_ENABLED => "TWRP (ADB Connected)",
            DeviceStateEnum.TWRP_MASS_STORAGE_ADB_DISABLED => "TWRP (Mass Storage Connected) (ADB Disconnected)",
            DeviceStateEnum.TWRP_MASS_STORAGE_ADB_ENABLED => "TWRP (Mass Storage Connected) (ADB Connected)",
            DeviceStateEnum.UFP => "UFP",
            DeviceStateEnum.WINDOWS => "Windows",
            DeviceStateEnum.DISCONNECTED => "Disconnected",
            _ => null,
        };

        public bool JustDisconnected = false;

        public bool IsConnected => State switch
        {
            DeviceStateEnum.DISCONNECTED => false,
            _ => !JustDisconnected,
        };

        public bool IsDisconnected => !IsConnected;

        public bool ADBConnected => State == DeviceStateEnum.ANDROID_ADB_ENABLED;

        public bool TWRPConnected => State == DeviceStateEnum.TWRP_ADB_ENABLED;

        public bool MassStorageConnected => State == DeviceStateEnum.TWRP_MASS_STORAGE_ADB_ENABLED;

        public bool FastBootDConnected => State == DeviceStateEnum.FASTBOOTD;

        public bool RecoveryConnected => State == DeviceStateEnum.RECOVERY_ADB_ENABLED;

        public bool SideloadConnected => State == DeviceStateEnum.SIDELOAD_ADB_ENABLED;

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
