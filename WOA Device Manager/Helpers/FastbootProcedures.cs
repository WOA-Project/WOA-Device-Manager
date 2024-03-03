using FastBoot;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using WOADeviceManager.Managers;

namespace WOADeviceManager.Helpers
{
    internal class FastbootProcedures
    {
        public static string GetProduct()
        {
            bool result = DeviceManager.Device.FastBootTransport.GetVariable("product", out string productGetVar);
            return !result ? null : productGetVar;
        }

        public static void Reboot()
        {
            _ = DeviceManager.Device.FastBootTransport.Reboot();
        }

        public static void RebootBootloader()
        {
            _ = DeviceManager.Device.FastBootTransport.RebootBootloader();
        }

        public static void RebootRecovery()
        {
            _ = DeviceManager.Device.FastBootTransport.RebootRecovery();
        }

        public static void RebootFastbootD()
        {
            _ = DeviceManager.Device.FastBootTransport.RebootFastBootD();
        }

        public static bool FlashUnlock(Control frameHost = null)
        {
            bool result = DeviceManager.Device.FastBootTransport.FlashingGetUnlockAbility(out bool canUnlock);
            if (result)
            {
                return false;
            }

            if (canUnlock)
            {
                ContentDialog dialog = new()
                {
                    Title = "⚠️ EVERYTHING WILL BE FORMATTED",
                    Content = "Flash unlocking requires everything to be formatted. MAKE SURE YOU HAVE MADE A COPY OF EVERYTHING. We're not responsible for data loss.",
                    PrimaryButtonText = "⚠️ Proceed"
                };
                dialog.PrimaryButtonClick += (ContentDialog dialog, ContentDialogButtonClickEventArgs args) =>
                {
                    Debug.WriteLine("hi");
                    // TODO: Disabled for safety
                    //return DeviceManager.Device.FastBootTransport.FlashingUnlock(); // TODO: error handling here, always returns true rn ofc
                    dialog.Hide();
                };
                dialog.CloseButtonText = "Cancel";
                if (frameHost != null)
                {
                    dialog.XamlRoot = frameHost.XamlRoot;
                }
                _ = dialog.ShowAsync();
                return true;
            }
            else
            {
                ContentDialog dialog = new()
                {
                    Title = "Unlocking is disabled",
                    Content = "Flash Unlocking is disabled from Developer Settings in Android. Please enable it manually from there.",
                    CloseButtonText = "OK"
                };
                if (frameHost != null)
                {
                    dialog.XamlRoot = frameHost.XamlRoot;
                }
                _ = dialog.ShowAsync();
                return false;
            }

        }

        public static bool FlashLock(Control frameHost = null)
        {
            // TODO: Check that the device doesn't have Windows installed
            ContentDialog dialog = new()
            {
                Title = "⚠️ Your bootloader will be locked",
                Content = "This procedure will lock your bootloader. You usually don't want to do this unless you have to sell your device.",
                PrimaryButtonText = "⚠️ Proceed"
            };
            dialog.PrimaryButtonClick += (ContentDialog dialog, ContentDialogButtonClickEventArgs args) =>
            {
                Debug.WriteLine("hi");
                // TODO: Disabled for safety
                //return DeviceManager.Device.FastBootTransport.FlashingLock(); // TODO: error handling here, always returns true rn ofc
                dialog.Hide();
            };
            dialog.CloseButtonText = "Cancel";
            if (frameHost != null)
            {
                dialog.XamlRoot = frameHost.XamlRoot;
            }
            _ = dialog.ShowAsync();
            return true;
        }

        public static async Task<bool> BootTWRP()
        {
            StorageFile twrp = await ResourcesManager.RetrieveFile(ResourcesManager.DownloadableComponent.TWRP_EPSILON);
            return twrp != null && DeviceManager.Device.FastBootTransport.BootImageIntoRam(twrp.Path);
        }

        public static string GetDeviceBatteryLevel()
        {
            bool result = DeviceManager.Device.FastBootTransport.GetVariable("battery-level", out string batteryLevel);
            return result ? batteryLevel : null;
        }
    }
}
