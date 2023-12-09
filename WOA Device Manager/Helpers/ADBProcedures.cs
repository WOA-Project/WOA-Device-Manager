using SAPTeam.AndroCtrl.Adb;
using SAPTeam.AndroCtrl.Adb.DeviceCommands;
using SAPTeam.AndroCtrl.Adb.Receivers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using WOADeviceManager.Managers;

namespace WOADeviceManager.Helpers
{
    public class ADBProcedures
    {
        public static string GetDeviceProductModel()
        {
            var receiver = new ConsoleOutputReceiver();
            try
            {
                ADBManager.Client.ExecuteRemoteCommand("getprop ro.product.model", DeviceManager.Device.Data, receiver);
            }
            catch (Exception) { }
            return receiver.ToString().Trim();
        }

        public static string GetDeviceBuildVersionRelease()
        {
            var receiver = new ConsoleOutputReceiver();
            try
            {
                ADBManager.Client.ExecuteRemoteCommand("getprop ro.build.version.release", DeviceManager.Device.Data, receiver);
                return "Android " + receiver.ToString().Trim();
            }
            catch (Exception) { }
            return null;
        }

        public static string GetDeviceBuildId()
        {
            var receiver = new ConsoleOutputReceiver();
            try
            {
                ADBManager.Client.ExecuteRemoteCommand("getprop ro.build.id", DeviceManager.Device.Data, receiver);
                return receiver.ToString().Trim();
            }
            catch (Exception) { }
            return null;
        }

        public static string GetDeviceWallpaperPath()
        {
            throw new NotImplementedException();
        }

        public static void RebootToBootloader()
        {
            ADBManager.Client.Reboot("bootloader", DeviceManager.Device.Data);
        }

        public static async Task<bool> PushParted()
        {
            StorageFile parted = await ResourcesManager.RetrieveFile(ResourcesManager.DownloadableComponent.PARTED);
            Progress<int> progress = new Progress<int>();
            bool completed = false;
            progress.ProgressChanged += (object sender, int e) =>
            {
                completed = e == 100;
            };
            try
            {
                using (SyncService service = new SyncService(ADBManager.Client, DeviceManager.Device.Data))
                using (Stream stream = File.OpenRead(@parted.Path))
                {
                    service.Push(stream, "/sdcard/parted", 755, DateTime.Now, progress, CancellationToken.None);
                }
                while (!completed) await Task.Delay(500);
                return true;
            }
            // TODO: Handle exception (file transfer failed)
            catch { }
            return false;
        }

        public static async Task<bool> PushMSCScript()
        {
            StorageFile msc = await ResourcesManager.RetrieveFile(ResourcesManager.DownloadableComponent.MASS_STORAGE_SCRIPT);
            Progress<int> progress = new Progress<int>();
            bool completed = false;
            progress.ProgressChanged += (object sender, int e) =>
            {
                completed = e == 100;
            };
            try
            {
                using (SyncService service = new SyncService(ADBManager.Client, DeviceManager.Device.Data))
                using (Stream stream = File.OpenRead(@msc.Path))
                {
                    service.Push(stream, "/sdcard/msc.tar", 755, DateTime.Now, progress, CancellationToken.None);
                }
                while (!completed) await Task.Delay(500);
                var receiver = new ConsoleOutputReceiver();
                try
                {
                    ADBManager.Client.ExecuteRemoteCommand("tar -xf /sdcard/msc.tar -C /sdcard --no-same-owner", DeviceManager.Device.Data, receiver);
                }
                catch (Exception) 
                {
                    return false;
                }
                return true;
            }
            // TODO: Handle exception (file transfer failed)
            catch { }
            return false;
        }

        public static string GetDeviceBatteryLevel()
        {
            var receiver = new ConsoleOutputReceiver();
            try
            {
                ADBManager.Client.ExecuteRemoteCommand("dumpsys battery", DeviceManager.Device.Data, receiver);
                string result = receiver.ToString().Trim();
                result = result.Split("level: ")[1].Split("\n")[0].Trim();
                return result;
            }
            catch (Exception) { }
            return null;
        }

        public static async Task EnableMassStorageMode()
        {
            await PushMSCScript();
            var receiver = new ConsoleOutputReceiver();
            try
            {
                ADBManager.Client.ExecuteRemoteCommand("sh /sdcard/msc.sh", DeviceManager.Device.Data, receiver);
                await Task.Delay(3000);
            }
            catch (Exception) { }
            return;
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