using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SAPTeam.AndroCtrl.Adb;
using System;
using WOADeviceManager.Helpers;
using WOADeviceManager.Managers;

namespace WOADeviceManager.Pages
{
    public sealed partial class zDebugPage : Page
    {
        Device device = null;
        DeviceData devicedata = null;

        public zDebugPage()
        {
            InitializeComponent();
        }

        private void RebootToBootloader_Click(object sender, RoutedEventArgs e)
        {
            MainPage.ToggleLoadingScreen(true);
            ADBProcedures.RebootToBootloader();
            MainPage.ToggleLoadingScreen(false);
        }

        private void RebootToAndroid_Click(object sender, RoutedEventArgs e)
        {
            MainPage.ToggleLoadingScreen(true);
            FastbootProcedures.Reboot(device.SerialNumber);
            MainPage.ToggleLoadingScreen(false);
        }

        private void FlashUnlock_Click(object sender, RoutedEventArgs e)
        {
            _ = FastbootProcedures.FlashUnlock(device.SerialNumber, this);
        }

        private void FlashLock_Click(object sender, RoutedEventArgs e)
        {
            FastbootProcedures.FlashLock(device.SerialNumber, this);
        }

        private async void BootTWRP_Click(object sender, RoutedEventArgs e)
        {
            MainPage.ToggleLoadingScreen(true);
            await FastbootProcedures.BootTWRP(device.SerialNumber);
            MainPage.ToggleLoadingScreen(false);
        }

        private async void PushParted_Click(object sender, RoutedEventArgs e)
        {
            MainPage.ToggleLoadingScreen(true);
            await ADBProcedures.PushParted();
            MainPage.ToggleLoadingScreen(false);
        }
    }
}
