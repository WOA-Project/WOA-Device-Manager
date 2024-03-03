using System;
using System.Threading.Tasks;
using WOADeviceManager.Managers;

namespace WOADeviceManager.Helpers
{
    public class ADBProcedures
    {
        private static object lockObject = new();

        public static string GetDeviceProductModel()
        {
            lock (lockObject)
            {
                string result = null;
                try
                {
                    result = DeviceManager.Device.AndroidDebugBridgeTransport.Shell("getprop ro.product.model");
                }
                catch (Exception) { }
                return result;
            }
        }

        public static string GetDeviceProductDevice()
        {
            lock (lockObject)
            {
                string result = null;
                try
                {
                    result = DeviceManager.Device.AndroidDebugBridgeTransport.Shell("getprop ro.product.device");
                }
                catch (Exception) { }
                return result;
            }
        }

        public static string GetDeviceProductName()
        {
            lock (lockObject)
            {
                string result = null;
                try
                {
                    result = DeviceManager.Device.AndroidDebugBridgeTransport.Shell("getprop ro.product.name");
                }
                catch (Exception) { }
                return result;
            }
        }

        public static string GetDeviceBuildVersionRelease()
        {
            lock (lockObject)
            {
                string result = null;
                try
                {
                    result = DeviceManager.Device.AndroidDebugBridgeTransport.Shell("getprop ro.build.version.release");
                    return "Android " + result;
                }
                catch (Exception) { }
                return result;
            }
        }

        public static string GetDeviceBuildId()
        {
            string result = null;
            try
            {
                result = DeviceManager.Device.AndroidDebugBridgeTransport.Shell("getprop ro.build.id");
            }
            catch (Exception) { }
            return result;
        }

        public static string GetDeviceWallpaperPath()
        {
            throw new NotImplementedException();
        }

        public static void RebootToBootloader()
        {
            lock (lockObject)
            {
                DeviceManager.Device.AndroidDebugBridgeTransport.RebootBootloader();
            }
        }

        public static void RebootToAndroid()
        {
            lock (lockObject)
            {
                DeviceManager.Device.AndroidDebugBridgeTransport.Reboot();
            }
        }

        public static void RebootToFastbootD()
        {
            lock (lockObject)
            {
                DeviceManager.Device.AndroidDebugBridgeTransport.RebootFastBootD();
            }
        }

        public static void RebootToRecovery()
        {
            lock (lockObject)
            {
                DeviceManager.Device.AndroidDebugBridgeTransport.RebootRecovery();
            }
        }

        public static async Task<bool> PushParted()
        {
            /*StorageFile parted = await ResourcesManager.RetrieveFile(ResourcesManager.DownloadableComponent.PARTED);
            Progress<int> progress = new();
            bool completed = false;
            progress.ProgressChanged += (object sender, int e) =>
            {
                completed = e == 100;
            };
            try
            {
                using (SyncService service = new(ADBManager.Client, DeviceManager.GetADBDeviceDataFromUSBID(DeviceManager.Device.ID)))
                using (Stream stream = File.OpenRead(@parted.Path))
                {
                    service.Push(stream, "/sdcard/parted", 755, DateTime.Now, progress, CancellationToken.None);
                }
                while (!completed)
                {
                    await Task.Delay(500);
                }

                return true;
            }
            // TODO: Handle exception (file transfer failed)
            catch { }*/
            return false;
        }

        public static string GetDeviceBatteryLevel()
        {
            lock (lockObject)
            {
                string result = null;
                try
                {
                    result = DeviceManager.Device.AndroidDebugBridgeTransport?.Shell("dumpsys battery") ?? "";
                    if (result.Contains("level: "))
                    {
                        result = result.Split("level: ")[1].Split("\n")[0].Trim();
                    }
                    else
                    {
                        result = null;
                    }
                    return result;
                }
                catch (Exception) { }
                return result;
            }
        }

        public static async Task EnableMassStorageMode()
        {
            string MassStorageOneLiner = @"setenforce 0; echo 0xEF > /config/usb_gadget/g1/bDeviceClass; echo 0x02 > /config/usb_gadget/g1/bDeviceSubClass; echo 0x01 > /config/usb_gadget/g1/bDeviceProtocol;ln -s /config/usb_gadget/g1/functions/mass_storage.0/ /config/usb_gadget/g1/configs/b.1/;echo /dev/block/sda > /config/usb_gadget/g1/configs/b.1/mass_storage.0/lun.0/file;echo 0 > /config/usb_gadget/g1/configs/b.1/mass_storage.0/lun.0/removable;sh -c 'echo > /config/usb_gadget/g1/UDC; echo a600000.dwc3 > /config/usb_gadget/g1/UDC' &";

            lock (lockObject)
            {
                try
                {
                    DeviceManager.Device.AndroidDebugBridgeTransport.Shell(MassStorageOneLiner);
                }
                catch (Exception) { }
            }
            await Task.Delay(200);
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