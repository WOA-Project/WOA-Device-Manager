using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WOADeviceManager.Managers;

namespace WOADeviceManager.Helpers
{
    public class ADBProcedures
    {
        public static async Task<string> GetDeviceProductModel(string deviceName)
        {
            return (await ADBManager.SendShellCommand("getprop ro.product.model", deviceName)).Split(Environment.NewLine)[0];
        }

        public static async Task<string> GetDeviceBuildVersionRelease(string deviceName)
        {
            return "Android " + (await ADBManager.SendShellCommand("getprop ro.build.version.release", deviceName)).Split(Environment.NewLine)[0];
        }

        public static async Task<string> GetDeviceBuildId(string deviceName)
        {
            return (await ADBManager.SendShellCommand("getprop ro.build.id", deviceName)).Split(Environment.NewLine)[0];
        }

        public static async Task<string> GetDeviceWallpaper(string deviceName)
        {
            return null;
        }

        public static void RebootToBootloader(string deviceName)
        {
            ADBManager.SendADBCommand("reboot bootloader", deviceName);
        }
    }
}

/*

Valuable info in getprop

[ro.build.id]: [2022.823.34]
[ro.vendor.build.security_patch]: [2023-02-01]
[ro.product.model]: [Surface Duo]
[ro.build.version.incremental]: [202208230034]
[ro.build.version.release]: [12]

[sys.boot.reason]: [shutdown,battery]
[sys.boot.reason.last]: [shutdown,battery]

*/