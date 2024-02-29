using FastBoot;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using WOADeviceManager.Managers;

namespace WOADeviceManager.Helpers
{
    internal class FastbootProcedures
    {
        private static string CleanupProductString(string product)
        {
            if (product == "surfaceduo")
            {
                return "Surface Duo";
            }

            if (product == "surfaceduo2")
            {
                return "Surface Duo 2";
            }

            return product;
        }

        public static string GetProduct()
        {
            using FastBootTransport fastBootTransport = new(DeviceManager.Device.FastbootID);
            bool result = fastBootTransport.GetVariable("product", out string productGetVar);
            if (!result)
            {
                return null;
            }

            return CleanupProductString(productGetVar);
        }

        public static void Reboot()
        {
            using FastBootTransport fastBootTransport = new(DeviceManager.Device.FastbootID);
            fastBootTransport.Reboot();
        }

        public static void RebootBootloader()
        {
            using FastBootTransport fastBootTransport = new(DeviceManager.Device.FastbootID);
            fastBootTransport.RebootBootloader();
        }

        public static void RebootRecovery()
        {
            using FastBootTransport fastBootTransport = new(DeviceManager.Device.FastbootID);
            fastBootTransport.RebootRecovery();
        }

        public static void RebootFastbootD()
        {
            using FastBootTransport fastBootTransport = new(DeviceManager.Device.FastbootID);
            fastBootTransport.RebootFastBootD();
        }

        public static async Task<bool> FlashUnlock(Control frameHost = null)
        {
            using FastBootTransport fastBootTransport = new(DeviceManager.Device.FastbootID);
            bool result = fastBootTransport.FlashingGetUnlockAbility(out bool canUnlock);
            if (result)
            {
                return false;
            }

            if (canUnlock)
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Title = "⚠️ EVERYTHING WILL BE FORMATTED";
                dialog.Content = "Flash unlocking requires everything to be formatted. MAKE SURE YOU HAVE MADE A COPY OF EVERYTHING. We're not responsible for data loss.";
                dialog.PrimaryButtonText = "⚠️ Proceed";
                dialog.PrimaryButtonClick += (ContentDialog dialog, ContentDialogButtonClickEventArgs args) =>
                {
                    Debug.WriteLine("hi");
                    // TODO: Disabled for safety
                    //return fastBootTransport.FlashingUnlock(); // TODO: error handling here, always returns true rn ofc
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
                ContentDialog dialog = new ContentDialog();
                dialog.Title = "Unlocking is disabled";
                dialog.Content = "Flash Unlocking is disabled from Developer Settings in Android. Please enable it manually from there.";
                dialog.CloseButtonText = "OK";
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
            using FastBootTransport fastBootTransport = new(DeviceManager.Device.FastbootID);

            // TODO: Check that the device doesn't have Windows installed
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "⚠️ Your bootloader will be locked";
            dialog.Content = "This procedure will lock your bootloader. You usually don't want to do this unless you have to sell your device.";
            dialog.PrimaryButtonText = "⚠️ Proceed";
            dialog.PrimaryButtonClick += (ContentDialog dialog, ContentDialogButtonClickEventArgs args) =>
            {
                Debug.WriteLine("hi");
                // TODO: Disabled for safety
                //return fastBootTransport.FlashingLock(); // TODO: error handling here, always returns true rn ofc
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
            using FastBootTransport fastBootTransport = new(DeviceManager.Device.FastbootID);

            StorageFile twrp = await ResourcesManager.RetrieveFile(ResourcesManager.DownloadableComponent.TWRP);
            if (twrp == null) return false;

            return fastBootTransport.BootImageIntoRam(twrp.Path);
        }

        public static string GetDeviceBatteryLevel()
        {
            using FastBootTransport fastBootTransport = new(DeviceManager.Device.FastbootID);
            bool result = fastBootTransport.GetVariable("battery-level", out string batteryLevel);
            if (result)
            {
                return batteryLevel;
            }

            return null;
        }
    }
}
