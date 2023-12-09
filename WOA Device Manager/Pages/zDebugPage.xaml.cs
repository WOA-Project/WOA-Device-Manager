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
        public zDebugPage()
        {
            InitializeComponent();
        }

        private async void RebootToBootloader_Click(object sender, RoutedEventArgs e)
        {
            MainPage.ToggleLoadingScreen(true);
            await DeviceRebootHelper.RebootToBootloaderAndWait();
            MainPage.ToggleLoadingScreen(false);
        }

        private void RebootToAndroid_Click(object sender, RoutedEventArgs e)
        {
            MainPage.ToggleLoadingScreen(true);
            FastbootProcedures.Reboot(DeviceManager.Device.SerialNumber);
            MainPage.ToggleLoadingScreen(false);
        }

        private void FlashUnlock_Click(object sender, RoutedEventArgs e)
        {
            _ = FastbootProcedures.FlashUnlock(DeviceManager.Device.SerialNumber, this);
        }

        private void FlashLock_Click(object sender, RoutedEventArgs e)
        {
            FastbootProcedures.FlashLock(DeviceManager.Device.SerialNumber, this);
        }

        private async void BootTWRP_Click(object sender, RoutedEventArgs e)
        {
            MainPage.ToggleLoadingScreen(true);
            await FastbootProcedures.BootTWRP(DeviceManager.Device.SerialNumber);
            MainPage.ToggleLoadingScreen(false);
        }

        private async void PushParted_Click(object sender, RoutedEventArgs e)
        {
            MainPage.ToggleLoadingScreen(true);
            await ADBProcedures.PushParted();
            MainPage.ToggleLoadingScreen(false);
        }

        private async void MassStorageMode_Click(object sender, RoutedEventArgs e)
        {
            MainPage.ToggleLoadingScreen(true);
            await ADBProcedures.EnableMassStorageMode();
            MainPage.ToggleLoadingScreen(false);
        }
    }
}
