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

            DeviceManager.DeviceConnectedEvent += DeviceManager_DeviceConnectedEvent;
            DeviceManager.DeviceDisconnectedEvent += Instance_DeviceDisconnectedEvent;
        }

        private async void RebootToAndroid_Click(object sender, RoutedEventArgs e)
        {
            MainPage.ToggleLoadingScreen(true);
            await DeviceRebootHelper.RebootToAndroidAndWait();
            MainPage.ToggleLoadingScreen(false);
        }

        private async void RebootToBootloader_Click(object sender, RoutedEventArgs e)
        {
            MainPage.ToggleLoadingScreen(true);
            await DeviceRebootHelper.RebootToBootloaderAndWait();
            MainPage.ToggleLoadingScreen(false);
        }

        private async void RebootToFastbootd_Clock(object sender, RoutedEventArgs e)
        {
            MainPage.ToggleLoadingScreen(true);
            await DeviceRebootHelper.RebootToFastbootDAndWait();
            MainPage.ToggleLoadingScreen(false);
        }

        private async void RebootToMassStorageMode_Click(object sender, RoutedEventArgs e)
        {
            MainPage.ToggleLoadingScreen(true);
            if (DeviceManager.Device.State != Device.DeviceStateEnum.TWRP)
            {
                await DeviceRebootHelper.RebootToTWRPAndWait();
            }
            await ADBProcedures.EnableMassStorageMode();
            MainPage.ToggleLoadingScreen(false);
        }

        private void RebootToRecovery_Click(object sender, RoutedEventArgs e)
        {
            MainPage.ToggleLoadingScreen(true);
            // TODO
            MainPage.ToggleLoadingScreen(false);
        }

        private async void RebootToTWRP_Click(object sender, RoutedEventArgs e)
        {
            MainPage.ToggleLoadingScreen(true);
            await DeviceRebootHelper.RebootToTWRPAndWait();
            MainPage.ToggleLoadingScreen(false);
        }

        private void RebootToWindows_Click(object sender, RoutedEventArgs e)
        {
            MainPage.ToggleLoadingScreen(true);
            // TODO
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
    }
}
