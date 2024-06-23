namespace WOADeviceManager.Managers.Connectivity
{
    public enum DeviceState
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
}
