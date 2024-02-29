using System;
using System.Threading.Tasks;
using WOADeviceManager.Managers;

namespace WOADeviceManager.Helpers
{
    public class DeviceRebootHelper
    {
        public static async Task RebootToBootloaderAndWait()
        {
            if (DeviceManager.Device.State == Device.DeviceStateEnum.ANDROID_ADB_ENABLED || DeviceManager.Device.State == Device.DeviceStateEnum.TWRP)
            {
                ADBProcedures.RebootToBootloader();
            }
            else if (DeviceManager.Device.State == Device.DeviceStateEnum.FASTBOOT)
            {
                FastbootProcedures.RebootBootloader();
            }
            else if (DeviceManager.Device.State == Device.DeviceStateEnum.ANDROID)
            {
                throw new Exception("Unauthorized ADB devices can't be rebooted to bootloader.");
            }
            else if (DeviceManager.Device.State == Device.DeviceStateEnum.WINDOWS)
            {
                throw new Exception("Rebooting from Windows to the bootloader is still unsupported.");
            }
            while (DeviceManager.Device.State != Device.DeviceStateEnum.BOOTLOADER) await Task.Delay(1000);
        }

        public static async Task RebootToAndroidAndWait()
        {
            if (DeviceManager.Device.State == Device.DeviceStateEnum.BOOTLOADER || DeviceManager.Device.State == Device.DeviceStateEnum.FASTBOOT)
            {
                FastbootProcedures.Reboot();
            }
            else if (DeviceManager.Device.State == Device.DeviceStateEnum.TWRP)
            {
                ADBProcedures.RebootToAndroid();
            }
            else if (DeviceManager.Device.State == Device.DeviceStateEnum.WINDOWS)
            {
                throw new Exception("Rebooting from Windows to Android is still unsupported.");
            }
            while (DeviceManager.Device.State != Device.DeviceStateEnum.ANDROID_ADB_ENABLED && DeviceManager.Device.State != Device.DeviceStateEnum.ANDROID) await Task.Delay(1000);
        }

        public static async Task RebootToTWRPAndWait()
        {
            await RebootToBootloaderAndWait();

            if (DeviceManager.Device.State == Device.DeviceStateEnum.BOOTLOADER || DeviceManager.Device.State == Device.DeviceStateEnum.FASTBOOT)
            {
                await FastbootProcedures.BootTWRP();
            }
            while (DeviceManager.Device.State != Device.DeviceStateEnum.TWRP) await Task.Delay(1000);
        }

        public static async Task RebootToFastbootDAndWait()
        {
            if (DeviceManager.Device.State == Device.DeviceStateEnum.ANDROID_ADB_ENABLED || DeviceManager.Device.State == Device.DeviceStateEnum.TWRP)
            {
                ADBProcedures.RebootToFastboot();
            }
            else if (DeviceManager.Device.State == Device.DeviceStateEnum.BOOTLOADER)
            {
                FastbootProcedures.RebootFastbootD();
            }
            else if (DeviceManager.Device.State == Device.DeviceStateEnum.ANDROID)
            {
                throw new Exception("Unauthorized ADB devices can't be rebooted to fastbootd.");
            }
            else if (DeviceManager.Device.State == Device.DeviceStateEnum.WINDOWS)
            {
                throw new Exception("Rebooting from Windows to the fastbootd is still unsupported.");
            }
            while (DeviceManager.Device.State != Device.DeviceStateEnum.FASTBOOT) await Task.Delay(1000);
        }
    }
}
