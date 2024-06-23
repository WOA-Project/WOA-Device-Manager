using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WOADeviceManager.Helpers;

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
            FastBootProcedures.Reboot();
            MainPage.ToggleLoadingScreen(false);
        }

        private void FlashUnlock_Click(object sender, RoutedEventArgs e)
        {
            _ = FastBootProcedures.FlashUnlock(this);
        }

        private void FlashLock_Click(object sender, RoutedEventArgs e)
        {
            _ = FastBootProcedures.FlashLock(this);
        }

        private async void BootTWRP_Click(object sender, RoutedEventArgs e)
        {
            MainPage.ToggleLoadingScreen(true);
            _ = await FastBootProcedures.BootTWRP();
            MainPage.ToggleLoadingScreen(false);
        }

        private async void PushParted_Click(object sender, RoutedEventArgs e)
        {
            MainPage.ToggleLoadingScreen(true);
            _ = await ADBProcedures.PushParted();
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
