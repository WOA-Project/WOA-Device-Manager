using System;
using System.Threading.Tasks;
using WOADeviceManager.Managers;
using WOADeviceManager.Managers.Connectivity;

namespace WOADeviceManager.Helpers
{
    public class DeviceRebootHelper
    {
        public static async Task RebootToBootloaderAndWait()
        {
            if (DeviceManager.Device.State == DeviceState.BOOTLOADER)
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
            else if (DeviceManager.Device.State == DeviceState.ANDROID)
            {
                throw new Exception("Unauthorized ADB devices can't be rebooted to bootloader.");
            }
            else if (DeviceManager.Device.State == DeviceState.WINDOWS)
            {
                throw new Exception("Rebooting from Windows to the bootloader is still unsupported.");
            }
            while (DeviceManager.Device.State != DeviceState.BOOTLOADER)
            {
                await Task.Delay(1000);
            }
        }

        public static async Task RebootToAndroidAndWait()
        {
            if (DeviceManager.Device.State is DeviceState.ANDROID or DeviceState.ANDROID_ADB_ENABLED or DeviceState.ANDROID_ADB_DISABLED)
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
            else if (DeviceManager.Device.IsInUFP)
            {
                UFPProcedures.Reboot();
            }
            else if (DeviceManager.Device.State == DeviceState.WINDOWS)
            {
                throw new Exception("Rebooting from Windows to Android is still unsupported.");
            }

            while (DeviceManager.Device.State is not DeviceState.ANDROID_ADB_ENABLED and not DeviceState.ANDROID and not DeviceState.ANDROID_ADB_DISABLED)
            {
                await Task.Delay(1000);
            }
        }

        public static async Task RebootToTWRPAndWait()
        {
            if (DeviceManager.Device.State is DeviceState.TWRP_ADB_ENABLED or DeviceState.TWRP_MASS_STORAGE_ADB_ENABLED or DeviceState.TWRP_ADB_DISABLED or DeviceState.TWRP_MASS_STORAGE_ADB_DISABLED)
            {
                return;
            }

            if (DeviceManager.Device.State != DeviceState.BOOTLOADER)
            {
                await RebootToBootloaderAndWait();
            }

            if (DeviceManager.Device.State is DeviceState.BOOTLOADER)
            {
                _ = await FastBootProcedures.BootTWRP();

                while (DeviceManager.Device.State is not DeviceState.TWRP_ADB_ENABLED and not DeviceState.TWRP_ADB_DISABLED)
                {
                    await Task.Delay(1000);
                }
            }
        }

        public static async Task RebootToUEFIAndWait(string UEFIFile = null)
        {
            if (DeviceManager.Device.State is DeviceState.WINDOWS or DeviceState.UFP)
            {
                return;
            }

            if (DeviceManager.Device.State != DeviceState.BOOTLOADER)
            {
                await RebootToBootloaderAndWait();
            }

            if (DeviceManager.Device.State is DeviceState.BOOTLOADER)
            {
                _ = await FastBootProcedures.BootUEFI(UEFIFile);

                while (DeviceManager.Device.State is not DeviceState.WINDOWS and not DeviceState.UFP)
                {
                    await Task.Delay(1000);
                }
            }
        }

        public static async Task RebootToMSCAndWait()
        {
            if (DeviceManager.Device.State is DeviceState.TWRP_MASS_STORAGE_ADB_ENABLED or DeviceState.TWRP_MASS_STORAGE_ADB_DISABLED)
            {
                return;
            }

            if (DeviceManager.Device.State is not DeviceState.TWRP_ADB_ENABLED)
            {
                await RebootToTWRPAndWait();

                while (DeviceManager.Device.State is not DeviceState.TWRP_ADB_ENABLED)
                {
                    await Task.Delay(1000);
                }
            }

            await ADBProcedures.EnableMassStorageMode();

            while (DeviceManager.Device.State is not DeviceState.TWRP_MASS_STORAGE_ADB_ENABLED and not DeviceState.TWRP_MASS_STORAGE_ADB_DISABLED)
            {
                await Task.Delay(1000);
            }
        }

        public static async Task RebootToRecoveryAndWait()
        {
            if (DeviceManager.Device.State is DeviceState.RECOVERY_ADB_ENABLED or DeviceState.RECOVERY_ADB_DISABLED or DeviceState.SIDELOAD_ADB_ENABLED or DeviceState.SIDELOAD_ADB_DISABLED)
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
            else if (DeviceManager.Device.State == DeviceState.ANDROID)
            {
                throw new Exception("Unauthorized ADB devices can't be rebooted to bootloader.");
            }
            else if (DeviceManager.Device.State == DeviceState.WINDOWS)
            {
                throw new Exception("Rebooting from Windows to the bootloader is still unsupported.");
            }

            while (DeviceManager.Device.State is not DeviceState.RECOVERY_ADB_ENABLED and not DeviceState.RECOVERY_ADB_DISABLED and not DeviceState.SIDELOAD_ADB_ENABLED and not DeviceState.SIDELOAD_ADB_DISABLED)
            {
                await Task.Delay(1000);
            }
        }

        public static async Task RebootToFastBootDAndWait()
        {
            if (DeviceManager.Device.State is DeviceState.FASTBOOTD)
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
            else if (DeviceManager.Device.State is DeviceState.ANDROID or DeviceState.ANDROID_ADB_DISABLED)
            {
                throw new Exception("Unauthorized ADB devices can't be rebooted to fastbootd.");
            }
            else if (DeviceManager.Device.State == DeviceState.WINDOWS)
            {
                throw new Exception("Rebooting from Windows to the fastbootd is still unsupported.");
            }

            while (DeviceManager.Device.State != DeviceState.FASTBOOTD)
            {
                await Task.Delay(1000);
            }
        }
    }
}
