using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WOADeviceManager.Helpers;
using WOADeviceManager.Managers;

namespace WOADeviceManager.Pages
{
    public sealed partial class SwitchModePage : Page
    {
        public SwitchModePage()
        {
            InitializeComponent();
        }

        private async void RebootToAndroid_Click(object sender, RoutedEventArgs e)
        {
            MainPage.SetStatus("Rebooting phone to Android mode...", Emoji: "🔄️");
            try
            {
                await DeviceRebootHelper.RebootToAndroidAndWait();
            }
            catch { }
            MainPage.ToggleLoadingScreen(false);
        }

        private async void RebootToBootloader_Click(object sender, RoutedEventArgs e)
        {
            MainPage.SetStatus("Rebooting phone to Bootloader mode...", Emoji: "🔄️");
            try
            {
                await DeviceRebootHelper.RebootToBootloaderAndWait();
            }
            catch { }
            MainPage.ToggleLoadingScreen(false);
        }

        private async void RebootToFastbootd_Click(object sender, RoutedEventArgs e)
        {
            MainPage.SetStatus("Rebooting phone to Fastbootd mode...", Emoji: "🔄️");
            try
            {
                await DeviceRebootHelper.RebootToFastbootDAndWait();
            }
            catch { }
            MainPage.ToggleLoadingScreen(false);
        }

        private async void RebootToMassStorageMode_Click(object sender, RoutedEventArgs e)
        {
            MainPage.SetStatus("Rebooting phone to Mass Storage mode...", Emoji: "🔄️");
            try
            {
                await DeviceRebootHelper.RebootToMSCAndWait();
            }
            catch { }
            MainPage.ToggleLoadingScreen(false);
        }

        private async void RebootToRecovery_Click(object sender, RoutedEventArgs e)
        {
            MainPage.SetStatus("Rebooting phone to Recovery mode...", Emoji: "🔄️");
            try
            {
                await DeviceRebootHelper.RebootToRecoveryAndWait();
            }
            catch { }
            MainPage.ToggleLoadingScreen(false);
        }

        private async void RebootToTWRP_Click(object sender, RoutedEventArgs e)
        {
            MainPage.SetStatus("Rebooting phone to TWRP mode...", Emoji: "🔄️");
            try
            {
                await DeviceRebootHelper.RebootToTWRPAndWait();
            }
            catch { }
            MainPage.ToggleLoadingScreen(false);
        }

        private async void RebootToWindows_Click(object sender, RoutedEventArgs e)
        {
            MainPage.SetStatus("Rebooting phone to Windows mode...", Emoji: "🔄️");
            try
            {
                await DeviceRebootHelper.RebootToUEFIAndWait();
            }
            catch { }
            MainPage.ToggleLoadingScreen(false);
        }

        private void Shutdown_Click(object sender, RoutedEventArgs e)
        {
            MainPage.SetStatus("Shutting Down Phone (UFP)...", SubMessage: "Please disconnect your phone now in order to shut it down!", Emoji: "🔄️");
            try
            {
                UFPProcedures.Shutdown();
            }
            catch { }
            MainPage.ToggleLoadingScreen(false);
        }

        private void Reboot_Click(object sender, RoutedEventArgs e)
        {
            MainPage.SetStatus("Rebooting phone (UFP)...", Emoji: "🔄️");
            try
            {
                UFPProcedures.Reboot();
            }
            catch { }
            MainPage.ToggleLoadingScreen(false);
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            MainPage.SetStatus("Continuing Boot (UFP)...", Emoji: "🔄️");
            try
            {
                UFPProcedures.ContinueBoot();
            }
            catch { }
            MainPage.ToggleLoadingScreen(false);
        }

        private void MassStorage_Click(object sender, RoutedEventArgs e)
        {
            MainPage.SetStatus("Rebooting phone to Mass Storage mode (UFP)...", Emoji: "🔄️");
            try
            {
                UFPProcedures.MassStorage();
            }
            catch { }
            MainPage.ToggleLoadingScreen(false);
        }

        private void Instance_DeviceDisconnectedEvent(object sender, Device device)
        {
            _ = DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
            {
                Bindings.Update();
            });
        }

        private void DeviceManager_DeviceConnectedEvent(object sender, Device device)
        {
            _ = DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
            {
                Bindings.Update();
            });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DeviceManager.DeviceConnectedEvent += DeviceManager_DeviceConnectedEvent;
            DeviceManager.DeviceDisconnectedEvent += Instance_DeviceDisconnectedEvent;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            DeviceManager.DeviceConnectedEvent -= DeviceManager_DeviceConnectedEvent;
            DeviceManager.DeviceDisconnectedEvent -= Instance_DeviceDisconnectedEvent;
        }
    }
}
