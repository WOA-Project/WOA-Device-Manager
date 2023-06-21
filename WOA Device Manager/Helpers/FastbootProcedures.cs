using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using WOADeviceManager.Managers;

namespace WOADeviceManager.Helpers
{
    internal class FastbootProcedures
    {
        public static string GetProduct(string deviceName)
        {
            return FastbootManager.SendFastbootCommand("getvar product").Split(Environment.NewLine)[0].Split(": ")[1];
        }

        public static void Reboot(string deviceName)
        {
            FastbootManager.SendFastbootCommand("reboot");
        }

        public static async Task<bool> FlashUnlock(string deviceName, Control frameHost = null)
        {
            if (FastbootManager.SendFastbootCommand("flashing get_unlock_ability").Contains("get_unlock_ability: 1"))
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Title = "⚠️ EVERYTHING WILL BE FORMATTED";
                dialog.Content = "Flash unlocking requires everything to be formatted. MAKE SURE YOU HAVE MADE A COPY OF EVERYTHING. We're not responsible for data loss.";
                dialog.PrimaryButtonText = "⚠️ Proceed";
                dialog.PrimaryButtonClick += (ContentDialog dialog, ContentDialogButtonClickEventArgs args) =>
                {
                    Debug.WriteLine("hi");
                    dialog.Hide();
                };
                dialog.CloseButtonText = "Cancel";
                if (frameHost != null)
                {
                    dialog.XamlRoot = frameHost.XamlRoot;
                }
                dialog.ShowAsync();
                //TODO: Send 932857957028950 warnings to the user that everything will be formatted
                //TODO: Disabled for safety
                //return FastbootManager.SendFastbootCommand("flashing unlock") != null; // TODO: error handling here, always returns true rn ofc
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
                dialog.ShowAsync();
                return false;
            }
            
        }

        public static bool FlashLock(string deviceName)
        {
            //TODO: Check that the device doesn't have Windows installed
            return FastbootManager.SendFastbootCommand("flashing lock") != null; // TODO: error handling here, always returns true rn ofc
        }
    }
}
