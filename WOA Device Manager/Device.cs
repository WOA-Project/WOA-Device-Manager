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


        public string BatteryLevel => JustDisconnected ? null : (IsADBEnabled
                    ? ADBProcedures.GetDeviceBatteryLevel()
                    : IsFastBootEnabled ? FastBootProcedures.GetDeviceBatteryLevel() : null);


        public string DeviceStateLocalized => State switch
        {
            DeviceStateEnum.ANDROID_ADB_DISABLED => "Android (ADB Disconnected)",
            DeviceStateEnum.ANDROID_ADB_ENABLED => "Android (ADB Connected)",
            DeviceStateEnum.ANDROID => "Android",
            DeviceStateEnum.BOOTLOADER => "Bootloader",
            DeviceStateEnum.FASTBOOTD => "FastBoot",
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


        public bool IsFastBootEnabled => State is DeviceStateEnum.FASTBOOTD or DeviceStateEnum.BOOTLOADER;

        public bool IsADBDisabled => State is DeviceStateEnum.ANDROID_ADB_DISABLED or DeviceStateEnum.RECOVERY_ADB_DISABLED or DeviceStateEnum.SIDELOAD_ADB_DISABLED or DeviceStateEnum.TWRP_ADB_DISABLED or DeviceStateEnum.TWRP_MASS_STORAGE_ADB_DISABLED;

        public bool IsADBEnabled => State is DeviceStateEnum.ANDROID_ADB_ENABLED or DeviceStateEnum.RECOVERY_ADB_ENABLED or DeviceStateEnum.SIDELOAD_ADB_ENABLED or DeviceStateEnum.TWRP_ADB_ENABLED or DeviceStateEnum.TWRP_MASS_STORAGE_ADB_ENABLED;


        public bool IsInAndroid => State is DeviceStateEnum.ANDROID or DeviceStateEnum.ANDROID_ADB_DISABLED or DeviceStateEnum.ANDROID_ADB_ENABLED;

        public bool IsInTWRP => State is DeviceStateEnum.TWRP_ADB_DISABLED or DeviceStateEnum.TWRP_ADB_ENABLED or DeviceStateEnum.TWRP_MASS_STORAGE_ADB_DISABLED or DeviceStateEnum.TWRP_MASS_STORAGE_ADB_ENABLED;

        public bool IsInMassStorage => State is DeviceStateEnum.TWRP_MASS_STORAGE_ADB_DISABLED or DeviceStateEnum.TWRP_MASS_STORAGE_ADB_ENABLED;

        public bool IsInRecovery => State is DeviceStateEnum.RECOVERY_ADB_DISABLED or DeviceStateEnum.RECOVERY_ADB_ENABLED;

        public bool IsInSideload => State is DeviceStateEnum.SIDELOAD_ADB_DISABLED or DeviceStateEnum.SIDELOAD_ADB_ENABLED;

        public bool IsInFastBootD => State == DeviceStateEnum.FASTBOOTD;

        public bool IsInBootloader => State == DeviceStateEnum.BOOTLOADER;

        public bool IsInWindows => State == DeviceStateEnum.WINDOWS;

        public bool IsInUFP => State == DeviceStateEnum.UFP;


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
