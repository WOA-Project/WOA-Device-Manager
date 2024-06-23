using System;
using System.Threading.Tasks;
using WOADeviceManager.Managers;

namespace WOADeviceManager.Helpers
{
    public class DeviceRebootHelper
    {
        public static async Task RebootToBootloaderAndWait()
        {
            if (DeviceManager.Device.State == Device.DeviceStateEnum.BOOTLOADER)
            {
                return;
            }

            if (DeviceManager.Device.IsADBEnabled)
            {
                ADBProcedures.RebootToBootloader();
            }
            else if (DeviceManager.Device.IsFastBootEnabled)
            {
                FastBootProcedures.RebootBootloader();
            }
            else if (DeviceManager.Device.State == Device.DeviceStateEnum.ANDROID)
            {
                throw new Exception("Unauthorized ADB devices can't be rebooted to bootloader.");
            }
            else if (DeviceManager.Device.State == Device.DeviceStateEnum.WINDOWS)
            {
                throw new Exception("Rebooting from Windows to the bootloader is still unsupported.");
            }
            while (DeviceManager.Device.State != Device.DeviceStateEnum.BOOTLOADER)
            {
                await Task.Delay(1000);
            }
        }

        public static async Task RebootToAndroidAndWait()
        {
            if (DeviceManager.Device.State is Device.DeviceStateEnum.ANDROID or Device.DeviceStateEnum.ANDROID_ADB_ENABLED)
            {
                return;
            }

            if (DeviceManager.Device.IsFastBootEnabled)
            {
                FastBootProcedures.Reboot();
            }
            else if (DeviceManager.Device.IsADBEnabled)
            {
                ADBProcedures.RebootToAndroid();
            }
            else if (DeviceManager.Device.State == Device.DeviceStateEnum.WINDOWS)
            {
                throw new Exception("Rebooting from Windows to Android is still unsupported.");
            }
            while (DeviceManager.Device.State is not Device.DeviceStateEnum.ANDROID_ADB_ENABLED and not Device.DeviceStateEnum.ANDROID)
            {
                await Task.Delay(1000);
            }
        }

        public static async Task RebootToTWRPAndWait()
        {
            if (DeviceManager.Device.State is Device.DeviceStateEnum.TWRP_ADB_ENABLED or Device.DeviceStateEnum.TWRP_MASS_STORAGE_ADB_ENABLED)
            {
                return;
            }

            if (DeviceManager.Device.State != Device.DeviceStateEnum.BOOTLOADER)
            {
                await RebootToBootloaderAndWait();
            }

            if (DeviceManager.Device.State is Device.DeviceStateEnum.BOOTLOADER)
            {
                _ = await FastBootProcedures.BootTWRP();

                while (DeviceManager.Device.State != Device.DeviceStateEnum.TWRP_ADB_ENABLED)
                {
                    await Task.Delay(1000);
                }
            }
        }

        public static async Task RebootToUEFIAndWait()
        {
            if (DeviceManager.Device.State is Device.DeviceStateEnum.WINDOWS)
            {
                return;
            }

            if (DeviceManager.Device.State != Device.DeviceStateEnum.BOOTLOADER)
            {
                await RebootToBootloaderAndWait();
            }

            if (DeviceManager.Device.State is Device.DeviceStateEnum.BOOTLOADER)
            {
                _ = await FastBootProcedures.BootUEFI();

                /*while (DeviceManager.Device.State != Device.DeviceStateEnum.WINDOWS)
                {
                    await Task.Delay(1000);
                }*/
            }
        }

        public static async Task RebootToMSCAndWait()
        {
            if (DeviceManager.Device.State is Device.DeviceStateEnum.TWRP_MASS_STORAGE_ADB_ENABLED)
            {
                return;
            }

            if (DeviceManager.Device.State != Device.DeviceStateEnum.TWRP_ADB_ENABLED)
            {
                await RebootToTWRPAndWait();
            }

            if (DeviceManager.Device.State is Device.DeviceStateEnum.TWRP_ADB_ENABLED)
            {
                await ADBProcedures.EnableMassStorageMode();

                while (DeviceManager.Device.State != Device.DeviceStateEnum.TWRP_MASS_STORAGE_ADB_ENABLED)
                {
                    await Task.Delay(1000);
                }
            }
        }

        public static async Task RebootToRecoveryAndWait()
        {
            if (DeviceManager.Device.State is Device.DeviceStateEnum.RECOVERY_ADB_ENABLED)
            {
                return;
            }

            if (DeviceManager.Device.IsADBEnabled)
            {
                ADBProcedures.RebootToRecovery();
            }
            else if (DeviceManager.Device.IsFastBootEnabled)
            {
                FastBootProcedures.RebootRecovery();
            }
            else if (DeviceManager.Device.State == Device.DeviceStateEnum.ANDROID)
            {
                throw new Exception("Unauthorized ADB devices can't be rebooted to bootloader.");
            }
            else if (DeviceManager.Device.State == Device.DeviceStateEnum.WINDOWS)
            {
                throw new Exception("Rebooting from Windows to the bootloader is still unsupported.");
            }

            while (DeviceManager.Device.State != Device.DeviceStateEnum.RECOVERY_ADB_ENABLED)
            {
                await Task.Delay(1000);
            }
        }

        public static async Task RebootToFastBootDAndWait()
        {
            if (DeviceManager.Device.State is Device.DeviceStateEnum.FASTBOOTD)
            {
                return;
            }

            if (DeviceManager.Device.IsADBEnabled)
            {
                ADBProcedures.RebootToFastBootD();
            }
            else if (DeviceManager.Device.IsFastBootEnabled)
            {
                FastBootProcedures.RebootFastBootD();
            }
            else if (DeviceManager.Device.State == Device.DeviceStateEnum.ANDROID)
            {
                throw new Exception("Unauthorized ADB devices can't be rebooted to fastbootd.");
            }
            else if (DeviceManager.Device.State == Device.DeviceStateEnum.WINDOWS)
            {
                throw new Exception("Rebooting from Windows to the fastbootd is still unsupported.");
            }
            while (DeviceManager.Device.State != Device.DeviceStateEnum.FASTBOOTD)
            {
                await Task.Delay(1000);
            }
        }
    }
}
