using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                FastbootProcedures.Reboot(DeviceManager.Device);
            }
            else if (DeviceManager.Device.State == Device.DeviceStateEnum.WINDOWS)
            {
                throw new Exception("Rebooting from Windows to Android is still unsupported.");
            }
            while (DeviceManager.Device.State != Device.DeviceStateEnum.ANDROID_ADB_ENABLED) await Task.Delay(1000);
        }

        public static async Task RebootToTWRPAndWait()
        {
            if (DeviceManager.Device.State == Device.DeviceStateEnum.ANDROID_ADB_ENABLED)
            {
                await RebootToBootloaderAndWait();
            }
            else if (DeviceManager.Device.State == Device.DeviceStateEnum.WINDOWS)
            {
                throw new Exception("Rebooting from Windows to TWRP is still unsupported.");
            }
            if (DeviceManager.Device.State == Device.DeviceStateEnum.BOOTLOADER || DeviceManager.Device.State == Device.DeviceStateEnum.FASTBOOT)
            {
                await FastbootProcedures.BootTWRP(DeviceManager.Device);
            }
            while (DeviceManager.Device.State != Device.DeviceStateEnum.TWRP) await Task.Delay(1000);
        }
    }
}
