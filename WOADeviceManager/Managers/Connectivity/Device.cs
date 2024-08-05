using AndroidDebugBridge;
using FastBoot;
using UnifiedFlashingPlatform;
using WOADeviceManager.Helpers;

namespace WOADeviceManager.Managers.Connectivity
{
    public partial class Device
    {
        public string? ID
        {
            get; set;
        }

        public string? MassStorageID
        {
            get; set;
        }

        public string? Name
        {
            get; set;
        }

        public string? Variant
        {
            get; set;
        }

        public DeviceProduct Product
        {
            get; set;
        }

        public DeviceState State { get; set; } = DeviceState.DISCONNECTED;

        public OEMUnlockState OEMUnlock => JustDisconnected ? OEMUnlockState.UNKNOWN : 
            IsADBEnabled ? ADBProcedures.IsUnlocked() ? OEMUnlockState.UNLOCKED : OEMUnlockState.LOCKED : 
            IsFastBootEnabled ? FastBootProcedures.IsUnlocked() ? OEMUnlockState.UNLOCKED : OEMUnlockState.LOCKED : 
            OEMUnlockState.UNKNOWN;

        public bool IsUnlocked => OEMUnlock == OEMUnlockState.UNLOCKED;
        public bool IsLocked => OEMUnlock == OEMUnlockState.LOCKED;
        public bool IsUnknownLockState => OEMUnlock == OEMUnlockState.UNKNOWN;

        public bool CanUnlock => JustDisconnected ? false :
            IsADBEnabled ? ADBProcedures.CanUnlock() :
            IsFastBootEnabled ? FastBootProcedures.CanUnlock() :
            false;

        public string? BatteryLevel => JustDisconnected ? null : IsADBEnabled
                    ? ADBProcedures.GetDeviceBatteryLevel()
                    : IsFastBootEnabled ? FastBootProcedures.GetDeviceBatteryLevel() : null;


        public string? DeviceStateLocalized => State switch
        {
            DeviceState.ANDROID_ADB_DISABLED => "Android (ADB Disconnected)",
            DeviceState.ANDROID_ADB_ENABLED => "Android (ADB Connected)",
            DeviceState.ANDROID => "Android",
            DeviceState.BOOTLOADER => "Bootloader",
            DeviceState.UEFI => "UEFI",
            DeviceState.FASTBOOTD => "FastBoot",
            DeviceState.OFFLINE_CHARGING => "Offline Charging",
            DeviceState.RECOVERY_ADB_DISABLED => "Recovery (ADB Disconnected)",
            DeviceState.RECOVERY_ADB_ENABLED => "Recovery (ADB Connected)",
            DeviceState.SIDELOAD_ADB_DISABLED => "Sideload (ADB Disconnected)",
            DeviceState.SIDELOAD_ADB_ENABLED => "Sideload (ADB Connected)",
            DeviceState.TWRP_ADB_DISABLED => "TWRP (ADB Disconnected)",
            DeviceState.TWRP_ADB_ENABLED => "TWRP (ADB Connected)",
            DeviceState.TWRP_MASS_STORAGE_ADB_DISABLED => "TWRP (Mass Storage Connected) (ADB Disconnected)",
            DeviceState.TWRP_MASS_STORAGE_ADB_ENABLED => "TWRP (Mass Storage Connected) (ADB Connected)",
            DeviceState.UFP => "UFP",
            DeviceState.WINDOWS => "Windows",
            DeviceState.DISCONNECTED => "Disconnected",
            _ => null,
        };


        public bool JustDisconnected = false;

        public bool IsConnected => State switch
        {
            DeviceState.DISCONNECTED => false,
            _ => !JustDisconnected,
        };

        public bool IsDisconnected => !IsConnected;


        public bool IsFastBootEnabled => State is DeviceState.FASTBOOTD or DeviceState.BOOTLOADER or DeviceState.UEFI;

        public bool IsADBDisabled => State is DeviceState.ANDROID_ADB_DISABLED or DeviceState.RECOVERY_ADB_DISABLED or DeviceState.SIDELOAD_ADB_DISABLED or DeviceState.TWRP_ADB_DISABLED or DeviceState.TWRP_MASS_STORAGE_ADB_DISABLED;

        public bool IsADBEnabled => State is DeviceState.ANDROID_ADB_ENABLED or DeviceState.RECOVERY_ADB_ENABLED or DeviceState.SIDELOAD_ADB_ENABLED or DeviceState.TWRP_ADB_ENABLED or DeviceState.TWRP_MASS_STORAGE_ADB_ENABLED;


        public bool IsUSBDebuggingDisabled => State is DeviceState.ANDROID;

        public bool IsInAndroid => State is DeviceState.ANDROID or DeviceState.ANDROID_ADB_DISABLED or DeviceState.ANDROID_ADB_ENABLED;

        public bool IsInTWRP => State is DeviceState.TWRP_ADB_DISABLED or DeviceState.TWRP_ADB_ENABLED or DeviceState.TWRP_MASS_STORAGE_ADB_DISABLED or DeviceState.TWRP_MASS_STORAGE_ADB_ENABLED;

        public bool IsInMassStorage => State is DeviceState.TWRP_MASS_STORAGE_ADB_DISABLED or DeviceState.TWRP_MASS_STORAGE_ADB_ENABLED;

        public bool IsInRecovery => State is DeviceState.RECOVERY_ADB_DISABLED or DeviceState.RECOVERY_ADB_ENABLED;

        public bool IsInSideload => State is DeviceState.SIDELOAD_ADB_DISABLED or DeviceState.SIDELOAD_ADB_ENABLED;

        public bool IsInFastBootD => State == DeviceState.FASTBOOTD;

        public bool IsInUEFI => State == DeviceState.UEFI;

        public bool IsInBootloader => State == DeviceState.BOOTLOADER;

        public bool IsInWindows => State == DeviceState.WINDOWS;

        public bool IsInUFP => State == DeviceState.UFP;


        public FastBootTransport? FastBootTransport
        {
            get; internal set;
        }

        public AndroidDebugBridgeTransport? AndroidDebugBridgeTransport
        {
            get; internal set;
        }

        public UnifiedFlashingPlatformTransport? UnifiedFlashingPlatformTransport
        {
            get; internal set;
        }

        public MassStorage? MassStorage
        {
            get; internal set;
        }
    }
}
